''' <summary>
''' 液态神经网络模块 (Liquid Neural Networks, LNN)
''' 基于Tensor对象实现的连续时间递归神经网络
''' 适用于时间序列分析、预测和控制任务
''' 
''' 核心特点：
''' 1. 使用常微分方程(ODE)描述神经元动态行为
''' 2. 可学习的时间常数实现自适应时间尺度
''' 3. 连续时间处理能力，适合不规则时间步长
''' </summary>
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

#Region "ODE求解器模块"

''' <summary>
''' 常微分方程求解器
''' 用于数值积分液态神经网络的微分方程
''' </summary>
Public Module ODESolver

    ''' <summary>
    ''' 常微分方程的右端函数委托类型
    ''' </summary>
    ''' <param name="state">当前状态</param>
    ''' <param name="input">外部输入</param>
    ''' <param name="time">当前时间</param>
    ''' <returns>状态导数 dx/dt</returns>
    Public Delegate Function ODEFunction(state As Tensor, input As Tensor, time As Double) As Tensor

    ''' <summary>
    ''' 欧拉法求解ODE（一阶精度，计算快速）
    ''' 适用于实时应用和快速原型开发
    ''' </summary>
    ''' <param name="odeFunc">ODE右端函数</param>
    ''' <param name="initialState">初始状态</param>
    ''' <param name="input">外部输入</param>
    ''' <param name="t0">起始时间</param>
    ''' <param name="dt">时间步长</param>
    ''' <returns>下一时刻的状态</returns>
    Public Function EulerStep(odeFunc As ODEFunction, initialState As Tensor, input As Tensor, t0 As Double, dt As Double) As Tensor
        ' 计算导数: dx/dt = f(x, u, t)
        Dim derivative = odeFunc(initialState, input, t0)

        ' 欧拉更新: x(t+dt) = x(t) + dt * dx/dt
        Dim result = initialState + derivative * CSng(dt)

        Return result
    End Function

    ''' <summary>
    ''' 改进欧拉法（Heun方法，二阶精度）
    ''' 比欧拉法更精确，计算量适中
    ''' </summary>
    Public Function HeunStep(odeFunc As ODEFunction, initialState As Tensor, input As Tensor, t0 As Double, dt As Double) As Tensor
        ' 第一步：预测
        Dim k1 = odeFunc(initialState, input, t0)
        Dim predicted = initialState + k1 * CSng(dt)

        ' 第二步：校正
        Dim k2 = odeFunc(predicted, input, t0 + dt)

        ' 平均斜率
        Dim avgSlope = (k1 + k2) * 0.5F

        ' 最终更新
        Dim result = initialState + avgSlope * CSng(dt)

        Return result
    End Function

    ''' <summary>
    ''' 四阶龙格-库塔法（四阶精度）
    ''' 高精度求解器，适用于精确模拟
    ''' </summary>
    Public Function RK4Step(odeFunc As ODEFunction, initialState As Tensor, input As Tensor, t0 As Double, dt As Double) As Tensor
        Dim dtSng = CSng(dt)

        ' k1 = f(x, u, t)
        Dim k1 = odeFunc(initialState, input, t0)

        ' k2 = f(x + dt/2 * k1, u, t + dt/2)
        Dim state2 = initialState + k1 * (dtSng * 0.5F)
        Dim k2 = odeFunc(state2, input, t0 + dt * 0.5)

        ' k3 = f(x + dt/2 * k2, u, t + dt/2)
        Dim state3 = initialState + k2 * (dtSng * 0.5F)
        Dim k3 = odeFunc(state3, input, t0 + dt * 0.5)

        ' k4 = f(x + dt * k3, u, t + dt)
        Dim state4 = initialState + k3 * dtSng
        Dim k4 = odeFunc(state4, input, t0 + dt)

        ' 加权平均: x(t+dt) = x(t) + dt/6 * (k1 + 2*k2 + 2*k3 + k4)
        Dim weightedSum = k1 + k2 * 2.0F + k3 * 2.0F + k4
        Dim result = initialState + weightedSum * (dtSng / 6.0F)

        Return result
    End Function

    ''' <summary>
    ''' 自适应步长RK45求解器（Dormand-Prince方法简化版）
    ''' 根据误差估计自动调整步长
    ''' </summary>
    Public Function AdaptiveRK45Step(odeFunc As ODEFunction, initialState As Tensor, input As Tensor,
                                      t0 As Double, ByRef dt As Double, tolerance As Double) As Tensor
        Dim safetyFactor = 0.9
        Dim minDt = 0.0001
        Dim maxDt = 0.1

        ' 使用当前步长计算RK4解
        Dim rk4Result = RK4Step(odeFunc, initialState, input, t0, dt)

        ' 使用半步长计算两次（更高精度）
        Dim halfDt = dt / 2.0
        Dim tempState = RK4Step(odeFunc, initialState, input, t0, halfDt)
        Dim rk4ResultHalf = RK4Step(odeFunc, tempState, input, t0 + halfDt, halfDt)

        ' 估计误差
        Dim [error] As Double = 0.0
        For i = 0 To initialState.Length - 1
            Dim diff = std.Abs(rk4Result(i) - rk4ResultHalf(i))
            [error] = std.Max([error], diff)
        Next

        ' 调整步长
        If [error] < tolerance Then
            ' 接受结果，可能增大步长
            dt = std.Min(maxDt, dt * safetyFactor * std.Pow(tolerance / ([error] + 0.0000000001), 0.2))
            Return rk4ResultHalf
        Else
            ' 拒绝结果，减小步长重试
            dt = std.Max(minDt, dt * safetyFactor * std.Pow(tolerance / ([error] + 0.0000000001), 0.25))
            Return AdaptiveRK45Step(odeFunc, initialState, input, t0, dt, tolerance)
        End If
    End Function

End Module

#End Region

#Region "激活函数模块"

''' <summary>
''' 神经网络激活函数集合
''' </summary>
Public Module ActivationFunctions

    ''' <summary>
    ''' Sigmoid激活函数: σ(x) = 1 / (1 + e^(-x))
    ''' 输出范围: (0, 1)
    ''' </summary>
    Public Function Sigmoid(x As Tensor) As Tensor
        Return x.Apply(Function(v As Double) As Double
                           If v < -20 Then Return 0.0
                           If v > 20 Then Return 1.0
                           Return 1.0 / (1.0 + std.Exp(-v))
                       End Function)
    End Function

    ''' <summary>
    ''' Sigmoid导数: σ'(x) = σ(x) * (1 - σ(x))
    ''' </summary>
    Public Function SigmoidDerivative(sigmoidOutput As Tensor) As Tensor
        Return sigmoidOutput.Apply(Function(v As Double) v * (1.0 - v))
    End Function

    ''' <summary>
    ''' Tanh激活函数: tanh(x)
    ''' 输出范围: (-1, 1)
    ''' </summary>
    Public Function Tanh(x As Tensor) As Tensor
        Return x.Apply(Function(v As Double) std.Tanh(v))
    End Function

    ''' <summary>
    ''' Tanh导数: tanh'(x) = 1 - tanh(x)^2
    ''' </summary>
    Public Function TanhDerivative(tanhOutput As Tensor) As Tensor
        Return tanhOutput.Apply(Function(v As Double) 1.0 - v * v)
    End Function

    ''' <summary>
    ''' ReLU激活函数: max(0, x)
    ''' </summary>
    Public Function ReLU(x As Tensor) As Tensor
        Return x.Apply(Function(v As Double) std.Max(0.0, v))
    End Function

    ''' <summary>
    ''' ReLU导数
    ''' </summary>
    Public Function ReLUDerivative(x As Tensor) As Tensor
        Return x.Apply(Function(v As Double) If(v > 0, 1.0, 0.0))
    End Function

    ''' <summary>
    ''' Leaky ReLU: max(αx, x)，其中α通常为0.01
    ''' </summary>
    Public Function LeakyReLU(x As Tensor, Optional alpha As Double = 0.01) As Tensor
        Return x.Apply(Function(v As Double) If(v > 0, v, alpha * v))
    End Function

    ''' <summary>
    ''' Softmax激活函数（用于多分类输出层）
    ''' </summary>
    Public Function Softmax(x As Tensor) As Tensor
        ' 减去最大值以提高数值稳定性
        Dim maxVal = Double.MinValue
        For i = 0 To x.Length - 1
            maxVal = std.Max(maxVal, x(i))
        Next

        Dim expSum = 0.0
        Dim expValues = New Double(x.Length - 1) {}

        For i = 0 To x.Length - 1
            expValues(i) = std.Exp(x(i) - maxVal)
            expSum += expValues(i)
        Next

        Dim result = New Tensor(x.Shape)
        For i = 0 To x.Length - 1
            result(i) = expValues(i) / expSum
        Next

        Return result
    End Function

