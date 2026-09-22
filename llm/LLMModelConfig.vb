' ---------------------------------------------------------------------------
' LLMModelConfig —— decoder-only 语言模型的超参
'
' 把"模型长什么样"这件事集中在一处，方便做对照实验：
'   * 关掉 MoE（UseMoE=False）即退化为标准稠密 Transformer，两者只差一个开关；
'   * NumKvHeads < NumHeads 即切换为 GQA / MQA，KV Cache 随之缩小；
'   * RoPE 的基数 Theta 与 MaxSeqLen 决定上下文窗口的形态；
'   * TieEmbedding 决定输出层是否与词嵌入共享权重。
' ---------------------------------------------------------------------------

''' <summary>
''' LLM 的结构超参。
''' </summary>
Public Class LLMModelConfig

#Region "基本结构"

    ''' <summary>词表大小（= 输出层与词嵌入的行数）。</summary>
    Public Property VocabSize As Integer = 1000

    ''' <summary>模型宽度 d_model。</summary>
    Public Property DModel As Integer = 128

    ''' <summary>解码层数。</summary>
    Public Property NumLayers As Integer = 4

    ''' <summary>注意力 Query 头数。</summary>
    Public Property NumHeads As Integer = 4

    ''' <summary>注意力 K/V 头数；&lt;= 0 表示等于 <see cref="NumHeads"/>（标准 MHA）。</summary>
    Public Property NumKvHeads As Integer = 0

    ''' <summary>单头维度；&lt;= 0 表示由 <c>DModel / NumHeads</c> 推导。</summary>
    Public Property HeadDim As Integer = 0

    ''' <summary>最大序列长度（RoPE 预计算范围与 KV Cache 容量）。</summary>
    Public Property MaxSeqLen As Integer = 256

    ''' <summary>RoPE 的旋转基数 θ。</summary>
    Public Property RopeTheta As Double = LLMTensorOps.DefaultRopeTheta

    ''' <summary>不使用 MoE 的层所用的稠密 SwiGLU 中间层宽度；&lt;= 0 表示自动取 4·dModel。</summary>
    Public Property DenseFfnHidden As Integer = 0

#End Region

