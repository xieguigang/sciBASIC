' ------------------------------------------------------------------------
' NVIDIA CUPTI（CUDA Profiling Tools Interface）的 P/Invoke 层
'
' 说明：
'   1. cupti64 动态库的文件名带版本号（例如 cupti64_2026.2.1.dll，位于
'      <CUDA>\extras\CUPTI\lib64\），无法用 DllImport 固定绑定，因此这里与
'      NvrtcLibrary.vb 保持一致的风格：NativeLibrary.TryLoad + GetExport +
'      Marshal.GetDelegateForFunctionPointer 动态绑定；
'   2. 动态绑定还有一个关键好处：CUPTI 的不同版本导出的符号集合不同
'      （旧 Event/Metric API 在 CUDA 13.0 起被移除语义支持，见下），
'      Bind(..., required:=False) 可以精确区分"缺库"与"缺符号"，
'      让上层做能力探测而不是直接崩溃；
'   3. CUPTI 在 Windows 上的调用约定是 __stdcall（cupti_common.h 的 CUPTIAPI 宏）；
'   4. 结构体必须与头文件字段逐一对应：CUPTI 用
'      CUPTI_PROFILER_STRUCT_SIZE(type, lastfield) = offsetof + sizeof(lastfield)
'      作为 structSize 的期望值，VB 的 LayoutKind.Sequential 采用与 C 相同的
'      自然对齐规则，因此 Marshal.SizeOf 得到的值恒 >= 期望值，CUPTI 按
'      ">= 需要读取的字段偏移" 判断兼容性，多余尾部填充是无害的。
'
' ★重要限制（决定上层设计）：
'   * 新 Profiling / PM Sampling 栈（cupti_pmsampling.h + cupti_profiler_host.h）
'     的 cuptiProfilerHostEvaluateToGpuValues 对每个指标只返回 1 个 double，
'     即"整卡聚合值"，没有任何 instance（每 SM）维度；
'   * 唯一能拿到 per-SM（instance）数组的是旧 Event/Metric API
'     （CUPTI_EVENT_GROUP_ATTR_PROFILE_ALL_DOMAIN_INSTANCES + cuptiEventGroupReadEvent）。
'     但 cupti_events.h 明确："This API was deprecated in the CUDA 12.8 release
'     and is unsupported in CUDA 13.0. Calling any event API will return the
'     error code CUPTI_ERROR_LEGACY_PROFILER_NOT_SUPPORTED."（错误码 38）
'     所以 per-SM 采集只能在 CUDA <= 12.7 的工具链上真正工作，
'     在 CUDA 13.x 上必须在运行期探测并降级。
' ------------------------------------------------------------------------

Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>
''' CUPTI 返回码（数值与 cupti_result.h 完全一致，只列出本框架会用到的项）。
''' </summary>
Public Enum CUptiResult As Integer
    CUPTI_SUCCESS = 0
    CUPTI_ERROR_INVALID_PARAMETER = 1
    CUPTI_ERROR_INVALID_DEVICE = 2
    CUPTI_ERROR_INVALID_CONTEXT = 3
    CUPTI_ERROR_INVALID_EVENT_DOMAIN_ID = 4
    CUPTI_ERROR_INVALID_EVENT_ID = 5
    CUPTI_ERROR_INVALID_EVENT_NAME = 6
    CUPTI_ERROR_INVALID_OPERATION = 7
    CUPTI_ERROR_OUT_OF_MEMORY = 8
    CUPTI_ERROR_HARDWARE = 9
    CUPTI_ERROR_PARAMETER_SIZE_NOT_SUFFICIENT = 10
    CUPTI_ERROR_API_NOT_IMPLEMENTED = 11
    CUPTI_ERROR_MAX_LIMIT_REACHED = 12
    CUPTI_ERROR_NOT_READY = 13
    CUPTI_ERROR_NOT_COMPATIBLE = 14
    CUPTI_ERROR_NOT_INITIALIZED = 15
    CUPTI_ERROR_INVALID_METRIC_ID = 16
    CUPTI_ERROR_INVALID_METRIC_NAME = 17
    CUPTI_ERROR_QUEUE_EMPTY = 18
    CUPTI_ERROR_INVALID_HANDLE = 19
    CUPTI_ERROR_INVALID_STREAM = 20
    CUPTI_ERROR_INVALID_KIND = 21
    CUPTI_ERROR_INVALID_EVENT_VALUE = 22
    CUPTI_ERROR_DISABLED = 23
    CUPTI_ERROR_INVALID_MODULE = 24
    CUPTI_ERROR_INVALID_METRIC_VALUE = 25
    CUPTI_ERROR_HARDWARE_BUSY = 26
    CUPTI_ERROR_NOT_SUPPORTED = 27
    CUPTI_ERROR_UM_PROFILING_NOT_SUPPORTED = 28
    CUPTI_ERROR_UM_PROFILING_NOT_SUPPORTED_ON_DEVICE = 29
    CUPTI_ERROR_INSUFFICIENT_PRIVILEGES = 35
    CUPTI_ERROR_OLD_PROFILER_API_INITIALIZED = 36
    CUPTI_ERROR_LEGACY_PROFILER_NOT_SUPPORTED = 38
    CUPTI_ERROR_MULTIPLE_SUBSCRIBERS_NOT_SUPPORTED = 39
    CUPTI_ERROR_INVALID_CHIP_NAME = 46
    CUPTI_ERROR_UNKNOWN = 999
End Enum

''' <summary>profiler 类型（cupti_profiler_host.h 的 CUpti_ProfilerType）。</summary>
Public Enum CUptiProfilerType As Integer
    CUPTI_PROFILER_TYPE_RANGE_PROFILER = 0
    CUPTI_PROFILER_TYPE_PM_SAMPLING = 1
End Enum

''' <summary>指标类型（CUpti_MetricType）。</summary>
Public Enum CUptiMetricType As Integer
    CUPTI_METRIC_TYPE_COUNTER = 0
    CUPTI_METRIC_TYPE_RATIO = 1
    CUPTI_METRIC_TYPE_THROUGHPUT = 2
End Enum

''' <summary>指标采集作用域（CUpti_MetricCollectionScope）。</summary>
Public Enum CUptiMetricCollectionScope As Integer
    CUPTI_METRIC_COLLECTION_SCOPE_CONTEXT = 0
    CUPTI_METRIC_COLLECTION_SCOPE_DEVICE = 1
End Enum

''' <summary>PM Sampling 触发模式（CUpti_PmSampling_TriggerMode）。</summary>
Public Enum CUptiPmSamplingTriggerMode As Integer
    ''' <summary>以 SYSCLK 周期数触发，samplingInterval 单位为 clock（全架构可用）</summary>
    CUPTI_PM_SAMPLING_TRIGGER_MODE_GPU_SYSCLK_INTERVAL = 0
    ''' <summary>以固定时间间隔触发，samplingInterval 单位为纳秒（Turing / GA100 不支持）</summary>
    CUPTI_PM_SAMPLING_TRIGGER_MODE_GPU_TIME_INTERVAL = 1
