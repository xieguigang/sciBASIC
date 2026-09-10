# Data_science/MachineLearning/LNN/LNN.vbproj

- RootNamespace : Microsoft.VisualBasic.DeepLearning.LiquidNeuralNetwork
- AssemblyName  : Microsoft.VisualBasic.DeepLearning.LNN
- TargetFramework: net10.0
- Source files  : 8
- Existing Title: Liquid Neural Networks with ODE Solvers for Time Series
- Existing Desc : Continuous-time liquid neural networks for sciBASIC#, where neuron dynamics follow learnable ODEs solved by Euler, Heun, RK4 or adaptive RK45 steps; includes SGD/Adam training and time-series preprocessing utilities.
- Existing Tags : scibasic;liquid-neural-network;ode;time-series;continuous-time

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.DeepLearning.LiquidNeuralNetwork)

## Public types
- Module ActivationFunctions (ActivationFunctions.vb) - 神经网络激活函数集合
- Enum LiquidMode (LiquidCell.vb) - 液态神经元的动力学模式
- Class StepRecord (LiquidCell.vb) - 单个时间步的前向记录，供反向模式自动微分回放 这里只登记 ODE 右端函数被求值过的状态点；各点的激活值、门控值与有效时间常数 在反向传播时重新计算（recompute），从而把每步的显存占用压到常数个向量。
- Class ParameterPair (LiquidCell.vb) - 参数张量与其梯度张量的配对 优化器（Adam/SGD）与梯度裁剪直接遍历这个配对列表， 避免旧实现里 "tau" / "weight_input" 这类裸键在多层堆叠时互相覆盖的问题。
- Class LiquidCell (LiquidCell.vb) - dh/dt = (A - h) / τ^sys = (1/τ + f) ⊙ (A - h) 其中: - h: 神经元状态（代谢建模中对应代谢物浓度）
- Class LiquidLayer (LiquidLayer.vb) - 液态神经网络层 包含多个LiquidCell，支持多层堆叠
- Class LiquidNeuralNetwork (LiquidNeuralNetwork.vb) - 液态神经网络模块 (Liquid Neural Networks, LNN) 基于Tensor对象实现的连续时间递归神经网络 适用于时间序列分析、预测和控制任务
- Class LNNTrainer (LNNTrainer.vb) - 液态神经网络训练器 实现基于时间的反向传播(BPTT)训练算法 与旧实现的关键区别：
- Module LNNMath (ODESolver.vb) - LNN 内部使用的张量数值工具（全程 Double 精度）
- Module ODESolver (ODESolver.vb) - 常微分方程求解器 用于数值积分液态神经网络的微分方程
- Class ODEStages (ODESolver.vb) - 一次数值积分内各阶斜率 f(·) 的求值点记录 反向模式自动微分（BPTT）需要按前向的图结构回放，因此这里只登记 被求值过的状态点；具体的中间激活值在反向时重新计算，避免占用额外内存。
- Module TimeSeriesUtils (TimeSeriesUtils.vb) - 时间序列处理工具

## Notable public members
- Public Function Sigmoid(x As Tensor) As Tensor
- Public Function SigmoidDerivative(sigmoidOutput As Tensor) As Tensor
- Public Function Tanh(x As Tensor) As Tensor
- Public Function TanhDerivative(tanhOutput As Tensor) As Tensor
- Public Function ReLU(x As Tensor) As Tensor
- Public Function ReLUDerivative(x As Tensor) As Tensor
- Public Function LeakyReLU(x As Tensor, Optional alpha As Double = 0.01) As Tensor
- Public Function Softmax(x As Tensor) As Tensor
- Public Property x0 As Tensor
- Public Property u As Tensor
- Public Property dt As Double
- Public Property s1 As Tensor
- Public Property s2 As Tensor
- Public Property s3 As Tensor
- Public Property s4 As Tensor
- Public Property p As Tensor
- Public Property solver As String
- Public ReadOnly Property Name As String
- Public ReadOnly Property Value As Tensor
- Public ReadOnly Property Gradient As Tensor
- Public Sub New(name As String, value As Tensor, gradient As Tensor)
- Public Overrides Function ToString() As String
- Public ReadOnly Property HiddenSize As Integer
- Public ReadOnly Property InputSize As Integer
- Public Property State As Tensor
- Public Property Tau As Tensor
- Public Property WeightInput As Tensor
- Public Property WeightRecurrent As Tensor
- Public Property Bias As Tensor
- Public Property WeightGate As Tensor
- Public Property WeightGateInput As Tensor
- Public Property BiasGate As Tensor
- Public Property ActivationType As String = "tanh"
- Public Property Mode As LiquidMode
- Public Property Training As Boolean = False
- Public ReadOnly Property HasGate As Boolean
- Public ReadOnly Property LastInputGradient As Tensor
- Public ReadOnly Property RecordCount As Integer
- Public Property UseBoundedTau As Boolean = True
- Public Property TauMin As Double = 0.1
- Public Property TauMax As Double = 10.0
- Public Property TauGradient As Tensor
- Public Property WeightInputGradient As Tensor
- Public Property WeightRecurrentGradient As Tensor
- Public Property BiasGradient As Tensor
- Public Property WeightGateGradient As Tensor
- Public Property WeightGateInputGradient As Tensor
- Public Property BiasGateGradient As Tensor
- Public Sub New(hiddenSize As Integer, inputSize As Integer,
- Public Sub SetMode(mode As LiquidMode, Optional seed As Integer? = Nothing)
- Public Function ComputeDerivative(state As Tensor, input As Tensor, time As Double) As Tensor
- Public Function GetSystemTau(state As Tensor, input As Tensor) As Tensor
- Public Function Forward(input As Tensor, dt As Double, Optional solverType As String = "rk4") As Tensor
- Public Sub ResetState()
- Public Sub SetState(initialState As Tensor)
- Public Sub ClearRecords()
- Public Function Backward(adjOut As Tensor) As Tensor
- Public Function GetParameters() As Dictionary(Of String, Tensor)
- Public Function GetGradients() As Dictionary(Of String, Tensor)
- Public Function GetParameterPairs() As List(Of ParameterPair)
- ... and 89 more

## Imports
- Microsoft.VisualBasic.MachineLearning.TensorFlow
- std = System.Math

## File tree
- ActivationFunctions.vb
- Documentation.vb
- LiquidCell.vb
- LiquidLayer.vb
- LiquidNeuralNetwork.vb
- LNNTrainer.vb
- ODESolver.vb
- TimeSeriesUtils.vb

