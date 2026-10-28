Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Math.SIMD

    ''' <summary>
    ''' 聚类算法专用的成对度量内核：全部为单趟、零堆分配，
    ''' 并在数据规模足够大时自动走 <see cref="SimdParallel"/> 的分块并行路径。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 与 <see cref="SimdReduce"/> 的分工：<see cref="SimdReduce"/> 提供通用的单对归约，
    ''' 这里提供聚类迭代里反复出现的"一条样本对 k 个中心"的批量度量
    ''' （<c>BatchDistanceSquared</c>），
    ''' 以及欧氏/曼哈顿/切比雪夫/余弦/皮尔逊五种成对度量的高性能实现，
    ''' 供 <c>HDBSCAN.Distance</c> 等上层度量类直接复用。
    ''' </para>
    ''' <para>
    ''' <b>范数展开距离</b>：<c>d² = ‖x‖² + ‖c‖² − 2·x·c</c>。
    ''' 在 k-means 场景里 <c>‖x‖²</c> 与 <c>‖c‖²</c> 每轮只需算一次，
    ''' 于是每次比较只剩一次点积（乘法减半），代价是存在抵消误差：
    ''' 当两点几乎重合时 <c>‖x‖²+‖c‖²</c> 与 <c>2x·c</c> 会相互抵消。
    ''' 因此这里在 <c>d²</c> 为负或远小于两侧范数之和时自动回退到
    ''' <see cref="SimdReduce.DistanceSquared(Double(), Double())"/> 精确计算。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class SimdDistance

        Private Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Sub CheckPair(a As Array, b As Array, nameA As String, nameB As String)
            If a Is Nothing Then Throw New ArgumentNullException(nameA)
            If b Is Nothing Then Throw New ArgumentNullException(nameB)
            If a.Length <> b.Length Then
                Throw New ArgumentException($"the length of the two vectors not agree: {a.Length} vs {b.Length}!")
            End If
        End Sub

#Region "norms"

        ''' <summary>
        ''' 平方范数：<c>‖v‖²</c>（= <c>SUM(v(i)^2)</c>）。
        ''' </summary>
        Public Shared Function SquaredNorm(v As Double()) As Double
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))

            If v.Length = 0 Then Return 0.0

            Return SimdReduce.SumSquares(v)
        End Function

        ''' <summary>
        ''' 欧几里得范数：<c>‖v‖</c>。
        ''' </summary>
        Public Shared Function Norm(v As Double()) As Double
            Return std.Sqrt(SquaredNorm(v))
        End Function

        ''' <summary>
        ''' 逐行计算平方范数，结果写入 <paramref name="dst"/>（长度不足会抛出异常）。
        ''' </summary>
        Public Shared Sub SquaredNorms(rows As Double()(), dst As Double())
            If rows Is Nothing Then Throw New ArgumentNullException(NameOf(rows))
            If dst Is Nothing Then Throw New ArgumentNullException(NameOf(dst))
            If dst.Length < rows.Length Then
                Throw New ArgumentException($"the destination array is too small: {dst.Length} < {rows.Length}!")
            End If

            For i As Integer = 0 To rows.Length - 1
                dst(i) = SimdReduce.SumSquares(rows(i))
            Next
        End Sub

        ''' <summary>
        ''' 逐行计算平方范数。
        ''' </summary>
        Public Shared Function SquaredNorms(rows As Double()()) As Double()
            If rows Is Nothing Then Throw New ArgumentNullException(NameOf(rows))

            Dim norms As Double() = New Double(rows.Length - 1) {}

            Call SquaredNorms(rows, norms)

            Return norms
        End Function

#End Region

#Region "norm-expanded distance"

        ''' <summary>
        ''' 由两侧平方范数与点积还原平方距离：<c>d² = ‖a‖² + ‖b‖² − 2·a·b</c>。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function DistanceSquaredFromNorms(normA As Double, normB As Double, dot As Double) As Double
            Dim d As Double = normA + normB - 2.0 * dot

            If d <= 0.0 Then Return 0.0

            Return d
        End Function

        ''' <summary>
        ''' 一条样本对 <paramref name="centers"/> 中每个中心的平方距离，写入 <paramref name="dst"/>。
        ''' </summary>
        ''' <param name="rowNorm">样本的平方范数 <c>‖row‖²</c>（由调用方缓存，避免每轮重算）</param>
        ''' <param name="row">样本向量</param>
        ''' <param name="centers">簇中心集合</param>
        ''' <param name="centerNorms">每个中心的平方范数</param>
        ''' <param name="dst">结果缓冲（长度必须不小于中心个数）</param>
        ''' <remarks>
        ''' 这是 k-means / fuzzy c-means 内层循环的核心：把
        ''' "k 次完整的 <c>SUM((x-c)^2)</c>" 降为 "k 次点积"，
        ''' 且不产生任何临时数组。当某个中心的计算结果出现抵消误差
        ''' （负值或远小于范数量级）时，该元素自动回退为直接计算。
        ''' </remarks>
        Public Shared Sub BatchDistanceSquared(rowNorm As Double, row As Double(),
                                               centers As Double()(), centerNorms As Double(), dst As Double())
            If row Is Nothing Then Throw New ArgumentNullException(NameOf(row))
            If centers Is Nothing Then Throw New ArgumentNullException(NameOf(centers))
            If centerNorms Is Nothing Then Throw New ArgumentNullException(NameOf(centerNorms))
            If dst Is Nothing Then Throw New ArgumentNullException(NameOf(dst))

            Dim k As Integer = centers.Length

            If centerNorms.Length < k OrElse dst.Length < k Then
                Throw New ArgumentException($"the center buffers are too small: {centerNorms.Length}/{dst.Length} < {k}!")
            End If

            ' 维度校验先单独走一遍，避免在热循环里每次都比较长度（这条路径每轮被调用 n 次）
            For j As Integer = 0 To k - 1
                Dim center As Double() = centers(j)

                If center Is Nothing OrElse center.Length <> row.Length Then
                    Throw New ArgumentException($"the dimension of center[{j}] not agree with the sample!")
                End If
            Next

            For j As Integer = 0 To k - 1
                Dim center As Double() = centers(j)
                Dim dot As Double = SIMDIntrinsics.DotFma(row, center)
                Dim d As Double = rowNorm + centerNorms(j) - 2.0 * dot

                ' 抵消保护：两点几乎重合时范数展开会丢掉全部有效位，回退到精确计算。
                If d <= rowNorm * 1.0E-14 Then
                    d = SimdReduce.DistanceSquared(row, center)
                End If

                dst(j) = d
            Next
        End Sub

        ''' <summary>
        ''' 一条样本对 <paramref name="centers"/> 中每个中心的平方距离。
        ''' </summary>
        Public Shared Function BatchDistanceSquared(rowNorm As Double, row As Double(),
                                                    centers As Double()(), centerNorms As Double()) As Double()
            Dim dst As Double() = New Double(centers.Length - 1) {}

            Call BatchDistanceSquared(rowNorm, row, centers, centerNorms, dst)

            Return dst
        End Function

