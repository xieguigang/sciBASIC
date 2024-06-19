Imports System
Imports System.Net.Security
Imports System.Net.Sockets
Imports System.Threading

Namespace SuperSimpleTcp
    Friend Class ClientMetadata
        Implements IDisposable
#Region "Public-Members"

        Friend ReadOnly Property Client As TcpClient
            Get
                Return _tcpClient
            End Get
        End Property

        Friend ReadOnly Property NetworkStream As NetworkStream
            Get
                Return _networkStream
            End Get
        End Property

        Friend Property SslStream As SslStream
            Get
                Return _sslStream
            End Get
            Set(value As SslStream)
                _sslStream = value
            End Set
        End Property

        Friend ReadOnly Property IpPort As String
            Get
                Return _ipPort
            End Get
        End Property

        Friend SendLock As SemaphoreSlim = New SemaphoreSlim(1, 1)
        Friend ReceiveLock As SemaphoreSlim = New SemaphoreSlim(1, 1)

        Friend Property TokenSource As CancellationTokenSource

        Friend Property Token As CancellationToken

#End Region

#Region "Private-Members"

        Private _tcpClient As TcpClient = Nothing
        Private _networkStream As NetworkStream = Nothing
        Private _sslStream As SslStream = Nothing
        Private _ipPort As String = Nothing

#End Region

#Region "Constructors-and-Factories"

        Friend Sub New(tcp As TcpClient)
            If tcp Is Nothing Then Throw New ArgumentNullException(NameOf(tcp))

            _tcpClient = tcp
            _networkStream = tcp.GetStream()
            _ipPort = tcp.Client.RemoteEndPoint.ToString()
            TokenSource = New CancellationTokenSource()
            Token = TokenSource.Token
        End Sub

#End Region

#Region "Public-Methods"

        Public Sub Dispose() Implements IDisposable.Dispose
            If TokenSource IsNot Nothing Then
                If Not TokenSource.IsCancellationRequested Then
                    TokenSource.Cancel()
                    TokenSource.Dispose()
                End If
            End If

            If _sslStream IsNot Nothing Then
                _sslStream.Close()
            End If

            If _networkStream IsNot Nothing Then
                _networkStream.Close()
            End If

            If _tcpClient IsNot Nothing Then
                _tcpClient.Close()
                _tcpClient.Dispose()
            End If

            SendLock.Dispose()
            ReceiveLock.Dispose()
        End Sub

#End Region
    End Class
End Namespace
