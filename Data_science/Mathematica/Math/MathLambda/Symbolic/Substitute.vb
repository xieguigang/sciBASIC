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
