#Region "Microsoft.VisualBasic::0ec5200d20ead1e3caefe4a9c6c802bc, Data_science\MachineLearning\SNN\RecurrentLIFLayer.vb"

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

    '   Total Lines: 644
    '    Code Lines: 352 (54.66%)
    ' Comment Lines: 194 (30.12%)
    '    - Xml Docs: 67.53%
    ' 
    '   Blank Lines: 98 (15.22%)
    '     File Size: 26.43 KB


    ' Structure RecurrentGradients
    ' 
    ' 
    ' 
    ' Class RecurrentLIFLayer
    ' 
    '     Properties: Alpha, Beta, HLast, Mask, MembraneState
    '                 Name, ResetMode, SHistory, SmoothForward, SprevHistory
    '                 SurrogateType, Threshold, TimeSteps, Trainable, UHistory
    '                 ULast, Units, Weight, WeightGrad
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: AddTensor, BackwardTime, Diagnose, EmptySpikeGradients, ForwardSequence
    '               ForwardStep, HasFrozenSynapses, MaskedWeightGrad, ToString
    ' 
    '     Sub: ApplyMaskToWeight, AssertSquare, ResetState
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' RecurrentLIFLayer.vb — 可训练的递归 LIF 层（含 BPTT + 代理梯度）
'
' 与既有两类 LIF 层的关系：
'   LIFLayer        稠密前馈层：I[t] = X[t]·W，输入是"当前时刻的外部特征"。
'   SparseLIFLayer  稀疏递归层：I[t] = W·S[t−1] + I_ext[t]，权重固定、仅前向。
'   本层            稠密递归层：I[t] = W·S[t−1] + I_ext[t]，权重可训练（BPTT）。
'
' 本层描述的是"同一神经元群内部的突触连接"：Weight 为方阵 W[N, N]，
' W[pre, post] 表示突触前神经元 pre 连到突触后神经元 post（行=pre，列=post），
' 与 SparseLIFLayer 的 SparseMatrix 行/列约定完全一致，因此可以在两者之间
' 自由地搬运同一张连接矩阵（稠密 ↔ CSR）。
'
' 每个时间步的前向动态：
'   I_rec[t] = S[t−1] · W            上一时刻脉冲经突触矩阵回灌（含循环连接与自反馈）
'   I[t]     = I_rec[t] + I_ext[t]   叠加外部注入电流（表达量编码后的持续电流）
'   U[t]     = β·H[t−1] + I[t]       泄漏积分（β = exp(−Δt/τ_m)）
'   S[t]     = Θ(U[t] − θ)           阈值触发（二值脉冲）
'   H[t]     = 复位(U[t], S[t])      发放后复位（硬/软复位）
'
' 复位模式（与 LIFLayer 一致）：
'   ZeroOnSpike        H = U·(1 − S)      硬复位：发放即清零
'   SubtractThreshold  H = U − θ·S        软复位：减去阈值，保留残余信息
'
' ---- 反向传播（BPTT + 代理梯度）--------------------------------------------
' 前向依赖链（以硬复位为例）：
'   I_rec[t] = S[t−1]·W ；I[t] = I_rec[t] + I_ext[t]
'   U[t] = β·H[t−1] + I[t] ；S[t] = Θ(U[t] − θ) ；H[t] = U[t]·(1 − S[t])
'
' 反向递推（t 从 T−1 到 0，gH/gU/gS 均表示 dL/d·）：
'   gH[t] = β·gU[t+1] + (直接输出梯度 dH_last，仅末步)
'   gS[t] = dS_ext[t] + gI_rec[t+1]·Wᵀ        ← S[t] 又是 t+1 步的递归输入
'   硬复位： gS[t] += −U[t]⊙gH[t] ；gU[t] = gU_ext[t] + gS[t]⊙σ′(U[t]−θ) + (1−S[t])⊙gH[t]
'   软复位： gS[t] += −θ·gH[t]    ；gU[t] = gU_ext[t] + gS[t]⊙σ′(U[t]−θ) + gH[t]
'   dW   += Σ_t S[t−1]ᵀ·gI_rec[t]             ← ∂I_rec/∂W = S[t−1]
'   dI_ext[t] = gU[t]                         ← ∂I/∂I_ext = 1
'
' 说明：初始膜电位 u0 被视为外部给定的常量（由数据映射得到），不产生梯度。
'
' ---- 梯度自检 --------------------------------------------------------------
' 与 LIFLayer 相同的技巧：把前向的 Heaviside 阶跃换成替代导数的原函数
' （见 Surrogate.SmoothSpike），此时"解析梯度 == 中心差分数值梯度"，
' 可用于验证本层 BPTT 实现的正确性。正常训练/推理必须保持 False。
'
' ---- 规模约束 --------------------------------------------------------------
' 本层使用稠密权重，单步计算 O(N²·batch)。推荐 N ≲ 1000；
' 超过 DenseNeuronLimit 时 Diagnose() 会给出告警，此时应改用
' SparseLIFLayer（CSR，仅前向）或先做基因子网络筛选。
' ============================================================================

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 递归 LIF 层 BPTT 的梯度结果。
''' </summary>
Public Structure RecurrentGradients

    ''' <summary>
    ''' 突触权重梯度 dL/dW [N, N]（<b>未乘</b>结构掩码）。
    ''' 上层如需冻结先验拓扑之外的突触，请调用
    ''' <see cref="RecurrentLIFLayer.MaskedWeightGrad"/> 或在优化前自行乘掩码。
    ''' </summary>
    Public WeightGrad As Tensor

    ''' <summary>
    ''' 外部注入电流的梯度 dL/dI_ext[t]，长度 = 仿真步数。
    ''' 当输入电流直接来自数据（无需回传梯度）时可以忽略。
    ''' </summary>
    Public InputGrad As List(Of Tensor)

    ''' <summary>
    ''' 各时间步输出脉冲的梯度 dL/dS[t]，长度 = 仿真步数。
    ''' </summary>
    Public SpikeGrad As List(Of Tensor)

End Structure

''' <summary>
''' 可训练的递归 LIF 层：同一神经元群内部的稠密稀疏化突触连接，
''' 支持结构掩码冻结、硬/软复位与代理梯度 BPTT。
''' </summary>
''' <remarks>
''' <para>
''' 本层是"脉冲递归网络"（SRNN）的基本构件，可直接用于基因调控网络建模
''' （每个基因一个神经元、TF→Target 为有向突触）、连接组仿真等场景。
''' </para>
''' <para>
''' 时间步之间的膜电位 H 与上一时刻脉冲 S[t−1] 都是状态：每个样本批次开始前
''' 必须调用 <see cref="ResetState"/>，否则上一批的"记忆"会污染本批结果。
''' </para>
''' </remarks>
Public Class RecurrentLIFLayer

#Region "规模约束"

    ''' <summary>
    ''' 稠密递归层的推荐神经元数上限。超过该规模时 <see cref="Diagnose"/> 给出告警，
    ''' 提示改用 CSR 稀疏实现（<see cref="SparseLIFLayer"/>）或先做子网络筛选。
    ''' </summary>
    Public Const DenseNeuronLimit As Integer = 1000

#End Region

#Region "超参数与参数"

    Public ReadOnly Property Name As String

    ''' <summary>神经元数量 N（= Weight 的行列数）</summary>
    Public ReadOnly Property Units As Integer

    ''' <summary>
    ''' 突触权重矩阵 W[N, N]（行 = 突触前，列 = 突触后）。
    ''' 先验拓扑之外的突触应保持为 0（由 <see cref="Mask"/> 约束）。
    ''' </summary>
    Public Property Weight As Tensor

    ''' <summary>
    ''' 结构掩码 [N, N]：1 = 该突触允许参与梯度更新，0 = 冻结（先验拓扑之外）。
    ''' 为 Nothing 时视为全 1（全连接、全部可训练）。
    ''' </summary>
    Public Property Mask As Tensor

    ''' <summary>膜电位衰减系数 β ∈ (0,1)，越小遗忘越快（β = exp(−Δt/τ_m)）</summary>
    Public Property Beta As Double

    ''' <summary>发放阈值 U_thr</summary>
    Public Property Threshold As Double

    ''' <summary>复位模式</summary>
    Public Property ResetMode As LIFResetMode

    ''' <summary>替代梯度函数类型</summary>
    Public Property SurrogateType As SurrogateKind

    ''' <summary>
    ''' 替代梯度陡度参数 α。
    ''' 该值可按训练轮次退火（逐步增大陡度）以缓解"梯度不匹配"随时间的累积。
    ''' </summary>
    Public Property Alpha As Double

    ''' <summary>
    ''' 权重是否参与梯度累积。设为 False 时 <see cref="BackwardTime"/> 跳过 O(N²) 的
    ''' 权重梯度累加（此时本层退化为固定权重的递归层，类似 SparseLIFLayer 的语义）。
    ''' </summary>
    Public Property Trainable As Boolean = True

    ''' <summary>
    ''' 梯度自检开关：前向用光滑函数 f(u) 代替 Heaviside 阶跃（f′ 恰为替代导数），
    ''' 使"解析梯度 == 数值差分梯度"。正常训练/推理必须为 False。
    ''' </summary>
    Public Property SmoothForward As Boolean = False

