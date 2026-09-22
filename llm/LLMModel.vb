#Region "Microsoft.VisualBasic::8b5508d3f0cd51a758caecc539644b04, llm\LLMModel.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 751
    '    Code Lines: 434 (57.79%)
    ' Comment Lines: 159 (21.17%)
    '    - Xml Docs: 81.76%
    ' 
    '   Blank Lines: 158 (21.04%)
    '     File Size: 30.43 KB


    ' Class LLMModel
    ' 
    '     Properties: ActivationRatio, ActiveParametersPerToken, Blocks, Config, EmbeddingGradient
    '                 EstimatedDeviceBytes, LastForwardCache, ParameterCount, Parameters, PinnedDeviceBytes
    '                 Rope, TotalParameters
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: CacheBytes, CreateCaches, DecodeStep, DescribeModel, Embed
    '               FirstMoELayer, Forward, ForwardWithoutCache, LastPositionLogits, LinearTransposed
    '               Load, Prefill, SumLengths, SyncFromDevice, TransposedEmbedding
    '               UpdateMoEBalancing
    ' 
    '     Sub: Backward, ResetCaches, ResetMoELifetimeLoad, ResetMoELoadStatistics, Save
    '     Class ForwardCache
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' LLMModel —— decoder-only 语言模型
'
' 数据流（与 readme 的主线一一对应）：
'
'     token id  →  词嵌入查表  →  N × [ RMSNorm → 因果自注意力 → RMSNorm → FFN/MoE ]
'               →  末端 RMSNorm  →  输出层投影到词表  →  概率分布  →  采样
'
' 两个关键取舍：
'
'   1. 权重绑定（weight tying）
'        输出层不用独立的权重矩阵，而是复用词嵌入矩阵的转置：
'            logits = h · Eᵀ,   E ∈ R^{Vocab × dModel}
'        好处是 [Vocab, dModel] 只驻留一份 —— 在 10 万级词表下这一项就是数千万参数；
'        代价是同一份权重同时收到"读入侧"与"读出侧"两股梯度（反向时会自然叠加）。
'
'   2. 训练与推理走两条前向路径
'        训练：整段序列并行前向，S×S 因果掩码，保存全部中间量以便反向；
'        推理：prefill 一次灌入 prompt，之后每次 DecodeStep 只处理 1 个 token，
'              K/V 从缓存读取，单步注意力开销从 O(t²) 降到 O(t)。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports SysIO = System.IO

