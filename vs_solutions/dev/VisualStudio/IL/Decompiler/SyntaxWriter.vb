#Region "Microsoft.VisualBasic::bb4c9f392e47243872d904c4701115dd, vs_solutions\dev\VisualStudio\IL\Decompiler\SyntaxWriter.vb"

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

    '   Total Lines: 231
    '    Code Lines: 166 (71.86%)
    ' Comment Lines: 14 (6.06%)
    '    - Xml Docs: 21.43%
    ' 
    '   Blank Lines: 51 (22.08%)
    '     File Size: 10.66 KB


    '     Module SyntaxWriter
    ' 
    '         Function: OperatorText, TypeName, WriteExpression, WriteMethod
    ' 
    '         Sub: WriteBlock, WriteStatement
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' AST -> 类 VB 伪代码
'
' 用途：
'   1) 反编译调试：把还原出来的 if / while / for 结构打印出来肉眼核对；
'   2) 测试快照：与 CUDA 发射结果并排输出，便于判断"是反编译错了还是发射错了"。
'
' 打印风格刻意贴近 VB：Dim 声明、If/Then/Else、While/End While、For/Next。
' ---------------------------------------------------------------------------

Imports System.Text

Namespace IL

    Public Module SyntaxWriter

        ''' <summary>把整棵方法语法树打印成伪代码</summary>
        Public Function WriteMethod(syntax As MethodSyntax) As String
            If syntax Is Nothing Then Return String.Empty

            Dim code As New StringBuilder()
            ' 形参在 AST 里用的是 SSA 名（p_xxx），签名必须和函数体保持一致
            Dim parameters = String.Join(", ",
                syntax.Parameters.Select(Function(p) $"{If(p.SsaName, p.Name)} As {TypeName(p.ParameterType)}"))

            code.AppendLine($"{(If(syntax.IsStatic, "Shared ", ""))}Function {syntax.Name}({parameters}) As {TypeName(syntax.ReturnType)}")
            WriteBlock(code, syntax.Body, 1)
            code.AppendLine("End Function")

            Return code.ToString()
        End Function

        ''' <summary>把一个语句块打印到 builder</summary>
        Public Sub WriteBlock(code As StringBuilder, block As BlockStatement, indent As Integer)
            If block Is Nothing Then Return

            For Each stmt As Statement In block.Statements
                WriteStatement(code, stmt, indent)
            Next
        End Sub

        Public Sub WriteStatement(code As StringBuilder, stmt As Statement, indent As Integer)
            If stmt Is Nothing Then Return

            Dim pad = New String(" "c, indent * 4)

            Select Case stmt.Kind
                Case SyntaxKind.Block
                    WriteBlock(code, DirectCast(stmt, BlockStatement), indent)

                Case SyntaxKind.VariableDeclaration
                    Dim decl = DirectCast(stmt, VariableDeclarationStatement)

                    If decl.Initializer Is Nothing Then
                        code.AppendLine($"{pad}Dim {decl.Name} As {TypeName(decl.VariableType)}")
                    Else
                        code.AppendLine($"{pad}Dim {decl.Name} As {TypeName(decl.VariableType)} = {WriteExpression(decl.Initializer)}")
                    End If

                Case SyntaxKind.Assignment
                    Dim assign = DirectCast(stmt, AssignmentStatement)
                    code.AppendLine($"{pad}{WriteExpression(assign.Target)} = {WriteExpression(assign.Value)}")

                Case SyntaxKind.ExpressionStatement
                    Dim expr = DirectCast(stmt, ExpressionStatement)
                    code.AppendLine($"{pad}{WriteExpression(expr.Value)}")

                Case SyntaxKind.ReturnStmt
                    Dim ret = DirectCast(stmt, ReturnStatement)
                    code.AppendLine($"{pad}Return {WriteExpression(ret.Value)}")

                Case SyntaxKind.IfStmt
                    Dim [if] = DirectCast(stmt, IfStatement)

                    code.AppendLine($"{pad}If {WriteExpression([if].Condition)} Then")
                    WriteBlock(code, [if].ThenBody, indent + 1)

                    If [if].ElseBody IsNot Nothing Then
                        code.AppendLine($"{pad}Else")
                        WriteBlock(code, [if].ElseBody, indent + 1)
                    End If

                    code.AppendLine($"{pad}End If")

                Case SyntaxKind.WhileStmt
                    Dim loopStmt = DirectCast(stmt, WhileStatement)

                    If loopStmt.IsPostCondition Then
                        code.AppendLine($"{pad}Do")
                        WriteBlock(code, loopStmt.Body, indent + 1)
                        code.AppendLine($"{pad}Loop While {WriteExpression(loopStmt.Condition)}")
                    Else
                        code.AppendLine($"{pad}While {WriteExpression(loopStmt.Condition)}")
                        WriteBlock(code, loopStmt.Body, indent + 1)
                        code.AppendLine($"{pad}End While")
                    End If

                Case SyntaxKind.ForStmt
                    Dim loopStmt = DirectCast(stmt, ForStatement)

                    code.AppendLine($"{pad}For {loopStmt.VariableName} = {WriteExpression(loopStmt.Initializer)} While {WriteExpression(loopStmt.Condition)} Step ({WriteExpression(loopStmt.Increment)} - {loopStmt.VariableName})")
                    WriteBlock(code, loopStmt.Body, indent + 1)
                    code.AppendLine($"{pad}Next")

                Case SyntaxKind.BreakStmt
                    code.AppendLine($"{pad}Exit While")

                Case SyntaxKind.ContinueStmt
                    code.AppendLine($"{pad}Continue While")

                Case SyntaxKind.Nop
                    ' 不输出

                Case Else
                    code.AppendLine($"{pad}' 未处理的语句 {stmt.Kind}")
            End Select
        End Sub

        ''' <summary>把表达式节点打印成一个字符串</summary>
        Public Function WriteExpression(expr As Expression) As String
            If expr Is Nothing Then Return "<null>"

            Select Case expr.Kind
                Case SyntaxKind.Literal
                    Dim lit = DirectCast(expr, LiteralExpression)

                    If lit.Value Is Nothing Then Return "Nothing"
                    If TypeOf lit.Value Is Single Then
                        Return CSng(lit.Value).ToString("R", System.Globalization.CultureInfo.InvariantCulture) & "F"
                    End If
                    If TypeOf lit.Value Is Double Then
                        Return CDbl(lit.Value).ToString("R", System.Globalization.CultureInfo.InvariantCulture)
                    End If

                    Return System.Convert.ToString(lit.Value, System.Globalization.CultureInfo.InvariantCulture)

                Case SyntaxKind.Parameter
                    Return DirectCast(expr, ParameterExpression).Name

                Case SyntaxKind.Local
                    Return DirectCast(expr, LocalExpression).Name

                Case SyntaxKind.Binary
                    Dim bin = DirectCast(expr, BinaryExpression)
                    Return $"({WriteExpression(bin.Left)} {OperatorText(bin.Operator)} {WriteExpression(bin.Right)})"

                Case SyntaxKind.Unary
                    Dim un = DirectCast(expr, UnaryExpression)

                    If un.Operator = UnaryOperator.Negate Then
                        Return $"(-{WriteExpression(un.Operand)})"
                    End If

                    Return $"(Not {WriteExpression(un.Operand)})"

                Case SyntaxKind.Convert
                    Dim conv = DirectCast(expr, ConvertExpression)
                    Return $"CType({WriteExpression(conv.Operand)}, {TypeName(conv.TargetType)})"

                Case SyntaxKind.Invoke
                    Dim callNode = DirectCast(expr, CallExpression)
                    Dim args = String.Join(", ", callNode.AllArguments.Select(Function(a) WriteExpression(a)))
                    Return $"{callNode.FullName}({args})"

                Case SyntaxKind.ArrayIndex
                    Dim indexNode = DirectCast(expr, ArrayIndexExpression)
                    Return $"{WriteExpression(indexNode.Array)}[{WriteExpression(indexNode.Index)}]"

                Case SyntaxKind.ArrayLength
                    Return $"{WriteExpression(DirectCast(expr, ArrayLengthExpression).Array)}.Length"

                Case SyntaxKind.Ternary
                    Dim ternary = DirectCast(expr, TernaryExpression)
                    Return $"(If({WriteExpression(ternary.Condition)}, {WriteExpression(ternary.WhenTrue)}, {WriteExpression(ternary.WhenFalse)}))"

                Case SyntaxKind.Phi
                    Return DirectCast(expr, PhiExpression).ToString()

                Case Else
                    Return $"<{expr.Kind}>"
            End Select
        End Function

        Public Function OperatorText(op As BinaryOperator) As String
            Select Case op
                Case BinaryOperator.Add : Return "+"
                Case BinaryOperator.Subtract : Return "-"
                Case BinaryOperator.Multiply : Return "*"
                Case BinaryOperator.Divide : Return "/"
                Case BinaryOperator.Modulo : Return "Mod"
                Case BinaryOperator.BitwiseAnd : Return "And"
                Case BinaryOperator.BitwiseOr : Return "Or"
                Case BinaryOperator.BitwiseXor : Return "Xor"
                Case BinaryOperator.ShiftLeft : Return "<<"
                Case BinaryOperator.ShiftRight : Return ">>"
                Case BinaryOperator.Equal : Return "="
                Case BinaryOperator.NotEqual : Return "<>"
                Case BinaryOperator.LessThan : Return "<"
                Case BinaryOperator.LessThanOrEqual : Return "<="
                Case BinaryOperator.GreaterThan : Return ">"
                Case BinaryOperator.GreaterThanOrEqual : Return ">="
                Case BinaryOperator.ShortCircuitAnd : Return "AndAlso"
                Case BinaryOperator.ShortCircuitOr : Return "OrElse"
                Case Else : Return "?"
            End Select
        End Function

        Public Function TypeName(t As System.Type) As String
            If t Is Nothing Then Return "?"

            If t.IsArray Then Return TypeName(t.GetElementType()) & "()"

            Select Case t.FullName
                Case "System.Single" : Return "Single"
                Case "System.Double" : Return "Double"
                Case "System.Int32" : Return "Integer"
                Case "System.Int64" : Return "Long"
                Case "System.Int16" : Return "Short"
                Case "System.Byte" : Return "Byte"
                Case "System.SByte" : Return "SByte"
                Case "System.UInt32" : Return "UInteger"
                Case "System.UInt64" : Return "ULong"
                Case "System.UInt16" : Return "UShort"
                Case "System.Boolean" : Return "Boolean"
                Case "System.Void" : Return "Void"
            End Select

            Return t.Name
        End Function
    End Module
End Namespace
