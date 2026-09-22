#Region "Microsoft.VisualBasic::03a9ce4c9d465af879e1aaac72aac16f, llm\MoE\MoELayer.vb"

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

    '   Total Lines: 865
    '    Code Lines: 515 (59.54%)
    ' Comment Lines: 167 (19.31%)
    '    - Xml Docs: 56.29%
    ' 
    '   Blank Lines: 183 (21.16%)
    '     File Size: 33.78 KB


    ' Class MoELayer
    ' 
    '     Properties: ActiveParametersPerToken, LastCache, LastRouteInfo, NumRoutedExperts, NumSharedExperts
    '                 TopK, TotalParameters
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Backward, BuildBuckets, BuildRouteInfo, CountOf, Forward
    '               GateWeightOf, LifetimeLoad, SumLengths, UpdateBalanceBias
    ' 
    '     Sub: AccumulateLoad, ApplyNodeLimitedRouting, MakeTrainingStep, RegisterParameters, ResetLifetimeLoad
    '          ResetLoadStatistics, ScatterAddRowsInto, ZeroGradients
    '     Class RouteInfo
    ' 
    ' 
    ' 
    '     Class Cache
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' MoELayer —— DeepSeek 风格的混合专家层
'
' 把 Transformer 的 FFN 换成"若干并行专家 + 一个路由器"，每个 token 只被路由到
' Top-K 个专家，从而做到「总参数量很大（知识容量大）而单 token 计算量只激活一小部分」。
'
' 本实现严格对齐 DeepSeekMoE 的两大架构创新：
'
'   1. 细粒度专家分割
'        把传统 MoE 的 N 个大专家切成 mN 个小专家，同时把激活数从 K 提到 mK，
'        在总计算量不变的前提下让可选组合数从 C(N,K) 变成 C(mN,mK) —— 组合爆炸。
'
'   2. 共享专家隔离
'        划出 K_s 个共享专家，每个 token 无条件激活它们，专门承载跨语境的通用知识
'        （语法、基础事实、格式），让路由专家卸下"通用底座"的包袱、专注特化。
'
' 层输出（与 readme 中的公式逐项对应）：
'
'     h_t = Σ_{i=1..Ks}  FFN_i(u_t)                    ← 共享专家，无条件计算
'         + Σ_{i>Ks}     g_{i,t} · FFN_i(u_t)          ← Top-K 路由专家，稀疏激活
'         + u_t                                        ← 残差
'
' 注意残差项 u_t 由调用方（<see cref="LLMBlock"/>）负责相加：本层只返回两个专家
' 求和项，这样它才能被放在 Pre-Norm 的残差分支里。
'
' 路由：Sigmoid 归一化 + Top-K（区别于经典 MoE 的 softmax 门控）
'
'     s_{i,t} = sigmoid(u_t · Wr)                       ← 每个专家的独立打分
'     Top-K 依据  s_{i,t} + b_i                         ← 偏置只影响"选择"，不影响"权重"
'     g_{i,t} = s_{i,t} / Σ_{j∈TopK} s_{j,t}            ← 在被选中的专家之间归一化
'
' 负载均衡：无辅助损失的动态偏置
'
'     经典做法是往主损失里加一项"负载均衡辅助损失"，但它会与语言建模目标互相拉扯。
'     DeepSeek 改为给每个路由专家维护一个偏置 b_i，只加在 Top-K 的选择打分上：
'
'         b_k ← b_k + ε_k · (L − A_k),    ε_k = u / |L − A_k|
'
'     其中 A_k 是专家 k 的当前负载占比、L = 1/nRoutedExperts 是理想均匀负载。
'     代入 ε_k 后该式等价于 b_k ← b_k + u · sign(L − A_k)：负载过重的专家被调低、
'     过轻的被调高，一步更新（固定步长 u）即可，且完全不改变梯度方向。
'
' 节点受限路由（node-limited routing）
'     把路由专家划分到若干"节点"上，每个 token 的 Top-K 专家被限制在最多 M 个节点内，
'     以控制跨机通信开销 —— 这是 MoE 推向千卡规模训练的关键工程决策。
'     实现方式是先用"组内最大的若干打分之和"给各节点打分，只保留得分最高的 M 个节点，
'     其余专家的选择打分置为 -inf，再在剩余范围内做 Top-K。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' DeepSeekMoE：细粒度专家分割 + 共享专家隔离 + Sigmoid Top-K 路由 + 无辅助损失负载均衡。
''' </summary>
Public Class MoELayer

    Private ReadOnly _dModel As Integer
    Private ReadOnly _expertHidden As Integer
    Private ReadOnly _nRoutedExperts As Integer
    Private ReadOnly _topK As Integer
    Private ReadOnly _nSharedExperts As Integer
    Private ReadOnly _nodeGroups As Integer
    Private ReadOnly _maxNodesPerToken As Integer
    Private ReadOnly _biasUpdateRate As Double

    ''' <summary>路由专家（细粒度小专家）。</summary>
    Private ReadOnly _experts As SwiGLUFeedForward()

    ''' <summary>共享专家（每个 token 无条件激活）。</summary>
    Private ReadOnly _sharedExperts As SwiGLUFeedForward()

    ''' <summary>路由器权重，形状 <c>[dModel, nRoutedExperts]</c>。</summary>
    Public ReadOnly Wr As Tensor

    ''' <summary>
    ''' 路由专家的动态选择偏置 <c>b_i</c>（长度 <c>nRoutedExperts</c>）。
    ''' </summary>
    ''' <remarks>
    ''' 它<b>不是</b>神经网络参数：不接收梯度、不被优化器更新，只由
    ''' <see cref="UpdateBalanceBias"/> 按负载统计直接调整。
    ''' </remarks>
    Public ReadOnly BalanceBias As Double()

    Private ReadOnly _wrOpt As AdamW

    ''' <summary>自上次偏置更新以来累积的专家命中次数。</summary>
    Private ReadOnly _loadCounts As Double()

    ''' <summary>自上次偏置更新以来累积的"总分配次数"（token 数 × topK）。</summary>
    Private _loadTotal As Double

    ''' <summary>路由专家被选中的历史总次数（用于展示负载是否趋于均匀）。</summary>
    Private ReadOnly _lifetimeCounts As Double()

    Private _lifetimeTotal As Double

    ''' <summary>每次前向路由统计的快照。</summary>
    Public Class RouteInfo

        ''' <summary>本次前向每个路由专家的负载占比（长度 = nRoutedExperts）。</summary>
        Public Load As Double()

        ''' <summary>本次前向被处理的 token 数。</summary>
        Public Tokens As Integer

        ''' <summary>每个 token 命中的专家下标，布局 <c>[token * topK + k]</c>。</summary>
        Public Selected As Integer()

        ''' <summary>每个 token 用于选择专家的节点个数（节点受限路由生效时才有意义）。</summary>
        Public NodesUsed As Integer()

        ''' <summary>本次前向被激活过的专家数（负载 &gt; 0 的专家个数）。</summary>
        Public ActivatedExperts As Integer

        ''' <summary>负载不均程度的度量：最大负载与理想均匀负载之比。1.0 表示完全均匀。</summary>
        Public MaxLoadRatio As Double

    End Class

    ''' <summary>前向传播的中间量缓存，供反向传播使用。</summary>
    Public Class Cache
        ''' <summary>本层输入 <c>[B, S, dModel]</c></summary>
        Public Input As Tensor
        ''' <summary>展平后的输入 <c>[N, dModel]</c>（N = B * S）</summary>
        Public FlatInput As Tensor
        ''' <summary>路由器 logits <c>[N, nRoutedExperts]</c>（sigmoid 之前）</summary>
        Public RouterLogits As Tensor
        ''' <summary>sigmoid 之后的专家打分 <c>[N, nRoutedExperts]</c></summary>
        Public Scores As Tensor
        ''' <summary>被选中的专家下标，布局 <c>[N * topK + k]</c></summary>
        Public Selected As Integer()
        ''' <summary>被选中专家的归一化门控权重，布局 <c>[N * topK + k]</c></summary>
        Public GateWeights As Double()
        ''' <summary>每个路由专家负责的 token 行下标</summary>
        Public ExpertTokens As Integer()()
        ''' <summary>每个路由专家输出的缓存（专家对象自身的 LastCache）</summary>
        Public ExpertCaches As SwiGLUFeedForward.Cache()
        ''' <summary>
        ''' 每个路由专家的实际输出，形状 <c>[rows.Length, dModel]</c>。
        ''' </summary>
        ''' <remarks>
        ''' 反向阶段计算门控权重 g 的梯度需要用到它：<c>dL/dg = Σ_d Expert(row,d) · dOut(row,d)</c>。
        ''' 保存下来可以避免在反向时再做一次冗余的前向。
        ''' </remarks>
        Public ExpertOutputs As Tensor()
        ''' <summary>输入形状（反向时还原）</summary>
        Public InputShape As Integer()
        ''' <summary>token 数 N</summary>
        Public Tokens As Integer
    End Class

    Private _lastCache As Cache
    Private _lastRouteInfo As RouteInfo

    ''' <summary>最近一次 <see cref="Forward"/> 的中间量缓存。</summary>
    Public ReadOnly Property LastCache As Cache
        Get
            Return _lastCache
        End Get
    End Property

    ''' <summary>最近一次 <see cref="Forward"/> 的路由统计。</summary>
    Public ReadOnly Property LastRouteInfo As RouteInfo
        Get
            Return _lastRouteInfo
        End Get
    End Property

    ''' <summary>路由专家个数。</summary>
    Public ReadOnly Property NumRoutedExperts As Integer
        Get
            Return _nRoutedExperts
        End Get
    End Property

    ''' <summary>共享专家个数。</summary>
    Public ReadOnly Property NumSharedExperts As Integer
        Get
            Return _nSharedExperts
        End Get
    End Property

    ''' <summary>每个 token 激活的路由专家数。</summary>
    Public ReadOnly Property TopK As Integer
        Get
            Return _topK
        End Get
    End Property

    ''' <summary>
    ''' 本层的总参数（全部专家 + 路由器），也就是"知识容量"。
    ''' </summary>
    Public ReadOnly Property TotalParameters As Long
        Get
            Dim total As Long = Wr.Length

            For Each e In _experts
                total += SumLengths(e.Parameters)
            Next

            For Each e In _sharedExperts
                total += SumLengths(e.Parameters)
            Next

            Return total
        End Get
    End Property

    ''' <summary>
    ''' 单个 token 实际激活的参数个数。
    ''' </summary>
    ''' <remarks>
    ''' 共享专家无条件激活，路由专家只激活 Top-K 个 —— 它的量与
    ''' <see cref="TotalParameters"/> 的比值就是本层的"激活率"。
    ''' </remarks>
    Public ReadOnly Property ActiveParametersPerToken As Long
        Get
            Dim total As Long = Wr.Length

            For Each e In _sharedExperts
                total += SumLengths(e.Parameters)
            Next

            If _experts.Length > 0 Then
                total += _topK * SumLengths(_experts(0).Parameters)
            End If

            Return total
        End Get
    End Property

    Private Shared Function SumLengths(ts As Tensor()) As Long
        Dim total As Long = 0

        For Each t In ts
            total += t.Length
        Next

        Return total
    End Function

    ''' <param name="dModel">输入输出宽度</param>
    ''' <param name="expertHidden">单个专家的中间层宽度（细粒度即该值较小）</param>
    ''' <param name="nRoutedExperts">路由专家个数（细粒度分割后的专家数 mN）</param>
    ''' <param name="topK">每个 token 激活的路由专家数 mK</param>
    ''' <param name="nSharedExperts">共享专家个数 K_s；传 0 表示不使用共享专家</param>
    ''' <param name="nodeGroups">把路由专家划分成的节点组数；&lt;= 1 表示不做节点受限路由</param>
    ''' <param name="maxNodesPerToken">每个 token 最多使用的节点数；&lt;= 0 表示不限制</param>
    ''' <param name="biasUpdateRate">负载均衡偏置的更新步长 u</param>
    Public Sub New(dModel As Integer,
                   expertHidden As Integer,
                   nRoutedExperts As Integer,
                   topK As Integer,
                   Optional nSharedExperts As Integer = 1,
                   Optional nodeGroups As Integer = 1,
                   Optional maxNodesPerToken As Integer = 0,
                   Optional biasUpdateRate As Double = 0.001)

        If dModel <= 0 Then Throw New ArgumentException($"dModel 必须为正数，实际 {dModel}")
        If nRoutedExperts <= 0 Then Throw New ArgumentException($"nRoutedExperts 必须为正数，实际 {nRoutedExperts}")
        If topK <= 0 OrElse topK > nRoutedExperts Then
            Throw New ArgumentException($"topK({topK}) 必须落在 [1, {nRoutedExperts}] 内")
        End If
        If nSharedExperts < 0 Then Throw New ArgumentException($"nSharedExperts 不能为负数，实际 {nSharedExperts}")

        If nodeGroups <= 0 Then nodeGroups = 1

        If nodeGroups > 1 Then
            If nRoutedExperts Mod nodeGroups <> 0 Then
                Throw New ArgumentException($"nRoutedExperts({nRoutedExperts}) 必须能被 nodeGroups({nodeGroups}) 整除")
            End If

            If maxNodesPerToken <= 0 Then maxNodesPerToken = nodeGroups
            If maxNodesPerToken > nodeGroups Then maxNodesPerToken = nodeGroups
        Else
            maxNodesPerToken = 0
        End If

        _dModel = dModel
        _expertHidden = expertHidden
        _nRoutedExperts = nRoutedExperts
        _topK = topK
        _nSharedExperts = nSharedExperts
        _nodeGroups = nodeGroups
        _maxNodesPerToken = maxNodesPerToken
        _biasUpdateRate = biasUpdateRate

        _experts = New SwiGLUFeedForward(nRoutedExperts - 1) {}

        For i As Integer = 0 To nRoutedExperts - 1
            _experts(i) = New SwiGLUFeedForward(dModel, expertHidden)
        Next

        _sharedExperts = New SwiGLUFeedForward(nSharedExperts - 1) {}

        For i As Integer = 0 To nSharedExperts - 1
            _sharedExperts(i) = New SwiGLUFeedForward(dModel, expertHidden)
        Next

        Wr = LLMTensorOps.HeNormalInit(New Integer() {dModel, nRoutedExperts})
        _wrOpt = New AdamW(Wr)

        BalanceBias = New Double(nRoutedExperts - 1) {}
        _loadCounts = New Double(nRoutedExperts - 1) {}
        _lifetimeCounts = New Double(nRoutedExperts - 1) {}
    End Sub

    ''' <summary>把本层参数登记进参数集。</summary>
    Public Sub RegisterParameters(registry As ParameterSet, prefix As String, Optional weightDecay As Double = 0.0)
        ' 路由器矩阵只被 BatchedMatMul 消费，主机侧不直接读它的 Data
        Call registry.Attach(prefix & ".Wr", Wr, _wrOpt, weightDecay, deviceResident:=True)

        For i As Integer = 0 To _nRoutedExperts - 1
            _experts(i).RegisterParameters(registry, $"{prefix}.expert{i}", weightDecay)
        Next

        For i As Integer = 0 To _nSharedExperts - 1
            _sharedExperts(i).RegisterParameters(registry, $"{prefix}.shared{i}", weightDecay)
        Next
    End Sub

#Region "前向"

    ''' <summary>
    ''' 前向：<c>h = Σ 共享专家(u) + Σ_{Top-K} g_i · 路由专家_i(u)</c>（不含残差）。
    ''' </summary>
    ''' <param name="x">输入 <c>[B, S, dModel]</c>，内部展平为 <c>[N, dModel]</c> 处理</param>
    Public Function Forward(x As Tensor) As Tensor
        If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))
        If x.Shape(x.Rank - 1) <> _dModel Then
            Throw New ArgumentException($"MoE 要求输入最后一维为 {_dModel}，实际 [{String.Join(",", x.Shape)}]")
        End If

        Dim shape = CType(x.Shape.Clone(), Integer())
        Dim N = x.Length \ _dModel
        Dim flat = Tensor.Wrap(x.Data, N, _dModel)

        ' ---- 1. 路由打分：sigmoid(u·Wr) ----
        Dim routerLogits = Transformer.TensorOps.BatchedMatMul(flat, Wr)
        Dim scores = Tensor.computeKernel.Sigmoid(routerLogits)

        ' ---- 2. 选择打分 = 打分 + 动态偏置（偏置只影响选择，不影响权重）----
        Dim selection = New Tensor(N, _nRoutedExperts)
        Dim selData = selection.Data
        Dim scoreData = scores.Data

        For r As Integer = 0 To N - 1
            Dim offset = r * _nRoutedExperts

            For e As Integer = 0 To _nRoutedExperts - 1
                selData(offset + e) = scoreData(offset + e) + BalanceBias(e)
            Next
        Next

        Call selection.MarkHostModified()

        ' ---- 3. 节点受限路由：把不在候选节点内的专家打分置为 -inf ----
        Dim nodesUsed(N - 1) As Integer

        If _maxNodesPerToken > 0 AndAlso _maxNodesPerToken < _nodeGroups Then
            Call ApplyNodeLimitedRouting(selData, N, nodesUsed)
        Else
            For r As Integer = 0 To N - 1
                nodesUsed(r) = _nodeGroups
            Next
        End If

        ' ---- 4. Top-K 选择 ----
        Dim topIndices As Tensor = Nothing
        Dim topValues = Tensor.computeKernel.TopK(selection, _topK, topIndices)
        Dim idxData = topIndices.Data

        Dim selected(N * _topK - 1) As Integer
        Dim gateWeights(N * _topK - 1) As Double

        For r As Integer = 0 To N - 1
            ' Gate 权重用的是"未加偏置"的 sigmoid 打分，在被选中的 K 个之间归一化
            Dim sum As Double = 0.0

            For k As Integer = 0 To _topK - 1
                Dim e = CInt(idxData(r * _topK + k))
                selected(r * _topK + k) = e
                sum += scoreData(r * _nRoutedExperts + e)
            Next

            If sum <= 0 Then sum = 1.0

            For k As Integer = 0 To _topK - 1
                gateWeights(r * _topK + k) = scoreData(r * _nRoutedExperts + selected(r * _topK + k)) / sum
            Next
        Next

        ' ---- 5. 统计负载（用于无辅助损失的负载均衡）----
        Call AccumulateLoad(N, selected)

        ' ---- 6. 共享专家：每个 token 无条件计算 ----
        ' 内部一律按 [N, dModel] 的展平形态运算，最后一次性地按输入形状包回去
        Dim output = New Tensor(N, _dModel)

        For i As Integer = 0 To _nSharedExperts - 1
            Dim sharedOut = _sharedExperts(i).Forward(flat)
            Call LLMTensorOps.Accumulate(output, sharedOut)
        Next

        ' ---- 7. 路由专家：只对分配到该专家的 token 做计算（稀疏激活）----
        Dim expertTokens(_nRoutedExperts - 1)() As Integer
        Dim expertCaches(_nRoutedExperts - 1) As SwiGLUFeedForward.Cache
        Dim expertOutputs(_nRoutedExperts - 1) As Tensor

        Dim buckets = BuildBuckets(N, selected)
        Dim outData = output.Data
        Dim width = _dModel

        For e As Integer = 0 To _nRoutedExperts - 1
            Dim rows = buckets(e)
            expertTokens(e) = rows

            If rows.Length = 0 Then
                expertCaches(e) = Nothing
                expertOutputs(e) = Nothing
                Continue For
            End If

            Dim gathered = LLMTensorOps.GatherRows(flat, rows)
            Dim expertOut = _experts(e).Forward(gathered)

            expertCaches(e) = _experts(e).LastCache
            expertOutputs(e) = expertOut

            Dim eOut = expertOut.Data

            For t As Integer = 0 To rows.Length - 1
                Dim weight = GateWeightOf(selected, gateWeights, rows(t), e)
                Dim srcBase = t * width
                Dim dstBase = rows(t) * width

                For d As Integer = 0 To width - 1
                    outData(dstBase + d) += weight * eOut(srcBase + d)
                Next
            Next
        Next

        Call output.MarkHostModified()

        _lastCache = New Cache With {
            .Input = x,
            .FlatInput = flat,
            .RouterLogits = routerLogits,
            .Scores = scores,
            .Selected = selected,
            .GateWeights = gateWeights,
            .ExpertTokens = expertTokens,
            .ExpertCaches = expertCaches,
            .ExpertOutputs = expertOutputs,
            .InputShape = shape,
            .Tokens = N
        }

        _lastRouteInfo = BuildRouteInfo(N, selected, nodesUsed)

        Return Tensor.Wrap(output.Data, shape)
    End Function

    ''' <summary>把 <c>-inf</c> 写入"不在候选节点内"的专家，实现节点受限路由。</summary>
    Private Sub ApplyNodeLimitedRouting(selData As Double(), N As Integer, nodesUsed As Integer())
        Dim perGroup = _nRoutedExperts \ _nodeGroups
        Dim groupScore(_nodeGroups - 1) As Double
        Dim groupRank(_nodeGroups - 1) As Integer
        Dim keep(_nodeGroups - 1) As Boolean
        Dim probe(perGroup - 1) As Integer

        For r As Integer = 0 To N - 1
            Dim offset = r * _nRoutedExperts

            ' 节点得分 = 组内最大的 min(topK, perGroup) 个选择打分之和
            Dim pick = std.Min(_topK, perGroup)

            For g As Integer = 0 To _nodeGroups - 1
                Dim groupBase = offset + g * perGroup
                Dim sum As Double = 0.0

                For i As Integer = 0 To pick - 1
                    Dim best = Double.NegativeInfinity
                    Dim bestIdx = -1

                    For j As Integer = 0 To perGroup - 1
                        Dim v = selData(groupBase + j)
                        Dim taken As Boolean = False

                        For p As Integer = 0 To i - 1
                            If probe(p) = j Then taken = True
                        Next

                        If Not taken AndAlso v > best Then
                            best = v
                            bestIdx = j
                        End If
                    Next

                    probe(i) = bestIdx
                    If best > Double.NegativeInfinity Then sum += best
                Next

                groupScore(g) = sum
                groupRank(g) = g
                keep(g) = False
            Next

            ' 选出得分最高的 maxNodesPerToken 个节点（简单选择排序）
            For i As Integer = 0 To _maxNodesPerToken - 1
                Dim bestI = i

                For j As Integer = i + 1 To _nodeGroups - 1
                    If groupScore(groupRank(j)) > groupScore(groupRank(bestI)) Then bestI = j
                Next

                Dim swap = groupRank(i)
                groupRank(i) = groupRank(bestI)
                groupRank(bestI) = swap
            Next

            For i As Integer = 0 To _maxNodesPerToken - 1
                keep(groupRank(i)) = True
            Next

            nodesUsed(r) = _maxNodesPerToken

            For g As Integer = 0 To _nodeGroups - 1
                If keep(g) Then Continue For

                For j As Integer = 0 To perGroup - 1
                    selData(offset + g * perGroup + j) = Double.NegativeInfinity
                Next
            Next
        Next
    End Sub

    ''' <summary>把每个 token 命中的专家整理成"每个专家对应一批 token 行下标"。</summary>
    Private Function BuildBuckets(N As Integer, selected As Integer()) As Integer()()
        Dim counts(_nRoutedExperts - 1) As Integer

        For i As Integer = 0 To N * _topK - 1
            counts(selected(i)) += 1
        Next

        Dim buckets(_nRoutedExperts - 1)() As Integer

        For e As Integer = 0 To _nRoutedExperts - 1
            buckets(e) = New Integer(counts(e) - 1) {}
            counts(e) = 0
        Next

        For r As Integer = 0 To N - 1
            For k As Integer = 0 To _topK - 1
                Dim e = selected(r * _topK + k)
                buckets(e)(counts(e)) = r
                counts(e) += 1
            Next
        Next

        Return buckets
    End Function

    ''' <summary>查某个 token 在某个专家上的门控权重（同一 token 的 topK 内专家互不重复）。</summary>
    Private Function GateWeightOf(selected As Integer(), gateWeights As Double(), token As Integer, expert As Integer) As Double
        For k As Integer = 0 To _topK - 1
            If selected(token * _topK + k) = expert Then Return gateWeights(token * _topK + k)
        Next

        Return 0.0
    End Function

    Private Sub AccumulateLoad(N As Integer, selected As Integer())
        For i As Integer = 0 To N * _topK - 1
            Dim e = selected(i)
            _loadCounts(e) += 1.0
            _lifetimeCounts(e) += 1.0
        Next

        _loadTotal += N * _topK
        _lifetimeTotal += N * _topK
    End Sub

    Private Function BuildRouteInfo(N As Integer, selected As Integer(), nodesUsed As Integer()) As RouteInfo
        Dim load(_nRoutedExperts - 1) As Double
        Dim activated As Integer = 0
        Dim maxLoad As Double = 0.0

        For e As Integer = 0 To _nRoutedExperts - 1
            load(e) = If(N * _topK = 0, 0.0, CountOf(selected, N, e) / CDbl(N * _topK))

            If load(e) > 0 Then activated += 1
            If load(e) > maxLoad Then maxLoad = load(e)
        Next

        Dim uniform = 1.0 / _nRoutedExperts

        Return New RouteInfo With {
            .Load = load,
            .Tokens = N,
            .Selected = CType(selected.Clone(), Integer()),
            .NodesUsed = CType(nodesUsed.Clone(), Integer()),
            .ActivatedExperts = activated,
            .MaxLoadRatio = If(uniform > 0, maxLoad / uniform, 0.0)
        }
    End Function

    Private Function CountOf(selected As Integer(), N As Integer, expert As Integer) As Integer
        Dim count As Integer = 0

        For i As Integer = 0 To N * _topK - 1
            If selected(i) = expert Then count += 1
        Next

        Return count
    End Function