End Module

#End Region

#Region "液态神经元单元"

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

#End Region

#Region "液态神经网络层"

''' <summary>
''' 液态神经网络层
''' 包含多个LiquidCell，支持多层堆叠
''' </summary>
Public Class LiquidLayer
    Implements IDisposable

    Private _disposed As Boolean = False

#Region "属性"

    ''' <summary>
    ''' 层中的神经元单元
    ''' </summary>
    Public ReadOnly Property Cells As List(Of LiquidCell)

    ''' <summary>
    ''' 层的隐藏维度
    ''' </summary>
    Public ReadOnly Property HiddenSize As Integer

    ''' <summary>
    ''' 输入维度
    ''' </summary>
    Public ReadOnly Property InputSize As Integer

    ''' <summary>
    ''' 层数
    ''' </summary>
    Public ReadOnly Property NumLayers As Integer

    ''' <summary>
    ''' 激活函数类型
    ''' </summary>
    Public Property ActivationType As String

    ''' <summary>
    ''' 是否使用层归一化
    ''' </summary>
    Public Property UseLayerNorm As Boolean = False

    ''' <summary>
    ''' 层归一化参数 - 缩放因子γ
    ''' </summary>
    Public Property LayerNormGamma As Tensor

    ''' <summary>
    ''' 层归一化参数 - 偏移因子β
    ''' </summary>
    Public Property LayerNormBeta As Tensor

#End Region

