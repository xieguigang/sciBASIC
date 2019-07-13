#Region "Microsoft.VisualBasic::d45d1aa32d36c48db2aa60266a5ca3c8, www\Microsoft.VisualBasic.NETProtocol\NETProtocol\AppServer\PushAPI\InvokeAPI.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class InvokeAPI
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: __invokePush, Handler
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Net
Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Namespace NETProtocol.PushAPI

    ''' <summary>
    ''' 服务器端的其他模块调用消息更新推送的接口
    ''' </summary>
    <Protocol(GetType(Protocols.InvokeAPI.Protocols))>
    Public Class InvokeAPI : Inherits APIBase

        Sub New(push As PushServer)
            Call MyBase.New(push)
            __protocols = New ProtocolHandler(Me)
        End Sub

        Public Overrides Function Handler(request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
            Return __protocols.HandleRequest(request, remote)
        End Function

        <Protocol(Protocols.InvokeAPI.Protocols.PushToUser)>
        Private Function __invokePush(CA As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
            Call PushServer.PushUpdate(request)
            Return NetResponse.RFC_OK
        End Function
    End Class
End Namespace