#End Region

#Region "反向"

    ''' <summary>
    ''' 反向传播：返回对输入的梯度，并累加路由器与全部专家的参数梯度。
    ''' </summary>
    ''' <param name="forwardCache">与该次前向对应的缓存快照</param>
    ''' <param name="dOut">对前向输出的梯度（形状同输入）</param>
    Public Function Backward(forwardCache As Cache, dOut As Tensor) As Tensor
        Dim cache = forwardCache

        If cache Is Nothing Then Throw New InvalidOperationException("必须先执行前向传播才能反向传播")

        Dim N = cache.Tokens
        Dim width = _dModel
        Dim flatGrad = Tensor.Wrap(dOut.Data, N, width)

        Dim dx = New Tensor(N, width)

        ' ---- 1. 共享专家的反向（每个 token 都经过，梯度直接透传）----
        For i As Integer = 0 To _nSharedExperts - 1
            Dim dShared = _sharedExperts(i).Backward(_sharedExperts(i).LastCache, flatGrad)
            Call LLMTensorOps.Accumulate(dx, dShared)
        Next

        ' ---- 2. 路由专家的反向 ----
        ' 每个专家只对"分配到它的那批 token"负责，上游梯度要先乘上门控权重 g。
        ' 与此同时把专家输出与 dOut 的内积累加起来，作为对 g 的梯度。
        Dim dGate = New Double(N * _topK - 1) {}
        Dim idxData = cache.Selected

        For e As Integer = 0 To _nRoutedExperts - 1
            Dim rows = cache.ExpertTokens(e)

            If rows Is Nothing OrElse rows.Length = 0 Then Continue For

            ' 2.1 构造该专家的上游梯度 g · dOut[rows]
            Dim perToken = New Tensor(rows.Length, width)
            Dim src = flatGrad.Data
            Dim dst = perToken.Data

            For t As Integer = 0 To rows.Length - 1
                Dim weight = GateWeightOf(idxData, cache.GateWeights, rows(t), e)
                Dim srcBase = rows(t) * width
                Dim dstBase = t * width

                For d As Integer = 0 To width - 1
                    dst(dstBase + d) = weight * src(srcBase + d)
                Next
            Next

            Call perToken.MarkHostModified()

            ' 2.2 对门控权重的梯度：dL/dg = Σ_d Expert(row, d) · dOut(row, d)
            Dim expertOut = cache.ExpertOutputs(e).Data

            For t As Integer = 0 To rows.Length - 1
                Dim token = rows(t)
                Dim outBase = t * width
                Dim upBase = token * width
                Dim dot As Double = 0.0

                For d As Integer = 0 To width - 1
                    dot += expertOut(outBase + d) * src(upBase + d)
                Next

                For k As Integer = 0 To _topK - 1
                    If idxData(token * _topK + k) = e Then
                        dGate(token * _topK + k) += dot
                    End If
                Next
            Next

            ' 2.3 专家的反向
            Dim dGathered = _experts(e).Backward(cache.ExpertCaches(e), perToken)

            ' 2.4 把梯度散射回 token 行
            Call ScatterAddRowsInto(dx, rows, dGathered)
        Next

        ' ---- 3. 门控权重 → 专家打分 ----
        ' g_i = s_i / Σ_{j∈K} s_j  ⇒  ds_i = dg_i / Σs − s_i · (Σ_j dg_j) / (Σs)²
        Dim dScores = New Double(N * _nRoutedExperts - 1) {}
        Dim scoreData = cache.Scores.Data

        For r As Integer = 0 To N - 1
            Dim base = r * _topK
            Dim scoreBase = r * _nRoutedExperts
            Dim sum As Double = 0.0
            Dim dot As Double = 0.0

            For k As Integer = 0 To _topK - 1
                Dim s = scoreData(scoreBase + idxData(base + k))
                sum += s
                dot += dGate(base + k) * s
            Next

            If sum <= 0 Then sum = 1.0

            For k As Integer = 0 To _topK - 1
                Dim e = idxData(base + k)
                Dim s = scoreData(scoreBase + e)
                dScores(scoreBase + e) += dGate(base + k) / sum - s * dot / (sum * sum)
            Next
        Next

        ' ---- 4. 打分 → logits（sigmoid 的导数）----
        Dim dLogits = New Tensor(N, _nRoutedExperts)
        Dim dLogitData = dLogits.Data

        For i As Integer = 0 To dLogitData.Length - 1
            Dim s = scoreData(i)
            dLogitData(i) = dScores(i) * s * (1.0 - s)
        Next

        Call dLogits.MarkHostModified()

        ' ---- 5. 路由器投影的反向 ----
        Dim dFlat As Tensor = Nothing
        Dim dWr As Tensor = Nothing
        Call Transformer.TensorOps.BatchedMatMulBackward(dLogits, cache.FlatInput, Wr, dFlat, dWr)
        Call LLMTensorOps.Accumulate(_wrOpt.Gradient, dWr)

        Call LLMTensorOps.Accumulate(dx, dFlat)

        ' ---- 6. 还原成输入形状 ----
        Return Tensor.Wrap(dx.Data, cache.InputShape)
    End Function

    Private Sub ScatterAddRowsInto(dx As Tensor, rows As Integer(), dGathered As Tensor)
        Dim width = _dModel
        Dim dst = dx.Data
        Dim src = dGathered.Data

        For t As Integer = 0 To rows.Length - 1
            Dim dstBase = rows(t) * width
            Dim srcBase = t * width

            For d As Integer = 0 To width - 1
                dst(dstBase + d) += src(srcBase + d)
            Next
        Next

        Call dx.MarkHostModified()
    End Sub

#End Region

#Region "无辅助损失的负载均衡"

    ''' <summary>
    ''' 按 <c>b_k ← b_k + ε_k · (L − A_k)</c>（<c>ε_k = u / |L − A_k|</c>）更新选择偏置。
    ''' </summary>
    ''' <remarks>
    ''' 该式等价于 <c>b_k ← b_k + u · sign(L − A_k)</c>：负载过重的专家被调低偏置、
    ''' 过轻的被调高，且因为使用固定步长 u 做"符号更新"，一步即可逼近均衡，
    ''' 同时完全不影响梯度方向（偏置不参与反向传播）。
    ''' 应在每个训练步结束时调用一次。
    ''' </remarks>
    ''' <returns>更新前的最大负载比（1.0 表示完全均匀），便于观测收敛过程。</returns>
    Public Function UpdateBalanceBias() As Double
        If _loadTotal <= 0 Then Return 0.0

        Dim uniform = 1.0 / _nRoutedExperts
        Dim maxRatio As Double = 0.0

        For e As Integer = 0 To _nRoutedExperts - 1
            Dim load = _loadCounts(e) / _loadTotal

            If load / uniform > maxRatio Then maxRatio = load / uniform

            Dim gap = uniform - load

            If gap = 0 Then Continue For

            Dim eps = _biasUpdateRate / std.Abs(gap)

            BalanceBias(e) += eps * gap
        Next

        Call ResetLoadStatistics()

        Return maxRatio
    End Function

    ''' <summary>清空累积的负载统计（不影响 <see cref="BalanceBias"/>）。</summary>
    Public Sub ResetLoadStatistics()
        Array.Clear(_loadCounts, 0, _loadCounts.Length)
        _loadTotal = 0.0
    End Sub

    ''' <summary>
    ''' 累计至今各路由专家的负载占比（用于在训练结束时展示"负载是否被拉平"）。
    ''' </summary>
    Public Function LifetimeLoad() As Double()
        Dim result(_nRoutedExperts - 1) As Double

        If _lifetimeTotal <= 0 Then Return result

        For e As Integer = 0 To _nRoutedExperts - 1
            result(e) = _lifetimeCounts(e) / _lifetimeTotal
        Next

        Return result
    End Function

    ''' <summary>把历史负载统计清零。</summary>
    Public Sub ResetLifetimeLoad()
        Array.Clear(_lifetimeCounts, 0, _lifetimeCounts.Length)
        _lifetimeTotal = 0.0
    End Sub

#End Region

#Region "训练步"

    ''' <summary>清零路由器与全部专家的梯度累加器。</summary>
    Public Sub ZeroGradients()
        _wrOpt.ZeroGrad()

        For Each e In _experts
            e.ZeroGradients()
        Next

        For Each e In _sharedExperts
            e.ZeroGradients()
        Next
    End Sub

    ''' <summary>按 AdamW 规则更新路由器与全部专家。</summary>
    Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
        _wrOpt.MakeTrainingStep(learningRate, [step], Wr)

        For Each e In _experts
            e.MakeTrainingStep(learningRate, [step])
        Next

        For Each e In _sharedExperts
            e.MakeTrainingStep(learningRate, [step])
        Next
    End Sub

#End Region

End Class
