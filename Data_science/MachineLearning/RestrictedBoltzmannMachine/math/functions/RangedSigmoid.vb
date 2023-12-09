
Namespace math.functions

    ''' <summary>
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class RangedSigmoid
        Inherits DoubleFunction
        Private ReadOnly min As Double

        Private ReadOnly max As Double

        Public Sub New(min As Double, max As Double)
            Me.min = min
            Me.max = max
        End Sub

        Public Overrides Function apply(x As Double) As Double
            Return min + (max - min) / (1.0 + System.Math.Exp(-x))
        End Function

        Public Overrides Function ToString() As String
            Return "sigmoid(x) = min + ((max - min) / (1 + e^(-x)))"
        End Function
    End Class

End Namespace