#Region "构造函数"

    ''' <summary>
    ''' 创建液态神经网络层
    ''' </summary>
    ''' <param name="inputSize">输入维度</param>
    ''' <param name="hiddenSize">隐藏维度</param>
    ''' <param name="numLayers">层数</param>
    ''' <param name="activationType">激活函数类型</param>
    ''' <param name="seed">随机种子</param>
    Public Sub New(inputSize As Integer, hiddenSize As Integer, numLayers As Integer,
                   Optional activationType As String = "tanh", Optional seed As Integer? = Nothing)
        Me.InputSize = inputSize
        Me.HiddenSize = hiddenSize
        Me.NumLayers = numLayers
        Me.ActivationType = activationType

        _Cells = New List(Of LiquidCell)()

        ' 创建多层LiquidCell
        For i = 0 To numLayers - 1
            Dim cellInputSize = If(i = 0, inputSize, hiddenSize)
            Dim cellSeed = If(seed, seed + i * 10)
            Dim cell As New LiquidCell(hiddenSize, cellInputSize, activationType, cellSeed)
            _Cells.Add(cell)
        Next

        ' 初始化层归一化参数
        If UseLayerNorm Then
            _LayerNormGamma = Tensor.Ones({hiddenSize})
            _LayerNormBeta = Tensor.Zeros({hiddenSize})
        End If
    End Sub

#End Region

#Region "核心方法"

    ''' <summary>
    ''' 前向传播
    ''' </summary>
    ''' <param name="input">输入张量</param>
    ''' <param name="dt">时间步长</param>
    ''' <param name="solverType">ODE求解器类型</param>
    ''' <returns>输出状态</returns>
    Public Function Forward(input As Tensor, dt As Double, Optional solverType As String = "rk4") As Tensor
        Dim currentInput = input

        For Each cell In _Cells
            currentInput = cell.Forward(currentInput, dt, solverType)

            ' 应用层归一化
            If UseLayerNorm Then
                currentInput = ApplyLayerNorm(currentInput)
            End If
        Next

        Return currentInput
    End Function

    ''' <summary>
    ''' 应用层归一化
    ''' </summary>
    Private Function ApplyLayerNorm(x As Tensor) As Tensor
        ' 计算均值和方差
        Dim mean = x.Mean()
        Dim variance = 0.0
        For i = 0 To x.Length - 1
            variance += (x(i) - mean) * (x(i) - mean)
        Next
        variance /= x.Length

        ' 归一化
        Dim normalized = x.Apply(Function(v As Double) (v - mean) / std.Sqrt(variance + 0.00000001))

        ' 缩放和偏移
        Dim result = Tensor.Zeros(x.Shape)
        For i = 0 To x.Length - 1
            result(i) = normalized(i) * _LayerNormGamma(i) + _LayerNormBeta(i)
        Next

        Return result
    End Function

    ''' <summary>
    ''' 重置所有神经元状态
    ''' </summary>
    Public Sub ResetState()
        For Each cell In _Cells
            cell.ResetState()
        Next
    End Sub

    ''' <summary>
    ''' 获取所有层的输出状态
    ''' </summary>
    Public Function GetAllStates() As List(Of Tensor)
        Dim states As New List(Of Tensor)()
        For Each cell In _Cells
            states.Add(CType(cell.State.Clone(), Tensor))
        Next
        Return states
    End Function

    ''' <summary>
    ''' 获取最后一层的输出状态
    ''' </summary>
    Public Function GetOutputState() As Tensor
        Return _Cells(_Cells.Count - 1).State
    End Function

    ''' <summary>
    ''' 获取所有可训练参数
    ''' </summary>
    Public Function GetParameters() As Dictionary(Of String, Tensor)
        Dim allParams As New Dictionary(Of String, Tensor)()

        For i = 0 To _Cells.Count - 1
            Dim cellParams = _Cells(i).GetParameters()
            For Each kvp In cellParams
                allParams.Add($"layer{i}_{kvp.Key}", kvp.Value)
            Next
        Next

        If UseLayerNorm Then
            allParams.Add("layer_norm_gamma", _LayerNormGamma)
            allParams.Add("layer_norm_beta", _LayerNormBeta)
        End If

        Return allParams
    End Function

#End Region

#Region "IDisposable实现"

    Public Sub Dispose() Implements IDisposable.Dispose
        If Not _disposed Then
            For Each cell In _Cells
                cell?.Dispose()
            Next
            _LayerNormGamma?.Dispose()
            _LayerNormBeta?.Dispose()
            _disposed = True
        End If
    End Sub

#End Region

End Class

#End Region

#Region "完整液态神经网络"

