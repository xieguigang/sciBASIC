# cuda/ILCudaTensor/ILCudaTensor.vbproj

- RootNamespace : Microsoft.VisualBasic.Computing.ILCuda
- AssemblyName  : Microsoft.VisualBasic.Computing.ILCuda.GPUTensor
- TargetFramework: net10.0
- Source files  : 9
- Existing Title: GPU Tensor Types and CUDA Kernels for the ILCuda Runtime
- Existing Desc : GPU tensor extension for the ILCuda framework of sciBASIC#: device resident dense and CSR sparse tensor storage with device/host caches, plus hand written double precision CUDA kernels for GEMM, softmax, reductions, convolution and pooling.
- Existing Tags : scibasic;cuda;gpu-tensor;sparse-matrix;csr;nvrtc;cuda-kernel;deep-learning

## Namespaces
- GPUTensor  [files: 8]

## Public types
- Class CudaTensor (GPUTensor\CudaTensor.vb) - <see cref="tfCompute.ITensorCompute"/> 的 CUDA GPU 实现（全 double 精度）。
- Module DoubleKernels (GPUTensor\DoubleKernels.vb) - 待反编译为 CUDA 的双精度逐元素函数（本身不会被托管调用）。
- Module DoubleKernelRegistry (GPUTensor\DoubleKernels.vb) - 把 <see cref="DoubleKernels"/> 里的方法逐个翻译成 CUDA 内核并注册到编译单元。 必须在 <c>CudaEngine.TryCreate</c> 之前调用。
- Module NamespaceDoc (GPUTensor\NamespaceDoc.vb) - Device resident tensor types for the ILCuda framework: dense CudaTensor and sparse CSR storage, host/device buffer caches and the tensor kernel entry points.
- Module TensorKernelNames (GPUTensor\TensorKernels.vb)
- Class NamespaceDoc (NamespaceDoc.vb) - ILCudaTensor extends the ILCuda compute framework with device resident tensor types and ready to use CUDA tensor kernels for dense and sparse tensors.

