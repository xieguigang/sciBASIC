Namespace Net.Http.JsonRPC

    Public Class RpcResponse

        Public Property jsonrpc As String
        Public Property result As Object
        Public Property [error] As RpcError
        Public Property id As Integer

    End Class

    Public Enum ErrorCode As Integer
        Success = 0
        ''' <summary>
        ''' Invalid JSON was received by the server.
        ''' An Error occurred On the server While parsing the JSON text.
        ''' </summary>
        ParserError = -32700
        ''' <summary>
        ''' The JSON sent is not a valid Request object.
        ''' </summary>
        InvalidRequest = -32600
        ''' <summary>
        ''' The method does not exist / is not available.
        ''' </summary>
        MethodNotFound = -32601
        ''' <summary>
        ''' Invalid method parameter(s).
        ''' </summary>
        InvalidParams = -32602
        InternalError = -32603
    End Enum

    Public Class RpcError

        Public Property code As ErrorCode
        Public Property message As String

    End Class
End Namespace