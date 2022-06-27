
Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.FileIO.Path

''' <summary>
''' Hierarchical Data Stream Pack, A hdf5 liked file format
''' </summary>
Public Class StreamPack : Implements IDisposable

    ReadOnly superBlock As StreamGroup
    ReadOnly buffer As Stream
    ReadOnly init_size As Integer

    Private disposedValue As Boolean

    Sub New(filepath As String, Optional init_size As Integer = 1024)
        Call Me.New(filepath.Open(FileMode.OpenOrCreate, doClear:=False, [readOnly]:=False), init_size:=init_size)
    End Sub

    Sub New(buffer As Stream, Optional init_size As Integer = 1024)
        Me.buffer = buffer
        Me.init_size = init_size

        If buffer.Length > 0 Then
            Call ParseTree()
        Else
            superBlock = StreamGroup.CreateRootTree
        End If
    End Sub

    Private Sub ParseTree()

    End Sub

    ''' <summary>
    ''' open a data block for read and write
    ''' 
    ''' if the target file block is missing from the tree, then this function will append a new file block
    ''' otherwise a substream object will be returns for read data
    ''' </summary>
    ''' <param name="fileName"></param>
    ''' <returns></returns>
    Public Function OpenBlock(fileName As String) As Stream
        Dim path As New FilePath("/" & fileName)
        Dim block As StreamBlock

        If path.IsDirectory Then
            Throw New Exception($"can not open a directry({fileName}) as a data block!")
        End If

        If superBlock.BlockExists(path) Then
            ' get current object data
            block = superBlock.GetDataBlock(path)

            Return New SubStream(buffer, block.offset, block.size)
        Else
            ' create a new data object
            block = superBlock.AddDataBlock(path)

            Return New StreamBuffer(buffer, block, init_size)
        End If
    End Function

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: 释放托管状态(托管对象)
                Call buffer.Flush()
                Call buffer.Close()
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
