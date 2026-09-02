#Region "Microsoft.VisualBasic::dff40165f54c463459fccbfaf105ba1e, Data_science\Mathematica\Math\Math\Distributions\Summary\SampleDistribution.vb"

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

    '   Total Lines: 260
    '    Code Lines: 160 (61.54%)
    ' Comment Lines: 56 (21.54%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 44 (16.92%)
    '     File Size: 9.78 KB


    '     Class SampleDistribution
    ' 
    '         Properties: average, CI95Range, CV, max, median
    '                     min, mode, outlierBoundary, quantile, range
    '                     size, stdErr, sum, variance
    ' 
    '         Constructor: (+5 Overloads) Sub New
    ' 
    '         Function: FromBlocks, GetPercentile, GetRange, ToString
    ' 
    '         Sub: Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Statistics.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports std = System.Math

Namespace Distributions.Summary

    ''' <summary>
    ''' The data sample model
    ''' </summary>
    ''' <remarks>
    ''' summary of the sample data vector
    ''' </remarks>
    Public Class SampleDistribution

        <XmlAttribute> Public Property min As Double
        <XmlAttribute> Public Property max As Double
        <XmlAttribute> Public Property average As Double
        <XmlAttribute> Public Property sum As Double
        <XmlAttribute> Public Property size As Long

        ''' <summary>
        ''' variance of the population
        ''' </summary>
        <XmlAttribute> Public Property variance As Double

        ''' <summary>
        ''' standard deviation of the population
        ''' </summary>
        <XmlAttribute> Public Property stdErr As Double

        ''' <summary>
        ''' coefficient of variation (stdErr / average)
        ''' </summary>
        <XmlAttribute> Public Property CV As Double

        ''' <summary>
        ''' range of the data (max - min)
        ''' </summary>
        <XmlAttribute> Public Property range As Double

        ''' <summary>
        ''' 分别为0%, 25%, 50%, 75%, 100%
        ''' </summary>
        <XmlAttribute> Public Property quantile As Double()

        ''' <summary>
        ''' 中位数 (50% quantile)
        ''' </summary>
        <XmlAttribute> Public Property median As Double

        <XmlAttribute> Public Property mode As Double

        Public ReadOnly Property CI95Range As Double()
            Get
                If size <= 1 Then
                    Return {average, average}
                Else
                    ' 均值的 95% 置信区间: average ± 1.96 * (SD / sqrt(n))
                    Dim se As Double = stdErr / std.Sqrt(size)

                    Return {
                        average - 1.96 * se,
                        average + 1.96 * se
                    }
                End If
            End Get
        End Property

        Public ReadOnly Property outlierBoundary As Double()
            Get
                If quantile Is Nothing OrElse quantile.Length < 4 Then
                    Return {Double.NaN, Double.NaN}
                Else
                    Dim Q1 = quantile(1)
                    Dim Q3 = quantile(3)
                    Dim IQR = Q3 - Q1

                    Return {
                        Q1 - 1.5 * IQR,
                        Q3 + 1.5 * IQR
                    }
                End If
            End Get
        End Property

        Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(data As IEnumerable(Of Double), Optional estimateQuantile As Boolean = True)
            Call Me.New(data.SafeQuery.ToArray, estimateQuantile)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(data As IEnumerable(Of Single), Optional estimateQuantile As Boolean = True)
            Call Me.New(data.SafeQuery.Select(Function(f) CDbl(f)).ToArray, estimateQuantile)
        End Sub

        ''' <summary>
        ''' Construct a feature data based on a specific dataframe column data
        ''' </summary>
        ''' <param name="v">the raw data matrix column data</param>
        ''' <param name="estimateQuantile"></param>
        Sub New(v As Double(), Optional estimateQuantile As Boolean = True)
            size = v.Length

            If size = 0 Then
                min = Double.NaN
                max = Double.NaN
                average = Double.NaN
                sum = 0
                stdErr = Double.NaN
                variance = Double.NaN
                CV = Double.NaN
                range = Double.NaN
                median = Double.NaN
            ElseIf size = 1 Then
                min = v(0) : max = v(0) : average = v(0) : sum = v(0)
                stdErr = 0 : variance = 0 : CV = 0 : range = 0 : median = v(0) : mode = v(0)

                If estimateQuantile Then
                    quantile = {v(0), v(0), v(0), v(0), v(0)}
                End If
            Else
                Call Evaluate(v, Me, estimateQuantile)
            End If
        End Sub

        ''' <summary>
        ''' Streaming construction from row blocks.
        ''' 面向数据总点数远超 Int32 数组上限的超大数据集:
        ''' 每一行 Double() 首尾相接构成完整样本, 全程内存占用恒定.
        ''' </summary>
        ''' <remarks>
        ''' 注意: quantile/median/mode 为近似值(精度由 histogramBins 控制),
        ''' size/sum/min/max/average/variance/stdErr 为精确值.
        ''' </remarks>
        Sub New(blocks As IEnumerable(Of Double()), Optional estimateQuantile As Boolean = True, Optional histogramBins As Integer = 65536)
            Dim stream As New StreamingSampleDistribution(histogramBins)

            For Each row As Double() In blocks
                If row IsNot Nothing Then
                    Call stream.AddRange(row)
                End If
            Next

            Call stream.WriteTo(Me, estimateQuantile)
        End Sub

        ''' <summary>
        ''' Streaming sample statistics for the huge row-wise dataset.
        ''' (数据总点数可以远超 Int32 数组上限, 内存占用恒定)
        ''' </summary>
        Public Shared Function FromBlocks(blocks As IEnumerable(Of Double()),
                                          Optional histogramBins As Integer = 65536,
                                          Optional estimateQuantile As Boolean = True) As SampleDistribution

            Return New SampleDistribution(blocks, estimateQuantile, histogramBins)
        End Function

        Private Shared Sub Evaluate(v As Double(), ByRef sample As SampleDistribution, estimateQuantile As Boolean)
            ' 1. 单次遍历计算 Sum, Min, Max, SumOfSquares (性能优化核心)
            Dim sumVal As Double = 0
            Dim sumSq As Double = 0
            Dim minVal As Double = v(0)
            Dim maxVal As Double = v(0)

            For i As Integer = 0 To sample.size - 1
                Dim val As Double = v(i)
                sumVal += val
                sumSq += val * val

                If val < minVal Then minVal = val
                If val > maxVal Then maxVal = val
            Next

            sample.sum = sumVal
            sample.min = minVal
            sample.max = maxVal
            sample.range = maxVal - minVal
            sample.average = sumVal / sample.size

            ' 总体方差: E(X^2) - (E(X))^2
            sample.variance = (sumSq / sample.size) - (sample.average * sample.average)

            ' 防止浮点数精度问题导致的微小负数
            If sample.variance < 0 Then
                sample.variance = 0
            End If

            sample.stdErr = std.Sqrt(sample.variance)

            If sample.average <> 0 Then
                sample.CV = sample.stdErr / sample.average
            Else
                sample.CV = Double.NaN
            End If

            ' 2. 一次性排序，复用于分位数和众数（避免多次排序和分配内存）
            Dim sortedArr As Double() = CType(v.Clone(), Double())

            Call Array.Sort(sortedArr)

            If estimateQuantile Then
                ' 精确分位数计算（基于线性插值法，与R/numpy默认类型一致）
                sample.quantile = {
                    sortedArr(0),
                    GetPercentile(sortedArr, 0.25),
                    GetPercentile(sortedArr, 0.5),
                    GetPercentile(sortedArr, 0.75),
                    sortedArr(sample.size - 1)
                }
                sample.median = sample.quantile(2)
            Else
                ' 即使不计算分位数，原逻辑也要求计算众数
                sample.median = GetPercentile(sortedArr, 0.5)
            End If

            ' 计算众数
            sample.mode = sortedArr.EvaluateMode
        End Sub

        ''' <summary>
        ''' 计算精确分位数 (Linear interpolation, similar to R type 7)
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function GetPercentile(sortedData As Double(), p As Double) As Double
            If sortedData.Length = 0 Then Return Double.NaN
            If sortedData.Length = 1 Then Return sortedData(0)

            Dim n As Integer = sortedData.Length
            Dim idx As Double = p * (n - 1)
            Dim lower As Integer = CInt(std.Floor(idx))
            Dim upper As Integer = CInt(std.Ceiling(idx))

            If lower = upper Then Return sortedData(lower)

            Dim frac As Double = idx - lower
            Return sortedData(lower) + (sortedData(upper) - sortedData(lower)) * frac
        End Function

        ''' <summary>
        ''' <see cref="DoubleRange"/> = ``[<see cref="min"/>, <see cref="max"/>]``
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetRange() As DoubleRange
            Return {min, max}
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return GetJson
        End Function
    End Class

End Namespace
