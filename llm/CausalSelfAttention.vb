#Region "Microsoft.VisualBasic::017b849040ccc8b642371bc5c52b61d2, llm\CausalSelfAttention.vb"

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

    '   Total Lines: 558
    '    Code Lines: 336 (60.22%)
    ' Comment Lines: 110 (19.71%)
    '    - Xml Docs: 55.45%
    ' 
    '   Blank Lines: 112 (20.07%)
    '     File Size: 22.77 KB


    ' Class CausalSelfAttention
    ' 
    '     Properties: GroupSize, LastCache, NumHeads, NumKvHeads, Parameters
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Backward, CreateCaches, Forward
    ' 
    '     Sub: MakeTrainingStep, RegisterParameters, ZeroGradients
    '     Class Cache
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' CausalSelfAttention —— 带 KV Cache 的因果自注意力（decoder-only）
'
' 前向有两条路径，数学上完全一致，差别只在"K/V 从哪里来"：
'
'   【训练 / 无缓存路径】cache = Nothing
'       Q、K、V 全部由当前输入现算，注意力矩阵是 S×S，上三角被因果掩码置为 -inf。
'       这是唯一支持反向传播的路径。
'
'   【增量解码路径】cache IsNot Nothing
'       只现算这 S 个新位置的 Q/K/V（解码时 S=1，prefill 时 S=prompt 长度），
'       把 K/V 追加进缓存，再对全部 [0, cacheBase+S) 个位置做注意力。
'       历史位置的 K/V 直接读缓存，O(t²) → O(t)。
'       该路径是纯推理路径，不保存反向所需的中间量。
'
' 多头与 GQA：
'   nKvHeads = nHeads  → 标准 MHA
'   nKvHeads < nHeads  → Grouped-Query Attention：每 groupSize = nHeads/nKvHeads 个
'                        Query 头共享一组 K/V 头，KV Cache 因而缩小 groupSize 倍。
'                       （当 nKvHeads = 1 时就是 Multi-Query Attention）
'
' 位置编码由外部注入（RoPE），因此本类只关心"按绝对位置旋转 Q/K"这一事实，
' 不关心旋转的具体形式，方便替换为 ALiBi 或正弦编码做对比实验。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 因果自注意力层：<c>y = Concat(head_1..head_H) · Wo</c>，
''' 其中 <c>head_h = softmax(Q_h K_hᵀ/√d_k + mask) V_h</c>。
''' </summary>
Public Class CausalSelfAttention

    Private ReadOnly _dModel As Integer
    Private ReadOnly _nHeads As Integer
    Private ReadOnly _nKvHeads As Integer
    Private ReadOnly _headDim As Integer
    Private ReadOnly _groupSize As Integer

    ''' <summary>Q 的投影权重，形状 <c>[dModel, nHeads * headDim]</c>。</summary>
    Public ReadOnly Wq As Tensor

    ''' <summary>K 的投影权重，形状 <c>[dModel, nKvHeads * headDim]</c>。</summary>
    Public ReadOnly Wk As Tensor

    ''' <summary>V 的投影权重，形状 <c>[dModel, nKvHeads * headDim]</c>。</summary>
    Public ReadOnly Wv As Tensor

    ''' <summary>输出投影权重，形状 <c>[nHeads * headDim, dModel]</c>。</summary>
    Public ReadOnly Wo As Tensor

    Private ReadOnly _wqOpt As AdamW
    Private ReadOnly _wkOpt As AdamW
    Private ReadOnly _wvOpt As AdamW
    Private ReadOnly _woOpt As AdamW

    ''' <summary>本层的全部可训练参数。</summary>
    Public ReadOnly Property Parameters As Tensor()
        Get
            Return New Tensor() {Wq, Wk, Wv, Wo}
        End Get
    End Property

    ''' <summary>注意力头数。</summary>
    Public ReadOnly Property NumHeads As Integer
        Get
            Return _nHeads
        End Get
    End Property

    ''' <summary>K/V 头数（GQA / MQA 时小于 <see cref="NumHeads"/>）。</summary>
    Public ReadOnly Property NumKvHeads As Integer
        Get
            Return _nKvHeads
        End Get
    End Property

    ''' <summary>每 <c>groupSize</c> 个 Query 头共享一组 K/V 头。</summary>
    Public ReadOnly Property GroupSize As Integer
        Get
            Return _groupSize
        End Get
    End Property

    ''' <summary>前向传播的中间量缓存，供反向传播使用。</summary>
    Public Class Cache
        ''' <summary>本层输入 <c>[B, S, dModel]</c></summary>
        Public Input As Tensor
        ''' <summary>绝对位置序列</summary>
        Public Positions As Integer()
        ''' <summary>旋转后的 Q，形状 <c>[B, S, dModel]</c></summary>
        Public Q As Tensor
        ''' <summary>旋转后的 K，形状 <c>[B, S, nKvHeads * headDim]</c></summary>
        Public K As Tensor
        ''' <summary>V，形状同 <see cref="K"/></summary>
        Public V As Tensor
        ''' <summary>softmax 概率，按 <c>((b * H + hq) * S + i) * totalLen + j</c> 排布</summary>
        Public Probs As Double()
        ''' <summary>各 head 拼接后、投影到 dModel 之前的结果 <c>[B, S, dModel]</c></summary>
        Public AttnOut As Tensor
        ''' <summary>注意力区间的总长度（训练路径下等于 S）</summary>
        Public TotalLen As Integer
        ''' <summary>batch 大小</summary>
        Public BatchSize As Integer
        ''' <summary>本次前向处理的 query 位置数（也就是 S）</summary>
        Public SeqLen As Integer
        ''' <summary>本次前向经过的 KV 缓存长度（用于诊断 O(t) / O(t²) 的差别）</summary>
        Public CacheBase As Integer
    End Class

    Private _lastCache As Cache

    ''' <summary>最近一次 <see cref="Forward"/> 的中间量缓存。</summary>
    Public ReadOnly Property LastCache As Cache
        Get
            Return _lastCache
        End Get
    End Property

    ''' <param name="dModel">输入输出宽度</param>
    ''' <param name="nHeads">Query 头数</param>
    ''' <param name="nKvHeads">
    ''' K/V 头数；传 0 或负数表示与 <paramref name="nHeads"/> 相同（标准 MHA）
    ''' </param>
    ''' <param name="headDim">单头维度；传 0 表示由 <c>dModel / nHeads</c> 推导</param>
    Public Sub New(dModel As Integer, nHeads As Integer,
                   Optional nKvHeads As Integer = 0,
                   Optional headDim As Integer = 0)

        If dModel <= 0 Then Throw New ArgumentException($"dModel 必须为正数，实际 {dModel}")
        If nHeads <= 0 Then Throw New ArgumentException($"nHeads 必须为正数，实际 {nHeads}")

        If headDim <= 0 Then
            If dModel Mod nHeads <> 0 Then
                Throw New ArgumentException($"dModel({dModel}) 必须能被 nHeads({nHeads}) 整除")
            End If
            headDim = dModel \ nHeads
        End If

        If headDim * nHeads <> dModel Then
            Throw New ArgumentException(
                $"headDim({headDim}) * nHeads({nHeads}) 必须等于 dModel({dModel})")
        End If

        If nKvHeads <= 0 Then nKvHeads = nHeads

        If nHeads Mod nKvHeads <> 0 Then
            Throw New ArgumentException($"nHeads({nHeads}) 必须能被 nKvHeads({nKvHeads}) 整除")
        End If

        _dModel = dModel
        _nHeads = nHeads
        _nKvHeads = nKvHeads
        _headDim = headDim
        _groupSize = nHeads \ nKvHeads

        Dim dKv = _nKvHeads * _headDim

        Wq = LLMTensorOps.HeNormalInit(New Integer() {dModel, dModel})
        Wk = LLMTensorOps.HeNormalInit(New Integer() {dModel, dKv})
        Wv = LLMTensorOps.HeNormalInit(New Integer() {dModel, dKv})
        Wo = LLMTensorOps.HeNormalInit(New Integer() {dModel, dModel})

        _wqOpt = New AdamW(Wq)
        _wkOpt = New AdamW(Wk)
        _wvOpt = New AdamW(Wv)
        _woOpt = New AdamW(Wo)
    End Sub

#Region "参数登记"

    ''' <summary>把本层参数登记进参数集。</summary>
    ''' <param name="registry">目标参数集</param>
    ''' <param name="prefix">参数名前缀，例如 <c>layer3.attn</c></param>
    ''' <param name="weightDecay">权重衰减系数（注意力投影均为权重矩阵，通常施加衰减）</param>
    Public Sub RegisterParameters(registry As ParameterSet, prefix As String, Optional weightDecay As Double = 0.0)
        ' 四个投影矩阵只被 BatchedMatMul 消费（最终走 MatMul → 设备常驻表），
        ' 主机侧没有任何循环直接读它们的 Data，因此可以安全地钉到显存
        Call registry.Attach(prefix & ".Wq", Wq, _wqOpt, weightDecay, deviceResident:=True)
        Call registry.Attach(prefix & ".Wk", Wk, _wkOpt, weightDecay, deviceResident:=True)
        Call registry.Attach(prefix & ".Wv", Wv, _wvOpt, weightDecay, deviceResident:=True)
        Call registry.Attach(prefix & ".Wo", Wo, _woOpt, weightDecay, deviceResident:=True)
    End Sub

#End Region

#Region "前向"

    ''' <summary>
    ''' 前向传播。
    ''' </summary>
    ''' <param name="x">输入 <c>[B, S, dModel]</c></param>
    ''' <param name="positions">
    ''' 长度 S 的绝对位置序列。增量解码时必须满足
    ''' <c>positions(i) = caches(0).Length_before_append + i</c>，否则抛异常。
    ''' </param>
    ''' <param name="rope">共享的 RoPE 模块</param>
    ''' <param name="caches">
    ''' 长度 B 的 K/V 缓存数组（每个 batch 元素一份）；传 <see langword="Nothing"/>
    ''' 表示走训练 / 无缓存路径。
    ''' </param>
    Public Function Forward(x As Tensor, positions As Integer(),
                            rope As RotaryEmbedding,
                            Optional caches As KVCache() = Nothing) As Tensor

        If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))
        If rope Is Nothing Then Throw New ArgumentNullException(NameOf(rope))
        If x.Rank <> 3 OrElse x.Shape(2) <> _dModel Then
            Throw New ArgumentException(
                $"注意力层要求输入 [B, S, {_dModel}]，实际 [{String.Join(",", x.Shape)}]")
        End If

        Dim nBatch = x.Shape(0)
        Dim nSeq = x.Shape(1)

        If positions Is Nothing OrElse positions.Length <> nSeq Then
            Throw New ArgumentException($"需要长度为 S={nSeq} 的 positions 序列")
        End If

        Dim useCache = caches IsNot Nothing

        If useCache AndAlso caches.Length <> nBatch Then
            Throw New ArgumentException($"需要 {nBatch} 份 K/V 缓存，实际传入 {caches.Length} 份")
        End If

        ' ---- 1. Q / K / V 投影 ----
        ' 形状 [B, S, dModel]（Q）与 [B, S, dKv]（K / V），
        ' 因为 dModel = nHeads * headDim，[B, S, dModel] 本身就等价于 [B, S, H, headDim]。
        Dim Q = Transformer.TensorOps.BatchedMatMul(x, Wq)
        Dim K = Transformer.TensorOps.BatchedMatMul(x, Wk)
        Dim V = Transformer.TensorOps.BatchedMatMul(x, Wv)

        ' ---- 2. RoPE ----
        Call rope.Apply(Tensor.Wrap(Q.Data, nBatch, nSeq, _nHeads, _headDim), positions)
        Call rope.Apply(Tensor.Wrap(K.Data, nBatch, nSeq, _nKvHeads, _headDim), positions)

        ' ---- 3. 写入 KV 缓存并确定注意力区间 ----
        Dim cacheBase As Integer = 0
        Dim totalLen As Integer = nSeq

        If useCache Then
            cacheBase = caches(0).Length

            ' 校验"位置序列必须紧接在缓存之后"，避免静默地产生错位的位置编码
            For i As Integer = 0 To nSeq - 1
                If positions(i) <> cacheBase + i Then
                    Throw New ArgumentException(
                        $"增量解码时 positions({i})={positions(i)}，但缓存要求 {cacheBase + i}")
                End If
            Next

            Dim kvStride = _nKvHeads * _headDim

            For bi As Integer = 0 To nBatch - 1
                Call caches(bi).Append(K.Data, bi * nSeq * kvStride,
                                       V.Data, bi * nSeq * kvStride, nSeq)
            Next

            totalLen = cacheBase + nSeq
        End If

        ' ---- 4. 逐 (batch, query 头) 计算缩放点积注意力 ----
        Dim attnOut = New Tensor(nBatch, nSeq, _dModel)
        Dim probs = New Double(nBatch * _nHeads * nSeq * totalLen - 1) {}

        Dim headStride = _nKvHeads * _headDim
        Dim dKv = headStride
        Dim scale = 1.0 / std.Sqrt(_headDim)

        Dim qData = Q.Data
        Dim attnData = attnOut.Data

        ' 复用缓冲区，避免在 B×H×S 次循环里反复分配
        Dim scores(totalLen - 1) As Double
        Dim outputs(_headDim - 1) As Double

        For bi As Integer = 0 To nBatch - 1
            ' K/V 的来源：无缓存时来自本层现算的 K/V（含 batch 偏移），
            ' 有缓存时来自缓存的底层数组（单个序列，无 batch 偏移）。
            Dim kBuf = If(useCache, caches(bi).KeysRaw, K.Data)
            Dim vBuf = If(useCache, caches(bi).ValuesRaw, V.Data)
            Dim kvBatchBase = If(useCache, 0, bi * nSeq * headStride)

            For hq As Integer = 0 To _nHeads - 1
                Dim hk = hq \ _groupSize
                Dim kvHeadBase = hk * _headDim

                For i As Integer = 0 To nSeq - 1
                    Dim absPos = cacheBase + i
                    Dim qBase = (bi * nSeq + i) * _dModel + hq * _headDim

                    ' 4.1 打分 + 因果掩码
                    Dim maxScore = Double.NegativeInfinity

                    For j As Integer = 0 To totalLen - 1
                        If j > absPos Then
                            ' 因果掩码：位置 i 只能看到 <= 自己的位置
                            scores(j) = Double.NegativeInfinity
                            Continue For
                        End If

                        Dim kBase = kvBatchBase + j * headStride + kvHeadBase
                        Dim dot As Double = 0.0

                        For d As Integer = 0 To _headDim - 1
                            dot += qData(qBase + d) * kBuf(kBase + d)
                        Next

                        dot *= scale
                        scores(j) = dot
                        If dot > maxScore Then maxScore = dot
                    Next

                    ' 4.2 softmax
                    Dim sumExp As Double = 0.0

                    For j As Integer = 0 To totalLen - 1
                        Dim e = std.Exp(scores(j) - maxScore)
                        scores(j) = e
                        sumExp += e
                    Next

                    If sumExp <= 0 Then sumExp = 1.0

                    Dim probsBase = ((bi * _nHeads + hq) * nSeq + i) * totalLen

                    For j As Integer = 0 To totalLen - 1
                        Dim p = scores(j) / sumExp
                        scores(j) = p
                        probs(probsBase + j) = p
                    Next

                    ' 4.3 按概率加权聚合 V
                    Call Array.Clear(outputs, 0, _headDim)

                    For j As Integer = 0 To totalLen - 1
                        Dim p = scores(j)
                        If p = 0.0 Then Continue For

                        Dim vBase = kvBatchBase + j * headStride + kvHeadBase

                        For d As Integer = 0 To _headDim - 1
                            outputs(d) += p * vBuf(vBase + d)
                        Next
                    Next

                    Dim outBase = (bi * nSeq + i) * _dModel + hq * _headDim

                    Call Array.Copy(outputs, 0, attnData, outBase, _headDim)
                Next
            Next
        Next

        Call attnOut.MarkHostModified()

        ' ---- 5. 输出投影 ----
        Dim y = Transformer.TensorOps.BatchedMatMul(attnOut, Wo)

        _lastCache = New Cache With {
            .Input = x,
            .Positions = positions,
            .Q = Q,
            .K = K,
            .V = V,
            .Probs = probs,
            .AttnOut = attnOut,
            .TotalLen = totalLen,
            .BatchSize = nBatch,
            .SeqLen = nSeq,
            .CacheBase = cacheBase
        }

        Return y
    End Function

