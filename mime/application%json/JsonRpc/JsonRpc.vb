Imports System.IO
Imports System.Net.Sockets
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports jsobj = Microsoft.VisualBasic.MIME.application.json.Javascript.JsonObject
Imports jsVal = Microsoft.VisualBasic.MIME.application.json.Javascript.JsonValue

''' <summary>
''' the bare json-rpc 2.0 / LSP transport layer. it implements the
''' Content-Length framed tcp stream described in src/LSP/jsonrpc.md: the header
''' is read byte by byte until the empty line, the body is then read exactly by
''' its declared length, and outgoing messages are wrapped in the same framing.
''' </summary>
''' <remarks>
''' never wrap <see cref="NetworkStream"/> in a <see cref="IO.StreamReader"/>:
''' its internal buffer would pre-read bytes that belong to the json body and
''' break the length based framing.
''' </remarks>
Public Module JsonRpc

    ''' <summary>
    ''' read one framed json-rpc message from the stream. returns <c>Nothing</c>
    ''' when the connection is closed by the peer. the stream may be any
    ''' byte stream (network socket, process stdin/stdout pipe, file, ...).
    ''' </summary>
    Public Async Function ReadMessageAsync(stream As Stream) As Task(Of jsobj)
        Dim contentLength As Integer = Await ReadHeaderAsync(stream)

        If contentLength <= 0 Then
            Return Nothing
        End If

        Dim body As Byte() = Await ReadBodyAsync(stream, contentLength)

        If body Is Nothing Then
            Return Nothing
        End If

        Dim json As String = Encoding.UTF8.GetString(body)
        Dim node As JsonElement = JsonElement.ParseJSON(json)

        Return TryCast(node, jsobj)
    End Function

    ''' <summary>
    ''' read the header block and return the declared Content-Length, or 0 when
    ''' the stream ended before a complete header was received.
    ''' </summary>
    Private Async Function ReadHeaderAsync(stream As Stream) As Task(Of Integer)
        Dim header As New StringBuilder()
        Dim buffer(0) As Byte

        While True
            Dim read As Integer = Await stream.ReadAsync(buffer, 0, 1)

            If read = 0 Then
                Return 0
            End If

            header.Append(ChrW(buffer(0)))

            If header.ToString().EndsWith(vbCrLf & vbCrLf) Then
                Exit While
            End If
        End While

        Dim contentLength As Integer = 0

        For Each line As String In header.ToString().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
            If line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase) Then
                Dim value As String = line.Substring("Content-Length:".Length).Trim()
                Integer.TryParse(value, contentLength)
                Exit For
            End If
        Next

        Return contentLength
    End Function

    ''' <summary>
    ''' read exactly <paramref name="contentLength"/> bytes from the stream,
    ''' returning <c>Nothing</c> when the peer disconnects mid body.
    ''' </summary>
    Private Async Function ReadBodyAsync(stream As Stream, contentLength As Integer) As Task(Of Byte())
        Dim body(contentLength - 1) As Byte
        Dim total As Integer = 0

        While total < contentLength
            Dim read As Integer = Await stream.ReadAsync(body, total, contentLength - total)

            If read = 0 Then
                Return Nothing
            End If

            total += read
        End While

        Return body
    End Function

    ''' <summary>
    ''' send a json-rpc message (request, response or notification) over the
    ''' stream using the Content-Length framing.
    ''' </summary>
    Public Async Function SendMessageAsync(stream As Stream, message As JsonElement) As Task
        Dim json As String = message.BuildJsonString
        Dim body As Byte() = Encoding.UTF8.GetBytes(json)
        Dim header As String = $"Content-Length: {body.Length}" & vbCrLf & vbCrLf
        Dim headerBytes As Byte() = Encoding.UTF8.GetBytes(header)

        Await stream.WriteAsync(headerBytes, 0, headerBytes.Length)
        Await stream.WriteAsync(body, 0, body.Length)
        Await stream.FlushAsync()
    End Function

    ''' <summary>
    ''' build a json-rpc response carrying a result. when <paramref name="result"/>
    ''' is <c>Nothing</c> the wire result is the json null value.
    ''' </summary>
    Public Function MakeResponse(id As JsonElement, result As JsonElement) As jsobj
        Dim obj As New jsobj()
        obj("jsonrpc") = New jsVal("2.0")
        obj("id") = id
        obj("result") = result
        Return obj
    End Function

    ''' <summary>
    ''' build a json-rpc error response.
    ''' </summary>
    Public Function MakeError(id As JsonElement, code As Integer, message As String) As jsobj
        Dim obj As New jsobj()
        obj("jsonrpc") = New jsVal("2.0")
        obj("id") = id
        obj("error") = New jsobj From {
            {"code", New jsVal(code)},
            {"message", New jsVal(message)}
        }
        Return obj
    End Function

    ''' <summary>
    ''' build a json-rpc notification (no id).
    ''' </summary>
    Public Function MakeNotification(method As String, params As JsonElement) As jsobj
        Dim obj As New jsobj()
        obj("jsonrpc") = New jsVal("2.0")
        obj("method") = New jsVal(method)
        obj("params") = params
        Return obj
    End Function

    ''' <summary>
    ''' build a json-rpc request (client side): carries an id and params, so the
    ''' peer knows to reply. used by json-rpc clients such as the MCP client.
    ''' </summary>
    Public Function MakeRequest(id As JsonElement, method As String, params As JsonElement) As jsobj
        Dim obj As New jsobj()
        obj("jsonrpc") = New jsVal("2.0")
        obj("id") = id
        obj("method") = New jsVal(method)

        If params IsNot Nothing Then
            obj("params") = params
        End If

        Return obj
    End Function

    ''' <summary>
    ''' build a json-rpc request with an integer id (client side convenience).
    ''' </summary>
    Public Function MakeRequest(id As Integer, method As String, params As JsonElement) As jsobj
        Return MakeRequest(New jsVal(id), method, params)
    End Function

    ''' <summary>
    ''' read a string property from a json node, returning an empty string when
    ''' the node or the property is missing.
    ''' </summary>
    ''' 
    <Extension>
    Public Function Str(node As JsonElement, ParamArray path As String()) As String
        Dim current As JsonElement = node

        For Each segment As String In path
            If current Is Nothing Then
                Return ""
            End If

            Dim obj = TryCast(current, jsobj)

            If obj Is Nothing Then
                Return ""
            End If

            current = obj(segment)
        Next

        If current Is Nothing Then
            Return ""
        End If

        Return current.ToString()
    End Function

    ''' <summary>
    ''' read an integer property from a json node.
    ''' </summary>
    ''' 
    <Extension>
    Public Function Int(node As JsonElement, ParamArray path As String()) As Integer
        Dim text As String = node.Str(path)

        If String.IsNullOrEmpty(text) Then
            Return 0
        End If

        Dim value As Integer
        If Integer.TryParse(text, value) Then
            Return value
        End If

        Return 0
    End Function
End Module

''' <summary>
''' raised on the client side when a json-rpc response carries an <c>error</c>
''' object. exposes the numeric <see cref="Code"/> and optional <see cref="ErrorData"/>.
''' </summary>
Public Class JsonRpcException : Inherits Exception

    ''' <summary>the json-rpc error code (e.g. -32601 method not found).</summary>
    Public ReadOnly Property Code As Integer

    ''' <summary>the optional <c>data</c> payload of the error object, if any.</summary>
    Public ReadOnly Property ErrorData As JsonElement

    Public Sub New(code As Integer, message As String, Optional data As JsonElement = Nothing)
        MyBase.New($"[jsonrpc {code}] {message}")
        Me.Code = code
        Me.ErrorData = data
    End Sub
End Class
