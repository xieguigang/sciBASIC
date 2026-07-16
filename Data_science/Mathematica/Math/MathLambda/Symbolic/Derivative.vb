' /********************************************************************************/
'
'     Module Derivative
' 
'         Symbolic differentiation engine: first / higher order derivatives,
'         partial derivatives, Jacobian and Hessian matrices and implicit
'         differentiation of an equation F(x, y) = 0.
'
'     Author: xie.guigang@live.com
'     Copyright (c) 2018 GPL3 Licensed
'
' /********************************************************************************/

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Symbolic

    Module Derivative

        ''' <summary>
        ''' Differentiate the expression with respect to <paramref name="wrt"/>. When
        ''' <paramref name="wrt"/> is omitted the (single) variable of the expression
        ''' is used.
        ''' </summary>
        <Extension>
        Public Function GetDerivative(exp As Expression, Optional wrt$ = Nothing) As Expression
            If wrt Is Nothing Then wrt = inferSingleVar(exp)
            Return simplifyExpr(diff(exp, wrt))
        End Function

        ''' <summary>
        ''' First derivative of <paramref name="expr"/> with respect to <paramref name="x"/>.
        ''' </summary>
        Public Function Differentiate(expr As Expression, x$) As Expression
            Return simplifyExpr(diff(expr, x))
        End Function

        ''' <summary>
        ''' The n-th order derivative of <paramref name="expr"/> with respect to <paramref name="x"/>.
        ''' </summary>
        Public Function DerivativeN(expr As Expression, x$, n As Integer) As Expression
            Dim cur = expr
            For i As Integer = 1 To n
                cur = simplifyExpr(diff(cur, x))
            Next
            Return cur
        End Function

        ''' <summary>
        ''' Partial derivative of a (possibly multivariate) expression with respect to
        ''' the variable <paramref name="x"/>.
        ''' </summary>
        Public Function PartialDerivative(expr As Expression, x$) As Expression
            Return simplifyExpr(diff(expr, x))
        End Function

        ''' <summary>
        ''' Jacobian matrix of a vector valued function F = (f1, ..., fm) with respect
        ''' to the variables (x1, ..., xn). The result is an m x n matrix where
        ''' J(i, j) = d(f_i) / d(x_j).
        ''' </summary>
        Public Function Jacobian(funcs As Expression(), vars As String()) As Expression(,)
            Dim m = funcs.Length, n = vars.Length
            Dim J(m - 1, n - 1) As Expression

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    J(i, j) = simplifyExpr(diff(funcs(i), vars(j)))
                Next
            Next

            Return J
        End Function

        ''' <summary>
        ''' Hessian matrix of a scalar function f(x1, ..., xn): H(i, j) = d^2 f / (dx_i dx_j).
        ''' </summary>
        Public Function Hessian(f As Expression, vars As String()) As Expression(,)
            Dim n = vars.Length
            Dim H(n - 1, n - 1) As Expression

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    Dim d1 = diff(f, vars(i))
                    Dim d2 = diff(d1, vars(j))
                    H(i, j) = simplifyExpr(d2)
                Next
            Next

            Return H
        End Function

        ''' <summary>
        ''' Implicit derivative dy/dx for an equation F(x, y) = 0.
        ''' Returns -Fx / Fy.
        ''' </summary>
        Public Function ImplicitDerivative(F As Expression, dependentVar$, independentVar$) As Expression
            Dim Fx = simplifyExpr(diff(F, independentVar))
            Dim Fy = simplifyExpr(diff(F, dependentVar))
            Return simplifyExpr(Div(Negate(Fx), Fy))
        End Function

        ' ------------------------------------------------------------------
        ' Core differentiation rules
        ' ------------------------------------------------------------------

        Private Function diff(expr As Expression, x$) As Expression
            If expr Is Nothing Then Return MakeLiteral(0)
            If TypeOf expr Is Literal Then Return MakeLiteral(0)
            If TypeOf expr Is SymbolExpression Then
                If DirectCast(expr, SymbolExpression).symbolName = x Then Return MakeLiteral(1) Else Return MakeLiteral(0)
            End If

            If TypeOf expr Is BinaryExpression Then
                Dim b = DirectCast(expr, BinaryExpression)
                Dim a = b.left, c = b.right
                Dim da = diff(a, x), dc = diff(c, x)
                Select Case b.operator
                    Case "+"c : Return Add(da, dc)
                    Case "-"c : Return Subt(da, dc)
                    Case "*"c : Return Add(Mul(da, c), Mul(a, dc))
                    Case "/"c : Return Div(Subt(Mul(da, c), Mul(a, dc)), Pow(c, MakeLiteral(2)))
                    Case "^"c : Return diffPower(a, c, da, dc, x)
                    Case Else : Return New BinaryExpression(a, c, b.operator)
                End Select
            End If

            If TypeOf expr Is FunctionInvoke Then
                Dim f = DirectCast(expr, FunctionInvoke)
                If f.parameters.Length = 1 Then
                    Dim u = f.parameters(0)
                    Dim du = diff(u, x)
                    Return diffFunc(f.funcName, u, du)
                End If
                Return expr
            End If

            If TypeOf expr Is UnaryExpression Then
                Dim u = DirectCast(expr, UnaryExpression).value
                Dim du = diff(u, x)
                If DirectCast(expr, UnaryExpression).operator = "-"c Then Return Negate(du) Else Return du
            End If

            If TypeOf expr Is UnaryNot Then Return MakeLiteral(0)
            If TypeOf expr Is Factorial Then Return MakeLiteral(0)

            Return MakeLiteral(0)
        End Function

        Private Function diffPower(a As Expression, c As Expression, da As Expression, dc As Expression, x$) As Expression
            If Not DependsOn(c, x) Then
                ' d(u^v)/dx = v * u^(v-1) * du
                Return Mul(Mul(c, Pow(a, Subt(c, MakeLiteral(1)))), da)
            ElseIf Not DependsOn(a, x) Then
                ' d(u^v)/dx = u^v * ln(u) * dv
                Return Mul(Mul(Pow(a, c), func("ln", a)), dc)
            Else
                ' d(u^v)/dx = u^v * (v * du / u + dv * ln(u))
                Return Mul(Pow(a, c), Add(Div(Mul(c, da), a), Mul(dc, func("ln", a))))
            End If
        End Function

        Private Function diffFunc(name$, u As Expression, du As Expression) As Expression
            Select Case name
                Case "sin" : Return Mul(func("cos", u), du)
                Case "cos" : Return Mul(Negate(func("sin", u)), du)
                Case "tan" : Return Mul(Div(MakeLiteral(1), Pow(func("cos", u), MakeLiteral(2))), du)
                Case "asin" : Return Mul(Div(MakeLiteral(1), func("sqrt", Subt(MakeLiteral(1), Pow(u, MakeLiteral(2))))), du)
                Case "acos" : Return Mul(Div(Negate(MakeLiteral(1)), func("sqrt", Subt(MakeLiteral(1), Pow(u, MakeLiteral(2))))), du)
                Case "atan" : Return Mul(Div(MakeLiteral(1), Add(MakeLiteral(1), Pow(u, MakeLiteral(2)))), du)
                Case "exp" : Return Mul(func("exp", u), du)
                Case "ln", "log" : Return Div(du, u)
                Case "sqrt" : Return Mul(Div(MakeLiteral(1), Mul(MakeLiteral(2), func("sqrt", u))), du)
                Case "sinh" : Return Mul(func("cosh", u), du)
                Case "cosh" : Return Mul(func("sinh", u), du)
                Case "tanh" : Return Mul(Subt(MakeLiteral(1), Pow(func("tanh", u), MakeLiteral(2))), du)
                Case Else : Return Mul(func(name, u), du)
            End Select
        End Function

        Private Function func(name$, arg As Expression) As FunctionInvoke
            Return New FunctionInvoke(name, New Expression() {arg})
        End Function

        Private Function inferSingleVar(expr As Expression) As String
            Dim s = GetSymbols(expr)
            If s.Length = 1 Then
                Return s(0)
            End If
            Throw New NotSupportedException("a variable to differentiate with respect to must be specified for multivariate expressions.")
        End Function
    End Module
End Namespace