#End Region

#Region "状态与前向缓存"

    ''' <summary>膜电位 H（跨时间步持续）[batch, Units]</summary>
    Private _H As Tensor

    ''' <summary>各步开始时用到的"上一时刻脉冲" S[t−1]（t=0 时为全 0）</summary>
    Private _Sprev As List(Of Tensor)

    ''' <summary>各步触发前膜电位 U[t]</summary>
    Private _U As List(Of Tensor)

    ''' <summary>各步输出脉冲 S[t]</summary>
    Private _S As List(Of Tensor)

    ''' <summary>各步外部注入电流 I_ext[t]（可为 Nothing）</summary>
    Private _X As List(Of Tensor)

    Private _batchSize As Integer

    ''' <summary>最近一次仿真的脉冲轨迹 S[t]（可视化/解码用）</summary>
    Public ReadOnly Property SHistory As List(Of Tensor)
        Get
            Return _S
        End Get
    End Property

    ''' <summary>最近一次仿真的膜电位轨迹 U[t]（触发前，可视化/解码用）</summary>
    Public ReadOnly Property UHistory As List(Of Tensor)
        Get
            Return _U
        End Get
    End Property

    ''' <summary>最近一次仿真中作为递归输入的上一时刻脉冲轨迹 S[t−1]</summary>
    Public ReadOnly Property SprevHistory As List(Of Tensor)
        Get
            Return _Sprev
        End Get
    End Property

    ''' <summary>当前膜电位状态 H[t]（仿真结束后即为最后一步的复位结果）</summary>
    Public ReadOnly Property MembraneState As Tensor
        Get
            Return _H
        End Get
    End Property

    ''' <summary>最近一次仿真最后一步的触发前膜电位 U[T−1]（膜电位读取解码用）</summary>
    Public ReadOnly Property ULast As Tensor
        Get
            If _U Is Nothing OrElse _U.Count = 0 Then Return Nothing
            Return _U(_U.Count - 1)
        End Get
    End Property

    ''' <summary>最近一次仿真最后一步的复位后膜电位 H[T−1]</summary>
    Public ReadOnly Property HLast As Tensor
        Get
            Return _H
        End Get
    End Property

    ''' <summary>仿真步数（最近一次前向的时间步数）</summary>
    Public ReadOnly Property TimeSteps As Integer
        Get
            If _U Is Nothing Then Return 0
            Return _U.Count
        End Get
    End Property

    ''' <summary>权重梯度（由 <see cref="BackwardTime"/> 填充）</summary>
    Public Property WeightGrad As Tensor

#End Region

#Region "构造与状态重置"

    ''' <summary>
    ''' 构建可训练的递归 LIF 层。
    ''' </summary>
    ''' <param name="name">层名称（用于优化器区分动量状态）</param>
    ''' <param name="units">神经元数量 N</param>
    ''' <param name="initialWeight">
    ''' 初始突触权重 W[N, N]（行 = 突触前，列 = 突触后）。为 Nothing 时使用 He 初始化。
    ''' 基因调控网络场景下应传入由先验网络构造的 W_init。
    ''' </param>
    ''' <param name="mask">
    ''' 结构掩码 [N, N]（1 = 可训练，0 = 冻结）。为 Nothing 时视为全部可训练。
    ''' </param>
    Public Sub New(name As String, units As Integer,
                   Optional initialWeight As Tensor = Nothing,
                   Optional mask As Tensor = Nothing,
                   Optional beta As Double = 0.9,
                   Optional threshold As Double = 1.0,
                   Optional resetMode As LIFResetMode = LIFResetMode.ZeroOnSpike,
                   Optional surrogate As SurrogateKind = SurrogateKind.FastSigmoid,
                   Optional alpha As Double = 2.0,
                   Optional seed As Integer? = Nothing)

        If units <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(units), "神经元数量必须为正整数")
        End If
        If beta <= 0.0 OrElse beta >= 1.0 Then
            Throw New ArgumentOutOfRangeException(NameOf(beta), $"β 必须落在 (0,1) 区间，实际 {beta}")
        End If

        Me.Name = name
        Me.Units = units
        Me.Beta = beta
        Me.Threshold = threshold
        Me.ResetMode = resetMode
        Me.SurrogateType = surrogate
        Me.Alpha = alpha

        If initialWeight Is Nothing Then
            Me.Weight = Tensor.HeInit(units, units, seed)
        Else
            AssertSquare(initialWeight, "initialWeight")
            Me.Weight = New Tensor(initialWeight.ToDoubleArray(), units, units)
        End If

        If mask Is Nothing Then
            Me.Mask = Tensor.Ones(New Integer() {units, units})
        Else
            AssertSquare(mask, "mask")
            Me.Mask = New Tensor(mask.ToDoubleArray(), units, units)
        End If

        Me.WeightGrad = New Tensor(units, units)
        _batchSize = 1
        _H = New Tensor(1, units)
        _Sprev = New List(Of Tensor)()
        _U = New List(Of Tensor)()
        _S = New List(Of Tensor)()
        _X = New List(Of Tensor)()
    End Sub

    ''' <summary>
    ''' 重置膜电位、递归脉冲状态与时间缓存。每个样本批次开始前必须调用！
    ''' </summary>
    ''' <param name="batchSize">批量大小</param>
    ''' <param name="u0">
    ''' 初始膜电位 [batch, Units]；为 Nothing 时初始化为全 0。
    ''' 基因调控网络中可由 t 时刻的表达状态映射得到（u0 = gain · U_seq[t]）。
    ''' </param>
    Public Sub ResetState(batchSize As Integer, Optional u0 As Tensor = Nothing)
        If batchSize <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(batchSize), "批量大小必须为正整数")
        End If

        If u0 Is Nothing Then
            _H = New Tensor(batchSize, Units)
        Else
            If u0.Rank <> 2 OrElse u0.Shape(0) <> batchSize OrElse u0.Shape(1) <> Units Then
                Throw New ArgumentException(
                    $"初始膜电位形状应为 [batch, {Units}]，实际 [{String.Join(",", u0.Shape)}]")
            End If
            _H = New Tensor(u0.ToDoubleArray(), batchSize, Units)
        End If

        _batchSize = batchSize
        _Sprev = New List(Of Tensor)()
        _U = New List(Of Tensor)()
        _S = New List(Of Tensor)()
        _X = New List(Of Tensor)()
    End Sub

    ''' <summary>把当前位置的突触权重裁剪回掩码约束（掩码为 0 的位置置零）</summary>
    ''' <remarks>
    ''' 就地写入底层数组后按既有设备端缓存契约调用 <see cref="Tensor.MarkHostModified"/>。
    ''' </remarks>
    Public Sub ApplyMaskToWeight()
        If Mask Is Nothing Then Return
        If Not HasFrozenSynapses() Then Return

        Dim w = Weight.Data
        Dim m = Mask.Data
        For i = 0 To w.Length - 1
            If m(i) = 0.0 Then w(i) = 0.0
        Next
        Weight.MarkHostModified()
    End Sub

#End Region

#Region "前向传播"

    ''' <summary>
    ''' 单时间步前向：外部注入电流 → 递归突触输入 → 泄漏积分 → 阈值触发 → 复位。
    ''' </summary>
    ''' <param name="externalCurrent">
    ''' 外部注入电流 I_ext[t] [batch, Units]；为 Nothing 表示本步无外部刺激。
    ''' </param>
    ''' <returns>本时间步的输出脉冲 S[t] [batch, Units]</returns>
    Public Function ForwardStep(Optional externalCurrent As Tensor = Nothing) As Tensor
        ' S[t−1]：上一步的输出脉冲（t=0 时为全 0）
        Dim Sprev As Tensor
        If _S.Count = 0 Then
            Sprev = New Tensor(_batchSize, Units)
        Else
            Sprev = _S(_S.Count - 1)
        End If

        ' 1. 递归输入：上一时刻脉冲经突触矩阵回灌（含循环连接与自反馈）
        Dim Irec = Sprev.MatMul(Weight)

        ' 2. 叠加外部注入电流
        Dim I As Tensor
        If externalCurrent Is Nothing Then
            I = Irec
        Else
            If externalCurrent.Rank <> 2 OrElse
               externalCurrent.Shape(0) <> _batchSize OrElse
               externalCurrent.Shape(1) <> Units Then
                Throw New ArgumentException(
                    $"externalCurrent 形状应为 [{_batchSize}, {Units}]，实际 [{String.Join(",", externalCurrent.Shape)}]")
            End If
            I = Irec + externalCurrent
        End If

        ' 3. 泄漏积分：U = β·H + I
        Dim U = I + (_H * CSng(Beta))

        ' 4. 阈值触发：S = Θ(U − θ)（自检模式下用光滑替代函数）
        Dim S As Tensor
        If SmoothForward Then
            S = U.Apply(Function(v As Double) Surrogate.SmoothSpike(v - Threshold, SurrogateType, Alpha))
        Else
            S = U.Apply(Function(v As Double) Surrogate.Spike(v - Threshold))
        End If

        ' 5. 复位
        Dim H As Tensor
        If ResetMode = LIFResetMode.ZeroOnSpike Then
            H = U.ElementwiseMultiply(S.Apply(Function(v As Double) 1.0 - v))
        Else
            H = U - (S * CSng(Threshold))
        End If

        _X.Add(externalCurrent)
        _Sprev.Add(Sprev)     ' 记录本步实际使用的 S[t−1]
        _U.Add(U)
        _S.Add(S)
        _H = H

        Return S
    End Function

    ''' <summary>
    ''' 按给定电流序列连续前向若干时间步（内部逐帧调用 <see cref="ForwardStep"/>）。
    ''' </summary>
    ''' <param name="externalCurrents">外部注入电流序列；为 Nothing 表示全时段无外部刺激</param>
    ''' <param name="u0">初始膜电位 [batch, Units]，见 <see cref="ResetState"/></param>
    ''' <returns>输出脉冲序列 S[0..T−1]</returns>
    Public Function ForwardSequence(externalCurrents As List(Of Tensor),
                                    Optional u0 As Tensor = Nothing) As List(Of Tensor)
        If externalCurrents Is Nothing OrElse externalCurrents.Count = 0 Then
            Throw New ArgumentException("电流序列不能为空", NameOf(externalCurrents))
        End If

        Dim batch = externalCurrents(0).Shape(0)
        ResetState(batch, u0)

        Dim out As New List(Of Tensor)()
        For t = 0 To externalCurrents.Count - 1
            out.Add(ForwardStep(externalCurrents(t)))
        Next
        Return out
    End Function

