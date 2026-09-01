#Region "Microsoft.VisualBasic::beca351d81f3261fd59b6263bdf29418, Data_science\Mathematica\Math\Math\Distributions\Sample.vb"

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

    '   Total Lines: 250
    '    Code Lines: 161 (64.40%)
    ' Comment Lines: 48 (19.20%)
    '    - Xml Docs: 72.92%
    ' 
    '   Blank Lines: 41 (16.40%)
    '     File Size: 9.06 KB


    '     Class SampleDistribution
    ' 
    '         Properties: average, CI95Range, CV, max, median
    '                     min, mode, outlierBoundary, quantile, range
    '                     size, stdErr, sum, variance
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Function: EvaluateMode, GetPercentile, GetRange, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports std = System.Math

Namespace Distributions

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
        <XmlAttribute> Public Property size As Integer

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
                If size <= 1 Then Return {average, average}
                ' 均值的 95% 置信区间: average ± 1.96 * (SD / sqrt(n))
                Dim se As Double = stdErr / std.Sqrt(size)
                Return {
                    average - 1.96 * se,
                    average + 1.96 * se
                }
            End Get
        End Property

        Public ReadOnly Property outlierBoundary As Double()
            Get
                If quantile Is Nothing OrElse quantile.Length < 4 Then Return {Double.NaN, Double.NaN}
                Dim Q1 = quantile(1)
                Dim Q3 = quantile(3)
                Dim IQR = Q3 - Q1
                Return {
                    Q1 - 1.5 * IQR,
                    Q3 + 1.5 * IQR
                }
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
                Return
            End If

            If size = 1 Then
                min = v(0) : max = v(0) : average = v(0) : sum = v(0)
                stdErr = 0 : variance = 0 : CV = 0 : range = 0 : median = v(0) : mode = v(0)
                If estimateQuantile Then
                    quantile = {v(0), v(0), v(0), v(0), v(0)}
                End If
                Return
            End If

            ' 1. 单次遍历计算 Sum, Min, Max, SumOfSquares (性能优化核心)
            Dim sumVal As Double = 0
            Dim sumSq As Double = 0
            Dim minVal As Double = v(0)
            Dim maxVal As Double = v(0)

            For i As Integer = 0 To size - 1
                Dim val As Double = v(i)
                sumVal += val
                sumSq += val * val
                If val < minVal Then minVal = val
                If val > maxVal Then maxVal = val
            Next

            sum = sumVal
            min = minVal
            max = maxVal
            range = maxVal - minVal
            average = sumVal / size

            ' 总体方差: E(X^2) - (E(X))^2
            variance = (sumSq / size) - (average * average)

            ' 防止浮点数精度问题导致的微小负数
            If variance < 0 Then variance = 0
            stdErr = std.Sqrt(variance)

            If average <> 0 Then
                CV = stdErr / average
            Else
                CV = Double.NaN
            End If

            ' 2. 一次性排序，复用于分位数和众数（避免多次排序和分配内存）
            Dim sortedArr As Double() = CType(v.Clone(), Double())
            Array.Sort(sortedArr)

            If estimateQuantile Then
                ' 精确分位数计算（基于线性插值法，与R/numpy默认类型一致）
                quantile = {
                    sortedArr(0),
                    GetPercentile(sortedArr, 0.25),
                    GetPercentile(sortedArr, 0.5),
                    GetPercentile(sortedArr, 0.75),
                    sortedArr(size - 1)
                }
                median = quantile(2)
            Else
                ' 即使不计算分位数，原逻辑也要求计算众数
                median = GetPercentile(sortedArr, 0.5)
            End If

            ' 计算众数
            mode = EvaluateMode(sortedArr)
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

        Public Shared Function EvaluateMode(data As Double()) As Double
            If data Is Nothing OrElse data.Length = 0 Then Return Double.NaN
            If data.Length = 1 Then Return data(0)

            ' data 必须为已排序数组！
            Dim modeValue As Double = data(0)
            Dim modeCount As Integer = 1
            Dim currValue As Double = data(0)
            Dim currCount As Integer = 1

            ' Count the amount of repeat And update mode variables
            For i As Integer = 1 To data.Length - 1
                If data(i) = currValue Then
                    currCount += 1
                Else
                    ' 修正：使用 > 而不是 >=，确保在多个值频次相同时保留最先出现的那个值
                    If currCount > modeCount Then
                        modeCount = currCount
                        modeValue = currValue
                    End If

                    currValue = data(i)
                    currCount = 1
                End If
            Next

            ' Check the last count
            If currCount > modeCount Then
                modeValue = currValue
            End If

            ' 如果所有值都只出现一次(没有重复)，众数概念上无意义，这里返回第一个元素
            If modeCount = 1 Then Return data(0)

            Return modeValue
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
