' /********************************************************************************/
'
'     Module Polynomial
' 
'         Univariate polynomial arithmetic over (exact) numeric coefficients:
'         construction from / serialisation to the expression tree, multiplication,
'         division with remainder, greatest common divisor (Euclidean algorithm)
'         and heuristic factorisation (rational-root / repeated division).
'
'     Author: xie.guigang@live.com
'     Copyright (c) 2018 GPL3 Licensed
'
' /********************************************************************************/

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Symbolic

    ''' <summary>
    ''' A univariate polynomial a0 + a1*x + ... + an*x^n with numeric coefficients.
    ''' </summary>
    Friend Class UnivariatePoly
        Public variable As String
        Public coeff As Double()

        Public ReadOnly Property Degree As Integer
            Get
                Dim d = coeff.Length - 1
                While d > 0 AndAlso System.Math.Abs(coeff(d)) < 1E-9
                    d -= 1
                End While
                Return d
            End Get
        End Property
    End Class

    Module Polynomial

        Private Const EPS As Double = 1.0E-9

        ' ------------------------------------------------------------------
        ' Construction / serialisation
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' Infer the single variable of a univariate polynomial expression.
        ''' </summary>
        Private Function inferVar(expr As Expression) As String
            Dim symbols = GetSymbols(expr)
            If symbols.Length = 1 Then
                Return symbols(0)
            ElseIf symbols.Length = 0 Then
                Throw New NotSupportedException("the expression contains no variable to build a polynomial from.")
            Else
                Throw New NotSupportedException($"multivariate polynomial is not supported, found variables: {symbols.JoinBy(", ")}")
            End If
        End Function

        Private Function parsePoly(expr As Expression, var$) As UnivariatePoly
            Dim terms = FlattenSum(expr)
            Dim dict As New Dictionary(Of Integer, Double)

            For Each t In terms
                Dim c As Double, p As Integer
                parseTerm(t, var, c, p)
                If dict.ContainsKey(p) Then
                    dict(p) += c
                Else
                    dict.Add(p, c)
                End If
            Next

            Dim maxP = 0
            For Each k In dict.Keys
                If k > maxP Then maxP = k
            Next

            Dim arr(maxP) As Double
            For i As Integer = 0 To maxP
                If dict.ContainsKey(i) Then arr(i) = dict(i)
            Next

            Return New UnivariatePoly With {.variable = var, .coeff = arr}
        End Function

        Private Sub parseTerm(t As Expression, var$, ByRef c As Double, ByRef p As Integer)
            If TypeOf t Is UnaryExpression AndAlso DirectCast(t, UnaryExpression).operator = "-"c Then
                Dim c2 As Double, p2 As Integer
                parseTerm(DirectCast(t, UnaryExpression).value, var, c2, p2)
                c = -c2
                p = p2
                Return
            End If

            If TypeOf t Is Literal Then
                c = DirectCast(t, Literal).number
                p = 0
                Return
            End If

            If TypeOf t Is SymbolExpression Then
                If DirectCast(t, SymbolExpression).symbolName = var Then
                    c = 1 : p = 1
                Else
                    Throw New NotSupportedException($"multivariate term not supported by the polynomial engine: {t}")
                End If
                Return
            End If

            If TypeOf t Is BinaryExpression Then
                Dim b = DirectCast(t, BinaryExpression)
                If b.operator = "^"c Then
                    If TypeOf b.left Is SymbolExpression AndAlso DirectCast(b.left, SymbolExpression).symbolName = var AndAlso TypeOf b.right Is Literal Then
                        c = 1
                        p = CInt(DirectCast(b.right, Literal).number)
                        Return
                    End If
                    Throw New NotSupportedException($"unsupported power term: {t}")
                ElseIf b.operator = "*"c Then
                    c = 1
                    p = 0
                    For Each f In FlattenProduct(t)
                        Dim c2 As Double, p2 As Integer
                        parseTerm(f, var, c2, p2)
                        c *= c2
                        p += p2
                    Next
                    Return
                End If
            End If

            Throw New NotSupportedException($"unsupported polynomial term: {t}")
        End Sub

        Private Function toExpr(poly As UnivariatePoly) As Expression
            Dim x = New SymbolExpression(poly.variable)
            Dim terms As New List(Of Expression)

            For i As Integer = 0 To poly.coeff.Length - 1
                Dim c = poly.coeff(i)
                If System.Math.Abs(c) < EPS Then Continue For
                If i = 0 Then
                    terms.Add(MakeLiteral(c))
                ElseIf i = 1 Then
                    terms.Add(Mul(MakeLiteral(c), x))
                Else
                    terms.Add(Mul(MakeLiteral(c), Pow(x, MakeLiteral(i))))
                End If
            Next

            If terms.Count = 0 Then Return MakeLiteral(0)
            If terms.Count = 1 Then Return terms(0)

            Dim acc = terms(0)
            For i As Integer = 1 To terms.Count - 1
                acc = Add(acc, terms(i))
            Next
            Return simplifyExpr(acc)
        End Function

        ' ------------------------------------------------------------------
        ' Arithmetic
        ' ------------------------------------------------------------------

        Private Function polyMultiply(a As UnivariatePoly, b As UnivariatePoly) As UnivariatePoly
            Dim n = a.Degree, m = b.Degree
            Dim res(n + m) As Double
            For i As Integer = 0 To n
                For j As Integer = 0 To m
                    res(i + j) += a.coeff(i) * b.coeff(j)
                Next
            Next
            Return New UnivariatePoly With {.variable = a.variable, .coeff = res}
        End Function

        Private Sub polyDivide(dividend As UnivariatePoly, divisor As UnivariatePoly, ByRef quotient As UnivariatePoly, ByRef remainder As UnivariatePoly)
            Dim remC = CType(dividend.coeff.Clone, Double())
            Dim dDeg = divisor.Degree
            Dim dLead = divisor.coeff(dDeg)
            Dim q(dividend.Degree) As Double
            Dim rDeg = dividend.Degree

            While rDeg >= dDeg AndAlso System.Math.Abs(remC(rDeg)) >= EPS
                Dim coef = remC(rDeg) / dLead
                Dim deg = rDeg - dDeg
                q(deg) = coef
                For i As Integer = 0 To dDeg
                    remC(deg + i) -= coef * divisor.coeff(i)
                Next
                rDeg -= 1
            End While

            quotient = New UnivariatePoly With {.variable = dividend.variable, .coeff = trimZero(q)}
            remainder = New UnivariatePoly With {.variable = dividend.variable, .coeff = trimZero(remC)}
        End Sub

        Private Function polyGCD(a As UnivariatePoly, b As UnivariatePoly) As UnivariatePoly
            Dim r0 = a, r1 = b

            Do While Not polyIsZero(r1)
                Dim q As UnivariatePoly, r As UnivariatePoly
                polyDivide(r0, r1, q, r)
                r0 = r1
                r1 = r
            Loop

            Dim lead = r0.coeff(r0.Degree)
            If System.Math.Abs(lead) >= EPS Then
                For i As Integer = 0 To r0.coeff.Length - 1
                    r0.coeff(i) /= lead
                Next
            End If

            Return r0
        End Function

        ''' <summary>
        ''' Test whether <paramref name="expr"/> is a polynomial in the single
        ''' variable <paramref name="var"/> (no transcendental functions, fractional
        ''' or negative exponents). Used by <see cref="Symbolic.Simplify"/> to decide
        ''' whether the auto-factorisation step is safe to apply.
        ''' </summary>
        Friend Function IsUnivariatePolynomial(expr As Expression, var$) As Boolean
            If expr Is Nothing Then
                Return True
            ElseIf TypeOf expr Is Literal Then
                Return True
            ElseIf TypeOf expr Is SymbolExpression Then
                Return DirectCast(expr, SymbolExpression).symbolName = var
            ElseIf TypeOf expr Is UnaryExpression AndAlso DirectCast(expr, UnaryExpression).operator = "-"c Then
                Return IsUnivariatePolynomial(DirectCast(expr, UnaryExpression).value, var)
            ElseIf TypeOf expr Is BinaryExpression Then
                Dim b = DirectCast(expr, BinaryExpression)
                Select Case b.operator
                    Case "+"c, "-"c, "*"c
                        Return IsUnivariatePolynomial(b.left, var) AndAlso IsUnivariatePolynomial(b.right, var)
                    Case "^"c
                        Return IsUnivariatePolynomial(b.left, var) AndAlso TypeOf b.right Is Literal
                    Case Else
                        Return False
                End Select
            End If
            Return False
        End Function

        Private Function polyIsZero(p As UnivariatePoly) As Boolean
            For i As Integer = 0 To p.coeff.Length - 1
                If System.Math.Abs(p.coeff(i)) >= EPS Then Return False
            Next
            Return True
        End Function

        Private Function trimZero(arr As Double()) As Double()
            Dim d = arr.Length - 1
            While d > 0 AndAlso System.Math.Abs(arr(d)) < EPS
                d -= 1
            End While
            If d = arr.Length - 1 Then Return arr
            Dim out(d) As Double
            Array.Copy(arr, out, d + 1)
            Return out
        End Function

        Private Function evalPoly(coeff As Double(), x As Double) As Double
            Dim result = 0.0
            For i As Integer = coeff.Length - 1 To 0 Step -1
                result = result * x + coeff(i)
            Next
            Return result
        End Function

        ' ------------------------------------------------------------------
        ' Heuristic factorisation
        ' ------------------------------------------------------------------

        Private Function factorLinearRoots(poly As UnivariatePoly) As List(Of Expression)
            Dim factors As New List(Of Expression)
            Dim x = New SymbolExpression(poly.variable)
            Dim current = poly

            ' pull out the numeric greatest common divisor of coefficients
            Dim numericGCD = 0.0
            For i As Integer = 0 To current.coeff.Length - 1
                numericGCD = gcd(numericGCD, System.Math.Abs(current.coeff(i)))
            Next
            If numericGCD > 1 + EPS Then
                For i As Integer = 0 To current.coeff.Length - 1
                    current.coeff(i) /= numericGCD
                Next
                factors.Add(MakeLiteral(numericGCD))
            End If

            Dim changed = True
            While changed AndAlso current.Degree >= 1
                changed = False
                For r As Integer = -20 To 20
                    If System.Math.Abs(evalPoly(current.coeff, r)) < 1.0E-6 Then
                        ' root r -> factor (x - r)
                        factors.Add(Subt(x, MakeLiteral(r)))

                        ' divide current by (x - r)
                        Dim tmp(1) As Double
                        tmp(0) = -r
                        tmp(1) = 1
                        Dim divisor = New UnivariatePoly With {
                            .variable = poly.variable,
                            .coeff = tmp
                        }
                        Dim q As UnivariatePoly, remPoly As UnivariatePoly
                        polyDivide(current, divisor, q, remPoly)
                        current = q
                        changed = True
                        Exit For
                    End If
                Next
            End While

            If current.Degree >= 1 Then
                factors.Add(toExpr(current))
            ElseIf System.Math.Abs(current.coeff(0)) >= EPS AndAlso factors.Count = 0 Then
                factors.Add(toExpr(current))
            End If

            Return factors
        End Function

        Private Function gcd(a As Double, b As Double) As Double
            a = System.Math.Abs(a)
            b = System.Math.Abs(b)
            While b > EPS
                Dim t = b
                b = a - System.Math.Floor(a / b) * b
                a = t
            End While
            Return a
        End Function

        ' ------------------------------------------------------------------
        ' Public API
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' Factorise a univariate polynomial expression (heuristic). Returns the
        ''' factorised expression, e.g. x^2 + 2*x + 1 -> (x + 1)^2.
        ''' </summary>
        Public Function Factor(expr As Expression, Optional var As String = Nothing) As Expression
            Dim syms = GetSymbols(expr)
            If syms.Length <= 1 Then
                Dim v = If(syms.Length = 1, syms(0), inferVar(expr))
                Return factorUnivariate(expr, v)
            End If
            Return factorMultivariate(expr, syms)
        End Function

        ''' <summary>
        ''' Factor a (possibly multivariate) polynomial over the explicitly given
        ''' variable set.
        ''' </summary>
        Public Function Factor(expr As Expression, vars As String()) As Expression
            If vars.Length <= 1 Then
                Dim v = If(vars.Length = 1, vars(0), inferVar(expr))
                Return factorUnivariate(expr, v)
            End If
            Return factorMultivariate(expr, vars)
        End Function

        Private Function factorUnivariate(expr As Expression, var As String) As Expression
            Dim expanded = simplifyRaw(Expands(expr))
            Dim poly = parsePoly(expanded, var)
            Dim factors = factorLinearRoots(poly)

            If factors.Count = 0 Then
                Return MakeLiteral(1)
            ElseIf factors.Count = 1 Then
                Return factors(0)
            End If

            Dim acc = factors(0)
            For i As Integer = 1 To factors.Count - 1
                acc = Mul(acc, factors(i))
            Next
            Return simplifyRaw(acc)
        End Function

        ''' <summary>
        ''' Multiply two univariate polynomials and return the result as an expression.
        ''' </summary>
        Public Function PolynomialMultiply(a As Expression, b As Expression, Optional var As String = Nothing) As Expression
            If var Is Nothing Then var = inferVar(a)
            Dim pa = parsePoly(simplifyExpr(Expands(a)), var)
            Dim pb = parsePoly(simplifyExpr(Expands(b)), var)
            Return toExpr(polyMultiply(pa, pb))
        End Function

        ''' <summary>
        ''' Divide <paramref name="dividend"/> by <paramref name="divisor"/>, returning the
        ''' quotient and assigning the remainder to <paramref name="remainder"/>.
        ''' </summary>
        Public Function PolynomialDivide(dividend As Expression, divisor As Expression, Optional var As String = Nothing) As Expression
            If var Is Nothing Then var = inferVar(dividend)
            Dim pa = parsePoly(simplifyExpr(Expands(dividend)), var)
            Dim pb = parsePoly(simplifyExpr(Expands(divisor)), var)
            Dim q As UnivariatePoly, r As UnivariatePoly
            polyDivide(pa, pb, q, r)
            Return toExpr(q)
        End Function

        ''' <summary>
        ''' Remainder of dividing <paramref name="dividend"/> by <paramref name="divisor"/>.
        ''' </summary>
        Public Function PolynomialRemainder(dividend As Expression, divisor As Expression, Optional var As String = Nothing) As Expression
            If var Is Nothing Then var = inferVar(dividend)
            Dim pa = parsePoly(simplifyExpr(Expands(dividend)), var)
            Dim pb = parsePoly(simplifyExpr(Expands(divisor)), var)
            Dim q As UnivariatePoly, r As UnivariatePoly
            polyDivide(pa, pb, q, r)
            Return toExpr(r)
        End Function

        ''' <summary>
        ''' Greatest common divisor of two univariate polynomials (returned monic).
        ''' </summary>
        Public Function PolynomialGCD(a As Expression, b As Expression, Optional var As String = Nothing) As Expression
            If var Is Nothing Then var = inferVar(a)
            Dim pa = parsePoly(simplifyExpr(Expands(a)), var)
            Dim pb = parsePoly(simplifyExpr(Expands(b)), var)
            Return toExpr(polyGCD(pa, pb))
        End Function

        ' ------------------------------------------------------------------
        ' Multivariate factorisation (heuristic)
        ' ------------------------------------------------------------------

        Private Class MonoTerm
            Public coeff As Double
            Public exps As New Dictionary(Of String, Integer)
        End Class

        Private Function isMultiPoly(expr As Expression, vars As String()) As Boolean
            If expr Is Nothing Then Return True
            If TypeOf expr Is Literal Then Return True
            If TypeOf expr Is SymbolExpression Then Return True
            If TypeOf expr Is UnaryExpression AndAlso DirectCast(expr, UnaryExpression).operator = "-"c Then
                Return isMultiPoly(DirectCast(expr, UnaryExpression).value, vars)
            End If
            If TypeOf expr Is BinaryExpression Then
                Dim b = DirectCast(expr, BinaryExpression)
                Select Case b.operator
                    Case "+"c, "-"c, "*"c
                        Return isMultiPoly(b.left, vars) AndAlso isMultiPoly(b.right, vars)
                    Case "^"c
                        Return isMultiPoly(b.left, vars) AndAlso TypeOf b.right Is Literal
                    Case Else
                        Return False
                End Select
            End If
            Return False
        End Function

        Private Function factorMultivariate(expr As Expression, vars As String()) As Expression
            If Not isMultiPoly(expr, vars) Then Return expr

            expr = simplifyRaw(Expands(expr))
            If Not isMultiPoly(expr, vars) Then Return expr

            Dim terms = FlattenSum(expr)
            Dim mts As New List(Of MonoTerm)
            For Each t In terms
                mts.Add(parseMultiTerm(t, vars))
            Next

            ' 1) numeric content (greatest common divisor of coefficients)
            Dim ng = 0.0
            For Each m In mts
                ng = gcd(ng, System.Math.Abs(m.coeff))
            Next
            If ng > 1.0 + EPS Then
                For Each m In mts
                    m.coeff /= ng
                Next
            End If

            ' 2) greatest common monomial factor
            Dim gcfExps As New Dictionary(Of String, Integer)
            For Each v In vars
                gcfExps(v) = Integer.MaxValue
            Next
            For Each m In mts
                For Each v In vars
                    Dim e = If(m.exps.ContainsKey(v), m.exps(v), 0)
                    If e < gcfExps(v) Then gcfExps(v) = e
                Next
            Next

            Dim commonMono As Expression = Nothing
            For Each v In vars
                Dim e = gcfExps(v)
                If e > 0 Then
                    Dim f = If(e = 1, New SymbolExpression(v), Pow(New SymbolExpression(v), MakeLiteral(e)))
                    commonMono = If(commonMono Is Nothing, f, Mul(commonMono, f))
                End If
            Next

            If commonMono IsNot Nothing Then
                Dim divided As New List(Of Expression)
                For Each t In terms
                    divided.Add(simplifyRaw(Div(Clone(t), Clone(commonMono))))
                Next
                Dim inner As Expression
                If divided.Count = 1 Then
                    inner = divided(0)
                Else
                    inner = divided(0)
                    For i As Integer = 1 To divided.Count - 1
                        inner = Add(inner, divided(i))
                    Next
                End If
                Return Mul(commonMono, factorMultivariate(inner, vars))
            End If

            ' 3) difference of squares A^2 - B^2 -> (A - B)(A + B)
            Dim ds = tryDiffSquares(mts, vars)
            If ds IsNot Nothing Then Return ds

            ' 3b) perfect square trinomial A^2 +/- 2AB + B^2 -> (A +/- B)^2
            Dim st = trySquareTrinomial(mts, vars)
            If st IsNot Nothing Then Return st

            ' 4) no further heuristic applies: rebuild (constant content * restored sum)
            Dim rebuilt As Expression = Nothing
            For Each m In mts
                Dim termExpr = monoToExpr(m, vars)
                rebuilt = If(rebuilt Is Nothing, termExpr, Add(rebuilt, termExpr))
            Next
            If ng > 1.0 + EPS Then rebuilt = Mul(MakeLiteral(ng), rebuilt)
            Return simplifyRaw(rebuilt)
        End Function

        Private Function monoToExpr(m As MonoTerm, vars As String()) As Expression
            Dim parts As New List(Of Expression)
            For Each v In vars
                Dim e = If(m.exps.ContainsKey(v), m.exps(v), 0)
                If e > 0 Then
                    If e = 1 Then
                        parts.Add(New SymbolExpression(v))
                    Else
                        parts.Add(Pow(New SymbolExpression(v), MakeLiteral(e)))
                    End If
                End If
            Next

            Dim mono As Expression
            If parts.Count = 0 Then
                mono = MakeLiteral(1)
            ElseIf parts.Count = 1 Then
                mono = parts(0)
            Else
                mono = parts(0)
                For i As Integer = 1 To parts.Count - 1
                    mono = Mul(mono, parts(i))
                Next
            End If

            If System.Math.Abs(m.coeff - 1.0) < EPS Then
                Return mono
            ElseIf System.Math.Abs(m.coeff + 1.0) < EPS Then
                Return Negate(mono)
            ElseIf parts.Count = 0 Then
                Return MakeLiteral(m.coeff)
            Else
                Return Mul(MakeLiteral(m.coeff), mono)
            End If
        End Function

        Private Function parseMultiTerm(t As Expression, vars As String()) As MonoTerm
            Dim m As New MonoTerm With {.coeff = 1.0, .exps = New Dictionary(Of String, Integer)}
            If TypeOf t Is UnaryExpression AndAlso DirectCast(t, UnaryExpression).operator = "-"c Then
                m.coeff = -1.0
                addMultiFactors(FlattenProduct(DirectCast(t, UnaryExpression).value), m)
            Else
                addMultiFactors(FlattenProduct(t), m)
            End If
            Return m
        End Function

        Private Sub addMultiFactors(factors As List(Of Expression), m As MonoTerm)
            For Each f In factors
                If TypeOf f Is Literal Then
                    m.coeff *= DirectCast(f, Literal).number
                ElseIf TypeOf f Is SymbolExpression Then
                    Dim nm = DirectCast(f, SymbolExpression).symbolName
                    If Not m.exps.ContainsKey(nm) Then m.exps(nm) = 0
                    m.exps(nm) += 1
                ElseIf TypeOf f Is BinaryExpression AndAlso DirectCast(f, BinaryExpression).operator = "^"c Then
                    Dim b = DirectCast(f, BinaryExpression)
                    If TypeOf b.left Is SymbolExpression AndAlso TypeOf b.right Is Literal Then
                        Dim nm = DirectCast(b.left, SymbolExpression).symbolName
                        Dim e = CInt(DirectCast(b.right, Literal).number)
                        If Not m.exps.ContainsKey(nm) Then m.exps(nm) = 0
                        m.exps(nm) += e
                    Else
                        Throw New NotSupportedException($"unsupported multivariate factor: {f}")
                    End If
                ElseIf TypeOf f Is BinaryExpression AndAlso DirectCast(f, BinaryExpression).operator = "/"c Then
                    Dim b = DirectCast(f, BinaryExpression)
                    Dim ln = NumericValue(b.left)
                    If ln.HasValue AndAlso ln.Value = 1.0 AndAlso TypeOf b.right Is Literal Then
                        m.coeff /= DirectCast(b.right, Literal).number
                    Else
                        Throw New NotSupportedException($"unsupported multivariate factor: {f}")
                    End If
                Else
                    Throw New NotSupportedException($"unsupported multivariate factor: {f}")
                End If
            Next
        End Sub

        Private Function tryDiffSquares(mts As List(Of MonoTerm), vars As String()) As Expression
            If mts.Count <> 2 Then Return Nothing

            Dim t0 = mts(0), t1 = mts(1)
            If Not ((t0.coeff > 0 AndAlso t1.coeff < 0) OrElse (t0.coeff < 0 AndAlso t1.coeff > 0)) Then Return Nothing

            Dim pos = If(t0.coeff > 0, t0, t1)
            Dim neg = If(t0.coeff < 0, t0, t1)
            Dim A = squareRootMono(pos, vars)
            Dim B = squareRootMono(neg, vars)
            If A Is Nothing OrElse B Is Nothing Then Return Nothing

            Return Mul(factorMultivariate(Subt(A, B), vars), factorMultivariate(Add(A, B), vars))
        End Function

        Private Function squareRootMono(m As MonoTerm, vars As String()) As Expression
            For Each v In vars
                Dim e = If(m.exps.ContainsKey(v), m.exps(v), 0)
                If e Mod 2 <> 0 Then Return Nothing
            Next

            Dim c = System.Math.Abs(m.coeff)
            Dim sc = System.Math.Sqrt(c)
            If System.Math.Abs(sc - System.Math.Round(sc)) > 1.0E-9 Then Return Nothing

            Dim parts As New List(Of Expression)
            Dim scc = System.Math.Round(sc)
            If scc <> 1 Then parts.Add(MakeLiteral(scc))
            For Each v In vars
                Dim e = If(m.exps.ContainsKey(v), m.exps(v), 0) \ 2
                If e > 0 Then
                    If e = 1 Then
                        parts.Add(New SymbolExpression(v))
                    Else
                        parts.Add(Pow(New SymbolExpression(v), MakeLiteral(e)))
                    End If
                End If
            Next

            If parts.Count = 0 Then Return MakeLiteral(1)
            If parts.Count = 1 Then Return parts(0)
            Dim acc = parts(0)
            For i As Integer = 1 To parts.Count - 1
                acc = Mul(acc, parts(i))
            Next
            Return acc
        End Function

        Private Function trySquareTrinomial(mts As List(Of MonoTerm), vars As String()) As Expression
            If mts.Count <> 3 Then Return Nothing

            Dim squares As New List(Of MonoTerm)
            Dim cross As MonoTerm = Nothing
            For Each m In mts
                If isSquareMono(m, vars) Then
                    squares.Add(m)
                Else
                    cross = m
                End If
            Next
            If squares.Count <> 2 OrElse cross Is Nothing Then Return Nothing

            Dim A = squareRootMono(squares(0), vars)
            Dim B = squareRootMono(squares(1), vars)
            If A Is Nothing OrElse B Is Nothing Then Return Nothing

            Dim aCoeff = System.Math.Round(System.Math.Sqrt(System.Math.Abs(squares(0).coeff)))
            Dim bCoeff = System.Math.Round(System.Math.Sqrt(System.Math.Abs(squares(1).coeff)))
            Dim plusCoeff = 2.0 * aCoeff * bCoeff

            ' expected cross exponents = (A exponents) + (B exponents)
            Dim expSum As New Dictionary(Of String, Integer)
            For Each v In vars
                Dim s = halfExp(squares(0), v) + halfExp(squares(1), v)
                If s <> 0 Then expSum(v) = s
            Next

            Dim crossExps As New Dictionary(Of String, Integer)
            For Each kv In cross.exps
                If kv.Value <> 0 Then crossExps(kv.Key) = kv.Value
            Next

            If Not dictEquals(expSum, crossExps) Then Return Nothing

            If System.Math.Abs(cross.coeff - plusCoeff) < 1.0E-9 Then
                Return Pow(Add(A, B), MakeLiteral(2))
            ElseIf System.Math.Abs(cross.coeff + plusCoeff) < 1.0E-9 Then
                Return Pow(Subt(A, B), MakeLiteral(2))
            End If
            Return Nothing
        End Function

        Private Function isSquareMono(m As MonoTerm, vars As String()) As Boolean
            If m.coeff <= 0 Then Return False
            For Each v In vars
                Dim e = If(m.exps.ContainsKey(v), m.exps(v), 0)
                If e Mod 2 <> 0 Then Return False
            Next
            Dim sc = System.Math.Sqrt(m.coeff)
            If System.Math.Abs(sc - System.Math.Round(sc)) > 1.0E-9 Then Return False
            Return True
        End Function

        Private Function halfExp(m As MonoTerm, v As String) As Integer
            Dim e = If(m.exps.ContainsKey(v), m.exps(v), 0)
            Return e \ 2
        End Function

        Private Function dictEquals(d1 As Dictionary(Of String, Integer), d2 As Dictionary(Of String, Integer)) As Boolean
            If d1.Count <> d2.Count Then Return False
            For Each kv In d1
                If Not d2.ContainsKey(kv.Key) OrElse d2(kv.Key) <> kv.Value Then Return False
            Next
            Return True
        End Function
    End Module
End Namespace