#End Region

#Region "反向传播（BPTT + 代理梯度）"

    ''' <summary>
    ''' 沿时间反向传播，计算突触权重梯度与外部电流梯度。
    ''' </summary>
    ''' <param name="dS_ext">上游对每步脉冲的梯度 dL/dS[t]（长度必须等于前向步数）</param>
    ''' <param name="dH_last">直接作用在末步复位后膜电位 H[T−1] 上的梯度（如解码头回传）</param>
    ''' <param name="dU_last">直接作用在末步触发前膜电位 U[T−1] 上的梯度（膜电位读取解码）</param>
    ''' <returns>权重梯度、外部电流梯度与脉冲梯度</returns>
    Public Function BackwardTime(dS_ext As List(Of Tensor),
                                 Optional dH_last As Tensor = Nothing,
                                 Optional dU_last As Tensor = Nothing) As RecurrentGradients

        Dim steps = _U.Count
        If steps = 0 Then
            Throw New InvalidOperationException("尚未执行前向仿真，无法反向传播")
        End If
        If dS_ext Is Nothing Then
            dS_ext = EmptySpikeGradients(steps)
        ElseIf dS_ext.Count <> steps Then
            Throw New ArgumentException(
                $"dS_ext 步数({dS_ext.Count})与前向仿真步数({steps})不一致", NameOf(dS_ext))
        End If

        Dim accumulateW = Trainable
        Dim gW As New Tensor(Units, Units)
        Dim gInput(steps - 1) As Tensor
        Dim gSpike(steps - 1) As Tensor

        ' dL/dU[t+1]：下一步的膜电位梯度，经 β 回流成为 H[t] 的梯度
        Dim gUnext As Tensor = Nothing
        ' dL/dI_rec[t+1]：下一步的递归输入梯度，经 Wᵀ 回流成为 S[t] 的梯度
        Dim gIrecNext As Tensor = Nothing

        For t = steps - 1 To 0 Step -1
            Dim U = _U(t)
            Dim S = _S(t)
            Dim Sprev = _Sprev(t)

            ' ---- H[t] 的梯度：β·gU[t+1] 与直接输出梯度 ----
            Dim gH As Tensor = Nothing
            If gUnext IsNot Nothing Then
                gH = gUnext * CSng(Beta)
            End If
            If t = steps - 1 AndAlso dH_last IsNot Nothing Then
                gH = AddTensor(gH, dH_last)
            End If

            ' ---- S[t] 的梯度：上游 + 下一步递归输入经 Wᵀ 的回流 ----
            Dim gS = dS_ext(t)
            If gIrecNext IsNot Nothing Then
                gS = gS + gIrecNext.MatMul(Weight.Transpose())
            End If

            ' ---- 复位路径：∂H/∂S 与 ∂H/∂U ----
            Dim gU As Tensor
            Dim surr = U.Apply(Function(v As Double) Surrogate.Derivative(v - Threshold, SurrogateType, Alpha))

            If ResetMode = LIFResetMode.ZeroOnSpike Then
                Dim oneMinusS = S.Apply(Function(v As Double) 1.0 - v)
                If gH IsNot Nothing Then
                    gS = gS - gH.ElementwiseMultiply(U)          ' ∂H/∂S = −U
                End If
                gU = gS.ElementwiseMultiply(surr)                ' 替代导数
                If gH IsNot Nothing Then
                    gU = gU + gH.ElementwiseMultiply(oneMinusS)  ' ∂H/∂U = 1−S
                End If
            Else
                If gH IsNot Nothing Then
                    gS = gS - (gH * CSng(Threshold))             ' ∂H/∂S = −θ
                End If
                gU = gS.ElementwiseMultiply(surr)
                If gH IsNot Nothing Then
                    gU = gU + gH                                 ' ∂H/∂U = 1
                End If
            End If

            ' ---- 末步的直接膜电位梯度 ----
            If t = steps - 1 AndAlso dU_last IsNot Nothing Then
                gU = gU + dU_last
            End If

            ' ---- dW += S[t−1]ᵀ·gI_rec[t]（∂I_rec/∂W = S[t−1]）----
            If accumulateW Then
                gW = gW + Sprev.Transpose().MatMul(gU)
            End If

            ' dI_ext[t] = dL/dI_rec[t] = dL/dU[t]
            gInput(t) = gU
            gSpike(t) = gS

            gUnext = gU
            gIrecNext = gU
        Next

        WeightGrad = gW

        Return New RecurrentGradients With {
            .WeightGrad = gW,
            .InputGrad = gInput.ToList(),
            .SpikeGrad = gSpike.ToList()
        }
    End Function

    ''' <summary>
    ''' 返回已乘结构掩码的权重梯度（掩码为 0 的突触梯度归零），
    ''' 可直接交给 <see cref="AdamOptimizer"/> 更新，从而保证先验拓扑之外
    ''' 的突触权重始终为 0。
    ''' </summary>
    Public Function MaskedWeightGrad() As Tensor
        If WeightGrad Is Nothing Then
            Return New Tensor(Units, Units)
        End If
        If Not HasFrozenSynapses() Then
            Return WeightGrad
        End If
        Return WeightGrad.ElementwiseMultiply(Mask)
    End Function

    ''' <summary>构造一个全零的脉冲梯度序列（当上游不需要对脉冲求导时使用）</summary>
    Public Function EmptySpikeGradients(steps As Integer) As List(Of Tensor)
        Dim list As New List(Of Tensor)()
        For t = 1 To steps
            list.Add(New Tensor(_batchSize, Units))
        Next
        Return list
    End Function

    ''' <summary>可空张量加法（任一为空时返回另一个的副本）</summary>
    Private Shared Function AddTensor(a As Tensor, b As Tensor) As Tensor
        If a Is Nothing Then Return b
        If b Is Nothing Then Return a
        Return a + b
    End Function

