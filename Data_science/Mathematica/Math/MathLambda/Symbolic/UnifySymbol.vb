#Region "Microsoft.VisualBasic::57bd05582d639f1491e93ee960f5a108, sciBASIC#\Data_science\Mathematica\Math\Math\Scripting\Symbolic\UnifySymbol.vb"

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

'   Total Lines: 27
'    Code Lines: 16
' Comment Lines: 5
'   Blank Lines: 6
'     File Size: 865 B


'     Class UnifySymbol
' 
'         Properties: factor, power
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: Evaluate, ToString
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
