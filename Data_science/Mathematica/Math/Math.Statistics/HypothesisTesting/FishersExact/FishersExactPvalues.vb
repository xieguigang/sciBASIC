Namespace Hypothesis.FishersExact

    ''' <summary>
    ''' `FishersExactPvalues` holds the pvalues calculated by the `fishers_exact` function.
    ''' </summary>
    Public Class FishersExactPvalues

        ''' <summary>
        ''' pvalue for the two-tailed test. Use this when there Is no prior alternative.
        ''' </summary>
        ''' <returns></returns>
        Public Property two_tail_pvalue As Double
        ''' <summary>
        ''' pvalue for the "left" Or "lesser" tail. Use this when the alternative to
        ''' independence Is that there Is negative association between the variables.
        ''' That Is, the observations tend to lie in lower left And upper right.
        ''' </summary>
        ''' <returns></returns>
        Public Property less_pvalue As Double
        ''' <summary>
        ''' Use this when the alternative to independence Is that there Is positive
        ''' association between the variables. That Is, the observations tend to lie
        ''' in upper left And lower right.
        ''' </summary>
        ''' <returns></returns>
        Public Property greater_pvalue As Double

        ''' <summary>
        ''' [a,b,c,d]
        ''' </summary>
        ''' <returns></returns>
        Public Property matrix As Integer()

        Public Property hyper_state As HyperState

        Public Overrides Function ToString() As String
            Return $"
	Fisher's Exact Test for Count Data

data:  [a={matrix(0)}, b={matrix(1)}, c={matrix(2)}, d={matrix(3)}]
p-value = {two_tail_pvalue}
alternative hypothesis: true odds ratio is not equal to 1
95 percent confidence interval:
  0.008512238 20.296715040
sample estimates:
odds ratio 
  0.693793 
"
        End Function
    End Class
End Namespace