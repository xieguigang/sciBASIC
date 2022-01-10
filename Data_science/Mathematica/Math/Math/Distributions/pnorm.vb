#Region "Microsoft.VisualBasic::93e5dc8f01ee650c736b33059c090441, Data_science\Mathematica\Math\Math\Distributions\pnorm.vb"

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

    '     Module pnorm
    ' 
    '         Function: AboveStandardDistribution, BelowStandardDistribution, BetweenStandardDistribution, DeviationStandardization, (+2 Overloads) Eval
    '                   Logistic, OutsideStandardDistribution, (+2 Overloads) ProbabilityDensity, StandardDistribution, TrapezodialRule
    '                   TruncNDist, (+2 Overloads) Z
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Scripting.Rscript.MathExtension
Imports stdNum = System.Math

Namespace Distributions

    ''' <summary>
    ''' 正态分布帮助模块
    ''' </summary>
    Public Module pnorm

        ''' <summary>
        ''' ###### Z-score 标准化(zero-mean normalization)
        ''' 
        ''' 也叫标准差标准化，经过处理的数据符合标准正态分布，即均值为0，标准差为1
        ''' 其中<paramref name="μ"/>为所有样本数据的均值，<paramref name="σ"/>为所有样本数据的标准差。
        ''' </summary>
        ''' <param name="x#">Sample data</param>
        ''' <param name="μ#">μ is the mean of the population.</param>
        ''' <param name="σ#">σ is the standard deviation of the population.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 注：是否要进行标准化，要根据具体实验定。如果特征非常稀疏，并且有大量的0（现实应用中很多特征都具有这个特点），
        ''' ``Z-score`` 标准化的过程几乎就是一个除0的过程，结果不可预料。
        ''' </remarks>
        Public Function Z#(x#, μ#, σ#)
            Return (x - μ) / σ
        End Function

        ''' <summary>
        ''' ## Standard score(z-score)
        ''' 
        ''' In statistics, the standard score is the signed number of standard deviations by which the value of 
        ''' an observation or data point is above the mean value of what is being observed or measured. Observed 
        ''' values above the mean have positive standard scores, while values below the mean have negative 
        ''' standard scores. The standard score is a dimensionless quantity obtained by subtracting the population 
        ''' mean from an individual raw score and then dividing the difference by the population standard deviation. 
        ''' This conversion process is called standardizing or normalizing (however, "normalizing" can refer to 
        ''' many types of ratios; see normalization for more).
        ''' 
        ''' > https://en.wikipedia.org/wiki/Standard_score
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' http://blog.163.com/huai_jing@126/blog/static/171861983201321074124426/
        ''' </remarks>
        <Extension>
        Public Function Z(x As Vector) As Vector
            Dim μ# = x.Average ' μ is the mean of the population.
            Dim σ# = x.SD   ' σ is the standard deviation of the population.
            Dim x1 As Vector = (x - μ) / σ
            Return x1
        End Function

        ''' <summary>
        ''' A logistic function or logistic curve is a common "S" shape (sigmoid curve)
        ''' > https://en.wikipedia.org/wiki/Logistic_function
        ''' </summary>
        ''' <param name="L#">the curve's maximum value</param>
        ''' <param name="x#">current x value</param>
        ''' <param name="x0#">the x-value of the sigmoid's midpoint,</param>
        ''' <param name="k#">the steepness of the curve.</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Logistic(L#, x#, x0#, k#) As Double
            Return L / (1 + stdNum.E ^ (-k * (x - x0)))
        End Function

        ''' <summary>
        ''' ###### 0-1标准化(0-1 normalization)
        ''' 也叫离差标准化，是对原始数据的线性变换，使结果落到[0,1]区间
        ''' 其中max为样本数据的最大值，min为样本数据的最小值。这种方法有一个缺陷就是当有新数据加入时，可能导致max和min的变化，需要重新定义。
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 数据的标准化（normalization）是将数据按比例缩放，使之落入一个小的特定区间。这样去除数据的单位限制，
        ''' 将其转化为无量纲的纯数值，便于不同单位或量级的指标能够进行比较和加权。
        ''' 其中最典型的就是0-1标准化和Z标准化
        ''' </remarks>
        <Extension> Public Function DeviationStandardization(x As Vector) As Vector
            Dim max# = x.Max
            Dim min# = x.Min
            Dim x1 As Vector = (x - min) / (max - min)
            Return x1
        End Function

        '' rcpp_trunc_ndist
        ''
        '' Truncated normal distribution (mean 1, respective upper and lower limits of
        '' 0 and 2).
        ''
        '' @param len Number of elements to be simulated
        '' @param sd Standard deviation
        ''
        '' @return A vector of truncated normally distributed values
        ''
        ' [[Rcpp::export]]
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="len"></param>
        ''' <param name="sd"></param>
        ''' <returns></returns>
        ''' <remarks>https://github.com/mpadge/tnorm</remarks>
        Public Function TruncNDist(len%, sd#) As Vector
            ' Set up truncated normal distribution
            Dim eps As Vector
            Dim z As New List(Of Double)()

            While z.Count < len
                eps = Normal.rnorm(len, 1.0, sd)
                For Each it As Double In eps
                    If it >= 0.0 AndAlso it <= 2.0 Then
                        z.Add(it)
                    End If
                    it += 1
                Next
            End While

            Return New Vector(z)
        End Function

        ''' <summary>
        ''' 标准正态分布, delta = 1, u = 0
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function StandardDistribution(x As Double) As Double
            Dim answer As Double = 1 / ((stdNum.Sqrt(2 * stdNum.PI)))
            Dim exp1 As Double = stdNum.Pow(x, 2) / 2
            Dim exp As Double = stdNum.Pow(stdNum.E, -(exp1))
            answer = answer * exp
            Return answer
        End Function

#Region "pnorm implementation"

        ''' <summary>
        ''' Density, distribution function, quantile function and random generation for the 
        ''' normal distribution with mean equal to mean and standard deviation equal to sd.
        ''' </summary>
        ''' <param name="q">vector of quantiles.</param>
        ''' <param name="mean">vector of means.</param>
        ''' <param name="sd">vector of standard deviations.</param>
        ''' <param name="lower_tail">logical; if TRUE (default), probabilities are ``P[X ≤ x]`` otherwise, ``P[X > x]``.</param>
        ''' <param name="logP">logical; if TRUE, probabilities p are given as log(p).</param>
        ''' <returns></returns>
        Public Function Eval(q#,
                             Optional mean# = 0,
                             Optional sd# = 1,
                             Optional lower_tail As Boolean = True,
                             Optional logP As Boolean = False,
                             Optional resolution% = 30000) As Double
            Dim p#

            If lower_tail Then
                p = pnorm.BelowStandardDistribution(q, resolution, mean, sd)
            Else
                p = pnorm.AboveStandardDistribution(q, resolution, mean, sd)
            End If

            If logP Then
                Return stdNum.Log10(p)
            Else
                Return p
            End If
        End Function

        ''' <summary>
        ''' Density, distribution function, quantile function and random generation for the 
        ''' normal distribution with mean equal to mean and standard deviation equal to sd.
        ''' </summary>
        ''' <param name="q">vector of quantiles.</param>
        ''' <param name="mean">vector of means.</param>
        ''' <param name="sd">vector of standard deviations.</param>
        ''' <param name="lower_tail">logical; if TRUE (default), probabilities are ``P[X ≤ x]`` otherwise, ``P[X > x]``.</param>
        ''' <param name="logP">logical; if TRUE, probabilities p are given as log(p).</param>
        ''' <returns></returns>
        Public Function Eval(q As Vector,
                             Optional mean# = 0,
                             Optional sd# = 1,
                             Optional lower_tail As Boolean = True,
                             Optional logP As Boolean = False,
                             Optional resolution% = 30000) As Vector
            Dim p As Vector
            Dim pdist As Func(Of Double, Double, Double, Double, Double)

            If lower_tail Then
                pdist = AddressOf pnorm.BelowStandardDistribution
            Else
                pdist = AddressOf pnorm.AboveStandardDistribution
            End If

            p = q.Select(Function(x)
                             Return pdist(x, resolution, mean, sd)
                         End Function) _
                 .AsVector

            If logP Then
                Return p.Log(base:=10)
            Else
                Return p
            End If
        End Function

        ''' <summary>
        ''' Normal Distribution.(正态分布)
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="m">Mean</param>
        ''' <param name="sd"></param>
        ''' <returns></returns>
        Public Function ProbabilityDensity(x#, m#, sd#) As Double
            Dim answer As Double = 1 / (sd * (stdNum.Sqrt(2 * stdNum.PI)))
            Dim exp As Double = (x - m) ^ 2
            Dim expP2 As Double = 2 * (sd ^ 2)
            Dim expP3 As Double = stdNum.E ^ (-(exp / expP2))
            answer = answer * expP3
            Return answer
        End Function

        <Extension>
        Public Function ProbabilityDensity(x As Vector, m#, sd#) As Vector
            Dim answer As Double = 1 / (sd * (stdNum.Sqrt(2 * stdNum.PI)))
            Dim exp = (x - m) ^ 2.0
            Dim expP2 As Double = 2 * stdNum.Pow(sd, 2.0)
            Dim expP3 = stdNum.E ^ -(exp / expP2)
            Dim y As Vector = answer * expP3

            Return y
        End Function

        Public Function AboveStandardDistribution(upperX As Double, resolution As Double, m As Double, sd As Double) As Double
            Dim lowerX As Double = m - 4.1 * sd
            Dim answer As Double = TrapezodialRule(lowerX, upperX, resolution, m, sd)

            Return 1 - answer
        End Function

        Public Function BelowStandardDistribution(upperX As Double, resolution As Double, m As Double, sd As Double) As Double
            Dim lowerX As Double = m + 4.1 * sd
            Dim answer As Double = TrapezodialRule(lowerX, upperX, resolution, m, sd)

            ' lol
            Return 1 + answer
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function BetweenStandardDistribution(lowerX As Double, upperX As Double, resolution As Double, m As Double, sd As Double) As Double
            Return TrapezodialRule(lowerX, upperX, resolution, m, sd)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function OutsideStandardDistribution(lowerX As Double, upperX As Double, resolution As Double, m As Double, sd As Double) As Double
            Return 1 - TrapezodialRule(lowerX, upperX, resolution, m, sd)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="a#"></param>
        ''' <param name="b#"></param>
        ''' <param name="resolution">计算的分辨率，越大越好</param>
        ''' <param name="m#"></param>
        ''' <param name="sd#"></param>
        ''' <returns></returns>
        Public Function TrapezodialRule(a#, b#, resolution%, m#, sd#) As Double
            Dim dx As Double = (b - a) / resolution
            Dim a1 As Double = ProbabilityDensity(a, m, sd)
            Dim b1 As Double = ProbabilityDensity(b, m, sd)
            Dim c As Double = 0.5 * (a1 + b1)

            For i As Integer = 1 To resolution - 1
                c = c + ProbabilityDensity((a + (i * dx)), m, sd)
            Next

            Return dx * c
        End Function
#End Region

    End Module
End Namespace
