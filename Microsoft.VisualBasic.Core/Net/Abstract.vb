#Region "Microsoft.VisualBasic::a491bd98f891310b14cf4957ddae8936, Microsoft.VisualBasic.Core\Net\Abstract.vb"

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

    '     Class IProtocolHandler
    ' 
    ' 
    ' 
    '     Delegate Sub
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Interface IServicesSocket
    ' 
    '         Properties: IsRunning, IsShutdown, LocalPort
    ' 
    '         Function: (+2 Overloads) Run
    ' 
    '     Interface IDataRequestHandler
    ' 
    '         Properties: Responsehandler
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Net.Tcp

Namespace Net.Abstract

    ''' <summary>
    ''' Object for handles the request <see cref="Protocol"/>.
    ''' </summary>
    Public MustInherit Class IProtocolHandler

        MustOverride ReadOnly Property ProtocolEntry As Long
        MustOverride Function HandleRequest(request As RequestStream, remoteDevcie As System.Net.IPEndPoint) As RequestStream
    End Class

#Region "Delegate Abstract Interface"

    Public Delegate Sub ForceCloseHandle(socket As StateObject)

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="request"></param>
    ''' <param name="RemoteAddress"></param>
    ''' <returns></returns>
    Public Delegate Function DataRequestHandler(request As RequestStream, RemoteAddress As System.Net.IPEndPoint) As RequestStream
#End Region

    ''' <summary>
    ''' Socket listening object which is running at the server side asynchronous able multiple threading.
    ''' (运行于服务器端上面的Socket监听对象，多线程模型)
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface IServicesSocket : Inherits IDisposable, ITaskDriver, IDataRequestHandler

        ''' <summary>
        ''' The server services listening on this local port.(当前的这个服务器对象实例所监听的本地端口号)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property LocalPort As Integer
        ReadOnly Property IsShutdown As Boolean
        ReadOnly Property IsRunning As Boolean

        ''' <summary>
        ''' This server waits for a connection and then uses  asychronous operations to
        ''' accept the connection, get data from the connected client,
        ''' echo that data back to the connected client.
        ''' It then disconnects from the client and waits for another client.(请注意，当服务器的代码运行到这里之后，代码将被阻塞在这里)
        ''' </summary>
        ''' <remarks></remarks>
        Overloads Function Run() As Integer

        ''' <summary>
        ''' This server waits for a connection and then uses  asychronous operations to
        ''' accept the connection, get data from the connected client,
        ''' echo that data back to the connected client.
        ''' It then disconnects from the client and waits for another client.(请注意，当服务器的代码运行到这里之后，代码将被阻塞在这里)
        ''' </summary>
        ''' <remarks></remarks>
        Overloads Function Run(localEndPoint As System.Net.IPEndPoint) As Integer
    End Interface

    Public Interface IDataRequestHandler

        ''' <summary>
        ''' This function pointer using for the data request handling of the data request from the client socket.
        ''' (这个函数指针用于处理来自于客户端的请求)
        ''' </summary>
        ''' <remarks></remarks>
        Property Responsehandler As DataRequestHandler
    End Interface
End Namespace
