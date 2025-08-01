Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling


    Public Class WeightedEvent
        Implements ITimeSignal

        Public Sub New(time As Long, value As Double)
            Me.Event = New PlainEvent(time, value)
        End Sub

        Public Sub New(e As ITimeSignal)
            Me.Event = e
        End Sub

        Public Overridable ReadOnly Property [Event] As ITimeSignal

        Public Overridable ReadOnly Property Time As Double Implements ITimeSignal.time
            Get
                Return _Event.time
            End Get
        End Property

        Public Overridable ReadOnly Property Value As Double Implements ITimeSignal.intensity
            Get
                Return _Event.intensity
            End Get
        End Property

        Public Overridable Property Weight As Double

        Public Overrides Function ToString() As String
            If _Event Is Nothing Then
                Return "[null event]"
            End If
            Return "[t=" & _Event.time & ", v=" & _Event.intensity & "]"
        End Function

        Public Overrides Function GetHashCode() As Integer
            If _Event Is Nothing Then
                Return MyBase.GetHashCode()
            End If
            Const prime As Integer = 31
            Dim result As Integer = 1
            result = prime * result + CInt(_Event.time Xor (CLng(CULng(_Event.time) >> 32)))
            Dim temp As Long
            temp = System.BitConverter.DoubleToInt64Bits(_Event.intensity)
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
            If other.Event Is Nothing OrElse _Event Is Nothing Then
                Return False
            End If
            If _Event.time <> other.Event.time Then
                Return False
            End If
            If _Event.intensity <> other.Event.intensity Then
                Return False
            End If
            Return True
        End Function

    End Class

End Namespace