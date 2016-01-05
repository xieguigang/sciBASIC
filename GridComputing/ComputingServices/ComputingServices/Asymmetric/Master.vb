Imports Microsoft.VisualBasic.Net.Protocol
Imports Microsoft.VisualBasic.Net.Protocol.Reflection
Imports NodeAbstract = System.Collections.Generic.KeyValuePair(Of String, Microsoft.VisualBasic.Net.SSL.Certificate)

Namespace Asymmetric

    ''' <summary>
    ''' 非对等网络里面的中心主节点
    ''' </summary>
    ''' 
    <Protocol(GetType(Protocols.Protocols))>
    Public Class Master

        Dim _socket As Net.SSL.SSLSynchronizationServicesSocket
        ''' <summary>
        ''' 键名是IP地址，由于一台物理主机上面只会有一个管理节点，所以端口号都是固定了的
        ''' </summary>
        Dim _nodes As SortedDictionary(Of String, Net.SSL.Certificate) =
            New SortedDictionary(Of String, Net.SSL.Certificate)
        Dim _protocol As ProtocolHandler

        ''' <summary>
        ''' 获取在当前的服务器上面注册了的所有的管理节点的位置
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Nodes As String()
            Get
                Return _nodes.Keys.ToArray
            End Get
        End Property

        Public ReadOnly Property IPAddress As String = Microsoft.VisualBasic.GetMyIPAddress

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="PublicToken">计算自于宿主节点的证书哈希值</param>
        Sub New(PublicToken As String)
            Dim [public] = Net.SSL.Certificate.Install(PublicToken, uid:=0)
            _socket = New Net.SSL.SSLSynchronizationServicesSocket(Protocols.MasterSvr, [public], container:=Me)
            _protocol = New ProtocolHandler(Me)
            _socket.Responsehandler = AddressOf _protocol.HandleRequest
        End Sub

        Public Sub Run()
            Call _socket.Run()
        End Sub

        Public Overrides Function ToString() As String
            Return _socket.ToString
        End Function

        ''' <summary>
        ''' 创建出一个新的计算节点里面的服务实例
        ''' </summary>
        ''' <param name="CLI"></param>
        ''' <returns></returns>
        Public Function Folk(CLI As String) As Asymmetric.DDM.Instance
            Dim Node As String = __getPreferNode()

            If String.IsNullOrEmpty(Node) Then
                Return Nothing
            End If

            Dim request As RequestStream = Protocols.FolkInstance(CLI)
            Try
                Return __folk(Node, request)
            Catch ex As Exception
                If WindowsServices.Initialized Then
                    Call ServicesLogs.LogException(ex)
                Else
                    Call App.LogException(ex)
                End If

                Return Nothing
            End Try
        End Function

        Private Function __folk(node As String, request As RequestStream) As Asymmetric.DDM.Instance
            Dim socket As New Net.AsynInvoke(node, Protocols.ParasitiferSvr)
            Dim CA As Net.SSL.Certificate = _nodes(node)
            Dim response As RequestStream = socket.SendMessage(request, CA)
            Dim instance = response.GetRawStream(Of DDM.Instance)
            Return instance
        End Function

        ''' <summary>
        ''' 获取当前网络之中的所有的已经运行的计算服务实例
        ''' </summary>
        ''' <returns></returns>
        Public Function GetInstances() As Microsoft.VisualBasic.Net.IPEndPoint()
            Dim request As RequestStream = Protocols.GetInstanceList
            Dim LQuery As Microsoft.VisualBasic.Net.IPEndPoint()() = (
                From node As NodeAbstract In _nodes.AsParallel
                Let result As RequestStream =
                    New Net.AsynInvoke(node.Key, Protocols.ParasitiferSvr) _
                        .SendMessage(request, node.Value) ' 请注意这里需要使用节点的证书来加密请求，否则节点会直接拒绝请求
                Where result.Protocol = HTTP_RFC.RFC_OK
                Select result.LoadObject(Of Microsoft.VisualBasic.Net.IPEndPoint())(AddressOf CreateObjectFromXml)).ToArray
            Dim list As Microsoft.VisualBasic.Net.IPEndPoint() = LQuery.MatrixToVector
            Return list
        End Function

        ''' <summary>
        ''' 获取得到最优先的物理机来开启新的计算节点，优先级别是和分布式计算网络之中的物理主机节点的CPU，内存负载量成反比的(本机的节点优先级别最低)
        ''' </summary>
        ''' <returns></returns>
        Private Function __getPreferNode() As String
            If _nodes.Count = 0 Then
                Return ""
            End If

            Dim LQuery = (From node As NodeAbstract In _nodes
                          Let load As Double = __getLoad(node.Key, node.Value)
                          Where load >= 0
                          Select load, node.Key
                          Order By load Ascending).ToArray

            If LQuery.IsNullOrEmpty Then
                Return ""
            End If

            Dim prefer = (From node In LQuery
                          Where Not String.Equals(IPAddress, node.Key)
                          Select node).FirstOrDefault

            If prefer Is Nothing Then  ' LQuery查询结果不为空，但是不是本机IP的节点却没有，说明都是本机节点，直接返回第一个
                Return LQuery.First.Key '  本机节点的优先级最低，这个是处于管理中心节点的性能的影响来考虑的
            Else
                Return prefer.Key  ' 优先利用其他节点的空闲的计算资源
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="CA"></param>
        ''' <param name="request"></param>
        ''' <param name="remote">可能得到的是内网IP，所以不太准确，不是用这个参数来标记节点的位置</param>
        ''' <returns></returns>
        <Protocol(Protocols.Protocols.NodeRegister)>
        Private Function NodeRegister(CA As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
            Dim post As Protocols.RegisterPost = Protocols.GetPostData(request)
            Dim nodeCA = _socket.PrivateKeys(post.uid)
            Dim IPAddr As String = post.IPAddress

            If Me._nodes.ContainsKey(IPAddr) Then
                Call Me._nodes.Remove(IPAddr)
                Call $"There is a duplicated machine entry ""{IPAddr}"" in the manager's hash table, this probably is a node services restart event...".__DEBUG_ECHO
            End If
            Call Me._nodes.Add(post.IPAddress, nodeCA)
#If DEBUG Then
            Call $"{post.IPAddress}  ===> {nodeCA.ToString}".__DEBUG_ECHO
#End If
            Return NetResponse.RFC_OK
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Node">IP地址</param>
        ''' <returns></returns>
        Private Function __getLoad(Node As String, CA As Net.SSL.Certificate) As Double
            Dim request As RequestStream = Protocols.GetLoad
            Dim socket = New Net.AsynInvoke(Node, Protocols.ParasitiferSvr)
            Dim response As RequestStream = socket.SafelySendMessage(request, CA, 5 * 1000)

            If Not response.Protocol = HTTP_RFC.RFC_OK Then
                Return -100
            Else
                Return Val(response.GetUTF8String)
            End If
        End Function
    End Class
End Namespace