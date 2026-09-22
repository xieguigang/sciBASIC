#Region "Microsoft.VisualBasic::972df8e72065139eb2f46cdb218d2fc6, Data_science\Mathematica\Math\mHG\test\ReferenceImpl.vb"

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

    '   Total Lines: 309
    '    Code Lines: 185 (59.87%)
    ' Comment Lines: 53 (17.15%)
    '    - Xml Docs: 90.57%
    ' 
    '   Blank Lines: 71 (22.98%)
    '     File Size: 9.89 KB


    ' Module ReferenceImpl
    ' 
    '     Function: Combinations, EnrichedLambdas, ExactPermutationPValue, HypergeometricPmf, HypergeometricTail
    '               LogChoose, LogFactorial, mHGStatisticSimple, RandomLambdas
    '     Class Lcg
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: [Next], NextDouble
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.Linq

''' <summary>
''' An independent (naive) reference implementation of the minimum-hypergeometric test.
'''
''' Nothing in this module uses any helper of the ``mHG.vbproj`` project, so that the results
''' of the library under test can be validated against a completely separate computation path:
'''
''' + the hypergeometric distribution is evaluated with log-factorials (log-sum-exp) instead
'''   of the ratio-based dynamic programming used by the library;
''' + the p-value is obtained by brute force enumeration of all the ``C(N, B)`` rearrangements
'''   of the ranked binary list instead of the R-separation-line / ``pi_r`` dynamic programming.
'''
''' NOTE: VisualBasic identifiers are case-insensitive, so the parameter names used here
''' deliberately avoid the ``N``/``n`` and ``B``/``b`` pairs.
''' </summary>
Public Module ReferenceImpl

    Public Const EPSILON# = 0.0000000001

#Region "Hypergeometric distribution"

    Private ReadOnly logFactorialCache As New Dictionary(Of Integer, Double)

    ''' <summary>
    ''' ``log(n!)``
    ''' </summary>
    Public Function LogFactorial(n As Integer) As Double
        If n < 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(n))
        End If
        If n <= 1 Then
            Return 0.0
        End If

        Dim cached As Double = 0

        If logFactorialCache.TryGetValue(n, cached) Then
            Return cached
        End If

        cached = LogFactorial(n - 1) + System.Math.Log(CDbl(n))
        logFactorialCache(n) = cached

        Return cached
    End Function

    ''' <summary>
    ''' ``log(C(n, k))``, or ``-Inf`` for the out of range ``k``.
    ''' </summary>
    Public Function LogChoose(n As Integer, k As Integer) As Double
        If k < 0 OrElse k > n OrElse n < 0 Then
            Return Double.NegativeInfinity
        End If
        Return LogFactorial(n) - LogFactorial(k) - LogFactorial(n - k)
    End Function

    ''' <summary>
    ''' The hypergeometric probability mass function ``P(X = hits)`` for the urn
    ''' containing ``numBalls`` balls among which ``numBlack`` are black, ``draws`` draws.
    ''' </summary>
    Public Function HypergeometricPmf(hits As Integer, numBalls As Integer,
                                      numBlack As Integer, draws As Integer) As Double
        Dim lo As Integer = System.Math.Max(0, draws - (numBalls - numBlack))
        Dim hi As Integer = System.Math.Min(draws, numBlack)

        If hits < lo OrElse hits > hi Then
            Return 0.0
        End If

        Return System.Math.Exp(LogChoose(numBlack, hits) +
                               LogChoose(numBalls - numBlack, draws - hits) -
                               LogChoose(numBalls, draws))
    End Function

    ''' <summary>
    ''' The hypergeometric tail ``P(X &gt;= hits)``, which equals to the R expression
    ''' ``phyper(hits - 1, B, N - B, n, lower.tail = FALSE)``.
    ''' </summary>
    Public Function HypergeometricTail(hits As Integer, numBalls As Integer,
                                       numBlack As Integer, draws As Integer) As Double
        Dim lo As Integer = System.Math.Max(hits, 0)
        Dim hi As Integer = System.Math.Min(draws, numBlack)

        If lo > hi Then
            Return 0.0
        End If

        Dim logs As New List(Of Double)

        For k As Integer = lo To hi
            logs.Add(LogChoose(numBlack, k) +
                     LogChoose(numBalls - numBlack, draws - k) -
                     LogChoose(numBalls, draws))
        Next

        ' numeric stable log-sum-exp
        Dim maxLog As Double = logs.Max
        Dim sum As Double = 0

        For Each x As Double In logs
            sum += System.Math.Exp(x - maxLog)
        Next

        Dim total As Double = System.Math.Exp(maxLog) * sum

        If total > 1.0 Then
            total = 1.0
        ElseIf total < 0.0 Then
            total = 0.0
        End If

        Return total
    End Function

#End Region

