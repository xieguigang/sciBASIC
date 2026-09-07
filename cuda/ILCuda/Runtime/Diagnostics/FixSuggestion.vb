Namespace Runtime

    ''' <summary>修复建议的分类</summary>
    Public Enum FixKind
        ''' <summary>环境探测本身失败（驱动不可用等）</summary>
        EnvironmentError
        ''' <summary>没有可用的 CUDA 设备</summary>
        NoDevice
        ''' <summary>没有任何参与编译的内核源码</summary>
        NoKernelSource
        ''' <summary>缺少 NVRTC 或版本与驱动不匹配</summary>
        InstallMatchingToolkit
        ''' <summary>需要升级显卡驱动</summary>
        UpgradeDriver
        ''' <summary>改用 nvcc 离线编译 cubin</summary>
        OfflineCompileCubin
        ''' <summary>显式指定 nvrtc64_*.dll 路径</summary>
        SpecifyNvrtc
    End Enum

    ''' <summary>
    ''' 一条可操作的修复建议（纯数据，由调用方决定如何呈现）
    ''' </summary>
    Public Class FixSuggestion
        Public Property Kind As FixKind
        ''' <summary>一句话标题</summary>
        Public Property Title As String
        ''' <summary>详细说明</summary>
        Public Property Detail As String
        ''' <summary>可以直接执行的命令 / 操作步骤</summary>
        Public Property Commands As IReadOnlyList(Of String)

        Public Sub New()
            Commands = Array.Empty(Of String)()
        End Sub

        Public Sub New(kind As FixKind, title As String, detail As String,
                       Optional commands As IEnumerable(Of String) = Nothing)
            Me.Kind = kind
            Me.Title = title
            Me.Detail = detail
            Me.Commands = If(commands Is Nothing, Array.Empty(Of String)(), commands.ToList())
        End Sub

        Public ReadOnly Property HasCommands As Boolean
            Get
                Return Commands IsNot Nothing AndAlso Commands.Count > 0
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{Kind}] {Title}"
        End Function
    End Class
End Namespace
