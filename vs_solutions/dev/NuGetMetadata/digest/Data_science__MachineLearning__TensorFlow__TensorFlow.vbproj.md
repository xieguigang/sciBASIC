# Data_science/MachineLearning/TensorFlow/TensorFlow.vbproj

- RootNamespace : Microsoft.VisualBasic.MachineLearning.TensorFlow
- AssemblyName  : Microsoft.VisualBasic.MachineLearning.TensorFlow
- TargetFramework: net10.0
- Source files  : 10
- Existing Title: Pure VB.NET Tensor Library with NumPy API and Autodiff
- Existing Desc : Self-contained tensor engine for sciBASIC#, not a Google TensorFlow binding: a managed multi-dimensional Tensor, a NumPy-style API, neural network activations and losses, and reverse-mode automatic differentiation.
- Existing Tags : scibasic;tensor;numpy;automatic-differentiation;numerical

## Namespaces
- AutomaticDifferentiation  [files: 4]
- AutomaticDifferentiation.Parallel  [files: 2]
- NumPy  [files: 1]

## Public types
- Class AddRevRev (AutomaticDifferentiation\AutomaticDifferentiation.vb)
- Class AddRevDouble (AutomaticDifferentiation\AutomaticDifferentiation.vb)
- Class AddDoubleRev (AutomaticDifferentiation\AutomaticDifferentiation.vb)
- Class SIMDMatMul (AutomaticDifferentiation\Parallel\MatMul.vb)
- Class MultiplyScale (AutomaticDifferentiation\Parallel\Multiply.vb)
- Class Rev (AutomaticDifferentiation\Rev.vb) - Data type for automatic differentiation with reverse mode accumulation.
- Class Tensor (AutomaticDifferentiation\Tensor.vb)
- Module Math (Math.vb) - TensorFlow 风格的数学运算模块 提供逐元素运算、规约操作和裁剪功能，全部基于 Tensor 实现
- Module nn (nn.vb) - TensorFlow 风格的神经网络模块 提供激活函数、损失函数和正则化操作，全部基于 Tensor 实现
- Module NumPyModule (NumPy.vb) - NumPy 兼容 API 模块 提供类 NumPy 的静态方法，全部基于 Tensor 实现 典型用法:
- Class RandomState (NumPy.vb) - NumPy 风格的随机数子模块 np.random.rand(...) 和 np.random.randn(...) 对应 NumPy.RandomState.rand(...)
- Class Tensor (Tensor.vb) - 张量类 - GNN中所有数值计算的基础数据结构 支持多维数组的存储和基本数学运算

## Notable public members
- Public MustOverride Sub Differentiation(dx As Double)
- Public Delegate Sub Differentiation(dx As Double)
- Public Sub New(lhs As Rev, rhs As Rev)
- Public Overrides Sub Differentiation(dx As Double)
- Public Sub New(lhs As Rev, rhs As Double)
- Public Overrides Sub Differentiation(dx As Double)
- Public Sub New(lhs As Double, rhs As Rev)
- Public Overrides Sub Differentiation(dx As Double)
- Public Shared ReadOnly Property Instance As New Checkpoints
- Public Function AddCheckpoint(data As Tensor) As Tensor
- Public Sub ClearCheckpoints()
- Public Sub CalculateCheckpointGradients()
- Public Sub New(A As Tensor, B As Tensor, C As Tensor, nrblocks As Integer,
- Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
- Public Overloads Shared Function Solve(A As Tensor, B As Tensor) As Tensor
- Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
- Public Shared Function Scale(tensor As Tensor, s As Double) As Tensor
- Public Shared ReadOnly Property Zero As Rev
- Public Sub New(y As Double)
- Public Overrides Function ToString() As String
- Public Function Pow(e As Double) As Rev
- Public Function Exp() As Rev
- Public Function Log() As Rev
- Public ReadOnly Property Dimension As Integer
- Public Overrides Function ToString() As String
- Public Sub New(ParamArray sizes As Integer())
- Public Sub New(T As Tensor, Optional copy As Boolean = False)
- Public Function Transpose() As Tensor
- Public Function Scale(s As Double) As Tensor
- Public Function Add(s As Double) As Tensor
- Public Function Pow(e As Double) As Tensor
- Public Sub GenerateNormalRandomValues()
- Public Function Softmax() As Tensor
- Public Sub Mask()
- Public Function Dropout(dropoutMask As Boolean(), dropoutRate As Double) As Tensor
- Public Sub ReLU()
- Public Function Flatten() As Tensor
- Public Function GetMaxIndex() As Integer()
- Public Function VecAdd(v As Tensor) As Tensor
- Public Sub MatAdd(T As Tensor)
- Public Sub ClearDerivatives()
- Public Function GetDerivatives() As Tensor
- Public Sub TransferDerivatives(T As Tensor)
- Public Shared Function MatAdd(A As Tensor, B As Tensor) As Tensor
- Public Shared Function MatDivElementWise(A As Tensor, B As Tensor) As Tensor
- Public Shared Function MatMulElementWise(A As Tensor, B As Tensor) As Tensor
- Public Shared Function MatMul(A As Tensor, B As Tensor) As Tensor
- Public Shared Function Concat(Tensors As Tensor()) As Tensor
- Public Shared Function AddNorm(A As Tensor, B As Tensor) As Tensor
- Public Function exp(t As Tensor) As Tensor
- Public Function log(t As Tensor) As Tensor
- Public Function sqrt(t As Tensor) As Tensor
- Public Function square(t As Tensor) As Tensor
- Public Function abs(t As Tensor) As Tensor
- Public Function sin(t As Tensor) As Tensor
- Public Function cos(t As Tensor) As Tensor
- Public Function tanh(t As Tensor) As Tensor
- Public Function sigmoid(t As Tensor) As Tensor
- Public Function pow(t As Tensor, exponent As Double) As Tensor
- Public Function negative(t As Tensor) As Tensor
- ... and 144 more

## Imports
- Microsoft.VisualBasic.MachineLearning.TensorFlow.AutomaticDifferentiation.Parallel
- Microsoft.VisualBasic.Parallel
- Microsoft.VisualBasic.Serialization.JSON
- randf = Microsoft.VisualBasic.Math.RandomExtensions
- std = System.Math
- System.Runtime.CompilerServices
- tf = Microsoft.VisualBasic.MachineLearning.TensorFlow

## File tree
- AutomaticDifferentiation\AutomaticDifferentiation.vb
- AutomaticDifferentiation\Checkpoints.vb
- AutomaticDifferentiation\Parallel\MatMul.vb
- AutomaticDifferentiation\Parallel\Multiply.vb
- AutomaticDifferentiation\Rev.vb
- AutomaticDifferentiation\Tensor.vb
- Math.vb
- nn.vb
- NumPy.vb
- Tensor.vb

