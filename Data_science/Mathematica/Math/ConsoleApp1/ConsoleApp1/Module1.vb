Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports VisualBasic = Microsoft.VisualBasic.Language.Runtime
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Vectorization

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
    ''' <param name="lambdas">``{0,1}^N``, sorted from top to bottom.</param>
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

        n_max = n_max Or CDbl(lambdas.Length).AsDefault.When(n_max.IsNaNImaginary)

        ' The uncorrected for MHT p-value
        Dim mHGstatisticinfo As mHGstatisticInfo = mHGstatisticcalc(lambdas, n_max)
        Dim p = mHGpvalcalc(mHGstatisticinfo.mHG, N, B, n_max)
        Dim result As New htest With {
            .pvalue = p,
            .n = mHGstatisticinfo.n, ' Not an official field Of htest
            .b = mHGstatisticinfo.b  ' Not an official field Of htest        
        }

        With New VisualBasic
            result.statistic = list(!mHG = mHGstatisticinfo.mHG).AsNumeric
            result.parameters = list(!N = N, !B = B, !n_max = n_max).AsNumeric
        End With

        Return result
    End Function

    Public Function mHGstatisticcalc(lambdas As Vector, Optional n_max# = Double.NaN) As mHGstatisticInfo
        '# Calculates the mHG statistic.
        '# mHG definition:
        '#   mHG(lambdas) = min over 1 <= n < N of HGT (b_n(lambdas); N, B, n)
        '# Where HGT Is the hypergeometric tail:
        '#   HGT(b; N, B, n) = Probability(X >= b)
        '# And:
        '#   b_n = sum over 1 <= i <= n of lambdas[i]
        '# In R, HGT can be obtained using:
        '#   HGT(b; N, B, n) = phyper((b-1), B, N - B, n, lower.tail = F)
        '#
        '# Input:
        '#   lambdas - sorted And labeled {0,1}^N.
        '#   n_max - the algorithm will only consider the first n_max partitions.
        '# Output: mHG.statistic
        '# 
        '# Statistic Is defined in the following thesis:
        '#   Eden, E. (2007). Discovering Motifs in Ranked Lists of DNA Sequences. Haifa. 
        '#   Retrieved from http://bioinfo.cs.technion.ac.il/people/zohar/thesis/eran.pdf
        '# 
        '# If several n gives the same mHG, then the lowest one Is taken.

        n_max = n_max Or CDbl(lambdas.Length).AsDefault.When(n_max.IsNaNImaginary)

        ' Input check
        ' stopifnot(n_max > 0)
        ' stopifnot(n_max <= length(lambdas))
        ' stopifnot(length(lambdas) > 0)
        ' stopifnot(all(lambdas == 1 | lambdas == 0))

        Dim N = lambdas.Length
        Dim B = lambdas.Sum
        Dim W = N - B

        Dim mHG = 1
        Dim mHGn = 0
        Dim mHGb = 0
        Dim m = 0 ' Last time we saw a one
        Dim HG_row As New Vector(B + 1) ' The first B + 1 hypergeometric probabilities, HG[i] = Prob(X == (i - 1))
        Dim HGT As Double

        ' updated For the current number Of tries n.
        HG_row(1) = 1 ' For n = 0, b = 0
        B = 0
        N = 0
        Do While (N < n_max)  ' iterating On different N To find minimal HGT
            N = N + 1
            B = B + lambdas(N)

            If (lambdas(N) = 1.0R) Then  ' Only Then HGT can decrease (see p. 19 In thesis)
                HG_row = HG_row_ncalc(HG_row, m, N, B, N, B)
                m = N

                HGT = 1 - HG_row("1:b").Sum  ' P(X >= b) = 1 - P(X <b)
                ' statistic
                If (HGT < mHG) Then
                    mHG = HGT
                    mHGn = N
                    mHGb = B
                End If
            End If
        Loop
        Return New mHGstatisticInfo With {.mHG = mHG, .n = mHGn, .b = mHGb}
    End Function

End Module

Public Class htest
    Public statistic As Dictionary(Of String, Double)
    Public parameters As Dictionary(Of String, Double)
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

