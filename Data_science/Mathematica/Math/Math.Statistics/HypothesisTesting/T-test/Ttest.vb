#Region "Microsoft.VisualBasic::890c1c9eed2c21b5e3e12acc604443a1, Data_science\Mathematica\Math\Math.Statistics\HypothesisTesting\T-test\Ttest.vb"

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

    '   Total Lines: 274
    '    Code Lines: 134
    ' Comment Lines: 114
    '   Blank Lines: 26
    '     File Size: 15.11 KB


    '     Module t
    ' 
    '         Function: Pvalue, (+2 Overloads) Tcdf, (+2 Overloads) Test, welch2df, welch2t
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Statistics.Linq
Imports Microsoft.VisualBasic.Math.Statistics.MomentFunctions
Imports Microsoft.VisualBasic.Serialization.JSON
Imports stdNum = System.Math

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
            Dim opt As New Topt With {
                .alpha = alpha,
                .mu = mu,
                .alternative = alternative
            }
            Dim pvalueAlter As Hypothesis = alternative

            If alternative <> Hypothesis.TwoSided Then
                If alternative = Hypothesis.Less Then
                    pvalueAlter = Hypothesis.Greater
                Else
                    pvalueAlter = Hypothesis.Less
                End If
            End If

            Return New TtestResult With {
                .DegreeFreedom = sample.SampleSize - 1,
                .SD = stdNum.Sqrt(sample.Variance),
                .StdErr = stdNum.Sqrt(.SD ^ 2 / sample.SampleSize),
                .TestValue = (sample.Mean - mu) / .StdErr,
                .Pvalue = Pvalue(.TestValue, .DegreeFreedom, pvalueAlter),
                .Mean = sample.Mean,
                .opt = opt,
                .x = sample.ToArray
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
                             Optional varEqual As Boolean = True,
                             Optional [strict] As Boolean = True) As TwoSampleResult

            Dim va#() = a.ToArray, vb = b.ToArray
            Dim left As New BasicProductMoments(a)
            Dim right As New BasicProductMoments(b)
            ' degree of freedom
            Dim df#

            If varEqual Then
                df = left.SampleSize + right.SampleSize - 2
            Else
                df = welch2df(va.Variance, vb.Variance, left.SampleSize, right.SampleSize)
            End If

            Dim commonVariance# = ((left.SampleSize - 1) * va.Variance + (right.SampleSize - 1) * vb.Variance) / df
            Dim testVal#
            Dim stdErr# = stdNum.Sqrt(commonVariance * (1 / left.SampleSize + 1 / right.SampleSize))

            If varEqual Then
                testVal = ((left.Mean - right.Mean) - mu) / stdErr
            Else
                testVal = welch2t(left.Mean, right.Mean, va.Variance, vb.Variance, left.SampleSize, right.SampleSize)
            End If

            Dim pvalue#
            Dim opt As New Topt With {
                .alpha = alpha,
                .alternative = alternative,
                .mu = mu
            }

            Try
                pvalue = t.Pvalue(testVal, df, alternative)
            Catch ex As Exception
                Dim debug As String = $"x = {va.GetJson}; y = {vb.GetJson}"

                ' 20230425
                '
                '  Error in <globalEnvironment> -> InitializeEnvironment -> using(pool) -> for_loop_[[1]: "./FD20221200635-negative"] -> for_loop_[[137]: "E:\spectrum-taxonomy\test\blood_pool_20230426\FD20221200635-nega..."] -> "addPool"(&data, "biosample" <- "blood", "...) -> addPool
                '   1. OverflowException: Value was either too large or too small for a Decimal.
                '   2. stackFrames:
                '    at System.Number.ThrowOverflowException(TypeCode type)
                '    at System.Decimal.DecCalc.VarDecFromR8(Double input, DecCalc& result)
                '    at Microsoft.VisualBasic.Math.Statistics.Hypothesis.t.Tcdf(Double t, Double v) 
                '    at Microsoft.VisualBasic.Math.Statistics.Hypothesis.t.Pvalue(Double t, Double v, Hypothesis hyp) 
                '    at Microsoft.VisualBasic.Math.Statistics.Hypothesis.t.Test(IEnumerable`1 a, IEnumerable`1 b, Hypothesis alternative, Double mu, Double alpha, Boolean varEqual) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData.SpectrumPool.Add(PeakMs2 spectrum) 
                '    at mzkit.MolecularSpectrumPool.add(SpectrumPool pool, Object x, String biosample, String organism, String project, String instrument, String file, Environment env) 

                '    Call "addPool"(&pool, &data, "biosample" <- "blood", "organism" <- "Homo sapiens", "project" <- &proj_id, "file" <- Call "basename"(&file), "instrument" <- "Thermo Scientific Q Exactive")
                '    ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

                ' spectrumPool.R#_interop::.addPool at mzDIA.dll:line <unknown>
                ' SMRUCC/R#.call_function."addPool"(&data, "biosample" <- "blood", "...) at http_human_blood-neg.R:line 32
                ' SMRUCC/R#.forloop.for_loop_[[137]: "E:\spectrum-taxonomy\test\blood_pool_20230426\FD20221200635-nega..."] at http_human_blood-neg.R:line 28
                ' SMRUCC/R#.forloop.for_loop_[[1]: "./FD20221200635-negative"] at http_human_blood-neg.R:line 19
                ' SMRUCC/R#.using_closure.using(pool) at http_human_blood-neg.R:line 16
                ' SMRUCC/R#.n/a.InitializeEnvironment at http_human_blood-neg.R:line 0
                ' SMRUCC/R#.global.<globalEnvironment> at <globalEnvironment>:line n/a

                If strict Then
                    Throw New InvalidProgramException(debug, ex)
                Else
                    ' evaluate the pvalue failure
                    If va.Average / vb.Average <> 1 Then
                        pvalue = 1.0E-17
                    Else
                        pvalue = Double.NaN
                    End If
                End If
            End Try

            If pvalue = 0.0 Then
                pvalue = 1.0E-100
            End If

            Return New TwoSampleResult With {
                .DegreeFreedom = df,
                .Mean = mu,
                .StdErr = stdErr,
                .SD = stdErr,
                .TestValue = testVal,
                .Pvalue = pvalue,
                .opt = opt,
                .MeanX = left.Mean,
                .MeanY = right.Mean,
                .x = va,
                .y = vb
            }
        End Function

        Private Function welch2t(m1#, m2#, s1#, s2#, N1#, N2#) As Double
            Dim a = m1 - m2
            Dim b = stdNum.Sqrt((s1 ^ 2) / N1 + (s2 ^ 2) / N2)
            Dim t = a / b
            Return t
        End Function

        Private Function welch2df(s1#, s2#, N1#, N2#) As Double
            Dim v1 = N1 - 1
            Dim v2 = N2 - 1
            Dim a = (s1 ^ 2 / N1 + s2 ^ 2 / N2) ^ 2
            Dim b = (s1 ^ 4) / ((N1 ^ 2) * v1) + (s2 ^ 4) / ((N2 ^ 2) * v2)
            Dim v = a / b
            Return v
        End Function

        ''' <summary>
        ''' two sample p-value
        ''' </summary>
        ''' <param name="t#">The t test value</param>
        ''' <param name="v">v is the degrees of freedom</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 请注意，双样本检测与单样本检测的pvalue在less和greater是反过来的
        ''' </remarks>
        Public Function Pvalue(t#, v#, Optional hyp As Hypothesis = Hypothesis.TwoSided) As Decimal
            Select Case hyp
                Case Hypothesis.Less
                    Return d128_one - Tcdf(t, v)
                Case Hypothesis.Greater
                    Return Tcdf(t, v)
                Case Else
                    Return d128_two * (d128_one - Tcdf(stdNum.Abs(t), v))
            End Select
        End Function

        Const d128_one As Decimal = 1.0
        Const d128_two As Decimal = 2.0

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
        Public Function Tcdf(t#, v#) As Decimal
            Dim x# = v / (v + t ^ 2)
            Dim inc As Decimal = SpecialFunctions.RegularizedIncompleteBetaFunction(v / 2.0, 0.5, x)
            ' there is a bug about the precision in small number
            ' this problem case the pvalue zero
            Dim cdf As Decimal = d128_one - inc / d128_two

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
            Return New Vector(t.Select(Function(x) CDbl(Tcdf(x, v))))
        End Function
    End Module
End Namespace
