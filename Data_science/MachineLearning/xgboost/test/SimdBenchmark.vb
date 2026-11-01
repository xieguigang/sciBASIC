Imports System.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.DataMining.Evaluation
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train
Imports sysmath = System.Math

''' <summary>
''' SIMD AB 基准与线程扩展性基准。
'''
''' + AB：同一数据集/种子下 <see cref="SimdOptions.UseSimd"/> True/False 的训练耗时；
''' + 扩展性：<c>num_thread</c> 取 1/8/32/64 的耗时曲线。
''' 结果全部写入 <see cref="DebugAudit"/>，用于回答「128 线程为什么用不满」。
''' </summary>
Public Module SimdBenchmark

    Private Const ROWS As Integer = 8000
    Private Const FEATURES As Integer = 24
    Private Const ROUNDS As Integer = 12
    Private Const DEPTH As Integer = 5

    Public Sub Run()
        Console.WriteLine("[SIMD 基准] 开始")
        Console.WriteLine($"  数据集: {ROWS} 行 × {FEATURES} 特征，{ROUNDS} 轮，深度 {DEPTH}")
        Console.WriteLine($"  能力: {SimdOptions.Describe()}")

        Dim ds = BenchmarkData.Create(ROWS, FEATURES, {5, 9}, 0.03, 2026)

        Dim withSimd As Long = measure("simd-on", ds, True, -1)
        Dim without As Long = measure("simd-off", ds, False, -1)

        Dim ratio As Double = without / sysmath.Max(withSimd, 1)

        Call DebugAudit.Mark("simd:speedup", $"{ratio:F2}x")
        Console.WriteLine($"  SIMD 加速比: {ratio:F2}x（{without} ms → {withSimd} ms）")

        ' 扩展性：同一份数据、同一随机种子
        Dim timings As New List(Of String)()

        For Each threads As Integer In {1, 8, 32, 64}
            Dim ms As Long = measure($"threads={threads}", ds, True, threads)

            If threads > 1 Then
                Call timings.Add($"{threads}:{ms}ms/{ms / sysmath.Max(timings.Count, 1)}")
            End If
        Next

        ' 只做宽松断言：SIMD 不应明显更慢（单次运行的毫秒级抖动不应让构建失败）
        Call DebugAudit.Check(ratio >= 0.8,
            $"SIMD 路径反而慢于标量（{withSimd} ms vs {without} ms，比值 {ratio:F2}）")

        Console.WriteLine("[SIMD 基准] 结束")
    End Sub

    Private Function measure(label As String, ds As SyntheticSet, useSimd As Boolean, threads As Integer) As Long
        Dim previous As Boolean = SimdOptions.UseSimd

        Try
            SimdOptions.UseSimd = useSimd
            Call GC.Collect()
            Call GC.WaitForPendingFinalizers()

            Dim before As Long = GC.GetTotalMemory(True)
            Dim clock As Stopwatch = Stopwatch.StartNew()
            Dim gbm = train(ds, threads)

            clock.Stop()

            Dim after As Long = GC.GetTotalMemory(True)
            Dim usedMb As Double = (after - before) / 1024.0 / 1024.0

            Call DebugAudit.Mark($"simd-bench:{label}", $"{clock.ElapsedMilliseconds} ms, trees={gbm.trees.Count}, live={usedMb:F2} MB")
            Console.WriteLine($"  {label}: {clock.ElapsedMilliseconds} ms，{gbm.trees.Count} 棵树，托管内存增量 {usedMb:F2} MB")

            Return clock.ElapsedMilliseconds
        Finally
            SimdOptions.UseSimd = previous
        End Try
    End Function

    Private Function train(ds As SyntheticSet, threads As Integer) As GBM
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
                     min_child_weight:=1.0,
                     min_sample_split:=5,
                     lambda:=1.0,
                     gamma:=0.0,
                     num_thread:=threads,
                     tree_method:=TreeMethod.Hist,
                     max_bin:=256,
                     random_seed:=2026)

        Return gbm
    End Function
End Module
