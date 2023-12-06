Namespace math.functions
    ''' <summary>
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class Divide
        Inherits DoubleFunction

        Private ReadOnly divisor As Double

        Public Sub New(divisor As Double)
            Me.divisor = divisor
        End Sub

        Public Overrides Function apply(v As Double) As Double
            Return v / divisor
        End Function
    End Class

End Namespace
