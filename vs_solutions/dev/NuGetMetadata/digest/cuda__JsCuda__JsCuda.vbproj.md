# cuda/JsCuda/JsCuda.vbproj

- RootNamespace : Microsoft.VisualBasic.Computing.JSCuda
- AssemblyName  : Microsoft.VisualBasic.Computing.JSCuda
- TargetFramework: net10.0
- Source files  : 6
- Existing Title: 
- Existing Desc : 
- Existing Tags : 

## Namespaces
- Global.Microsoft.VisualBasic.Computing.JSCuda  [files: 6]

## Public types
- Class JsCudaDecompiler (JsCudaDecompiler.vb)
- Class JsCudaException (JsCudaException.vb) - Raised when a JavaScript function cannot be translated into a CUDA kernel: an unsupported statement / expression, a value that is not a script function, a captured outer variable, an unmapped math call, and so on.
- Class JsCudaHost (JsCudaHost.vb) - result back. <para> From a script: <c>import Microsoft.VisualBasic.Computing.JSCuda as jscuda from '&lt;dll&gt;'</c>
- Module JsCudaTranslator (JsCudaTranslator.vb)
- Enum JsKernelParamKind (JsKernel.vb)
- Class JsKernelParam (JsKernel.vb)
- Class JsKernel (JsKernel.vb) - The outcome of translating one JavaScript function: the typed AST that <see cref="IL2Cuda.CudaEmitter"/> turns into device code, plus the metadata the host needs to build the kernel wrapper and to marshal the arguments.
- Class JsKernelOptions (JsKernelOptions.vb) - Tuning knobs of the JavaScript &gt; CUDA translation. <para> Everything has a working default: an empty options object reproduces the documented

## Notable public members
- Public Sub New(Optional options As JsKernelOptions = Nothing)
- Public Function Decompile(func As FuncExpr) As JsKernel
- Public Property FunctionName As String = ""
- Public Property NodeKind As String = ""
- Public Property SourcePosition As String = ""
- Public Sub New(message As String)
- Public Sub New(message As String, inner As Exception)
- Public Shared Function OfNode(functionName As String, node As Object, message As String) As JsCudaException
- Public Shared Function translate(function_ As Object,
- Public Shared Function translateSource(source As String,
- Public Shared Function cudaSource(kernel As IlCudaKernel) As String
- Public Shared Function pseudoCode(kernel As IlCudaKernel) As String
- Public Shared Function register(kernel As IlCudaKernel) As String
- Public Shared Function indexMode(kernel As IlCudaKernel) As String
- Public Shared Function parameterNames(kernel As IlCudaKernel) As String()
- Public Shared Function available() As Boolean
- Public Shared Function deviceName() As String
- Public Shared Function diagnostics() As String()
- Public Shared Function createEngine(Optional deviceOrdinal As Integer = 0) As CudaEngine
- Public Shared Sub synchronize(engine As CudaEngine)
- Public Shared Function buffer(values As Double()) As DeviceBuffer(Of Double)
- Public Shared Function bufferSize(count As Integer) As DeviceBuffer(Of Double)
- Public Shared Function read(memory As DeviceMemory) As Double()
- Public Shared Sub launch1D(kernel As IlCudaKernel, engine As CudaEngine,
- Public Shared Sub launch2D(kernel As IlCudaKernel, engine As CudaEngine,
- Public Shared Function maxError(gpu As Double(), cpu As Double()) As Double
- Public Shared Function preview(values As Double(), rows As Integer, Optional n As Integer = 6) As String
- Public Const OutputName As String = "js_out"
- Public Function Translate(scriptFunction As Object, Optional options As JsKernelOptions = Nothing) As IlCudaKernel
- Public Function TranslateSource(source As String, Optional options As JsKernelOptions = Nothing) As IlCudaKernel
- Public Function FindKernel(kernel As IlCudaKernel) As JsKernel
- Public Function ParameterKinds(kernel As IlCudaKernel) As JsKernelParam()
- Public Sub ClearCache()
- Friend Function TranslateNode(node As FuncExpr, options As JsKernelOptions) As IlCudaKernel
- Public Function BuildSource(typed As JsKernel) As String
- Public Property Name As String
- Public Property Kind As JsKernelParamKind = JsKernelParamKind.Number
- Public Sub New()
- Public Sub New(name As String, kind As JsKernelParamKind)
- Public Overrides Function ToString() As String
- Public Property JsName As String = ""
- Public Property KernelName As String = ""
- Public Property DeviceFunctionName As String = ""
- Public Property Mode As CudaIndexMode = CudaIndexMode.Grid1D
- Public Property IndexNames As String() = {}
- Public Property Parameters As New List(Of JsKernelParam)
- Public Property Syntax As IL.MethodSyntax
- Public Property ReturnsValue As Boolean = True
- Public Property ElementWise As Boolean = False
- Public ReadOnly Property ParameterNames As String()
- Public Overrides Function ToString() As String
- Public Property Name As String = ""
- Public Property IndexParameterNames As String() = {"i", "j"}
- Public Property ContextParameter As String = "ctx"
- Public Property BlockSize As Integer = 256
- Public Function Clone() As JsKernelOptions
- Friend Function IsIndexName(name As String) As Boolean
- Friend Function IndexRank(name As String) As Integer

## Imports
- IL = Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.IL
- Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
- Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
- Microsoft.VisualBasic.Computing.ILCuda.IL2Cuda
- Microsoft.VisualBasic.Computing.ILCuda.Kernels
- Microsoft.VisualBasic.Computing.ILCuda.Runtime
- std = System.Math
- System.Collections
- System.Reflection
- System.Text

## File tree
- JsCudaDecompiler.vb
- JsCudaException.vb
- JsCudaHost.vb
- JsCudaTranslator.vb
- JsKernel.vb
- JsKernelOptions.vb

