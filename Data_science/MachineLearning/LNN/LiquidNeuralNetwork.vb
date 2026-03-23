#Region "Microsoft.VisualBasic::5e8c3b6fb7c1aeef992866f102f5923a, Data_science\MachineLearning\LNN\LiquidNeuralNetwork.vb"

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

    '   Total Lines: 283
    '    Code Lines: 126 (44.52%)
    ' Comment Lines: 101 (35.69%)
    '    - Xml Docs: 80.20%
    ' 
    '   Blank Lines: 56 (19.79%)
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
Public Class LiquidNeuralNetwork
    Implements IDisposable

    Private _disposed As Boolean = False

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
    Public Sub New(inputSize As Integer, hiddenSize As Integer, outputSize As Integer,
                   Optional numLiquidLayers As Integer = 1,
                   Optional activationType As String = "tanh",
                   Optional outputActivation As String = "none",
                   Optional seed As Integer? = Nothing)
        Me.InputSize = inputSize
        Me.HiddenSize = hiddenSize
        Me.OutputSize = outputSize
        Me.NumLiquidLayers = numLiquidLayers
        Me.OutputActivation = outputActivation

        ' 创建液态层
        _LiquidLayer = New LiquidLayer(inputSize, hiddenSize, numLiquidLayers, activationType, seed)

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
    ''' 前向传播
    ''' </summary>
    ''' <param name="input">输入张量</param>
    ''' <param name="dt">时间步长（可选，使用默认值）</param>
    ''' <returns>输出张量</returns>
    Public Function Forward(input As Tensor, Optional dt As Double? = Nothing) As Tensor
        Dim actualDt = If(dt, DefaultDt)

        ' 清空历史记录
        If RecordHistory Then
            StateHistory.Clear()
        End If

        ' 通过液态层
        Dim hiddenState = _LiquidLayer.Forward(input, actualDt, SolverType)

        ' 记录状态
        If RecordHistory Then
            StateHistory.Add(CType(hiddenState.Clone(), Tensor))
        End If

        ' 通过输出层
        Dim output = ComputeOutput(hiddenState)

        Return output
    End Function

    ''' <summary>
    ''' 处理完整的时间序列
    ''' </summary>
    ''' <param name="sequence">时间序列输入，形状为 (seqLength, inputSize)</param>
    ''' <param name="dt">时间步长</param>
    ''' <returns>输出序列，形状为 (seqLength, outputSize)</returns>
    Public Function ProcessSequence(sequence As Tensor, Optional dt As Double? = Nothing) As Tensor
        Dim actualDt = If(dt, DefaultDt)
        Dim seqLength = sequence.Shape(0)

        ' 重置状态
        ResetState()

        ' 清空历史记录
        If RecordHistory Then
            StateHistory.Clear()
        End If

        ' 输出序列
        Dim outputs = Tensor.Zeros({seqLength, OutputSize})

        For t = 0 To seqLength - 1
            ' 获取当前时间步的输入
            Dim currentInput = Tensor.Zeros({InputSize})
            For i = 0 To InputSize - 1
                currentInput(i) = sequence(t, i)
            Next

            ' 前向传播
            Dim output = Forward(currentInput, actualDt)

            ' 存储输出
            For i = 0 To OutputSize - 1
                outputs(t, i) = output(i)
            Next
        Next

        Return outputs
    End Function

    ''' <summary>
    ''' 计算输出层
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

        Return output
    End Function

    ''' <summary>
    ''' 重置网络状态
    ''' </summary>
    Public Sub ResetState()
        _LiquidLayer.ResetState()
    End Sub

    ''' <summary>
    ''' 获取所有可训练参数
    ''' </summary>
    Public Function GetParameters() As Dictionary(Of String, Tensor)
        Dim allParams = _LiquidLayer.GetParameters()
        allParams.Add("output_weight", _OutputWeight)
        allParams.Add("output_bias", _OutputBias)
        Return allParams
    End Function

    ''' <summary>
    ''' 获取参数总数
    ''' </summary>
    Public Function GetParameterCount() As Integer
        Dim count = 0
        Dim params = GetParameters()
        For Each kvp In params
            count += kvp.Value.Length
        Next
        Return count
    End Function

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