#End Region

#Region "pairwise metrics"

        ''' <summary>
        ''' 欧氏距离：<c>SQRT(SUM((a-b)^2))</c>。零分配、4 路 SIMD 累加。
        ''' </summary>
        Public Shared Function Euclidean(a As Double(), b As Double()) As Double
            CheckPair(a, b, NameOf(a), NameOf(b))

            Return std.Sqrt(SimdReduce.DistanceSquared(a, b))
        End Function

        ''' <summary>
        ''' 曼哈顿距离（L1）：<c>SUM(|a(i)-b(i)|)</c>。零分配、4 路 SIMD 累加。
        ''' </summary>
        Public Shared Function Manhattan(a As Double(), b As Double()) As Double
            CheckPair(a, b, NameOf(a), NameOf(b))

            Dim len As Integer = a.Length
            If len = 0 Then Return 0.0

            Dim count As Integer = Vector(Of Double).Count
            Dim step4 As Integer = count * 4
            Dim ones As New Vector(Of Double)(1.0)
            Dim i As Integer = 0
            Dim total As Double = 0

            If SIMDEnvironment.IsEnabled AndAlso len >= step4 Then
                Dim acc0 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc1 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc2 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc3 As Vector(Of Double) = Vector(Of Double).Zero
                Dim last4 As Integer = len - step4

                Do While i <= last4
                    acc0 = Vector.Add(Of Double)(acc0, Vector.Abs(Of Double)(Vector.Subtract(Of Double)(New Vector(Of Double)(a, i), New Vector(Of Double)(b, i))))
                    acc1 = Vector.Add(Of Double)(acc1, Vector.Abs(Of Double)(Vector.Subtract(Of Double)(New Vector(Of Double)(a, i + count), New Vector(Of Double)(b, i + count))))
                    acc2 = Vector.Add(Of Double)(acc2, Vector.Abs(Of Double)(Vector.Subtract(Of Double)(New Vector(Of Double)(a, i + count * 2), New Vector(Of Double)(b, i + count * 2))))
                    acc3 = Vector.Add(Of Double)(acc3, Vector.Abs(Of Double)(Vector.Subtract(Of Double)(New Vector(Of Double)(a, i + count * 3), New Vector(Of Double)(b, i + count * 3))))
                    i += step4
                Loop

                acc0 = Vector.Add(Of Double)(Vector.Add(Of Double)(acc0, acc1), Vector.Add(Of Double)(acc2, acc3))
                total = Vector.Dot(Of Double)(acc0, ones)
            End If

            For k As Integer = i To len - 1
                total += std.Abs(a(k) - b(k))
            Next

            Return total
        End Function

        ''' <summary>
        ''' 切比雪夫（上确界）距离：<c>MAX(|a(i)-b(i)|)</c>。零分配、SIMD 归约。
        ''' </summary>
        Public Shared Function Supremum(a As Double(), b As Double()) As Double
            CheckPair(a, b, NameOf(a), NameOf(b))

            Dim len As Integer = a.Length
            If len = 0 Then Return 0.0

            Dim count As Integer = Vector(Of Double).Count
            Dim i As Integer = 0
            Dim m As Double = 0

            If SIMDEnvironment.IsEnabled AndAlso len >= count Then
                Dim acc As Vector(Of Double) = Vector.Abs(Of Double)(Vector.Subtract(Of Double)(New Vector(Of Double)(a, 0), New Vector(Of Double)(b, 0)))
                Dim last As Integer = len - count

                i = count

                Do While i <= last
                    acc = Vector.Max(Of Double)(acc, Vector.Abs(Of Double)(Vector.Subtract(Of Double)(New Vector(Of Double)(a, i), New Vector(Of Double)(b, i))))
                    i += count
                Loop

                m = acc.GetElement(0)

                For k As Integer = 1 To count - 1
                    m = std.Max(m, acc.GetElement(k))
                Next
            End If

            For k As Integer = i To len - 1
                m = std.Max(m, std.Abs(a(k) - b(k)))
            Next

            Return m
        End Function

        ''' <summary>
        ''' 余弦相似度：<c>a·b / (‖a‖·‖b‖)</c>。任一侧为零向量时返回 0。
        ''' </summary>
        Public Shared Function CosineSimilarity(a As Double(), b As Double()) As Double
            CheckPair(a, b, NameOf(a), NameOf(b))

            Dim dot As Double = SimdReduce.Dot(a, b)
            Dim normA As Double = SimdReduce.SumSquares(a)
            Dim normB As Double = SimdReduce.SumSquares(b)

            If normA <= 0.0 OrElse normB <= 0.0 Then Return 0.0

            Return dot / (std.Sqrt(normA) * std.Sqrt(normB))
        End Function

        ''' <summary>
        ''' 余弦距离：<c>1 - cos(a, b)</c>。
        ''' </summary>
        Public Shared Function CosineDistance(a As Double(), b As Double()) As Double
            Return 1.0 - CosineSimilarity(a, b)
        End Function

        ''' <summary>
        ''' 皮尔逊相关系数：<c>cov(a,b) / (σa·σb)</c>。任一侧方差为零时返回 0。
        ''' </summary>
        ''' <remarks>
        ''' 采用"和/平方和/点积"三条归约直接算协方差（不做中心化临时数组）：
        ''' <c>n·Σab − Σa·Σb</c> 与 <c>n·Σa² − (Σa)²</c> 的形式，
        ''' 在大样本上比先减均值再点积少两趟内存往返。
        ''' </remarks>
        Public Shared Function PearsonCorrelation(a As Double(), b As Double()) As Double
            CheckPair(a, b, NameOf(a), NameOf(b))

            Dim n As Integer = a.Length
            If n < 2 Then Return 0.0

            Dim sumA As Double = SimdReduce.Sum(a)
            Dim sumB As Double = SimdReduce.Sum(b)
            Dim sumAA As Double = SimdReduce.SumSquares(a)
            Dim sumBB As Double = SimdReduce.SumSquares(b)
            Dim sumAB As Double = SimdReduce.Dot(a, b)

            Dim cov As Double = n * sumAB - sumA * sumB
            Dim varA As Double = n * sumAA - sumA * sumA
            Dim varB As Double = n * sumBB - sumB * sumB

            If varA <= 0.0 OrElse varB <= 0.0 Then Return 0.0

            Return cov / (std.Sqrt(varA) * std.Sqrt(varB))
        End Function

        ''' <summary>
        ''' 皮尔逊距离：<c>1 - r(a, b)</c>。
        ''' </summary>
        Public Shared Function PearsonDistance(a As Double(), b As Double()) As Double
            Return 1.0 - PearsonCorrelation(a, b)
        End Function

#End Region
    End Class
End Namespace
