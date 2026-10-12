# Data_science/MachineLearning/TensorFlow/TensorFlow.vbproj

- RootNamespace : Microsoft.VisualBasic.MachineLearning.TensorFlow
- AssemblyName  : Microsoft.VisualBasic.MachineLearning.TensorFlow
- TargetFramework: net10.0
- Source files  : 15
- Existing Title: Pure VB.NET Tensor Library with NumPy API and Pluggable Compute Backend
- Existing Desc : Self-contained tensor engine for sciBASIC#, not a Google TensorFlow binding: a managed multi-dimensional Tensor, a NumPy-style API, neural network activations and losses, and a pluggable SIMD/CUDA compute backend.
- Existing Tags : scibasic;tensor;numpy;numerical;computation-engine;activation-functions;simd

## Namespaces
- Compute  [files: 9]
- NumPy  [files: 2]

## Public types
- Module CellActivation (Compute\CellActivation.vb) - 批量细胞管线融合算子使用的激活函数编码。 融合算子（<see cref="ITensorCompute.LtcRk4Batch"/> / <see cref="ITensorCompute.GraphLayerBatch"/>） 的签名一律使用 <c>Integer</c> 编码而不是委托或字符串：
- Interface ITensorCompute (Compute\ITensorCompute.vb) - 张量计算后端契约。 形状校验、广播等“语义边界”仍然由 <see cref="Tensor"/> 层负责， 后端只需要实现纯粹的数值计算。后端应当是**无状态**（或线程安全）的，
- Interface ITensorComputeF (Compute\ITensorComputeF.vb) - 单精度张量计算后端契约。 形状校验、广播等"语义边界"仍然由 <see cref="TensorF"/> 层负责， 后端只需要实现纯粹的数值计算。后端应当是**无状态**（或线程安全）的，
- Module NamespaceDoc (Compute\NamespaceDoc.vb) - The tensor computation engine.
- Class SIMDTensor (Compute\SIMDTensor.vb) - 默认的 SIMD 加速 CPU 计算后端。
- Class SIMDTensorF (Compute\SIMDTensorF.vb) - 单精度张量的默认 SIMD 加速 CPU 计算后端。
- Class SparseCsr (Compute\SparseCsr.vb) - CSR 稀疏矩阵载体（行 = 突触前，列 = 突触后）。 承载 <see cref="ITensorCompute.SpMM"/> 算子所需的全部稀疏结构信息。
- Module Math (Math.vb) - TensorFlow 风格的数学运算模块 提供逐元素运算、规约操作和裁剪功能，全部基于 Tensor 实现
- Class NamespaceDoc (NamespaceDoc.vb) - The TensorFlow style tensor computation library: tensors, numerical operations and the neural network helper API.
- Module NamespaceDoc (NamespaceDoc.vb) - NumPy compatible array helpers.
- Module nn (nn.vb) - TensorFlow 风格的神经网络模块 提供激活函数、损失函数和正则化操作，全部基于 Tensor 实现
- Module NumPyModule (NumPy.vb) - NumPy 兼容 API 模块 提供类 NumPy 的静态方法，全部基于 Tensor 实现 典型用法:
- Class RandomState (NumPy.vb) - NumPy 风格的随机数子模块 np.random.rand(...) 和 np.random.randn(...) 对应 NumPy.RandomState.rand(...)
- Class Tensor (Tensor.vb) - 张量类 - GNN中所有数值计算的基础数据结构 支持多维数组的存储和基本数学运算
- Class TensorF (TensorF.vb) - 单精度（Single / float32）多维张量。与双精度 <see cref="Tensor"/> 并列， 通过 <see cref="ToTensor"/> / <see cref="FromTensor"/> 双向互转。 索引器被刻意设计为"纯数组读写"，不包含版本计数或任何原子操作，

