Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace EmGaussian

    ''' <summary>
    ''' A possible signle peak
    ''' </summary>
    Public Class Variable

        Public Property weight As Double
        Public Property mean As Double
        Public Property variance As Double

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function gauss(x As Double) As Double
            Return pnorm.ProbabilityDensity(x, mean, variance)
        End Function

    End Class
End Namespace