#Region "Microsoft.VisualBasic::99d8464c102c17962dbe7488c724ddde, cuda\ILCuda\IL2Cuda\CudaEmitter.vb"

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

    '   Total Lines: 253
    '    Code Lines: 173 (68.38%)
    ' Comment Lines: 24 (9.49%)
    '    - Xml Docs: 37.50%
    ' 
    '   Blank Lines: 56 (22.13%)
    '     File Size: 10.33 KB


    '     Class CudaEmitter
    ' 
    '         Properties: IndentSize, Text
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: EmitCall, EmitExpression, EmitOperand
    ' 
    '         Sub: AppendLine, DecreaseIndent, EmitBlock, EmitFor, EmitIf
    '              EmitStatement, EmitWhile, IncreaseIndent
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' 表达式 / 语句 -> CUDA C 文本
'
' 只做三件事：
'   1) 按节点类型翻译成 C 语法；
'   2) 嵌套的二元表达式一律加括号（宁可多一层括号，也不赌运算符优先级）；
'   3) 遇到困难节点（数组长度、非数学调用）直接抛异常 —— 生成错代码不如明确失败。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.IL
Imports System.Text

Namespace IL2Cuda

    Public Class CudaEmitter

        Private ReadOnly _code As New StringBuilder()
        Private _indent As Integer = 0

        ''' <summary>每级缩进四个空格</summary>
        Public Property IndentSize As Integer = 4

        Public Sub New(Optional indent As Integer = 0)
            _indent = indent
        End Sub

        ''' <summary>已经生成的代码</summary>
        Public ReadOnly Property Text As String
            Get
                Return _code.ToString()
            End Get
        End Property

        Public Sub AppendLine(Optional text As String = Nothing)
            If String.IsNullOrEmpty(text) Then
                _code.AppendLine()
            Else
                _code.AppendLine(New String(" "c, _indent * IndentSize) & text)
            End If
        End Sub

        Public Sub IncreaseIndent()
            _indent += 1
        End Sub

        Public Sub DecreaseIndent()
            If _indent > 0 Then _indent -= 1
        End Sub

        ' ==================================================================
        ' 语句
        ' ==================================================================

        Public Sub EmitStatement(stmt As Statement)
            If stmt Is Nothing Then Return

            Select Case stmt.Kind
                Case SyntaxKind.Block
                    EmitBlock(DirectCast(stmt, BlockStatement))

                Case SyntaxKind.VariableDeclaration
                    Dim decl = DirectCast(stmt, VariableDeclarationStatement)
                    Dim typeName = CudaTypeMap.CudaType(decl.VariableType)

                    If typeName Is Nothing Then
                        Throw New NotSupportedException(
                            $"局部变量 {decl.Name} 的类型 {If(decl.VariableType Is Nothing, "?", decl.VariableType.FullName)} 无法映射为 CUDA 类型")
                    End If

                    If decl.Initializer Is Nothing Then
                        AppendLine($"{typeName} {CudaTypeMap.SafeName(decl.Name)};")
                    Else
                        AppendLine($"{typeName} {CudaTypeMap.SafeName(decl.Name)} = {EmitExpression(decl.Initializer)};")
                    End If

                Case SyntaxKind.Assignment
                    Dim assign = DirectCast(stmt, AssignmentStatement)
                    AppendLine($"{EmitExpression(assign.Target)} = {EmitExpression(assign.Value)};")

                Case SyntaxKind.ExpressionStatement
                    AppendLine($"{EmitExpression(DirectCast(stmt, ExpressionStatement).Value)};")

                Case SyntaxKind.ReturnStmt
                    Dim ret = DirectCast(stmt, ReturnStatement)

                    If ret.Value Is Nothing Then
                        AppendLine("return;")
                    Else
                        AppendLine($"return {EmitExpression(ret.Value)};")
                    End If

                Case SyntaxKind.IfStmt
                    EmitIf(DirectCast(stmt, IfStatement))

                Case SyntaxKind.WhileStmt
                    EmitWhile(DirectCast(stmt, WhileStatement))

                Case SyntaxKind.ForStmt
                    EmitFor(DirectCast(stmt, ForStatement))

                Case SyntaxKind.Nop
                    ' 不产生代码

                Case Else
                    Throw New NotSupportedException($"语句 {stmt.Kind} 无法发射为 CUDA 代码")
            End Select
        End Sub

        Public Sub EmitBlock(block As BlockStatement)
            If block Is Nothing Then Return

            For Each stmt As Statement In block.Statements
                EmitStatement(stmt)
            Next
        End Sub

        Private Sub EmitIf([if] As IfStatement)
            AppendLine($"if ({EmitExpression([if].Condition)}) {{")
            IncreaseIndent()
            EmitBlock([if].ThenBody)
            DecreaseIndent()

            If [if].ElseBody IsNot Nothing Then
                AppendLine("} else {")
                IncreaseIndent()
                EmitBlock([if].ElseBody)
                DecreaseIndent()
            End If

            AppendLine("}")
        End Sub

        Private Sub EmitWhile(loopStmt As WhileStatement)
            If loopStmt.IsPostCondition Then
                AppendLine("do {")
                IncreaseIndent()
                EmitBlock(loopStmt.Body)
                DecreaseIndent()
                AppendLine($"}} while ({EmitExpression(loopStmt.Condition)});")
            Else
                AppendLine($"while ({EmitExpression(loopStmt.Condition)}) {{")
                IncreaseIndent()
                EmitBlock(loopStmt.Body)
                DecreaseIndent()
                AppendLine("}")
            End If
        End Sub

        ''' <summary>
        ''' for 的三段式：初值用赋值（变量已在函数体开头提升声明），
        ''' 步进直接写成 "变量 = 步进表达式"（步进表达式形如 i + 1）。
        ''' </summary>
        Private Sub EmitFor(loopStmt As ForStatement)
            Dim varName = CudaTypeMap.SafeName(loopStmt.VariableName)

            AppendLine($"for ({varName} = {EmitExpression(loopStmt.Initializer)}; " &
                       $"{EmitExpression(loopStmt.Condition)}; " &
                       $"{varName} = {EmitExpression(loopStmt.Increment)}) {{")
            IncreaseIndent()
            EmitBlock(loopStmt.Body)
            DecreaseIndent()
            AppendLine("}")
        End Sub

        ' ==================================================================
        ' 表达式
        ' ==================================================================

        Public Function EmitExpression(expr As Expression) As String
            If expr Is Nothing Then Return "0"

            Select Case expr.Kind
                Case SyntaxKind.Literal
                    Return CudaTypeMap.FormatLiteral(DirectCast(expr, LiteralExpression).Value)

                Case SyntaxKind.Parameter
                    Return CudaTypeMap.SafeName(DirectCast(expr, ParameterExpression).Name)

                Case SyntaxKind.Local
                    Return CudaTypeMap.SafeName(DirectCast(expr, LocalExpression).Name)

                Case SyntaxKind.Binary
                    Dim bin = DirectCast(expr, BinaryExpression)
                    Return $"{EmitOperand(bin.Left)} {CudaTypeMap.OperatorText(bin.Operator)} {EmitOperand(bin.Right)}"

                Case SyntaxKind.Unary
                    Dim un = DirectCast(expr, UnaryExpression)
                    Dim operand = EmitOperand(un.Operand)

                    If un.Operator = UnaryOperator.Negate Then Return $"(-{operand})"

                    Return $"(!{operand})"

                Case SyntaxKind.Convert
                    Dim conv = DirectCast(expr, ConvertExpression)
                    Dim typeName = CudaTypeMap.CudaType(conv.TargetType)

                    If typeName Is Nothing Then
                        Throw New NotSupportedException($"转换目标类型 {conv.TargetType?.FullName} 无法映射为 CUDA 类型")
                    End If

                    Return $"(({typeName}){EmitOperand(conv.Operand)})"

                Case SyntaxKind.Invoke
                    Return EmitCall(DirectCast(expr, CallExpression))

                Case SyntaxKind.ArrayIndex
                    Dim indexNode = DirectCast(expr, ArrayIndexExpression)
                    Return $"{EmitOperand(indexNode.Array)}[{EmitExpression(indexNode.Index)}]"

                Case SyntaxKind.Ternary
                    Dim ternary = DirectCast(expr, TernaryExpression)
                    Return $"({EmitExpression(ternary.Condition)} ? {EmitExpression(ternary.WhenTrue)} : {EmitExpression(ternary.WhenFalse)})"

                Case SyntaxKind.ArrayLength
                    Throw New NotSupportedException("CUDA 内核里拿不到托管数组的长度，请把它作为整型参数传入")

                Case Else
                    Throw New NotSupportedException($"表达式 {expr.Kind} 无法发射为 CUDA 代码")
            End Select
        End Function

        ''' <summary>
        ''' 嵌套的复合表达式加括号。宁可多括号也不赌 C 的运算符优先级与结合性。
        ''' </summary>
        Private Function EmitOperand(expr As Expression) As String
            Dim text = EmitExpression(expr)

            Select Case expr.Kind
                Case SyntaxKind.Binary, SyntaxKind.Unary, SyntaxKind.Ternary, SyntaxKind.Convert
                    Return "(" & text & ")"
                Case Else
                    Return text
            End Select
        End Function

        Private Function EmitCall(callNode As CallExpression) As String
            Dim cudaName As String = Nothing

            If Not CudaTypeMap.TryMapMathFunction(callNode.DeclaringTypeName,
                                                  callNode.MethodName,
                                                  CudaTypeMap.PrefersSingle(callNode),
                                                  cudaName) Then
                Throw New NotSupportedException(
                    $"调用 {callNode.FullName} 不在 CUDA 数学函数映射表内，无法发射为设备代码")
            End If

            Dim args = callNode.AllArguments.Select(Function(a) EmitExpression(a))

            Return $"{cudaName}({String.Join(", ", args)})"
        End Function
    End Class
End Namespace
