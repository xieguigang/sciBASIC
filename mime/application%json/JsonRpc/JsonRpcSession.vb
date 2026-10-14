Imports System.Net.Sockets
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports jsobj = Microsoft.VisualBasic.MIME.application.json.Javascript.JsonObject

''' <summary>
''' the reusable json-rpc 2.0 session skeleton. it owns the per connection message
''' pump (read / dispatch / error handling) and leaves the actual protocol handling
''' to a subclass through <see cref="HandleRequestAsync"/> and
''' <see cref="HandleNotificationAsync"/>. hand it a <see cref="TcpClient"/> and
''' override the two handlers to build an LSP server or any other json-rpc service.
''' </summary>
''' <remarks>
''' a session runs on a single task, so subclass state is never accessed concurrently
''' and needs no locking. override <see cref="OnSessionError"/> / <see cref="OnSessionClosed"/>
''' to customise logging. this type depends only on the <see cref="JsonRpc"/> transport
''' module, so it can be lifted into a shared library unchanged.
''' </remarks>
Public MustInherit Class JsonRpcSession

    ''' <summary>the accepted tcp client for this session. exposed so subclasses can close the connection (e.g. on an "exit" notification).</summary>
    Protected ReadOnly client As TcpClient

    Protected Sub New(client As TcpClient)
        Me.client = client
    End Sub

    ''' <summary>the per connection message pump. runs until the client disconnects or an unrecoverable error occurs.</summary>
    Public Async Function RunAsync() As Task
        Try
            Using stream As NetworkStream = client.GetStream()
                While client.Connected
                    Dim message As jsobj = Await JsonRpc.ReadMessageAsync(stream)

                    If message Is Nothing Then
                        Exit While
                    End If

                    Await DispatchAsync(stream, message)
                End While
            End Using
        Catch ex As Exception
            ' a broken connection must not crash the whole server
            OnSessionError(ex)
        Finally
            client.Dispose()
            OnSessionClosed()
        End Try
    End Function

    ''' <summary>
    ''' decode one framed message and route it to the right handler. the request id
    ''' is deep cloned because it still belongs to the parsed request tree and can not
    ''' be re-parented into the response object.
    ''' </summary>
    Private Async Function DispatchAsync(stream As NetworkStream, message As jsobj) As Task
        Dim method As String = JsonRpc.Str(message, "method")
        Dim id As JsonElement = message("id")
        ' the id node belongs to the parsed request tree, so it already has a parent
        ' and can not be re-parented into the response; clone it first.
        Dim idClone As JsonElement = If(id Is Nothing, Nothing, id.DeepClone())
        Dim params As JsonElement = message("params")

        If idClone IsNot Nothing Then
            Await HandleRequestAsync(stream, method, idClone, params)
        Else
            Await HandleNotificationAsync(stream, method, params)
        End If
    End Function

    ''' <summary>handle a json-rpc request (carries an id and expects a response).</summary>
    Protected MustOverride Function HandleRequestAsync(stream As NetworkStream, method As String, id As JsonElement, params As JsonElement) As Task

    ''' <summary>handle a json-rpc notification (no id, fire and forget).</summary>
    Protected MustOverride Function HandleNotificationAsync(stream As NetworkStream, method As String, params As JsonElement) As Task

    ''' <summary>send a successful json-rpc response carrying <paramref name="result"/>.</summary>
    Protected Async Function ReplyAsync(stream As NetworkStream, id As JsonElement, result As JsonElement) As Task
        Await JsonRpc.SendMessageAsync(stream, JsonRpc.MakeResponse(id, result))
    End Function

    ''' <summary>send a json-rpc error response (default method-not-found / -32601 from the subclass).</summary>
    Protected Async Function ReplyErrorAsync(stream As NetworkStream, id As JsonElement, code As Integer, message As String) As Task
        Await JsonRpc.SendMessageAsync(stream, JsonRpc.MakeError(id, code, message))
    End Function

    ''' <summary>called when the pump catches an exception; default logs the message to the console.</summary>
    Protected Overridable Sub OnSessionError(ex As Exception)
        Console.WriteLine($"client error: {ex.Message}")
    End Sub

    ''' <summary>called after the connection is closed and the client is disposed; default logs to the console.</summary>
    Protected Overridable Sub OnSessionClosed()
        Console.WriteLine("client disconnected")
    End Sub
End Class
