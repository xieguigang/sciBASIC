Imports System.Diagnostics
Imports System.Text
Imports Microsoft.VisualBasic.DataMining.Clustering
Imports Microsoft.VisualBasic.DataMining.DBSCAN
Imports Microsoft.VisualBasic.DataMining.FuzzyCMeans
Imports Microsoft.VisualBasic.DataMining.HDBSCAN.Distance
Imports Microsoft.VisualBasic.DataMining.HDBSCAN.Runner
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math.SIMD
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

''' <summary>
''' 聚类算法的基准用例集合。
''' </summary>
Public Module BenchCases

    ''' <summary>
    ''' 单个用例的测量结果。
    ''' </summary>
    Public Structure CaseResult
        Public Name As String
        Public Milliseconds As Double
        Public AllocatedMB As Double
        Public Detail As String
    End Structure

    ''' <summary>
    ''' 生成 <paramref name="clusters"/> 个高斯团的合成数据集（固定 seed，保证可复现）。
    ''' </summary>
    Public Function Synthetic(rows As Integer, cols As Integer, clusters As Integer, seed As Integer) As Double()()
        Dim rng As New Random(seed)
        Dim centers As Double()() = New Double(clusters - 1)() {}

        For i As Integer = 0 To clusters - 1
            Dim center As Double() = New Double(cols - 1) {}

            For j As Integer = 0 To cols - 1
                center(j) = rng.NextDouble() * 20 - 10
            Next

            centers(i) = center
        Next

        Dim data As Double()() = New Double(rows - 1)() {}

        For i As Integer = 0 To rows - 1
            Dim center As Double() = centers(i Mod clusters)
            Dim row As Double() = New Double(cols - 1) {}

            For j As Integer = 0 To cols - 1
                ' Box-Muller，避免依赖第三方正态分布实现
                Dim u1 As Double = 1.0 - rng.NextDouble()
                Dim u2 As Double = rng.NextDouble()
                Dim gauss As Double = std.Sqrt(-2.0 * std.Log(u1)) * std.Cos(2.0 * std.PI * u2)

                row(j) = center(j) + gauss * 0.5
            Next

            data(i) = row
        Next

        Return data
    End Function

    ''' <summary>
    ''' 生成自增样本 ID。
    ''' </summary>
    Public Function Ids(rows As Integer) As String()
        ' 注意：局部变量不能叫 ids —— VB 大小写不敏感，会与函数名 Ids 冲突
        Dim buffer As String() = New String(rows - 1) {}

        For i As Integer = 0 To rows - 1
            buffer(i) = $"sample_{i}"
        Next

        Return buffer
    End Function

    ''' <summary>
    ''' 静默的日志回调（DBSCAN 会在每个阶段打印进度）。
    ''' </summary>
    Private Sub Silent(message As Object)
    End Sub

    Private Function Measure(name As String, action As Action, Optional detail As String = "") As CaseResult
        GC.Collect()
        GC.WaitForPendingFinalizers()
        GC.Collect()

        Dim allocBefore As Long = GC.GetTotalAllocatedBytes(True)
        Dim timer As Stopwatch = Stopwatch.StartNew()

        action()

        timer.Stop()

        Dim allocAfter As Long = GC.GetTotalAllocatedBytes(True)

        Return New CaseResult With {
            .Name = name,
            .Milliseconds = timer.Elapsed.TotalMilliseconds,
            .AllocatedMB = (allocAfter - allocBefore) / 1048576.0,
            .Detail = detail
        }
    End Function

    ''' <summary>
    ''' 运行全部基准用例。
    ''' </summary>
    ''' <param name="quick">快速模式（小规模，用于 CI/冒烟）</param>
    ''' <param name="includeSpectral">是否包含谱聚类（O(n^3) 特征分解，耗时较长）</param>
    Public Function RunAll(quick As Boolean, includeSpectral As Boolean) As CaseResult()
        Dim results As New List(Of CaseResult)()

        ' ---------- 1. 成对距离内核 ----------
        Dim pairs As Integer = If(quick, 50000, 500000)
        Dim dims As Integer = 100
        Dim probe As Double()() = Synthetic(2, dims, 1, 7)
        Dim a As Double() = probe(0)
        Dim b As Double() = probe(1)

        Call results.Add(Measure("distance-squared x" & pairs,
            Sub()
                Dim sink As Double = 0

                For i As Integer = 0 To pairs - 1
                    sink += SimdReduce.DistanceSquared(a, b)
                Next

                If sink = Double.MinValue Then Console.WriteLine(sink)
            End Sub, $"d={dims}"))

        Call results.Add(Measure("square-distance(alloc path) x" & pairs,
            Sub()
                Dim sink As Double = 0

                For i As Integer = 0 To pairs - 1
                    sink += Microsoft.VisualBasic.Math.Correlations.DistanceMethods.SquareDistance(a, b)
                Next

                If sink = Double.MinValue Then Console.WriteLine(sink)
            End Sub, $"d={dims}"))

        ' ---------- 2. Top-K 选择 ----------
        Dim poolSize As Integer = If(quick, 20000, 200000)
        Dim topK As Integer = 10
        Dim values As Double() = New Double(poolSize - 1) {}
        Dim rng As New Random(11)

        For i As Integer = 0 To poolSize - 1
            values(i) = rng.NextDouble()
        Next

        Call results.Add(Measure("topk(select) n=" & poolSize & " k=" & topK,
            Sub()
                Dim idx As Integer() = SimdSelect.TopKIndices(values, topK)

                If idx.Length <> topK Then Throw New Exception("topk length mismatch!")
            End Sub))

        Call results.Add(Measure("topk(linq orderby) n=" & poolSize & " k=" & topK,
            Sub()
                Dim idx As Integer() = Enumerable _
                    .Range(0, values.Length) _
                    .OrderBy(Function(i) values(i)) _
                    .Take(topK) _
                    .ToArray()

                If idx.Length <> topK Then Throw New Exception("topk length mismatch!")
            End Sub))

        ' ---------- 3. KMeans ----------
        Dim kmeansRows As Integer = If(quick, 2000, 10000)
        Dim kmeansCols As Integer = If(quick, 50, 100)
        Dim k As Integer = 10
        Dim kdata As Double()() = Synthetic(kmeansRows, kmeansCols, k, 42)
        Dim kmeansIds As String() = Ids(kmeansRows)

        Call results.Add(Measure($"kmeans {kmeansRows}x{kmeansCols} k={k}",
            Sub()
                Dim engine As New KMeansEngine(max_iters:=50)

                randf.SetSeed(100)
                Call engine.ClusterDataSet(kdata, kmeansIds, k)
            End Sub))

        ' ---------- 4. Fuzzy C-Means ----------
        Dim fuzzyRows As Integer = If(quick, 1000, 5000)
        Dim fuzzyCols As Integer = If(quick, 20, 50)
        Dim fuzzyK As Integer = If(quick, 5, 8)
        Dim fdata As Double()() = Synthetic(fuzzyRows, fuzzyCols, fuzzyK, 43)

        Call results.Add(Measure($"fuzzy-cmeans {fuzzyRows}x{fuzzyCols} c={fuzzyK}",
            Sub()
                Dim engine As New CMeansEngine()

                randf.SetSeed(200)
                Call engine.Run(fdata, fuzzyK, threshold:=0.001, maxLoop:=30)
            End Sub))

        ' ---------- 5. DBSCAN ----------
        Dim dbRows As Integer = If(quick, 1000, 5000)
        Dim dbCols As Integer = If(quick, 10, 20)
        Dim dbData As Double()() = Synthetic(dbRows, dbCols, 4, 44)
        Dim dbPoints As NumericRow() = New NumericRow(dbRows - 1) {}
        Dim dbIds As String() = Ids(dbRows)

        For i As Integer = 0 To dbRows - 1
            dbPoints(i) = New NumericRow(dbIds(i), dbData(i), i)
        Next

        Call results.Add(Measure($"dbscan {dbRows}x{dbCols} eps=3 minPts=5",
            Sub()
                Dim algorithm As New DbscanAlgorithm(Of NumericRow)(
                    metricFunc:=Function(x As NumericRow, y As NumericRow) SimdDistance.Euclidean(x.data, y.data),
                    println:=AddressOf Silent
                )

                Call algorithm.ComputeClusterDBSCAN(dbPoints, 3.0, 5, filterNoise:=True)
            End Sub))

        ' ---------- 6. HDBSCAN ----------
        Dim hdbRows As Integer = If(quick, 500, 2000)
        Dim hdbCols As Integer = If(quick, 8, 10)
        Dim hdbData As Double()() = Synthetic(hdbRows, hdbCols, 4, 45)

        Call results.Add(Measure($"hdbscan {hdbRows}x{hdbCols} minPts=5 (no cache)",
            Sub()
                Dim parameters As New HdbscanParameters(Of Double()) With {
                    .DataSet = hdbData,
                    .DistanceFunction = New EuclideanDistance(),
                    .MinPoints = 5,
                    .MinClusterSize = 5,
                    .MaxDegreeOfParallelism = 0
                }

                Call HdbscanRunner.Run(parameters)
            End Sub))

        ' ---------- 7. 成对距离矩阵 ----------
        Dim pairRows As Integer = If(quick, 400, 2000)
        Dim pairData As Double()() = Synthetic(pairRows, If(quick, 8, 20), 4, 46)

        Call results.Add(Measure($"pairwise-distance n={pairRows} (upper triangle)",
            Sub()
                Dim matrix As Double() = SimdParallel.PairwiseDistance(pairData)

                If matrix.Length <> CInt(SimdParallel.UpperTriangleLength(pairRows)) Then
                    Throw New Exception("pairwise matrix length mismatch!")
                End If
            End Sub))

        ' ---------- 8. 谱聚类（可选） ----------
        If includeSpectral Then
            Dim spRows As Integer = 300
            Dim spData As Double()() = Synthetic(spRows, 10, 3, 47)
            Dim spMatrix As New Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.NumericMatrix(spData)

            Call results.Add(Measure($"spectral n={spRows} (O(n^3) eigendecomposition)",
                Sub()
                    Dim spectral As New Spectral(spMatrix)

                    spectral.set_centers(3)
                    spectral.set_gamma(0.1)
                    Call spectral.cluster()
                End Sub))
        End If

        Return results.ToArray()
    End Function
End Module