#End Region

#Region "反向"

    ''' <summary>
    ''' 反向传播（只支持 <c>caches = Nothing</c> 的全序列路径）。
    ''' </summary>
    ''' <param name="forwardCache">与该次前向对应的缓存快照</param>
    ''' <param name="dOut">对前向输出 <c>y</c> 的梯度 <c>[B, S, dModel]</c></param>
    ''' <param name="rope">前向使用的同一个 RoPE 模块（反向需要 <c>R(-pos)</c>）</param>
    Public Function Backward(forwardCache As Cache, dOut As Tensor, rope As RotaryEmbedding) As Tensor
        Dim cache = forwardCache

        If cache Is Nothing Then Throw New InvalidOperationException("必须先执行前向传播才能反向传播")
        If cache.CacheBase <> 0 Then
            Throw New NotSupportedException(
                "带 KV Cache 的前向是纯推理路径，不保存反向所需的中间量；训练请使用 caches = Nothing")
        End If

        Dim nBatch = cache.BatchSize
        Dim nSeq = cache.SeqLen
        Dim totalLen = cache.TotalLen

        Dim qData = cache.Q.Data
        Dim kData = cache.K.Data
        Dim vData = cache.V.Data

        Dim headStride = _nKvHeads * _headDim
        Dim dKv = headStride
        Dim scale = 1.0 / std.Sqrt(_headDim)

        ' ---- 1. 输出投影的反向 ----
        Dim dAttnOut As Tensor = Nothing
        Dim dWo As Tensor = Nothing
        Call Transformer.TensorOps.BatchedMatMulBackward(dOut, cache.AttnOut, Wo, dAttnOut, dWo)
        Call LLMTensorOps.Accumulate(_woOpt.Gradient, dWo)

        ' ---- 2. 注意力本身的反向 ----
        Dim dQ = New Tensor(nBatch, nSeq, _dModel)
        Dim dK = New Tensor(nBatch, nSeq, dKv)
        Dim dV = New Tensor(nBatch, nSeq, dKv)

        Dim dQData = dQ.Data
        Dim dKData = dK.Data
        Dim dVData = dV.Data
        Dim dAttnData = dAttnOut.Data

        Dim dScores(totalLen - 1) As Double
        Dim dHead(_headDim - 1) As Double

        For bi As Integer = 0 To nBatch - 1
            For hq As Integer = 0 To _nHeads - 1
                Dim hk = hq \ _groupSize
                Dim kvHeadBase = hk * _headDim

                For i As Integer = 0 To nSeq - 1
                    Dim qBase = (bi * nSeq + i) * _dModel + hq * _headDim
                    Dim probsBase = ((bi * _nHeads + hq) * nSeq + i) * totalLen

                    Call Array.Copy(dAttnData, qBase, dHead, 0, _headDim)

                    ' 2.1 head 输出 = probs · V 的对偶：dProbs 与 dV
                    Dim dotPV As Double = 0.0

                    For j As Integer = 0 To totalLen - 1
                        If j > i Then
                            dScores(j) = 0.0
                            Continue For
                        End If

                        Dim kBase = (bi * nSeq + j) * dKv + kvHeadBase
                        Dim ds As Double = 0.0

                        For d As Integer = 0 To _headDim - 1
                            ds += dHead(d) * vData(kBase + d)
                            dVData(kBase + d) += cache.Probs(probsBase + j) * dHead(d)
                        Next

                        dScores(j) = ds
                    Next

                    ' 2.2 softmax 的反向： dpre_j = p_j · (dp_j − Σ_k dp_k·p_k)
                    For j As Integer = 0 To totalLen - 1
                        If j > i Then Continue For
                        dotPV += dScores(j) * cache.Probs(probsBase + j)
                    Next

                    For j As Integer = 0 To totalLen - 1
                        If j > i Then Continue For
                        ' 缩放 1/√d_k 与 softmax 的导数合并到这一步
                        dScores(j) = cache.Probs(probsBase + j) * (dScores(j) - dotPV) * scale
                    Next

                    ' 2.3 打分矩阵 = Q·Kᵀ/√d_k 的对偶：dQ 与 dK
                    For j As Integer = 0 To totalLen - 1
                        If j > i Then Continue For

                        Dim ds = dScores(j)
                        If ds = 0.0 Then Continue For

                        Dim kBase = (bi * nSeq + j) * dKv + kvHeadBase

                        For d As Integer = 0 To _headDim - 1
                            dQData(qBase + d) += ds * kData(kBase + d)
                            dKData(kBase + d) += ds * qData(qBase + d)
                        Next
                    Next
                Next
            Next
        Next

        Call dQ.MarkHostModified()
        Call dK.MarkHostModified()
        Call dV.MarkHostModified()

        ' ---- 3. RoPE 的反向 = 按同一位置做逆旋转 ----
        Call rope.ApplyInverse(Tensor.Wrap(dQ.Data, nBatch, nSeq, _nHeads, _headDim), cache.Positions)
        Call rope.ApplyInverse(Tensor.Wrap(dK.Data, nBatch, nSeq, _nKvHeads, _headDim), cache.Positions)

        ' ---- 4. 三个输入投影的反向 ----
        Dim dXq As Tensor = Nothing
        Dim dXk As Tensor = Nothing
        Dim dXv As Tensor = Nothing

        Dim dWq As Tensor = Nothing
        Dim dWk As Tensor = Nothing
        Dim dWv As Tensor = Nothing

        Call Transformer.TensorOps.BatchedMatMulBackward(dQ, cache.Input, Wq, dXq, dWq)
        Call Transformer.TensorOps.BatchedMatMulBackward(dK, cache.Input, Wk, dXk, dWk)
        Call Transformer.TensorOps.BatchedMatMulBackward(dV, cache.Input, Wv, dXv, dWv)

        Call LLMTensorOps.Accumulate(_wqOpt.Gradient, dWq)
        Call LLMTensorOps.Accumulate(_wkOpt.Gradient, dWk)
        Call LLMTensorOps.Accumulate(_wvOpt.Gradient, dWv)

        Dim dx = LLMTensorOps.CloneTensor(dXq)
        Call LLMTensorOps.Accumulate(dx, dXk)
        Call LLMTensorOps.Accumulate(dx, dXv)

        Return dx
    End Function

