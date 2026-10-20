' ------------------------------------------------------------------------
' CUPTI PM Sampling（性能计数器周期采样）的托管封装
'
' 数据来源：cupti_pmsampling.h + cupti_profiler_host.h（CUDA 12.x/13.x）
'   一次会话 = 一组硬件指标 + 一个硬件缓冲 + 一个 counter data 镜像；
'   GPU 按固定间隔把指标快照写入硬件缓冲，我们把缓冲解码成
'   "带起止时间戳的采样切片（sample）"，再用 profiler host 求出每个
'   指标在 GPU 上的一次取值（double）。
'
' 关键事实（务必理解，直接影响能力边界）：
'   1. cuptiProfilerHostEvaluateToGpuValues 对每个指标只输出 1 个 double，
'      也就是"整卡聚合值"——PM Sampling 没有 instance（每 SM）维度；
'   2. 真正能拆到每 SM 的是旧 Event API（见 CuptiPerSmEvents.vb），
'      但它在 CUDA 13.0 起被硬禁用（错误码 38）；
'   3. PM Sampling 的 config image 必须是"单 pass"，否则
'      cuptiPmSamplingSetConfig 返回 CUPTI_ERROR_NOT_SUPPORTED。因此本类
'      在 Open() 时会用 cuptiProfilerHostGetNumOfPasses 校验，并做贪心裁剪；
'   4. 采样与 Nsight Compute / 其它 profiler 互斥，且通常需要管理员权限
'      （CUPTI_ERROR_INSUFFICIENT_PRIVILEGES）。
'
' 设计约定（与 Nvml.vb / GpuProcessCounters.vb 一致）：
'   * 只产出纯数据，不依赖 Console；
'   * 所有失败都收敛为 capability.Reason（中文），绝不把异常抛给上层；
'   * 单线程使用（CUPTI profiling 会话本身要求串行）。
' ------------------------------------------------------------------------

Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.InteropServices

