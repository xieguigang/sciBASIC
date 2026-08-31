#Region "Microsoft.VisualBasic::29d5c22b99e40cd5037f7a0f7be4c212, Data_science\MachineLearning\LNN\LiquidNeuralNetwork.vb"

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

    '   Total Lines: 281
    '    Code Lines: 125 (44.48%)
    ' Comment Lines: 101 (35.94%)
    '    - Xml Docs: 80.20%
    ' 
    '   Blank Lines: 55 (19.57%)
    '     File Size: 8.22 KB


    ' Class LiquidNeuralNetwork
    ' 
    '     Properties: DefaultDt, HiddenSize, InputSize, LiquidLayer, NumLiquidLayers
    '                 OutputActivation, OutputBias, OutputBiasGradient, OutputSize, OutputWeight
    '                 OutputWeightGradient, RecordHistory, SolverType, StateHistory
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: ComputeOutput, Forward, GetParameterCount, GetParameters, ProcessSequence
    ' 
    '     Sub: Dispose, ResetState
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' 液态神经网络模块 (Liquid Neural Networks, LNN)
''' 基于Tensor对象实现的连续时间递归神经网络
''' 适用于时间序列分析、预测和控制任务
''' 
''' 核心特点：
''' 1. 使用常微分方程(ODE)描述神经元动态行为
''' 2. 可学习的时间常数实现自适应时间尺度
''' 3. 连续时间处理能力，适合不规则时间步长
'''
''' 完整的液态神经网络
''' 用于时间序列预测和分析
''' </summary>
Public Class LiquidNeuralNetwork : Implements IDisposable

    Private _disposed As Boolean = False

    ''' <summary>最近一次前向的输出（反向传播时需要用它计算输出激活的导数）</summary>
    Private _lastOutput As Tensor
    ''' <summary>最近一次前向的隐藏状态（反向传播时需要用它计算输出层权重梯度）</summary>
    Private _lastHidden As Tensor

#Region "属性"

    ''' <summary>
    ''' 液态层
    ''' </summary>
    Public ReadOnly Property LiquidLayer As LiquidLayer

    ''' <summary>
    ''' 输出层权重 (HiddenSize × OutputSize)
    ''' </summary>
    Public Property OutputWeight As Tensor

    ''' <summary>
    ''' 输出层偏置
    ''' </summary>
    Public Property OutputBias As Tensor

    ''' <summary>
    ''' 输入维度
    ''' </summary>
    Public ReadOnly Property InputSize As Integer

    ''' <summary>
    ''' 隐藏层维度
    ''' </summary>
    Public ReadOnly Property HiddenSize As Integer

    ''' <summary>
    ''' 输出维度
    ''' </summary>
    Public ReadOnly Property OutputSize As Integer

    ''' <summary>
    ''' 液态层数量
    ''' </summary>
    Public ReadOnly Property NumLiquidLayers As Integer

    ''' <summary>
    ''' 输出层激活函数
    ''' </summary>
    Public Property OutputActivation As String = "none"

    ''' <summary>
    ''' ODE求解器类型
    ''' </summary>
    Public Property SolverType As String = "rk4"

    ''' <summary>
    ''' 默认时间步长
    ''' </summary>
    Public Property DefaultDt As Double = 0.1

    ''' <summary>
    ''' 液态层的动力学模式（CT_RNN / LTC / CFC）
    ''' </summary>
    Public Property Mode As LiquidMode
        Get
            Return _LiquidLayer.Mode
        End Get
        Set(value As LiquidMode)
            _LiquidLayer.Mode = value
        End Set
    End Property

    ''' <summary>
    ''' 训练开关。为 True 时液态层会登记前向记录以支持 <see cref="BackwardLiquid"/>。
    ''' </summary>
    Public Property Training As Boolean
        Get
            Return _LiquidLayer.Training
        End Get
        Set(value As Boolean)
            _LiquidLayer.Training = value
        End Set
    End Property

#End Region

#Region "梯度属性"

    Public Property OutputWeightGradient As Tensor
    Public Property OutputBiasGradient As Tensor

#End Region

#Region "历史记录"

    ''' <summary>
    ''' 状态历史记录（用于分析和可视化）
    ''' </summary>
    Public Property StateHistory As New List(Of Tensor)()

    ''' <summary>
    ''' 是否记录状态历史
    ''' </summary>
    Public Property RecordHistory As Boolean = False

#End Region

#Region "构造函数"

    ''' <summary>
    ''' 创建液态神经网络
    ''' </summary>
    ''' <param name="inputSize">输入特征维度</param>
    ''' <param name="hiddenSize">隐藏层维度</param>
    ''' <param name="outputSize">输出维度</param>
    ''' <param name="numLiquidLayers">液态层数量</param>
    ''' <param name="activationType">隐藏层激活函数</param>
    ''' <param name="outputActivation">输出层激活函数: "none", "sigmoid", "tanh", "softmax"</param>
    ''' <param name="seed">随机种子</param>
    ''' <param name="mode">液态层的动力学模式</param>
    Public Sub New(inputSize As Integer, hiddenSize As Integer, outputSize As Integer,
                   Optional numLiquidLayers As Integer = 1,
                   Optional activationType As String = "tanh",
                   Optional outputActivation As String = "none",
                   Optional seed As Integer? = Nothing,
                   Optional mode As LiquidMode = LiquidMode.CT_RNN)
        Me.InputSize = inputSize
        Me.HiddenSize = hiddenSize
        Me.OutputSize = outputSize
        Me.NumLiquidLayers = numLiquidLayers
        Me.OutputActivation = outputActivation

        ' 创建液态层
        _LiquidLayer = New LiquidLayer(inputSize, hiddenSize, numLiquidLayers, activationType, seed, mode)

        ' 初始化输出层权重
        _OutputWeight = Tensor.XavierInit(hiddenSize, outputSize, If(seed, seed + 100))
        _OutputBias = Tensor.Zeros({outputSize})

        ' 初始化梯度
        _OutputWeightGradient = Tensor.Zeros({hiddenSize, outputSize})
        _OutputBiasGradient = Tensor.Zeros({outputSize})
    End Sub

