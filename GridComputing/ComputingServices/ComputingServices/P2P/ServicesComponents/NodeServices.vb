
Imports System.Net

Namespace ServicesComponents

    ''' <summary>
    ''' 运行于节点之中的随系统自动启动的节点服务
    ''' </summary>
    ''' <remarks></remarks>
    Public Class NodeServices : Inherits InternalServicesModule
        Implements System.IDisposable

        ''' <summary>
        ''' 由于是在同一个局域网之内，所以每一个节点实例都是使用一个服务Socket来获取来自于节点服务器的工作指令
        ''' 这个列表枚举着当前的节点服务至上所管理的本机上的节点实例
        ''' </summary>
        ''' <remarks></remarks>
        Dim _InstanceHash As Dictionary(Of Guid, PbsThread)
        Dim _ClusterEp As System.Net.IPEndPoint

        ''' <summary>
        ''' 当中心节点需要维护或者崩溃的时候，则当前的这个节点服务会使用这个同步数据来作为备用节点主机而工作
        ''' 由于中心节点不需要执行具体的计算，所以在初始化了<see cref="ClusterServices"/>之后，本节点服务会被销毁
        ''' </summary>
        Public Property SynchronizeData As PbsThread()

        ReadOnly _NodeEntry As String

        ''' <summary>
        ''' 中心节点的位置
        ''' </summary>
        ''' <param name="ClusterServices">中心节点服务器的网络位置，只需要提供IP地址就可以了</param>
        ''' <param name="NodeEntry">本地计算节点的可执行文件位置</param>
        ''' <param name="WANIP">公网IP地址，在初始化的时候，假若没有公网IP，则使用局域网本机IP</param>
        Sub New(ClusterServices As String, NodeEntry As String, WANIP As String)
            If String.Equals(ClusterServices, "localhost", StringComparison.OrdinalIgnoreCase) Then
                ClusterServices = Net.AsynInvoke.LocalIPAddress
            End If

            _ClusterEp = New System.Net.IPEndPoint(System.Net.IPAddress.Parse(ClusterServices), PbsProtocol.PBS_CLUSTER_SERVICES_PORT)

            Call New Threading.Thread(AddressOf Me._runningServicesProtocol).Start()
            Call WaitForSocketStart()
            Dim Message = PbsProtocol.NodeRegisterToServices(New Net.IPEndPoint(WANIP, _ServicesSocket.LocalPort), PbsThread.ThreadTypes.NodeServices)
            Message = New Net.AsynInvoke(_ClusterEp).SendMessage(Message)

            Me._NodeEntry = FileIO.FileSystem.GetFileInfo(NodeEntry).FullName
        End Sub

        Public Overrides Sub RunServices(argvs As CommandLine.CommandLine)
            Call _runningShoalShell()
        End Sub

        Protected Overrides Sub ImportsAPI()
            Call _ShoalShell.Imports(New NodeServicesAPI(Me))
        End Sub

        Protected Overrides Function GetServicesPort() As Integer
            If Net.TCPExtensions.Ping(ep:=_ClusterEp， operationTimeOut:=1000) = -1.0R Then
                Return PbsProtocol.PBS_NODE_SERVICES_PORT         '主机Ping不通，则进入等待模式
            Else
                Return Net.GetFirstAvailablePort
            End If
        End Function

        Public Function NewNode(Cli As String) As String
            Return Process.Start(Me._NodeEntry, Cli).Id
        End Function
    End Class
End Namespace