' ============================================================================
' LIFLayer.vb — LIF（Leaky Integrate-and-Fire）脉冲神经元层
'
' 前向动态（每个时间步 t，readme 模块二）：
'   I[t] = X[t]·W            积分：加权输入电流
'   U[t] = β·H[t−1] + I[t]   泄漏：膜电位按 β 衰减后叠加输入
'   S[t] = Θ(U[t] − U_thr)   触发：超过阈值发放二值脉冲
'   H[t] = 复位(U[t], S[t])  复位：发放后膜电位回退
'
' 复位模式：
'   ZeroOnSpike        H[t] = U[t]·(1 − S[t])        发放即清零（readme 模块二默认）
'   SubtractThreshold  H[t] = U[t] − U_thr·S[t]      软复位（减去阈值）
'
' 膜电位 H 跨时间步持续——SNN 与 RNN 同理但更严格：
' 每个样本批次开始前必须 ResetState()，否则上一批的"记忆"污染本批结果。
'
' 反向传播：BPTT（沿时间展开）+ 替代梯度，见 BackwardTime 注释中的递推式。
' ============================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace SpikingNN

    ''' <summary>膜电位复位模式</summary>
    Public Enum LIFResetMode
        ''' <summary>H = U·(1−S)：发放后膜电位清零（硬复位）</summary>
        ZeroOnSpike
        ''' <summary>H = U − U_thr·S：发放后减去阈值（软复位）</summary>
        SubtractThreshold
    End Enum

    Public Class LIFLayer

#Region "超参数与参数"

        Public ReadOnly Property Name As String
        Public ReadOnly Property InputSize As Integer
        Public ReadOnly Property Units As Integer

        ''' <summary>突触权重矩阵 [InputSize, Units]</summary>
        Public Property Weight As Tensor

        ''' <summary>权重梯度（BackwardTime 中累积）[InputSize, Units]</summary>
        Public Property WeightGrad As Tensor

        ''' <summary>膜电位衰减系数 β ∈ (0,1)，越小遗忘越快</summary>
        Public Property Beta As Double

        ''' <summary>发放阈值 U_thr</summary>
        Public Property Threshold As Double

        ''' <summary>复位模式</summary>
        Public Property ResetMode As LIFResetMode

        ''' <summary>替代梯度函数类型</summary>
        Public Property SurrogateType As SurrogateKind

        ''' <summary>替代梯度陡度参数 α（fast sigmoid: σ' = 1/(α|u|+1)²）</summary>
        Public Property Alpha As Double

        ''' <summary>
        ''' 梯度自检开关：前向用光滑函数 f(u) 代替 Heaviside 阶跃。
        ''' f'(u) 恰好等于对应的替代导数，因此"解析梯度 == 数值差分梯度"
        ''' 可用来验证 BPTT 实现的正确性。正常训练/推理必须为 False。
        ''' </summary>
        Public Property SmoothForward As Boolean = False

#End Region

#Region "状态与前向缓存"

        ''' <summary>膜电位 H（跨时间步持续）[batch, Units]</summary>
        Private _H As Tensor

        Private _X As List(Of Tensor)   ' 各时间步输入 X[t]
        Private _U As List(Of Tensor)   ' 各时间步触发前膜电位 U[t]
        Private _S As List(Of Tensor)   ' 各时间步脉冲 S[t]

        ''' <summary>最近一次前向仿真的膜电位轨迹（可视化用）</summary>
        Public ReadOnly Property UHistory As List(Of Tensor)
            Get
                Return _U
            End Get
        End Property

        ''' <summary>最近一次前向仿真的脉冲轨迹（可视化用）</summary>
        Public ReadOnly Property SHistory As List(Of Tensor)
            Get
                Return _S
            End Get
        End Property

#End Region

