Imports Microsoft.VisualBasic.Net.NETProtocol.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection

<Protocol(GetType(UserProtocols.Protocols))>
Public Class User

    ReadOnly __updateThread As Persistent.Application.USER

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="remote">User API的接口</param>
    Sub New(remote As IPEndPoint, uid As String)
        __updateThread = __register(UserAPI.InitUser(remote, uid), Me)
    End Sub

    ''' <summary>
    ''' 在消息推送服务器上面注册自己的句柄
    ''' </summary>
    ''' <returns></returns>
    Private Shared Function __register(args As InitPOSTBack, endpoint As User) As Persistent.Application.USER
        Dim protocols As New ProtocolHandler(endpoint)
        Dim user As New Persistent.Application.USER(args.Portal, args.uid, AddressOf protocols.HandlePush)
    End Function
End Class
