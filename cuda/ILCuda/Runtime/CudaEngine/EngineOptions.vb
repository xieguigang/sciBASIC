Namespace Runtime

    ''' <summary>引擎创建参数</summary>
    Public Class EngineOptions
        ''' <summary>使用第几块 GPU（默认 0）</summary>
        Public Property DeviceOrdinal As Integer = 0
        ''' <summary>--nvrtc 显式指定的 NVRTC 动态库路径</summary>
        Public Property NvrtcPath As String
        ''' <summary>--cubin 显式指定的预编译镜像路径</summary>
        Public Property ImagePath As String
        ''' <summary>--force-image：跳过镜像与驱动版本的兼容性预检（可能导致驱动崩溃，仅供调试）</summary>
        Public Property ForceImage As Boolean = False
        ''' <summary>内核获取过程的诊断日志</summary>
        Public Property Diagnostics As List(Of String) = New List(Of String)()
        ''' <summary>失败原因</summary>
        Public Property ErrorMessage As String
    End Class
End Namespace