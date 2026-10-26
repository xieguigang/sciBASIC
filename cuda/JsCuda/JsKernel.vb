Imports IL = Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.IL
Imports Microsoft.VisualBasic.Computing.ILCuda.IL2Cuda

Namespace Global.Microsoft.VisualBasic.Computing.JSCuda

    ''' <summary>How one kernel parameter is passed from the host.</summary>
    Public Enum JsKernelParamKind
        ''' <summary>A device buffer (<c>const double* __restrict__</c>).</summary>
        Buffer = 0
        ''' <summary>A 32 bit integer (<c>int</c>): sizes, strides, lengths.</summary>
        Int32 = 1
        ''' <summary>A double precision scalar (<c>double</c>).</summary>
        Number = 2
    End Enum

    ''' <summary>One kernel parameter (thread index parameters are not part of this list).</summary>
    Public Class JsKernelParam

        Public Property Name As String
        Public Property Kind As JsKernelParamKind = JsKernelParamKind.Number

        Public Sub New()
        End Sub

        Public Sub New(name As String, kind As JsKernelParamKind)
            Me.Name = name
            Me.Kind = kind
        End Sub

        Public Overrides Function ToString() As String
            Return Name & " As " & Kind.ToString()
        End Function

    End Class

    ''' <summary>
    ''' The outcome of translating one JavaScript function: the typed AST that
    ''' <see cref="IL2Cuda.CudaEmitter"/> turns into device code, plus the metadata the host needs
    ''' to build the kernel wrapper and to marshal the arguments.
    ''' </summary>
    Public Class JsKernel

        ''' <summary>Name of the JavaScript function (empty for an anonymous arrow function).</summary>
        Public Property JsName As String = ""

        ''' <summary>Generated <c>extern "C" __global__</c> symbol (always <c>js_</c> prefixed).</summary>
        Public Property KernelName As String = ""

        ''' <summary>Generated <c>__device__</c> scalar function symbol.</summary>
        Public Property DeviceFunctionName As String = ""

        ''' <summary>Thread index mode inferred from the signature / body.</summary>
        Public Property Mode As CudaIndexMode = CudaIndexMode.Grid1D

        ''' <summary>The index parameter names in launch order (<c>i</c> or <c>i, j</c>).</summary>
        Public Property IndexNames As String() = {}

        ''' <summary>Non-index parameters, in kernel argument order.</summary>
        Public Property Parameters As New List(Of JsKernelParam)

        ''' <summary>The typed AST (also handy for <c>SyntaxWriter.WriteMethod</c>).</summary>
        Public Property Syntax As IL.MethodSyntax

        ''' <summary>Does the JavaScript function return the cell value?</summary>
        Public Property ReturnsValue As Boolean = True

        ''' <summary>
        ''' Element-wise wrapping: a pure scalar formula with no index parameter, every scalar
        ''' parameter becomes a buffer read at the thread index.
        ''' </summary>
        Public Property ElementWise As Boolean = False

        Public ReadOnly Property ParameterNames As String()
            Get
                Return Parameters.Select(Function(p) p.Name).ToArray()
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{KernelName} [{Mode}] js:{If(JsName, "<anonymous>")}"
        End Function

    End Class

End Namespace