''' <summary>
''' 纯 decoder-only 的语言模型：词嵌入 + 若干 <see cref="LLMBlock"/> + 末端 RMSNorm + 绑定输出层。
''' </summary>
Public Class LLMModel

    Private ReadOnly _config As LLMModelConfig
    Private ReadOnly _rope As RotaryEmbedding
    Private ReadOnly _blocks As LLMBlock()
    Private ReadOnly _finalNorm As RmsNorm
    Private ReadOnly _parameters As ParameterSet
    Private ReadOnly _embeddingOptimizer As AdamW

    ''' <summary>词嵌入矩阵，形状 <c>[VocabSize, DModel]</c>。</summary>
    Public ReadOnly TokenEmbedding As Tensor

    ''' <summary>模型超参。</summary>
    Public ReadOnly Property Config As LLMModelConfig
        Get
            Return _config
        End Get
    End Property

    ''' <summary>全部解码层。</summary>
    Public ReadOnly Property Blocks As LLMBlock()
        Get
            Return _blocks
        End Get
    End Property

    ''' <summary>共享的 RoPE 模块。</summary>
    Public ReadOnly Property Rope As RotaryEmbedding
        Get
            Return _rope
        End Get
    End Property

    ''' <summary>参数注册表（含 AdamW 状态与梯度累加器）。</summary>
    Public ReadOnly Property Parameters As ParameterSet
        Get
            Return _parameters
        End Get
    End Property

    ''' <summary>token 嵌入的梯度累加器（输出层与它共享，梯度在此叠加）。</summary>
    Public ReadOnly Property EmbeddingGradient As Tensor
        Get
            Return _embeddingOptimizer.Gradient
        End Get
    End Property

#Region "构造"

    ''' <param name="config">结构超参</param>
    ''' <param name="weightDecay">施加在权重矩阵（含词嵌入）上的解耦权重衰减系数</param>
    Public Sub New(config As LLMModelConfig, Optional weightDecay As Double = 0.01)
        If config Is Nothing Then Throw New ArgumentNullException(NameOf(config))

        Call config.Validate()

        _config = config
        _rope = New RotaryEmbedding(config.EffectiveHeadDim, config.MaxSeqLen, config.RopeTheta)
        _parameters = New ParameterSet()

        TokenEmbedding = LLMTensorOps.HeNormalInit(New Integer() {config.VocabSize, config.DModel})
        _embeddingOptimizer = New AdamW(TokenEmbedding, weightDecay)
        ' 用 Attach 而不是 Add：反向传播往 _embeddingOptimizer.Gradient 累加，
        ' 参数集必须复用同一份优化器状态，否则梯度会被写进一块没人读的内存。
        Call _parameters.Attach("embedding.token", TokenEmbedding, _embeddingOptimizer, weightDecay)

        _blocks = New LLMBlock(config.NumLayers - 1) {}

        For i As Integer = 0 To config.NumLayers - 1
            Dim moe As MoELayer = Nothing

            If config.IsMoELayer(i) Then
                moe = New MoELayer(config.DModel, config.EffectiveExpertHidden,
                                   config.NumRoutedExperts, config.TopKExperts,
                                   config.NumSharedExperts, config.NodeGroups,
                                   config.MaxNodesPerToken, config.BalanceBiasRate)
            End If

            _blocks(i) = New LLMBlock(config.DModel, config.NumHeads, config.EffectiveKvHeads,
                                      config.EffectiveHeadDim, moe, config.EffectiveDenseHidden)
            Call _blocks(i).RegisterParameters(_parameters, "layer" & i, weightDecay)
        Next

        _finalNorm = New RmsNorm(config.DModel)
        Call _finalNorm.RegisterParameters(_parameters, "final_norm", 0.0)
    End Sub

#End Region

#Region "规模统计"

    ''' <summary>模型总参数量。</summary>
    Public ReadOnly Property TotalParameters As Long
        Get
            Return _parameters.TotalParameters
        End Get
    End Property

    ''' <summary>
    ''' 单个 token 实际激活的参数量。
    ''' </summary>
    ''' <remarks>
    ''' 词嵌入是查表，只激活一行；MoE 层只激活 Top-K 个路由专家加上全部共享专家。
    ''' <see cref="TotalParameters"/> 与它的比值即"激活率"，是 MoE 稀疏性的核心指标。
    ''' </remarks>
    Public ReadOnly Property ActiveParametersPerToken As Long
        Get
            Dim total As Long = _config.DModel                 ' 词嵌入查表：只激活一行
            total += SumLengths(_finalNorm.Parameters)          ' 末端 RMSNorm 的 γ

            For Each block In _blocks
                total += block.ActiveParametersPerToken
            Next

            ' 输出层与词嵌入共享权重，因此不额外贡献"激活参数"
            Return total
        End Get
    End Property

    ''' <summary>激活率 = 单 token 激活参数 / 总参数。</summary>
    Public ReadOnly Property ActivationRatio As Double
        Get
            If TotalParameters <= 0 Then Return 0.0
            Return ActiveParametersPerToken / CDbl(TotalParameters)
        End Get
    End Property

    Private Shared Function SumLengths(ts As Tensor()) As Long
        Dim total As Long = 0

        For Each t In ts
            total += t.Length
        Next

        Return total
    End Function

    ''' <summary>统计输出：总参数 / 激活参数 / 激活率 / 各层子模块参数量。</summary>
    Public Function DescribeModel() As String
        Dim sb As New System.Text.StringBuilder()

        Call sb.AppendLine("model config : " & _config.ToString())
        Call sb.AppendLine($"total params : {TotalParameters,14:N0}  ({TotalParameters * 8L / 1024.0 / 1024.0:N1} MB @ double)")
        Call sb.AppendLine($"active/token : {ActiveParametersPerToken,14:N0}  (activation ratio {ActivationRatio:P2})")
        Call sb.AppendLine($"KV cache/layer: {(_config.EffectiveKvHeads * _config.EffectiveHeadDim * 2 * 8L * _config.MaxSeqLen) / 1024.0:N1} KB @ max_seq={_config.MaxSeqLen}")
        Call sb.AppendLine()
        Call sb.AppendLine("per-layer breakdown:")

        For i As Integer = 0 To _blocks.Length - 1
            Dim block = _blocks(i)

            If block.IsMixtureOfExperts Then
                Dim moe = block.MoE

                Call sb.AppendLine($"  layer {i,2}  MoE    total {block.TotalParameters,10:N0}   " &
                                   $"active {block.ActiveParametersPerToken,10:N0}   " &
                                   $"({moe.NumRoutedExperts} routed + {moe.NumSharedExperts} shared, top-{moe.TopK})")
            Else
                Call sb.AppendLine($"  layer {i,2}  dense  total {block.TotalParameters,10:N0}   " &
                                   $"active {block.ActiveParametersPerToken,10:N0}")
            End If
        Next

        Return sb.ToString()
    End Function

#End Region

#Region "训练前向 / 反向"

    ''' <summary>一次训练前向的中间量，供反向使用。</summary>
    Public Class ForwardCache
        ''' <summary>Flattened token ids of the batch that was forwarded.</summary>
        Public Ids As Integer()
        ''' <summary>Batch size of the forward pass.</summary>
        Public BatchSize As Integer
        ''' <summary>Sequence length of the forward pass.</summary>
        Public SeqLen As Integer
        ''' <summary>The token embeddings produced by the embedding lookup.</summary>
        Public Embeddings As Tensor
        ''' <summary>Per block forward caches, consumed by the backward pass.</summary>
        Public BlockCaches As LLMBlock.Cache()
        ''' <summary>Forward cache of the final RMSNorm.</summary>
        Public FinalNormCache As RmsNorm.Cache
        ''' <summary>末端 RMSNorm 的输出（也就是输出层的输入）</summary>
        Public NormOutput As Tensor
    End Class

    Private _lastForward As ForwardCache

    ''' <summary>最近一次 <see cref="Forward"/> 的中间量缓存。</summary>
    Public ReadOnly Property LastForwardCache As ForwardCache
        Get
            Return _lastForward
        End Get
    End Property

    ''' <summary>
    ''' 训练前向：整段序列并行计算，返回词表上的 logits。
    ''' </summary>
    ''' <param name="ids">展平的 token 序列，长度必须等于 <c>batchSize * seqLen</c></param>
    ''' <param name="batchSize">batch 大小</param>
    ''' <param name="seqLen">序列长度</param>
    ''' <returns>logits，形状 <c>[batchSize * seqLen, VocabSize]</c></returns>
    Public Function Forward(ids As Integer(), batchSize As Integer, seqLen As Integer) As Tensor
        If ids Is Nothing Then Throw New ArgumentNullException(NameOf(ids))
        If batchSize <= 0 OrElse seqLen <= 0 Then Throw New ArgumentException("batchSize / seqLen 必须为正数")
        If ids.Length <> batchSize * seqLen Then
            Throw New ArgumentException($"ids 长度 {ids.Length} 与 batchSize*seqLen = {batchSize * seqLen} 不一致")
        End If
        If seqLen > _config.MaxSeqLen Then
            Throw New ArgumentException($"seqLen({seqLen}) 超过 MaxSeqLen({_config.MaxSeqLen})")
        End If

        Dim positions(seqLen - 1) As Integer

        For i As Integer = 0 To seqLen - 1
            positions(i) = i
        Next

        Dim embeddings = Embed(ids, batchSize, seqLen)
        Dim h = embeddings

        Dim blockCaches(_blocks.Length - 1) As LLMBlock.Cache

        For i As Integer = 0 To _blocks.Length - 1
            h = _blocks(i).Forward(h, positions, _rope, Nothing)
            blockCaches(i) = _blocks(i).LastCache
        Next

        Dim normOutput = _finalNorm.Forward(h)
        Dim logits = LinearTransposed(normOutput)

        _lastForward = New ForwardCache With {
            .Ids = ids,
            .BatchSize = batchSize,
            .SeqLen = seqLen,
            .Embeddings = embeddings,
            .BlockCaches = blockCaches,
            .FinalNormCache = _finalNorm.LastCache,
            .NormOutput = normOutput
        }

        Return logits
    End Function

    ''' <summary>
    ''' 反向传播：把 <paramref name="dLogits"/> 一路回传到词嵌入，并把全部参数梯度累加进参数集。
    ''' </summary>
    ''' <param name="dLogits">对 <see cref="Forward"/> 返回值的梯度，形状 <c>[N, VocabSize]</c></param>
    Public Sub Backward(dLogits As Tensor)
        Dim cache = _lastForward

        If cache Is Nothing Then Throw New InvalidOperationException("必须先执行前向传播才能反向传播")

        Dim b = cache.BatchSize
        Dim s = cache.SeqLen
        Dim d = _config.DModel
        Dim n = b * s

        If dLogits.Rank <> 2 OrElse dLogits.Shape(0) <> n OrElse dLogits.Shape(1) <> _config.VocabSize Then
            Throw New ArgumentException(
                $"dLogits 形状应为 [{n}, {_config.VocabSize}]，实际 [{String.Join(",", dLogits.Shape)}]")
        End If

        Dim normOut = Tensor.Wrap(cache.NormOutput.Data, n, d)

        ' ---- 输出层（与词嵌入权重绑定）：logits = h · Eᵀ ----
        ' dH = dLogits · E ；dE = dLogitsᵀ · h
        Dim dH2 = Tensor.computeKernel.MatMul(dLogits, TokenEmbedding)
        Dim dLogitsT = Tensor.computeKernel.Transpose(dLogits)
        Dim dTied = Tensor.computeKernel.MatMul(dLogitsT, normOut)

        Call LLMTensorOps.Accumulate(_embeddingOptimizer.Gradient, dTied)

        ' ---- 末端 RMSNorm ----
        Dim dEmbedOut = _finalNorm.Backward(cache.FinalNormCache, Tensor.Wrap(dH2.Data, b, s, d))

        ' ---- 逐层反向 ----
        For i As Integer = _blocks.Length - 1 To 0 Step -1
            dEmbedOut = _blocks(i).Backward(cache.BlockCaches(i), dEmbedOut, _rope)
        Next

        ' ---- 词嵌入的散射累加（同一 token 出现多次会自然叠加）----
        Call LLMTensorOps.ScatterAddRows(_embeddingOptimizer.Gradient, cache.Ids,
                                         Tensor.Wrap(dEmbedOut.Data, n, d))
    End Sub

    ''' <summary>词嵌入查表：把 <paramref name="ids"/> 映射为 <c>[batch, seq, dModel]</c>。</summary>
    Private Function Embed(ids As Integer(), batchSize As Integer, seqLen As Integer) As Tensor
        Dim d = _config.DModel
        Dim result = New Tensor(batchSize, seqLen, d)
        Dim src = TokenEmbedding.Data
        Dim dst = result.Data

        For r As Integer = 0 To ids.Length - 1
            Dim row = ids(r)

            If row < 0 OrElse row >= _config.VocabSize Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(ids), $"token id {row} 超出词表范围 [0, {_config.VocabSize})")
            End If

            Call Array.Copy(src, row * d, dst, r * d, d)
        Next

        Call result.MarkHostModified()

        Return result
    End Function

    ''' <summary>
    ''' 输出层权重的转置 <c>Eᵀ [dModel, Vocab]</c>，按 <see cref="Tensor.Version"/> 缓存。
    ''' </summary>
    ''' <remarks>
    ''' 这是一个很实在的推理优化：转置矩阵有 <c>dModel × Vocab</c> 个元素
    ''' （12.8 万词表下约 1650 万），而它在整个生成过程中是<b>常量</b>。
    ''' 每步都重转一次的话，"跨步随机写 1650 万次"的开销会远超过小 batch 下真正的
    ''' 矩阵乘，把 KV Cache 带来的复杂度收益完全淹没 —— 表现为"有没有缓存耗时差不多"。
    ''' 训练时权重每步都变（<c>AdamW</c> 更新后会递增版本号），缓存自然失效。
    ''' </remarks>
    Private Function TransposedEmbedding() As Tensor
        If _lmHeadTransposed Is Nothing OrElse _lmHeadVersion <> TokenEmbedding.Version Then
            _lmHeadTransposed = Tensor.computeKernel.Transpose(TokenEmbedding)
            _lmHeadVersion = TokenEmbedding.Version
        End If

        Return _lmHeadTransposed
    End Function

    Private _lmHeadTransposed As Tensor
    Private _lmHeadVersion As Long = -1L

    ''' <summary>
    ''' 把 <c>[.., dModel]</c> 的张量投影到词表：<c>logits = x · Eᵀ</c>。
    ''' </summary>
    ''' <remarks>
    ''' 词表维度极大，因此这一步是整个模型里最重的矩阵乘；GPU 后端
    ''' （<c>CudaTensor.Register()</c>）会在该规模上自动接管。
    ''' </remarks>
    Private Function LinearTransposed(x As Tensor) As Tensor
        Dim width = x.Shape(x.Rank - 1)
        Dim rows = x.Length \ width
        Dim x2 = Tensor.Wrap(x.Data, rows, width)

        Return Tensor.computeKernel.MatMul(x2, TransposedEmbedding())
    End Function

#End Region

#Region "增量推理（KV Cache）"

    ''' <summary>按 batch 创建一组新的 K/V 缓存（每个 batch 元素一份，每层一份）。</summary>
    ''' <param name="maxSeq">缓存容量，不得超过 <c>MaxSeqLen</c></param>
    ''' <param name="batchSize">batch 大小</param>
    Public Function CreateCaches(Optional maxSeq As Integer = 0, Optional batchSize As Integer = 1) As KVCache()()
        If maxSeq <= 0 Then maxSeq = _config.MaxSeqLen

        If maxSeq > _config.MaxSeqLen Then
            Throw New ArgumentException($"KV Cache 容量 {maxSeq} 超过模型 MaxSeqLen({_config.MaxSeqLen})")
        End If

        Dim result(batchSize - 1)() As KVCache

        For b As Integer = 0 To batchSize - 1
            Dim layers(_blocks.Length - 1) As KVCache

            For i As Integer = 0 To _blocks.Length - 1
                layers(i) = New KVCache(maxSeq, _config.EffectiveKvHeads, _config.EffectiveHeadDim)
            Next

            result(b) = layers
        Next

        Return result
    End Function

    ''' <summary>把一组缓存清空（复用同一块缓冲区开始新一轮会话）。</summary>
    Public Shared Sub ResetCaches(caches As KVCache()())
        If caches Is Nothing Then Return

        For Each layers In caches
            For Each cache In layers
                cache.Reset()
            Next
        Next
    End Sub

    ''' <summary>统计一组缓存当前占用的字节数。</summary>
    Public Shared Function CacheBytes(caches As KVCache()()) As Long
        If caches Is Nothing Then Return 0

        Dim total As Long = 0

        For Each layers In caches
            For Each cache In layers
                total += cache.UsedBytes
            Next
        Next

        Return total
    End Function

    ''' <summary>
    ''' prefill：一次性把 prompt 灌入模型，并把 K/V 写入缓存。
    ''' </summary>
    ''' <param name="tokenIds">prompt 的 token 序列</param>
    ''' <param name="caches">batchSize = 1 的缓存组（<see cref="CreateCaches"/> 的返回值的单元素形式）</param>
    ''' <returns>最后一个位置在词表上的 logits（长度 <c>VocabSize</c>）</returns>
    ''' <remarks>
    ''' 只需要最后一个位置的 logits：prompt 中间的每个位置虽然后续也会预测，
    ''' 但在自回归生成里它们已经被"已知"了，算出来只会白白浪费一次 [Vocab] 级的投影。
    ''' </remarks>
    Public Function Prefill(tokenIds As Integer(), caches As KVCache()) As Double()
        If tokenIds Is Nothing OrElse tokenIds.Length = 0 Then
            Throw New ArgumentException("prefill 的 token 序列不能为空")
        End If
        If caches Is Nothing OrElse caches.Length <> _blocks.Length Then
            Throw New ArgumentException(
                $"prefill 需要每层一个 K/V 缓存（应为 {_blocks.Length} 个），实际 {If(caches Is Nothing, 0, caches.Length)} 个")
        End If

        Dim s = tokenIds.Length
        Dim basePos = caches(0).Length

        If basePos + s > caches(0).Capacity Then
            Throw New InvalidOperationException(
                $"KV Cache 容量不足：已用 {basePos}，本次需要 {s}，容量 {caches(0).Capacity}")
        End If

        Dim positions(s - 1) As Integer

        For i As Integer = 0 To s - 1
            positions(i) = basePos + i
        Next

        Dim h = Embed(tokenIds, 1, s)

        For i As Integer = 0 To _blocks.Length - 1
            h = _blocks(i).Forward(h, positions, _rope, New KVCache() {caches(i)})
        Next

        h = _finalNorm.Forward(h)

        Return LastPositionLogits(h, s)
    End Function

    ''' <summary>
    ''' 增量解码一步：只处理 1 个新 token，K/V 从缓存读取。
    ''' </summary>
    ''' <param name="tokenId">上一步采样得到的 token</param>
    ''' <param name="caches">由 <see cref="CreateCaches"/> 创建、并已 prefill 过的缓存组</param>
    ''' <returns>下一个位置在词表上的 logits（长度 <c>VocabSize</c>）</returns>
    Public Function DecodeStep(tokenId As Integer, caches As KVCache()) As Double()
        If caches Is Nothing OrElse caches.Length <> _blocks.Length Then
            Throw New ArgumentException(
                $"DecodeStep 需要每层一个 K/V 缓存（应为 {_blocks.Length} 个），实际 {If(caches Is Nothing, 0, caches.Length)} 个")
        End If

        Dim pos = caches(0).Length

        If pos >= caches(0).Capacity Then
            Throw New InvalidOperationException($"KV Cache 已满（容量 {caches(0).Capacity}），无法继续解码")
        End If

        Dim positions As Integer() = {pos}
        Dim h = Embed(New Integer() {tokenId}, 1, 1)

        For i As Integer = 0 To _blocks.Length - 1
            h = _blocks(i).Forward(h, positions, _rope, New KVCache() {caches(i)})
        Next

        h = _finalNorm.Forward(h)

        Return LastPositionLogits(h, 1)
    End Function

    ''' <summary>
    ''' 不带缓存的全序列续算：把"已经走过的整段前缀"重新前向一遍再取最后一个位置的 logits。
    ''' </summary>
    ''' <remarks>
    ''' 它给出的结果与"prefill + DecodeStep"在数学上完全一致，但每步都要重算全部历史，
    ''' 单步复杂度是 O(t²) 而不是 O(t)。它存在的意义是充当 KV Cache 的<b>正确性对照组</b>：
    ''' 测试程序会让两条路径在同样的 prompt 上逐 token 生成，并断言输出完全一致。
    ''' </remarks>
    Public Function ForwardWithoutCache(tokenIds As Integer()) As Double()
        If tokenIds Is Nothing OrElse tokenIds.Length = 0 Then
            Throw New ArgumentException("token 序列不能为空")
        End If

        Dim s = tokenIds.Length

        If s > _config.MaxSeqLen Then
            Throw New ArgumentException($"序列长度 {s} 超过 MaxSeqLen({_config.MaxSeqLen})")
        End If

        Dim positions(s - 1) As Integer

        For i As Integer = 0 To s - 1
            positions(i) = i
        Next

        Dim h = Embed(tokenIds, 1, s)

        For i As Integer = 0 To _blocks.Length - 1
            h = _blocks(i).Forward(h, positions, _rope, Nothing)
        Next

        h = _finalNorm.Forward(h)

        Return LastPositionLogits(h, s)
    End Function

    ''' <summary>取 <c>[1, S, dModel]</c> 中最后一个位置的 logits。</summary>
    Private Function LastPositionLogits(h As Tensor, seqLen As Integer) As Double()
        Dim d = _config.DModel
        Dim last = New Tensor(1, d)

        Call Array.Copy(h.Data, (seqLen - 1) * d, last.Data, 0, d)
        Call last.MarkHostModified()

        Dim logits = Tensor.computeKernel.MatMul(last, TransposedEmbedding())

        Return CType(logits.Data.Clone(), Double())
    End Function

#End Region

#Region "MoE 负载均衡"

    ''' <summary>
    ''' 对全部 MoE 层执行一次负载均衡偏置更新（应在每个训练步结束后调用一次）。
    ''' </summary>
    ''' <returns>各 MoE 层中最差的"最大负载比"（1.0 表示完全均匀）</returns>
    Public Function UpdateMoEBalancing() As Double
        Dim worst As Double = 0.0

        For Each b In _blocks
            If b.IsMixtureOfExperts Then
                Dim ratio = b.MoE.UpdateBalanceBias()
                If ratio > worst Then worst = ratio
            End If
        Next

        Return worst
    End Function

    ''' <summary>清空全部 MoE 层累计的负载统计（不影响已学到的偏置）。</summary>
    Public Sub ResetMoELoadStatistics()
        For Each b In _blocks
            If b.IsMixtureOfExperts Then Call b.MoE.ResetLoadStatistics()
        Next
    End Sub

    ''' <summary>清空全部 MoE 层的历史负载统计。</summary>
    Public Sub ResetMoELifetimeLoad()
        For Each b In _blocks
            If b.IsMixtureOfExperts Then Call b.MoE.ResetLifetimeLoad()
        Next
    End Sub

    ''' <summary>取得第一个 MoE 层（用于演示专家负载分布）。</summary>
    Public Function FirstMoELayer() As MoELayer
        For Each b In _blocks
            If b.IsMixtureOfExperts Then Return b.MoE
        Next

        Return Nothing
    End Function

#End Region

#Region "设备常驻状态"

    ''' <summary>
    ''' 把全部被钉在显存里的参数回写到主机。
    ''' </summary>
    ''' <returns>实际被回写的参数个数</returns>
    ''' <remarks>
    ''' 平时<b>不需要</b>调用：前向/反向里的矩阵乘会通过设备常驻表直接读到最新权重，
    ''' 刻意避免"每步把整个模型下载回主机"。只有在需要读主机内容的场合才必须同步 ——
    ''' 检查点落盘、以及在主机循环里直接读权重的模块。
    ''' </remarks>
    Public Function SyncFromDevice() As Integer
        Return _parameters.SyncFromDevice()
    End Function

    ''' <summary>当前钉在显存里的参数字节数。</summary>
    Public ReadOnly Property PinnedDeviceBytes As Long
        Get
            Return _parameters.PinnedBytes
        End Get
    End Property

    ''' <summary>登记的参数项个数（用于显存占用估算与报告）。</summary>
    Public ReadOnly Property ParameterCount As Integer
        Get
            Return _parameters.Entries.Count
        End Get
    End Property

    ''' <summary>
    ''' 估算训练时的<b>设备侧</b>显存需求（字节）。
    ''' </summary>
    ''' <remarks>
    ''' 参数被钉住时占 <c>参数数 × 4</c>（单精度），AdamW 的一阶/二阶矩同样常驻，
    ''' 因此稳定占用约为 <c>参数数 × 4 × 3</c>。梯度不常驻，但它每步都要上传一次，
    ''' 会额外占用一份瞬时缓冲。
    ''' </remarks>
    Public ReadOnly Property EstimatedDeviceBytes As Long
        Get
            Return TotalParameters * 4L * 3L
        End Get
    End Property

#End Region

#Region "权重持久化"

    Private Const Magic As Integer = &H4C4D4D31   ' "LMM1"

    ''' <summary>
    ''' 把全部参数（按登记名）写成二进制文件。
    ''' </summary>
    ''' <remarks>
    ''' 格式：<c>magic(i32) | count(i32) | { nameLen(i32) | name(utf8) | rank(i32) | dims(i32*) | data(f64*) }*</c>。
    ''' 只保存参数值，不保存优化器状态 —— 因此加载后的模型可以直接推理，
    ''' 若要继续训练则 AdamW 会从零开始重新累积动量（这是刻意选择：优化器状态
    ''' 属于"训练过程"，不属于"模型"）。
    ''' <para>
    ''' 落盘前必须调用 <see cref="SyncFromDevice"/>：被钉在显存里的参数以设备为主副本，
    ''' 主机 <c>Data</c> 是陈旧的，不先同步就会把旧的权重写进文件。
    ''' </para>
    ''' </remarks>
    Public Sub Save(path As String)
        Call SyncFromDevice()

        Using stream As New SysIO.FileStream(path, SysIO.FileMode.Create, SysIO.FileAccess.Write)
            Using writer As New SysIO.BinaryWriter(stream)

                Call writer.Write(Magic)
                Call writer.Write(_parameters.Entries.Count)

                For Each e In _parameters.Entries
                    Dim bytes = System.Text.Encoding.UTF8.GetBytes(e.Name)

                    Call writer.Write(bytes.Length)
                    Call writer.Write(bytes)
                    Call writer.Write(e.Value.Rank)

                    For i As Integer = 0 To e.Value.Rank - 1
                        Call writer.Write(e.Value.Shape(i))
                    Next

                    Dim data = e.Value.Data

                    For i As Integer = 0 To data.Length - 1
                        Call writer.Write(data(i))
                    Next
                Next
            End Using
        End Using
    End Sub

    ''' <summary>从 <see cref="Save"/> 写出的文件恢复参数（按名称匹配，形状必须一致）。</summary>
    ''' <returns>成功恢复的参数个数</returns>
    Public Function Load(path As String) As Integer
        Using stream As New SysIO.FileStream(path, SysIO.FileMode.Open, SysIO.FileAccess.Read)
            Using reader As New SysIO.BinaryReader(stream)

                If reader.ReadInt32() <> Magic Then
                    Throw New SysIO.InvalidDataException("文件头不匹配，不是本模型导出的权重文件")
                End If

                Dim count = reader.ReadInt32()
                Dim restored As Integer = 0

                For i As Integer = 0 To count - 1
                    Dim nameLen = reader.ReadInt32()
                    Dim name = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(nameLen))
                    Dim rank = reader.ReadInt32()
                    Dim shape(rank - 1) As Integer

                    For j As Integer = 0 To rank - 1
                        shape(j) = reader.ReadInt32()
                    Next

                    Dim length As Integer = 1

                    For j As Integer = 0 To rank - 1
                        length *= shape(j)
                    Next

                    Dim data(length - 1) As Double

                    For j As Integer = 0 To length - 1
                        data(j) = reader.ReadDouble()
                    Next

                    Dim entry = _parameters.Find(name)

                    If entry Is Nothing Then Continue For

                    If Not entry.Value.Shape.SequenceEqual(shape) Then
                        Throw New SysIO.InvalidDataException(
                            $"参数 '{name}' 的形状不匹配：文件 [{String.Join(",", shape)}] vs 模型 [{String.Join(",", entry.Value.Shape)}]")
                    End If

                    Call Array.Copy(data, entry.Value.Data, length)
                    Call entry.Value.MarkHostModified()
                    restored += 1
                Next

                Return restored
            End Using
        End Using
    End Function

#End Region

End Class



