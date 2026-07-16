' /********************************************************************************/
'
'     Module PolyExpansion
' 
'         Expand products and integer powers of sums into a fully distributed
'         polynomial form, e.g. (a+b)^2 -> a^2 + a*b + a*b + b^2 and
'         (a+b)*(c+d) -> a*c + a*d + b*c + b*d.
'
'     Author: xie.guigang@live.com
'     Copyright (c) 2018 GPL3 Licensed
'
' /********************************************************************************/

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Symbolic

    ''' <summary>
    ''' Expand products and powers of sums into distributed form.
    ''' </summary>
    Public Module PolyExpansion

        <Extension>
        Public Function Expands(expression As Expression) As Expression
            If expression Is Nothing Then
                Return Nothing
            ElseIf TypeOf expression Is Literal OrElse TypeOf expression Is SymbolExpression Then
                Return expression
            ElseIf TypeOf expression Is BinaryExpression Then
                Dim b = DirectCast(expression, BinaryExpression)
                Dim l = Expands(b.left)
                Dim r = Expands(b.right)

                Select Case b.operator
                    Case "*"c
                        Return distributeProduct(l, r)
                    Case "^"c
                        Dim n = NumericValue(r)
                        If n.HasValue AndAlso n.Value >= 0 AndAlso n.Value = CDbl(CInt(n.Value)) Then
                            Return expandPower(l, CInt(n.Value))
                        End If
                        Return Pow(l, r)
                    Case Else
                        Return New BinaryExpression(l, r, b.operator)
                End Select
            ElseIf TypeOf expression Is FunctionInvoke Then
                Dim f = DirectCast(expression, FunctionInvoke)
                Return New FunctionInvoke(f.funcName, f.parameters.Select(AddressOf Expands).ToArray)
            ElseIf TypeOf expression Is UnaryExpression Then
                Dim u = DirectCast(expression, UnaryExpression)
                Return New UnaryExpression With {.[operator] = u.operator, .value = Expands(u.value)}
            ElseIf TypeOf expression Is UnaryNot Then
                Dim n = DirectCast(expression, UnaryNot)
                Return New UnaryNot With {.value = Expands(n.value)}
            ElseIf TypeOf expression Is Factorial Then
                Dim fa = DirectCast(expression, Factorial)
                Return New Factorial(Expands(fa.factor).ToString)
            End If

            Return expression
        End Function

        Private Function distributeProduct(l As Expression, r As Expression) As Expression
            If TypeOf l Is BinaryExpression AndAlso (DirectCast(l, BinaryExpression).operator = "+"c OrElse DirectCast(l, BinaryExpression).operator = "-"c) Then
                Dim parts As New List(Of Expression)
                For Each t In FlattenSum(l)
                    parts.Add(Mul(t, r))
                Next
                Return combineSumRaw(parts)
            End If

            If TypeOf r Is BinaryExpression AndAlso (DirectCast(r, BinaryExpression).operator = "+"c OrElse DirectCast(r, BinaryExpression).operator = "-"c) Then
                Dim parts As New List(Of Expression)
                For Each t In FlattenSum(r)
                    parts.Add(Mul(l, t))
                Next
                Return combineSumRaw(parts)
            End If

            Return Mul(l, r)
        End Function

        Private Function expandPower(base As Expression, n As Integer) As Expression
            If n = 0 Then Return MakeLiteral(1)
            If n = 1 Then Return base

            Dim acc = base
            For i As Integer = 2 To n
                acc = distributeProduct(acc, base)
            Next
            Return acc
        End Function

        Private Function combineSumRaw(parts As List(Of Expression)) As Expression
            If parts.Count = 0 Then Return MakeLiteral(0)
            If parts.Count = 1 Then Return parts(0)

            Dim acc = parts(0)
            For i As Integer = 1 To parts.Count - 1
                acc = Add(acc, parts(i))
            Next
            Return acc
        End Function
    End Module
End Namespace
