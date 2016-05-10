Imports System.IO

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

        ''' <summary>
        ''' Get user home folder
        ''' </summary>
        ''' <returns></returns>
        Public Function HOME() As String
            Dim homeDrive As String = Environment.GetEnvironmentVariable("HOMEDRIVE")
            Dim homePath = Environment.GetEnvironmentVariable("HOMEPATH")

            Call VBDebugger.Assertion(Not String.IsNullOrEmpty(homeDrive), "Environment variable error, there is no 'HOMEDRIVE'")
            Call VBDebugger.Assertion(Not String.IsNullOrEmpty(homePath), "Environment variable error, there is no 'HOMEPATH'")

            Dim fullHomePath As String = homeDrive & Path.DirectorySeparatorChar & homePath
            Return fullHomePath
        End Function
    End Module
End Namespace