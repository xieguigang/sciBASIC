' ============================================================================
' LinearReadout.vb — 线性回归解码头（可训练）
'
' 脉冲网络的输出处于"脉冲/膜电位"域，而回归目标（如基因表达量）是连续值，
' 因此需要一层可训练的线性映射把状态读成预测值：
'
'   y = u · Wout + b
'
' 其中 u 为解码输入（末步膜电位 u_last 或发放率 rate，见 SpikeDecoders），
' Wout [InputSize, Units] 与 b [Units] 为可训练参数。
'
' 反向（给定上游梯度 dL/dy）：
'   dWout += uᵀ·(dL/dy)
'   db    += Σ_batch (dL/dy)
'   du     = (dL/dy)·Woutᵀ          ← 回传给 LIF 层的 dU_last
'
' 设计说明：
'   1. 偏置不做张量广播——Tensor 的 '+' 在"某一维为 1"时会走外积语义的
'      广播加法，与"逐行加偏置"完全不同，因此这里按索引手工累加，避免踩坑。
'   2. 梯度就地累加后按既有设备端缓存契约调用 MarkHostModified()，保证
'      CUDA 后端不会复用过期的主机数据副本。
'   3. 输入 u 会在前向时缓存，供反向计算使用；每次前向只保留最近一次，
'      与 LIFLayer 的"每批 ResetState"约定一致（每个 batch 前向一次、反向一次）。
' ============================================================================

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 线性回归解码头：把脉冲网络状态（膜电位 / 发放率）线性映射为连续预测值。
''' </summary>
Public Class LinearReadout

#Region "参数"

    Public ReadOnly Property Name As String

    ''' <summary>解码输入维度（如神经元数量 N，或拼接解码时的 2·N）</summary>
    Public ReadOnly Property InputSize As Integer

    ''' <summary>输出维度（回归目标维度，通常等于基因数量 N）</summary>
    Public ReadOnly Property Units As Integer

    ''' <summary>权重 [InputSize, Units]</summary>
    Public Property Weight As Tensor

    ''' <summary>偏置 [Units]（一维张量）</summary>
    Public Property Bias As Tensor

    ''' <summary>权重梯度（<see cref="BackwardTime"/> 中累积）</summary>
    Public Property WeightGrad As Tensor

    ''' <summary>偏置梯度（<see cref="BackwardTime"/> 中累积）</summary>
    Public Property BiasGrad As Tensor

    ''' <summary>参数是否参与梯度累积</summary>
    Public Property Trainable As Boolean = True

#End Region

#Region "构造"

    ''' <summary>
    ''' 构建线性回归解码头。
    ''' </summary>
    ''' <param name="name">层名称（用于优化器区分动量状态）</param>
    ''' <param name="inputSize">解码输入维度</param>
    ''' <param name="units">输出维度</param>
    ''' <param name="initialWeight">
    ''' 初始权重 [InputSize, Units]；为 Nothing 时使用 He 初始化。
    ''' 当解码目标与网络状态同维且希望"恒等起步"时，可传入单位阵。
    ''' </param>
    ''' <param name="initialBias">初始偏置 [Units]（一维）；为 Nothing 时初始化为全 0</param>
    Public Sub New(name As String, inputSize As Integer, units As Integer,
                   Optional initialWeight As Tensor = Nothing,
                   Optional initialBias As Tensor = Nothing,
                   Optional seed As Integer? = Nothing)

        If inputSize <= 0 OrElse units <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(units), "输入/输出维度必须为正整数")
        End If

        Me.Name = name
        Me.InputSize = inputSize
        Me.Units = units

        If initialWeight Is Nothing Then
            Me.Weight = Tensor.HeInit(inputSize, units, seed)
        Else
            If initialWeight.Rank <> 2 OrElse
               initialWeight.Shape(0) <> inputSize OrElse
               initialWeight.Shape(1) <> units Then
                Throw New ArgumentException(
                    $"initialWeight 形状应为 [{inputSize}, {units}]，实际 [{String.Join(",", initialWeight.Shape)}]")
            End If
            Me.Weight = New Tensor(initialWeight.ToDoubleArray(), inputSize, units)
        End If

        If initialBias Is Nothing Then
            Me.Bias = New Tensor(units)
        Else
            If initialBias.Length <> units Then
                Throw New ArgumentException(
                    $"initialBias 长度应为 {units}，实际 {initialBias.Length}")
            End If
            Me.Bias = New Tensor(initialBias.ToDoubleArray(), units)
        End If

        Me.WeightGrad = New Tensor(inputSize, units)
        Me.BiasGrad = New Tensor(units)
    End Sub

    ''' <summary>
    ''' 以单位阵初始化权重、零偏置构造解码头（当解码目标与输入同维、
    ''' 且希望训练从"直通"映射开始时的推荐用法）。
    ''' </summary>
    Public Shared Function IdentityReadout(name As String, units As Integer) As LinearReadout
        Return New LinearReadout(name, units, units, Tensor.Identity(units), Nothing)
    End Function

    ''' <summary>清空梯度累积（每个训练步开始前调用）</summary>
    Public Sub ZeroGrad()
        WeightGrad = New Tensor(InputSize, Units)
        BiasGrad = New Tensor(Units)
    End Sub

