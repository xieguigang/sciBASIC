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

        Call monotoneConvergence()
        Call leafOptimality()
        Call routingConsistency()
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
                           Optional seed As Integer = 20240501,
                           Optional rowsample As Double = 0.8,
                           Optional colsample As Double = 0.8) As GBM
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
                     rowsample:=rowsample,
                     colsample:=colsample,
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
    ''' 训练损失必须随轮次单调下降。
    '''
    ''' 这是最关键的一条回归护栏：本轮重构过程中出现过的真实缺陷
    ''' （类别分裂被物化成 <c>&lt;= 0</c> 的数值分裂）会导致「划分与梯度不一致」，
    ''' 表现为树结构每轮相同、而损失随轮次单调**上升**。
    ''' </summary>
    Private Sub monotoneConvergence()
        Dim ds = BenchmarkData.Create(2000, 6, {3}, 0.05, 20240501)
        Dim previous As Double = Double.MaxValue
        Dim trace As New List(Of String)()

        For Each rounds As Integer In {1, 2, 3, 5, 8}
            Dim gbm = train(ds, rounds, 1000)
            Dim loss As Double = TestSupport.LogLoss(gbm.predict(ds.TrainRows), ds.TrainLabels)

            Call trace.Add($"{rounds}={loss:F6}")
            Call DebugAudit.Check(loss < previous,
                $"第 {rounds} 轮后训练损失未下降（{loss:F6} >= {previous:F6}）")

            previous = loss
        Next

        Call DebugAudit.Mark("train:monotone", String.Join(", ", trace))
        Console.WriteLine($"  损失单调性: {String.Join(", ", trace)}")
    End Sub

    ''' <summary>
    ''' 训练必须真正降低损失，并取得可接受的 AUC
    ''' </summary>
    Private Sub convergence()
        Dim ds = BenchmarkData.Create(2400, 8, {3}, 0.05, 20240501)
        Dim gbm = train(ds, 40, 200)

        ' 训练集损失必须下降（验证集损失可能因过拟合而上升，不在此判定）
        Dim trainPred As Double() = gbm.predict(ds.TrainRows)
        Dim validPred As Double() = gbm.predict(ds.ValidationRows)

        Dim p0 As Double = 1.0 / (1.0 + sysmath.Exp(-gbm.first_round_pred))
        Dim baseLine = TestSupport.BasePrediction(p0, trainPred.Length)
        Dim baseLoss = TestSupport.LogLoss(baseLine, ds.TrainLabels)
        Dim finalLoss = TestSupport.LogLoss(trainPred, ds.TrainLabels)
        Dim aucMetric As IMetric = Metric.GetMetric(Metrics.auc)
        Dim auc As Double = aucMetric(validPred, ds.ValidationLabels)

        Call DebugAudit.Mark("train:loss", $"base={baseLoss:F6}, final={finalLoss:F6}, val-auc={auc:F4}")
        Console.WriteLine($"  训练损失: base={baseLoss:F6} -> final={finalLoss:F6}，验证 AUC={auc:F4}，trees={gbm.trees.Count}")

        Call DebugAudit.Check(finalLoss < baseLoss,
            $"训练后损失未下降（base={baseLoss:F6}, final={finalLoss:F6}）")
        Call DebugAudit.Check(finalLoss < 0.5,
            $"训练 40 轮后训练损失仍偏高（{finalLoss:F6}）")
        Call DebugAudit.Check(auc > 0.75,
            $"验证集 AUC 偏低（{auc:F4}），模型未学到数据中的结构")

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
        ' 标签完全由类别列决定的合成数据：必须真的在类别列上发生分裂
        Dim ds = BenchmarkData.CreateCategoricalOnly(1200, 2468)
        Dim gbm = train(ds, 20, 200)
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
        Call DebugAudit.Check(values.Count > 0,
            "标签完全由类别列决定，但树里没有出现任何类别分裂")
        Call DebugAudit.Check(bad.Count = 0,
            $"类别左子集中出现被截断的取值：{String.Join(", ", bad.Take(5))}")
    End Sub

    ''' <summary>
    ''' 叶值最优性：每片叶子的 <c>leaf = -G/(H+λ)</c>，其中 G/H 由落在该叶子里的样本
    ''' 在当前轮次（上一轮的预测）上的梯度/海森累加得到。
    '''
    ''' 该检查同时验证：树的划分与梯度是否一致、叶值是否按正确的一阶/二阶导计算。
    ''' </summary>
    Private Sub leafOptimality()
        Dim ds = BenchmarkData.Create(1200, 6, {3}, 0.05, 424242)
        Dim first = train(ds, 1, 1000, TreeMethod.Exact, 7, rowsample:=1.0, colsample:=1.0)
        Dim second = train(ds, 2, 1000, TreeMethod.Exact, 7, rowsample:=1.0, colsample:=1.0)

        ' 第 1 棵树：所有样本的预测都等于常数项
        Dim p0 As Double = 1.0 / (1.0 + sysmath.Exp(-first.first_round_pred))
        Dim uniform As Double() = TestSupport.BasePrediction(p0, ds.TrainRows.Length)
        Dim firstTree = checkLeaf(first.trees(0), ds, uniform, "leaf:tree0")

        ' 第 2 棵树：样本预测来自第 1 棵树的输出
        Dim p1 As Double() = first.predict(ds.TrainRows)
        Dim secondTree = checkLeaf(second.trees(1), ds, p1, "leaf:tree1")
    End Sub

    Private Function checkLeaf(tree As Tree, ds As SyntheticSet, pred As Double(), label As String) As Integer
        Dim stat As New Dictionary(Of TreeNode, (g As Double, h As Double, n As Integer))()
        Const LAMBDA As Double = 1.0

        For i As Integer = 0 To ds.TrainRows.Length - 1
            Dim node As TreeNode = walkToNode(tree.root, ds.TrainRows(i))
            Dim p As Double = pred(i)
            Dim g As Double = p - ds.TrainLabels(i)
            Dim h As Double = p * (1 - p)
            Dim cur As (g As Double, h As Double, n As Integer) = Nothing

            If stat.TryGetValue(node, cur) Then
                stat(node) = (cur.g + g, cur.h + h, cur.n + 1)
            Else
                stat(node) = (g, h, 1)
            End If
        Next

        Dim maxDiff As Double = 0
        Dim outliers As Integer = 0
        Dim detail As Integer = 0

        For Each item As KeyValuePair(Of TreeNode, (g As Double, h As Double, n As Integer)) In stat
            Dim expected As Double = -item.Value.g / (item.Value.h + LAMBDA)
            Dim diff As Double = sysmath.Abs(expected - item.Key.leafValue)

            If diff > maxDiff Then
                maxDiff = diff
            End If

            If diff > 1.0E-6 Then
                outliers += 1

                ' 最多打印 3 条明细，便于定位划分与梯度不一致的叶子
                If detail < 3 Then
                    detail += 1
                    Console.WriteLine($"    [不匹配] n={item.Value.n}, G={item.Value.g:F4}, H={item.Value.h:F4}, 期望={expected:F6}, 实际={item.Key.leafValue:F6}")
                End If
            End If
        Next

        Call DebugAudit.Mark(label, $"leaves={stat.Count}, outliers={outliers}, maxDiff={maxDiff:G6}")
        Console.WriteLine($"  叶值最优性[{label}]: {stat.Count} 片叶子，不符 {outliers}，最大偏差 {maxDiff:G6}")

        Call DebugAudit.Check(outliers = 0,
            $"{label}: 叶值不是 -G/(H+λ)，{outliers}/{stat.Count} 片叶子不符（最大偏差 {maxDiff:G6}）")

        Return outliers
    End Function

    ''' <summary>
    ''' 扁平树（预测路径）与节点对象树（参考实现）必须给出相同的叶值
    ''' </summary>
    Private Sub routingConsistency()
        Dim ds = BenchmarkData.Create(1200, 6, {3}, 0.05, 13579)
        Dim gbm = train(ds, 5, 1000)
        Dim mismatches As Integer = 0
        Dim maxDiff As Double = 0

        For Each tree As Tree In gbm.trees
            Dim flat = tree.Flat

            For i As Integer = 0 To ds.TrainRows.Length - 1
                Dim expected As Double = walkToLeaf(tree.root, ds.TrainRows(i))
                Dim actual As Double = flat.Predict(ds.TrainRows(i))
                Dim diff As Double = sysmath.Abs(expected - actual)

                If diff > maxDiff Then
                    maxDiff = diff
                End If

                If diff > 1.0E-12 Then
                    mismatches += 1
                End If
            Next
        Next

        Call DebugAudit.Mark("routing:flat-vs-tree", $"mismatches={mismatches}, maxDiff={maxDiff:G6}")
        Console.WriteLine($"  扁平树 vs 引用实现: 不一致 {mismatches} 处，最大偏差 {maxDiff:G6}")

        Call DebugAudit.Check(mismatches = 0,
            $"扁平树的叶路由与 TreeNode 参考实现不一致（{mismatches} 处，最大偏差 {maxDiff:G6}）")
    End Sub

    ''' <summary>
    ''' 参照实现：直接按 <see cref="TreeNode"/> 的公共字段走一遍
    ''' </summary>
    Private Function walkToLeaf(root As TreeNode, row As Single()) As Double
        Return walkToNode(root, row).leafValue
    End Function

    ''' <summary>
    ''' 参照实现：直接按 <see cref="TreeNode"/> 的公共字段走到叶节点并返回该节点
    ''' </summary>
    Private Function walkToNode(root As TreeNode, row As Single()) As TreeNode
        Dim node As TreeNode = root

        While Not node.is_leaf
            Dim v As Single = row(node.split_feature)
            Dim child As TreeNode

            If Single.IsNaN(v) OrElse v = TrainData.NA Then
                Select Case CInt(node.nan_go_to)
                    Case 0
                        child = node.nan_child
                    Case 1
                        child = node.left_child
                    Case 2
                        child = node.right_child
                    Case Else
                        child = If(node.left_child.num_sample > node.right_child.num_sample,
                                   node.left_child, node.right_child)
                End Select
            ElseIf node.split_left_child_catvalue IsNot Nothing Then
                child = If(node.split_left_child_catvalue.Contains(CDbl(v)), node.left_child, node.right_child)
            Else
                child = If(CDbl(v) <= node.split_threshold, node.left_child, node.right_child)
            End If

            If child Is Nothing Then
                Return node
            End If

            node = child
        End While

        Return node
    End Function

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