#Region "mHG statistic (direct definition)"

    ''' <summary>
    ''' The mHG statistic computed straight from its definition, without any dynamic programming:
    '''
    ''' ``min over 1 &lt;= cutoff &lt;= n_max of P(X &gt;= hits)``, where ``hits`` is the number of
    ''' ones among the first ``cutoff`` elements of the ranked list.
    ''' </summary>
    Public Function mHGStatisticSimple(lambdas As Integer(),
                                       Optional n_max As Integer = -1) As (mhg As Double, n As Integer, b As Integer)
        Dim numBalls As Integer = lambdas.Length
        Dim numBlack As Integer = lambdas.Sum

        If n_max < 0 Then
            n_max = numBalls
        End If

        Dim mhg As Double = 1
        Dim bestN As Integer = 0
        Dim bestB As Integer = 0
        Dim hits As Integer = 0

        For cutoff As Integer = 1 To n_max
            hits += lambdas(cutoff - 1)

            Dim hgt As Double = HypergeometricTail(hits, numBalls, numBlack, cutoff)

            If hgt < mhg Then
                mhg = hgt
                bestN = cutoff
                bestB = hits
            End If
        Next

        Return (mhg, bestN, bestB)
    End Function

#End Region

#Region "Exact permutation p-value (brute force)"

    ''' <summary>
    ''' Enumerates all the ``k``-subsets of ``{0, 1, ..., n - 1}`` in lexicographic order.
    ''' </summary>
    Public Iterator Function Combinations(n As Integer, k As Integer) As IEnumerable(Of Integer())
        If k < 0 OrElse k > n Then
            Return
        End If

        Dim idx As Integer() = Enumerable.Range(0, k).ToArray

        While True
            Yield idx.ToArray

            Dim i As Integer = k - 1

            While i >= 0 AndAlso idx(i) = i + n - k
                i -= 1
            End While

            If i < 0 Then
                Exit While
            End If

            idx(i) += 1

            For j As Integer = i + 1 To k - 1
                idx(j) = idx(j - 1) + 1
            Next
        End While
    End Function

    ''' <summary>
    ''' The exact mHG p-value obtained by enumerating every rearrangement of the ranked binary
    ''' list: ``count(statistic &lt;= observed) / C(N, B)``.
    ''' </summary>
    Public Function ExactPermutationPValue(lambdas As Integer(),
                                           Optional n_max As Integer = -1) As Double
        Dim numBalls As Integer = lambdas.Length
        Dim numBlack As Integer = lambdas.Sum

        If n_max < 0 Then
            n_max = numBalls
        End If

        Dim observed As Double = mHGStatisticSimple(lambdas, n_max).mhg
        Dim hitCount As Long = 0
        Dim totalCount As Long = 0

        For Each ones As Integer() In Combinations(numBalls, numBlack)
            Dim arrangement As Integer() = New Integer(numBalls - 1) {}

            For Each i As Integer In ones
                arrangement(i) = 1
            Next

            Dim statistic As Double = mHGStatisticSimple(arrangement, n_max).mhg

            If statistic <= (observed + EPSILON) Then
                hitCount += 1
            End If

            totalCount += 1
        Next

        Return CDbl(hitCount) / CDbl(totalCount)
    End Function

#End Region

#Region "Deterministic test data generator"

    ''' <summary>
    ''' A tiny deterministic linear congruential generator, used to make the demo/random test
    ''' cases reproducible across runs (``System.Random`` may vary between runtime versions).
    ''' </summary>
    Public Class Lcg

        Private state As Long

        Public Sub New(seed As Long)
            state = seed And &H7FFFFFFFL
        End Sub

        Public Function [Next](bound As Integer) As Integer
            state = (state * 1103515245L + 12345L) And &H7FFFFFFFL
            Return CInt(state Mod bound)
        End Function

        Public Function NextDouble() As Double
            Return [Next](1000000) / 1000000.0
        End Function
    End Class

    ''' <summary>
    ''' shuffle ``numBlack`` ones into a list of ``numBalls`` elements.
    ''' </summary>
    Public Function RandomLambdas(numBalls As Integer, numBlack As Integer, rnd As Lcg) As Integer()
        Dim x As Integer() = New Integer(numBalls - 1) {}

        For i As Integer = 0 To numBlack - 1
            x(i) = 1
        Next

        For i As Integer = numBalls - 1 To 1 Step -1
            Dim j As Integer = rnd.Next(i + 1)
            Dim tmp As Integer = x(i)
            x(i) = x(j)
            x(j) = tmp
        Next

        Return x
    End Function

    ''' <summary>
    ''' A "biased towards the top" ranked binary list, simulating a GO term whose annotated
    ''' genes are enriched among the top ranked (e.g. differentially expressed) genes.
    ''' </summary>
    Public Function EnrichedLambdas(numBalls As Integer, numBlack As Integer, rnd As Lcg) As Integer()
        Dim x As Integer() = New Integer(numBalls - 1) {}
        Dim placed As Integer = 0
        Dim i As Integer = 0

        While placed < numBlack AndAlso i < numBalls
            Dim p As Double = If(i < (numBalls \ 2), 0.13, 0.07)

            If rnd.NextDouble() < p Then
                x(i) = 1
                placed += 1
            End If

            i += 1
        End While

        Dim k As Integer = numBalls - 1

        While placed < numBlack AndAlso k >= 0
            If x(k) = 0 Then
                x(k) = 1
                placed += 1
            End If

            k -= 1
        End While

        Return x
    End Function

#End Region

End Module