## Notable public members
- Public Const DefaultCacheBytes As Long = 1024L * 1024L * 1024L
- Public Const RowBlockSize As Integer = 256
- Public Const StageBlockSize As Integer = 256
- Public Shared Property MaxStageBlocks As Integer = 1024
- Public Shared Property MinGpuElements As Integer = 4096
- Public Shared Property MinGemmElements As Integer = 65536
- Public Shared Property MinSparseNnz As Integer = 65536
- Public Shared Property LastError As String
- Public Shared ReadOnly Property Current As CudaTensor
- Public Property UseFp32Gemm As Boolean = True
- Public Property UseResidentStore As Boolean = False
- Public Property UseFp64Residency As Boolean = False
- Public Sub New(elements As Integer)
- Public Sub Dispose() Implements IDisposable.Dispose
- Public Sub New(engine As ILCudaRuntime.CudaEngine, Optional cacheBytes As Long = 0)
- Public Const MaxCacheBytes As Long = 4L * 1024L * 1024L * 1024L
- Public Const MinCacheBytes As Long = 256L * 1024L * 1024L
- Public ReadOnly Property Resident As DeviceResidentStore
- Public Overrides ReadOnly Property Name As String = "CUDA"
- Public ReadOnly Property Engine As ILCudaRuntime.CudaEngine
- Public Shared ReadOnly Property KernelFailures As IReadOnlyDictionary(Of String, String)
- Public Shared Function Register(Optional options As ILCudaRuntime.EngineOptions = Nothing,
- Public Shared Sub Unregister()
- Public Overrides Function Add(a As tf.Tensor, b As tf.Tensor) As tf.Tensor
- Public Overrides Function Subtract(a As tf.Tensor, b As tf.Tensor) As tf.Tensor
- Public Overrides Function Multiply(a As tf.Tensor, b As tf.Tensor) As tf.Tensor
- Public Overrides Function Divide(a As tf.Tensor, b As tf.Tensor) As tf.Tensor
- Public Overrides Function Exp(t As tf.Tensor) As tf.Tensor
- Public Overrides Function Log(t As tf.Tensor) As tf.Tensor
- Public Overrides Function Sqrt(t As tf.Tensor) As tf.Tensor
- Public Overrides Function Abs(t As tf.Tensor) As tf.Tensor
- Public Overrides Function Negate(t As tf.Tensor) As tf.Tensor
- Public Overrides Function Reciprocal(t As tf.Tensor) As tf.Tensor
- Public Overrides Function Square(t As tf.Tensor) As tf.Tensor
- Public Overrides Function Tanh(t As tf.Tensor) As tf.Tensor
- Public Overrides Function Sigmoid(t As tf.Tensor) As tf.Tensor
- Public Overrides Function Sin(t As tf.Tensor) As tf.Tensor
- Public Overrides Function Cos(t As tf.Tensor) As tf.Tensor
- Public Overrides Function Relu(t As tf.Tensor) As tf.Tensor
- Public Overrides Function Heaviside(t As tf.Tensor) As tf.Tensor
- Public Overrides Function Gelu(t As tf.Tensor) As tf.Tensor
- Public Overrides Function Swish(t As tf.Tensor) As tf.Tensor
- Public Overrides Function LeakyRelu(t As tf.Tensor, alpha As Double) As tf.Tensor
- Public Overrides Function Elu(t As tf.Tensor, alpha As Double) As tf.Tensor
- Public Overrides Function AddScalar(t As tf.Tensor, scalar As Double) As tf.Tensor
- Public Overrides Function MultiplyScalar(t As tf.Tensor, scalar As Double) As tf.Tensor
- Public Overrides Function DivideScalar(t As tf.Tensor, scalar As Double) As tf.Tensor
- Public Overrides Function Pow(t As tf.Tensor, exponent As Double) As tf.Tensor
- Public Overrides Function Clip(t As tf.Tensor, low As Double, high As Double) As tf.Tensor
- Public Overrides Function MatMul(a As tf.Tensor, b As tf.Tensor) As tf.Tensor
- Public Overrides Function SpMM(csr As tfCompute.SparseCsr, dense As tf.Tensor) As tf.Tensor
- Public Overrides Function LifStep(synapses As tfCompute.SparseCsr,
- Public ReadOnly Property LifWorkspaceCount As Integer
- Public Overrides Function Transpose(t As tf.Tensor) As tf.Tensor
- Public Overrides Function LtcRk4Batch(state As tf.Tensor, input As tf.Tensor,
- Public Overrides Function GraphLayerBatch(x As tf.Tensor, wSelf As tf.Tensor, wRel As tf.Tensor,
- Public Overrides Function GraphFeatureBatch(xNorm As tf.Tensor, flag As tf.Tensor, embed As tf.Tensor,
- Public Overrides Function FluxHeadBatch(h As tf.Tensor, u As tf.Tensor, wFlux As tf.Tensor,
- Public Overrides Function SystemTauBatch(h As tf.Tensor, u As tf.Tensor, weightGate As tf.Tensor,
- Public Overrides Function Conv2D(x As tf.Tensor, filters As tf.Tensor, bias As tf.Tensor,
- ... and 177 more

## Imports
- ILCudaKernels = Microsoft.VisualBasic.Computing.ILCuda.Kernels
- ILCudaRuntime = Microsoft.VisualBasic.Computing.ILCuda.Runtime
- Microsoft.VisualBasic.Computing.ILCuda.IL2Cuda
- Microsoft.VisualBasic.Computing.ILCuda.Runtime
- std = System.Math
- System.Reflection
- System.Runtime.CompilerServices
- System.Runtime.InteropServices
- tf = Microsoft.VisualBasic.MachineLearning.TensorFlow
- tfCompute = Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute

## File tree
- GPUTensor\CudaTensor.vb
- GPUTensor\DeviceCache.vb
- GPUTensor\DeviceResidentStore.vb
- GPUTensor\DeviceResidentStore64.vb
- GPUTensor\DoubleKernels.vb
- GPUTensor\NamespaceDoc.vb
- GPUTensor\SparseCsrCache.vb
- GPUTensor\TensorKernels.vb
- NamespaceDoc.vb

