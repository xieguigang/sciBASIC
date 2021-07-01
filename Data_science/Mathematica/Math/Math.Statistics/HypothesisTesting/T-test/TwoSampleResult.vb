Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Hypothesis

    Public Class TwoSampleResult : Inherits TtestResult

        Public Property MeanX As Double
        Public Property MeanY As Double

        Public Property y As Double()

        Public Overrides Function ToString() As String
            Dim ci95 = Me.ci95

            Return $"
	Welch Two Sample t-test

data:  {x.GetJson} and {y.GetJson}
t = {TestValue}, df = {DegreeFreedom}, p-value <= {Pvalue}
alternative hypothesis: {Valid.ToString.ToUpper} difference in means is {opt.alternative.Description} {Mean}
{(1 - opt.alpha) * 100} percent confidence interval:
 {ci95(0)} {ci95(1)}
sample estimates:
mean of x mean of y 
 {MeanX}  {MeanY}"
        End Function
    End Class
End Namespace