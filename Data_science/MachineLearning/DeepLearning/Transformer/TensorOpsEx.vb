' ---------------------------------------------------------------------------
' TensorOpsEx —— smiles-transformer 迁移所需的补充算子与显式反向传播
'
' 与 TensorOps 的分工：
'   * TensorOps 服务于英西翻译 demo（AddNorm 无仿射参数、FFN 固定 ReLU）；
'   * 本模块严格对齐 PyTorch 的 nn.LayerNorm / nn.Linear / nn.GELU / F.nll_loss 语义，
'     用于加载官方权重后仍能得到一致的数值。
'
' 数值约定（与 torch 一致，务必不要"顺手改成更稳的写法"）：
'   * LayerNorm 使用有偏方差（除以 n），eps 位于 sqrt 之内；
'   * GELU 使用 tanh 近似；
'   * nll_loss 的分母是非 ignore 元素的个数（而不是 batch*seq）。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Namespace Transformer

    ''' <summary>
    ''' 面向序列模型的补充算子：GELU、仿射 LayerNorm、逐位置线性层、NLL 损失与多项式采样。
    ''' </summary>
    Public Module TensorOpsEx

        ''' <summary>GELU tanh 近似中的常数 <c>sqrt(2/pi)</c>。</summary>
        Public ReadOnly GeluC As Double = std.Sqrt(2.0 / std.PI)

        ''' <summary>GELU 立方项系数（tanh 近似的标准取值）。</summary>
        Public Const GeluCubic As Double = 0.044715

#Region "GELU"

        ''' <summary>
        ''' GELU 激活函数（tanh 近似）：<c>0.5·x·(1 + tanh(c·(x + 0.044715x³)))</c>。
        ''' </summary>
        ''' <param name="x">输入张量。</param>
        Public Function Gelu(x As Tensor) As Tensor
            If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))

            Dim result As New Tensor(x.Shape)
            Dim src = x.Data
            Dim dst = result.Data

            For i As Integer = 0 To src.Length - 1
                dst(i) = geluScalar(src(i))
            Next

            Call result.MarkHostModified()

            Return result
        End Function

        ''' <summary>
        ''' GELU 的反向传播。
        ''' </summary>
        ''' <param name="dOut">上游梯度。</param>
        ''' <param name="x">前向阶段的输入（GELU 的导数需要它，而不是它的输出）。</param>
        Public Function GeluBackward(dOut As Tensor, x As Tensor) As Tensor
            If dOut Is Nothing Then Throw New ArgumentNullException(NameOf(dOut))
            If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))
            If Not dOut.Shape.SequenceEqual(x.Shape) Then
                Throw New ArgumentException("GELU 反向要求梯度与输入同形")
            End If

            Dim result As New Tensor(x.Shape)
            Dim g = dOut.Data
            Dim src = x.Data
            Dim dst = result.Data

            For i As Integer = 0 To src.Length - 1
                Dim xv As Double = src(i)
                Dim u As Double = GeluC * (xv + GeluCubic * xv * xv * xv)
                Dim t As Double = std.Tanh(u)
                Dim dy As Double = 0.5 * (1.0 + t) +
                                   0.5 * xv * (1.0 - t * t) * GeluC * (1.0 + 3.0 * GeluCubic * xv * xv)

                dst(i) = g(i) * dy
            Next

            Call result.MarkHostModified()

            Return result
        End Function

        Private Function geluScalar(x As Double) As Double
            Dim u As Double = GeluC * (x + GeluCubic * x * x * x)

            Return 0.5 * x * (1.0 + std.Tanh(u))
        End Function

#End Region

