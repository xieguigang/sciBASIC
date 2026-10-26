Namespace Global.Microsoft.VisualBasic.Computing.JSCuda

    ''' <summary>
    ''' Tuning knobs of the JavaScript &gt; CUDA translation.
    ''' <para>
    ''' Everything has a working default: an empty options object reproduces the documented
    ''' convention (parameters named <c>i</c>/<c>j</c> are thread indices, the other parameters
    ''' are device buffers or scalars, the returned value is written to the output array).
    ''' </para>
    ''' </summary>
    Public Class JsKernelOptions

        ''' <summary>
        ''' Kernel name; empty means "use the JavaScript function name". The generated symbol is
        ''' always prefixed with <c>js_</c> so it can never collide with the <c>il_*</c> kernels
        ''' produced by the IL pipeline or with the framework's own <c>.cu</c> files.
        ''' </summary>
        Public Property Name As String = ""

        ''' <summary>
        ''' Parameter names that carry the thread index. The first name is the row (or the linear
        ''' index for a 1D launch) and the second one is the column, matching the IL pipeline.
        ''' </summary>
        Public Property IndexParameterNames As String() = {"i", "j"}

        ''' <summary>
        ''' Name of the single context parameter accepted by the <c>ctx</c> style kernels
        ''' (<c>function cell(ctx) { ... ctx.i ... ctx.dot[k] ... }</c>). A function with exactly
        ''' this one parameter is translated in context mode; any other signature uses the plain
        ''' name convention.
        ''' </summary>
        Public Property ContextParameter As String = "ctx"

        ''' <summary>Threads per block used by <see cref="JsCudaHost.Launch1D"/> / <see cref="JsCudaHost.Launch2D"/>.</summary>
        Public Property BlockSize As Integer = 256

        Public Function Clone() As JsKernelOptions
            Return New JsKernelOptions With {
                .Name = Name,
                .IndexParameterNames = If(IndexParameterNames, {"i", "j"}),
                .ContextParameter = ContextParameter,
                .BlockSize = BlockSize
            }
        End Function

        ''' <summary>Is <paramref name="name"/> one of the configured index parameters?</summary>
        Friend Function IsIndexName(name As String) As Boolean
            Dim names As String() = If(IndexParameterNames, {"i", "j"})

            For Each candidate As String In names
                If String.Equals(candidate, name, StringComparison.Ordinal) Then Return True
            Next

            Return False
        End Function

        ''' <summary>The rank of <paramref name="name"/> in the index parameter list (-1 = not an index).</summary>
        Friend Function IndexRank(name As String) As Integer
            Dim names As String() = If(IndexParameterNames, {"i", "j"})

            For i As Integer = 0 To names.Length - 1
                If String.Equals(names(i), name, StringComparison.Ordinal) Then Return i
            Next

            Return -1
        End Function

    End Class

End Namespace
