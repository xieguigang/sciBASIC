#Region "Microsoft.VisualBasic::b7264459fc432f41ff7af91c66598588, Microsoft.VisualBasic.Core\src\Scripting\TokenIcer\OperatorExpression.vb"

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

    '   Total Lines: 115
    '    Code Lines: 104 (90.43%)
    ' Comment Lines: 4 (3.48%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (6.09%)
    '     File Size: 5.88 KB


    '     Module OperatorExpression
    ' 
    '         Properties: Linq2Symbols, opName2Linq
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: __linq2Symbols, __opName2Linq
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq.Expressions

Namespace Scripting.TokenIcer

    Public Module OperatorExpression

        ''' <summary>
        ''' Linq Type to operator symbols.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Linq2Symbols As New Dictionary(Of ExpressionType, String)
        Public ReadOnly Property opName2Linq As New Dictionary(Of String, ExpressionType)

        Sub New()
            Call __opName2Linq()
            Call __linq2Symbols()
        End Sub

        Const op_LessThan$ = NameOf(op_LessThan)
        Const op_GreaterThan$ = NameOf(op_GreaterThan)
        Const op_LessThanOrEqual$ = NameOf(op_LessThanOrEqual)
        Const op_GreaterThanOrEqual$ = NameOf(op_GreaterThanOrEqual)
        Const op_Subtraction$ = NameOf(op_Subtraction)
        Const op_Division$ = NameOf(op_Division)
        Const op_Implicit$ = NameOf(op_Implicit)
        Const op_UnaryPlus$ = NameOf(op_UnaryPlus)
        Const op_Addition$ = NameOf(op_Addition)
        Const op_LeftShift$ = NameOf(op_LeftShift)
        Const op_Equality$ = NameOf(op_Equality)
        Const op_Inequality$ = NameOf(op_Inequality)
        Const op_Concatenate$ = NameOf(op_Concatenate)
        Const op_Multiply$ = NameOf(op_Multiply)
        Const op_Exponent$ = NameOf(op_Exponent)
        Const op_Modulus$ = NameOf(op_Modulus)
        Const op_Like$ = NameOf(op_Like)
        Const op_ExclusiveOr$ = NameOf(op_ExclusiveOr)
        Const op_BitwiseAnd$ = NameOf(op_BitwiseAnd)
        Const op_BitwiseOr$ = NameOf(op_BitwiseOr)
        Const op_IntegerDivision$ = NameOf(op_IntegerDivision)
        Const op_UnaryNegation$ = NameOf(op_UnaryNegation)

        Private Sub __opName2Linq()
            With opName2Linq
                Call .Add(op_Addition, ExpressionType.Add)
                Call .Add(op_BitwiseAnd, ExpressionType.And)
                Call .Add(op_BitwiseOr, ExpressionType.Or)
                Call .Add(op_Division, ExpressionType.Divide)
                Call .Add(op_Equality, ExpressionType.Equal)
                Call .Add(op_ExclusiveOr, ExpressionType.ExclusiveOr)
                Call .Add(op_Exponent, ExpressionType.Power)
                Call .Add(op_GreaterThan, ExpressionType.GreaterThan)
                Call .Add(op_GreaterThanOrEqual, ExpressionType.GreaterThanOrEqual)
                Call .Add(op_Implicit, ExpressionType.Convert)
                Call .Add(op_Inequality, ExpressionType.NotEqual)
                Call .Add(op_LeftShift, ExpressionType.LeftShift)
                Call .Add(op_LessThan, ExpressionType.LessThan)
                Call .Add(op_LessThanOrEqual, ExpressionType.LessThanOrEqual)
                Call .Add(op_Modulus, ExpressionType.Modulo)
                Call .Add(op_Multiply, ExpressionType.Multiply)
                Call .Add(op_Subtraction, ExpressionType.Subtract)
                Call .Add(op_UnaryPlus, ExpressionType.UnaryPlus)
                Call .Add(op_UnaryNegation, ExpressionType.Negate)
            End With
        End Sub

        Private Sub __linq2Symbols()
            With Linq2Symbols
                Call .Add(ExpressionType.Add, "+")
                Call .Add(ExpressionType.AddAssign, "+=")
                Call .Add(ExpressionType.AddAssignChecked, "+=")
                Call .Add(ExpressionType.AddChecked, "+")
                Call .Add(ExpressionType.And, "And")
                Call .Add(ExpressionType.AndAlso, "AndAlso")
                Call .Add(ExpressionType.Assign, "=")
                Call .Add(ExpressionType.Convert, "CType")
                Call .Add(ExpressionType.ConvertChecked, "CType")
                Call .Add(ExpressionType.Decrement, "-")
                Call .Add(ExpressionType.Divide, "/")
                Call .Add(ExpressionType.DivideAssign, "/=")
                Call .Add(ExpressionType.Equal, "=")
                Call .Add(ExpressionType.ExclusiveOr, "Xor")
                Call .Add(ExpressionType.GreaterThan, ">")
                Call .Add(ExpressionType.GreaterThanOrEqual, ">=")
                Call .Add(ExpressionType.Increment, "+")
                Call .Add(ExpressionType.IsFalse, "IsFalse")
                Call .Add(ExpressionType.IsTrue, "IsTrue")
                Call .Add(ExpressionType.LeftShift, "<<")
                Call .Add(ExpressionType.LeftShiftAssign, "<<=")
                Call .Add(ExpressionType.LessThan, "<")
                Call .Add(ExpressionType.LessThanOrEqual, "<=")
                Call .Add(ExpressionType.Modulo, "Mod")
                Call .Add(ExpressionType.Multiply, "*")
                Call .Add(ExpressionType.MultiplyAssign, "*=")
                Call .Add(ExpressionType.MultiplyAssignChecked, "*=")
                Call .Add(ExpressionType.MultiplyChecked, "*")
                Call .Add(ExpressionType.Negate, "-")
                Call .Add(ExpressionType.NegateChecked, "-")
                Call .Add(ExpressionType.Not, "Not")
                Call .Add(ExpressionType.NotEqual, "<>")
                Call .Add(ExpressionType.Or, "Or")
                Call .Add(ExpressionType.OrElse, "OrElse")
                Call .Add(ExpressionType.Power, "^")
                Call .Add(ExpressionType.PowerAssign, "^=")
                Call .Add(ExpressionType.RightShift, ">>")
                Call .Add(ExpressionType.RightShiftAssign, ">>=")
                Call .Add(ExpressionType.Subtract, "-")
                Call .Add(ExpressionType.SubtractAssign, "-=")
                Call .Add(ExpressionType.SubtractAssignChecked, "-=")
                Call .Add(ExpressionType.SubtractChecked, "-")
                Call .Add(ExpressionType.TypeAs, "TryCast")
                Call .Add(ExpressionType.UnaryPlus, "+")
            End With
        End Sub
    End Module
End Namespace
