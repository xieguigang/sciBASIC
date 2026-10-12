# Data_science/MachineLearning/SNN/SNN.vbproj

- RootNamespace : Microsoft.VisualBasic.DeepLearning.SpikingNeuralNetwork
- AssemblyName  : Microsoft.VisualBasic.DeepLearning.SpikingNeuralNetwork
- TargetFramework: net10.0
- Source files  : 13
- Existing Title: Spiking Neural Network  ·  LIF + SurrogateGrad + STDP
- Existing Desc : Spiking neural network toolkit for sciBASIC#: leaky integrate and fire (LIF) and recurrent/sparse LIF layers, population and rate encoders and decoders, surrogate gradient training and STDP plasticity on the tensor runtime.
- Existing Tags : scibasic;deep-learning;spiking-neural-network;lif;surrogate-gradient;stdp;recurrent;sparse;tensor

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.DeepLearning.SpikingNeuralNetwork)

## Public types
- Module SpikeDecoders (Decoder.vb)
- Enum SpikeEncoding (Encoder.vb)
- Module SpikeEncoders (Encoder.vb)
- Enum LIFResetMode (LIFLayer.vb)
- Class LIFLayer (LIFLayer.vb)
- Class LinearReadout (LinearReadout.vb) - 线性回归解码头：把脉冲网络状态（膜电位 / 发放率）线性映射为连续预测值。
- Class NamespaceDoc (NamespaceDoc.vb) - Spiking neural networks: LIF layers, encoders and decoders, STDP learning, surrogate gradients and the spiking network model.
- Structure LossGradient (Network.vb)
- Module Losses (Network.vb)
- Class AdamOptimizer (Network.vb) - Adam 优化器（含梯度 L2 范数裁剪）。 key 用于区分不同参数的动量状态（例如各层权重）。
- Class SpikingNetwork (Network.vb) - 脉冲神经网络：编码器 + 多个 LIF 层 + 时间展开 + 计数解码。 支持两种连接拓扑（互斥）： 1. 全连接多层（<see cref="AddLayer"/>）：前馈 LIF 层栈，支持 BPTT 训练。
- Structure RecurrentGradients (RecurrentLIFLayer.vb) - 递归 LIF 层 BPTT 的梯度结果。
- Class RecurrentLIFLayer (RecurrentLIFLayer.vb) - 可训练的递归 LIF 层：同一神经元群内部的稠密稀疏化突触连接， 支持结构掩码冻结、硬/软复位与代理梯度 BPTT。 <para>
- Module RegressionLosses (RegressionLosses.vb)
- Enum LifResidentPrecision (SparseLIFLayer.vb) - 融合 LIF 状态在设备上的精度档位。
- Enum LifStepPath (SparseLIFLayer.vb) - 某个时间步实际走的那条前向路径（诊断 / 报告用）。
- Class SparseLIFLayer (SparseLIFLayer.vb)
- Enum SparseNormalization (SparseMatrix.vb)
- Class SparseMatrix (SparseMatrix.vb) - 稀疏突触连接矩阵（CSR 格式）。行 = 突触前神经元，列 = 突触后神经元。
- Class STDPLayer (STDP.vb)
- Enum SurrogateKind (Surrogate.vb)
- Module Surrogate (Surrogate.vb)
- Module TensorHelper (TensorHelper.vb)

## Notable public members
- Public Function SpikeCounts(sHistory As List(Of Tensor)) As Tensor
- Public Function SpikeCounts(layer As SparseLIFLayer) As Tensor
- Public Function FiringRate(sHistory As List(Of Tensor)) As Tensor
- Public Function TotalSpikeCount(sHistory As List(Of Tensor)) As Double
- Public Function ActiveNeurons(sHistory As List(Of Tensor)) As Integer()
- Public Function MembranePotential(uLast As Tensor) As Tensor
- Public Function ConcatRateAndPotential(rate As Tensor, membranePotential As Tensor) As Tensor
- Public Function RasterText(sHistory As List(Of Tensor),
- Public Function RateEncode(x As Tensor, T As Integer,
- Public Function LatencyEncode(x As Tensor, T As Integer,
- Public Function DirectCurrentEncode(x As Tensor, T As Integer,
- Public ReadOnly Property Name As String
- Public ReadOnly Property InputSize As Integer
- Public ReadOnly Property Units As Integer
- Public Property Weight As Tensor
- Public Property WeightGrad As Tensor
- Public Property Beta As Double
- Public Property Threshold As Double
- Public Property ResetMode As LIFResetMode
- Public Property SurrogateType As SurrogateKind
- Public Property Alpha As Double
- Public Property SmoothForward As Boolean = False
- Public ReadOnly Property UHistory As List(Of Tensor)
- Public ReadOnly Property SHistory As List(Of Tensor)
- Public Sub New(name As String, inputSize As Integer, units As Integer,
- Public Sub ResetState(batchSize As Integer)
- Public Function ForwardStep(x As Tensor) As Tensor
- Public Function BackwardTime(dS_ext As List(Of Tensor)) As List(Of Tensor)
- Public ReadOnly Property Name As String
- Public ReadOnly Property InputSize As Integer
- Public ReadOnly Property Units As Integer
- Public Property Weight As Tensor
- Public Property Bias As Tensor
- Public Property WeightGrad As Tensor
- Public Property BiasGrad As Tensor
- Public Property Trainable As Boolean = True
- Public Sub New(name As String, inputSize As Integer, units As Integer,
- Public Shared Function IdentityReadout(name As String, units As Integer) As LinearReadout
- Public Sub ZeroGrad()
- Public Function Forward(u As Tensor) As Tensor
- Public Function BackwardTime(dY As Tensor) As Tensor
- Public Overrides Function ToString() As String
- Public Function SoftmaxCrossEntropy(logits As Tensor, labels As Integer()) As LossGradient
- Public Property LearningRate As Double
- Public Property Beta1 As Double = 0.9
- Public Property Beta2 As Double = 0.999
- Public Property Epsilon As Double = 0.00000001
- Public Property ClipNorm As Double
- Public Sub New(Optional learningRate As Double = 0.002, Optional clipNorm As Double = 1.0)
- Public Sub Update(param As Tensor, grad As Tensor, key As String)
- Public ReadOnly Property Layers As New List(Of LIFLayer)()
- Public Property SparseLayer As SparseLIFLayer
- Public Property SparseInputMap As Integer()
- Public Property TimeSteps As Integer
- Public Property Encoding As SpikeEncoding
- Public Property Rng As New Random(42)
- Public Sub New(inputSize As Integer, timeSteps As Integer,
- Public Function AddLayer(units As Integer,
- Public Function AddSparseLayer(synapses As SparseMatrix,
- Public Function AddSparseLayer(pre As Integer(), post As Integer(), weight As Double(),
- ... and 96 more

## Imports
- Microsoft.VisualBasic.Data
- Microsoft.VisualBasic.MachineLearning.TensorFlow
- Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
- std = System.Math
- System.Text

## File tree
- Decoder.vb
- Encoder.vb
- LIFLayer.vb
- LinearReadout.vb
- NamespaceDoc.vb
- Network.vb
- RecurrentLIFLayer.vb
- RegressionLosses.vb
- SparseLIFLayer.vb
- SparseMatrix.vb
- STDP.vb
- Surrogate.vb
- TensorHelper.vb

