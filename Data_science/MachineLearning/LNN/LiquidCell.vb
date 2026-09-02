#Region "Microsoft.VisualBasic::3e4a650020ff1a8250e3a96237f4d17c, Data_science\MachineLearning\LNN\LiquidCell.vb"

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

    '   Total Lines: 977
    '    Code Lines: 546 (55.89%)
    ' Comment Lines: 264 (27.02%)
    '    - Xml Docs: 90.15%
    ' 
    '   Blank Lines: 167 (17.09%)
    '     File Size: 34.13 KB


    ' Enum LiquidMode
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class StepRecord
    ' 
    '     Properties: dt, p, s1, s2, s3
    '                 s4, solver, u, x0
    ' 
    ' Class ParameterPair
    ' 
    '     Properties: Gradient, Name, Value
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ToString
    ' 
    ' Class LiquidCell
    ' 
    '     Properties: ActivationType, Bias, BiasGate, BiasGateGradient, BiasGradient
    '                 HasGate, HiddenSize, InputSize, LastInputGradient, Mode
    '                 RecordCount, State, Tau, TauGradient, TauMax
    '                 TauMin, Training, UseBoundedTau, WeightGate, WeightGateGradient
    '                 WeightGateInput, WeightGateInputGradient, WeightInput, WeightInputGradient, WeightRecurrent
    '                 WeightRecurrentGradient
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: ApplyActivation, ApplyActivationDerivative, Backward, BackwardCfC, BackwardEuler
    '               BackwardHeun, BackwardRK4, BackwardThroughF, ComputeDerivative, EffectiveTau
    '               EffectiveTauDerivative, Forward, ForwardCfC, GetGradients, GetParameterPairs
    '               GetParameters, GetSystemTau, LinearForward, ToRow
    ' 
    '     Sub: AccumulateParamGradients, ClearRecords, Dispose, EnsureGateParameters, PropagateToStateAndInput
    '          ResetState, SetMode, SetState, ZeroGradients
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 液态神经元的动力学模式
''' </summary>
Public Enum LiquidMode
    ''' <summary>
    ''' 连续时间 RNN（旧版本行为，向后兼容）：
    ''' <c>dh/dt = -h/τ + σ(W·h + U·u + b)</c>，τ 为可学习常数。
    ''' </summary>
    CT_RNN = 0
    ''' <summary>
    ''' 液态时间常数网络（LTC）：
    ''' <c>dh/dt = (1/τ + f(h,u)) ⊙ (A - h)</c>，
    ''' 其中 <c>A = σ(W·h + U·u + b)</c> 为目标状态，<c>f = σ(Wf·h + Uf·u + bf)</c> 为门控。
    ''' 系统时间常数 <c>τ^sys = τ / (1 + τ·f)</c> 随状态与输入自适应。
    ''' </summary>
    LTC = 1
    ''' <summary>
    ''' 闭式连续时间（CfC）：
    ''' <c>h(t+dt) = A + (h - A) ⊙ exp(-(1/τ + f)·dt)</c>，
    ''' 用解析解替代数值积分，单步只需一次前向求值。
    ''' </summary>
    CFC = 2
End Enum

''' <summary>
''' 单个时间步的前向记录，供反向模式自动微分回放
''' </summary>
''' <remarks>
''' 这里只登记 ODE 右端函数被求值过的状态点；各点的激活值、门控值与有效时间常数
''' 在反向传播时重新计算（recompute），从而把每步的显存占用压到常数个向量。
''' </remarks>
Public Class StepRecord

    ''' <summary>步首状态 h(t)</summary>
    Public Property x0 As Tensor
    ''' <summary>步内恒定的外部输入 u</summary>
    Public Property u As Tensor
    ''' <summary>真实时间步长（支持不规则采样）</summary>
    Public Property dt As Double
    ''' <summary>k1 的求值点</summary>
    Public Property s1 As Tensor
    ''' <summary>k2 的求值点（仅 RK4）</summary>
    Public Property s2 As Tensor
    ''' <summary>k3 的求值点（仅 RK4）</summary>
    Public Property s3 As Tensor
    ''' <summary>k4 的求值点（仅 RK4）</summary>
    Public Property s4 As Tensor
    ''' <summary>Heun 方法的预测点</summary>
    Public Property p As Tensor
    ''' <summary>使用的求解器标识：rk4 / heun / euler / cfc</summary>
    Public Property solver As String

