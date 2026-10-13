' ---------------------------------------------------------------------------
' 设备常驻与 AdamW 的用例
'
' 为什么单独成组：这两项是"单层 tensorF 端到端"能否真正拿到 GPU 收益的关键，
' 但从普通的算子对拍里看不出来 ——
'
'   * 常驻没生效时，算子仍然算得对（只是每步多跑一趟显存往返），
'     表现为"改完了但一点没变快"，是最容易被忽略的性能回归；
'   * AdamW 的底座默认返回 False（不提供），只有真的接上内核才会返回 True，
'     而内核更新是**就地**的，写错的话参数会静默跑飞。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
Imports std = System.Math

Namespace MixedPrecisionBench

    Module ResidencyCases

        Friend Sub RunAll(results As List(Of CaseResult))
            Call ResidencyCase(results)
            Call AdamWCase(results)
        End Sub

        ''' <summary>
        ''' 钉住 -> 在被钉住的张量上跑算子 -> 同步回主机 -> 解除钉住。
        ''' </summary>
        Private Sub ResidencyCase(results As List(Of CaseResult))
            Dim kernel = TensorF.computeKernelF

            If Not kernel.SupportsDeviceResidency Then
                results.Add(New CaseResult("设备常驻", True, "后端不支持常驻（CPU 后端），跳过"))
                Return
            End If

            Dim data = Randoms(8192, seed:=91, scale:=2.0)
            Dim t = AsF(data, 8192)
            Dim before = kernel.PinnedDeviceBytes

            If Not kernel.PinDevice(t, "bench.residency", False) Then
                results.Add(New CaseResult("设备常驻 钉住", False, "PinDevice 返回 False"))
                Return
            End If

            Dim pinnedOk = kernel.IsDevicePinned(t)
            Dim grew = kernel.PinnedDeviceBytes > before

            ' 被钉住的张量仍然要能正常参与运算
            Dim expected(data.Length - 1) As Single
            For i = 0 To data.Length - 1
                expected(i) = CSng(data(i) + 0.5)
            Next

            Dim computed = kernel.AddScalar(t, 0.5F)
            Dim maxDiff As Double = 0
            For i = 0 To expected.Length - 1
                maxDiff = std.Max(maxDiff, std.Abs(CDbl(expected(i)) - computed.Data(i)))
            Next

            ' 同步回主机：常驻张量的主机副本是陈旧的，这一步之后才可读
            Dim synced = kernel.SyncFromDevice(t)
            Dim released = kernel.UnpinDevice(t)
            Dim backToZero = kernel.PinnedDeviceBytes = before

            results.Add(New CaseResult("设备常驻 钉住/判等", pinnedOk AndAlso grew,
                                       $"IsDevicePinned={pinnedOk} 字节数增长={grew}"))
            ' 容差 1e-6：主机参考是"Double 相加后再舍入"，设备是"Single 相加"，
            ' 两者在 1 ULP 量级上不同是正常现象，不是错误。
            results.Add(New CaseResult("设备常驻 被钉住张量可参与运算", maxDiff <= 0.000001, $"maxAbsDiff={maxDiff:E3}"))
            results.Add(New CaseResult("设备常驻 同步与释放", synced AndAlso released AndAlso backToZero,
                                       $"sync={synced} unpin={released} 字节归零={backToZero}"))
        End Sub

        ''' <summary>
        ''' AdamW：把设备端内核结果与主机上的参考公式对照。
        ''' </summary>
        ''' <remarks>
        ''' 内核里有一条"单元素更新幅度不超过 10 倍学习率"的自愈钳位，
        ''' 专治单精度下二阶矩 v 下溢导致的爆炸（v→0 时 m/√v 会被放大好几个数量级）。
        ''' 这条钳位是**纯单精度特有的语义差异**：双精度栈的 AdamW 没有它。
        ''' 因此本用例把 v 的初值抬到明显远离 0 的区间，让钳位不被触发，
        ''' 从而在"与教科书公式等价"的前提下验证内核接线是否正确。
        ''' <para>
        ''' 实施时务必记住：把优化器切到 FP32 后，一旦 v 下溢，
        ''' 参数更新会被这条上限悄悄改写 —— 这是设计如此，不是缺陷。
        ''' </para>
        ''' </remarks>
        Private Sub AdamWCase(results As List(Of CaseResult))
            Dim n = 4096
            Dim param = Randoms(n, seed:=101, scale:=1.0)
            Dim grad = Randoms(n, seed:=102, scale:=0.3)
            Dim mom = Randoms(n, seed:=103, scale:=0.1)

            ' v 恒为正且远离 0：让 sqrt(vHat) 有稳定的量级
            Dim vel = Randoms(n, seed:=104, scale:=0.3)
            For i = 0 To vel.Length - 1
                vel(i) = 0.2 + std.Abs(vel(i))
            Next

            Dim lr = 0.01, b1 = 0.9, b2 = 0.999, eps = 0.00000001
            Dim bc1 = 0.1, bc2 = 0.01, wd = 0.001

            ' 主机参考实现
            Dim refParam(n - 1) As Double
            For i = 0 To n - 1
                Dim m = b1 * mom(i) + (1.0 - b1) * grad(i)
                Dim v = b2 * vel(i) + (1.0 - b2) * grad(i) * grad(i)
                Dim mHat = m / bc1
                Dim vHat = v / bc2
                Dim delta = lr * mHat / (std.Sqrt(vHat) + eps)

                If wd > 0.0 Then delta += lr * wd * param(i)

                refParam(i) = param(i) - delta
            Next

            Dim pT = AsF(param, n), gT = AsF(grad, n), mT = AsF(mom, n), vT = AsF(vel, n)
            Dim done = TensorF.computeKernelF.TryAdamWStep(pT, gT, mT, vT, lr, b1, b2, eps, bc1, bc2, wd)

            If Not done Then
                ' 不提供设备端 AdamW 是合法行为（CPU 后端默认返回 False）
                results.Add(New CaseResult("AdamW 设备端更新", True, "后端未提供设备端 AdamW，跳过"))
                Return
            End If

            Dim maxDiff = MaxAbsDiff(refParam, pT.Data)
            Dim scale = std.Max(MaxAbs(refParam), 1.0)
            Dim tol = std.Max(0.00001 * scale, 0.000001)

            results.Add(New CaseResult("AdamW 设备端更新", maxDiff <= tol, $"maxAbsDiff={maxDiff:E3} tol={tol:E3}"))

            ' 内核约定"更新即清零"：成功后梯度累加器必须是全 0，
            ' 否则下一步会把同一批梯度重复累加，等效学习率被悄悄放大。
            Dim maxGrad = MaxAbs(gT.ToDoubles())
            results.Add(New CaseResult("AdamW 更新后梯度清零", maxGrad = 0.0, $"maxAbs(grad)={maxGrad:E3}"))
        End Sub

    End Module

End Namespace
