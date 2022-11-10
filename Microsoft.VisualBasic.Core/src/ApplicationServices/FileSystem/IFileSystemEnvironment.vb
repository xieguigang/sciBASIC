
Imports System.IO

Namespace ApplicationServices

    ''' <summary>
    ''' an abstract interface for union filesystem:
    ''' 
    ''' 1. local filesystem
    ''' 2. zip archive
    ''' 3. HDS streampack filesystem
    ''' 
    ''' </summary>
    Public Interface IFileSystemEnvironment

        ''' <summary>
        ''' the file system environment is readonly?
        ''' </summary>
        ReadOnly Property [readonly] As Boolean

        ''' <summary>
        ''' open a specific file for read and write
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="mode"></param>
        ''' <param name="access"></param>
        ''' <returns></returns>
        Function OpenFile(path As String,
                          Optional mode As FileMode = FileMode.OpenOrCreate,
                          Optional access As FileAccess = FileAccess.Read) As Stream
        ''' <summary>
        ''' delete target file which is specific by path
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Function DeleteFile(path As String) As Boolean
        ''' <summary>
        ''' check the specific file is exists on current filesystem or not?
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Function FileExists(path As String) As Boolean
        ''' <summary>
        ''' get file size
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns>-1 means file is not exists on the filesystem</returns>
        Function FileSize(path As String) As Long
        ''' <summary>
        ''' write text content to a specific file
        ''' </summary>
        ''' <param name="text"></param>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Function WriteText(text As String, path As String) As Boolean
        Function ReadAllText(path As String) As String

        ''' <summary>
        ''' close current filesystem session
        ''' </summary>
        ''' <remarks>
        ''' apply for the zip archive/HDS streampack
        ''' </remarks>
        Sub Close()
        ''' <summary>
        ''' save stream data
        ''' </summary>
        ''' <remarks>
        ''' apply for the zip archive/HDS streampack
        ''' </remarks>
        Sub Flush()

    End Interface
End Namespace