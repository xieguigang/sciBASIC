
''' <summary>
''' A wrapper object for the processing of relative file path. 
''' </summary>
Public Class Directory

    Public ReadOnly Property DIR As String

    ''' <summary>
    ''' Construct a directory object from the specific Dir path value.
    ''' </summary>
    ''' <param name="DIR">Target directory path</param>
    Sub New(DIR As String)
        Me.DIR = FileIO.FileSystem.GetDirectoryInfo(DIR).FullName
    End Sub

    ''' <summary>
    ''' Gets the full path of the target file based on the path relative to this directory object.
    ''' </summary>
    ''' <param name="file">
    ''' The relative path of the target file, and this parameter is also compatible with absolute file path.
    ''' (相对路径)</param>
    ''' <returns></returns>
    Public Function GetFullPath(file As String) As String
        If Not IsAbsolutePath(file) Then
            file = $"{DIR}/{file}"
        End If

        file = FileIO.FileSystem.GetFileInfo(file).FullName
        Return file
    End Function

    ''' <summary>
    ''' Determined that the input file path is a absolute path or not?
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    Public Shared Function IsAbsolutePath(file As String) As Boolean
        If InStr(file, ":\") > 0 OrElse InStr(file, ":/") > 0 Then
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
