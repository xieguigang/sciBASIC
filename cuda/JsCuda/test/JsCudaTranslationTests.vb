Imports Microsoft.VisualBasic.Computing.ILCuda.IL2Cuda
Imports Microsoft.VisualBasic.Computing.JSCuda
Imports Xunit

''' <summary>
''' Pure translation tests of the JavaScript &gt; CUDA pipeline: no device is required, every
''' assertion is made against the generated <c>.cu</c> text and the kernel metadata.
''' <para>
''' The device side is exercised by the tutorial demos (<c>tutorials\VBS\scripts\cuda</c>), which
''' run the kernels on a real GPU when one is available.
''' </para>
''' </summary>
Public Class JsCudaTranslationTests

    ' ======================================================== 2D name convention

    Private Const CellKernel As String =
        "function cell(i, j, dot, rowSumSq, rows) {
            const n = rows;
            let c = dot[i * n + j];
            if (c > 1) { c = 1; }
            if (c < -1) { c = -1; }
            return c;
        }"

    <Fact>
    Public Sub Translate_NamedParametersBecomeThreadIndices()
        Dim kernel = JsCudaTranslator.TranslateSource(CellKernel)

        Assert.Equal("js_cell_kernel", kernel.Name)
        Assert.Equal("js_cell_scalar", kernel.DeviceFunctionName)
        Assert.Equal(CudaIndexMode.Grid2D, kernel.IndexMode)
    End Sub

    <Fact>
    Public Sub Translate_EmitsADeviceFunctionAndAGlobalKernel()
        Dim source = JsCudaTranslator.TranslateSource(CellKernel).Source

        ' __device__ scalar function: index parameters are int, buffers are const pointers
        Assert.Contains("__device__ double js_cell_scalar(int i, int j, const double* __restrict__ dot", source)
        Assert.Contains("double rowSumSq", source)
        ' __global__ wrapper: thread index, bounds check, output write
        Assert.Contains("extern ""C"" __global__ void js_cell_kernel(", source)
        Assert.Contains("int i = blockIdx.y * blockDim.y + threadIdx.y;", source)
        Assert.Contains("int j = blockIdx.x * blockDim.x + threadIdx.x;", source)
        Assert.Contains("if (i >= js_nRows || j >= js_nCols) return;", source)
        Assert.Contains("js_out[i * js_nCols + j] = js_cell_scalar(i, j, dot, rowSumSq, rows);", source)
        ' the source is self contained C
        Assert.Contains("double* __restrict__ js_out", source)
    End Sub

    <Fact>
    Public Sub Translate_KeepsControlFlow()
        Dim source = JsCudaTranslator.TranslateSource(CellKernel).Source

        Assert.Contains("if (c > 1) {", source)
        Assert.Contains("c = 1;", source)
        Assert.Contains("return c;", source)
    End Sub

    <Fact>
    Public Sub Translate_ArrayCellIsCastToInteger()
        ' the script index expression is (i * n + j) with n a double scalar: the emitted subscript
        ' must be an integer, otherwise the device compiler rejects the code
        Dim source = JsCudaTranslator.TranslateSource(CellKernel).Source

        Assert.Contains("dot[((int)", source)
    End Sub

    ' ======================================================== element-wise

    <Fact>
    Public Sub Translate_ArrowFunctionBecomesAnElementWiseKernel()
        Dim kernel = JsCudaTranslator.TranslateSource("const square = (x) => x * x;")
        Dim source = kernel.Source

        Assert.Equal("js_square_kernel", kernel.Name)
        Assert.Equal(CudaIndexMode.Grid1D, kernel.IndexMode)
        ' every scalar parameter is read at the thread index
        Assert.Contains("int il_i = blockIdx.x * blockDim.x + threadIdx.x;", source)
        Assert.Contains("if (il_i >= js_n) return;", source)
        Assert.Contains("js_out[il_i] = js_square_scalar(x[il_i]);", source)
        Assert.Contains("const double* __restrict__ x", source)
    End Sub

    <Fact>
    Public Sub Translate_OneDimensionalKernelUsesTheLinearIndex()
        Dim source = JsCudaTranslator.TranslateSource(
            "function scale(i, x, factor) { return x[i] * factor; }").Source

        ' a real index parameter is used as-is (the element-wise synthetic index is il_i)
        Assert.Contains("int i = blockIdx.x * blockDim.x + threadIdx.x;", source)
        Assert.Contains("if (i >= js_n) return;", source)
        Assert.Contains("js_out[i] = js_scale_scalar(i, x, factor);", source)
    End Sub

    ' ======================================================== ctx sugar

    <Fact>
    Public Sub Translate_ContextStyleKernelIsEquivalent()
        Dim kernel = JsCudaTranslator.TranslateSource(
            "function clamp(ctx) {
                const v = ctx.x[ctx.i];
                return v > ctx.min ? v : ctx.min;
            }")

        Assert.Equal(CudaIndexMode.Grid1D, kernel.IndexMode)
        Assert.Contains("__device__ double js_clamp_scalar(int i, const double* __restrict__ x, double min)",
                        kernel.Source)
        Assert.Contains("js_out[i] = js_clamp_scalar(i, x, min);", kernel.Source)
    End Sub

    <Fact>
    Public Sub Translate_ContextStyleTwoDimensionalKernel()
        Dim kernel = JsCudaTranslator.TranslateSource(
            "const cell = (ctx) => {
                return ctx.dot[ctx.i * ctx.n + ctx.j];
            };")

        Assert.Equal(CudaIndexMode.Grid2D, kernel.IndexMode)
        Assert.Contains("int i = blockIdx.y * blockDim.y + threadIdx.y;", kernel.Source)
        Assert.Contains("int j = blockIdx.x * blockDim.x + threadIdx.x;", kernel.Source)
    End Sub

    ' ======================================================== math mapping

    <Fact>
    Public Sub Translate_MapsTheMathFunctionsToDeviceOnes()
        Dim source = JsCudaTranslator.TranslateSource(
            "function f(i, x) { return Math.sqrt(x[i]) + Math.abs(Math.min(1, x[i])); }").Source

        Assert.Contains("sqrt(", source)
        Assert.Contains("fabs(", source)
        Assert.Contains("fmin(", source)
    End Sub

    <Fact>
    Public Sub Translate_PowerBecomesPow()
        Dim source = JsCudaTranslator.TranslateSource("function f(i, x) { return x[i] ** 2; }").Source

        Assert.Contains("pow(", source)
    End Sub

    ' ======================================================== metadata

    <Fact>
    Public Sub Translate_RecordsTheParameterKinds()
        Dim kernel = JsCudaTranslator.TranslateSource(CellKernel)
        Dim kinds = JsCudaTranslator.ParameterKinds(kernel)

        Assert.Equal(3, kinds.Length)
        Assert.Equal("dot", kinds(0).Name)
        Assert.Equal(JsKernelParamKind.Buffer, kinds(0).Kind)
        Assert.Equal("rowSumSq", kinds(1).Name)
        Assert.Equal(JsKernelParamKind.Number, kinds(1).Kind)
        Assert.Equal("rows", kinds(2).Name)
        Assert.Equal(JsKernelParamKind.Number, kinds(2).Kind)
    End Sub

    <Fact>
    Public Sub Translate_ProducesReadablePseudoCode()
        Dim kernel = JsCudaTranslator.TranslateSource("function f(i, x) { return x[i] * 2; }")
        Dim code = JsCudaHost.PseudoCode(kernel)

        Assert.False(String.IsNullOrWhiteSpace(code))
        Assert.Contains("Function", code)
    End Sub

    ' ======================================================== failures

    <Fact>
    Public Sub Translate_BufferLengthIsRejectedWithAHint()
        Dim ex = Assert.Throws(Of JsCudaException)(
            Function() JsCudaTranslator.TranslateSource("function f(i, x) { return x.length; }"))

        Assert.Contains("pass the length as an integer parameter", ex.Message)
        Assert.Equal("f", ex.FunctionName)
    End Sub

    <Fact>
    Public Sub Translate_ModuloOnDoublesIsRejected()
        Dim ex = Assert.Throws(Of JsCudaException)(
            Function() JsCudaTranslator.TranslateSource("function f(i, x) { return x[i] % 2.0; }"))

        Assert.Contains("modulo operator", ex.Message)
    End Sub

    <Fact>
    Public Sub Translate_IntegerModuloIsAccepted()
        Dim source = JsCudaTranslator.TranslateSource(
            "function f(i, x) { const k = i % 4; return x[k]; }").Source

        Assert.Contains("%", source)
    End Sub

    <Fact>
    Public Sub Translate_UnknownIdentifierIsRejected()
        Dim ex = Assert.Throws(Of JsCudaException)(
            Function() JsCudaTranslator.TranslateSource("function f(i, x) { return ghost + 1; }"))

        Assert.Contains("unknown identifier 'ghost'", ex.Message)
    End Sub

    <Fact>
    Public Sub Translate_ScriptFunctionCallIsRejected()
        ' the first function of the source is the kernel, so 'f' comes first here
        Dim ex = Assert.Throws(Of JsCudaException)(
            Function() JsCudaTranslator.TranslateSource(
                "function f(i, x) { return helper(x[i]); }
                 function helper(v) { return v; }"))

        Assert.Contains("only the Math.* functions", ex.Message)
    End Sub

    <Fact>
    Public Sub Translate_ConsoleLogIsRejected()
        Dim ex = Assert.Throws(Of JsCudaException)(
            Function() JsCudaTranslator.TranslateSource("function f(i, x) { console.log(x[i]); return 1; }"))

        Assert.Contains("only assignments", ex.Message)
    End Sub

    <Fact>
    Public Sub Translate_ForOfIsRejected()
        Dim ex = Assert.Throws(Of JsCudaException)(
            Function() JsCudaTranslator.TranslateSource(
                "function f(i, x) { for (const v of x) { } return 1; }"))

        Assert.Contains("for..of", ex.Message)
    End Sub

    <Fact>
    Public Sub Translate_FunctionWithoutReturnIsRejected()
        Dim ex = Assert.Throws(Of JsCudaException)(
            Function() JsCudaTranslator.TranslateSource("function f(i, x) { const v = x[i]; }"))

        Assert.Contains("never returns a value", ex.Message)
    End Sub

    <Fact>
    Public Sub Translate_SourceWithoutAFunctionIsRejected()
        Dim ex = Assert.Throws(Of JsCudaException)(
            Function() JsCudaTranslator.TranslateSource("const a = 1;"))

        Assert.Contains("does not declare a function", ex.Message)
    End Sub

    <Fact>
    Public Sub Translate_HostDelegateIsRejected()
        Dim ex = Assert.Throws(Of JsCudaException)(
            Function() JsCudaTranslator.Translate(New Func(Of Object(), Object)(Function(args) 0.0)))

        Assert.Contains("not a script function", ex.Message)
    End Sub

    ' ======================================================== loop translation

    <Fact>
    Public Sub Translate_ForLoopBecomesACountedLoop()
        Dim source = JsCudaTranslator.TranslateSource(
            "function rowSum(i, x, cols) {
                let s = 0;
                for (let k = 0; k < cols; k++) {
                    s = s + x[i * cols + k];
                }
                return s;
            }").Source

        Assert.Contains("for (k = 0; k < cols; k = k + 1) {", source)
        Assert.Contains("return s;", source)
    End Sub

    <Fact>
    Public Sub Translate_WhileLoopIsKept()
        Dim source = JsCudaTranslator.TranslateSource(
            "function f(i, x) {
                let k = i;
                while (k > 0) { k = k - 1; }
                return k;
            }").Source

        Assert.Contains("while (k > 0) {", source)
    End Sub

End Class
