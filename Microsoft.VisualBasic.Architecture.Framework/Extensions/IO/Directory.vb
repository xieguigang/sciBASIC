
Namespace FileIO

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

        Public Shared Function Exists(DIR As String) As Boolean
            Return IO.Directory.Exists(DIR)
        End Function

        ''' <summary>
        ''' Creates a directory.
        ''' </summary>
        ''' <param name="junctionPoint">Name and location of the directory.</param>
        ''' <remarks>
        ''' Exceptions:
        '''   T:System.ArgumentException:
        '''     The directory name is malformed. For example, it contains illegal characters
        '''     or is only white space.
        '''
        '''   T:System.ArgumentNullException:
        '''     directory is Nothing or an empty string.
        '''
        '''   T:System.IO.PathTooLongException:
        '''     The directory name is too long.
        '''
        '''   T:System.NotSupportedException:
        '''     The directory name is only a colon (:).
        '''
        '''   T:System.IO.IOException:
        '''     The parent directory of the directory to be created is read-only
        '''
        '''   T:System.UnauthorizedAccessException:
        '''     The user does not have permission to create the directory.
        ''' </remarks>
        Public Shared Sub CreateDirectory(junctionPoint As String)
            Call FileIO.FileSystem.CreateDirectory(junctionPoint)
        End Sub

        Public Shared Sub Delete(DIR As String)
            Call IO.Directory.Delete(DIR)
        End Sub
    End Class
End Namespace