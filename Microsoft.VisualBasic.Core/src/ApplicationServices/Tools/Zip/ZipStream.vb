Imports System.IO
Imports System.IO.Compression

Namespace ApplicationServices.Zip

    Public Class ZipStream : Implements IFileSystemEnvironment, IDisposable

        Dim disposedValue As Boolean

        Public ReadOnly Property [readonly] As Boolean Implements IFileSystemEnvironment.readonly
        Public ReadOnly Property zip As ZipArchive

        Sub New(file As Stream, Optional is_readonly As Boolean = False)
            [readonly] = is_readonly

            If is_readonly Then
                zip = New ZipArchive(file, ZipArchiveMode.Read)
            Else
                zip = New ZipArchive(file, ZipArchiveMode.Update)
            End If
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
        Public Function GetFileEntry(path As String) As ZipArchiveEntry
            If [readonly] Then
                Return zip.GetEntry(path)
            Else

            End If
        End Function

        Public Function OpenFile(path As String, Optional mode As FileMode = FileMode.OpenOrCreate, Optional access As FileAccess = FileAccess.Read) As Stream Implements IFileSystemEnvironment.OpenFile
            Throw New NotImplementedException()
        End Function

        Public Function DeleteFile(path As String) As Boolean Implements IFileSystemEnvironment.DeleteFile
            Throw New NotImplementedException()
        End Function

        Public Function FileExists(path As String, Optional ZERO_Nonexists As Boolean = False) As Boolean Implements IFileSystemEnvironment.FileExists
            Throw New NotImplementedException()
        End Function

        Public Function FileSize(path As String) As Long Implements IFileSystemEnvironment.FileSize
            Throw New NotImplementedException()
        End Function

        Public Function GetFullPath(filename As String) As String Implements IFileSystemEnvironment.GetFullPath
            Throw New NotImplementedException()
        End Function

        Public Function WriteText(text As String, path As String) As Boolean Implements IFileSystemEnvironment.WriteText
            Throw New NotImplementedException()
        End Function

        Public Function ReadAllText(path As String) As String Implements IFileSystemEnvironment.ReadAllText
            Throw New NotImplementedException()
        End Function

        Public Function GetFiles() As IEnumerable(Of String) Implements IFileSystemEnvironment.GetFiles
            Throw New NotImplementedException()
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