#End Region

#Region "训练步"

    ''' <summary>清零本层全部参数的梯度累加器。</summary>
    Public Sub ZeroGradients()
        _wqOpt.ZeroGrad()
        _wkOpt.ZeroGrad()
        _wvOpt.ZeroGrad()
        _woOpt.ZeroGrad()
    End Sub

    ''' <summary>按 AdamW 规则更新本层参数。</summary>
    Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
        _wqOpt.MakeTrainingStep(learningRate, [step], Wq)
        _wkOpt.MakeTrainingStep(learningRate, [step], Wk)
        _wvOpt.MakeTrainingStep(learningRate, [step], Wv)
        _woOpt.MakeTrainingStep(learningRate, [step], Wo)
    End Sub

    ''' <summary>
    ''' 创建 <paramref name="batchSize"/> 份新的 K/V 缓存（供增量解码使用）。
    ''' </summary>
    ''' <param name="maxSeq">可容纳的最大位置数</param>
    ''' <param name="batchSize">batch 大小</param>
    Public Function CreateCaches(maxSeq As Integer, Optional batchSize As Integer = 1) As KVCache()
        Dim result(batchSize - 1) As KVCache

        For b As Integer = 0 To batchSize - 1
            result(b) = New KVCache(maxSeq, _nKvHeads, _headDim)
        Next

        Return result
    End Function

#End Region

End Class