''' <summary>
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

#End Region

#Region "训练器"

''' <summary>
''' 液态神经网络训练器
''' 实现基于时间的反向传播(BPTT)训练算法
''' </summary>
Public Class LNNTrainer

#Region "属性"

    ''' <summary>
    ''' 要训练的网络
    ''' </summary>
    Public Property Network As LiquidNeuralNetwork

    ''' <summary>
    ''' 学习率
    ''' </summary>
    Public Property LearningRate As Double = 0.001

    ''' <summary>
    ''' 优化器类型
    ''' </summary>
    Public Property OptimizerType As String = "adam"

    ''' <summary>
    ''' Adam优化器参数 - 一阶矩估计
    ''' </summary>
    Private _AdamM As Dictionary(Of String, Tensor)

    ''' <summary>
    ''' Adam优化器参数 - 二阶矩估计
    ''' </summary>
    Private _AdamV As Dictionary(Of String, Tensor)

    ''' <summary>
    ''' Adam优化器时间步
    ''' </summary>
    Private _AdamT As Integer = 0

    ''' <summary>
    ''' Adam beta1参数
    ''' </summary>
    Public Property AdamBeta1 As Double = 0.9

    ''' <summary>
    ''' Adam beta2参数
    ''' </summary>
    Public Property AdamBeta2 As Double = 0.999

    ''' <summary>
    ''' Adam epsilon参数
    ''' </summary>
    Public Property AdamEpsilon As Double = 0.00000001

    ''' <summary>
    ''' 梯度裁剪阈值
    ''' </summary>
    Public Property GradientClipValue As Double = 1.0

    ''' <summary>
    ''' 是否使用梯度裁剪
    ''' </summary>
    Public Property UseGradientClipping As Boolean = True

#End Region

#Region "构造函数"

    Public Sub New(network As LiquidNeuralNetwork, Optional learningRate As Double = 0.001)
        _Network = network
        _LearningRate = learningRate

        ' 初始化Adam优化器状态
        InitializeAdamState()
    End Sub

    Private Sub InitializeAdamState()
        _AdamM = New Dictionary(Of String, Tensor)()
        _AdamV = New Dictionary(Of String, Tensor)()

        Dim params = _Network.GetParameters()
        For Each kvp In params
            _AdamM.Add(kvp.Key, Tensor.Zeros(kvp.Value.Shape))
            _AdamV.Add(kvp.Key, Tensor.Zeros(kvp.Value.Shape))
        Next
    End Sub

#End Region

