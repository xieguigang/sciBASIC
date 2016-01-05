Imports System.ComponentModel
Imports Microsoft.VisualBasic.ComputingServices.ServicesComponents
Imports Microsoft.VisualBasic.Net.Protocol.Reflection.ProtocolHandler
Imports Microsoft.VisualBasic.Net
Imports Microsoft.VisualBasic.Net.Protocol

''' <summary>
''' 由于节点计算都是在局域网之中的，并且数据传输量大，所以在这里不需要在进行加密处理了，
''' 可能有一些数据会需求已加密的形式来保证数据能够被正确解析
''' </summary>
''' <remarks></remarks>
''' 
<Protocol.Reflection.Protocol(GetType(PbsProtocol))>
Public Class PbsProtocol

    ''' <summary>
    ''' 中心节点的服务端口
    ''' </summary>
    Public Const PBS_CLUSTER_SERVICES_PORT As Integer = 8345
    Public Const PBS_NODE_SERVICES_PORT As Integer = 1336

    Public Enum PROTOCOLS As Integer
        <Description("POST/001")> NEW_NODE = 1000
        <Description("POST/103")> NODE_REGISTER_TO_SERVICES
        <Description("GET/GRID-889")> INIT_QUERY_SERVICES_ONLINE
        ''' <summary>
        ''' 每一个主机节点之间进行数据同步
        ''' </summary>
        <Description("POP/SYN-670")> SYNCHRONIZE_DATA
        <Description("PUSH/404")> CURRENT_SERVICES_SHUTDOWN
    End Enum

    Sub New(NodeServices As NodeServices)
        NodesSvr = NodeServices
    End Sub

    Sub New(ClusterServices As ClusterServices)
        ClusterSvr = ClusterServices
    End Sub

#Region "NEW_NODE"

    ''' <summary>
    ''' 包含有两个校验数据
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function CreateNewNode(Cli As String) As RequestStream
        Dim code As Integer = CInt(RandomDouble() * 10000) + 1
        Return New RequestStream(
            PbsProtocol.ProtocolCategory,
            PROTOCOLS.NEW_NODE,
            $"{ DynamicsEncryption.EncryptData(code)} { DynamicsEncryption.EncryptData(3.5 * code)} {DynamicsEncryption.EncryptData(Cli)}")
    End Function

    <Protocol.Reflection.Protocol(PROTOCOLS.NEW_NODE)>
    Public Function IsCreateNewNodeRequest(request As RequestStream, remoteDevice As System.Net.IPEndPoint) As RequestStream
        Dim Tokens As String() = request.GetUTF8String.Split
        Dim a As Integer = CInt(Val(DynamicsEncryption.DecryptString(Tokens(0))))
        Dim b As Integer = CInt(Val(DynamicsEncryption.DecryptString(Tokens(1))) / 3.5)

        If Math.Abs(a - b) < 2 Then
            Dim Cli As String = Tokens(2)
            Cli = DynamicsEncryption.DecryptString(Cli)
            Call NodesSvr.NewNode(Cli)

            Return NetResponse.RFC_OK
        Else
            Return NetResponse.RFC_BAD_REQUEST
        End If
    End Function
#End Region

