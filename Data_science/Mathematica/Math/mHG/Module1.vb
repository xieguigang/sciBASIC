#Region "Microsoft.VisualBasic::d247ddeb1767da2e3a8e7c12214ad5e5, sciBASIC#\Data_science\Mathematica\Math\mHG\Module1.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 470
    '    Code Lines: 196
    ' Comment Lines: 220
    '   Blank Lines: 54
    '     File Size: 23.25 KB


    ' Module Module1
    ' 
    '     Function: HG_row_ncalc, mHGpvalcalc, mHGstatisticcalc, mHGtest, pi_rcalc
    '               R_separation_linecalc
    ' 
    '     Sub: Main
    '     Structure HG_row_n
    ' 
    '         Function: d_ratio, v_ratio
    '         Delegate Function
    ' 
    ' 
    '         Structure calc
    ' 
    '             Function: iter, recur
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports VisualBasic = Microsoft.VisualBasic.Language.Runtime

' author: Kobi Perl
' Based On the following thesis:
'   Eden, E. (2007). Discovering Motifs In Ranked Lists Of DNA Sequences. Haifa. 
'   Retrieved from http://bioinfo.cs.technion.ac.il/people/zohar/thesis/eran.pdf

Module Module1

    Sub Main()

    End Sub

    ''' <summary>
    ''' We define ``EPSILON`` to account for small changes in the calculation of ``p-value``
    ''' between the Function calculating the statistic And this Function calculating the p-values
    ''' Specifically, If ``(statistic + EPSILON)`` Is higher than the hypergeometric tail associated by a cell
    ''' In the ``W*B`` path matrix, used In the ``p-value`` calculation, Then the cell Is Not In the "R region"
    ''' We warn if the ``mHG`` statistic gets below ``-EPSILON``
    ''' </summary>
    Public Const EPSILON# = 0.0000000001

    ''' <summary>
    ''' Performs a minimum-hypergeometric test.
    ''' 
    ''' Test Is based On the following thesis:
    ''' 
    ''' > Eden, E. (2007). Discovering Motifs In Ranked Lists Of DNA Sequences. Haifa. 
    ''' > Retrieved from http://bioinfo.cs.technion.ac.il/people/zohar/thesis/eran.pdf
    '''
    ''' The null-hypothesis Is that the 1S In the lambda list are randomly And uniformly 
    ''' distributed In the lambdas list. The alternative hypothesis Is that the 1S tend
    ''' To appeard In the top Of the list. As the designation Of "top" Is Not a clear-cut
    ''' multiple hypergeometric tests are performed, With increasing length Of lambdas 
    ''' being considered To be In the "top". The statistic Is the minimal p-value obtained 
    ''' In those tests. A ``p-value`` Is calculated based On the statistics.
    ''' </summary>
    ''' <param name="lambdas">``{0,1}^N``, sorted from top to bottom.</param>
    ''' <param name="n_max#">the algorithm will only consider the first ``n_max`` partitions.</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' ```R
    ''' mHG.test &lt;- function(lambdas, n_max = length(lambdas)) {...}
    ''' ```
    ''' </remarks>
    Function mHGtest(lambdas As Vector, Optional n_max# = Double.NaN) As htest
        Dim N% = lambdas.Length
        Dim B# = lambdas.Sum
        Dim W% = lambdas.Length - B

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

    Public Function mHGpvalcalc(p#, N%, B#, Optional n_max# = Double.NaN) As Double
        '# Calculates the p-value associated with the mHG statistic.
        '# Guidelines for the calculation are to be found in:
        '#   Eden, E. (2007). Discovering Motifs in Ranked Lists of DNA Sequences. Haifa. 
        '#   Retrieved from http://bioinfo.cs.technion.ac.il/people/zohar/thesis/eran.pdf
        '# (pages 11-12, 19-20)
        '# Input:
        '#   p - the mHG statistic. Marked as p, as it represenets an "uncorrected" p-value.
        '#   N - total number of white And black balls (according to the hypergeometric problem definition).
        '#   B - number of black balls.
        '#   n_max - the algorithm will calculate the p-value under the null hypothesis that only the 
        '#           first n_max partitions are taken into account in determining in minimum.
        '# Output: p-value.

        n_max = n_max Or CDbl(N).AsDefault.When(n_max.IsNaNImaginary)

        ' Input check
        'stopifnot(n_max > 0)
        'stopifnot(n_max <= N)
        'stopifnot(N >= B)
        'stopifnot(p <= 1)

        If (p < -EPSILON) Then
            Warning("p-value calculation will be highly inaccurate due to an extremely small mHG statistic")
        End If

        ' p - the statistic.
        ' N\B - number Of all \ black balls.
        Dim W = N - B
        Dim R_separation_line = R_separation_linecalc(p, N, B, n_max)
        Dim pi_r = pi_rcalc(N, B, R_separation_line)
        Dim p_corrected As Double = 1 - pi_r(W + 2, B + 2)

        Return p_corrected
    End Function

    Public Function R_separation_linecalc(p#, N%, B%, n_max%) As Vector
        '# Determine R separation line - This Is the highest (broken) line crossing the B*W matrix horizontally, that underneath it all
        '# the associated p-values are higher than p, Or w + b > n_max.
        '#
        '# (This Is a bit different from the original definition To make the calculation more efficient)
        '#
        '# Input:
        '#   p - the mHG statistic. Marked As p, As it represenets an "uncorrected" p-value.
        '#   N - total number Of white And black balls (according To the hypergeometric problem definition).
        '#   B - number Of black balls.
        '#   n_max - Part Of the constraint On the line, the null hypothesis Is calculated under
        '#           the assumption that the first n_max partitions are taken into account In determining the minimum.
        '# Output:
        '#   R_separation_line - represented As a vector size B + 1, index b + 1 containing 
        '#                       the first (high enough) w To the right Of the R separation line (Or W + 1 If no such w exist).
        '# See:
        '#   Eden, E. (2007). Discovering Motifs In Ranked Lists Of DNA Sequences. Haifa. 
        '#   Retrieved from http://bioinfo.cs.technion.ac.il/people/zohar/thesis/eran.pdf
        '#   (pages 11-12)
        Dim W# = N - B
        Dim R_separation_line As Vector = Repeats(W + 1, times:=B + 1)

        Dim HG_row As New Vector(B) ' First B in HG_row
        HG_row(1) = 1 ' For n = 0, b = 0
        Dim bi = 0
        Dim wi = 0
        Dim HGT = 1 ' Initial HGT

        ' We are tracing the R line - increasing b until we Get To a cell where the associated p-values are smaller
        ' than p, And Then increasing w until we Get To a cell where the associated p-values are bigger than p
        Dim should_inc_w = (HGT <= (p + EPSILON)) AndAlso (wi < W) AndAlso (bi <= (n_max - wi))
        Dim should_inc_b = (HGT > (p + EPSILON)) AndAlso (bi < B) AndAlso (bi <= (n_max - wi))

        Do While (should_inc_w OrElse should_inc_b)
            Do While (should_inc_b)  ' Increase b until we Get To the R line (Or going outside the n_max zone)
                R_separation_line(bi + 1) = wi
                bi = bi + 1
                HG_row(bi + 1) = HG_row(bi) * d_ratio(bi + wi, bi, N, B)
                HG_row($"1:{bi}") = HG_row($"1:{bi}") * v_ratio(bi + wi, seq(0, bi - 1), N, B)
                HGT = 1 - HG_row($"1:{bi}").Sum  ' P(X >= b) = 1 - P(X <b)
                should_inc_b = (HGT > (p + EPSILON)) AndAlso (bi < B) AndAlso (bi <= (n_max - wi))
            Loop

            If (bi > (n_max - wi)) Then
                ' We can Stop immediately And we Do Not need To calculate HG_row anymore
                R_separation_line($"({bi}+1):({B}+1)") = W
                should_inc_w = False
            Else
                should_inc_w = (HGT <= (p + EPSILON)) AndAlso (W < W)
                Do While (should_inc_w) ' Increase w until we Get outside the R line (Or going outside the n_max zone)
                    W = W + 1
                    HG_row("1:(b+1)") = HG_row("1:(b+1)") * v_ratio(B + W, Seq(0, B), N, B)
                    HGT = 1 - HG_row("1:B").Sum  ' P(X >= b) = 1 - P(X <b)
                    should_inc_w = (HGT <= (p + EPSILON)) AndAlso (W < W) AndAlso (B <= (n_max - W))
                Loop
                If (B > (n_max - W)) Then
                    ' We can stop immediately And we do Not need to calculate HG_row anymore
                    R_separation_line("(b+1):(B+1)") = W
                    should_inc_b = False
                Else
                    should_inc_b = (HGT > (p + EPSILON)) AndAlso (B < B) AndAlso (B <= (n_max - W))
                End If
            End If
        Loop
        If (HGT > (p + EPSILON)) Then ' Last one
            R_separation_line(B + 1) = W
        End If
        Return (R_separation_line)
    End Function

    Public Function pi_rcalc(N%, B%, R_separation_line As Vector) As Matrix
        '# Consider an urn With N balls, B Of which are black And W white. pi_r stores 
        '# The probability Of drawing w white And b black balls In n draws (n = w + b)
        '# With the constraint Of P(w,b) = 0 If (w, b) Is On Or above separation line.
        '# Row 1 Of the matrix represents w = -1, Col 1 represents b = -1.
        '#
        '# Input:
        '#   N - total number Of white And black balls (according To the hypergeometric problem definition).
        '#   B - number Of black balls.
        '#   R_separation_line - represented As a vector size B + 1, index b + 1 containing 
        '#                       the first (high enough) w To the right Of the R separation line.
        '# See:
        '#   Eden, E. (2007). Discovering Motifs In Ranked Lists Of DNA Sequences. Haifa. 
        '#   Retrieved from http://bioinfo.cs.technion.ac.il/people/zohar/thesis/eran.pdf
        '#   (pages 20)
        Dim W As Double = N - B
        Dim pi_r As Matrix = Matrix.Create(nrow:=W + 2, ncol:=B + 2)

        ' NOTE: Different from the thesis (see page 20 last paragraph),
        ' should be 1 according To that paragraph, but this seems wrong.
        pi_r.Row(1) = 0
        pi_r.Column(1) = 0

        For bi As Integer = 0 To B
            Dim wi = R_separation_line(bi + 1)
            Do While (wi < (W + 1))
                If ((wi = 0) AndAlso (bi = 0)) Then
                    pi_r(2, 2) = 1 ' Note, this cell will be 0 If it's left to the R separation line (should not occure) 
                Else
                    ' Apply the recursion rule:
                    ' P(w,b) = P((w,b)|(w-1,b))*P(w-1,b)+P((w,b)|(w,b-1))*P(w,b-1)
                    pi_r(wi + 2, bi + 2) = (W - wi + 1) / (B + W - bi - wi + 1) * pi_r(wi + 1, bi + 2) +
                        (B - bi + 1) / (B + W - bi - wi + 1) * pi_r(wi + 2, bi + 1)
                End If
                wi = wi + 1
            Loop
        Next
        Return (pi_r)
    End Function

    ''' <summary>
    ''' Calculate HG row n. This row contains the first (b_n  + 1)
    ''' hypergeometric probabilities, HG[i] = Prob(X == (i - 1)), For number Of tries n.
    ''' Does so given an updated HG row m (m &lt; n), which contains the first (b_n)
    ''' hypergeometric probabilities.
    ''' </summary>
    ''' <param name="HG_row_m">updated HG row m (m &lt; n), which contains the first (b_n)
    ''' hypergeometric probabilities.</param>
    ''' <param name="m">the number Of tries (m &lt; n) For which the HG_row_m fits.</param>
    ''' <param name="ni">the number Of tries (n > m) For which we want To calculate the HG row</param>
    ''' <param name="b_n">The maximal b For which we need To calculate the hypergeometric probabilities.</param>
    ''' <param name="N">total number Of white And black balls (according To the hypergeometric problem definition).</param>
    ''' <param name="B">number Of black balls.</param>
    ''' <param name="RECURSION_OVERHEAD_MULTIPLIER">
    ''' The Function directs the calculation To an iteration solution (With the cost Of B(n-m)) 
    ''' Or a recursive solution (With the cost B * log(B)). This multiplier helps To determine
    ''' When To use the recursion solution - it Is Not a theoretical result, but an empirical one.
    ''' </param>
    ''' <returns></returns>
    Public Function HG_row_ncalc(HG_row_m As Vector, m%, ni%, b_n#, N%, B%, Optional RECURSION_OVERHEAD_MULTIPLIER% = 20) As Vector
        Dim HG_row_ncalcfunc As HG_row_n.Calculator = Nothing

        If ((N - m) <= (RECURSION_OVERHEAD_MULTIPLIER * Log2(b_n))) Then
            HG_row_ncalcfunc = AddressOf HG_row_n.calc.iter
        Else
            HG_row_ncalcfunc = AddressOf HG_row_n.calc.recur
        End If

        Return HG_row_ncalcfunc(HG_row_m, m, ni, b_n, N, B)
    End Function

    Public Structure HG_row_n

        Public Delegate Function Calculator(HG_row_m As Vector, m%, ni%, b_n#, N%, B%) As Vector

        Public Structure calc

            ''' <summary>
            ''' Calculate HG row n recursively.
            ''' See Function documentation For "HG_row_n.calc", To gain insight On input And outputs. 
            '''
            ''' NOTE: This implementation Is my interpretation Of a very unclear statement In Eden's thesis that a row can be
            ''' calculated In O(B) without considering previous rows. I did this In O(B * log(B)).
            ''' </summary>
            ''' <param name="HG_row_m"></param>
            ''' <param name="m"></param>
            ''' <param name="ni"></param>
            ''' <param name="b_n"></param>
            ''' <param name="N"></param>
            ''' <param name="B"></param>
            ''' <returns></returns>
            Public Shared Function recur(HG_row_m As Vector, m%, ni%, b_n#, N%, B%) As Vector
                Dim HG_row_ncalcrecurinner As Action(Of Integer, Integer, Integer) =
                    Sub(b_n_start%, b_n_end%, m_start%)
                        '# The code works directly On HG_row_m - updating it recursively, filling the right
                        '# values from right To left. Filling this row In a directed manner allow us To update a cell
                        '# In a recursive manner according To the one left To it, without being concerned that the cell
                        '# To the left was already updated To a "too big" number Of tries.
                        '#
                        '# The code work On the subtree induced by the limits:
                        '# Rows (m_start, n) And columns (b_n_start, b_n_end),
                        '# updating the subtree In a recursive manner until row_n.
                        '# 
                        '# The code assumes that:
                        '#   HG_row_m[b_n_start] has the correct hypergeometric probability value For number Of tries (row) 
                        '#   m_start.

                        ' # Stop condition
                        If ((N - m_start) = 0) Then
                            Return
                        Else
                            ' split the tree To two subtrees To be evaluated separately.
                            ' R_tree Will be used To calculate the HG_row_n entries corresponding To (b_n_start:b_n_start + r_split),
                            ' And L_tree will be used To calculate the rest.
                            Dim r_split = Floor((b_n_end - b_n_start + 1) / 2)
                            Dim l_split = (b_n_end - b_n_start + 1) - r_split

                            'If m Is above the root Of the tree we are working On - Then some rows were already
                            ' calculated And we can take this To our advantage.
                            Dim rows_already_calc = Max(m - m_start, 0)

                            ' Go diagonally (increasing both b_n_start And m) until we Get To the root Of the right tree.
                            Dim i = rows_already_calc
                            Do While (i < l_split) ' Needs To occur sequentially
                                i = i + 1
                                HG_row_m(b_n_start + i + 1) = HG_row_m(b_n_start + i) * d_ratio(m_start + i, b_n_start + i, N, B)
                            Loop
                            ' Calculate the right tree.
                            HG_row_ncalcrecurinner(b_n_start + l_split, b_n_end, m_start + l_split)

                            ' Go upwards (increasing only m) until we Get To the root Of the left tree.      
                            i = rows_already_calc
                            Do While (i < (N + 1 - l_split - m_start)) ' Needs To occur sequentially
                                i = i + 1
                                HG_row_m(b_n_start + 1) = HG_row_m(b_n_start + 1) * v_ratio(m_start + i, b_n_start, N, B)
                            Loop
                            ' Calculate the left tree.
                            HG_row_ncalcrecurinner(b_n_start, b_n_start + l_split - 1, N + 1 - l_split)
                        End If
                    End Sub

                '# HG_row_n Is calculated from a tree, For which the root Is located In
                '# b_n rows beneath - we will mark this As m_start.
                '#If rowThenThen m Is "beneath" this root, We "go up" And calculate it.
                '#If weThenThen are "above" this root, we will ignore this For now, And use the fact that
                '# we have some rows calculated above m_start, In the recursion.
                '# NOTE: We can technically initialize HG_row_m[2:] To 0, but knowing that we will
                '# only use the cell In index 1, we Do Not bother.
                Dim i_start = N - b_n

                Do While (m < i_start)
                    m = m + 1
                    HG_row_m(1) = HG_row_m(1) * v_ratio(m, 0, N, B)
                Loop

                ' NOTE: this code works On HG_row_m directly.
                HG_row_ncalcrecurinner(0, b_n, i_start)

                Return HG_row_m
            End Function

            ''' <summary>
            ''' Calculate HG row n iteratively.
            ''' See function documentation for "HG_row_n.calc", to gain insight on input And outputs. 
            '''
            ''' NOTE: The code works directly on HG_row_m, m - increasing m until it becomes n.
            ''' </summary>
            ''' <param name="HG_row_m"></param>
            ''' <param name="m#"></param>
            ''' <param name="ni#"></param>
            ''' <param name="b_n#"></param>
            ''' <param name="N#"></param>
            ''' <param name="B#"></param>
            ''' <returns></returns>
            Public Shared Function iter(HG_row_m As Vector, m%, ni%, b_n#, N%, B%) As Vector
                '# Go upwards (increasing only m) until we get to row n-1.    
                Dim b_to_update As Vector = seq(0, b_n - 1)

                Do While (m < (N - 1))
                    m = m + 1
                    HG_row_m(b_to_update + 1) = HG_row_m(b_to_update + 1) * v_ratio(m, b_to_update, N, B)
                Loop

                m = m + 1
                ' Last row To go - first update b_n from the diagonal, 
                ' Then the rest vertically
                HG_row_m(b_n + 1) = HG_row_m(b_n) * d_ratio(m, b_n, N, B)
                HG_row_m(b_to_update + 1) = HG_row_m(b_to_update + 1) * v_ratio(m, b_to_update, N, B)

                Return HG_row_m
            End Function
        End Structure
    End Structure

    ''' <summary>
    ''' The ratio between HG(n,b,B,N) And HG(n-1,b-1,B,N)
    ''' See page 19 In Eden's theis.
    ''' </summary>
    ''' <param name="ni"></param>
    ''' <param name="bi"></param>
    ''' <param name="N"></param>
    ''' <param name="B"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function d_ratio(ni, bi, N, B) As Double
        Return (N * (B - (B - 1)) / (B * (N - (N - 1))))
    End Function

    ''' <summary>
    ''' The ratio between HG(n,b,B,N) And HG(n-1,b,B,N)
    ''' See page 19 In Eden's theis
    ''' </summary>
    ''' <param name="ni"></param>
    ''' <param name="bi"></param>
    ''' <param name="N"></param>
    ''' <param name="B"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function v_ratio(ni, bi, N, B) As Double
        Return ((N * (N - N - B + B + 1)) / ((N - B) * (N - N + 1)))
    End Function

End Module
