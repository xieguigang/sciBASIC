#Region "Microsoft.VisualBasic::ba4b3e584043f9fa69f5c65703fe47d4, Data_science\Mathematica\SignalProcessing\SignalProcessing\Cluster\NetworkSignalClassifier.vb"

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

    '   Total Lines: 109
    '    Code Lines: 74 (67.89%)
    ' Comment Lines: 18 (16.51%)
    '    - Xml Docs: 16.67%
    ' 
    '   Blank Lines: 17 (15.60%)
    '     File Size: 4.48 KB


    ' Module NetworkSignalClassifier
    ' 
    '     Function: CalculateSlope, CalculateStandardDeviation, ClassifyMetaboliteSignal
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Public Module NetworkSignalClassifier

    ' 参数配置
    Private Const FLAT_THRESHOLD As Double = 0.001
    Private Const TREND_SIGNIFICANCE_RATIO As Double = 0.1
    Private Const MIN_ZERO_CROSSINGS As Integer = 4
    Private Const OSCILLATION_REGULARITY_THRESHOLD As Double = 0.35

    ''' <summary>
    ''' 主分类函数：返回枚举类型的分类结果
    ''' </summary>
    Public Function ClassifyMetaboliteSignal(data As Double()) As NetworkSignalType
        Dim n As Integer = data.Length
        Dim minVal As Double = data.Min()
        Dim maxVal As Double = data.Max()
        Dim rangeVal As Double = maxVal - minVal
        Dim meanAbsVal As Double = data.Select(Function(x) std.Abs(x)).Average()
        If meanAbsVal < 1.0E-20 Then meanAbsVal = 1.0

        ' --- 1. 孤立节点检测 ---
        Dim isFlat As Boolean = False
        If rangeVal < 0.000000001 OrElse (rangeVal / meanAbsVal) < FLAT_THRESHOLD Then
            isFlat = True
        End If
        If isFlat Then Return NetworkSignalType.Isolated

        ' --- 趋势分析 ---
        Dim slope As Double = CalculateSlope(data)
        Dim expectedChange As Double = slope * n

        ' 检查单调性（忽略微小噪声）
        Dim decreaseCount As Integer = 0
        Dim increaseCount As Integer = 0
        Dim noiseThreshold = rangeVal * 0.01
        For i As Integer = 1 To n - 1
            If data(i) < data(i - 1) - noiseThreshold Then decreaseCount += 1
            If data(i) > data(i - 1) + noiseThreshold Then increaseCount += 1
        Next

        ' --- 2. 递增信号检测 ---
        ' 斜率为正且预期变化显著，且下降点很少
        If slope > 0 AndAlso (expectedChange > rangeVal * TREND_SIGNIFICANCE_RATIO) AndAlso (decreaseCount < n * 0.3) Then
            Return NetworkSignalType.Increasing
        End If

        ' --- 3. 递减信号检测 ---
        ' 斜率为负且预期变化显著，且上升点很少
        If slope < 0 AndAlso (std.Abs(expectedChange) > rangeVal * TREND_SIGNIFICANCE_RATIO) AndAlso (increaseCount < n * 0.3) Then
            Return NetworkSignalType.Decreasing
        End If

        ' --- 震荡 vs 随机 ---
        Dim meanVal As Double = data.Average()
        Dim zeroCrossingIndices As New List(Of Integer)()

        ' 计算过零点
        For i As Integer = 1 To n - 1
            Dim y1 = data(i - 1) - meanVal
            Dim y2 = data(i) - meanVal
            If (y1 > 0 AndAlso y2 <= 0) OrElse (y1 < 0 AndAlso y2 >= 0) Then
                zeroCrossingIndices.Add(i)
            End If
        Next

        ' --- 4. 周期性震荡检测 ---
        If zeroCrossingIndices.Count >= MIN_ZERO_CROSSINGS Then
            Dim intervals As New List(Of Double)()
            For i As Integer = 1 To zeroCrossingIndices.Count - 1
                intervals.Add(zeroCrossingIndices(i) - zeroCrossingIndices(i - 1))
            Next

            If intervals.Count > 0 Then
                Dim avgInterval = intervals.Average()
                Dim stdInterval = CalculateStandardDeviation(intervals)

                If avgInterval > 0 Then
                    Dim cv As Double = stdInterval / avgInterval
                    ' 变异系数小，间隔规律
                    If cv < OSCILLATION_REGULARITY_THRESHOLD AndAlso avgInterval > 2 Then
                        Return NetworkSignalType.Oscillating
                    End If
                End If
            End If
        End If

        ' --- 5. 随机信号 ---
        ' 排除以上所有情况
        Return NetworkSignalType.RandomSignal
    End Function

    ' --- 辅助计算函数 ---
    Private Function CalculateSlope(values As Double()) As Double
        Dim n As Integer = values.Length
        Dim sumX As Double = 0, sumY As Double = 0, sumXY As Double = 0, sumXX As Double = 0
        For i As Integer = 0 To n - 1
            sumX += i : sumY += values(i) : sumXY += i * values(i) : sumXX += i * i
        Next
        Return (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX)
    End Function

    Private Function CalculateStandardDeviation(values As List(Of Double)) As Double
        If values.Count < 2 Then Return 0.0
        Dim avg = values.Average()
        Return std.Sqrt(values.Sum(Function(v) std.Pow(v - avg, 2)) / (values.Count - 1))
    End Function

End Module
