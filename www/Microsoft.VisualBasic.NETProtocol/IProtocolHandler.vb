Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Net.Tcp
Imports Microsoft.VisualBasic.Parallel

''' <summary>
''' Object for handles the request <see cref="Protocol"/>.
''' </summary>
Public MustInherit Class IProtocolHandler

    MustOverride ReadOnly Property ProtocolEntry As Long
    MustOverride Function HandleRequest(request As RequestStream, remoteDevcie As System.Net.IPEndPoint) As RequestStream
End Class

#Region "Delegate Abstract Interface"

Public Delegate Function SendMessageInvoke(Message As String) As String

Public Delegate Sub ForceCloseHandle(socket As StateObject)


#End Region

Public Delegate Sub ProcessMessagePush(message As RequestStream)

<HideModuleName>
Public Module Extensions

    ''' <summary>
    ''' -1标识Ping不通
    ''' </summary>
    ''' <param name="operationTimeOut">ms</param>
    ''' <returns></returns>
    Public Function Ping(ep As System.Net.IPEndPoint, Optional operationTimeOut As Integer = 3 * 1000) As Double
        Return New TcpRequest(ep).Ping(operationTimeOut)
    End Function

    ''' <summary>
    ''' -1 ping failure
    ''' </summary>
    ''' <param name="invoke"></param>
    ''' <param name="timeout"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Ping(invoke As TcpRequest, Optional timeout As Integer = 3 * 1000) As Double
        Dim sw As Stopwatch = Stopwatch.StartNew
        Dim request As RequestStream = RequestStream.SystemProtocol(RequestStream.Protocols.Ping, PING_REQUEST)
        Dim response As RequestStream = invoke.SendMessage(request, timeout:=timeout)

        If HTTP_RFC.RFC_REQUEST_TIMEOUT = response.Protocol Then
            Return -1
        End If

        Return sw.ElapsedMilliseconds
    End Function

    Public Const PING_REQUEST As String = "PING/TTL-78973"
End Module