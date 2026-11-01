Imports System.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.DataMining.Evaluation
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train
Imports sysmath = System.Math

''' <summary>
''' 性能基准：exact vs hist、串行 vs 并行、训练耗时与预测吞吐。
'''
''' 所有数据都由 <see cref="BenchmarkData"/> 以固定种子生成，结果可复现。
''' </summary>
Public Module Benchmark

    Private Const ROWS As Integer = 6000
    Private Const FEATURES As Integer = 20
    Private Const ROUNDS As Integer = 20
    Private Const DEPTH As Integer = 5

    Public Sub Run()
        Console.WriteLine("[性能基准] 开始")

        Dim ds = BenchmarkData.Create(ROWS, FEATURES, {5, 9}, 0.03, 2026)

        Console.WriteLine($"  数据集: {ROWS} 行 × {FEATURES} 特征，{ROUNDS} 轮，深度 {DEPTH}")

        Dim exact = measure(ds, TreeMethod.Exact, -1, "exact-parallel")
        Dim hist = measure(ds, TreeMethod.Hist, -1, "hist-parallel")
        Dim serial = measure(ds, TreeMethod.Exact, 1, "exact-serial")

        ' 预测吞吐
        Dim model = trained(ds, TreeMethod.Hist, -1)
        Dim clock As Stopwatch = Stopwatch.StartNew()
        Dim pred As Double() = model.predict(ds.Rows)
        clock.Stop()

        Dim rowsPerSecond As Double = ds.Rows.Length / sysmath.Max(clock.Elapsed.TotalSeconds, 0.000001)
        Dim finite As Boolean = True

        For Each v As Double In pred

            If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then
                finite = False
            End If
        Next

        Call DebugAudit.Mark("predict:throughput", $"{rowsPerSecond:F0} rows/s")
        Console.WriteLine($"  预测: {clock.ElapsedMilliseconds} ms，{rowsPerSecond:F0} rows/s（{model.trees.Count} 棵树）")

        Call DebugAudit.Check(finite, "基准预测结果中出现 NaN/Infinity")
        Call DebugAudit.Check(rowsPerSecond > 20000, $"预测吞吐偏低：{rowsPerSecond:F0} rows/s")

        ' 耗时对比只作为观测量记录：单次运行的 JIT/GC 抖动足以让毫秒级差异翻倍，
        ' 不适合作为会导致构建失败的断言。
        Call DebugAudit.Mark("bench:hist-vs-exact", $"exact={exact} ms, hist={hist} ms, 比值={hist / sysmath.Max(exact, 1):F2}")
        Console.WriteLine($"  hist/exact 耗时比: {hist / sysmath.Max(exact, 1):F2}")

        If serial > 0 Then
            Call DebugAudit.Mark("speedup:parallel", $"{serial / sysmath.Max(exact, 1):F2}x")
            Console.WriteLine($"  并行加速比（exact）: {serial / sysmath.Max(exact, 1):F2}x")
        End If

        Console.WriteLine("[性能基准] 结束")
    End Sub

    Private Function trained(ds As SyntheticSet, method As TreeMethod, threads As Integer) As GBM
        Dim gbm As New GBM()

        Call gbm.fit(trainset:=ds.Train,
                     valset:=Nothing,
                     early_stopping_rounds:=1000,
                     maximize:=True,
                     eval_metric:=Metrics.auc,
                     loss:="logloss",
                     eta:=0.3,
                     num_boost_round:=ROUNDS,
                     max_depth:=DEPTH,
                     rowsample:=0.8,
                     colsample:=0.8,
                     num_thread:=threads,
                     tree_method:=method,
                     max_bin:=256,
                     random_seed:=2026)

        Return gbm
    End Function

    ''' <summary>
    ''' 训练并记录耗时（毫秒）
    ''' </summary>
    Private Function measure(ds As SyntheticSet, method As TreeMethod, threads As Integer, label As String) As Long
        ' 预热一次 GC，避免把首次分配算进耗时
        Call GC.Collect()
        Call GC.WaitForPendingFinalizers()

        Dim before As Long = GC.GetTotalMemory(True)
        Dim clock As Stopwatch = Stopwatch.StartNew()
        Dim gbm = trained(ds, method, threads)

        clock.Stop()

        Dim after As Long = GC.GetTotalMemory(True)
        Dim usedMb As Double = (after - before) / 1024.0 / 1024.0

        Call DebugAudit.Mark($"bench:{label}", $"{clock.ElapsedMilliseconds} ms, trees={gbm.trees.Count}, live={usedMb:F2} MB")
        Console.WriteLine($"  {label}: {clock.ElapsedMilliseconds} ms，{gbm.trees.Count} 棵树，托管内存增量 {usedMb:F2} MB")

        Call DebugAudit.Check(gbm.trees.Count > 0, $"{label}: 未训练出任何树")

        Return clock.ElapsedMilliseconds
    End Function
End Module
