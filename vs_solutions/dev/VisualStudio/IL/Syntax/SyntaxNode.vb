#Region "Microsoft.VisualBasic::91a473c9cd3ecbcae695f318f922cbec, vs_solutions\dev\VisualStudio\IL\Syntax\SyntaxNode.vb"

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

    '   Total Lines: 161
    '    Code Lines: 113 (70.19%)
    ' Comment Lines: 32 (19.88%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 16 (9.94%)
    '     File Size: 5.69 KB


    '     Enum SyntaxKind
    ' 
    '         ArrayIndex, ArrayLength, Assignment, Binary, Block
    '         BreakStmt, ContinueStmt, Convert, ExpressionStatement, ForStmt
    '         IfStmt, Invoke, Literal, Local, Nop
    '         Parameter, Phi, ReturnStmt, Ternary, Unary
    '         VariableDeclaration, WhileStmt
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum BinaryOperator
    ' 
    '         Add, BitwiseAnd, BitwiseOr, BitwiseXor, Divide
    '         Equal, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual
    '         Modulo, Multiply, NotEqual, ShiftLeft, ShiftRight
    '         ShortCircuitAnd, ShortCircuitOr, Subtract
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum UnaryOperator
    ' 
    '         LogicalNot, Negate, OnesComplement
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class SyntaxNode
    ' 
    '         Properties: IsExpression, Kind, Type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class Expression
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class Statement
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Module OperatorPrecedence
    ' 
    '         Function: Negate, OfBinary
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' IL 反编译产物的自定义表达式/语句节点模型（AST）
'
' 这一层是"纯数据"：既不知道 IL，也不知道 CUDA。
'   IL/Decompiler 负责把 CIL 指令流归约成这里的节点；
'   IL/SyntaxWriter 把它打印成类 VB 伪代码（调试 / 测试快照）；
'   cuda/ILCuda/IL2Cuda 把它发射成 CUDA C 源码。
'
' 每个节点都带 Kind（用于 Select Case 分派）与 Type（静态结果类型，
' 供 CUDA 侧做 float / double / int 的类型映射）。
' ---------------------------------------------------------------------------

Namespace IL

    ''' <summary>AST 节点类型</summary>
    Public Enum SyntaxKind
        ' ---- 表达式 ----
        Literal
        Parameter
        Local
        Binary
        Unary
        Convert
        ' 方法调用（C# 里的 Call 是关键字，这里改名 Invoke）
        Invoke
        ArrayIndex
        ArrayLength
        Ternary
        Phi
        ' ---- 语句 ----
        Block
        VariableDeclaration
        Assignment
        ExpressionStatement
        ReturnStmt
        IfStmt
        WhileStmt
        ForStmt
        BreakStmt
        ContinueStmt
        Nop
    End Enum

    ''' <summary>二元运算符</summary>
    Public Enum BinaryOperator
        Add
        Subtract
        Multiply
        Divide
        Modulo
        BitwiseAnd
        BitwiseOr
        BitwiseXor
        ShiftLeft
        ShiftRight
        Equal
        NotEqual
        LessThan
        LessThanOrEqual
        GreaterThan
        GreaterThanOrEqual
        ' 短路与（由 brfalse 菱形折叠而来；AndAlso 是 VB 保留字，故改名）
        ShortCircuitAnd
        ' 短路或（由 brtrue 菱形折叠而来；OrElse 是 VB 保留字，故改名）
        ShortCircuitOr
    End Enum

    ''' <summary>一元运算符</summary>
    Public Enum UnaryOperator
        ''' <summary>算术取负 neg</summary>
        Negate
        ''' <summary>逻辑/按位取反 not</summary>
        LogicalNot
        ''' <summary>按位取反（与 LogicalNot 在 IL 层同指令，保留以区分意图）</summary>
        OnesComplement
    End Enum

    ''' <summary>所有 AST 节点的基类</summary>
    Public MustInherit Class SyntaxNode

        ''' <summary>节点类型</summary>
        Public ReadOnly Property Kind As SyntaxKind

        ''' <summary>该节点的静态结果类型；未能推断时为 Nothing</summary>
        Public Property Type As System.Type

        Protected Sub New(kind As SyntaxKind)
            Me.Kind = kind
        End Sub

        ''' <summary>该节点在语义上是否产生一个值</summary>
        Public ReadOnly Property IsExpression As Boolean
            Get
                Return TypeOf Me Is Expression
            End Get
        End Property
    End Class

    ''' <summary>表达式节点基类</summary>
    Public MustInherit Class Expression : Inherits SyntaxNode

        Protected Sub New(kind As SyntaxKind)
            MyBase.New(kind)
        End Sub
    End Class

    ''' <summary>语句节点基类</summary>
    Public MustInherit Class Statement : Inherits SyntaxNode

        Protected Sub New(kind As SyntaxKind)
            MyBase.New(kind)
        End Sub
    End Class

    ''' <summary>
    ''' 一个二元运算符的优先级（数字越大越紧），供发射器决定是否需要加括号。
    ''' </summary>
    Public Module OperatorPrecedence

        Public Function OfBinary(op As BinaryOperator) As Integer
            Select Case op
                Case BinaryOperator.ShortCircuitOr
                    Return 1
                Case BinaryOperator.ShortCircuitAnd
                    Return 2
                Case BinaryOperator.BitwiseOr
                    Return 3
                Case BinaryOperator.BitwiseXor
                    Return 4
                Case BinaryOperator.BitwiseAnd
                    Return 5
                Case BinaryOperator.Equal, BinaryOperator.NotEqual
                    Return 6
                Case BinaryOperator.LessThan, BinaryOperator.LessThanOrEqual,
                     BinaryOperator.GreaterThan, BinaryOperator.GreaterThanOrEqual
                    Return 7
                Case BinaryOperator.ShiftLeft, BinaryOperator.ShiftRight
                    Return 8
                Case BinaryOperator.Add, BinaryOperator.Subtract
                    Return 9
                Case BinaryOperator.Multiply, BinaryOperator.Divide, BinaryOperator.Modulo
                    Return 10
                Case Else
                    Return 0
            End Select
        End Function

        ''' <summary>比较运算取反（用于把 clt 之类的结果取反成 &gt;= 形态）</summary>
        Public Function Negate(op As BinaryOperator) As BinaryOperator
            Select Case op
                Case BinaryOperator.Equal : Return BinaryOperator.NotEqual
                Case BinaryOperator.NotEqual : Return BinaryOperator.Equal
                Case BinaryOperator.LessThan : Return BinaryOperator.GreaterThanOrEqual
                Case BinaryOperator.LessThanOrEqual : Return BinaryOperator.GreaterThan
                Case BinaryOperator.GreaterThan : Return BinaryOperator.LessThanOrEqual
                Case BinaryOperator.GreaterThanOrEqual : Return BinaryOperator.LessThan
                Case Else : Return op
            End Select
        End Function
    End Module
End Namespace
