#Region "Microsoft.VisualBasic::cf4f863597edd1c0a8f8c1d46fa8d261, Data_science\Mathematica\Math\MathLambda\Symbolic\PolyExpansion.vb"

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

    '   Total Lines: 113
    '    Code Lines: 91 (80.53%)
    ' Comment Lines: 3 (2.65%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 19 (16.81%)
    '     File Size: 3.94 KB


    '     Module PolyExpansion
    ' 
    '         Function: Expands, ExpandsBinary, ExpandsPower, simple
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Symbolic

    ''' <summary>
    ''' (a+b)^2 = a^2 + ab + ab + b ^ 2
    ''' </summary>
    Public Module PolyExpansion

        <Extension>
        Public Function Expands(expression As Expression) As Expression
            If Not TypeOf expression Is BinaryExpression Then
                Return expression
            Else
                Return ExpandsBinary(expression)
            End If
        End Function

        Private Function ExpandsBinary(bin As BinaryExpression) As Expression
            Select Case bin.operator
                Case "^"
                    If TypeOf bin.right Is Literal AndAlso DirectCast(bin.right, Literal).isInteger Then
                        If TypeOf bin.left Is BinaryExpression Then
                            Return ExpandsPower(bin.left, bin.right.Evaluate(Nothing))
                        Else
                            Return bin
                        End If
                    Else
                        Return bin
                    End If
                Case "+", "-"
                    Return bin
                Case "*", "/"
                    Return bin
                Case Else
                    Throw New NotImplementedException(bin.operator)
            End Select
        End Function

        Private Function ExpandsPower(base As BinaryExpression, power As Integer) As Expression
            If Not (base.operator = "+" OrElse base.operator = "-") Then
                Return BinaryExpression.Power(base, power)
            ElseIf power = 1 Then
                Return base
            ElseIf power = 0 Then
                Return New Literal(1)
            End If

            Dim a As Expression = base.left
            Dim b As Expression = base.right

            If base.operator = "-" Then
                b = New UnaryExpression With {.[operator] = "-", .value = Expands(b)}
            End If

            Dim combines As New List(Of Expression())

            combines.Add({a, a})
            combines.Add({a, b})
            combines.Add({b, a})
            combines.Add({b, b})

            For i As Integer = 3 To power
                Dim list = combines.ToArray
                Dim empty As New List(Of Expression())

                For Each line As Expression() In list
                    empty.Add(line.JoinIterates({a}).ToArray)
                Next
                For Each line As Expression() In list
                    empty.Add(line.JoinIterates({b}).ToArray)
                Next

                combines = empty
            Next

            Dim simplify = combines _
                .GroupBy(Function(r)
                             Return r.Select(Function(t) t.ToString).OrderBy(Function(t) t).JoinBy("*")
                         End Function) _
                .Select(Function(tokens)
                            Return simple(tokens)
                        End Function) _
                .ToArray

            Dim bin As Expression = simplify.First

            For Each i In simplify.Skip(1)
                bin = New BinaryExpression(bin, i, "+")
            Next

            Return bin
        End Function

        Private Function simple(group As IGrouping(Of String, Expression())) As Expression
            Dim all = group.ToArray
            Dim merge = all.First
            Dim PI As Expression = merge.First

            For Each i In merge.Skip(1)
                PI = New BinaryExpression(PI, i, "*")
            Next

            If all.Length = 1 Then
                Return PI
            Else
                Return New BinaryExpression(New Literal(all.Length), PI, "*")
            End If
        End Function
    End Module
End Namespace