End Enum

''' <summary>硬件缓冲的追加模式（CUpti_PmSampling_HardwareBuffer_AppendMode）。</summary>
Public Enum CUptiPmSamplingBufferAppendMode As Integer
    CUPTI_PM_SAMPLING_HARDWARE_BUFFER_APPEND_MODE_KEEP_OLDEST = 0
    CUPTI_PM_SAMPLING_HARDWARE_BUFFER_APPEND_MODE_KEEP_LATEST = 1
End Enum

''' <summary>解码停止原因（CUpti_PmSampling_DecodeStopReason）。</summary>
Public Enum CUptiPmSamplingDecodeStopReason As Integer
    CUPTI_PM_SAMPLING_DECODE_STOP_REASON_OTHER = 0
    CUPTI_PM_SAMPLING_DECODE_STOP_REASON_COUNTER_DATA_FULL = 1
    CUPTI_PM_SAMPLING_DECODE_STOP_REASON_END_OF_RECORDS = 2
End Enum

''' <summary>旧 Event API 的 event group 属性（CUpti_EventGroupAttribute）。</summary>
Public Enum CUptiEventGroupAttribute As Integer
    ''' <summary>1 = 采集该 domain 全部 instance（per-SM）的值</summary>
    CUPTI_EVENT_GROUP_ATTR_PROFILE_ALL_DOMAIN_INSTANCES = 1
    ''' <summary>只读：当前 group 实际采集的 instance 数量</summary>
    CUPTI_EVENT_GROUP_ATTR_INSTANCE_COUNT = 5
End Enum

''' <summary>旧 Event API 的读取标志（CUpti_ReadEventFlags）。</summary>
Public Enum CUptiReadEventFlags As Integer
    CUPTI_EVENT_READ_FLAG_NONE = 0
End Enum

''' <summary>旧 Event API 的采集模式（CUpti_EventCollectionMode）。</summary>
Public Enum CUptiEventCollectionMode As Integer
    CUPTI_EVENT_COLLECTION_MODE_CONTINUOUS = 0
    CUPTI_EVENT_COLLECTION_MODE_KERNEL = 1
End Enum

#Region "Profiler target / PM sampling 参数结构体"

''' <summary>CUpti_Profiler_Initialize_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiProfilerInitializeParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
End Structure

''' <summary>CUpti_Profiler_DeInitialize_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiProfilerDeInitializeParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
End Structure

''' <summary>CUpti_Device_GetChipName_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiDeviceGetChipNameParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public deviceIndex As IntPtr
    ''' <summary>[out] CUPTI 内部持有的 ASCII 字符串，不要释放</summary>
    Public pChipName As IntPtr
End Structure

''' <summary>CUpti_PmSampling_GetCounterAvailability_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiPmSamplingGetCounterAvailabilityParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public deviceIndex As IntPtr
    ''' <summary>[inout] 传入 NULL image 时返回所需大小</summary>
    Public counterAvailabilityImageSize As IntPtr
    Public pCounterAvailabilityImage As IntPtr
End Structure

''' <summary>CUpti_PmSampling_Enable_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiPmSamplingEnableParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public deviceIndex As IntPtr
    ''' <summary>[out] PM sampling 对象句柄</summary>
    Public pPmSamplingObject As IntPtr
End Structure

''' <summary>CUpti_PmSampling_Disable_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiPmSamplingDisableParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public pPmSamplingObject As IntPtr
End Structure

''' <summary>CUpti_PmSampling_SetConfig_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiPmSamplingSetConfigParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public pPmSamplingObject As IntPtr
    Public configSize As IntPtr
    Public pConfig As IntPtr
    ''' <summary>存放原始采样记录的硬件缓冲字节数</summary>
    Public hardwareBufferSize As IntPtr
    ''' <summary>SYSCLK 模式为 clock 数，TIME 模式为纳秒</summary>
    Public samplingInterval As ULong
    Public triggerMode As CUptiPmSamplingTriggerMode
    Public hwBufferAppendMode As CUptiPmSamplingBufferAppendMode
End Structure

''' <summary>CUpti_PmSampling_Start_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiPmSamplingStartParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public pPmSamplingObject As IntPtr
End Structure

''' <summary>CUpti_PmSampling_Stop_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiPmSamplingStopParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public pPmSamplingObject As IntPtr
End Structure

''' <summary>CUpti_PmSampling_DecodeData_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiPmSamplingDecodeDataParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public pPmSamplingObject As IntPtr
    Public pCounterDataImage As IntPtr
    Public counterDataImageSize As IntPtr
    ''' <summary>[out] 解码停止原因</summary>
    Public decodeStopReason As CUptiPmSamplingDecodeStopReason
    ''' <summary>[out] 硬件缓冲是否溢出（1 = 溢出）</summary>
    Public overflow As Byte
End Structure

''' <summary>CUpti_PmSampling_GetCounterDataSize_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiPmSamplingGetCounterDataSizeParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public pPmSamplingObject As IntPtr
    Public pMetricNames As IntPtr
    Public numMetrics As IntPtr
    ''' <summary>counter data 镜像中最多存放多少个采样</summary>
    Public maxSamples As UInteger
    ''' <summary>[out] 所需镜像字节数</summary>
    Public counterDataSize As IntPtr
End Structure

''' <summary>CUpti_PmSampling_CounterDataImage_Initialize_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiPmSamplingCounterDataImageInitializeParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public pPmSamplingObject As IntPtr
    Public counterDataSize As IntPtr
    Public pCounterData As IntPtr
End Structure

''' <summary>CUpti_PmSampling_GetCounterDataInfo_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiPmSamplingGetCounterDataInfoParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public pCounterDataImage As IntPtr
    Public counterDataImageSize As IntPtr
    ''' <summary>[out] 镜像中总的采样槽位数</summary>
    Public numTotalSamples As IntPtr
    ''' <summary>[out] 已经写入数据的采样数</summary>
    Public numPopulatedSamples As IntPtr
    ''' <summary>[out] 已经采集完成的采样数</summary>
    Public numCompletedSamples As IntPtr
End Structure

