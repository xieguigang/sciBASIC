Namespace DownSampling


    Public Class WeightedEvent
        Implements [Event]

        Private event_Conflict As [Event]
        Private weight_Conflict As Double

        Public Sub New(time As Long, value As Double)
            Me.event_Conflict = New PlainEvent(time, value)
        End Sub

        Public Sub New(e As [Event])
            Me.event_Conflict = e
        End Sub

        Public Overridable ReadOnly Property [Event] As [Event]
            Get
                Return event_Conflict
            End Get
        End Property

        Public Overridable ReadOnly Property Time As Long Implements [Event].Time
            Get
                Return event_Conflict.Time
            End Get
        End Property

        Public Overridable ReadOnly Property Value As Double Implements [Event].Value
            Get
                Return event_Conflict.Value
            End Get
        End Property

        Public Overridable Property Weight As Double
            Get
                Return weight_Conflict
            End Get
            Set(weight As Double)
                Me.weight_Conflict = weight
            End Set
        End Property


        Public Overrides Function ToString() As String
            If event_Conflict Is Nothing Then
                Return "[null event]"
            End If
            Return "[t=" & event_Conflict.Time & ", v=" & event_Conflict.Value & "]"
        End Function

        Public Overrides Function GetHashCode() As Integer
            If event_Conflict Is Nothing Then
                Return MyBase.GetHashCode()
            End If
            Const prime As Integer = 31
            Dim result As Integer = 1
            result = prime * result + CInt(event_Conflict.Time Xor (CLng(CULng(event_Conflict.Time) >> 32)))
            Dim temp As Long
            temp = System.BitConverter.DoubleToInt64Bits(event_Conflict.Value)
            result = prime * result + CInt(temp Xor (CLng(CULng(temp) >> 32)))
            Return result
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If Me Is obj Then
                Return True
            End If
            If obj Is Nothing Then
                Return False
            End If
            If Me.GetType() <> obj.GetType() Then
                Return False
            End If
            Dim other As WeightedEvent = DirectCast(obj, WeightedEvent)
            If other.event_Conflict Is Nothing OrElse event_Conflict Is Nothing Then
                Return False
            End If
            If event_Conflict.Time <> other.event_Conflict.Time Then
                Return False
            End If
            If event_Conflict.Value <> other.event_Conflict.Value Then
                Return False
            End If
            Return True
        End Function

    End Class

End Namespace