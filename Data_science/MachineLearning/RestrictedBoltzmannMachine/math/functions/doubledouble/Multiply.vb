Namespace math.functions.doubledouble
    ''' <summary>
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class Multiply
        Inherits DoubleDoubleFunction

        Public Overrides Function apply(v As Double, v2 As Double) As Double
            Return v * v2
        End Function
    End Class

End Namespace
