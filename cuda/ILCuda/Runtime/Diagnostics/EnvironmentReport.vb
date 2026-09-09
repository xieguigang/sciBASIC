#Region "Microsoft.VisualBasic::d4fe9f9026c3d84424a3a5fd75dcc8aa, cuda\ILCuda\Runtime\Diagnostics\EnvironmentReport.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 199
    '    Code Lines: 146 (73.37%)
    ' Comment Lines: 26 (13.07%)
    '    - Xml Docs: 88.46%
    ' 
    '   Blank Lines: 27 (13.57%)
    '     File Size: 9.96 KB


    '     Class NvrtcCandidate
    ' 
    '         Properties: FilePath, FitsDriver, VersionKey, VersionText
    ' 
    '         Function: ToString
    ' 
    '     Class EnvironmentReport
    ' 
    '         Properties: AllNvrtcNewerThanDriver, DeviceCount, Devices, DriverVersionKey, DriverVersionText
    '                     ErrorMessage, HasDevice, HasError, HasKernelSource, HasNvrtc
    '                     KernelSources, NvrtcCandidates
    ' 
    '     Module CudaEnvironment
    ' 
    '         Function: Probe, Suggest
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Computing.ILCuda.Kernels

Namespace Runtime

    ''' <summary>系统中发现的一个 NVRTC 动态库候选</summary>
    Public Class NvrtcCandidate
        ''' <summary>nvrtc64_*.dll 的完整路径</summary>
        Public Property FilePath As String
        ''' <summary>形如 13.0 的版本文本；无法解析时为 "未知版本"</summary>
        Public Property VersionText As String
        ''' <summary>与驱动版本比较用的整数键，例如 13.0 -> 130；无法解析时为 0</summary>
        Public Property VersionKey As Integer
        ''' <summary>该版本是否不高于驱动支持的 CUDA 版本</summary>
        Public Property FitsDriver As Boolean

        Public Overrides Function ToString() As String
            Dim flag = If(FitsDriver, "匹配", "高于驱动")
            Return $"[{VersionText} / {flag}] {FilePath}"
        End Function
    End Class

    ''' <summary>
    ''' 一次 CUDA 环境探测的结果：驱动版本、设备列表、NVRTC 候选、参与编译的内核源码。
    ''' 全部是纯数据，探测过程中的异常会被记录到 <see cref="ErrorMessage"/> 而不抛出。
    ''' </summary>
    Public Class EnvironmentReport
        ''' <summary>驱动支持的 CUDA 版本文本，例如 12.8；探测失败时为 "未知"</summary>
        Public Property DriverVersionText As String = "未知"
        ''' <summary>驱动支持的 CUDA 版本整数键，例如 128；探测失败时为 0</summary>
        Public Property DriverVersionKey As Integer
        ''' <summary>探测到的设备数量</summary>
        Public Property DeviceCount As Integer
        ''' <summary>设备快照</summary>
        Public Property Devices As IReadOnlyList(Of DeviceReport) = Array.Empty(Of DeviceReport)()
        ''' <summary>按优先级排序的 NVRTC 候选（优先"不高于驱动版本"的）</summary>
        Public Property NvrtcCandidates As IReadOnlyList(Of NvrtcCandidate) = Array.Empty(Of NvrtcCandidate)()
        ''' <summary>当前会参与 NVRTC 编译的内核源码（内置 + 已注册）</summary>
        Public Property KernelSources As IReadOnlyList(Of String) = Array.Empty(Of String)()
        ''' <summary>探测过程中出现的错误信息；正常时为 Nothing</summary>
        Public Property ErrorMessage As String

        Public ReadOnly Property HasError As Boolean
            Get
                Return Not String.IsNullOrEmpty(ErrorMessage)
            End Get
        End Property

        Public ReadOnly Property HasDevice As Boolean
            Get
                Return DeviceCount > 0
            End Get
        End Property

        Public ReadOnly Property HasNvrtc As Boolean
            Get
                Return NvrtcCandidates.Count > 0
            End Get
        End Property

        Public ReadOnly Property HasKernelSource As Boolean
            Get
                Return KernelSources.Count > 0
            End Get
        End Property

        ''' <summary>是否存在"所有 NVRTC 都比驱动新"的错配情况</summary>
        Public ReadOnly Property AllNvrtcNewerThanDriver As Boolean
            Get
                Dim known = NvrtcCandidates.Where(Function(c) c.VersionKey > 0).ToList()
                Return known.Count > 0 AndAlso known.All(Function(c) Not c.FitsDriver)
            End Get
        End Property
    End Class

    ''' <summary>CUDA 环境探测入口</summary>
    Public Module CudaEnvironment

        ''' <summary>
        ''' 探测当前进程的 CUDA 环境。该方法不会抛出：任何异常都会记进
        ''' <see cref="EnvironmentReport.ErrorMessage"/>，便于无条件打印诊断。
        ''' </summary>
        Public Function Probe(Optional nvrtcPath As String = Nothing) As EnvironmentReport
            Dim report As New EnvironmentReport()

            ' ---- 内核源码（不依赖驱动，先取） ----
            report.KernelSources = KernelSources.All().Select(Function(f) f.ToString()).ToList()

            ' ---- 驱动版本与设备 ----
            Try
                report.DriverVersionText = CudaDevice.DriverVersionText()
                report.DriverVersionKey = CudaDevice.DriverVersionKey()
            Catch ex As Exception
                report.ErrorMessage = $"读取驱动版本失败: {ex.Message}"
                Return report
            End Try

            Try
                Dim devices = CudaDevice.Enumerate()
                report.DeviceCount = devices.Count
                report.Devices = DeviceReport.From(devices)
            Catch ex As Exception
                report.Devices = Array.Empty(Of DeviceReport)()
                report.DeviceCount = 0
                report.ErrorMessage = $"枚举 CUDA 设备失败: {ex.Message}"
            End Try

            ' ---- NVRTC 候选 ----
            Try
                report.NvrtcCandidates = NvrtcCompiler.FindCandidates(nvrtcPath) _
                    .Select(Function(path)
                                Dim key = NvrtcLibrary.VersionNumberFromFileName(path)
                                Dim text = If(key > 0, $"{key \ 10}.{key Mod 10}", "未知版本")

                                Return New NvrtcCandidate With {
                                    .FilePath = path,
                                    .VersionKey = key,
                                    .VersionText = text,
                                    .FitsDriver = key > 0 AndAlso key <= report.DriverVersionKey
                                }
                            End Function) _
                    .ToList()
            Catch ex As Exception
                report.NvrtcCandidates = Array.Empty(Of NvrtcCandidate)()
            End Try

            Return report
        End Function

        ''' <summary>根据探测结果给出可操作的修复建议（纯数据，由调用方决定如何显示）</summary>
        Public Function Suggest(report As EnvironmentReport) As IReadOnlyList(Of FixSuggestion)
            Dim result As New List(Of FixSuggestion)()

            If report Is Nothing Then Return result

            If report.HasError Then
                result.Add(New FixSuggestion(
                    FixKind.EnvironmentError,
                    "环境探测失败",
                    report.ErrorMessage & "。请确认已安装 NVIDIA 驱动。",
                    {"检查 nvidia-smi 是否能正常运行"}))
            End If

            If Not report.HasDevice Then
                result.Add(New FixSuggestion(
                    FixKind.NoDevice,
                    "没有检测到支持 CUDA 的设备",
                    "系统中没有可用的 NVIDIA GPU，或驱动未正确安装。程序会自动回退到 CPU 参考实现。",
                    {"nvidia-smi"}))
            End If

            If Not report.HasKernelSource Then
                result.Add(New FixSuggestion(
                    FixKind.NoKernelSource,
                    "没有参与编译的内核源码",
                    "框架内置的 .cu 资源缺失，且没有通过 KernelSources.Register(...) 注册任何外部内核源码。",
                    {"确认程序集包含内嵌的 Kernels\*.cu 资源"}))
            End If

            If Not report.HasNvrtc Then
                result.Add(New FixSuggestion(
                    FixKind.InstallMatchingToolkit,
                    "没有找到 NVRTC 动态库",
                    "请安装与驱动匹配的 CUDA Toolkit，或把 nvrtc64_*.dll 复制到程序目录后用 --nvrtc 指定。",
                    {"set CUDA_PATH", "ILCuda demo --nvrtc <nvrtc64_*.dll>"}))

                result.Add(New FixSuggestion(
                    FixKind.OfflineCompileCubin,
                    "离线编译内核作为兜底镜像",
                    "用 nvcc 把内核源码编译成 cubin/ptx，再用 --cubin 指定（同样要求工具包版本不高于驱动）。",
                    {"ILCuda emit-kernels ./kernels",
                     "nvcc -arch=sm_86 -cubin -o kernels/kernels.cubin kernels/all.cu",
                     "ILCuda demo --cubin kernels/kernels.cubin"}))
            ElseIf report.AllNvrtcNewerThanDriver Then
                result.Add(New FixSuggestion(
                    FixKind.UpgradeDriver,
                    "所有 NVRTC 版本都高于驱动支持的 CUDA 版本",
                    $"当前驱动支持 CUDA {report.DriverVersionText}；更高版本 NVRTC 产出的 PTX 会被驱动拒绝。",
                    {"升级 NVIDIA 驱动（CUDA 13.x 需要 r580 及以上，CUDA 12.8 对应 572.x）"}))

                result.Add(New FixSuggestion(
                    FixKind.InstallMatchingToolkit,
                    "安装与驱动匹配的 CUDA 工具包",
                    $"例如驱动 {report.DriverVersionText} 对应 CUDA Toolkit {report.DriverVersionText}，会提供同版本的 nvrtc64_*.dll。",
                    {"安装对应版本的 CUDA Toolkit"}))

                result.Add(New FixSuggestion(
                    FixKind.OfflineCompileCubin,
                    "离线编译内核作为兜底镜像",
                    "若暂时无法升级驱动/工具包，可用 nvcc 离线编译后用 --cubin 指定。注意预编译镜像同样要求" &
                    "『工具包版本 <= 驱动支持的 CUDA 版本』，否则驱动会在 cuModuleLoadData 内部崩溃。",
                    {"ILCuda emit-kernels ./kernels",
                     "nvcc -arch=sm_86 -cubin -o kernels/kernels.cubin kernels/all.cu",
                     "ILCuda demo --cubin kernels/kernels.cubin"}))
            End If

            Return result
        End Function
    End Module
End Namespace

