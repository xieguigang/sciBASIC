' ------------------------------------------------------------------------
' 基于 CUPTI 旧 Event API 的 per-SM（instance）计数器采集
'
' 背景（务必先读）：
'   CUPTI 的 per-instance / per-SM 能力只存在于旧的 Event + Metric API：
'     cuptiEventGroupSetAttribute(group, CUPTI_EVENT_GROUP_ATTR_PROFILE_ALL_DOMAIN_INSTANCES, ...)
'     cuptiEventGroupReadEvent(...)   → 每个 domain instance（= 每个 SM）一个 uint64
'     cuptiEventGroupGetAttribute(group, CUPTI_EVENT_GROUP_ATTR_INSTANCE_COUNT, ...)
'   新版 Profiling / PM Sampling 栈（cupti_profiler_host.h）没有 instance 概念，
'   只能给出整卡聚合值。
'
'   NVIDIA 在 CUDA 12.8 把该 API 标记为 deprecated，并从 CUDA 13.0 起硬禁用：
'   cupti_events.h 原文 —— "Calling any event API will return the error code
'   CUPTI_ERROR_LEGACY_PROFILER_NOT_SUPPORTED."（错误码 38）。
'   因此本类在 CUDA 13.x 上 IsSupported() 恒为 False，上层必须降级展示；
'   在 CUDA <= 12.7 的机器上则可以直接拿到每个 SM 的活跃周期。
'
' 设计约定：
'   * 只产出纯数据；不支持时返回空表并把原因写进 UnavailableReason；
'   * 单线程使用，且需要调用方持有一个 CUDA 上下文（事件组绑定 context）；
'   * 采集模式固定为"仅统计内核执行期间"（CUPTI_EVENT_COLLECTION_MODE_KERNEL），
'     这样 Read() 得到的是"上一次 Read 之后本进程内核执行期间"的增量。
' ------------------------------------------------------------------------

Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.InteropServices

Namespace Runtime

    ''' <summary>
    ''' 单个 SM（domain instance）在一段采集窗口内的原始计数与派生利用率。
    ''' </summary>
    Public Structure PerSmCounterSample
        ''' <summary>SM 序号（CUPTI instance 索引 0..N-1）</summary>
        Public SmIndex As Integer
        ''' <summary>SM 处于活跃状态的周期数</summary>
        Public CyclesActive As ULong
        ''' <summary>经过的周期数（采集窗口的时间基准）</summary>
        Public CyclesElapsed As ULong
        ''' <summary>活跃 warp 数（若未采集该事件则为 -1）</summary>
        Public WarpsActive As Double
        ''' <summary>SM 利用率（活跃周期 / 经过周期 × 100，0~100）</summary>
        Public UtilizationPercent As Double
        ''' <summary>活跃 warp 占峰值活跃 warp 的百分比（若未采集该事件则为 -1）</summary>
        Public WarpsActivePercent As Double

        ''' <summary>该 SM 是否处于空转（利用率低于阈值）</summary>
        Public Function IsIdle(Optional thresholdPercent As Double = 5.0) As Boolean
            Return UtilizationPercent >= 0 AndAlso UtilizationPercent < thresholdPercent
        End Function
    End Structure

    ''' <summary>
    ''' per-SM 计数器采集器（旧 CUPTI Event API）。
    '''
    ''' 典型用法：
    ''' <code>
    ''' If CuptiPerSmCounter.IsSupported(device.Ordinal) Then
    '''     Using counter As New CuptiPerSmCounter()
    '''         counter.Open(context, device)
    '''         ' ... 运行内核 ...
    '''         Dim rows = counter.Read()
    '''     End Using
    ''' End If
    ''' </code>
    ''' </summary>
    Public NotInheritable Class CuptiPerSmCounter
        Implements IDisposable

        ''' <summary>SM 活跃周期事件名候选（按优先级）</summary>
        Public Shared ReadOnly CyclesActiveCandidates As String() = {"sm__cycles_active", "smsp__cycles_active"}
        ''' <summary>周期基准事件名候选</summary>
        Public Shared ReadOnly CyclesElapsedCandidates As String() = {"sm__cycles_elapsed", "smsp__cycles_elapsed"}
        ''' <summary>活跃 warp 事件名候选</summary>
        Public Shared ReadOnly WarpsActiveCandidates As String() = {"sm__warps_active", "smsp__warps_active"}

        Private Shared ReadOnly _supportCache As New Dictionary(Of Integer, Boolean)()
        Private Shared ReadOnly _supportSync As New Object()

        Private _library As CuptiLibrary
        Private _eventGroup As IntPtr = IntPtr.Zero
        Private _opened As Boolean
        Private _disposed As Boolean
        Private _reason As String = String.Empty

        Private _eventIds As Integer() = New Integer() {}
        Private _eventNames As String() = New String() {}
        Private _instanceCount As Integer
        Private _activeIndex As Integer = -1
        Private _elapsedIndex As Integer = -1
        Private _warpsIndex As Integer = -1
        Private _peakWarps As Double = -1

        ''' <summary>不可用原因（中文）；可用时为空串</summary>
        Public ReadOnly Property UnavailableReason As String
            Get
                Return _reason
            End Get
        End Property

        ''' <summary>是否已成功打开事件组</summary>
        Public ReadOnly Property IsOpen As Boolean
            Get
                Return _opened
            End Get
        End Property

        ''' <summary>事件组实际采集的 instance 数量（= 参与计数的 SM 数）</summary>
        Public ReadOnly Property InstanceCount As Integer
            Get
                Return _instanceCount
            End Get
        End Property

        ''' <summary>实际参与采集的事件名</summary>
        Public ReadOnly Property EventNames As IReadOnlyList(Of String)
            Get
                Return _eventNames
            End Get
        End Property

        ''' <summary>
        ''' 探测指定设备是否还能提供 per-SM（instance）数据。
        ''' CUDA 13.x 上任何 Event API 都会返回 38（LEGACY_PROFILER_NOT_SUPPORTED），
        ''' 本方法用 cuptiEventGetIdFromName 做最小代价的探测（不需要 CUDA 上下文），
        ''' 结果按设备缓存。
        ''' </summary>
        Public Shared Function IsSupported(deviceIndex As Integer) As Boolean
            SyncLock _supportSync
                Dim cached As Boolean
                If _supportCache.TryGetValue(deviceIndex, cached) Then Return cached

                Dim supported = ProbeSupport(deviceIndex)
                _supportCache(deviceIndex) = supported
                Return supported
            End SyncLock
        End Function

        ''' <summary>清空探测缓存（切换 cupti64 动态库之后调用）。</summary>
        Public Shared Sub ResetSupportCache()
            SyncLock _supportSync
                _supportCache.Clear()
            End SyncLock
        End Sub

        Private Shared Function ProbeSupport(deviceIndex As Integer) As Boolean
            Dim loadError As String = Nothing
            Dim library = CuptiLibrary.TryGetCurrent(loadError)

            If library Is Nothing OrElse Not library.HasLegacyEventApi Then Return False

            Try
                For Each name In CyclesActiveCandidates
                    Dim eventId As Integer = -1
                    If library.EventGetIdFromName(deviceIndex, name, eventId) = CUptiResult.CUPTI_SUCCESS Then Return True
                Next

                For Each name In CyclesElapsedCandidates
                    Dim eventId As Integer = -1
                    If library.EventGetIdFromName(deviceIndex, name, eventId) = CUptiResult.CUPTI_SUCCESS Then Return True
                Next
            Catch
            End Try

            Return False
        End Function

        ''' <summary>
        ''' 创建事件组并开启 per-instance 采集。
        ''' 失败时返回 False，原因见 <see cref="UnavailableReason"/>。
        ''' </summary>
        Public Function Open(context As CudaContext,
                             device As CudaDevice,
                             Optional extraEventNames As IEnumerable(Of String) = Nothing) As Boolean

            If _disposed Then Throw New ObjectDisposedException(NameOf(CuptiPerSmCounter))
            If _opened Then Return True
            If context Is Nothing Then
                _reason = "缺少 CUDA 上下文：per-SM 事件采集必须绑定到一个 CUDA context"
                Return False
            End If

            Dim loadError As String = Nothing
            _library = CuptiLibrary.TryGetCurrent(loadError)

            If _library Is Nothing Then
                _reason = If(String.IsNullOrEmpty(loadError), "cupti64 动态库不可用", loadError)
                Return False
            End If

            If Not _library.HasLegacyEventApi Then
                _reason = "该 cupti64 未导出旧 Event API 符号，无法采集 per-SM 数据"
                Return False
            End If

            ' 仅统计内核执行期间（与 nvprof 的 kernel 模式一致）
            If _library.SetEventCollectionMode IsNot Nothing Then
                Dim modeResult = _library.SetEventCollectionMode(context.Handle, CUptiEventCollectionMode.CUPTI_EVENT_COLLECTION_MODE_KERNEL)
                If modeResult = CUptiResult.CUPTI_ERROR_LEGACY_PROFILER_NOT_SUPPORTED Then
                    _reason = LegacyNotSupportedReason()
                    Return False
                End If
            End If

            Dim group As IntPtr = IntPtr.Zero
            Dim res = _library.EventGroupCreate(context.Handle, group, 0UI)

            If res = CUptiResult.CUPTI_ERROR_LEGACY_PROFILER_NOT_SUPPORTED Then
                _reason = LegacyNotSupportedReason()
                Return False
            End If

            If res <> CUptiResult.CUPTI_SUCCESS Then
                _reason = $"cuptiEventGroupCreate 失败：{CUptiApi.Describe(res)}"
                Return False
            End If

            _eventGroup = group

            ' 解析事件名（同一 domain 才能放进同一个事件组）
            Dim names As New List(Of String)()
            Dim ids As New List(Of Integer)()

            _activeIndex = AddEvent(device.Ordinal, CyclesActiveCandidates, names, ids)
            _elapsedIndex = AddEvent(device.Ordinal, CyclesElapsedCandidates, names, ids)
            _warpsIndex = AddEvent(device.Ordinal, WarpsActiveCandidates, names, ids)

            If extraEventNames IsNot Nothing Then
                For Each extra In extraEventNames
                    If String.IsNullOrWhiteSpace(extra) Then Continue For
                    AddEvent(device.Ordinal, {extra}, names, ids)
                Next
            End If

            If ids.Count = 0 Then
                _reason = LegacyNotSupportedReason()
                Dispose()
                Return False
            End If

            _eventNames = names.ToArray()
            _eventIds = ids.ToArray()

            ' 打开 per-instance 采集
            Dim flag As Integer = 1
            Dim flagPtr = Marshal.AllocHGlobal(4)

            Try
                Marshal.WriteInt32(flagPtr, flag)
                res = _library.EventGroupSetAttribute(_eventGroup,
                    CUptiEventGroupAttribute.CUPTI_EVENT_GROUP_ATTR_PROFILE_ALL_DOMAIN_INSTANCES,
                    New IntPtr(4), flagPtr)
            Finally
                Marshal.FreeHGlobal(flagPtr)
            End Try

            If res <> CUptiResult.CUPTI_SUCCESS Then
                _reason = $"cuptiEventGroupSetAttribute(PROFILE_ALL_DOMAIN_INSTANCES) 失败：{CUptiApi.Describe(res)}"
                Dispose()
                Return False
            End If

            _instanceCount = QueryInstanceCount()
            If _instanceCount <= 0 Then _instanceCount = device.MultiprocessorCount

            ' 峰值 warp 数（用于把 warps_active 折算成百分比）
            If device.MaxThreadsPerMultiprocessor > 0 AndAlso device.WarpSize > 0 Then
                _peakWarps = device.MaxThreadsPerMultiprocessor \ device.WarpSize
            End If

            _opened = True
            _reason = String.Empty

            ' 立即开始计数
            res = _library.EventGroupEnable(_eventGroup)
            If res <> CUptiResult.CUPTI_SUCCESS Then
                _reason = $"cuptiEventGroupEnable 失败：{CUptiApi.Describe(res)}"
                _opened = False
                Dispose()
                Return False
            End If

            _enabled = True
            Return True
        End Function

        Private _enabled As Boolean

        ''' <summary>
        ''' 读取自上次调用以来（本进程内核执行期间）各 SM 的计数，并把计数器清零后重新开始。
        ''' 未打开或采集失败时返回空表。
        ''' </summary>
        Public Function Read() As IReadOnlyList(Of PerSmCounterSample)
            Dim result As New List(Of PerSmCounterSample)()
            If _disposed OrElse Not _opened OrElse _eventGroup = IntPtr.Zero OrElse _instanceCount <= 0 Then Return result

            ' 停止 → 读取 → 清零 → 重新开启
            Try
                If _enabled Then
                    _library.EventGroupDisable(_eventGroup)
                    _enabled = False
                End If
            Catch
            End Try

            Dim active(_instanceCount - 1) As ULong
            Dim elapsed(_instanceCount - 1) As ULong
            Dim warps(_instanceCount - 1) As ULong

            Dim hasActive = TryReadEvent(_activeIndex, active)
            Dim hasElapsed = TryReadEvent(_elapsedIndex, elapsed)
            Dim hasWarps = TryReadEvent(_warpsIndex, warps)

            If Not hasActive AndAlso Not hasElapsed Then
                Restart()
                Return result
            End If

            For i As Integer = 0 To _instanceCount - 1
                Dim row As New PerSmCounterSample()
                row.SmIndex = i
                row.CyclesActive = If(hasActive, active(i), 0UL)
                row.CyclesElapsed = If(hasElapsed, elapsed(i), 0UL)
                row.WarpsActive = If(hasWarps, CDbl(warps(i)), -1)
                row.WarpsActivePercent = If(hasWarps AndAlso _peakWarps > 0, warps(i) / _peakWarps * 100.0, -1)

                If row.CyclesElapsed > 0 Then
                    row.UtilizationPercent = System.Math.Min(100.0, row.CyclesActive / CDbl(row.CyclesElapsed) * 100.0)
                Else
                    row.UtilizationPercent = -1
                End If

                result.Add(row)
            Next

            Restart()
            Return result
        End Function

        Private Sub Restart()
            If _disposed OrElse Not _opened OrElse _eventGroup = IntPtr.Zero Then Return

            Try
                If _library.EventGroupResetAllEvents IsNot Nothing Then
                    _library.EventGroupResetAllEvents(_eventGroup)
                End If

                If _library.EventGroupEnable(_eventGroup) = CUptiResult.CUPTI_SUCCESS Then
                    _enabled = True
                End If
            Catch
            End Try
        End Sub

        Private Function TryReadEvent(eventIndex As Integer, ByRef values As ULong()) As Boolean
            If eventIndex < 0 OrElse eventIndex >= _eventIds.Length Then Return False
            If _instanceCount <= 0 Then Return False

            Dim byteSize = _instanceCount * 8
            Dim buffer = Marshal.AllocHGlobal(byteSize)

            Try
                Dim bufferSize As IntPtr = New IntPtr(byteSize)
                Dim res = _library.EventGroupReadEvent(_eventGroup, CUptiReadEventFlags.CUPTI_EVENT_READ_FLAG_NONE,
                                                       _eventIds(eventIndex), bufferSize, buffer)

                If res <> CUptiResult.CUPTI_SUCCESS Then Return False

                Dim count As Integer = CInt(bufferSize.ToInt64() \ 8)
                If count <= 0 Then Return False
                If count > _instanceCount Then count = _instanceCount

                ' Marshal.Copy 没有 ULong 重载，这里按 8 字节逐个读取（SM 数量很小，开销可忽略）
                For i As Integer = 0 To count - 1
                    Dim raw = Marshal.ReadInt64(buffer, i * 8)
                    values(i) = If(raw < 0L, 0UL, CULng(raw))
                Next

                Return True
            Catch
                Return False
            Finally
                Marshal.FreeHGlobal(buffer)
            End Try
        End Function

        Private Function AddEvent(deviceIndex As Integer, candidates As String(),
                                  names As List(Of String), ids As List(Of Integer)) As Integer
            For Each name In candidates
                Dim eventId As Integer = -1
                Dim res = _library.EventGetIdFromName(deviceIndex, name, eventId)
                If res <> CUptiResult.CUPTI_SUCCESS Then Continue For

                Dim addResult = _library.EventGroupAddEvent(_eventGroup, eventId)
                If addResult <> CUptiResult.CUPTI_SUCCESS Then Continue For

                names.Add(name)
                ids.Add(eventId)
                Return ids.Count - 1
            Next

            Return -1
        End Function

        Private Function QueryInstanceCount() As Integer
            If _eventGroup = IntPtr.Zero Then Return 0

            Dim buffer = Marshal.AllocHGlobal(4)

            Try
                Dim size As IntPtr = New IntPtr(4)
                Dim res = _library.EventGroupGetAttribute(_eventGroup,
                    CUptiEventGroupAttribute.CUPTI_EVENT_GROUP_ATTR_INSTANCE_COUNT, size, buffer)

                If res = CUptiResult.CUPTI_SUCCESS Then
                    Return Marshal.ReadInt32(buffer)
                End If
            Catch
            Finally
                Marshal.FreeHGlobal(buffer)
            End Try

            Return 0
        End Function

        Private Shared Function LegacyNotSupportedReason() As String
            Return "per-SM（instance）采集不可用：CUPTI 的旧 Event/Metric API 自 CUDA 12.8 弃用、" &
                   "CUDA 13.0 起任何调用返回 CUPTI_ERROR_LEGACY_PROFILER_NOT_SUPPORTED(38)。" &
                   "如需真实的逐 SM 数据，请使用 CUDA 12.7 及更早的 CUPTI。"
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True

            If _eventGroup <> IntPtr.Zero AndAlso _library IsNot Nothing Then
                Try
                    If _enabled Then _library.EventGroupDisable(_eventGroup)
                Catch
                End Try

                Try
                    _library.EventGroupDestroy(_eventGroup)
                Catch
                End Try
            End If

            _enabled = False
            _opened = False
            _eventGroup = IntPtr.Zero
        End Sub
    End Class
End Namespace
