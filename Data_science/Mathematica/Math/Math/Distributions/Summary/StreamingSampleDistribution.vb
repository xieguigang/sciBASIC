Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Distributions.Summary

    ''' <summary>
    ''' Streaming builder of the <see cref="SampleDistribution"/> model.
    ''' </summary>
    ''' <remarks>
    ''' 面向数据总点数远超 .NET 数组元素上限(约 21.4 亿)的大型数据集:
    ''' 以行块 Double() 为单位流式喂数据, 全程内存占用恒定,
    ''' 只与 histogramBins / modeDistinctCap 相关, 与数据总量无关.
    '''
    ''' 统计量精度:
    ''' 1. size, sum, min, max, average, variance, stdErr: 精确值
    '''    (方差采用 Welford 在线算法, 大数据下数值稳定性优于 sum-of-squares 方法)
    ''' 2. quantile, median: 近似值. 基于自适应等宽直方图(数据范围扩张时自动倍增桶宽压缩),
    '''    误差约为一个桶宽(~= range / histogramBins); 0% 与 100% 分位数恒为精确的 min/max.
    ''' 3. mode: 频率表精确计数; 唯一值数量超过 modeDistinctCap 时冻结为当时的众数(近似值).
    '''
    ''' 注意: NaN 会被自动跳过(可通过 SkippedNaN 查看跳过数量); 本类型非线程安全.
    ''' </remarks>
    Public Class StreamingSampleDistribution

        ''' <summary>直方图最大桶数(每桶约占 24 字节内存)</summary>
        ReadOnly maxBins As Integer
        ''' <summary>众数频率表允许的最大唯一值数量</summary>
        ReadOnly modeCap As Integer

        ' ##### 精确统计量
        Dim count As Long
        Dim sumVal As Double
        Dim minVal As Double, maxVal As Double

        ' Welford online algorithm: wMean = E(X), wM2 = sum((x - mean)^2)
        Dim wMean As Double
        Dim wM2 As Double

        ' ##### 自适应直方图(用于流式分位数估计)
        ''' <summary>histogram bin: 计数 + 桶内精确的 [min, max]</summary>
        Public Structure Bucket
            Dim n As Long
            Dim lo As Double
            Dim hi As Double
        End Structure

        Dim histWidth As Double
        Dim histInited As Boolean
        Dim histBins As Dictionary(Of Long, Bucket)
        Dim histTotal As Long

        ' 桶宽初始化之前, 先缓冲一小段样本用于估计初始桶宽
        Const initCap As Integer = 8192
        Dim initBuffer As List(Of Double)

        ' ##### 众数频率表
        Dim freq As Dictionary(Of Double, Long)
        Dim bestValue As Double
        Dim bestCount As Long
        ''' <summary>唯一值过多, 众数已冻结(近似模式)</summary>
        Dim modeFrozen As Boolean

        Dim nanSkipped As Long

        Private Const MaxKey As Double = 4.6116860184273879E+18R  ' 2^62

        ''' <summary>已跳过的 NaN 数据点数量</summary>
        Public ReadOnly Property SkippedNaN As Long
            Get
                Return nanSkipped
            End Get
        End Property

        ''' <summary>已流入的数据点数量(64 位计数, 无 Int32 溢出问题)</summary>
        Public ReadOnly Property TotalCount As Long
            Get
                Return count
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="histogramBins">
        ''' 分位数估计的直方图桶数上限, 越大越精确, 默认 65536(几 MB 内存),
        ''' 分位数误差约为 range / 65536
        ''' </param>
        ''' <param name="modeDistinctCap">
        ''' 众数频率表的唯一值上限, 默认 2^20(约 50MB 内存), 超过后众数冻结为近似值
        ''' </param>
        Sub New(Optional histogramBins As Integer = 65536,
            Optional modeDistinctCap As Integer = 1 << 20)

            maxBins = std.Max(64, histogramBins)
            modeCap = std.Max(1024, modeDistinctCap)
            initBuffer = New List(Of Double)(initCap)
            histBins = New Dictionary(Of Long, Bucket)
            freq = New Dictionary(Of Double, Long)
        End Sub

        ''' <summary>
        ''' add one single data value
        ''' </summary>
        Public Sub Add(x As Double)
            Call AddCore(x)
        End Sub

        ''' <summary>
        ''' add one row of the dataset (每一行的数据首尾相接进入统计)
        ''' </summary>
        Public Sub AddRange(values As Double())
            If values Is Nothing Then Return

            For i As Integer = 0 To values.Length - 1
                Call AddCore(values(i))
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub AddCore(x As Double)
            If Double.IsNaN(x) Then
                nanSkipped += 1L
                Return
            End If

            ' 1. Welford 在线均值/方差
            count += 1L
            Dim delta As Double = x - wMean
            wMean += delta / count
            wM2 += delta * (x - wMean)

            sumVal += x

            If count = 1 Then
                minVal = x : maxVal = x
            Else
                If x < minVal Then minVal = x
                If x > maxVal Then maxVal = x
            End If

            ' 2. 众数频率表
            Call AddMode(x)

            ' 3. 直方图
            If histInited Then
                Call AddHistogram(x)
            Else
                ' 桶宽初始化之前先缓冲一小段样本
                initBuffer.Add(x)

                If initBuffer.Count >= initCap Then
                    Call InitHistogram()
                End If
            End If
        End Sub

        Private Sub AddMode(x As Double)
            If modeFrozen Then Return

            Dim c As Long

            If Not freq.TryGetValue(x, c) Then
                c = 0
            End If

            c += 1L
            freq(x) = c

            If c > bestCount Then
                bestCount = c
                bestValue = x
            End If

            If freq.Count > modeCap Then
                ' 唯一值过多: 冻结众数, 释放频率表内存
                modeFrozen = True
                freq = Nothing
            End If
        End Sub

        ''' <summary>
        ''' 根据缓冲样本估计初始桶宽, 然后将缓冲数据灌入直方图
        ''' </summary>
        Private Sub InitHistogram()
            Dim range As Double = maxVal - minVal

            If range > 0 AndAlso Not Double.IsInfinity(range) Then
                histWidth = range / maxBins
            Else
                ' 数据全为同一个值
                histWidth = 1.0
            End If

            histInited = True

            For Each x As Double In initBuffer
                Call AddHistogram(x)
            Next

            initBuffer = Nothing
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub AddHistogram(x As Double)
            Dim k As Long = KeyOf(x)
            Dim b As Bucket

            If histBins.TryGetValue(k, b) Then
                b.n += 1L
                If x < b.lo Then b.lo = x
                If x > b.hi Then b.hi = x
                histBins(k) = b
            Else
                b.n = 1L : b.lo = x : b.hi = x
                histBins.Add(k, b)

                If histBins.Count > maxBins Then
                    ' 桶数超限: 桶宽翻倍, 相邻两桶合并(内存封顶的关键)
                    Call Compress()
                End If
            End If

            histTotal += 1L
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function KeyOf(x As Double) As Long
            Dim k As Double = std.Floor(x / histWidth)

            ' 防止极端数量级导致 Long 溢出(clamp 保序, 不破坏直方图结构)
            If k > MaxKey Then Return CLng(MaxKey)
            If k < -MaxKey Then Return -CLng(MaxKey)

            Return CLng(k)
        End Function

        ''' <summary>
        ''' 桶宽翻倍, 相邻两桶合并为一个, 保证内存占用始终有界
        ''' </summary>
        Private Sub Compress()
            Dim merged As New Dictionary(Of Long, Bucket)(histBins.Count \ 2 + 1)

            For Each kv As KeyValuePair(Of Long, Bucket) In histBins
                Dim k2 As Long = kv.Key >> 1
                Dim b As Bucket = kv.Value

                If merged.ContainsKey(k2) Then
                    Dim m As Bucket = merged(k2)
                    m.n += b.n
                    If b.lo < m.lo Then m.lo = b.lo
                    If b.hi > m.hi Then m.hi = b.hi
                    merged(k2) = m
                Else
                    merged.Add(k2, b)
                End If
            Next

            histBins = merged
            histWidth *= 2.0
        End Sub

        ''' <summary>
        ''' 估计 p 分位数(与 <see cref="SampleDistribution"/> 相同的 type-7 线性插值语义)
        ''' </summary>
        Private Function ComputeQuantile(p As Double) As Double
            If histTotal = 0 Then Return Double.NaN

            Dim idx As Double = p * (histTotal - 1)

            If idx <= 0 Then Return minVal
            If idx >= histTotal - 1 Then Return maxVal

            Dim lo As Long = CLng(std.Floor(idx))
            Dim frac As Double = idx - lo
            Dim vLo As Double = ValueAtRank(lo)

            If frac = 0 Then Return vLo

            Dim vHi As Double = ValueAtRank(lo + 1)

            Return vLo + (vHi - vLo) * frac
        End Function

        ''' <summary>
        ''' 取排序后第 r 个(0-based)数据点的近似值
        ''' (桶内均匀分布假设, 且由桶内精确的 min/max 界定误差范围)
        ''' </summary>
        Private Function ValueAtRank(r As Long) As Double
            Dim keys As Long() = histBins.Keys.ToArray
            Array.Sort(keys)

            Dim cum As Long = 0

            For i As Integer = 0 To keys.Length - 1
                Dim b As Bucket = histBins(keys(i))

                If r < cum + b.n Then
                    Dim within As Long = r - cum

                    If b.n = 1 OrElse b.lo = b.hi Then
                        Return b.lo
                    Else
                        Return b.lo + within * (b.hi - b.lo) / (b.n - 1)
                    End If
                End If

                cum += b.n
            Next

            Return maxVal
        End Function

        Private Shared Function ExactPercentile(sorted As Double(), p As Double) As Double
            ' 与 SampleDistribution.GetPercentile 一致的 type-7 线性插值
            If sorted.Length = 0 Then Return Double.NaN
            If sorted.Length = 1 Then Return sorted(0)

            Dim idx As Double = p * (sorted.Length - 1)
            Dim lower As Integer = CInt(std.Floor(idx))
            Dim upper As Integer = CInt(std.Ceiling(idx))

            If lower = upper Then Return sorted(lower)

            Return sorted(lower) + (sorted(upper) - sorted(lower)) * (idx - lower)
        End Function

        ''' <summary>
        ''' 生成最终统计结果模型
        ''' </summary>
        ''' <param name="estimateQuantile">是否输出 quantile 数组(median 总是会计算)</param>
        Public Function ToSampleDistribution(Optional estimateQuantile As Boolean = True) As SampleDistribution
            Dim dist As New SampleDistribution
            Call WriteTo(dist, estimateQuantile)
            Return dist
        End Function

        Friend Sub WriteTo(target As SampleDistribution, estimateQuantile As Boolean)
            target.size = count

            If count = 0 Then
                target.min = Double.NaN
                target.max = Double.NaN
                target.average = Double.NaN
                target.sum = 0
                target.stdErr = Double.NaN
                target.variance = Double.NaN
                target.CV = Double.NaN
                target.range = Double.NaN
                target.median = Double.NaN
                Return
            End If

            target.sum = sumVal
            target.min = minVal
            target.max = maxVal
            target.range = maxVal - minVal
            target.average = wMean

            Dim variance As Double = wM2 / count

            If variance < 0 Then variance = 0
            target.variance = variance
            target.stdErr = std.Sqrt(variance)
            target.CV = If(wMean <> 0, target.stdErr / wMean, Double.NaN)

            ' ---- quantile / median
            Dim q1 As Double, q2 As Double, q3 As Double

            If histInited Then
                q1 = ComputeQuantile(0.25)
                q2 = ComputeQuantile(0.5)
                q3 = ComputeQuantile(0.75)
            Else
                ' 数据总量比初始化缓冲区还小, 直接精确计算
                Dim sorted As Double() = initBuffer.ToArray
                Array.Sort(sorted)

                q1 = ExactPercentile(sorted, 0.25)
                q2 = ExactPercentile(sorted, 0.5)
                q3 = ExactPercentile(sorted, 0.75)
            End If

            If estimateQuantile Then
                target.quantile = {minVal, q1, q2, q3, maxVal}
            End If

            target.median = q2

            ' ---- mode
            If bestCount > 1 Then
                target.mode = bestValue
            Else
                ' 所有值只出现一次, 与原 EvaluateMode 的行为保持一致
                target.mode = minVal
            End If
        End Sub
    End Class

End Namespace