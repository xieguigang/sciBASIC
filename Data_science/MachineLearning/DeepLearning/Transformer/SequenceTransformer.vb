' ---------------------------------------------------------------------------
' SequenceTransformer —— smiles-transformer 的 TrfmSeq2seq 等价实现
'
' 对应 Python：
'     embedded = self.pe(self.embed(src))          # (T,B,H) → 这里用 (B,T,H)
'     hidden   = self.trfm(embedded, embedded)     # 编码器/解码器共用同一份输入，且**无因果掩码**
'     out      = log_softmax(self.out(hidden), 2)  # 逐位置线性投影到词表
'
' 与 `TransformerModel.vb` 的差别（后者是英西翻译 demo，不能承载该模型的权重）：
'   * 整序列并行前向（不是逐词解码）；
'   * 解码器自注意力默认不加因果掩码；
'   * 输出层是逐位置 Linear(H,V)，不是 [T*H, V] 的压平投影；
'   * 仿射 LayerNorm、投影 bias、标准位置编码；
'   * 提供 encode()：mean/max/first + 倒数第二层 first → [B, 4H] 指纹。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Namespace Transformer

    ''' <summary>
    ''' 自编码式序列 Transformer：编码器-解码器共享同一份输入，逐位置输出词表分布。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 前向输入 <c>ids(batch, time)</c>；所有中间张量形状为 <c>[batch, time, hidden]</c>。
    ''' 损失为 <c>nll_loss(reduction='mean', ignore_index=pad)</c>，分母是非 pad 元素个数。
    ''' </para>
    ''' <para>
    ''' 权重初始化按"能训练"而非"与 torch 逐位一致"设计：加载官方 <c>state_dict</c> 后
    ''' 初始化值会被整体覆盖，从头训练时则只影响随机性。
    ''' </para>
    ''' </remarks>
    Public Class SequenceTransformer

        ''' <summary>一次前向的缓存集合（反向传播需要）。</summary>
        Private Class ForwardCache
            Public Embedded As Tensor
            Public EncoderCaches As New List(Of SequenceEncoderLayer.Cache)
            Public EncoderNormCache As LayerNormAffine.Cache
            Public DecoderCaches As New List(Of SequenceDecoderLayer.Cache)
            Public DecoderNormCache As LayerNormAffine.Cache
            Public DecoderNormInput As Tensor
            Public LogProbs As Tensor
        End Class

        Private ReadOnly _vocabSize As Integer
        Private ReadOnly _hiddenSize As Integer
        Private ReadOnly _nLayers As Integer
        Private ReadOnly _nHeads As Integer
        Private ReadOnly _dff As Integer
        Private ReadOnly _sequenceLength As Integer
        Private ReadOnly _dropout As Double
        Private ReadOnly _headDim As Integer

        Private ReadOnly _embedding As TokenEmbedding
        Private ReadOnly _encoderLayers As New List(Of SequenceEncoderLayer)
        Private ReadOnly _decoderLayers As New List(Of SequenceDecoderLayer)
        Private ReadOnly _encoderNorm As LayerNormAffine
        Private ReadOnly _decoderNorm As LayerNormAffine

        Private ReadOnly _outWeights As Tensor
        Private ReadOnly _outBias As Tensor
        Private ReadOnly _outWeightsOptimizer As Optimizer
        Private ReadOnly _outBiasOptimizer As Optimizer

        Private _lastForward As ForwardCache

        ''' <summary>词表大小（= 输出维度）。</summary>
        Public ReadOnly Property VocabSize As Integer
            Get
                Return _vocabSize
            End Get
        End Property

        ''' <summary>模型宽度（embedding / hidden 维度）。</summary>
        Public ReadOnly Property HiddenSize As Integer
            Get
                Return _hiddenSize
            End Get
        End Property

        ''' <summary>编码器/解码器的层数。</summary>
        Public ReadOnly Property NLayers As Integer
            Get
                Return _nLayers
            End Get
        End Property

        ''' <summary>注意力头数。</summary>
        Public ReadOnly Property NHeads As Integer
            Get
                Return _nHeads
            End Get
        End Property

        ''' <summary>前馈网络隐藏宽度。</summary>
        Public ReadOnly Property Dff As Integer
            Get
                Return _dff
            End Get
        End Property

        ''' <summary>固定序列长度（位置编码容量）。</summary>
        Public ReadOnly Property SequenceLength As Integer
            Get
                Return _sequenceLength
            End Get
        End Property

        ''' <summary>dropout 概率。</summary>
        Public ReadOnly Property DropoutRate As Double
            Get
                Return _dropout
            End Get
        End Property

        ''' <summary>每个注意力头的 Q/K/V 维度。</summary>
        Public ReadOnly Property HeadDim As Integer
            Get
                Return _headDim
            End Get
        End Property

        ''' <summary>词嵌入层。</summary>
        Friend ReadOnly Property EmbeddingLayer As TokenEmbedding
            Get
                Return _embedding
            End Get
        End Property

        ''' <summary>编码器各层。</summary>
        Friend ReadOnly Property EncoderLayers As IReadOnlyList(Of SequenceEncoderLayer)
            Get
                Return _encoderLayers
            End Get
        End Property

        ''' <summary>解码器各层。</summary>
        Friend ReadOnly Property DecoderLayers As IReadOnlyList(Of SequenceDecoderLayer)
            Get
                Return _decoderLayers
            End Get
        End Property

        ''' <summary>编码器末端的 LayerNorm。</summary>
        Friend ReadOnly Property EncoderNorm As LayerNormAffine
            Get
                Return _encoderNorm
            End Get
        End Property

        ''' <summary>解码器末端的 LayerNorm。</summary>
        Friend ReadOnly Property DecoderNorm As LayerNormAffine
            Get
                Return _decoderNorm
            End Get
        End Property

        ''' <summary>输出层权重，形状 <c>[hiddenSize, vocabSize]</c>。</summary>
        Friend ReadOnly Property OutputWeights As Tensor
            Get
                Return _outWeights
            End Get
        End Property

        ''' <summary>输出层偏置，形状 <c>[vocabSize]</c>。</summary>
        Friend ReadOnly Property OutputBias As Tensor
            Get
                Return _outBias
            End Get
        End Property

        ''' <summary>
        ''' 创建序列 Transformer。
        ''' </summary>
        ''' <param name="vocabSize">词表大小。</param>
        ''' <param name="hiddenSize">模型宽度，默认 256（论文取值）。</param>
        ''' <param name="nLayers">编码器/解码器层数，默认 4（论文取值）。</param>
        ''' <param name="nHeads">注意力头数，默认 4（论文取值）。</param>
        ''' <param name="dff">前馈网络隐藏宽度，默认 256（论文取值）。</param>
        ''' <param name="seqLen">固定序列长度，默认 220（论文取值）。</param>
        ''' <param name="dropout">dropout 概率。</param>
        ''' <param name="causalMask">解码器自注意力是否使用因果掩码，默认 <c>False</c>。</param>
        ''' <param name="seed">权重初始化的随机种子。</param>
        ''' <exception cref="ArgumentException">hiddenSize 不能被 nHeads 整除。</exception>
        Public Sub New(vocabSize As Integer,
                       Optional hiddenSize As Integer = 256,
                       Optional nLayers As Integer = 4,
                       Optional nHeads As Integer = 4,
                       Optional dff As Integer = 256,
                       Optional seqLen As Integer = 220,
                       Optional dropout As Double = 0.1,
                       Optional causalMask As Boolean = False,
                       Optional seed As Integer? = Nothing)
            If vocabSize <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(vocabSize))
            If nHeads <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(nHeads))
            If hiddenSize Mod nHeads <> 0 Then
                Throw New ArgumentException($"hiddenSize({hiddenSize}) 必须能被 nHeads({nHeads}) 整除")
            End If

            _vocabSize = vocabSize
            _hiddenSize = hiddenSize
            _nLayers = nLayers
            _nHeads = nHeads
            _dff = dff
            _sequenceLength = seqLen
            _dropout = dropout
            _headDim = hiddenSize \ nHeads

            _embedding = New TokenEmbedding(vocabSize, hiddenSize, seqLen, dropout, seed)
            _encoderNorm = New LayerNormAffine(hiddenSize)
            _decoderNorm = New LayerNormAffine(hiddenSize)

            For i As Integer = 0 To nLayers - 1
                _encoderLayers.Add(New SequenceEncoderLayer(hiddenSize, _headDim, _headDim, nHeads, dff, dropout))
                _decoderLayers.Add(New SequenceDecoderLayer(hiddenSize, _headDim, _headDim, nHeads, dff, dropout, causalMask))
            Next

            ' nn.Linear 的默认初始化：U(-1/sqrt(fan_in), 1/sqrt(fan_in))
            Dim bound As Single = CSng(1.0 / std.Sqrt(hiddenSize))

            _outWeights = Tensor.Random(New Integer() {hiddenSize, vocabSize}, -bound, bound, seed)
            _outBias = Tensor.Random(New Integer() {vocabSize}, -bound, bound, seed)
            _outWeightsOptimizer = New Optimizer(_outWeights)
            _outBiasOptimizer = New Optimizer(_outBias)
        End Sub

        ''' <summary>
        ''' 前向传播（整序列并行），返回 log-softmax 后的 <c>[batch, time, vocab]</c>。
        ''' </summary>
        ''' <param name="ids">token 下标，形状 <c>[batch, time]</c>。</param>
        ''' <param name="isTraining">是否应用 dropout 并记录反向缓存。</param>
        Public Function Forward(ids As Integer(,), Optional isTraining As Boolean = False) As Tensor
            Dim cache As New ForwardCache()
            Dim embedded As Tensor = _embedding.Embed(ids, isTraining)

            cache.Embedded = embedded

            ' 编码器
            Dim encoded As Tensor = embedded

            For Each layer As SequenceEncoderLayer In _encoderLayers
                encoded = layer.Forward(encoded, isTraining)
                cache.EncoderCaches.Add(layer.LastCache)
            Next

            encoded = _encoderNorm.Forward(encoded)
            cache.EncoderNormCache = _encoderNorm.LastCache

            ' 解码器（输入与编码器相同，且默认不加因果掩码）
            Dim decoded As Tensor = embedded

            For Each layer As SequenceDecoderLayer In _decoderLayers
                decoded = layer.Forward(decoded, encoded, isTraining)
                cache.DecoderCaches.Add(layer.LastCache)
            Next

            cache.DecoderNormInput = decoded
            decoded = _decoderNorm.Forward(decoded)
            cache.DecoderNormCache = _decoderNorm.LastCache

            Dim logits As Tensor = TensorOpsEx.LinearForward(decoded, _outWeights, _outBias)
            Dim logProbs As Tensor = TensorOpsEx.LogSoftmaxLastDim(logits)

            cache.LogProbs = logProbs
            _lastForward = cache

            Return logProbs
        End Function

        ''' <summary>
        ''' 推理（关闭 dropout），返回 <c>[batch, time, vocab]</c> 的 log 概率。
        ''' </summary>
        ''' <param name="ids">token 下标，形状 <c>[batch, time]</c>。</param>
        Public Function Predict(ids As Integer(,)) As Tensor
            Return Forward(ids, False)
        End Function

        ''' <summary>
        ''' 一次训练步：前向 → NLL → 反向 → Adam 更新。
        ''' </summary>
        ''' <param name="ids">token 下标，形状 <c>[batch, time]</c>；<c>&lt;pad&gt;</c> 位置不参与损失。</param>
        ''' <param name="learningRate">学习率。</param>
        ''' <param name="[step]">训练步序号（从 1 开始，用于 Adam 偏置校正）。</param>
        ''' <param name="ignoreIndex">被忽略的目标下标，默认 0（<c>&lt;pad&gt;</c>）。</param>
        ''' <returns>该批次的平均负对数似然。</returns>
        Public Function TrainStep(ids As Integer(,), learningRate As Double, [step] As Integer,
                                  Optional ignoreIndex As Integer = 0) As Double
            Dim logProbs As Tensor = Forward(ids, True)
            Dim cache As ForwardCache = _lastForward
            Dim batch As Integer = ids.GetLength(0)
            Dim time As Integer = ids.GetLength(1)
            Dim targets As Integer() = flattenTargets(ids)

            Dim dLogits As Tensor = Nothing
            Dim loss As Double = TensorOpsEx.NllLoss(logProbs, targets, ignoreIndex, dLogits)

            ' 输出层
            Dim dDecoded As Tensor = Nothing
            Dim dWo As Tensor = Nothing
            Dim dBo As Tensor = Nothing
            dDecoded = TensorOpsEx.LinearBackward(dLogits, cache.DecoderNormInput, _outWeights, dWo, dBo)

            Call TensorOps.Accumulate(_outWeightsOptimizer.Gradient, dWo)
            Call TensorOps.Accumulate(_outBiasOptimizer.Gradient, dBo)

            ' 解码器
            dDecoded = _decoderNorm.Backward(cache.DecoderNormCache, dDecoded)

            Dim dEncoded As Tensor = Nothing

            For i As Integer = _decoderLayers.Count - 1 To 0 Step -1
                dDecoded = _decoderLayers(i).Backward(cache.DecoderCaches(i), dDecoded, dEncoded)
            Next

            ' 编码器
            Dim dEncodedNorm As Tensor = _encoderNorm.Backward(cache.EncoderNormCache, dEncoded)

            For i As Integer = _encoderLayers.Count - 1 To 0 Step -1
                dEncodedNorm = _encoderLayers(i).Backward(cache.EncoderCaches(i), dEncodedNorm)
            Next

            ' 两条分支共用同一份 embedding 输出
            Dim dEmbedded As Tensor = TensorOpsEx.AddTensors(dEncodedNorm, dDecoded)

            _embedding.Backward(dEmbedded)

            ' 参数更新
            _embedding.MakeTrainingStep(learningRate, [step])
            _encoderNorm.MakeTrainingStep(learningRate, [step])
            _decoderNorm.MakeTrainingStep(learningRate, [step])
            _outWeightsOptimizer.MakeTrainingStep(learningRate, [step], _outWeights)
            _outBiasOptimizer.MakeTrainingStep(learningRate, [step], _outBias)

            For Each layer As SequenceEncoderLayer In _encoderLayers
                layer.MakeTrainingStep(learningRate, [step])
            Next

            For Each layer As SequenceDecoderLayer In _decoderLayers
                layer.MakeTrainingStep(learningRate, [step])
            Next

            Return loss
        End Function

        ''' <summary>
        ''' 抽取分子指纹：与 Python 版 <c>_encode</c> 一致，返回 <c>[batch, 4·hidden]</c>。
        ''' </summary>
        ''' <param name="ids">token 下标，形状 <c>[batch, time]</c>。</param>
        ''' <param name="chunkSize">分块大小（Python 版为 100），避免一次分配过大中间张量。</param>
        ''' <remarks>
        ''' 四个拼接块依次为：编码器输出的时间维均值、时间维最大值、第一个位置、
        ''' 以及**倒数第二层**输出的第一个位置。
        ''' 该方法会覆盖各层的前向缓存，因此不要与 <see cref="TrainStep"/> 交错调用。
        ''' </remarks>
        Public Function Encode(ids As Integer(,), Optional chunkSize As Integer = 100) As Double(,)
            If ids Is Nothing Then Throw New ArgumentNullException(NameOf(ids))

            Dim batch As Integer = ids.GetLength(0)
            Dim effectiveChunk As Integer = If(chunkSize <= 0, batch, chunkSize)

            If batch <= effectiveChunk Then Return encodeChunk(ids)

            Dim parts As New List(Of Double(,))
            Dim start As Integer = 0

            While start < batch
                Dim count As Integer = std.Min(effectiveChunk, batch - start)

                parts.Add(encodeChunk(sliceBatch(ids, start, count)))
                start += count
            End While

            Return concatRows(parts, batch, _hiddenSize * 4)
        End Function

        ''' <summary>清零全部参数的梯度累加器。</summary>
        Public Sub ZeroGradients()
            _embedding.ZeroGradients()
            _encoderNorm.ZeroGradients()
            _decoderNorm.ZeroGradients()
            _outWeightsOptimizer.ZeroGrad()
            _outBiasOptimizer.ZeroGrad()

            For Each layer As SequenceEncoderLayer In _encoderLayers
                layer.ZeroGradients()
            Next

            For Each layer As SequenceDecoderLayer In _decoderLayers
                layer.ZeroGradients()
            Next
        End Sub

        ''' <summary>参数总数（诊断用）。</summary>
        Public Function ParameterCount() As Long
            Dim total As Long = _embedding.Weights.Length + _outWeights.Length + _outBias.Length

            For Each layer As SequenceEncoderLayer In _encoderLayers
                total += countLayerParameters(layer)
            Next

            For Each layer As SequenceDecoderLayer In _decoderLayers
                total += countLayerParameters(layer)
            Next

            Return total
        End Function

        Private Shared Function countLayerParameters(layer As SequenceEncoderLayer) As Long
            Dim total As Long = layer.Attention.OutputWeights.Length + layer.Attention.OutputBias.Length +
                                layer.FeedForwardLayer.Weights1.Length + layer.FeedForwardLayer.Bias1.Length +
                                layer.FeedForwardLayer.Weights2.Length + layer.FeedForwardLayer.Bias2.Length +
                                layer.Norm1.Gamma.Length * 2 + layer.Norm2.Gamma.Length * 2

            For h As Integer = 0 To layer.Attention.QueryWeights.Length - 1
                total += layer.Attention.QueryWeights(h).Length +
                         layer.Attention.KeyWeights(h).Length +
                         layer.Attention.ValueWeights(h).Length +
                         layer.Attention.QueryBias(h).Length +
                         layer.Attention.KeyBias(h).Length +
                         layer.Attention.ValueBias(h).Length
            Next

            Return total
        End Function

        Private Shared Function countLayerParameters(layer As SequenceDecoderLayer) As Long
            Dim total As Long = countAttention(layer.SelfAttentionLayer) + countAttention(layer.CrossAttentionLayer) +
                                layer.FeedForwardLayer.Weights1.Length + layer.FeedForwardLayer.Bias1.Length +
                                layer.FeedForwardLayer.Weights2.Length + layer.FeedForwardLayer.Bias2.Length +
                                layer.Norm1.Gamma.Length * 2 + layer.Norm2.Gamma.Length * 2 +
                                layer.Norm3.Gamma.Length * 2

            Return total
        End Function

        Private Shared Function countAttention(attention As MultiHeadAttention) As Long
            Dim total As Long = attention.OutputWeights.Length + attention.OutputBias.Length

            For h As Integer = 0 To attention.QueryWeights.Length - 1
                total += attention.QueryWeights(h).Length +
                         attention.KeyWeights(h).Length +
                         attention.ValueWeights(h).Length +
                         attention.QueryBias(h).Length +
                         attention.KeyBias(h).Length +
                         attention.ValueBias(h).Length
            Next

            Return total
        End Function

        ''' <summary>单块编码：返回 <c>[batch, 4·hidden]</c> 指纹。</summary>
        Private Function encodeChunk(ids As Integer(,)) As Double(,)
            Dim batch As Integer = ids.GetLength(0)
            Dim time As Integer = ids.GetLength(1)
            Dim h As Integer = _hiddenSize
            Dim encoded As Tensor = _embedding.Embed(ids, False)
            Dim layerOutputs As New List(Of Tensor)

            For Each layer As SequenceEncoderLayer In _encoderLayers
                encoded = layer.Forward(encoded, False)
                layerOutputs.Add(encoded)
            Next

            ' 倒数第二层的输出（等价于最后一层的输入）
            Dim penultimate As Tensor = If(layerOutputs.Count >= 2,
                                           layerOutputs(layerOutputs.Count - 2),
                                           layerOutputs(0))
            Dim output As Tensor = _encoderNorm.Forward(encoded)
            Dim result(batch - 1, h * 4 - 1) As Double
            Dim outData = output.Data
            Dim penData = penultimate.Data

            For b As Integer = 0 To batch - 1
                Dim baseOffset As Integer = b * time * h

                For i As Integer = 0 To h - 1
                    Dim sum As Double = 0.0
                    Dim maxVal As Double = Double.NegativeInfinity

                    For t As Integer = 0 To time - 1
                        Dim v As Double = outData(baseOffset + t * h + i)

                        sum += v
                        If v > maxVal Then maxVal = v
                    Next

                    result(b, i) = sum / time
                    result(b, h + i) = maxVal
                    result(b, 2 * h + i) = outData(baseOffset + i)
                    result(b, 3 * h + i) = penData(baseOffset + i)
                Next
            Next

            Return result
        End Function

        ''' <summary>把 <c>[batch, time]</c> 的目标按行优先展开成一维（与 <c>[B,T,V]</c> 的展平顺序一致）。</summary>
        Private Shared Function flattenTargets(ids As Integer(,)) As Integer()
            Dim batch As Integer = ids.GetLength(0)
            Dim time As Integer = ids.GetLength(1)
            Dim targets(batch * time - 1) As Integer

            For b As Integer = 0 To batch - 1
                For t As Integer = 0 To time - 1
                    targets(b * time + t) = ids(b, t)
                Next
            Next

            Return targets
        End Function

        ''' <summary>截取 <c>[batch, time]</c> 的一段行。</summary>
        Private Shared Function sliceBatch(ids As Integer(,), start As Integer, count As Integer) As Integer(,)
            Dim time As Integer = ids.GetLength(1)
            Dim part(count - 1, time - 1) As Integer

            For b As Integer = 0 To count - 1
                For t As Integer = 0 To time - 1
                    part(b, t) = ids(start + b, t)
                Next
            Next

            Return part
        End Function

        ''' <summary>按行拼接多个 <c>[rows, cols]</c> 矩阵。</summary>
        Private Shared Function concatRows(parts As List(Of Double(,)), totalRows As Integer, cols As Integer) As Double(,)
            Dim result(totalRows - 1, cols - 1) As Double
            Dim offset As Integer = 0

            For Each part As Double(,) In parts
                Dim rows As Integer = part.GetLength(0)

                For r As Integer = 0 To rows - 1
                    For c As Integer = 0 To cols - 1
                        result(offset + r, c) = part(r, c)
                    Next
                Next

                offset += rows
            Next

            Return result
        End Function

    End Class

End Namespace
