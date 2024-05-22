#Region "Microsoft.VisualBasic::8b964c20733e582688889c2231b6d251, www\Microsoft.VisualBasic.NETProtocol\NETProtocol\User\User.vb"

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

    '   Total Lines: 117
    '    Code Lines: 40 (34.19%)
    ' Comment Lines: 59 (50.43%)
    '    - Xml Docs: 27.12%
    ' 
    '   Blank Lines: 18 (15.38%)
    '     File Size: 4.61 KB


    '     Class User
    ' 
    '         Properties: Id, UserInvoke
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: __pushUpdate
    ' 
    '         Sub: __close, __downloadMsg, (+2 Overloads) Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Net.NETProtocol.Protocols
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Net.Tcp
Imports Microsoft.VisualBasic.Parallel

Namespace NETProtocol

    <ProtocolAttribute(GetType(UserProtocols.Protocols))>
    Public Class User : Implements IDisposable

        ' ReadOnly __updateThread As Persistent.Application.USER

        ''' <summary>
        ''' Public Event PushMessage(msg As <see cref="RequestStream"/>)
        ''' </summary>
        ''' <param name="msg"></param>
        Public Event PushMessage(msg As RequestStream)

        Public ReadOnly Property Id As String
        Public ReadOnly Property UserInvoke As IPEndPoint

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="remote">User API的接口</param>
        Sub New(remote As IPEndPoint, uid As String)
            ' __updateThread = __register(UserAPI.InitUser(remote, uid), Me)
            _Id = uid
            _UserInvoke = remote
        End Sub

        '''' <summary>
        '''' 在消息推送服务器上面注册自己的句柄
        '''' </summary>
        '''' <returns></returns>
        'Private Shared Function __register(args As InitPOSTBack, endpoint As User) As Persistent.Application.USER
        '    Dim protocols As New ProtocolHandler(endpoint)
        '    Dim user As New Persistent.Application.USER(args.Portal, args.uid, AddressOf protocols.HandlePush)
        '    Call RunTask(Sub() user.BeginConnect(AddressOf endpoint.__close))
        '    Call Threading.Thread.Sleep(100)

        '    Return user
        'End Function

        ''' <summary>
        ''' 得到服务器端发送过来的更新推送的消息头
        ''' </summary>
        ''' <param name="uid"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        <ProtocolAttribute(UserProtocols.Protocols.PushInit)>
        Private Function __pushUpdate(uid As Long, args As RequestStream) As RequestStream
            Call RunTask(AddressOf __downloadMsg)
            Return NetResponse.RFC_OK
        End Function

        ''' <summary>
        ''' 可能会存在多条数据
        ''' </summary>
        Private Sub __downloadMsg()
            'Dim category = UserAPI.ProtocolEntry
            'Dim protocol = UserAPI.Protocols.GetData
            'Dim req As RequestStream = RequestStream.CreateProtocol(Of UserId)(
            '    category, protocol, New UserId With {
            '        .sId = Id,
            '        .uid = __updateThread.USER_ID
            '    })
            'Dim invoke As New TcpRequest(UserInvoke)
            'Dim rep As RequestStream = invoke.SendMessage(req)

            'Do While Not rep.IsNull ' 读取服务器上面的数据缓存，直到没有数据为止
            '    RaiseEvent PushMessage(rep)
            '    rep = invoke.SendMessage(req)
            'Loop
        End Sub

        Private Sub __close()
            ' DO NOTHING
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    '    Call __updateThread.Free
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace
