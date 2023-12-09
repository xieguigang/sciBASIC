Namespace math.functions

    ''' <summary>
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class Round
        Inherits DoubleFunction
        Private ReadOnly threshold As Double

        Public Sub New()
            Me.New(0.80)
        End Sub

        Public Sub New(threshold As Double)
            Me.threshold = threshold
        End Sub

        Public Overrides Function apply(x As Double) As Double
            Return If(x >= threshold, 1.0, 0.0)
        End Function

        Public Overrides Function ToString() As String
            Return "Round{" & "threshold=" & threshold.ToString() & "}"c.ToString()
        End Function
    End Class

End Namespace
