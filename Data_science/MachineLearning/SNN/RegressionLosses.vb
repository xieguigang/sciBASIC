' ============================================================================
' RegressionLosses.vb — 回归任务的损失函数与前向结构正则原语
'
' 既有的 Losses（Network.vb）只提供 Softmax 交叉熵，服务于分类任务。
' 但脉冲神经网络的另一大类应用是"连续值回归"——例如用 SNN 拟合基因表达量、
' 时序信号、膜电位轨迹等。本模块补齐这部分通用能力：
'
'   MeanSquaredError        均方误差（可选逐元素权重）
'   MeanAbsoluteError       平均绝对误差（L1，次梯度）
'   L1Sum / L2Sum           权重的 L1 / L2 标量和（稀疏正则的标量项）
'   L1Gradient / L2Gradient 稀疏正则的梯度
'   MaskedDeviationPenalty  带掩码与置信度的"结构先验"偏差惩罚
'                           （阻止可训练权重漂离先验初始值）
'
' 返回类型复用 Network.vb 中的 LossGradient（Loss + dL/dpred 张量），
' 因此训练循环可以像使用 SoftmaxCrossEntropy 一样统一处理两类损失。
'
' 约定：所有损失都按"元素平均"归一化（加权时按权重和归一化），
' 因此其梯度量级与 batch/规模无关，便于统一设置学习率。
' ============================================================================

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>回归损失与结构正则原语</summary>
Public Module RegressionLosses

#Region "回归损失"

    ''' <summary>
    ''' 均方误差 MSE。
    '''   L = Σ w⊙(ŷ − y)² / Σ w        （无权重时即元素平均）
    '''   dL/dŷ = 2·w⊙(ŷ − y) / Σ w
    ''' </summary>
    ''' <param name="prediction">预测值 ŷ（任意形状，按展平元素计算）</param>
    ''' <param name="target">目标值 y，形状必须与预测值一致</param>
    ''' <param name="weights">
    ''' 可选的逐元素权重（形状一致）：用于强调重要基因/时间步，为 Nothing 时全部为 1。
    ''' </param>
    Public Function MeanSquaredError(prediction As Tensor, target As Tensor,
                                     Optional weights As Tensor = Nothing) As LossGradient
        AssertSameShape(prediction, target, "target")
        If weights IsNot Nothing Then AssertSameShape(prediction, weights, "weights")

        Dim n = prediction.Length
        Dim norm = Normalizer(weights, n)
        Dim p = prediction.Data
        Dim y = target.Data
        Dim wd = If(weights Is Nothing, Nothing, weights.Data)

        Dim grad = New Tensor(prediction.Shape)
        Dim gd = grad.Data
        Dim loss = 0.0

        For i = 0 To n - 1
            Dim d = p(i) - y(i)
            Dim wi = If(wd Is Nothing, 1.0, wd(i))
            loss += wi * d * d
            gd(i) = 2.0 * wi * d / norm
        Next

        Return New LossGradient With {.Loss = loss / norm, .Grad = grad}
    End Function

    ''' <summary>
    ''' 平均绝对误差（L1 损失）。
    '''   L = Σ w⊙|ŷ − y| / Σ w
    '''   dL/dŷ = w⊙sign(ŷ − y) / Σ w    （在 ŷ = y 处取 0 次梯度）
    ''' </summary>
    Public Function MeanAbsoluteError(prediction As Tensor, target As Tensor,
                                      Optional weights As Tensor = Nothing) As LossGradient
        AssertSameShape(prediction, target, "target")
        If weights IsNot Nothing Then AssertSameShape(prediction, weights, "weights")

        Dim n = prediction.Length
        Dim norm = Normalizer(weights, n)
        Dim p = prediction.Data
        Dim y = target.Data
        Dim wd = If(weights Is Nothing, Nothing, weights.Data)

        Dim grad = New Tensor(prediction.Shape)
        Dim gd = grad.Data
        Dim loss = 0.0

        For i = 0 To n - 1
            Dim d = p(i) - y(i)
            Dim wi = If(wd Is Nothing, 1.0, wd(i))
            loss += wi * std.Abs(d)
            gd(i) = wi * Sign(d) / norm
        Next

        Return New LossGradient With {.Loss = loss / norm, .Grad = grad}
    End Function

#End Region

#Region "稀疏正则（L1 / L2）"

    ''' <summary>权重的 L1 和 Σ|w|（脉冲冗余 / 突触稀疏的惩罚项标量）</summary>
    Public Function L1Sum(w As Tensor) As Double
        Dim d = w.Data
        Dim s = 0.0
        For i = 0 To d.Length - 1
            s += std.Abs(d(i))
        Next
        Return s
    End Function

    ''' <summary>权重的 L2 平方和 Σw²</summary>
    Public Function L2Sum(w As Tensor) As Double
        Dim d = w.Data
        Dim s = 0.0
        For i = 0 To d.Length - 1
            s += d(i) * d(i)
        Next
        Return s
    End Function

    ''' <summary>L1 稀疏正则的梯度 scale·sign(w)（在 w = 0 处取 0 次梯度）</summary>
    Public Function L1Gradient(w As Tensor, scale As Double) As Tensor
        Return w.Apply(Function(v As Double) scale * Sign(v))
    End Function

    ''' <summary>L2 正则的梯度 2·scale·w</summary>
    Public Function L2Gradient(w As Tensor, scale As Double) As Tensor
        Return w.Apply(Function(v As Double) 2.0 * scale * v)
    End Function

#End Region

#Region "结构先验正则"

    ''' <summary>
    ''' 结构先验偏差惩罚：约束可训练权重不要漂离先验初始值。
    '''   L = Σ (mask⊙conf⊙(W − W₀)²) / norm
    ''' 其中 mask 标记"存在先验证据的突触"（1 = 有边），conf 为该边的证据置信度，
    ''' 因此高置信度的 TF→Target 调控关系会被更强地钉在初始权重附近。
    ''' </summary>
    ''' <param name="current">当前可训练权重 W</param>
    ''' <param name="reference">先验初始权重 W₀（形状一致）</param>
    ''' <param name="mask">结构掩码（1 = 有先验证据的突触，形状一致）</param>
    ''' <param name="confidence">逐边置信度（形状一致）；为 Nothing 时全部按 1 处理</param>
    ''' <param name="normalizeByAllElements">
    ''' True = 除以元素总数（readme 中的 MEAN 语义）；
    ''' False（默认）= 除以 Σ(mask⊙conf)，即"按有证据的边取平均"，
    ''' 在稀疏先验下量级更稳定。
    ''' </param>
    Public Function MaskedDeviationPenalty(current As Tensor, reference As Tensor, mask As Tensor,
                                           Optional confidence As Tensor = Nothing,
                                           Optional normalizeByAllElements As Boolean = False) As LossGradient
        AssertSameShape(current, reference, "reference")
        AssertSameShape(current, mask, "mask")
        If confidence IsNot Nothing Then AssertSameShape(current, confidence, "confidence")

        Dim n = current.Length
        Dim w = current.Data
        Dim w0 = reference.Data
        Dim md = mask.Data
        Dim cd = If(confidence Is Nothing, Nothing, confidence.Data)

        Dim norm = 1.0
        If normalizeByAllElements Then
            norm = n
        Else
            Dim s = 0.0
            For i = 0 To n - 1
                s += md(i) * If(cd Is Nothing, 1.0, cd(i))
            Next
            norm = If(s > 0.0, s, 1.0)
        End If

        Dim grad = New Tensor(current.Shape)
        Dim gd = grad.Data
        Dim loss = 0.0

        For i = 0 To n - 1
            Dim ci = If(cd Is Nothing, 1.0, cd(i))
            Dim mi = md(i) * ci
            Dim d = w(i) - w0(i)
            loss += mi * d * d
            gd(i) = 2.0 * mi * d / norm
        Next

        Return New LossGradient With {.Loss = loss / norm, .Grad = grad}
    End Function

