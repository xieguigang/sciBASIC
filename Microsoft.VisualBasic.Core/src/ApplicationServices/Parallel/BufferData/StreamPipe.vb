Imports System.IO
Imports Microsoft.VisualBasic.Linq

Namespace Parallel

    Public Class StreamPipe : Inherits BufferPipe
        Implements IDisposable

        ReadOnly buf As Stream
        Private disposedValue As Boolean

        Sub New(buf As Stream)
            Me.buf = buf
        End Sub

        Public Overrides Iterator Function GetBlocks() As IEnumerable(Of Byte())
            Dim chunk As Byte() = New Byte(1024 - 1) {}
            Dim delta As Long

            Do While buf.Position <= (buf.Length - 1)
                delta = buf.Length - buf.Position

                If delta < chunk.Length Then
                    chunk = New Byte(delta - 1) {}
                End If

                buf.Read(chunk, Scan0, chunk.Length)

                Yield chunk
            Loop
        End Function

        Public Overrides Function Read() As Byte()
            Return GetBlocks.IteratesALL.ToArray
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    buf.Close()
                    buf.Dispose()
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并替代终结器
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