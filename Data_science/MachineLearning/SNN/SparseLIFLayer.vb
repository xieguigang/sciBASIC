' ============================================================================
' SparseLIFLayer.vb — 稀疏递归 LIF 层（单层大稀疏连接）
'
' 与稠密 LIFLayer 的区别：
'   LIFLayer 为多输入 → 多输出的前馈全连接层（Weight [in, units] 稠密）。
'   本层为"同一神经元群"内部的稀疏突触连接：Synapses 是方阵 W[N, N]，
'   W[pre, post] 表示突触前神经元 pre 连到突触后神经元 post（含自反馈 pre=post）。
'
' 每个时间步的前向动态（readme 模块二）：
'   I_rec[t] = W · S[t−1]                递归输入：上一时刻脉冲经稀疏矩阵回灌（含循环/自反馈）
'   I[t]     = I_ext[t] + I_rec[t]        叠加外部注入电流（如感觉神经元的刺激）
'   U[t]     = β·H[t−1] + I[t]           泄漏积分
'   S[t]     = Θ(U[t] − U_thr)           阈值触发（二值脉冲）
'   H[t]     = 复位(U[t], S[t])          发放后复位
'
' 由于 S[t−1] 作为跨时间步状态参与计算，本层天然是一个脉冲递归网络（SRNN），
' 对连接矩阵中存在的循环连接与自反馈均正确建模——这正是真实连接组仿真的核心。
'
' 注意：本层仅用于前向仿真（权重固定），不保存 BPTT 反向缓存，也不提供反向传播。
' ============================================================================

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Public Class SparseLIFLayer

#Region "超参数与连接"

    Public ReadOnly Property Name As String

    ''' <summary>输入特征维度（外部注入电流的原始特征数，经 inputMap 散射到神经元）</summary>
    Public ReadOnly Property InputSize As Integer

    ''' <summary>神经元数量 N（= Synapses 的行列数）</summary>
    Public ReadOnly Property Units As Integer

    ''' <summary>稀疏突触连接矩阵 W[N, N]：行 = 突触前，列 = 突触后</summary>
    Public ReadOnly Property Synapses As SparseMatrix

    ''' <summary>膜电位衰减系数 β ∈ (0,1)，越小遗忘越快</summary>
    Public Property Beta As Double

    ''' <summary>发放阈值 U_thr</summary>
    Public Property Threshold As Double

    ''' <summary>复位模式</summary>
    Public Property ResetMode As LIFResetMode

#End Region

#Region "状态与轨迹缓存"

    ''' <summary>膜电位 H（跨时间步持续）[batch, Units]</summary>
    Private _H As Tensor

    ''' <summary>上一时间步的输出脉冲 S[t−1]，作为本步递归输入来源 [batch, Units]</summary>
    Private _Sprev As Tensor

    Private _U As New List(Of Tensor)()
    Private _S As New List(Of Tensor)()

    ''' <summary>最近一次仿真中各时间步的触发前膜电位轨迹（可视化用）</summary>
    Public ReadOnly Property UHistory As List(Of Tensor)
        Get
            Return _U
        End Get
    End Property

    ''' <summary>最近一次仿真中各时间步的输出脉冲轨迹（可视化用）</summary>
    Public ReadOnly Property SHistory As List(Of Tensor)
        Get
            Return _S
        End Get
    End Property

#End Region

#Region "构造与状态重置"

    ''' <summary>
    ''' 构建稀疏递归 LIF 层。
    ''' </summary>
    ''' <param name="synapses">方阵稀疏突触连接 W[N, N]（行=pre，列=post）</param>
    Public Sub New(name As String, inputSize As Integer, synapses As SparseMatrix,
                   Optional beta As Double = 0.9,
                   Optional threshold As Double = 1.0,
                   Optional resetMode As LIFResetMode = LIFResetMode.ZeroOnSpike)

        If synapses Is Nothing Then
            Throw New ArgumentNullException(NameOf(synapses))
        End If
        If synapses.Rows <> synapses.Columns Then
            Throw New ArgumentException(
                $"稀疏连接矩阵必须为方阵（pre/post 为同一神经元群），实际 {synapses.Rows}x{synapses.Columns}")
        End If

        Me.Name = name
        Me.InputSize = inputSize
        Me.Units = synapses.Columns
        Me.Synapses = synapses
        Me.Beta = beta
        Me.Threshold = threshold
        Me.ResetMode = resetMode

        _H = New Tensor(1, Units)
        _Sprev = New Tensor(1, Units)
    End Sub

    ''' <summary>
    ''' 重置膜电位、递归脉冲状态与轨迹缓存。每个样本批次开始前必须调用！
    ''' </summary>
    Public Sub ResetState(batchSize As Integer)
        _H = New Tensor(batchSize, Units)
        _Sprev = New Tensor(batchSize, Units)
        _U = New List(Of Tensor)()
        _S = New List(Of Tensor)()
    End Sub

#End Region

#Region "前向传播"

    ''' <summary>
    ''' 单时间步前向：外部注入电流 → 递归稀疏输入 → LIF 触发 → 复位 → 更新递归状态。
    ''' </summary>
    ''' <param name="externalCurrent">
    ''' 外部注入电流 [batch, Units]（可为 Nothing 表示本步无外部刺激）。
    ''' 通常由编码后的脉冲按 inputMap 散射得到。
    ''' </param>
    ''' <returns>本时间步输出脉冲 S[t] [batch, Units]</returns>
    Public Function ForwardStep(externalCurrent As Tensor) As Tensor
        Dim batch = _Sprev.Shape(0)

        ' 1. 递归输入：上一时刻脉冲经稀疏突触矩阵回灌（含循环连接与自反馈）
        Dim I = Synapses.SpMM(_Sprev)

        ' 2. 叠加外部注入电流
        If externalCurrent IsNot Nothing Then
            If externalCurrent.Rank <> 2 OrElse
               externalCurrent.Shape(0) <> batch OrElse
               externalCurrent.Shape(1) <> Units Then
                Throw New ArgumentException(
                    $"externalCurrent 形状应为 [batch, {Units}]，实际 [{String.Join(",", externalCurrent.Shape)}]")
            End If
            I = I + externalCurrent
        End If

        ' 3. 泄漏积分：U = β·H + I
        Dim U = I + (_H * CSng(Beta))

        ' 4. 阈值触发：S = Θ(U − U_thr)
        Dim S = U.Apply(Function(v As Double) Surrogate.Spike(v - Threshold))

        ' 5. 复位
        Dim H As Tensor
        If ResetMode = LIFResetMode.ZeroOnSpike Then
            H = U.ElementwiseMultiply(S.Apply(Function(v As Double) 1.0 - v))
        Else
            H = U - (S * CSng(Threshold))
        End If

        _U.Add(U)
        _S.Add(S)
        _H = H
        _Sprev = S

        Return S
    End Function

#End Region

    Public Overrides Function ToString() As String
        Return $"{Name}(SparseLIF, N={Units}, nnz={Synapses.NonZeros}, β={Beta})"
    End Function

End Class
