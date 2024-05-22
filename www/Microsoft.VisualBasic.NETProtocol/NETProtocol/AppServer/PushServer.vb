#Region "Microsoft.VisualBasic::fd84f557dba7919f02e713f28b404399, www\Microsoft.VisualBasic.NETProtocol\NETProtocol\AppServer\PushServer.vb"

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


    ' Code Statistics:

    '   Total Lines: 176
    '    Code Lines: 0 (0.00%)
    ' Comment Lines: 152 (86.36%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 24 (13.64%)
    '     File Size: 6.65 KB


    ' 
    ' /********************************************************************************/

#End Region

'#Region "Microsoft.VisualBasic::dee57dd6e1a7b66d81ff85d257a787c9, www\Microsoft.VisualBasic.NETProtocol\NETProtocol\AppServer\PushServer.vb"

'    ' Author:
'    ' 
'    '       asuka (amethyst.asuka@gcmodeller.org)
'    '       xie (genetics@smrucc.org)
'    '       xieguigang (xie.guigang@live.com)
'    ' 
'    ' Copyright (c) 2018 GPL3 Licensed
'    ' 
'    ' 
'    ' GNU GENERAL PUBLIC LICENSE (GPL3)
'    ' 
'    ' 
'    ' This program is free software: you can redistribute it and/or modify
'    ' it under the terms of the GNU General Public License as published by
'    ' the Free Software Foundation, either version 3 of the License, or
'    ' (at your option) any later version.
'    ' 
'    ' This program is distributed in the hope that it will be useful,
'    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
'    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    ' GNU General Public License for more details.
'    ' 
'    ' You should have received a copy of the GNU General Public License
'    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



'    ' /********************************************************************************/

'    ' Summaries:

'    '     Class PushServer
'    ' 
'    '         Constructor: (+1 Overloads) Sub New
'    ' 
'    '         Function: GetMsg, SendMessage
'    ' 
'    '         Sub: (+2 Overloads) Dispose, PushUpdate, Run
'    ' 
'    ' 
'    ' /********************************************************************************/

'#End Region

'Imports Microsoft.VisualBasic.Net.Protocols
'Imports Microsoft.VisualBasic.Net.Tcp
'Imports Microsoft.VisualBasic.Parallel

'Namespace NETProtocol

'    ''' <summary>
'    ''' 保持长连接，向用户客户端发送更新消息的服务器
'    ''' </summary>
'    Public Class PushServer : Implements IDisposable

'        ' ''' <summary>
'        ' ''' Push update notification to user client
'        ' ''' </summary>
'        ' ''' <returns></returns>
'        '   Public ReadOnly Property UserSocket As Tcp.Persistent.Application.MessagePushServer

'        ''' <summary>
'        ''' 其他的服务器模块对消息推送模块进行操作更新的通道
'        ''' </summary>
'        ReadOnly __invokeAPI As TcpServicesSocket
'        ''' <summary>
'        ''' 客户端进行数据读取的通道
'        ''' </summary>
'        ReadOnly __userAPI As TcpServicesSocket
'        ''' <summary>
'        ''' 用户数据缓存池
'        ''' </summary>
'        ReadOnly __msgs As PushAPI.UserMsgPool = New PushAPI.UserMsgPool

'        ''' <summary>
'        ''' 
'        ''' </summary>
'        ''' <param name="services">长连接socket的端口</param>
'        ''' <param name="invoke">服务器模块工作端口</param>
'        ''' <param name="userAPI">用户端口</param>
'        Sub New(services As Integer, invoke As Integer, userAPI As Integer)
'            '    UserSocket = New Persistent.Application.MessagePushServer(services)
'            '    __invokeAPI = New TcpServicesSocket(invoke) With {
'            '    .Responsehandler = AddressOf New PushAPI.InvokeAPI(Me).Handler
'            '}
'            __userAPI = New TcpServicesSocket(userAPI) With {
'            .Responsehandler = AddressOf New PushAPI.UserAPI(Me).Handler
'        }
'        End Sub

'        ''' <summary>
'        ''' 线程会在这里被阻塞
'        ''' </summary>
'        Sub Run()
'            '   Call RunTask(AddressOf UserSocket.Run)
'            Call RunTask(AddressOf __userAPI.Run)
'            Call __invokeAPI.Run() ' 需要使用这一个代码来保持线程的阻塞
'        End Sub

'        ''' <summary>
'        ''' 其他的服务器模块通过API发送数据包来推送服务器上，通过这个方法写入数据缓存，然后发送消息更新
'        ''' </summary>
'        ''' <param name="req"></param>
'        Public Sub PushUpdate(req As RequestStream)

'        End Sub

'        ''' <summary>
'        ''' 得到某一个用户的消息
'        ''' </summary>
'        ''' <param name="uid"></param>
'        ''' <returns></returns>
'        Public Function GetMsg(uid As Long) As RequestStream
'            Dim msg = __msgs.Pop(uid)
'            If msg Is Nothing Then
'                Return NullMsg()
'            Else
'                Return msg
'            End If
'        End Function

'        ''' <summary>
'        ''' 向用户socket发送消息
'        ''' </summary>
'        ''' <param name="uid"></param>
'        ''' <param name="msg"></param>
'        ''' <returns></returns>
'        Public Function SendMessage(uid As Long, msg As RequestStream) As Boolean
'            Try
'                '    Call UserSocket.SendMessage(-1L, uid, msg)
'            Catch ex As Exception
'                Call App.LogException(ex)
'                Return False
'            End Try

'            Return True
'        End Function

'#Region "IDisposable Support"
'        Private disposedValue As Boolean ' To detect redundant calls

'        ' IDisposable
'        Protected Overridable Sub Dispose(disposing As Boolean)
'            If Not Me.disposedValue Then
'                If disposing Then
'                    ' TODO: dispose managed state (managed objects).
'                    '   Call _UserSocket.Free
'                    Call __invokeAPI.Free
'                    Call __userAPI.Free
'                End If

'                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
'                ' TODO: set large fields to null.
'            End If
'            Me.disposedValue = True
'        End Sub

'        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
'        'Protected Overrides Sub Finalize()
'        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
'        '    Dispose(False)
'        '    MyBase.Finalize()
'        'End Sub

'        ' This code added by Visual Basic to correctly implement the disposable pattern.
'        Public Sub Dispose() Implements IDisposable.Dispose
'            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
'            Dispose(True)
'            ' TODO: uncomment the following line if Finalize() is overridden above.
'            ' GC.SuppressFinalize(Me)
'        End Sub
'#End Region
'    End Class
'End Namespace