#Region "构造与状态重置"

        Public Sub New(name As String, inputSize As Integer, units As Integer,
                       Optional beta As Double = 0.9,
                       Optional threshold As Double = 1.0,
                       Optional resetMode As LIFResetMode = LIFResetMode.ZeroOnSpike,
                       Optional surrogate As SurrogateKind = SurrogateKind.FastSigmoid,
                       Optional alpha As Double = 2.0,
                       Optional seed As Integer? = Nothing)

            Me.Name = name
            Me.InputSize = inputSize
            Me.Units = units
            Me.Beta = beta
            Me.Threshold = threshold
            Me.ResetMode = resetMode
            Me.SurrogateType = surrogate
            Me.Alpha = alpha

            ' He 初始化（阈值触发型激活的方差适配）
            Weight = Tensor.HeInit(inputSize, units, seed)
            WeightGrad = New Tensor(inputSize, units)

            _H = New Tensor(1, units)
            _X = New List(Of Tensor)()
            _U = New List(Of Tensor)()
            _S = New List(Of Tensor)()
        End Sub

        ''' <summary>
        ''' 重置膜电位与时间缓存。每个样本批次开始前必须调用！
        ''' </summary>
        Public Sub ResetState(batchSize As Integer)
            _H = New Tensor(batchSize, Units)
            _X = New List(Of Tensor)()
            _U = New List(Of Tensor)()
            _S = New List(Of Tensor)()
        End Sub

#End Region

#Region "前向传播"

        ''' <summary>
        ''' 单时间步前向：X[t] → S[t]
        ''' 积分 → 泄漏 → 触发 → 复位，并缓存中间量供 BPTT 使用。
        ''' </summary>
        Public Function ForwardStep(x As Tensor) As Tensor
            Dim I = x.MatMul(Weight)                        ' 1. 积分：I = X·W
            Dim U = I + (_H * CSng(Beta))                   ' 2. 泄漏：U = β·H + I

            Dim S As Tensor                                 ' 3. 触发：S = Θ(U − U_thr)
            If SmoothForward Then
                S = U.Apply(Function(v As Double) Surrogate.SmoothSpike(v - Threshold, SurrogateType, Alpha))
            Else
                S = U.Apply(Function(v As Double) Surrogate.Spike(v - Threshold))
            End If

            Dim H As Tensor                                 ' 4. 复位
            If ResetMode = LIFResetMode.ZeroOnSpike Then
                H = U.ElementwiseMultiply(S.Apply(Function(v As Double) 1.0 - v))
            Else
                H = U - (S * CSng(Threshold))
            End If

            _X.Add(x)
            _U.Add(U)
            _S.Add(S)
            _H = H

            Return S
        End Function

#End Region

#Region "反向传播（BPTT + 替代梯度）"

        ''' <summary>
        ''' 沿时间反向传播。
        '''
        ''' 前向依赖链（ZeroOnSpike 为例）：
        '''   U[t] = β·H[t−1] + X[t]·W
        '''   S[t] = Θ(U[t] − θ)
        '''   H[t] = U[t]·(1 − S[t])
        '''
        ''' 反向递推（从 T−1 到 0）：
        '''   dH[t]  = β·dU[t+1]                              ← H[t] 只经 U[t+1] 影响损失
        '''   dS[t]  = dS_ext[t] − dH[t]⊙U[t]                 ← ∂H/∂S = −U
        '''   dU[t]  = dS[t]⊙σ'(U[t]−θ) + dH[t]⊙(1−S[t])     ← 替代梯度 + ∂H/∂U = 1−S
        '''   dW    += X[t]ᵀ·dU[t]                            ← dL/dI[t] = dL/dU[t]
        '''   dX[t]  = dU[t]·Wᵀ
        '''
        ''' SubtractThreshold 模式把复位导数换为 ∂H/∂S = −θ、∂H/∂U = 1。
        ''' </summary>
        ''' <param name="dS_ext">上游对每步脉冲的梯度 dL/dS[t]（[batch, Units] × T）</param>
        ''' <returns>各时间步的 dL/dX[t]（传给更早的层）</returns>
        Public Function BackwardTime(dS_ext As List(Of Tensor)) As List(Of Tensor)
            Dim T = _U.Count
            If dS_ext.Count <> T Then
                Throw New ArgumentException(
                    $"dS_ext 步数({dS_ext.Count})与前向仿真步数({T})不一致")
            End If

            WeightGrad = New Tensor(InputSize, Units)
            Dim dX As New List(Of Tensor)()
            Dim dU_next As Tensor = Nothing     ' dL/dU[t+1]

            For t = T - 1 To 0 Step -1
                Dim U = _U(t)
                Dim S = _S(t)
                Dim X = _X(t)

                ' dH[t] = β·dU[t+1]（t = T−1 时为 0）
                Dim dH As Tensor =
                    If(dU_next Is Nothing, New Tensor(U.Shape), dU_next * CSng(Beta))

                ' 复位路径给 S 带来的额外梯度
                Dim dS_total As Tensor
                If ResetMode = LIFResetMode.ZeroOnSpike Then
                    dS_total = dS_ext(t) - dH.ElementwiseMultiply(U)      ' ∂H/∂S = −U
                Else
                    dS_total = dS_ext(t) - (dH * CSng(Threshold))         ' ∂H/∂S = −θ
                End If

                ' 替代导数 σ'(U[t] − θ)
                Dim surr = U.Apply(
                    Function(v As Double) Surrogate.Derivative(v - Threshold, SurrogateType, Alpha))

                Dim dU As Tensor
                If ResetMode = LIFResetMode.ZeroOnSpike Then
                    Dim oneMinusS = S.Apply(Function(v As Double) 1.0 - v)
                    dU = dS_total.ElementwiseMultiply(surr) + dH.ElementwiseMultiply(oneMinusS)
                Else
                    dU = dS_total.ElementwiseMultiply(surr) + dH          ' ∂H/∂U = 1
                End If

                ' dW += Xᵀ·dU（dL/dI[t] = dL/dU[t]，因为 U = β·H + I）
                WeightGrad = WeightGrad + X.Transpose().MatMul(dU)

                ' dX[t] = dU·Wᵀ（传给更早一层作为 dS_ext）
                dX.Add(dU.MatMul(Weight.Transpose()))

                dU_next = dU
            Next

            dX.Reverse()
            Return dX
        End Function

#End Region

    End Class

End Namespace
