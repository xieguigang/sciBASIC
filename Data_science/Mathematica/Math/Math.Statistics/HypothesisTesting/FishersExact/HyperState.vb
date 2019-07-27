Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Hypothesis.FishersExact

    Public Class HyperState
        Public Property n11 As Integer
        Public Property n1_ As Integer
        Public Property n_1 As Integer
        Public Property n As Integer
        Public Property prob As Double
        Public Property valid As Boolean

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

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

        Public Property hyper_state As HyperState

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace