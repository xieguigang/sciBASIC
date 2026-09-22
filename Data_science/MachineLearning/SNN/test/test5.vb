Imports Microsoft.VisualBasic.DeepLearning.SpikingNeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

' ============================================================================
' Part 5：回归型 SNN 组件的数值梯度自检
'
' 本 Part 验证为"脉冲网络回归建模"新增的三个通用构件是否实现正确：
'   A. RecurrentLIFLayer  递归 LIF 层（可训练）的 BPTT 梯度
'   B. LinearReadout      线性回归解码头的前向/反向
'   C. RegressionLosses   回归损失（MSE / L1）与结构先验正则的梯度
'   D. 掩码冻结           结构掩码对权重更新的约束是否真正生效
'
' 验证方法沿用 test1 的既有技巧：把前向的 Heaviside 阶跃替换为"替代导数的
' 原函数"（layer.SmoothForward = True），此时解析梯度与中心差分数值梯度在数学上
' 严格相等，从而可以逐元素对拍。
'
' 说明：数值对拍只使用 FastSigmoid 替代函数（σ' = 1/(α|u|+1)²，其原函数为
' u/(α|u|+1)，两者精确一致）。ATan / STE 的原函数与导数形式不成对，
' 不适用于本对拍，这里不做梯度校验。
' ============================================================================
Module test5

    ''' <summary>中心差分步长</summary>
    Private Const H As Double = 0.00001

    ''' <summary>数值对拍容差（绝对误差）</summary>
    Private Const Tol As Double = 0.00001

    Public Sub RecurrentLayerSelfCheck()
        Console.WriteLine("================================================================")
        Console.WriteLine("  Part 5 · 回归型 SNN 组件自检（数值梯度对拍）")
        Console.WriteLine("================================================================")
        Console.WriteLine()

        Dim passA = RecurrentGradientCheck(LIFResetMode.ZeroOnSpike)
        Dim passB = RecurrentGradientCheck(LIFResetMode.SubtractThreshold)
        Dim passC = ReadoutGradientCheck()
        Dim passD = LossGradientCheck()
        Dim passE = MaskFreezeCheck()

        Console.WriteLine()
        If passA AndAlso passB AndAlso passC AndAlso passD AndAlso passE Then
            Console.WriteLine("  [OK] Part 5 全部通过。")
        Else
            Console.WriteLine("  [FAIL] Part 5 存在未通过项，请检查上方明细。")
        End If
    End Sub

    ' -------------------------------------------------------------------------
    ' A/B. 递归 LIF 层的 BPTT 梯度对拍
    '
    ' 目标函数（对 W、I_ext、u0 均可导）：
    '   loss = Σ (wu ⊙ U_last)  +  Σ (ws ⊙ S_last)
    ' 因此解析反传时的上游梯度为：
    '   dU_last = wu ，dS_ext[T−1] = ws ，其余步 dS_ext = 0
    ' -------------------------------------------------------------------------
    Private Function RecurrentGradientCheck(resetMode As LIFResetMode) As Boolean
        Console.WriteLine($"--- A · RecurrentLIFLayer BPTT 梯度对拍（复位模式 = {resetMode}）---")

        Dim rng As New Random(20240916 + CInt(resetMode))
        Dim n = 4
        Dim batch = 2
        Dim steps = 5
        Dim beta = 0.85
        Dim theta = 0.6
        Dim alpha = 1.7

        Dim w0 = RandomTensor(rng, n, n, -0.6, 0.6)
        Dim mask = MaskTensor(n)
        Dim u0 = RandomTensor(rng, batch, n, 0.0, 0.5)
        Dim wu = RandomTensor(rng, batch, n, -1.0, 1.0)
        Dim ws = RandomTensor(rng, batch, n, -1.0, 1.0)

        Dim iExt As New List(Of Tensor)()
        For t = 1 To steps
            iExt.Add(RandomTensor(rng, batch, n, 0.0, 1.0))
        Next

        Dim evaluate As Func(Of Tensor, List(Of Tensor), Double) =
            Function(w As Tensor, currents As List(Of Tensor)) As Double
                Dim layer As New RecurrentLIFLayer("check", n, w, mask,
                                                   beta, theta, resetMode,
                                                   SurrogateKind.FastSigmoid, alpha)
                layer.SmoothForward = True
                Dim sHist = layer.ForwardSequence(currents, u0)

                Dim loss = 0.0
                Dim ud = layer.ULast.Data
                Dim wud = wu.Data
                For i = 0 To ud.Length - 1
                    loss += wud(i) * ud(i)
                Next

                Dim sd = sHist(steps - 1).Data
                Dim wsd = ws.Data
                For i = 0 To sd.Length - 1
                    loss += wsd(i) * sd(i)
                Next

                Return loss
            End Function

        ' ---- 解析梯度 ----
        Dim analyticLayer As New RecurrentLIFLayer("check", n, w0, mask,
                                                   beta, theta, resetMode,
                                                   SurrogateKind.FastSigmoid, alpha)
        analyticLayer.SmoothForward = True
        analyticLayer.ForwardSequence(iExt, u0)

        Dim dS As New List(Of Tensor)()
        For t = 1 To steps - 1
            dS.Add(New Tensor(batch, n))
        Next
        dS.Add(ws)

        Dim grads = analyticLayer.BackwardTime(dS, Nothing, wu)

        ' ---- 权重数值梯度 ----
        Dim maxErrW = ElementwiseMaxError(w0, grads.WeightGrad, Function(wt) evaluate(wt, iExt))
        Dim scaleW = MaxAbs(grads.WeightGrad)

        ' ---- 外部电流数值梯度（抽查若干位置） ----
        Dim maxErrX = 0.0
        Dim scaleX = 0.0
        For t = 0 To steps - 1
            For probe = 0 To std.Min(2, batch * n - 1)
                Dim idx = (probe * 7 + t * 3) Mod (batch * n)
                Dim analytic = grads.InputGrad(t).Data(idx)

                Dim lp = evaluate(w0, PerturbedCurrents(iExt, t, idx, H))
                Dim lm = evaluate(w0, PerturbedCurrents(iExt, t, idx, -H))
                Dim numeric = (lp - lm) / (2.0 * H)

                maxErrX = std.Max(maxErrX, std.Abs(analytic - numeric))
                scaleX = std.Max(scaleX, std.Abs(analytic))
            Next
        Next

        Console.WriteLine($"  N={n}, batch={batch}, T={steps}, β={beta}, θ={theta}, α={alpha}")
        Console.WriteLine($"  dW     最大绝对误差 = {maxErrW:E3}（|grad|max = {scaleW:F4}）")
        Console.WriteLine($"  dI_ext 最大绝对误差 = {maxErrX:E3}（|grad|max = {scaleX:F4}）")

        Dim ok = maxErrW < Tol AndAlso maxErrX < Tol
        Console.WriteLine(If(ok, "  [PASS] 递归层 BPTT 梯度与数值差分一致。",
                             "  [FAIL] 递归层 BPTT 梯度与数值差分不一致。"))
        Console.WriteLine()
        Return ok
    End Function

    ' -------------------------------------------------------------------------
    ' C. 线性解码头梯度对拍
    ' -------------------------------------------------------------------------
    Private Function ReadoutGradientCheck() As Boolean
        Console.WriteLine("--- B · LinearReadout 梯度对拍 ---")

        Dim rng As New Random(777)
        Dim n = 5
        Dim batch = 3

        Dim u = RandomTensor(rng, batch, n, -1.0, 1.0)
        Dim target = RandomTensor(rng, batch, n, -1.0, 1.0)
        Dim w0 = RandomTensor(rng, n, n, -0.5, 0.5)
        Dim b0 = RandomTensor(rng, 1, n, -0.2, 0.2)

        Dim lossOf As Func(Of Tensor, Tensor, Double) =
            Function(w As Tensor, b As Tensor) As Double
                Dim head As New LinearReadout("ro", n, n, w, b)
                Dim pred = head.Forward(u)
                Return RegressionLosses.MeanSquaredError(pred, target).Loss
            End Function

        Dim head0 As New LinearReadout("ro", n, n, w0, b0)
        Dim pred0 = head0.Forward(u)
        Dim lg = RegressionLosses.MeanSquaredError(pred0, target)

        head0.ZeroGrad()
        Dim du = head0.BackwardTime(lg.Grad)

        ' dW 数值梯度
        Dim maxErrW = ElementwiseMaxError(w0, head0.WeightGrad, Function(wt) lossOf(wt, b0))
        ' db 数值梯度
        Dim maxErrB = ElementwiseMaxError(b0, head0.BiasGrad, Function(bt) lossOf(w0, bt))
        ' du 数值梯度（对解码输入做扰动，此处直接扰动原始输入张量副本）
        Dim maxErrU = 0.0
        Dim ud = u.Data
        For i = 0 To ud.Length - 1
            Dim save = ud(i)
            ud(i) = save + H
            Dim lp = RegressionLosses.MeanSquaredError(head0.Forward(u), target).Loss
            ud(i) = save - H
            Dim lm = RegressionLosses.MeanSquaredError(head0.Forward(u), target).Loss
            ud(i) = save
            maxErrU = std.Max(maxErrU, std.Abs(du.Data(i) - (lp - lm) / (2.0 * H)))
        Next

        Console.WriteLine($"  MSE = {lg.Loss:F6}（batch={batch}, n={n}）")
        Console.WriteLine($"  dW 最大绝对误差 = {maxErrW:E3}")
        Console.WriteLine($"  db 最大绝对误差 = {maxErrB:E3}")
        Console.WriteLine($"  du 最大绝对误差 = {maxErrU:E3}")

        Dim ok = maxErrW < Tol AndAlso maxErrB < Tol AndAlso maxErrU < Tol
        Console.WriteLine(If(ok, "  [PASS] 解码头前向/反向实现正确。",
                             "  [FAIL] 解码头梯度与数值差分不一致。"))
        Console.WriteLine()
        Return ok
    End Function

    ' -------------------------------------------------------------------------
    ' D. 回归损失与结构正则的梯度对拍
    ' -------------------------------------------------------------------------
    Private Function LossGradientCheck() As Boolean
        Console.WriteLine("--- C · RegressionLosses 梯度对拍 ---")

        Dim rng As New Random(2025)
        Dim pred = RandomTensor(rng, 2, 4, -1.0, 1.0)
        Dim target = RandomTensor(rng, 2, 4, -1.0, 1.0)
        Dim weights = RandomTensor(rng, 2, 4, 0.5, 2.0)

        Dim maxErrMse = 0.0
        Dim maxErrMae = 0.0
        Dim maxErrWm = 0.0
        Dim pd = pred.Data

        Dim gMse = RegressionLosses.MeanSquaredError(pred, target)
        Dim gMae = RegressionLosses.MeanAbsoluteError(pred, target)
        Dim gWm = RegressionLosses.MeanSquaredError(pred, target, weights)

        For i = 0 To pd.Length - 1
            Dim save = pd(i)
            pd(i) = save + H
            Dim mseP = RegressionLosses.MeanSquaredError(pred, target).Loss
            Dim maeP = RegressionLosses.MeanAbsoluteError(pred, target).Loss
            Dim wmP = RegressionLosses.MeanSquaredError(pred, target, weights).Loss
            pd(i) = save - H
            Dim mseM = RegressionLosses.MeanSquaredError(pred, target).Loss
            Dim maeM = RegressionLosses.MeanAbsoluteError(pred, target).Loss
            Dim wmM = RegressionLosses.MeanSquaredError(pred, target, weights).Loss
            pd(i) = save

            maxErrMse = std.Max(maxErrMse, std.Abs(gMse.Grad.Data(i) - (mseP - mseM) / (2.0 * H)))
            maxErrMae = std.Max(maxErrMae, std.Abs(gMae.Grad.Data(i) - (maeP - maeM) / (2.0 * H)))
            maxErrWm = std.Max(maxErrWm, std.Abs(gWm.Grad.Data(i) - (wmP - wmM) / (2.0 * H)))
        Next

        ' 结构先验正则：对 W 的梯度做数值对拍
        Dim current = RandomTensor(rng, 4, 4, -0.5, 0.5)
        Dim reference = RandomTensor(rng, 4, 4, -0.5, 0.5)
        Dim mask = MaskTensor(4)
        Dim conf = RandomTensor(rng, 4, 4, 0.2, 1.0)

        Dim penalty = RegressionLosses.MaskedDeviationPenalty(current, reference, mask, conf)
        Dim penaltyLossOf As Func(Of Tensor, Double) =
            Function(w As Tensor) RegressionLosses.MaskedDeviationPenalty(w, reference, mask, conf).Loss
        Dim maxErrPrior = ElementwiseMaxError(current, penalty.Grad, penaltyLossOf)

        Console.WriteLine($"  MSE  梯度最大误差 = {maxErrMse:E3}（loss={gMse.Loss:F6}）")
        Console.WriteLine($"  MAE  梯度最大误差 = {maxErrMae:E3}（loss={gMae.Loss:F6}）")
        Console.WriteLine($"  加权 MSE 梯度最大误差 = {maxErrWm:E3}（loss={gWm.Loss:F6}）")
        Console.WriteLine($"  先验正则梯度最大误差 = {maxErrPrior:E3}（loss={penalty.Loss:F6}）")
        Console.WriteLine($"  权重 L1 和 = {RegressionLosses.L1Sum(current):F4}，L2 平方和 = {RegressionLosses.L2Sum(current):F4}")

        Dim ok = maxErrMse < Tol AndAlso maxErrMae < Tol AndAlso maxErrWm < Tol AndAlso maxErrPrior < Tol
        Console.WriteLine(If(ok, "  [PASS] 回归损失与先验正则梯度正确。",
                             "  [FAIL] 损失梯度与数值差分不一致。"))
        Console.WriteLine()
        Return ok
    End Function

    ' -------------------------------------------------------------------------
    ' E. 结构掩码冻结生效性
    ' -------------------------------------------------------------------------
    Private Function MaskFreezeCheck() As Boolean
        Console.WriteLine("--- D · 结构掩码冻结生效性 ---")

        Dim rng As New Random(4096)
        Dim n = 6
        Dim batch = 3
        Dim steps = 4

        Dim w0 = RandomTensor(rng, n, n, -0.4, 0.4)
        Dim mask = MaskTensor(n)         ' 约一半位置被冻结
        Dim u0 = RandomTensor(rng, batch, n, 0.0, 0.4)

        Dim iExt As New List(Of Tensor)()
        ' 让网络产生脉冲：输入电流量级略高于阈值
        For t = 1 To steps
            Dim c = RandomTensor(rng, batch, n, 0.2, 1.4)
            iExt.Add(c)
        Next

        Dim layer As New RecurrentLIFLayer("masked", n, w0, mask,
                                           0.9, 1.0, LIFResetMode.ZeroOnSpike)
        layer.ForwardSequence(iExt, u0)

        ' 必须提供非零的上游梯度，否则 BPTT 结果全为 0（无法验证"是否有突触被更新"）
        Dim dS As New List(Of Tensor)()
        For t = 1 To steps - 1
            dS.Add(New Tensor(batch, n))
        Next
        dS.Add(Tensor.Ones(New Integer() {batch, n}))

        Dim grad = layer.BackwardTime(dS)
        Dim masked = layer.MaskedWeightGrad()

        ' 模拟一次梯度下降 + 掩码裁剪
        Dim lr = 0.1
        Dim wData = layer.Weight.Data
        For i = 0 To wData.Length - 1
            wData(i) -= lr * masked.Data(i)
        Next
        layer.Weight.MarkHostModified()
        layer.ApplyMaskToWeight()

        Dim frozenViolation = 0.0
        Dim movedFree = 0
        Dim wd = layer.Weight.Data
        Dim md = mask.Data
        Dim w0d = w0.Data
        For i = 0 To wd.Length - 1
            If md(i) = 0.0 Then
                frozenViolation = std.Max(frozenViolation, std.Abs(wd(i)))
            ElseIf std.Abs(wd(i) - w0d(i)) > 1.0E-12 Then
                movedFree += 1
            End If
        Next

        ' 诊断接口冒烟测试
        Dim warns = layer.Diagnose()

        Console.WriteLine($"  掩码冻结位置残留权重最大值 = {frozenViolation:E3}（应为 0）")
        Console.WriteLine($"  被更新的可训练突触数 = {movedFree}/{MaskTensor(n).Data.Count(Function(v) v > 0)}")
        Console.WriteLine($"  Diagnose() 返回 {warns.Count} 条告警（{layer}）")
        For Each w In warns
            Console.WriteLine($"    · {w}")
        Next

        Dim ok = frozenViolation = 0.0 AndAlso movedFree > 0
        Console.WriteLine(If(ok, "  [PASS] 掩码成功冻结先验拓扑之外的突触。",
                             "  [FAIL] 掩码未生效或没有任何突触被更新。"))
        Console.WriteLine()
        Return ok
    End Function

    ' -------------------------------------------------------------------------
    ' 工具
    ' -------------------------------------------------------------------------

    ''' <summary>对一个张量的所有元素做中心差分数值梯度，返回与解析梯度的最大绝对误差</summary>
    Private Function ElementwiseMaxError(base As Tensor, analytic As Tensor,
                                         lossOf As Func(Of Tensor, Double)) As Double
        Dim d = base.Data
        Dim maxErr = 0.0

        For i = 0 To d.Length - 1
            Dim save = d(i)
            d(i) = save + H
            Dim lp = lossOf(base)
            d(i) = save - H
            Dim lm = lossOf(base)
            d(i) = save

            Dim numeric = (lp - lm) / (2.0 * H)
            maxErr = std.Max(maxErr, std.Abs(analytic.Data(i) - numeric))
        Next

        Return maxErr
    End Function

    Private Function MaxAbs(t As Tensor) As Double
        Dim d = t.Data
        Dim m = 0.0
        For i = 0 To d.Length - 1
            m = std.Max(m, std.Abs(d(i)))
        Next
        Return m
    End Function

    Private Function RandomTensor(rng As Random, rows As Integer, cols As Integer,
                                  lo As Double, hi As Double) As Tensor
        Dim d(rows * cols - 1) As Double
        For i = 0 To d.Length - 1
            d(i) = lo + rng.NextDouble() * (hi - lo)
        Next
        Return Tensor.Wrap(d, rows, cols)
    End Function

    ''' <summary>逐元素翻转的 0/1 掩码：约一半突触被冻结（0）</summary>
    Private Function MaskTensor(n As Integer) As Tensor
        Dim d(n * n - 1) As Double
        For i = 0 To d.Length - 1
            d(i) = If(i Mod 2 = 0, 1.0, 0.0)
        Next
        Return Tensor.Wrap(d, n, n)
    End Function

    ''' <summary>复制电流序列并只扰动 (t, idx) 处的单个元素</summary>
    Private Function PerturbedCurrents(source As List(Of Tensor), t As Integer,
                                       idx As Integer, delta As Double) As List(Of Tensor)
        Dim copy As New List(Of Tensor)()
        For k = 0 To source.Count - 1
            Dim d = source(k).Data
            Dim c = CType(d.Clone(), Double())
            If k = t Then c(idx) += delta
            copy.Add(Tensor.Wrap(c, source(k).Shape))
        Next
        Return copy
    End Function

End Module