End Class

''' <summary>
''' 参数张量与其梯度张量的配对
''' </summary>
''' <remarks>
''' 优化器（Adam/SGD）与梯度裁剪直接遍历这个配对列表，
''' 避免旧实现里 "tau" / "weight_input" 这类裸键在多层堆叠时互相覆盖的问题。
''' </remarks>
Public Class ParameterPair

    ''' <summary>全局唯一的参数名（形如 liquid_layer0_tau）</summary>
    Public ReadOnly Property Name As String
    ''' <summary>参数张量</summary>
    Public ReadOnly Property Value As Tensor
    ''' <summary>与参数同形的梯度累加器</summary>
    Public ReadOnly Property Gradient As Tensor

    Public Sub New(name As String, value As Tensor, gradient As Tensor)
        _Name = name
        _Value = value
        _Gradient = gradient
    End Sub

    Public Overrides Function ToString() As String
        Return $"{Name} {String.Join("x", Value.Shape)}"
    End Function

End Class

''' <summary>
''' 液态神经元单元 (Liquid Cell)
''' 实现基于ODE的连续时间神经元模型
''' 
''' 核心方程（LTC 模式）: 
'''     A     = σ(W·h + U·u + b)
'''     f     = σ(Wf·h + Uf·u + bf)
'''     τ^sys = τ / (1 + τ·f)
'''     dh/dt = (A - h) / τ^sys = (1/τ + f) ⊙ (A - h)
''' 其中:
''' - h: 神经元状态（代谢建模中对应代谢物浓度）
''' - τ: 基线时间常数（可学习）
''' - f: 门控，使时间常数依赖于状态与输入 —— 这是"液态"的来源
''' - W/U: 循环/输入权重
''' - u: 外部输入（酶表达、底物浓度等）
''' - b: 偏置
''' - σ: 激活函数
''' 
''' 训练时通过 <see cref="Backward(Tensor)"/> 做精确的反向模式自动微分，
''' 梯度可流经 τ、W、U、b 以及门控参数 Wf/Uf/bf。
''' </summary>
Public Class LiquidCell : Implements IDisposable

#Region "私有字段"

    Private _disposed As Boolean = False

    ''' <summary>前向记录栈（前向 push、反向 pop）</summary>
    Private ReadOnly _records As New List(Of StepRecord)()
    ''' <summary>本次反向得到的对外部输入的伴随向量 dL/du</summary>
    Private _lastInputGradient As Tensor

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
    ''' 时间常数 τ 的原始参数（可学习参数）
    ''' 经 <see cref="EffectiveTau"/> 映射为正的有效时间常数
    ''' </summary>
    Public Property Tau As Tensor

    ''' <summary>
    ''' 输入权重矩阵 U (InputSize × HiddenSize)
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
    ''' 门控的循环权重矩阵 Wf (HiddenSize × HiddenSize)，仅 LTC/CfC 模式使用
    ''' </summary>
    Public Property WeightGate As Tensor

    ''' <summary>
    ''' 门控的输入权重矩阵 Uf (InputSize × HiddenSize)，仅 LTC/CfC 模式使用
    ''' </summary>
    Public Property WeightGateInput As Tensor

    ''' <summary>
    ''' 门控的偏置向量 bf (HiddenSize)，仅 LTC/CfC 模式使用
    ''' </summary>
    Public Property BiasGate As Tensor

    ''' <summary>
    ''' 激活函数类型
    ''' </summary>
    Public Property ActivationType As String = "tanh"

    ''' <summary>
    ''' 动力学模式：<see cref="LiquidMode.CT_RNN"/> / LTC / CFC
    ''' </summary>
    Public Property Mode As LiquidMode

    ''' <summary>
    ''' 是否处于训练模式。为 True 时前向会登记 <see cref="StepRecord"/> 以支持反向传播。
    ''' </summary>
    Public Property Training As Boolean = False

    ''' <summary>
    ''' 当前是否启用了门控参数（即模式为 LTC 或 CfC）
    ''' </summary>
    Public ReadOnly Property HasGate As Boolean
        Get
            Return _WeightGate IsNot Nothing
        End Get
    End Property

    ''' <summary>
    ''' 最近一次 <see cref="Backward(Tensor)"/> 得到的对外部输入的梯度 dL/du
    ''' （用于多层堆叠时把梯度继续向前一层传播）
    ''' </summary>
    Public ReadOnly Property LastInputGradient As Tensor
        Get
            Return _lastInputGradient
        End Get
    End Property

    ''' <summary>
    ''' 尚未被反向消费的前向记录条数
    ''' </summary>
    Public ReadOnly Property RecordCount As Integer
        Get
            Return _records.Count
        End Get
    End Property

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

    ''' <summary>
    ''' 门控循环权重的梯度 (HiddenSize × HiddenSize)
    ''' </summary>
    Public Property WeightGateGradient As Tensor

    ''' <summary>
    ''' 门控输入权重的梯度 (InputSize × HiddenSize)
    ''' </summary>
    Public Property WeightGateInputGradient As Tensor

    ''' <summary>
    ''' 门控偏置的梯度 (HiddenSize)
    ''' </summary>
    Public Property BiasGateGradient As Tensor

