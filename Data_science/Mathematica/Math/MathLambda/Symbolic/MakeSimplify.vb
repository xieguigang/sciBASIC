Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Symbolic

    Module MakeSimplify

        ''' <summary>
        ''' a op x
        ''' </summary>
        ''' <param name="left"></param>
        ''' <param name="right"></param>
        ''' <param name="bin">the math binary operator</param>
        ''' <returns></returns>
        Private Function makeSimple(left As Literal, right As Expression, bin As Char) As Expression
            If bin = "*"c Then
                If left = 0 Then
                    Return Literal.Zero
                End If

                If TypeOf right Is SymbolExpression Then
                    Return New UnifySymbol(right) With {
                        .factor = left,
                        .power = New Literal(1)
                    }
                End If
            ElseIf bin = "+"c Then
                If left = 0 Then
                    ' 0 + exp = exp
                    Return right
                End If
            End If

            Return New BinaryExpression(left, right, bin)
        End Function

        Private Function makeSimple(left As Expression, right As Literal, bin As Char) As Expression
            If bin = "+"c OrElse bin = "*"c Then
                ' 加法与乘法可以交换位置
                Return makeSimple(New BinaryExpression(right, left, bin))
            ElseIf bin = "-"c Then
                Return makeSimple(New BinaryExpression(right.GetNegative, left, bin))
            ElseIf bin = "/"c Then
                Return makeSimple(New BinaryExpression(right.GetReciprocal, left, bin))
            ElseIf bin = "^" Then
                If TypeOf left Is SymbolExpression Then
                    Return New UnifySymbol(left) With {
                        .power = right
                    }.GetSimplify
                Else
                    Return New BinaryExpression(left, right, bin)
                End If
            Else
                Return makeSimple(New BinaryExpression(left, right, bin))
            End If
        End Function

        Friend Function makeSimple(raw As BinaryExpression) As Expression
            Dim bin As BinaryExpression = raw

            If raw.isNormalized Then
                Return raw
            End If

            Dim left = Simplify(bin.left)
            Dim right = Simplify(bin.right)

            If TypeOf left Is Literal AndAlso TypeOf right Is Literal Then
                Return Literal.Evaluate(left, bin.operator, right)
            ElseIf TypeOf left Is Literal Then
                Return makeSimple(DirectCast(left, Literal), right, bin.operator)
            ElseIf TypeOf right Is Literal Then
                Return makeSimple(left, DirectCast(right, Literal), bin.operator)
            ElseIf (TypeOf left Is SymbolExpression AndAlso TypeOf right Is SymbolExpression) AndAlso bin.operator = "*" Then
                If (DirectCast(left, SymbolExpression).symbolName = DirectCast(right, SymbolExpression).symbolName) Then
                    Return New BinaryExpression(left, New Literal(2), "^")
                Else
                    raw = New BinaryExpression(left, right, bin.operator)
                End If
            Else
                raw = New BinaryExpression(left, right, bin.operator)
            End If

            ' 都是binary表达式
            ' 并且都已经归一化为左边为常数，右边为变量
            Dim a As BinaryExpression
            Dim b As BinaryExpression

            If TypeOf left Is UnifySymbol Then
                a = CType(DirectCast(left, UnifySymbol), BinaryExpression)
            Else
                a = left
            End If
            If TypeOf right Is UnifySymbol Then
                b = CType(DirectCast(right, UnifySymbol), BinaryExpression)
            Else
                b = right
            End If

            If Not (a.isNormalized AndAlso b.isNormalized) Then
                Return raw
            ElseIf a.operator <> b.operator Then
                Return raw
            Else
                If TypeOf a.right Is SymbolExpression AndAlso TypeOf b.right Is SymbolExpression Then
                    If DirectCast(a.right, SymbolExpression).symbolName <> DirectCast(b.right, SymbolExpression).symbolName Then
                        Return raw
                    End If
                End If
            End If

            Dim symbol As New SymbolExpression(DirectCast(a.right, SymbolExpression).symbolName)

            Return makeSimple(a, b, bin.operator, symbol)
        End Function

        Private Function makeSimple(a As BinaryExpression, b As BinaryExpression, bin As Char, symbol As SymbolExpression)
            Select Case bin
                Case "+"c
                    Dim aplusb As Literal = Literal.Evaluate(a.left, "+", b.left)
                    Dim result As New BinaryExpression(aplusb, symbol, "*")
                    Return result
                Case "-"c
                    Dim aminusb As Literal = Literal.Evaluate(a.left, "-", b.left)
                    Dim result As New BinaryExpression(aminusb, symbol, "*")
                    Return result
                Case "*"c
                    Dim atimesb As Literal = Literal.Evaluate(a.left, "*", b.left)
                    Dim result As New BinaryExpression(atimesb, New BinaryExpression(symbol, New Literal(2), "^"), "*")
                    Return result
                Case "/"c
                    Dim adivb As Literal = Literal.Evaluate(a.left, "/", b.left)
                    Return adivb
                Case "%"c
                    Return New BinaryExpression(a, b, bin)
                Case "^"c
                    Return New BinaryExpression(a, b, bin)
                Case Else
                    Throw New NotImplementedException(bin.ToString)
            End Select
        End Function

        <Extension>
        Friend Function isNormalized(exp As BinaryExpression) As Boolean
            If exp.operator <> "^" Then
                Return TypeOf exp.left Is Literal AndAlso TypeOf exp.right Is SymbolExpression
            Else
                Return TypeOf exp.right Is Literal AndAlso TypeOf exp.left Is SymbolExpression
            End If
        End Function

    End Module

End Namespace