#End Region

#Region "前向"

    ''' <summary>
    ''' 前向：y = u·Wout + b。
    ''' </summary>
    ''' <param name="u">解码输入 [batch, InputSize]（膜电位或发放率）</param>
    ''' <returns>预测值 [batch, Units]</returns>
    Public Function Forward(u As Tensor) As Tensor
        If u Is Nothing Then
            Throw New ArgumentNullException(NameOf(u))
        End If
        If u.Rank <> 2 OrElse u.Shape(1) <> InputSize Then
            Throw New ArgumentException(
                $"解码头输入形状应为 [batch, {InputSize}]，实际 [{String.Join(",", u.Shape)}]")
        End If

        Dim batch = u.Shape(0)
        Dim y = u.MatMul(Weight)
        Dim b = Bias.Data
        Dim yd = y.Data

        ' 逐行加偏置（不使用 Tensor 的广播加法，见文件头说明）
        For n = 0 To batch - 1
            Dim off = n * Units
            For j = 0 To Units - 1
                yd(off + j) += b(j)
            Next
        Next

        ' 就地写入后声明主机数据已修改
        y.MarkHostModified()
        _u = u

        Return y
    End Function

#End Region

#Region "反向"

    Private _u As Tensor

    ''' <summary>
    ''' 反向：给定上游梯度 dL/dy，累积 dWout / db 并返回 dL/du。
    ''' </summary>
    ''' <param name="dY">上游梯度 [batch, Units]</param>
    ''' <returns>dL/du [batch, InputSize]（回传给 LIF 层作为膜电位梯度）</returns>
    Public Function BackwardTime(dY As Tensor) As Tensor
        If _u Is Nothing Then
            Throw New InvalidOperationException("尚未执行前向传播，无法反向")
        End If
        If dY Is Nothing OrElse dY.Rank <> 2 OrElse dY.Shape(0) <> _u.Shape(0) OrElse dY.Shape(1) <> Units Then
            Throw New ArgumentException(
                $"dY 形状应为 [{_u.Shape(0)}, {Units}]，实际 [{String.Join(",", dY.Shape)}]")
        End If

        If Trainable Then
            ' dWout += uᵀ·dY
            WeightGrad = WeightGrad + _u.Transpose().MatMul(dY)

            ' db += Σ_batch dY（就地累加）
            Dim bgd = BiasGrad.Data
            Dim dd = dY.Data
            For n = 0 To dY.Shape(0) - 1
                Dim off = n * Units
                For j = 0 To Units - 1
                    bgd(j) += dd(off + j)
                Next
            Next
            BiasGrad.MarkHostModified()
        End If

        ' du = dY·Woutᵀ
        Return dY.MatMul(Weight.Transpose())
    End Function

#End Region

    Public Overrides Function ToString() As String
        Return $"{Name}(LinearReadout, {InputSize}→{Units})"
    End Function

End Class
