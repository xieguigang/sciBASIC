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

        Sub New()
        End Sub

        ''' <summary>
        ''' make data copy
        ''' </summary>
        ''' <param name="clone"></param>
        Sub New(clone As Variable)
            weight = clone.weight
            mean = clone.mean
            variance = clone.variance
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function gauss(x As Double) As Double
            Return pnorm.ProbabilityDensity(x, mean, variance) * weight
        End Function

    End Class
End Namespace