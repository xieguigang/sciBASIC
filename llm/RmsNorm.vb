' ---------------------------------------------------------------------------
' RmsNorm —— 均方根归一化（Root Mean Square Layer Normalization）
'
' 与 LayerNorm 的差别只有一处：RMSNorm 不减均值、也不加偏置，只把向量按
' 自身的均方根长度缩放到单位尺度，再乘一个可学习的逐维缩放 γ：
'
'     rms(x) = sqrt( mean(x²) + eps )
'     ŷ      = x / rms(x)
'     y      = ŷ ⊙ γ
'
' 之所以现代 LLM 普遍用它：省掉了均值统计与 β 参数，计算量更小、对硬件更友好，
' 而实验表明"重新缩放不变性"才是 LayerNorm 起作用的真正来源，减均值贡献很小。
'
' Pre-Norm 用法：每个子层先做 RMSNorm 再进注意力 / MoE，残差绕过归一化直连，
' 从而让梯度有一条贯穿全部层的高速通路（这也是深层 Transformer 能训起来的关键）。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' RMSNorm 归一化层（Pre-Norm 用法）。
''' </summary>
Public Class RmsNorm

    Private ReadOnly _dModel As Integer
    Private ReadOnly _eps As Double

    ''' <summary>可学习的逐维缩放参数 γ，形状 <c>[dModel]</c>。</summary>
    Public ReadOnly Gamma As Tensor

    Private ReadOnly _gammaOptimizer As AdamW

    ''' <summary>前向传播的中间量缓存，供反向传播使用。</summary>
    Public Class Cache
        ''' <summary>归一化但未乘 γ 的结果：<c>x / rms(x)</c>，形状与输入同形</summary>
        Public XHat As Tensor
        ''' <summary>每个 (batch, position) 的 <c>1 / rms(x)</c></summary>
        Public InvRms As Double()
        ''' <summary>输入形状（反向时用于还原）</summary>
        Public InputShape As Integer()
    End Class

    Private _lastCache As Cache

    ''' <summary>最近一次 <see cref="Forward"/> 的中间量缓存。</summary>
    Public ReadOnly Property LastCache As Cache
        Get
            Return _lastCache
        End Get
    End Property

    ''' <summary>γ 的梯度累加器（由参数集统一管理）。</summary>
    Friend ReadOnly Property GammaOptimizer As AdamW
        Get
            Return _gammaOptimizer
        End Get
    End Property

    ''' <summary>参与本层的可训练参数。</summary>
    Public ReadOnly Property Parameters As Tensor()
        Get
            Return New Tensor() {Gamma}
        End Get
    End Property

    ''' <param name="dModel">最后一维的宽度</param>
    ''' <param name="eps">数值稳定项。RMSNorm 的 eps 是加在 mean(x²) 上的，不是标准差上</param>
    Public Sub New(dModel As Integer, Optional eps As Double = LLMTensorOps.DefaultRmsNormEps)
        _dModel = dModel
        _eps = eps

        ' γ 初始化为全 1，此时 RMSNorm 在训练初期就是"纯归一化"
        Gamma = New Tensor(New Integer() {dModel})
        Dim g = Gamma.Data
        For i As Integer = 0 To dModel - 1
            g(i) = 1.0
        Next
        Call Gamma.MarkHostModified()

        _gammaOptimizer = New AdamW(Gamma)
    End Sub

    ''' <summary>
    ''' 前向：<c>y = x / sqrt(mean(x²) + eps) ⊙ γ</c>。
    ''' </summary>
    ''' <param name="x">任意形状张量，归一化沿最后一维进行（典型形状 <c>[B, S, dModel]</c>）</param>
    Public Function Forward(x As Tensor) As Tensor
        If x.Shape(x.Rank - 1) <> _dModel Then
            Throw New ArgumentException(
                $"RMSNorm 的输入最后一维应为 {_dModel}，实际 [{String.Join(",", x.Shape)}]")
        End If

        Dim n = _dModel
        Dim blocks = x.Length \ n
        Dim shape = x.Shape

        Dim xHat = New Tensor(shape)
        Dim result = New Tensor(shape)
        Dim invRms(blocks - 1) As Double

        Dim src = x.Data
        Dim hat = xHat.Data
        Dim dst = result.Data
        Dim gammaData = Gamma.Data

        For blk As Integer = 0 To blocks - 1
            Dim offset = blk * n
            Dim sumSq As Double = 0.0

            For i As Integer = 0 To n - 1
                sumSq += src(offset + i) * src(offset + i)
            Next

            Dim inv = 1.0 / std.Sqrt(sumSq / n + _eps)
            invRms(blk) = inv

            For i As Integer = 0 To n - 1
                Dim h = src(offset + i) * inv
                hat(offset + i) = h
                dst(offset + i) = h * gammaData(i)
            Next
        Next

        Call xHat.MarkHostModified()
        Call result.MarkHostModified()

        _lastCache = New Cache With {
            .XHat = xHat,
            .InvRms = invRms,
            .InputShape = CType(shape.Clone(), Integer())
        }

        Return result
    End Function

    ''' <summary>
    ''' 反向传播：返回对输入的梯度，并把对 γ 的梯度累加到 γ 的梯度累加器。
    ''' </summary>
    ''' <param name="forwardCache">与该次前向对应的缓存快照</param>
    ''' <param name="dOut">对前向输出的梯度</param>
    ''' <remarks>
    ''' 记 <c>ŷ = x/rms</c>、<c>a_i = dOut_i · γ_i</c>，则
    ''' <c>dOut/dx_j = invRms · (a_j − ŷ_j · mean_i(a_i · ŷ_i))</c>。
    ''' 这一步不需要重算 rms，因为 <c>invRms</c> 已经在缓存里。
    ''' </remarks>
    Public Function Backward(forwardCache As Cache, dOut As Tensor) As Tensor
        Dim cache = forwardCache

        If cache Is Nothing Then Throw New InvalidOperationException("必须先执行前向传播才能反向传播")

        Dim n = _dModel
        Dim blocks = cache.XHat.Length \ n
        Dim dx = New Tensor(cache.InputShape)

        Dim hat = cache.XHat.Data
        Dim invRms = cache.InvRms
        Dim dOutData = dOut.Data
        Dim gammaData = Gamma.Data
        Dim dxData = dx.Data
        Dim dGamma = _gammaOptimizer.Gradient.Data

        ' 逐 block 的临时缓冲，避免在热循环里反复分配
        Dim a(n - 1) As Double

        For blk As Integer = 0 To blocks - 1
            Dim offset = blk * n
            Dim inner As Double = 0.0

            For i As Integer = 0 To n - 1
                Dim ai = dOutData(offset + i) * gammaData(i)
                a(i) = ai
                inner += ai * hat(offset + i)
            Next

            inner /= n

            Dim inv = invRms(blk)

            For i As Integer = 0 To n - 1
                dxData(offset + i) = inv * (a(i) - hat(offset + i) * inner)
                dGamma(i) += dOutData(offset + i) * hat(offset + i)
            Next
        Next

        Call dx.MarkHostModified()
        Call _gammaOptimizer.Gradient.MarkHostModified()

        Return dx
    End Function

    ''' <summary>
    ''' 把 γ 登记进参数集。
    ''' </summary>
    ''' <param name="registry">目标参数集</param>
    ''' <param name="prefix">参数名前缀</param>
    ''' <param name="weightDecay">
    ''' 权重衰减系数。RMSNorm 的 γ 承担的是"逐维尺度控制"而非知识强度，
    ''' 惯例上不对它施加权重衰减，因此默认 0。
    ''' </param>
    Public Sub RegisterParameters(registry As ParameterSet, prefix As String, Optional weightDecay As Double = 0.0)
        Call registry.Attach(prefix & ".gamma", Gamma, _gammaOptimizer, weightDecay)
    End Sub

    ''' <summary>清零 γ 的梯度累加器。</summary>
    Public Sub ZeroGradients()
        _gammaOptimizer.ZeroGrad()
    End Sub

    ''' <summary>按 AdamW 规则更新 γ。</summary>
    Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
        _gammaOptimizer.MakeTrainingStep(learningRate, [step], Gamma)
    End Sub

End Class


