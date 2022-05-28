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
            Return Variance(x)
        End Get
    End Property

    Public ReadOnly Property YVariance As Double
        Get
            Return Variance(y)
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
        Return $"

	F test to compare two variances

data:  x and y
F = {F}, num df = {XdegreeOfFreedom}, denom df = {YdegreeOfFreedom}, p-value = {PValue}
alternative hypothesis: true ratio of variances is not equal to 1
95 percent confidence interval:
  1.089699 17.662528
sample estimates:
ratio of variances 
          {F}
"
    End Function

    Private Shared Function Variance(values As Double()) As Double
        Dim total As Double
        Dim totalSquared As Double
        Dim counter As Integer

        For Each value In values
            counter += 1
            total += value
            totalSquared += value ^ 2
        Next

        Return (totalSquared - ((total * total) / counter)) / (counter - 1)
    End Function

End Class
