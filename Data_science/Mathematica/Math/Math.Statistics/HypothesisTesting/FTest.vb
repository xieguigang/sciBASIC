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

    Public ReadOnly Property F As Double
        Get
            Dim q As Double = XVariance ^ 2
            Dim p As Double = YVariance ^ 2

            Return q / p
        End Get
    End Property

    Public ReadOnly Property PValue As Double
        Get
            Dim f As Double = Me.F
            Dim cumulative As Double = Distribution.FDistribution(f, XdegreeOfFreedom, YdegreeOfFreedom)

            Return (1 - cumulative) * 2
        End Get
    End Property

    Sub New(x As Double(), y As Double())
        Me.x = x
        Me.y = y
    End Sub

    Public Overrides Function ToString() As String
        Return $"F-statistics: {F.ToString("G4")}, p-value: {PValue.ToString("G3")}"
    End Function

End Class
