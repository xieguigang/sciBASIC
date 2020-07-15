#Region "Microsoft.VisualBasic::111d7c25571ec428aa24efc0b4a77dd4, Data_science\Mathematica\Math\Math\Scripting\Expression\Expression\Literal.vb"

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

    '     Class Literal
    ' 
    '         Properties: number
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Scripting.MathExpression.Impl

    Public Class Literal : Inherits Expression

        Public ReadOnly Property number As Double

        Sub New(text As String)
            Me.number = Val(text)
        End Sub

        Sub New(x As Double)
            Me.number = x
        End Sub

        Public Function GetNegative() As Literal
            Return New Literal(-1 * number)
        End Function

        Public Function GetReciprocal() As Literal
            Return New Literal(1 / number)
        End Function

        Public Overrides Function Evaluate(env As ExpressionEngine) As Double
            Return number
        End Function

        Public Overloads Shared Function Evaluate(left As Literal, op As Char, right As Literal) As Literal
            Dim a As Double = left.Evaluate(Nothing)
            Dim b As Double = right.Evaluate(Nothing)

            Select Case op
                Case "+" : Return a + b
                Case "-" : Return a - b
                Case "*" : Return a * b
                Case "/" : Return a / b
                Case "%" : Return a Mod b
                Case "^" : Return a ^ b
                Case Else
                    Throw New NotImplementedException(op)
            End Select
        End Function

        Public Overrides Function ToString() As String
            Return number
        End Function

        Public Overloads Shared Widening Operator CType(x As Double) As Literal
            Return New Literal(x)
        End Operator
    End Class
End Namespace
