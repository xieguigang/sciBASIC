Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling


    Public Class PlainEvent
        Implements ITimeSignal

        Private time_Conflict As Long
        Private value_Conflict As Double

        Public Sub New(time As Long, value As Double)
            Me.time_Conflict = time
            Me.value_Conflict = value
        End Sub

        Public Overridable ReadOnly Property Time As Long Implements ITimeSignal.time
            Get
                Return time_Conflict
            End Get
        End Property

        Public Overridable ReadOnly Property Value As Double Implements ITimeSignal.Value
            Get
                Return value_Conflict
            End Get
        End Property

        Public Overrides Function GetHashCode() As Integer
            Const prime As Integer = 31
            Dim result As Integer = 1
            result = prime * result + CInt(time_Conflict Xor (CLng(CULng(time_Conflict) >> 32)))
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
            If time_Conflict <> other.time_Conflict Then
                Return False
            End If
            Return True
        End Function

    End Class

End Namespace