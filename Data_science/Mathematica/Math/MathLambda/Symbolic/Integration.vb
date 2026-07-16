' /********************************************************************************/
'
'     Module Integration
' 
'         Heuristic symbolic integration. The indefinite integral is computed by
'         pattern matching over monomials, powers, exponentials, trigonometric
'         and logarithmic primitives, rational forms (1/x, 1/(ax+b),
'         1/(1+x^2), 1/sqrt(1-x^2)) and integration by parts for a polynomial
'         multiplied by exp / sin / cos. The definite integral is evaluated by
'         the Newton-Leibniz formula over the antiderivative.
'
'     Author: xie.guigang@live.com
'     Copyright (c) 2018 GPL3 Licensed
'
' /********************************************************************************/

Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl
Imports Script = Microsoft.VisualBasic.Math.Scripting.ScriptEngine

Namespace Symbolic

    Module Integration

        ''' <summary>
        ''' Indefinite integral of <paramref name="expr"/> with respect to <paramref name="var"/>.
        ''' Returns the antiderivative (without the constant of integration).
        ''' </summary>
        Public Function Integrate(expr As String, var$) As Expression
            Return Integrate(Script.ParseExpression(expr), var)
        End Function

        ''' <summary>
        ''' Indefinite integral of <paramref name="expr"/> with respect to <paramref name="var"/>.
        ''' </summary>
        Public Function Integrate(expr As Expression, var$) As Expression
            Dim terms = FlattenSum(expr)
            Dim parts As New List(Of Expression)

            For Each t In terms
                Dim c As Double, body As Expression
                extractConst(t, c, body)
                If c = 0.0 Then Continue For

                Dim integral = integrateCore(body, var)
                If integral Is Nothing Then
                    ' Unsupported primitive: keep an explicit unevaluated marker.
                    parts.Add(New FunctionInvoke("integrate", New Expression() {body, New SymbolExpression(var)}))
                    Continue For
                End If

                If c <> 1.0 Then
                    parts.Add(Mul(MakeLiteral(c), integral))
                Else
                    parts.Add(integral)
                End If
            Next

            If parts.Count = 0 Then Return MakeLiteral(0)
            Dim acc = parts(0)
            For i As Integer = 1 To parts.Count - 1
                acc = Add(acc, parts(i))
            Next
            Return simplifyExpr(acc)
        End Function

        ''' <summary>
        ''' Definite integral of <paramref name="expr"/> over [<paramref name="lower"/>, <paramref name="upper"/>]
        ''' with respect to <paramref name="var"/>, evaluated by Newton-Leibniz.
        ''' </summary>
        Public Function DefiniteIntegral(expr As String, var$, lower As Double, upper As Double) As Double
            Return DefiniteIntegral(Script.ParseExpression(expr), var, lower, upper)
        End Function

        Public Function DefiniteIntegral(expr As Expression, var$, lower As Double, upper As Double) As Double
            Dim F = Integrate(expr, var)
            If F Is Nothing Then Return Double.NaN
            Try
                Dim atUpper = Substitute(F, var, upper).Evaluate(Script.Expression)
                Dim atLower = Substitute(F, var, lower).Evaluate(Script.Expression)
                Return atUpper - atLower
            Catch
                Return Double.NaN
            End Try
        End Function

        ' ------------------------------------------------------------------
        ' Core integration rules
        ' ------------------------------------------------------------------

        Private Function integrateCore(core As Expression, var$) As Expression
            If TypeOf core Is Literal Then
                Return Mul(core, New SymbolExpression(var))
            End If

            If TypeOf core Is SymbolExpression Then
                If DirectCast(core, SymbolExpression).symbolName = var Then
                    Return Div(Pow(core, MakeLiteral(2)), MakeLiteral(2))
                Else
                    Return Mul(core, New SymbolExpression(var))
                End If
            End If

            If TypeOf core Is UnaryExpression AndAlso DirectCast(core, UnaryExpression).operator = "-"c Then
                Return Negate(integrateCore(DirectCast(core, UnaryExpression).value, var))
            End If

            If TypeOf core Is BinaryExpression Then
                Dim b = DirectCast(core, BinaryExpression)
                Select Case b.operator
                    Case "^"c : Return integratePower(b.left, b.right, var)
                    Case "/"c : Return integrateQuotient(b.left, b.right, var)
                    Case "*"c : Return integrateProduct(core, var)
                    Case Else : Return Nothing
                End Select
            End If

            If TypeOf core Is FunctionInvoke Then
                Dim f = DirectCast(core, FunctionInvoke)
                If f.parameters.Length = 1 Then
                    Return integrateFunction(f.funcName, f.parameters(0), var)
                End If
            End If

            Return Nothing
        End Function

        Private Function integrateFunction(name$, u As Expression, var$) As Expression
            Dim lf = decomposeLinear(u, var)
            If lf.isLinear AndAlso lf.a <> 0.0 Then
                Select Case name
                    Case "exp" : Return Div(funcExp(u), MakeLiteral(lf.a))
                    Case "sin" : Return Div(Negate(funcCos(u)), MakeLiteral(lf.a))
                    Case "cos" : Return Div(funcSin(u), MakeLiteral(lf.a))
                    Case "ln" : Return Div(Subt(Mul(Clone(u), funcLn(Clone(u))), Clone(u)), MakeLiteral(lf.a))
                    Case "sqrt" : Return Div(Mul(MakeLiteral(2.0 / 3.0), Pow(Clone(u), MakeLiteral(1.5))), MakeLiteral(lf.a))
                    Case "tan" : Return Div(Negate(funcLn(funcCos(Clone(u)))), MakeLiteral(lf.a))
                End Select
            End If
            Return Nothing
        End Function

        Private Function integratePower(left As Expression, right As Expression, var$) As Expression
            Dim eNum = NumericValue(right)
            If Not eNum.HasValue Then Return Nothing
            Dim n = eNum.Value

            If n = -1.0 Then
                If TypeOf left Is SymbolExpression AndAlso DirectCast(left, SymbolExpression).symbolName = var Then
                    Return funcLn(left)
                End If
                Dim lf = decomposeLinear(left, var)
                If lf.isLinear AndAlso lf.a <> 0.0 Then Return Div(funcLn(Clone(left)), MakeLiteral(lf.a))
                Return funcLn(Clone(left))
            End If

            If TypeOf left Is SymbolExpression AndAlso DirectCast(left, SymbolExpression).symbolName = var Then
                Return Div(Pow(left, MakeLiteral(n + 1)), MakeLiteral(n + 1))
            End If
            Dim lf2 = decomposeLinear(left, var)
            If lf2.isLinear AndAlso lf2.a <> 0.0 Then
                Return Div(Pow(Clone(left), MakeLiteral(n + 1)), MakeLiteral(lf2.a * (n + 1)))
            End If
            Return Nothing
        End Function

        Private Function integrateQuotient(num As Expression, den As Expression, var$) As Expression
            If TypeOf num Is Literal AndAlso DirectCast(num, Literal).number = 1.0 Then
                Dim lf = decomposeLinear(den, var)
                If lf.isLinear AndAlso lf.a <> 0.0 Then
                    Return Div(funcLn(Clone(den)), MakeLiteral(lf.a))
                End If
                If isOnePlusVarSquared(den, var) Then Return funcAtan(var)
                If isSqrtOneMinusVarSq(den, var) Then Return funcAsin(var)
            Else
                Dim lf = decomposeLinear(den, var)
                If lf.isLinear AndAlso lf.a <> 0.0 Then
                    Return Mul(Clone(num), Div(funcLn(Clone(den)), MakeLiteral(lf.a)))
                End If
            End If
            Return Nothing
        End Function

        Private Function integrateProduct(core As Expression, var$) As Expression
            Dim factors = FlattenProduct(core)

            Dim specialIdx = -1, gType$ = "", gInner As Expression = Nothing, ga As Double = 0.0
            For i As Integer = 0 To factors.Count - 1
                If matchSpecial(factors(i), var, gType, gInner, ga) Then
                    specialIdx = i
                    Exit For
                End If
            Next

            If specialIdx < 0 Then Return Nothing

            Dim rest As New List(Of Expression)
            For i As Integer = 0 To factors.Count - 1
                If i <> specialIdx Then rest.Add(factors(i))
            Next

            Dim u As Expression
            If rest.Count = 0 Then
                u = MakeLiteral(1)
            ElseIf rest.Count = 1 Then
                u = rest(0)
            Else
                u = rest(0)
                For i As Integer = 1 To rest.Count - 1
                    u = Mul(u, rest(i))
                Next
            End If

            If isPolynomial(u, var) Then
                Return integrateByParts(u, gType, gInner, ga, var)
            End If
            Return Nothing
        End Function

        Private Function integrateByParts(u As Expression, gType$, gInner As Expression, ga As Double, var$) As Expression
            Dim v = integrateSpecial(gType, gInner, ga)
            If v Is Nothing Then Return Nothing
            Dim du = Differentiate(u, var)
            If Not isPolynomial(du, var) Then Return Nothing
            Dim inner = Mul(v, du)
            Dim innerInt = Integrate(inner, var)
            If innerInt Is Nothing Then Return Nothing
            Return simplifyExpr(Subt(Mul(Clone(u), v), innerInt))
        End Function

        Private Function integrateSpecial(gType$, inner As Expression, a As Double) As Expression
            Select Case gType
                Case "exp" : Return Div(funcExp(Clone(inner)), MakeLiteral(a))
                Case "sin" : Return Div(Negate(funcCos(Clone(inner))), MakeLiteral(a))
                Case "cos" : Return Div(funcSin(Clone(inner)), MakeLiteral(a))
                Case Else : Return Nothing
            End Select
        End Function

        Private Function matchSpecial(factor As Expression, var$, ByRef gType$, ByRef gInner As Expression, ByRef ga As Double) As Boolean
            If TypeOf factor Is FunctionInvoke Then
                Dim f = DirectCast(factor, FunctionInvoke)
                If f.parameters.Length = 1 Then
                    Dim name = f.funcName
                    If name = "exp" OrElse name = "sin" OrElse name = "cos" Then
                        Dim lf = decomposeLinear(f.parameters(0), var)
                        If lf.isLinear AndAlso lf.a <> 0.0 Then
                            gType = name
                            gInner = f.parameters(0)
                            ga = lf.a
                            Return True
                        End If
                    End If
                End If
            End If
            Return False
        End Function

        ' ------------------------------------------------------------------
        ' Small helpers
        ' ------------------------------------------------------------------

        Private Sub extractConst(term As Expression, ByRef coeff As Double, ByRef body As Expression)
            If TypeOf term Is UnaryExpression AndAlso DirectCast(term, UnaryExpression).operator = "-"c Then
                Dim c As Double, b As Expression
                extractConst(DirectCast(term, UnaryExpression).value, c, b)
                coeff = -c
                body = b
                Return
            End If
            SplitCoefficient(term, coeff, body)
        End Sub

        Private Function isPolynomial(expr As Expression, var$) As Boolean
            If TypeOf expr Is Literal Then Return True
            If TypeOf expr Is SymbolExpression Then Return DirectCast(expr, SymbolExpression).symbolName = var
            If TypeOf expr Is UnaryExpression AndAlso DirectCast(expr, UnaryExpression).operator = "-"c Then
                Return isPolynomial(DirectCast(expr, UnaryExpression).value, var)
            End If
            If TypeOf expr Is BinaryExpression Then
                Dim b = DirectCast(expr, BinaryExpression)
                Select Case b.operator
                    Case "+"c, "-"c, "*"c : Return isPolynomial(b.left, var) AndAlso isPolynomial(b.right, var)
                    Case "^"c : Return isPolynomial(b.left, var) AndAlso TypeOf b.right Is Literal
                    Case Else : Return False
                End Select
            End If
            Return False
        End Function

        Private Structure LinearForm
            Public isLinear As Boolean
            Public a As Double
            Public b As Double
        End Structure

        Private Function decomposeLinear(expr As Expression, var$) As LinearForm
            If TypeOf expr Is Literal Then
                Return New LinearForm With {.isLinear = True, .a = 0, .b = DirectCast(expr, Literal).number}
            End If
            If TypeOf expr Is SymbolExpression Then
                If DirectCast(expr, SymbolExpression).symbolName = var Then
                    Return New LinearForm With {.isLinear = True, .a = 1, .b = 0}
                End If
                Return New LinearForm With {.isLinear = False}
            End If
            If TypeOf expr Is UnaryExpression AndAlso DirectCast(expr, UnaryExpression).operator = "-"c Then
                Dim r = decomposeLinear(DirectCast(expr, UnaryExpression).value, var)
                If r.isLinear Then r.a = -r.a : r.b = -r.b
                Return r
            End If
            If TypeOf expr Is BinaryExpression Then
                Dim b = DirectCast(expr, BinaryExpression)
                Select Case b.operator
                    Case "+"c
                        Dim l = decomposeLinear(b.left, var), rr = decomposeLinear(b.right, var)
                        If l.isLinear AndAlso rr.isLinear Then Return New LinearForm With {.isLinear = True, .a = l.a + rr.a, .b = l.b + rr.b}
                    Case "-"c
                        Dim l = decomposeLinear(b.left, var), rr = decomposeLinear(b.right, var)
                        If l.isLinear AndAlso rr.isLinear Then Return New LinearForm With {.isLinear = True, .a = l.a - rr.a, .b = l.b - rr.b}
                    Case "*"c
                        If TypeOf b.left Is Literal Then
                            Dim r2 = decomposeLinear(b.right, var)
                            If r2.isLinear Then Return New LinearForm With {.isLinear = True, .a = DirectCast(b.left, Literal).number * r2.a, .b = DirectCast(b.left, Literal).number * r2.b}
                        End If
                        If TypeOf b.right Is Literal Then
                            Dim l2 = decomposeLinear(b.left, var)
                            If l2.isLinear Then Return New LinearForm With {.isLinear = True, .a = DirectCast(b.right, Literal).number * l2.a, .b = DirectCast(b.right, Literal).number * l2.b}
                        End If
                    Case "/"c
                        Dim num = decomposeLinear(b.left, var), den = decomposeLinear(b.right, var)
                        If num.isLinear AndAlso den.isLinear AndAlso den.a = 0.0 Then
                            Return New LinearForm With {.isLinear = True, .a = num.a / den.b, .b = num.b / den.b}
                        End If
                    Case "^"c
                        Dim e = NumericValue(b.right)
                        If e.HasValue AndAlso e.Value = 1.0 Then Return decomposeLinear(b.left, var)
                End Select
            End If
            Return New LinearForm With {.isLinear = False}
        End Function

        Private Function isOnePlusVarSquared(den As Expression, var$) As Boolean
            If TypeOf den Is BinaryExpression AndAlso DirectCast(den, BinaryExpression).operator = "+"c Then
                Dim b = DirectCast(den, BinaryExpression)
                If TypeOf b.left Is Literal AndAlso DirectCast(b.left, Literal).number = 1.0 Then
                    Return isVarSquared(b.right, var)
                End If
            End If
            Return False
        End Function

        Private Function isVarSquared(e As Expression, var$) As Boolean
            If TypeOf e Is BinaryExpression AndAlso DirectCast(e, BinaryExpression).operator = "^"c Then
                Dim b = DirectCast(e, BinaryExpression)
                If TypeOf b.left Is SymbolExpression AndAlso DirectCast(b.left, SymbolExpression).symbolName = var Then
                    Dim n = NumericValue(b.right)
                    Return n.HasValue AndAlso n.Value = 2.0
                End If
            End If
            Return False
        End Function

        Private Function isSqrtOneMinusVarSq(den As Expression, var$) As Boolean
            If TypeOf den Is FunctionInvoke AndAlso DirectCast(den, FunctionInvoke).funcName = "sqrt" Then
                Dim p = DirectCast(den, FunctionInvoke).parameters
                If p.Length = 1 AndAlso TypeOf p(0) Is BinaryExpression Then
                    Dim b = DirectCast(p(0), BinaryExpression)
                    If b.operator = "-"c Then
                        If TypeOf b.left Is Literal AndAlso DirectCast(b.left, Literal).number = 1.0 Then
                            Return isVarSquared(b.right, var)
                        End If
                    End If
                End If
            End If
            Return False
        End Function

        Private Function funcExp(x As Expression) As FunctionInvoke
            Return New FunctionInvoke("exp", New Expression() {x})
        End Function
        Private Function funcSin(x As Expression) As FunctionInvoke
            Return New FunctionInvoke("sin", New Expression() {x})
        End Function
        Private Function funcCos(x As Expression) As FunctionInvoke
            Return New FunctionInvoke("cos", New Expression() {x})
        End Function
        Private Function funcLn(x As Expression) As FunctionInvoke
            Return New FunctionInvoke("ln", New Expression() {x})
        End Function
        Private Function funcAtan(x$) As FunctionInvoke
            Return New FunctionInvoke("atan", New Expression() {New SymbolExpression(x)})
        End Function
        Private Function funcAsin(x$) As FunctionInvoke
            Return New FunctionInvoke("asin", New Expression() {New SymbolExpression(x)})
        End Function
    End Module
End Namespace