#End Region

#Region "构造函数"

    ''' <summary>
    ''' 创建液态神经元单元
    ''' </summary>
    ''' <param name="hiddenSize">隐藏层神经元数量</param>
    ''' <param name="inputSize">输入维度</param>
    ''' <param name="activationType">激活函数类型: "tanh", "sigmoid", "relu"</param>
    ''' <param name="seed">随机种子（可选）</param>
    ''' <param name="mode">动力学模式，默认 <see cref="LiquidMode.CT_RNN"/> 以保持向后兼容</param>
    Public Sub New(hiddenSize As Integer, inputSize As Integer,
                   Optional activationType As String = "tanh",
                   Optional seed As Integer? = Nothing,
                   Optional mode As LiquidMode = LiquidMode.CT_RNN)
        Me.HiddenSize = hiddenSize
        Me.InputSize = inputSize
        Me.ActivationType = activationType.ToLower()
        Me.Mode = mode

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
        _lastInputGradient = Tensor.Zeros({inputSize})

        ' LTC / CfC 需要额外的门控参数来产生输入依赖的液态时间常数
        Call EnsureGateParameters(seed)
    End Sub

    ''' <summary>
    ''' 切换动力学模式；切换到 LTC / CfC 时会惰性初始化门控参数
    ''' </summary>
    Public Sub SetMode(mode As LiquidMode, Optional seed As Integer? = Nothing)
        Me.Mode = mode
        Call EnsureGateParameters(seed)
    End Sub

    Private Sub EnsureGateParameters(Optional seed As Integer? = Nothing)
        If Mode = LiquidMode.CT_RNN Then
            Return
        End If
        If _WeightGate IsNot Nothing Then
            Return
        End If

        Dim seedGate As Integer? = Nothing
        Dim seedGateIn As Integer? = Nothing

        If seed.HasValue Then
            seedGate = seed.Value + 3
            seedGateIn = seed.Value + 4
        End If

        _WeightGate = Tensor.XavierInit(HiddenSize, HiddenSize, seedGate)
        _WeightGateInput = Tensor.XavierInit(InputSize, HiddenSize, seedGateIn)
        _BiasGate = Tensor.Zeros({HiddenSize})

        _WeightGateGradient = Tensor.Zeros({HiddenSize, HiddenSize})
        _WeightGateInputGradient = Tensor.Zeros({InputSize, HiddenSize})
        _BiasGateGradient = Tensor.Zeros({HiddenSize})
    End Sub

#End Region

#Region "核心方法"

    ''' <summary>
    ''' 获取有效的时间常数 τ（应用边界约束），长度 HiddenSize 的一维张量
    ''' </summary>
    Private Function EffectiveTau() As Tensor
        Dim min = TauMin
        Dim max = TauMax

        If UseBoundedTau Then
            ' 使用sigmoid将tau限制在[TauMin, TauMax]范围内
            Return _Tau.Apply(Function(v As Double) As Double
                                  Dim normalized = 1.0 / (1.0 + std.Exp(-v))  ' sigmoid
                                  Return min + normalized * (max - min)
                              End Function)
        Else
            ' 使用softplus确保正值（数值稳定形式，避免 Exp 溢出）
            Return _Tau.Apply(Function(v As Double) As Double
                                  If v > 20.0 Then
                                      Return v
                                  End If
                                  Return std.Log(1.0 + std.Exp(v))
                              End Function)
        End If
    End Function

    ''' <summary>
    ''' 有效时间常数对原始参数 τ 的导数 dτ_eff/dτ_param
    ''' </summary>
    Private Function EffectiveTauDerivative() As Tensor
        Dim min = TauMin
        Dim max = TauMax

        If UseBoundedTau Then
            ' d/dv [min + σ(v)·(max-min)] = σ(v)·(1-σ(v))·(max-min)
            Return _Tau.Apply(Function(v As Double) As Double
                                  Dim s = 1.0 / (1.0 + std.Exp(-v))
                                  Return s * (1.0 - s) * (max - min)
                              End Function)
        Else
            ' d/dv softplus(v) = σ(v)
            Return _Tau.Apply(Function(v As Double) As Double
                                  Return 1.0 / (1.0 + std.Exp(-v))
                              End Function)
        End If
    End Function

    ''' <summary>
    ''' 把一维张量包装成 (1 × n) 的行向量，便于做矩阵乘法
    ''' </summary>
    Private Shared Function ToRow(t As Tensor, n As Integer) As Tensor
        If t.Rank = 1 Then
            Return New Tensor(t.Data, 1, n)
        End If

        Return t
    End Function

    ''' <summary>
    ''' 计算线性组合 z = S·W + Uin·U + b，返回形状 (1 × HiddenSize)
    ''' </summary>
    Private Function LinearForward(sRow As Tensor, uRow As Tensor,
                                   recurrent As Tensor, inputWeight As Tensor, bias As Tensor) As Tensor
        Dim rec = sRow.MatMul(recurrent)
        Dim inp = uRow.MatMul(inputWeight)
        Dim z = New Tensor(1, HiddenSize)

        For i = 0 To HiddenSize - 1
            z(0, i) = rec(0, i) + inp(0, i) + bias(i)
        Next

        Return z
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
        Dim tauEff = EffectiveTau()
        Dim sRow = ToRow(state, HiddenSize)
        Dim uRow = ToRow(input, InputSize)

        ' 目标状态 A = σ(W·x + U·u + b)
        Dim z = LinearForward(sRow, uRow, _WeightRecurrent, _WeightInput, _Bias)
        Dim target = ApplyActivation(z)
        Dim derivative = New Tensor(HiddenSize)

        If Mode = LiquidMode.CT_RNN Then
            ' 旧行为：dx/dt = -x/τ + σ(z)
            For i = 0 To HiddenSize - 1
                derivative(i) = -state(i) / tauEff(i) + target(0, i)
            Next
        Else
            ' LTC：dx/dt = (1/τ + f) ⊙ (A - x)
            Dim zf = LinearForward(sRow, uRow, _WeightGate, _WeightGateInput, _BiasGate)
            Dim f = ActivationFunctions.Sigmoid(zf)

            For i = 0 To HiddenSize - 1
                Dim decay As Double = 1.0 / tauEff(i) + f(0, i)
                derivative(i) = decay * (target(0, i) - state(i))
            Next
        End If

        Return derivative
    End Function

    ''' <summary>
    ''' 读出给定 (状态, 输入) 下的系统时间常数 τ^sys = 1 / (1/τ + f(h,u))
    ''' </summary>
    ''' <remarks>
    ''' 这是 LNN 最重要的可解释性输出：τ^sys 小意味着该单元快速响应（对应快反应），
    ''' τ^sys 大意味着状态缓慢演化（对应慢的代谢重编程）。
    ''' CT_RNN 模式下门控 f ≡ 0，τ^sys 退化为常量 τ_eff。
    ''' </remarks>
    ''' <param name="state">当前状态</param>
    ''' <param name="input">当前外部输入</param>
    ''' <returns>长度为 HiddenSize 的系统时间常数向量</returns>
    Public Function GetSystemTau(state As Tensor, input As Tensor) As Tensor
        Dim tauEff = EffectiveTau()
        Dim result = New Tensor(HiddenSize)

        If Mode = LiquidMode.CT_RNN Then
            For i = 0 To HiddenSize - 1
                result(i) = tauEff(i)
            Next
        Else
            Dim sRow = ToRow(state, HiddenSize)
            Dim uRow = ToRow(input, InputSize)
            Dim zf = LinearForward(sRow, uRow, _WeightGate, _WeightGateInput, _BiasGate)
            Dim f = ActivationFunctions.Sigmoid(zf)

            For i = 0 To HiddenSize - 1
                Dim decay As Double = 1.0 / tauEff(i) + f(0, i)
                result(i) = 1.0 / decay
            Next
        End If

        Return result
    End Function

    ''' <summary>
    ''' 前向传播：使用指定ODE求解器（或 CfC 闭式解）更新状态
    ''' </summary>
    ''' <param name="input">当前时刻输入</param>
    ''' <param name="dt">时间步长</param>
    ''' <param name="solverType">ODE求解器类型: "euler", "heun", "rk4"（CfC 模式下被忽略）</param>
    ''' <returns>更新后的状态</returns>
    Public Function Forward(input As Tensor, dt As Double, Optional solverType As String = "rk4") As Tensor
        If Mode = LiquidMode.CFC Then
            Return ForwardCfC(input, dt)
        End If

        Dim solver = If(String.IsNullOrEmpty(solverType), "rk4", solverType.ToLower())
        Dim stages As ODEStages = Nothing

        If Training Then
            stages = New ODEStages()
        End If

        Select Case solver
            Case "euler"
                _State = ODESolver.EulerStep(AddressOf ComputeDerivative, _State, input, 0.0, dt, stages)
            Case "heun"
                _State = ODESolver.HeunStep(AddressOf ComputeDerivative, _State, input, 0.0, dt, stages)
            Case Else
                _State = ODESolver.RK4Step(AddressOf ComputeDerivative, _State, input, 0.0, dt, stages)
        End Select

        If Training Then
            _records.Add(New StepRecord With {
                .x0 = stages.s1,
                .u = CType(input.Clone(), Tensor),
                .dt = dt,
                .solver = solver,
                .s1 = stages.s1,
                .s2 = stages.s2,
                .s3 = stages.s3,
                .s4 = stages.s4,
                .p = stages.p
            })
        End If

        Return _State
    End Function

    ''' <summary>
    ''' CfC 闭式解前向：h(t+dt) = A + (h - A) ⊙ exp(-(1/τ + f)·dt)
    ''' </summary>
    Private Function ForwardCfC(input As Tensor, dt As Double) As Tensor
        Dim s = _State
        Dim tauEff = EffectiveTau()
        Dim sRow = ToRow(s, HiddenSize)
        Dim uRow = ToRow(input, InputSize)

        Dim z = LinearForward(sRow, uRow, _WeightRecurrent, _WeightInput, _Bias)
        Dim target = ApplyActivation(z)
        Dim zf = LinearForward(sRow, uRow, _WeightGate, _WeightGateInput, _BiasGate)
        Dim f = ActivationFunctions.Sigmoid(zf)
        Dim nxt = New Tensor(HiddenSize)

        For i = 0 To HiddenSize - 1
            Dim decay As Double = 1.0 / tauEff(i) + f(0, i)
            Dim e As Double = std.Exp(-decay * dt)

            nxt(i) = target(0, i) + (s(i) - target(0, i)) * e
        Next

        If Training Then
            _records.Add(New StepRecord With {
                .x0 = s,
                .u = CType(input.Clone(), Tensor),
                .dt = dt,
                .solver = "cfc"
            })
        End If

        _State = nxt

        Return _State
    End Function

    ''' <summary>
    ''' 重置神经元状态为零，并丢弃尚未消费的前向记录
    ''' </summary>
    Public Sub ResetState()
        _State = Tensor.Zeros({HiddenSize})
        _records.Clear()
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
    ''' 丢弃全部前向记录（在序列训练开始前调用）
    ''' </summary>
    Public Sub ClearRecords()
        _records.Clear()
    End Sub

#End Region

#Region "反向传播（BPTT）"

    ''' <summary>
    ''' 单步反向传播：消费最近一次 <see cref="Forward"/> 登记的记录，
    ''' 累加本 cell 全部可训练参数的梯度，并返回对步首状态的伴随向量 dL/dh(t)。
    ''' </summary>
    ''' <param name="adjOut">来自下游（输出层或下一时刻）的对步末状态 h(t+dt) 的梯度</param>
    ''' <returns>对步首状态 h(t) 的梯度，调用方应把它累加到上一时刻的输出梯度上以完成 BPTT</returns>
    Public Function Backward(adjOut As Tensor) As Tensor
        If _records.Count = 0 Then
            Throw New InvalidOperationException(
                "没有可用的前向记录：请先在 Training=True 的状态下完成前向传播，再按逆序调用 Backward。")
        End If

        Dim rec = _records(_records.Count - 1)
        _records.RemoveAt(_records.Count - 1)

        _lastInputGradient = Tensor.Zeros({InputSize})

        Select Case rec.solver
            Case "cfc"
                Return BackwardCfC(rec, adjOut)
            Case "euler"
                Return BackwardEuler(rec, adjOut)
            Case "heun"
                Return BackwardHeun(rec, adjOut)
            Case Else
                Return BackwardRK4(rec, adjOut)
        End Select
    End Function

    Private Function BackwardRK4(rec As StepRecord, adjOut As Tensor) As Tensor
        Dim dt = rec.dt
        Dim adjK1 = LNNMath.Scale(adjOut, dt / 6.0)
        Dim adjK2 = LNNMath.Scale(adjOut, dt / 3.0)
        Dim adjK3 = LNNMath.Scale(adjOut, dt / 3.0)
        Dim adjK4 = LNNMath.Scale(adjOut, dt / 6.0)
        Dim adjX0 = CType(adjOut.Clone(), Tensor)

        ' i = 4 : s4 = x0 + dt·k3
        Dim a4 = BackwardThroughF(rec.s4, rec.u, adjK4)
        LNNMath.AddInPlace(adjX0, a4)
        LNNMath.AddInPlace(adjK3, a4, dt)

        ' i = 3 : s3 = x0 + (dt/2)·k2
        Dim a3 = BackwardThroughF(rec.s3, rec.u, adjK3)
        LNNMath.AddInPlace(adjX0, a3)
        LNNMath.AddInPlace(adjK2, a3, dt / 2.0)

        ' i = 2 : s2 = x0 + (dt/2)·k1
        Dim a2 = BackwardThroughF(rec.s2, rec.u, adjK2)
        LNNMath.AddInPlace(adjX0, a2)
        LNNMath.AddInPlace(adjK1, a2, dt / 2.0)

        ' i = 1 : s1 = x0
        Dim a1 = BackwardThroughF(rec.s1, rec.u, adjK1)
        LNNMath.AddInPlace(adjX0, a1)

        Return adjX0
    End Function

    Private Function BackwardHeun(rec As StepRecord, adjOut As Tensor) As Tensor
        Dim dt = rec.dt
        Dim adjK1 = LNNMath.Scale(adjOut, dt / 2.0)
        Dim adjK2 = LNNMath.Scale(adjOut, dt / 2.0)
        Dim adjX0 = CType(adjOut.Clone(), Tensor)

        ' 先回传 k2（在预测点 p = x0 + dt·k1 处求值）
        Dim a2 = BackwardThroughF(rec.p, rec.u, adjK2)
        LNNMath.AddInPlace(adjX0, a2)
        LNNMath.AddInPlace(adjK1, a2, dt)

        ' 再回传 k1（在步首状态处求值）
        Dim a1 = BackwardThroughF(rec.s1, rec.u, adjK1)
        LNNMath.AddInPlace(adjX0, a1)

        Return adjX0
    End Function

    Private Function BackwardEuler(rec As StepRecord, adjOut As Tensor) As Tensor
        Dim adjK1 = LNNMath.Scale(adjOut, rec.dt)
        Dim a1 = BackwardThroughF(rec.s1, rec.u, adjK1)
        Dim adjX0 = CType(adjOut.Clone(), Tensor)

        LNNMath.AddInPlace(adjX0, a1)

        Return adjX0
    End Function

    Private Function BackwardCfC(rec As StepRecord, adjOut As Tensor) As Tensor
        Dim H = HiddenSize
        Dim s = rec.x0
        Dim u = rec.u
        Dim dt = rec.dt
        Dim tauEff = EffectiveTau()
        Dim tauDeriv = EffectiveTauDerivative()
        Dim sRow = ToRow(s, H)
        Dim uRow = ToRow(u, InputSize)

        Dim z = LinearForward(sRow, uRow, _WeightRecurrent, _WeightInput, _Bias)
        Dim target = ApplyActivation(z)
        Dim dact = ApplyActivationDerivative(target)
        Dim zf = LinearForward(sRow, uRow, _WeightGate, _WeightGateInput, _BiasGate)
        Dim f = ActivationFunctions.Sigmoid(zf)

        Dim dz = New Double(H - 1) {}
        Dim dzf = New Double(H - 1) {}
        Dim adjS = New Tensor(H)

        For i = 0 To H - 1
            Dim fi = f(0, i)
            Dim decay As Double = 1.0 / tauEff(i) + fi
            Dim e As Double = std.Exp(-decay * dt)

            ' h1 = A + (h0 - A)·e
            Dim adjA As Double = adjOut(i) * (1.0 - e)
            Dim adjDecay As Double = adjOut(i) * (s(i) - target(0, i)) * (-dt * e)

            adjS(i) = adjOut(i) * e
            dz(i) = adjA * dact(0, i)
            dzf(i) = adjDecay * fi * (1.0 - fi)

            ' decay = 1/τ + f  ⇒  ∂decay/∂τ_eff = -1/τ²
            _TauGradient(i) += adjDecay * (-1.0 / (tauEff(i) * tauEff(i))) * tauDeriv(i)
        Next

        AccumulateParamGradients(s, u, dz, dzf)
        PropagateToStateAndInput(adjS, u, dz, dzf)

        Return adjS
    End Function

    ''' <summary>
    ''' 在求值点 s 处重新计算前向中间量，累加参数梯度，并返回对该点的伴随向量 dL/ds
    ''' </summary>
    Private Function BackwardThroughF(s As Tensor, u As Tensor, adjOut As Tensor) As Tensor
        Dim H = HiddenSize
        Dim tauEff = EffectiveTau()
        Dim tauDeriv = EffectiveTauDerivative()
        Dim sRow = ToRow(s, H)
        Dim uRow = ToRow(u, InputSize)

        Dim z = LinearForward(sRow, uRow, _WeightRecurrent, _WeightInput, _Bias)
        Dim target = ApplyActivation(z)
        Dim dact = ApplyActivationDerivative(target)

        Dim dz = New Double(H - 1) {}
        Dim dzf As Double() = Nothing
        Dim adjS = New Tensor(H)

        If Mode = LiquidMode.CT_RNN Then
            ' k = -s/τ + σ(z)
            For i = 0 To H - 1
                adjS(i) = adjOut(i) * (-1.0 / tauEff(i))
                dz(i) = adjOut(i) * dact(0, i)
                ' ∂(-s/τ)/∂τ_eff = s/τ²
                _TauGradient(i) += adjOut(i) * s(i) / (tauEff(i) * tauEff(i)) * tauDeriv(i)
            Next
        Else
            ' k = decay ⊙ (A - s)，decay = 1/τ + f
            Dim zf = LinearForward(sRow, uRow, _WeightGate, _WeightGateInput, _BiasGate)
            Dim f = ActivationFunctions.Sigmoid(zf)

            dzf = New Double(H - 1) {}

            For i = 0 To H - 1
                Dim fi = f(0, i)
                Dim decay As Double = 1.0 / tauEff(i) + fi

                adjS(i) = adjOut(i) * (-decay)
                dz(i) = adjOut(i) * decay * dact(0, i)
                dzf(i) = adjOut(i) * (target(0, i) - s(i)) * fi * (1.0 - fi)

                ' ∂k/∂τ_eff = (A - s)·∂decay/∂τ_eff = (A - s)·(-1/τ²)
                _TauGradient(i) += adjOut(i) * (target(0, i) - s(i)) * (-1.0 / (tauEff(i) * tauEff(i))) * tauDeriv(i)
            Next
        End If

        AccumulateParamGradients(s, u, dz, dzf)
        PropagateToStateAndInput(adjS, u, dz, dzf)

        Return adjS
    End Function

    ''' <summary>
    ''' 把本步的 dz / dzf 累加到 W、U、b（以及门控的 Wf、Uf、bf）梯度上
    ''' </summary>
    Private Sub AccumulateParamGradients(s As Tensor, u As Tensor, dz As Double(), dzf As Double())
        Dim H = HiddenSize
        Dim N = InputSize

        For i = 0 To H - 1
            _BiasGradient(i) += dz(i)

            If dzf IsNot Nothing Then
                _BiasGateGradient(i) += dzf(i)
            End If

            For j = 0 To H - 1
                _WeightRecurrentGradient(j, i) += s(j) * dz(i)

                If dzf IsNot Nothing Then
                    _WeightGateGradient(j, i) += s(j) * dzf(i)
                End If
            Next

            For j = 0 To N - 1
                _WeightInputGradient(j, i) += u(j) * dz(i)

                If dzf IsNot Nothing Then
                    _WeightGateInputGradient(j, i) += u(j) * dzf(i)
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' 把梯度继续回传到状态 s 与外部输入 u
    ''' </summary>
    Private Sub PropagateToStateAndInput(adjS As Tensor, u As Tensor, dz As Double(), dzf As Double())
        Dim H = HiddenSize
        Dim N = InputSize

        For i = 0 To H - 1
            Dim acc As Double = 0.0

            For j = 0 To H - 1
                acc += _WeightRecurrent(j, i) * dz(j)

                If dzf IsNot Nothing Then
                    acc += _WeightGate(j, i) * dzf(j)
                End If
            Next

            adjS(i) += acc
        Next

        For j = 0 To N - 1
            Dim acc As Double = 0.0

            For i = 0 To H - 1
                acc += _WeightInput(j, i) * dz(i)

                If dzf IsNot Nothing Then
                    acc += _WeightGateInput(j, i) * dzf(i)
                End If
            Next

            _lastInputGradient(j) += acc
        Next
    End Sub

