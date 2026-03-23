#Region "Microsoft.VisualBasic::28e37c5092ba38cf83fb1ecf79ece4f3, Data_science\MachineLearning\LNN\LiquidCell.vb"

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

    '   Total Lines: 357
    '    Code Lines: 171 (47.90%)
    ' Comment Lines: 126 (35.29%)
    '    - Xml Docs: 87.30%
    ' 
    '   Blank Lines: 60 (16.81%)
    '     File Size: 11.56 KB


    ' Class LiquidCell
    ' 
    '     Properties: ActivationType, Bias, BiasGradient, HiddenSize, InputSize
    '                 State, Tau, TauGradient, TauMax, TauMin
    '                 UseBoundedTau, WeightInput, WeightInputGradient, WeightRecurrent, WeightRecurrentGradient
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: ApplyActivation, ApplyActivationDerivative, ComputeDerivative, Forward, GetEffectiveTau
    '               GetGradients, GetParameters
    ' 
    '     Sub: Dispose, ResetState, SetState
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 液态神经元单元 (Liquid Cell)
''' 实现基于ODE的连续时间神经元模型
''' 
''' 核心方程: dx/dt = -x/τ + σ(W·x + U·u + b)
''' 其中:
''' - x: 神经元状态
''' - τ: 时间常数（可学习）
''' - W: 循环权重
''' - U: 输入权重
''' - u: 外部输入
''' - b: 偏置
''' - σ: 激活函数
''' </summary>
Public Class LiquidCell
    Implements IDisposable

#Region "私有字段"

    Private _disposed As Boolean = False

#End Region

#Region "属性"

    ''' <summary>
    ''' 神经元数量（状态维度）
    ''' </summary>
    Public ReadOnly Property HiddenSize As Integer

    ''' <summary>
    ''' 输入维度
    ''' </summary>
    Public ReadOnly Property InputSize As Integer

    ''' <summary>
    ''' 当前神经元状态
    ''' </summary>
    Public Property State As Tensor

    ''' <summary>
    ''' 时间常数 τ（可学习参数）
    ''' 控制状态衰减速度，值越大衰减越慢
    ''' </summary>
    Public Property Tau As Tensor

    ''' <summary>
    ''' 输入权重矩阵 U (HiddenSize × InputSize)
    ''' </summary>
    Public Property WeightInput As Tensor

    ''' <summary>
    ''' 循环权重矩阵 W (HiddenSize × HiddenSize)
    ''' </summary>
    Public Property WeightRecurrent As Tensor

    ''' <summary>
    ''' 偏置向量 b (HiddenSize)
    ''' </summary>
    Public Property Bias As Tensor

    ''' <summary>
    ''' 激活函数类型
    ''' </summary>
    Public Property ActivationType As String = "tanh"

    ''' <summary>
    ''' 是否使用有界时间常数
    ''' </summary>
    Public Property UseBoundedTau As Boolean = True

    ''' <summary>
    ''' 时间常数最小值
    ''' </summary>
    Public Property TauMin As Double = 0.1

    ''' <summary>
    ''' 时间常数最大值
    ''' </summary>
    Public Property TauMax As Double = 10.0

#End Region

#Region "梯度属性（用于反向传播）"

    ''' <summary>
    ''' Tau的梯度
    ''' </summary>
    Public Property TauGradient As Tensor

    ''' <summary>
    ''' 输入权重的梯度
    ''' </summary>
    Public Property WeightInputGradient As Tensor

    ''' <summary>
    ''' 循环权重的梯度
    ''' </summary>
    Public Property WeightRecurrentGradient As Tensor

    ''' <summary>
    ''' 偏置的梯度
    ''' </summary>
    Public Property BiasGradient As Tensor

#End Region

