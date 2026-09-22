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
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports VB = Microsoft.VisualBasic.Language.Runtime
Imports std = System.Math

' author: Kobi Perl
' Based On the following thesis:
'   Eden, E. (2007). Discovering Motifs In Ranked Lists Of DNA Sequences. Haifa. 
'   Retrieved from http://bioinfo.cs.technion.ac.il/people/zohar/thesis/eran.pdf
'
' NOTE about the indexing convention:
'   The original algorithm (R implementation of the CRAN package ``mHG``) uses 1-based
'   index. Because the ``Vector``/``NumericMatrix`` types of this runtime are 0-based,
'   every array access ``x[k]`` in the R reference is translated into ``x(k - 1)`` here.
'   The helper functions below always mention the R expression they implement so that the
'   two implementations can be compared line by line.

Public Module mHGtest

    ''' <summary>
    ''' We define ``EPSILON`` to account for small changes in the calculation of ``p-value``
    ''' between the Function calculating the statistic And this Function calculating the p-values
    ''' Specifically, If ``(statistic + EPSILON)`` Is higher than the hypergeometric tail associated by a cell
    ''' In the ``W*B`` path matrix, used In the ``p-value`` calculation, Then the cell Is Not In the "R region"
    ''' We warn if the ``mHG`` statistic gets below ``-EPSILON``
    ''' </summary>
    Public Const EPSILON# = 0.0000000001

    ''' <summary>
    ''' ``sum(HG_row[1:b])`` in the R reference, which is the sum of the first ``b``
    ''' hypergeometric probabilities ``HG_row(i) = Prob(X == i)``, ``i &lt; b``.
    ''' </summary>
    ''' <param name="HG_row"></param>
    ''' <param name="b">the number of probabilities that should be accumulated.</param>
    ''' <returns></returns>
    Private Function sumFirst(HG_row As Vector, b As Integer) As Double
        Dim sum# = 0

        For i As Integer = 0 To b - 1
            sum += HG_row(i)
        Next

        Return sum
    End Function

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
    ''' In those tests. A ``p-value`` Is calculated Based On the statistics.
    ''' </summary>
    ''' <param name="lambdas">``{0,1}^N``, sorted from top to bottom.</param>
    ''' <param name="n_max#">the algorithm will only consider the first ``n_max`` partitions.</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' equals to the R function ``mHG.test(lambdas, n_max = length(lambdas))``
    ''' </remarks>
    Public Function mHGtest(lambdas As Vector, Optional n_max# = Double.NaN) As htest
        Dim N% = lambdas.Length
        Dim B# = lambdas.Sum

        n_max = n_max Or CDbl(lambdas.Length).AsDefault.When(n_max.IsNaNImaginary)

        ' The uncorrected for MHT p-value
        Dim mHGstatisticinfo As mHGstatisticInfo = mHGstatisticcalc(lambdas, n_max)
        Dim p# = mHGpvalcalc(mHGstatisticinfo.mHG, N, B, n_max)
        Dim result As New htest With {
            .pvalue = p,
            .n = CInt(mHGstatisticinfo.n), ' Not an official field Of htest
            .b = mHGstatisticinfo.b       ' Not an official field Of htest        
        }

        With New VB
            result.statistic = list(!mHG = mHGstatisticinfo.mHG).AsNumeric
            result.parameters = list(!N = N, !B = B, !n_max = n_max).AsNumeric
        End With

        Return result
    End Function

    ''' <summary>
    ''' Calculates the mHG statistic.
    '''
    ''' ``mHG(lambdas) = min over 1 &lt;= n &lt;= N of HGT (b_n(lambdas); N, B, n)``
    '''
    ''' Where ``HGT`` Is the hypergeometric tail:
    '''
    ''' ``HGT(b; N, B, n) = Probability(X &gt;= b)``
    '''
    ''' And: ``b_n = sum over 1 &lt;= i &lt;= n of lambdas[i]``
    '''
    ''' If several n gives the same mHG, then the lowest one Is taken.
    ''' </summary>
    ''' <param name="lambdas">sorted And labeled ``{0,1}^N``.</param>
    ''' <param name="n_max#">the algorithm will only consider the first ``n_max`` partitions.</param>
    ''' <returns></returns>
    Public Function mHGstatisticcalc(lambdas As Vector, Optional n_max# = Double.NaN) As mHGstatisticInfo
        n_max = n_max Or CDbl(lambdas.Length).AsDefault.When(n_max.IsNaNImaginary)

        ' Input check
        ' stopifnot(n_max > 0)
        ' stopifnot(n_max <= length(lambdas))
        ' stopifnot(length(lambdas) > 0)
        ' stopifnot(all(lambdas == 1 | lambdas == 0))

        Dim N% = lambdas.Length
        Dim B% = CInt(lambdas.Sum)

        Dim mHG# = 1
        Dim mHGn% = 0
        Dim mHGb% = 0
        Dim m% = 0 ' Last time we saw a one
        ' The first B + 1 hypergeometric probabilities: HG_row(i) = Prob(X == i),
        ' updated for the current number of tries n.
        Dim HG_row As New Vector(B + 1)
        HG_row(0) = 1 ' For n = 0, b = 0

        ' bCount and nCount are the loop counters, while N and B are the total counts.
        ' (VisualBasic identifiers are case insensitive, so the counters cannot be named
        ' as "b"/"n" while "B"/"N" are in use.)
        Dim bCount% = 0
        Dim nCount% = 0
        Do While (nCount < n_max)  ' iterating On different N To find minimal HGT
            nCount += 1
            bCount += CInt(lambdas(nCount - 1))

            If (lambdas(nCount - 1) = 1.0R) Then
                ' Only Then HGT can decrease (see p. 19 In thesis)
                HG_row = HG_row_ncalc_func(HG_row, m, nCount, bCount, N, B)
                m = nCount

                Dim HGT# = 1 - sumFirst(HG_row, bCount)  ' P(X >= b) = 1 - P(X < b)

                ' statistic
                If (HGT < mHG) Then
                    mHG = HGT
                    mHGn = nCount
                    mHGb = bCount
                End If
            End If
        Loop
        Return New mHGstatisticInfo With {.mHG = mHG, .n = mHGn, .b = mHGb}
    End Function

    ''' <summary>
    ''' Calculates the p-value associated with the mHG statistic.
    ''' Guidelines for the calculation are to be found in:
    ''' 
    ''' > Eden, E. (2007). Discovering Motifs in Ranked Lists of DNA Sequences. Haifa. 
    ''' > Retrieved from http://bioinfo.cs.technion.ac.il/people/zohar/thesis/eran.pdf
    ''' > (pages 11-12, 19-20)
    ''' </summary>
    ''' <param name="p">the mHG statistic. Marked as p, as it represenets an "uncorrected" p-value.</param>
    ''' <param name="N">total number of white And black balls (according to the hypergeometric problem definition).</param>
    ''' <param name="B">number of black balls.</param>
    ''' <param name="n_max">
    ''' the algorithm will calculate the p-value under the null hypothesis that only the 
    ''' first n_max partitions are taken into account in determining in minimum.
    ''' </param>
    ''' <returns>p-value.</returns>
    Public Function mHGpvalcalc(p#, N%, B#, Optional n_max# = Double.NaN) As Double
        n_max = n_max Or CDbl(N).AsDefault.When(n_max.IsNaNImaginary)

        ' Input check
        'stopifnot(n_max > 0)
        'stopifnot(n_max <= N)
        'stopifnot(N >= B)
        'stopifnot(p <= 1)

        If (p < -EPSILON) Then
            Call VBDebugger.warning("p-value calculation will be highly inaccurate due to an extremely small mHG statistic")
        End If

        ' p - the statistic.
        ' N\B - number Of all \ black balls.
        Dim nB% = CInt(B)
        Dim nMax% = CInt(n_max)
        Dim W% = N - nB
        Dim R_separation_line As Vector = R_separation_linecalc(p, N, nB, nMax)
        Dim pi_r As NumericMatrix = pi_rcalc(N, nB, R_separation_line)

        ' R: p_corrected = 1 - pi_r[W + 2, B + 2]
        Dim p_corrected# = 1 - pi_r(W + 1, nB + 1)

        Return p_corrected
    End Function

    ''' <summary>
    ''' Determine R separation line - This Is the highest (broken) line crossing the B*W matrix horizontally, that underneath it all
    ''' the associated p-values are higher than p, Or w + b &gt; n_max.
    '''
    ''' (This Is a bit different from the original definition To make the calculation more efficient)
    ''' </summary>
    ''' <param name="p">the mHG statistic. Marked As p, As it represenets an "uncorrected" p-value.</param>
    ''' <param name="N">total number Of white And black balls (according To the hypergeometric problem definition).</param>
    ''' <param name="B">number Of black balls.</param>
    ''' <param name="n_max">
    ''' Part Of the constraint On the line, the null hypothesis Is calculated under
    ''' the assumption that the first n_max partitions are taken into account In determining the minimum.
    ''' </param>
    ''' <returns>
    ''' R_separation_line - represented As a vector size B + 1, index b containing 
    ''' the first (high enough) w To the right Of the R separation line (Or W + 1 If no such w exist).
    ''' </returns>
    ''' <remarks>
    ''' See Eden, E. (2007) pages 11-12.
    ''' </remarks>
    Public Function R_separation_linecalc(p#, N%, B%, n_max%) As Vector
        Dim W% = N - B
        ' R: R_separation_line <- rep(W + 1, times = B + 1)
        Dim R_separation_line As New Vector(Repeats(CDbl(W + 1), times:=B + 1))

        ' R: HG_row <- numeric(B); HG_row[1] <- 1
        Dim HG_row As New Vector(B + 1)
        HG_row(0) = 1 ' For n = 0, b = 0

        Dim bi% = 0
        Dim wi% = 0
        Dim HGT# = 1 ' Initial HGT

        ' We are tracing the R line - increasing b until we Get To a cell where the associated p-values are smaller
        ' than p, And Then increasing w until we Get To a cell where the associated p-values are bigger than p
        Dim should_inc_w As Boolean = (HGT <= (p + EPSILON)) AndAlso (wi < W) AndAlso (bi <= (n_max - wi))
        Dim should_inc_b As Boolean = (HGT > (p + EPSILON)) AndAlso (bi < B) AndAlso (bi <= (n_max - wi))

        Do While (should_inc_w OrElse should_inc_b)
            Do While (should_inc_b)  ' Increase b until we get to the R line (or going outside the n_max zone)
                R_separation_line(bi) = wi
                bi += 1
                HG_row(bi) = HG_row(bi - 1) * d_ratio(bi + wi, bi, N, B)
                ' R: HG_row[1:b] <- HG_row[1:b] * v_ratio(b + w, 0:(b-1), N, B)
                For k As Integer = 0 To bi - 1
                    HG_row(k) = HG_row(k) * v_ratio(bi + wi, k, N, B)
                Next
                HGT = 1 - sumFirst(HG_row, bi)  ' P(X >= b) = 1 - P(X <b)
                should_inc_b = (HGT > (p + EPSILON)) AndAlso (bi < B) AndAlso (bi <= (n_max - wi))
            Loop

            If (bi > (n_max - wi)) Then
                ' We can Stop immediately And we Do Not need To calculate HG_row anymore
                ' R: R_separation_line[(b+1):(B+1)] <- w
                For k As Integer = bi To B
                    R_separation_line(k) = wi
                Next
                should_inc_w = False
            Else
                should_inc_w = (HGT <= (p + EPSILON)) AndAlso (wi < W)
                Do While (should_inc_w) ' Increase w until we get outside the R line (or going outside the n_max zone)
                    wi += 1
                    ' R: HG_row[1:(b+1)] <- HG_row[1:(b+1)] * v_ratio(b + w, 0:b, N, B)
                    For k As Integer = 0 To bi
                        HG_row(k) = HG_row(k) * v_ratio(bi + wi, k, N, B)
                    Next
                    HGT = 1 - sumFirst(HG_row, bi)  ' P(X >= b) = 1 - P(X <b)
                    should_inc_w = (HGT <= (p + EPSILON)) AndAlso (wi < W) AndAlso (bi <= (n_max - wi))
                Loop
                If (bi > (n_max - wi)) Then
                    ' We can Stop immediately And we Do Not need To calculate HG_row anymore
                    ' R: R_separation_line[(b+1):(B+1)] <- w
                    For k As Integer = bi To B
                        R_separation_line(k) = wi
                    Next
                    should_inc_b = False
                Else
                    should_inc_b = (HGT > (p + EPSILON)) AndAlso (bi < B) AndAlso (bi <= (n_max - wi))
                End If
            End If
        Loop

        If (HGT > (p + EPSILON)) Then ' Last one
            ' R: R_separation_line[b+1] <- w
            R_separation_line(bi) = wi
        End If

        Return R_separation_line
    End Function

    ''' <summary>
    ''' Consider an urn With N balls, B Of which are black And W white. pi_r stores 
    ''' The probability Of drawing w white And b black balls In n draws (n = w + b)
    ''' With the constraint Of P(w,b) = 0 If (w, b) Is On Or above separation line.
    ''' 
    ''' R's row 1 Of the matrix represents w = -1, col 1 represents b = -1; therefore
    ''' the 0-based matrix here uses row 0 / col 0 fer the ``w = -1``/``b = -1`` padding.
    ''' </summary>
    ''' <param name="N">total number Of white And black balls (according To the hypergeometric problem definition).</param>
    ''' <param name="B">number Of black balls.</param>
    ''' <param name="R_separation_line">
    ''' represented As a vector size B + 1, index b containing 
    ''' the first (high enough) w To the right Of the R separation line.
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>
    ''' See Eden, E. (2007) page 20.
    ''' </remarks>
    Public Function pi_rcalc(N%, B%, R_separation_line As Vector) As NumericMatrix
        Dim W% = N - B
        Dim pi_r As NumericMatrix = NumericMatrix.Create(nrow:=W + 2, ncol:=B + 2)

        ' NOTE: Different from the thesis (see page 20 last paragraph),
        ' should be 1 according To that paragraph, but this seems wrong.
        ' The R statements ``pi_r[1,] <- 0`` and ``pi_r[,1] <- 0`` are already
        ' satisfied by the zero initialized matrix.

        For bi As Integer = 0 To B
            Dim wi As Integer = CInt(R_separation_line(bi))
            Do While (wi < (W + 1))
                If ((wi = 0) AndAlso (bi = 0)) Then
                    ' Note, this cell will be 0 if it's left to the R separation line (should not occure)
                    ' R: pi_r[2, 2] <- 1
                    pi_r(1, 1) = 1
                Else
                    ' Apply the recursion rule:
                    ' P(w,b) = P((w,b)|(w-1,b))*P(w-1,b)+P((w,b)|(w,b-1))*P(w,b-1)
                    ' R: pi_r[w + 2, b + 2] <- ... pi_r[w + 1, b + 2] ... pi_r[w + 2, b + 1]
                    pi_r(wi + 1, bi + 1) = (W - wi + 1) / (B + W - bi - wi + 1) * pi_r(wi, bi + 1) +
                        (B - bi + 1) / (B + W - bi - wi + 1) * pi_r(wi + 1, bi)
                End If
                wi += 1
            Loop
        Next
        Return pi_r
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
    Private Function HG_row_ncalc_func(HG_row_m As Vector, m%, ni%, b_n#, N%, B%, Optional RECURSION_OVERHEAD_MULTIPLIER% = 20) As Vector
        Dim HG_row_ncalcfunc As Calculator = Nothing

        ' R: if ((n - m) <= (RECURSION_OVERHEAD_MULTIPLIER * log2(b_n)))
        If ((ni - m) <= (RECURSION_OVERHEAD_MULTIPLIER * std.Log(b_n, 2))) Then
            HG_row_ncalcfunc = AddressOf HG_row_ncalc.iter
        Else
            HG_row_ncalcfunc = AddressOf HG_row_ncalc.recur
        End If

        Return HG_row_ncalcfunc(HG_row_m, m, ni, b_n, N, B)
    End Function

    ''' <summary>
    ''' The ratio between HG(n,b,B,N) And HG(n-1,b-1,B,N)
    ''' See page 19 In Eden's thesis.
    '''
    ''' equals to the R expression: ``d_ratio = n * (B - (b - 1)) / (b * (N - (n - 1)))``
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function d_ratio(ni#, bi#, N#, B#) As Double
        Return ni * (B - (bi - 1)) / (bi * (N - (ni - 1)))
    End Function

    ''' <summary>
    ''' The ratio between HG(n,b,B,N) And HG(n-1,b,B,N)
    ''' See page 19 In Eden's thesis.
    '''
    ''' equals to the R expression: ``v_ratio = (n * (N - n - B + b + 1)) / ((n - b) * (N - n + 1))``
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function v_ratio(ni#, bi#, N#, B#) As Double
        Return (ni * (N - ni - B + bi + 1)) / ((ni - bi) * (N - ni + 1))
    End Function

    ''' <summary>
    ''' The vectorized version of <see cref="v_ratio(Double, Double, Double, Double)"/>,
    ''' equals to the R call ``v_ratio(n, b, N, B)`` when ``b`` is a vector of integers.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function v_ratio(ni#, bi As Vector, N#, B#) As Vector
        Return New Vector(bi.Select(Function(x) v_ratio(ni, x, N, B)))
    End Function

End Module