#Region "NODE_REGISTER_TO_SERVICES"

    Dim ClusterSvr As ClusterServices
    Dim NodesSvr As NodeServices

    ''' <summary>
    ''' 当任务进程开启的时候，会发送这个信号
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function NodeRegisterToServices(ep As Net.IPEndPoint, NodeType As PbsThread.ThreadTypes) As RequestStream
        Return New RequestStream(PbsProtocol.ProtocolCategory, PROTOCOLS.NODE_REGISTER_TO_SERVICES, PbsThread.InitNodeID(ep, NodeType).GetXml)
    End Function

    <Protocol.Reflection.Protocol(PROTOCOLS.NODE_REGISTER_TO_SERVICES)>
    Public Function GetNodeRegisterData(request As RequestStream, remoteDevice As System.Net.IPEndPoint) As RequestStream
        Dim InitNodeID = request.GetUTF8String.CreateObjectFromXml(Of PbsThread)()

        If InitNodeID.NodeType = PbsThread.ThreadTypes.NodeServices Then
            Call ClusterSvr.NodeServices.Add(InitNodeID)
            Call $"Registered a node services: {InitNodeID.ToString}".__DEBUG_ECHO
        Else
            Call ClusterSvr.Nodes.Add(InitNodeID)
            Call $"Registered a node thread: {InitNodeID.ToString}".__DEBUG_ECHO
        End If

        '进行数据同步
        Dim ChunkBuffer = ClusterSvr.NodeServices.Join(ClusterSvr.Nodes)
        Dim data = PbsProtocol.SynchronizeData(ChunkBuffer)
        Dim SynchronizeThreads = (From NodeServices In ClusterSvr.NodeServices.AsParallel Select NodeServices.SendMessage(data)).ToArray

        Return NetResponse.RFC_OK
    End Function
#End Region

#Region "INIT_QUERY_SERVICES_ONLINE"

    Public Shared Function IsClusterServicesOnline(IPAddress As System.Net.IPAddress) As Boolean
        Dim Message = New RequestStream(PbsProtocol.ProtocolCategory, PROTOCOLS.INIT_QUERY_SERVICES_ONLINE, CStr(CInt(RandomDouble() * 10000)))
        Dim ServicesEp = New System.Net.IPEndPoint(IPAddress, PBS_CLUSTER_SERVICES_PORT)
        Message = New Net.AsynInvoke(ServicesEp, Sub(ex) Call App.LogException(ex, $"{NameOf(PbsProtocol)}::{NameOf(IsClusterServicesOnline)}")).SendMessage(Message)
        Return Message.Protocol = HTTP_RFC.RFC_OK
    End Function

    <Protocol.Reflection.Protocol(PROTOCOLS.INIT_QUERY_SERVICES_ONLINE)>
    Public Function IsOnlineQuery(request As RequestStream, remoteDevice As System.Net.IPEndPoint) As RequestStream
        If Not IsNumeric(request.GetUTF8String) Then
            Return NetResponse.RFC_BAD_REQUEST
        Else
            Return NetResponse.RFC_OK
        End If
    End Function
#End Region

#Region "SYNCHRONIZE_DATA"

    ''' <summary>
    ''' 每当有一个新的节点主机或者节点进程实例注册到中心节点之后，都会在节点网络之中进行数据同步
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Shared Function SynchronizeData(data As Generic.IEnumerable(Of PbsThread)) As RequestStream
        Dim str As String = data.ToArray.GetXml
        Return New RequestStream(PbsProtocol.ProtocolCategory, PROTOCOLS.SYNCHRONIZE_DATA, str)
    End Function

    <Protocol.Reflection.Protocol(PROTOCOLS.SYNCHRONIZE_DATA)>
    Public Function IsClusterServicesSynchronizeData(request As RequestStream, remoteDevice As System.Net.IPEndPoint) As RequestStream
        Dim pbsData = request.GetUTF8String.CreateObjectFromXml(Of PbsThread())
        NodesSvr.SynchronizeData = pbsData
    End Function
#End Region

#Region "CURRENT_SERVICES_SHUTDOWN"

    Public Shared Function CurrentServicesShutdown() As RequestStream
        Return New RequestStream(PbsProtocol.ProtocolCategory, PROTOCOLS.CURRENT_SERVICES_SHUTDOWN, CStr(CInt(10000 * RandomDouble())))
    End Function

    <Protocol.Reflection.Protocol(PROTOCOLS.CURRENT_SERVICES_SHUTDOWN)>
    Public Function IsCurrentServicesShutdown(request As RequestStream, remoteDevice As System.Net.IPEndPoint) As RequestStream
        If Not IsNumeric(request.GetUTF8String) Then
            Return NetResponse.RFC_BAD_REQUEST
        End If
    End Function
#End Region

    Public Shared ReadOnly Property ProtocolCategory As Integer =
        New Protocol.Reflection.Protocol(GetType(PbsProtocol)).EntryPoint

End Class
