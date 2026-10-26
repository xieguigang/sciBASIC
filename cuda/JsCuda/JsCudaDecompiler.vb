Imports System.Collections
Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports IL = Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.IL
Imports Microsoft.VisualBasic.Computing.ILCuda.IL2Cuda
Imports std = System.Math

' ---------------------------------------------------------------------------
'  JavaScript AST  ->  IL 风格 AST（MethodSyntax）
'
'  这一层只做"类型化"：JavaScript 是动态类型，而 CUDA 需要确定的数值类型。
'  翻译期的约定是：
'    * 线程索引参数（默认名字 i / j）        -> Integer
'    * 被下标访问的参数（arr[k]）            -> Double()   -> const double*
'    * 其余参数                              -> Double
'    * 局部变量                              -> 由初值表达式推断（整型字面量/整型运算为
'                                               Integer，其余为 Double，比较结果为 Boolean）
'
'  产物直接交给 ILCuda 的 CudaEmitter 发射成 C 代码，因此 IL 流水线与 JS 流水线
'  生成的设备代码风格完全一致（同一个 CudaTypeMap，同一套 System.Math 映射表）。
'
'  不支持的语法一律抛 JsCudaException（带函数名与源码位置），绝不生成可疑代码。
' ---------------------------------------------------------------------------

