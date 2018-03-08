#Region "Microsoft.VisualBasic::df7cdde33d591d9dcd3e818f8102b1e4, Microsoft.VisualBasic.Core\ApplicationServices\Tools\Network\Tcp\Persistent\Socket\ServicesSocket.vb"

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

    '     Class ServicesSocket
    ' 
    '         Properties: Connections, IsShutdown, LocalPort, Running
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Run
    ' 
    '         Sub: __acceptSocket, __initSocket, __initSocketThread, __runHost, AcceptCallback
    '              Run, WaitForRunning
    '         Delegate Sub
    ' 
    '             Properties: AcceptCallbackHandleInvoke
    ' 
    '             Sub: __socketCleanup, (+2 Overloads) Dispose, ForceCloseHandle
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic.Net.Abstract
Imports Microsoft.VisualBasic.Net.Persistent.Application.Protocols

Namespace Net.Persistent.Socket

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' 一、TCP长连接
    ''' 
    ''' 正常情况下，一条TCP连接建立后，只要双不提出关闭请求并且不出现异常情况，这条连接是一直存在的，
    ''' 操作系统不会自动去关闭它，甚至经过物理网络拓扑的改变之后仍然可以使用。
    ''' 所以一条连接保持几天、几个月、几年或者更长时间都有可能，只要不出现异常情况或由用户（应用层）主动关闭。
    ''' 在编程中， 往往需要建立一条TCP连接， 并且长时间处于连接状态。
    ''' 所谓的TCP长连接并没有确切的时间限制， 而是说这条连接需要的时间比较长。
    ''' 
    ''' 二、TCP连接的正常中断
    ''' 
    ''' TCP连接在事务处理完毕之后， 由一方提出关闭连接请求， 双方通过四次握手（建立连接是三次握手， 
    ''' 当然可以通过优化TCP / IP协议栈来减少握手的次数来提高性能， 但这样会形成不规范或者不优雅的通信）来正常关闭连接
    ''' 
    ''' 三、TCP连接的异常中断
    ''' 
    ''' 导致TCP连接异常中断的因素有： 物理连接被中断、操作系统down机、程序崩溃等等。
    ''' </remarks>
    Public Class ServicesSocket : Implements System.IDisposable

#Region "INTERNAL FIELDS"

        Dim _ThreadEndAccept As Boolean = True

        ''' <summary>
        ''' Socket对象监听的端口号
        ''' </summary>
        ''' <remarks></remarks>
        Protected _LocalPort As Integer
        Protected __exceptionHandle As Abstract.ExceptionHandler
        Dim _socketListener As System.Net.Sockets.Socket

