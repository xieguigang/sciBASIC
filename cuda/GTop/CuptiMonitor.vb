' ------------------------------------------------------------------------
' GTop 的 CUPTI 编排层（--sm / --sm --top / --sm-doctor）
'
' 职责：把 ILCuda 的两个底层能力组合成"给人看的一张表"：
'   1. CuptiPmSamplingSession —— 设备级、按时间切片的硬件指标
'      （sm__cycles_active / sm__warps_active / sm__throughput ...）；
'   2. CuptiPerSmCounter       —— 旧 Event API 的 per-SM（instance）增量计数，
'      只在 CUDA <= 12.7 的 CUPTI 上可用。
'
' 能力边界（不要试图绕过）：
'   * PM Sampling 只能给出整卡聚合值，没有每 SM 明细；
'   * 每 SM 明细依赖被 CUDA 13.0 移除的旧 API，本机（CUDA 13.x）恒不可用；
'   * CUPTI 只统计"调用进程发起的内核"与设备级计数器，不能把别的进程的
'     内核拆到具体 SM 上。
' 因此 PerSmAvailable=False 时本层不会伪造逐 SM 数据，而是用
' SmBalanceEstimate（由 sm__cycles_active.avg/.max/.min 推出的两极估计）
' 给出"整卡层面哪个 SM 可能空转"的可判读结论。
' ------------------------------------------------------------------------

Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualBasic.Computing.ILCuda.Runtime

''' <summary>CUPTI 监控的启动参数。</summary>
Public Class CuptiMonitorOptions
    ''' <summary>CUDA 设备序号（默认 0）</summary>
    Public Property DeviceIndex As Integer = 0
    ''' <summary>指标集合；Nothing 表示使用 PmSamplingMetrics.DefaultSet</summary>
    Public Property Metrics As String() = Nothing
    ''' <summary>采样间隔（毫秒，默认 1 ms）</summary>
    Public Property SamplingIntervalMs As Double = CuptiPmSamplingSession.DefaultSamplingIntervalMs
    ''' <summary>硬件缓冲字节数</summary>
    Public Property HardwareBufferBytes As Long = CuptiPmSamplingSession.DefaultHardwareBufferBytes
    ''' <summary>counter data 镜像最多容纳的采样数</summary>
    Public Property MaxSamples As Integer = CuptiPmSamplingSession.DefaultMaxSamples
    ''' <summary>强制使用 SYSCLK 触发模式（排查纳秒触发不被驱动接受的情况）</summary>
    Public Property ForceSystemClockTrigger As Boolean = False
End Class

''' <summary>单个 SM 的聚合行（仅在 per-SM 可用时存在）。</summary>
Public Structure SmRow
    ''' <summary>SM 序号</summary>
    Public SmIndex As Integer
    ''' <summary>利用率（0~100，-1 = 未知）</summary>
    Public UtilizationPercent As Double
    ''' <summary>活跃 warp 占峰值百分比（-1 = 未知）</summary>
    Public WarpsActivePercent As Double
    ''' <summary>SM 吞吐百分比（-1 = 未知）</summary>
    Public ThroughputPercent As Double

    ''' <summary>该 SM 是否空转（默认阈值 5%）</summary>
    Public Function IsIdle(Optional thresholdPercent As Double = 5.0) As Boolean
        Return UtilizationPercent >= 0 AndAlso UtilizationPercent < thresholdPercent
    End Function
End Structure

''' <summary>一个 PM 采样切片的摘要（整卡聚合值）。</summary>
Public Structure SmSliceSummary
    ''' <summary>切片序号</summary>
    Public Index As Integer
    ''' <summary>切片时长（毫秒）</summary>
    Public DurationMs As Double
    ''' <summary>平均 SM 利用率（0~100，-1 = 未知）</summary>
    Public UtilizationPercent As Double
    ''' <summary>活跃 warp 占峰值百分比（-1 = 未知）</summary>
    Public WarpsActivePercent As Double
    ''' <summary>SM 吞吐百分比（-1 = 未知）</summary>
    Public ThroughputPercent As Double
    ''' <summary>每 SM 平均活跃周期</summary>
    Public CyclesActiveAvg As Double
    ''' <summary>由 avg/max/min 推出的均衡估计</summary>
    Public Balance As SmBalanceEstimate
End Structure

''' <summary>
''' 一次采集/刷新的完整结果。<see cref="Rows"/> 非空才算拿到了真实的 per-SM 数据；
''' 否则应展示 <see cref="Aggregate"/> 的整卡结论与 <see cref="Reason"/> 说明。
''' </summary>
Public Class SmSnapshot
    ''' <summary>本次会话的能力与配置</summary>
    Public Property Capability As PmSamplingCapability
    ''' <summary>设备名（可能为空）</summary>
    Public Property DeviceName As String
    ''' <summary>设备序号</summary>
    Public Property DeviceIndex As Integer
    ''' <summary>SM 数量（0 = 未知）</summary>
    Public Property SmCount As Integer
    ''' <summary>是否拿到了真实的逐 SM 数据</summary>
    Public Property PerSmAvailable As Boolean
    ''' <summary>逐 SM 数据（PerSmAvailable=False 时为空）</summary>
    Public Property Rows As IReadOnlyList(Of SmRow)
    ''' <summary>PM 采样切片（按时间顺序）</summary>
    Public Property Slices As IReadOnlyList(Of SmSliceSummary)
    ''' <summary>跨切片加权平均</summary>
    Public Property Aggregate As SmSliceSummary
    ''' <summary>采集时长（秒）</summary>
    Public Property CaptureSeconds As Double
    ''' <summary>有效切片数</summary>
    Public Property SampleCount As Integer
    ''' <summary>硬件缓冲是否溢出过（溢出时应增大缓冲或采样间隔）</summary>
    Public Property Overflow As Boolean
    ''' <summary>降级/失败原因（中文）；一切正常时为空串</summary>
    Public Property Reason As String

    ''' <summary>空转 SM 数量（per-SM 可用时为精确值，否则取两极估计，-1 = 未知）</summary>
    Public ReadOnly Property IdleCount As Integer
        Get
            If Rows IsNot Nothing AndAlso Rows.Count > 0 Then
                Return Rows.Where(Function(r) r.IsIdle()).Count()
            End If

            Return Aggregate.Balance.IdleSmEstimate
        End Get
    End Property

    ''' <summary>最热的 <paramref name="count"/> 个 SM（按利用率降序；per-SM 不可用时为空）</summary>
    Public Function TopHot(Optional count As Integer = 3) As IReadOnlyList(Of SmRow)
        If Rows Is Nothing Then Return New SmRow() {}
        Return Rows.OrderByDescending(Function(r) r.UtilizationPercent).Take(count).ToArray()
    End Function

    ''' <summary>最冷的 <paramref name="count"/> 个 SM（按利用率升序；per-SM 不可用时为空）</summary>
    Public Function TopCold(Optional count As Integer = 3) As IReadOnlyList(Of SmRow)
        If Rows Is Nothing Then Return New SmRow() {}
        Return Rows.OrderBy(Function(r) r.UtilizationPercent).Take(count).ToArray()
    End Function
