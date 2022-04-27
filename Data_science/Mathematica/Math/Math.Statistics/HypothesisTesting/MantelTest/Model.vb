Namespace Hypothesis.Mantel

    ''' <summary>
    ''' the test model
    ''' </summary>
    Public Class Model

        ''' <summary>
        ''' reference statistic
        ''' </summary>
        ''' <returns></returns>
        Public Property coef As Double
        ''' <summary>
        ''' p-value
        ''' </summary>
        ''' <returns></returns>
        Public Property proba As Double
        ''' <summary>
        ''' number of randomizations
        ''' </summary>
        ''' <returns></returns>
        Public Property numrand As Integer
        ''' <summary>
        ''' size of matrices
        ''' </summary>
        ''' <returns></returns>
        Public Property matsize As Integer
        ''' <summary>
        ''' number of elements in the half-matrix without diagonal values
        ''' </summary>
        ''' <returns></returns>
        Public Property numelt As Integer
        ''' <summary>
        ''' option partial 0|1
        ''' </summary>
        ''' <returns></returns>
        Public Property [partial] As Integer = True
        ''' <summary>
        ''' option raw 0|1
        ''' </summary>
        ''' <returns></returns>
        Public Property raw As Integer
        ''' <summary>
        ''' option exact permutation 0|1
        ''' </summary>
        ''' <returns></returns>
        Public Property exact As Integer ' 

    End Class
End Namespace