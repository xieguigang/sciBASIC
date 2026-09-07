' ---------------------------------------------------------------------------
' IL -> AST -> CUDA 的端到端翻译
'
'   IlCudaTranslator.Translate(MethodInfo) -> IlCudaKernel
'
' 产物是一段完整的 .cu 源码，包含：
'   1) 一个 __device__ 标量函数：方法体原样翻译（含 if / while / for 结构）；
'   2) （可选）一个 extern "C" __global__ 内核：负责线程索引、越界保护、
'      从数组里取参数、调用上面的标量函数、把结果写回输出数组。
'
' 内核参数 = 方法参数中去掉"线程索引参数"的部分 + 输出数组 + 元素个数，
' 顺序稳定，调用方按这个顺序传参即可（见 IlCudaKernel.Launch）。
'
' 所有生成的符号统一带 il_ / il 前缀，避免与 metrics.cu 及框架自带的四个
' .cu 撞名——它们最终会合并进同一个 NVRTC 编译单元。
' ---------------------------------------------------------------------------

Imports System.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.IL
Imports Microsoft.VisualBasic.Computing.ILCuda.Runtime
Imports Microsoft.VisualBasic.Computing.ILCuda.Kernels

Namespace IL2Cuda

    ''' <summary>一个由 IL 反编译生成的 CUDA 内核</summary>
    Public Class IlCudaKernel

        ''' <summary>内核函数名（与 .cu 中 extern "C" 的名字一致）</summary>
        Public Property Name As String
        ''' <summary>完整 .cu 源码</summary>
        Public Property Source As String
        ''' <summary>__device__ 标量函数名</summary>
        Public Property DeviceFunctionName As String
        ''' <summary>线程索引模式</summary>
        Public Property IndexMode As CudaIndexMode
        ''' <summary>反编译出来的 AST（调试 / 自检用）</summary>
        Public Property Syntax As MethodSyntax
        ''' <summary>源方法的显示名</summary>
        Public Property MethodName As String
        ''' <summary>内核的启动规模上限参数名（1D：il_n；2D：il_nRows / il_nCols）</summary>
        Public Property CountParameterName As String = "il_n"
        ''' <summary>输出数组的参数名</summary>
        Public Property OutputParameterName As String = "il_out"

        ''' <summary>把生成的源码注入框架（必须在 CudaEngine.TryCreate 之前调用）</summary>
        Public Sub Register()
            KernelSources.RegisterSource(Name & ".cu", Source, "il")
        End Sub

        ''' <summary>按启动配置启动内核；args 的顺序见本类的 XML 说明</summary>
        Public Sub Launch(engine As CudaEngine, config As LaunchConfig, ParamArray args As Object())
            If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))

            Dim kernel = engine.GetKernel(Name)

            kernel.Launch(config, args)
        End Sub

        Public Sub Launch(engine As CudaEngine, stream As CudaStream, config As LaunchConfig,
                          ParamArray args As Object())
            If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))

            engine.GetKernel(Name).Launch(stream, config, args)
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Name} [{IndexMode}] <- {MethodName}"
        End Function
    End Class

    Public Module IlCudaTranslator

        ''' <summary>
        ''' 把一个已编译的 VB.NET 方法反编译并翻译成 CUDA 内核源码。
        ''' </summary>
        ''' <param name="method">目标方法（必须 Shared）</param>
        ''' <param name="kernelName">可选的内核名；留空时用注解里的名字或 il_&lt;方法名&gt;_kernel</param>
        Public Function Translate(method As MethodInfo, Optional kernelName As String = Nothing) As IlCudaKernel
            If method Is Nothing Then Throw New ArgumentNullException(NameOf(method))

            Dim options As New DecompileOptions()
            Dim syntax = MethodDecompiler.Decompile(method, options)

            Dim attr = method.GetCustomAttribute(Of CudaKernelAttribute)()
            Dim mode As CudaIndexMode
            Dim explicitName As String = Nothing

            If attr IsNot Nothing Then
                mode = attr.Mode
                explicitName = attr.Name
            Else
                mode = InferIndexMode(syntax)
            End If

            If String.IsNullOrWhiteSpace(kernelName) Then
                kernelName = If(String.IsNullOrWhiteSpace(explicitName),
                                "il_" & method.Name & "_kernel",
                                explicitName)
            End If

            If Not kernelName.StartsWith("il_", StringComparison.Ordinal) Then
                kernelName = "il_" & kernelName
            End If

            Dim deviceName = "il_" & method.Name & "_scalar"
            Dim source = BuildSource(syntax, kernelName, deviceName, mode)

            Return New IlCudaKernel With {
                .Name = kernelName,
                .Source = source,
                .DeviceFunctionName = deviceName,
                .IndexMode = mode,
                .Syntax = syntax,
                .MethodName = If(method.DeclaringType Is Nothing,
                                 method.Name,
                                 method.DeclaringType.Name & "." & method.Name)
            }
        End Function

        ' ==================================================================
        ' 索引模式推断（无标注时的默认约定）
        ' ==================================================================

        Private Function InferIndexMode(syntax As MethodSyntax) As CudaIndexMode
            Dim names = syntax.Parameters _
                .Select(Function(p) If(p.Name, String.Empty).ToLowerInvariant()) _
                .ToList()

            Dim hasRow = names.Contains("i") OrElse names.Contains("row")
            Dim hasCol = names.Contains("j") OrElse names.Contains("col")

            If hasRow AndAlso hasCol Then Return CudaIndexMode.Grid2D
            If hasRow Then Return CudaIndexMode.Grid1D

            ' 没有显式的索引参数：只要方法体是纯标量（不做数组取元素），
            ' 就可以把每个标量参数都当作"按线程下标读取的数组"来包裹成一个逐元素内核。
            If IsPureScalar(syntax.Body) Then Return CudaIndexMode.Grid1D

            Return CudaIndexMode.None
        End Function

        ''' <summary>方法体里是否完全没有数组取元素</summary>
        Private Function IsPureScalar(body As BlockStatement) As Boolean
            If body Is Nothing Then Return True

            For Each stmt As Statement In body.Statements
                If Not IsPureScalarStatement(stmt) Then Return False
            Next

            Return True
        End Function

        Private Function IsPureScalarStatement(stmt As Statement) As Boolean
            If stmt Is Nothing Then Return True

            Select Case stmt.Kind
                Case SyntaxKind.Block
                    Return IsPureScalar(DirectCast(stmt, BlockStatement))

                Case SyntaxKind.VariableDeclaration
                    Dim decl = DirectCast(stmt, VariableDeclarationStatement)
                    Return decl.Initializer Is Nothing OrElse IsPureScalarExpr(decl.Initializer)

                Case SyntaxKind.Assignment
                    Dim assign = DirectCast(stmt, AssignmentStatement)
                    Return IsPureScalarExpr(assign.Target) AndAlso IsPureScalarExpr(assign.Value)

                Case SyntaxKind.ExpressionStatement
                    Return IsPureScalarExpr(DirectCast(stmt, ExpressionStatement).Value)

                Case SyntaxKind.ReturnStmt
                    Dim ret = DirectCast(stmt, ReturnStatement)
                    Return ret.Value Is Nothing OrElse IsPureScalarExpr(ret.Value)

                Case SyntaxKind.IfStmt
                    Dim [if] = DirectCast(stmt, IfStatement)
                    Return IsPureScalarExpr([if].Condition) AndAlso
                           IsPureScalar([if].ThenBody) AndAlso
                           IsPureScalar([if].ElseBody)

                Case SyntaxKind.WhileStmt
                    Dim loopStmt = DirectCast(stmt, WhileStatement)
                    Return IsPureScalarExpr(loopStmt.Condition) AndAlso IsPureScalar(loopStmt.Body)

                Case SyntaxKind.ForStmt
                    Dim loopStmt = DirectCast(stmt, ForStatement)
                    Return IsPureScalarExpr(loopStmt.Initializer) AndAlso
                           IsPureScalarExpr(loopStmt.Condition) AndAlso
                           IsPureScalarExpr(loopStmt.Increment) AndAlso
                           IsPureScalar(loopStmt.Body)

                Case Else
                    Return True
            End Select
        End Function

        Private Function IsPureScalarExpr(expr As Expression) As Boolean
            If expr Is Nothing Then Return True

            Select Case expr.Kind
                Case SyntaxKind.ArrayIndex
                    Return False
                Case SyntaxKind.Binary
                    Dim bin = DirectCast(expr, BinaryExpression)
                    Return IsPureScalarExpr(bin.Left) AndAlso IsPureScalarExpr(bin.Right)
                Case SyntaxKind.Unary
                    Return IsPureScalarExpr(DirectCast(expr, UnaryExpression).Operand)
                Case SyntaxKind.Convert
                    Return IsPureScalarExpr(DirectCast(expr, ConvertExpression).Operand)
                Case SyntaxKind.Invoke
                    Dim callNode = DirectCast(expr, CallExpression)
                    For Each arg In callNode.AllArguments
                        If Not IsPureScalarExpr(arg) Then Return False
                    Next
                    Return True
                Case SyntaxKind.Ternary
                    Dim ternary = DirectCast(expr, TernaryExpression)
                    Return IsPureScalarExpr(ternary.Condition) AndAlso
                           IsPureScalarExpr(ternary.WhenTrue) AndAlso
                           IsPureScalarExpr(ternary.WhenFalse)
                Case Else
                    Return True
            End Select
        End Function

        ' ==================================================================
        ' 源码生成
        ' ==================================================================

        Private Function BuildSource(syntax As MethodSyntax,
                                     kernelName As String,
                                     deviceName As String,
                                     mode As CudaIndexMode) As String
            Dim code As New StringBuilder()

            code.AppendLine("// ===== 本文件由 IL -> AST -> CUDA 流水线自动生成，请勿手工编辑 =====")
            code.AppendLine($"// 来源方法 : {syntax.Name}")
            code.AppendLine($"// 设备函数 : {deviceName}")
            code.AppendLine($"// 内核     : {kernelName}（索引模式 {mode}）")
            code.AppendLine()

            ' ---------- 1) __device__ 标量函数 ----------
            Dim returnType = CudaTypeMap.CudaType(syntax.ReturnType)

            If returnType Is Nothing AndAlso syntax.ReturnType IsNot GetType(Void) Then
                Throw New NotSupportedException(
                    $"方法 {syntax.Name} 的返回类型 {syntax.ReturnType.FullName} 无法映射为 CUDA 类型")
            End If

            ' 数组参数一律写成 const T* __restrict__：内核侧传进来的就是只读指针，
            ' 不加 const 会在调用处报 "const float* 无法转成 float*"
            Dim signature = syntax.Parameters.Select(
                Function(p)
                    If p.ParameterType.IsArray Then
                        Dim element = CudaTypeMap.CudaType(CudaTypeMap.ElementType(p.ParameterType))
                        Return $"const {element}* __restrict__ {CudaTypeMap.SafeName(ParameterNameOf(p))}"
                    End If

                    Return $"{CudaTypeMap.CudaType(p.ParameterType)} {CudaTypeMap.SafeName(ParameterNameOf(p))}"
                End Function)

            code.AppendLine($"__device__ {If(returnType, "void")} {deviceName}({String.Join(", ", signature)}) {{")

            Dim emitter As New CudaEmitter(1)

            emitter.EmitBlock(syntax.Body)
            code.Append(emitter.Text)
            code.AppendLine("}")
            code.AppendLine()

            If mode = CudaIndexMode.None Then Return code.ToString()

            ' ---------- 2) __global__ 内核 ----------
            Dim indexNames = IndexParameterNames(syntax, mode)
            Dim kernelParams As New List(Of String)()

            For Each p In syntax.Parameters
                If indexNames.Contains(ParameterNameOf(p)) Then Continue For

                Dim elementType = CudaTypeMap.CudaType(CudaTypeMap.ElementType(p.ParameterType))

                If elementType Is Nothing Then
                    Throw New NotSupportedException($"参数 {p.Name} 的类型无法映射为 CUDA 类型")
                End If

                If IsScalarWrapped(syntax, mode) Then
                    ' 逐元素包裹：每个标量参数都变成"按下标读取"的输入数组
                    kernelParams.Add($"const {elementType}* __restrict__ {CudaTypeMap.SafeName(ParameterNameOf(p))}")
                ElseIf p.ParameterType.IsArray Then
                    kernelParams.Add($"const {elementType}* __restrict__ {CudaTypeMap.SafeName(ParameterNameOf(p))}")
                Else
                    kernelParams.Add($"{elementType} {CudaTypeMap.SafeName(ParameterNameOf(p))}")
                End If
            Next

            Dim outType = CudaTypeMap.CudaType(CudaTypeMap.ElementType(syntax.ReturnType))

            If outType Is Nothing Then
                Throw New NotSupportedException($"方法 {syntax.Name} 没有返回值，无法生成输出数组")
            End If

            kernelParams.Add($"{outType}* __restrict__ il_out")

            If mode = CudaIndexMode.Grid2D Then
                kernelParams.Add("int il_nRows")
                kernelParams.Add("int il_nCols")
            Else
                kernelParams.Add("int il_n")
            End If

            code.AppendLine($"extern ""C"" __global__ void {kernelName}({String.Join(", ", kernelParams)}) {{")

            ' 线程索引 + 越界保护
            If mode = CudaIndexMode.Grid2D Then
                code.AppendLine($"    int {indexNames(0)} = blockIdx.y * blockDim.y + threadIdx.y;")
                code.AppendLine($"    int {indexNames(1)} = blockIdx.x * blockDim.x + threadIdx.x;")
                code.AppendLine($"    if ({indexNames(0)} >= il_nRows || {indexNames(1)} >= il_nCols) return;")
            Else
                code.AppendLine($"    int {indexNames(0)} = blockIdx.x * blockDim.x + threadIdx.x;")
                code.AppendLine($"    if ({indexNames(0)} >= il_n) return;")
            End If

            ' 调用标量函数
            Dim callArgs As New List(Of String)()

            For Each p In syntax.Parameters
                Dim nm = CudaTypeMap.SafeName(ParameterNameOf(p))

                If IsScalarWrapped(syntax, mode) AndAlso Not p.ParameterType.IsArray Then
                    callArgs.Add($"{nm}[{indexNames(0)}]")
                Else
                    callArgs.Add(nm)
                End If
            Next

            Dim writeIndex = If(mode = CudaIndexMode.Grid2D,
                                $"{indexNames(0)} * il_nCols + {indexNames(1)}",
                                indexNames(0))

            code.AppendLine($"    il_out[{writeIndex}] = {deviceName}({String.Join(", ", callArgs)});")
            code.AppendLine("}")

            Return code.ToString()
        End Function

        ''' <summary>该翻译是否走"逐元素包裹"（方法体不含任何数组取元素）</summary>
        Private Function IsScalarWrapped(syntax As MethodSyntax, mode As CudaIndexMode) As Boolean
            Return mode = CudaIndexMode.Grid1D AndAlso IsPureScalar(syntax.Body)
        End Function

        ''' <summary>形参在 AST / 生成代码里的名字（与 SSA 的命名规则保持一致）</summary>
        Private Function ParameterNameOf(p As ParameterDeclaration) As String
            Return If(String.IsNullOrEmpty(p.SsaName),
                      SsaBuilder.ParameterBaseName(p.Name),
                      p.SsaName)
        End Function

        ''' <summary>
        ''' 找出承担线程索引的形参名。
        ''' 优先读 <see cref="CudaIndexAttribute"/>；没有标注时按名字约定推断。
        ''' </summary>
        Private Function IndexParameterNames(syntax As MethodSyntax, mode As CudaIndexMode) As List(Of String)
            Dim result As New List(Of String)()

            If syntax.Method IsNot Nothing Then
                Dim parameters = syntax.Method.GetParameters()

                For i As Integer = 0 To parameters.Length - 1
                    Dim indexAttr = parameters(i).GetCustomAttribute(Of CudaIndexAttribute)()

                    If indexAttr IsNot Nothing Then
                        result.Add(SsaBuilder.ParameterBaseName(parameters(i).Name))
                    End If
                Next
            End If

            If result.Count > 0 Then
                If mode = CudaIndexMode.Grid2D AndAlso result.Count >= 2 Then
                    ' 保证顺序是 [行, 列]
                    Dim ordered As New List(Of String)()

                    For i As Integer = 0 To syntax.Parameters.Count - 1
                        Dim nm = ParameterNameOf(syntax.Parameters(i))
                        If result.Contains(nm) Then ordered.Add(nm)
                    Next

                    Return ordered
                End If

                Return result
            End If

            ' 按名字约定推断
            For Each p In syntax.Parameters
                Dim lower = If(p.Name, String.Empty).ToLowerInvariant()

                If mode = CudaIndexMode.Grid2D Then
                    If lower = "i" OrElse lower = "row" Then result.Add(ParameterNameOf(p))
                Else
                    If lower = "i" OrElse lower = "row" Then result.Add(ParameterNameOf(p))
                End If
            Next

            If mode = CudaIndexMode.Grid2D Then
                For Each p In syntax.Parameters
                    Dim lower = If(p.Name, String.Empty).ToLowerInvariant()
                    If lower = "j" OrElse lower = "col" Then result.Add(ParameterNameOf(p))
                Next
            End If

            If result.Count = 0 Then
                ' 逐元素包裹：索引只用于取数组元素，不进入标量函数
                result.Add("il_i")
            End If

            Return result
        End Function
    End Module
End Namespace