#End Region

#Region "诊断"

    ''' <summary>
    ''' 对最近一次前向仿真做健康检查（全静默 / 过度发放 / 规模超限），
    ''' 返回人类可读的告警列表；空列表表示未见异常。
    ''' </summary>
    ''' <remarks>
    ''' readme 明确指出的两类失败模式：整体静默（Dropout 式虚假零值导致无脉冲）
    ''' 与全体持续发放（权重尺度过大）。这里是廉价的前置体检。
    ''' </remarks>
    Public Function Diagnose() As List(Of String)
        Dim warns As New List(Of String)()

        If Units > DenseNeuronLimit Then
            warns.Add($"[{Name}] 神经元数 N={Units} 超过稠密递归层推荐上限 " &
                      $"{DenseNeuronLimit}，单步计算 O(N²) 可能过慢；" &
                      "建议先做子网络筛选，或改用 SparseLIFLayer（CSR，仅前向）")
        End If

        If _S Is Nothing OrElse _S.Count = 0 Then
            Return warns
        End If

        Dim total = 0.0
        Dim active As New HashSet(Of Integer)()
        For Each s In _S
            Dim d = s.Data
            For i = 0 To d.Length - 1
                If d(i) > 0 Then
                    total += 1.0
                    If active.Count < Units Then
                        active.Add(i Mod Units)
                    End If
                End If
            Next
        Next

        Dim rate = total / (_S.Count * CDbl(_batchSize) * Units)
        If rate <= 0.0 Then
            warns.Add($"[{Name}] 最近一次仿真完全静默（发放率为 0）：" &
                      "请检查输入电流量级、阈值（Threshold）与初始权重尺度")
        ElseIf rate > 0.5 Then
            warns.Add($"[{Name}] 最近一次仿真发放率 {rate:P1} 过高（疑似持续发放）：" &
                      "请检查权重归一化、阈值与复位模式")
        End If

        If active.Count > 0 AndAlso active.Count < Units / 10 Then
            warns.Add($"[{Name}] 仅 {active.Count}/{Units} 个神经元曾被激活（存活性偏低）")
        End If

        Return warns
    End Function

    ''' <summary>是否真正启用了掩码约束（掩码非空且存在 0 元素）</summary>
    Private Function HasFrozenSynapses() As Boolean
        If Mask Is Nothing Then Return False
        Dim m = Mask.Data
        For i = 0 To m.Length - 1
            If m(i) = 0.0 Then Return True
        Next
        Return False
    End Function

    Private Shared Sub AssertSquare(t As Tensor, paramName As String)
        If t Is Nothing Then
            Throw New ArgumentNullException(paramName)
        End If
        If t.Rank <> 2 OrElse t.Shape(0) <> t.Shape(1) Then
            Throw New ArgumentException(
                $"{paramName} 必须为方阵 [N, N]，实际 [{String.Join(",", t.Shape)}]", paramName)
        End If
    End Sub

#End Region

    Public Overrides Function ToString() As String
        Return $"{Name}(RecurrentLIF, N={Units}, β={Beta}, θ={Threshold}, reset={ResetMode})"
    End Function

End Class