#End Region

#Region "核心方法"

    ''' <summary>
    ''' 前向传播一个时间步
    ''' </summary>
    ''' <param name="input">输入张量</param>
    ''' <param name="dt">时间步长（可选，使用默认值）</param>
    ''' <returns>输出张量</returns>
    Public Function Forward(input As Tensor, Optional dt As Double? = Nothing) As Tensor
        Dim actualDt = If(dt, DefaultDt)

        ' 通过液态层
        Dim hiddenState = _LiquidLayer.Forward(input, actualDt, SolverType)

        ' 记录状态（注意：历史只在 ResetState/ClearHistory 时清空，
        ' 旧实现在这里清空会导致整段序列只剩最后一个时间步）
        If RecordHistory Then
            StateHistory.Add(CType(hiddenState.Clone(), Tensor))
        End If

        ' 通过输出层
        Return ComputeOutput(hiddenState)
    End Function

    ''' <summary>
    ''' 处理完整的时间序列（固定步长）
    ''' </summary>
    ''' <param name="sequence">时间序列输入，形状为 (seqLength, inputSize)</param>
    ''' <param name="dt">时间步长</param>
    ''' <returns>输出序列，形状为 (seqLength, outputSize)</returns>
    Public Function ProcessSequence(sequence As Tensor, Optional dt As Double? = Nothing) As Tensor
        Dim actualDt = If(dt, DefaultDt)
        Dim seqLength = sequence.Shape(0)

        ' 重置状态（同时清空状态历史）
        ResetState()

        ' 输出序列
        Dim outputs = Tensor.Zeros({seqLength, OutputSize})

        For t = 0 To seqLength - 1
            Dim currentInput = RowVector(sequence, t)
            Dim output = Forward(currentInput, actualDt)

            For i = 0 To OutputSize - 1
                outputs(t, i) = output(i)
            Next
        Next

        Return outputs
    End Function

    ''' <summary>
    ''' 按真实时间网格推进（支持不规则采样）
    ''' </summary>
    ''' <remarks>
    ''' 约定：times(0) 处的输出直接由初始隐藏状态读出；
    ''' 之后用 t-1 时刻的输入把状态从 times(t-1) 推到 times(t)。
    ''' 这符合物理模拟中"由当前状态与驱动外推下一状态"的语义。
    ''' </remarks>
    ''' <param name="sequence">驱动输入序列，形状为 (seqLength, inputSize)</param>
    ''' <param name="times">与序列等长的真实时间戳（单调递增）</param>
    ''' <returns>输出序列，形状为 (seqLength, outputSize)</returns>
    Public Function ForwardSequence(sequence As Tensor, times As Double()) As Tensor
        If sequence.Shape(0) <> times.Length Then
            Throw New ArgumentException($"序列长度 {sequence.Shape(0)} 与时间戳数量 {times.Length} 不一致")
        End If
        If times.Length = 0 Then
            Return Tensor.Zeros({0, OutputSize})
        End If

        ResetState()

        Dim outputs = Tensor.Zeros({times.Length, OutputSize})
        Dim first = ComputeOutput(_LiquidLayer.GetOutputState())

        For i = 0 To OutputSize - 1
            outputs(0, i) = first(i)
        Next

        For t = 1 To times.Length - 1
            Dim dt = times(t) - times(t - 1)

            If dt <= 0 Then
                Throw New ArgumentException($"时间戳必须严格单调递增，但在索引 {t} 处出现 dt={dt}")
            End If

            Dim driven = RowVector(sequence, t - 1)
            Dim hiddenState = _LiquidLayer.Forward(driven, dt, SolverType)

            If RecordHistory Then
                StateHistory.Add(CType(hiddenState.Clone(), Tensor))
            End If

            Dim output = ComputeOutput(hiddenState)

            For i = 0 To OutputSize - 1
                outputs(t, i) = output(i)
            Next
        Next

        Return outputs
    End Function

    ''' <summary>
    ''' 取出二维张量的第 rowIndex 行，返回一维张量
    ''' </summary>
    Private Function RowVector(sequence As Tensor, rowIndex As Integer) As Tensor
        Dim row = Tensor.Zeros({InputSize})

        For i = 0 To InputSize - 1
            row(i) = sequence(rowIndex, i)
        Next

        Return row
    End Function

    ''' <summary>
    ''' 由给定隐藏状态计算输出层结果（纯函数，不写入前向缓存）
    ''' </summary>
    ''' <remarks>
    ''' BPTT 训练器需要在整段序列上逐步取输出，但又不能污染 <c>_lastHidden</c> / <c>_lastOutput</c>，
    ''' 因此这里提供一个无副作用版本；<see cref="BackwardOutput"/> 支持显式传入隐藏状态与之配套。
    ''' </remarks>
    Public Function ComputeOutputFrom(hiddenState As Tensor) As Tensor
        Dim hiddenReshaped = New Tensor(hiddenState.Data, 1, HiddenSize)
        Dim linear = hiddenReshaped.MatMul(_OutputWeight)
        Dim output = New Tensor(OutputSize)

        For i = 0 To OutputSize - 1
            output(i) = linear(0, i) + _OutputBias(i)
        Next

        Select Case OutputActivation.ToLower()
            Case "sigmoid"
                output = ActivationFunctions.Sigmoid(output)
            Case "tanh"
                output = ActivationFunctions.Tanh(output)
            Case "softmax"
                output = ActivationFunctions.Softmax(output)
        End Select

        Return output
    End Function

    ''' <summary>
    ''' 计算输出层（并记录前向缓存）
    ''' </summary>
    Private Function ComputeOutput(hiddenState As Tensor) As Tensor
        ' 将隐藏状态reshape为行向量
        Dim hiddenReshaped = New Tensor(hiddenState.Data, 1, HiddenSize)

        ' 计算: output = hidden @ OutputWeight + OutputBias
        Dim linear = hiddenReshaped.MatMul(_OutputWeight)

        ' 添加偏置
        Dim output = Tensor.Zeros({OutputSize})
        For i = 0 To OutputSize - 1
            output(i) = linear(0, i) + _OutputBias(i)
        Next

        ' 应用输出激活函数
        Select Case OutputActivation.ToLower()
            Case "sigmoid"
                output = ActivationFunctions.Sigmoid(output)
            Case "tanh"
                output = ActivationFunctions.Tanh(output)
            Case "softmax"
                output = ActivationFunctions.Softmax(output)
                ' "none" - 不应用激活函数
        End Select

        _lastHidden = hiddenState
        _lastOutput = output

        Return output
    End Function

    ''' <summary>
    ''' 重置网络状态并清空状态历史
    ''' </summary>
    Public Sub ResetState()
        _LiquidLayer.ResetState()
        StateHistory.Clear()
    End Sub

    ''' <summary>
    ''' 只清空状态历史，不影响神经元状态
    ''' </summary>
    Public Sub ClearHistory()
        StateHistory.Clear()
    End Sub

    ''' <summary>
    ''' 丢弃液态层的全部前向记录
    ''' </summary>
    Public Sub ClearRecords()
        _LiquidLayer.ClearRecords()
    End Sub

#End Region

#Region "反向传播"

    ''' <summary>
    ''' 回传输出层：累加输出权重/偏置的梯度，并返回对隐藏状态的伴随向量
    ''' </summary>
    ''' <param name="outputGradient">对网络输出的梯度 dL/doutput</param>
    ''' <param name="hidden">
    ''' 前向时对应的隐藏状态。BPTT 逆序回放时必须显式传入第 t 步的隐藏状态，
    ''' 省略则使用最近一次前向的隐藏状态。
    ''' </param>
    ''' <param name="output">
    ''' 前向时对应的网络输出，用于计算输出激活函数的导数；省略则使用最近一次前向的输出。
    ''' </param>
    ''' <returns>对隐藏状态 h 的梯度 dL/dh</returns>
    Public Function BackwardOutput(outputGradient As Tensor,
                                   Optional hidden As Tensor = Nothing,
                                   Optional output As Tensor = Nothing) As Tensor
        ' BPTT 逆序回放时会显式传入每一时间步的隐藏状态与输出；
        ' 只有在省略这些参数时才回退到最近一次前向留下的缓存。
        If hidden Is Nothing Then
            hidden = _lastHidden

            If hidden Is Nothing Then
                Throw New InvalidOperationException("尚未完成前向传播，也没有显式传入隐藏状态，无法回传输出层梯度")
            End If
        End If

        Dim out = If(output, _lastOutput)

        If out Is Nothing AndAlso OutputActivation.ToLower() <> "none" Then
            Throw New InvalidOperationException(
                $"输出激活函数为 '{OutputActivation}'，回传时需要显式传入前向时该步的输出张量")
        End If

        Dim H = HiddenSize
        Dim O = OutputSize
        Dim dLin = CType(outputGradient.Clone(), Tensor)

        ' 输出激活函数的导数
        Select Case OutputActivation.ToLower()
            Case "sigmoid"
                dLin = LNNMath.Mul(dLin, ActivationFunctions.SigmoidDerivative(out))
            Case "tanh"
                dLin = LNNMath.Mul(dLin, ActivationFunctions.TanhDerivative(out))
            Case "softmax"
                ' J = diag(y) - y·y^T  ⇒  dLin = y ⊙ (dOut - (y·dOut))
                Dim dot As Double = 0.0
                For i = 0 To O - 1
                    dot += out(i) * dLin(i)
                Next
                For i = 0 To O - 1
                    dLin(i) = out(i) * (dLin(i) - dot)
                Next
        End Select

        hidden = If(hidden, _lastHidden)

        For i = 0 To O - 1
            _OutputBiasGradient(i) += dLin(i)

            For j = 0 To H - 1
                _OutputWeightGradient(j, i) += hidden(j) * dLin(i)
            Next
        Next

        Dim adjH = New Tensor(H)

        For j = 0 To H - 1
            Dim acc As Double = 0.0

            For i = 0 To O - 1
                acc += _OutputWeight(j, i) * dLin(i)
            Next

            adjH(j) = acc
        Next

        Return adjH
    End Function

    ''' <summary>
    ''' 回传液态层：消费一个时间步的前向记录，返回对步首状态的梯度
    ''' </summary>
    ''' <param name="adjHidden">对隐藏状态 h 的梯度（可来自输出层、通量读取头或下一时刻）</param>
    ''' <returns>对网络外部输入的梯度</returns>
    Public Function BackwardLiquid(adjHidden As Tensor) As Tensor
        Return _LiquidLayer.Backward(adjHidden)
    End Function

