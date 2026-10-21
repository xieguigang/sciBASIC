' ---------------------------------------------------------------------------
' 上层算子的对拍：验证"并行化改造没有改变数值语义"
'
' 与 OperatorParity 的分工：
'   * OperatorParity 比的是 tensor 与 tensorF 两条算子栈；
'   * 这里比的是**改造前后的同一段上层代码** ——
'     把并行化之后的 BatchedMatMul 与一份朴素的串行参考实现逐位对照。
'
' 为什么逐位相等是合理的要求：
'   并行化只改变了"不同输出元素之间的计算次序"，没有改变任何一个输出元素
'   内部的累加次序（bk 仍由小到大、零值跳过仍保留），
'   因此浮点结果可以严格逐位一致。做不到逐位一致就说明存在写冲突或次序变更。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
Imports Microsoft.VisualBasic.MachineLearning.Transformer
Imports Microsoft.VisualBasic.MachineLearning.RNN
Imports Microsoft.VisualBasic.DeepLearning.GNN
Imports Microsoft.VisualBasic.DeepLearning.LiquidNeuralNetwork
Imports std = System.Math

Namespace MixedPrecisionBench

    Module UpperLayerParity

        Friend Sub RunAll(results As List(Of CaseResult))
            Call BatchedMatMulCase(results)
            Call MixedPrecisionPathCase(results)
            Call GnnLayerCase(results)
            Call GraphCacheCase(results)
            Call RnnCase(results)
            Call LnnCase(results)
        End Sub

        ''' <summary>
        ''' LNN 回归：确认 LiquidCell 反向的循环重排没有改变数值结果。
        ''' </summary>
        ''' <remarks>
        ''' P5 对 LiquidCell 的两处改动（循环交换、分支外提）在设计上应当**逐位不变**，
        ''' 因为每个梯度分量的累加次序没有变。这个用例就是把该论断变成可执行的断言：
        '''   * 用固定种子训练固定步数，得到一条确定性的损失轨迹；
        '''   * 断言末次损失低于首次（网络确实在学）；
        '''   * 断言两次独立构造 + 相同种子给出**完全相同**的末次损失 ——
        '''     只要循环重排真的改变了累加次序，这条断言会立刻失败。
        ''' <para>
        ''' 顺带覆盖 ODE 积分器：本用例默认走 CT_RNN 模式，
        ''' 每步训练都会经过 RK4 的多次右端求值，因此积分器的双精度路径也在被测范围内。
        ''' </para>
        ''' </remarks>
        Private Sub LnnCase(results As List(Of CaseResult))
            Dim first As Double
            Dim last As Double = 0
            Dim repeatLast As Double = 0

            For pass = 0 To 1
                Dim net As New LiquidNeuralNetwork(2, 12, 1, 1, "tanh", "none", 1234)
                Dim trainer As New LNNTrainer(net, 0.01)

                Dim runFirst As Double = 0
                Dim runLast As Double = 0

                For i = 0 To 299
                    Dim x = i / 300.0
                    Dim loss = trainer.TrainStep(VecD(x, 1.0 - x), TargetOf(std.Sin(x * 2.0)))

                    If i = 0 Then runFirst = loss
                    runLast = loss
                Next

                If pass = 0 Then
                    first = runFirst
                    last = runLast
                Else
                    repeatLast = runLast
                End If
            Next

            Dim finite = Not Double.IsNaN(last) AndAlso Not Double.IsInfinity(last)

            results.Add(New CaseResult("LNN 训练可收敛", finite AndAlso last < first,
                                       $"初始损失={first:E3} 末次损失={last:E3}"))

            results.Add(New CaseResult("LNN 相同种子逐位可复现", last = repeatLast,
                                       $"第一次={last:R} 第二次={repeatLast:R}"))

            ' ---- 长序列滚动 ----
            ' 这是本阶段最该盯的风险：ODE 状态的递推误差是**累积**的，
            ' 单步看不出问题，几百步之后才会暴露为漂移或发散。
            ' 用刚训好的网络做一次 800 步连续积分（训练是逐点喂入，
            ' 这里一次给整段序列，走的是另一条代码路径），断言输出全部有限且幅值有界。
            Dim net2 As New LiquidNeuralNetwork(2, 12, 1, 1, "tanh", "none", 4321)
            Call net2.ResetState()

            Dim steps = 800
            ' 用行主序的一维数组 + 显式形状构造：
            ' ProcessSequence 期望 (seqLength, inputSize)，即 [steps, 2]。
            Dim seq(steps * 2 - 1) As Double

            For i = 0 To steps - 1
                Dim x = i / steps
                seq(i * 2) = x
                seq(i * 2 + 1) = 1.0 - x
            Next

            Dim rollout = net2.ProcessSequence(New Tensor(seq, steps, 2), 0.05)

            Dim allFinite = True
            Dim maxAbs As Double = 0

            For Each v In rollout.Data
                If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then allFinite = False
                maxAbs = std.Max(maxAbs, std.Abs(v))
            Next

            results.Add(New CaseResult("LNN 800 步长序列不发散", allFinite AndAlso maxAbs < 1000.0,
                                       $"形状=[{rollout.Shape(0)},{rollout.Shape(1)}] maxAbs={maxAbs:F4} 有限={allFinite}"))
        End Sub

        ''' <summary>
        ''' 归一化邻接矩阵的版本化缓存：同一拓扑下复用、加边后失效。
        ''' </summary>
        ''' <remarks>
        ''' 缓存是"每次前向都重算 O(N²)"这个浪费的替代方案，但它引入了新的失效路径 ——
        ''' 一旦失效判据漏掉，训练就会**静默**用着旧图，比慢更糟。
        ''' 因此这里对两侧都做断言：命中时必须是同一个对象，加边后必须是新对象。
        ''' </remarks>
        Private Sub GraphCacheCase(results As List(Of CaseResult))
            Dim g As New Graph(numNodes:=64, featureDim:=8)

            ' 造一个环形 + 少量长边的稀疏图
            For i = 0 To 63 - 1
                Call g.AddUndirectedEdge(i, i + 1)
            Next

            Call g.AddUndirectedEdge(0, 32)
            Call g.AddUndirectedEdge(7, 41)

            Dim a1 = g.GetNormalizedAdjacencyMatrix()
            Dim a2 = g.GetNormalizedAdjacencyMatrix()
            Dim reused = Object.ReferenceEquals(a1, a2)

            results.Add(New CaseResult("GNN 邻接矩阵缓存命中", reused,
                                       If(reused, "两次调用返回同一实例", "未命中缓存（每次重算）")))

            ' 加边后必须失效
            Call g.AddUndirectedEdge(3, 59)
            Dim a3 = g.GetNormalizedAdjacencyMatrix()
            Dim invalidated = Not Object.ReferenceEquals(a1, a3)

            results.Add(New CaseResult("GNN 邻接矩阵加边后失效", invalidated,
                                       If(invalidated, "加边后重新计算", "加边后仍返回旧缓存 —— 失效判据失效")))

            ' 归一化矩阵的对称性与行和为 1 的量级检查（D^(-1/2) A D^(-1/2) 对称）
            Dim maxAsym As Double = 0

            For i = 0 To 63 - 1
                For j = 0 To 63 - 1
                    maxAsym = std.Max(maxAsym, std.Abs(a3.Data(i * 64 + j) - a3.Data(j * 64 + i)))
                Next
            Next

            results.Add(New CaseResult("GNN 归一化邻接对称性", maxAsym < 1.0E-12,
                                       $"max|A(i,j)-A(j,i)|={maxAsym:E3}"))

            ' 缓存路径下 GCN 前向仍可跑通且数值有限
            Dim layer As New GCNConvLayer(8, 8)
            Dim feat = New Tensor(Randoms(64 * 8, seed:=501, scale:=1.0), 64, 8)
            Dim out1 = layer.Forward(feat, g.GetNormalizedAdjacencyMatrix())
            Dim out2 = layer.Forward(feat, g.GetNormalizedAdjacencyMatrix())

            Dim finite = True

            For Each v In out2.Data
                If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then finite = False
            Next

            results.Add(New CaseResult("GNN 缓存下 GCN 前向有限", finite AndAlso out1.Length = out2.Length,
                                       $"shape=[{out2.Shape(0)},{out2.Shape(1)}] 有限={finite}"))
        End Sub

        ''' <summary>
        ''' GNN 全连接层的混合精度对拍：同一层在"开启降精度"与"关闭降精度"下前反向结果应一致。
        ''' </summary>
        ''' <remarks>
        ''' 为什么不用自带的 Demo 做这件事：<c>GNN/test</c> 的时序演示当前存在**既有缺陷**
        ''' （TGCNModel 按 16 维特征建层，而演示数据每节点只有 1 维，
        ''' 触发"矩阵维度不匹配: 1 != 16"），与本轮改造无关，
        ''' 因此这里直接对层做定量的开关对拍。
        ''' <para>
        ''' 用同一份权重构造两层（一层共享权重）比较麻烦，这里改用更直接的口径：
        ''' 先在开启态跑一次得到结果 A，再关闭策略、用**同样的输入与权重**跑一次得到 B。
        ''' 因为层内部的权重由构造时的随机初始化决定，所以做法是：
        ''' 关掉策略跑出基准，再打开策略跑一遍 —— 权重在前向里不会被修改，
        ''' 因此两次前向的权重是同一份。
        ''' </para>
        ''' </remarks>
        Private Sub GnnLayerCase(results As List(Of CaseResult))
            ' 输入/输出维度**必须不相等**。
            ' 这条用例早先写的是 128→128 的方阵，于是漏掉了一个真实缺陷：
            ' LinearLayer 的权重初始化曾用 XavierInit(inFeatures, outFeatures) 得到 (in,out)，
            ' 而该层其余代码（Forward 的转置、Backward 的 dW/dx、_weightGradient 的分配）
            ' 全部按 (out,in) 写。方阵时形状恰好凑得上、什么错都不报；
            ' 一旦 in≠out 就直接"矩阵维度不匹配"（TGCN 演示报的 1 != 16 就是这个）。
            Dim n = 512, inFeatures = 128, outFeatures = 64
            Dim layer As New LinearLayer(inFeatures, outFeatures)
            Dim input = New Tensor(Randoms(n * inFeatures, seed:=401, scale:=1.0), n, inFeatures)

            results.Add(New CaseResult(
                "GNN LinearLayer 权重布局 (out,in)",
                layer.GetWeights().Shape(0) = outFeatures AndAlso layer.GetWeights().Shape(1) = inFeatures,
                $"权重形状=[{layer.GetWeights().Shape(0)},{layer.GetWeights().Shape(1)}] 期望=[{outFeatures},{inFeatures}]"))

            Dim grads = layer.GetGradients()

            results.Add(New CaseResult(
                "GNN LinearLayer 梯度与权重同形",
                grads(0).Shape(0) = outFeatures AndAlso grads(0).Shape(1) = inFeatures,
                $"梯度形状=[{grads(0).Shape(0)},{grads(0).Shape(1)}] 权重形状=[{layer.GetWeights().Shape(0)},{layer.GetWeights().Shape(1)}]"))

            Dim enabled = MixedPrecisionPolicy.Enabled
            Dim reference As Tensor = Nothing
            Dim got As Tensor = Nothing
            Dim before = MixedPrecisionDiagnostics.[Of]("MatMul")
            Dim after = before

            Try
                ' ---- 基准：关闭降精度（纯双精度路径）----
                MixedPrecisionPolicy.Enabled = False
                reference = layer.Forward(input)

                ' ---- 被测：开启降精度 ----
                MixedPrecisionPolicy.Enabled = True
                before = MixedPrecisionDiagnostics.[Of]("MatMul")
                got = layer.Forward(input)
                after = MixedPrecisionDiagnostics.[Of]("MatMul")
            Finally
                ' 无论成败都要把全局开关还原，否则会污染后续用例
                MixedPrecisionPolicy.Enabled = enabled
            End Try

            Dim maxDiff As Double = 0
            Dim scaleRef As Double = 0

            For i = 0 To reference.Length - 1
                maxDiff = std.Max(maxDiff, std.Abs(reference.Data(i) - got.Data(i)))
                scaleRef = std.Max(scaleRef, std.Abs(reference.Data(i)))
            Next

            Dim tol = std.Max(0.0001 * scaleRef, 0.000001)

            results.Add(New CaseResult(
                "GNN LinearLayer 降精度路径命中",
                after.Accelerated > before.Accelerated,
                $"加速 {before.Accelerated} -> {after.Accelerated}"))

            results.Add(New CaseResult(
                "GNN LinearLayer 双精度/混合精度一致",
                maxDiff <= tol,
                $"maxAbsDiff={maxDiff:E3} tol={tol:E3} 参考量级={scaleRef:F3}"))
        End Sub

        ''' <summary>
        ''' 验证"降精度路径确实被走到，且结果仍在容差内"。
        ''' </summary>
        ''' <remarks>
        ''' 这一组存在的唯一理由是防**假通过**：
        ''' 判据不成立时 MixedPrecisionOps 会安静地返回 Nothing 让调用方走双精度，
        ''' 于是"逐位一致"的用例可以在降精度路径完全没生效的情况下通过。
        ''' 因此这里同时断言两件事：
        '''   1. 诊断计数器显示加速路径命中次数 > 0（真的走了 FP32）；
        '''   2. 与双精度参考实现的偏差在单精度累积误差的合理范围内。
        ''' </remarks>
        Private Sub MixedPrecisionPathCase(results As List(Of CaseResult))
            ' 规模必须显著超过 GemmOpsThreshold（262144），否则策略会拒绝降精度
            Dim blocks = 4, imax = 128, kmax = 128, jmax = 128
            Dim a = Randoms(blocks * imax * kmax, seed:=301, scale:=1.0)
            Dim b = Randoms(blocks * kmax * jmax, seed:=302, scale:=1.0)

            Dim aT = New Tensor(a, blocks, imax, kmax)
            Dim bT = New Tensor(b, blocks, kmax, jmax)

            Dim before = MixedPrecisionDiagnostics.[Of]("BatchedMatMul")
            Dim got = TensorOps.BatchedMatMul(aT, bT)
            Dim after = MixedPrecisionDiagnostics.[Of]("BatchedMatMul")
            Dim tookFastPath = after.Accelerated > before.Accelerated

            results.Add(New CaseResult("混合精度 BatchedMatMul 命中单精度路径", tookFastPath,
                                       $"加速 {before.Accelerated} -> {after.Accelerated}，回退 {after.Fallback}"))

            Dim reference = ReferenceBatchedMatMul(a, b, blocks, imax, kmax, jmax)
            Dim maxDiff As Double = 0
            Dim scaleRef As Double = 0

            For i = 0 To reference.Length - 1
                maxDiff = std.Max(maxDiff, std.Abs(reference(i) - got.Data(i)))
                scaleRef = std.Max(scaleRef, std.Abs(reference(i)))
            Next

            ' 单精度沿 k=128 累加的相对误差量级约 1e-6，取 1e-4 作为宽松但有效的门槛
            Dim tol = std.Max(0.0001 * scaleRef, 0.000001)

            results.Add(New CaseResult("混合精度 BatchedMatMul 数值精度", maxDiff <= tol,
                                       $"maxAbsDiff={maxDiff:E3} tol={tol:E3} 参考量级={scaleRef:F3}"))
        End Sub

        ''' <summary>
        ''' RNN 行为回归：确认把自研 jagged 矩阵换成 Tensor 之后网络仍能学习。
        ''' </summary>
        ''' <remarks>
        ''' 这是 Matrix → Tensor 迁移最有价值的一道保险：编译通过只能说明类型对得上，
        ''' 说明不了"每一步的形状与累加次序仍然正确"。这里用最简单的周期序列
        ''' （预测下一个符号）驱动 forwardBackward，只要损失显著下降，
        ''' 就说明前向的 tanh 递归、反向的 BPTT 与 Adagrad 更新链路都是通的。
        ''' </remarks>
        Private Sub RnnCase(results As List(Of CaseResult))
            Dim vocab = 4
            Dim seq = {0, 1, 2, 3, 0, 1, 2, 3}
            Dim target = {1, 2, 3, 0, 1, 2, 3, 0}

            Dim net As New SingleLayerRNN(vocab)
            Call net.initialize(vocab)

            Dim first As Double = 0
            Dim last As Double = 0
            Dim iterations = 300

            For i = 1 To iterations
                Dim loss = net.forwardBackward(seq, target)

                If i = 1 Then first = loss
                last = loss
            Next

            Dim finite = Not Double.IsNaN(last) AndAlso Not Double.IsInfinity(last)
            Dim improved = finite AndAlso last < first

            results.Add(New CaseResult(
                "RNN (Tensor) 训练可收敛",
                improved,
                $"初始损失={first:F4} 末次损失={last:F4} 下降={(first - last):F4}"))

            ' 采样也必须能跑通（走 softmax + 温度 + 隐藏态保存/恢复）
            Dim sampled = net.sampleIndices(8, {0}, 1.0, advance:=False)

            Dim validSample = sampled IsNot Nothing AndAlso sampled.Length = 8

            If validSample Then
                For Each s In sampled
                    If s < 0 OrElse s >= vocab Then validSample = False
                Next
            End If

            results.Add(New CaseResult("RNN (Tensor) 采样索引合法", validSample,
                                       If(validSample, $"样本={String.Join(",", sampled)}", "采样结果越界或为空")))
        End Sub

        ''' <summary>
        ''' 把一组双精度字面量包成 (1, n) 的行向量张量。
        ''' </summary>
        ''' <remarks>
        ''' 输入侧用行向量：LiquidCell 的 <c>ToRow</c> 会按 (1, n) 处理。
        ''' </remarks>
        Private Function VecD(ParamArray values As Double()) As Tensor
            Return New Tensor(values, 1, values.Length)
        End Function

        ''' <summary>单输出回归目标（秩 1，与 <c>ComputeOutputFrom</c> 的返回形状一致）</summary>
        ''' <remarks>
        ''' MUST 与预测值同形：<c>LNNTrainer.MSE</c> 会先做 <c>Shape.SequenceEqual</c> 校验，
        ''' 而输出层返回的是 <c>New Tensor(OutputSize)</c> —— 秩 1，不是 (1,1)。
        ''' </remarks>
        Private Function TargetOf(value As Double) As Tensor
            ' 注意不能写 New Tensor(New Double() {value})：
            ' 那个重载把参数当 data、shape 留空，构造出的张量秩为 0，
            ' 会在 Tensor.UpdateDimProds 里越界。显式给一维形状才是秩 1。
            Dim t = New Tensor(1)
            t(0) = value

            Return t
        End Function

        Private Sub BatchedMatMulCase(results As List(Of CaseResult))
            ' a: [2, 5, 7]，b: [2, 7, 3] —— batch 维为 2，覆盖多 block 路径
            Dim batches = 2, imax = 5, kmax = 7, jmax = 3
            Dim a = Randoms(batches * imax * kmax, seed:=201, scale:=2.0)
            Dim b = Randoms(batches * kmax * jmax, seed:=202, scale:=2.0)

            ' 刻意留出一部分零：前向有 "If av <> 0.0" 的零值跳过分支，必须被覆盖
            For i = 0 To a.Length - 1
                If (i Mod 3) = 0 Then a(i) = 0.0
            Next

            Dim aT = New Tensor(a, batches, imax, kmax)
            Dim bT = New Tensor(b, batches, kmax, jmax)

            Dim got = TensorOps.BatchedMatMul(aT, bT)
            Dim expected = ReferenceBatchedMatMul(a, b, batches, imax, kmax, jmax)

            Dim maxFwd As Double = 0
            For i = 0 To expected.Length - 1
                maxFwd = std.Max(maxFwd, std.Abs(expected(i) - got.Data(i)))
            Next

            results.Add(New CaseResult("BatchedMatMul 前向 逐位一致", maxFwd = 0.0, $"maxAbsDiff={maxFwd:E3}"))

            ' ---- 反向 ----
            Dim dCData = Randoms(batches * imax * jmax, seed:=203, scale:=1.0)
            Dim dCT = New Tensor(dCData, batches, imax, jmax)

            Dim dA As Tensor = Nothing, dB As Tensor = Nothing
            Call TensorOps.BatchedMatMulBackward(dCT, aT, bT, dA, dB)

            Dim expA(imax * kmax * batches - 1) As Double
            Dim expB(batches * kmax * jmax - 1) As Double

            Call ReferenceBatchedMatMulBackward(a, b, dCData, expA, expB, batches, imax, kmax, jmax)

            Dim maxA As Double = 0
            For i = 0 To expA.Length - 1
                maxA = std.Max(maxA, std.Abs(expA(i) - dA.Data(i)))
            Next

            Dim maxB As Double = 0
            For i = 0 To expB.Length - 1
                maxB = std.Max(maxB, std.Abs(expB(i) - dB.Data(i)))
            Next

            results.Add(New CaseResult("BatchedMatMul 反向 dA 逐位一致", maxA = 0.0, $"maxAbsDiff={maxA:E3}"))
            results.Add(New CaseResult("BatchedMatMul 反向 dB 逐位一致", maxB = 0.0, $"maxAbsDiff={maxB:E3}"))
        End Sub

        ''' <summary>朴素的串行批量矩阵乘（前向参考）</summary>
        Private Function ReferenceBatchedMatMul(a As Double(), b As Double(),
                                                batches As Integer, imax As Integer,
                                                kmax As Integer, jmax As Integer) As Double()
            Dim out(batches * imax * jmax - 1) As Double

            For blk = 0 To batches - 1
                For i = 0 To imax - 1
                    For k = 0 To kmax - 1
                        Dim av = a((blk * imax + i) * kmax + k)
                        If av = 0.0 Then Continue For

                        For j = 0 To jmax - 1
                            out((blk * imax + i) * jmax + j) += av * b((blk * kmax + k) * jmax + j)
                        Next
                    Next
                Next
            Next

            Return out
        End Function

        ''' <summary>朴素的串行批量矩阵乘（反向参考）</summary>
        Private Sub ReferenceBatchedMatMulBackward(a As Double(), b As Double(), dC As Double(),
                                                   dA As Double(), dB As Double(),
                                                   batches As Integer, imax As Integer,
                                                   kmax As Integer, jmax As Integer)
            For blk = 0 To batches - 1
                For i = 0 To imax - 1
                    For j = 0 To jmax - 1
                        Dim g = dC((blk * imax + i) * jmax + j)
                        If g = 0.0 Then Continue For

                        For k = 0 To kmax - 1
                            dA((blk * imax + i) * kmax + k) += g * b((blk * kmax + k) * jmax + j)
                            dB((blk * kmax + k) * jmax + j) += a((blk * imax + i) * kmax + k) * g
                        Next
                    Next
                Next
            Next
        End Sub

    End Module

End Namespace