#Region "损失函数"

    ''' <summary>
    ''' 计算均方误差损失
    ''' </summary>
    Public Shared Function MSE(predicted As Tensor, target As Tensor) As Double
        If Not predicted.Shape.SequenceEqual(target.Shape) Then
            Throw New ArgumentException("预测值和目标值形状必须相同")
        End If

        Dim sum As Double = 0
        For i = 0 To predicted.Length - 1
            Dim diff = predicted(i) - target(i)
            sum += diff * diff
        Next

        Return sum / predicted.Length
    End Function

    ''' <summary>
    ''' 计算均方误差损失的梯度
    ''' </summary>
    Public Shared Function MSEGradient(predicted As Tensor, target As Tensor) As Tensor
        Dim gradient = Tensor.Zeros(predicted.Shape)
        Dim n = predicted.Length

        For i = 0 To n - 1
            gradient(i) = 2.0 * (predicted(i) - target(i)) / n
        Next

        Return gradient
    End Function

    ''' <summary>
    ''' 计算平均绝对误差损失
    ''' </summary>
    Public Shared Function MAE(predicted As Tensor, target As Tensor) As Double
        Dim sum As Double = 0
        For i = 0 To predicted.Length - 1
            sum += std.Abs(predicted(i) - target(i))
        Next
        Return sum / predicted.Length
    End Function

#End Region

#Region "训练方法"

    ''' <summary>
    ''' 训练单个时间步
    ''' </summary>
    ''' <param name="input">输入</param>
    ''' <param name="target">目标输出</param>
    ''' <param name="dt">时间步长</param>
    ''' <returns>损失值</returns>
    Public Function TrainStep(input As Tensor, target As Tensor, Optional dt As Double? = Nothing) As Double
        ' 前向传播
        Dim output = _Network.Forward(input, dt)

        ' 计算损失
        Dim loss = MSE(output, target)

        ' 计算输出层梯度
        Dim outputGradient = MSEGradient(output, target)

        ' 反向传播（简化版本）
        Backpropagate(outputGradient)

        ' 更新参数
        UpdateParameters()

        Return loss
    End Function

    ''' <summary>
    ''' 训练完整序列
    ''' </summary>
    ''' <param name="inputSequence">输入序列</param>
    ''' <param name="targetSequence">目标序列</param>
    ''' <param name="dt">时间步长</param>
    ''' <returns>平均损失</returns>
    Public Function TrainSequence(inputSequence As Tensor, targetSequence As Tensor, Optional dt As Double? = Nothing) As Double
        Dim actualDt = If(dt, _Network.DefaultDt)
        Dim seqLength = inputSequence.Shape(0)
        Dim totalLoss As Double = 0

        ' 重置网络状态
        _Network.ResetState()

        ' 逐时间步训练
        For t = 0 To seqLength - 1
            ' 获取当前输入和目标
            Dim currentInput = Tensor.Zeros({_Network.InputSize})
            Dim currentTarget = Tensor.Zeros({_Network.OutputSize})

            For i = 0 To _Network.InputSize - 1
                currentInput(i) = inputSequence(t, i)
            Next

            For i = 0 To _Network.OutputSize - 1
                currentTarget(i) = targetSequence(t, i)
            Next

            ' 训练当前时间步
            totalLoss += TrainStep(currentInput, currentTarget, actualDt)
        Next

        Return totalLoss / seqLength
    End Function

    ''' <summary>
    ''' 反向传播（简化实现）
    ''' </summary>
    Private Sub Backpropagate(outputGradient As Tensor)
        ' 获取网络参数
        Dim hiddenSize = _Network.HiddenSize
        Dim outputSize = _Network.OutputSize

        ' 计算输出层权重梯度
        ' dL/dW_out = hidden^T @ outputGradient
        Dim hiddenState = _Network.LiquidLayer.GetOutputState()
        Dim hiddenReshaped = New Tensor(hiddenState.Data, 1, hiddenSize)
        Dim gradReshaped = New Tensor(outputGradient.Data, outputSize, 1)

        ' 输出权重梯度
        For i = 0 To hiddenSize - 1
            For j = 0 To outputSize - 1
                _Network.OutputWeightGradient(i, j) += hiddenState(i) * outputGradient(j)
            Next
        Next

        ' 输出偏置梯度
        For i = 0 To outputSize - 1
            _Network.OutputBiasGradient(i) += outputGradient(i)
        Next

        ' 传播到隐藏层（简化：使用固定的时间常数导数）
        Dim hiddenGradient = Tensor.Zeros({hiddenSize})
        For i = 0 To hiddenSize - 1
            For j = 0 To outputSize - 1
                hiddenGradient(i) += outputGradient(j) * _Network.OutputWeight(i, j)
            Next
        Next

        ' 梯度裁剪
        If UseGradientClipping Then
            Dim norm = hiddenGradient.L2Norm()
            If norm > GradientClipValue Then
                Dim scale = GradientClipValue / norm
                hiddenGradient = hiddenGradient * CSng(scale)
            End If
        End If

        ' 更新液态层梯度（简化版本）
        UpdateLiquidLayerGradients(hiddenGradient)
    End Sub

    ''' <summary>
    ''' 更新液态层梯度
    ''' </summary>
    Private Sub UpdateLiquidLayerGradients(hiddenGradient As Tensor)
        ' 获取最后一层的cell
        Dim lastCell = _Network.LiquidLayer.Cells(_Network.LiquidLayer.Cells.Count - 1)

        ' 更新循环权重梯度
        Dim state = lastCell.State
        For i = 0 To lastCell.HiddenSize - 1
            For j = 0 To lastCell.HiddenSize - 1
                lastCell.WeightRecurrentGradient(i, j) += hiddenGradient(i) * state(j) * 0.1
            Next
        Next

        ' 更新偏置梯度
        For i = 0 To lastCell.HiddenSize - 1
            lastCell.BiasGradient(i) += hiddenGradient(i) * 0.1
        Next
    End Sub

    ''' <summary>
    ''' 更新参数
    ''' </summary>
    Private Sub UpdateParameters()
        Select Case OptimizerType.ToLower()
            Case "adam"
                UpdateParametersAdam()
            Case "sgd"
                UpdateParametersSGD()
            Case Else
                UpdateParametersAdam()
        End Select
    End Sub

    ''' <summary>
    ''' 使用SGD更新参数
    ''' </summary>
    Private Sub UpdateParametersSGD()
        ' 更新输出层权重
        For i = 0 To _Network.OutputWeight.Shape(0) - 1
            For j = 0 To _Network.OutputWeight.Shape(1) - 1
                _Network.OutputWeight(i, j) -= LearningRate * _Network.OutputWeightGradient(i, j)
            Next
        Next

        ' 更新输出层偏置
        For i = 0 To _Network.OutputBias.Length - 1
            _Network.OutputBias(i) -= LearningRate * _Network.OutputBiasGradient(i)
        Next

        ' 清零梯度
        ZeroGradients()
    End Sub

    ''' <summary>
    ''' 使用Adam优化器更新参数
    ''' </summary>
    Private Sub UpdateParametersAdam()
        _AdamT += 1
        Dim params = _Network.GetParameters()

        ' 更新输出权重
        UpdateParamAdam(_Network.OutputWeight, _Network.OutputWeightGradient, "output_weight")

        ' 更新输出偏置
        UpdateParamAdam(_Network.OutputBias, _Network.OutputBiasGradient, "output_bias")

        ' 更新液态层参数
        For Each cell In _Network.LiquidLayer.Cells
            UpdateParamAdam(cell.Tau, cell.TauGradient, "tau")
            UpdateParamAdam(cell.WeightInput, cell.WeightInputGradient, "weight_input")
            UpdateParamAdam(cell.WeightRecurrent, cell.WeightRecurrentGradient, "weight_recurrent")
            UpdateParamAdam(cell.Bias, cell.BiasGradient, "bias")
        Next

        ' 清零梯度
        ZeroGradients()
    End Sub

    ''' <summary>
    ''' 使用Adam更新单个参数
    ''' </summary>
    Private Sub UpdateParamAdam(param As Tensor, gradient As Tensor, name As String)
        Dim key = name
        If Not _AdamM.ContainsKey(key) Then
            _AdamM.Add(key, Tensor.Zeros(param.Shape))
            _AdamV.Add(key, Tensor.Zeros(param.Shape))
        End If

        Dim m = _AdamM(key)
        Dim v = _AdamV(key)

        For i = 0 To param.Length - 1
            ' 更新一阶矩估计
            m(i) = AdamBeta1 * m(i) + (1 - AdamBeta1) * gradient(i)

            ' 更新二阶矩估计
            v(i) = AdamBeta2 * v(i) + (1 - AdamBeta2) * gradient(i) * gradient(i)

            ' 偏差校正
            Dim mHat = m(i) / (1 - std.Pow(AdamBeta1, _AdamT))
            Dim vHat = v(i) / (1 - std.Pow(AdamBeta2, _AdamT))

            ' 更新参数
            param(i) -= LearningRate * mHat / (std.Sqrt(vHat) + AdamEpsilon)
        Next
    End Sub

    ''' <summary>
    ''' 清零所有梯度
    ''' </summary>
    Private Sub ZeroGradients()
        ' 清零输出层梯度
        For i = 0 To _Network.OutputWeightGradient.Length - 1
            _Network.OutputWeightGradient(i) = 0
        Next
        For i = 0 To _Network.OutputBiasGradient.Length - 1
            _Network.OutputBiasGradient(i) = 0
        Next

        ' 清零液态层梯度
        For Each cell In _Network.LiquidLayer.Cells
            For i = 0 To cell.TauGradient.Length - 1
                cell.TauGradient(i) = 0
            Next
            For i = 0 To cell.WeightInputGradient.Length - 1
                cell.WeightInputGradient(i) = 0
            Next
            For i = 0 To cell.WeightRecurrentGradient.Length - 1
                cell.WeightRecurrentGradient(i) = 0
            Next
            For i = 0 To cell.BiasGradient.Length - 1
                cell.BiasGradient(i) = 0
            Next
        Next
    End Sub