Namespace Runtime

    ''' <summary>
    ''' PM Sampling 能力探测结果。Available=False 时 <see cref="Reason"/>
    ''' 给出中文原因（缺库 / 缺符号 / 权限不足 / 芯片不支持 / 指标不可单 pass 等）。
    ''' </summary>
    Public Structure PmSamplingCapability
        ''' <summary>本次会话是否可用（= 会话已成功打开并配置）</summary>
        Public Available As Boolean
        ''' <summary>旧 Event API 是否能提供 per-SM（instance）数据；CUDA 13.x 恒为 False</summary>
        Public LegacyPerSmSupported As Boolean
        ''' <summary>CUPTI 识别到的芯片名（例如 GA104）</summary>
        Public ChipName As String
        ''' <summary>cupti64 版本号（来自 cuptiGetVersion 或文件名）</summary>
        Public CuptiVersion As String
        ''' <summary>实际载入的 cupti64 动态库路径</summary>
        Public CuptiPath As String
        ''' <summary>不可用时的中文原因；可用时为空串</summary>
        Public Reason As String
        ''' <summary>实际参与采集的指标名（已通过单 pass 校验）</summary>
        Public Metrics As String()
        ''' <summary>因超出单 pass 限制而被裁掉的指标名</summary>
        Public SkippedMetrics As String()
        ''' <summary>采样触发间隔（毫秒，按实际生效的触发模式折算）</summary>
        Public SamplingIntervalMs As Double
        ''' <summary>该芯片单 pass 可调度的硬件指标数上限（查询失败为 0）</summary>
        Public SinglePassLimit As Integer
        ''' <summary>是否退化为 SYSCLK 触发模式（Turing / GA100 不支持纳秒触发）</summary>
        Public UsingSysClockTrigger As Boolean
        ''' <summary>
        ''' cuptiProfilerDeviceSupported 的诊断文本（架构/SLI/vGPU/WSL/SKU 等支持级别），
        ''' 用于解释"为什么 PM Sampling 起不来"。
        ''' </summary>
        Public DeviceSupported As String

        Public ReadOnly Property HasLegacyGap As Boolean
            Get
                ' 有 PM 采样但拿不到 per-SM：需要在上层明确提示降级
                Return Available AndAlso Not LegacyPerSmSupported
            End Get
        End Property
    End Structure

    ''' <summary>
    ''' 一次 PM 采样切片：一个时间窗口内、整卡各指标的取值。
    ''' </summary>
    Public Structure PmSamplingSample
        ''' <summary>切片序号（同一会话内递增）</summary>
        Public Index As Integer
        ''' <summary>窗口起始时间戳（ns，GPU 时钟域）</summary>
        Public StartTimestampNs As ULong
        ''' <summary>窗口结束时间戳（ns，GPU 时钟域）</summary>
        Public EndTimestampNs As ULong
        ''' <summary>指标名 → 取值</summary>
        Public Values As IReadOnlyDictionary(Of String, Double)

        ''' <summary>窗口长度（毫秒）；时间戳异常时返回 0</summary>
        Public ReadOnly Property DurationMs As Double
            Get
                If EndTimestampNs <= StartTimestampNs Then Return 0
                Return (EndTimestampNs - StartTimestampNs) / 1000000.0
            End Get
        End Property

        ''' <summary>时间戳是否有效（CUPTI 在空槽位会给出 UINT64_MAX）</summary>
        Public ReadOnly Property IsValid As Boolean
            Get
                Return StartTimestampNs <> ULong.MaxValue AndAlso
                       EndTimestampNs <> ULong.MaxValue AndAlso
                       EndTimestampNs > StartTimestampNs
            End Get
        End Property

        ''' <summary>取指标值；不存在时返回 <paramref name="fallback"/></summary>
        Public Function ValueOf(metricName As String, Optional fallback As Double = 0) As Double
            If Values Is Nothing Then Return fallback

            Dim v As Double
            If Values.TryGetValue(metricName, v) Then Return v
            Return fallback
        End Function

        ''' <summary>该切片是否包含指定指标</summary>
        Public Function Has(metricName As String) As Boolean
            If Values Is Nothing Then Return False
            Return Values.ContainsKey(metricName)
        End Function
    End Structure

    ''' <summary>
    ''' 默认指标集合与指标名规范化工具。
    '''
    ''' 用户口语里的 "sm__cycles_active" / "sm__throughput" 是基名，
    ''' 求值必须带 rollup 后缀（.avg / .max / .min / .sum）或百分比后缀，
    ''' 因此这里统一补全；顺序即"优先级顺序"，单 pass 容量不足时按此裁剪。
    ''' </summary>
    Public Module PmSamplingMetrics

        ''' <summary>活跃周期（每 SM 平均）</summary>
        Public Const CyclesActiveAvg As String = "sm__cycles_active.avg"
        ''' <summary>活跃周期（最热 SM）</summary>
        Public Const CyclesActiveMax As String = "sm__cycles_active.max"
        ''' <summary>活跃周期（最冷 SM）</summary>
        Public Const CyclesActiveMin As String = "sm__cycles_active.min"
        ''' <summary>经过周期（每 SM 平均），用于把活跃周期归一化成利用率</summary>
        Public Const CyclesElapsedAvg As String = "sm__cycles_elapsed.avg"
        ''' <summary>活跃 warp 占峰值活跃 warp 的百分比（占用率代理）</summary>
        Public Const WarpsActivePercent As String = "sm__warps_active.avg.pct_of_peak_sustained_active"
        ''' <summary>
        ''' SM 发射槽利用率（占峰值持续活跃的百分比）——"SM 吞吐"在 PM Sampling 下的代理指标。
        '''
        ''' 说明：<c>sm__throughput.avg.pct_of_peak_sustained_*</c> 是复合吞吐指标，
        ''' 实测在 GA104 上即使单独请求也需要多 pass，而 PM Sampling 只接受单 pass 配置
        ''' （否则 cuptiPmSamplingSetConfig 返回 CUPTI_ERROR_NOT_SUPPORTED），
        ''' 因此默认集合用 issue_active 作为可用的吞吐/忙碌度代理。
        ''' </summary>
        Public Const ThroughputPercent As String = "sm__issue_active.avg.pct_of_peak_sustained_active"
        ''' <summary>复合吞吐指标全名（仅作说明/显式请求用；PM Sampling 通常无法单 pass 采集）</summary>
        Public Const CompositeThroughputMetric As String = "sm__throughput.avg.pct_of_peak_sustained_elapsed"
        ''' <summary>每活跃周期的指令数（IPC，指令吞吐代理）</summary>
        Public Const InstPerCycle As String = "sm__inst_executed.avg.per_cycle_active"

        Private _defaultSet As String() = {
            CyclesActiveAvg,
            WarpsActivePercent,
            ThroughputPercent,
            CyclesElapsedAvg,
            CyclesActiveMax,
            CyclesActiveMin
        }

        ''' <summary>
        ''' 默认指标集合（已按优先级排序）：
        ''' 活跃周期、warp 占用率、SM 吞吐、经过周期、最热/最冷 SM 的活跃周期。
        ''' 单 pass 容量不足时按此顺序保留前若干个。
        ''' </summary>
        Public ReadOnly Property DefaultSet As IReadOnlyList(Of String)
            Get
                Return _defaultSet
            End Get
        End Property

        ''' <summary>
        ''' 把用户输入的基名补全为可求值的完整指标名；已带后缀的原样返回。
        ''' </summary>
        Public Function Normalize(metricName As String) As String
            If String.IsNullOrWhiteSpace(metricName) Then Return String.Empty

            Dim name = metricName.Trim()

            Select Case name
                Case "sm__cycles_active" : Return CyclesActiveAvg
                Case "sm__cycles_elapsed" : Return CyclesElapsedAvg
                Case "sm__warps_active" : Return WarpsActivePercent
                ' 复合吞吐指标无法单 pass 采集，统一映射到可用的发射槽利用率代理
                Case "sm__throughput", CompositeThroughputMetric : Return ThroughputPercent
                Case "sm__issue_active" : Return ThroughputPercent
                Case "sm__inst_executed" : Return InstPerCycle
            End Select

            ' 形如 sm__xxx 且完全没有 "." 的裸名：补 .avg
            If name.StartsWith("sm__", StringComparison.Ordinal) AndAlso Not name.Contains("."c) Then
                Return name & ".avg"
            End If

            Return name
        End Function

        ''' <summary>批量规范化并去重（保持输入顺序）。</summary>
        Public Function NormalizeAll(metricNames As IEnumerable(Of String)) As String()
            If metricNames Is Nothing Then Return _defaultSet.ToArray()

            Return metricNames _
                .Select(AddressOf Normalize) _
                .Where(Function(x) Not String.IsNullOrEmpty(x)) _
                .Distinct(StringComparer.Ordinal) _
                .ToArray()
        End Function
    End Module

    ''' <summary>
    ''' 由 PM 采样切片推断的 SM 负载均衡情况。
    '''
    ''' 说明：PM Sampling 只有整卡聚合值（avg / max / min），没有逐 SM 明细。
    ''' 这里用"两极模型"做一次粗估：假设非均衡分布只在最热与最冷之间，
    ''' 则由 avg = p·max + (1-p)·min 反解出忙碌 SM 占比 p。
    ''' 这是估计值，<see cref="IsEstimated"/> = False 表示连 avg/max/min 都不全，
    ''' 此时所有推断字段都是 -1（未知）。
    ''' </summary>
    Public Structure SmBalanceEstimate
        ''' <summary>设备的 SM 数量（CU_DEVICE_ATTRIBUTE_MULTIPROCESSOR_COUNT）</summary>
        Public SmCount As Integer
        ''' <summary>平均活跃 SM 数（= 平均利用率 × SM 数）</summary>
        Public AverageActiveSm As Double
        ''' <summary>推断的空转 SM 数（-1 = 未知）</summary>
        Public IdleSmEstimate As Integer
        ''' <summary>最热 SM 的活跃度（0~100，-1 = 未知）</summary>
        Public HotSmPercent As Double
        ''' <summary>最冷 SM 的活跃度（0~100，-1 = 未知）</summary>
        Public ColdSmPercent As Double
        ''' <summary>平均活跃度（0~100，-1 = 未知）</summary>
        Public AveragePercent As Double
        ''' <summary>失衡比 = (max - min) / max（0 = 完全均衡）</summary>
        Public ImbalanceRatio As Double
        ''' <summary>结论是否来自完整的 avg/max/min 数据</summary>
        Public IsEstimated As Boolean
    End Structure

    ''' <summary>
    ''' CUPTI PM Sampling 会话：Open → Start → Poll（可反复）→ StopAndFlush → Dispose。
    '''
    ''' 典型用法：
    ''' <code>
    ''' Using session As New CuptiPmSamplingSession()
    '''     If session.Open(0) Then
    '''         session.Start()
    '''         ' ... 让 GPU 跑负载 ...
    '''         For Each sample In session.Poll()
    '''         Next
    '''     End If
    ''' End Using
    ''' </code>
    ''' </summary>
    Public NotInheritable Class CuptiPmSamplingSession
        Implements IDisposable

        ''' <summary>
        ''' 硬件缓冲默认 64 MB。
        '''
        ''' 实测（GA104 / 1 ms 采样 / 6 指标）：4 MB 在 GPU 满载时会出现记录溢出，
        ''' 导致 cuptiPmSamplingDecodeData 返回 CUPTI_ERROR_OUT_OF_MEMORY(8)；
        ''' 64 MB 在同样负载下可稳定连续采样。
        ''' </summary>
        Public Const DefaultHardwareBufferBytes As Long = 64L << 20
        ''' <summary>counter data 镜像默认最多容纳 4096 个采样</summary>
        Public Const DefaultMaxSamples As Integer = 4096
        ''' <summary>默认采样间隔（毫秒）</summary>
        Public Const DefaultSamplingIntervalMs As Double = 1.0

        Private _library As CuptiLibrary
        Private _hostObject As IntPtr = IntPtr.Zero
        Private _pmObject As IntPtr = IntPtr.Zero
        Private _profilerInitialized As Boolean
        Private _hostInitialized As Boolean
        Private _pmEnabled As Boolean
        Private _running As Boolean
        Private _disposed As Boolean

        Private _deviceIndex As Integer
        Private _metrics As String() = New String() {}
        Private _configImage As CuptiApi.CuptiByteBuffer
        Private _counterImage As CuptiApi.CuptiByteBuffer
        Private _capability As PmSamplingCapability
        Private _samplingIntervalMs As Double = DefaultSamplingIntervalMs
        Private _hwBufferBytes As Long = DefaultHardwareBufferBytes
        Private _maxSamples As Integer = DefaultMaxSamples
        Private _sampleCounter As Integer
        Private _usingSysClock As Boolean
        Private _singlePassLimit As Integer
        Private _configApplied As Boolean
        Private _everStarted As Boolean

        ''' <summary>最近一次解码的停止原因（诊断用）</summary>
        Public ReadOnly Property LastDecodeStopReason As CUptiPmSamplingDecodeStopReason
            Get
                Return _lastDecodeStopReason
            End Get
        End Property

        ''' <summary>最近一次解码时硬件缓冲是否溢出（诊断用，溢出后应增大缓冲或采样间隔）</summary>
        Public ReadOnly Property LastDecodeOverflow As Boolean
            Get
                Return _lastDecodeOverflow
            End Get
        End Property

        Private _lastDecodeStopReason As CUptiPmSamplingDecodeStopReason
        Private _lastDecodeOverflow As Boolean
        Private _lastDecodePopulated As Integer
        Private _lastStartResult As CUptiResult = CUptiResult.CUPTI_SUCCESS
        Private _lastSetConfigResult As CUptiResult = CUptiResult.CUPTI_SUCCESS
        Private _lastError As String = String.Empty

        ''' <summary>最近一次 cuptiPmSamplingSetConfig 的返回码（诊断用）</summary>
        Public ReadOnly Property LastSetConfigResult As CUptiResult
            Get
                Return _lastSetConfigResult
            End Get
        End Property

        ''' <summary>最近一次 cuptiPmSamplingStart 的返回码（诊断用）</summary>
        Public ReadOnly Property LastStartResult As CUptiResult
            Get
                Return _lastStartResult
            End Get
        End Property

        ''' <summary>最近一次解码时写入的采样条数（诊断用）</summary>
        Public ReadOnly Property LastDecodePopulatedSamples As Integer
            Get
                Return _lastDecodePopulated
            End Get
        End Property

        ''' <summary>最近一次失败的中文描述（诊断用）；成功时为空串</summary>
        Public ReadOnly Property LastError As String
            Get
                Return _lastError
            End Get
        End Property

        ''' <summary>本次会话的能力与配置结果</summary>
        Public ReadOnly Property Capability As PmSamplingCapability
            Get
                Return _capability
            End Get
        End Property

        ''' <summary>设备序号（CUDA 设备索引）</summary>
        Public ReadOnly Property DeviceIndex As Integer
            Get
                Return _deviceIndex
            End Get
        End Property

        ''' <summary>实际参与采集的指标名</summary>
        Public ReadOnly Property Metrics As IReadOnlyList(Of String)
            Get
                Return _metrics
            End Get
        End Property

        ''' <summary>会话是否已经成功打开（可用）</summary>
        Public ReadOnly Property IsOpen As Boolean
            Get
                Return _pmEnabled
            End Get
        End Property

        ''' <summary>
        ''' 强制使用 SYSCLK 触发模式（默认优先纳秒触发）。
        ''' 某些驱动组合下纳秒触发会被接受但随后 Start 失败，可用此开关排查。
        ''' </summary>
        Public Property ForceSystemClockTrigger As Boolean = False

        ''' <summary>采样是否正在运行</summary>
        Public ReadOnly Property IsRunning As Boolean
            Get
                Return _running
            End Get
        End Property

        ''' <summary>
        ''' 只做一次能力探测（不会留下任何会话）：载入 cupti64、取芯片名、
        ''' 查询 counter availability、尝试构建单 pass 配置，然后全部释放。
        ''' </summary>
        Public Shared Function Probe(deviceIndex As Integer,
                                     Optional metricNames As IEnumerable(Of String) = Nothing) As PmSamplingCapability
            Using session As New CuptiPmSamplingSession()
                session.Open(deviceIndex, metricNames, probeOnly:=True)
                Return session.Capability
            End Using
        End Function

        ''' <summary>
        ''' 打开会话：载入 CUPTI → 初始化 profiler → 取芯片名与 counter availability
        ''' → 构建单 pass config image → Enable PM sampling → 分配 counter data 镜像。
        ''' 失败时返回 False，原因在 <see cref="Capability"/>.Reason（中文）。
        ''' </summary>
        Public Function Open(deviceIndex As Integer,
                             Optional metricNames As IEnumerable(Of String) = Nothing,
                             Optional samplingIntervalMs As Double = DefaultSamplingIntervalMs,
                             Optional hwBufferBytes As Long = DefaultHardwareBufferBytes,
                             Optional maxSamples As Integer = DefaultMaxSamples,
                             Optional probeOnly As Boolean = False) As Boolean

            If _disposed Then Throw New ObjectDisposedException(NameOf(CuptiPmSamplingSession))
            If _pmEnabled Then Return True

            _deviceIndex = deviceIndex
            _samplingIntervalMs = If(samplingIntervalMs > 0, samplingIntervalMs, DefaultSamplingIntervalMs)
            _hwBufferBytes = If(hwBufferBytes > 0, hwBufferBytes, DefaultHardwareBufferBytes)
            _maxSamples = If(maxSamples > 0, maxSamples, DefaultMaxSamples)
            _metrics = PmSamplingMetrics.NormalizeAll(metricNames)

            Dim capability As New PmSamplingCapability()
            capability.Available = False
            capability.ChipName = String.Empty
            capability.CuptiVersion = String.Empty
            capability.CuptiPath = String.Empty
            capability.Reason = String.Empty
            capability.Metrics = _metrics
            capability.SkippedMetrics = New String() {}
            capability.SamplingIntervalMs = _samplingIntervalMs
            capability.LegacyPerSmSupported = False
            capability.SinglePassLimit = 0

            ' ---- 1. 动态库 ----
            Dim loadError As String = Nothing
            _library = CuptiLibrary.TryGetCurrent(loadError)

            If _library Is Nothing Then
                capability.Reason = If(String.IsNullOrEmpty(loadError), "cupti64 动态库不可用", loadError)
                _capability = capability
                Return False
            End If

            capability.CuptiPath = _library.FilePath
            capability.CuptiVersion = _library.Version

            If Not _library.SupportsPmSampling Then
                capability.Reason = $"该 cupti64（{_library.Version}）未导出 PM Sampling 接口，请安装 CUDA 12.0 及以上版本的 CUPTI"
                _capability = capability
                Return False
            End If

            ' ---- 2. 初始化 profiler 接口 ----
            ' 官方 pm_sampling 示例的顺序是"先 cuDeviceGet（隐式 cuInit）、再 cuptiProfilerInitialize、
            ' 再 cuptiDeviceGetChipName"；若驱动尚未初始化，chip name 查询会返回 INVALID_PARAMETER。
            Try
                CudaDevice.Enumerate()
            Catch
            End Try

            Dim res = InitializeProfiler()
            If res <> CUptiResult.CUPTI_SUCCESS Then
                capability.Reason = $"cuptiProfilerInitialize 失败：{CUptiApi.Describe(res)}"
                _capability = capability
                Cleanup()
                Return False
            End If

            ' ---- 3. 芯片名 ----
            Dim chipName As String = Nothing
            res = TryGetChipName(deviceIndex, chipName)
            If res <> CUptiResult.CUPTI_SUCCESS Then
                capability.Reason = $"cuptiDeviceGetChipName 失败：{CUptiApi.Describe(res)}"
                _capability = capability
                Cleanup()
                Return False
            End If

            capability.ChipName = chipName
            capability.DeviceSupported = ProbeDeviceSupport(deviceIndex)

            ' ---- 4. counter availability image ----
            Dim availability As CuptiApi.CuptiByteBuffer = Nothing
            res = TryGetCounterAvailability(deviceIndex, availability)
            If res <> CUptiResult.CUPTI_SUCCESS Then
                capability.Reason = $"cuptiPmSamplingGetCounterAvailability 失败：{CUptiApi.Describe(res)}"
                _capability = capability
                Cleanup()
                Return False
            End If

            ' ---- 5. 构建单 pass config image（贪心裁剪）----
            Dim skipped As New List(Of String)()
            Dim passes As Integer = 0

            If Not TryBuildSinglePassConfig(chipName, availability, _metrics, skipped, passes) Then
                availability.Dispose()

                capability.Reason = If(skipped.Count > 0,
                    "没有指标能在单 pass 内完成采集（PM Sampling 只支持单 pass）",
                    "无法生成 PM Sampling 的 config image")
                capability.SkippedMetrics = skipped.ToArray()
                _capability = capability
                Cleanup()
                Return False
            End If

            capability.Metrics = _metrics
            capability.SkippedMetrics = skipped.ToArray()
            capability.SinglePassLimit = _singlePassLimit
            capability.LegacyPerSmSupported = CuptiPerSmCounter.IsSupported(deviceIndex)

            ' 探测模式：拿到需要的信息就收工
            If probeOnly Then
                availability.Dispose()
                Cleanup()

                capability.Available = True
                _capability = capability
                Return True
            End If

            ' ---- 6. Enable PM sampling ----
            ' 官方示例在 target 侧也会再调用一次 cuptiProfilerInitialize（幂等），这里保持一致
            InitializeProfiler()

            res = EnablePmSampling(deviceIndex)
            If res <> CUptiResult.CUPTI_SUCCESS Then
                availability.Dispose()

                capability.Reason = If(res = CUptiResult.CUPTI_ERROR_INSUFFICIENT_PRIVILEGES,
                    "权限不足：PM Sampling 需要以管理员身份运行（CUPTI_ERROR_INSUFFICIENT_PRIVILEGES）",
                    $"cuptiPmSamplingEnable 失败：{CUptiApi.Describe(res)}")

                _capability = capability
                Cleanup()
                Return False
            End If

            ' ---- 7. SetConfig（必须在创建 counter data 镜像之前，与官方示例一致）----
            Dim applyResult = ApplyConfig()

            If applyResult <> CUptiResult.CUPTI_SUCCESS Then
                availability.Dispose()

                capability.Reason = $"cuptiPmSamplingSetConfig 失败：{CUptiApi.Describe(applyResult)}"
                _capability = capability
                Cleanup()
                Return False
            End If

            _configApplied = True

            ' ---- 8. counter data 镜像 ----
            res = CreateCounterDataImage()
            availability.Dispose()

            If res <> CUptiResult.CUPTI_SUCCESS Then
                capability.Reason = $"cuptiPmSamplingGetCounterDataSize/Initialize 失败：{CUptiApi.Describe(res)}"
                _capability = capability
                Cleanup()
                Return False
            End If

            capability.Available = True
            _capability = capability
            Return True
        End Function

        ''' <summary>
        ''' 按配置启动采样（SetConfig + Start）。已启动时直接返回 True。
        ''' </summary>
        Private Shared ReadOnly Property Trace As Boolean
            Get
                Return Environment.GetEnvironmentVariable("GTOP_SM_TRACE") = "1"
            End Get
        End Property

        Private Sub LogTrace(message As String)
            If Trace Then Console.Error.WriteLine($"[cupti] {message}")
        End Sub

        Public Function Start() As Boolean
            If _disposed OrElse Not _pmEnabled Then Return False
            If _running Then Return True

            If Not _configApplied Then
                Dim applyResult = ApplyConfig()

                If applyResult <> CUptiResult.CUPTI_SUCCESS Then
                    _capability.Reason = $"cuptiPmSamplingSetConfig 失败：{CUptiApi.Describe(applyResult)}"
                    _lastError = _capability.Reason
                    Return False
                End If

                _configApplied = True
            End If

            Dim res = StartPmSampling()
            _lastStartResult = res
            LogTrace($"cuptiPmSamplingStart -> {CInt(res)}")

            If res <> CUptiResult.CUPTI_SUCCESS Then
                ' 部分驱动在"设备刚被 profiler 接管"时首次 Start 会返回 UNKNOWN，
                ' 立即重试一次即可恢复
                res = StartPmSampling()
                If res = CUptiResult.CUPTI_SUCCESS Then _lastStartResult = res
            End If

            If res <> CUptiResult.CUPTI_SUCCESS Then
                _capability.Reason = $"cuptiPmSamplingStart 失败：{CUptiApi.Describe(res)}"
                _lastError = _capability.Reason
                Return False
            End If

            _running = True
            _everStarted = True
            _capability.UsingSysClockTrigger = _usingSysClock
            Return True
        End Function

        ''' <summary>停止采样（不读取数据）。</summary>
        Public Sub StopSampling()
            If Not _pmEnabled OrElse Not _running Then Return

            Try
                StopPmSampling()
            Catch
            End Try

            _running = False
        End Sub

        ''' <summary>
        ''' 取走自上次调用以来采集到的全部切片：
        ''' Stop → Decode → 逐样本求值 → 重新初始化镜像 → Start（保持连续采样）。
        ''' 未启动时会自动先 Start。
        ''' </summary>
        Public Function Poll() As IReadOnlyList(Of PmSamplingSample)
            If _disposed OrElse Not _pmEnabled Then Return New PmSamplingSample() {}

            If Not _running Then
                If Not Start() Then Return New PmSamplingSample() {}
            End If

            ' 必须停止后再解码（与官方 pm_sampling 示例一致：Start → 采集 → Stop → Decode）
            Try
                StopPmSampling()
            Catch
            End Try

            _running = False

            Dim samples = DecodeSamples()

            ' 重新开始下一批（镜像已重置）
            If Not Start() Then _running = False

            Return samples
        End Function

        ''' <summary>
        ''' 停止采样并取走剩余切片（会话结束后调用；可重复调用）。</summary>
        Public Function StopAndFlush() As IReadOnlyList(Of PmSamplingSample)
            If _disposed OrElse Not _pmEnabled Then Return New PmSamplingSample() {}

            If Not _running Then Return FallbackDecode()

            StopPmSampling()
            _running = False

            Return DecodeSamples()
        End Function

        ' ------------------------------------------------------------------
        ' 采样切片求值
        ' ------------------------------------------------------------------

        Private Function FallbackDecode() As IReadOnlyList(Of PmSamplingSample)
            Try
                Return DecodeSamples()
            Catch
                Return New PmSamplingSample() {}
            End Try
        End Function

        Private Function DecodeSamples() As IReadOnlyList(Of PmSamplingSample)
            Dim result As New List(Of PmSamplingSample)()
            If _counterImage Is Nothing OrElse _counterImage.Pointer = IntPtr.Zero Then Return result
            If _metrics.Length = 0 Then Return result

            ' 从未成功启动过采样时，镜像里没有任何数据，解码会报 INVALID_PARAMETER，
            ' 这里直接返回空表（把真正的失败原因保留为 Start/SetConfig 的错误）。
            If Not _everStarted Then Return result

            Dim dataParams As New CUptiPmSamplingDecodeDataParams()
            ' 该结构体 C 侧大小是 45（末尾 uint8_t），不能用 Marshal.SizeOf（48）
            dataParams.structSize = New IntPtr(CUptiApi.PmSamplingDecodeDataStructSize)
            dataParams.pPmSamplingObject = _pmObject
            dataParams.pCounterDataImage = _counterImage.Pointer
            dataParams.counterDataImageSize = New IntPtr(_counterImage.Size)

            Dim res = _library.PmSamplingDecodeData(dataParams)
            LogTrace($"cuptiPmSamplingDecodeData -> {CInt(res)}, imageSize={_counterImage.Size}, everStarted={_everStarted}")

            If res = CUptiResult.CUPTI_ERROR_OUT_OF_MEMORY Then
                ' 硬件缓冲记录溢出：标记并尝试重建会话（否则后续 Start 会一直失败）
                _lastDecodeOverflow = True
                _lastError = "PM 采样的硬件缓冲已溢出（记录过多）：请增大硬件缓冲（--sm-hwbuffer）或加大采样间隔"
                ResetCounterDataImage()
                Recover()
                Return result
            End If

            If res <> CUptiResult.CUPTI_SUCCESS Then
                _capability.Reason = $"cuptiPmSamplingDecodeData 失败：{CUptiApi.Describe(res)}"
                _lastError = _capability.Reason
                Return result
            End If

            _lastDecodeStopReason = dataParams.decodeStopReason
            _lastDecodeOverflow = dataParams.overflow <> 0

            Dim infoParams As New CUptiPmSamplingGetCounterDataInfoParams()
            infoParams.structSize = CUptiApi.StructSize(Of CUptiPmSamplingGetCounterDataInfoParams)()
            infoParams.pCounterDataImage = _counterImage.Pointer
            infoParams.counterDataImageSize = New IntPtr(_counterImage.Size)

            res = _library.PmSamplingGetCounterDataInfo(infoParams)
            If res <> CUptiResult.CUPTI_SUCCESS Then Return result

            Dim count As Integer = infoParams.numPopulatedSamples.ToInt32()
            _lastDecodePopulated = count

            If count <= 0 Then
                ResetCounterDataImage()
                Return result
            End If

            For i As Integer = 0 To count - 1
                Dim sample = EvaluateSample(i)
                sample.Index = _sampleCounter
                _sampleCounter += 1
                result.Add(sample)
            Next

            ' 关键：解码完成后必须重置镜像，否则下一批数据无处可写
            ResetCounterDataImage()

            Return result
        End Function

        Private Function EvaluateSample(sampleIndex As Integer) As PmSamplingSample
            Dim sample As New PmSamplingSample()

            ' 时间戳
            Dim timeParams As New CUptiPmSamplingCounterDataGetSampleInfoParams()
            timeParams.structSize = CUptiApi.StructSize(Of CUptiPmSamplingCounterDataGetSampleInfoParams)()
            timeParams.pPmSamplingObject = _pmObject
            timeParams.pCounterDataImage = _counterImage.Pointer
            timeParams.counterDataImageSize = New IntPtr(_counterImage.Size)
            timeParams.sampleIndex = New IntPtr(sampleIndex)

            If _library.PmSamplingCounterDataGetSampleInfo(timeParams) = CUptiResult.CUPTI_SUCCESS Then
                sample.StartTimestampNs = timeParams.startTimestamp
                sample.EndTimestampNs = timeParams.endTimestamp
            Else
                sample.StartTimestampNs = ULong.MaxValue
                sample.EndTimestampNs = ULong.MaxValue
            End If

            ' 指标值
            Dim values(_metrics.Length - 1) As Double
            Dim handle As GCHandle = GCHandle.Alloc(values, GCHandleType.Pinned)

            Try
                Using names As New CUptiApi.CuptiStringBuffer(_metrics)
                    Dim evalParams As New CUptiProfilerHostEvaluateToGpuValuesParams()
                    evalParams.structSize = CUptiApi.StructSize(Of CUptiProfilerHostEvaluateToGpuValuesParams)()
                    evalParams.pHostObject = _hostObject
                    evalParams.pCounterDataImage = _counterImage.Pointer
                    evalParams.counterDataImageSize = New IntPtr(_counterImage.Size)
                    evalParams.rangeIndex = New IntPtr(sampleIndex)
                    evalParams.ppMetricNames = names.Pointer
                    evalParams.numMetrics = New IntPtr(_metrics.Length)
                    evalParams.pMetricValues = handle.AddrOfPinnedObject()

                    Dim res = _library.HostEvaluateToGpuValues(evalParams)

                    If res = CUptiResult.CUPTI_SUCCESS Then
                        Dim dict As New Dictionary(Of String, Double)(StringComparer.Ordinal)

                        For i As Integer = 0 To _metrics.Length - 1
                            dict(_metrics(i)) = values(i)
                        Next

                        sample.Values = dict
                    Else
                        sample.Values = New Dictionary(Of String, Double)(StringComparer.Ordinal)
                    End If
                End Using
            Finally
                handle.Free()
            End Try

            Return sample
        End Function

        ''' <summary>
        ''' 由切片推断 SM 负载均衡（详见 <see cref="SmBalanceEstimate"/> 的说明）。
        ''' </summary>
        Public Shared Function EstimateBalance(sample As PmSamplingSample, smCount As Integer) As SmBalanceEstimate
            Dim est As New SmBalanceEstimate()
            est.SmCount = If(smCount > 0, smCount, 0)
            est.AverageActiveSm = -1
            est.IdleSmEstimate = -1
            est.HotSmPercent = -1
            est.ColdSmPercent = -1
            est.AveragePercent = -1
            est.ImbalanceRatio = -1
            est.IsEstimated = False

            If Not sample.Has(PmSamplingMetrics.CyclesActiveAvg) Then Return est

            Dim activeAvg = sample.ValueOf(PmSamplingMetrics.CyclesActiveAvg)
            Dim elapsed = sample.ValueOf(PmSamplingMetrics.CyclesElapsedAvg)

            If elapsed <= 0 Then
                ' 没有经过周期就退化为"用 max 做分母"
                elapsed = sample.ValueOf(PmSamplingMetrics.CyclesActiveMax)
            End If

            If elapsed <= 0 Then Return est

            Dim avgPercent = System.Math.Min(100.0, activeAvg / elapsed * 100.0)
            est.AveragePercent = avgPercent
            est.AverageActiveSm = avgPercent / 100.0 * est.SmCount

            Dim hotPercent As Double = -1
            Dim coldPercent As Double = -1

            If sample.Has(PmSamplingMetrics.CyclesActiveMax) Then
                hotPercent = System.Math.Min(100.0, sample.ValueOf(PmSamplingMetrics.CyclesActiveMax) / elapsed * 100.0)
                est.HotSmPercent = hotPercent
            End If

            If sample.Has(PmSamplingMetrics.CyclesActiveMin) Then
                coldPercent = System.Math.Min(100.0, sample.ValueOf(PmSamplingMetrics.CyclesActiveMin) / elapsed * 100.0)
                est.ColdSmPercent = coldPercent
            End If

            If hotPercent >= 0 AndAlso coldPercent >= 0 AndAlso est.SmCount > 0 Then
                est.IsEstimated = True

                ' 防御：不同驱动下 max/min 的相对关系可能出现倒挂，先归一化
                If coldPercent > hotPercent Then
                    Dim swap = hotPercent
                    hotPercent = coldPercent
                    coldPercent = swap
                    est.HotSmPercent = hotPercent
                    est.ColdSmPercent = coldPercent
                End If

                If hotPercent > 0 Then
                    est.ImbalanceRatio = System.Math.Max(0, hotPercent - coldPercent) / hotPercent
                Else
                    est.ImbalanceRatio = 0
                End If

                ' 两极模型（与 GTop 聚合口径一致）：
                ' 若把负载全部集中在"最热 SM 的活跃水平"上，只需要 avg/hot 比例的 SM
                ' 就能产生观测到的平均值，其余 SM 视为空转（空转数的上限估计）。
                Dim busyEquivalent = If(hotPercent > 0, avgPercent / hotPercent * est.SmCount, 0)
                est.IdleSmEstimate = CInt(System.Math.Max(0, System.Math.Min(est.SmCount,
                    System.Math.Round(est.SmCount - busyEquivalent))))
            End If

            Return est
        End Function

        ' ------------------------------------------------------------------
        ' CUPTI 调用包装
        ' ------------------------------------------------------------------

        Private Function InitializeProfiler() As CUptiResult
            Dim p As New CUptiProfilerInitializeParams()
            p.structSize = CUptiApi.StructSize(Of CUptiProfilerInitializeParams)()

            Dim res = _library.ProfilerInitialize(p)
            _profilerInitialized = (res = CUptiResult.CUPTI_SUCCESS)
            Return res
        End Function

        ''' <summary>
        ''' 查询 cuptiProfilerDeviceSupported（PM Sampling），返回可读的支持级别文本。
        ''' </summary>
        Private Function ProbeDeviceSupport(deviceIndex As Integer) As String
            If _library Is Nothing OrElse _library.ProfilerDeviceSupported Is Nothing Then Return String.Empty

            Dim cuDevice As Integer = -1

            Try
                Dim devices = CudaDevice.Enumerate()
                If deviceIndex >= 0 AndAlso deviceIndex < devices.Count Then cuDevice = devices(deviceIndex).Handle
            Catch
            End Try

            If cuDevice < 0 Then Return String.Empty

            Dim p As New CUptiProfilerDeviceSupportedParams()
            p.structSize = CUptiApi.StructSize(Of CUptiProfilerDeviceSupportedParams)()
            p.cuDevice = cuDevice
            p.api = CUptiProfilerApi.CUPTI_PROFILER_PM_SAMPLING

            Dim res = _library.ProfilerDeviceSupported(p)
            If res <> CUptiResult.CUPTI_SUCCESS Then Return $"查询失败：{CUptiApi.Describe(res)}"

            Return $"isSupported={SupportLevelText(p.isSupported)}, arch={SupportLevelText(p.architecture)}, " &
                   $"sli={SupportLevelText(p.sli)}, vGpu={SupportLevelText(p.vGpu)}, " &
                   $"机密计算={SupportLevelText(p.confidentialCompute)}, cmp={SupportLevelText(p.cmp)}, " &
                   $"wsl={SupportLevelText(p.wsl)}, sku={SupportLevelText(p.sku)}"
        End Function

        Private Shared Function SupportLevelText(level As Integer) As String
            Select Case level
                Case 0 : Return "UNKNOWN"
                Case 1 : Return "UNSUPPORTED"
                Case 2 : Return "DISABLED"
                Case 3 : Return "SUPPORTED"
                Case 4 : Return "LIMITED"
                Case Else : Return level.ToString()
            End Select
        End Function

        Private Function TryGetChipName(deviceIndex As Integer, ByRef chipName As String) As CUptiResult
            chipName = String.Empty

            Dim p As New CUptiDeviceGetChipNameParams()
            p.structSize = CUptiApi.StructSize(Of CUptiDeviceGetChipNameParams)()
            p.deviceIndex = New IntPtr(deviceIndex)

            Dim res = _library.DeviceGetChipName(p)
            If res <> CUptiResult.CUPTI_SUCCESS Then Return res

            chipName = CUptiApi.PtrToString(p.pChipName)
            Return CUptiResult.CUPTI_SUCCESS
        End Function

        Private Function TryGetCounterAvailability(deviceIndex As Integer,
                                                   ByRef image As CuptiApi.CuptiByteBuffer) As CUptiResult
            image = Nothing

            Dim sizeParams As New CUptiPmSamplingGetCounterAvailabilityParams()
            sizeParams.structSize = CUptiApi.StructSize(Of CUptiPmSamplingGetCounterAvailabilityParams)()
            sizeParams.deviceIndex = New IntPtr(deviceIndex)

            Dim res = _library.PmSamplingGetCounterAvailability(sizeParams)
            If res <> CUptiResult.CUPTI_SUCCESS Then Return res

            Dim size As Integer = sizeParams.counterAvailabilityImageSize.ToInt32()
            If size <= 0 Then Return CUptiResult.CUPTI_ERROR_UNKNOWN

            image = New CUptiApi.CuptiByteBuffer(size)

            Dim imageParams As New CUptiPmSamplingGetCounterAvailabilityParams()
            imageParams.structSize = CUptiApi.StructSize(Of CUptiPmSamplingGetCounterAvailabilityParams)()
            imageParams.deviceIndex = New IntPtr(deviceIndex)
            imageParams.counterAvailabilityImageSize = New IntPtr(image.Size)
            imageParams.pCounterAvailabilityImage = image.Pointer

            res = _library.PmSamplingGetCounterAvailability(imageParams)
            If res <> CUptiResult.CUPTI_SUCCESS Then
                image.Dispose()
                image = Nothing
            End If

            Return res
        End Function

        ''' <summary>
        ''' 构建单 pass 的 config image：
        ''' 先按请求的整套指标尝试；若需要多 pass，则逐个指标试探出可单 pass 的子集，
        ''' 再做贪心合并。最终 <see cref="_metrics"/> 被替换为实际可采集的指标。
        ''' </summary>
        Private Function TryBuildSinglePassConfig(chipName As String,
                                                  availability As CuptiApi.CuptiByteBuffer,
                                                  requested As String(),
                                                  skipped As List(Of String),
                                                  ByRef passes As Integer) As Boolean

            skipped.Clear()
            passes = 0

            If requested.Length = 0 Then Return False

            ' 记录单 pass 上限（尽力而为，仅用于诊断文案）
            _singlePassLimit = QuerySinglePassLimit(chipName, availability)

            ' 1) 整套尝试
            Dim config As CuptiApi.CuptiByteBuffer = Nothing
            Dim host As IntPtr = IntPtr.Zero
            Dim errorCode As CUptiResult
            Dim count As Integer = 0

            If TryCreateConfig(chipName, availability, requested, host, config, passes, errorCode, count) AndAlso passes = 1 Then
                _hostObject = host
                _hostInitialized = True
                _configImage = config
                _metrics = requested
                Return True
            End If

            CleanupConfigAttempt(host, config)

            If requested.Length = 1 Then
                skipped.AddRange(requested)
                Return False
            End If

            ' 2) 逐个试探
            Dim singles As New List(Of String)()
            For Each metricName In requested
                Dim soloHost As IntPtr = IntPtr.Zero
                Dim soloConfig As CuptiApi.CuptiByteBuffer = Nothing
                Dim soloPasses As Integer = 0
                Dim soloError As CUptiResult
                Dim soloCount As Integer = 0

                If TryCreateConfig(chipName, availability, {metricName}, soloHost, soloConfig, soloPasses, soloError, soloCount) AndAlso soloPasses = 1 Then
                    singles.Add(metricName)
                Else
                    skipped.Add(metricName)
                End If

                CleanupConfigAttempt(soloHost, soloConfig)
            Next

            If singles.Count = 0 Then Return False

            ' 3) 贪心合并（保持原始优先级顺序）
            Dim accepted As New List(Of String)()

            For Each metricName In singles
                Dim trial As String() = accepted.Concat({metricName}).ToArray()
                Dim trialHost As IntPtr = IntPtr.Zero
                Dim trialConfig As CuptiApi.CuptiByteBuffer = Nothing
                Dim trialPasses As Integer = 0
                Dim trialError As CUptiResult
                Dim trialCount As Integer = 0

                If TryCreateConfig(chipName, availability, trial, trialHost, trialConfig, trialPasses, trialError, trialCount) AndAlso trialPasses = 1 Then
                    accepted.Add(metricName)
                    CleanupConfigAttempt(trialHost, trialConfig)
                Else
                    skipped.Add(metricName)
                    CleanupConfigAttempt(trialHost, trialConfig)
                End If
            Next

            If accepted.Count = 0 Then Return False

            ' 4) 用最终集合再构建一次，拿到可用的 host 对象与 config image
            Dim finalHost As IntPtr = IntPtr.Zero
            Dim finalConfig As CuptiApi.CuptiByteBuffer = Nothing
            Dim finalPasses As Integer = 0
            Dim finalError As CUptiResult
            Dim finalCount As Integer = 0

            If Not TryCreateConfig(chipName, availability, accepted.ToArray(), finalHost, finalConfig, finalPasses, finalError, finalCount) Then
                CleanupConfigAttempt(finalHost, finalConfig)
                Return False
            End If

            _hostObject = finalHost
            _hostInitialized = True
            _configImage = finalConfig
            _metrics = accepted.ToArray()
            passes = finalPasses

            Return True
        End Function

        ''' <summary>
        ''' 用给定指标集合创建 host 对象并产出 config image（调用方负责释放）。
        ''' </summary>
        Private Function TryCreateConfig(chipName As String,
                                         availability As CuptiApi.CuptiByteBuffer,
                                         metrics As String(),
                                         ByRef hostObject As IntPtr,
                                         ByRef config As CuptiApi.CuptiByteBuffer,
                                         ByRef passes As Integer,
                                         ByRef errorCode As CUptiResult,
                                         ByRef metricCount As Integer) As Boolean

            hostObject = IntPtr.Zero
            config = Nothing
            passes = 0
            metricCount = metrics.Length
            errorCode = CUptiResult.CUPTI_SUCCESS

            Dim initParams As New CUptiProfilerHostInitializeParams()
            initParams.structSize = CUptiApi.StructSize(Of CUptiProfilerHostInitializeParams)()
            initParams.profilerType = CUptiProfilerType.CUPTI_PROFILER_TYPE_PM_SAMPLING

            Dim chipHandle As New CUptiApi.CuptiStringHandle(chipName)

            Try
                initParams.pChipName = chipHandle.Pointer
                If availability IsNot Nothing Then initParams.pCounterAvailabilityImage = availability.Pointer

                errorCode = _library.HostInitialize(initParams)
                If errorCode <> CUptiResult.CUPTI_SUCCESS Then Return False

                hostObject = initParams.pHostObject

                Using names As New CUptiApi.CuptiStringBuffer(metrics)
                    Dim addParams As New CUptiProfilerHostConfigAddMetricsParams()
                    addParams.structSize = CUptiApi.StructSize(Of CUptiProfilerHostConfigAddMetricsParams)()
                    addParams.pHostObject = hostObject
                    addParams.ppMetricNames = names.Pointer
                    addParams.numMetrics = New IntPtr(metrics.Length)

                    errorCode = _library.HostConfigAddMetrics(addParams)
                    If errorCode <> CUptiResult.CUPTI_SUCCESS Then Return False
                End Using

                Dim sizeParams As New CUptiProfilerHostGetConfigImageSizeParams()
                sizeParams.structSize = CUptiApi.StructSize(Of CUptiProfilerHostGetConfigImageSizeParams)()
                sizeParams.pHostObject = hostObject

                errorCode = _library.HostGetConfigImageSize(sizeParams)
                If errorCode <> CUptiResult.CUPTI_SUCCESS Then Return False

                Dim size As Integer = sizeParams.configImageSize.ToInt32()
                If size <= 0 Then
                    errorCode = CUptiResult.CUPTI_ERROR_UNKNOWN
                    Return False
                End If

                config = New CuptiApi.CuptiByteBuffer(size)

                Dim imageParams As New CUptiProfilerHostGetConfigImageParams()
                imageParams.structSize = CUptiApi.StructSize(Of CUptiProfilerHostGetConfigImageParams)()
                imageParams.pHostObject = hostObject
                imageParams.configImageSize = New IntPtr(config.Size)
                imageParams.pConfigImage = config.Pointer

                errorCode = _library.HostGetConfigImage(imageParams)
                If errorCode <> CUptiResult.CUPTI_SUCCESS Then Return False

                Dim passParams As New CUptiProfilerHostGetNumOfPassesParams()
                passParams.structSize = CUptiApi.StructSize(Of CUptiProfilerHostGetNumOfPassesParams)()
                passParams.configImageSize = New IntPtr(config.Size)
                passParams.pConfigImage = config.Pointer

                errorCode = _library.HostGetNumOfPasses(passParams)
                If errorCode <> CUptiResult.CUPTI_SUCCESS Then Return False

                passes = passParams.numOfPasses.ToInt32()
                Return passes > 0
            Finally
                chipHandle.Dispose()
            End Try
        End Function

        Private Sub CleanupConfigAttempt(ByRef hostObject As IntPtr, ByRef config As CuptiApi.CuptiByteBuffer)
            If config IsNot Nothing Then
                config.Dispose()
                config = Nothing
            End If

            If hostObject <> IntPtr.Zero AndAlso _library IsNot Nothing AndAlso _library.HostDeinitialize IsNot Nothing Then
                Dim p As New CUptiProfilerHostDeinitializeParams()
                p.structSize = CUptiApi.StructSize(Of CUptiProfilerHostDeinitializeParams)()
                p.pHostObject = hostObject

                Try : _library.HostDeinitialize(p) : Catch : End Try
                hostObject = IntPtr.Zero
            End If
        End Sub

        Private Function QuerySinglePassLimit(chipName As String, availability As CuptiApi.CuptiByteBuffer) As Integer
            If _library Is Nothing OrElse _library.HostGetMaxNumHardwareMetricsPerPass Is Nothing Then Return 0

            Dim chipHandle As New CUptiApi.CuptiStringHandle(chipName)

            Try
                Dim p As New CUptiProfilerHostGetMaxNumHardwareMetricsPerPassParams()
                p.structSize = CUptiApi.StructSize(Of CUptiProfilerHostGetMaxNumHardwareMetricsPerPassParams)()
                p.profilerType = CUptiProfilerType.CUPTI_PROFILER_TYPE_PM_SAMPLING
                p.pChipName = chipHandle.Pointer
                If availability IsNot Nothing Then p.pCounterAvailabilityImage = availability.Pointer

                If _library.HostGetMaxNumHardwareMetricsPerPass(p) = CUptiResult.CUPTI_SUCCESS Then
                    Return p.maxMetricsPerPass.ToInt32()
                End If
            Catch
            Finally
                chipHandle.Dispose()
            End Try

            Return 0
        End Function

        Private Function EnablePmSampling(deviceIndex As Integer) As CUptiResult
            Dim p As New CUptiPmSamplingEnableParams()
            p.structSize = CUptiApi.StructSize(Of CUptiPmSamplingEnableParams)()
            p.deviceIndex = New IntPtr(deviceIndex)

            Dim res = _library.PmSamplingEnable(p)
            If res = CUptiResult.CUPTI_SUCCESS Then
                _pmObject = p.pPmSamplingObject
                _pmEnabled = True
            End If

            Return res
        End Function

        ''' <summary>
        ''' 应用采样配置：优先纳秒触发（GA10x+），失败则退化为 SYSCLK 触发。
        ''' </summary>
        Private Function ApplyConfig() As CUptiResult
            Dim nanosecondInterval As ULong = CULng(System.Math.Max(1, _samplingIntervalMs * 1000000.0))
            Dim clockInterval As ULong = CULng(System.Math.Max(1, _samplingIntervalMs * 2000000.0))

            If _usingSysClock OrElse ForceSystemClockTrigger Then
                _usingSysClock = True
                Return SetConfig(clockInterval, CUptiPmSamplingTriggerMode.CUPTI_PM_SAMPLING_TRIGGER_MODE_GPU_SYSCLK_INTERVAL)
            End If

            Dim res = SetConfig(nanosecondInterval, CUptiPmSamplingTriggerMode.CUPTI_PM_SAMPLING_TRIGGER_MODE_GPU_TIME_INTERVAL)
            If res = CUptiResult.CUPTI_SUCCESS Then Return res

            ' 纳秒触发在 Turing / GA100 上不支持，部分驱动组合下还会直接报错
            _usingSysClock = True
            Return SetConfig(clockInterval, CUptiPmSamplingTriggerMode.CUPTI_PM_SAMPLING_TRIGGER_MODE_GPU_SYSCLK_INTERVAL)
        End Function

        Private Function SetConfig(interval As ULong, triggerMode As CUptiPmSamplingTriggerMode) As CUptiResult
            If _configImage Is Nothing Then Return CUptiResult.CUPTI_ERROR_INVALID_PARAMETER

            Dim p As New CUptiPmSamplingSetConfigParams()
            p.structSize = CUptiApi.StructSize(Of CUptiPmSamplingSetConfigParams)()
            p.pPmSamplingObject = _pmObject
            p.configSize = New IntPtr(_configImage.Size)
            p.pConfig = _configImage.Pointer
            p.hardwareBufferSize = New IntPtr(CLng(_hwBufferBytes))
            p.samplingInterval = interval
            p.triggerMode = triggerMode
            ' 与官方 pm_sampling 示例保持一致：KEEP_OLDEST（部分驱动对 KEEP_LATEST 支持不完整）
            p.hwBufferAppendMode = CUptiPmSamplingBufferAppendMode.CUPTI_PM_SAMPLING_HARDWARE_BUFFER_APPEND_MODE_KEEP_OLDEST

            _lastSetConfigResult = _library.PmSamplingSetConfig(p)
            Return _lastSetConfigResult
        End Function

        Private Function StartPmSampling() As CUptiResult
            Dim p As New CUptiPmSamplingStartParams()
            p.structSize = CUptiApi.StructSize(Of CUptiPmSamplingStartParams)()
            p.pPmSamplingObject = _pmObject

            Return _library.PmSamplingStart(p)
        End Function

        Private Function StopPmSampling() As CUptiResult
            Dim p As New CUptiPmSamplingStopParams()
            p.structSize = CUptiApi.StructSize(Of CUptiPmSamplingStopParams)()
            p.pPmSamplingObject = _pmObject

            Return _library.PmSamplingStop(p)
        End Function

        Private Function CreateCounterDataImage() As CUptiResult
            Using names As New CUptiApi.CuptiStringBuffer(_metrics)
                Dim sizeParams As New CUptiPmSamplingGetCounterDataSizeParams()
                sizeParams.structSize = CUptiApi.StructSize(Of CUptiPmSamplingGetCounterDataSizeParams)()
                sizeParams.pPmSamplingObject = _pmObject
                sizeParams.pMetricNames = names.Pointer
                sizeParams.numMetrics = New IntPtr(_metrics.Length)
                sizeParams.maxSamples = CUInt(_maxSamples)

                Dim res = _library.PmSamplingGetCounterDataSize(sizeParams)
                If res <> CUptiResult.CUPTI_SUCCESS Then Return res

                Dim size As Integer = sizeParams.counterDataSize.ToInt32()
                If size <= 0 Then Return CUptiResult.CUPTI_ERROR_UNKNOWN

                _counterImage = New CuptiApi.CuptiByteBuffer(size)

                Dim initParams As New CUptiPmSamplingCounterDataImageInitializeParams()
                initParams.structSize = CUptiApi.StructSize(Of CUptiPmSamplingCounterDataImageInitializeParams)()
                initParams.pPmSamplingObject = _pmObject
                initParams.counterDataSize = New IntPtr(_counterImage.Size)
                initParams.pCounterData = _counterImage.Pointer

                Return _library.PmSamplingCounterDataImageInitialize(initParams)
            End Using
        End Function

        Private Sub ResetCounterDataImage()
            If _counterImage Is Nothing OrElse _counterImage.Pointer = IntPtr.Zero Then Return

            Dim initParams As New CUptiPmSamplingCounterDataImageInitializeParams()
            initParams.structSize = CUptiApi.StructSize(Of CUptiPmSamplingCounterDataImageInitializeParams)()
            initParams.pPmSamplingObject = _pmObject
            initParams.counterDataSize = New IntPtr(_counterImage.Size)
            initParams.pCounterData = _counterImage.Pointer

            Try
                _library.PmSamplingCounterDataImageInitialize(initParams)
            Catch
            End Try
        End Sub

        ''' <summary>
        ''' 会话异常（例如硬件缓冲溢出）后的自愈：
        ''' 销毁并重建 PM sampling 对象 → 重新 SetConfig → 重建 counter data 镜像 → Start。
        ''' </summary>
        Private Function Recover() As Boolean
            LogTrace("试图重建 PM sampling 会话")

            Try
                If _running Then StopPmSampling()
            Catch
            End Try

            _running = False

            If _pmObject <> IntPtr.Zero AndAlso _library IsNot Nothing AndAlso _library.PmSamplingDisable IsNot Nothing Then
                Dim p As New CUptiPmSamplingDisableParams()
                p.structSize = CUptiApi.StructSize(Of CUptiPmSamplingDisableParams)()
                p.pPmSamplingObject = _pmObject

                Try : _library.PmSamplingDisable(p) : Catch : End Try
            End If

            _pmEnabled = False
            _pmObject = IntPtr.Zero

            If _counterImage IsNot Nothing Then
                _counterImage.Dispose()
                _counterImage = Nothing
            End If

            If EnablePmSampling(_deviceIndex) <> CUptiResult.CUPTI_SUCCESS Then Return False

            _configApplied = False

            If ApplyConfig() <> CUptiResult.CUPTI_SUCCESS Then Return False
            _configApplied = True

            If CreateCounterDataImage() <> CUptiResult.CUPTI_SUCCESS Then Return False

            Return Start()
        End Function

        Private Sub Cleanup()
            If _running Then
                Try : StopPmSampling() : Catch : End Try
                _running = False
            End If

            If _pmEnabled AndAlso _pmObject <> IntPtr.Zero AndAlso _library IsNot Nothing AndAlso _library.PmSamplingDisable IsNot Nothing Then
                Dim p As New CUptiPmSamplingDisableParams()
                p.structSize = CUptiApi.StructSize(Of CUptiPmSamplingDisableParams)()
                p.pPmSamplingObject = _pmObject

                Try : _library.PmSamplingDisable(p) : Catch : End Try
            End If

            _pmEnabled = False
            _pmObject = IntPtr.Zero

            If _hostInitialized AndAlso _hostObject <> IntPtr.Zero AndAlso _library IsNot Nothing AndAlso _library.HostDeinitialize IsNot Nothing Then
                Dim p As New CUptiProfilerHostDeinitializeParams()
                p.structSize = CUptiApi.StructSize(Of CUptiProfilerHostDeinitializeParams)()
                p.pHostObject = _hostObject

                Try : _library.HostDeinitialize(p) : Catch : End Try
            End If

            _hostInitialized = False
            _hostObject = IntPtr.Zero

            If _counterImage IsNot Nothing Then
                _counterImage.Dispose()
                _counterImage = Nothing
            End If

            If _configImage IsNot Nothing Then
                _configImage.Dispose()
                _configImage = Nothing
            End If

            If _profilerInitialized AndAlso _library IsNot Nothing AndAlso _library.ProfilerDeInitialize IsNot Nothing Then
                Dim p As New CUptiProfilerDeInitializeParams()
                p.structSize = CUptiApi.StructSize(Of CUptiProfilerDeInitializeParams)()

                Try : _library.ProfilerDeInitialize(p) : Catch : End Try
            End If

            _profilerInitialized = False
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True

            Cleanup()
        End Sub
    End Class
End Namespace
