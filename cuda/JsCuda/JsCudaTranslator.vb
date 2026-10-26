Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Microsoft.VisualBasic.Computing.ILCuda.IL2Cuda
Imports Microsoft.VisualBasic.Computing.ILCuda.Kernels

' ---------------------------------------------------------------------------
'  JavaScript 函数 -> .cu 源码 -> IlCudaKernel
'
'  与 IL 流水线（IlCudaTranslator）产出同样的形状：
'   1) 一个 __device__ 标量函数：函数体原样翻译（if / while / for 结构保留）；
'   2) 一个 extern "C" __global__ 内核：负责线程索引、越界保护、从数组里取参数、
'      调用上面的标量函数、把返回值写回输出数组。
'
'  所有生成的符号统一 js_ 前缀，避免与 il_* 以及框架内置的 .cu 撞名——它们最终会
'  合并进同一个 NVRTC 编译单元。
' ---------------------------------------------------------------------------

Namespace Global.Microsoft.VisualBasic.Computing.JSCuda

    ''' <summary>Translation of JavaScript functions / sources into CUDA kernels.</summary>
    Public Module JsCudaTranslator

        ''' <summary>Output parameter name of every generated kernel.</summary>
        Public Const OutputName As String = "js_out"

        ' the parameter kinds of each translated kernel, keyed by generated kernel name
        Private ReadOnly _signatures As New Dictionary(Of String, JsKernel)(StringComparer.OrdinalIgnoreCase)

        ''' <summary>
        ''' Translate a script function value (the delegate the interpreter created for a
        ''' <c>function</c> / arrow function) into a CUDA kernel.
        ''' </summary>
        ''' <param name="scriptFunction">a JavaScript function value, or a <c>FuncExpr</c> AST node</param>
        Public Function Translate(scriptFunction As Object, Optional options As JsKernelOptions = Nothing) As IlCudaKernel
            Dim node As FuncExpr = TryCast(scriptFunction, FuncExpr)

            If node Is Nothing Then
                If scriptFunction Is Nothing Then
                    Throw New JsCudaException("js->cuda: nothing to translate, expected a JavaScript function")
                End If

                If Not JsFunctions.TryGetNode(scriptFunction, node) Then
                    Throw New JsCudaException(
                        "js->cuda: the given value is not a script function " &
                        "(host delegates have no source AST; pass the function source to translateSource instead)")
                End If
            End If

            Return TranslateNode(node, options)
        End Function

        ''' <summary>
        ''' Translate JavaScript <b>source text</b> (one function declaration or arrow function) into
        ''' a CUDA kernel. Handy for tests, documentation and generated scripts.
        ''' </summary>
        Public Function TranslateSource(source As String, Optional options As JsKernelOptions = Nothing) As IlCudaKernel
            If String.IsNullOrWhiteSpace(source) Then
                Throw New JsCudaException("js->cuda: the kernel source is empty")
            End If

            Dim program As Program

            Try
                program = Parser.Parse(source)
            Catch ex As Exception
                Throw New JsCudaException("js->cuda: cannot parse the kernel source: " & ex.Message, ex)
            End Try

            Dim node As FuncExpr = FindFunction(program.Body)

            If node Is Nothing Then
                Throw New JsCudaException(
                    "js->cuda: the source does not declare a function (expected 'function name(...) {...}' " &
                    "or 'const name = (...) => {...}')")
            End If

            Return TranslateNode(node, options)
        End Function

        ''' <summary>The typed AST + metadata of an already translated kernel (Nothing if unknown).</summary>
        Public Function FindKernel(kernel As IlCudaKernel) As JsKernel
            If kernel Is Nothing Then Return Nothing

            Dim found As JsKernel = Nothing

            If _signatures.TryGetValue(kernel.Name, found) Then Return found

            Return Nothing
        End Function

        ''' <summary>The parameter kinds of the kernel, in kernel argument order (may be empty).</summary>
        Public Function ParameterKinds(kernel As IlCudaKernel) As JsKernelParam()
            Dim typed = FindKernel(kernel)

            If typed Is Nothing Then Return New JsKernelParam() {}

            Return typed.Parameters.ToArray
        End Function

        ''' <summary>Drop the translated kernel signatures (tests / hot reload).</summary>
        Public Sub ClearCache()
            SyncLock _signatures
                Call _signatures.Clear()
            End SyncLock
        End Sub

        Friend Function TranslateNode(node As FuncExpr, options As JsKernelOptions) As IlCudaKernel
            Dim typed As JsKernel = New JsCudaDecompiler(options).Decompile(node)

            If Not typed.ReturnsValue Then
                Throw New JsCudaException(
                    $"js->cuda: function '{typed.JsName}' never returns a value; a kernel must return the " &
                    "value of the cell it computes")
            End If

            Dim source As String = BuildSource(typed)
            Dim kernel As IlCudaKernel = New IlCudaKernel With {
                .Name = typed.KernelName,
                .Source = source,
                .DeviceFunctionName = typed.DeviceFunctionName,
                .IndexMode = typed.Mode,
                .Syntax = typed.Syntax,
                .MethodName = "js:" & If(typed.JsName, "<anonymous>"),
                .CountParameterName = If(typed.Mode = CudaIndexMode.Grid2D, "js_nRows", "js_n"),
                .OutputParameterName = OutputName
            }

            SyncLock _signatures
                _signatures(kernel.Name) = typed
            End SyncLock

            Return kernel
        End Function

        ' ======================================================== source generation

        ''' <summary>Generate the complete <c>.cu</c> text of a translated kernel.</summary>
        Public Function BuildSource(typed As JsKernel) As String
            If typed Is Nothing Then Throw New ArgumentNullException(NameOf(typed))

            Dim code As New StringBuilder()
            Dim syntax = typed.Syntax

            code.AppendLine("// ===== 本文件由 JS -> AST -> CUDA 流水线自动生成，请勿手工编辑 =====")
            code.AppendLine($"// 来源函数 : {If(typed.JsName, "<anonymous>")}")
            code.AppendLine($"// 设备函数 : {typed.DeviceFunctionName}")
            code.AppendLine($"// 内核     : {typed.KernelName}（索引模式 {typed.Mode}）")
            code.AppendLine()

            ' ---------- 1) __device__ 标量函数 ----------
            Dim signature As New List(Of String)()

            For Each parameter In syntax.Parameters
                Call signature.Add(DeviceParameter(parameter.Name, parameter.ParameterType))
            Next

            code.AppendLine($"__device__ double {typed.DeviceFunctionName}({String.Join(", ", signature)}) {{")

            Dim emitter As New CudaEmitter(1)

            Try
                emitter.EmitBlock(syntax.Body)
            Catch ex As NotSupportedException
                Throw New JsCudaException(
                    $"js->cuda: function '{typed.JsName}': the device code cannot be emitted: " & ex.Message, ex)
            End Try

            code.Append(emitter.Text)
            code.AppendLine("}")
            code.AppendLine()

            If typed.Mode = CudaIndexMode.None Then Return code.ToString()

            ' ---------- 2) __global__ 内核 ----------
            Dim kernelParams As New List(Of String)()

            For Each parameter In typed.Parameters
                Dim element As String = If(parameter.Kind = JsKernelParamKind.Buffer, "const double* __restrict__", "double")

                If typed.ElementWise AndAlso parameter.Kind <> JsKernelParamKind.Buffer Then
                    ' element-wise wrapping: every scalar parameter is read at the thread index
                    element = "const double* __restrict__"
                End If

                If parameter.Kind = JsKernelParamKind.Int32 Then element = "int"

                Call kernelParams.Add($"{element} {parameter.Name}")
            Next

            Call kernelParams.Add($"double* __restrict__ {OutputName}")

            If typed.Mode = CudaIndexMode.Grid2D Then
                Call kernelParams.Add("int js_nRows")
                Call kernelParams.Add("int js_nCols")
            Else
                Call kernelParams.Add("int js_n")
            End If

            code.AppendLine($"extern ""C"" __global__ void {typed.KernelName}({String.Join(", ", kernelParams)}) {{")

            Dim rowName As String = If(typed.IndexNames.Length > 0, typed.IndexNames(0), "il_i")

            If typed.Mode = CudaIndexMode.Grid2D Then
                Dim colName As String = If(typed.IndexNames.Length > 1, typed.IndexNames(1), "il_j")

                code.AppendLine($"    int {rowName} = blockIdx.y * blockDim.y + threadIdx.y;")
                code.AppendLine($"    int {colName} = blockIdx.x * blockDim.x + threadIdx.x;")
                code.AppendLine($"    if ({rowName} >= js_nRows || {colName} >= js_nCols) return;")
            Else
                code.AppendLine($"    int {rowName} = blockIdx.x * blockDim.x + threadIdx.x;")
                code.AppendLine("    if (" & rowName & " >= js_n) return;")
            End If

            ' the device function arguments: index parameters first, then the kernel parameters
            Dim callArgs As New List(Of String)()

            For Each name As String In typed.IndexNames
                Call callArgs.Add(name)
            Next

            For Each parameter In typed.Parameters
                If typed.ElementWise AndAlso parameter.Kind <> JsKernelParamKind.Buffer Then
                    Call callArgs.Add($"{parameter.Name}[{rowName}]")
                Else
                    Call callArgs.Add(parameter.Name)
                End If
            Next

            Dim writeIndex As String

            If typed.Mode = CudaIndexMode.Grid2D Then
                writeIndex = $"{rowName} * js_nCols + {If(typed.IndexNames.Length > 1, typed.IndexNames(1), "il_j")}"
            Else
                writeIndex = rowName
            End If

            code.AppendLine($"    {OutputName}[{writeIndex}] = {typed.DeviceFunctionName}({String.Join(", ", callArgs)});")
            code.AppendLine("}")

            Return code.ToString()
        End Function

        Private Function DeviceParameter(name As String, type As System.Type) As String
            If type Is GetType(Double()) Then Return $"const double* __restrict__ {name}"
            If type Is GetType(Integer) Then Return $"int {name}"

            Return $"double {name}"
        End Function

        ' ======================================================== helpers

        Private Function FindFunction(body As List(Of Statement)) As FuncExpr
            If body Is Nothing Then Return Nothing

            For Each statement In body
                Dim declaration = TryCast(statement, FuncDeclStmt)

                If declaration IsNot Nothing Then
                    Return New FuncExpr(
                        declaration.Name,
                        declaration.Parameters,
                        declaration.Body,
                        False,
                        declaration.Defaults,
                        declaration.RestParam,
                        declaration.IsAsync,
                        declaration.IsGenerator) With {
                        .BodySource = declaration.BodySource
                    }
                End If

                Dim varStmt = TryCast(statement, VarStmt)

                If varStmt IsNot Nothing Then
                    For Each declarant In varStmt.Declarations
                        Dim func = TryCast(declarant.Init, FuncExpr)

                        If func IsNot Nothing Then
                            Return func
                        End If

                        Dim spec = TryCast(declarant.Init, ClassExpr)

                        If spec IsNot Nothing AndAlso spec.Spec IsNot Nothing AndAlso spec.Spec.Constructor IsNot Nothing Then
                            Return spec.Spec.Constructor
                        End If
                    Next
                End If

                Dim exprStmt = TryCast(statement, ExprStmt)

                If exprStmt IsNot Nothing Then
                    Dim func = TryCast(exprStmt.Expr, FuncExpr)

                    If func IsNot Nothing Then Return func
                End If
            Next

            Return Nothing
        End Function

    End Module

End Namespace