''' <summary>CUpti_PmSampling_CounterData_GetSampleInfo_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiPmSamplingCounterDataGetSampleInfoParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public pPmSamplingObject As IntPtr
    Public pCounterDataImage As IntPtr
    Public counterDataImageSize As IntPtr
    Public sampleIndex As IntPtr
    ''' <summary>[out] 采样窗口起始时间戳（ns）</summary>
    Public startTimestamp As ULong
    ''' <summary>[out] 采样窗口结束时间戳（ns）</summary>
    Public endTimestamp As ULong
End Structure

#End Region

#Region "Profiler host 参数结构体"

''' <summary>
''' CUpti_Profiler_DeviceSupported_Params —— 探测"该设备/配置是否允许 profiling"。
''' 各字段取值见 CUpti_Profiler_Support_Level（0 未知 / 1 不支持 / 2 被系统关闭 / 3 支持 / 4 有限支持）。
''' </summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiProfilerDeviceSupportedParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    ''' <summary>CUdevice（驱动 API 的设备句柄，整数）</summary>
    Public cuDevice As Integer
    ''' <summary>[out] 总体支持级别</summary>
    Public isSupported As Integer
    ''' <summary>[out] 架构支持（CC &gt;= 7.0）</summary>
    Public architecture As Integer
    ''' <summary>[out] SLI 配置</summary>
    Public sli As Integer
    ''' <summary>[out] vGPU 配置</summary>
    Public vGpu As Integer
    ''' <summary>[out] 机密计算</summary>
    Public confidentialCompute As Integer
    ''' <summary>[out] CMP（矿卡）</summary>
    Public cmp As Integer
    ''' <summary>[out] WSL</summary>
    Public wsl As Integer
    ''' <summary>[in] 查询的 API 类型（CUPTI_PROFILER_PM_SAMPLING = 3）</summary>
    Public api As Integer
    ''' <summary>[out] SKU 支持</summary>
    Public sku As Integer
End Structure

''' <summary>CUti_Profiler_API 取值</summary>
Friend Enum CUptiProfilerApi As Integer
    CUPTI_PROFILER_RANGE_PROFILING = 0
    CUPTI_PROFILER_PC_SAMPLING = 1
    CUPTI_PROFILER_SASS_METRICS = 2
    CUPTI_PROFILER_PM_SAMPLING = 3
End Enum

''' <summary>CUpti_Profiler_Host_Initialize_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiProfilerHostInitializeParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public profilerType As CUptiProfilerType
    Public pChipName As IntPtr
    Public pCounterAvailabilityImage As IntPtr
    ''' <summary>[out] host 对象句柄</summary>
    Public pHostObject As IntPtr
    ''' <summary>PM sampling 可选的单 pass 指标集名称（可为 NULL）</summary>
    Public pSinglePassMetricSetName As IntPtr
End Structure

''' <summary>CUpti_Profiler_Host_Deinitialize_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiProfilerHostDeinitializeParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public pHostObject As IntPtr
End Structure

''' <summary>CUpti_Profiler_Host_ConfigAddMetrics_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiProfilerHostConfigAddMetricsParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public pHostObject As IntPtr
    Public ppMetricNames As IntPtr
    Public numMetrics As IntPtr
End Structure

''' <summary>CUpti_Profiler_Host_GetConfigImageSize_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiProfilerHostGetConfigImageSizeParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public pHostObject As IntPtr
    ''' <summary>[out] config image 字节数</summary>
    Public configImageSize As IntPtr
End Structure

''' <summary>CUpti_Profiler_Host_GetConfigImage_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiProfilerHostGetConfigImageParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public pHostObject As IntPtr
    Public configImageSize As IntPtr
    Public pConfigImage As IntPtr
End Structure

''' <summary>CUpti_Profiler_Host_GetNumOfPasses_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiProfilerHostGetNumOfPassesParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public configImageSize As IntPtr
    Public pConfigImage As IntPtr
    ''' <summary>[out] 该 config 需要的 pass 数（PM sampling 必须为 1）</summary>
    Public numOfPasses As IntPtr
End Structure

''' <summary>CUpti_Profiler_Host_GetMetricProperties_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiProfilerHostGetMetricPropertiesParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public pHostObject As IntPtr
    Public pMetricName As IntPtr
    ''' <summary>[out] 指标描述</summary>
    Public pDescription As IntPtr
    ''' <summary>[out] 所属硬件单元（例如 sm / l1tex / dram）</summary>
    Public pHwUnit As IntPtr
    ''' <summary>[out] 值单位</summary>
    Public pDimUnit As IntPtr
    Public metricType As CUptiMetricType
    Public metricCollectionScope As CUptiMetricCollectionScope
End Structure

''' <summary>CUpti_Profiler_Host_GetSubMetrics_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiProfilerHostGetSubMetricsParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public pHostObject As IntPtr
    Public metricType As CUptiMetricType
    Public pMetricName As IntPtr
    ''' <summary>[out] 子指标（rollup 变体）数量</summary>
    Public numOfSubmetrics As IntPtr
    ''' <summary>[out] 子指标名数组指针</summary>
    Public ppSubMetrics As IntPtr
End Structure

''' <summary>CUpti_Profiler_Host_GetMaxNumHardwareMetricsPerPass_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiProfilerHostGetMaxNumHardwareMetricsPerPassParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public profilerType As CUptiProfilerType
    Public pChipName As IntPtr
    Public pCounterAvailabilityImage As IntPtr
    ''' <summary>[out] 单 pass 可调度的硬件指标上限</summary>
    Public maxMetricsPerPass As IntPtr
End Structure

''' <summary>CUpti_Profiler_Host_GetSinglePassSets_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiProfilerHostGetSinglePassSetsParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public pChipName As IntPtr
    ''' <summary>[inout] 单 pass 指标集数量</summary>
    Public numOfSinglePassSets As IntPtr
    ''' <summary>[out] 指标集名称数组指针</summary>
    Public ppSinglePassSets As IntPtr
End Structure

''' <summary>CUpti_Profiler_Host_GetMetricsInSinglePassSet_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiProfilerHostGetMetricsInSinglePassSetParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public pChipName As IntPtr
    Public pSinglePassSetName As IntPtr
    ''' <summary>[inout] 该集合中的指标数量</summary>
    Public numOfMetricsInSinglePassSet As IntPtr
    ''' <summary>[inout] 指标名字符串缓冲字节数</summary>
    Public metricsBufferSize As IntPtr
    ''' <summary>[out] 指标名在缓冲中的偏移（size_t 数组）</summary>
    Public pMetricsIndicesBuffer As IntPtr
    ''' <summary>[out] 指标名缓冲</summary>
    Public pMetricsBuffer As IntPtr
End Structure

''' <summary>CUpti_Profiler_Host_EvaluateToGpuValues_Params</summary>
<StructLayout(LayoutKind.Sequential)>
Friend Structure CUptiProfilerHostEvaluateToGpuValuesParams
    Public structSize As IntPtr
    Public pPriv As IntPtr
    Public pHostObject As IntPtr
    Public pCounterDataImage As IntPtr
    Public counterDataImageSize As IntPtr
    ''' <summary>PM sampling 场景下即 sampleIndex</summary>
    Public rangeIndex As IntPtr
    Public ppMetricNames As IntPtr
    Public numMetrics As IntPtr
    ''' <summary>[out] 每个指标一个 double</summary>
    Public pMetricValues As IntPtr
