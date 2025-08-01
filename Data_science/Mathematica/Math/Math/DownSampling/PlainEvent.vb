Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling

    Public Class PlainEvent : Implements ITimeSignal

        Public Sub New(time As Long, value As Double)
            Me.Time = time
            Me.Value = value
        End Sub

        Public Sub New(time As Double, value As Double)
            Me.Time = time
            Me.Value = value
        End Sub

        Public Overridable ReadOnly Property Time As Double Implements ITimeSignal.time
        Public Overridable ReadOnly Property Value As Double Implements ITimeSignal.intensity

        Public Overrides Function GetHashCode() As Integer
            Const prime As Integer = 31
            Dim result As Integer = 1
            result = prime * result + CInt(Time Xor (CLng(CULng(Time) >> 32)))
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
            Dim other As PlainEvent = DirectCast(obj, PlainEvent)
            If Time <> other.Time Then
                Return False
            End If
            Return True
        End Function

    End Class

End Namespace