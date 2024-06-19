Imports System.Collections.Concurrent
Imports System.IO
Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Net.Security
Imports System.Net.Sockets
Imports System.Runtime.InteropServices
Imports System.Security.Authentication
Imports System.Security.Cryptography.X509Certificates
Imports System.Text
Imports System.Threading

Namespace SuperSimpleTcp

    ''' <summary>
    ''' SimpleTcp server with SSL support.  
    ''' Set the ClientConnected, ClientDisconnected, and DataReceived events.  
    ''' Once set, use Start() to begin listening for connections.
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/jchristn/SuperSimpleTcp
    ''' </remarks>
    Public Class SimpleTcpServer
        Implements IDisposable
#Region "Public-Members"

        ''' <summary>
        ''' Indicates if the server is listening for connections.
        ''' </summary>
        Public ReadOnly Property IsListening As Boolean
            Get
                Return _isListening
            End Get
        End Property

        ''' <summary>
        ''' SimpleTcp server settings.
        ''' </summary>
        Public Property Settings As SimpleTcpServerSettings
            Get
                Return _settings
            End Get
            Set(value As SimpleTcpServerSettings)
                If value Is Nothing Then
                    _settings = New SimpleTcpServerSettings()
                Else
                    _settings = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' SimpleTcp server events.
        ''' </summary>
        Public Property Events As SimpleTcpServerEvents
            Get
                Return _events
            End Get
            Set(value As SimpleTcpServerEvents)
                If value Is Nothing Then
                    _events = New SimpleTcpServerEvents()
                Else
                    _events = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' SimpleTcp statistics.
        ''' </summary>
        Public ReadOnly Property Statistics As SimpleTcpStatistics
            Get
                Return _statistics
            End Get
        End Property

        ''' <summary>
        ''' SimpleTcp keepalive settings.
        ''' </summary>
        Public Property Keepalive As SimpleTcpKeepaliveSettings
            Get
                Return _keepalive
            End Get
            Set(value As SimpleTcpKeepaliveSettings)
                If value Is Nothing Then
                    _keepalive = New SimpleTcpKeepaliveSettings()
                Else
                    _keepalive = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Retrieve the number of current connected clients.
        ''' </summary>
        Public ReadOnly Property Connections As Integer
            Get
                Return _clients.Count
            End Get
        End Property

        ''' <summary>
        ''' The IP address on which the server is configured to listen.
        ''' </summary>
        Public ReadOnly Property IpAddress As IPAddress
            Get
                Return _ipAddress
            End Get
        End Property

        ''' <summary>
        ''' The IPEndPoint on which the server is configured to listen.
        ''' </summary>
        Public ReadOnly Property Endpoint As EndPoint
            Get
                Return If(_listener Is Nothing, Nothing, CType(_listener.LocalEndpoint, System.Net.IPEndPoint))
            End Get
        End Property
        ''' <summary>
        ''' The port on which the server is configured to listen.
        ''' </summary>
        Public ReadOnly Property Port As Integer
            Get
                Return If(_listener Is Nothing, 0, CType(_listener.LocalEndpoint, System.Net.IPEndPoint).Port)
            End Get
        End Property

        ''' <summary>
        ''' Method to invoke to send a log message.
        ''' </summary>
        Public Logger As Action(Of String) = Nothing

#End Region

#Region "Private-Members"

        Private ReadOnly _header As String = "[SimpleTcp.Server] "
        Private _settings As SimpleTcpServerSettings = New SimpleTcpServerSettings()
        Private _events As SimpleTcpServerEvents = New SimpleTcpServerEvents()
        Private _keepalive As SimpleTcpKeepaliveSettings = New SimpleTcpKeepaliveSettings()
        Private _statistics As SimpleTcpStatistics = New SimpleTcpStatistics()

        Private ReadOnly _listenerIp As String = Nothing
        Private ReadOnly _ipAddress As IPAddress = Nothing
        Private ReadOnly _port As Integer = 0
        Private ReadOnly _ssl As Boolean = False

        Private ReadOnly _sslCertificate As X509Certificate2 = Nothing
        Private ReadOnly _sslCertificateCollection As X509Certificate2Collection = Nothing

        Private ReadOnly _clients As ConcurrentDictionary(Of String, ClientMetadata) = New ConcurrentDictionary(Of String, ClientMetadata)()
        Private ReadOnly _clientsLastSeen As ConcurrentDictionary(Of String, Date) = New ConcurrentDictionary(Of String, Date)()
        Private ReadOnly _clientsKicked As ConcurrentDictionary(Of String, Date) = New ConcurrentDictionary(Of String, Date)()
        Private ReadOnly _clientsTimedout As ConcurrentDictionary(Of String, Date) = New ConcurrentDictionary(Of String, Date)()

        Private _listener As TcpListener = Nothing
        Private _isListening As Boolean = False

        Private _tokenSource As CancellationTokenSource = New CancellationTokenSource()
        Private _token As CancellationToken
        Private _listenerTokenSource As CancellationTokenSource = New CancellationTokenSource()
        Private _listenerToken As CancellationToken
        Private _acceptConnections As Task = Nothing
        Private _idleClientMonitor As Task = Nothing

#End Region

#Region "Constructors-and-Factories"

        ''' <summary>
        ''' Instantiates the TCP server without SSL.  Set the ClientConnected, ClientDisconnected, and DataReceived callbacks.  Once set, use Start() to begin listening for connections.
        ''' </summary>
        ''' <param name="ipPort">The IP:port of the server.</param> 
        Public Sub New(ipPort As String)
            If String.IsNullOrEmpty(ipPort) Then Throw New ArgumentNullException(NameOf(ipPort))

            ParseIpPort(ipPort, _listenerIp, _port)

            If _port < 0 Then Throw New ArgumentException("Port must be zero or greater.")
            If String.IsNullOrEmpty(_listenerIp) Then
                _ipAddress = IPAddress.Loopback
                _listenerIp = _ipAddress.ToString()
            ElseIf Equals(_listenerIp, "*") OrElse Equals(_listenerIp, "+") Then
                _ipAddress = IPAddress.Any
            Else
                If Not IPAddress.TryParse(_listenerIp, _ipAddress) Then
                    _ipAddress = Dns.GetHostEntry(_listenerIp).AddressList(0)
                    _listenerIp = _ipAddress.ToString()
                End If
            End If

            _isListening = False
            _token = _tokenSource.Token
        End Sub

        ''' <summary>
        ''' Instantiates the TCP server without SSL.  Set the ClientConnected, ClientDisconnected, and DataReceived callbacks.  Once set, use Start() to begin listening for connections.
        ''' </summary>
        ''' <param name="listenerIp">The listener IP address or hostname.</param>
        ''' <param name="port">The TCP port on which to listen.</param>
        Public Sub New(listenerIp As String, port As Integer)
            If port < 0 Then Throw New ArgumentException("Port must be zero or greater.")

            _listenerIp = listenerIp
            _port = port

            If String.IsNullOrEmpty(_listenerIp) Then
                _ipAddress = IPAddress.Loopback
                _listenerIp = _ipAddress.ToString()
            ElseIf Equals(_listenerIp, "*") OrElse Equals(_listenerIp, "+") Then
                _ipAddress = IPAddress.Any
                _listenerIp = listenerIp
            Else
                If Not IPAddress.TryParse(_listenerIp, _ipAddress) Then
                    _ipAddress = Dns.GetHostEntry(listenerIp).AddressList(0)
                    _listenerIp = _ipAddress.ToString()
                End If
            End If

            _isListening = False
            _token = _tokenSource.Token
        End Sub

        ''' <summary>
        ''' Instantiates the TCP server.  Set the ClientConnected, ClientDisconnected, and DataReceived callbacks.  Once set, use Start() to begin listening for connections.
        ''' </summary>
        ''' <param name="ipPort">The IP:port of the server.</param> 
        ''' <param name="ssl">Enable or disable SSL.</param>
        ''' <param name="pfxCertFilename">The filename of the PFX certificate file.</param>
        ''' <param name="pfxPassword">The password to the PFX certificate file.</param>
        Public Sub New(ipPort As String, ssl As Boolean, pfxCertFilename As String, pfxPassword As String)
            If String.IsNullOrEmpty(ipPort) Then Throw New ArgumentNullException(NameOf(ipPort))

            ParseIpPort(ipPort, _listenerIp, _port)
            If _port < 0 Then Throw New ArgumentException("Port must be zero or greater.")

            If String.IsNullOrEmpty(_listenerIp) Then
                _ipAddress = IPAddress.Loopback
                _listenerIp = _ipAddress.ToString()
            ElseIf Equals(_listenerIp, "*") OrElse Equals(_listenerIp, "+") Then
                _ipAddress = IPAddress.Any
            Else
                If Not IPAddress.TryParse(_listenerIp, _ipAddress) Then
                    _ipAddress = Dns.GetHostEntry(_listenerIp).AddressList(0)
                    _listenerIp = _ipAddress.ToString()
                End If
            End If

            _ssl = ssl
            _isListening = False
            _token = _tokenSource.Token

            If _ssl Then
                If String.IsNullOrEmpty(pfxPassword) Then
                    _sslCertificate = New X509Certificate2(pfxCertFilename)
                Else
                    _sslCertificate = New X509Certificate2(pfxCertFilename, pfxPassword)
                End If

                _sslCertificateCollection = New X509Certificate2Collection From {
    _sslCertificate
}
            End If
        End Sub

        ''' <summary>
        ''' Instantiates the TCP server.  Set the ClientConnected, ClientDisconnected, and DataReceived callbacks.  Once set, use Start() to begin listening for connections.
        ''' </summary>
        ''' <param name="listenerIp">The listener IP address or hostname.</param>
        ''' <param name="port">The TCP port on which to listen.</param>
        ''' <param name="ssl">Enable or disable SSL.</param>
        ''' <param name="pfxCertFilename">The filename of the PFX certificate file.</param>
        ''' <param name="pfxPassword">The password to the PFX certificate file.</param>
        Public Sub New(listenerIp As String, port As Integer, ssl As Boolean, pfxCertFilename As String, pfxPassword As String)
            If port < 0 Then Throw New ArgumentException("Port must be zero or greater.")

            _listenerIp = listenerIp
            _port = port

            If String.IsNullOrEmpty(_listenerIp) Then
                _ipAddress = IPAddress.Loopback
                _listenerIp = _ipAddress.ToString()
            ElseIf Equals(_listenerIp, "*") OrElse Equals(_listenerIp, "+") Then
                _ipAddress = IPAddress.Any
            Else
                If Not IPAddress.TryParse(_listenerIp, _ipAddress) Then
                    _ipAddress = Dns.GetHostEntry(listenerIp).AddressList(0)
                    _listenerIp = _ipAddress.ToString()
                End If
            End If

            _ssl = ssl
            _isListening = False
            _token = _tokenSource.Token

            If _ssl Then
                If String.IsNullOrEmpty(pfxPassword) Then
                    _sslCertificate = New X509Certificate2(pfxCertFilename)
                Else
                    _sslCertificate = New X509Certificate2(pfxCertFilename, pfxPassword)
                End If

                _sslCertificateCollection = New X509Certificate2Collection From {
    _sslCertificate
}
            End If
        End Sub

        ''' <summary>
        ''' Instantiates the TCP server with SSL.  Set the ClientConnected, ClientDisconnected, and DataReceived callbacks.  Once set, use Start() to begin listening for connections.
        ''' </summary>
        ''' <param name="listenerIp">The listener IP address or hostname.</param>
        ''' <param name="port">The TCP port on which to listen.</param>
        ''' <param name="certificate">Byte array containing the certificate.</param>
        Public Sub New(listenerIp As String, port As Integer, certificate As Byte())
            If port < 0 Then Throw New ArgumentException("Port must be zero or greater.")
            If certificate Is Nothing Then Throw New ArgumentNullException(NameOf(certificate))

            _listenerIp = listenerIp
            _port = port

            If String.IsNullOrEmpty(_listenerIp) Then
                _ipAddress = IPAddress.Loopback
                _listenerIp = _ipAddress.ToString()
            ElseIf Equals(_listenerIp, "*") OrElse Equals(_listenerIp, "+") Then
                _ipAddress = IPAddress.Any
            Else
                If Not IPAddress.TryParse(_listenerIp, _ipAddress) Then
                    _ipAddress = Dns.GetHostEntry(listenerIp).AddressList(0)
                    _listenerIp = _ipAddress.ToString()
                End If
            End If

            _ssl = True
            _sslCertificate = New X509Certificate2(certificate)
            _sslCertificateCollection = New X509Certificate2Collection From {
    _sslCertificate
}

            _isListening = False
            _token = _tokenSource.Token
        End Sub

#End Region

#Region "Public-Methods"

        ''' <summary>
        ''' Dispose of the TCP server.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        ''' <summary>
        ''' Start accepting connections.
        ''' </summary>
        Public Sub Start()
            If _isListening Then Throw New InvalidOperationException("SimpleTcpServer is already running.")

            _listener = New TcpListener(_ipAddress, _port)
            _listener.Server.NoDelay = _settings.NoDelay
            _listener.Start()
            _isListening = True

            _tokenSource = New CancellationTokenSource()
            _token = _tokenSource.Token
            _listenerTokenSource = New CancellationTokenSource()
            _listenerToken = _listenerTokenSource.Token

            _statistics = New SimpleTcpStatistics()

            If _idleClientMonitor Is Nothing Then
                _idleClientMonitor = Task.Run(Function() IdleClientMonitor(), _token)
            End If

            _acceptConnections = Task.Run(Function() AcceptConnections(), _listenerToken)
        End Sub

        ''' <summary>
        ''' Start accepting connections.
        ''' </summary>
        ''' <returns>Task.</returns>
        Public Function StartAsync() As Task
            If _isListening Then Throw New InvalidOperationException("SimpleTcpServer is already running.")

            _listener = New TcpListener(_ipAddress, _port)

            If _keepalive.EnableTcpKeepAlives Then EnableKeepalives()

            _listener.Start()
            _isListening = True

            _tokenSource = New CancellationTokenSource()
            _token = _tokenSource.Token
            _listenerTokenSource = New CancellationTokenSource()
            _listenerToken = _listenerTokenSource.Token

            _statistics = New SimpleTcpStatistics()

            If _idleClientMonitor Is Nothing Then
                _idleClientMonitor = Task.Run(Function() IdleClientMonitor(), _token)
            End If

            _acceptConnections = Task.Run(Function() AcceptConnections(), _listenerToken)
            Return _acceptConnections
        End Function

        ''' <summary>
        ''' Stop accepting new connections.
        ''' </summary>
        Public Sub [Stop]()
            If Not _isListening Then Throw New InvalidOperationException("SimpleTcpServer is not running.")

            _isListening = False
            _listener.Stop()
            _listenerTokenSource.Cancel()
            _acceptConnections.Wait()
            _acceptConnections = Nothing

            Logger?.Invoke($"{_header}stopped")
        End Sub

        ''' <summary>
        ''' Retrieve a list of client IP:port connected to the server.
        ''' </summary>
        ''' <returns>IEnumerable of strings, each containing client IP:port.</returns>
        Public Function GetClients() As IEnumerable(Of String)
            Dim clients As List(Of String) = New List(Of String)(_clients.Keys)
            Return clients
        End Function

        ''' <summary>
        ''' Determines if a client is connected by its IP:port.
        ''' </summary>
        ''' <param name="ipPort">The client IP:port string.</param>
        ''' <returns>True if connected.</returns>
        Public Function IsConnected(ipPort As String) As Boolean
            If String.IsNullOrEmpty(ipPort) Then
                Throw New ArgumentNullException(NameOf(ipPort))
            End If

            Return _clients.TryGetValue(ipPort, Nothing)
        End Function

        ''' <summary>
        ''' Send data to the specified client by IP:port.
        ''' </summary>
        ''' <param name="ipPort">The client IP:port string.</param>
        ''' <param name="data">String containing data to send.</param>
        Public Sub Send(ipPort As String, data As String)
            If String.IsNullOrEmpty(ipPort) Then Throw New ArgumentNullException(NameOf(ipPort))
            If String.IsNullOrEmpty(data) Then Throw New ArgumentNullException(NameOf(data))
            Dim bytes = Encoding.UTF8.GetBytes(data)

            Using ms As MemoryStream = New MemoryStream()
                ms.Write(bytes, 0, bytes.Length)
                ms.Seek(0, SeekOrigin.Begin)
                SendInternal(ipPort, bytes.Length, ms)
            End Using
        End Sub

        ''' <summary>
        ''' Send data to the specified client by IP:port.
        ''' </summary>
        ''' <param name="ipPort">The client IP:port string.</param>
        ''' <param name="data">Byte array containing data to send.</param>
        Public Sub Send(ipPort As String, data As Byte())
            If String.IsNullOrEmpty(ipPort) Then Throw New ArgumentNullException(NameOf(ipPort))
            If data Is Nothing OrElse data.Length < 1 Then Throw New ArgumentNullException(NameOf(data))

            Using ms As MemoryStream = New MemoryStream()
                ms.Write(data, 0, data.Length)
                ms.Seek(0, SeekOrigin.Begin)
                SendInternal(ipPort, data.Length, ms)
            End Using
        End Sub

        ''' <summary>
        ''' Send data to the specified client by IP:port.
        ''' </summary>
        ''' <param name="ipPort">The client IP:port string.</param>
        ''' <param name="contentLength">The number of bytes to read from the source stream to send.</param>
        ''' <param name="stream">Stream containing the data to send.</param>
        Public Sub Send(ipPort As String, contentLength As Long, stream As Stream)
            If String.IsNullOrEmpty(ipPort) Then Throw New ArgumentNullException(NameOf(ipPort))
            If contentLength < 1 Then Return
            If stream Is Nothing Then Throw New ArgumentNullException(NameOf(stream))
            If Not stream.CanRead Then Throw New InvalidOperationException("Cannot read from supplied stream.")

            SendInternal(ipPort, contentLength, stream)
        End Sub

        ''' <summary>
        ''' Send data to the specified client by IP:port asynchronously.
        ''' </summary>
        ''' <param name="ipPort">The client IP:port string.</param>
        ''' <param name="data">String containing data to send.</param>
        ''' <param name="token">Cancellation token for canceling the request.</param>
        Public Async Function SendAsync(ipPort As String, data As String, Optional token As CancellationToken = Nothing) As Task
            If String.IsNullOrEmpty(ipPort) Then Throw New ArgumentNullException(NameOf(ipPort))
            If String.IsNullOrEmpty(data) Then Throw New ArgumentNullException(NameOf(data))
            If token = Nothing Then token = _token

            Dim bytes = Encoding.UTF8.GetBytes(data)
            Using ms As MemoryStream = New MemoryStream()
                Await ms.WriteAsync(bytes, 0, bytes.Length, token).ConfigureAwait(False)
                ms.Seek(0, SeekOrigin.Begin)
                Await SendInternalAsync(ipPort, bytes.Length, ms, token).ConfigureAwait(False)
            End Using
        End Function

        ''' <summary>
        ''' Send data to the specified client by IP:port asynchronously.
        ''' </summary>
        ''' <param name="ipPort">The client IP:port string.</param>
        ''' <param name="data">Byte array containing data to send.</param>
        ''' <param name="token">Cancellation token for canceling the request.</param>
        Public Async Function SendAsync(ipPort As String, data As Byte(), Optional token As CancellationToken = Nothing) As Task
            If String.IsNullOrEmpty(ipPort) Then Throw New ArgumentNullException(NameOf(ipPort))
            If data Is Nothing OrElse data.Length < 1 Then Throw New ArgumentNullException(NameOf(data))
            If token = Nothing Then token = _token

            Using ms As MemoryStream = New MemoryStream()
                Await ms.WriteAsync(data, 0, data.Length, token).ConfigureAwait(False)
                ms.Seek(0, SeekOrigin.Begin)
                Await SendInternalAsync(ipPort, data.Length, ms, token).ConfigureAwait(False)
            End Using
        End Function

        ''' <summary>
        ''' Send data to the specified client by IP:port asynchronously.
        ''' </summary>
        ''' <param name="ipPort">The client IP:port string.</param>
        ''' <param name="contentLength">The number of bytes to read from the source stream to send.</param>
        ''' <param name="stream">Stream containing the data to send.</param>
        ''' <param name="token">Cancellation token for canceling the request.</param>
        Public Async Function SendAsync(ipPort As String, contentLength As Long, stream As Stream, Optional token As CancellationToken = Nothing) As Task
            If String.IsNullOrEmpty(ipPort) Then Throw New ArgumentNullException(NameOf(ipPort))
            If contentLength < 1 Then Return
            If stream Is Nothing Then Throw New ArgumentNullException(NameOf(stream))
            If Not stream.CanRead Then Throw New InvalidOperationException("Cannot read from supplied stream.")
            If token = Nothing Then token = _token

            Await SendInternalAsync(ipPort, contentLength, stream, token).ConfigureAwait(False)
        End Function

        ''' <summary>
        ''' Disconnects the specified client.
        ''' </summary>
        ''' <param name="ipPort">IP:port of the client.</param>
        Public Sub DisconnectClient(ipPort As String)
            If String.IsNullOrEmpty(ipPort) Then Throw New ArgumentNullException(NameOf(ipPort))

            Dim client As ClientMetadata = Nothing

            If Not _clients.TryGetValue(ipPort, client) Then
                Logger?.Invoke($"{_header}unable to find client: {ipPort}")
            Else
                If Not _clientsTimedout.ContainsKey(ipPort) Then
                    Logger?.Invoke($"{_header}kicking: {ipPort}")
                    _clientsKicked.TryAdd(ipPort, Date.Now)
                End If
            End If

            If client IsNot Nothing Then
                If Not client.TokenSource.IsCancellationRequested Then
                    client.TokenSource.Cancel()
                    Logger?.Invoke($"{_header}requesting disposal of: {ipPort}")
                End If

                client.Dispose()
            End If
        End Sub

#End Region

#Region "Private-Methods"

        ''' <summary>
        ''' Dispose of the TCP server.
        ''' </summary>
        ''' <param name="disposing">Dispose of resources.</param>
        Protected Overridable Sub Dispose(disposing As Boolean)
            If disposing Then
                Try
                    If _clients IsNot Nothing AndAlso _clients.Count > 0 Then
                        For Each curr In _clients
                            curr.Value.Dispose()
                            Logger?.Invoke($"{_header}disconnected client: {curr.Key}")
                        Next
                    End If

                    If _tokenSource IsNot Nothing Then
                        If Not _tokenSource.IsCancellationRequested Then
                            _tokenSource.Cancel()
                        End If

                        _tokenSource.Dispose()
                    End If

                    If _listener IsNot Nothing AndAlso _listener.Server IsNot Nothing Then
                        _listener.Server.Close()
                        _listener.Server.Dispose()
                    End If

                    If _listener IsNot Nothing Then
                        _listener.Stop()
                    End If
                Catch e As Exception
                    Logger?.Invoke($"{_header}dispose exception:{Environment.NewLine}{e}{Environment.NewLine}")
                End Try

                _isListening = False

                Logger?.Invoke($"{_header}disposed")
            End If
        End Sub

        Private Function IsClientConnected(client As TcpClient) As Boolean
            If client Is Nothing Then Return False

            Dim state = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().FirstOrDefault(Function(x) x.LocalEndPoint.Equals(client.Client.LocalEndPoint) AndAlso x.RemoteEndPoint.Equals(client.Client.RemoteEndPoint))

            If state Is Nothing OrElse state.State = TcpState.Unknown OrElse state.State = TcpState.FinWait1 OrElse state.State = TcpState.FinWait2 OrElse state.State = TcpState.Closed OrElse state.State = TcpState.Closing OrElse state.State = TcpState.CloseWait Then
                Return False
            End If

            If client.Client.Poll(0, SelectMode.SelectWrite) AndAlso Not client.Client.Poll(0, SelectMode.SelectError) Then
                Dim buffer = New Byte(0) {}
                If client.Client.Receive(buffer, SocketFlags.Peek) = 0 Then
                    Return False
                Else
                    Return True
                End If
            End If

            Return False
        End Function

        Private Async Function AcceptConnections() As Task
            While Not _listenerToken.IsCancellationRequested
                Dim client As ClientMetadata = Nothing

                Try
#Region "Check-for-Maximum-Connections"

                    If Not _isListening AndAlso _clients.Count >= _settings.MaxConnections Then
                        Call Task.Delay(100).Wait()
                        Continue While
                    ElseIf Not _isListening Then
                        _listener.Start()
                        _isListening = True
                    End If

#End Region

                    Dim tcpClient As TcpClient = Await _listener.AcceptTcpClientAsync().ConfigureAwait(False)
                    Dim clientIpPort As String = tcpClient.Client.RemoteEndPoint.ToString()

                    Dim clientIp As String = Nothing
                    Dim clientPort = 0
                    ParseIpPort(clientIpPort, clientIp, clientPort)

                    If _settings.PermittedIPs.Count > 0 AndAlso Not _settings.PermittedIPs.Contains(clientIp) Then
                        Logger?.Invoke($"{_header}rejecting connection from {clientIp} (not permitted)")
                        tcpClient.Close()
                        Continue While
                    End If

                    If _settings.BlockedIPs.Count > 0 AndAlso _settings.BlockedIPs.Contains(clientIp) Then
                        Logger?.Invoke($"{_header}rejecting connection from {clientIp} (blocked)")
                        tcpClient.Close()
                        Continue While
                    End If

                    client = New ClientMetadata(tcpClient)

                    If _ssl Then
                        If _settings.AcceptInvalidCertificates Then
                            client.SslStream = New SslStream(client.NetworkStream, False, New RemoteCertificateValidationCallback(AddressOf AcceptCertificate))
                        ElseIf _settings.CertificateValidationCallback IsNot Nothing Then
                            client.SslStream = New SslStream(client.NetworkStream, False, _settings.CertificateValidationCallback)
                        Else
                            client.SslStream = New SslStream(client.NetworkStream, False)
                        End If

                        Dim tlsCts = CancellationTokenSource.CreateLinkedTokenSource(_listenerToken, _token)
                        tlsCts.CancelAfter(3000)

                        Dim success = Await StartTls(client, tlsCts.Token).ConfigureAwait(False)
                        If Not success Then
                            client.Dispose()
                            Continue While
                        End If
                    End If

                    _clients.TryAdd(clientIpPort, client)
                    _clientsLastSeen.TryAdd(clientIpPort, Date.Now)
                    Logger?.Invoke($"{_header}starting data receiver for: {clientIpPort}")
                    _events.HandleClientConnected(Me, New ConnectionEventArgs(clientIpPort))

                    If _keepalive.EnableTcpKeepAlives Then EnableKeepalives(tcpClient)

                    Dim linkedCts = CancellationTokenSource.CreateLinkedTokenSource(client.Token, _token)
                    Dim unawaited = Task.Run(Function() DataReceiver(client), linkedCts.Token)

#Region "Check-for-Maximum-Connections"

                    If _clients.Count >= _settings.MaxConnections Then
                        Logger?.Invoke($"{_header}maximum connections {_settings.MaxConnections} met (currently {_clients.Count} connections), pausing")
                        _isListening = False
                        _listener.Stop()

#End Region
                    End If
                Catch ex As Exception
                    If TypeOf ex Is TaskCanceledException OrElse TypeOf ex Is OperationCanceledException OrElse TypeOf ex Is ObjectDisposedException OrElse TypeOf ex Is InvalidOperationException Then
                        _isListening = False
                        If client IsNot Nothing Then client.Dispose()
                        Logger?.Invoke($"{_header}stopped listening")
                        Exit While
                    Else
                        If client IsNot Nothing Then client.Dispose()
                        Logger?.Invoke($"{_header}exception while awaiting connections: {ex}")
                        Continue While
                    End If
                End Try
            End While

            _isListening = False
        End Function

        Private Async Function StartTls(client As ClientMetadata, token As CancellationToken) As Task(Of Boolean)
            Try
                Await client.SslStream.AuthenticateAsServerAsync(_sslCertificate, _settings.MutuallyAuthenticate, SslProtocols.Tls12, _settings.CheckCertificateRevocation).ConfigureAwait(False)

                If Not client.SslStream.IsEncrypted Then
                    Logger?.Invoke($"{_header}client {client.IpPort} not encrypted, disconnecting")
                    client.Dispose()
                    Return False
                End If

                If Not client.SslStream.IsAuthenticated Then
                    Logger?.Invoke($"{_header}client {client.IpPort} not SSL/TLS authenticated, disconnecting")
                    client.Dispose()
                    Return False
                End If

                If _settings.MutuallyAuthenticate AndAlso Not client.SslStream.IsMutuallyAuthenticated Then
                    Logger?.Invoke($"{_header}client {client.IpPort} failed mutual authentication, disconnecting")
                    client.Dispose()
                    Return False
                End If
            Catch e As Exception
                If TypeOf e Is TaskCanceledException OrElse TypeOf e Is OperationCanceledException Then
                    Logger?.Invoke($"{_header}client {client.IpPort} timeout during SSL/TLS establishment")
                Else
                    Logger?.Invoke($"{_header}client {client.IpPort} SSL/TLS exception: {Environment.NewLine}{e}")
                End If

                client.Dispose()
                Return False
            End Try

            Return True
        End Function

        Private Function AcceptCertificate(sender As Object, certificate As X509Certificate, chain As X509Chain, sslPolicyErrors As SslPolicyErrors) As Boolean
            ' return true; // Allow untrusted certificates.
            Return _settings.AcceptInvalidCertificates
        End Function

        Private Async Function DataReceiver(client As ClientMetadata) As Task
            Dim ipPort = client.IpPort
            Logger?.Invoke($"{_header}data receiver started for client {ipPort}")

            Dim linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_token, client.Token)

            While True
                Try
                    If Not IsClientConnected(client.Client) Then
                        Logger?.Invoke($"{_header}client {ipPort} disconnected")
                        Exit While
                    End If

                    If client.Token.IsCancellationRequested Then
                        Logger?.Invoke($"{_header}cancellation requested (data receiver for client {ipPort})")
                        Exit While
                    End If

                    Dim data As ArraySegment(Of Byte) = Await DataReadAsync(client, linkedCts.Token).ConfigureAwait(False)
                    If data.Array Is Nothing Then
                        Await Task.Delay(10, linkedCts.Token).ConfigureAwait(False)
                        Continue While
                    End If

                    Dim action As Action = Sub() _events.HandleDataReceived(Me, New DataReceivedEventArgs(ipPort, data))
                    If _settings.UseAsyncDataReceivedEvents Then
                        Call Task.Run(action, linkedCts.Token)
                    Else
                        action.Invoke()
                    End If

                    _statistics.ReceivedBytes += data.Count
                    UpdateClientLastSeen(client.IpPort)
                Catch __unusedIOException1__ As IOException
                    Logger?.Invoke($"{_header}data receiver canceled, peer disconnected [{ipPort}]")
                    Exit While
                Catch __unusedSocketException2__ As SocketException
                    Logger?.Invoke($"{_header}data receiver canceled, peer disconnected [{ipPort}]")
                    Exit While
                Catch __unusedTaskCanceledException3__ As TaskCanceledException
                    Logger?.Invoke($"{_header}data receiver task canceled [{ipPort}]")
                    Exit While
                Catch __unusedObjectDisposedException4__ As ObjectDisposedException
                    Logger?.Invoke($"{_header}data receiver canceled due to disposal [{ipPort}]")
                    Exit While
                Catch e As Exception
                    Logger?.Invoke($"{_header}data receiver exception [{ipPort}]:{Environment.NewLine}{e}{Environment.NewLine}")
                    Exit While
                End Try
            End While

            Logger?.Invoke($"{_header}data receiver terminated for client {ipPort}")

            If _clientsKicked.ContainsKey(ipPort) Then
                _events.HandleClientDisconnected(Me, New ConnectionEventArgs(ipPort, DisconnectReason.Kicked))
            ElseIf _clientsTimedout.ContainsKey(client.IpPort) Then
                _events.HandleClientDisconnected(Me, New ConnectionEventArgs(ipPort, DisconnectReason.Timeout))
            Else
                _events.HandleClientDisconnected(Me, New ConnectionEventArgs(ipPort, DisconnectReason.Normal))
            End If

            _clients.TryRemove(ipPort, Nothing)
            _clientsLastSeen.TryRemove(ipPort, Nothing)
            _clientsKicked.TryRemove(ipPort, Nothing)
            _clientsTimedout.TryRemove(ipPort, Nothing)

            If client IsNot Nothing Then client.Dispose()
        End Function

        Private Async Function DataReadAsync(client As ClientMetadata, token As CancellationToken) As Task(Of ArraySegment(Of Byte))
            Dim buffer = New Byte(_settings.StreamBufferSize - 1) {}
            Dim read = 0

            If Not _ssl Then
                Using ms As MemoryStream = New MemoryStream()
                    While True
                        read = Await client.NetworkStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(False)

                        If read > 0 Then
                            Await ms.WriteAsync(buffer, 0, read, token).ConfigureAwait(False)
                            Return New ArraySegment(Of Byte)(ms.GetBuffer(), 0, ms.Length)
                        Else
                            Throw New SocketException()
                        End If
                    End While
                End Using
            Else
                Using ms As MemoryStream = New MemoryStream()
                    While True
                        read = Await client.SslStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(False)

                        If read > 0 Then
                            Await ms.WriteAsync(buffer, 0, read, token).ConfigureAwait(False)
                            Return New ArraySegment(Of Byte)(ms.GetBuffer(), 0, ms.Length)
                        Else
                            Throw New SocketException()
                        End If
                    End While
                End Using
            End If

            Throw New InvalidDataException
        End Function

        Private Async Function IdleClientMonitor() As Task
            While Not _token.IsCancellationRequested
                Await Task.Delay(_settings.IdleClientEvaluationIntervalMs, _token).ConfigureAwait(False)

                If _settings.IdleClientTimeoutMs = 0 Then Continue While

                Try
                    Dim idleTimestamp = Date.Now.AddMilliseconds(-1 * _settings.IdleClientTimeoutMs)

                    For Each curr In _clientsLastSeen
                        If curr.Value < idleTimestamp Then
                            _clientsTimedout.TryAdd(curr.Key, Date.Now)
                            Logger?.Invoke($"{_header}disconnecting {curr.Key} due to timeout")
                            DisconnectClient(curr.Key)
                        End If
                    Next
                Catch e As Exception
                    Logger?.Invoke($"{_header}monitor exception: {e}")
                End Try
            End While
        End Function

        Private Sub UpdateClientLastSeen(ipPort As String)
            If _clientsLastSeen.ContainsKey(ipPort) Then
                _clientsLastSeen.TryRemove(ipPort, Nothing)
            End If

            _clientsLastSeen.TryAdd(ipPort, Date.Now)
        End Sub

        Private Sub SendInternal(ipPort As String, contentLength As Long, stream As Stream)
            Dim client As ClientMetadata = Nothing
            If Not _clients.TryGetValue(ipPort, client) Then Return
            If client Is Nothing Then Return

            Dim bytesRemaining = contentLength
            Dim bytesRead = 0
            Dim buffer = New Byte(_settings.StreamBufferSize - 1) {}

            Try
                client.SendLock.Wait()

                While bytesRemaining > 0
                    bytesRead = stream.Read(buffer, 0, buffer.Length)
                    If bytesRead > 0 Then
                        If Not _ssl Then
                            client.NetworkStream.Write(buffer, 0, bytesRead)
                        Else
                            client.SslStream.Write(buffer, 0, bytesRead)
                        End If

                        bytesRemaining -= bytesRead
                        _statistics.SentBytes += bytesRead
                    End If
                End While

                If Not _ssl Then
                    client.NetworkStream.Flush()
                Else
                    client.SslStream.Flush()
                End If
                _events.HandleDataSent(Me, New DataSentEventArgs(ipPort, contentLength))
            Finally
                If client IsNot Nothing Then client.SendLock.Release()
            End Try
        End Sub

        Private Async Function SendInternalAsync(ipPort As String, contentLength As Long, stream As Stream, token As CancellationToken) As Task
            Dim client As ClientMetadata = Nothing

            Try
                If Not _clients.TryGetValue(ipPort, client) Then Return
                If client Is Nothing Then Return

                Dim bytesRemaining = contentLength
                Dim bytesRead = 0
                Dim buffer = New Byte(_settings.StreamBufferSize - 1) {}

                Await client.SendLock.WaitAsync(token).ConfigureAwait(False)

                While bytesRemaining > 0
                    bytesRead = Await stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(False)
                    If bytesRead > 0 Then
                        If Not _ssl Then
                            Await client.NetworkStream.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(False)
                        Else
                            Await client.SslStream.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(False)
                        End If

                        bytesRemaining -= bytesRead
                        _statistics.SentBytes += bytesRead
                    End If
                End While

                If Not _ssl Then
                    Await client.NetworkStream.FlushAsync(token).ConfigureAwait(False)
                Else
                    Await client.SslStream.FlushAsync(token).ConfigureAwait(False)
                End If
                _events.HandleDataSent(Me, New DataSentEventArgs(ipPort, contentLength))
            Catch __unusedTaskCanceledException1__ As TaskCanceledException
            Catch __unusedOperationCanceledException2__ As OperationCanceledException
            Finally
                If client IsNot Nothing Then client.SendLock.Release()
            End Try
        End Function

        Private Sub EnableKeepalives()
            ' issues with definitions: https://github.com/dotnet/sdk/issues/14540

            Try
#If NETCOREAPP3_1_OR_GREATER Or NET6_0_OR_GREATER Then
                ' NETCOREAPP3_1_OR_GREATER catches .NET 5.0

                _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, True)
                _listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, _keepalive.TcpKeepAliveTime)
                _listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, _keepalive.TcpKeepAliveInterval)
                _listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, _keepalive.TcpKeepAliveRetryCount)
#Else
                Dim keepAlive = New Byte(12 - 1) {}

                ' Turn keepalive on
                Buffer.BlockCopy(BitConverter.GetBytes(CUInt(1)), 0, keepAlive, 0, 4)
                ' Set TCP keepalive time
                Buffer.BlockCopy(BitConverter.GetBytes(CUInt(_keepalive.TcpKeepAliveTimeMilliseconds)), 0, keepAlive, 4, 4)
                ' Set TCP keepalive interval
                Buffer.BlockCopy(BitConverter.GetBytes(CUInt(_keepalive.TcpKeepAliveIntervalMilliseconds)), 0, keepAlive, 8, 4)

                ' Set keepalive settings on the underlying Socket
                _listener.Server.IOControl(IOControlCode.KeepAliveValues, keepAlive, Nothing)
#End If
            Catch __unusedException1__ As Exception
                Logger?.Invoke($"{_header}keepalives not supported on this platform, disabled")
            End Try
        End Sub

        Private Sub EnableKeepalives(client As TcpClient)
            Try
#If NETCOREAPP3_1_OR_GREATER Or NET6_0_OR_GREATER Then
                ' NETCOREAPP3_1_OR_GREATER catches .NET 5.0

                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, True)
                client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, _keepalive.TcpKeepAliveTime)
                client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, _keepalive.TcpKeepAliveInterval)

                ' Windows 10 version 1703 or later

                If RuntimeInformation.IsOSPlatform(OSPlatform.Windows) AndAlso Environment.OSVersion.Version >= New Version(10, 0, 15063) Then
                    client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, _keepalive.TcpKeepAliveRetryCount)
                End If
#Else
                Dim keepAlive = New Byte(12 - 1) {}

                ' Turn keepalive on
                Buffer.BlockCopy(BitConverter.GetBytes(CUInt(1)), 0, keepAlive, 0, 4)
                ' Set TCP keepalive time
                Buffer.BlockCopy(BitConverter.GetBytes(CUInt(_keepalive.TcpKeepAliveTimeMilliseconds)), 0, keepAlive, 4, 4)
                ' Set TCP keepalive interval
                Buffer.BlockCopy(BitConverter.GetBytes(CUInt(_keepalive.TcpKeepAliveIntervalMilliseconds)), 0, keepAlive, 8, 4)

                ' Set keepalive settings on the underlying Socket
                client.Client.IOControl(IOControlCode.KeepAliveValues, keepAlive, Nothing)
#End If
            Catch __unusedException1__ As Exception
                Logger?.Invoke($"{_header}keepalives not supported on this platform, disabled")
                _keepalive.EnableTcpKeepAlives = False
            End Try
        End Sub

#End Region
    End Class
End Namespace
