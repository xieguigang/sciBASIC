Imports System

Namespace SuperSimpleTcp
    ''' <summary>
    ''' SimpleTcp keepalive settings.
    ''' Keepalive probes are sent after an idle period defined by TcpKeepAliveTime (seconds).
    ''' Should a keepalive response not be received within TcpKeepAliveInterval (seconds), a subsequent keepalive probe will be sent.
    ''' For .NET Framework, should 10 keepalive probes fail, the connection will terminate.
    ''' For .NET Core, should a number of probes fail as specified in TcpKeepAliveRetryCount, the connection will terminate.
    ''' TCP keepalives are not supported in .NET Standard.
    ''' </summary>
    Public Class SimpleTcpKeepaliveSettings
#Region "Public-Members"

        ''' <summary>
        ''' Enable or disable TCP-based keepalive probes.
        ''' TCP keepalives are only supported in .NET Core and .NET Framework projects.  .NET Standard does not provide facilities to support TCP keepalives.
        ''' </summary>
        Public EnableTcpKeepAlives As Boolean = False

        ''' <summary>
        ''' TCP keepalive interval, i.e. the number of seconds a TCP connection will wait for a keepalive response before sending another keepalive probe.
        ''' Default is 5 seconds.  Value must be greater than zero.
        ''' </summary>
        Public Property TcpKeepAliveInterval As Integer
            Get
                Return _tcpKeepAliveInterval
            End Get
            Set(value As Integer)
                If value < 1 Then Throw New ArgumentException("TcpKeepAliveInterval must be greater than zero.")
                _tcpKeepAliveInterval = value
            End Set
        End Property

        ''' <summary>
        ''' TCP keepalive time, i.e. the number of seconds a TCP connection will remain alive/idle before keepalive probes are sent to the remote. 
        ''' Default is 5 seconds.  Value must be greater than zero.
        ''' </summary>
        Public Property TcpKeepAliveTime As Integer
            Get
                Return _tcpKeepAliveTime
            End Get
            Set(value As Integer)
                If value < 1 Then Throw New ArgumentException("TcpKeepAliveTime must be greater than zero.")
                _tcpKeepAliveTime = value
            End Set
        End Property

        ''' <summary>
        ''' TCP keepalive retry count, i.e. the number of times a TCP probe will be sent in effort to verify the connection.
        ''' After the specified number of probes fail, the connection will be terminated.
        ''' </summary>
        Public Property TcpKeepAliveRetryCount As Integer
            Get
                Return _tcpKeepAliveRetryCount
            End Get
            Set(value As Integer)
                If value < 1 Then Throw New ArgumentException("TcpKeepAliveRetryCount must be greater than zero.")
                _tcpKeepAliveRetryCount = value
            End Set
        End Property

#End Region

#Region "Internal-Members"

        Friend ReadOnly Property TcpKeepAliveIntervalMilliseconds As Integer
            Get
                Return TcpKeepAliveInterval * 1000
            End Get
        End Property

        Friend ReadOnly Property TcpKeepAliveTimeMilliseconds As Integer
            Get
                Return TcpKeepAliveTime * 1000
            End Get
        End Property

#End Region

#Region "Private-Members"

        Private _tcpKeepAliveInterval As Integer = 2
        Private _tcpKeepAliveTime As Integer = 2
        Private _tcpKeepAliveRetryCount As Integer = 3

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
