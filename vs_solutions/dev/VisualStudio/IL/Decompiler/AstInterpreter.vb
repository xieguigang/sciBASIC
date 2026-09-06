' ---------------------------------------------------------------------------
' AST 的 CPU 解释求值
'
' 存在的意义：在没有可用 GPU（驱动/工具包版本错配、CI 机器没卡）的环境下，
' 仍然可以验证"反编译出来的 AST 与原方法语义一致"——
' 用同一组参数分别跑一次原方法与一次 AST 解释，比较返回值。
'
' 这一步把"IL 反编译是否正确"与"CUDA 发射/编译是否正确"两个问题解耦：
' 解释求值不过，说明反编译有问题；解释求值过而 GPU 结果不对，说明发射有问题。
'
' 只支持数值语义：Single / Double / Integer / Long / Boolean 与一维数组。
' ---------------------------------------------------------------------------

Namespace IL

    ''' <summary>在 CPU 上直接执行一棵 <see cref="MethodSyntax"/></summary>
    Public Class AstInterpreter

        Private ReadOnly _syntax As MethodSyntax
        ''' <summary>变量名 -> 当前值</summary>
        Private ReadOnly _env As New Dictionary(Of String, Object)()

        Private _returning As Boolean
        Private _returnValue As Object

        ''' <summary>单步执行的最大语句数，防止还原出错时出现死循环</summary>
        Public Property StepLimit As Integer = 50_000_000

        Public Sub New(syntax As MethodSyntax)
            If syntax Is Nothing Then Throw New ArgumentNullException(NameOf(syntax))
            _syntax = syntax
        End Sub

        ''' <summary>用给定实参执行方法体，返回返回值（Sub 返回 Nothing）</summary>
        Public Function Invoke(ParamArray args As Object()) As Object
            _env.Clear()
            _returning = False
            _returnValue = Nothing

            For i As Integer = 0 To _syntax.Parameters.Count - 1
                Dim p = _syntax.Parameters(i)

                ' AST 里 ParameterExpression 用的是 SSA 名（p_xxx），环境也必须用同一个键
                _env(p.SsaName) = If(i < args.Length, args(i), Nothing)
            Next

            ExecuteBlock(_syntax.Body)

            Return _returnValue
        End Function

        ' ==================================================================
        ' 语句
        ' ==================================================================

        Private Sub ExecuteBlock(block As BlockStatement)
            If block Is Nothing Then Return

            For Each stmt As Statement In block.Statements
                ExecuteStatement(stmt)
                If _returning Then Return
            Next
        End Sub

        Private Sub ExecuteStatement(stmt As Statement)
            If stmt Is Nothing OrElse _returning Then Return

            Select Case stmt.Kind
                Case SyntaxKind.Block
                    ExecuteBlock(DirectCast(stmt, BlockStatement))

                Case SyntaxKind.VariableDeclaration
                    Dim decl = DirectCast(stmt, VariableDeclarationStatement)
                    _env(decl.Name) = If(decl.Initializer Is Nothing,
                                         DefaultOf(decl.VariableType),
                                         Evaluate(decl.Initializer))

                Case SyntaxKind.Assignment
                    Dim assign = DirectCast(stmt, AssignmentStatement)
                    AssignValue(assign.Target, Evaluate(assign.Value))

                Case SyntaxKind.ExpressionStatement
                    Evaluate(DirectCast(stmt, ExpressionStatement).Value)

                Case SyntaxKind.ReturnStmt
                    Dim ret = DirectCast(stmt, ReturnStatement)
                    _returnValue = If(ret.Value Is Nothing, Nothing, Evaluate(ret.Value))
                    _returning = True

                Case SyntaxKind.IfStmt
                    Dim [if] = DirectCast(stmt, IfStatement)

                    If Truth(Evaluate([if].Condition)) Then
                        ExecuteBlock([if].ThenBody)
                    ElseIf [if].ElseBody IsNot Nothing Then
                        ExecuteBlock([if].ElseBody)
                    End If

                Case SyntaxKind.WhileStmt
                    Dim loopStmt = DirectCast(stmt, WhileStatement)
                    Dim steps As Integer = 0

                    If loopStmt.IsPostCondition Then
                        Do
                            ExecuteBlock(loopStmt.Body)
                            If _returning Then Return
                            steps += 1
                            If steps > StepLimit Then Throw New InvalidOperationException("解释执行超过步数上限（可能是死循环）")
                        Loop While Truth(Evaluate(loopStmt.Condition))
                    Else
                        Do While Truth(Evaluate(loopStmt.Condition))
                            ExecuteBlock(loopStmt.Body)
                            If _returning Then Return
                            steps += 1
                            If steps > StepLimit Then Throw New InvalidOperationException("解释执行超过步数上限（可能是死循环）")
                        Loop
                    End If

                Case SyntaxKind.ForStmt
                    Dim loopStmt = DirectCast(stmt, ForStatement)
                    Dim steps As Integer = 0

                    _env(loopStmt.VariableName) = Evaluate(loopStmt.Initializer)

                    Do While Truth(Evaluate(loopStmt.Condition))
                        ExecuteBlock(loopStmt.Body)
                        If _returning Then Return

                        _env(loopStmt.VariableName) = Evaluate(loopStmt.Increment)

                        steps += 1
                        If steps > StepLimit Then Throw New InvalidOperationException("解释执行超过步数上限（可能是死循环）")
                    Loop

                Case SyntaxKind.Nop
                    ' 空操作

                Case Else
                    Throw New NotSupportedException($"解释器不支持语句 {stmt.Kind}")
            End Select
        End Sub

        Private Sub AssignValue(target As Expression, value As Object)
            Dim local = TryCast(target, LocalExpression)
            If local IsNot Nothing Then
                _env(local.Name) = value
                Return
            End If

            Dim parameter = TryCast(target, ParameterExpression)
            If parameter IsNot Nothing Then
                _env(parameter.Name) = value
                Return
            End If

            Dim indexNode = TryCast(target, ArrayIndexExpression)
            If indexNode IsNot Nothing Then
                Dim arr = DirectCast(Evaluate(indexNode.Array), Array)
                arr.SetValue(value, System.Convert.ToInt32(Evaluate(indexNode.Index)))
                Return
            End If

            Throw New NotSupportedException($"解释器不支持的赋值目标 {target.Kind}")
        End Sub

        ' ==================================================================
        ' 表达式
        ' ==================================================================

        Private Function Evaluate(expr As Expression) As Object
            If expr Is Nothing Then Return Nothing

            Select Case expr.Kind
                Case SyntaxKind.Literal
                    Return DirectCast(expr, LiteralExpression).Value

                Case SyntaxKind.Parameter
                    Return Lookup(DirectCast(expr, ParameterExpression).Name)

                Case SyntaxKind.Local
                    Return Lookup(DirectCast(expr, LocalExpression).Name)

                Case SyntaxKind.Binary
                    Dim bin = DirectCast(expr, BinaryExpression)
                    Return EvaluateBinary(bin.Operator, Evaluate(bin.Left), Evaluate(bin.Right))

                Case SyntaxKind.Unary
                    Dim un = DirectCast(expr, UnaryExpression)
                    Dim operand = Evaluate(un.Operand)

                    If un.Operator = UnaryOperator.Negate Then
                        Return ArithUnary(operand, Function(v) -v)
                    End If

                    If TypeOf operand Is Boolean Then Return Not CBool(operand)
                    Return ArithUnary(operand, Function(v) -v)

                Case SyntaxKind.Convert
                    Dim conv = DirectCast(expr, ConvertExpression)
                    Return System.Convert.ChangeType(Evaluate(conv.Operand), If(conv.TargetType, GetType(Double)))

                Case SyntaxKind.Invoke
                    Return EvaluateCall(DirectCast(expr, CallExpression))

                Case SyntaxKind.ArrayIndex
                    Dim indexNode = DirectCast(expr, ArrayIndexExpression)
                    Dim arr = DirectCast(Evaluate(indexNode.Array), Array)

                    Return arr.GetValue(System.Convert.ToInt32(Evaluate(indexNode.Index)))

                Case SyntaxKind.ArrayLength
                    Dim arr = DirectCast(Evaluate(DirectCast(expr, ArrayLengthExpression).Array), Array)
                    Return arr.Length

                Case SyntaxKind.Ternary
                    Dim ternary = DirectCast(expr, TernaryExpression)
                    Return If(Truth(Evaluate(ternary.Condition)),
                              Evaluate(ternary.WhenTrue),
                              Evaluate(ternary.WhenFalse))

                Case Else
                    Throw New NotSupportedException($"解释器不支持表达式 {expr.Kind}")
            End Select
        End Function

        Private Function Lookup(name As String) As Object
            Dim value As Object = Nothing
            If _env.TryGetValue(name, value) Then Return value
            Return Nothing
        End Function

        Private Shared Function Truth(value As Object) As Boolean
            If value Is Nothing Then Return False
            If TypeOf value Is Boolean Then Return CBool(value)

            Return System.Convert.ToDouble(value) <> 0.0
        End Function

        Private Shared Function DefaultOf(t As System.Type) As Object
            If t Is Nothing Then Return Nothing
            If t = GetType(Single) Then Return 0.0F
            If t = GetType(Double) Then Return 0.0R
            If t = GetType(Integer) Then Return 0
            If t = GetType(Long) Then Return 0L
            If t = GetType(Boolean) Then Return False

            Return Nothing
        End Function

        ' ---------------- 算术 ----------------

        Private Shared Function ArithUnary(value As Object, f As Func(Of Double, Double)) As Object
            If TypeOf value Is Single Then Return CSng(f(CDbl(value)))
            If TypeOf value Is Integer OrElse TypeOf value Is Long OrElse TypeOf value Is Short Then
                Return CInt(System.Math.Round(f(CDbl(value))))
            End If

            Return f(CDbl(value))
        End Function

        Private Shared Function EvaluateBinary(op As BinaryOperator, leftObj As Object, rightObj As Object) As Object
            ' 逻辑短路
            If op = BinaryOperator.ShortCircuitAnd Then
                Return Truth(leftObj) AndAlso Truth(rightObj)
            End If
            If op = BinaryOperator.ShortCircuitOr Then
                Return Truth(leftObj) OrElse Truth(rightObj)
            End If

            ' 位运算
            If op = BinaryOperator.BitwiseAnd OrElse op = BinaryOperator.BitwiseOr OrElse
               op = BinaryOperator.BitwiseXor Then
                Dim a = CLng(System.Convert.ToDouble(leftObj))
                Dim b = CLng(System.Convert.ToDouble(rightObj))

                Select Case op
                    Case BinaryOperator.BitwiseAnd : Return CInt(a And b)
                    Case BinaryOperator.BitwiseOr : Return CInt(a Or b)
                    Case Else : Return CInt(a Xor b)
                End Select
            End If

            If op = BinaryOperator.ShiftLeft Then
                Return CInt(CLng(System.Convert.ToDouble(leftObj)) << CInt(System.Convert.ToDouble(rightObj)))
            End If
            If op = BinaryOperator.ShiftRight Then
                Return CInt(CLng(System.Convert.ToDouble(leftObj)) >> CInt(System.Convert.ToDouble(rightObj)))
            End If

            ' 比较
            Dim la = System.Convert.ToDouble(leftObj)
            Dim rb = System.Convert.ToDouble(rightObj)

            Select Case op
                Case BinaryOperator.Equal : Return la = rb
                Case BinaryOperator.NotEqual : Return la <> rb
                Case BinaryOperator.LessThan : Return la < rb
                Case BinaryOperator.LessThanOrEqual : Return la <= rb
                Case BinaryOperator.GreaterThan : Return la > rb
                Case BinaryOperator.GreaterThanOrEqual : Return la >= rb
            End Select

            ' 算术：单精度参与时用单精度算，避免与 GPU 上的 float 语义差太远
            Dim asSingle = TypeOf leftObj Is Single OrElse TypeOf rightObj Is Single

            If asSingle Then
                Dim a = CSng(leftObj)
                Dim b = CSng(rightObj)

                Select Case op
                    Case BinaryOperator.Add : Return a + b
                    Case BinaryOperator.Subtract : Return a - b
                    Case BinaryOperator.Multiply : Return a * b
                    Case BinaryOperator.Divide : Return a / b
                    Case BinaryOperator.Modulo : Return a Mod b
                End Select
            End If

            Dim integral = Not (TypeOf leftObj Is Double OrElse TypeOf rightObj Is Double)

            If integral Then
                Dim a = CLng(System.Convert.ToDouble(leftObj))
                Dim b = CLng(System.Convert.ToDouble(rightObj))

                Select Case op
                    Case BinaryOperator.Add : Return CInt(a + b)
                    Case BinaryOperator.Subtract : Return CInt(a - b)
                    Case BinaryOperator.Multiply : Return CInt(a * b)
                    Case BinaryOperator.Divide : Return CInt(a \ b)
                    Case BinaryOperator.Modulo : Return CInt(a Mod b)
                End Select
            End If

            Dim da = System.Convert.ToDouble(leftObj)
            Dim db = System.Convert.ToDouble(rightObj)

            Select Case op
                Case BinaryOperator.Add : Return da + db
                Case BinaryOperator.Subtract : Return da - db
                Case BinaryOperator.Multiply : Return da * db
                Case BinaryOperator.Divide : Return da / db
                Case BinaryOperator.Modulo : Return da Mod db
            End Select

            Throw New NotSupportedException($"不支持的二元运算 {op}")
        End Function

        ' ---------------- 调用 ----------------

        Private Function EvaluateCall(callNode As CallExpression) As Object
            Dim args As New List(Of Object)()

            For Each arg In callNode.AllArguments
                args.Add(Evaluate(arg))
            Next

            Dim useSingle = callNode.Type IsNot Nothing AndAlso callNode.Type = GetType(Single)

            If Not useSingle Then
                For Each a In args
                    If TypeOf a Is Single Then
                        useSingle = True
                        Exit For
                    End If
                Next
            End If

            Select Case callNode.MethodName
                Case "Sqrt"
                    Return Num(Function(x) System.Math.Sqrt(x), args, useSingle)
                Case "Abs"
                    Return Num(Function(x) System.Math.Abs(x), args, useSingle)
                Case "Exp"
                    Return Num(Function(x) System.Math.Exp(x), args, useSingle)
                Case "Log"
                    If args.Count = 2 Then
                        Return Num2(Function(x, y) System.Math.Log(x, y), args, useSingle)
                    End If
                    Return Num(Function(x) System.Math.Log(x), args, useSingle)
                Case "Log10"
                    Return Num(Function(x) System.Math.Log10(x), args, useSingle)
                Case "Pow"
                    Return Num2(Function(x, y) System.Math.Pow(x, y), args, useSingle)
                Case "Floor"
                    Return Num(Function(x) System.Math.Floor(x), args, useSingle)
                Case "Ceiling"
                    Return Num(Function(x) System.Math.Ceiling(x), args, useSingle)
                Case "Round"
                    Return Num(Function(x) System.Math.Round(x), args, useSingle)
                Case "Truncate"
                    Return Num(Function(x) System.Math.Truncate(x), args, useSingle)
                Case "Sign"
                    Return Num(Function(x) CDbl(System.Math.Sign(x)), args, useSingle)
                Case "Sin"
                    Return Num(Function(x) System.Math.Sin(x), args, useSingle)
                Case "Cos"
                    Return Num(Function(x) System.Math.Cos(x), args, useSingle)
                Case "Tan"
                    Return Num(Function(x) System.Math.Tan(x), args, useSingle)
                Case "Asin"
                    Return Num(Function(x) System.Math.Asin(x), args, useSingle)
                Case "Acos"
                    Return Num(Function(x) System.Math.Acos(x), args, useSingle)
                Case "Atan"
                    Return Num(Function(x) System.Math.Atan(x), args, useSingle)
                Case "Atan2"
                    Return Num2(Function(x, y) System.Math.Atan2(x, y), args, useSingle)
                Case "Sinh", "Cosh", "Tanh"
                    If callNode.MethodName = "Sinh" Then Return Num(Function(x) System.Math.Sinh(x), args, useSingle)
                    If callNode.MethodName = "Cosh" Then Return Num(Function(x) System.Math.Cosh(x), args, useSingle)
                    Return Num(Function(x) System.Math.Tanh(x), args, useSingle)
                Case "Min"
                    Return Num2(Function(x, y) System.Math.Min(x, y), args, useSingle)
                Case "Max"
                    Return Num2(Function(x, y) System.Math.Max(x, y), args, useSingle)
                Case Else
                    Throw New NotSupportedException($"解释器不支持调用 {callNode.FullName}")
            End Select
        End Function

        Private Shared Function Num(f As Func(Of Double, Double), args As List(Of Object), asSingle As Boolean) As Object
            Dim v = f(System.Convert.ToDouble(args(0)))
            Return If(asSingle, CSng(v), CObj(v))
        End Function

        Private Shared Function Num2(f As Func(Of Double, Double, Double), args As List(Of Object), asSingle As Boolean) As Object
            Dim v = f(System.Convert.ToDouble(args(0)), System.Convert.ToDouble(args(1)))
            Return If(asSingle, CSng(v), CObj(v))
        End Function
    End Class
End Namespace
