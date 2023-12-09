Namespace math.functions

    ''' <summary>
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class Multiply
        Inherits DoubleFunction

        Private ReadOnly value As Double

        Public Sub New(value As Double)
            Me.value = value
        End Sub

        Public Overrides Function apply(v As Double) As Double
            Return v * value
        End Function
    End Class

End Namespace
