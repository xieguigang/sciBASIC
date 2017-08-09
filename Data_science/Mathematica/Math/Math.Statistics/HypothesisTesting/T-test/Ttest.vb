#Region "Microsoft.VisualBasic::f5f3baed00a8f5ff185fbdbcde0a2ab0, ..\sciBASIC#\Data_science\Mathematica\Math\Math.Statistics\HypothesisTesting\T-test\Ttest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Statistics.MomentFunctions
Imports Microsoft.VisualBasic.Math.StatisticsMathExtensions

Namespace Hypothesis

    ''' <summary>
    ''' Performs one and two sample t-tests on vectors of data.
    ''' </summary>
    Public Module t

        ''' <summary>
        ''' Performs one sample t-tests on vectors of data.
        ''' </summary>
        ''' <param name="x">a (non-empty) numeric vector of data values.</param>
        ''' <param name="mu#">a number indicating the True value Of the mean (Or difference In means If you are performing a two sample test).</param>
        ''' <param name="alpha#"></param>
        ''' <param name="alternative">specifying the alternative hypothesis</param>
        ''' <remarks>``ttest({0,1,1,1}, mu:= 1).valid() = True``</remarks>
        ''' <returns></returns>
        Public Function Test(x As IEnumerable(Of Double),
                             Optional alternative As Hypothesis = Hypothesis.TwoSided,
                             Optional mu# = 0,
                             Optional alpha# = 0.05) As TtestResult

            Dim sample As New BasicProductMoments(x)

            Return New TtestResult With {
                .alpha = alpha,
                .DegreeFreedom = sample.SampleSize - 1,
                .StdErr = Math.Sqrt(x.Variance / sample.SampleSize),
                .TestValue = (sample.Mean - mu) / .StdErr,
                .Pvalue = Pvalue(.TestValue, .DegreeFreedom, alternative),
                .Mean = sample.Mean,
                .Alternative = alternative
            }
        End Function

        ''' <summary>
        ''' Performs two sample t-tests on vectors of data.
        ''' </summary>
        ''' <param name="a">a (non-empty) numeric vector of data values.</param>
        ''' <param name="b">a (non-empty) numeric vector of data values.</param>
        ''' <param name="mu#">a number indicating the True value Of the mean (Or difference In means If you are performing a two sample test).</param>
        ''' <param name="alpha#"></param>
        ''' <param name="alternative">specifying the alternative hypothesis</param>
        ''' <param name="varEqual">Default using **student's t-test**, set this parameter to False using **Welch's t-test**</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' ``ttest({0,1,1,1}, {1,2,2,2}, mu:= -1).valid() = True``
        ''' </remarks>
        Public Function Test(a As IEnumerable(Of Double),
                             b As IEnumerable(Of Double),
                             Optional alternative As Hypothesis = Hypothesis.TwoSided,
                             Optional mu# = 0,
                             Optional alpha# = 0.05,
                             Optional varEqual As Boolean = True) As TwoSampleResult

            Dim va#() = a.ToArray, vb = b.ToArray
            Dim left As New BasicProductMoments(a)
            Dim right As New BasicProductMoments(b)
            Dim v# = If(varequal,
                left.SampleSize + right.SampleSize - 2,
                __welch2df(va.Variance, vb.Variance, left.SampleSize, right.SampleSize))
            Dim commonVariance# = ((left.SampleSize - 1) * va.Variance + (right.SampleSize - 1) * vb.Variance) / v

            Return New TwoSampleResult With {
                .alpha = alpha,
                .DegreeFreedom = v,
                .Mean = left.Mean - right.Mean,
                .StdErr = Math.Sqrt(commonVariance * (1 / left.SampleSize + 1 / right.SampleSize)),
                .TestValue = If(varEqual,
                    (.Mean - mu) / .StdErr,
                    __welch2t(left.Mean, right.Mean, va.Variance, vb.Variance, left.SampleSize, right.SampleSize)),
                .Pvalue = Pvalue(.TestValue, v, alternative),
                .Alternative = alternative,
                .MeanX = left.Mean,
                .MeanY = right.Mean
            }
        End Function

        Private Function __welch2t(m1#, m2#, s1#, s2#, N1#, N2#) As Double
            Dim a = m1 - m2
            Dim b = Math.Sqrt((s1 ^ 2) / N1 + (s2 ^ 2) / N2)
            Dim t = a / b
            Return t
        End Function

        Private Function __welch2df(s1#, s2#, N1#, N2#) As Double
            Dim v1 = N1 - 1
            Dim v2 = N2 - 1
            Dim a = (s1 ^ 2 / N1 + s2 ^ 2 / N2) ^ 2
            Dim b = (s1 ^ 4) / ((N1 ^ 2) * v1) + (s2 ^ 4) / ((N2 ^ 2) * v2)
            Dim v = a / b
            Return v
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="t#">The t test value</param>
        ''' <param name="v">v is the degrees of freedom</param>
        ''' <returns></returns>
        Public Function Pvalue(t#, v#, Optional hyp As Hypothesis = Hypothesis.TwoSided) As Double
            Select Case hyp
                Case Hypothesis.Greater
                    Return 1 - Tcdf(t, v)
                Case Hypothesis.Less
                    Return Tcdf(t, v)
                Case Else
                    Return 2 * (1 - Tcdf(Math.Abs(t), v))
            End Select
        End Function

        ''' <summary>
        ''' ###### Student's t-distribution CDF
        ''' 
        ''' https://en.wikipedia.org/wiki/Student%27s_t-distribution#Non-standardized_Student.27s_t-distribution
        ''' </summary>
        ''' <param name="t">Only works for ``t > 0``</param>
        ''' <param name="v">v is the degrees of freedom</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' ###### 2017-1-11 test success!
        ''' 
        ''' ```
        ''' tcdf(1,1) = 0.75
        ''' tcdf(0,5) = 0.5
        ''' ```
        ''' </remarks>
        Public Function Tcdf(t#, v#) As Double
            Dim x# = v / (v + t ^ 2)
            Dim inc = SpecialFunctions.RegularizedIncompleteBetaFunction(v / 2, 0.5, x)
            Dim cdf# = 1 - 0.5 * inc
            Return cdf
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="t"></param>
        ''' <param name="v#"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' ```
        ''' Tcdf({0, 2, 4}, 5) = {0.5, 0.949, 0.995}
        ''' ```
        ''' </remarks>
        Public Function Tcdf(t As Vector, v#) As Vector
            Return New Vector(t.Select(Function(x) Tcdf(x, v)))
        End Function
    End Module
End Namespace
