Namespace Runtime

    ''' <summary>一份可以直接喂给 cuModuleLoadData 的内核镜像</summary>
    Public Class KernelImage
        ''' <summary>PTX 文本或 cubin 二进制</summary>
        Public Property Image As Byte()
        ''' <summary>镜像来源描述，例如 "NVRTC 11.2"</summary>
        Public Property Source As String
        ''' <summary>编译使用的 -arch，例如 compute_86</summary>
        Public Property Arch As String
        ''' <summary>补充信息（动态库路径或文件路径）</summary>
        Public Property Detail As String

        Public Overrides Function ToString() As String
            If String.IsNullOrEmpty(Arch) Then
                Return $"{Source} [{Detail}]"
            End If
            Return $"{Source} arch={Arch} [{Detail}]"
        End Function
    End Class
End Namespace