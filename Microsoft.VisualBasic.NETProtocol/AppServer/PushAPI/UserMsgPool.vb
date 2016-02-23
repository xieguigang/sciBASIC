Imports Microsoft.VisualBasic.Net.Protocols

Namespace PushAPI

    Public Class UserMsgPool

        ReadOnly __msgs As New Dictionary(Of Long, Queue(Of RequestStream))

        Public Sub Allocation(uid As Long)
            Call __msgs.Add(uid, New Queue(Of RequestStream))
        End Sub

        Public Sub Push(uid As Long, msg As RequestStream)
            Call __msgs(uid).Enqueue(msg)
        End Sub

        Public Function Pop(uid As Long) As RequestStream
            Dim pool = __msgs(uid)
            If pool.Count = 0 Then
                Return Nothing
            Else
                Return pool.Dequeue()
            End If
        End Function
    End Class
End Namespace