#End Region

#Region "训练循环"

    ''' <summary>
    ''' 训练多个epoch
    ''' </summary>
    ''' <param name="inputSequences">输入序列列表</param>
    ''' <param name="targetSequences">目标序列列表</param>
    ''' <param name="epochs">训练轮数</param>
    ''' <param name="dt">时间步长</param>
    ''' <returns>每轮的平均损失</returns>
    Public Function Fit(inputSequences As List(Of Tensor), targetSequences As List(Of Tensor),
                        epochs As Integer, Optional dt As Double? = Nothing) As List(Of Double)
        If inputSequences.Count <> targetSequences.Count Then
            Throw New ArgumentException("输入序列和目标序列数量必须相同")
        End If

        Dim losses As New List(Of Double)()

        For epoch = 1 To epochs
            Dim epochLoss As Double = 0

            For i = 0 To inputSequences.Count - 1
                Dim seqLoss = TrainSequence(inputSequences(i), targetSequences(i), dt)
                epochLoss += seqLoss
            Next

            epochLoss /= inputSequences.Count
            losses.Add(epochLoss)

            ' 输出训练进度
            If epoch Mod 10 = 0 OrElse epoch = 1 Then
                Console.WriteLine($"Epoch {epoch}/{epochs}, Loss: {epochLoss:F6}")
            End If
        Next

        Return losses
    End Function

#End Region

End Class

#End Region

#Region "时间序列工具类"