## Notable public members
- Public Const Linear As Integer = 0
- Public Const Tanh As Integer = 1
- Public Const Sigmoid As Integer = 2
- Public Const ReLU As Integer = 3
- Public Function Apply(x As Double, code As Integer) As Double
- Public Const MatrixDotThreshold As Integer = 10000
- Public Overrides ReadOnly Property Name As String = "SIMD"
- Public Shared Sub Register()
- Public Overrides Function Add(a As Tensor, b As Tensor) As Tensor
- Public Overrides Function Subtract(a As Tensor, b As Tensor) As Tensor
- Public Overrides Function Multiply(a As Tensor, b As Tensor) As Tensor
- Public Overrides Function Divide(a As Tensor, b As Tensor) As Tensor
- Public Overrides Function Maximum(a As Tensor, b As Tensor) As Tensor
- Public Overrides Function Minimum(a As Tensor, b As Tensor) As Tensor
- Public Overrides Function Exp(t As Tensor) As Tensor
- Public Overrides Function Log(t As Tensor) As Tensor
- Public Overrides Function Sqrt(t As Tensor) As Tensor
- Public Overrides Function Square(t As Tensor) As Tensor
- Public Overrides Function Abs(t As Tensor) As Tensor
- Public Overrides Function Negate(t As Tensor) As Tensor
- Public Overrides Function Reciprocal(t As Tensor) As Tensor
- Public Overrides Function AddScalar(t As Tensor, scalar As Double) As Tensor
- Public Overrides Function MultiplyScalar(t As Tensor, scalar As Double) As Tensor
- Public Overrides Function DivideScalar(t As Tensor, scalar As Double) As Tensor
- Public Overrides Function Pow(t As Tensor, exponent As Double) As Tensor
- Public Overrides Function Clip(t As Tensor, low As Double, high As Double) As Tensor
- Public Overrides Function MatMul(a As Tensor, b As Tensor) As Tensor
- Public Overrides Function LifStep(synapses As SparseCsr,
- Protected Overrides Sub LifUpdateInPlace(synapticInput As Double(),
- Public Const ConvParallelThreshold As Long = 200000
- Public Overrides Function Conv2D(x As Tensor, filters As Tensor, bias As Tensor,
- Public Overrides Function Conv2DBackwardFilter(gradOutput As Tensor, x As Tensor,
- Public Overrides Function Conv2DBackwardInput(gradOutput As Tensor, filters As Tensor,
- Public Overrides Function SumAll(t As Tensor) As Double
- Public Overrides Function MeanAll(t As Tensor) As Double
- Public Overrides Function Sum(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor
- Public Overrides Function Mean(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor
- Public Overrides Function Max(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor
- Public Overrides Function Min(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor
- Public Overrides Function L2Norm(t As Tensor) As Double
- Public Overrides Function StdDev(t As Tensor) As Double
- Public Shared ReadOnly Property VectorWidth As Integer
- Public Overrides ReadOnly Property Name As String = "SIMD-F32"
- Public Shared Sub Register()
- Public Overrides Function Add(a As TensorF, b As TensorF) As TensorF
- Public Overrides Function Subtract(a As TensorF, b As TensorF) As TensorF
- Public Overrides Function Multiply(a As TensorF, b As TensorF) As TensorF
- Public Overrides Function Divide(a As TensorF, b As TensorF) As TensorF
- Public Overrides Function Maximum(a As TensorF, b As TensorF) As TensorF
- Public Overrides Function Minimum(a As TensorF, b As TensorF) As TensorF
- Public Overrides Function Sqrt(t As TensorF) As TensorF
- Public Overrides Function Square(t As TensorF) As TensorF
- Public Overrides Function Abs(t As TensorF) As TensorF
- Public Overrides Function Negate(t As TensorF) As TensorF
- Public Overrides Function AddScalar(t As TensorF, scalar As Single) As TensorF
- Public Overrides Function MultiplyScalar(t As TensorF, scalar As Single) As TensorF
- Public Overrides Function DivideScalar(t As TensorF, scalar As Single) As TensorF
- Public Overrides Function Clip(t As TensorF, low As Single, high As Single) As TensorF
- Public Overrides Function SumAll(t As TensorF) As Double
- Public Overrides Function MinAll(t As TensorF) As Single
- ... and 323 more

## Imports
- Microsoft.VisualBasic.Math.SIMD
- nv = System.Numerics
- std = System.Math
- System.Threading
- tf = Microsoft.VisualBasic.MachineLearning.TensorFlow
- tfCompute = Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
- tpl = System.Threading.Tasks

## File tree
- Compute\CellActivation.vb
- Compute\ITensorCompute.vb
- Compute\ITensorComputeF.vb
- Compute\NamespaceDoc.vb
- Compute\SIMDTensor.vb
- Compute\SIMDTensorF.vb
- Compute\SparseCsr.vb
- Compute\TensorComputeBase.vb
- Compute\TensorComputeFBase.vb
- Math.vb
- NamespaceDoc.vb
- nn.vb
- NumPy.vb
- Tensor.vb
- TensorF.vb

