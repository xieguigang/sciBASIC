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
Imports Microsoft.VisualBasic.MachineLearning.Transformer
Imports Microsoft.VisualBasic.MachineLearning.RNN
Imports std = System.Math

Namespace MixedPrecisionBench

    Module UpperLayerParity

        Friend Sub RunAll(results As List(Of CaseResult))
            Call BatchedMatMulCase(results)
            Call RnnCase(results)
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
