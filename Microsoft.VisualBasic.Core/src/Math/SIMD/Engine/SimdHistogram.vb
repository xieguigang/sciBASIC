Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Math.SIMD

    ''' <summary>
    ''' 直方图内核：分箱(bin)、累加、归一化与极值提取。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 点云特征描述子（PFH/FPFH 的 33 维、SHOT 的 352 维、ESF 的 640 维、VFH 的 768 维）
    ''' 本质上都是“把大量角度/距离观测投票进固定数量的桶”。这类运算有两个特点：
    ''' 写入位置由数据决定（scatter），因此<b>无法整块向量化</b>；
    ''' 但每次投票的工作量极小（一个除法 + 一次加法），因此瓶颈在分支与边界判断上。
    ''' </para>
    ''' <para>
    ''' 这个类提供的是：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>无分支的分箱计算 <see cref="BinIndex(Double, Double, Double, Integer)"/>（越界/NaN 统一返回 -1，调用方只需判断一次）。</description></item>
    ''' <item><description>把“逐观测累加”与“归一化/极值”分离，后者可以向量化（<see cref="Normalize(Double())"/>、<see cref="Extrema"/>）。</description></item>
    ''' <item><description>加权累加重载，供距离/法向夹角加权投票（SHOT、FPFH 邻域加权）复用。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>确定性</b>：所有累加都按观测顺序串行执行，因此同一输入永远得到逐位相同的结果
    ''' （这也是描述子可以跨平台对拍的前提）。并行场景请让每个线程持有自己的直方图，
    ''' 最后用 <see cref="Accumulate(Double(), Double())"/> 合并。
    ''' </para>
    ''' <para>
    ''' <b>线程安全</b>：所有函数都是纯静态的、只操作调用方传入的数组，因此天然线程安全，
    ''' 前提是不同线程不写同一块直方图内存。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class SimdHistogram

        Private Sub New()
        End Sub

#Region "helpers"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Sub CheckNull(v As Array, name As String)
            If v Is Nothing Then
                Throw New ArgumentNullException(name, "the input array can not be NULL!")
            End If
        End Sub

        Private Shared Sub CheckSizes(len As Integer, hist As Array, name As String)
            Call CheckNull(hist, name)

            If hist.Length < len Then
                Throw New ArgumentException($"the histogram array is too small: {hist.Length} < {len}!", name)
            End If
        End Sub

#End Region

#Region "binning"

        ''' <summary>
        ''' 把连续值映射到 <c>[0, nBins)</c> 的桶下标：
        ''' <c>bin = FLOOR((value − min) · nBins / (max − min))</c>。
        ''' </summary>
        ''' <returns>
        ''' 桶下标；当 <paramref name="value"/> 为 NaN、区间退化（<c>max &lt;= min</c>）
        ''' 或取值落在 <c>[min, max]</c> 之外时返回 <c>-1</c>。
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' <paramref name="max"/> 处的样本被并入最后一个桶（闭区间右端），
        ''' 这是直方图统计的通行约定，可以避免“最大值恰好落在边界”时凭空多出一个空桶。
        ''' </para>
        ''' <para>
        ''' 返回 -1 而不是抛异常是刻意的：描述子计算里越界值是<b>常态</b>
        ''' （噪声导致的微小角度越界），每个观测都抛一次异常会把性能彻底毁掉；
        ''' 调用方用 <c>If bin &gt;= 0 Then</c> 一次判断即可跳过。
        ''' </para>
        ''' </remarks>
        Public Shared Function BinIndex(value As Double, min As Double, max As Double, nBins As Integer) As Integer
            If nBins <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(nBins), "the bin count should be positive!")

            If Double.IsNaN(value) Then Return -1

            Dim range As Double = max - min

            If range <= 0.0 Then Return -1
            If value < min OrElse value > max Then Return -1

            Dim bin As Integer = CInt(std.Floor((value - min) * nBins / range))

            ' 浮点误差可能让 value = max 得到 nBins，夹到最后一个桶
            If bin >= nBins Then bin = nBins - 1
            If bin < 0 Then bin = 0

            Return bin
        End Function

        ''' <summary>
        ''' 预计算版分箱：把区间宽度换算成乘法，供逐个观测的热循环使用。
        ''' </summary>
        ''' <param name="scale">预先算好的 <c>nBins / (max − min)</c></param>
        Public Shared Function BinIndexFast(value As Double, min As Double, scale As Double, nBins As Integer) As Integer
            Dim bin As Integer = CInt(std.Floor((value - min) * scale))

            If bin >= nBins Then Return nBins - 1
            If bin < 0 Then Return 0

            Return bin
        End Function

        ''' <summary>
        ''' 计算分箱所需的缩放因子：<c>nBins / (max − min)</c>。
        ''' </summary>
        ''' <remarks>
        ''' 描述子的每个观测都要做一次除法；把除法提到循环外（用一次乘法替代）
        ''' 在 640 桶、数万次投票的 ESF 上可以省掉数万次 <c>FDIV</c>（约 10~20 周期/次）。
        ''' </remarks>
        Public Shared Function BinScale(min As Double, max As Double, nBins As Integer) As Double
            If nBins <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(nBins), "the bin count should be positive!")

            Dim range As Double = max - min

            If range <= 0.0 Then Return 0.0

            Return nBins / range
        End Function

