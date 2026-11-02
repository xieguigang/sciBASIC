' ---------------------------------------------------------------------------
' SequenceDecoderLayer —— 序列解码器单层（post-LN + 仿射 LayerNorm）
'
' 结构（与 nn.TransformerDecoderLayer(norm_first=False) 一致）：
'     x = norm1(x + dropout(self_attn(x)))
'     x = norm2(x + dropout(cross_attn(x, memory)))
'     x = norm3(x + dropout(ffn(x)))
'
' 自注意力默认**不加因果掩码**：smiles-transformer 的 TrfmSeq2seq 把整条序列同时喂给
' encoder 与 decoder（`self.trfm(embedded, embedded)`，未传 tgt_mask），属于去噪自编码。
' 需要因果掩码时把 causalMask 设为 True。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace Transformer

    ''' <summary>
    ''' 序列解码器单层：自注意力 + 交叉注意力 + 前馈网络，三个子层各自「残差 + 仿射 LayerNorm」。
    ''' </summary>
    Public Class SequenceDecoderLayer

        ''' <summary>前向缓存（反向传播需要）。</summary>
        Public Class Cache
            ''' <summary>本层输入（解码侧）。</summary>
            Public Input As Tensor
            ''' <summary>自注意力输出。</summary>
            Public SelfAttention As Tensor
            ''' <summary>dropout 之后的自注意力输出。</summary>
            Public SelfAttentionDropped As Tensor
            ''' <summary>自注意力的 dropout 掩码。</summary>
            Public SelfAttentionMask As Boolean()
            ''' <summary>第一个 AddNorm 的输出。</summary>
            Public Normalized1 As Tensor
            ''' <summary>第一个 LayerNorm 的缓存。</summary>
            Public Norm1Cache As LayerNormAffine.Cache
            ''' <summary>交叉注意力输出。</summary>
            Public CrossAttention As Tensor
            ''' <summary>dropout 之后的交叉注意力输出。</summary>
            Public CrossAttentionDropped As Tensor
            ''' <summary>交叉注意力的 dropout 掩码。</summary>
            Public CrossAttentionMask As Boolean()
            ''' <summary>第二个 AddNorm 的输出。</summary>
            Public Normalized2 As Tensor
            ''' <summary>第二个 LayerNorm 的缓存。</summary>
            Public Norm2Cache As LayerNormAffine.Cache
            ''' <summary>前馈网络的输出。</summary>
            Public FeedForward As Tensor
            ''' <summary>dropout 之后的前馈输出。</summary>
            Public FeedForwardDropped As Tensor
            ''' <summary>前馈的 dropout 掩码。</summary>
            Public FeedForwardMask As Boolean()
            ''' <summary>第三个 LayerNorm 的缓存。</summary>
            Public Norm3Cache As LayerNormAffine.Cache
            ''' <summary>自注意力的前向缓存。</summary>
            Public SelfCache As MultiHeadAttention.Cache
            ''' <summary>交叉注意力的前向缓存。</summary>
            Public CrossCache As MultiHeadAttention.Cache
            ''' <summary>前馈网络的前向缓存。</summary>
            Public FfCache As FeedForwardNetwork.Cache
            ''' <summary>本次前向是否应用了 dropout。</summary>
            Public DropoutApplied As Boolean
        End Class

        Private ReadOnly _selfAttention As MultiHeadAttention
        Private ReadOnly _crossAttention As MultiHeadAttention
        Private ReadOnly _feedForward As FeedForwardNetwork
        Private ReadOnly _norm1 As LayerNormAffine
        Private ReadOnly _norm2 As LayerNormAffine
        Private ReadOnly _norm3 As LayerNormAffine
        Private ReadOnly _dropoutRate As Double

        Private _lastCache As Cache

        ''' <summary>多头自注意力子层。</summary>
        Friend ReadOnly Property SelfAttentionLayer As MultiHeadAttention
            Get
                Return _selfAttention
            End Get
        End Property

        ''' <summary>多头交叉注意力子层。</summary>
        Friend ReadOnly Property CrossAttentionLayer As MultiHeadAttention
            Get
                Return _crossAttention
            End Get
        End Property

        ''' <summary>位置前馈子层。</summary>
        Friend ReadOnly Property FeedForwardLayer As FeedForwardNetwork
            Get
                Return _feedForward
            End Get
        End Property

        ''' <summary>第一个 LayerNorm。</summary>
        Friend ReadOnly Property Norm1 As LayerNormAffine
            Get
                Return _norm1
            End Get
        End Property

        ''' <summary>第二个 LayerNorm。</summary>
        Friend ReadOnly Property Norm2 As LayerNormAffine
            Get
                Return _norm2
            End Get
        End Property

        ''' <summary>第三个 LayerNorm。</summary>
        Friend ReadOnly Property Norm3 As LayerNormAffine
            Get
                Return _norm3
            End Get
        End Property

        ''' <summary>最近一次前向的缓存。</summary>
        Public ReadOnly Property LastCache As Cache
            Get
                Return _lastCache
            End Get
        End Property

        ''' <summary>本层是否对自注意力施加因果掩码。</summary>
        Public ReadOnly Property CausalMask As Boolean

        ''' <summary>
        ''' 创建解码器单层。
        ''' </summary>
        ''' <param name="embeddingSize">模型宽度。</param>
        ''' <param name="dk">每个注意力头的 Q/K 维度。</param>
        ''' <param name="dv">每个注意力头的 V 维度。</param>
        ''' <param name="nHeads">注意力头数。</param>
        ''' <param name="dff">前馈网络隐藏宽度。</param>
        ''' <param name="dropout">dropout 概率。</param>
        ''' <param name="causalMask">自注意力是否使用因果掩码（默认 <c>False</c>，与 TrfmSeq2seq 一致）。</param>
        Public Sub New(embeddingSize As Integer, dk As Integer, dv As Integer, nHeads As Integer,
                       dff As Integer, Optional dropout As Double = 0.1, Optional causalMask As Boolean = False)
            _dropoutRate = dropout
            Me.CausalMask = causalMask
            _selfAttention = New MultiHeadAttention(dk, dv, nHeads, embeddingSize, causalMask, True)
            _crossAttention = New MultiHeadAttention(dk, dv, nHeads, embeddingSize, False, True)
            _feedForward = New FeedForwardNetwork(dff, embeddingSize, FeedForwardActivation.ReLU)
            _norm1 = New LayerNormAffine(embeddingSize)
            _norm2 = New LayerNormAffine(embeddingSize)
            _norm3 = New LayerNormAffine(embeddingSize)
        End Sub

        ''' <summary>执行解码：自注意力 → 交叉注意力 → 前馈。</summary>
        ''' <param name="x">解码侧输入，形状 <c>[batch, time, H]</c>。</param>
        ''' <param name="encoderOutput">编码器输出（交叉注意力的 key/value）。</param>
        ''' <param name="isTraining">是否应用 dropout。</param>
        Public Function Forward(x As Tensor, encoderOutput As Tensor, isTraining As Boolean) As Tensor
            Dim applyDropout As Boolean = isTraining AndAlso _dropoutRate > 0

            Dim selfAttention As Tensor = _selfAttention.Update(x)
            Dim selfMask As Boolean() = Nothing
            Dim selfDropped As Tensor = selfAttention

            If applyDropout Then
                selfDropped = TensorOpsEx.DropoutForward(selfAttention, _dropoutRate, selfMask)
            End If

            Dim normalized1 As Tensor = _norm1.Forward(TensorOpsEx.AddTensors(x, selfDropped))

            Dim crossAttention As Tensor = _crossAttention.Update(encoderOutput, normalized1)
            Dim crossMask As Boolean() = Nothing
            Dim crossDropped As Tensor = crossAttention

            If applyDropout Then
                crossDropped = TensorOpsEx.DropoutForward(crossAttention, _dropoutRate, crossMask)
            End If

            Dim normalized2 As Tensor = _norm2.Forward(TensorOpsEx.AddTensors(normalized1, crossDropped))

            Dim feedForward As Tensor = _feedForward.FeedForward(normalized2)
            Dim ffMask As Boolean() = Nothing
            Dim feedForwardDropped As Tensor = feedForward

            If applyDropout Then
                feedForwardDropped = TensorOpsEx.DropoutForward(feedForward, _dropoutRate, ffMask)
            End If

            Dim output As Tensor = _norm3.Forward(TensorOpsEx.AddTensors(normalized2, feedForwardDropped))

            _lastCache = New Cache With {
                .Input = x,
                .SelfAttention = selfAttention,
                .SelfAttentionDropped = selfDropped,
                .SelfAttentionMask = selfMask,
                .Normalized1 = normalized1,
                .Norm1Cache = _norm1.LastCache,
                .CrossAttention = crossAttention,
                .CrossAttentionDropped = crossDropped,
                .CrossAttentionMask = crossMask,
                .Normalized2 = normalized2,
                .Norm2Cache = _norm2.LastCache,
                .FeedForward = feedForward,
                .FeedForwardDropped = feedForwardDropped,
                .FeedForwardMask = ffMask,
                .Norm3Cache = _norm3.LastCache,
                .SelfCache = _selfAttention.LastCache,
                .CrossCache = _crossAttention.LastCache,
                .FfCache = _feedForward.LastCache,
                .DropoutApplied = applyDropout
            }

            Return output
        End Function

        ''' <summary>
        ''' 反向传播，返回对解码侧输入的梯度，并输出对编码器输出的梯度。
        ''' </summary>
        ''' <param name="forwardCache">该次前向的缓存。</param>
        ''' <param name="dOut">对输出的梯度。</param>
        ''' <param name="dEncoderOutput">累加器：对编码器输出的梯度（交叉注意力部分）。</param>
        Public Function Backward(forwardCache As Cache, dOut As Tensor,
                                 ByRef dEncoderOutput As Tensor) As Tensor
            Dim cache As Cache = If(forwardCache, _lastCache)

            If cache Is Nothing Then Throw New InvalidOperationException("必须先执行前向传播才能反向传播")

            ' 第三个 AddNorm
            Dim dSum3 As Tensor = _norm3.Backward(cache.Norm3Cache, dOut)
            Dim dFeedForward As Tensor = If(cache.DropoutApplied,
                                            TensorOpsEx.DropoutBackward(dSum3, cache.FeedForwardMask, _dropoutRate),
                                            dSum3)
            Dim dNormalized2FromFf As Tensor = _feedForward.Backward(cache.FfCache, dFeedForward)
            Dim dNormalized2 As Tensor = TensorOpsEx.AddTensors(dSum3, dNormalized2FromFf)

            ' 第二个 AddNorm
            Dim dSum2 As Tensor = _norm2.Backward(cache.Norm2Cache, dNormalized2)
            Dim dCross As Tensor = If(cache.DropoutApplied,
                                      TensorOpsEx.DropoutBackward(dSum2, cache.CrossAttentionMask, _dropoutRate),
                                      dSum2)

            Dim dEncoder As Tensor = Nothing
            Dim dNormalized1FromCross As Tensor = _crossAttention.Backward(cache.CrossCache, dCross, dEncoder)

            ' 多层解码器都会向编码器回传梯度，必须累加而不是覆盖
            If dEncoder IsNot Nothing Then
                If dEncoderOutput Is Nothing Then
                    dEncoderOutput = dEncoder
                Else
                    Call TensorOps.Accumulate(dEncoderOutput, dEncoder)
                End If
            End If

            Dim dNormalized1 As Tensor = TensorOpsEx.AddTensors(dSum2, dNormalized1FromCross)

            ' 第一个 AddNorm
            Dim dSum1 As Tensor = _norm1.Backward(cache.Norm1Cache, dNormalized1)
            Dim dSelf As Tensor = If(cache.DropoutApplied,
                                     TensorOpsEx.DropoutBackward(dSum1, cache.SelfAttentionMask, _dropoutRate),
                                     dSum1)

            Dim ignore As Tensor = Nothing
            Dim dInputFromSelf As Tensor = _selfAttention.Backward(cache.SelfCache, dSelf, ignore)

            Return TensorOpsEx.AddTensors(dSum1, dInputFromSelf)
        End Function

        ''' <summary>清零本层全部参数的梯度累加器。</summary>
        Public Sub ZeroGradients()
            _selfAttention.ZeroGradients()
            _crossAttention.ZeroGradients()
            _feedForward.ZeroGradients()
            _norm1.ZeroGradients()
            _norm2.ZeroGradients()
            _norm3.ZeroGradients()
        End Sub

        ''' <summary>对本层全部参数执行一次优化器更新。</summary>
        ''' <param name="learningRate">学习率。</param>
        ''' <param name="[step]">训练步序号（从 1 开始）。</param>
        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            _selfAttention.MakeTrainingStep(learningRate, [step])
            _crossAttention.MakeTrainingStep(learningRate, [step])
            _feedForward.MakeTrainingStep(learningRate, [step])
            _norm1.MakeTrainingStep(learningRate, [step])
            _norm2.MakeTrainingStep(learningRate, [step])
            _norm3.MakeTrainingStep(learningRate, [step])
        End Sub

    End Class

End Namespace
