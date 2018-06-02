Imports Microsoft.VisualBasic.Math.LinearAlgebra

' author: Kobi Perl
' Based On the following thesis:
'   Eden, E. (2007). Discovering Motifs In Ranked Lists Of DNA Sequences. Haifa. 
'   Retrieved from http://bioinfo.cs.technion.ac.il/people/zohar/thesis/eran.pdf

Module Module1

    Sub Main()

    End Sub

    ''' <summary>
    ''' We define EPSILON to account for small changes in the calculation of p-value
    ''' between the Function calculating the statistic And this Function calculating the p-values
    ''' Specifically, If (statistic + EPSILON) Is higher than the hypergeometric tail associated by a cell
    ''' In the W*B path matrix, used In the p-value calculation, Then the cell Is Not In the "R region"
    ''' We warn ifthe mHG statistic gets below -EPSILON
    ''' </summary>
    Public Const EPSILON# = 0.0000000001

    ''' <summary>
    ''' Performs a minimum-hypergeometric test.
    ''' Test Is based On the following thesis:
    '''   Eden, E. (2007). Discovering Motifs In Ranked Lists Of DNA Sequences. Haifa. 
    '''   Retrieved from http://bioinfo.cs.technion.ac.il/people/zohar/thesis/eran.pdf
    '''
    ''' The null-hypothesis Is that the 1S In the lambda list are randomly And uniformly 
    ''' distributed In the lambdas list. The alternative hypothesis Is that the 1S tend
    ''' To appeard In the top Of the list. As the designation Of "top" Is Not a clear-cut
    ''' multiple hypergeometric tests are performed, With increasing length Of lambdas 
    ''' being considered To be In the "top". The statistic Is the minimal p-value obtained 
    ''' In those tests. A p-value Is calculated based On the statistics.
    ''' </summary>
    ''' <param name="lambdas">{0,1}^N, sorted from top to bottom.</param>
    ''' <param name="n_max#">the algorithm will only consider the first n_max partitions.</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' ```R
    ''' mHG.test &lt;- function(lambdas, n_max = length(lambdas)) {...}
    ''' ```
    ''' </remarks>
    Function mHGtest(lambdas As Vector, Optional n_max# = Double.NaN) As htest
        Dim N = lambdas.Length
        Dim B = lambdas.Sum
        Dim W = lambdas.Length - B

        ' The uncorrected for MHT p-value
        Dim mHGstatisticinfo As mHGstatisticInfo = mHGstatisticcalc(lambdas, n_max)
        Dim p = mHGpvalcalc(mHGstatisticinfo.mHG, N, B, n_max)
        Dim result As New htest
        result.statistic = c("mHG" = mHG.statistic.info@mHG)
        result.parameters = c("N" = N, "B" = B, "n_max" = n_max)
        result.pvalue = p
        result.n = mHGstatisticinfo.n ' Not an official field Of htest
        result.b = mHGstatisticinfo.b ' Not an official field Of htest

        Return result
    End Function

End Module

Public Class htest
    Public statistic
    Public parameters
    Public pvalue As Double
    Public n As Integer
    Public b As Double
End Class

''' <summary>
''' mHG definition:
'''   mHG(lambdas) = min over 1 &lt;= n &lt;= N Of HGT (b_n(lambdas); N, B, n)
''' Where HGT Is the hypergeometric tail:
'''   HGT(b; N, B, n) = Probability(X >= b)
''' And:
'''   b_n = sum over 1 &lt;= i &lt;= n Of lambdas[i]
''' Fields:
'''   mHG - the statistic itself
'''   n - the index For which it was obtained
'''   b (Short For b_n) - sum over 1 &lt;= i &lt;= n Of lambdas[i]
''' </summary>
Class mHGstatisticInfo
    Public mHG As Double
    Public n As Double
    Public b As Double
End Class

