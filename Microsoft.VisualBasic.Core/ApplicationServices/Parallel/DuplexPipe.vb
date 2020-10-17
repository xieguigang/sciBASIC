Imports System.Threading
Imports Microsoft.VisualBasic.Language

Namespace Parallel

    Public Class DuplexPipe : Inherits BufferPipe

        ReadOnly dataFragments As New Queue(Of Byte())
        ReadOnly writeClose As New Value(Of Boolean)(False)

        Public Sub Close()
            writeClose.Value = True
        End Sub

        Public Sub Write(buffer As IEnumerable(Of Byte))
            SyncLock dataFragments
                dataFragments.Enqueue(buffer.ToArray)
            End SyncLock
        End Sub

        Public Overrides Function Read() As Byte()
            Do While dataFragments.Count = 0
                If writeClose.Value AndAlso dataFragments.Count = 0 Then
                    Return {}
                Else
                    Call Thread.Sleep(1)
                End If
            Loop

            SyncLock dataFragments
                Return dataFragments.Dequeue
            End SyncLock
        End Function

        Public Overrides Iterator Function GetBlocks() As IEnumerable(Of Byte())
            Do While Not writeClose.Value
                Yield Read()
            Loop
        End Function
    End Class

    Public MustInherit Class BufferPipe

        Public MustOverride Iterator Function GetBlocks() As IEnumerable(Of Byte())
        Public MustOverride Function Read() As Byte()

    End Class

    Public Class DataPipe : Inherits BufferPipe

        ReadOnly data As Byte()

        Sub New(data As IEnumerable(Of Byte))
            Me.data = data.ToArray
        End Sub

        Sub New(data As RequestStream)
            Call Me.New(data.Serialize)
        End Sub

        Public Overrides Iterator Function GetBlocks() As IEnumerable(Of Byte())
            Yield data
        End Function

        Public Overrides Function Read() As Byte()
            Return data
        End Function
    End Class
End Namespace