
Namespace math.functions
    ''' <summary>
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class Power
        Inherits DoubleFunction

        Private ReadOnly power As Double

        Public Sub New(power As Double)
            Me.power = power
        End Sub

        Public Overrides Function apply(v As Double) As Double
            Return System.Math.Pow(v, power)
        End Function
    End Class

End Namespace
