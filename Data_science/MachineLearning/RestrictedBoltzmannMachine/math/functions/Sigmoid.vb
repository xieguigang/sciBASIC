
Namespace math.functions
    ''' <summary>
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class Sigmoid
        Inherits DoubleFunction

        Public Overrides Function apply(x As Double) As Double
            Return 1.0 / (1.0 + System.Math.Exp(-x))
        End Function

        Public Overrides Function ToString() As String
            Return "sigmoid(x) = 1 / (1 + e^(-x))"
        End Function

    End Class

End Namespace