#Region "MoE"

    ''' <summary>是否启用 MoE。</summary>
    Public Property UseMoE As Boolean = True

    ''' <summary>
    ''' 从第几层开始使用 MoE（0 基）。
    ''' </summary>
    ''' <remarks>
    ''' DeepSeek-V3 的首层保持稠密：最靠近输入的表示还很"生"，用稠密层先做一次
    ''' 统一的浅层变换比让路由器在噪声上做选择更稳。
    ''' </remarks>
    Public Property MoEStartLayer As Integer = 1

    ''' <summary>单个专家的中间层宽度（细粒度即该值明显小于稠密层的宽度）。</summary>
    Public Property ExpertHidden As Integer = 0

    ''' <summary>路由专家个数（细粒度分割后的专家数）。</summary>
    Public Property NumRoutedExperts As Integer = 8

    ''' <summary>每个 token 激活的路由专家数。</summary>
    Public Property TopKExperts As Integer = 2

    ''' <summary>共享专家个数。</summary>
    Public Property NumSharedExperts As Integer = 1

    ''' <summary>节点组数；&lt;= 1 表示不做节点受限路由。</summary>
    Public Property NodeGroups As Integer = 1

    ''' <summary>每个 token 最多使用的节点数；&lt;= 0 表示不限制。</summary>
    Public Property MaxNodesPerToken As Integer = 0

    ''' <summary>负载均衡偏置的更新步长 u。</summary>
    Public Property BalanceBiasRate As Double = 0.001

#End Region

#Region "推导与校验"

    ''' <summary>实际生效的 K/V 头数。</summary>
    Public ReadOnly Property EffectiveKvHeads As Integer
        Get
            Return If(NumKvHeads <= 0, NumHeads, NumKvHeads)
        End Get
    End Property

    ''' <summary>实际生效的单头维度。</summary>
    Public ReadOnly Property EffectiveHeadDim As Integer
        Get
            If HeadDim > 0 Then Return HeadDim
            If NumHeads <= 0 Then Return 0
            Return DModel \ NumHeads
        End Get
    End Property

    ''' <summary>实际生效的稠密 FFN 中间层宽度。</summary>
    Public ReadOnly Property EffectiveDenseHidden As Integer
        Get
            If DenseFfnHidden > 0 Then Return DenseFfnHidden
            Return AlignUp(4 * DModel, 64)
        End Get
    End Property

    ''' <summary>实际生效的专家中间层宽度。</summary>
    Public ReadOnly Property EffectiveExpertHidden As Integer
        Get
            If ExpertHidden > 0 Then Return ExpertHidden

            ' 细粒度专家：把稠密 FFN 的宽度摊到被激活的全部专家上，
            ' 于是"总计算量不变、单个专家更窄"这一细粒度分割的前提得以保持。
            Dim perToken = System.Math.Max(1, TopKExperts)
            Dim width = 4 * DModel \ (perToken * 2)

            Return AlignUp(System.Math.Max(width, 64), 64)
        End Get
    End Property

    Private Shared Function AlignUp(value As Integer, alignment As Integer) As Integer
        If alignment <= 1 Then Return System.Math.Max(value, 1)
        Return System.Math.Max(((value + alignment - 1) \ alignment) * alignment, alignment)
    End Function

    ''' <summary>第 <paramref name="layer"/> 层是否使用 MoE。</summary>
    Public Function IsMoELayer(layer As Integer) As Boolean
        If Not UseMoE Then Return False
        Return layer >= MoEStartLayer
    End Function

    ''' <summary>
    ''' 校验超参组合是否自洽；不合法时抛 <see cref="ArgumentException"/>。
    ''' </summary>
    Public Sub Validate()
        If VocabSize <= 0 Then Throw New ArgumentException($"VocabSize 必须为正数，实际 {VocabSize}")
        If DModel <= 0 Then Throw New ArgumentException($"DModel 必须为正数，实际 {DModel}")
        If NumLayers <= 0 Then Throw New ArgumentException($"NumLayers 必须为正数，实际 {NumLayers}")
        If NumHeads <= 0 Then Throw New ArgumentException($"NumHeads 必须为正数，实际 {NumHeads}")
        If MaxSeqLen <= 0 Then Throw New ArgumentException($"MaxSeqLen 必须为正数，实际 {MaxSeqLen}")

        Dim headDim = EffectiveHeadDim

        If headDim <= 0 OrElse headDim * NumHeads <> DModel Then
            Throw New ArgumentException(
                $"HeadDim({headDim}) × NumHeads({NumHeads}) 必须等于 DModel({DModel})")
        End If
        If headDim Mod 2 <> 0 Then
            Throw New ArgumentException($"RoPE 要求 HeadDim 为偶数，实际 {headDim}")
        End If

        Dim kvHeads = EffectiveKvHeads

        If NumHeads Mod kvHeads <> 0 Then
            Throw New ArgumentException($"NumHeads({NumHeads}) 必须能被 NumKvHeads({kvHeads}) 整除")
        End If

        If UseMoE Then
            If NumRoutedExperts <= 0 Then
                Throw New ArgumentException($"NumRoutedExperts 必须为正数，实际 {NumRoutedExperts}")
            End If
            If TopKExperts <= 0 OrElse TopKExperts > NumRoutedExperts Then
                Throw New ArgumentException($"TopKExperts({TopKExperts}) 必须落在 [1, {NumRoutedExperts}] 内")
            End If
            If NumSharedExperts < 0 Then
                Throw New ArgumentException($"NumSharedExperts 不能为负数，实际 {NumSharedExperts}")
            End If
            If MoEStartLayer < 0 OrElse MoEStartLayer >= NumLayers Then
                Throw New ArgumentException($"MoEStartLayer({MoEStartLayer}) 必须落在 [0, {NumLayers - 1}] 内")
            End If
        End If
    End Sub

    ''' <summary>输出一句话摘要，便于在控制台核对配置。</summary>
    Public Overrides Function ToString() As String
        Dim moe = If(UseMoE,
            $"MoE(layer >= {MoEStartLayer}: {NumRoutedExperts} routed + {NumSharedExperts} shared, top-{TopKExperts}, hidden {EffectiveExpertHidden})",
            "dense SwiGLU")

        Return $"d_model={DModel}, layers={NumLayers}, heads={NumHeads}/{EffectiveKvHeads}, " &
               $"head_dim={EffectiveHeadDim}, seq={MaxSeqLen}, rope_theta={RopeTheta}, " &
               $"tied_embedding=True, vocab={VocabSize}, ffn={moe}"
    End Function

#End Region

End Class


