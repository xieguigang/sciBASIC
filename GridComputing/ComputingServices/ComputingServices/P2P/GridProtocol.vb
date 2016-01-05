Public Module GridProtocol

    Public ReadOnly Property DynamicsEncryption As SecurityString.SHA256 = New SecurityString.SHA256("xie.guigang@gcmodeller.org", "aBCdeFgh")

    ''' <summary>
    ''' 计算节点程序，而非本节点程序，可以使用/wan_ip制定公网ip，假若节点网络不再局域网之中的话
    ''' </summary>
    ''' <param name="NodeEntry"></param>
    ''' <param name="cli">cli /wan &lt;wan_ip_list> /wan_ip &lt;wan_ip></param>
    ''' <returns></returns>
    Public Function CLI_InitStart(NodeEntry As String, Optional cli As CommandLine.CommandLine = Nothing) As Integer
        If cli Is Nothing Then
            cli = ""
        End If

        Using NodeClusterServices As New Microsoft.VisualBasic.ComputingServices.ServicesComponents.ClusterServices
            Dim IPAddress As String = NodeClusterServices.Initialize(cli)

            If String.IsNullOrEmpty(IPAddress) Then '自身是一个中心节点
                Call NodeClusterServices.RunServices(cli)
            Else

                '作为一个子节点模型运行
                Dim WanIP As String = cli("/wan_ip")

                If String.IsNullOrEmpty(WanIP) Then
                    WanIP = Net.AsynInvoke.LocalIPAddress
                End If

                Using NodeServices As New Microsoft.VisualBasic.ComputingServices.ServicesComponents.NodeServices(IPAddress, NodeEntry, WanIP)

                    Call NodeServices.RunServices(cli)

                End Using

            End If

            Return 0
        End Using
    End Function

    ''' <summary>
    ''' ，可以使用/wan_ip制定公网ip，假若节点网络不再局域网之中的话
    ''' </summary>
    ''' <param name="cli"></param>
    ''' <param name="API"></param>
    ''' <returns></returns>
    Public Function CLI_StartCluster(cli As CommandLine.CommandLine,
                                     ByRef API As ComputingServices.ServicesComponents.ClusterServices) As Integer

        Using NodeClusterServices As New Microsoft.VisualBasic.ComputingServices.ServicesComponents.ClusterServices
            Call NodeClusterServices.Initialize(If(cli Is Nothing, "", cli))
            Call API.InvokeSet(NodeClusterServices)

            Try
                Call NodeClusterServices.RunServices(cli) '自身是一个中心节点
            Catch ex As Exception
                Return -1
            End Try
        End Using

        Return 0
    End Function

    ''' <summary>
    ''' 只启动节点服务，而非自动初始化为中心节点，可以使用/wan_ip制定公网ip，假若节点网络不再局域网之中的话
    ''' </summary>
    ''' <returns></returns>
    Public Function CLI_InitStartNodeServices(IPAddress As String,
                                              NodeEntry As String,
                                              Optional Cli As CommandLine.CommandLine = Nothing) As Integer
        '作为一个子节点模型运行
        Dim WanIP As String = ""

        If Cli Is Nothing OrElse String.IsNullOrEmpty(Cli("/wan_ip").ShadowCopy(WanIP)) Then
            WanIP = Net.AsynInvoke.LocalIPAddress
        End If

        Using NodeServices As New Microsoft.VisualBasic.ComputingServices.ServicesComponents.NodeServices(
            IPAddress,
            NodeEntry,
            WanIP)
            Call NodeServices.RunServices(Cli)
        End Using

        Return 0
    End Function
End Module