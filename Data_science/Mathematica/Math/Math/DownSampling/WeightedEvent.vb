Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling


    Public Class WeightedEvent
        Implements ITimeSignal

        Private event_Conflict As ITimeSignal
        Private weight_Conflict As Double

        Public Sub New(time As Long, value As Double)
            Me.event_Conflict = New PlainEvent(time, value)
        End Sub

        Public Sub New(e As ITimeSignal)
            Me.event_Conflict = e
        End Sub

        Public Overridable ReadOnly Property [Event] As ITimeSignal
            Get
                Return event_Conflict
            End Get
        End Property

        Public Overridable ReadOnly Property Time As Double Implements ITimeSignal.time
            Get
                Return event_Conflict.time
            End Get
        End Property

        Public Overridable ReadOnly Property Value As Double Implements ITimeSignal.intensity
            Get
                Return event_Conflict.intensity
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
            Return "[t=" & event_Conflict.time & ", v=" & event_Conflict.Value & "]"
        End Function

        Public Overrides Function GetHashCode() As Integer
            If event_Conflict Is Nothing Then
                Return MyBase.GetHashCode()
            End If
            Const prime As Integer = 31
            Dim result As Integer = 1
            result = prime * result + CInt(event_Conflict.time Xor (CLng(CULng(event_Conflict.time) >> 32)))
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
            If event_Conflict.time <> other.event_Conflict.time Then
                Return False
            End If
            If event_Conflict.Value <> other.event_Conflict.Value Then
                Return False
            End If
            Return True
        End Function

    End Class

End Namespace