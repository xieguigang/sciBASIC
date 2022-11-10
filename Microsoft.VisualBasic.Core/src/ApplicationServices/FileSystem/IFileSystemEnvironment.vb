
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
        ''' close current filesystem session
        ''' </summary>
        ''' <remarks>
        ''' apply for the zip archive/HDS streampack
        ''' </remarks>
        Sub Close()

    End Interface
End Namespace