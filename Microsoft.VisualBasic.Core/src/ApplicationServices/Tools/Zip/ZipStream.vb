Imports System.IO
Imports System.IO.Compression

Namespace ApplicationServices.Zip

    ''' <summary>
    ''' using a zip archive file as a virtual filesystem
    ''' </summary>
    Public Class ZipStream : Implements IFileSystemEnvironment, IDisposable

        Dim disposedValue As Boolean
        Dim virtual_fs As FileSystemTree

        Public ReadOnly Property [readonly] As Boolean Implements IFileSystemEnvironment.readonly
        Public ReadOnly Property zip As ZipArchive

        Sub New(file As Stream, Optional is_readonly As Boolean = False)
            [readonly] = is_readonly

            If is_readonly Then
                zip = New ZipArchive(file, ZipArchiveMode.Read)
            Else
                zip = New ZipArchive(file, ZipArchiveMode.Update)
            End If

            virtual_fs = FileSystemTree.BuildTree(GetFiles)
        End Sub

        Sub New(filepath As String, Optional is_readonly As Boolean = False)
            Call Me.New(filepath.Open(FileMode.OpenOrCreate, doClear:=False, [readOnly]:=is_readonly), is_readonly)
        End Sub

        Public Sub Close() Implements IFileSystemEnvironment.Close
            Call zip.Dispose()
        End Sub

        Public Sub Flush() Implements IFileSystemEnvironment.Flush
            ' do nothing
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns>
        ''' return nothing if the required archive entry could not be found
        ''' </returns>
        Public Function GetFileEntry(path As String, allow_new As Boolean) As ZipArchiveEntry
            Dim ref As FileSystemTree = FileSystemTree.GetFile(virtual_fs, path)

            If Not ref Is Nothing Then
                Return zip.GetEntry(CStr(ref.data))
            End If

            If [readonly] Then
                Return Nothing
            Else
                ' create new?
                Throw New NotImplementedException
            End If
        End Function

        Public Function OpenFile(path As String, Optional mode As FileMode = FileMode.OpenOrCreate, Optional access As FileAccess = FileAccess.Read) As Stream Implements IFileSystemEnvironment.OpenFile
            Dim file As ZipArchiveEntry = GetFileEntry(path, allow_new:=mode = FileMode.OpenOrCreate OrElse mode = FileMode.CreateNew)

            If file Is Nothing Then
                Return Nothing
            Else
                Return file.Open
            End If
        End Function

        Public Function DeleteFile(path As String) As Boolean Implements IFileSystemEnvironment.DeleteFile
            Dim node = FileSystemTree.DeleteFile(virtual_fs, path)

            If Not node Is Nothing Then
                Call zip.GetEntry(CStr(node.data)).Delete()
            End If

            Return True
        End Function

        Public Function FileExists(path As String, Optional ZERO_Nonexists As Boolean = False) As Boolean Implements IFileSystemEnvironment.FileExists
            Dim file As ZipArchiveEntry = GetFileEntry(path, allow_new:=False)

            If file Is Nothing Then
                Return False
            ElseIf file.Length = 0 Then
                Return ZERO_Nonexists
            Else
                Return True
            End If
        End Function

        Public Function FileSize(path As String) As Long Implements IFileSystemEnvironment.FileSize
            Dim file As ZipArchiveEntry = GetFileEntry(path, allow_new:=False)

            If file Is Nothing Then
                Return -1
            Else
                Return file.Length
            End If
        End Function

        Public Function GetFullPath(filename As String) As String Implements IFileSystemEnvironment.GetFullPath
            Return "/" & filename
        End Function

        Public Function WriteText(text As String, path As String) As Boolean Implements IFileSystemEnvironment.WriteText
            Dim file As ZipArchiveEntry = GetFileEntry(path, allow_new:=True)

            If file Is Nothing Then
                Throw New NotImplementedException
            End If

            Using s As New StreamWriter(file.Open)
                Call s.WriteLine(text)
                Call s.Flush()
            End Using

            Return True
        End Function

        Public Function ReadAllText(path As String) As String Implements IFileSystemEnvironment.ReadAllText
            Dim file As ZipArchiveEntry = GetFileEntry(path, allow_new:=False)

            If file Is Nothing Then
                Return Nothing
            Else
                Using s As New StreamReader(file.Open)
                    Return s.ReadToEnd
                End Using
            End If
        End Function

        Public Function GetFiles() As IEnumerable(Of String) Implements IFileSystemEnvironment.GetFiles
            Return zip.Entries.Select(Function(f) f.FullName)
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Call Flush()
                    Call Close()
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace