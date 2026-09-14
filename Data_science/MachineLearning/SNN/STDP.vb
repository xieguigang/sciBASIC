' ============================================================================
' STDP.vb — 脉冲时序依赖可塑性（Spike-Timing-Dependent Plasticity）
'
' Hebb 学习规则的时序化（readme 第三节）：
'   pre 脉冲先于 post 到达 → 突触增强（LTP，长时程增强）
'   post 脉冲先于 pre 到达 → 突触削弱（LTD，长时程抑制）
'   Δw(t) ∝ +A₊·exp(−Δt/τ₊)  当 Δt = t_post − t_pre > 0
'   Δw(t) ∝ −A₋·exp(−|Δt|/τ₋) 当 Δt < 0
'
' 在线等价实现（指数衰减迹 + 事件驱动更新）：
'   preTrace[i]  = Σ exp(−(t−t_k)/τ)·S_pre[k,i]    pre 脉冲指数衰减迹
'   postTrace[j] = 同上（post 脉冲迹）
'   post 发放时:  w[i,j] += A₊ · preTrace[i]        （LTP）
'   pre 发放时:   w[i,j] −= A₋ · postTrace[j]       （LTD）
'   w clip 到 [0, WMax]
'
' 输出层采用赢者通吃（WTA）：每步至多一个神经元发放并复位，
' 通过竞争促进不同神经元特化到不同输入模式（马太效应）。
' ============================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Namespace SpikingNN

    Public Class STDPLayer

        ''' <summary>突触权重 [InputSize, OutputSize]，取值 [0, WMax]</summary>
        Public Property Weight As Tensor

        ''' <summary>LTP 学习率（potentiation）。pre 脉冲数远多于 post，A₊/A₋ 需与迹量级匹配</summary>
        Public Property APlus As Double = 0.004

        ''' <summary>LTD 学习率（depression）</summary>
        Public Property AMinus As Double = 0.003

        ''' <summary>脉冲迹衰减系数 = exp(−Δt/τ)，τ≈20 步 → 0.95</summary>
        Public Property TraceDecay As Double = 0.95

        ''' <summary>权重上限</summary>
        Public Property WMax As Double = 1.0

        ''' <summary>
        ''' 突触归一化目标：每个输出神经元的入边权重总和。
        ''' STDP 的经典搭档机制——当"获胜通路"被 LTP 增强后，归一化会
        ''' 自动按比例削弱同一神经元的其余连接（守恒竞争），
        ''' 从机制上保证模式特化。设为 0 表示禁用。
        ''' </summary>
        Public Property NormalizeSum As Double = 0.0

        ''' <summary>
        ''' 把每个输出神经元的入边权重和缩放到 NormalizeSum（列归一化）。
        ''' 建议在每个模式呈现结束后调用。
        ''' </summary>
        Public Sub NormalizeWeights()
            If NormalizeSum <= 0 Then Return
            For j = 0 To _out - 1
                Dim s = 0.0
                For i = 0 To _in - 1 : s += Weight(i, j) : Next
                If s > 0.000001 Then
                    Dim scale = NormalizeSum / s
                    For i = 0 To _in - 1
                        Weight(i, j) = std.Max(0.0, std.Min(WMax, Weight(i, j) * scale))
                    Next
                End If
            Next
        End Sub

        ''' <summary>膜电位衰减系数 β</summary>
        Public Property Beta As Double = 0.9

        ''' <summary>发放阈值基线</summary>
        Public Property Threshold As Double = 1.0

        ''' <summary>
        ''' 自适应阈值（intrinsic plasticity）：每次发放后阈值抬升 AdaptRate，
        ''' 并按 AdaptDecay 指数回落。发放越频繁的神经元有效阈值越高，
        ''' 打破"赢家垄断"，促使输出神经元轮流接管不同输入模式 → 促进分化。
        ''' </summary>
        Public Property AdaptRate As Double = 0.03
        ''' <summary>自适应阈值的每步衰减（0.99 ≈ 100 步时间尺度）</summary>
        Public Property AdaptDecay As Double = 0.99

        Private _in As Integer
        Private _out As Integer
        Private _preTrace As Double()    ' [inputSize]
        Private _postTrace As Double()   ' [outputSize]
        Private _U As Double()           ' 膜电位（单样本在线推理）
        Private _adapt As Double()       ' 自适应阈值分量（跨呈现保留）

        Public Sub New(inputSize As Integer, outputSize As Integer, Optional seed As Integer? = Nothing)
            _in = inputSize
            _out = outputSize
            Weight = Tensor.Random(New Integer() {inputSize, outputSize}, 0.0F, 0.25F, seed)
            _adapt = New Double(outputSize - 1) {}   ' 自适应阈值跨呈现保留，仅在构造时清零
            Call Reset()
        End Sub

        ''' <summary>
        ''' 清空膜电位与脉冲迹（每次模式呈现前调用）。
        ''' 注意：自适应阈值 _adapt 有意跨呈现保留——它记录的是每个神经元的
        ''' "历史发放负荷"，Reset 后清零会让同一个赢家立刻重新垄断。
        ''' </summary>
        Public Sub Reset()
            _preTrace = New Double(_in - 1) {}
            _postTrace = New Double(_out - 1) {}
            _U = New Double(_out - 1) {}
        End Sub

        ''' <summary>
        ''' 单时间步：输入脉冲 → LIF 动态（WTA 触发）→ STDP 权重更新 → 迹更新。
        ''' </summary>
        ''' <param name="x">输入脉冲 [1, InputSize]</param>
        ''' <returns>输出脉冲 [1, OutputSize]（至多一个 1）</returns>
        Public Function [Step](x As Tensor) As Tensor
            If x.Rank <> 2 OrElse x.Shape(0) <> 1 OrElse x.Shape(1) <> _in Then
                Throw New ArgumentException($"STDP 输入形状应为 [1, {_in}]")
            End If

            ' 1. 积分 + 泄漏：U = β·U + x·W
            For j = 0 To _out - 1
                Dim s = 0.0
                For i = 0 To _in - 1
                    s += x.Data(i) * Weight(i, j)
                Next
                _U(j) = Beta * _U(j) + s
            Next

            ' 2. WTA 触发：有效阈值 θ_j = Threshold + 自适应分量；
            '    膜电位最高且超过自身有效阈值者发放并复位
            Dim spike = New Tensor(1, _out)
            Dim bestJ = -1
            Dim bestMargin = -1.0
            For j = 0 To _out - 1
                Dim margin = _U(j) - (Threshold + _adapt(j))
                If margin >= 0.0 AndAlso margin > bestMargin Then
                    bestMargin = margin
                    bestJ = j
                End If
            Next
            If bestJ >= 0 Then
                spike.Data(bestJ) = 1.0
                _U(bestJ) = 0.0
                _adapt(bestJ) += AdaptRate
            End If
            ' 自适应阈值回落
            For j = 0 To _out - 1
                _adapt(j) *= AdaptDecay
            Next

            ' 3. STDP 权重更新（使用更新前的旧迹）
            '    LTP: post 发放 → 用 preTrace 加强所有"最近活跃"的输入连接
            For j = 0 To _out - 1
                If spike.Data(j) > 0 Then
                    For i = 0 To _in - 1
                        If _preTrace(i) > 0 Then
                            Weight(i, j) = std.Min(WMax, Weight(i, j) + APlus * _preTrace(i))
                        End If
                    Next
                End If
            Next
            '    LTD: pre 发放 → 用 postTrace 削弱所有"最近发放过"的输出连接
            For i = 0 To _in - 1
                If x.Data(i) > 0 Then
                    For j = 0 To _out - 1
                        If _postTrace(j) > 0 Then
                            Weight(i, j) = std.Max(0.0, Weight(i, j) - AMinus * _postTrace(j))
                        End If
                    Next
                End If
            Next

            ' 4. 更新脉冲迹
            For i = 0 To _in - 1
                _preTrace(i) = TraceDecay * _preTrace(i) + x.Data(i)
            Next
            For j = 0 To _out - 1
                _postTrace(j) = TraceDecay * _postTrace(j) + spike.Data(j)
            Next

            Return spike
        End Function

    End Class

End Namespace
