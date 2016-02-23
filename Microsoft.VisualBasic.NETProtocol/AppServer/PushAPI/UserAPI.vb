Imports System.Net
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Namespace PushAPI

    ''' <summary>
    ''' 对User client开放的协议接口
    ''' </summary>
    <Protocol(GetType(Protocols.UserAPI.Protocols))>
    Public Class UserAPI : Inherits APIBase

        Public ReadOnly Property UserHash As New Dictionary(Of String, Long)

        Sub New(push As PushServer)
            Call MyBase.New(push)
            __protocols = New ProtocolHandler(Me)
        End Sub

        Public Overrides Function Handler(CA As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
            Return __protocols.HandleRequest(CA, request, remote)
        End Function

        ''' <summary>
        ''' 第一步，初始化哈希表
        ''' </summary>
        ''' <param name="CA"></param>
        ''' <param name="request">user_id</param>
        ''' <param name="remote"></param>
        ''' <returns></returns>
        ''' 
        <Protocol(Protocols.UserAPI.Protocols.InitUser)>
        Private Function __userInitPOST(CA As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
            Dim sId As String = request.GetUTF8String
            Dim uid As Long = Protocols.UserAPI.Uid(sId)

            If UserHash.ContainsKey(sId) Then
            Else
                Call UserHash.Add(sId, uid)
            End If

            Dim post As New Protocols.InitPOSTBack With {
                .uid = uid,
                .Portal = New IPEndPoint("", Me.PushServer.UserSocket.LocalPort) ' 在客户端已处理
            }
            Return RequestStream.CreatePackage(post)
        End Function
    End Class
End Namespace