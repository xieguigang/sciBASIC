Namespace math.functions.doubledouble.rbm
    ''' <summary>
    ''' Created by kenny on 5/25/14.
    ''' </summary>
    Public Class ActivationState
        Inherits DoubleDoubleFunction
        Public Overrides Function apply(x As Double, y As Double) As Double
            Return If(x >= y, 1.0, 0.0)
        End Function
    End Class

End Namespace
