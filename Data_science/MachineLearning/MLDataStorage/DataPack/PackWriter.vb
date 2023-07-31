Imports System.IO
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem

Public Class PackWriter : Implements IDisposable

    Dim disposedValue As Boolean
    Dim stream As StreamPack

    Sub New(stream As Stream)
        Me.stream = New StreamPack(stream, meta_size:=32 * 1024 * 1024, [readonly]:=False)
    End Sub

    Public Sub AddSamples(samples As IEnumerable(Of Sample))
        Dim allSamples As New List(Of Sample)

        For Each sample As Sample In samples
            Using file As Stream = stream.OpenBlock($"/samples/{sample.ID}.dat")
                Dim buf As New BinaryDataWriter(file, byteOrder:=ByteOrder.BigEndian)

                Call buf.Write(sample.label, BinaryStringFormat.DwordLengthPrefix)
                Call buf.Write(sample.ID, BinaryStringFormat.DwordLengthPrefix)
                Call buf.Write(sample.target)
                Call buf.Write(sample.vector)
            End Using
        Next
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: 释放托管状态(托管对象)
                Call stream.Dispose()
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
