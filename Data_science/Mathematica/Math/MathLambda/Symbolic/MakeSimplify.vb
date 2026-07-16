' /********************************************************************************/
'
'     Module MakeSimplify
' 
'         The algebraic simplification core. Performs recursive simplification
'         of the immutable expression tree: constant folding, identity rules,
'         like-term collection for sums and products (with power combining) and
'         a handful of trigonometric identities.
'
'     Author: xie.guigang@live.com
'     Copyright (c) 2018 GPL3 Licensed
'
' /********************************************************************************/

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Symbolic

    Module MakeSimplify

        ''' <summary>
        ''' Entry point used by the public <see cref="Symbolic"/> API. Returns a
        ''' simplified copy of the expression.
        ''' </summary>
        Friend Function simplifyExpr(raw As Expression) As Expression
            Return makeSimple(raw)
        End Function

        ''' <summary>
        ''' Pure algebraic simplification without any (auto) factorisation. Used by
        ''' the rationalisation and polynomial modules so that a factorisation step
        ''' inside <see cref="Symbolic.Simplify"/> cannot recurse back into
        ''' <see cref="Polynomial.Factor"/> and cause an infinite loop.
        ''' </summary>
        Friend Function simplifyRaw(raw As Expression) As Expression
            Return makeSimple(raw)
        End Function

        Private Function makeSimple(expr As Expression) As Expression
            If expr Is Nothing Then
                Return Nothing
            ElseIf TypeOf expr Is Literal Then
                Return expr
            ElseIf TypeOf expr Is SymbolExpression Then
                Return expr
            End If

            If TypeOf expr Is BinaryExpression Then
                Dim b = DirectCast(expr, BinaryExpression)
                Dim left = makeSimple(b.left)
                Dim right = makeSimple(b.right)

                Select Case b.operator
                    Case "+"c
                        Dim trig = tryTrigSum(left, right, "+")
                        If trig IsNot Nothing Then Return trig
                        Return simplifySum(left, right)
                    Case "-"c
                        Dim trig = tryTrigSum(left, right, "-")
                        If trig IsNot Nothing Then Return trig
                        Return simplifySum(left, Negate(right))
                    Case "*"c
                        Return simplifyProduct(left, right)
                    Case "/"c
                        Return simplifyQuotient(left, right)
                    Case "^"c
                        Return simplifyPower(left, right)
                    Case Else
                        Return New BinaryExpression(left, right, b.operator)
                End Select
            End If

            If TypeOf expr Is FunctionInvoke Then
                Dim f = DirectCast(expr, FunctionInvoke)
                Dim args = f.parameters.Select(AddressOf makeSimple).ToArray

                If f.funcName = "sin" AndAlso args.Length = 1 Then
                    Dim u As Expression = Nothing
                    If isNegated(args(0), u) Then Return Negate(makeFunc("sin", u))
                ElseIf f.funcName = "cos" AndAlso args.Length = 1 Then
                    Dim u As Expression = Nothing
                    If isNegated(args(0), u) Then Return makeFunc("cos", u)
                ElseIf f.funcName = "tan" AndAlso args.Length = 1 Then
                    Dim u As Expression = Nothing
                    If isNegated(args(0), u) Then Return Negate(makeFunc("tan", u))
                End If

                Return New FunctionInvoke(f.funcName, args)
            End If

            If TypeOf expr Is UnaryExpression Then
                Dim u = DirectCast(expr, UnaryExpression)
                Dim v = makeSimple(u.value)
                If u.operator = "-"c Then
                    If TypeOf v Is UnaryExpression AndAlso DirectCast(v, UnaryExpression).operator = "-"c Then
                        Return DirectCast(v, UnaryExpression).value
                    ElseIf TypeOf v Is Literal Then
                        Return New Literal(-DirectCast(v, Literal).number)
                    End If
                End If
                Return New UnaryExpression With {.[operator] = u.operator, .value = v}
            End If

            If TypeOf expr Is UnaryNot Then
                Dim n = DirectCast(expr, UnaryNot)
                Return New UnaryNot With {.value = makeSimple(n.value)}
            End If

            If TypeOf expr Is Factorial Then
                Dim fa = DirectCast(expr, Factorial)
                Return New Factorial(makeSimple(fa.factor).ToString)
            End If

            Return expr
        End Function

        ' -------------------------------------------------------------------
        ' Addition / subtraction
        ' -------------------------------------------------------------------

        Private Function simplifySum(left As Expression, right As Expression) As Expression
            Dim terms = FlattenSum(New BinaryExpression(left, right, "+"))
            Return combineSum(terms)
        End Function

        Private Class TermGroup
            Public body As Expression
            Public coeff As Double
        End Class

        Private Function combineSum(terms As List(Of Expression)) As Expression
            Dim groups As New List(Of TermGroup)

            For Each t In terms
                Dim c As Double, b As Expression
                splitSumTerm(t, c, b)

                Dim found As TermGroup = Nothing
                For Each g In groups
                    If ExprEquals(g.body, b) Then
                        found = g
                        Exit For
                    End If
                Next
                If found Is Nothing Then
                    groups.Add(New TermGroup With {.body = b, .coeff = c})
                Else
                    found.coeff += c
                End If
            Next

            Dim result As New List(Of Expression)
            For Each g In groups
                If g.coeff = 0 Then
                    Continue For
                ElseIf TypeOf g.body Is Literal AndAlso DirectCast(g.body, Literal).number = 1 Then
                    result.Add(MakeLiteral(g.coeff))
                ElseIf g.coeff = 1 Then
                    result.Add(g.body)
                ElseIf g.coeff = -1 Then
                    result.Add(Negate(g.body))
                Else
                    result.Add(Mul(MakeLiteral(g.coeff), g.body))
                End If
            Next

            If result.Count = 0 Then Return MakeLiteral(0)
            If result.Count = 1 Then Return result(0)

            Dim acc = result(0)
            For i As Integer = 1 To result.Count - 1
                acc = Add(acc, result(i))
            Next
            Return acc
        End Function

        Private Sub splitSumTerm(term As Expression, ByRef coeff As Double, ByRef body As Expression)
            If TypeOf term Is UnaryExpression AndAlso DirectCast(term, UnaryExpression).operator = "-"c Then
                Dim c As Double, b As Expression
                SplitCoefficient(DirectCast(term, UnaryExpression).value, c, b)
                coeff = -c
                body = makeSimple(b)
            Else
                SplitCoefficient(term, coeff, body)
                body = makeSimple(body)
            End If
        End Sub

        ' -------------------------------------------------------------------
        ' Multiplication / division
        ' -------------------------------------------------------------------

        Private Function simplifyProduct(left As Expression, right As Expression) As Expression
            If isLiteralZero(left) OrElse isLiteralZero(right) Then Return MakeLiteral(0)
            If isLiteralOne(left) Then Return right
            If isLiteralOne(right) Then Return left
            Dim factors = FlattenProduct(New BinaryExpression(left, right, "*"))
            Return combineProduct(factors)
        End Function

        Private Class ProdGroup
            Public baseExpr As Expression
            Public exp As Double
        End Class

        Private Function combineProduct(factors As List(Of Expression)) As Expression
            Dim coeff As Double = 1
            Dim groups As New List(Of ProdGroup)

            For Each raw In factors
                Dim f = raw

                If TypeOf f Is UnaryExpression AndAlso DirectCast(f, UnaryExpression).operator = "-"c Then
                    coeff *= -1
                    f = DirectCast(f, UnaryExpression).value
                End If

                ' reciprocal: 1 / x  (built by Reciprocal / Div(1, x))
                If TypeOf f Is BinaryExpression AndAlso DirectCast(f, BinaryExpression).operator = "/"c Then
                    Dim b = DirectCast(f, BinaryExpression)
                    If isLiteralOne(b.left) Then
                        addFactor(groups, makeSimple(b.right), -1, coeff)
                        Continue For
                    End If
                End If

                ' power: base ^ literalExp
                If TypeOf f Is BinaryExpression AndAlso DirectCast(f, BinaryExpression).operator = "^"c Then
                    Dim p = DirectCast(f, BinaryExpression)
                    Dim e = NumericValue(p.right)
                    If e.HasValue Then
                        addFactor(groups, makeSimple(p.left), e.Value, coeff)
                        Continue For
                    End If
                End If

                If TypeOf f Is Literal Then
                    coeff *= DirectCast(f, Literal).number
                    Continue For
                End If

                addFactor(groups, f, 1, coeff)
            Next

            If coeff = 0 Then Return MakeLiteral(0)

            Dim result As New List(Of Expression)
            For Each g In groups
                If g.exp = 0 Then
                    Continue For
                End If

                Dim folded = foldSpecialSquare(g.baseExpr, g.exp)
                If folded IsNot Nothing Then
                    ' sqrt(y) ^ 2  ->  y ;  i ^ 2  ->  -1
                    result.Add(folded)
                ElseIf g.exp = 1 Then
                    result.Add(g.baseExpr)
                Else
                    result.Add(Pow(g.baseExpr, MakeLiteral(g.exp)))
                End If
            Next

            ' Canonicalise the factor order so that commutative products such as
            ' a*b and b*a compare equal and cancel in sums (needed by rationalisation
            ' to clear cross terms like sqrt(a)*sqrt(b) - sqrt(b)*sqrt(a)).
            result.Sort(Function(a, b) canonicalKey(a).CompareTo(canonicalKey(b)))

            If result.Count = 0 Then
                Return MakeLiteral(coeff)
            End If

            Dim prod = result(0)
            For i As Integer = 1 To result.Count - 1
                prod = Mul(prod, result(i))
            Next

            ' Render a negative coefficient as a Negate wrapper rather than a *(-1)
            ' literal factor, so that e.g. -a*b and a*b compare equal and cancel.
            If coeff = 1 Then
                Return prod
            ElseIf coeff = -1 Then
                Return Negate(prod)
            Else
                Return Mul(MakeLiteral(coeff), prod)
            End If
        End Function

        Private Function canonicalKey(e As Expression) As String
            If e Is Nothing Then Return ""
            If TypeOf e Is Literal Then
                Return "L" & DirectCast(e, Literal).number.ToString
            ElseIf TypeOf e Is SymbolExpression Then
                Return "S" & DirectCast(e, SymbolExpression).symbolName
            ElseIf TypeOf e Is FunctionInvoke Then
                Dim f = DirectCast(e, FunctionInvoke)
                Return "F" & f.funcName & "(" & String.Join(",", f.parameters.Select(AddressOf canonicalKey)) & ")"
            ElseIf TypeOf e Is BinaryExpression Then
                Dim b = DirectCast(e, BinaryExpression)
                Return "B" & b.operator & "(" & canonicalKey(b.left) & "," & canonicalKey(b.right) & ")"
            ElseIf TypeOf e Is UnaryExpression Then
                Dim u = DirectCast(e, UnaryExpression)
                Return "U" & u.operator & "(" & canonicalKey(u.value) & ")"
            End If
            Return "X" & e.GetType.Name
        End Function

        Private Sub addFactor(groups As List(Of ProdGroup), baseExpr As Expression, exponent As Double, ByRef coeff As Double)
            If TypeOf baseExpr Is Literal Then
                coeff *= (DirectCast(baseExpr, Literal).number ^ exponent)
                Return
            End If
            For Each g In groups
                If ExprEquals(g.baseExpr, baseExpr) Then
                    g.exp += exponent
                    Return
                End If
            Next
            groups.Add(New ProdGroup With {.baseExpr = baseExpr, .exp = exponent})
        End Sub

        Private Function simplifyQuotient(left As Expression, right As Expression) As Expression
            If isLiteralZero(right) Then Return Div(left, right)
            If isLiteralOne(right) Then Return left
            If isLiteralZero(left) Then Return MakeLiteral(0)
            If TypeOf left Is Literal AndAlso TypeOf right Is Literal Then
                Return MakeLiteral(DirectCast(left, Literal).number / DirectCast(right, Literal).number)
            End If
            If ExprEquals(left, right) Then Return MakeLiteral(1)

            Dim factors = FlattenProduct(Div(left, right))
            Return combineProduct(factors)
        End Function

        Private Function simplifyPower(left As Expression, right As Expression) As Expression
            If TypeOf left Is Literal AndAlso TypeOf right Is Literal Then
                Return MakeLiteral(DirectCast(left, Literal).number ^ DirectCast(right, Literal).number)
            End If
            If TypeOf right Is Literal Then
                Dim e = DirectCast(right, Literal).number
                If e = 0 Then Return MakeLiteral(1)
                If e = 1 Then Return left
            End If
            If TypeOf left Is Literal Then
                Dim b = DirectCast(left, Literal).number
                If b = 1 Then Return MakeLiteral(1)
                If b = 0 Then Return MakeLiteral(0)
            End If

            ' (a ^ b) ^ c  ->  a ^ (b * c)
            If TypeOf left Is BinaryExpression AndAlso DirectCast(left, BinaryExpression).operator = "^"c Then
                Dim inner = DirectCast(left, BinaryExpression)
                Dim e1 = NumericValue(inner.right)
                Dim e2 = NumericValue(right)
                If e1.HasValue AndAlso e2.HasValue Then
                    Return Pow(inner.left, MakeLiteral(e1.Value * e2.Value))
                End If
            End If

            Return Pow(left, right)
        End Function

        ' -------------------------------------------------------------------
        ' Trigonometric identities
        ' -------------------------------------------------------------------

        Private Function tryTrigSum(l As Expression, r As Expression, op$) As Expression
            Dim f1 As String = "", f2 As String = ""
            Dim u1 As Expression = Nothing, u2 As Expression = Nothing

            If op = "+"c Then
                If matchTrigSquared(l, f1, u1) AndAlso matchTrigSquared(r, f2, u2) AndAlso f1 <> f2 AndAlso ExprEquals(u1, u2) Then
                    Return MakeLiteral(1)
                End If
            ElseIf op = "-"c Then
                ' 1 - sin(u)^2 = cos(u)^2 ; 1 - cos(u)^2 = sin(u)^2
                If TypeOf l Is Literal AndAlso DirectCast(l, Literal).number = 1 Then
                    If matchTrigSquared(r, f2, u2) Then
                        Dim keep = If(f2 = "sin", "cos", "sin")
                        Return Pow(makeFunc(keep, u2), MakeLiteral(2))
                    End If
                End If
            End If

            Return Nothing
        End Function

        Private Function matchTrigSquared(expr As Expression, ByRef funcName$, ByRef u As Expression) As Boolean
            If TypeOf expr Is BinaryExpression AndAlso DirectCast(expr, BinaryExpression).operator = "^"c Then
                Dim p = DirectCast(expr, BinaryExpression)
                If TypeOf p.left Is FunctionInvoke Then
                    Dim f = DirectCast(p.left, FunctionInvoke)
                    Dim e = NumericValue(p.right)
                    If e.HasValue AndAlso e.Value = 2 AndAlso f.parameters.Length = 1 Then
                        funcName = f.funcName
                        u = f.parameters(0)
                        Return True
                    End If
                End If
            End If
            Return False
        End Function

        ' -------------------------------------------------------------------
        ' Small helpers
        ' -------------------------------------------------------------------

        Private Function makeFunc(name$, u As Expression) As FunctionInvoke
            Return New FunctionInvoke(name, New Expression() {u})
        End Function

        Private Function isLiteralOne(expr As Expression) As Boolean
            Dim v = NumericValue(expr)
            Return v.HasValue AndAlso v.Value = 1
        End Function

        Private Function isLiteralZero(expr As Expression) As Boolean
            Dim v = NumericValue(expr)
            Return v.HasValue AndAlso v.Value = 0
        End Function

        Private Function isNegated(expr As Expression, ByRef u As Expression) As Boolean
            If TypeOf expr Is UnaryExpression AndAlso DirectCast(expr, UnaryExpression).operator = "-"c Then
                u = DirectCast(expr, UnaryExpression).value
                Return True
            End If
            Return False
        End Function

        ''' <summary>
        ''' If <paramref name="baseExpr"/> is sqrt(y) raised to an even integer power n,
        ''' return the simplified factor y ^ (n / 2). If it is the imaginary unit i
        ''' squared, return -1. Otherwise return Nothing. These folds let the
        ''' rationalisation step clear radicals and the imaginary unit from denominators.
        ''' </summary>
        Private Function foldSpecialSquare(baseExpr As Expression, exp As Double) As Expression
            If TypeOf baseExpr Is FunctionInvoke Then
                Dim f = DirectCast(baseExpr, FunctionInvoke)
                If f.funcName = "sqrt" AndAlso f.parameters.Length = 1 Then
                    Dim n = System.Math.Round(exp)
                    If System.Math.Abs(exp - n) < 1.0E-9 AndAlso n >= 2 AndAlso (n Mod 2 = 0) Then
                        Return Pow(f.parameters(0), MakeLiteral(n \ 2))
                    End If
                End If
            ElseIf TypeOf baseExpr Is SymbolExpression Then
                Dim nm = DirectCast(baseExpr, SymbolExpression).symbolName
                If (nm = "i" OrElse nm = "I") AndAlso System.Math.Abs(exp - 2) < 1.0E-9 Then
                    Return MakeLiteral(-1)
                End If
            End If
            Return Nothing
        End Function
    End Module
End Namespace
