Namespace Graphic.Axis

    Public Class TimeRange

        Public ReadOnly Property [From] As Date
        Public ReadOnly Property [To] As Date
        Public ReadOnly Property Ticks As Date()

        Sub New(from As Date, [to] As Date)
            Dim axis#() = {from. }
            Me.from = from
            Me.to = [to]
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{from}, {[to]}]"
        End Function
    End Class
End Namespace