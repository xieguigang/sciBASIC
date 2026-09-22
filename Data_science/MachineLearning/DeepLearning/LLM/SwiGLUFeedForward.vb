' ---------------------------------------------------------------------------
' SwiGLUFeedForward —— 门控前馈网络（SwiGLU）
'
' 传统 FFN 是"升维 → ReLU → 降维"：
'     y = ReLU(x·W1) · W2
'
' SwiGLU 引入一条额外的"门控"支路，用 SiLU（= Swish）做开关：
'     gate = x·Wg
'     up   = x·Wu
'     y    = ( SiLU(gate) ⊙ up ) · Wd
'
' 直觉：up 提供"候选内容"，SiLU(gate) 提供"逐维的通过率"。乘性交互比
' ReLU 的硬门限表达能力更强，也是 Llama / Qwen / Mistral / DeepSeek 的标配。
'
' 参数量：三条权重使得同样 hidden 宽度下参数比传统两层 MLP 多 50%，
' 因此实践中 hidden 通常取 (8/3)·dModel 再对齐到 64 的倍数，而不是 4·dModel。
' 本类同时被"稠密 FFN"与"MoE 的单个专家"复用 —— 两者只是调用方的组织方式不同。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Namespace LLM

    ''' <summary>
    ''' SwiGLU 前馈网络：<c>y = (SiLU(x·Wg) ⊙ (x·Wu)) · Wd</c>。
    ''' </summary>
    Public Class SwiGLUFeedForward

        Private ReadOnly _dModel As Integer
        Private ReadOnly _hidden As Integer

        ''' <summary>门控投影，形状 <c>[dModel, hidden]</c>。</summary>
        Public ReadOnly Wg As Tensor

        ''' <summary>内容投影，形状 <c>[dModel, hidden]</c>。</summary>
        Public ReadOnly Wu As Tensor

        ''' <summary>输出投影（降维），形状 <c>[hidden, dModel]</c>。</summary>
        Public ReadOnly Wd As Tensor

        Private ReadOnly _wgOpt As AdamW
        Private ReadOnly _wuOpt As AdamW
        Private ReadOnly _wdOpt As AdamW

        ''' <summary>中间层宽度。</summary>
        Public ReadOnly Property HiddenSize As Integer
            Get
                Return _hidden
            End Get
        End Property

        ''' <summary>本模块的全部可训练参数。</summary>
        Public ReadOnly Property Parameters As Tensor()
            Get
                Return New Tensor() {Wg, Wu, Wd}
            End Get
        End Property

        ''' <summary>前向传播的中间量缓存，供反向传播使用。</summary>
        Public Class Cache
            ''' <summary>本层输入 <c>[B, S, dModel]</c></summary>
            Public Input As Tensor
            ''' <summary>门控支路的预激活值 <c>x·Wg</c></summary>
            Public Gate As Tensor
            ''' <summary>内容支路 <c>x·Wu</c></summary>
            Public Up As Tensor
            ''' <summary>门控后的激活值 <c>SiLU(gate) ⊙ up</c></summary>
            Public Activation As Tensor
        End Class

        Private _lastCache As Cache

        ''' <summary>最近一次 <see cref="Forward"/> 的中间量缓存。</summary>
        Public ReadOnly Property LastCache As Cache
            Get
                Return _lastCache
            End Get
        End Property

        ''' <param name="dModel">输入输出宽度</param>
        ''' <param name="hidden">中间层宽度</param>
        Public Sub New(dModel As Integer, hidden As Integer)
            If dModel <= 0 Then Throw New ArgumentException($"dModel 必须为正数，实际 {dModel}")
            If hidden <= 0 Then Throw New ArgumentException($"hidden 必须为正数，实际 {hidden}")

            _dModel = dModel
            _hidden = hidden

            Wg = LLMTensorOps.HeNormalInit(New Integer() {dModel, hidden})
            Wu = LLMTensorOps.HeNormalInit(New Integer() {dModel, hidden})
            Wd = LLMTensorOps.HeNormalInit(New Integer() {hidden, dModel})

            _wgOpt = New AdamW(Wg)
            _wuOpt = New AdamW(Wu)
            _wdOpt = New AdamW(Wd)
        End Sub

        ''' <summary>把本模块参数登记进参数集。</summary>
        Public Sub RegisterParameters(registry As ParameterSet, prefix As String, Optional weightDecay As Double = 0.0)
            ' 三个权重矩阵只被 BatchedMatMul 消费（最终走 MatMul → 设备常驻表），
            ' 主机侧没有循环直接读它们的 Data
            Call registry.Attach(prefix & ".Wg", Wg, _wgOpt, weightDecay, deviceResident:=True)
            Call registry.Attach(prefix & ".Wu", Wu, _wuOpt, weightDecay, deviceResident:=True)
            Call registry.Attach(prefix & ".Wd", Wd, _wdOpt, weightDecay, deviceResident:=True)
        End Sub

        ''' <summary>前向：<c>y = (SiLU(x·Wg) ⊙ (x·Wu)) · Wd</c>。</summary>
        ''' <param name="x">输入 <c>[B, S, dModel]</c></param>
        Public Function Forward(x As Tensor) As Tensor
            If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))
            If x.Shape(x.Rank - 1) <> _dModel Then
                Throw New ArgumentException(
                    $"SwiGLU 要求输入最后一维为 {_dModel}，实际 [{String.Join(",", x.Shape)}]")
            End If

            Dim gate = Transformer.TensorOps.BatchedMatMul(x, Wg)
            Dim up = Transformer.TensorOps.BatchedMatMul(x, Wu)

            ' SiLU(gate) ⊙ up —— 手工循环以便同时把中间量留在缓存里
            Dim activation = New Tensor(gate.Shape)
            Dim g = gate.Data
            Dim u = up.Data
            Dim a = activation.Data

            For i As Integer = 0 To g.Length - 1
                Dim z = g(i)
                Dim silu = z / (1.0 + std.Exp(-z))
                a(i) = silu * u(i)
            Next

            Call activation.MarkHostModified()

            Dim y = Transformer.TensorOps.BatchedMatMul(activation, Wd)

            _lastCache = New Cache With {
                .Input = x,
                .Gate = gate,
                .Up = up,
                .Activation = activation
            }

            Return y
        End Function

        ''' <summary>反向传播：返回对输入的梯度，并把三条权重的梯度累加进各自的梯度累加器。</summary>
        ''' <param name="forwardCache">与该次前向对应的缓存快照</param>
        ''' <param name="dOut">对前向输出的梯度 <c>[B, S, dModel]</c></param>
        Public Function Backward(forwardCache As Cache, dOut As Tensor) As Tensor
            Dim cache = forwardCache

            If cache Is Nothing Then Throw New InvalidOperationException("必须先执行前向传播才能反向传播")

            ' y = act · Wd
            Dim dAct As Tensor = Nothing
            Dim dWd As Tensor = Nothing
            Call Transformer.TensorOps.BatchedMatMulBackward(dOut, cache.Activation, Wd, dAct, dWd)
            Call LLMTensorOps.Accumulate(_wdOpt.Gradient, dWd)

            ' act = SiLU(gate) ⊙ up
            Dim gate = cache.Gate.Data
            Dim up = cache.Up.Data
            Dim a = cache.Activation.Data
            Dim dActData = dAct.Data

            ' 注意：SiLU 的导数是对 gate 求的，而 Act 还乘过 up。
            '   dGate = dAct ⊙ up ⊙ SiLU'(gate)
            '   dUp   = dAct ⊙ SiLU(gate)
            Dim dGate = New Tensor(cache.Gate.Shape)
            Dim dUp = New Tensor(cache.Up.Shape)

            Dim dGateData = dGate.Data
            Dim dUpData = dUp.Data

            For i As Integer = 0 To gate.Length - 1
                Dim z = gate(i)
                Dim sig = 1.0 / (1.0 + std.Exp(-z))
                Dim silu = z * sig

                ' SiLU'(z) = sigmoid(z) · (1 + z · (1 − sigmoid(z)))
                Dim dSilu = sig * (1.0 + z * (1.0 - sig))
                Dim d = dActData(i)

                dGateData(i) = d * up(i) * dSilu
                dUpData(i) = d * silu
            Next

            Call dGate.MarkHostModified()
            Call dUp.MarkHostModified()

            ' gate = x · Wg ； up = x · Wu
            Dim dXg As Tensor = Nothing
            Dim dXu As Tensor = Nothing
            Dim dWg As Tensor = Nothing
            Dim dWu As Tensor = Nothing

            Call Transformer.TensorOps.BatchedMatMulBackward(dGate, cache.Input, Wg, dXg, dWg)
            Call Transformer.TensorOps.BatchedMatMulBackward(dUp, cache.Input, Wu, dXu, dWu)

            Call LLMTensorOps.Accumulate(_wgOpt.Gradient, dWg)
            Call LLMTensorOps.Accumulate(_wuOpt.Gradient, dWu)

            Call LLMTensorOps.Accumulate(dXg, dXu)

            Return dXg
        End Function

        ''' <summary>清零本模块全部参数的梯度累加器。</summary>
        Public Sub ZeroGradients()
            _wgOpt.ZeroGrad()
            _wuOpt.ZeroGrad()
            _wdOpt.ZeroGrad()
        End Sub

        ''' <summary>按 AdamW 规则更新本模块参数。</summary>
        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            _wgOpt.MakeTrainingStep(learningRate, [step], Wg)
            _wuOpt.MakeTrainingStep(learningRate, [step], Wu)
            _wdOpt.MakeTrainingStep(learningRate, [step], Wd)
        End Sub

    End Class

End Namespace