#End Region

#Region "accumulate"

        ''' <summary>
        ''' 整数桶计数累加：<c>hist(bins(i)) += 1</c>（忽略 <c>bins(i) &lt; 0</c> 的观测）。
        ''' </summary>
        Public Shared Sub AddBins(bins As Integer(), hist As Integer())
            Call CheckNull(bins, NameOf(bins))
            Call CheckSizes(bins.Length, hist, NameOf(hist))

            Call AddBins(bins, hist, 0, bins.Length)
        End Sub

        ''' <summary>
        ''' 整数桶计数累加（区间版）。
        ''' </summary>
        Public Shared Sub AddBins(bins As Integer(), hist As Integer(), start As Integer, ends As Integer)
            Call CheckNull(bins, NameOf(bins))
            Call CheckSizes(bins.Length, hist, NameOf(hist))

            If start < 0 OrElse ends > bins.Length OrElse start > ends Then
                Throw New ArgumentOutOfRangeException($"invalid range [{start}, {ends}) for an array of length {bins.Length}!")
            End If

            For i As Integer = start To ends - 1
                Dim bin As Integer = bins(i)

                If bin >= 0 Then hist(bin) += 1
            Next
        End Sub

        ''' <summary>
        ''' 加权桶累加：<c>hist(bins(i)) += weights(i)</c>（忽略 <c>bins(i) &lt; 0</c> 的观测）。
        ''' </summary>
        Public Shared Sub AddBinsWeighted(bins As Integer(), weights As Double(), hist As Double())
            Call CheckNull(bins, NameOf(bins))
            Call CheckNull(weights, NameOf(weights))
            Call CheckSizes(bins.Length, hist, NameOf(hist))

            If weights.Length < bins.Length Then
                Throw New ArgumentException($"the weight array is too small: {weights.Length} < {bins.Length}!", NameOf(weights))
            End If

            Call AddBinsWeighted(bins, weights, hist, 0, bins.Length)
        End Sub

        ''' <summary>
        ''' 加权桶累加（区间版）。
        ''' </summary>
        Public Shared Sub AddBinsWeighted(bins As Integer(), weights As Double(), hist As Double(),
                                          start As Integer, ends As Integer)
            Call CheckNull(bins, NameOf(bins))
            Call CheckNull(weights, NameOf(weights))
            Call CheckSizes(bins.Length, hist, NameOf(hist))

            If start < 0 OrElse ends > bins.Length OrElse start > ends Then
                Throw New ArgumentOutOfRangeException($"invalid range [{start}, {ends}) for an array of length {bins.Length}!")
            End If
            If weights.Length < ends Then
                Throw New ArgumentException($"the weight array is too small: {weights.Length} < {ends}!", NameOf(weights))
            End If

            For i As Integer = start To ends - 1
                Dim bin As Integer = bins(i)

                If bin >= 0 Then hist(bin) += weights(i)
            Next
        End Sub

        ''' <summary>
        ''' 把一份直方图合并到另一份：<c>dst(i) += src(i)</c>（并行分块的返回阶段使用）。
        ''' </summary>
        Public Shared Sub Accumulate(src As Double(), dst As Double())
            Call CheckNull(src, NameOf(src))
            Call CheckNull(dst, NameOf(dst))
            Call CheckSizes(src.Length, dst, NameOf(dst))

            Call SimdEngine.AddInPlace(Of Double)(dst, src)
        End Sub

#End Region

#Region "normalize"

        ''' <summary>
        ''' L1 归一化（就地）：<c>hist(i) /= Σ|hist(j)|</c>。
        ''' </summary>
        ''' <remarks>
        ''' 邻域点数不同的描述子必须归一到“概率分布”才能互相比较；
        ''' 总和为零（空直方图）时保持全零，不做除零。
        ''' </remarks>
        Public Shared Sub Normalize(hist As Double())
            Call CheckNull(hist, NameOf(hist))

            Dim sum As Double = SimdReduce.L1Norm(hist)

            If sum <= 0.0 Then Return

            Call DivideInPlace(hist, sum)
        End Sub

        ''' <summary>
        ''' L2 归一化（就地）：<c>hist /= ‖hist‖</c>。SHOT 等“按欧氏范数归一”的描述子使用。
        ''' </summary>
        Public Shared Sub NormalizeL2(hist As Double())
            Call CheckNull(hist, NameOf(hist))

            Dim sum As Double = std.Sqrt(SimdReduce.SumSquares(hist))

            If sum <= 0.0 Then Return

            Call DivideInPlace(hist, sum)
        End Sub

        ''' <summary>
        ''' 就地除以标量（零分配的向量化实现）。
        ''' </summary>
        ''' <remarks>
        ''' 这里没有直接调用 <see cref="SimdEngine.DivideScalar(Double(), Double)"/>
        ''' 是因为它会为结果分配一整个新数组；描述子归一化的典型长度只有 33~768，
        ''' 但调用次数极多（每点一次），分配开销会盖过算术本身。
        ''' </remarks>
        Private Shared Sub DivideInPlace(hist As Double(), scalar As Double)
            Dim len As Integer = hist.Length
            Dim count As Integer = Vector(Of Double).Count
            Dim splat As New Vector(Of Double)(scalar)
            Dim i As Integer = 0

            If SIMDEnvironment.IsEnabled AndAlso len >= count Then
                Do While i <= len - count
                    Vector.Divide(Of Double)(New Vector(Of Double)(hist, i), splat).CopyTo(hist, i)
                    i += count
                Loop
            End If

            Do While i < len
                hist(i) /= scalar
                i += 1
            Loop
        End Sub

#End Region

#Region "statistics"

        ''' <summary>
        ''' 直方图极值：同时给出最小值/最大值及其所在桶下标（相等时取较小下标）。
        ''' </summary>
        Public Shared Sub Extrema(hist As Double(), ByRef minValue As Double, ByRef maxValue As Double,
                                  ByRef minBin As Integer, ByRef maxBin As Integer)
            Call CheckNull(hist, NameOf(hist))

            If hist.Length = 0 Then
                minValue = 0.0
                maxValue = 0.0
                minBin = -1
                maxBin = -1

                Return
            End If

            minValue = hist(0)
            maxValue = hist(0)
            minBin = 0
            maxBin = 0

            For i As Integer = 1 To hist.Length - 1
                Dim value As Double = hist(i)

                If value < minValue Then
                    minValue = value
                    minBin = i
                End If
                If value > maxValue Then
                    maxValue = value
                    maxBin = i
                End If
            Next
        End Sub

        ''' <summary>
        ''' 直方图的最大值所在桶（等价于 <see cref="Extrema"/> 的第二个输出）。
        ''' </summary>
        Public Shared Function MaxBin(hist As Double()) As Integer
            Dim minValue As Double, maxValue As Double
            Dim minIndex As Integer, maxIndex As Integer

            Call Extrema(hist, minValue, maxValue, minIndex, maxIndex)

            Return maxIndex
        End Function

        ''' <summary>
        ''' 直方图的信息熵：<c>−Σ p(i)·log₂ p(i)</c>，其中 <c>p</c> 为 L1 归一化后的分布。
        ''' </summary>
        ''' <remarks>
        ''' 用于快速判断描述子的“信息丰富度”：常量直方图熵为 0，
        ''' 全均匀分布熵达到上限 <c>log₂(nBins)</c>。
        ''' 描述子检索里可以用它剔除平面/球面等退化区域的低信息量特征点。
        ''' </remarks>
        Public Shared Function Entropy(hist As Double()) As Double
            Call CheckNull(hist, NameOf(hist))

            Dim sum As Double = SimdReduce.L1Norm(hist)

            If sum <= 0.0 Then Return 0.0

            Dim inv As Double = 1.0 / sum
            Dim h As Double = 0

            For i As Integer = 0 To hist.Length - 1
                Dim p As Double = hist(i) * inv

                If p > 0.0 Then
                    h -= p * std.Log2(p)
                End If
            Next

            Return h
        End Function

        ''' <summary>
        ''' 直方图的卡方距离（描述子匹配常用）：<c>0.5·Σ (a−b)² / (a+b)</c>。
        ''' </summary>
        ''' <remarks>
        ''' 两个桶都为零时该项为 0（跳过），避免 <c>0/0</c> 产生 NaN。
        ''' 相比欧氏距离，卡方距离对“少数大峰值”更敏感，
        ''' 在 FPFH/SHOT 的特征匹配中通常能给出更清晰的比值检验分布。
        ''' </remarks>
        Public Shared Function ChiSquare(a As Double(), b As Double()) As Double
            Call CheckNull(a, NameOf(a))
            Call CheckNull(b, NameOf(b))

            If a.Length <> b.Length Then
                Throw New ArgumentException($"the length of the two histograms not agree: {a.Length} vs {b.Length}!")
            End If

            Dim total As Double = 0

            For i As Integer = 0 To a.Length - 1
                Dim d As Double = a(i) - b(i)
                Dim s As Double = a(i) + b(i)

                If s > 0.0 Then
                    total += d * d / s
                End If
            Next

            Return 0.5 * total
        End Function

#End Region

    End Class
End Namespace
