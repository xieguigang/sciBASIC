Imports Microsoft.VisualBasic.Math.Statistics.Distributions
Imports Microsoft.VisualBasic.Math.Statistics.Linq

Public Class FTest

    Dim x, y As Double()

    Public ReadOnly Property XdegreeOfFreedom As Integer
        Get
            Return x.Length - 1
        End Get
    End Property

    Public ReadOnly Property YdegreeOfFreedom As Integer
        Get
            Return y.Length - 1
        End Get
    End Property

    Public ReadOnly Property XVariance As Double
        Get
            Return x.Variance
        End Get
    End Property

    Public ReadOnly Property YVariance As Double
        Get
            Return y.Variance
        End Get
    End Property

    Sub New(x As Double(), y As Double())
        Me.x = x
        Me.y = y
    End Sub

    Public Function PValue() As Double
        Dim q As Double = XVariance ^ 2
        Dim p As Double = YVariance ^ 2
        Dim f As Double = q / p
        Dim cumulative As Double = Distribution.FDistribution(f, XdegreeOfFreedom, YdegreeOfFreedom)

        Return (1 - cumulative) * 2
    End Function

End Class
