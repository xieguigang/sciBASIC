#Region "Microsoft.VisualBasic::e8f564de070f6278e09b9c113300a338, Data_science\Mathematica\Math\MathLambda\Symbolic\Rationalize.vb"

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

    '   Total Lines: 250
    '    Code Lines: 161 (64.40%)
    ' Comment Lines: 48 (19.20%)
    '    - Xml Docs: 37.50%
    ' 
    '   Blank Lines: 41 (16.40%)
    '     File Size: 10.68 KB


    '     Module Rationalizer
    ' 
    '         Function: conjugateOf, containsRadical, distributeAll, distributeMul, isSumExpr
    '                   Rationalize, rebuildSum, recurse, simplifyInner
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' /********************************************************************************/
'
'     Module Rationalizer
' 
'         Denominator rationalisation. Removes radicals and the imaginary unit
'         from denominators by multiplying the numerator and denominator by the
'         conjugate of the irrational part. The transformation is applied
'         recursively (with a bounded depth) so that multiple radicals are cleared
'         one at a time: 1/(a + sqrt(b)) -> (a - sqrt(b))/(a^2 - b),
'         1/(sqrt(a) + sqrt(b)) -> (sqrt(a) - sqrt(b))/(a - b) and the complex
'         form 1/(a + b*i) -> (a - b*i)/(a^2 + b^2).
'
'     Author: xie.guigang@live.com
'     Copyright (c) 2018 GPL3 Licensed
'
' /********************************************************************************/

Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Symbolic

    ''' <summary>
    ''' Rationalise the denominators of an expression: eliminate square roots and
    ''' the imaginary unit from every denominator by conjugate multiplication.
    ''' </summary>
    Friend Module Rationalizer

        Private Const MAX_DEPTH As Integer = 12

        ''' <summary>
        ''' Rationalise every denominator in <paramref name="expr"/>.
        ''' </summary>
        Friend Function Rationalize(expr As Expression) As Expression
            Return recurse(expr, 0)
        End Function

        ' ------------------------------------------------------------------
        ' Recursive rationalisation
        ' ------------------------------------------------------------------

        Private Function recurse(expr As Expression, depth As Integer) As Expression
            If expr Is Nothing OrElse depth > MAX_DEPTH Then
                Return expr
            ElseIf TypeOf expr Is Literal OrElse TypeOf expr Is SymbolExpression Then
                Return expr
            End If

            If TypeOf expr Is BinaryExpression Then
                Dim b = DirectCast(expr, BinaryExpression)

                If b.operator = "/"c Then
                    Dim num = recurse(b.left, depth)
                    Dim den = recurse(b.right, depth)

                    If containsRadical(den) Then
                        Dim g = conjugateOf(den)
                        If g IsNot Nothing Then
                            Dim newExpr = Div(Mul(num, Clone(g)), Mul(den, Clone(g)))
                            ' multiply out and simplify, then continue to clear any
                            ' remaining radicals that may have surfaced.
                            Return recurse(simplifyInner(newExpr), depth + 1)
                        End If
                    End If

                    Return Div(num, den)
                End If

                Return New BinaryExpression(recurse(b.left, depth), recurse(b.right, depth), b.operator)
            End If

            If TypeOf expr Is FunctionInvoke Then
                Dim f = DirectCast(expr, FunctionInvoke)
                Return New FunctionInvoke(f.funcName, f.parameters.Select(Function(p) recurse(p, depth)).ToArray)
            End If

            If TypeOf expr Is UnaryExpression Then
                Dim u = DirectCast(expr, UnaryExpression)
                Return New UnaryExpression With {.[operator] = u.operator, .value = recurse(u.value, depth)}
            End If

            If TypeOf expr Is UnaryNot Then
                Dim n = DirectCast(expr, UnaryNot)
                Return New UnaryNot With {.value = recurse(n.value, depth)}
            End If

            If TypeOf expr Is Factorial Then
                Dim fa = DirectCast(expr, Factorial)
                Return New Factorial(recurse(fa.factor, depth).ToString)
            End If

            Return expr
        End Function

        ' ------------------------------------------------------------------
        ' Radical detection
        ' ------------------------------------------------------------------

        Private Function containsRadical(expr As Expression) As Boolean
            If expr Is Nothing Then
                Return False
            ElseIf TypeOf expr Is FunctionInvoke Then
                Dim f = DirectCast(expr, FunctionInvoke)
                If f.funcName = "sqrt" Then Return True
                For Each p In f.parameters
                    If containsRadical(p) Then Return True
                Next
                Return False
            ElseIf TypeOf expr Is SymbolExpression Then
                Dim nm = DirectCast(expr, SymbolExpression).symbolName
                Return nm = "i" OrElse nm = "I"
            ElseIf TypeOf expr Is BinaryExpression Then
                Dim b = DirectCast(expr, BinaryExpression)
                If b.operator = "^"c Then
                    Dim e = NumericValue(b.right)
                    If e.HasValue AndAlso System.Math.Abs(e.Value - 0.5) < 1.0E-9 Then Return True
                End If
                Return containsRadical(b.left) OrElse containsRadical(b.right)
            ElseIf TypeOf expr Is UnaryExpression Then
                Return containsRadical(DirectCast(expr, UnaryExpression).value)
            ElseIf TypeOf expr Is UnaryNot Then
                Return containsRadical(DirectCast(expr, UnaryNot).value)
            ElseIf TypeOf expr Is Factorial Then
                Return containsRadical(DirectCast(expr, Factorial).factor)
            End If
            Return False
        End Function

        ' ------------------------------------------------------------------
        ' Conjugate construction
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' Build the conjugate of <paramref name="den"/> by flipping the sign of the
        ''' first additive term that contains a radical (or the imaginary unit). This
        ''' single flip removes that radical from the product den*conjugate, after
        ''' which the recursion clears any remaining radicals.
        ''' </summary>
        Private Function conjugateOf(den As Expression) As Expression
            Dim terms = FlattenSum(den)
            Dim idx = -1

            For i As Integer = 0 To terms.Count - 1
                If containsRadical(terms(i)) Then
                    idx = i
                    Exit For
                End If
            Next

            If idx < 0 Then Return Nothing

            Dim conjTerms As New List(Of Expression)
            For i As Integer = 0 To terms.Count - 1
                If i = idx Then
                    conjTerms.Add(Negate(terms(i)))
                Else
                    conjTerms.Add(terms(i))
                End If
            Next

            Return rebuildSum(conjTerms)
        End Function

        Private Function rebuildSum(terms As List(Of Expression)) As Expression
            If terms.Count = 0 Then Return MakeLiteral(0)
            If terms.Count = 1 Then Return terms(0)

            Dim acc = terms(0)
            For i As Integer = 1 To terms.Count - 1
                acc = Add(acc, terms(i))
            Next
            Return acc
        End Function

        ' ------------------------------------------------------------------
        ' Local simplification (fully distribute products then simplify)
        ' ------------------------------------------------------------------

        Private Function simplifyInner(expr As Expression) As Expression
            Return MakeSimplify.simplifyRaw(distributeAll(expr))
        End Function

        ''' <summary>
        ''' Fully expand every product of sums (FOIL) so that a conjugate product
        ''' such as (a + b*sqrt(c)) * (a - b*sqrt(c)) becomes the rational sum
        ''' a^2 - b^2*c, which the shared simplifier then reduces to a number.
        ''' </summary>
        Private Function distributeAll(expr As Expression) As Expression
            If expr Is Nothing Then
                Return expr
            ElseIf TypeOf expr Is Literal OrElse TypeOf expr Is SymbolExpression Then
                Return expr
            End If

            If TypeOf expr Is BinaryExpression Then
                Dim b = DirectCast(expr, BinaryExpression)
                Dim l = distributeAll(b.left)
                Dim r = distributeAll(b.right)
                If b.operator = "*"c Then
                    Return distributeMul(l, r)
                End If
                Return New BinaryExpression(l, r, b.operator)
            End If

            If TypeOf expr Is FunctionInvoke Then
                Dim f = DirectCast(expr, FunctionInvoke)
                Return New FunctionInvoke(f.funcName, f.parameters.Select(AddressOf distributeAll).ToArray)
            End If

            If TypeOf expr Is UnaryExpression Then
                Dim u = DirectCast(expr, UnaryExpression)
                Return New UnaryExpression With {.[operator] = u.operator, .value = distributeAll(u.value)}
            End If

            If TypeOf expr Is UnaryNot Then
                Dim n = DirectCast(expr, UnaryNot)
                Return New UnaryNot With {.value = distributeAll(n.value)}
            End If

            Return expr
        End Function

        Private Function isSumExpr(e As Expression) As Boolean
            Return TypeOf e Is BinaryExpression AndAlso DirectCast(e, BinaryExpression).operator = "+"c
        End Function

        Private Function distributeMul(l As Expression, r As Expression) As Expression
            If isSumExpr(l) Then
                Dim b = DirectCast(l, BinaryExpression)
                Return Add(distributeMul(b.left, r), distributeMul(b.right, r))
            End If

            If TypeOf l Is BinaryExpression AndAlso DirectCast(l, BinaryExpression).operator = "-"c Then
                Dim b = DirectCast(l, BinaryExpression)
                Return distributeMul(Add(b.left, Negate(b.right)), r)
            End If

            If isSumExpr(r) Then
                Dim b = DirectCast(r, BinaryExpression)
                Return Add(distributeMul(l, b.left), distributeMul(l, b.right))
            End If

            If TypeOf r Is BinaryExpression AndAlso DirectCast(r, BinaryExpression).operator = "-"c Then
                Dim b = DirectCast(r, BinaryExpression)
                Return distributeMul(l, Add(b.left, Negate(b.right)))
            End If

            Return New BinaryExpression(l, r, "*"c)
        End Function
    End Module
End Namespace

