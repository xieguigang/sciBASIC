Option Strict On
Option Explicit On

Imports System

Namespace TSK

    ''' <summary>
    ''' 一次批量前向传播的全部中间量（五层网络的激活值）。
    ''' </summary>
    ''' <remarks>
    ''' 全部按"行 = 样本"存放，便于按行块切分做最小二乘累加，也便于直接看某一条样本
    ''' 的隶属度与规则触发强度（可解释性诊断）。
    ''' </remarks>
    Public Class TskInferenceTrace

        ''' <summary>本批次在原始数据中的起始行号。</summary>
        Public Property RowStart As Integer
        ''' <summary>本批次的样本数。</summary>
        Public Property SampleCount As Integer
        ''' <summary>输入维数 n。</summary>
        Public Property InputCount As Integer
        ''' <summary>规则数 R。</summary>
        Public Property RuleCount As Integer
        ''' <summary>输出维数。</summary>
        Public Property OutputCount As Integer

        ''' <summary>输入矩阵，形状 <c>[SampleCount, InputCount]</c>。</summary>
        Public Property X As Double(,)
        ''' <summary>
        ''' 第 1 层的隶属度，形状 <c>[SampleCount, sum(m_i)]</c>；
        ''' 第 i 个输入的模糊集从 <c>MfOffsets(i)</c> 开始连续存放。
        ''' </summary>
        Public Property Mu As Double(,)
        ''' <summary>隶属度的对数（乘积在对数域累加，避免下溢）。</summary>
        Public Property LogMu As Double(,)
        ''' <summary>每个输入的模糊集在 <see cref="Mu"/> 列方向上的起始下标。</summary>
        Public Property MfOffsets As Integer()
        ''' <summary>规则触发强度 w，形状 <c>[SampleCount, RuleCount]</c>（第 2 层）。</summary>
        Public Property W As Double(,)
        ''' <summary>归一化触发强度 <c>w-bar</c>，形状 <c>[SampleCount, RuleCount]</c>（第 3 层）。</summary>
        Public Property Wbar As Double(,)
        ''' <summary>每行的触发强度之和 S。</summary>
        Public Property SumW As Double()
        ''' <summary>每行是否有效（S 未下溢到 0）。</summary>
        Public Property Valid As Boolean()
        ''' <summary>
        ''' 每条规则的线性结论值 <c>f_r</c>，形状 <c>[SampleCount, RuleCount, OutputCount]</c>（第 4 层）。
        ''' </summary>
        Public Property F As Double(,,)
        ''' <summary>网络输出，形状 <c>[SampleCount, OutputCount]</c>（第 5 层）。</summary>
        Public Property Z As Double(,)

    End Class

    ''' <summary>
    ''' 一次批量反向传播得到的梯度：前提参数（扁平向量）、结论参数矩阵与误差统计。
    ''' </summary>
    Public Class TskGradient

        ''' <summary>前提参数梯度，布局与 <see cref="TskNetwork.GetPremiseParameters"/> 完全一致。</summary>
        Public Property Premise As Double()
        ''' <summary>结论参数梯度，形状 <c>[ConsequentParameterCount, OutputCount]</c>。</summary>
        Public Property Theta As Double(,)
        ''' <summary>本批次的平方误差和 <c>sum((z-y)^2)</c>。</summary>
        Public Property SquaredError As Double
        ''' <summary>本批次包含的样本数。</summary>
        Public Property SampleCount As Integer
        ''' <summary>每条规则的归一化触发强度之和（用于剪枝与可解释性诊断）。</summary>
        Public Property RuleActivation As Double()

        ''' <summary>按网络的参数布局创建零梯度。</summary>
        Public Sub New(network As TskNetwork)
            Premise = New Double(network.PremiseParameterCount - 1) {}
            Theta = New Double(network.ConsequentParameterCount - 1, network.OutputCount - 1) {}
            RuleActivation = New Double(network.Rules.RuleCount - 1) {}
        End Sub

    End Class

End Namespace
