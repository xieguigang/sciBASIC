Imports System.Reflection
Imports Microsoft.VisualBasic.Net.Protocol
Imports Microsoft.VisualBasic.Net.Protocol.Reflection
Imports Microsoft.VisualBasic.Net.SSL
Imports Microsoft.VisualBasic.Net.TCPExtensions

Namespace Asymmetric

    ''' <summary>
    ''' 主节点下面的每一台物理机上面的宿主服务，提供给该物理机上面的服务实例
    ''' </summary>
    ''' 
    <Protocol(GetType(Protocols.Protocols))>
    Public Class Parasitifer : Implements System.IDisposable

        ''' <summary>
        ''' 获取当前物理主机上面的系统负载
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function SystemLoad() As Double
            Return WindowsServices.CPU_Usages.NextValue
        End Function

        ''' <summary>
        ''' 服务实例的文件名
        ''' </summary>
        ReadOnly _instance As String
        ReadOnly _master As System.Net.IPEndPoint
        ''' <summary>
        ''' 在当前的物理主机上面所运行的服务实例列表
        ''' </summary>
        ReadOnly _instanceList As SortedDictionary(Of String, DDM.Instance) =
            New SortedDictionary(Of String, DDM.Instance)
        ReadOnly _protocolHandler As Net.Protocol.Reflection.ProtocolHandler
        ReadOnly _socket As Net.SSL.SSLSynchronizationServicesSocket
        ''' <summary>
        ''' 这个节点在主节点上面的授权认证信息
        ''' </summary>
        ReadOnly _OAuth As Net.SSL.Certificate
        ''' <summary>
        ''' 当前的这个管理节点和其所管理的服务实例之间相互通信所需要的身份认证信息
        ''' </summary>
        ReadOnly _invokeAuthnic As Net.SSL.Certificate

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Instance">全路径</param>
        ''' <param name="Master">主节点的IP公网地址</param>
        ''' <param name="PublicToken"></param>
        Sub New(Instance As String, Master As String, PublicToken As String)
            Dim uid As Long = SecurityString.ToLong(SecurityString.NewUid)
            Dim CA As Net.SSL.Certificate = Net.SSL.Certificate.Install(PublicToken, uid)

            _instance = FileIO.FileSystem.GetFileInfo(Instance).FullName
            _master = New Microsoft.VisualBasic.Net.IPEndPoint(
                Master, Protocols.MasterSvr).GetIPEndPoint
            _protocolHandler = New ProtocolHandler(Me)
            _socket = New Net.SSL.SSLSynchronizationServicesSocket(Protocols.ParasitiferSvr, CA, container:=Me)
            _OAuth = Net.SSL.SSLProtocols.Handshaking(CA, _master)
            _invokeAuthnic = New Certificate(Guid.NewGuid.ToString, Now.ToBinary)

#If DEBUG Then
            Call _OAuth.__DEBUG_ECHO
#End If

            Call __register()
        End Sub

        Private Sub __register()
            Dim request As RequestStream = Protocols.NodeRegister(__getIPAddress, _OAuth)
            Dim socket As New Net.AsynInvoke(_master)
            Dim response As RequestStream = socket.SendMessage(request, _OAuth)
#If DEBUG Then
            If response.Protocol = HTTP_RFC.RFC_OK Then
                Call $"{MethodBase.GetCurrentMethod.GetFullName} ==> OK!!!".__DEBUG_ECHO
            Else
                Call response.__DEBUG_ECHO
            End If
#End If
            Call Me._socket.Install(_OAuth, True)
        End Sub

        ''' <summary>
        ''' 获取这台主机的公网IP地址
        ''' </summary>
        ''' <returns></returns>
        Protected Function __getIPAddress() As String
#If DEBUG Then
            Return Net.AsynInvoke.LocalIPAddress
#Else
             Return WebServices.GetMyIPAddress
#End If
        End Function

        Public Sub Run()
            _socket.Responsehandler = AddressOf _protocolHandler.HandleRequest
            _socket.Run()
        End Sub

#Region "Protocols"

        <Protocol(Protocols.Protocols.FolkInstance)>
        Private Function FolkInstance(CA As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
            Dim cli As String = $"{request.GetUTF8String} {Protocols.OAuth} {_invokeAuthnic.BuildOAuth}"

            Call $"Parallel folk >>  {_instance}  {cli}".__DEBUG_ECHO

            Dim proc As Process = Nothing
            Dim Port As Integer = Microsoft.VisualBasic.Parallel.Folk(_instance, cli, proc)
            Dim Portal As Microsoft.VisualBasic.Net.IPEndPoint =
                New Net.IPEndPoint(WebServices.GetMyIPAddress, Port) With {
                    .uid = proc.Id
            }
            Dim inst As New DDM.Instance With {
                .Process = proc,
                .Portal = Portal
            }

            Call Me._instanceList.Add(Portal.ToString, inst)
            Call inst.Portal.GetXml.__DEBUG_ECHO

            Return New RequestStream(0, HTTP_RFC.RFC_OK, inst)
        End Function

        <Protocol(Protocols.Protocols.GetLoad)>
        Private Function GetLoad() As RequestStream
            Return New RequestStream(0, HTTP_RFC.RFC_OK, CStr(SystemLoad()))
        End Function

        <Protocol(Protocols.Protocols.GetInstanceList)>
        Private Function GetInstanceList() As RequestStream
            Dim list As Microsoft.VisualBasic.Net.IPEndPoint() =
                Me._instanceList.Values.ToArray(Function(svr) svr.Portal)
            Dim xml As String = list.GetXml
            Return New RequestStream(0, HTTP_RFC.RFC_OK, xml)
        End Function
#End Region

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call _socket.Dispose()
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

    Namespace DDM

        Public Class Instance : Inherits Net.Protocol.RawStream

            <Xml.Serialization.XmlIgnore>
            Public Property Process As Process
            ''' <summary>
            ''' 直接对这个节点进行交互的端口
            ''' </summary>
            ''' <returns></returns>
            <Xml.Serialization.XmlElement>
            Public Property Portal As Microsoft.VisualBasic.Net.IPEndPoint

            Sub New(rawStream As Byte())
                Dim str As String = System.Text.Encoding.UTF8.GetString(rawStream)
                Try
                    Portal = str.CreateObjectFromXml(Of Microsoft.VisualBasic.Net.IPEndPoint)
                Catch ex As Exception
                    Throw New Exception(str, ex)
                End Try
            End Sub

            Friend Sub New()
            End Sub

            Public Overrides Function Serialize() As Byte()
                Dim str As String = Portal.GetXml
                Dim bytData As Byte() = System.Text.Encoding.UTF8.GetBytes(str)
                Return bytData
            End Function

            Public Overrides Function ToString() As String
                Return Portal.ToString
            End Function
        End Class
    End Namespace
End Namespace