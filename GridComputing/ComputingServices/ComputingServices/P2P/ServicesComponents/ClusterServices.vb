Imports Microsoft.VisualBasic.Parallel
Imports System.Net
Imports Microsoft.VisualBasic.Net.Protocol

Namespace ServicesComponents

    ''' <summary>
    ''' 所需要进行节点调用的中心主机程序上面的节点计算服务
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ClusterServices : Inherits InternalServicesModule

        Implements System.IDisposable

        Public ReadOnly Property IsServicesRunning As Boolean
            Get
                Return Not _ServicesSocket Is Nothing
            End Get
        End Property

        ''' <summary>
        ''' 主机节点列表
        ''' </summary>
        Public ReadOnly Property NodeServices As New List(Of PbsThread)

        ''' <summary>
        ''' 每一个主机节点之上所运行的节点实例进程
        ''' </summary>
        Public ReadOnly Property Nodes As New List(Of PbsThread)

        ''' <summary>
        ''' 线程会被阻塞在这里
        ''' </summary>
        ''' <param name="argvs">cli /console &lt;TRUE/FALSE></param>
        Public Overrides Sub RunServices(argvs As CommandLine.CommandLine)
            Call Console.WriteLine()
            Call Console.WriteLine("Cluster Services Started!")

            If argvs Is Nothing OrElse String.Equals(argvs("/console"), "TRUE", StringComparison.OrdinalIgnoreCase) Then
                Call Run(AddressOf _runningShoalShell)
            End If

            Call _runningServicesProtocol(New PbsProtocol(Me))
        End Sub

        Protected Overrides Sub ImportsAPI()
            Call _ShoalShell.Imports(ClusterServicesAPI)
        End Sub

        Public ReadOnly Property ClusterServicesAPI As ClusterServicesAPI = New ClusterServicesAPI(Me)

        Protected Overrides Function GetServicesPort() As Integer
            Return PbsProtocol.PBS_CLUSTER_SERVICES_PORT
        End Function

        Public Function GetLightLoadNode() As PbsThread
            Dim LQuery = (From Node In Me.NodeServices.AsParallel Select Node, Load = Node.GetUpdatedServerLoad Order By Load Ascending).ToArray
            Return LQuery.FirstOrDefault?.Node
        End Function

        ''' <summary>
        ''' 查询是否有可用的计算节点
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HaveNodes As Boolean
            Get
                Return Not NodeServices.IsNullOrEmpty
            End Get
        End Property

        ''' <summary>
        ''' 创建一个新的计算节点
        ''' </summary>
        ''' <returns></returns>
        Public Function CreateNewNode(Cli As String) As Net.IPEndPoint
            Dim request As RequestStream = PbsProtocol.CreateNewNode(Cli)
            '向负载最轻的节点发送请求，然后等待连接
            Dim Node As Net.IPEndPoint = GetLightLoadNode.RemoteHostEp
            Call New Net.AsynInvoke(Node).SendMessage(request)
            Return Node
        End Function

        ''' <summary>
        ''' 由于需要构建的是一个去中心化的网络模型，所以这个主机节点模型实例对象会在启动初始化的时候扫描当前的局域网之中的所有节点主机
        ''' 假若没有节点主机在运行，则会将自己以节点主机的模型进行运行
        ''' </summary>
        ''' <param name="argvs"></param>
        ''' <returns></returns>
        Public Function Initialize(argvs As CommandLine.CommandLine) As String
            Dim SvrHostCollection As List(Of System.Net.IPAddress)
            Dim WAN_HostList As String = ""

            If String.IsNullOrEmpty(argvs("/wan").ShadowCopy(WAN_HostList)) Then
                SvrHostCollection = (From Device In Microsoft.VisualBasic.Net.LANTools.GetAllDevicesOnLAN.AsParallel
                                     Where Not Device.Value Is Nothing
                                     Select Device.Key).ToList
                Call SvrHostCollection.Add(System.Net.IPAddress.Parse(Net.AsynInvoke.LocalIPAddress))
            Else
                SvrHostCollection = (From addr As String In WAN_HostList.Split(CChar(",")).AsParallel Let IPAddress = System.Net.IPAddress.Parse(addr) Select IPAddress).ToList
            End If

            Try
                '测试在线主机之中是否有在线的节点主机服务
                Dim ServicesNode = (From Host In SvrHostCollection.AsParallel Where PbsProtocol.IsClusterServicesOnline(Host) Select Host).ToArray

                If ServicesNode.IsNullOrEmpty Then '返回的查询列表是空的，说明没有主机服务在当前的网络之中运行，则初始化自身为一个主机服务
                    Return ""
                Else
                    Return ServicesNode.First.ToString
                End If
            Catch ex As Exception
                Call ex.PrintException
            End Try

            Return ""
        End Function
    End Class
End Namespace