''' <summary>
''' 时间序列处理工具
''' </summary>
Public Module TimeSeriesUtils

    ''' <summary>
    ''' 创建滑动窗口数据集
    ''' </summary>
    ''' <param name="data">原始时间序列数据</param>
    ''' <param name="windowSize">窗口大小</param>
    ''' <param name="forecastHorizon">预测步长</param>
    ''' <returns>输入窗口和目标值</returns>
    Public Function CreateSlidingWindowDataset(data As Double(), windowSize As Integer, forecastHorizon As Integer) As (inputs As List(Of Tensor), targets As List(Of Tensor))
        Dim inputs As New List(Of Tensor)()
        Dim targets As New List(Of Tensor)()

        For i = 0 To data.Length - windowSize - forecastHorizon
            ' 创建输入窗口
            Dim inputWindow = New Tensor(windowSize)
            For j = 0 To windowSize - 1
                inputWindow(j) = data(i + j)
            Next

            ' 创建目标值
            Dim targetValue = New Tensor(forecastHorizon)
            For j = 0 To forecastHorizon - 1
                targetValue(j) = data(i + windowSize + j)
            Next

            inputs.Add(inputWindow)
            targets.Add(targetValue)
        Next

        Return (inputs, targets)
    End Function

    ''' <summary>
    ''' 归一化数据到[0, 1]范围
    ''' </summary>
    Public Function Normalize(data As Double()) As (normalized As Double(), min As Double, max As Double)
        Dim minVal = data.Min()
        Dim maxVal = data.Max()
        Dim range = maxVal - minVal

        If range = 0 Then
            Return (data.Select(Function(x) 0.5).ToArray(), minVal, maxVal)
        End If

        Dim normalized = data.Select(Function(x) (x - minVal) / range).ToArray()
        Return (normalized, minVal, maxVal)
    End Function

    ''' <summary>
    ''' 反归一化
    ''' </summary>
    Public Function Denormalize(normalized As Double(), min As Double, max As Double) As Double()
        Dim range = max - min
        Return normalized.Select(Function(x) x * range + min).ToArray()
    End Function

    ''' <summary>
    ''' 标准化数据（零均值，单位方差）
    ''' </summary>
    Public Function Standardize(data As Double()) As (standardized As Double(), mean As Double, std As Double)
        Dim mean = data.Average()
        Dim variance = data.Select(Function(x) (x - mean) * (x - mean)).Average()
        Dim std As Double = System.Math.Sqrt(variance)

        If std = 0 Then
            Return (data.Select(Function(x) 0.0).ToArray(), mean, std)
        End If

        Dim standardized = data.Select(Function(x) (x - mean) / std).ToArray()
        Return (standardized, mean, std)
    End Function

    ''' <summary>
    ''' 反标准化
    ''' </summary>
    Public Function Destandardize(standardized As Double(), mean As Double, std As Double) As Double()
        Return standardized.Select(Function(x) x * std + mean).ToArray()
    End Function

    ''' <summary>
    ''' 生成正弦波数据
    ''' </summary>
    Public Function GenerateSineWave(length As Integer, frequency As Double, amplitude As Double, phase As Double, noiseLevel As Double) As Double()
        Dim data = New Double(length - 1) {}
        Dim random As New Random(42)

        For i = 0 To length - 1
            data(i) = amplitude * std.Sin(2 * std.PI * frequency * i + phase)
            If noiseLevel > 0 Then
                data(i) += (random.NextDouble() * 2 - 1) * noiseLevel
            End If
        Next

        Return data
    End Function

    ''' <summary>
    ''' 计算预测指标
    ''' </summary>
    Public Function CalculateMetrics(predicted As Double(), actual As Double()) As (mse As Double, mae As Double, rmse As Double, mape As Double)
        Dim n = predicted.Length
        Dim sumSquaredError = 0.0
        Dim sumAbsoluteError = 0.0
        Dim sumAbsolutePercentError = 0.0

        For i = 0 To n - 1
            Dim [error] = predicted(i) - actual(i)
            sumSquaredError += [error] ^ 2
            sumAbsoluteError += std.Abs([error])

            If actual(i) <> 0 Then
                sumAbsolutePercentError += std.Abs([error] / actual(i))
            End If
        Next

        Dim mse = sumSquaredError / n
        Dim mae = sumAbsoluteError / n
        Dim rmse = std.Sqrt(mse)
        Dim mape = sumAbsolutePercentError / n * 100

        Return (mse, mae, rmse, mape)
    End Function

End Module

#End Region

#Region "示例和测试"

