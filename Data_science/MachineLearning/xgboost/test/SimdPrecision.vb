Imports System.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.DataMining.Evaluation
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train
Imports Microsoft.VisualBasic.Math.SIMD
Imports sysmath = System.Math

''' <summary>
''' SIMD 精度与等价性用例。
'''
''' 覆盖三类风险：
''' 1. <c>SimdMath.ExpInPlace</c> 的多项式近似误差（训练对梯度精度敏感）；
''' 2. <c>SimdReduce</c> 多累加器求和的重结合误差；
''' 3. 端到端：<see cref="SimdOptions.UseSimd"/> 开/关两条路径训练出的模型必须等价。
''' </summary>
Public Module SimdPrecision

    Private Const ROWS As Integer = 1200
    Private Const SEED As Integer = 20250101

    Public Sub Run()
        Console.WriteLine($"[SIMD 精度] 开始（{SimdOptions.Describe()}）")

        Call expPrecision()
        Call sumReassociate()
        Call endToEnd()

        Console.WriteLine("[SIMD 精度] 结束")
    End Sub

    ''' <summary>
    ''' <c>ExpInPlace</c> 与 <c>Math.Exp</c> 的相对误差：输入覆盖 <c>-40..0</c>
    ''' （即 sigmoid 里的 <c>-|z|</c> 区间）
    ''' </summary>
    Private Sub expPrecision()
        Dim n As Integer = 100001
        Dim values As Double() = New Double(n - 1) {}
        Dim reference As Double() = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            Dim z As Double = -40.0R + 40.0R * i / (n - 1)

            values(i) = z
            reference(i) = sysmath.Exp(z)
        Next

        Call SimdMath.ExpInPlace(values, n)

        Dim maxRel As Double = 0
        Dim worstAt As Double = 0

        For i As Integer = 0 To n - 1
            Dim diff As Double = sysmath.Abs(values(i) - reference(i))
            Dim rel As Double = If(reference(i) > 0, diff / reference(i), diff)

            If rel > maxRel Then
                maxRel = rel
                worstAt = -40.0R + 40.0R * i / (n - 1)
            End If
        Next

        Call DebugAudit.Mark("simd:exp", $"maxRelErr={maxRel:G6} at z={worstAt:G4}")
        Console.WriteLine($"  exp 近似: 最大相对误差 {maxRel:G6}（z={worstAt:G4}）")

        ' 13 阶多项式 + 区间约减，在 double 上的误差应在 1e-13 以内
        Call DebugAudit.Check(maxRel < 1.0E-12,
            $"SimdMath.ExpInPlace 相对误差过大：{maxRel:G6}（z={worstAt:G4}）")
    End Sub

    ''' <summary>
    ''' 多累加器求和与逐次累加的差异（预期 ~1e-15，不应影响指标）
    ''' </summary>
    Private Sub sumReassociate()
        Dim n As Integer = 200001
        Dim values As Double() = New Double(n - 1) {}
        Dim rand As New Random(7)

        For i As Integer = 0 To n - 1
            values(i) = rand.NextDouble() * 2 - 1
        Next

        ' 参考：标量逐次累加
        Dim scalar As Double = 0

        For i As Integer = 0 To n - 1
            scalar += values(i)
        Next

        Dim simd As Double = SimdReduce.Sum(values)
        Dim relDiff As Double = sysmath.Abs(simd - scalar) / sysmath.Max(sysmath.Abs(scalar), 1.0E-30)

        Call DebugAudit.Mark("simd:sum", $"scalar={scalar:G17}, simd={simd:G17}, rel={relDiff:G3}")
        Console.WriteLine($"  求和重结合: rel={relDiff:G3}")

        Call DebugAudit.Check(relDiff < 1.0E-12,
            $"SimdReduce.Sum 与标量求和的相对偏差过大：{relDiff:G3}")
    End Sub

    ''' <summary>
    ''' 端到端：SIMD 开/关两次训练必须给出等价的模型
    ''' </summary>
    Private Sub endToEnd()
        Dim ds = BenchmarkData.Create(ROWS, 6, {3}, 0.05, SEED)
        Dim withSimd = trainOnce(ds, True)
        Dim without = trainOnce(ds, False)

        Dim predSimd As Double() = withSimd.predict(ds.TrainRows)
        Dim predPlain As Double() = without.predict(ds.TrainRows)

        Dim lossSimd As Double = TestSupport.LogLoss(predSimd, ds.TrainLabels)
        Dim lossPlain As Double = TestSupport.LogLoss(predPlain, ds.TrainLabels)
        Dim maxDiff As Double = 0

        For i As Integer = 0 To predSimd.Length - 1
            Dim diff As Double = sysmath.Abs(predSimd(i) - predPlain(i))

            If diff > maxDiff Then
                maxDiff = diff
            End If
        Next

        Dim relLoss As Double = sysmath.Abs(lossSimd - lossPlain) / sysmath.Max(lossPlain, 1.0E-30)
        Dim aucMetric As IMetric = Metric.GetMetric(Metrics.auc)
        Dim aucDiff As Double = sysmath.Abs(aucMetric(predSimd, ds.TrainLabels) - aucMetric(predPlain, ds.TrainLabels))

        Call DebugAudit.Mark("simd:end-to-end",
            $"loss(simd)={lossSimd:F9}, loss(scalar)={lossPlain:F9}, rel={relLoss:G3}, aucDiff={aucDiff:G3}, maxPredDiff={maxDiff:G6}")
        Console.WriteLine($"  端到端: loss {lossPlain:F9} → {lossSimd:F9}（rel={relLoss:G3}），AUC 偏差 {aucDiff:G3}")

        ' 注意：不能按「逐位一致」要求端到端结果。
        ' 求和重结合（~1e-14）足以让两个增益极接近的候选切分点交换胜负，
        ' 树结构一旦不同，损失差异会被放大到 1e-4 量级——这是离散分裂选择的固有放大，
        ' 不是数值错误。因此这里考核的是「指标等价」而非「结果相同」。
        Call DebugAudit.Check(sysmath.Abs(lossSimd - lossPlain) < 1.0E-3,
            $"SIMD 与标量的训练损失偏差过大：{lossPlain:F9} vs {lossSimd:F9}")
        Call DebugAudit.Check(aucDiff < 1.0E-4,
            $"SIMD 与标量的 AUC 偏差过大：{aucDiff:G3}")
    End Sub

    Private Function trainOnce(ds As SyntheticSet, useSimd As Boolean) As GBM
        Dim previous As Boolean = SimdOptions.UseSimd

        Try
            SimdOptions.UseSimd = useSimd

            Dim gbm As New GBM()

            Call gbm.fit(trainset:=ds.Train,
                         valset:=Nothing,
                         early_stopping_rounds:=1000,
                         maximize:=True,
                         eval_metric:=Metrics.auc,
                         loss:="logloss",
                         eta:=0.3,
                         num_boost_round:=10,
                         max_depth:=4,
                         rowsample:=0.8,
                         colsample:=0.8,
                         min_child_weight:=1.0,
                         min_sample_split:=5,
                         lambda:=1.0,
                         gamma:=0.0,
                         num_thread:=-1,
                         tree_method:=TreeMethod.Exact,
                         max_bin:=256,
                         random_seed:=SEED)

            Return gbm
        Finally
            SimdOptions.UseSimd = previous
        End Try
    End Function
End Module
