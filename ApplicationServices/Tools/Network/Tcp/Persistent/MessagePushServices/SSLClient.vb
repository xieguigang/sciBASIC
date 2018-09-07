#Region "Microsoft.VisualBasic::136ae1fd52e866f4673e143fade2a4ea, Microsoft.VisualBasic.Core\ApplicationServices\Tools\Network\Tcp\Persistent\MessagePushServices\SSLClient.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Class SSLClient
    ' 
    '         Properties: PrivateKey, PushUser
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: __sslRedirect, SendMessage
    ' 
    '         Sub: Handshaking, Logon, SetDisconnectHandle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports Microsoft.VisualBasic.Net.Abstract
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Namespace Net.Persistent.Application

    Public Class SSLClient

        Public ReadOnly Property PrivateKey As Net.SSL.Certificate
        Public ReadOnly Property PushUser As Net.Persistent.Application.USER

        ReadOnly _DataRequestHandle As PushMessage

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="services"></param>
        ''' <param name="ID"></param>
        ''' <param name="DataRequestHandle">Public Delegate Function PushMessage(USER_ID As <see cref="Long"/>, Message As <see cref="RequestStream"/>) As <see cref="RequestStream"/></param>
        ''' <param name="ExceptionHandler"></param>
        Sub New(services As System.Net.IPEndPoint, ID As Long, DataRequestHandle As PushMessage, Optional ExceptionHandler As Abstract.ExceptionHandler = Nothing)
            _DataRequestHandle = DataRequestHandle
            PushUser = New USER(services, ID, AddressOf __sslRedirect, ExceptionHandler)
        End Sub

        Private Function __sslRedirect(USER_ID As Long, request As RequestStream) As RequestStream
            request = PrivateKey.Decrypt(request)  ' 解密之后在讲数据传递到实际的业务逻辑之上
            request = _DataRequestHandle(USER_ID, request)
            Return request
        End Function

        Public Sub Handshaking(PublicToken As Net.SSL.Certificate)
            Dim Services = New System.Net.IPEndPoint(System.Net.IPAddress.Parse(PushUser.remoteHost), PushUser.remotePort)
            _PrivateKey = Net.SSL.Certificate.CopyFrom(PublicToken, PushUser.USER_ID)
            _PrivateKey = Net.SSL.SSLProtocols.Handshaking(PrivateKey, Services)
            Call PushUser.BeginConnect(PrivateKey, _disconnectHandler)
        End Sub

        ''' <summary>
        ''' 使用已经拥有的用户证书登录服务器，这一步省略了握手步骤
        ''' </summary>
        ''' <param name="UserToken"></param>
        Public Sub Logon(UserToken As Net.SSL.Certificate)
            _PrivateKey = UserToken
            _PushUser.BeginConnect(UserToken, _disconnectHandler)
        End Sub

        Dim _disconnectHandler As MethodInvoker

        Public Sub SetDisconnectHandle([handle] As MethodInvoker)
            _disconnectHandler = handle
            _PushUser.SetDisconnectHandle(handle)
        End Sub

        ''' <summary>
        ''' 消息在这个函数之中自动被加密处理
        ''' </summary>
        ''' <param name="USER_ID"></param>
        ''' <param name="request"></param>
        ''' <returns></returns>
        Public Function SendMessage(USER_ID As Long, request As RequestStream) As Boolean
            Return PushUser.SendMessage(USER_ID, request, PrivateKey)
        End Function
    End Class
End Namespace
