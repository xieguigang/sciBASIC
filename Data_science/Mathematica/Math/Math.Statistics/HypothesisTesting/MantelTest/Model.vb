Namespace Hypothesis.Mantel

    ''' <summary>
    ''' the test model
    ''' </summary>
    Public Class Model

        Public coef As Double ' reference statistic
        Public proba As Double ' p-value
        Public numrand As Integer ' number of randomizations
        Public matsize As Integer ' size of matrices
        Public numelt As Integer ' number of elements in the half-matrix without diagonal values
        Public [partial] As Integer ' option partial 0|1
        Public raw As Integer ' option raw 0|1
        Public help As Integer ' option help 0|1
        Public exact As Integer ' option exact permutation 0|1
        Public licence As Integer ' option licence terms 0|1

    End Class
End Namespace