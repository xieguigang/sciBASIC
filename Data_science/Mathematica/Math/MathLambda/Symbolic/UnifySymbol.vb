#Region "Microsoft.VisualBasic::a32e52df8e46c8aa8362b23b8eb2774a, Data_science\Mathematica\Math\MathLambda\Symbolic\UnifySymbol.vb"

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

    '   Total Lines: 95
    '    Code Lines: 65 (68.42%)
    ' Comment Lines: 17 (17.89%)
    '    - Xml Docs: 52.94%
    ' 
    '   Blank Lines: 13 (13.68%)
    '     File Size: 3.38 KB


    '     Class UnifySymbol
    ' 
    '         Properties: factor, power
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, GetExpressionType, GetSimplify, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Symbolic

    ''' <summary>
    ''' a unify symbol model
    ''' 
    ''' a * (x ^ n)
    ''' </summary>
    Public Class UnifySymbol : Inherits SymbolExpression

        Public Property factor As Expression = New Literal(1)
        Public Property power As Expression = New Literal(1)

        Public Sub New(symbol As SymbolExpression)
            MyBase.New(symbol.symbolName)
        End Sub

        Public Function GetExpressionType() As Type
            If factor = Literal.Zero OrElse power = Literal.Zero Then
                ' 0 * x ^ n = 0, or
                ' m * x ^ 0 = m
                Return GetType(Literal)
            ElseIf factor = Literal.One AndAlso power = Literal.One Then
                ' 1 * x ^ 1 = x
                Return GetType(SymbolExpression)
            Else
                Return GetType(BinaryExpression)
            End If
        End Function

        Public Function GetSimplify() As Expression
            If factor = Literal.Zero Then
                ' 0 * x ^ n = 0
                Return Literal.Zero
            ElseIf power = Literal.Zero Then
                ' m * x ^ 0 = m
                Return factor
            ElseIf factor = Literal.One AndAlso power = Literal.One Then
                ' 1 * x ^ 1 = x
                Return New SymbolExpression(symbolName)
            Else
                Dim bin = CType(Me, BinaryExpression)

                If bin.isNormalized Then
                    Return bin
                Else
                    Return MakeSimplify.makeSimple(bin)
                End If
            End If
        End Function

        Public Overrides Function Evaluate(env As ExpressionEngine) As Double
            Return factor.Evaluate(env) * (MyBase.Evaluate(env) ^ power.Evaluate(env))
        End Function

        Public Overrides Function ToString() As String
            If power = Literal.One Then
                If factor = Literal.One Then
                    Return symbolName
                End If
                Return $"({factor} * {symbolName})"
            ElseIf power = Literal.Zero Then
                Return factor.ToString
            ElseIf factor = Literal.One Then
                Return $"({symbolName} ^ {power})"
            Else
                Return $"({factor} * ({symbolName} ^ {power}))"
            End If
        End Function

        ''' <summary>
        ''' factor * [x ^ n]
        ''' </summary>
        ''' <param name="symbol"></param>
        ''' <returns></returns>
        Public Shared Narrowing Operator CType(symbol As UnifySymbol) As BinaryExpression
            Dim left As Expression = symbol.factor
            Dim right As Expression

            If symbol.power = Literal.One Then
                right = New SymbolExpression(symbol.symbolName)
            ElseIf symbol.power = Literal.Zero Then
                ' x ^ 0 = 1
                right = Literal.One
            Else
                right = New BinaryExpression(New SymbolExpression(symbol.symbolName), symbol.power, op:="^")
            End If

            Return New BinaryExpression(left, right, op:="*")
        End Operator

    End Class
End Namespace
