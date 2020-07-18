#Region "Microsoft.VisualBasic::af9b381a2b51d965e27cf6a3c884e29b, Data_science\Mathematica\Math\MathLambda\Symbolic.vb"

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

    ' Module Symbolic
    ' 
    '     Function: isNormalized, (+4 Overloads) makeSimple, Simplify
    ' 
    ' Class UnifySymbol
    ' 
    '     Properties: factor, power
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Evaluate, ToString
    ' 
    ' /********************************************************************************/

#End Region


Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

''' <summary>
''' symbolic computation engine
''' </summary>
Public Module Symbolic

    Public Function Simplify(raw As Expression) As Expression
        If Not TypeOf raw Is BinaryExpression Then
            Return raw
        Else
            Return makeSimple(raw)
        End If
    End Function

    ''' <summary>
    ''' a op x
    ''' </summary>
    ''' <param name="left"></param>
    ''' <param name="right"></param>
    ''' <param name="bin"></param>
    ''' <returns></returns>
    Private Function makeSimple(left As Literal, right As Expression, bin As Char) As Expression
        If bin = "*"c Then
            If TypeOf right Is SymbolExpression Then
                Return New UnifySymbol(right) With {
                    .factor = left,
                    .power = New Literal(1)
                }
            End If
        End If
        Return New BinaryExpression(left, right, bin)
    End Function

    Private Function makeSimple(left As Expression, right As Literal, bin As Char) As Expression
        If bin = "+"c OrElse bin = "*"c Then
            ' 加法与乘法可以交换位置
            Return New BinaryExpression(right, left, bin)
        ElseIf bin = "-"c Then
            Return New BinaryExpression(right.GetNegative, left, bin)
        ElseIf bin = "/"c Then
            Return New BinaryExpression(right.GetReciprocal, left, bin)
        ElseIf bin = "^" Then
            If TypeOf left Is SymbolExpression Then
                Return New UnifySymbol(left) With {
                    .power = right
                }
            Else
                Return New BinaryExpression(left, right, bin)
            End If
        Else
            Return New BinaryExpression(left, right, bin)
        End If
    End Function

    Private Function makeSimple(raw As BinaryExpression) As Expression
        Dim bin As BinaryExpression = raw
        Dim left = Simplify(bin.left)
        Dim right = Simplify(bin.right)

        If TypeOf left Is Literal AndAlso TypeOf right Is Literal Then
            Return Literal.Evaluate(left, bin.operator, right)
        ElseIf TypeOf left Is Literal Then
            Return makeSimple(DirectCast(left, Literal), right, bin.operator)
        ElseIf TypeOf right Is Literal Then
            Return makeSimple(left, DirectCast(right, Literal), bin.operator)
        Else
            raw = New BinaryExpression(left, right, bin.operator)
        End If

        ' 都是binary表达式
        ' 并且都已经归一化为左边为常数，右边为变量
        Dim a As BinaryExpression = left
        Dim b As BinaryExpression = right

        If Not (a.isNormalized AndAlso b.isNormalized) Then
            Return raw
        ElseIf DirectCast(a.right, SymbolExpression).symbolName <> DirectCast(b.right, SymbolExpression).symbolName Then
            Return raw
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
    Private Function isNormalized(exp As BinaryExpression) As Boolean
        Return TypeOf exp.left Is Literal AndAlso TypeOf exp.right Is SymbolExpression
    End Function
End Module

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
        Return $"({factor} * ({symbolName} ^ {power}))"
    End Function
End Class
