Imports System
Imports System.Collections.Generic
Imports System.Net.Security

Namespace SuperSimpleTcp
    ''' <summary>
    ''' SimpleTcp server settings.
    ''' </summary>
    Public Class SimpleTcpServerSettings
#Region "Public-Members"

        ''' <summary>
        ''' Nagle's algorithm.
        ''' Gets or sets a value that disables a delay when send or receive buffers are not full.
        ''' true if the delay is disabled; otherwise, false. The default value is false.
        ''' </summary>
        Public Property NoDelay As Boolean
            Get
                Return _noDelay
            End Get
            Set(value As Boolean)
                _noDelay = value
            End Set
        End Property

        ''' <summary>
        ''' Buffer size to use while interacting with streams. 
        ''' </summary>
        Public Property StreamBufferSize As Integer
            Get
                Return _streamBufferSize
            End Get
            Set(value As Integer)
                If value < 1 Then Throw New ArgumentException("StreamBufferSize must be one or greater.")
                ' If value > 65536 Then Throw New ArgumentException("StreamBufferSize must be less than or equal to 65,536.")
                _streamBufferSize = value
            End Set
        End Property

        ''' <summary>
        ''' Maximum amount of time to wait before considering a client idle and disconnecting them. 
        ''' By default, this value is set to 0, which will never disconnect a client due to inactivity.
        ''' The timeout is reset any time a message is received from a client.
        ''' For instance, if you set this value to 30000, the client will be disconnected if the server has not received a message from the client within 30 seconds.
        ''' </summary>
        Public Property IdleClientTimeoutMs As Integer
            Get
                Return _idleClientTimeoutMs
            End Get
            Set(value As Integer)
                If value < 0 Then Throw New ArgumentException("IdleClientTimeoutMs must be zero or greater.")
                _idleClientTimeoutMs = value
            End Set
        End Property

        ''' <summary>
        ''' Maximum number of connections the server will accept.
        ''' Default is 4096.  Value must be greater than zero.
        ''' </summary>
        Public Property MaxConnections As Integer
            Get
                Return _maxConnections
            End Get
            Set(value As Integer)
                If value < 1 Then Throw New ArgumentException("Max connections must be greater than zero.")
                _maxConnections = value
            End Set
        End Property

        ''' <summary>
        ''' Number of milliseconds to wait between each iteration of evaluating connected clients to see if they have exceeded the configured timeout interval.
        ''' </summary>
        Public Property IdleClientEvaluationIntervalMs As Integer
            Get
                Return _idleClientEvaluationIntervalMs
            End Get
            Set(value As Integer)
                If value < 1 Then Throw New ArgumentOutOfRangeException("IdleClientEvaluationIntervalMs must be one or greater.")
                _idleClientEvaluationIntervalMs = value
            End Set
        End Property

        ''' <summary>
        ''' Enable or disable acceptance of invalid SSL certificates.
        ''' </summary>
        Public AcceptInvalidCertificates As Boolean = True

        ''' <summary>
        ''' Enable or disable mutual authentication of SSL client and server.
        ''' </summary>
        Public MutuallyAuthenticate As Boolean = True

        ''' <summary>
        ''' Enable or disable whether the data receiver thread fires the DataReceived event from a background task.
        ''' The default is enabled.
        ''' </summary>
        Public UseAsyncDataReceivedEvents As Boolean = True

        ''' <summary>
        ''' Enable or disable checking certificate revocation list during the validation process.
        ''' </summary>
        Public CheckCertificateRevocation As Boolean = True

        ''' <summary>
        ''' Delegate responsible for validating a certificate supplied by a remote party.
        ''' </summary>
        Public CertificateValidationCallback As RemoteCertificateValidationCallback = Nothing

        ''' <summary>
        ''' The list of permitted IP addresses from which connections can be received.
        ''' </summary>
        Public Property PermittedIPs As List(Of String)
            Get
                Return _permittedIPs
            End Get
            Set(value As List(Of String))
                If value Is Nothing Then
                    _permittedIPs = New List(Of String)()
                Else
                    _permittedIPs = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' The list of blocked IP addresses from which connections will be declined.
        ''' </summary>
        Public Property BlockedIPs As List(Of String)
            Get
                Return _blockedIPs
            End Get
            Set(value As List(Of String))
                If value Is Nothing Then
                    _blockedIPs = New List(Of String)()
                Else
                    _blockedIPs = value
                End If
            End Set
        End Property

#End Region

#Region "Private-Members"

        Private _noDelay As Boolean = True
        Private _streamBufferSize As Integer = 65536
        Private _maxConnections As Integer = 4096
        Private _idleClientTimeoutMs As Integer = 0
        Private _idleClientEvaluationIntervalMs As Integer = 5000
        Private _permittedIPs As List(Of String) = New List(Of String)()
        Private _blockedIPs As List(Of String) = New List(Of String)()

#End Region

#Region "Constructors-and-Factories"

        ''' <summary>
        ''' Instantiate the object.
        ''' </summary>
        Public Sub New()

        End Sub

#End Region

#Region "Public-Methods"

#End Region

#Region "Private-Methods"

#End Region
    End Class
End Namespace
