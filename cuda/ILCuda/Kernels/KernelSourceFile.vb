Namespace Kernels

    ''' <summary>
    ''' 一个内核源码文件（通常来自程序集的内嵌资源，也可以是运行时注册的内联源码）
    ''' </summary>
    Public Class KernelSourceFile

        ''' <summary>资源全名，例如 <c>Enigma.ILCuda.Kernels.basic.cu</c></summary>
        Public Property Name As String

        ''' <summary>源码文本</summary>
        Public Property Text As String

        ''' <summary>
        ''' 来源标识：框架自带为 <see cref="KernelSources.BuiltinOrigin"/>，
        ''' 外部注册时默认取程序集名。
        ''' </summary>
        Public Property Origin As String

        ''' <summary>去重键：来源 + 资源名</summary>
        Public ReadOnly Property Key As String
            Get
                Return $"{If(Origin, String.Empty)}::{If(Name, String.Empty)}"
            End Get
        End Property

        ''' <summary>
        ''' 由资源名推导出的文件名（basic.cu）。
        ''' 资源名形如 <c>xxx.Kernels.basic.cu</c>，这里取 "Kernels." 之后的部分；
        ''' 推导失败时退回资源名本身。
        ''' </summary>
        Public ReadOnly Property FileName As String
            Get
                Dim full = If(Name, String.Empty)

                ' 资源名形如 <RootNamespace>.<子目录>.xxx.cu，这里尽量还原成 xxx.cu：
                '   1) 优先取 "Kernels." 之后的部分；
                '   2) 否则取最后两段（MSBuild 默认会把子目录丢掉，只剩 根命名空间.xxx.cu）。
                Dim idx = full.LastIndexOf("Kernels.", StringComparison.Ordinal)

                If idx >= 0 Then
                    full = full.Substring(idx + 8)
                Else
                    Dim parts = full.Split("."c)

                    If parts.Length >= 3 AndAlso
                       parts(parts.Length - 1).Equals("cu", StringComparison.OrdinalIgnoreCase) Then
                        full = parts(parts.Length - 2) & "." & parts(parts.Length - 1)
                    End If
                End If

                If String.IsNullOrWhiteSpace(full) Then full = If(Name, "kernel.cu")
                If Not full.EndsWith(".cu", StringComparison.OrdinalIgnoreCase) Then full &= ".cu"

                Return full
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{Origin}/{FileName}"
        End Function
    End Class
End Namespace
