#Region "Microsoft.VisualBasic::bd12d709de313d6c48e8413f7a13268b, vs_solutions\dev\VisualStudio\IL\Decompiler\StructureRecovery.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 370
    '    Code Lines: 231 (62.43%)
    ' Comment Lines: 48 (12.97%)
    '    - Xml Docs: 25.00%
    ' 
    '   Blank Lines: 91 (24.59%)
    '     File Size: 15.61 KB


    '     Class StructureRecovery
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Build, BuildBranch, BuildLoop, EmitRegion, IsPhiWriteBack
    '                   Negate, PathEndsWithReturn, ReferencesLocal, TryBuildFor
    '         Class RegionOutput
    ' 
    '             Properties: NextId, Statements
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

            ' 循环体末尾是清一色的 phi 回写（"V_x = V_y"），真正属于循环体的语句在它们之前。
            ' 从后往前找到第一条不是 phi 回写的语句，它应该就是归纳变量的步进。
            Dim phiStart = body.Statements.Count

            While phiStart > 0 AndAlso IsPhiWriteBack(body.Statements(phiStart - 1))
                phiStart -= 1
            End While

            If phiStart = 0 OrElse phiStart = body.Statements.Count Then Return Nothing

            Dim increment = TryCast(body.Statements(phiStart - 1), VariableDeclarationStatement)

            If increment Is Nothing OrElse increment.Initializer Is Nothing Then Return Nothing

            Dim step1 = TryCast(increment.Initializer, BinaryExpression)

            If step1 Is Nothing Then Return Nothing
            If step1.Operator <> BinaryOperator.Add AndAlso
               step1.Operator <> BinaryOperator.Subtract Then Return Nothing

            Dim readVar = TryCast(step1.Left, LocalExpression)

            If readVar Is Nothing Then Return Nothing

            ' 在 phi 回写区里找 "readVar = increment.Name"——即把步进结果写回循环变量
            Dim phiIndex As Integer = -1

            For i As Integer = phiStart To body.Statements.Count - 1
                Dim write = TryCast(body.Statements(i), AssignmentStatement)

                If write Is Nothing Then Continue For

                Dim target = TryCast(write.Target, LocalExpression)
                Dim value = TryCast(write.Value, LocalExpression)

                If target Is Nothing OrElse value Is Nothing Then Continue For
                If target.Name = readVar.Name AndAlso value.Name = increment.Name Then
                    phiIndex = i
                    Exit For
                End If
            Next

            If phiIndex < 0 Then Return Nothing

            ' 循环条件必须真的用到这个变量
            If Not ReferencesLocal(header.Condition, readVar.Name) Then Return Nothing

            ' 初值：前置块末尾（同样是 phi 回写区）里对同一个变量名的赋值。
            ' phi 的插入顺序不稳定，因此这里从后往前搜，而不是只看最后一条。
            Dim initIndex As Integer = -1

            For i As Integer = parentStatements.Count - 1 To 0 Step -1
                Dim init = TryCast(parentStatements(i), AssignmentStatement)

                If init Is Nothing Then Continue For

                Dim initTarget = TryCast(init.Target, LocalExpression)

                If initTarget IsNot Nothing AndAlso initTarget.Name = readVar.Name Then
                    initIndex = i
                    Exit For
                End If
            Next

            If initIndex < 0 Then Return Nothing

            Dim initializer = DirectCast(parentStatements(initIndex), AssignmentStatement).Value

            parentStatements.RemoveAt(initIndex)
            body.Statements.RemoveAt(phiIndex)
            body.Statements.RemoveAt(phiStart - 1)

            Return New ForStatement With {
                .VariableName = readVar.Name,
                .Initializer = initializer,
                .Condition = header.Condition,
                .Increment = step1,
                .Body = body
            }
        End Function

        ''' <summary>是否为"V_a = V_b"这种把值直接搬给 phi 变量的回写</summary>
        Private Shared Function IsPhiWriteBack(stmt As Statement) As Boolean
            Dim write = TryCast(stmt, AssignmentStatement)
            If write Is Nothing Then Return False

            Return TypeOf write.Target Is LocalExpression AndAlso
                   TypeOf write.Value Is LocalExpression
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
