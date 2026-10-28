Option Strict On
Option Explicit On

Imports System

''' <summary>
''' ANFIS 的参数更新策略（对应 readme 中列出的四种混合方案）。
''' </summary>
Public Enum AnfisLearningMode As Integer

    ''' <summary>
    ''' 纯梯度下降：前提参数与结论参数都走反向传播。
    ''' </summary>
    ''' <remarks>
    ''' 作为对照实验用。收敛明显慢于混合方案，且结论参数一旦进入病态区域很难靠 SGD 拉回来。
    ''' </remarks>
    GradientOnly = 0

    ''' <summary>
    ''' 训练开始时用当前前提参数做一次 LSE，此后冻结结论参数，只调前提参数。
    ''' </summary>
    GradientPlusOneShotLse = 1

    ''' <summary>
    ''' 标准混合学习：每轮先在全量训练集上重解结论参数（LSE），再用梯度下降修正前提参数。
    ''' </summary>
    Hybrid = 2

    ''' <summary>
    ''' 递推最小二乘（RLS）：结论参数按块增量更新，不做全量重解，适合流式/内存受限场景。
    ''' </summary>
    SequentialLse = 3

End Enum

''' <summary>前提参数的优化器。</summary>
Public Enum AnfisOptimizerKind As Integer

    ''' <summary>最朴素的 <c>p -= lr*g</c>。</summary>
    GradientDescent = 0
    ''' <summary>带动量的梯度下降。</summary>
    Momentum = 1
    ''' <summary>Adam（一阶/二阶矩估计 + 偏差校正）。</summary>
    Adam = 2
    ''' <summary>AdamW（解耦权重衰减）。</summary>
    AdamW = 3

End Enum

''' <summary>
''' ANFIS 的训练与结构配置。
''' </summary>
''' <remarks>
''' 这里的每一项都直接参与"非凸优化 + 线性最小二乘"的取舍：
''' <see cref="Ridge"/> 决定结论参数的数值稳定性，
''' <see cref="LearningRate"/> / <see cref="LearningRateDecay"/> 决定前提参数的步长，
''' <see cref="MaxRules"/> 决定结构是否还落在可计算范围内。
''' </remarks>
Public Class AnfisOptions

    ''' <summary>特征列名称；为空时自动生成 <c>x1..xn</c>。</summary>
    Public Property FeatureNames As String() = Nothing
    ''' <summary>输出名称；为空时自动生成 <c>y1..yK</c>。</summary>
    Public Property OutputNames As String() = Nothing

    ''' <summary>隶属函数类型（默认高斯）。</summary>
    Public Property MfKind As MembershipKind = MembershipKind.Gaussian
    ''' <summary>每个输入的隶属函数个数（默认 3）。</summary>
    Public Property MfCount As Integer = 3
    ''' <summary>
    ''' 逐输入的隶属函数个数；给定时覆盖 <see cref="MfCount"/>（长度必须等于输入维数）。
    ''' </summary>
    Public Property MfCounts As Integer() = Nothing
    ''' <summary>
    ''' 规则启用掩膜（长度 = <c>prod(m_i)</c>，编号顺序即全网格枚举顺序）。
    ''' 给定时只保留为 <c>True</c> 的规则，可用于注入专家规则或跳过不可能的规则组合。
    ''' </summary>
    Public Property RuleEnableMask As Boolean() = Nothing

    ''' <summary>参数更新策略。</summary>
    Public Property LearningMode As AnfisLearningMode = AnfisLearningMode.Hybrid
    ''' <summary>前提参数的优化器。</summary>
    Public Property Optimizer As AnfisOptimizerKind = AnfisOptimizerKind.Adam

    ''' <summary>前提参数的基础学习率。</summary>
    Public Property LearningRate As Double = 0.02
    ''' <summary>
    ''' 学习率衰减系数：第 e 轮（从 0 计）的实际学习率为 <c>lr / (1 + decay*e)</c>。
    ''' </summary>
    ''' <remarks>
    ''' 前提参数（隶属函数的中心与宽度）的梯度量级随训练的推进迅速变化，
    ''' 固定步长几乎必然在后期震荡；Jang 原文用的是 <c>lr / sqrt(epoch)</c>，
    ''' 这里用同量级的双曲衰减，行为更平缓。
    ''' </remarks>
    Public Property LearningRateDecay As Double = 0.05

    ''' <summary>动量系数（Momentum 优化器）。</summary>
    Public Property Momentum As Double = 0.9
    ''' <summary>Adam 的一阶矩衰减率。</summary>
    Public Property Beta1 As Double = 0.9
    ''' <summary>Adam 的二阶矩衰减率。</summary>
    Public Property Beta2 As Double = 0.999
    ''' <summary>Adam 的数值稳定项。</summary>
    Public Property Epsilon As Double = 0.00000001
    ''' <summary>AdamW 的解耦权重衰减。</summary>
    Public Property WeightDecay As Double = 0.0

    ''' <summary>梯度裁剪阈值（按整体 L2 范数），&lt;= 0 表示不裁剪。</summary>
    Public Property GradientClip As Double = 5.0

    ''' <summary>
    ''' LSE 的相对岭项系数：实际岭项为 <c>ridge * tr(A'A) / P</c>。
    ''' </summary>
    ''' <remarks>
    ''' 用相对值而不是绝对值，是因为 <c>A'A</c> 的量级随样本数与特征尺度变化好几个数量级，
    ''' 固定的绝对岭项在一种数据上不起作用、在另一种数据上会把模型压成常数。
    ''' </remarks>
    Public Property Ridge As Double = 0.000001

    ''' <summary>分块处理的每块行数（控制临时内存与缓存命中率）。</summary>
    Public Property ChunkRows As Integer = 512

    ''' <summary>最大训练轮数。</summary>
    Public Property MaxEpoch As Integer = 100
    ''' <summary>早停耐心：验证误差连续多少轮没有改善就停止。</summary>
    Public Property Patience As Integer = 20
    ''' <summary>判定"有改善"的最小相对幅度。</summary>
    Public Property MinImprovement As Double = 0.000001
    ''' <summary>验证集比例；0 表示不划分验证集（用训练误差做早停）。</summary>
    Public Property ValidationRatio As Double = 0.2
    ''' <summary>数据划分与初始化的随机种子（固定种子 + 单线程 = 结果可复现）。</summary>
    Public Property RandomSeed As Integer = 0
    ''' <summary>是否在每轮训练前打乱样本顺序。</summary>
    Public Property Shuffle As Boolean = True

    ''' <summary>冻结前提参数（只做结论参数的 LSE）。</summary>
    Public Property FreezePremise As Boolean = False
    ''' <summary>冻结结论参数（只做前提参数的梯度下降）。</summary>
    Public Property FreezeConsequent As Boolean = False

    ''' <summary>规则数上限，超过直接报错（默认 1024，对应 n=6、m=4 的量级）。</summary>
    Public Property MaxRules As Integer = 1024
    ''' <summary>结论参数总数上限 <c>R*(n+1)</c>，超过直接报错（默认 4096）。</summary>
    ''' <remarks>
    ''' 主要约束来自 <c>A'A</c> 的 <c>P^2</c> 内存：P=4096 时约 134 MB，
    ''' 再往上就应该改用递推最小二乘或先做规则剪枝。
    ''' </remarks>
    Public Property MaxConsequentParams As Integer = 4096

    ''' <summary>RLS 的遗忘因子（1 表示不遗忘，等价于累积最小二乘）。</summary>
    Public Property RlsForgetting As Double = 1.0
    ''' <summary>RLS 初始协方差矩阵的对角初值。</summary>
    Public Property RlsInitialCovariance As Double = 1000.0

    ''' <summary>逐属性校验并返回自身。</summary>
    Public Function Validate() As AnfisOptions
        If MfCount <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(MfCount), MfCount, "the membership function count must be positive")
        End If
        If MfCounts IsNot Nothing Then
            For i As Integer = 0 To MfCounts.Length - 1
                If MfCounts(i) <= 0 Then
                    Throw New ArgumentOutOfRangeException(NameOf(MfCounts), $"input [{i}] has no membership function")
                End If
            Next
        End If
        If LearningRate <= 0.0 Then
            Throw New ArgumentOutOfRangeException(NameOf(LearningRate), LearningRate, "the learning rate must be positive")
        End If
        If LearningRateDecay < 0.0 Then
            Throw New ArgumentOutOfRangeException(NameOf(LearningRateDecay), LearningRateDecay, "the decay must not be negative")
        End If
        If Ridge < 0.0 Then
            Throw New ArgumentOutOfRangeException(NameOf(Ridge), Ridge, "the ridge factor must not be negative")
        End If
        If ChunkRows <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(ChunkRows), ChunkRows, "the chunk size must be positive")
        End If
        If MaxEpoch <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(MaxEpoch), MaxEpoch, "the epoch count must be positive")
        End If
        If Patience <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(Patience), Patience, "the patience must be positive")
        End If
        If ValidationRatio < 0.0 OrElse ValidationRatio >= 1.0 Then
            Throw New ArgumentOutOfRangeException(NameOf(ValidationRatio), ValidationRatio, "the validation ratio must be in [0, 1)")
        End If
        If Beta1 <= 0.0 OrElse Beta1 >= 1.0 OrElse Beta2 <= 0.0 OrElse Beta2 >= 1.0 Then
            Throw New ArgumentOutOfRangeException(NameOf(Beta1), "the Adam decay rates must be in (0, 1)")
        End If
        If RlsForgetting <= 0.0 OrElse RlsForgetting > 1.0 Then
            Throw New ArgumentOutOfRangeException(NameOf(RlsForgetting), RlsForgetting, "the forgetting factor must be in (0, 1]")
        End If
        If MaxRules <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(MaxRules), MaxRules, "the rule limit must be positive")
        End If
        If MaxConsequentParams <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(MaxConsequentParams), MaxConsequentParams, "the consequent parameter limit must be positive")
        End If

        Return Me
    End Function

    ''' <summary>浅拷贝（数组字段单独复制）。</summary>
    Public Function Clone() As AnfisOptions
        Dim copy As AnfisOptions = DirectCast(Me.MemberwiseClone(), AnfisOptions)

        If MfCounts IsNot Nothing Then
            copy.MfCounts = CType(MfCounts.Clone(), Integer())
        End If
        If RuleEnableMask IsNot Nothing Then
            copy.RuleEnableMask = CType(RuleEnableMask.Clone(), Boolean())
        End If
        If FeatureNames IsNot Nothing Then
            copy.FeatureNames = CType(FeatureNames.Clone(), String())
        End If
        If OutputNames IsNot Nothing Then
            copy.OutputNames = CType(OutputNames.Clone(), String())
        End If

        Return copy
    End Function

    Public Overrides Function ToString() As String
        Return $"{LearningMode}/{Optimizer}, mf={MfKind}x{MfCount}, lr={LearningRate:G4}, ridge={Ridge:G4}"
    End Function

End Class