#Region "LayerNorm（仿射）"

        ''' <summary>
        ''' 仿射 LayerNorm 的前向传播。
        ''' </summary>
        ''' <param name="x">输入张量，最后一维为特征维。</param>
        ''' <param name="gamma">缩放参数（长度 = 特征维）。</param>
        ''' <param name="beta">偏移参数（长度 = 特征维）。</param>
        ''' <param name="mean">输出：每个样本的均值。</param>
        ''' <param name="invStd">输出：每个样本的 <c>1/sqrt(var+eps)</c>。</param>
        ''' <param name="eps">数值稳定项，默认与 <c>nn.LayerNorm</c> 一致。</param>
        Public Function LayerNormForward(x As Tensor, gamma As Tensor, beta As Tensor,
                                         ByRef mean As Double(), ByRef invStd As Double(),
                                         Optional eps As Double = 0.00001) As Tensor
            If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))

            Dim n As Integer = x.Shape(x.Rank - 1)
            Dim blocks As Integer = x.Length \ n
            Dim result As New Tensor(x.Shape)
            Dim src = x.Data
            Dim dst = result.Data
            Dim g = If(gamma Is Nothing, Nothing, gamma.Data)
            Dim b = If(beta Is Nothing, Nothing, beta.Data)

            ReDim mean(blocks - 1)
            ReDim invStd(blocks - 1)

            For blk As Integer = 0 To blocks - 1
                Dim offset As Integer = blk * n
                Dim mu As Double = 0.0

                For i As Integer = 0 To n - 1
                    mu += src(offset + i)
                Next

                mu /= n

                Dim vari As Double = 0.0

                For i As Integer = 0 To n - 1
                    Dim d As Double = src(offset + i) - mu

                    vari += d * d
                Next

                ' torch F.layer_norm 使用有偏方差，且 eps 位于 sqrt 之内
                vari /= n

                Dim isd As Double = 1.0 / std.Sqrt(vari + eps)

                mean(blk) = mu
                invStd(blk) = isd

                For i As Integer = 0 To n - 1
                    Dim norm As Double = (src(offset + i) - mu) * isd

                    If g IsNot Nothing Then norm *= g(i)
                    If b IsNot Nothing Then norm += b(i)

                    dst(offset + i) = norm
                Next
            Next

            Call result.MarkHostModified()

            Return result
        End Function

        ''' <summary>
        ''' 仿射 LayerNorm 的反向传播。
        ''' </summary>
        ''' <param name="dOut">上游梯度。</param>
        ''' <param name="x">前向输入。</param>
        ''' <param name="gamma">缩放参数。</param>
        ''' <param name="mean">前向缓存的均值。</param>
        ''' <param name="invStd">前向缓存的 <c>1/sqrt(var+eps)</c>。</param>
        ''' <param name="dGamma">输出：缩放参数的梯度（累加到已有值）。</param>
        ''' <param name="dBeta">输出：偏移参数的梯度（累加到已有值）。</param>
        ''' <returns>对输入的梯度。</returns>
        Public Function LayerNormBackward(dOut As Tensor, x As Tensor, gamma As Tensor,
                                          mean As Double(), invStd As Double(),
                                          ByRef dGamma As Tensor, ByRef dBeta As Tensor) As Tensor
            If dOut Is Nothing Then Throw New ArgumentNullException(NameOf(dOut))
            If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))
            If Not dOut.Shape.SequenceEqual(x.Shape) Then
                Throw New ArgumentException("LayerNorm 反向要求梯度与输入同形")
            End If

            Dim n As Integer = x.Shape(x.Rank - 1)
            Dim blocks As Integer = x.Length \ n
            Dim dx As New Tensor(x.Shape)
            Dim src = x.Data
            Dim gIn = dOut.Data
            Dim dxData = dx.Data
            Dim g = If(gamma Is Nothing, Nothing, gamma.Data)

            Dim gammaGrad As New Tensor(New Integer() {n})
            Dim betaGrad As New Tensor(New Integer() {n})
            Dim dg = gammaGrad.Data
            Dim db = betaGrad.Data
            Dim dOutArr = dOut.Data

            For blk As Integer = 0 To blocks - 1
                Dim offset As Integer = blk * n
                Dim mu As Double = mean(blk)
                Dim isd As Double = invStd(blk)
                Dim sumDy As Double = 0.0
                Dim sumDyX As Double = 0.0

                For i As Integer = 0 To n - 1
                    Dim dy As Double = gIn(offset + i) * (If(g Is Nothing, 1.0, g(i)))
                    Dim xh As Double = (src(offset + i) - mu) * isd

                    sumDy += dy
                    sumDyX += dy * xh

                    ' 参数梯度与输入无关的归一化项
                    If g IsNot Nothing Then dg(i) += dOutArr(offset + i) * xh
                    db(i) += dOutArr(offset + i)
                Next

                Dim meanDy As Double = sumDy / n
                Dim meanDyX As Double = sumDyX / n

                For i As Integer = 0 To n - 1
                    Dim dy As Double = gIn(offset + i) * (If(g Is Nothing, 1.0, g(i)))
                    Dim xh As Double = (src(offset + i) - mu) * isd

                    dxData(offset + i) = isd * (dy - meanDy - xh * meanDyX)
                Next
            Next

            Call dx.MarkHostModified()
            Call gammaGrad.MarkHostModified()
            Call betaGrad.MarkHostModified()

            dGamma = gammaGrad
            dBeta = betaGrad

            Return dx
        End Function

