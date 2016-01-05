Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.Reflection
Imports System.Security
Imports System.Threading

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Friend Class IOUtils
        ' Methods
        Private Sub New()
        End Sub

        Private Shared Function FindFileFilter(oAssemblyData As AssemblyData) As String
            Dim dirFiles As FileSystemInfo() = oAssemblyData.m_DirFiles
            Dim dirNextFileIndex As Integer = oAssemblyData.m_DirNextFileIndex
            Do While True
                If (dirNextFileIndex > dirFiles.GetUpperBound(0)) Then
                    oAssemblyData.m_DirFiles = Nothing
                    oAssemblyData.m_DirNextFileIndex = 0
                    Return Nothing
                End If
                Dim info As FileSystemInfo = dirFiles(dirNextFileIndex)
                If (((info.Attributes And (FileAttributes.Directory Or (FileAttributes.System Or FileAttributes.Hidden))) = 0) OrElse ((info.Attributes And oAssemblyData.m_DirAttributes) <> 0)) Then
                    oAssemblyData.m_DirNextFileIndex = (dirNextFileIndex + 1)
                    Return dirFiles(dirNextFileIndex).Name
                End If
                dirNextFileIndex += 1
            Loop
        End Function

        <SecurityCritical>
        Friend Shared Function FindFirstFile(assem As Assembly, PathName As String, Attributes As FileAttributes) As String
            Dim fullPath As String = Nothing
            Dim fileName As String
            Dim fileSystemInfos As FileSystemInfo()
            If ((PathName.Length > 0) AndAlso (PathName.Chars((PathName.Length - 1)) = Path.DirectorySeparatorChar)) Then
                fullPath = Path.GetFullPath(PathName)
                fileName = "*.*"
            Else
                If (PathName.Length = 0) Then
                    fileName = "*.*"
                Else
                    fileName = Path.GetFileName(PathName)
                    fullPath = Path.GetDirectoryName(PathName)
                    If (((fileName Is Nothing) OrElse (fileName.Length = 0)) OrElse (fileName = ".")) Then
                        fileName = "*.*"
                    End If
                End If
                If ((fullPath Is Nothing) OrElse (fullPath.Length = 0)) Then
                    If Path.IsPathRooted(PathName) Then
                        fullPath = Path.GetPathRoot(PathName)
                    Else
                        fullPath = Environment.CurrentDirectory
                        If (fullPath.Chars((fullPath.Length - 1)) <> Path.DirectorySeparatorChar) Then
                            fullPath = (fullPath & Conversions.ToString(Path.DirectorySeparatorChar))
                        End If
                    End If
                ElseIf (fullPath.Chars((fullPath.Length - 1)) <> Path.DirectorySeparatorChar) Then
                    fullPath = (fullPath & Conversions.ToString(Path.DirectorySeparatorChar))
                End If
                If (fileName = "..") Then
                    fullPath = (fullPath & "..\")
                    fileName = "*.*"
                End If
            End If
            Try
                fileSystemInfos = Directory.GetParent((fullPath & fileName)).GetFileSystemInfos(fileName)
            Catch exception As SecurityException
                Throw exception
            Catch obj1 As Object When (?)
                Throw ExceptionUtils.VbMakeException(&H34)
            Catch exception3 As StackOverflowException
                Throw exception3
            Catch exception4 As OutOfMemoryException
                Throw exception4
            Catch exception5 As ThreadAbortException
                Throw exception5
            Catch exception9 As Exception
                Return ""
            End Try
            Dim assemblyData As AssemblyData = ProjectData.GetProjectData.GetAssemblyData(assem)
            assemblyData.m_DirFiles = fileSystemInfos
            assemblyData.m_DirNextFileIndex = 0
            assemblyData.m_DirAttributes = Attributes
            If ((fileSystemInfos Is Nothing) OrElse (fileSystemInfos.Length = 0)) Then
                Return ""
            End If
            Return IOUtils.FindFileFilter(assemblyData)
        End Function

        Friend Shared Function FindNextFile(assem As Assembly) As String
            Dim assemblyData As AssemblyData = ProjectData.GetProjectData.GetAssemblyData(assem)
            If (assemblyData.m_DirFiles Is Nothing) Then
                Throw New ArgumentException(Utils.GetResourceString("DIR_IllegalCall"))
            End If
            If (assemblyData.m_DirNextFileIndex > assemblyData.m_DirFiles.GetUpperBound(0)) Then
                assemblyData.m_DirFiles = Nothing
                assemblyData.m_DirNextFileIndex = 0
                Return Nothing
            End If
            Return IOUtils.FindFileFilter(assemblyData)
        End Function

    End Class
End Namespace