''' <summary>
''' 液态神经网络示例和测试
''' </summary>
Public Module LNNExamples

    ''' <summary>
    ''' 运行基础时间序列预测示例
    ''' </summary>
    Public Sub RunBasicTimeSeriesExample()
        Console.WriteLine("=== 液态神经网络 - 时间序列预测示例 ===")
        Console.WriteLine()

        ' 1. 生成示例数据
        Console.WriteLine("1. 生成正弦波时间序列数据...")
        Dim data = TimeSeriesUtils.GenerateSineWave(500, 0.05, 1.0, 0.0, 0.05)
        Console.WriteLine($"   数据长度: {data.Length}")

        ' 2. 归一化数据
        Console.WriteLine("2. 归一化数据...")
        Dim normalizedData = TimeSeriesUtils.Normalize(data)

        ' 3. 创建滑动窗口数据集
        Console.WriteLine("3. 创建滑动窗口数据集...")
        Dim windowSize = 20
        Dim forecastHorizon = 5
        Dim dataset = TimeSeriesUtils.CreateSlidingWindowDataset(normalizedData.normalized, windowSize, forecastHorizon)
        Console.WriteLine($"   窗口大小: {windowSize}, 预测步长: {forecastHorizon}")
        Console.WriteLine($"   样本数量: {dataset.inputs.Count}")

        ' 4. 划分训练集和测试集
        Console.WriteLine("4. 划分训练集和测试集...")
        Dim splitIndex = CInt(dataset.inputs.Count * 0.8)
        Dim trainInputs = dataset.inputs.Take(splitIndex).ToList()
        Dim trainTargets = dataset.targets.Take(splitIndex).ToList()
        Dim testInputs = dataset.inputs.Skip(splitIndex).ToList()
        Dim testTargets = dataset.targets.Skip(splitIndex).ToList()
        Console.WriteLine($"   训练样本: {trainInputs.Count}, 测试样本: {testInputs.Count}")

        ' 5. 创建液态神经网络
        Console.WriteLine("5. 创建液态神经网络...")
        Dim lnn As New LiquidNeuralNetwork(
            inputSize:=windowSize,
            hiddenSize:=32,
            outputSize:=forecastHorizon,
            numLiquidLayers:=2,
            activationType:="tanh",
            outputActivation:="tanh",
            seed:=42
        )
        lnn.DefaultDt = 0.1
        lnn.SolverType = "rk4"
        Console.WriteLine($"   参数总数: {lnn.GetParameterCount()}")

        ' 6. 创建训练器
        Console.WriteLine("6. 创建训练器...")
        Dim trainer As New LNNTrainer(lnn, 0.005)
        trainer.OptimizerType = "adam"
        trainer.UseGradientClipping = True

        ' 7. 训练模型
        Console.WriteLine("7. 开始训练...")
        Dim epochs = 50

        ' 将数据转换为序列格式
        Dim trainSequences As New List(Of Tensor)()
        Dim targetSequences As New List(Of Tensor)()

        For i = 0 To trainInputs.Count - 1
            ' 将输入reshape为序列格式
            Dim inputSeq = New Tensor(1, windowSize)
            For j = 0 To windowSize - 1
                inputSeq(0, j) = trainInputs(i)(j)
            Next
            trainSequences.Add(inputSeq)

            ' 目标已经是正确的形状
            targetSequences.Add(trainTargets(i))
        Next

        Dim losses = trainer.Fit(trainSequences, targetSequences, epochs)

        ' 8. 评估模型
        Console.WriteLine()
        Console.WriteLine("8. 评估模型...")
        Dim allPredictions As New List(Of Double)()
        Dim allActuals As New List(Of Double)()

        lnn.ResetState()
        For i = 0 To std.Min(testInputs.Count - 1, 50)
            Dim inputSeq = New Tensor(1, windowSize)
            For j = 0 To windowSize - 1
                inputSeq(0, j) = testInputs(i)(j)
            Next

            Dim predicted = lnn.Forward(testInputs(i))
            Dim actual = testTargets(i)

            allPredictions.Add(predicted(0))
            allActuals.Add(actual(0))
        Next

        ' 9. 计算评估指标
        Console.WriteLine("9. 计算评估指标...")
        Dim metrics = TimeSeriesUtils.CalculateMetrics(allPredictions.ToArray(), allActuals.ToArray())
        Console.WriteLine($"   MSE: {metrics.mse:F6}")
        Console.WriteLine($"   MAE: {metrics.mae:F6}")
        Console.WriteLine($"   RMSE: {metrics.rmse:F6}")
        Console.WriteLine($"   MAPE: {metrics.mape:F2}%")

        ' 10. 输出一些预测示例
        Console.WriteLine()
        Console.WriteLine("10. 预测示例（前5个）:")
        For i = 0 To std.Min(4, allPredictions.Count - 1)
            Console.WriteLine($"   预测: {allPredictions(i):F4}, 实际: {allActuals(i):F4}, 误差: { std.Abs(allPredictions(i) - allActuals(i)):F4}")
        Next

        Console.WriteLine()
        Console.WriteLine("=== 示例完成 ===")

        lnn.Dispose()
    End Sub

    ''' <summary>
    ''' 运行ODE求解器测试
    ''' </summary>
    Public Sub TestODESolvers()
        Console.WriteLine("=== ODE求解器测试 ===")

        ' 测试简单的指数衰减: dx/dt = -x
        ' 解析解: x(t) = x0 * e^(-t)
        Dim initialState = Tensor.Ones({1})
        initialState(0) = 1.0

        Dim decayFunc As ODESolver.ODEFunction = Function(state, input, time)
                                                     Dim derivative = Tensor.Zeros(state.Shape)
                                                     derivative(0) = -state(0)
                                                     Return derivative
                                                 End Function

        Dim dt = 0.1
        Dim steps = 50
        Dim t = 0.0

        ' 欧拉法
        Dim stateEuler = CType(initialState.Clone(), Tensor)
        For i = 1 To steps
            stateEuler = ODESolver.EulerStep(decayFunc, stateEuler, Nothing, t, dt)
            t += dt
        Next
        Console.WriteLine($"欧拉法结果: {stateEuler(0):F6} (解析解: {std.Exp(-steps * dt):F6})")

        ' RK4法
        t = 0.0
        Dim stateRK4 = CType(initialState.Clone(), Tensor)
        For i = 1 To steps
            stateRK4 = ODESolver.RK4Step(decayFunc, stateRK4, Nothing, t, dt)
            t += dt
        Next
        Console.WriteLine($"RK4法结果: {stateRK4(0):F6} (解析解: {std.Exp(-steps * dt):F6})")

        Console.WriteLine("=== 测试完成 ===")
    End Sub

    ''' <summary>
    ''' 测试单个LiquidCell
    ''' </summary>
    Public Sub TestLiquidCell()
        Console.WriteLine("=== LiquidCell测试 ===")

        ' 创建一个简单的液态神经元
        Dim cell As New LiquidCell(hiddenSize:=4, inputSize:=2, activationType:="tanh", seed:=42)

        ' 创建输入
        Dim input = New Tensor(2)
        input(0) = 0.5
        input(1) = -0.3

        Console.WriteLine($"输入: [{input(0):F4}, {input(1):F4}]")
        Console.WriteLine($"初始状态: [{cell.State(0):F4}, {cell.State(1):F4}, {cell.State(2):F4}, {cell.State(3):F4}]")

        ' 模拟多个时间步
        Dim dt = 0.1
        Console.WriteLine()
        Console.WriteLine("时间演化:")
        For i = 1 To 10
            Dim newState = cell.Forward(input, dt, "rk4")
            Console.WriteLine($"  t={i * dt:F1}: [{newState(0):F4}, {newState(1):F4}, {newState(2):F4}, {newState(3):F4}]")
        Next

        cell.Dispose()
        Console.WriteLine("=== 测试完成 ===")
    End Sub

End Module

#End Region
