' ---------------------------------------------------------------------------
' 结构化还原：把"每块一段平铺语句 + CFG"折叠成嵌套的 if / while / for
'
' 依赖的 CFG 信息：
'   * ImmediatePostDominator —— 求条件分支的汇聚点（join）；
'   * IsLoopHeader / LoopBody / LoopFollow —— 识别自然循环。
'
' 覆盖的形状：
'   1) 顺序：单后继直接拼接；
'   2) 菱形：两条分支汇聚到同一个 join         -> if / if-else；
'   3) guard clause：一条分支以 return 结束     -> 只有 Then 的 if，后续代码排在 if 之后；
'   4) 自然循环：循环头两个后继（体内 / 体外）  -> while；
'   5) 归纳变量形态的 while                    -> for（把" latch 里的 i = i + k "与
'      "前置块里的初值赋值"从语句流中摘出来，重组为 for 的三段式）。
'
' 覆盖不了的形状一律抛 <see cref="DecompileException"/> 并带上块号，
' 绝不退化成 goto —— 与其生成语义不明的 CUDA，不如明确失败。
' ---------------------------------------------------------------------------

Namespace IL

    Public Class StructureRecovery

        ''' <summary>一段已归约好的语句 + 该段之后应该继续处理的块</summary>
        Private Class RegionOutput
            Public Property Statements As New List(Of Statement)
            Public Property NextId As Integer = -1
        End Class

        Private ReadOnly _cfg As ControlFlowGraph
        Private ReadOnly _diagnostics As DecompileDiagnostics

        Public Sub New(cfg As ControlFlowGraph, diagnostics As DecompileDiagnostics)
            _cfg = cfg
            _diagnostics = If(diagnostics, New DecompileDiagnostics())
        End Sub

        ''' <summary>从入口块出发，还原出整个方法体的语句块</summary>
        Public Function Build() As BlockStatement
            Dim output = EmitRegion(0, ControlFlowGraph.VirtualExit)
            Dim body As New BlockStatement()
            body.Statements.AddRange(output.Statements)

            Return body
        End Function

        ' ==================================================================
        ' 线性区域
        ' ==================================================================

        Private Function EmitRegion(startId As Integer, followId As Integer) As RegionOutput
            Dim result As New RegionOutput()
            Dim current = startId
            Dim guard As Integer = 0

            While current >= 0 AndAlso current <> followId
                guard += 1

                If guard > 4096 Then
                    Throw New DecompileException("结构化还原未能收敛，可能存在不可归约的控制流")
                End If

                Dim b = _cfg.Blocks(current)

                If b.ReversePostOrder < 0 Then Exit While

                result.Statements.AddRange(b.Statements)

                If b.IsLoopHeader Then
                    result.Statements.Add(BuildLoop(b, result.Statements))
                    current = b.LoopFollow

                ElseIf b.Successors.Count = 0 Then
                    current = -1

                ElseIf b.Successors.Count = 1 Then
                    current = b.Successors(0)

                ElseIf b.Successors.Count = 2 Then
                    Dim branch = BuildBranch(b)
                    result.Statements.Add(branch.Statements(0))
                    current = branch.NextId
                Else
                    Throw New DecompileException(
                        $"块 bb{b.Id} 有 {b.Successors.Count} 个后继，无法结构化还原")
                End If
            End While

            result.NextId = current

            Return result
        End Function

        ' ==================================================================
        ' 条件分支
        ' ==================================================================

        Private Function BuildBranch(b As BasicBlock) As RegionOutput
            Dim result As New RegionOutput()

            If b.Condition Is Nothing Then
                Throw New DecompileException($"块 bb{b.Id} 是条件分支，但没有还原出条件表达式")
            End If

            Dim join = b.ImmediatePostDominator
            Dim t = b.TrueSuccessor
            Dim f = b.FalseSuccessor

            If t < 0 OrElse f < 0 Then
                Throw New DecompileException($"块 bb{b.Id} 的条件分支后继缺失，无法结构化还原")
            End If

            ' ---- 不汇聚到真实 join：至少一条分支以 return 结束 ----
            If join = ControlFlowGraph.VirtualExit Then
                Dim tReturns = PathEndsWithReturn(t)
                Dim fReturns = PathEndsWithReturn(f)

                If tReturns AndAlso Not fReturns Then
                    result.Statements.Add(New IfStatement(
                        b.Condition,
                        New BlockStatement(EmitRegion(t, ControlFlowGraph.VirtualExit).Statements)))
                    result.NextId = f
                    Return result
                End If

                If fReturns AndAlso Not tReturns Then
                    result.Statements.Add(New IfStatement(
                        Negate(b.Condition),
                        New BlockStatement(EmitRegion(f, ControlFlowGraph.VirtualExit).Statements)))
                    result.NextId = t
                    Return result
                End If

                result.Statements.Add(New IfStatement(
                    b.Condition,
                    New BlockStatement(EmitRegion(t, ControlFlowGraph.VirtualExit).Statements),
                    New BlockStatement(EmitRegion(f, ControlFlowGraph.VirtualExit).Statements)))
                result.NextId = ControlFlowGraph.VirtualExit

                Return result
            End If

            ' ---- 标准菱形 ----
            Dim thenBody As New BlockStatement(EmitRegion(t, join).Statements)
            Dim elseBody As BlockStatement = Nothing

            If f <> join Then
                elseBody = New BlockStatement(EmitRegion(f, join).Statements)
            End If

            result.Statements.Add(New IfStatement(b.Condition, thenBody, elseBody))
            result.NextId = join

            Return result
        End Function

        Private Function Negate(condition As Expression) As Expression
            Dim unary = TryCast(condition, UnaryExpression)

            ' 消除双重取反，避免出现 !(!x)
            If unary IsNot Nothing AndAlso unary.Operator = UnaryOperator.LogicalNot Then
                Return unary.Operand
            End If

            Return New UnaryExpression(UnaryOperator.LogicalNot, condition, GetType(Boolean))
        End Function

        ''' <summary>从 startId 出发的单后继链是否以 return 结束</summary>
        Private Function PathEndsWithReturn(startId As Integer) As Boolean
            Dim current = startId
            Dim guard As Integer = 0

            While current >= 0
                guard += 1
                If guard > 4096 Then Return False

                Dim b = _cfg.Blocks(current)

                If b.Successors.Count = 0 Then Return True
                If b.Successors.Count > 1 Then Return False

                current = b.Successors(0)
            End While

            Return True
        End Function

        ' ==================================================================
        ' 循环
        ' ==================================================================

        Private Function BuildLoop(header As BasicBlock, parentStatements As List(Of Statement)) As Statement
            If header.Condition Is Nothing Then
                Throw New DecompileException($"循环头 bb{header.Id} 没有还原出条件表达式")
            End If

            If header.Successors.Count <> 2 Then
                Throw New DecompileException(
                    $"循环头 bb{header.Id} 有 {header.Successors.Count} 个后继，无法归约为 while / for")
            End If

            Dim bodyStart As Integer = -1

            For Each s As Integer In header.Successors
                If header.LoopBody.Contains(s) Then
                    bodyStart = s
                    Exit For
                End If
            Next

            If bodyStart < 0 Then
                Throw New DecompileException($"循环头 bb{header.Id} 找不到循环体入口")
            End If

            Dim bodyOut = EmitRegion(bodyStart, header.Id)
            Dim body As New BlockStatement(bodyOut.Statements)

            Dim [for] = TryBuildFor(header, body, parentStatements)

            If [for] IsNot Nothing Then Return [for]

            Return New WhileStatement(header.Condition, body)
        End Function

        ''' <summary>
        ''' 识别归纳变量形态的循环并归约为 for。
        ''' 形态：循环体末尾是 "V_k = V_j + step" 紧跟 "V_j = V_k"（phi 回写），
        ''' 且循环头的条件里用到了 V_j。满足时把这两条语句从循环体摘掉，
        ''' 并把前置块里对 V_j 的初值赋值也摘出来，组成 for 的三段式。
        ''' 任何一步不匹配都返回 Nothing，回退成 while。
        ''' </summary>
        Private Function TryBuildFor(header As BasicBlock,
                                     body As BlockStatement,
                                     parentStatements As List(Of Statement)) As ForStatement
            If body.Statements.Count < 2 Then Return Nothing

            Dim n = body.Statements.Count
            Dim phiWrite = TryCast(body.Statements(n - 1), AssignmentStatement)
            Dim increment = TryCast(body.Statements(n - 2), VariableDeclarationStatement)

            If phiWrite Is Nothing OrElse increment Is Nothing Then Return Nothing
            If increment.Initializer Is Nothing Then Return Nothing

            Dim loopVar = TryCast(phiWrite.Target, LocalExpression)
            Dim writtenBack = TryCast(phiWrite.Value, LocalExpression)

            If loopVar Is Nothing OrElse writtenBack Is Nothing Then Return Nothing
            If writtenBack.Name <> increment.Name Then Return Nothing

            Dim step1 = TryCast(increment.Initializer, BinaryExpression)

            If step1 Is Nothing Then Return Nothing
            If step1.Operator <> BinaryOperator.Add AndAlso
               step1.Operator <> BinaryOperator.Subtract Then Return Nothing

            Dim readVar = TryCast(step1.Left, LocalExpression)

            If readVar Is Nothing Then Return Nothing
            If readVar.Name <> loopVar.Name Then Return Nothing

            ' 循环条件必须真的用到这个变量
            If Not ReferencesLocal(header.Condition, loopVar.Name) Then Return Nothing

            ' 初值：前置块末尾对同一个变量名的赋值
            If parentStatements.Count = 0 Then Return Nothing

            Dim init = TryCast(parentStatements(parentStatements.Count - 1), AssignmentStatement)

            If init Is Nothing Then Return Nothing

            Dim initTarget = TryCast(init.Target, LocalExpression)

            If initTarget Is Nothing OrElse initTarget.Name <> loopVar.Name Then Return Nothing

            parentStatements.RemoveAt(parentStatements.Count - 1)
            body.Statements.RemoveRange(n - 2, 2)

            Return New ForStatement With {
                .VariableName = loopVar.Name,
                .Initializer = init.Value,
                .Condition = header.Condition,
                .Increment = step1,
                .Body = body
            }
        End Function

        ''' <summary>表达式树里是否引用了名为 name 的局部变量</summary>
        Private Shared Function ReferencesLocal(expr As Expression, name As String) As Boolean
            If expr Is Nothing Then Return False

            Dim local = TryCast(expr, LocalExpression)
            If local IsNot Nothing Then Return local.Name = name

            Dim binary = TryCast(expr, BinaryExpression)
            If binary IsNot Nothing Then
                Return ReferencesLocal(binary.Left, name) OrElse ReferencesLocal(binary.Right, name)
            End If

            Dim unary = TryCast(expr, UnaryExpression)
            If unary IsNot Nothing Then Return ReferencesLocal(unary.Operand, name)

            Dim convert = TryCast(expr, ConvertExpression)
            If convert IsNot Nothing Then Return ReferencesLocal(convert.Operand, name)

            Dim index = TryCast(expr, ArrayIndexExpression)
            If index IsNot Nothing Then
                Return ReferencesLocal(index.Array, name) OrElse ReferencesLocal(index.Index, name)
            End If

            Dim length = TryCast(expr, ArrayLengthExpression)
            If length IsNot Nothing Then Return ReferencesLocal(length.Array, name)

            Dim ternary = TryCast(expr, TernaryExpression)
            If ternary IsNot Nothing Then
                Return ReferencesLocal(ternary.Condition, name) OrElse
                       ReferencesLocal(ternary.WhenTrue, name) OrElse
                       ReferencesLocal(ternary.WhenFalse, name)
            End If

            Dim callNode = TryCast(expr, CallExpression)
            If callNode IsNot Nothing Then
                For Each arg In callNode.AllArguments
                    If ReferencesLocal(arg, name) Then Return True
                Next
            End If

            Return False
        End Function
    End Class
End Namespace