#End Region

#Region "逐位置线性层"

        ''' <summary>
        ''' 逐位置仿射变换：<c>y[..., j] = Σ x[..., i]·W[i, j] + b[j]</c>。
        ''' </summary>
        ''' <param name="x">输入张量，最后一维为输入特征维。</param>
        ''' <param name="w">权重，形状 <c>[in, out]</c>（与 torch 的 <c>[out, in]</c> 互为转置）。</param>
        ''' <param name="b">偏置；<c>Nothing</c> 表示无偏置。</param>
        Public Function LinearForward(x As Tensor, w As Tensor, b As Tensor) As Tensor
            If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))
            If w Is Nothing Then Throw New ArgumentNullException(NameOf(w))

            Dim y As Tensor = TensorOps.BatchedMatMul(x, w)

            If b IsNot Nothing Then y = TensorOps.VecAdd(y, b)

            Return y
        End Function

        ''' <summary>
        ''' 逐位置线性层的反向传播。
        ''' </summary>
        ''' <param name="dOut">上游梯度。</param>
        ''' <param name="x">前向输入。</param>
        ''' <param name="w">前向权重。</param>
        ''' <param name="dW">输出：权重梯度（形状同 <paramref name="w"/>）。</param>
        ''' <param name="dB">输出：偏置梯度；无偏置时返回 <c>Nothing</c>。</param>
        ''' <returns>对输入的梯度。</returns>
        Public Function LinearBackward(dOut As Tensor, x As Tensor, w As Tensor,
                                       ByRef dW As Tensor, ByRef dB As Tensor) As Tensor
            If dOut Is Nothing Then Throw New ArgumentNullException(NameOf(dOut))

            Dim dX As Tensor = Nothing

            Call TensorOps.BatchedMatMulBackward(dOut, x, w, dX, dW)

            dB = TensorOps.VecAddBackward(dOut)

            Return dX
        End Function

#End Region

#Region "LogSoftmax / NLL"

        ''' <summary>
        ''' 沿最后一维的数值稳定 log-softmax。
        ''' </summary>
        ''' <param name="logits">输入 logits。</param>
        Public Function LogSoftmaxLastDim(logits As Tensor) As Tensor
            If logits Is Nothing Then Throw New ArgumentNullException(NameOf(logits))

            Dim n As Integer = logits.Shape(logits.Rank - 1)
            Dim blocks As Integer = logits.Length \ n
            Dim result As New Tensor(logits.Shape)
            Dim src = logits.Data
            Dim dst = result.Data

            For blk As Integer = 0 To blocks - 1
                Dim offset As Integer = blk * n
                Dim maxVal As Double = src(offset)

                For i As Integer = 1 To n - 1
                    If src(offset + i) > maxVal Then maxVal = src(offset + i)
                Next

                Dim sumExp As Double = 0.0

                For i As Integer = 0 To n - 1
                    sumExp += std.Exp(src(offset + i) - maxVal)
                Next

                Dim logSumExp As Double = maxVal + std.Log(sumExp)

                For i As Integer = 0 To n - 1
                    dst(offset + i) = src(offset + i) - logSumExp
                Next
            Next

            Call result.MarkHostModified()

            Return result
        End Function

        ''' <summary>
        ''' 负对数似然损失（输入为 log-softmax 输出），并给出对 logits 的梯度。
        ''' </summary>
        ''' <param name="logProbs">log-softmax 输出，最后一维为词表维。</param>
        ''' <param name="targets">每个位置的正确 token 下标。</param>
        ''' <param name="ignoreIndex">被忽略的目标下标（通常是 <c>&lt;pad&gt;</c>），不参与损失与梯度。</param>
        ''' <param name="dLogits">输出：对 logits 的梯度（已按有效元素数归一化）。</param>
        ''' <returns>平均损失（分母为非 ignore 元素个数）。</returns>
        Public Function NllLoss(logProbs As Tensor, targets As Integer(), ignoreIndex As Integer,
                                ByRef dLogits As Tensor) As Double
            If logProbs Is Nothing Then Throw New ArgumentNullException(NameOf(logProbs))
            If targets Is Nothing Then Throw New ArgumentNullException(NameOf(targets))

            Dim n As Integer = logProbs.Shape(logProbs.Rank - 1)
            Dim blocks As Integer = logProbs.Length \ n

            If targets.Length <> blocks Then
                Throw New ArgumentException($"目标个数（{targets.Length}）与样本行数（{blocks}）不一致")
            End If

            Dim grad As New Tensor(logProbs.Shape)
            Dim lp = logProbs.Data
            Dim g = grad.Data
            Dim total As Double = 0.0
            Dim count As Integer = 0

            For blk As Integer = 0 To blocks - 1
                Dim offset As Integer = blk * n
                Dim target As Integer = targets(blk)

                If target = ignoreIndex Then Continue For

                If target < 0 OrElse target >= n Then
                    Throw New ArgumentOutOfRangeException(NameOf(targets),
                        $"第 {blk} 个目标下标 {target} 超出词表范围 [0, {n - 1}]")
                End If

                total -= lp(offset + target)
                count += 1
            Next

            If count = 0 Then
                Call grad.MarkHostModified()
                dLogits = grad

                Return 0.0
            End If

            ' log_softmax 之后，交叉熵对 logits 的梯度就是 softmax - onehot
            Dim scale As Double = 1.0 / count

            For blk As Integer = 0 To blocks - 1
                Dim offset As Integer = blk * n
                Dim target As Integer = targets(blk)

                If target = ignoreIndex Then Continue For

                For i As Integer = 0 To n - 1
                    g(offset + i) = std.Exp(lp(offset + i)) * scale
                Next

                g(offset + target) -= scale
            Next

            Call grad.MarkHostModified()
            dLogits = grad

            Return total / count
        End Function

#End Region

#Region "逐元素运算与 Dropout"

        ''' <summary>逐元素相加（残差连接使用，返回新张量，不修改入参）。</summary>
        ''' <param name="a">左操作数。</param>
        ''' <param name="b">右操作数。</param>
        Public Function AddTensors(a As Tensor, b As Tensor) As Tensor
            If a Is Nothing Then Throw New ArgumentNullException(NameOf(a))
            If b Is Nothing Then Throw New ArgumentNullException(NameOf(b))
            If Not a.Shape.SequenceEqual(b.Shape) Then Throw New ArgumentException("逐元素相加要求形状一致")

            Return Tensor.computeKernel.Add(a, b)
        End Function

        ''' <summary>
        ''' 标准 inverted dropout：训练时以概率 <paramref name="rate"/> 置零，保留项放大 <c>1/(1-rate)</c>。
        ''' </summary>
        ''' <param name="x">输入张量。</param>
        ''' <param name="rate">丢弃概率。</param>
        ''' <param name="mask">输出：与 <paramref name="x"/> 等长的丢弃掩码（True 表示丢弃）。</param>
        ''' <param name="rnd">随机数发生器。</param>
        Public Function DropoutForward(x As Tensor, rate As Double, ByRef mask As Boolean(),
                                       Optional rnd As Random = Nothing) As Tensor
            If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))

            If rate <= 0 Then
                mask = Nothing

                Return x
            End If

            If rate >= 1 Then Throw New ArgumentException("dropout 概率必须小于 1")

            Dim generator As Random = If(rnd, sharedRandom.Value)
            Dim result As New Tensor(x.Shape)
            Dim src = x.Data
            Dim dst = result.Data
            Dim keep As Double = 1.0 / (1.0 - rate)

            mask = New Boolean(src.Length - 1) {}

            For i As Integer = 0 To src.Length - 1
                If generator.NextDouble() < rate Then
                    mask(i) = True
                    dst(i) = 0.0
                Else
                    dst(i) = src(i) * keep
                End If
            Next

            Call result.MarkHostModified()

            Return result
        End Function

        ''' <summary><see cref="DropoutForward"/> 的反向传播。</summary>
        ''' <param name="dOut">上游梯度。</param>
        ''' <param name="mask">前向阶段的丢弃掩码。</param>
        ''' <param name="rate">丢弃概率。</param>
        Public Function DropoutBackward(dOut As Tensor, mask As Boolean(), rate As Double) As Tensor
            If dOut Is Nothing Then Throw New ArgumentNullException(NameOf(dOut))

            If rate <= 0 OrElse mask Is Nothing Then Return dOut

            Dim result As New Tensor(dOut.Shape)
            Dim src = dOut.Data
            Dim dst = result.Data
            Dim keep As Double = 1.0 / (1.0 - rate)

            For i As Integer = 0 To src.Length - 1
                dst(i) = If(mask(i), 0.0, src(i) * keep)
            Next

            Call result.MarkHostModified()

            Return result
        End Function

#End Region

#Region "采样"

        ''' <summary>
        ''' 按给定权重做一次多项式采样。
        ''' </summary>
        ''' <param name="weights">非负权重（通常是已经取 exp 的概率或 logits 的指数）。</param>
        ''' <param name="rnd">随机数发生器；<c>Nothing</c> 时使用共享实例。</param>
        Public Function Multinomial(weights As Tensor, Optional rnd As Random = Nothing) As Integer
            If weights Is Nothing Then Throw New ArgumentNullException(NameOf(weights))
            If weights.Rank <> 1 Then Throw New ArgumentException("多项式采样要求一维权重")

            Dim generator As Random = If(rnd, sharedRandom.Value)
            Dim data = weights.Data
            Dim total As Double = 0.0

            For i As Integer = 0 To data.Length - 1
                If data(i) < 0 Then Throw New ArgumentException("采样权重不能为负")
                total += data(i)
            Next

            If total <= 0 Then Throw New ArgumentException("采样权重之和必须为正")

            Dim threshold As Double = generator.NextDouble() * total
            Dim cumulative As Double = 0.0

            For i As Integer = 0 To data.Length - 1
                cumulative += data(i)
                If threshold <= cumulative Then Return i
            Next

            Return data.Length - 1
        End Function

        Private ReadOnly sharedRandom As New Threading.ThreadLocal(Of Random)(Function() New Random(Guid.NewGuid().GetHashCode()))

#End Region

    End Module

End Namespace
