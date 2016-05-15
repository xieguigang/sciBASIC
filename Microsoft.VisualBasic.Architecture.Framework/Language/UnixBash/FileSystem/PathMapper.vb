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
                path = Mid(path, 2)

                If path.First = "/" Then
                    path = App.UsrHome & "/" & path
                Else
                    ' ~username
                    Dim DIR As String = App.UsrHome.ParentPath
                    path = DIR & "/" & path
                End If
            ElseIf path.First = "/" Then  ' /   ROOT
                path = "C:\" & path
            ElseIf InStr(path, "/usr/bin", CompareMethod.Text) = 1 Then
                path = Mid(path, 9)
                path = "C:\Program Files/" & path
            ElseIf InStr(path, "/usr", CompareMethod.Text) = 1 Then
                path = Mid(path, 5)
                path = App.UsrHome.ParentPath & "/" & path
            ElseIf InStr(path, "-/") = 1 Then
                ' 前一个文件夹
                path = Mid(path, 2)
                path = App.PreviousDirectory & "/" & path
            ElseIf path = "-" Then
                path = App.PreviousDirectory
            End If

            Return path
        End Function

        ''' <summary>
        ''' Get user home folder
        ''' </summary>
        ''' <returns></returns>
        Public Function HOME() As String
            If platform = PlatformID.MacOSX OrElse platform = PlatformID.Unix Then
                Return Environment.GetEnvironmentVariable("HOME")
            End If

            Dim homeDrive As String = Environment.GetEnvironmentVariable("HOMEDRIVE")
            Dim homePath = Environment.GetEnvironmentVariable("HOMEPATH")

            Call VBDebugger.Assertion(Not String.IsNullOrEmpty(homeDrive), "Environment variable error, there is no 'HOMEDRIVE'")
            Call VBDebugger.Assertion(Not String.IsNullOrEmpty(homePath), "Environment variable error, there is no 'HOMEPATH'")

            Dim fullHomePath As String = homeDrive & Path.DirectorySeparatorChar & homePath
            Return fullHomePath
        End Function
    End Module
End Namespace