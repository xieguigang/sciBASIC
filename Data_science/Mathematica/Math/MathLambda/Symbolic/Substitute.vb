#Region "Microsoft.VisualBasic::131ea09ac142708e63981a64929ef8d9, Data_science\Mathematica\Math\MathLambda\Symbolic\Substitute.vb"

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

    '   Total Lines: 85
    '    Code Lines: 53 (62.35%)
    ' Comment Lines: 25 (29.41%)
    '    - Xml Docs: 48.00%
    ' 
    '   Blank Lines: 7 (8.24%)
    '     File Size: 3.80 KB


    '     Module Substitute
    ' 
    '         Function: recurse, (+3 Overloads) Substitute
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' /********************************************************************************/
'
'     Module Substitute
' 
'         Symbolic substitution / replacement. Replaces every occurrence of a
'         variable symbol by another expression (or a numeric value). All
'         transformations are functional: the input tree is never mutated and
'         each replaced occurrence gets a fresh clone of the replacement.
'
'     Author: xie.guigang@live.com
'     Copyright (c) 2018 GPL3 Licensed
'
' /********************************************************************************/

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Symbolic

    Module Substitute

        ''' <summary>
        ''' Replace every occurrence of the symbol <paramref name="oldSymbol"/> by the
        ''' expression <paramref name="replacement"/>.
        ''' </summary>
        <Extension>
        Public Function Substitute(expr As Expression, oldSymbol$, replacement As Expression) As Expression
            Dim map As New Dictionary(Of String, Expression)
            map(oldSymbol) = replacement
            Return recurse(expr, map)
        End Function

        ''' <summary>
        ''' Replace every occurrence of the symbol <paramref name="oldSymbol"/> by the
        ''' numeric <paramref name="value"/>.
        ''' </summary>
        <Extension>
        Public Function Substitute(expr As Expression, oldSymbol$, value As Double) As Expression
            Return expr.Substitute(oldSymbol, New Literal(value))
        End Function

        ''' <summary>
        ''' Simultaneously replace every symbol listed in <paramref name="mapping"/> by
        ''' its associated expression.
        ''' </summary>
        <Extension>
        Public Function Substitute(expr As Expression, mapping As Dictionary(Of String, Expression)) As Expression
            Return recurse(expr, mapping)
        End Function

        Private Function recurse(expr As Expression, map As Dictionary(Of String, Expression)) As Expression
            If expr Is Nothing Then
                Return Nothing
            ElseIf TypeOf expr Is SymbolExpression Then
                Dim nm = DirectCast(expr, SymbolExpression).symbolName
                If map.ContainsKey(nm) Then
                    Return Clone(map(nm))
                End If
                Return expr
            ElseIf TypeOf expr Is Literal Then
                Return expr
            ElseIf TypeOf expr Is BinaryExpression Then
                Dim b = DirectCast(expr, BinaryExpression)
                Return New BinaryExpression(recurse(b.left, map), recurse(b.right, map), b.operator)
            ElseIf TypeOf expr Is FunctionInvoke Then
                Dim f = DirectCast(expr, FunctionInvoke)
                Return New FunctionInvoke(f.funcName, f.parameters.Select(Function(p) recurse(p, map)).ToArray)
            ElseIf TypeOf expr Is UnaryExpression Then
                Dim u = DirectCast(expr, UnaryExpression)
                Return New UnaryExpression With {.[operator] = u.operator, .value = recurse(u.value, map)}
            ElseIf TypeOf expr Is UnaryNot Then
                Dim n = DirectCast(expr, UnaryNot)
                Return New UnaryNot With {.value = recurse(n.value, map)}
            ElseIf TypeOf expr Is LogicalLiteral Then
                Return expr
            ElseIf TypeOf expr Is Factorial Then
                Dim fa = DirectCast(expr, Factorial)
                Return New Factorial(recurse(fa.factor, map).ToString)
            Else
                Return expr
            End If
        End Function
    End Module
End Namespace