End Class

''' <summary>
''' CUPTI 采集编排器：创建会话 → 采集/刷新 → 产出 <see cref="SmSnapshot"/>。
''' 线程不安全，请单线程使用（与 Nvml / GpuProcessCounter 一致）。
''' </summary>
Public NotInheritable Class CuptiMonitor
    Implements IDisposable

    Private ReadOnly _options As CuptiMonitorOptions
    Private ReadOnly _session As CuptiPmSamplingSession
    Private ReadOnly _slices As New List(Of SmSliceSummary)()

    Private _device As CudaDevice
    Private _context As CudaContext
    Private _smCounter As CuptiPerSmCounter
    Private _perSmRows As New List(Of SmRow)()
    Private _perSmReason As String = String.Empty
    Private _openError As String = String.Empty
    Private _disposed As Boolean

    ''' <summary>实时模式下保留的最大切片数</summary>
    Public Property MaxSlices As Integer = 240

    Public Sub New(Optional options As CuptiMonitorOptions = Nothing)
        _options = If(options Is Nothing, New CuptiMonitorOptions(), options)
        _session = New CuptiPmSamplingSession()
        _session.ForceSystemClockTrigger = _options.ForceSystemClockTrigger

        ' 1) 先初始化 CUDA 驱动并建立本进程上下文：
        '    官方 pm_sampling 示例在 cuptiPmSamplingStart 之前就已经创建了 CUDA 上下文，
        '    否则 cuptiPmSamplingStart 会返回 CUPTI_ERROR_UNKNOWN（999）。
        Try
            Dim devices = CudaDevice.Enumerate()

            If _options.DeviceIndex >= 0 AndAlso _options.DeviceIndex < devices.Count Then
                _device = devices(_options.DeviceIndex)
            End If

            If _device IsNot Nothing Then
                _context = New CudaContext(_device)
                _context.MakeCurrent()
            End If
        Catch ex As Exception
            _openError = $"初始化 CUDA 上下文失败：{ex.Message}"
        End Try

        ' 2) 打开 CUPTI 会话（内部会调用 cuptiProfilerInitialize）
        Try
            _session.Open(_options.DeviceIndex, _options.Metrics, _options.SamplingIntervalMs,
                          _options.HardwareBufferBytes, _options.MaxSamples)
        Catch ex As Exception
            ' Open 内部已把常规失败转成 Capability.Reason，这里只兜住意外异常
            _openError = ex.Message
        End Try

        If _session.Capability.Available AndAlso _session.Capability.LegacyPerSmSupported Then
            OpenPerSmCounter()
        ElseIf Not _session.Capability.LegacyPerSmSupported Then
            _perSmReason = "per-SM 明细不可用：CUPTI 旧 Event API 自 CUDA 12.8 弃用、13.0 起调用即返回 " &
                           "CUPTI_ERROR_LEGACY_PROFILER_NOT_SUPPORTED(38)；PM Sampling 只有整卡聚合值。"
        End If
    End Sub

    ''' <summary>本次会话的能力与配置</summary>
    Public ReadOnly Property Capability As PmSamplingCapability
        Get
            Return _session.Capability
        End Get
    End Property

    ''' <summary>设备名（无法获取时为空串）</summary>
    Public ReadOnly Property DeviceName As String
        Get
            Return If(_device Is Nothing, String.Empty, _device.Name)
        End Get
    End Property

    ''' <summary>最近一次 cuptiPmSamplingStart 的返回码（诊断用，0 = 成功）</summary>
    Public ReadOnly Property LastStartResultCode As Integer
        Get
            Return CInt(_session.LastStartResult)
        End Get
    End Property

    ''' <summary>最近一次 cuptiPmSamplingSetConfig 的返回码（诊断用，0 = 成功）</summary>
    Public ReadOnly Property LastSetConfigResultCode As Integer
        Get
            Return CInt(_session.LastSetConfigResult)
        End Get
    End Property

    ''' <summary>最近一次解码写入的采样条数（诊断用）</summary>
    Public ReadOnly Property LastDecodePopulatedSamples As Integer
        Get
            Return _session.LastDecodePopulatedSamples
        End Get
    End Property

    ''' <summary>最近一次失败的中文描述（诊断用）</summary>
    Public ReadOnly Property LastError As String
        Get
            Return _session.LastError
        End Get
    End Property

    ''' <summary>SM 数量（0 = 未知）</summary>
    Public ReadOnly Property SmCount As Integer
        Get
            If _device Is Nothing Then Return 0
            Return _device.MultiprocessorCount
        End Get
    End Property

    ''' <summary>
    ''' 采集一段时间（毫秒）内的全部切片并聚合，用于一次性模式。
    ''' </summary>
    Public Function Capture(durationMs As Integer) As SmSnapshot
        Dim watch = Diagnostics.Stopwatch.StartNew()

        _slices.Clear()
        _perSmRows.Clear()

        If _session.IsOpen Then
            _session.Start()

            Dim lastPoll As Long = 0

            While watch.ElapsedMilliseconds < durationMs
                Threading.Thread.Sleep(50)

                ' 每 200 ms 解码一次，避免一段长采集里硬件缓冲被写满
                If watch.ElapsedMilliseconds - lastPoll >= 200 Then
                    lastPoll = watch.ElapsedMilliseconds
                    AppendSamples(_session.Poll())
                End If
            End While

            AppendSamples(_session.StopAndFlush())
        End If

        ReadPerSm()

        Return BuildSnapshot(watch.Elapsed.TotalSeconds)
    End Function

    ''' <summary>
    ''' 实时模式：取走自上次刷新以来新产生的切片并聚合（切片历史滚动保留）。
    ''' </summary>
    Public Function Refresh() As SmSnapshot
        Dim watch = Diagnostics.Stopwatch.StartNew()

        If _session.IsOpen Then
            AppendSamples(_session.Poll())
        End If

        ReadPerSm()

        Return BuildSnapshot(watch.Elapsed.TotalSeconds)
    End Function

    ' ------------------------------------------------------------------
    ' 内部实现
    ' ------------------------------------------------------------------

    Private Sub OpenPerSmCounter()
        If _device Is Nothing OrElse _context Is Nothing Then
            _perSmReason = "无法获取 CUDA 设备/上下文，per-SM 采集不可用"
            Return
        End If

        Try
            _context.MakeCurrent()

            Dim counter As New CuptiPerSmCounter()

            If counter.Open(_context, _device) Then
                _smCounter = counter
            Else
                _perSmReason = counter.UnavailableReason
                counter.Dispose()
            End If
        Catch ex As Exception
            _perSmReason = $"打开 per-SM 事件组失败：{ex.Message}"
        End Try
    End Sub

    Private Sub ReadPerSm()
        _perSmRows.Clear()
        If _smCounter Is Nothing Then Return

        Try
            Dim samples = _smCounter.Read()
            If samples Is Nothing OrElse samples.Count = 0 Then Return

            For Each item In samples
                Dim row As New SmRow()
                row.SmIndex = item.SmIndex
                row.UtilizationPercent = item.UtilizationPercent
                row.WarpsActivePercent = item.WarpsActivePercent
                row.ThroughputPercent = -1
                _perSmRows.Add(row)
            Next
        Catch ex As Exception
            _perSmReason = $"读取 per-SM 计数失败：{ex.Message}"
            _perSmRows.Clear()
        End Try
    End Sub

    Private Sub AppendSamples(samples As IReadOnlyList(Of PmSamplingSample))
        If samples Is Nothing Then Return

        For Each sample In samples
            If Not sample.IsValid Then Continue For

            _slices.Add(Summarize(sample))

            While _slices.Count > System.Math.Max(1, MaxSlices)
                _slices.RemoveAt(0)
            End While
        Next
    End Sub

    Private Function Summarize(sample As PmSamplingSample) As SmSliceSummary
        Dim item As New SmSliceSummary()
        item.Index = sample.Index
        item.DurationMs = sample.DurationMs
        item.CyclesActiveAvg = sample.ValueOf(PmSamplingMetrics.CyclesActiveAvg)
        item.WarpsActivePercent = sample.ValueOf(PmSamplingMetrics.WarpsActivePercent, -1)
        item.ThroughputPercent = sample.ValueOf(PmSamplingMetrics.ThroughputPercent, -1)
        item.UtilizationPercent = UtilizationOf(sample)
        item.Balance = CuptiPmSamplingSession.EstimateBalance(sample, SmCount)

        Return item
    End Function

    ''' <summary>整卡平均 SM 利用率：优先用 active.avg / elapsed.avg，退化为 active.avg / active.max。</summary>
    Private Shared Function UtilizationOf(sample As PmSamplingSample) As Double
        Dim active = sample.ValueOf(PmSamplingMetrics.CyclesActiveAvg)
        Dim elapsed = sample.ValueOf(PmSamplingMetrics.CyclesElapsedAvg)

        If elapsed <= 0 Then elapsed = sample.ValueOf(PmSamplingMetrics.CyclesActiveMax)
        If elapsed <= 0 Then Return -1

        Return System.Math.Min(100.0, active / elapsed * 100.0)
    End Function

    Private Function BuildSnapshot(captureSeconds As Double) As SmSnapshot
        Dim snapshot As New SmSnapshot()
        snapshot.Capability = _session.Capability
        snapshot.DeviceName = DeviceName
        snapshot.DeviceIndex = _options.DeviceIndex
        snapshot.SmCount = SmCount
        snapshot.CaptureSeconds = captureSeconds
        snapshot.Slices = _slices.ToArray()
        snapshot.SampleCount = _slices.Count
        snapshot.Overflow = _session.LastDecodeOverflow
        snapshot.Rows = _perSmRows.ToArray()
        snapshot.PerSmAvailable = snapshot.Rows.Count > 0
        snapshot.Aggregate = Aggregate(_slices)

        If Not String.IsNullOrEmpty(_openError) Then
            snapshot.Reason = _openError
        ElseIf Not _session.Capability.Available Then
            snapshot.Reason = _session.Capability.Reason
        ElseIf snapshot.SampleCount = 0 AndAlso Not String.IsNullOrEmpty(_session.LastError) Then
            ' 会话配置成功但采样没能启动/解码出数据：把底层错误如实带出来
            snapshot.Reason = _session.LastError
        ElseIf Not snapshot.PerSmAvailable Then
            snapshot.Reason = _perSmReason
        Else
            snapshot.Reason = String.Empty
        End If

        Return snapshot
    End Function

    Private Shared Function Aggregate(slices As IReadOnlyList(Of SmSliceSummary)) As SmSliceSummary
        Dim result As New SmSliceSummary()
        result.Index = -1
        result.UtilizationPercent = -1
        result.WarpsActivePercent = -1
        result.ThroughputPercent = -1

        ' 结构体默认值是 0，这里统一改成 -1（未知），避免界面把"没有数据"显示成 0%
        Dim void As New SmBalanceEstimate()
        void.HotSmPercent = -1
        void.ColdSmPercent = -1
        void.AveragePercent = -1
        void.ImbalanceRatio = -1
        void.IdleSmEstimate = -1
        void.AverageActiveSm = -1
        void.IsEstimated = False
        result.Balance = void

        If slices Is Nothing OrElse slices.Count = 0 Then Return result

        Dim totalWeight As Double = 0
        Dim utilSum As Double = 0
        Dim warpsSum As Double = 0
        Dim warpsWeight As Double = 0
        Dim thrSum As Double = 0
        Dim thrWeight As Double = 0
        Dim activeSum As Double = 0
        Dim hotSum As Double = 0
        Dim coldSum As Double = 0
        Dim avgSum As Double = 0
        Dim imbalanceSum As Double = 0
        Dim estimated As Integer = 0
        Dim idleSum As Integer = 0

        For Each item In slices
            Dim weight = If(item.DurationMs > 0, item.DurationMs, 1.0)
            totalWeight += weight
            result.DurationMs += item.DurationMs

            If item.UtilizationPercent >= 0 Then utilSum += item.UtilizationPercent * weight

            If item.WarpsActivePercent >= 0 Then
                warpsSum += item.WarpsActivePercent * weight
                warpsWeight += weight
            End If

            If item.ThroughputPercent >= 0 Then
                thrSum += item.ThroughputPercent * weight
                thrWeight += weight
            End If

            activeSum += item.CyclesActiveAvg

            If item.Balance.IsEstimated Then
                estimated += 1
                hotSum += item.Balance.HotSmPercent
                coldSum += item.Balance.ColdSmPercent
                avgSum += item.Balance.AveragePercent
                imbalanceSum += item.Balance.ImbalanceRatio
                idleSum += item.Balance.IdleSmEstimate
            End If
        Next

        If totalWeight > 0 Then
            result.UtilizationPercent = utilSum / totalWeight
        End If

        If warpsWeight > 0 Then result.WarpsActivePercent = warpsSum / warpsWeight
        If thrWeight > 0 Then result.ThroughputPercent = thrSum / thrWeight

        result.CyclesActiveAvg = activeSum / slices.Count

        Dim balance As New SmBalanceEstimate()
        balance.SmCount = If(slices(0).Balance.SmCount > 0, slices(0).Balance.SmCount, 0)
        balance.HotSmPercent = -1
        balance.ColdSmPercent = -1
        balance.AveragePercent = -1
        balance.ImbalanceRatio = -1
        balance.IdleSmEstimate = -1
        balance.IsEstimated = False

        If estimated > 0 Then
            balance.IsEstimated = True
            balance.HotSmPercent = hotSum / estimated
            balance.ColdSmPercent = coldSum / estimated
            balance.AveragePercent = If(result.UtilizationPercent >= 0, result.UtilizationPercent, avgSum / estimated)
            balance.ImbalanceRatio = imbalanceSum / estimated
            balance.IdleSmEstimate = CInt(System.Math.Round(idleSum / CDbl(estimated)))
            balance.AverageActiveSm = balance.AveragePercent / 100.0 * balance.SmCount
        ElseIf result.UtilizationPercent >= 0 AndAlso balance.SmCount > 0 Then
            balance.AveragePercent = result.UtilizationPercent
            balance.AverageActiveSm = result.UtilizationPercent / 100.0 * balance.SmCount
        End If

        result.Balance = balance
        Return result
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True

        If _smCounter IsNot Nothing Then
            Try : _smCounter.Dispose() : Catch : End Try
            _smCounter = Nothing
        End If


        If _context IsNot Nothing Then
            Try : _context.Dispose() : Catch : End Try
            _context = Nothing
        End If

        _session.Dispose()
    End Sub
End Class
