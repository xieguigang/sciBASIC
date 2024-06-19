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
    ''' SimpleTcp client with SSL support.  
    ''' Set the Connected, Disconnected, and DataReceived events.  
    ''' Once set, use Connect() to connect to the server.
    ''' </summary>
    Public Class SimpleTcpClient
        Implements IDisposable
#Region "Public-Members"

        ''' <summary>
        ''' Indicates whether or not the client is connected to the server.
        ''' </summary>
        Public Property IsConnected As Boolean
            Get
                Return _isConnected
            End Get
            Private Set(value As Boolean)
                _isConnected = value
            End Set
        End Property

        ''' <summary>
        ''' Client IPEndPoint if connected.
        ''' </summary>
        Public ReadOnly Property LocalEndpoint As System.Net.IPEndPoint
            Get
                If _client IsNot Nothing AndAlso _isConnected Then
                    Return CType(_client.Client.LocalEndPoint, System.Net.IPEndPoint)
                End If

                Return Nothing
            End Get
        End Property

        ''' <summary>
        ''' SimpleTcp client settings.
        ''' </summary>
        Public Property Settings As SimpleTcpClientSettings
            Get
                Return _settings
            End Get
            Set(value As SimpleTcpClientSettings)
                If value Is Nothing Then
                    _settings = New SimpleTcpClientSettings()
                Else
                    _settings = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' SimpleTcp client events.
        ''' </summary>
        Public Property Events As SimpleTcpClientEvents
            Get
                Return _events
            End Get
            Set(value As SimpleTcpClientEvents)
                If value Is Nothing Then
                    _events = New SimpleTcpClientEvents()
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
        ''' Method to invoke to send a log message.
        ''' </summary>
        Public Logger As Action(Of String) = Nothing

        ''' <summary>
        ''' The IP:port of the server to which this client is mapped.
        ''' </summary>
        Public ReadOnly Property ServerIpPort As String
            Get
                Return $"{_serverIp}:{_serverPort}"
            End Get
        End Property

#End Region

#Region "Private-Members"

        Private ReadOnly _header As String = "[SimpleTcp.Client] "
        Private _settings As SimpleTcpClientSettings = New SimpleTcpClientSettings()
        Private _events As SimpleTcpClientEvents = New SimpleTcpClientEvents()
        Private _keepalive As SimpleTcpKeepaliveSettings = New SimpleTcpKeepaliveSettings()
        Private _statistics As SimpleTcpStatistics = New SimpleTcpStatistics()

        Private _serverIp As String = Nothing
        Private _serverPort As Integer = 0
        Private ReadOnly _ipAddress As IPAddress = Nothing
        Private _client As TcpClient = Nothing
        Private _networkStream As NetworkStream = Nothing

        Private _ssl As Boolean = False
        Private _pfxCertFilename As String = Nothing
        Private _pfxPassword As String = Nothing
        Private _sslStream As SslStream = Nothing
        Private _sslCert As X509Certificate2 = Nothing
        Private _sslCertCollection As X509Certificate2Collection = Nothing

        Private ReadOnly _sendLock As SemaphoreSlim = New SemaphoreSlim(1, 1)
        Private _isConnected As Boolean = False

        Private _dataReceiver As Task = Nothing
        Private _idleServerMonitor As Task = Nothing
        Private _connectionMonitor As Task = Nothing
        Private _tokenSource As CancellationTokenSource = New CancellationTokenSource()
        Private _token As CancellationToken

        Private _lastActivity As Date = Date.Now
        Private _isTimeout As Boolean = False

#End Region

#Region "Constructors-and-Factories"

        ''' <summary>
        ''' Instantiates the TCP client without SSL. 
        ''' Set the Connected, Disconnected, and DataReceived callbacks. Once set, use Connect() to connect to the server.
        ''' </summary>
        ''' <param name="ipPort">The IP:port of the server.</param> 
        Public Sub New(ipPort As String)
            If String.IsNullOrEmpty(ipPort) Then Throw New ArgumentNullException(NameOf(ipPort))

            ParseIpPort(ipPort, _serverIp, _serverPort)
            If _serverPort < 0 Then Throw New ArgumentException("Port must be zero or greater.")
            If String.IsNullOrEmpty(_serverIp) Then Throw New ArgumentNullException("Server IP or hostname must not be null.")

            If Not IPAddress.TryParse(_serverIp, _ipAddress) Then
                _ipAddress = Dns.GetHostEntry(_serverIp).AddressList(0)
                _serverIp = _ipAddress.ToString()
            End If
        End Sub

        ''' <summary>
        ''' Instantiates the TCP client. 
        ''' Set the Connected, Disconnected, and DataReceived callbacks. Once set, use Connect() to connect to the server.
        ''' </summary>
        ''' <param name="ipPort">The IP:port of the server.</param> 
        ''' <param name="ssl">Enable or disable SSL.</param>
        ''' <param name="pfxCertFilename">The filename of the PFX certificate file.</param>
        ''' <param name="pfxPassword">The password to the PFX certificate file.</param>
        Public Sub New(ipPort As String, ssl As Boolean, pfxCertFilename As String, pfxPassword As String)
            Me.New(ipPort)
            _ssl = ssl
            _pfxCertFilename = pfxCertFilename
            _pfxPassword = pfxPassword
        End Sub

        ''' <summary>
        ''' Instantiates the TCP client without SSL. 
        ''' Set the Connected, Disconnected, and DataReceived callbacks. Once set, use Connect() to connect to the server.
        ''' </summary>
        ''' <param name="serverIpOrHostname">The server IP address or hostname.</param>
        ''' <param name="port">The TCP port on which to connect.</param>
        Public Sub New(serverIpOrHostname As String, port As Integer)
            If String.IsNullOrEmpty(serverIpOrHostname) Then Throw New ArgumentNullException(NameOf(serverIpOrHostname))
            If port < 0 Then Throw New ArgumentException("Port must be zero or greater.")

            _serverIp = serverIpOrHostname
            _serverPort = port

            If Not IPAddress.TryParse(_serverIp, _ipAddress) Then
                _ipAddress = Dns.GetHostEntry(serverIpOrHostname).AddressList(0)
                _serverIp = _ipAddress.ToString()
            End If
        End Sub

        ''' <summary>
        ''' Instantiates the TCP client.  
        ''' Set the Connected, Disconnected, and DataReceived callbacks.  Once set, use Connect() to connect to the server.
        ''' </summary>
        ''' <param name="serverIpOrHostname">The server IP address or hostname.</param>
        ''' <param name="port">The TCP port on which to connect.</param>
        ''' <param name="ssl">Enable or disable SSL.</param>
        ''' <param name="pfxCertFilename">The filename of the PFX certificate file.</param>
        ''' <param name="pfxPassword">The password to the PFX certificate file.</param>
        Public Sub New(serverIpOrHostname As String, port As Integer, ssl As Boolean, pfxCertFilename As String, pfxPassword As String)
            Me.New(serverIpOrHostname, port)
            _ssl = ssl
            _pfxCertFilename = pfxCertFilename
            _pfxPassword = pfxPassword
        End Sub

        ''' <summary>
        ''' Instantiates the TCP client with SSL.  
        ''' Set the Connected, Disconnected, and DataReceived callbacks.  Once set, use Connect() to connect to the server.
        ''' </summary>
        ''' <param name="serverIpOrHostname">The server IP address or hostname.</param>
        ''' <param name="port">The TCP port on which to connect.</param>
        ''' <param name="certificate">Certificate.</param>
        Public Sub New(serverIpOrHostname As String, port As Integer, certificate As X509Certificate2)
            Me.New(serverIpOrHostname, port)
            If certificate Is Nothing Then Throw New ArgumentNullException(NameOf(certificate))
            _ssl = True
            _sslCert = certificate
        End Sub

        ''' <summary>
        ''' Instantiates the TCP client with SSL.  
        ''' Set the Connected, Disconnected, and DataReceived callbacks.  Once set, use Connect() to connect to the server.
        ''' </summary>
        ''' <param name="serverIpOrHostname">The server IP address or hostname.</param>
        ''' <param name="port">The TCP port on which to connect.</param>
        ''' <param name="certificate">Byte array containing the certificate.</param>
        Public Sub New(serverIpOrHostname As String, port As Integer, certificate As Byte())
            Me.New(serverIpOrHostname, port)
            If certificate Is Nothing Then Throw New ArgumentNullException(NameOf(certificate))
            _ssl = True
            _sslCert = New X509Certificate2(certificate)
        End Sub

        ''' <summary>
        ''' Instantiates the TCP client without SSL.  
        ''' Set the Connected, Disconnected, and DataReceived callbacks.  Once set, use Connect() to connect to the server.
        ''' </summary>
        ''' <param name="serverIpAddress">The server IP address.</param>
        ''' <param name="port">The TCP port on which to connect.</param>
        Public Sub New(serverIpAddress As IPAddress, port As Integer)
            Me.New(New System.Net.IPEndPoint(serverIpAddress, port))
        End Sub

        ''' <summary>
        ''' Instantiates the TCP client.  
        ''' Set the Connected, Disconnected, and DataReceived callbacks.  Once set, use Connect() to connect to the server.
        ''' </summary>
        ''' <param name="serverIpAddress">The server IP address.</param>
        ''' <param name="port">The TCP port on which to connect.</param>
        ''' <param name="ssl">Enable or disable SSL.</param>
        ''' <param name="pfxCertFilename">The filename of the PFX certificate file.</param>
        ''' <param name="pfxPassword">The password to the PFX certificate file.</param>
        Public Sub New(serverIpAddress As IPAddress, port As Integer, ssl As Boolean, pfxCertFilename As String, pfxPassword As String)
            Me.New(serverIpAddress, port)
            _ssl = ssl
            _pfxCertFilename = pfxCertFilename
            _pfxPassword = pfxPassword
        End Sub

        ''' <summary>
        ''' Instantiates the TCP client with SSL.  
        ''' Set the Connected, Disconnected, and DataReceived callbacks.  Once set, use Connect() to connect to the server.
        ''' </summary>
        ''' <param name="serverIpAddress">The server IP address.</param>
        ''' <param name="port">The TCP port on which to connect.</param>
        ''' <param name="certificate">Certificate.</param>
        Public Sub New(serverIpAddress As IPAddress, port As Integer, certificate As X509Certificate2)
            Me.New(serverIpAddress, port)
            If certificate Is Nothing Then Throw New ArgumentNullException(NameOf(certificate))
            _ssl = True
            _sslCert = certificate
        End Sub

        ''' <summary>
        ''' Instantiates the TCP client with SSL.  
        ''' Set the Connected, Disconnected, and DataReceived callbacks.  Once set, use Connect() to connect to the server.
        ''' </summary>
        ''' <param name="serverIpAddress">The server IP address.</param>
        ''' <param name="port">The TCP port on which to connect.</param>
        ''' <param name="certificate">Byte array containing the certificate.</param>
        Public Sub New(serverIpAddress As IPAddress, port As Integer, certificate As Byte())
            Me.New(serverIpAddress, port)
            If certificate Is Nothing Then Throw New ArgumentNullException(NameOf(certificate))
            _ssl = True
            _sslCert = New X509Certificate2(certificate)
            _sslCertCollection = New X509Certificate2Collection From {
                _sslCert
            }
        End Sub

        ''' <summary>
        ''' Instantiates the TCP client without SSL.  
        ''' Set the Connected, Disconnected, and DataReceived callbacks.  Once set, use Connect() to connect to the server.
        ''' </summary>
        ''' <param name="serverIpEndPoint">The server IP endpoint.</param>
        Public Sub New(serverIpEndPoint As System.Net.IPEndPoint)
            If serverIpEndPoint Is Nothing Then
                Throw New ArgumentNullException(NameOf(serverIpEndPoint))
            ElseIf serverIpEndPoint.Port < 0 Then
                Throw New ArgumentException("Port must be zero or greater.")
            Else
                _ipAddress = serverIpEndPoint.Address
                _serverIp = serverIpEndPoint.Address.ToString()
                _serverPort = serverIpEndPoint.Port
            End If
        End Sub

        ''' <summary>
        ''' Instantiates the TCP client.  
        ''' Set the Connected, Disconnected, and DataReceived callbacks.  Once set, use Connect() to connect to the server.
        ''' </summary>
        ''' <param name="serverIpEndPoint">The server IP endpoint.</param>
        ''' <param name="ssl">Enable or disable SSL.</param>
        ''' <param name="pfxCertFilename">The filename of the PFX certificate file.</param>
        ''' <param name="pfxPassword">The password to the PFX certificate file.</param>
        Public Sub New(serverIpEndPoint As IPEndPoint, ssl As Boolean, pfxCertFilename As String, pfxPassword As String)
            Me.New(serverIpEndPoint)
            _ssl = ssl
            _pfxCertFilename = pfxCertFilename
            _pfxPassword = pfxPassword
        End Sub

        ''' <summary>
        ''' Instantiates the TCP client with SSL.  
        ''' Set the Connected, Disconnected, and DataReceived callbacks.  Once set, use Connect() to connect to the server.
        ''' </summary>
        ''' <param name="serverIpEndPoint">The server IP endpoint.</param>
        ''' <param name="certificate">Certificate.</param>
        Public Sub New(serverIpEndPoint As IPEndPoint, certificate As X509Certificate2)
            Me.New(serverIpEndPoint)
            If certificate Is Nothing Then Throw New ArgumentNullException(NameOf(certificate))
            _ssl = True
            _sslCert = certificate
        End Sub

        ''' <summary>
        ''' Instantiates the TCP client with SSL.  
        ''' Set the Connected, Disconnected, and DataReceived callbacks.  Once set, use Connect() to connect to the server.
        ''' </summary>
        ''' <param name="serverIpEndPoint">The server IP endpoint.</param>
        ''' <param name="certificate">Byte array containing the certificate.</param>
        Public Sub New(serverIpEndPoint As IPEndPoint, certificate As Byte())
            Me.New(serverIpEndPoint)
            If certificate Is Nothing Then Throw New ArgumentNullException(NameOf(certificate))
            _ssl = True
            _sslCert = New X509Certificate2(certificate)
        End Sub

#End Region

#Region "Public-Methods"

        ''' <summary>
        ''' Dispose of the TCP client.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        ''' <summary>
        ''' Establish a connection to the server.
        ''' </summary>
        Public Sub Connect()
            If IsConnected Then
                Logger?.Invoke($"{_header}already connected")
                Return
            Else
                Logger?.Invoke($"{_header}initializing client")
                InitializeClient(_ssl, _pfxCertFilename, _pfxPassword, _sslCert)
                Logger?.Invoke($"{_header}connecting to {ServerIpPort}")
            End If

            _tokenSource = New CancellationTokenSource()
            _token = _tokenSource.Token
            _token.Register(Sub()
                                If Not _ssl Then
                                    If _sslStream Is Nothing Then Return
                                    _sslStream.Close()
                                Else
                                    If _networkStream Is Nothing Then Return
                                    _networkStream.Close()
                                End If
                            End Sub)

            If Not String.IsNullOrEmpty(_pfxCertFilename) Then
                If String.IsNullOrEmpty(_pfxPassword) Then _sslCert = New X509Certificate2(_pfxCertFilename)
                _sslCert = New X509Certificate2(_pfxCertFilename, _pfxPassword)
                _sslCertCollection = New X509Certificate2Collection From {
                    _sslCert
                }
            End If

            Dim ar = _client.BeginConnect(_serverIp, _serverPort, Nothing, Nothing)
            Dim wh = ar.AsyncWaitHandle

            Try
                If Not ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(_settings.ConnectTimeoutMs), False) Then
                    _client.Close()
                    Throw New TimeoutException($"Timeout connecting to {ServerIpPort}")
                End If

                _client.EndConnect(ar)
                _networkStream = _client.GetStream()
                _networkStream.ReadTimeout = _settings.ReadTimeoutMs

                If _ssl Then
                    If _settings.AcceptInvalidCertificates Then
                        _sslStream = New SslStream(_networkStream, False, New RemoteCertificateValidationCallback(AddressOf AcceptCertificate))
                    ElseIf _settings.CertificateValidationCallback IsNot Nothing Then
                        _sslStream = New SslStream(_networkStream, False, _settings.CertificateValidationCallback)
                    Else
                        _sslStream = New SslStream(_networkStream, False)
                    End If

                    _sslStream.ReadTimeout = _settings.ReadTimeoutMs
                    _sslStream.AuthenticateAsClient(_serverIp, _sslCertCollection, SslProtocols.Tls12, _settings.CheckCertificateRevocation)

                    If Not _sslStream.IsEncrypted Then Throw New AuthenticationException("Stream is not encrypted")
                    If Not _sslStream.IsAuthenticated Then Throw New AuthenticationException("Stream is not authenticated")
                    If _settings.MutuallyAuthenticate AndAlso Not _sslStream.IsMutuallyAuthenticated Then Throw New AuthenticationException("Mutual authentication failed")
                End If

                If _keepalive.EnableTcpKeepAlives Then EnableKeepalives()
            Catch __unusedException1__ As Exception
                Throw
            End Try

            _isConnected = True
            _lastActivity = Date.Now
            _isTimeout = False
            _events.HandleConnected(Me, New ConnectionEventArgs(ServerIpPort))
            _dataReceiver = Task.Run(Function() DataReceiver(_token), _token)
            _idleServerMonitor = Task.Run(New Func(Of Task)(AddressOf IdleServerMonitor), _token)
            _connectionMonitor = Task.Run(New Func(Of Task)(AddressOf ConnectedMonitor), _token)
        End Sub

        ''' <summary>
        ''' Establish the connection to the server with retries up to either the timeout specified or the value in Settings.ConnectTimeoutMs.
        ''' </summary>
        ''' <param name="timeoutMs">The amount of time in milliseconds to continue attempting connections.</param>
        Public Sub ConnectWithRetries(Optional timeoutMs As Integer? = Nothing)
            If timeoutMs IsNot Nothing AndAlso timeoutMs < 1 Then Throw New ArgumentException("Timeout milliseconds must be greater than zero.")
            If timeoutMs IsNot Nothing Then _settings.ConnectTimeoutMs = timeoutMs.Value

            If IsConnected Then
                Logger?.Invoke($"{_header}already connected")
                Return
            Else
                Logger?.Invoke($"{_header}initializing client")

                InitializeClient(_ssl, _pfxCertFilename, _pfxPassword, _sslCert)

                Logger?.Invoke($"{_header}connecting to {ServerIpPort}")
            End If

            _tokenSource = New CancellationTokenSource()
            _token = _tokenSource.Token
            _token.Register(Sub()
                                If Not _ssl Then
                                    If _sslStream Is Nothing Then Return
                                    _sslStream.Close()
                                Else
                                    If _networkStream Is Nothing Then Return
                                    _networkStream.Close()
                                End If
                            End Sub)


            Using connectTokenSource As CancellationTokenSource = New CancellationTokenSource()
                Dim connectToken = connectTokenSource.Token

                Dim cancelTask = Task.Delay(_settings.ConnectTimeoutMs, _token)
                Dim connectTask As Task = Task.Run(Sub()
                                                       Dim retryCount = 0

                                                       While True
                                                           Try
                                                               Dim msg = $"{_header}attempting connection to {_serverIp}:{_serverPort}"
                                                               If retryCount > 0 Then msg += $" ({retryCount} retries)"
                                                               Logger?.Invoke(msg)

                                                               _client.Dispose()
                                                               _client = If(_settings.LocalEndpoint Is Nothing, New TcpClient(), New TcpClient(_settings.LocalEndpoint))
                                                               _client.NoDelay = _settings.NoDelay
                                                               _client.ConnectAsync(_serverIp, _serverPort).Wait(1000, connectToken)

                                                               If _client.Connected Then
                                                                   Logger?.Invoke($"{_header}connected to {_serverIp}:{_serverPort}")
                                                                   Exit While
                                                               End If
                                                           Catch __unusedTaskCanceledException1__ As TaskCanceledException
                                                               Exit While
                                                           Catch __unusedOperationCanceledException2__ As OperationCanceledException
                                                               Exit While
                                                           Catch e As Exception
                                                               Logger?.Invoke($"{_header}failed connecting to {_serverIp}:{_serverPort}: {e.Message}")
                                                           Finally
                                                               retryCount += 1
                                                           End Try
                                                       End While
                                                   End Sub, connectToken)

                Call Task.WhenAny(cancelTask, connectTask).Wait()

                If cancelTask.IsCompleted Then
                    connectTokenSource.Cancel()
                    _client.Close()
                    Throw New TimeoutException($"Timeout connecting to {ServerIpPort}")
                End If

                Try
                    _networkStream = _client.GetStream()
                    _networkStream.ReadTimeout = _settings.ReadTimeoutMs

                    If _ssl Then
                        If _settings.AcceptInvalidCertificates Then
                            _sslStream = New SslStream(_networkStream, False, New RemoteCertificateValidationCallback(AddressOf AcceptCertificate))
                        ElseIf _settings.CertificateValidationCallback IsNot Nothing Then
                            _sslStream = New SslStream(_networkStream, False, _settings.CertificateValidationCallback)
                        Else
                            _sslStream = New SslStream(_networkStream, False)
                        End If

                        _sslStream.ReadTimeout = _settings.ReadTimeoutMs
                        _sslStream.AuthenticateAsClient(_serverIp, _sslCertCollection, SslProtocols.Tls12, _settings.CheckCertificateRevocation)

                        If Not _sslStream.IsEncrypted Then Throw New AuthenticationException("Stream is not encrypted")
                        If Not _sslStream.IsAuthenticated Then Throw New AuthenticationException("Stream is not authenticated")
                        If _settings.MutuallyAuthenticate AndAlso Not _sslStream.IsMutuallyAuthenticated Then Throw New AuthenticationException("Mutual authentication failed")
                    End If

                    If _keepalive.EnableTcpKeepAlives Then EnableKeepalives()
                Catch __unusedException1__ As Exception
                    Throw
                End Try

            End Using

            _isConnected = True
            _lastActivity = Date.Now
            _isTimeout = False
            _events.HandleConnected(Me, New ConnectionEventArgs(ServerIpPort))
            _dataReceiver = Task.Run(Function() DataReceiver(_token), _token)
            _idleServerMonitor = Task.Run(New Func(Of Task)(AddressOf IdleServerMonitor), _token)
            _connectionMonitor = Task.Run(New Func(Of Task)(AddressOf ConnectedMonitor), _token)
        End Sub

        ''' <summary>
        ''' Disconnect from the server.
        ''' </summary>
        Public Sub Disconnect()
            If Not IsConnected Then
                Logger?.Invoke($"{_header}already disconnected")
                Return
            End If

            Logger?.Invoke($"{_header}disconnecting from {ServerIpPort}")

            _tokenSource.Cancel()
            WaitCompletion()
            _client.Close()
            _isConnected = False
        End Sub

        ''' <summary>
        ''' Disconnect from the server.
        ''' </summary>
        Public Async Function DisconnectAsync() As Task
            If Not IsConnected Then
                Logger?.Invoke($"{_header}already disconnected")
                Return
            End If

            Logger?.Invoke($"{_header}disconnecting from {ServerIpPort}")

            _tokenSource.Cancel()
            Await WaitCompletionAsync()
            _client.Close()
            _isConnected = False
        End Function

        ''' <summary>
        ''' Send data to the server.
        ''' </summary>
        ''' <param name="data">String containing data to send.</param>
        Public Sub Send(data As String)
            If String.IsNullOrEmpty(data) Then Throw New ArgumentNullException(NameOf(data))
            If Not _isConnected Then Throw New IOException("Not connected to the server; use Connect() first.")

            Dim bytes = Encoding.UTF8.GetBytes(data)
            Send(bytes)
        End Sub

        ''' <summary>
        ''' Send data to the server.
        ''' </summary> 
        ''' <param name="data">Byte array containing data to send.</param>
        Public Sub Send(data As Byte())
            If data Is Nothing OrElse data.Length < 1 Then Throw New ArgumentNullException(NameOf(data))
            If Not _isConnected Then Throw New IOException("Not connected to the server; use Connect() first.")

            Using ms As MemoryStream = New MemoryStream()
                ms.Write(data, 0, data.Length)
                ms.Seek(0, SeekOrigin.Begin)
                SendInternal(data.Length, ms)
            End Using
        End Sub

        ''' <summary>
        ''' Send data to the server.
        ''' </summary>
        ''' <param name="contentLength">The number of bytes to read from the source stream to send.</param>
        ''' <param name="stream">Stream containing the data to send.</param>
        Public Sub Send(contentLength As Long, stream As Stream)
            If contentLength < 1 Then Return
            If stream Is Nothing Then Throw New ArgumentNullException(NameOf(stream))
            If Not stream.CanRead Then Throw New InvalidOperationException("Cannot read from supplied stream.")
            If Not _isConnected Then Throw New IOException("Not connected to the server; use Connect() first.")

            SendInternal(contentLength, stream)
        End Sub

        ''' <summary>
        ''' Send data to the server asynchronously.
        ''' </summary>
        ''' <param name="data">String containing data to send.</param>
        ''' <param name="token">Cancellation token for canceling the request.</param>
        Public Async Function SendAsync(data As String, Optional token As CancellationToken = Nothing) As Task
            If String.IsNullOrEmpty(data) Then Throw New ArgumentNullException(NameOf(data))
            If Not _isConnected Then Throw New IOException("Not connected to the server; use Connect() first.")
            If token = Nothing Then token = _token

            Dim bytes = Encoding.UTF8.GetBytes(data)

            Using ms As MemoryStream = New MemoryStream()
                Await ms.WriteAsync(bytes, 0, bytes.Length, token).ConfigureAwait(False)
                ms.Seek(0, SeekOrigin.Begin)
                Await SendInternalAsync(bytes.Length, ms, token).ConfigureAwait(False)
            End Using
        End Function

        ''' <summary>
        ''' Send data to the server asynchronously.
        ''' </summary> 
        ''' <param name="data">Byte array containing data to send.</param>
        ''' <param name="token">Cancellation token for canceling the request.</param>
        Public Async Function SendAsync(data As Byte(), Optional token As CancellationToken = Nothing) As Task
            If data Is Nothing OrElse data.Length < 1 Then Throw New ArgumentNullException(NameOf(data))
            If Not _isConnected Then Throw New IOException("Not connected to the server; use Connect() first.")
            If token = Nothing Then token = _token

            Using ms As MemoryStream = New MemoryStream()
                Await ms.WriteAsync(data, 0, data.Length, token).ConfigureAwait(False)
                ms.Seek(0, SeekOrigin.Begin)
                Await SendInternalAsync(data.Length, ms, token).ConfigureAwait(False)
            End Using
        End Function

        ''' <summary>
        ''' Send data to the server asynchronously.
        ''' </summary>
        ''' <param name="contentLength">The number of bytes to read from the source stream to send.</param>
        ''' <param name="stream">Stream containing the data to send.</param>
        ''' <param name="token">Cancellation token for canceling the request.</param>
        Public Async Function SendAsync(contentLength As Long, stream As Stream, Optional token As CancellationToken = Nothing) As Task
            If contentLength < 1 Then Return
            If stream Is Nothing Then Throw New ArgumentNullException(NameOf(stream))
            If Not stream.CanRead Then Throw New InvalidOperationException("Cannot read from supplied stream.")
            If Not _isConnected Then Throw New IOException("Not connected to the server; use Connect() first.")
            If token = Nothing Then token = _token

            Await SendInternalAsync(contentLength, stream, token).ConfigureAwait(False)
        End Function

#End Region

#Region "Private-Methods"

        ''' <summary>
        ''' Dispose of the TCP client.
        ''' </summary>
        ''' <param name="disposing">Dispose of resources.</param>
        Protected Overridable Sub Dispose(disposing As Boolean)
            If disposing Then
                _isConnected = False

                If _tokenSource IsNot Nothing Then
                    If Not _tokenSource.IsCancellationRequested Then
                        _tokenSource.Cancel()
                        _tokenSource.Dispose()
                    End If
                End If

                If _sslStream IsNot Nothing Then
                    _sslStream.Close()
                    _sslStream.Dispose()
                End If

                If _networkStream IsNot Nothing Then
                    _networkStream.Close()
                    _networkStream.Dispose()
                End If

                If _client IsNot Nothing Then
                    _client.Close()
                    _client.Dispose()
                End If

                Logger?.Invoke($"{_header}dispose complete")
            End If
        End Sub

        Private Sub InitializeClient(ssl As Boolean, pfxCertFilename As String, pfxPassword As String, sslCert As X509Certificate2)
            _ssl = ssl
            _pfxCertFilename = pfxCertFilename
            _pfxPassword = pfxPassword

            _client = If(_settings.LocalEndpoint Is Nothing, New TcpClient(), New TcpClient(_settings.LocalEndpoint))
            _client.NoDelay = _settings.NoDelay

            _sslStream = Nothing
            _sslCert = Nothing
            _sslCertCollection = Nothing

            If _ssl Then
                If sslCert IsNot Nothing Then
                    _sslCert = sslCert
                ElseIf String.IsNullOrEmpty(pfxPassword) Then
                    _sslCert = New X509Certificate2(pfxCertFilename)
                Else
                    _sslCert = New X509Certificate2(pfxCertFilename, pfxPassword)
                End If

                _sslCertCollection = New X509Certificate2Collection From {
    _sslCert
}
            End If
        End Sub

        Private Function AcceptCertificate(sender As Object, certificate As X509Certificate, chain As X509Chain, sslPolicyErrors As SslPolicyErrors) As Boolean
            Return _settings.AcceptInvalidCertificates
        End Function

        Private Async Function DataReceiver(token As CancellationToken) As Task
            Dim outerStream As Stream = Nothing
            If Not _ssl Then
                outerStream = _networkStream
            Else
                outerStream = _sslStream
            End If

            While Not token.IsCancellationRequested AndAlso _client IsNot Nothing AndAlso _client.Connected
                Try
                    Await DataReadAsync(CType(token, CancellationToken)) _
                        .ContinueWith(Of Task(Of Global.System.ArraySegment(Of Global.System.[Byte])))(CType(Async Function(task)
                                                                                                                 If task.IsCanceled Then Return DirectCast(Nothing, ArraySegment(Of Byte))
                                                                                                                 Dim data As ArraySegment(Of Byte) = task.Result

                                                                                                                 If data.Array IsNot Nothing Then
                                                                                                                     _lastActivity = Date.Now

                                                                                                                     Dim action As Action = Sub() _events.HandleDataReceived(CObj(Me), CType(New DataReceivedEventArgs(CStr(ServerIpPort), CType(data, ArraySegment(Of Byte))), DataReceivedEventArgs))
                                                                                                                     If _settings.UseAsyncDataReceivedEvents Then
                                                                                                                         Call Tasks.Task.Run(CType(action, Action), CType(token, CancellationToken))
                                                                                                                     Else
                                                                                                                         action.Invoke()
                                                                                                                     End If

                                                                                                                     _statistics.ReceivedBytes += data.Count

                                                                                                                     Return data
                                                                                                                 Else
                                                                                                                     Await Tasks.Task.Delay(CInt(100)).ConfigureAwait(CBool(False))
                                                                                                                     Return DirectCast(Nothing, ArraySegment(Of Byte))
                                                                                                                 End If

                                                                                                             End Function, Func(Of Task(Of ArraySegment(Of Byte)), Task(Of ArraySegment(Of Byte)))), CType(token, CancellationToken)).ContinueWith(CType(Sub(task)
                                                                                                                                                                                                                                                         End Sub, Action(Of Task(Of Task(Of ArraySegment(Of Byte)))))).ConfigureAwait(False)
                Catch __unusedAggregateException1__ As AggregateException
                    Logger?.Invoke($"{_header}data receiver canceled, disconnected")
                    Exit While
                Catch __unusedIOException2__ As IOException
                    Logger?.Invoke($"{_header}data receiver canceled, disconnected")
                    Exit While
                Catch __unusedSocketException3__ As SocketException
                    Logger?.Invoke($"{_header}data receiver canceled, disconnected")
                    Exit While
                Catch __unusedTaskCanceledException4__ As TaskCanceledException
                    Logger?.Invoke($"{_header}data receiver task canceled, disconnected")
                    Exit While
                Catch __unusedOperationCanceledException5__ As OperationCanceledException
                    Logger?.Invoke($"{_header}data receiver operation canceled, disconnected")
                    Exit While
                Catch __unusedObjectDisposedException6__ As ObjectDisposedException
                    Logger?.Invoke($"{_header}data receiver canceled due to disposal, disconnected")
                    Exit While
                Catch e As Exception
                    Logger?.Invoke($"{_header}data receiver exception:{Environment.NewLine}{e}{Environment.NewLine}")
                    Exit While
                End Try
            End While

            Logger?.Invoke($"{_header}disconnection detected")

            _isConnected = False

            If Not _isTimeout Then
                _events.HandleClientDisconnected(Me, New ConnectionEventArgs(ServerIpPort, DisconnectReason.Normal))
            Else
                _events.HandleClientDisconnected(Me, New ConnectionEventArgs(ServerIpPort, DisconnectReason.Timeout))
            End If

            Dispose()
        End Function

        Private Async Function DataReadAsync(token As CancellationToken) As Task(Of ArraySegment(Of Byte))
            Dim buffer = New Byte(_settings.StreamBufferSize - 1) {}
            Dim read = 0

            Try
                If Not _ssl Then
                    read = Await _networkStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(False)
                Else
                    read = Await _sslStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(False)
                End If

                If read > 0 Then
                    Using ms As MemoryStream = New MemoryStream()
                        ms.Write(buffer, 0, read)
                        Return New ArraySegment(Of Byte)(ms.GetBuffer(), 0, ms.Length)
                    End Using
                Else
                    Dim ipProperties As IPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties()
                    Dim tcpConnections As TcpConnectionInformation() = ipProperties.GetActiveTcpConnections().Where(Function(x) x.LocalEndPoint.Equals(_client.Client.LocalEndPoint) AndAlso x.RemoteEndPoint.Equals(_client.Client.RemoteEndPoint)).ToArray()

                    Dim isOk = False

                    If tcpConnections IsNot Nothing AndAlso tcpConnections.Length > 0 Then
                        Dim stateOfConnection = Enumerable.First(tcpConnections).State
                        If stateOfConnection = TcpState.Established Then
                            isOk = True
                        End If
                    End If

                    If Not isOk Then
                        Await DisconnectAsync()
                    End If

                    Throw New SocketException()
                End If
            Catch __unusedIOException1__ As IOException
                ' thrown if ReadTimeout (ms) is exceeded
                ' see https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.networkstream.readtimeout?view=net-6.0
                ' and https://github.com/dotnet/runtime/issues/24093
                Return Nothing
            End Try
        End Function

        Private Sub SendInternal(contentLength As Long, stream As Stream)
            Dim bytesRemaining = contentLength
            Dim bytesRead = 0
            Dim buffer = New Byte(_settings.StreamBufferSize - 1) {}

            Try
                _sendLock.Wait()

                While bytesRemaining > 0
                    bytesRead = stream.Read(buffer, 0, buffer.Length)
                    If bytesRead > 0 Then
                        If Not _ssl Then
                            _networkStream.Write(buffer, 0, bytesRead)
                        Else
                            _sslStream.Write(buffer, 0, bytesRead)
                        End If

                        bytesRemaining -= bytesRead
                        _statistics.SentBytes += bytesRead
                    End If
                End While

                If Not _ssl Then
                    _networkStream.Flush()
                Else
                    _sslStream.Flush()
                End If
                _events.HandleDataSent(Me, New DataSentEventArgs(ServerIpPort, contentLength))
            Finally
                _sendLock.Release()
            End Try
        End Sub

        Private Async Function SendInternalAsync(contentLength As Long, stream As Stream, token As CancellationToken) As Task
            Try
                Dim bytesRemaining = contentLength
                Dim bytesRead = 0
                Dim buffer = New Byte(_settings.StreamBufferSize - 1) {}

                Await _sendLock.WaitAsync(token).ConfigureAwait(False)

                While bytesRemaining > 0
                    bytesRead = Await stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(False)
                    If bytesRead > 0 Then
                        If Not _ssl Then
                            Await _networkStream.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(False)
                        Else
                            Await _sslStream.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(False)
                        End If

                        bytesRemaining -= bytesRead
                        _statistics.SentBytes += bytesRead
                    End If
                End While

                If Not _ssl Then
                    Await _networkStream.FlushAsync(token).ConfigureAwait(False)
                Else
                    Await _sslStream.FlushAsync(token).ConfigureAwait(False)
                End If
                _events.HandleDataSent(Me, New DataSentEventArgs(ServerIpPort, contentLength))
            Catch __unusedTaskCanceledException1__ As TaskCanceledException
            Catch __unusedOperationCanceledException2__ As OperationCanceledException
            Finally
                _sendLock.Release()
            End Try
        End Function

        Private Sub WaitCompletion()
            Try
                _dataReceiver.Wait()
            Catch ex As AggregateException When TypeOf ex.InnerException Is TaskCanceledException
                Logger?.Invoke("Awaiting a canceled task")
            End Try
        End Sub

        Private Async Function WaitCompletionAsync() As Task
            Try
                Await _dataReceiver
            Catch __unusedTaskCanceledException1__ As TaskCanceledException
                Logger?.Invoke("Awaiting a canceled task")
            End Try
        End Function

        Private Sub EnableKeepalives()
            ' issues with definitions: https://github.com/dotnet/sdk/issues/14540

            Try
#If NETCOREAPP3_1_OR_GREATER Or NET6_0_OR_GREATER Then
                ' NETCOREAPP3_1_OR_GREATER catches .NET 5.0

                _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, True)
                _client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, _keepalive.TcpKeepAliveTime)
                _client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, _keepalive.TcpKeepAliveInterval)

                ' Windows 10 version 1703 or later

                If RuntimeInformation.IsOSPlatform(OSPlatform.Windows) AndAlso Environment.OSVersion.Version >= New Version(10, 0, 15063) Then
                    _client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, _keepalive.TcpKeepAliveRetryCount)
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
                _client.Client.IOControl(IOControlCode.KeepAliveValues, keepAlive, Nothing)
#End If
            Catch __unusedException1__ As Exception
                Logger?.Invoke($"{_header}keepalives not supported on this platform, disabled")
                _keepalive.EnableTcpKeepAlives = False
            End Try
        End Sub

        Private Async Function IdleServerMonitor() As Task
            While Not _token.IsCancellationRequested
                Await Task.Delay(_settings.IdleServerEvaluationIntervalMs, _token).ConfigureAwait(False)

                If _settings.IdleServerTimeoutMs = 0 Then Continue While

                Dim timeoutTime = _lastActivity.AddMilliseconds(_settings.IdleServerTimeoutMs)

                If Date.Now > timeoutTime Then
                    Logger?.Invoke($"{_header}disconnecting from {ServerIpPort} due to timeout")
                    _isConnected = False
                    _isTimeout = True
                    _tokenSource.Cancel() ' DataReceiver will fire events including dispose
                End If
            End While
        End Function

        Private Async Function ConnectedMonitor() As Task
            While Not _token.IsCancellationRequested
                Await Task.Delay(_settings.ConnectionLostEvaluationIntervalMs, _token).ConfigureAwait(False)

                If Not _isConnected Then Continue While 'Just monitor connected clients

                If Not PollSocket() Then
                    Logger?.Invoke($"{_header}disconnecting from {ServerIpPort} due to connection lost")
                    _isConnected = False
                    _tokenSource.Cancel() ' DataReceiver will fire events including dispose
                End If
            End While
        End Function

        Private Function PollSocket() As Boolean
            Try
                If _client.Client Is Nothing OrElse Not _client.Client.Connected Then Return False

                ' pear to the documentation on Poll:
                ' When passing SelectMode.SelectRead as a parameter to the Poll method it will return 
                ' -either- true if Socket.Listen(Int32) has been called and a connection is pending;
                ' -or- true if data is available for reading; 
                ' -or- true if the connection has been closed, reset, or terminated; 
                ' otherwise, returns false

                If Not _client.Client.Poll(0, SelectMode.SelectRead) Then Return True

                Dim buff = New Byte(0) {}
                Dim clientSentData = _client.Client.Receive(buff, SocketFlags.Peek) <> 0
                Return clientSentData 'False here though Poll() succeeded means we had a disconnect!
            Catch ex As SocketException
                Logger?.Invoke($"{_header}poll socket from {ServerIpPort} failed with ex = {ex}")
                Return ex.SocketErrorCode = SocketError.TimedOut
            Catch __unusedException2__ As Exception
                Return False
            End Try
        End Function

#End Region
    End Class
End Namespace
