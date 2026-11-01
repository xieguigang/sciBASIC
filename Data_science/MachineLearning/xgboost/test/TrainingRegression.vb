Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.DataMining.Evaluation
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train
Imports sysmath = System.Math

''' <summary>
''' 训练回归：覆盖收敛性、早停回滚、序列化往返、类别编码保真、可复现性、
''' 以及 exact / hist 双路径的一致性。
''' </summary>
Public Module TrainingRegression

    Public Sub Run()
        Console.WriteLine("[训练回归] 开始")

        Call convergence()
        Call earlyStopping()
        Call roundTrip()
        Call categoryFidelity()
        Call determinism()
        Call histogramConsistency()

        Console.WriteLine("[训练回归] 结束")
    End Sub

    ''' <summary>
    ''' 统一的训练入口
    ''' </summary>
    Private Function train(ds As SyntheticSet,
                           Optional rounds As Integer = 40,
                           Optional earlyStop As Integer = 200,
                           Optional method As TreeMethod = TreeMethod.Exact,
                           Optional seed As Integer = 20240501) As GBM
        Dim gbm As New GBM()

        Call gbm.fit(trainset:=ds.Train,
                     valset:=ds.Validation,
                     early_stopping_rounds:=earlyStop,
                     maximize:=True,
                     eval_metric:=Metrics.auc,
                     loss:="logloss",
                     eta:=0.3,
                     num_boost_round:=rounds,
                     max_depth:=4,
                     scale_pos_weight:=1.0,
                     rowsample:=0.8,
                     colsample:=0.8,
                     min_child_weight:=1.0,
                     min_sample_split:=5,
                     lambda:=1.0,
                     gamma:=0.0,
                     num_thread:=-1,
                     tree_method:=method,
                     max_bin:=256,
                     random_seed:=seed)

        Return gbm
    End Function

    ''' <summary>
    ''' 训练必须真正降低损失，并取得可接受的 AUC
    ''' </summary>
    Private Sub convergence()
        Dim ds = BenchmarkData.Create(2400, 8, {3}, 0.05, 20240501)
        Dim gbm = train(ds, 40, 200)
        Dim pred As Double() = gbm.predict(ds.Rows)

        Dim p0 As Double = 1.0 / (1.0 + sysmath.Exp(-gbm.first_round_pred))
        Dim baseLine = TestSupport.BasePrediction(p0, pred.Length)
        Dim baseLoss = TestSupport.LogLoss(baseLine, ds.Labels)
        Dim finalLoss = TestSupport.LogLoss(pred, ds.Labels)
        Dim aucMetric As IMetric = Metric.GetMetric(Metrics.auc)
        Dim auc As Double = aucMetric(pred, ds.Labels)

        Call DebugAudit.Mark("train:loss", $"base={baseLoss:F6}, final={finalLoss:F6}, auc={auc:F4}")
        Console.WriteLine($"  loss: base={baseLoss:F6} -> final={finalLoss:F6}, auc={auc:F4}, trees={gbm.trees.Count}")

        Call DebugAudit.Check(finalLoss < baseLoss,
            $"训练后损失未下降（base={baseLoss:F6}, final={finalLoss:F6}）")
        Call DebugAudit.Check(finalLoss < 0.7,
            $"训练 40 轮后损失仍偏高（{finalLoss:F6}）")
        Call DebugAudit.Check(auc > 0.8,
            $"AUC 偏低（{auc:F4}），模型未学到数据中的结构")

        ' 树必须真的被建立（防止 num_thread / colsample 等参数导致空树）
        Call DebugAudit.Check(gbm.trees.Count > 0, "没有训练出任何树")
        Call DebugAudit.Check(gbm.trees(0).root IsNot Nothing, "树根为空")
    End Sub

    ''' <summary>
    ''' 早停必须回滚到 best_iteration（旧实现只打日志、保留多余树）
    ''' </summary>
    Private Sub earlyStopping()
        Dim ds = BenchmarkData.Create(2000, 6, {2}, 0.05, 777)
        Dim gbm = train(ds, 80, 3)

        Call DebugAudit.Mark("early-stop", $"best={gbm.bestIteration}, trees={gbm.trees.Count}")
        Console.WriteLine($"  early stopping: best={gbm.bestIteration}, trees={gbm.trees.Count}")

        Call DebugAudit.Check(gbm.bestIteration >= 0, "未记录 best_iteration")
        Call DebugAudit.Check(gbm.trees.Count = gbm.bestIteration + 1,
            $"早停未回滚到最优轮次（trees={gbm.trees.Count}, best={gbm.bestIteration}）")
    End Sub

    ''' <summary>
    ''' 序列化往返：保存再加载后预测必须完全一致（旧实现用 F6 格式化 + 区域相关解析）
    ''' </summary>
    Private Sub roundTrip()
        Dim ds = BenchmarkData.Create(1200, 6, {3}, 0.05, 31415)
        Dim gbm = train(ds, 15, 200)
        Dim lines As String() = ModelSerializer.save_model(gbm).ToArray()
        Dim loaded As GBM = ModelSerializer.load_model(lines)
        Dim a As Double() = gbm.predict(ds.Rows)
        Dim b As Double() = loaded.predict(ds.Rows)
        Dim maxDiff As Double = 0

        For i As Integer = 0 To a.Length - 1
            Dim diff = sysmath.Abs(a(i) - b(i))

            If diff > maxDiff Then
                maxDiff = diff
            End If
        Next

        Call DebugAudit.Mark("serialize:roundtrip", $"maxDiff={maxDiff:G6}, lines={lines.Length}")
        Console.WriteLine($"  序列化往返: maxDiff={maxDiff:G6}")

        Call DebugAudit.Check(maxDiff < 1.0E-12,
            $"序列化往返后预测不一致（最大偏差 {maxDiff:G6}）")
        Call DebugAudit.Check(loaded.trees.Count = gbm.trees.Count, "往返后树数量不一致")
    End Sub

    ''' <summary>
    ''' 类别分裂的左子集必须保持原始类别取值（旧实现经 CStr-&gt;CDbl-&gt;CInt 截断）
    ''' </summary>
    Private Sub categoryFidelity()
        Dim ds = BenchmarkData.Create(2400, 6, {2}, 0.0, 2468)
        Dim gbm = train(ds, 25, 200)
        Dim values As New List(Of Double)()

        For Each tree As Tree In gbm.trees
            Call collectCategories(tree.root, values)
        Next

        Dim allowed As Double() = BenchmarkData.CategoryLevels
        Dim bad As New List(Of Double)()

        For Each v As Double In values
            Dim hit As Boolean = False

            For Each level As Double In allowed

                If sysmath.Abs(level - v) < 1.0E-6 Then
                    hit = True
                    Exit For
                End If
            Next

            If Not hit Then
                Call bad.Add(v)
            End If
        Next

        Call DebugAudit.Mark("category:levels", $"splits={values.Count}, invalid={bad.Count}")
        Console.WriteLine($"  类别分裂取值: {values.Count} 个，非法取值 {bad.Count} 个")

        Call DebugAudit.Check(gbm.trees.Count > 0, "没有训练出树")
        Call DebugAudit.Check(bad.Count = 0,
            $"类别左子集中出现被截断的取值：{String.Join(", ", bad.Take(5))}")
    End Sub

    Private Sub collectCategories(node As TreeNode, output As List(Of Double))
        If node Is Nothing Then
            Return
        End If

        If node.is_leaf Then
            Return
        End If

        If node.split_left_child_catvalue IsNot Nothing Then

            For Each v As Double In node.split_left_child_catvalue
                Call output.Add(v)
            Next
        End If

        Call collectCategories(node.left_child, output)
        Call collectCategories(node.right_child, output)
        Call collectCategories(node.nan_child, output)
    End Sub

    ''' <summary>
    ''' 相同种子必须得到相同模型（并行归并不改变结果）
    ''' </summary>
    Private Sub determinism()
        Dim ds = BenchmarkData.Create(1000, 6, {3}, 0.05, 555)
        Dim a = train(ds, 10, 200, TreeMethod.Exact, 99)
        Dim b = train(ds, 10, 200, TreeMethod.Exact, 99)
        Dim pa As Double() = a.predict(ds.Rows)
        Dim pb As Double() = b.predict(ds.Rows)
        Dim maxDiff As Double = 0

        For i As Integer = 0 To pa.Length - 1
            Dim diff = sysmath.Abs(pa(i) - pb(i))

            If diff > maxDiff Then
                maxDiff = diff
            End If
        Next

        Call DebugAudit.Mark("determinism", $"maxDiff={maxDiff:G6}")
        Console.WriteLine($"  可复现性: maxDiff={maxDiff:G6}")

        Call DebugAudit.Check(maxDiff < 1.0E-12,
            $"相同随机种子产生了不同的模型（最大偏差 {maxDiff:G6}）")
    End Sub

    ''' <summary>
    ''' histogram 路径：结果必须有限，且指标不显著劣于 exact
    ''' </summary>
    Private Sub histogramConsistency()
        Dim ds = BenchmarkData.Create(1500, 8, {3}, 0.05, 8642)
        Dim exact = train(ds, 20, 200, TreeMethod.Exact, 11)
        Dim hist = train(ds, 20, 200, TreeMethod.Hist, 11)
        Dim pe As Double() = exact.predict(ds.Rows)
        Dim ph As Double() = hist.predict(ds.Rows)
        Dim lossExact = TestSupport.LogLoss(pe, ds.Labels)
        Dim lossHist = TestSupport.LogLoss(ph, ds.Labels)
        Dim finite As Boolean = True

        For Each v As Double In ph

            If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then
                finite = False
            End If
        Next

        Call DebugAudit.Mark("hist-vs-exact", $"exact={lossExact:F6}, hist={lossHist:F6}")
        Console.WriteLine($"  hist vs exact: exact={lossExact:F6}, hist={lossHist:F6}")

        Call DebugAudit.Check(finite, "histogram 路径产生了 NaN/Infinity 预测值")
        Call DebugAudit.Check(lossHist < lossExact + 0.15,
            $"histogram 路径的损失明显劣于 exact（{lossHist:F6} vs {lossExact:F6}）")
    End Sub
End Module
