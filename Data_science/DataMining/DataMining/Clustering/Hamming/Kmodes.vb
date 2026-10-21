Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading.Tasks
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Hamming

    ''' <summary>
    ''' k-modes 的初始中心选取策略。
    ''' </summary>
    Public Enum KmodesInitialization
        ''' <summary>
        ''' 从样本中不放回地随机抽取 k 个对象作为初始质心。
        ''' </summary>
        Random = 0
        ''' <summary>
        ''' k-modes++：第一个质心随机，后续质心按照到最近已选质心的距离平方加权抽样，
        ''' 使初始质心尽量分散（需要为 <c>Run</c> 传入固定的 <c>seed</c> 才能复现）。
        ''' </summary>
        PlusPlus = 1
    End Enum

    ''' <summary>
    ''' k-modes 聚类的计算结果。
    ''' 
    ''' 标签为 **0 基**（``0 .. k-1``），表扩展层（
    ''' <see cref="HammingTableExtensions.hammingCluster"/>) 会统一加 1 变成
    ''' 与 <c>dbscan</c> / <c>bisectingKMeans</c> 一致的 1 基编号。
    ''' </summary>
    Public Class KmodesResult

        Private ReadOnly _labels As Integer()
        Private ReadOnly _centroids As Integer()()
        Private ReadOnly _clusters As Integer()()
        Private ReadOnly _cost As Double
        Private ReadOnly _iterations As Integer
        Private ReadOnly _converged As Boolean
        Private ReadOnly _emptyClusters As Integer
        Private ReadOnly _seed As Integer

        ''' <summary>
        ''' 构造 k-modes 的计算结果。
        ''' </summary>
        ''' <param name="labels">每一个样本所属的簇编号（0 基）</param>
        ''' <param name="centroids">每一个簇的众数质心（类别码向量）</param>
        ''' <param name="clusters">每一个簇所包含的样本下标</param>
        ''' <param name="cost">全部样本到其所属质心的 Hamming 距离之和</param>
        ''' <param name="iterations">实际执行的迭代轮数</param>
        ''' <param name="converged">是否因为质心不再变化而提前收敛</param>
        ''' <param name="emptyClusters">最后一轮迭代中没有任何样本的簇数量</param>
        ''' <param name="seed">本次计算实际使用的随机种子（便于复现）</param>
        Public Sub New(labels As Integer(),
                       centroids As Integer()(),
                       clusters As Integer()(),
                       cost As Double,
                       iterations As Integer,
                       converged As Boolean,
                       emptyClusters As Integer,
                       seed As Integer)

            _labels = labels
            _centroids = centroids
            _clusters = clusters
            _cost = cost
            _iterations = iterations
            _converged = converged
            _emptyClusters = emptyClusters
            _seed = seed
        End Sub

        ''' <summary>
        ''' 簇的数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property K As Integer
            Get
                Return _centroids.Length
            End Get
        End Property

        ''' <summary>
        ''' 每一个样本所属的簇编号（0 基）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Labels As Integer()
            Get
                Return _labels
            End Get
        End Property

        ''' <summary>
        ''' 每一个簇的众数质心，``centroids(cluster)(feature)``。
        ''' 
        ''' 质心的每一个分量都是该簇在该特征上出现频率最高的取值，
        ''' 因此质心本身就是一条真实存在的「取值组合」。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Centroids As Integer()()
            Get
                Return _centroids
            End Get
        End Property

        ''' <summary>
        ''' 每一个簇所包含的样本下标集合。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Clusters As Integer()()
            Get
                Return _clusters
            End Get
        End Property

        ''' <summary>
        ''' 每一个簇的样本数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ClusterSizes As Integer()
            Get
                Dim sizes As Integer() = New Integer(_clusters.Length - 1) {}

                For i As Integer = 0 To _clusters.Length - 1
                    sizes(i) = If(_clusters(i) Is Nothing, 0, _clusters(i).Length)
                Next

                Return sizes
            End Get
        End Property

        ''' <summary>
        ''' 全部样本到其所属质心的 Hamming 距离之和（越小表示簇内越紧凑）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Cost As Double
            Get
                Return _cost
            End Get
        End Property

        ''' <summary>
        ''' 实际执行的迭代轮数
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Iterations As Integer
            Get
                Return _iterations
            End Get
        End Property

        ''' <summary>
        ''' 是否因为质心不再变化而提前收敛（``False`` 表示用尽了最大迭代次数）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Converged As Boolean
            Get
                Return _converged
            End Get
        End Property

        ''' <summary>
        ''' 最后一轮迭代中没有任何样本的簇数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property EmptyClusters As Integer
            Get
                Return _emptyClusters
            End Get
        End Property

        ''' <summary>
        ''' 本次计算实际使用的随机种子，传入同一个种子可以得到完全相同的结果
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Seed As Integer
            Get
                Return _seed
            End Get
        End Property

        ''' <summary>
        ''' 把众数质心转换为位模式（仅当输入数据是二值的时候可用）。
        ''' </summary>
        ''' <returns></returns>
        Public Function CentroidPatterns() As BitPattern()
            Dim patterns As BitPattern() = New BitPattern(_centroids.Length - 1) {}

            For i As Integer = 0 To _centroids.Length - 1
                patterns(i) = BitPattern.FromIntegers(_centroids(i))
            Next

            Return patterns
        End Function

        ''' <summary>
        ''' 输出每个簇的样本数量与总代价，便于调试。
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"k-modes[k={K}, cost={Cost}, iterations={Iterations}, converged={Converged}, sizes={String.Join(",", ClusterSizes)}]"
        End Function
    End Class

    ''' <summary>
    ''' k-modes 聚类引擎：用 Hamming 距离替代欧氏距离，
    ''' 用簇内「众数」（逐位多数表决）替代均值作为质心。
    ''' 
    ''' 标准流程：
    ''' 
    ''' 1. 初始化：随机或 k-modes++ 选取 k 个样本作为初始质心；
    ''' 2. 分配：把每个样本划分到 Hamming 距离最近的质心所在的簇
    '''    （距离并列时取较小的簇编号，保证结果可复现）；
    ''' 3. 更新：对每个簇逐位取众数（平票时取较小的类别码）；
    ''' 4. 迭代：直到质心不再变化或达到最大迭代次数。
    ''' 
    ''' 当全部特征的取值基数都不大于 2 时，距离计算会自动走
    ''' 「异或 + popcount」的位运算快速路径。
    ''' </summary>
    Public Class KmodesEngine

        ''' <summary>
        ''' 样本数量达到这个阈值之后，分配阶段才会启用并行计算。
        ''' </summary>
        Private Const ParallelThreshold As Integer = 512

        ''' <summary>
        ''' 在统一编码之后的中间表示上执行 k-modes 聚类。
        ''' </summary>
        ''' <param name="data">统一编码之后的类别码矩阵</param>
        ''' <param name="k">簇的数量，必须满足 ``1 &lt;= k &lt;= 样本数量``</param>
        ''' <param name="seed">随机种子；缺省时自动生成一个并写入 <see cref="KmodesResult.Seed"/></param>
        ''' <param name="initialization">初始质心的选取策略</param>
        ''' <param name="maxIterations">最大迭代次数</param>
        ''' <param name="parallel">是否在分配阶段并行计算（样本数较少时自动退化为串行）</param>
        ''' <returns></returns>
        Public Function Run(data As HammingData,
                            k As Integer,
                            Optional seed As Integer? = Nothing,
                            Optional initialization As KmodesInitialization = KmodesInitialization.PlusPlus,
                            Optional maxIterations As Integer = 100,
                            Optional parallel As Boolean = True) As KmodesResult

            If data Is Nothing Then
                Throw New ArgumentNullException(NameOf(data))
            End If

            Dim samples As Integer = data.Samples

            If k < 1 Then
                Throw New ArgumentException($"the cluster count must be a positive integer, but {k}!", NameOf(k))
            End If
            If k > samples Then
                Throw New ArgumentException($"the cluster count {k} cannot be greater than the sample count {samples}!", NameOf(k))
            End If
            If maxIterations < 1 Then
                Throw New ArgumentException($"the max iterations must be a positive integer, but {maxIterations}!", NameOf(maxIterations))
            End If

            Dim actualSeed As Integer = If(seed.HasValue, seed.Value, randf.seeds.Next())
            Dim rng As New Random(actualSeed)
            Dim patterns As BitPattern() = Nothing

            If data.IsBinary Then
                patterns = data.ToPatterns()
            End If

            Dim centroids As Integer()() = CreateInitialCenters(data, patterns, k, rng, initialization)
            Dim labels As Integer() = New Integer(samples - 1) {}
            Dim iterations As Integer = 0
            Dim converged As Boolean = False
            Dim emptyClusters As Integer = 0

            While iterations < maxIterations
                Dim assigned As Integer() = Assign(data, patterns, centroids, parallel)
                Dim empty As Integer = 0
                Dim updated As Integer()() = UpdateModes(data, assigned, centroids, empty)

                iterations += 1
                labels = assigned
                emptyClusters = empty

                Dim stable As Boolean = CentroidsEquals(centroids, updated)

                centroids = updated

                If stable Then
                    converged = True
                    Exit While
                End If
            End While

            If Not converged Then
                ' 迭代次数用尽时，标签必须和最终质心保持一致
                labels = Assign(data, patterns, centroids, parallel)
                emptyClusters = CountEmpty(labels, centroids.Length)
            End If

            Dim cost As Double = 0

            For i As Integer = 0 To samples - 1
                cost += SampleDistance(data, patterns, i, centroids, labels(i))
            Next

            Dim clusters As Integer()() = GroupMembers(labels, centroids.Length)

            Return New KmodesResult(labels, centroids, clusters, cost, iterations, converged, emptyClusters, actualSeed)
        End Function

        ''' <summary>
        ''' 直接在数值矩阵上执行 k-modes 聚类（内部会先做统一编码）。
        ''' </summary>
        ''' <param name="rows">数值矩阵，行主序</param>
        ''' <param name="k">簇的数量</param>
        ''' <param name="binary">为 ``True`` 时按阈值二值化，否则把相异取值当作定类取值</param>
        ''' <param name="threshold">二值化阈值</param>
        ''' <param name="seed">随机种子</param>
        ''' <param name="initialization">初始质心的选取策略</param>
        ''' <param name="maxIterations">最大迭代次数</param>
        ''' <param name="parallel">是否并行计算</param>
        ''' <returns></returns>
        Public Function Run(rows As Double()(),
                            k As Integer,
                            Optional binary As Boolean = True,
                            Optional threshold As Double = 0.5,
                            Optional seed As Integer? = Nothing,
                            Optional initialization As KmodesInitialization = KmodesInitialization.PlusPlus,
                            Optional maxIterations As Integer = 100,
                            Optional parallel As Boolean = True) As KmodesResult

            Return Run(HammingEncoder.FromValues(rows, binary, threshold), k, seed, initialization, maxIterations, parallel)
        End Function

        ''' <summary>
        ''' 直接在二进制位串集合上执行 k-modes 聚类。
        ''' </summary>
        ''' <param name="bits">例如 ``{"10110010", "10100011", "01001100", "01101100"}``</param>
        ''' <param name="k">簇的数量</param>
        ''' <param name="seed">随机种子</param>
        ''' <param name="initialization">初始质心的选取策略</param>
        ''' <param name="maxIterations">最大迭代次数</param>
        ''' <param name="parallel">是否并行计算</param>
        ''' <returns></returns>
        Public Function RunBitStrings(bits As String(),
                                      k As Integer,
                                      Optional seed As Integer? = Nothing,
                                      Optional initialization As KmodesInitialization = KmodesInitialization.PlusPlus,
                                      Optional maxIterations As Integer = 100,
                                      Optional parallel As Boolean = True) As KmodesResult

            Return Run(HammingEncoder.FromBitStrings(bits), k, seed, initialization, maxIterations, parallel)
        End Function

        Private Shared Function CreateInitialCenters(data As HammingData,
                                                     patterns As BitPattern(),
                                                     k As Integer,
                                                     rng As Random,
                                                     initialization As KmodesInitialization) As Integer()()

            Dim samples As Integer = data.Samples
            Dim chosen As New List(Of Integer)()
            Dim picked As New HashSet(Of Integer)()

            If initialization = KmodesInitialization.Random Then
                While chosen.Count < k
                    Dim index As Integer = rng.Next(samples)

                    If picked.Add(index) Then
                        Call chosen.Add(index)
                    End If
                End While
            Else
                ' k-modes++：首个质心随机，后续质心按 D² 加权抽样
                Dim first As Integer = rng.Next(samples)
                Dim nearest As Integer() = New Integer(samples - 1) {}

                Call chosen.Add(first)
                Call picked.Add(first)

                For i As Integer = 0 To samples - 1
                    nearest(i) = PairwiseDistance(data, patterns, i, first)
                Next

                While chosen.Count < k
                    Dim total As Double = 0

                    For i As Integer = 0 To samples - 1
                        If Not picked.Contains(i) Then
                            total += nearest(i) * nearest(i)
                        End If
                    Next

                    Dim nextIndex As Integer = -1

                    If total <= 0 Then
                        ' 剩余样本与已选质心完全重合：退化为顺序取第一个未选样本
                        For i As Integer = 0 To samples - 1
                            If Not picked.Contains(i) Then
                                nextIndex = i
                                Exit For
                            End If
                        Next
                    Else
                        Dim target As Double = rng.NextDouble() * total
                        Dim cumulative As Double = 0

                        For i As Integer = 0 To samples - 1
                            If picked.Contains(i) OrElse nearest(i) = 0 Then
                                Continue For
                            End If

                            cumulative += nearest(i) * nearest(i)

                            If cumulative >= target Then
                                nextIndex = i
                                Exit For
                            End If
                        Next

                        If nextIndex < 0 Then
                            ' 浮点累加的舍入可能落空，回退到最后一个有权重的候选
                            For i As Integer = samples - 1 To 0 Step -1
                                If Not picked.Contains(i) AndAlso nearest(i) > 0 Then
                                    nextIndex = i
                                    Exit For
                                End If
                            Next
                        End If
                    End If

                    If nextIndex < 0 Then
                        Exit While
                    End If

                    Call chosen.Add(nextIndex)
                    Call picked.Add(nextIndex)

                    For i As Integer = 0 To samples - 1
                        Dim distance As Integer = PairwiseDistance(data, patterns, i, nextIndex)

                        If distance < nearest(i) Then
                            nearest(i) = distance
                        End If
                    Next
                End While
            End If

            If chosen.Count < k Then
                Throw New InvalidOperationException($"unable to select {k} distinct initial centers from {samples} samples!")
            End If

            Dim centroids As Integer()() = New Integer(k - 1)() {}

            For i As Integer = 0 To k - 1
                centroids(i) = DirectCast(data.Codes(chosen(i)).Clone(), Integer())
            Next

            Return centroids
        End Function

        Private Shared Function Assign(data As HammingData,
                                       patterns As BitPattern(),
                                       centroids As Integer()(),
                                       parallel As Boolean) As Integer()

            Dim samples As Integer = data.Samples
            Dim centroidPatterns As BitPattern() = Nothing

            If patterns IsNot Nothing Then
                centroidPatterns = New BitPattern(centroids.Length - 1) {}

                For c As Integer = 0 To centroids.Length - 1
                    centroidPatterns(c) = BitPattern.FromIntegers(centroids(c))
                Next
            End If

            If parallel AndAlso samples >= ParallelThreshold Then
                Return Enumerable _
                    .Range(0, samples) _
                    .AsParallel() _
                    .AsOrdered() _
                    .Select(Function(i) Nearest(data, patterns, centroidPatterns, centroids, i)) _
                    .ToArray()
            End If

            Dim labels As Integer() = New Integer(samples - 1) {}

            For i As Integer = 0 To samples - 1
                labels(i) = Nearest(data, patterns, centroidPatterns, centroids, i)
            Next

            Return labels
        End Function

        Private Shared Function Nearest(data As HammingData,
                                        patterns As BitPattern(),
                                        centroidPatterns As BitPattern(),
                                        centroids As Integer()(),
                                        sample As Integer) As Integer

            Dim best As Integer = 0
            Dim bestDistance As Integer = Integer.MaxValue

            For c As Integer = 0 To centroids.Length - 1
                Dim distance As Integer

                If patterns IsNot Nothing Then
                    distance = patterns(sample).DistanceTo(centroidPatterns(c))
                Else
                    distance = CategoricalDistance(data.Codes(sample), centroids(c))
                End If

                ' 只在严格更小时替换，因此距离并列时总是落到较小的簇编号
                If distance < bestDistance Then
                    bestDistance = distance
                    best = c
                End If
            Next

            Return best
        End Function

        Private Shared Function UpdateModes(data As HammingData,
                                            labels As Integer(),
                                            centroids As Integer()(),
                                            ByRef emptyClusters As Integer) As Integer()()

            Dim k As Integer = centroids.Length
            Dim width As Integer = data.Dimension
            Dim sizes As Integer() = New Integer(k - 1) {}
            Dim maxCardinality As Integer = 1

            For i As Integer = 0 To labels.Length - 1
                sizes(labels(i)) += 1
            Next

            For j As Integer = 0 To width - 1
                If data.Cardinality(j) > maxCardinality Then
                    maxCardinality = data.Cardinality(j)
                End If
            Next

            Dim updated As Integer()() = New Integer(k - 1)() {}
            Dim counts As Integer() = New Integer(maxCardinality - 1) {}
            Dim empty As Integer = 0

            For c As Integer = 0 To k - 1
                updated(c) = New Integer(width - 1) {}

                If sizes(c) = 0 Then
                    ' 空簇保留上一轮的质心，避免出现没有中心的簇
                    For j As Integer = 0 To width - 1
                        updated(c)(j) = centroids(c)(j)
                    Next

                    empty += 1
                    Continue For
                End If

                For j As Integer = 0 To width - 1
                    Call Array.Clear(counts, 0, counts.Length)

                    For i As Integer = 0 To labels.Length - 1
                        If labels(i) = c Then
                            counts(data.Codes(i)(j)) += 1
                        End If
                    Next

                    Dim bestValue As Integer = 0
                    Dim bestCount As Integer = counts(0)

                    ' 平票时取较小的类别码，保证结果确定
                    For value As Integer = 1 To data.Cardinality(j) - 1
                        If counts(value) > bestCount Then
                            bestCount = counts(value)
                            bestValue = value
                        End If
                    Next

                    updated(c)(j) = bestValue
                Next
            Next

            emptyClusters = empty

            Return updated
        End Function

        Private Shared Function CentroidsEquals(x As Integer()(), y As Integer()()) As Boolean
            For c As Integer = 0 To x.Length - 1
                For j As Integer = 0 To x(c).Length - 1
                    If x(c)(j) <> y(c)(j) Then
                        Return False
                    End If
                Next
            Next

            Return True
        End Function

        Private Shared Function CountEmpty(labels As Integer(), k As Integer) As Integer
            Dim sizes As Integer() = New Integer(k - 1) {}

            For i As Integer = 0 To labels.Length - 1
                sizes(labels(i)) += 1
            Next

            Dim empty As Integer = 0

            For c As Integer = 0 To k - 1
                If sizes(c) = 0 Then
                    empty += 1
                End If
            Next

            Return empty
        End Function

        Private Shared Function GroupMembers(labels As Integer(), k As Integer) As Integer()()
            Dim sizes As Integer() = New Integer(k - 1) {}

            For i As Integer = 0 To labels.Length - 1
                sizes(labels(i)) += 1
            Next

            Dim clusters As Integer()() = New Integer(k - 1)() {}

            For c As Integer = 0 To k - 1
                clusters(c) = New Integer(sizes(c) - 1) {}
            Next

            Dim offsets As Integer() = New Integer(k - 1) {}

            For i As Integer = 0 To labels.Length - 1
                Dim c As Integer = labels(i)

                clusters(c)(offsets(c)) = i
                offsets(c) += 1
            Next

            Return clusters
        End Function

        Private Shared Function SampleDistance(data As HammingData,
                                               patterns As BitPattern(),
                                               sample As Integer,
                                               centroids As Integer()(),
                                               center As Integer) As Integer

            If patterns IsNot Nothing Then
                Return patterns(sample).DistanceTo(BitPattern.FromIntegers(centroids(center)))
            Else
                Return CategoricalDistance(data.Codes(sample), centroids(center))
            End If
        End Function

        Private Shared Function PairwiseDistance(data As HammingData,
                                                 patterns As BitPattern(),
                                                 sample As Integer,
                                                 other As Integer) As Integer

            If patterns IsNot Nothing Then
                Return patterns(sample).DistanceTo(patterns(other))
            Else
                Return CategoricalDistance(data.Codes(sample), data.Codes(other))
            End If
        End Function

        Private Shared Function CategoricalDistance(x As Integer(), y As Integer()) As Integer
            Dim sum As Integer = 0

            For j As Integer = 0 To x.Length - 1
                If x(j) <> y(j) Then
                    sum += 1
                End If
            Next

            Return sum
        End Function
    End Class
End Namespace
