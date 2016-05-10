Namespace Language.UnixBash

    ''' <summary>
    ''' 这个模块是将Linux路径映射为Windows路径的
    ''' </summary>
    Public Module PathMapper

        ''' <summary>
        ''' Gets a System.PlatformID enumeration value that identifies the operating system
        ''' platform.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property platform As PlatformID = Environment.OSVersion.Platform

        Public Function GetMapPath(path As String) As String
            If platform = PlatformID.MacOSX OrElse platform = PlatformID.Unix Then
                Return path
            End If

            If path.First = "~" Then ' HOME

            End If

            Return path
        End Function
    End Module
End Namespace