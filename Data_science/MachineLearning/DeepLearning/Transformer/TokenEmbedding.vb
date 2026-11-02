' ---------------------------------------------------------------------------
' TokenEmbedding —— 按 token 下标查表的词嵌入层 + 标准正弦位置编码
'
' 与 `Transformer\Embedding.vb` 的区别（后者服务于英西翻译 demo，无法用于 SMILES）：
'   * 以整数下标为键（字符串键 + ToLower 会把 SMILES 的芳香 `c` 与脂肪 `C` 合并）；
'   * 支持 `<pad>/<unk>` 等特殊 token，词表由外部提供；
'   * 位置编码使用标准公式 sin/cos(pos / 10000^(2i/d))。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Namespace Transformer

    ''' <summary>
    ''' 词嵌入 + 正弦位置编码。
    ''' </summary>
    ''' <remarks>
    ''' 前向：<c>y[b,t,:] = W[ids[b,t],:] + pe[t,:]</c>，训练时再叠加 dropout。
    ''' 反向：穿过 dropout 后按 <c>(b,t)</c> 把梯度散射累加回 <c>W</c> 的对应行
    ''' （同一个 token 出现多次会自然累加）。
    ''' </remarks>
    Public Class TokenEmbedding

        Private ReadOnly _weights As Tensor
        Private ReadOnly _positional As Tensor
        Private ReadOnly _optimizer As Optimizer
        Private ReadOnly _vocabSize As Integer
        Private ReadOnly _embeddingSize As Integer
        Private ReadOnly _sequenceLength As Integer
        Private ReadOnly _dropoutRate As Double

        Private _lastIds As Integer(,)
        Private _lastMask As Boolean()
        Private _lastRate As Double

        ''' <summary>嵌入矩阵，形状 <c>[vocabSize, embeddingSize]</c>。</summary>
        Public ReadOnly Property Weights As Tensor
            Get
                Return _weights
            End Get
        End Property

        ''' <summary>位置编码缓冲区，形状 <c>[sequenceLength, embeddingSize]</c>（不参与训练）。</summary>
        Public ReadOnly Property Positional As Tensor
            Get
                Return _positional
            End Get
        End Property

        ''' <summary>词表大小。</summary>
        Public ReadOnly Property VocabSize As Integer
            Get
                Return _vocabSize
            End Get
        End Property

        ''' <summary>嵌入维度。</summary>
        Public ReadOnly Property EmbeddingSize As Integer
            Get
                Return _embeddingSize
            End Get
        End Property

        ''' <summary>固定序列长度。</summary>
        Public ReadOnly Property SequenceLength As Integer
            Get
                Return _sequenceLength
            End Get
        End Property

        ''' <summary>
        ''' 创建词嵌入层。
        ''' </summary>
        ''' <param name="vocabSize">词表大小。</param>
        ''' <param name="embeddingSize">嵌入维度（= 模型宽度）。</param>
        ''' <param name="sequenceLength">最大序列长度（决定位置编码表大小）。</param>
        ''' <param name="dropout">dropout 概率（作用在"嵌入 + 位置编码"的输出上）。</param>
        ''' <param name="seed">权重初始化与位置编码的随机种子（仅用于权重初始化）。</param>
        Public Sub New(vocabSize As Integer, embeddingSize As Integer, sequenceLength As Integer,
                       Optional dropout As Double = 0.1,
                       Optional seed As Integer? = Nothing)
            If vocabSize <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(vocabSize))
            If embeddingSize <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(embeddingSize))
            If sequenceLength <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(sequenceLength))

            _vocabSize = vocabSize
            _embeddingSize = embeddingSize
            _sequenceLength = sequenceLength
            _dropoutRate = dropout

            ' nn.Embedding 的默认初始化是 N(0, 1)
            _weights = Tensor.RandomNormal(New Integer() {vocabSize, embeddingSize}, 0.0F, 1.0F, seed)
            _optimizer = New Optimizer(_weights)
            _positional = buildPositionalEncoding(sequenceLength, embeddingSize)
        End Sub

        ''' <summary>
        ''' 前向：查表 + 位置编码（+ 训练期 dropout）。
        ''' </summary>
        ''' <param name="ids">token 下标，形状 <c>[batch, time]</c>。</param>
        ''' <param name="isTraining">是否应用 dropout。</param>
        Public Function Embed(ids As Integer(,), Optional isTraining As Boolean = False) As Tensor
            validateIds(ids)

            Dim batch As Integer = ids.GetLength(0)
            Dim time As Integer = ids.GetLength(1)
            Dim h As Integer = _embeddingSize
            Dim result As New Tensor(batch, time, h)
            Dim dst = result.Data
            Dim w = _weights.Data
            Dim pe = _positional.Data

            For b As Integer = 0 To batch - 1
                For t As Integer = 0 To time - 1
                    Dim id As Integer = ids(b, t)
                    Dim dstBase As Integer = (b * time + t) * h
                    Dim srcBase As Integer = id * h
                    Dim peBase As Integer = t * h

                    For i As Integer = 0 To h - 1
                        dst(dstBase + i) = w(srcBase + i) + pe(peBase + i)
                    Next
                Next
            Next

            Call result.MarkHostModified()

            _lastIds = ids
            _lastRate = If(isTraining, _dropoutRate, 0.0)

            If _lastRate > 0 Then
                result = TensorOpsEx.DropoutForward(result, _lastRate, _lastMask)
            Else
                _lastMask = Nothing
            End If

            Return result
        End Function

        ''' <summary>
        ''' 反向：把梯度散射回嵌入矩阵，并返回对该矩阵的梯度。
        ''' </summary>
        ''' <param name="dOut">对 <see cref="Embed"/> 输出的梯度，形状 <c>[batch, time, H]</c>。</param>
        ''' <remarks>参数梯度会同时累加到内部优化器，调用方只需在训练步结束时执行更新。</remarks>
        Public Function Backward(dOut As Tensor) As Tensor
            If dOut Is Nothing Then Throw New ArgumentNullException(NameOf(dOut))
            If _lastIds Is Nothing Then Throw New InvalidOperationException("必须先执行前向传播才能反向传播")

            Dim d = If(_lastRate > 0, TensorOpsEx.DropoutBackward(dOut, _lastMask, _lastRate), dOut)
            Dim ids = _lastIds
            Dim batch As Integer = ids.GetLength(0)
            Dim time As Integer = ids.GetLength(1)
            Dim h As Integer = _embeddingSize
            Dim dWeights As New Tensor(_weights.Shape)
            Dim grad = dWeights.Data
            Dim src = d.Data

            For b As Integer = 0 To batch - 1
                For t As Integer = 0 To time - 1
                    Dim id As Integer = ids(b, t)
                    Dim srcBase As Integer = (b * time + t) * h
                    Dim dstBase As Integer = id * h

                    For i As Integer = 0 To h - 1
                        grad(dstBase + i) += src(srcBase + i)
                    Next
                Next
            Next

            Call dWeights.MarkHostModified()
            Call TensorOps.Accumulate(_optimizer.Gradient, dWeights)

            Return dWeights
        End Function

        ''' <summary>清零嵌入矩阵的梯度累加器。</summary>
        Public Sub ZeroGradients()
            _optimizer.ZeroGrad()
        End Sub

        ''' <summary>对嵌入矩阵执行一次优化器更新。</summary>
        ''' <param name="learningRate">学习率。</param>
        ''' <param name="[step]">训练步序号（从 1 开始）。</param>
        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            _optimizer.MakeTrainingStep(learningRate, [step], _weights)
        End Sub

        ''' <summary>
        ''' 生成标准正弦位置编码：<c>pe[pos, 2i] = sin(pos / 10000^(2i/d))</c>，
        ''' <c>pe[pos, 2i+1] = cos(pos / 10000^(2i/d))</c>。
        ''' </summary>
        ''' <param name="sequenceLength">序列长度。</param>
        ''' <param name="embeddingSize">嵌入维度。</param>
        Public Shared Function buildPositionalEncoding(sequenceLength As Integer, embeddingSize As Integer) As Tensor
            Dim pe As New Tensor(sequenceLength, embeddingSize)
            Dim data = pe.Data

            For pos As Integer = 0 To sequenceLength - 1
                For i As Integer = 0 To embeddingSize - 1 Step 2
                    Dim theta As Double = pos / std.Pow(10000.0, i / CDbl(embeddingSize))

                    data(pos * embeddingSize + i) = std.Sin(theta)

                    If i + 1 < embeddingSize Then
                        data(pos * embeddingSize + i + 1) = std.Cos(theta)
                    End If
                Next
            Next

            Call pe.MarkHostModified()

            Return pe
        End Function

        Private Sub validateIds(ids As Integer(,))
            If ids Is Nothing Then Throw New ArgumentNullException(NameOf(ids))

            If ids.GetLength(1) > _sequenceLength Then
                Throw New ArgumentException(
                    $"序列长度 {ids.GetLength(1)} 超过位置编码容量 {_sequenceLength}")
            End If

            Dim batch As Integer = ids.GetLength(0)
            Dim time As Integer = ids.GetLength(1)

            For b As Integer = 0 To batch - 1
                For t As Integer = 0 To time - 1
                    Dim id As Integer = ids(b, t)

                    If id < 0 OrElse id >= _vocabSize Then
                        Throw New ArgumentOutOfRangeException(NameOf(ids),
                            $"ids({b},{t}) = {id} 超出词表范围 [0, {_vocabSize - 1}]")
                    End If
                Next
            Next
        End Sub

    End Class

End Namespace