#End Region

#Region "参数与梯度管理"

    ''' <summary>
    ''' 获取所有可训练参数
    ''' </summary>
    Public Function GetParameters() As Dictionary(Of String, Tensor)
        Dim params As New Dictionary(Of String, Tensor)()

        For Each pair In GetParameterPairs()
            params.Add(pair.Name, pair.Value)
        Next

        Return params
    End Function

    ''' <summary>
    ''' 获取所有梯度
    ''' </summary>
    Public Function GetGradients() As Dictionary(Of String, Tensor)
        Dim grads As New Dictionary(Of String, Tensor)()

        For Each pair In GetParameterPairs()
            grads.Add(pair.Name, pair.Gradient)
        Next

        Return grads
    End Function

    ''' <summary>
    ''' 获取 (参数名, 参数, 梯度) 配对列表——优化器与梯度裁剪的唯一数据源
    ''' </summary>
    Public Function GetParameterPairs() As List(Of ParameterPair)
        Dim pairs As New List(Of ParameterPair) From {
            New ParameterPair("tau", _Tau, _TauGradient),
            New ParameterPair("weight_input", _WeightInput, _WeightInputGradient),
            New ParameterPair("weight_recurrent", _WeightRecurrent, _WeightRecurrentGradient),
            New ParameterPair("bias", _Bias, _BiasGradient)
        }

        If HasGate Then
            pairs.Add(New ParameterPair("weight_gate", _WeightGate, _WeightGateGradient))
            pairs.Add(New ParameterPair("weight_gate_input", _WeightGateInput, _WeightGateInputGradient))
            pairs.Add(New ParameterPair("bias_gate", _BiasGate, _BiasGateGradient))
        End If

        Return pairs
    End Function

    ''' <summary>
    ''' 清零本 cell 的全部梯度累加器
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
            _State?.Dispose()
            _Tau?.Dispose()
            _WeightInput?.Dispose()
            _WeightRecurrent?.Dispose()
            _Bias?.Dispose()
            _TauGradient?.Dispose()
            _WeightInputGradient?.Dispose()
            _WeightRecurrentGradient?.Dispose()
            _BiasGradient?.Dispose()
            _WeightGate?.Dispose()
            _WeightGateInput?.Dispose()
            _BiasGate?.Dispose()
            _WeightGateGradient?.Dispose()
            _WeightGateInputGradient?.Dispose()
            _BiasGateGradient?.Dispose()
            _lastInputGradient?.Dispose()
            _disposed = True
        End If
    End Sub

#End Region

End Class