End Structure

#End Region

''' <summary>
''' CUPTI 的函数入口声明（全部按 __stdcall + 结构体指针绑定；可选符号允许为 Nothing）。
''' </summary>
Friend Module CUptiApi

#Region "委托类型"

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiGetVersion(ByRef version As UInteger) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiProfilerInitialize(ByRef pParams As CUptiProfilerInitializeParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiProfilerDeInitialize(ByRef pParams As CUptiProfilerDeInitializeParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiDeviceGetChipName(ByRef pParams As CUptiDeviceGetChipNameParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiProfilerDeviceSupported(ByRef pParams As CUptiProfilerDeviceSupportedParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiPmSamplingGetCounterAvailability(ByRef pParams As CUptiPmSamplingGetCounterAvailabilityParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiPmSamplingEnable(ByRef pParams As CUptiPmSamplingEnableParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiPmSamplingDisable(ByRef pParams As CUptiPmSamplingDisableParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiPmSamplingSetConfig(ByRef pParams As CUptiPmSamplingSetConfigParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiPmSamplingStart(ByRef pParams As CUptiPmSamplingStartParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiPmSamplingStop(ByRef pParams As CUptiPmSamplingStopParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiPmSamplingDecodeData(ByRef pParams As CUptiPmSamplingDecodeDataParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiPmSamplingGetCounterDataSize(ByRef pParams As CUptiPmSamplingGetCounterDataSizeParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiPmSamplingCounterDataImageInitialize(ByRef pParams As CUptiPmSamplingCounterDataImageInitializeParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiPmSamplingGetCounterDataInfo(ByRef pParams As CUptiPmSamplingGetCounterDataInfoParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiPmSamplingCounterDataGetSampleInfo(ByRef pParams As CUptiPmSamplingCounterDataGetSampleInfoParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiProfilerHostInitialize(ByRef pParams As CUptiProfilerHostInitializeParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiProfilerHostDeinitialize(ByRef pParams As CUptiProfilerHostDeinitializeParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiProfilerHostConfigAddMetrics(ByRef pParams As CUptiProfilerHostConfigAddMetricsParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiProfilerHostGetConfigImageSize(ByRef pParams As CUptiProfilerHostGetConfigImageSizeParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiProfilerHostGetConfigImage(ByRef pParams As CUptiProfilerHostGetConfigImageParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiProfilerHostGetNumOfPasses(ByRef pParams As CUptiProfilerHostGetNumOfPassesParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiProfilerHostGetMetricProperties(ByRef pParams As CUptiProfilerHostGetMetricPropertiesParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiProfilerHostGetSubMetrics(ByRef pParams As CUptiProfilerHostGetSubMetricsParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiProfilerHostGetMaxNumHardwareMetricsPerPass(ByRef pParams As CUptiProfilerHostGetMaxNumHardwareMetricsPerPassParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiProfilerHostGetSinglePassSets(ByRef pParams As CUptiProfilerHostGetSinglePassSetsParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiProfilerHostGetMetricsInSinglePassSet(ByRef pParams As CUptiProfilerHostGetMetricsInSinglePassSetParams) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiProfilerHostEvaluateToGpuValues(ByRef pParams As CUptiProfilerHostEvaluateToGpuValuesParams) As CUptiResult

    ' ---- 旧 Event / Metric API（CUDA 13.0 起调用即返回错误码 38）----

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiSetEventCollectionMode(context As IntPtr, mode As CUptiEventCollectionMode) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiEventGetIdFromName(device As Integer, eventName As String, ByRef eventId As Integer) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiEventGroupCreate(context As IntPtr, ByRef eventGroup As IntPtr, flags As UInteger) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiEventGroupDestroy(eventGroup As IntPtr) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiEventGroupGetAttribute(eventGroup As IntPtr, attrib As CUptiEventGroupAttribute,
                                                          ByRef valueSize As IntPtr, value As IntPtr) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiEventGroupSetAttribute(eventGroup As IntPtr, attrib As CUptiEventGroupAttribute,
                                                          valueSize As IntPtr, value As IntPtr) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiEventGroupAddEvent(eventGroup As IntPtr, eventId As Integer) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiEventGroupEnable(eventGroup As IntPtr) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiEventGroupDisable(eventGroup As IntPtr) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiEventGroupReadEvent(eventGroup As IntPtr, flags As CUptiReadEventFlags, eventId As Integer,
                                                        ByRef eventValueBufferSizeBytes As IntPtr, eventValueBuffer As IntPtr) As CUptiResult

    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Ansi)>
    Friend Delegate Function FnCuptiEventGroupResetAllEvents(eventGroup As IntPtr) As CUptiResult

#End Region

    ''' <summary>算法复用：结构体的托管大小（即 CUPTI 期望的 structSize）。</summary>
    Friend Function StructSize(Of T As Structure)() As IntPtr
        Return New IntPtr(Marshal.SizeOf(Of T)())
    End Function

    ''' <summary>
    ''' CUpti_PmSampling_DecodeData_Params 在 C 侧的确切大小 = 45 字节
    ''' （offsetof(overflow)=44 + sizeof(uint8_t)=1）。
    '''
    ''' 注意：托管侧的 LayoutKind.Sequential 会把结构体尾部补齐到 8 字节对齐（48），
    ''' 而 CUPTI 对这个结构做的是精确比较，传 48 会直接返回 CUPTI_ERROR_INVALID_PARAMETER。
    ''' 这是全 CUPTI 里唯一一个"末尾是 uint8_t"的结构体，因此这里单独给出常量。
    ''' </summary>
    Friend Const PmSamplingDecodeDataStructSize As Integer = 45

    ''' <summary>把返回码翻译成中文可读说明（用于 GTop 的提示行）。</summary>
    Friend Function Describe(result As CUptiResult) As String
        Select Case result
            Case CUptiResult.CUPTI_SUCCESS
                Return "成功"
            Case CUptiResult.CUPTI_ERROR_INVALID_PARAMETER
                Return "参数非法"
            Case CUptiResult.CUPTI_ERROR_INVALID_DEVICE
                Return "设备编号非法"
            Case CUptiResult.CUPTI_ERROR_INVALID_CONTEXT
                Return "CUDA 上下文非法"
            Case CUptiResult.CUPTI_ERROR_INVALID_EVENT_NAME
                Return "事件名非法（该芯片/域不支持该事件）"
            Case CUptiResult.CUPTI_ERROR_INVALID_OPERATION
                Return "操作顺序非法（例如重复 Enable/Start）"
            Case CUptiResult.CUPTI_ERROR_OUT_OF_MEMORY
                Return "内存不足"
            Case CUptiResult.CUPTI_ERROR_HARDWARE
                Return "硬件错误"
            Case CUptiResult.CUPTI_ERROR_NOT_INITIALIZED
                Return "CUPTI/nvperf 库未初始化"
            Case CUptiResult.CUPTI_ERROR_INVALID_METRIC_NAME
                Return "指标名非法（该芯片不支持）"
            Case CUptiResult.CUPTI_ERROR_HARDWARE_BUSY
                Return "硬件计数器忙（可能已有其它 profiler 会话）"
            Case CUptiResult.CUPTI_ERROR_NOT_SUPPORTED
                Return "该芯片/驱动不支持此操作"
            Case CUptiResult.CUPTI_ERROR_INSUFFICIENT_PRIVILEGES
                Return "权限不足（请以管理员身份运行）"
            Case CUptiResult.CUPTI_ERROR_OLD_PROFILER_API_INITIALIZED
                Return "已初始化了旧版 profiler API（与新版互斥）"
            Case CUptiResult.CUPTI_ERROR_LEGACY_PROFILER_NOT_SUPPORTED
                Return "旧版 Event/Metric API 已被移除（CUDA 12.8 弃用、13.0 起不支持），无法获取 per-SM instance 数据"
            Case CUptiResult.CUPTI_ERROR_MULTIPLE_SUBSCRIBERS_NOT_SUPPORTED
                Return "不支持同时存在多个 CUPTI 订阅者"
            Case CUptiResult.CUPTI_ERROR_INVALID_CHIP_NAME
                Return "芯片名非法（CUPTI 不认识该 GPU 型号）"
            Case Else
                Return $"CUPTI 错误码 {CInt(result)}"
        End Select
    End Function

    ''' <summary>
    ''' 构造 "const char* 数组" 的连续缓冲区；返回的指针在 Dispose 前有效。
    ''' </summary>
    Friend NotInheritable Class CuptiStringBuffer
        Implements IDisposable

        Private ReadOnly _strings As New List(Of IntPtr)()
        Private ReadOnly _pointerBlock As IntPtr
        Private _disposed As Boolean

        Public Sub New(values As IEnumerable(Of String))
            Dim items = If(values Is Nothing, New String() {}, values.ToArray())
            If items.Length = 0 Then
                _pointerBlock = IntPtr.Zero
                Return
            End If

            _pointerBlock = Marshal.AllocHGlobal(IntPtr.Size * items.Length)
            For i As Integer = 0 To items.Length - 1
                Dim str = Marshal.StringToHGlobalAnsi(If(items(i), String.Empty))
                _strings.Add(str)
                Marshal.WriteIntPtr(_pointerBlock, i * IntPtr.Size, str)
            Next
        End Sub

        ''' <summary>指向 char*[] 的指针（元素个数为 <see cref="Count"/>）</summary>
        Public ReadOnly Property Pointer As IntPtr
            Get
                Return _pointerBlock
            End Get
        End Property

        Public ReadOnly Property Count As Integer
            Get
                Return _strings.Count
            End Get
        End Property

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True

            For Each itemPointer In _strings
                If itemPointer <> IntPtr.Zero Then Marshal.FreeHGlobal(itemPointer)
            Next

            If _pointerBlock <> IntPtr.Zero Then Marshal.FreeHGlobal(_pointerBlock)
        End Sub
    End Class

    ''' <summary>内容为 ANSI 字符串的非托管块（用于单值 const char* 入参）。</summary>
    Friend NotInheritable Class CuptiStringHandle
        Implements IDisposable

        Private ReadOnly _pointer As IntPtr

        Public Sub New(value As String)
            _pointer = If(String.IsNullOrEmpty(value), IntPtr.Zero, Marshal.StringToHGlobalAnsi(value))
        End Sub

        Public ReadOnly Property Pointer As IntPtr
            Get
                Return _pointer
            End Get
        End Property

        Public Sub Dispose() Implements IDisposable.Dispose
            If _pointer <> IntPtr.Zero Then Marshal.FreeHGlobal(_pointer)
        End Sub
    End Class

    ''' <summary>可 Pin 的字节缓冲（counter data image / config image）。</summary>
    Friend NotInheritable Class CuptiByteBuffer
        Implements IDisposable

        Private _pointer As IntPtr
        Private ReadOnly _size As Integer

        Public Sub New(size As Integer)
            _size = If(size > 0, size, 0)
            If size > 0 Then
                _pointer = Marshal.AllocHGlobal(size)
                ' 清零，避免 CUPTI 读到脏数据（分块拷贝，避免一次申请超大托管数组）
                Dim chunkSize = System.Math.Min(size, 65536)
                Dim chunk(chunkSize - 1) As Byte
                Dim offset As Integer = 0
                While offset < size
                    Dim count = System.Math.Min(chunkSize, size - offset)
                    Marshal.Copy(chunk, 0, IntPtr.Add(_pointer, offset), count)
                    offset += count
                End While
            End If
        End Sub

        Public ReadOnly Property Pointer As IntPtr
            Get
                Return _pointer
            End Get
        End Property

        Public ReadOnly Property Size As Integer
            Get
                Return _size
            End Get
        End Property

        Public Sub Dispose() Implements IDisposable.Dispose
            If _pointer <> IntPtr.Zero Then
                Marshal.FreeHGlobal(_pointer)
                _pointer = IntPtr.Zero
            End If
        End Sub
    End Class

    ''' <summary>把 UTF-8/ANSI 指针转成托管字符串（NULL 安全）。</summary>
    Friend Function PtrToString(pointer As IntPtr) As String
        If pointer = IntPtr.Zero Then Return String.Empty
        Try
            Return Marshal.PtrToStringAnsi(pointer)
        Catch
            Return String.Empty
        End Try
    End Function

