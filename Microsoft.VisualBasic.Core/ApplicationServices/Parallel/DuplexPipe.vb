Imports System.Threading
Imports Microsoft.VisualBasic.Language

Namespace Parallel

    Public Class DuplexPipe

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

        Public Function Read() As Byte()
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

        Public Iterator Function GetBlocks() As IEnumerable(Of Byte())
            Do While Not writeClose.Value
                Yield Read()
            Loop
        End Function
    End Class
End Namespace