#End Region

        ''' <summary>
        ''' The server services listening on this local port.(当前的这个服务器对象实例所监听的本地端口号)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property LocalPort As Integer
            Get
                Return _LocalPort
            End Get
        End Property

        Public ReadOnly Property IsShutdown As Boolean
            Get
                Return disposedValue
            End Get
        End Property

        ''' <summary>
        ''' 消息处理的方法接口： Public Delegate Function DataResponseHandler(str As String, RemotePort As Integer) As String
        ''' </summary>
        ''' <param name="LocalPort">监听的本地端口号，假若需要进行端口映射的话，则可以在<see cref="Run"></see>方法之中设置映射的端口号</param>
        ''' <remarks></remarks>
        Sub New(Optional LocalPort As Integer = 11000, Optional exHandler As Abstract.ExceptionHandler = Nothing)
            Me._LocalPort = LocalPort
            Me.__exceptionHandle = If(exHandler Is Nothing, AddressOf PrintException, exHandler)
        End Sub

        Sub New(Optional exHandler As Abstract.ExceptionHandler = Nothing)
            Me.__exceptionHandle = If(exHandler Is Nothing, AddressOf PrintException, exHandler)
        End Sub

        ''' <summary>
        ''' This server waits for a connection and then uses  asychronous operations to
        ''' accept the connection, get data from the connected client,
        ''' echo that data back to the connected client.
        ''' It then disconnects from the client and waits for another client.(请注意，当服务器的代码运行到这里之后，代码将被阻塞在这里)
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable Function Run() As Integer

            ' Establish the local endpoint for the socket.
            Dim localEndPoint As System.Net.IPEndPoint = New System.Net.IPEndPoint(System.Net.IPAddress.Any, _LocalPort)
            Call Run(localEndPoint)
            Return 0
        End Function 'Main

        ''' <summary>
        ''' This server waits for a connection and then uses  asychronous operations to
        ''' accept the connection, get data from the connected client,
        ''' echo that data back to the connected client.
        ''' It then disconnects from the client and waits for another client.(请注意，当服务器的代码运行到这里之后，代码将被阻塞在这里)
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable Sub Run(localEndPoint As System.Net.IPEndPoint)
            _LocalPort = localEndPoint.Port

            Try
                Call __initSocket(localEndPoint)
            Catch ex As Exception
                ex = New Exception("Exception on try initialize the socket connection local_EndPoint=" & localEndPoint.ToString, ex)
                Call Me.__exceptionHandle(ex)
                Throw ex
            End Try

            Call __runHost()
        End Sub

        Private Sub __runHost()
            _ThreadEndAccept = True
            _Running = True

            While Not Me.disposedValue
                Call Thread.Sleep(1)
                If _ThreadEndAccept Then Call __acceptSocket()
            End While
        End Sub

        Private Sub __acceptSocket()
            _ThreadEndAccept = False

            Dim Callback As AsyncCallback = New AsyncCallback(AddressOf AcceptCallback)
            Call _socketListener.BeginAccept(Callback, _socketListener)
        End Sub

        ''' <summary>
        ''' Bind the socket to the local endpoint and listen for incoming connections.
        ''' </summary>
        ''' <param name="localEndPoint"></param>
        Private Sub __initSocket(localEndPoint As System.Net.IPEndPoint)
            ' Create a TCP/IP socket.
            _socketListener = New Sockets.Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            )

            Call _socketListener.Bind(localEndPoint)
            Call _socketListener.ReceiveBufferSize.SetValue(4096)
            Call _socketListener.SendBufferSize.SetValue(4096)
            Call _socketListener.Listen(backlog:=100)
        End Sub

        Public ReadOnly Property Running As Boolean = False

        Public Sub WaitForRunning()
            Do While Not Running
                Call Thread.Sleep(10)
            Loop
        End Sub

        ''' <summary>
        ''' Get the socket that handles the client request.
        ''' </summary>
        ''' <param name="ar"></param>
        Public Sub AcceptCallback(ar As IAsyncResult)
            Dim listener As Sockets.Socket =
                DirectCast(ar.AsyncState, Sockets.Socket)

            ' End the operation.
            Dim handler As Sockets.Socket

            Try
                handler = listener.EndAccept(ar)
                Call __initSocketThread(handler)
            Catch ex As Exception
                _ThreadEndAccept = True
                Return
            Finally
                _ThreadEndAccept = True
            End Try
        End Sub 'AcceptCallback

        ''' <summary>
        ''' Create the state object for the async receive.
        ''' </summary>
        ''' <param name="handler"></param>
        Private Sub __initSocketThread(handler As Sockets.Socket)
            Dim state As StateObject = New StateObject With {
                .workSocket = handler
            }
            Dim socket As New WorkSocket(state) With {
                .ExceptionHandle = __exceptionHandle,
                .ForceCloseHandle = AddressOf Me.ForceCloseHandle
            }

            Try
                Call handler.BeginReceive(state.readBuffer, 0, StateObject.BufferSize, 0, New AsyncCallback(AddressOf socket.ReadCallback), state)
                Call AcceptCallbackHandleInvoke()(socket)
                Call _Connections.Add(socket.GetHashCode, socket)
                Call Thread.Sleep(500)
                Call socket.SendMessage(ServicesProtocol.SendServerHash(socket.GetHashCode))

            Catch ex As Exception   ' 远程强制关闭主机连接，则放弃这一条数据请求的线程
                Call ForceCloseHandle(socket)
            End Try
        End Sub

        Protected _Connections As New Dictionary(Of Integer, WorkSocket)

        Public ReadOnly Property Connections As WorkSocket()
            Get
                Try
                    Return Me._Connections.Values.ToArray
                Catch ex As Exception
                    Call ex.PrintException
                    Return New WorkSocket() {}
                End Try
            End Get
        End Property

        Public Delegate Sub AcceptCallbackHandle(socket As WorkSocket)

        Public Property AcceptCallbackHandleInvoke As AcceptCallbackHandle

        Protected Sub ForceCloseHandle(socket As WorkSocket)
            Dim hash As Integer = socket.GetHashCode

            On Error Resume Next
            Call $"Connection was force closed by {socket.workSocket.RemoteEndPoint.ToString}, services thread abort!".__DEBUG_ECHO
            Call Me._Connections.Remove(hash)
            Call socket.Dispose()
            Call __socketCleanup(hash)
        End Sub

        Protected Overridable Sub __socketCleanup(hash As Integer)

        End Sub

#Region "IDisposable Support"

        ''' <summary>
        ''' 退出监听线程所需要的
        ''' </summary>
        ''' <remarks></remarks>
        Private disposedValue As Boolean = False  ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then

                    Call _socketListener.Dispose()
                    Call _socketListener.Free()
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(      disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(      disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.

        ''' <summary>
        ''' Stop the server socket listening threads.(终止服务器Socket监听线程)
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