End Module

''' <summary>
''' 已绑定好函数入口的 CUPTI 动态库实例（cupti64_&lt;ver&gt;.dll）。
''' 线程不安全：CUPTI 的 profiling 会话本身要求单线程串行调用，请勿并发使用。
''' </summary>
Public NotInheritable Class CuptiLibrary
    Implements IDisposable

    Private ReadOnly _handle As IntPtr

    ''' <summary>动态库文件全路径</summary>
    Public ReadOnly Property FilePath As String
    ''' <summary>形如 2026.2.1 的版本文本（来自文件名或 cuptiGetVersion）</summary>
    Public ReadOnly Property Version As String

    ''' <summary>该动态库是否导出了新版 Profiling / PM Sampling API</summary>
    Public ReadOnly Property SupportsPmSampling As Boolean
    ''' <summary>该动态库是否仍导出了旧 Event API 符号（导出不等于可用，见 CUDA 13 限制）</summary>
    Public ReadOnly Property HasLegacyEventApi As Boolean

    Friend ReadOnly GetVersion As FnCuptiGetVersion

    Friend ReadOnly ProfilerInitialize As FnCuptiProfilerInitialize
    Friend ReadOnly ProfilerDeInitialize As FnCuptiProfilerDeInitialize
    Friend ReadOnly DeviceGetChipName As FnCuptiDeviceGetChipName
    Friend ReadOnly ProfilerDeviceSupported As FnCuptiProfilerDeviceSupported

    Friend ReadOnly PmSamplingGetCounterAvailability As FnCuptiPmSamplingGetCounterAvailability
    Friend ReadOnly PmSamplingEnable As FnCuptiPmSamplingEnable
    Friend ReadOnly PmSamplingDisable As FnCuptiPmSamplingDisable
    Friend ReadOnly PmSamplingSetConfig As FnCuptiPmSamplingSetConfig
    Friend ReadOnly PmSamplingStart As FnCuptiPmSamplingStart
    Friend ReadOnly PmSamplingStop As FnCuptiPmSamplingStop
    Friend ReadOnly PmSamplingDecodeData As FnCuptiPmSamplingDecodeData
    Friend ReadOnly PmSamplingGetCounterDataSize As FnCuptiPmSamplingGetCounterDataSize
    Friend ReadOnly PmSamplingCounterDataImageInitialize As FnCuptiPmSamplingCounterDataImageInitialize
    Friend ReadOnly PmSamplingGetCounterDataInfo As FnCuptiPmSamplingGetCounterDataInfo
    Friend ReadOnly PmSamplingCounterDataGetSampleInfo As FnCuptiPmSamplingCounterDataGetSampleInfo

    Friend ReadOnly HostInitialize As FnCuptiProfilerHostInitialize
    Friend ReadOnly HostDeinitialize As FnCuptiProfilerHostDeinitialize
    Friend ReadOnly HostConfigAddMetrics As FnCuptiProfilerHostConfigAddMetrics
    Friend ReadOnly HostGetConfigImageSize As FnCuptiProfilerHostGetConfigImageSize
    Friend ReadOnly HostGetConfigImage As FnCuptiProfilerHostGetConfigImage
    Friend ReadOnly HostGetNumOfPasses As FnCuptiProfilerHostGetNumOfPasses
    Friend ReadOnly HostGetMetricProperties As FnCuptiProfilerHostGetMetricProperties
    Friend ReadOnly HostGetSubMetrics As FnCuptiProfilerHostGetSubMetrics
    Friend ReadOnly HostGetMaxNumHardwareMetricsPerPass As FnCuptiProfilerHostGetMaxNumHardwareMetricsPerPass
    Friend ReadOnly HostGetSinglePassSets As FnCuptiProfilerHostGetSinglePassSets
    Friend ReadOnly HostGetMetricsInSinglePassSet As FnCuptiProfilerHostGetMetricsInSinglePassSet
    Friend ReadOnly HostEvaluateToGpuValues As FnCuptiProfilerHostEvaluateToGpuValues

    Friend ReadOnly SetEventCollectionMode As FnCuptiSetEventCollectionMode
    Friend ReadOnly EventGetIdFromName As FnCuptiEventGetIdFromName
    Friend ReadOnly EventGroupCreate As FnCuptiEventGroupCreate
    Friend ReadOnly EventGroupDestroy As FnCuptiEventGroupDestroy
    Friend ReadOnly EventGroupGetAttribute As FnCuptiEventGroupGetAttribute
    Friend ReadOnly EventGroupSetAttribute As FnCuptiEventGroupSetAttribute
    Friend ReadOnly EventGroupAddEvent As FnCuptiEventGroupAddEvent
    Friend ReadOnly EventGroupEnable As FnCuptiEventGroupEnable
    Friend ReadOnly EventGroupDisable As FnCuptiEventGroupDisable
    Friend ReadOnly EventGroupReadEvent As FnCuptiEventGroupReadEvent
    Friend ReadOnly EventGroupResetAllEvents As FnCuptiEventGroupResetAllEvents

    Private Sub New(handle As IntPtr, filePath As String)
        Me._handle = handle
        Me.FilePath = filePath
        Me.Version = ParseVersionFromFileName(filePath)

        GetVersion = Bind(Of FnCuptiGetVersion)(handle, "cuptiGetVersion")

        ProfilerInitialize = Bind(Of FnCuptiProfilerInitialize)(handle, "cuptiProfilerInitialize")
        ProfilerDeInitialize = Bind(Of FnCuptiProfilerDeInitialize)(handle, "cuptiProfilerDeInitialize")
        DeviceGetChipName = Bind(Of FnCuptiDeviceGetChipName)(handle, "cuptiDeviceGetChipName")
        ProfilerDeviceSupported = Bind(Of FnCuptiProfilerDeviceSupported)(handle, "cuptiProfilerDeviceSupported")

        PmSamplingGetCounterAvailability = Bind(Of FnCuptiPmSamplingGetCounterAvailability)(handle, "cuptiPmSamplingGetCounterAvailability")
        PmSamplingEnable = Bind(Of FnCuptiPmSamplingEnable)(handle, "cuptiPmSamplingEnable")
        PmSamplingDisable = Bind(Of FnCuptiPmSamplingDisable)(handle, "cuptiPmSamplingDisable")
        PmSamplingSetConfig = Bind(Of FnCuptiPmSamplingSetConfig)(handle, "cuptiPmSamplingSetConfig")
        PmSamplingStart = Bind(Of FnCuptiPmSamplingStart)(handle, "cuptiPmSamplingStart")
        PmSamplingStop = Bind(Of FnCuptiPmSamplingStop)(handle, "cuptiPmSamplingStop")
        PmSamplingDecodeData = Bind(Of FnCuptiPmSamplingDecodeData)(handle, "cuptiPmSamplingDecodeData")
        PmSamplingGetCounterDataSize = Bind(Of FnCuptiPmSamplingGetCounterDataSize)(handle, "cuptiPmSamplingGetCounterDataSize")
        PmSamplingCounterDataImageInitialize = Bind(Of FnCuptiPmSamplingCounterDataImageInitialize)(handle, "cuptiPmSamplingCounterDataImageInitialize")
        PmSamplingGetCounterDataInfo = Bind(Of FnCuptiPmSamplingGetCounterDataInfo)(handle, "cuptiPmSamplingGetCounterDataInfo")
        PmSamplingCounterDataGetSampleInfo = Bind(Of FnCuptiPmSamplingCounterDataGetSampleInfo)(handle, "cuptiPmSamplingCounterDataGetSampleInfo")

        HostInitialize = Bind(Of FnCuptiProfilerHostInitialize)(handle, "cuptiProfilerHostInitialize")
        HostDeinitialize = Bind(Of FnCuptiProfilerHostDeinitialize)(handle, "cuptiProfilerHostDeinitialize")
        HostConfigAddMetrics = Bind(Of FnCuptiProfilerHostConfigAddMetrics)(handle, "cuptiProfilerHostConfigAddMetrics")
        HostGetConfigImageSize = Bind(Of FnCuptiProfilerHostGetConfigImageSize)(handle, "cuptiProfilerHostGetConfigImageSize")
        HostGetConfigImage = Bind(Of FnCuptiProfilerHostGetConfigImage)(handle, "cuptiProfilerHostGetConfigImage")
        HostGetNumOfPasses = Bind(Of FnCuptiProfilerHostGetNumOfPasses)(handle, "cuptiProfilerHostGetNumOfPasses")
        HostGetMetricProperties = Bind(Of FnCuptiProfilerHostGetMetricProperties)(handle, "cuptiProfilerHostGetMetricProperties")
        HostGetSubMetrics = Bind(Of FnCuptiProfilerHostGetSubMetrics)(handle, "cuptiProfilerHostGetSubMetrics")
        HostGetMaxNumHardwareMetricsPerPass = Bind(Of FnCuptiProfilerHostGetMaxNumHardwareMetricsPerPass)(handle, "cuptiProfilerHostGetMaxNumHardwareMetricsPerPass")
        HostGetSinglePassSets = Bind(Of FnCuptiProfilerHostGetSinglePassSets)(handle, "cuptiProfilerHostGetSinglePassSets")
        HostGetMetricsInSinglePassSet = Bind(Of FnCuptiProfilerHostGetMetricsInSinglePassSet)(handle, "cuptiProfilerHostGetMetricsInSinglePassSet")
        HostEvaluateToGpuValues = Bind(Of FnCuptiProfilerHostEvaluateToGpuValues)(handle, "cuptiProfilerHostEvaluateToGpuValues")

        SetEventCollectionMode = Bind(Of FnCuptiSetEventCollectionMode)(handle, "cuptiSetEventCollectionMode")
        EventGetIdFromName = Bind(Of FnCuptiEventGetIdFromName)(handle, "cuptiEventGetIdFromName")
        EventGroupCreate = Bind(Of FnCuptiEventGroupCreate)(handle, "cuptiEventGroupCreate")
        EventGroupDestroy = Bind(Of FnCuptiEventGroupDestroy)(handle, "cuptiEventGroupDestroy")
        EventGroupGetAttribute = Bind(Of FnCuptiEventGroupGetAttribute)(handle, "cuptiEventGroupGetAttribute")
        EventGroupSetAttribute = Bind(Of FnCuptiEventGroupSetAttribute)(handle, "cuptiEventGroupSetAttribute")
        EventGroupAddEvent = Bind(Of FnCuptiEventGroupAddEvent)(handle, "cuptiEventGroupAddEvent")
        EventGroupEnable = Bind(Of FnCuptiEventGroupEnable)(handle, "cuptiEventGroupEnable")
        EventGroupDisable = Bind(Of FnCuptiEventGroupDisable)(handle, "cuptiEventGroupDisable")
        EventGroupReadEvent = Bind(Of FnCuptiEventGroupReadEvent)(handle, "cuptiEventGroupReadEvent")
        EventGroupResetAllEvents = Bind(Of FnCuptiEventGroupResetAllEvents)(handle, "cuptiEventGroupResetAllEvents")

        SupportsPmSampling = ProfilerInitialize IsNot Nothing AndAlso HostInitialize IsNot Nothing AndAlso PmSamplingSetConfig IsNot Nothing
        HasLegacyEventApi = EventGroupReadEvent IsNot Nothing AndAlso EventGroupCreate IsNot Nothing AndAlso EventGroupEnable IsNot Nothing

        If GetVersion IsNot Nothing Then
            Dim raw As UInteger = 0
            Try
                If GetVersion(raw) = CUptiResult.CUPTI_SUCCESS AndAlso raw > 0UI Then
                    Version = $"{raw \ 1000UI}.{(raw Mod 1000UI) \ 10UI}"
                End If
            Catch
            End Try
        End If
    End Sub

    Private Shared Function Bind(Of TDelegate As Class)(handle As IntPtr, symbol As String) As TDelegate
        Dim address As IntPtr
        Try
            address = NativeLibrary.GetExport(handle, symbol)
        Catch ex As EntryPointNotFoundException
            Return Nothing
        Catch
            Return Nothing
        End Try

        Try
            Return Marshal.GetDelegateForFunctionPointer(Of TDelegate)(address)
        Catch
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' 从 cupti64_2026.2.1.dll（或旧式 cupti64_120.dll）文件名中解析版本字符串。
    ''' </summary>
    Private Shared Function ParseVersionFromFileName(filePath As String) As String
        If String.IsNullOrEmpty(filePath) Then Return String.Empty

        Dim name = IO.Path.GetFileNameWithoutExtension(filePath)
        Dim idx = name.IndexOf("_"c)
        If idx < 0 Then Return String.Empty

        Dim tail = name.Substring(idx + 1)

        ' 注意：本类的 Version 属性会遮蔽 System.Version，这里必须全限定
        Dim parsed As System.Version = Nothing
        If System.Version.TryParse(tail, parsed) Then Return tail

        Return String.Empty
    End Function

    ' ------------------------------------------------------------------
    ' 动态库定位
    ' ------------------------------------------------------------------

    Private Shared ReadOnly _sync As New Object()
    Private Shared _current As CuptiLibrary
    Private Shared _currentError As String
    Private Shared _resolved As Boolean

    ''' <summary>显式指定 cupti64 动态库路径（GTop --cupti）；为空则自动探测。</summary>
    Public Shared Property ExplicitPath As String

    ''' <summary>显式指定 cupti64 所在目录；为空则自动探测。</summary>
    Public Shared Property SearchFolder As String

    ''' <summary>
    ''' 取得（并缓存）本进程的 CUPTI 实例。失败时返回 Nothing 并通过
    ''' <paramref name="errorMessage"/> 给出中文原因；结果会被缓存，重复调用不会反复扫描磁盘。
    ''' </summary>
    Public Shared Function TryGetCurrent(ByRef errorMessage As String) As CuptiLibrary
        SyncLock _sync
            If Not _resolved Then
                _resolved = True
                Dim probeError As String = Nothing
                _current = TryLoadInternal(ExplicitPath, probeError)
                _currentError = probeError
            End If

            errorMessage = _currentError
            Return _current
        End SyncLock
    End Function

    ''' <summary>清空缓存（修改了 <see cref="ExplicitPath"/> 之后调用）。</summary>
    Public Shared Sub ResetCache()
        SyncLock _sync
            _resolved = False
            _currentError = Nothing
            _current = Nothing
        End SyncLock
    End Sub

    ''' <summary>探测 cupti64 是否可用（不改写缓存，便于诊断模式反复调用）。</summary>
    Public Shared Function Probe(ByRef library As CuptiLibrary, ByRef errorMessage As String) As Boolean
        library = TryLoadInternal(ExplicitPath, errorMessage)
        Return library IsNot Nothing
    End Function

    Private Shared Function TryLoadInternal(explicitPath As String, ByRef errorMessage As String) As CuptiLibrary
        errorMessage = Nothing

        ' 显式指定了路径就必须用它，不存在时直接报错（不要静默回退到自动探测）
        If Not String.IsNullOrWhiteSpace(explicitPath) AndAlso Not IO.File.Exists(explicitPath) Then
            errorMessage = $"指定的 cupti64 动态库不存在：{explicitPath}"
            Return Nothing
        End If

        Dim candidates = EnumerateCandidates(explicitPath).ToArray()

        If candidates.Length = 0 Then
            errorMessage = "未找到 cupti64 动态库。可用 --cupti <dll路径> 或设置环境变量 CUPTI_PATH；" &
                           "该库随 CUDA Toolkit 安装在 <CUDA>\extras\CUPTI\lib64\ 下。"
            Return Nothing
        End If

        Dim failures As New List(Of String)()

        For Each path In candidates
            Dim handle As IntPtr = IntPtr.Zero
            Try
                ' CUPTI 会依赖同目录下的 nvperf_* 等库，先把目录加入搜索路径
                NvrtcApi.NativeSearchPath.AddFolder(IO.Path.GetDirectoryName(path))

                If Not NativeLibrary.TryLoad(path, handle) Then
                    failures.Add($"{path}：LoadLibrary 失败")
                    Continue For
                End If

                Dim library As New CuptiLibrary(handle, path)
                If Not library.SupportsPmSampling AndAlso Not library.HasLegacyEventApi Then
                    failures.Add($"{path}：缺少 PM Sampling 与 Event API 符号")
                    NativeLibrary.Free(handle)
                    Continue For
                End If

                Return library
            Catch ex As Exception
                If handle <> IntPtr.Zero Then
                    Try : NativeLibrary.Free(handle) : Catch : End Try
                End If
                failures.Add($"{path}：{ex.Message}")
            End Try
        Next

        errorMessage = "无法载入 cupti64：" & String.Join("；", failures)
        Return Nothing
    End Function

    ''' <summary>
    ''' 枚举候选动态库路径：显式路径 → 显式目录 → 环境变量 → 标准 CUDA 安装目录（版本降序）→ 程序目录。
    ''' </summary>
    Private Shared Function EnumerateCandidates(explicitPath As String) As IEnumerable(Of String)
        Dim list As New List(Of String)()

        If Not String.IsNullOrWhiteSpace(explicitPath) Then
            If IO.File.Exists(explicitPath) Then
                Return {explicitPath}
            Else
                list.AddRange(LibraryFilesIn(IO.Path.GetDirectoryName(explicitPath)))
                If list.Count > 0 Then Return list
            End If
        End If

        Dim folders As New List(Of String)()

        If Not String.IsNullOrWhiteSpace(SearchFolder) Then folders.Add(SearchFolder)

        For Each envName In {"CUPTI_PATH", "CUPTI_HOME"}
            Dim value = Environment.GetEnvironmentVariable(envName)
            If Not String.IsNullOrWhiteSpace(value) Then
                If IO.File.Exists(value) Then
                    list.Add(value)
                    Return list
                End If
                folders.Add(value)
                folders.Add(IO.Path.Combine(value, "lib64"))
            End If
        Next

        Dim cudaPath = Environment.GetEnvironmentVariable("CUDA_PATH")
        If Not String.IsNullOrWhiteSpace(cudaPath) Then
            folders.Add(IO.Path.Combine(cudaPath, "extras", "CUPTI", "lib64"))
        End If

        Dim root = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "NVIDIA GPU Computing Toolkit", "CUDA")
        If IO.Directory.Exists(root) Then
            Dim versions = IO.Directory.GetDirectories(root) _
                .OrderByDescending(Function(d) NumericKey(IO.Path.GetFileName(d))) _
                .ToArray()

            For Each versionFolder In versions
                folders.Add(IO.Path.Combine(versionFolder, "extras", "CUPTI", "lib64"))
            Next
        End If

        folders.Add(AppContext.BaseDirectory)

        For Each folder In folders
            list.AddRange(LibraryFilesIn(folder))
        Next

        Return list.Distinct(StringComparer.OrdinalIgnoreCase)
    End Function

    Private Shared Function LibraryFilesIn(folder As String) As IEnumerable(Of String)
        If String.IsNullOrWhiteSpace(folder) OrElse Not IO.Directory.Exists(folder) Then
            Return New String() {}
        End If

        Try
            Return IO.Directory.GetFiles(folder, "cupti64*.dll") _
                .OrderByDescending(Function(f) NumericKey(IO.Path.GetFileName(f))) _
                .ToArray()
        Catch
            Return New String() {}
        End Try
    End Function

    ''' <summary>从文件名中提取数字组成的排序键（v13.3 → 13*10^6+3*10^3；cupti64_2026.2.1 → 2026*10^6+2*10^3+1）。</summary>
    Private Shared Function NumericKey(name As String) As Long
        If String.IsNullOrEmpty(name) Then Return 0

        Dim numbers = Regex.Matches(name, "\d+")
        Dim key As Long = 0

        For i As Integer = 0 To System.Math.Min(numbers.Count, 3) - 1
            Dim part As Long
            If Long.TryParse(numbers(i).Value, part) Then
                Select Case i
                    Case 0 : key += part * 1000000L
                    Case 1 : key += part * 1000L
                    Case 2 : key += part
                End Select
            End If
        Next

        Return key
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If _handle <> IntPtr.Zero Then
            Try
                NativeLibrary.Free(_handle)
            Catch
            End Try
        End If
    End Sub
End Class
