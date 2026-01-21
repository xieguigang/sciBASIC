Imports std = System.Math

Public Module MetaboliteSignalClassifier

    ' 参数配置
    Private Const FLAT_THRESHOLD As Double = 0.001
    Private Const TREND_SIGNIFICANCE_RATIO As Double = 0.1
    Private Const MIN_ZERO_CROSSINGS As Integer = 4
    Private Const OSCILLATION_REGULARITY_THRESHOLD As Double = 0.35

    ''' <summary>
    ''' 主分类函数：返回枚举类型的分类结果
    ''' </summary>
    Public Function ClassifyMetaboliteSignal(data As Double()) As MetaboliteSignalType
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
        If isFlat Then Return MetaboliteSignalType.Isolated

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
            Return MetaboliteSignalType.Increasing
        End If

        ' --- 3. 递减信号检测 ---
        ' 斜率为负且预期变化显著，且上升点很少
        If slope < 0 AndAlso (std.Abs(expectedChange) > rangeVal * TREND_SIGNIFICANCE_RATIO) AndAlso (increaseCount < n * 0.3) Then
            Return MetaboliteSignalType.Decreasing
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
                        Return MetaboliteSignalType.Oscillating
                    End If
                End If
            End If
        End If

        ' --- 5. 随机信号 ---
        ' 排除以上所有情况
        Return MetaboliteSignalType.RandomSignal
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