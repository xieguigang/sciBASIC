Imports System.Text
Imports Microsoft.VisualBasic.DataMining.Clustering
Imports Microsoft.VisualBasic.DataMining.FuzzyCMeans
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.SIMD
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

''' <summary>
''' 数值对拍：用纯标量参考实现校验 SIMD 内核与聚类结果的正确性。
''' </summary>
Public Module ParityChecks

    Private _asserts As Integer
    Private _failures As Integer

    Private Sub Check(name As String, condition As Boolean, Optional detail As String = "")
        _asserts += 1

        If condition Then
            Console.WriteLine($"  [PASS] {name}")
        Else
            _failures += 1
            Console.WriteLine($"  [FAIL] {name} {detail}")
        End If
    End Sub

    Private Sub CheckClose(name As String, actual As Double, expected As Double, tolerance As Double)
        Dim scale As Double = std.Max(1.0, std.Abs(expected))
        Dim diff As Double = std.Abs(actual - expected) / scale

        Call Check(name, diff <= tolerance, $"actual={actual:G17} expected={expected:G17} rel={diff:E3}")
    End Sub

    ''' <summary>
    ''' 运行全部对拍，返回退出码（0 = 全部通过）。
    ''' </summary>
    Public Function Run() As Integer
        _asserts = 0
        _failures = 0

        Console.WriteLine("== 距离内核对拍 ==")
        Call CheckDistanceKernels()

        Console.WriteLine("== 选择内核对拍 ==")
        Call CheckSelectKernels()

        Console.WriteLine("== 成对距离矩阵对拍 ==")
        Call CheckPairwiseMatrix()

        Console.WriteLine("== KMeans / FuzzyCMeans 一致性对拍 ==")
        Call CheckClusteringParity()

        Console.WriteLine()
        Console.WriteLine($"断言: {_asserts}, 失败: {_failures}")

        Return If(_failures = 0, 0, 1)
    End Function

    Private Function ScalarDistanceSquared(a As Double(), b As Double()) As Double
        Dim sum As Double = 0

        For i As Integer = 0 To std.Min(a.Length, b.Length) - 1
            Dim d As Double = a(i) - b(i)

            sum += d * d
        Next

        Return sum
    End Function

    Private Function ScalarManhattan(a As Double(), b As Double()) As Double
        Dim sum As Double = 0

        For i As Integer = 0 To a.Length - 1
            sum += std.Abs(a(i) - b(i))
        Next

        Return sum
    End Function

    Private Function ScalarSupremum(a As Double(), b As Double()) As Double
        Dim m As Double = 0

        For i As Integer = 0 To a.Length - 1
            m = std.Max(m, std.Abs(a(i) - b(i)))
        Next

        Return m
    End Function

    Private Function ScalarPearson(a As Double(), b As Double()) As Double
        Dim n As Integer = a.Length
        Dim ma As Double = 0
        Dim mb As Double = 0

        For i As Integer = 0 To n - 1
            ma += a(i)
            mb += b(i)
        Next

        ma /= n
        mb /= n

        Dim cov As Double = 0
        Dim va As Double = 0
        Dim vb As Double = 0

        For i As Integer = 0 To n - 1
            Dim da As Double = a(i) - ma
            Dim db As Double = b(i) - mb

            cov += da * db
            va += da * da
            vb += db * db
        Next

        Return cov / std.Sqrt(va * vb)
    End Function

    Private Sub CheckDistanceKernels()
        Dim rng As New Random(1)
        Dim dims As Integer = 128
        Dim maxSqErr As Double = 0
        Dim maxManhattanErr As Double = 0
        Dim maxSupremumErr As Double = 0
        Dim maxPearsonErr As Double = 0
        Dim maxEuclidErr As Double = 0

        For t As Integer = 0 To 199
            Dim a As Double() = New Double(dims - 1) {}
            Dim b As Double() = New Double(dims - 1) {}

            For i As Integer = 0 To dims - 1
                a(i) = rng.NextDouble() * 20 - 10
                b(i) = a(i) + rng.NextDouble() * 0.01
            Next

            Dim refSq As Double = ScalarDistanceSquared(a, b)
            Dim actualSq As Double = SimdReduce.DistanceSquared(a, b)

            maxSqErr = std.Max(maxSqErr,
                               std.Abs(actualSq - refSq) / std.Max(1.0, refSq))

            maxEuclidErr = std.Max(maxEuclidErr,
                                   std.Abs(SimdDistance.Euclidean(a, b) - std.Sqrt(refSq)) / std.Max(1.0, std.Sqrt(refSq)))

            maxManhattanErr = std.Max(maxManhattanErr,
                                      std.Abs(SimdDistance.Manhattan(a, b) - ScalarManhattan(a, b)) / std.Max(1.0, ScalarManhattan(a, b)))

            maxSupremumErr = std.Max(maxSupremumErr,
                                     std.Abs(SimdDistance.Supremum(a, b) - ScalarSupremum(a, b)))

            maxPearsonErr = std.Max(maxPearsonErr,
                                    std.Abs(SimdDistance.PearsonCorrelation(a, b) - ScalarPearson(a, b)))
        Next

        Call Check("DistanceSquared 相对误差 <= 1e-9", maxSqErr <= 1.0E-9, $"max={maxSqErr:E3}")
        Call Check("Euclidean 相对误差 <= 1e-9", maxEuclidErr <= 1.0E-9, $"max={maxEuclidErr:E3}")
        Call Check("Manhattan 相对误差 <= 1e-9", maxManhattanErr <= 1.0E-9, $"max={maxManhattanErr:E3}")
        Call Check("Supremum 绝对误差 <= 1e-9", maxSupremumErr <= 1.0E-9, $"max={maxSupremumErr:E3}")
        Call Check("Pearson 绝对误差 <= 1e-9", maxPearsonErr <= 1.0E-9, $"max={maxPearsonErr:E3}")

        ' 长度不一致必须抛出，而不是静默截断
        Dim longVec As Double() = New Double(dims - 1) {}
        Dim shortVec As Double() = New Double(3) {}
        Dim thrown As Boolean = False

        Try
            Call SimdReduce.DistanceSquared(longVec, shortVec)
        Catch ex As ArgumentException
            thrown = True
        End Try

        Call Check("长度不一致抛出 ArgumentException", thrown)
    End Sub

    Private Sub CheckSelectKernels()
        Dim rng As New Random(2)
        Dim values As Double() = New Double(999) {}

        For i As Integer = 0 To values.Length - 1
            ' 刻意制造大量重复值，检验 tie-break 是否稳定
            values(i) = rng.Next(0, 50)
        Next

        For Each k As Integer In New Integer() {1, 5, 32, 100, 500, 1000}
            Dim expected As Integer() = Enumerable _
                .Range(0, values.Length) _
                .OrderBy(Function(i) values(i)) _
                .Take(k) _
                .ToArray()
            Dim actual As Integer() = SimdSelect.TopKIndices(values, k)

            Call Check($"TopK n=1000 k={k} 与 OrderBy 完全一致",
                       actual.SequenceEqual(expected),
                       $"actual={String.Join(",", actual.Take(5))} expected={String.Join(",", expected.Take(5))}")
        Next

        Dim argMinExpected As Integer = Array.IndexOf(values, values.Min)
        Dim argMaxExpected As Integer = Array.IndexOf(values, values.Max)

        Call Check("ArgMin 与 Array.IndexOf(Min) 一致", SimdSelect.ArgMin(values) = argMinExpected)
        Call Check("ArgMax 与 Array.IndexOf(Max) 一致", SimdSelect.ArgMax(values) = argMaxExpected)

        ' 区间版本
        Dim start As Integer = 100
        Dim ends As Integer = 300
        Dim expectedRange As Integer = start

        For i As Integer = start + 1 To ends - 1
            If values(i) < values(expectedRange) Then expectedRange = i
        Next

        Call Check("ArgMin(start, ends) 与手工循环一致",
                   SimdSelect.ArgMin(values, start, ends) = expectedRange)

        ' 空/越界输入
        Dim thrown As Boolean = False

        Try
            Call SimdSelect.ArgMin(Array.Empty(Of Double)())
        Catch ex As ArgumentException
            thrown = True
        End Try

        Call Check("空数组 ArgMin 抛出异常", thrown)
        Call Check("k<=0 返回空数组", SimdSelect.TopKIndices(values, 0).Length = 0)
    End Sub

    Private Sub CheckPairwiseMatrix()
        Dim rows As Double()() = BenchCases.Synthetic(120, 15, 3, 5)
        Dim upper As Double() = SimdParallel.PairwiseDistance(rows)
        Dim full As Double() = SimdParallel.PairwiseDistance(rows, upperTriangleOnly:=False)
        Dim n As Integer = rows.Length
        Dim maxErr As Double = 0

        For i As Integer = 0 To n - 1
            For j As Integer = i + 1 To n - 1
                Dim expected As Double = ScalarDistanceSquared(rows(i), rows(j))
                Dim idx As Integer = SimdParallel.UpperTriangleIndex(n, i, j)

                maxErr = std.Max(maxErr, std.Abs(upper(idx) - expected) / std.Max(1.0, expected))
                maxErr = std.Max(maxErr, std.Abs(full(i * n + j) - expected) / std.Max(1.0, expected))
                maxErr = std.Max(maxErr, std.Abs(full(j * n + i) - expected) / std.Max(1.0, expected))
            Next
        Next

        Call Check("上三角长度 = n(n-1)/2",
                   upper.Length = CInt(SimdParallel.UpperTriangleLength(n)),
                   $"{upper.Length} vs {SimdParallel.UpperTriangleLength(n)}")
        Call Check("PairwiseDistance 与标量参考一致 (1e-9)", maxErr <= 1.0E-9, $"max={maxErr:E3}")
        Call Check("对称矩阵对角为 0", full(0) = 0 AndAlso full(n * n - 1) = 0)
    End Sub

    Private Sub CheckClusteringParity()
        Dim rows As Double()() = BenchCases.Synthetic(400, 12, 4, 9)
        Dim ids As String() = BenchCases.Ids(rows.Length)

        ' --- KMeans：SIMD 路径 vs 纯标量路径（同一随机种子）---
        randf.SetSeed(321)

        SIMDEnvironment.config = SIMDConfiguration.auto
        SimdParallel.Enable = True

        Dim enabledCss As New KMeansEngine(max_iters:=25)
        Dim enabledResult = enabledCss.ClusterDataSet(rows, ids, 4)
        Dim enabledCost As Double = TotalCost(enabledResult)

        SIMDEnvironment.config = SIMDConfiguration.disable
        SimdParallel.Enable = False

        Dim scalarCss As New KMeansEngine(max_iters:=25)

        randf.SetSeed(321)

        Dim scalarResult = scalarCss.ClusterDataSet(rows, ids, 4)
        Dim scalarCost As Double = TotalCost(scalarResult)

        Call CheckClose("KMeans 目标函数值（SIMD vs 标量）", enabledCost, scalarCost, 1.0E-9)

        ' 恢复配置
        SIMDEnvironment.config = SIMDConfiguration.auto
        SimdParallel.Enable = True

        ' --- FuzzyCMeans：隶属度矩阵一致性 ---
        randf.SetSeed(777)

        Dim fuzzy As New CMeansEngine()
        Dim fuzzyResult = fuzzy.Run(rows, 4, threshold:=0.0001, maxLoop:=10, parallel:=False)

        randf.SetSeed(777)

        SandboxScalar()

        Dim fuzzyScalar = fuzzy.Run(rows, 4, threshold:=0.0001, maxLoop:=10, parallel:=False)
        Dim maxDiff As Double = 0

        For i As Integer = 0 To fuzzyResult.membership.Length - 1
            For j As Integer = 0 To fuzzyResult.membership(i).Length - 1
                maxDiff = std.Max(maxDiff, std.Abs(fuzzyResult.membership(i)(j) - fuzzyScalar.membership(i)(j)))
            Next
        Next

        Call CheckClose("FuzzyCMeans 隶属度（SIMD vs 标量）", maxDiff, 0.0, 1.0E-9)

        SIMDEnvironment.config = SIMDConfiguration.auto
        SimdParallel.Enable = True
    End Sub

    Private Sub SandboxScalar()
        SIMDEnvironment.config = SIMDConfiguration.disable
        SimdParallel.Enable = False
    End Sub

    Private Function TotalCost(clusters As NumericClusterCollection) As Double
        Dim total As Double = 0

        For i As Integer = 0 To clusters.NumOfCluster - 1
            total += clusters(i).CalculateKMeansCost()
        Next

        Return total
    End Function
End Module