#End Region

#Region "参数与梯度管理"

    ''' <summary>
    ''' 获取 (参数名, 参数, 梯度) 配对列表
    ''' </summary>
    Public Function GetParameterPairs() As List(Of ParameterPair)
        Dim all As New List(Of ParameterPair)()

        For Each pair In _LiquidLayer.GetParameterPairs()
            all.Add(New ParameterPair($"liquid_{pair.Name}", pair.Value, pair.Gradient))
        Next

        all.Add(New ParameterPair("output_weight", _OutputWeight, _OutputWeightGradient))
        all.Add(New ParameterPair("output_bias", _OutputBias, _OutputBiasGradient))

        Return all
    End Function

    ''' <summary>
    ''' 获取所有可训练参数
    ''' </summary>
    Public Function GetParameters() As Dictionary(Of String, Tensor)
        Dim all As New Dictionary(Of String, Tensor)()

        For Each pair In GetParameterPairs()
            all.Add(pair.Name, pair.Value)
        Next

        Return all
    End Function

    ''' <summary>
    ''' 获取参数总数
    ''' </summary>
    Public Function GetParameterCount() As Integer
        Dim count = 0

        For Each pair In GetParameterPairs()
            count += pair.Value.Length
        Next

        Return count
    End Function

    ''' <summary>
    ''' 清零全部梯度累加器
    ''' </summary>
    Public Sub ZeroGradients()
        For Each pair In GetParameterPairs()
            Dim g = pair.Gradient

            For i = 0 To g.Length - 1
                g(i) = 0
            Next
        Next
    End Sub

#End Region

#Region "IDisposable实现"

    Public Sub Dispose() Implements IDisposable.Dispose
        If Not _disposed Then
            _LiquidLayer?.Dispose()
            _OutputWeight?.Dispose()
            _OutputBias?.Dispose()
            _OutputWeightGradient?.Dispose()
            _OutputBiasGradient?.Dispose()
            For Each state In StateHistory
                state?.Dispose()
            Next
            _disposed = True
        End If
    End Sub

#End Region

End Class