#Region "中间状态（用于反向传播）"

    ''' <summary>
    ''' 上一步的输入（用于反向传播）
    ''' </summary>
    Private _lastInput As Tensor

    ''' <summary>
    ''' 上一步的激活值
    ''' </summary>
    Private _lastActivation As Tensor

    ''' <summary>
    ''' 上一步的线性组合值
    ''' </summary>
    Private _lastLinear As Tensor

#End Region

#Region "构造函数"

    ''' <summary>
    ''' 创建液态神经元单元
    ''' </summary>
    ''' <param name="hiddenSize">隐藏层神经元数量</param>
    ''' <param name="inputSize">输入维度</param>
    ''' <param name="activationType">激活函数类型: "tanh", "sigmoid", "relu"</param>
    ''' <param name="seed">随机种子（可选）</param>
    Public Sub New(hiddenSize As Integer, inputSize As Integer, Optional activationType As String = "tanh", Optional seed As Integer? = Nothing)
        Me.HiddenSize = hiddenSize
        Me.InputSize = inputSize
        Me.ActivationType = activationType.ToLower()

        ' 初始化状态为零
        _State = Tensor.Zeros({hiddenSize})

        ' 初始化时间常数（使用softplus确保正值）
        _Tau = Tensor.Random({hiddenSize}, 0.5F, 2.0F, seed)

        ' Xavier初始化权重
        _WeightInput = Tensor.XavierInit(inputSize, hiddenSize, If(seed, seed + 1))
        _WeightRecurrent = Tensor.XavierInit(hiddenSize, hiddenSize, If(seed, seed + 2))
        _Bias = Tensor.Zeros({hiddenSize})

        ' 初始化梯度存储
        _TauGradient = Tensor.Zeros({hiddenSize})
        _WeightInputGradient = Tensor.Zeros({inputSize, hiddenSize})
        _WeightRecurrentGradient = Tensor.Zeros({hiddenSize, hiddenSize})
        _BiasGradient = Tensor.Zeros({hiddenSize})
    End Sub

#End Region

