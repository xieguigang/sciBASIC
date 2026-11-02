Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.DeepLearning.GCN
Imports Microsoft.VisualBasic.DeepLearning.GNN
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
Imports std = System.Math

''' <summary>
''' GCN 的演示与回归用例。
''' </summary>
''' <remarks>
''' 用例分三类，缺一不可：
''' <list type="number">
'''   <item><b>公式</b>：readme 第四节那个 4 节点例子，逐值核对归一化系数与单层输出 ——
'''         它能锁死行/列主序的方向，是最便宜也最有价值的一条断言；</item>
'''   <item><b>学习能力</b>：Karate（34 节点、稠密路径）与合成 SBM（300 节点、越过 fp32 阈值），
'''         断言损失下降且准确率显著高于随机；</item>
'''   <item><b>等价性</b>：稠密 vs 稀疏、fp64 vs fp32 —— 前者验证两条消息传递路径语义一致，
'''         后者验证“降精度只带来噪声级的差异”，并用诊断计数证明加速路径**确实被走到**。</item>
''' </list>
''' </remarks>
Public Module GCNTestSuite

    ' =====================================================================
    ' 1. 公式校验
    ' =====================================================================

    ''' <summary>
    ''' readme 第四节：4 个节点、节点 1 连接 2/3/4，验证 Â 与单层输出。
    ''' </summary>
    Public Sub RunNormalizationExample()
        ' 0-based：节点 0 连接 1、2、3
        Dim graph As New Graph(4, 1)

        Call graph.AddUndirectedEdge(0, 1)
        Call graph.AddUndirectedEdge(0, 2)
        Call graph.AddUndirectedEdge(0, 3)

        Dim adjacency = NormalizedAdjacency.FromGraph(graph, PropagationMode.Dense, buildDense:=True)
        Dim dense = adjacency.Dense

        Call DebugAudit.AreEqual(4, adjacency.Nodes, "节点数")
        Call DebugAudit.AreEqual(3, adjacency.UndirectedEdges, "无向边数")
        Call DebugAudit.AreEqual(10, adjacency.NonZeros, "非零元数应为 3×2（双向边）+ 4（自环）= 10")

        ' 度：d̃_1 = 4（3 邻居 + 自环），其余为 2
        Call DebugAudit.Check(std.Abs(adjacency.Degree(0) - 4.0) < 1.0E-12,
                              $"d̃_1 应为 4，实际 {adjacency.Degree(0)}")

        For i = 1 To 3
            Call DebugAudit.Check(std.Abs(adjacency.Degree(i) - 2.0) < 1.0E-12,
                                  $"d̃_{i + 1} 应为 2，实际 {adjacency.Degree(i)}")
        Next

        ' 对角线 = 1/d̃（自环自身的权重是 1）
        Call DebugAudit.Check(std.Abs(dense(0, 0) - 0.25) < 1.0E-12, $"Â(1,1) 应为 0.25，实际 {dense(0, 0)}")
        Call DebugAudit.Check(std.Abs(dense(1, 1) - 0.5) < 1.0E-12, $"Â(2,2) 应为 0.5，实际 {dense(1, 1)}")

        ' 非对角 = 1/√(d̃_i·d̃_j)
        Dim expectedEdge = 1.0 / (2.0 * std.Sqrt(2.0))

        Call DebugAudit.Check(std.Abs(dense(0, 1) - expectedEdge) < 1.0E-12,
                              $"Â(1,2) 应为 1/(2√2)={expectedEdge:G6}，实际 {dense(0, 1)}")

        ' 节点 2 与 3 之间无边
        Call DebugAudit.Check(std.Abs(dense(1, 2)) < 1.0E-15,
                              $"Â(2,3) 应为 0（无边），实际 {dense(1, 2)}")

        ' 对称性：GCN 的全部推导都建立在 Â = Âᵀ 上
        Dim asymmetry = 0.0

        For i = 0 To 3
            For j = 0 To 3
                Dim delta = std.Abs(dense(i, j) - dense(j, i))

                If delta > asymmetry Then asymmetry = delta
            Next
        Next

        Call DebugAudit.Mark("normalize:symmetry", $"maxAsymmetry={asymmetry:G3}")
        Call DebugAudit.Check(asymmetry < 1.0E-15, $"Â 必须严格对称，最大偏差 {asymmetry:G6}")

        ' CSR 与稠密表示必须逐值一致（否则两条路径不可比）
        Dim fromCsr = adjacency.Sparse.ToDense()
        Dim csrDiff = MaxAbsDiff(dense, fromCsr)

        Call DebugAudit.Mark("normalize:csr-vs-dense", $"maxDiff={csrDiff:G3}")
        Call DebugAudit.Check(csrDiff < 1.0E-15, $"CSR 与稠密表示不一致，最大偏差 {csrDiff:G6}")

        ' 单层输出（readme 第四节的手算结果）：
        '   h_1^new = 1/4·h_1 + (h_2 + h_3 + h_4)/(2√2)，这里 h 全为 1、W = 1、无 bias
        Dim layer As New GraphConvLayer(1, 1, ActivationType.None, useBias:=False)
        layer.Weights(0, 0) = 1.0

        Dim inputT = New Tensor(1, 4)

        For j = 0 To 3
            inputT(0, j) = 1.0
        Next

        Dim outputT = layer.ForwardT(inputT, adjacency)
        Dim expected = 0.25 + 3.0 / (2.0 * std.Sqrt(2.0))

        Call DebugAudit.Mark("layer:forward", $"node1={outputT(0, 0):G6}, expected={expected:G6}")
        Call DebugAudit.Check(std.Abs(outputT(0, 0) - expected) < 1.0E-12,
                              $"节点 1 的一层输出应为 {expected:G6}，实际 {outputT(0, 0):G6}")

        ' 反向：W = 1 时 dX 应当等于 Â 的行和（与上面对称，所以同一组数）
        Dim gradT = New Tensor(1, 4)

        For j = 0 To 3
            gradT(0, j) = 1.0
        Next

        Dim inputGradT = layer.BackwardT(gradT)

        Call DebugAudit.Mark("layer:backward", $"node1={inputGradT(0, 0):G6}")
        Call DebugAudit.Check(std.Abs(inputGradT(0, 0) - expected) < 1.0E-12,
                              $"节点 1 的输入梯度应为 {expected:G6}，实际 {inputGradT(0, 0):G6}")
    End Sub

    ' =====================================================================
    ' 2. Karate Club
    ' =====================================================================

    ''' <summary>Karate Club 半监督节点分类。</summary>
    Public Sub RunKarateClub()
        Dim sample = KarateClub.Create(labelsPerClass:=2, validationPerClass:=3)

        Console.WriteLine($"  样本: {sample}")

        Dim homophily = KarateClub.Homophily(sample.Labels)

        Call DebugAudit.Mark("karate:homophily", $"edgeHomophily={homophily:P1}")
        Call DebugAudit.Check(homophily > 0.7,
                              $"Karate 的边同质性应显著高于随机（0.5），实际 {homophily:P1} —— " &
                              "低于这个值说明标签划分与拓扑不一致（标签没对，后面测什么都没意义）")

        Dim adjacency = NormalizedAdjacency.FromGraph(sample.Graph, PropagationMode.Dense)
        Dim network As New GCNNetwork(sample.NumFeatures, 16, sample.NumClasses,
                                      dropoutRate:=0.5, seed:=7)
        Dim optimizer As New GCNAdamW(network.GetParameters(), network.GetGradients(),
                                      learningRate:=0.01, weightDecay:=5.0E-4)
        Dim trainer As New SemiSupervisedTrainer(network, optimizer)
        Dim stopper As New EarlyStopping(patience:=60)

        Dim history = trainer.Train(sample.Features, adjacency, sample.Labels,
                                    sample.TrainMask, sample.ValidationMask,
                                    epochs:=200, earlyStopping:=stopper, printEvery:=0)

        Dim testEval = trainer.Evaluate(sample.Features, adjacency, sample.Labels, sample.TestMask)
        Dim trainEval = trainer.Evaluate(sample.Features, adjacency, sample.Labels, sample.TrainMask)

        Call DebugAudit.Mark("karate:history", history.ToString())
        Call DebugAudit.Mark("karate:test", testEval.ToString())
        Call DebugAudit.Mark("karate:train", trainEval.ToString())
        Call DebugAudit.Mark("karate:mode", $"propagation={adjacency.EffectiveMode}, density={adjacency.Density:P1}")

        Console.WriteLine($"  训练: {history}")
        Console.WriteLine($"  训练集: {trainEval}")
        Console.WriteLine($"  测试集: {testEval}")

        Call DebugAudit.Check(history.FinalLoss < history.InitialLoss * 0.5,
                              $"损失应显著下降：{history.InitialLoss:F4} → {history.FinalLoss:F4}")
        Call DebugAudit.Check(trainEval.Accuracy >= 0.9,
                              $"训练节点准确率应 ≥ 90%，实际 {trainEval.Accuracy:P2}")
        Call DebugAudit.Check(testEval.Accuracy >= 0.85,
                              $"测试节点准确率应 ≥ 85%，实际 {testEval.Accuracy:P2}")
    End Sub

    ' =====================================================================
    ' 3. 合成 SBM
    ' =====================================================================

    ''' <summary>合成同质图（300 节点 / 3 社区）半监督节点分类。</summary>
    Public Sub RunSyntheticSbm()
        Dim sample = SyntheticSbm.Create()

        Console.WriteLine($"  样本: {sample}")

        Dim adjacency = NormalizedAdjacency.[Get](sample.Graph, PropagationMode.Dense)
        Dim network As New GCNNetwork(sample.NumFeatures, 32, sample.NumClasses,
                                      dropoutRate:=0.3, seed:=13)
        Dim optimizer As New GCNAdamW(network.GetParameters(), network.GetGradients(),
                                      learningRate:=0.01, weightDecay:=5.0E-4)
        Dim trainer As New SemiSupervisedTrainer(network, optimizer)
        Dim stopper As New EarlyStopping(patience:=50)

        Dim history = trainer.Train(sample.Features, adjacency, sample.Labels,
                                    sample.TrainMask, sample.ValidationMask,
                                    epochs:=200, earlyStopping:=stopper, printEvery:=0)

        Dim testEval = trainer.Evaluate(sample.Features, adjacency, sample.Labels, sample.TestMask)
        Dim valEval = trainer.Evaluate(sample.Features, adjacency, sample.Labels, sample.ValidationMask)

        Call DebugAudit.Mark("sbm:history", history.ToString())
        Call DebugAudit.Mark("sbm:test", testEval.ToString())
        Call DebugAudit.Mark("sbm:val", valEval.ToString())

        Console.WriteLine($"  训练: {history}")
        Console.WriteLine($"  验证集: {valEval}")
        Console.WriteLine($"  测试集: {testEval}")

        Call DebugAudit.Check(history.FinalLoss < history.InitialLoss * 0.5,
                              $"损失应显著下降：{history.InitialLoss:F4} → {history.FinalLoss:F4}")
        Call DebugAudit.Check(testEval.Accuracy >= 0.85,
                              $"测试节点准确率应 ≥ 85%（随机基线约 33%），实际 {testEval.Accuracy:P2}")
        Call DebugAudit.Check(valEval.Accuracy >= 0.85,
                              $"验证节点准确率应 ≥ 85%，实际 {valEval.Accuracy:P2}")
    End Sub

    ' =====================================================================
    ' 4. 稠密 vs 稀疏
    ' =====================================================================

    ''' <summary>
    ''' 同一份权重、同一个 Â，稠密 GEMM 与 CSR SpMM 的前向结果必须一致。
    ''' </summary>
    ''' <remarks>
    ''' 比较时把混合精度关掉：这条用例检验的是**语义**（两条路径算的是不是同一个东西），
    ''' 精度差异留给下一条用例单独度量。混在一起会让失败原因无法定位。
    ''' </remarks>
    Public Sub RunDenseSparseParity()
        Dim sample = SyntheticSbm.Create()
        Dim denseAdjacency = NormalizedAdjacency.FromGraph(sample.Graph, PropagationMode.Dense, buildDense:=True)
        Dim sparseAdjacency = NormalizedAdjacency.FromGraph(sample.Graph, PropagationMode.Sparse)

        ' 先确认两种表示构造出的 Â 本身一致
        Dim matrixDiff = MaxAbsDiff(denseAdjacency.Dense, sparseAdjacency.Sparse.ToDense())

        Call DebugAudit.Mark("parity:matrix", $"maxDiff={matrixDiff:G3}, nnz={sparseAdjacency.NonZeros}")
        Call DebugAudit.Check(matrixDiff < 1.0E-15,
                              $"稠密与 CSR 的 Â 不一致，最大偏差 {matrixDiff:G6}")
        Call DebugAudit.Check(sparseAdjacency.EffectiveMode = PropagationMode.Sparse,
                              $"Auto 模式应选择 Sparse，实际 {sparseAdjacency.EffectiveMode}")
        Call DebugAudit.Check(denseAdjacency.EffectiveMode = PropagationMode.Dense,
                              $"显式 Dense 模式不应被改写，实际 {denseAdjacency.EffectiveMode}")

        Dim network As New GCNNetwork(sample.NumFeatures, 16, sample.NumClasses,
                                      dropoutRate:=0.0, seed:=11)

        network.SetTraining(False)

        Dim previous = MixedPrecisionPolicy.Enabled

        Try
            MixedPrecisionPolicy.Enabled = False

            Dim logitsDense = network.Forward(sample.Features, denseAdjacency)
            Dim logitsSparse = network.Forward(sample.Features, sparseAdjacency)

            Dim diff = MaxAbsDiff(logitsDense, logitsSparse)

            Call DebugAudit.Mark("parity:logits", $"maxDiff={diff:G3}")
            Call DebugAudit.Check(diff < 1.0E-9,
                                  $"稠密与稀疏前向结果必须一致（fp64），最大偏差 {diff:G6}")
        Finally
            MixedPrecisionPolicy.Enabled = previous
        End Try

        ' 再验证反向也一样（否则“前向对得上、梯度对不上”会被漏掉）
        Try
            MixedPrecisionPolicy.Enabled = False

            network.Forward(sample.Features, denseAdjacency)

            Dim grad = New Tensor(sample.NumNodes, sample.NumClasses)

            For i = 0 To grad.Length - 1
                grad(i) = 0.5
            Next

            Dim gradDense = network.Backward(grad)

            network.Forward(sample.Features, sparseAdjacency)

            Dim gradSparse = network.Backward(grad)
            Dim diff = MaxAbsDiff(gradDense, gradSparse)

            Call DebugAudit.Mark("parity:gradient", $"maxDiff={diff:G3}")
            Call DebugAudit.Check(diff < 1.0E-9,
                                  $"稠密与稀疏反向结果必须一致（fp64），最大偏差 {diff:G6}")
        Finally
            MixedPrecisionPolicy.Enabled = previous
        End Try
    End Sub

    ' =====================================================================
    ' 5. 混合精度
    ' =====================================================================

    ''' <summary>
    ''' fp32 与 fp64 的差异必须只停留在噪声级，并且要证明加速路径确实被走到。
    ''' </summary>
    Public Sub RunPrecisionRouting()
        ' ---- 5.1 单元级：规模达标 → 命中 fp32；数值越界 / 开关关闭 → 回退 ----
        ' 64³ = 262144，正好等于 GemmOpsThreshold，是“最小可加速规模”
        Dim a = Tensor.Random({64, 64}, seed:=1)
        Dim b = Tensor.Random({64, 64}, seed:=2)

        Call MixedPrecisionDiagnostics.Reset()

        Dim fast = GCNPrecision.TryMatMul(a, b)

        Call DebugAudit.Check(fast IsNot Nothing, "64×64×64 的 GEMM 应当命中 fp32 路径")

        If fast IsNot Nothing Then
            Dim reference = a.MatMul(b)
            Dim diff = MaxAbsDiff(reference, fast)
            Dim scale = std.Max(1.0, MaxAbs(reference))

            Call DebugAudit.Mark("precision:gemm", $"maxDiff={diff:G3}, scale={scale:G3}")
            Call DebugAudit.Check(diff / scale < 1.0E-5,
                                  $"fp32 与 fp64 的 GEMM 相对偏差应 < 1e-5，实际 {diff / scale:G3}")
        End If

        Call DebugAudit.Check(MixedPrecisionDiagnostics.[Of]("GCN.MatMul").Accelerated > 0,
                              "加速计数应当增加（否则说明判据根本没生效）")

        ' 超出单精度范围 → 必须回退，而不是悄悄变成 Infinity
        a(0, 0) = 1.0E39
        Call DebugAudit.Check(GCNPrecision.TryMatMul(a, b) Is Nothing,
                              "存在超出单精度范围的值时必须回退 fp64")

        a(0, 0) = 0.5

        ' 开关关闭 → 一律回退
        GCNPrecision.Enabled = False

        Try
            Call DebugAudit.Check(GCNPrecision.TryMatMul(a, b) Is Nothing,
                                  "GCNPrecision.Enabled = False 时必须回退 fp64")
        Finally
            GCNPrecision.Enabled = True
        End Try

        ' ---- 5.2 端到端：同一份权重的 logits 在 fp64 / fp32 下应当接近 ----
        Dim sample = SyntheticSbm.Create()
        Dim adjacency = NormalizedAdjacency.FromGraph(sample.Graph, PropagationMode.Dense, buildDense:=True)
        Dim network As New GCNNetwork(sample.NumFeatures, 32, sample.NumClasses,
                                      dropoutRate:=0.0, seed:=17)

        network.SetTraining(False)

        Dim previous = MixedPrecisionPolicy.Enabled

        Try
            Call MixedPrecisionDiagnostics.Reset()

            MixedPrecisionPolicy.Enabled = False
            Dim logits64 = network.Forward(sample.Features, adjacency)

            MixedPrecisionPolicy.Enabled = True
            Dim logits32 = network.Forward(sample.Features, adjacency)

            Dim diff = MaxAbsDiff(logits64, logits32)
            Dim scale = std.Max(1.0, MaxAbs(logits64))
            Dim accelerated = MixedPrecisionDiagnostics.[Of]("GCN.MatMul").Accelerated
            Dim fallback = MixedPrecisionDiagnostics.[Of]("GCN.MatMul").Fallback

            Call DebugAudit.Mark("precision:end-to-end",
                                 $"maxDiff={diff:G3}, scale={scale:G3}, accelerated={accelerated}, fallback={fallback}")
            Call DebugAudit.Check(accelerated > 0,
                                  "端到端前向应当至少命中一次 fp32 GEMM（Â·H 的规模已越过阈值）")
            Call DebugAudit.Check(diff / scale < 1.0E-3,
                                  $"fp32 与 fp64 的 logits 相对偏差应 < 1e-3，实际 {diff / scale:G3}")
        Finally
            MixedPrecisionPolicy.Enabled = previous
            network.SetTraining(False)
        End Try
    End Sub

    ' =====================================================================
    ' 6. 早停与权重回滚
    ' =====================================================================

    ''' <summary>
    ''' 早停判定与权重回滚：不涉及训练，纯粹验证“指标不再改善 → 停 → 参数回到最优那一轮”。
    ''' </summary>
    ''' <remarks>
    ''' 这条用例存在的理由：回滚如果写错（例如把快照存成张量引用而不是数值拷贝），
    ''' 表面上一切正常 —— 训练照跑、日志照打，只是“回滚”回滚到了一半的状态。
    ''' 这种错误只有在参数值上才看得出来。
    ''' </remarks>
    Public Sub RunEarlyStoppingRollback()
        Dim parameters As New List(Of Tensor) From {New Tensor(2, 2)}
        Dim stopper As New EarlyStopping(patience:=2, minDelta:=1.0E-3, mode:=EarlyStoppingMode.Minimize)

        parameters(0)(0, 0) = 1.0

        ' 第 1 轮：改善 → 记录最优并快照
        Call DebugAudit.Check(Not stopper.ShouldStop(1.0, 1, parameters), "第 1 轮不应触发早停")
        Call DebugAudit.Check(stopper.BestEpoch = 1, $"最优 epoch 应为 1，实际 {stopper.BestEpoch}")

        ' 训练把参数改坏
        parameters(0)(0, 0) = 5.0

        ' 第 2、3 轮：指标没有改善（1.0005 相对 1.0 的改善小于 minDelta）
        Call DebugAudit.Check(Not stopper.ShouldStop(1.0005, 2, parameters), "第 2 轮应继续等待")
        Call DebugAudit.Check(stopper.ShouldStop(1.0005, 3, parameters), "连续 2 轮无改善后应触发早停")
        Call DebugAudit.Check(stopper.Stopped, "Stopped 标志应被置位")
        Call DebugAudit.Check(stopper.BestEpoch = 1, $"最优 epoch 仍应为 1，实际 {stopper.BestEpoch}")

        ' 回滚：参数必须回到第 1 轮的 1.0
        Call DebugAudit.Check(stopper.RestoreBest(parameters), "RestoreBest 应返回 True")

        Dim restored = parameters(0)(0, 0)

        Call DebugAudit.Mark("early-stop:restore", $"restored={restored:G6}, best={stopper.BestValue:G6}")
        Call DebugAudit.Check(std.Abs(restored - 1.0) < 1.0E-15,
                              $"参数应回滚到 1.0，实际 {restored:G6}（回滚失效通常意味着快照存的是引用而不是数值）")

        ' 发散（NaN）应当直接判停，但不能被记成最优
        Dim divergent As New EarlyStopping(patience:=5)

        Call DebugAudit.Check(divergent.ShouldStop(Double.NaN), "NaN 指标应直接判停")
        Call DebugAudit.Check(divergent.BestEpoch = 0, "发散值不应被记成最优")
    End Sub

    ' =====================================================================
    ' 工具
    ' =====================================================================

    ''' <summary>两个同形张量的最大绝对偏差</summary>
    Private Function MaxAbsDiff(a As Tensor, b As Tensor) As Double
        If a.Length <> b.Length Then Throw New ArgumentException("张量长度不一致")

        Dim left = a.Data
        Dim right = b.Data
        Dim maxDiff = 0.0

        For i = 0 To left.Length - 1
            Dim delta = std.Abs(left(i) - right(i))

            If delta > maxDiff Then maxDiff = delta
        Next

        Return maxDiff
    End Function

    ''' <summary>张量的最大绝对值</summary>
    Private Function MaxAbs(a As Tensor) As Double
        Dim data = a.Data
        Dim maxValue = 0.0

        For i = 0 To data.Length - 1
            Dim value = std.Abs(data(i))

            If value > maxValue Then maxValue = value
        Next

        Return maxValue
    End Function

End Module