#End Region

#Region "组合辅助"

    ''' <summary>
    ''' 把正则项梯度按权重就地累加进主梯度：grad ← grad + scale · delta
    ''' 用于把结构先验正则与稀疏正则的梯度并入主梯度后再交给优化器。
    ''' </summary>
    ''' <param name="target">主梯度（就地累加；调用前请确保它是本步新建的副本）</param>
    ''' <param name="delta">正则项梯度张量（形状/长度必须与主梯度一致）</param>
    ''' <param name="scale">正则项权重（α 或 β）</param>
    Public Sub AddScaledGradientInPlace(target As Tensor, delta As Tensor, scale As Double)
        If target Is Nothing Then
            Throw New ArgumentNullException(NameOf(target))
        End If
        If delta Is Nothing OrElse scale = 0.0 Then
            Return
        End If
        If delta.Length <> target.Length Then
            Throw New ArgumentException(
                $"正则梯度长度({delta.Length})与主梯度长度({target.Length})不一致", NameOf(delta))
        End If

        Dim g = target.Data
        Dim d = delta.Data
        For i = 0 To g.Length - 1
            g(i) += scale * d(i)
        Next

        ' 绕过索引器就地写入：声明主机数据已修改，使设备端缓存失效
        target.MarkHostModified()
    End Sub

    ''' <summary>符号函数（0 → 0，用于 L1 的次梯度）</summary>
    Private Function Sign(v As Double) As Double
        If v > 0.0 Then Return 1.0
        If v < 0.0 Then Return -1.0
        Return 0.0
    End Function

    ''' <summary>加权损失的归一化因子：Σw（权重为 Nothing / 全零时回退元素总数）</summary>
    Private Function Normalizer(weights As Tensor, count As Integer) As Double
        If weights Is Nothing Then Return count

        Dim d = weights.Data
        Dim s = 0.0
        For i = 0 To d.Length - 1
            s += d(i)
        Next
        Return If(s > 0.0, s, 1.0)
    End Function

    Private Sub AssertSameShape(a As Tensor, b As Tensor, paramName As String)
        If b Is Nothing Then
            Throw New ArgumentNullException(paramName)
        End If
        If Not a.Shape.SequenceEqual(b.Shape) Then
            Throw New ArgumentException(
                $"{paramName} 形状 [{String.Join(",", b.Shape)}] 与 [{String.Join(",", a.Shape)}] 不一致",
                paramName)
        End If
    End Sub

#End Region

End Module
