Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace math.functions
    ''' <summary>
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class RandomGaussian
        Inherits DoubleFunction
        Public Overrides Function apply(v As Double) As Double
            Return randf.NextGaussian() * 0.1
        End Function

    End Class

End Namespace