Namespace Global.Microsoft.VisualBasic.Computing.JSCuda

    ''' <summary>JavaScript function (AST) to typed IL-style AST, ready for the CUDA emitter.</summary>
    Public Class JsCudaDecompiler

        ' JavaScript operator -> IL operator. '**' and the JavaScript '/' semantics (always
        ' floating point) are handled separately.
        Private Shared ReadOnly CompareOperators As New Dictionary(Of String, IL.BinaryOperator) From {
            {"<", IL.BinaryOperator.LessThan},
            {"<=", IL.BinaryOperator.LessThanOrEqual},
            {">", IL.BinaryOperator.GreaterThan},
            {">=", IL.BinaryOperator.GreaterThanOrEqual},
            {"==", IL.BinaryOperator.Equal},
            {"===", IL.BinaryOperator.Equal},
            {"!=", IL.BinaryOperator.NotEqual},
            {"!==", IL.BinaryOperator.NotEqual}
        }

        Private Shared ReadOnly ArithmeticOperators As New Dictionary(Of String, IL.BinaryOperator) From {
            {"+", IL.BinaryOperator.Add},
            {"-", IL.BinaryOperator.Subtract},
            {"*", IL.BinaryOperator.Multiply},
            {"%", IL.BinaryOperator.Modulo},
            {"&", IL.BinaryOperator.BitwiseAnd},
            {"|", IL.BinaryOperator.BitwiseOr},
            {"^", IL.BinaryOperator.BitwiseXor},
            {"<<", IL.BinaryOperator.ShiftLeft},
            {">>", IL.BinaryOperator.ShiftRight}
        }

        ' JavaScript Math.* -> System.Math.*; the C name is then resolved by CudaTypeMap, so the
        ' math surface is exactly the one the IL pipeline already supports.
        Private Shared ReadOnly MathFunctions As New Dictionary(Of String, String) From {
            {"sqrt", "Sqrt"}, {"abs", "Abs"}, {"pow", "Pow"}, {"exp", "Exp"},
            {"log", "Log"}, {"log10", "Log10"}, {"min", "Min"}, {"max", "Max"},
            {"floor", "Floor"}, {"ceil", "Ceiling"}, {"round", "Round"}, {"trunc", "Truncate"},
            {"sin", "Sin"}, {"cos", "Cos"}, {"tan", "Tan"},
            {"asin", "Asin"}, {"acos", "Acos"}, {"atan", "Atan"}, {"atan2", "Atan2"},
            {"sinh", "Sinh"}, {"cosh", "Cosh"}, {"tanh", "Tanh"}
        }

        Private ReadOnly _options As JsKernelOptions
        Private ReadOnly _symbols As New Dictionary(Of String, Type)(StringComparer.Ordinal)
        Private ReadOnly _parameterNames As New HashSet(Of String)(StringComparer.Ordinal)
        Private ReadOnly _parameters As New List(Of JsKernelParam)
        Private ReadOnly _indexNames As New List(Of String)
        Private _functionName As String = ""
        Private _returnsValue As Boolean
        Private _ctxMode As Boolean

        Public Sub New(Optional options As JsKernelOptions = Nothing)
            _options = If(options, New JsKernelOptions())
        End Sub

        ''' <summary>
        ''' Translate one JavaScript function into a typed AST + kernel metadata.
        ''' </summary>
        Public Function Decompile(func As FuncExpr) As JsKernel
            If func Is Nothing Then
                Throw New JsCudaException("js->cuda: the given value is not a script function")
            End If

            _functionName = If(Not String.IsNullOrEmpty(func.Name), func.Name, If(func.InferredName, ""))

            If func.IsAsync OrElse func.IsGenerator Then
                Throw OfNode(func, "async and generator functions cannot run on the device")
            End If
            If func.RestParam IsNot Nothing Then
                Throw OfNode(func, "rest parameters (" & func.RestParam & ") are not supported")
            End If
            If func.Defaults IsNot Nothing Then
                For Each item In func.Defaults
                    If item IsNot Nothing Then
                        Throw OfNode(func, "default parameter values are not supported")
                    End If
                Next
            End If

            _ctxMode = func.Parameters.Length = 1 AndAlso
                       String.Equals(func.Parameters(0), _options.ContextParameter, StringComparison.Ordinal)

            If _ctxMode Then
                Call DeclareContextParameters(func.Body)
            Else
                Call DeclareSignatureParameters(func.Parameters, func.Body)
            End If

            Dim body As IL.BlockStatement = TranslateStatements(func.Body)
            Dim mode As CudaIndexMode = ResolveMode(body)
            Dim name As String = KernelName()

            Return New JsKernel With {
                .JsName = _functionName,
                .KernelName = "js_" & name & "_kernel",
                .DeviceFunctionName = "js_" & name & "_scalar",
                .Mode = mode,
                .IndexNames = _indexNames.ToArray,
                .Parameters = _parameters,
                .ReturnsValue = _returnsValue,
                .ElementWise = mode = CudaIndexMode.Grid1D AndAlso _indexNames.Count = 0,
                .Syntax = BuildSyntax(name, body)
            }
        End Function

        ' ======================================================== parameters

        Private Sub DeclareSignatureParameters(names As String(), body As List(Of Statement))
            For Each name As String In names
                If String.IsNullOrEmpty(name) Then
                    Throw New JsCudaException($"js->cuda: function '{_functionName}': a parameter has no name")
                End If
                If _symbols.ContainsKey(name) Then
                    Throw New JsCudaException(
                        $"js->cuda: function '{_functionName}': duplicate parameter '{name}'")
                End If

                Dim type As Type

                If _options.IndexRank(name) >= 0 Then
                    type = GetType(Integer)
                    Call _indexNames.Add(name)
                ElseIf UsesAsBuffer(body, name) Then
                    type = GetType(Double())
                    Call _parameters.Add(New JsKernelParam(name, JsKernelParamKind.Buffer))
                Else
                    type = GetType(Double)
                    Call _parameters.Add(New JsKernelParam(name, JsKernelParamKind.Number))
                End If

                _symbols(name) = type
                Call _parameterNames.Add(name)
            Next

            Call OrderIndexNames()
        End Sub

        ''' <summary>
        ''' <c>function cell(ctx) { ... ctx.i ... ctx.dot[k] ... }</c>: the parameters are the
        ''' <c>ctx</c> members the body actually uses, in order of appearance.
        ''' </summary>
        Private Sub DeclareContextParameters(body As List(Of Statement))
            Dim members As New List(Of String)
            Dim indexed As New HashSet(Of String)(StringComparer.Ordinal)

            Call CollectContextMembers(body, members, indexed)

            If members.Count = 0 Then
                Throw New JsCudaException(
                    $"js->cuda: function '{_functionName}': the context parameter '{_options.ContextParameter}' is never used")
            End If

            For Each name As String In members
                Dim type As Type

                If _options.IndexRank(name) >= 0 Then
                    type = GetType(Integer)
                    Call _indexNames.Add(name)
                ElseIf indexed.Contains(name) Then
                    type = GetType(Double())
                Else
                    type = GetType(Double)
                End If

                _symbols(name) = type
                Call _parameterNames.Add(name)
            Next

            ' the kernel argument order is deterministic: buffers first, then scalars, each group in
            ' order of appearance — so a context kernel has a readable, documented calling
            ' convention (see JsCudaHost.parameterNames)
            For Each name As String In members
                If _symbols(name) Is GetType(Double()) Then
                    Call _parameters.Add(New JsKernelParam(name, JsKernelParamKind.Buffer))
                End If
            Next

            For Each name As String In members
                If _symbols(name) Is GetType(Double) Then
                    Call _parameters.Add(New JsKernelParam(name, JsKernelParamKind.Number))
                End If
            Next

            Call OrderIndexNames()
        End Sub

        Private Sub OrderIndexNames()
            If _indexNames.Count <= 1 Then Return

            ' the launch order is defined by the options (row first, then column), not by the
            ' order the names happen to appear in the script
            Dim ordered = _indexNames.
                OrderBy(Function(name) _options.IndexRank(name)).
                ToArray()

            Call _indexNames.Clear()
            Call _indexNames.AddRange(ordered)
        End Sub

        ''' <summary>Is <paramref name="name"/> indexed somewhere in the body (<c>name[...]</c>)?</summary>
        Private Function UsesAsBuffer(body As List(Of Statement), name As String) As Boolean
            Dim found As Boolean = False

            Call WalkAll(body,
                Sub(node)
                    If found Then Return

                    Dim member = TryCast(node, MemberExpr)

                    If member Is Nothing OrElse Not member.Computed Then Return

                    Dim target = TryCast(member.Obj, IdentExpr)

                    If target IsNot Nothing AndAlso String.Equals(target.Name, name, StringComparison.Ordinal) Then
                        found = True
                    End If
                End Sub)

            Return found
        End Function

        ''' <summary>Collect the <c>ctx.member</c> usages of a context style kernel, in order.</summary>
        Private Sub CollectContextMembers(body As List(Of Statement),
                                          members As List(Of String),
                                          indexed As HashSet(Of String))
            Call WalkAll(body,
                Sub(node)
                    Dim member = TryCast(node, MemberExpr)

                    If member Is Nothing Then Return

                    ' ctx.arr[k] is a computed member whose Obj is the ctx.arr member
                    Dim ctxMember As MemberExpr = If(member.Computed, TryCast(member.Obj, MemberExpr), member)

                    If ctxMember Is Nothing OrElse ctxMember.Computed Then Return

                    Dim context = TryCast(ctxMember.Obj, IdentExpr)

                    If context Is Nothing Then Return
                    If Not String.Equals(context.Name, _options.ContextParameter, StringComparison.Ordinal) Then Return

                    Dim name = TryCast(ctxMember.Name, String)

                    If String.IsNullOrEmpty(name) Then Return

                    If member.Computed Then Call indexed.Add(name)

                    If Not members.Contains(name) Then
                        Call members.Add(name)
                    End If
                End Sub)
        End Sub

        ' ======================================================== mode

        Private Function ResolveMode(body As IL.BlockStatement) As CudaIndexMode
            If _indexNames.Count >= 2 Then Return CudaIndexMode.Grid2D
            If _indexNames.Count = 1 Then Return CudaIndexMode.Grid1D

            ' no thread index parameter: a pure scalar formula is wrapped element-wise (every
            ' scalar parameter is then read at the thread index), anything that touches a buffer
            ' without an index cannot be launched at all
            If ContainsArrayIndex(body) Then Return CudaIndexMode.None

            Return CudaIndexMode.Grid1D
        End Function

        Private Function ContainsArrayIndex(node As IL.SyntaxNode) As Boolean
            If node Is Nothing Then Return False
            If TypeOf node Is IL.ArrayIndexExpression Then Return True

            Select Case node.Kind
                Case IL.SyntaxKind.Block
                    For Each statement In DirectCast(node, IL.BlockStatement).Statements
                        If ContainsArrayIndex(statement) Then Return True
                    Next
                Case IL.SyntaxKind.VariableDeclaration
                    Return ContainsArrayIndex(DirectCast(node, IL.VariableDeclarationStatement).Initializer)
                Case IL.SyntaxKind.Assignment
                    Dim assign = DirectCast(node, IL.AssignmentStatement)
                    Return ContainsArrayIndex(assign.Target) OrElse ContainsArrayIndex(assign.Value)
                Case IL.SyntaxKind.ExpressionStatement
                    Return ContainsArrayIndex(DirectCast(node, IL.ExpressionStatement).Value)
                Case IL.SyntaxKind.ReturnStmt
                    Return ContainsArrayIndex(DirectCast(node, IL.ReturnStatement).Value)
                Case IL.SyntaxKind.IfStmt
                    Dim [if] = DirectCast(node, IL.IfStatement)
                    Return ContainsArrayIndex([if].Condition) OrElse
                           ContainsArrayIndex([if].ThenBody) OrElse
                           ContainsArrayIndex([if].ElseBody)
                Case IL.SyntaxKind.WhileStmt
                    Dim loopStmt = DirectCast(node, IL.WhileStatement)
                    Return ContainsArrayIndex(loopStmt.Condition) OrElse ContainsArrayIndex(loopStmt.Body)
                Case IL.SyntaxKind.ForStmt
                    Dim loopStmt = DirectCast(node, IL.ForStatement)
                    Return ContainsArrayIndex(loopStmt.Condition) OrElse ContainsArrayIndex(loopStmt.Body)
                Case IL.SyntaxKind.Binary
                    Dim bin = DirectCast(node, IL.BinaryExpression)
                    Return ContainsArrayIndex(bin.Left) OrElse ContainsArrayIndex(bin.Right)
                Case IL.SyntaxKind.Unary
                    Return ContainsArrayIndex(DirectCast(node, IL.UnaryExpression).Operand)
                Case IL.SyntaxKind.Convert
                    Return ContainsArrayIndex(DirectCast(node, IL.ConvertExpression).Operand)
                Case IL.SyntaxKind.Ternary
                    Dim ternary = DirectCast(node, IL.TernaryExpression)
                    Return ContainsArrayIndex(ternary.Condition) OrElse
                           ContainsArrayIndex(ternary.WhenTrue) OrElse
                           ContainsArrayIndex(ternary.WhenFalse)
                Case IL.SyntaxKind.Invoke
                    For Each arg In DirectCast(node, IL.CallExpression).AllArguments
                        If ContainsArrayIndex(arg) Then Return True
                    Next
            End Select

            Return False
        End Function

        ' ======================================================== statements

        Private Function TranslateStatements(body As List(Of Statement)) As IL.BlockStatement
            Dim result As New IL.BlockStatement()

            If body Is Nothing Then Return result

            For Each statement In body
                Dim translated = TranslateStatement(statement)

                If translated IsNot Nothing Then
                    Call result.Statements.Add(translated)
                End If
            Next

            Return result
        End Function

        Private Function TranslateStatement(node As Statement) As IL.Statement
            If node Is Nothing Then Return New IL.NopStatement()

            If TypeOf node Is BlockStmt Then
                Return TranslateStatements(DirectCast(node, BlockStmt).Body)
            ElseIf TypeOf node Is VarStmt Then
                Return TranslateVariable(DirectCast(node, VarStmt))
            ElseIf TypeOf node Is ExprStmt Then
                Return TranslateExpressionStatement(DirectCast(node, ExprStmt))
            ElseIf TypeOf node Is ReturnStmt Then
                Dim returnNode = DirectCast(node, ReturnStmt)

                If returnNode.Value IsNot Nothing Then _returnsValue = True

                Return New IL.ReturnStatement(
                    If(returnNode.Value Is Nothing, Nothing, TranslateExpression(returnNode.Value)))
            ElseIf TypeOf node Is IfStmt Then
                Dim ifNode = DirectCast(node, IfStmt)

                Return New IL.IfStatement(
                    TranslateExpression(ifNode.Test),
                    AsBlock(ifNode.ThenBranch),
                    If(ifNode.ElseBranch Is Nothing, Nothing, AsBlock(ifNode.ElseBranch)))
            ElseIf TypeOf node Is WhileStmt Then
                Dim loopStmt = DirectCast(node, WhileStmt)

                Return New IL.WhileStatement(TranslateExpression(loopStmt.Test), AsBlock(loopStmt.Body), False)
            ElseIf TypeOf node Is DoWhileStmt Then
                Dim loopStmt = DirectCast(node, DoWhileStmt)

                Return New IL.WhileStatement(TranslateExpression(loopStmt.Test), AsBlock(loopStmt.Body), True)
            ElseIf TypeOf node Is ForStmt Then
                Return TranslateFor(DirectCast(node, ForStmt))
            ElseIf TypeOf node Is EmptyStmt Then
                Return New IL.NopStatement()
            ElseIf TypeOf node Is BreakStmt OrElse TypeOf node Is ContinueStmt Then
                Throw OfNode(node, "break / continue is not supported inside a GPU kernel")
            ElseIf TypeOf node Is ThrowStmt Then
                Throw OfNode(node, "throw is not supported inside a GPU kernel")
            ElseIf TypeOf node Is TryStmt Then
                Throw OfNode(node, "try / catch is not supported inside a GPU kernel")
            ElseIf TypeOf node Is SwitchStmt Then
                Throw OfNode(node, "switch is not supported inside a GPU kernel, use if / else")
            ElseIf TypeOf node Is ForOfStmt OrElse TypeOf node Is ForInStmt Then
                Throw OfNode(node, "for..of / for..in loops are not supported inside a GPU kernel")
            ElseIf TypeOf node Is FuncDeclStmt Then
                Throw OfNode(node, "nested function declarations are not supported inside a GPU kernel")
            Else
                Throw OfNode(node, "statement " & node.GetType().Name & " cannot run on the device")
            End If
        End Function

        Private Function AsBlock(node As Statement) As IL.BlockStatement
            If node Is Nothing Then Return New IL.BlockStatement()

            Dim translated = TranslateStatement(node)
            Dim block = TryCast(translated, IL.BlockStatement)

            If block IsNot Nothing Then Return block

            Dim wrapped As New IL.BlockStatement()
            Call wrapped.Statements.Add(translated)

            Return wrapped
        End Function

        Private Function TranslateVariable(node As VarStmt) As IL.Statement
            Dim translated As New IL.BlockStatement()

            For Each declarant In node.Declarations
                Dim name = TryCast(declarant.Target, String)

                If name Is Nothing Then
                    Throw OfNode(node, "destructuring declarations are not supported inside a GPU kernel")
                End If

                ' JavaScript numbers are doubles: a local must stay double even when it is
                ' initialised with an integral literal, otherwise "let s = 0; s = s + x[k]"
                ' would truncate every fractional value (the classic silent-wrong-result bug).
                ' Subscripts are cast to int at the point of use instead.
                Dim inferred As Type = If(declarant.Init Is Nothing, GetType(Double), InferType(declarant.Init))

                If inferred Is GetType(Double()) Then
                    Throw OfNode(node, $"'{name}' is assigned a buffer, which is not supported")
                End If

                Dim type As Type = GetType(Double)

                _symbols(name) = type
                Call translated.Statements.Add(
                    New IL.VariableDeclarationStatement(
                        name, type,
                        If(declarant.Init Is Nothing, Nothing, TranslateExpression(declarant.Init))))
            Next

            If translated.Statements.Count = 0 Then Return New IL.NopStatement()
            If translated.Statements.Count = 1 Then Return translated.Statements(0)

            Return translated
        End Function

        Private Function TranslateExpressionStatement(node As ExprStmt) As IL.Statement
            Dim expr = node.Expr

            If TypeOf expr Is AssignExpr Then
                Return TranslateAssignment(DirectCast(expr, AssignExpr))
            ElseIf TypeOf expr Is UpdateExpr Then
                Dim update = DirectCast(expr, UpdateExpr)
                Dim target = VariableRef(update.Target)
                Dim targetType As Type = ExpressionTypeOf(target)

                Return New IL.AssignmentStatement(
                    target,
                    New IL.BinaryExpression(
                        If(update.Op = "++", IL.BinaryOperator.Add, IL.BinaryOperator.Subtract),
                        target, One(targetType), targetType))
            End If

            Throw OfNode(node, "only assignments may be used as a statement inside a GPU kernel")
        End Function

        Private Function TranslateAssignment(node As AssignExpr) As IL.Statement
            Dim target As IL.Expression = Nothing
            Dim isBufferCell As Boolean = False

            If TypeOf node.Target Is IdentExpr Then
                target = VariableRef(node.Target)
            Else
                Dim member = TryCast(node.Target, MemberExpr)

                If member Is Nothing OrElse Not member.Computed Then
                    Throw OfNode(node, "unsupported assignment target")
                End If

                Dim bufferName As String = Nothing

                If Not TryBufferReference(member.Obj, bufferName) Then
                    Throw OfNode(node, "unsupported assignment target")
                End If

                Dim cellIndex = TryCast(member.Name, Expression)

                If cellIndex Is Nothing Then
                    Throw OfNode(node, "a computed member name is not supported")
                End If

                target = New IL.ArrayIndexExpression(
                    ParameterRef(bufferName), AsIntegerIndex(cellIndex), GetType(Double))
                isBufferCell = True
            End If

            Dim cell As IL.Expression

            Select Case node.Op
                Case "="
                    cell = TranslateExpression(node.Value)
                Case "+=", "-=", "*=", "/="
                    cell = Binary(ArithmeticOperators(node.Op.Substring(0, 1)),
                                  target,
                                  TranslateExpression(node.Value))
                Case Else
                    Throw OfNode(node, "assignment operator '" & node.Op & "' is not supported")
            End Select

            If isBufferCell Then
                ' a buffer write is a statement of its own; the cell always holds a double
                Return New IL.AssignmentStatement(target, ToDouble(cell))
            End If

            Return New IL.AssignmentStatement(target, cell)
        End Function

        Private Function TranslateFor(node As ForStmt) As IL.Statement
            Dim loopVariable As String = Nothing
            Dim initial As IL.Expression = Nothing
            Dim prelude As New IL.BlockStatement()

            If TypeOf node.Init Is VarStmt Then
                Dim declaration = DirectCast(node.Init, VarStmt)
                Dim declarant = declaration.Declarations(0)

                loopVariable = TryCast(declarant.Target, String)

                If loopVariable Is Nothing Then
                    Throw OfNode(node, "for-loop destructuring is not supported")
                End If

                Dim loopType As Type = If(declarant.Init Is Nothing, GetType(Integer), InferType(declarant.Init))

                _symbols(loopVariable) = loopType
                initial = TranslateExpression(declarant.Init)
                Call prelude.Statements.Add(
                    New IL.VariableDeclarationStatement(loopVariable, loopType, initial))
            ElseIf TypeOf node.Init Is AssignExpr Then
                Dim assign = DirectCast(node.Init, AssignExpr)
                Dim ident = TryCast(assign.Target, IdentExpr)

                If ident Is Nothing Then Throw OfNode(node, "unsupported for-loop initializer")

                loopVariable = ident.Name
                initial = TranslateExpression(assign.Value)

                If Not _symbols.ContainsKey(loopVariable) Then
                    Throw OfNode(node, $"the for-loop variable '{loopVariable}' is not declared")
                End If
            Else
                Throw OfNode(node, "a for loop needs an initializer to be translated")
            End If

            Dim condition = TranslateExpression(node.Test)
            Dim loopVarType As Type = _symbols(loopVariable)
            Dim increment As IL.Expression

            If TypeOf node.Update Is UpdateExpr Then
                Dim update = DirectCast(node.Update, UpdateExpr)

                increment = New IL.BinaryExpression(
                    If(update.Op = "++", IL.BinaryOperator.Add, IL.BinaryOperator.Subtract),
                    VariableRefByName(loopVariable), One(loopVarType), loopVarType)
            ElseIf TypeOf node.Update Is AssignExpr Then
                Dim assign = DirectCast(node.Update, AssignExpr)

                Select Case assign.Op
                    Case "="
                        increment = TranslateExpression(assign.Value)
                    Case "+=", "-=", "*=", "/="
                        increment = Binary(ArithmeticOperators(assign.Op.Substring(0, 1)),
                                           VariableRefByName(loopVariable),
                                           TranslateExpression(assign.Value))
                    Case Else
                        Throw OfNode(node, "for-loop update operator '" & assign.Op & "' is not supported")
                End Select
            Else
                Throw OfNode(node, "a for loop needs a ++/-- or += update to be translated")
            End If

            Call prelude.Statements.Add(
                New IL.ForStatement With {
                    .VariableName = loopVariable,
                    .Initializer = initial,
                    .Condition = condition,
                    .Increment = increment,
                    .Body = AsBlock(node.Body)
                })

            Return prelude
        End Function

        ' ======================================================== expressions

        Private Function TranslateExpression(node As Expression) As IL.Expression
            If node Is Nothing Then Return Number(0.0)

            If TypeOf node Is LiteralExpr Then Return TranslateLiteral(DirectCast(node, LiteralExpr))
            If TypeOf node Is IdentExpr Then Return TranslateIdent(DirectCast(node, IdentExpr))
            If TypeOf node Is BinaryExpr Then Return TranslateBinary(DirectCast(node, BinaryExpr))
            If TypeOf node Is LogicalExpr Then Return TranslateLogical(DirectCast(node, LogicalExpr))
            If TypeOf node Is UnaryExpr Then Return TranslateUnary(DirectCast(node, UnaryExpr))
            If TypeOf node Is CondExpr Then Return TranslateConditional(DirectCast(node, CondExpr))
            If TypeOf node Is MemberExpr Then Return TranslateMember(DirectCast(node, MemberExpr))
            If TypeOf node Is CallExpr Then Return TranslateCall(DirectCast(node, CallExpr))

            If TypeOf node Is UpdateExpr Then
                Throw OfNode(node, "increment / decrement may only be used as a statement or a for-loop update")
            End If
            If TypeOf node Is AssignExpr Then
                Throw OfNode(node, "an assignment may only be used as a statement")
            End If

            Throw OfNode(node, "expression " & node.GetType().Name & " cannot run on the device")
        End Function

        Private Function TranslateLiteral(node As LiteralExpr) As IL.Expression
            Dim item = node.Value

            If item.IsBoolean Then Return Number(If(item.AsBoolean, 1.0, 0.0))

            If item.IsNumber Then
                If item.VarType = TypeCode.Int32 Then
                    Return New IL.LiteralExpression(item.AsInt32, GetType(Integer))
                End If

                ' a JavaScript number is a double, but an integral one prints as the very same C
                ' integer literal ("3" not "3.0"), which is what keeps subscripts and loop counters
                ' valid device code
                If IsIntegralLiteral(item.AsDouble) Then
                    Return New IL.LiteralExpression(CInt(item.AsDouble), GetType(Integer))
                End If

                Return Number(item.AsDouble)
            End If

            Throw OfNode(node, "only numeric literals are supported inside a GPU kernel")
        End Function

        Private Shared Function IsIntegralLiteral(value As Double) As Boolean
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Return False
            If std.Abs(value) > Integer.MaxValue Then Return False

            Return std.Floor(value) = value
        End Function

        ''' <summary>Does this expression yield a value that prints as a C integer literal?</summary>
        Private Function IsIntegralOperand(node As Expression) As Boolean
            If node Is Nothing Then Return False

            If TypeOf node Is LiteralExpr Then
                Dim item = DirectCast(node, LiteralExpr).Value

                If item.IsBoolean Then Return True
                If Not item.IsNumber Then Return False

                Return item.VarType = TypeCode.Int32 OrElse IsIntegralLiteral(item.AsDouble)
            End If

            If TypeOf node Is IdentExpr Then
                Dim name = DirectCast(node, IdentExpr).Name

                Return _symbols.ContainsKey(name) AndAlso _symbols(name) Is GetType(Integer)
            End If

            If TypeOf node Is UnaryExpr Then
                Dim unary = DirectCast(node, UnaryExpr)

                If unary.Op = "-" OrElse unary.Op = "+" Then Return IsIntegralOperand(unary.Operand)

                Return False
            End If

            If TypeOf node Is BinaryExpr Then
                Dim bin = DirectCast(node, BinaryExpr)

                If bin.Op = "/" OrElse bin.Op = "**" Then Return False
                If CompareOperators.ContainsKey(bin.Op) Then Return False

                Return IsIntegralOperand(bin.Left) AndAlso IsIntegralOperand(bin.Right)
            End If

            If TypeOf node Is CondExpr Then
                Dim cond = DirectCast(node, CondExpr)

                Return IsIntegralOperand(cond.Consequent) AndAlso IsIntegralOperand(cond.Alternate)
            End If

            Return False
        End Function

        Private Function TranslateIdent(node As IdentExpr) As IL.Expression
            If Not _symbols.ContainsKey(node.Name) Then
                Throw OfNode(node,
                    $"unknown identifier '{node.Name}': a GPU kernel may only use its parameters and local variables")
            End If

            Return VariableRefByName(node.Name)
        End Function

        Private Function TranslateBinary(node As BinaryExpr) As IL.Expression
            If node.Op = "**" Then
                ' JavaScript has no integer power: always the double version
                Return New IL.CallExpression With {
                    .DeclaringTypeName = "System.Math",
                    .MethodName = "Pow",
                    .IsStaticCall = True,
                    .Type = GetType(Double),
                    .Arguments = New List(Of IL.Expression) From {
                        TranslateExpression(node.Left),
                        TranslateExpression(node.Right)
                    }
                }
            End If

            If node.Op = "/" Then
                ' JavaScript division is always floating point, C integer division is not
                Return New IL.BinaryExpression(
                    IL.BinaryOperator.Divide,
                    ToDouble(TranslateExpression(node.Left)),
                    ToDouble(TranslateExpression(node.Right)),
                    GetType(Double))
            End If

            If node.Op = "%" Then
                If Not IsIntegralOperand(node.Left) OrElse Not IsIntegralOperand(node.Right) Then
                    Throw OfNode(node,
                        "the modulo operator is only supported on integer operands on the device; " &
                        "use x - floor(x / n) * n for floating point values")
                End If
            End If

            If CompareOperators.ContainsKey(node.Op) Then
                Return New IL.BinaryExpression(
                    CompareOperators(node.Op),
                    TranslateExpression(node.Left),
                    TranslateExpression(node.Right),
                    GetType(Boolean))
            End If

            If ArithmeticOperators.ContainsKey(node.Op) Then
                Return Binary(ArithmeticOperators(node.Op),
                              TranslateExpression(node.Left),
                              TranslateExpression(node.Right))
            End If

            Throw OfNode(node, "operator '" & node.Op & "' cannot run on the device")
        End Function

        Private Function TranslateLogical(node As LogicalExpr) As IL.Expression
            Dim op As IL.BinaryOperator

            Select Case node.Op
                Case "&&" : op = IL.BinaryOperator.ShortCircuitAnd
                Case "||" : op = IL.BinaryOperator.ShortCircuitOr
                Case Else
                    Throw OfNode(node, "logical operator '" & node.Op & "' cannot run on the device")
            End Select

            Return New IL.BinaryExpression(
                op, TranslateExpression(node.Left), TranslateExpression(node.Right), GetType(Boolean))
        End Function

        Private Function TranslateUnary(node As UnaryExpr) As IL.Expression
            Select Case node.Op
                Case "-"
                    Return New IL.UnaryExpression(
                        IL.UnaryOperator.Negate, TranslateExpression(node.Operand), InferType(node.Operand))
                Case "+"
                    Return TranslateExpression(node.Operand)
                Case "!"
                    Return New IL.UnaryExpression(
                        IL.UnaryOperator.LogicalNot, TranslateExpression(node.Operand), GetType(Boolean))
                Case Else
                    Throw OfNode(node, "unary operator '" & node.Op & "' cannot run on the device")
            End Select
        End Function

        Private Function TranslateConditional(node As CondExpr) As IL.Expression
            Dim type As Type = InferType(node.Consequent)

            If InferType(node.Alternate) IsNot type Then type = GetType(Double)

            Return New IL.TernaryExpression(
                TranslateExpression(node.Test),
                TranslateExpression(node.Consequent),
                TranslateExpression(node.Alternate),
                type)
        End Function

        Private Function TranslateMember(node As MemberExpr) As IL.Expression
            If node.Computed Then
                Dim bufferName As String = Nothing

                If TryBufferReference(node.Obj, bufferName) Then
                    Dim cellIndex = TryCast(node.Name, Expression)

                    If cellIndex Is Nothing Then
                        Throw OfNode(node, "a computed member name is not supported")
                    End If

                    Return New IL.ArrayIndexExpression(
                        ParameterRef(bufferName), AsIntegerIndex(cellIndex), GetType(Double))
                End If

                Throw OfNode(node, "computed member access is only supported as buffer[index]")
            End If

            Dim memberName = TryCast(node.Name, String)

            If memberName Is Nothing Then
                Throw OfNode(node, "a computed member name is not supported")
            End If

            If String.Equals(memberName, "length", StringComparison.Ordinal) Then
                Throw OfNode(node,
                    "buffer.length is not available on the device: pass the length as an integer parameter")
            End If

            If IsContextMember(node, memberName) Then
                If Not _symbols.ContainsKey(memberName) Then
                    Throw OfNode(node, $"unknown context member '{memberName}'")
                End If

                Return VariableRefByName(memberName)
            End If

            Throw OfNode(node,
                "only Math.* and the context (ctx.*) members are available inside a GPU kernel")
        End Function

        Private Function TranslateCall(node As CallExpr) As IL.Expression
            Dim member = TryCast(node.Callee, MemberExpr)

            If member Is Nothing OrElse member.Computed Then
                Throw OfNode(node,
                    "calling a script function inside a GPU kernel is not supported; " &
                    "only the Math.* functions are available")
            End If

            Dim owner = TryCast(member.Obj, IdentExpr)
            Dim methodName = TryCast(member.Name, String)

            If owner Is Nothing OrElse methodName Is Nothing Then
                Throw OfNode(node, "unsupported call target")
            End If

            If Not String.Equals(owner.Name, "Math", StringComparison.Ordinal) Then
                Throw OfNode(node,
                    $"'{owner.Name}.{methodName}' is not available on the device; " &
                    "only the Math.* functions are supported")
            End If

            Dim netName As String = Nothing

            If Not MathFunctions.TryGetValue(methodName.ToLowerInvariant(), netName) Then
                Throw OfNode(node,
                    $"Math.{methodName} has no CUDA mapping; supported: " &
                    String.Join(", ", MathFunctions.Keys.OrderBy(Function(k) k)))
            End If

            Dim cudaName As String = Nothing

            If Not CudaTypeMap.TryMapMathFunction("System.Math", netName, False, cudaName) Then
                Throw OfNode(node, $"Math.{methodName} has no CUDA mapping")
            End If

            Dim callArgs As New List(Of IL.Expression)()

            For Each argExpr In node.Arguments
                Call callArgs.Add(TranslateExpression(argExpr))
            Next

            Return New IL.CallExpression With {
                .DeclaringTypeName = "System.Math",
                .MethodName = netName,
                .IsStaticCall = True,
                .Type = GetType(Double),
                .Arguments = callArgs
            }
        End Function

        ' ======================================================== type inference

        Private Function InferType(node As Expression) As Type
            If node Is Nothing Then Return GetType(Double)

            If TypeOf node Is LiteralExpr Then
                Dim literal = DirectCast(node, LiteralExpr)

                If literal.Value.IsBoolean Then Return GetType(Boolean)
                If literal.Value.VarType = TypeCode.Int32 Then Return GetType(Integer)
                If IsIntegralLiteral(literal.Value.AsDouble) Then Return GetType(Integer)

                Return GetType(Double)
            End If

            If TypeOf node Is IdentExpr Then
                Dim name = DirectCast(node, IdentExpr).Name

                Return If(_symbols.ContainsKey(name), _symbols(name), GetType(Double))
            End If

            If TypeOf node Is BinaryExpr Then
                Dim bin = DirectCast(node, BinaryExpr)

                If CompareOperators.ContainsKey(bin.Op) Then Return GetType(Boolean)
                If bin.Op = "/" OrElse bin.Op = "**" Then Return GetType(Double)

                If InferType(bin.Left) Is GetType(Integer) AndAlso InferType(bin.Right) Is GetType(Integer) Then
                    Return GetType(Integer)
                End If

                Return GetType(Double)
            End If

            If TypeOf node Is LogicalExpr Then Return GetType(Boolean)

            If TypeOf node Is UnaryExpr Then
                Dim unary = DirectCast(node, UnaryExpr)

                If unary.Op = "!" Then Return GetType(Boolean)
                If unary.Op = "-" OrElse unary.Op = "+" Then Return InferType(unary.Operand)

                Return GetType(Double)
            End If

            If TypeOf node Is CondExpr Then
                Dim cond = DirectCast(node, CondExpr)
                Dim leftType = InferType(cond.Consequent)

                Return If(leftType Is InferType(cond.Alternate), leftType, GetType(Double))
            End If

            If TypeOf node Is MemberExpr Then
                Dim member = DirectCast(node, MemberExpr)

                If member.Computed Then Return GetType(Double)

                Dim name = TryCast(member.Name, String)

                Return If(name IsNot Nothing AndAlso _symbols.ContainsKey(name), _symbols(name), GetType(Double))
            End If

            If TypeOf node Is CallExpr Then Return GetType(Double)
            If TypeOf node Is UpdateExpr Then Return InferType(DirectCast(node, UpdateExpr).Target)

            Return GetType(Double)
        End Function

        ' ======================================================== helpers

        Private Function KernelName() As String
            Dim name = If(String.IsNullOrEmpty(_options.Name), _functionName, _options.Name)

            If String.IsNullOrWhiteSpace(name) Then name = "kernel"

            Dim buffer As New System.Text.StringBuilder()

            For Each ch As Char In name
                If Char.IsLetterOrDigit(ch) OrElse ch = "_"c Then
                    Call buffer.Append(ch)
                Else
                    Call buffer.Append("_"c)
                End If
            Next

            Dim text = buffer.ToString()

            If Char.IsDigit(text(0)) Then text = "k_" & text

            Return text
        End Function

        Private Function BuildSyntax(name As String, body As IL.BlockStatement) As IL.MethodSyntax
            Dim syntax As New IL.MethodSyntax With {
                .Name = name,
                .IsStatic = True,
                .ReturnType = If(_returnsValue, GetType(Double), GetType(Void)),
                .Body = body
            }

            Dim index As Integer = 0

            For Each item As String In _indexNames
                Call syntax.Parameters.Add(
                    New IL.ParameterDeclaration(index, item, GetType(Integer), item))
                index += 1
            Next

            For Each parameter In _parameters
                Dim type As Type = GetType(Double)

                If parameter.Kind = JsKernelParamKind.Buffer Then type = GetType(Double())
                If parameter.Kind = JsKernelParamKind.Int32 Then type = GetType(Integer)

                Call syntax.Parameters.Add(
                    New IL.ParameterDeclaration(index, parameter.Name, type, parameter.Name))
                index += 1
            Next

            Return syntax
        End Function

        Private Function VariableRef(node As Expression) As IL.Expression
            Dim ident = TryCast(node, IdentExpr)

            If ident Is Nothing Then
                Throw OfNode(node, "unsupported assignment target")
            End If

            If Not _symbols.ContainsKey(ident.Name) Then
                Throw OfNode(node, $"unknown variable '{ident.Name}'")
            End If

            Return VariableRefByName(ident.Name)
        End Function

        Private Function VariableRefByName(name As String) As IL.Expression
            Dim type As Type = _symbols(name)

            If type Is GetType(Double()) Then
                Throw New JsCudaException(
                    $"js->cuda: function '{_functionName}': the buffer '{name}' must be indexed: {name}[index]")
            End If

            If _parameterNames.Contains(name) Then
                Return New IL.ParameterExpression(0, name, type)
            End If

            Return New IL.LocalExpression(0, name, type)
        End Function

        Private Function ParameterRef(name As String) As IL.Expression
            Return New IL.ParameterExpression(0, name, GetType(Double()))
        End Function

        Private Function TryBufferReference(node As Expression, ByRef name As String) As Boolean
            name = Nothing

            Dim ident = TryCast(node, IdentExpr)

            If ident IsNot Nothing Then
                If _symbols.ContainsKey(ident.Name) AndAlso _symbols(ident.Name) Is GetType(Double()) Then
                    name = ident.Name
                    Return True
                End If

                Return False
            End If

            ' ctx mode: ctx.arr[k]
            Dim member = TryCast(node, MemberExpr)

            If member Is Nothing OrElse member.Computed Then Return False

            Dim memberName = TryCast(member.Name, String)

            If memberName Is Nothing OrElse Not IsContextMember(member, memberName) Then Return False
            If Not _symbols.ContainsKey(memberName) Then Return False
            If _symbols(memberName) IsNot GetType(Double()) Then Return False

            name = memberName

            Return True
        End Function

        Private Function IsContextMember(member As MemberExpr, name As String) As Boolean
            If Not _ctxMode Then Return False

            Dim owner = TryCast(member.Obj, IdentExpr)

            If owner Is Nothing Then Return False

            Return String.Equals(owner.Name, _options.ContextParameter, StringComparison.Ordinal)
        End Function

        Private Function AsIntegerIndex(node As Expression) As IL.Expression
            Dim index = TranslateExpression(node)

            If InferType(node) Is GetType(Integer) Then Return index

            Return New IL.ConvertExpression(index, GetType(Integer))
        End Function

        Private Function ToDouble(node As IL.Expression) As IL.Expression
            If node Is Nothing Then Return Nothing
            If node.Type Is GetType(Double) Then Return node

            Return New IL.ConvertExpression(node, GetType(Double))
        End Function

        Private Function Binary(op As IL.BinaryOperator,
                               left As IL.Expression,
                               right As IL.Expression) As IL.Expression
            Dim type As Type = If(left.Type Is GetType(Integer) AndAlso right.Type Is GetType(Integer),
                                  GetType(Integer), GetType(Double))

            Return New IL.BinaryExpression(op, left, right, type)
        End Function

        Private Function Number(value As Double) As IL.Expression
            Return New IL.LiteralExpression(value, GetType(Double))
        End Function

        Private Function One(type As Type) As IL.Expression
            If type Is GetType(Integer) Then Return New IL.LiteralExpression(1, GetType(Integer))

            Return Number(1.0)
        End Function

        Private Function ExpressionTypeOf(node As IL.Expression) As Type
            If node Is Nothing OrElse node.Type Is Nothing Then Return GetType(Double)

            Return node.Type
        End Function

        Private Function OfNode(node As Object, message As String) As JsCudaException
            Return JsCudaException.OfNode(_functionName, node, message)
        End Function

        ' ---------------- AST walking (diagnostics only) ----------------

        ''' <summary>
        ''' Visit every JavaScript expression reachable from <paramref name="body"/>. Reflection
        ''' based: the AST has many node types and every new node kind must not silently escape the
        ''' parameter analysis (a missed node would mean a mis-typed parameter).
        ''' </summary>
        Private Shared Sub WalkAll(body As List(Of Statement), visit As Action(Of Expression))
            If body Is Nothing Then Return

            For Each statement In body
                Call WalkObject(statement, visit, 0)
            Next
        End Sub

        Private Shared Sub WalkObject(node As Object, visit As Action(Of Expression), depth As Integer)
            If node Is Nothing OrElse depth > 16 Then Return

            Dim nodeType As Type = node.GetType()

            If nodeType.IsPrimitive OrElse nodeType Is GetType(String) Then Return

            Dim expr = TryCast(node, Expression)

            If expr IsNot Nothing Then Call visit(expr)

            If nodeType.IsValueType Then Return

            ' the JavaScript AST exposes most members as public READ-ONLY FIELDS and only a few as
            ' properties, so both must be walked
            Dim members As New List(Of MemberInfo)()

            Try
                Call members.AddRange(nodeType.GetProperties(BindingFlags.Public Or BindingFlags.Instance))
                Call members.AddRange(nodeType.GetFields(BindingFlags.Public Or BindingFlags.Instance))
            Catch ex As Exception
                Return
            End Try

            For Each memberInfo In members
                Dim propertyInfo = TryCast(memberInfo, PropertyInfo)

                If propertyInfo IsNot Nothing AndAlso propertyInfo.GetIndexParameters().Length > 0 Then Continue For

                Dim child As Object

                Try
                    If propertyInfo IsNot Nothing Then
                        child = propertyInfo.GetValue(node)
                    Else
                        child = DirectCast(memberInfo, FieldInfo).GetValue(node)
                    End If
                Catch ex As Exception
                    Continue For
                End Try

                If child Is Nothing Then Continue For

                Dim childType As Type = child.GetType()

                If childType.IsPrimitive OrElse childType Is GetType(String) OrElse childType.IsEnum Then
                    Continue For
                End If

                Dim items = TryCast(child, IEnumerable)

                If items IsNot Nothing Then
                    For Each item In items
                        Call WalkObject(item, visit, depth + 1)
                    Next
                ElseIf Not childType.IsValueType Then
                    Call WalkObject(child, visit, depth + 1)
                End If
            Next
        End Sub

    End Class

End Namespace
