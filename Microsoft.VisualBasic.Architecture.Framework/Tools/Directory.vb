Public Class Directory

    Public ReadOnly Property DIR As String

    Sub New(DIR As String)
        Me.DIR = FileIO.FileSystem.GetDirectoryInfo(DIR).FullName
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="file">相对路径</param>
    ''' <returns></returns>
    Public Function GetFullPath(file As String) As String
        If Not IsAbsolutePath(file) Then
            file = $"{DIR}/{file}"
        End If

        file = FileIO.FileSystem.GetFileInfo(file).FullName
        Return file
    End Function

    Public Shared Function IsAbsolutePath(file As String) As Boolean
        If InStr(file, ":\") > 0 Then
            Return True
        ElseIf file.First = "/" AndAlso
            (Environment.OSVersion.Platform = PlatformID.Unix OrElse
            Environment.OSVersion.Platform = PlatformID.MacOSX) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Overrides Function ToString() As String
        Return DIR
    End Function
End Class
