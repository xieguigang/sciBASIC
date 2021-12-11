Namespace Net.Http.JsonRPC

    Public Class RpcRequest

        Public Property jsonrpc As String
        Public Property method As String
        Public Property params As Dictionary(Of String, Object)
        Public Property id As Integer

    End Class
End Namespace