#Region "核心方法"

    ''' <summary>
    ''' 获取有效的时间常数（应用边界约束）
    ''' </summary>
    Private Function GetEffectiveTau() As Tensor
        If UseBoundedTau Then
            ' 使用sigmoid将tau限制在[TauMin, TauMax]范围内
            Return _Tau.Apply(Function(v As Double) As Double
                                  Dim normalized = 1.0 / (1.0 + std.Exp(-v))  ' sigmoid
                                  Return TauMin + normalized * (TauMax - TauMin)
                              End Function)
        Else
            ' 使用softplus确保正值
            Return _Tau.Apply(Function(v As Double) std.Log(1.0 + std.Exp(v)))
        End If
    End Function

    ''' <summary>
    ''' 应用激活函数
    ''' </summary>
    Private Function ApplyActivation(x As Tensor) As Tensor
        Select Case ActivationType
            Case "tanh"
                Return ActivationFunctions.Tanh(x)
            Case "sigmoid"
                Return ActivationFunctions.Sigmoid(x)
            Case "relu"
                Return ActivationFunctions.ReLU(x)
            Case "leaky_relu"
                Return ActivationFunctions.LeakyReLU(x)
            Case Else
                Return ActivationFunctions.Tanh(x)
        End Select
    End Function

    ''' <summary>
    ''' 计算激活函数的导数
    ''' </summary>
    Private Function ApplyActivationDerivative(activationOutput As Tensor) As Tensor
        Select Case ActivationType
            Case "tanh"
                Return ActivationFunctions.TanhDerivative(activationOutput)
            Case "sigmoid"
                Return ActivationFunctions.SigmoidDerivative(activationOutput)
            Case "relu"
                Return ActivationFunctions.ReLUDerivative(activationOutput)
            Case Else
                Return ActivationFunctions.TanhDerivative(activationOutput)
        End Select
    End Function

    ''' <summary>
    ''' 计算ODE右端函数: dx/dt = f(x, u, t)
    ''' </summary>
    ''' <param name="state">当前状态</param>
    ''' <param name="input">外部输入</param>
    ''' <param name="time">当前时间（可用于时变系统）</param>
    ''' <returns>状态导数</returns>
    Public Function ComputeDerivative(state As Tensor, input As Tensor, time As Double) As Tensor
        ' 获取有效时间常数
        Dim effectiveTau = GetEffectiveTau()

        ' 计算线性组合: z = W·x + U·u + b
        ' 需要将输入reshape为列向量进行矩阵乘法
        Dim inputReshaped = input
        If input.Rank = 1 Then
            inputReshaped = New Tensor(input.Data, 1, input.Length)
        End If

        Dim stateReshaped = state
        If state.Rank = 1 Then
            stateReshaped = New Tensor(state.Data, 1, state.Length)
        End If

        ' U·u (inputSize × hiddenSize) * (1 × inputSize)^T = (hiddenSize × 1)
        Dim inputContribution = inputReshaped.MatMul(_WeightInput)

        ' W·x (hiddenSize × hiddenSize) * (1 × hiddenSize)^T = (hiddenSize × 1)
        Dim recurrentContribution = stateReshaped.MatMul(_WeightRecurrent)

        ' 合并: z = U·u + W·x + b
        Dim linear = Tensor.Zeros({1, HiddenSize})
        For i = 0 To HiddenSize - 1
            linear(0, i) = inputContribution(0, i) + recurrentContribution(0, i) + _Bias(i)
        Next

        ' 应用激活函数
        Dim activation = ApplyActivation(linear)

        ' 保存中间状态用于反向传播
        _lastInput = CType(input.Clone(), Tensor)
        _lastLinear = CType(linear.Clone(), Tensor)
        _lastActivation = CType(activation.Clone(), Tensor)

        ' 计算导数: dx/dt = -x/τ + activation
        Dim derivative = Tensor.Zeros({HiddenSize})
        For i = 0 To HiddenSize - 1
            derivative(i) = -state(i) / effectiveTau(i) + activation(0, i)
        Next

        Return derivative
    End Function

    ''' <summary>
    ''' 前向传播：使用指定ODE求解器更新状态
    ''' </summary>
    ''' <param name="input">当前时刻输入</param>
    ''' <param name="dt">时间步长</param>
    ''' <param name="solverType">ODE求解器类型: "euler", "heun", "rk4"</param>
    ''' <returns>更新后的状态</returns>
    Public Function Forward(input As Tensor, dt As Double, Optional solverType As String = "rk4") As Tensor
        Select Case solverType.ToLower()
            Case "euler"
                _State = ODESolver.EulerStep(AddressOf ComputeDerivative, _State, input, 0.0, dt)
            Case "heun"
                _State = ODESolver.HeunStep(AddressOf ComputeDerivative, _State, input, 0.0, dt)
            Case "rk4"
                _State = ODESolver.RK4Step(AddressOf ComputeDerivative, _State, input, 0.0, dt)
            Case Else
                _State = ODESolver.RK4Step(AddressOf ComputeDerivative, _State, input, 0.0, dt)
        End Select

        Return _State
    End Function

    ''' <summary>
    ''' 重置神经元状态为零
    ''' </summary>
    Public Sub ResetState()
        _State = Tensor.Zeros({HiddenSize})
    End Sub

    ''' <summary>
    ''' 设置初始状态
    ''' </summary>
    Public Sub SetState(initialState As Tensor)
        If initialState.Length <> HiddenSize Then
            Throw New ArgumentException($"状态维度不匹配: 期望 {HiddenSize}, 实际 {initialState.Length}")
        End If
        _State = CType(initialState.Clone(), Tensor)
    End Sub

    ''' <summary>
    ''' 获取所有可训练参数
    ''' </summary>
    Public Function GetParameters() As Dictionary(Of String, Tensor)
        Dim params As New Dictionary(Of String, Tensor) From {
            {"tau", _Tau},
            {"weight_input", _WeightInput},
            {"weight_recurrent", _WeightRecurrent},
            {"bias", _Bias}
        }
        Return params
    End Function

    ''' <summary>
    ''' 获取所有梯度
    ''' </summary>
    Public Function GetGradients() As Dictionary(Of String, Tensor)
        Dim grads As New Dictionary(Of String, Tensor) From {
            {"tau", _TauGradient},
            {"weight_input", _WeightInputGradient},
            {"weight_recurrent", _WeightRecurrentGradient},
            {"bias", _BiasGradient}
        }
        Return grads
    End Function

#End Region

#Region "IDisposable实现"

    Public Sub Dispose() Implements IDisposable.Dispose
        If Not _disposed Then
            _State?.Dispose()
            _Tau?.Dispose()
            _WeightInput?.Dispose()
            _WeightRecurrent?.Dispose()
            _Bias?.Dispose()
            _TauGradient?.Dispose()
            _WeightInputGradient?.Dispose()
            _WeightRecurrentGradient?.Dispose()
            _BiasGradient?.Dispose()
            _lastInput?.Dispose()
            _lastActivation?.Dispose()
            _lastLinear?.Dispose()
            _disposed = True
        End If
    End Sub

#End Region

End Class

