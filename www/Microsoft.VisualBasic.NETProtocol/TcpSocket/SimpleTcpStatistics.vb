Imports System

Namespace SuperSimpleTcp
    ''' <summary>
    ''' SimpleTcp statistics.
    ''' </summary>
    Public Class SimpleTcpStatistics
#Region "Public-Members"

        ''' <summary>
        ''' The time at which the client or server was started.
        ''' </summary>
        Public ReadOnly Property StartTime As Date
            Get
                Return _startTime
            End Get
        End Property

        ''' <summary>
        ''' The amount of time which the client or server has been up.
        ''' </summary>
        Public ReadOnly Property UpTime As TimeSpan
            Get
                Return Date.Now.ToUniversalTime() - _startTime
            End Get
        End Property

        ''' <summary>
        ''' The number of bytes received.
        ''' </summary>
        Public Property ReceivedBytes As Long
            Get
                Return _receivedBytes
            End Get
            Friend Set(value As Long)
                _receivedBytes = value
            End Set
        End Property

        ''' <summary>
        ''' The number of bytes sent.
        ''' </summary>
        Public Property SentBytes As Long
            Get
                Return _sentBytes
            End Get
            Friend Set(value As Long)
                _sentBytes = value
            End Set
        End Property

#End Region

#Region "Private-Members"

        Private _startTime As Date = Date.Now.ToUniversalTime()
        Private _receivedBytes As Long = 0
        Private _sentBytes As Long = 0

#End Region

#Region "Constructors-and-Factories"

        ''' <summary>
        ''' Initialize the statistics object.
        ''' </summary>
        Public Sub New()

        End Sub

#End Region

#Region "Public-Methods"

        ''' <summary>
        ''' Return human-readable version of the object.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Dim ret As String = "--- Statistics ---" & Environment.NewLine &
                "    Started        : " & _startTime.ToString() & Environment.NewLine &
                "    Uptime         : " & UpTime.ToString() & Environment.NewLine &
                "    Received bytes : " & ReceivedBytes.ToString() & Environment.NewLine &
                "    Sent bytes     : " & SentBytes.ToString() & Environment.NewLine
            Return ret
        End Function

        ''' <summary>
        ''' Reset statistics other than StartTime and UpTime.
        ''' </summary>
        Public Sub Reset()
            _receivedBytes = 0
            _sentBytes = 0
        End Sub

#End Region
    End Class
End Namespace
