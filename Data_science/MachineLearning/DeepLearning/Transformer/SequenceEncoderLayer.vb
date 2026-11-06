' ---------------------------------------------------------------------------
' SequenceEncoderLayer —— 序列编码器单层（post-LN + 仿射 LayerNorm）
'
' 结构（与 nn.TransformerEncoderLayer(norm_first=False) 一致）：
'     x = norm1(x + dropout(self_attn(x)))
'     x = norm2(x + dropout(ffn(x)))
'
' 与 `EncoderLayer.vb` 的区别：本类走 TensorOpsEx 的仿射 LayerNorm 与全张量 dropout，
' 因而可以承载 `trfm.encoder.layers.N.*` 的官方权重。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace Transformer

    ''' <summary>
    ''' 序列编码器单层：多头自注意力 + 位置前馈网络，两个子层各自「残差 + 仿射 LayerNorm」（post-LN）。
    ''' </summary>
    Public Class SequenceEncoderLayer

        ''' <summary>前向缓存（反向传播需要）。</summary>
        Public Class Cache
            ''' <summary>本层输入。</summary>
            Public Input As Tensor
            ''' <summary>自注意力输出。</summary>
            Public Attention As Tensor
            ''' <summary>dropout 之后的自注意力输出。</summary>
            Public AttentionDropped As Tensor
            ''' <summary>自注意力的 dropout 掩码。</summary>
            Public AttentionMask As Boolean()
            ''' <summary>第一个 AddNorm 的输出。</summary>
            Public Normalized1 As Tensor
            ''' <summary>第一个 LayerNorm 的缓存。</summary>
            Public Norm1Cache As LayerNormAffine.Cache
            ''' <summary>前馈网络的输出。</summary>
            Public FeedForward As Tensor
            ''' <summary>dropout 之后的前馈输出。</summary>
            Public FeedForwardDropped As Tensor
            ''' <summary>前馈的 dropout 掩码。</summary>
            Public FeedForwardMask As Boolean()
            ''' <summary>第二个 LayerNorm 的缓存。</summary>
            Public Norm2Cache As LayerNormAffine.Cache
            ''' <summary>自注意力的前向缓存。</summary>
            Public MhaCache As MultiHeadAttention.Cache
            ''' <summary>前馈网络的前向缓存。</summary>
            Public FfCache As FeedForwardNetwork.Cache
            ''' <summary>本次前向是否应用了 dropout。</summary>
            Public DropoutApplied As Boolean
        End Class

        Private ReadOnly _attention As MultiHeadAttention
        Private ReadOnly _feedForward As FeedForwardNetwork
        Private ReadOnly _norm1 As LayerNormAffine
        Private ReadOnly _norm2 As LayerNormAffine
        Private ReadOnly _dropoutRate As Double

        Private _lastCache As Cache

        ''' <summary>多头自注意力子层。</summary>
        Friend ReadOnly Property Attention As MultiHeadAttention
            Get
                Return _attention
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

        ''' <summary>最近一次前向的缓存。</summary>
        Public ReadOnly Property LastCache As Cache
            Get
                Return _lastCache
            End Get
        End Property

        ''' <summary>
        ''' 创建编码器单层。
        ''' </summary>
        ''' <param name="embeddingSize">模型宽度。</param>
        ''' <param name="dk">每个注意力头的 Q/K 维度。</param>
        ''' <param name="dv">每个注意力头的 V 维度。</param>
        ''' <param name="nHeads">注意力头数。</param>
        ''' <param name="dff">前馈网络隐藏宽度。</param>
        ''' <param name="dropout">dropout 概率。</param>
        Public Sub New(embeddingSize As Integer, dk As Integer, dv As Integer, nHeads As Integer,
                       dff As Integer, Optional dropout As Double = 0.1)
            _dropoutRate = dropout
            _attention = New MultiHeadAttention(dk, dv, nHeads, embeddingSize, False, True)
            _feedForward = New FeedForwardNetwork(dff, embeddingSize, FeedForwardActivation.ReLU)
            _norm1 = New LayerNormAffine(embeddingSize)
            _norm2 = New LayerNormAffine(embeddingSize)
        End Sub

        ''' <summary>执行编码：自注意力 → AddNorm → 前馈 → AddNorm。</summary>
        ''' <param name="x">输入，形状 <c>[batch, time, H]</c>。</param>
        ''' <param name="isTraining">是否应用 dropout。</param>
        Public Function Forward(x As Tensor, isTraining As Boolean) As Tensor
            Dim applyDropout As Boolean = isTraining AndAlso _dropoutRate > 0

            Dim attention As Tensor = _attention.Update(x)
            Dim attentionMask As Boolean() = Nothing
            Dim attentionDropped As Tensor = attention

            If applyDropout Then
                attentionDropped = TensorOpsEx.DropoutForward(attention, _dropoutRate, attentionMask)
            End If

            Dim normalized1 As Tensor = _norm1.Forward(TensorOpsEx.AddTensors(x, attentionDropped))

            Dim feedForward As Tensor = _feedForward.FeedForward(normalized1)
            Dim feedForwardMask As Boolean() = Nothing
            Dim feedForwardDropped As Tensor = feedForward

            If applyDropout Then
                feedForwardDropped = TensorOpsEx.DropoutForward(feedForward, _dropoutRate, feedForwardMask)
            End If

            Dim output As Tensor = _norm2.Forward(TensorOpsEx.AddTensors(normalized1, feedForwardDropped))

            _lastCache = New Cache With {
                .Input = x,
                .Attention = attention,
                .AttentionDropped = attentionDropped,
                .AttentionMask = attentionMask,
                .Normalized1 = normalized1,
                .Norm1Cache = _norm1.LastCache,
                .FeedForward = feedForward,
                .FeedForwardDropped = feedForwardDropped,
                .FeedForwardMask = feedForwardMask,
                .Norm2Cache = _norm2.LastCache,
                .MhaCache = _attention.LastCache,
                .FfCache = _feedForward.LastCache,
                .DropoutApplied = applyDropout
            }

            Return output
        End Function

        ''' <summary>反向传播，返回对输入的梯度。</summary>
        ''' <param name="forwardCache">该次前向的缓存。</param>
        ''' <param name="dOut">对输出的梯度。</param>
        Public Function Backward(forwardCache As Cache, dOut As Tensor) As Tensor
            Dim cache As Cache = If(forwardCache, _lastCache)

            If cache Is Nothing Then Throw New InvalidOperationException("必须先执行前向传播才能反向传播")

            ' 第二个 AddNorm：output = norm2(normalized1 + feedForwardDropped)
            Dim dSum2 As Tensor = _norm2.Backward(cache.Norm2Cache, dOut)
            Dim dFeedForward As Tensor = If(cache.DropoutApplied,
                                            TensorOpsEx.DropoutBackward(dSum2, cache.FeedForwardMask, _dropoutRate),
                                            dSum2)

            Dim dNormalized1FromFf As Tensor = _feedForward.Backward(cache.FfCache, dFeedForward)
            Dim dNormalized1 As Tensor = TensorOpsEx.AddTensors(dSum2, dNormalized1FromFf)

            ' 第一个 AddNorm：normalized1 = norm1(x + attentionDropped)
            Dim dSum1 As Tensor = _norm1.Backward(cache.Norm1Cache, dNormalized1)
            Dim dAttention As Tensor = If(cache.DropoutApplied,
                                          TensorOpsEx.DropoutBackward(dSum1, cache.AttentionMask, _dropoutRate),
                                          dSum1)

            Dim ignore As Tensor = Nothing
            Dim dInputFromAttention As Tensor = _attention.Backward(cache.MhaCache, dAttention, ignore)

            Return TensorOpsEx.AddTensors(dSum1, dInputFromAttention)
        End Function

        ''' <summary>清零本层全部参数的梯度累加器。</summary>
        Public Sub ZeroGradients()
            _attention.ZeroGradients()
            _feedForward.ZeroGradients()
            _norm1.ZeroGradients()
            _norm2.ZeroGradients()
        End Sub

        ''' <summary>对本层全部参数执行一次优化器更新。</summary>
        ''' <param name="learningRate">学习率。</param>
        ''' <param name="[step]">训练步序号（从 1 开始）。</param>
        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            _attention.MakeTrainingStep(learningRate, [step])
            _feedForward.MakeTrainingStep(learningRate, [step])
            _norm1.MakeTrainingStep(learningRate, [step])
            _norm2.MakeTrainingStep(learningRate, [step])
        End Sub

    End Class

End Namespace
