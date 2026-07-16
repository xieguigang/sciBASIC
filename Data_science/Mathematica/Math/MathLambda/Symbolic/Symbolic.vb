' /********************************************************************************/
'
'     Module Symbolic
' 
'         Unified facade of the symbolic computation engine. Every transformation
'         is functional: it consumes an immutable expression tree and returns a
'         new one. The underlying parser in Microsoft.VisualBasic.Math.Scripting
'         is never modified.
'
'     Author: xie.guigang@live.com
'     Copyright (c) 2018 GPL3 Licensed
'
' /********************************************************************************/

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl
Imports Script = Microsoft.VisualBasic.Math.Scripting.ScriptEngine

Namespace Symbolic

    ''' <summary>
    ''' symbolic computation engine
    ''' </summary>
    Public Module Symbolic

        ' ------------------------------------------------------------------
        ' Simplification
        ' ------------------------------------------------------------------

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Simplify(expr As String) As Expression
            Return Simplify(Script.ParseExpression(expr))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Simplify(raw As Expression) As Expression
            If TypeOf raw Is UnifySymbol Then
                Return DirectCast(raw, UnifySymbol).GetSimplify
            End If

            Return MakeSimplify.simplifyExpr(raw)
        End Function

        ' ------------------------------------------------------------------
        ' Substitution
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' Replace every occurrence of the symbol <paramref name="oldSymbol"/> by the
        ''' expression parsed from <paramref name="replacement"/>.
        ''' </summary>
        Public Function Substitute(expr As Expression, oldSymbol$, replacement As String) As Expression
            Return expr.Substitute(oldSymbol, Script.ParseExpression(replacement))
        End Function

        ''' <summary>
        ''' Simultaneously replace symbols by the expressions parsed from <paramref name="mapping"/>.
        ''' </summary>
        Public Function Substitute(expr As Expression, mapping As Dictionary(Of String, String)) As Expression
            Dim m As New Dictionary(Of String, Expression)
            For Each kv In mapping
                m(kv.Key) = Script.ParseExpression(kv.Value)
            Next
            Return expr.Substitute(m)
        End Function

        ' ------------------------------------------------------------------
        ' Differentiation
        ' ------------------------------------------------------------------

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Derivative(expr As String) As Expression
            Return Derivative(Script.ParseExpression(expr))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Derivative(exp As Expression) As Expression
            Return exp.GetDerivative
        End Function

        ''' <summary>
        ''' First derivative of <paramref name="expr"/> with respect to <paramref name="x"/>.
        ''' </summary>
        Public Function Derivative(expr As String, x$) As Expression
            Return Derivative(Script.ParseExpression(expr), x)
        End Function

        Public Function Derivative(expr As Expression, x$) As Expression
            Return GetDerivative(expr, x)
        End Function

        ''' <summary>
        ''' The n-th order derivative of <paramref name="expr"/> with respect to <paramref name="x"/>.
        ''' </summary>
        Public Function DerivativeN(expr As String, x$, n%) As Expression
            Return DerivativeN(Script.ParseExpression(expr), x, n)
        End Function

        Public Function DerivativeN(expr As Expression, x$, n%) As Expression
            Return Microsoft.VisualBasic.Math.Lambda.Symbolic.Derivative.DerivativeN(expr, x, n)
        End Function

        ''' <summary>
        ''' Partial derivative of a (possibly multivariate) expression with respect to <paramref name="x"/>.
        ''' </summary>
        Public Function PartialDerivative(expr As String, x$) As Expression
            Return PartialDerivative(Script.ParseExpression(expr), x)
        End Function

        Public Function PartialDerivative(expr As Expression, x$) As Expression
            Return Microsoft.VisualBasic.Math.Lambda.Symbolic.Derivative.PartialDerivative(expr, x)
        End Function

        ''' <summary>
        ''' Jacobian matrix of a vector valued function F with respect to the variables.
        ''' </summary>
        Public Function Jacobian(funcs As Expression(), vars As String()) As Expression(,)
            Return Microsoft.VisualBasic.Math.Lambda.Symbolic.Derivative.Jacobian(funcs, vars)
        End Function

        ''' <summary>
        ''' Hessian matrix of a scalar function f with respect to the variables.
        ''' </summary>
        Public Function Hessian(f As Expression, vars As String()) As Expression(,)
            Return Microsoft.VisualBasic.Math.Lambda.Symbolic.Derivative.Hessian(f, vars)
        End Function

        ''' <summary>
        ''' Implicit derivative dy/dx for an equation F(x, y) = 0. Returns -Fx / Fy.
        ''' </summary>
        Public Function ImplicitDerivative(F As Expression, dependentVar$, independentVar$) As Expression
            Return Microsoft.VisualBasic.Math.Lambda.Symbolic.Derivative.ImplicitDerivative(F, dependentVar, independentVar)
        End Function

        ' ------------------------------------------------------------------
        ' Integration
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' Indefinite integral of <paramref name="expr"/> with respect to <paramref name="var"/>.
        ''' </summary>
        Public Function Integrate(expr As String, var$) As Expression
            Return Integration.Integrate(Script.ParseExpression(expr), var)
        End Function

        Public Function Integrate(expr As Expression, var$) As Expression
            Return Integration.Integrate(expr, var)
        End Function

        ''' <summary>
        ''' Definite integral of <paramref name="expr"/> over [<paramref name="lower"/>, <paramref name="upper"/>].
        ''' </summary>
        Public Function DefiniteIntegral(expr As String, var$, lower As Double, upper As Double) As Double
            Return Integration.DefiniteIntegral(Script.ParseExpression(expr), var, lower, upper)
        End Function

        Public Function DefiniteIntegral(expr As Expression, var$, lower As Double, upper As Double) As Double
            Return Integration.DefiniteIntegral(expr, var, lower, upper)
        End Function

        ' ------------------------------------------------------------------
        ' Limits
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' Compute lim_{var -> target} expr.
        ''' </summary>
        Public Function Limit(expr As String, var$, target$) As Expression
            Return Limit(Script.ParseExpression(expr), var, Script.ParseExpression(target))
        End Function

        Public Function Limit(expr As Expression, var$, target As Expression) As Expression
            Return Microsoft.VisualBasic.Math.Lambda.Symbolic.Limit.Limit(expr, var, target)
        End Function

        ' ------------------------------------------------------------------
        ' Taylor series
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' Taylor expansion of <paramref name="expr"/> about <paramref name="point"/> up to <paramref name="order"/>.
        ''' </summary>
        Public Function Taylor(expr As String, var$, point$, order%) As Expression
            Return Taylor(Script.ParseExpression(expr), var, Script.ParseExpression(point), order)
        End Function

        Public Function Taylor(expr As Expression, var$, point As Expression, order%) As Expression
            Return Microsoft.VisualBasic.Math.Lambda.Symbolic.Taylor.Taylor(expr, var, point, order)
        End Function

        ' ------------------------------------------------------------------
        ' Polynomial arithmetic / factorisation
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' Factorise a univariate polynomial expression (heuristic).
        ''' </summary>
        Public Function Factor(expr As String, Optional var As String = Nothing) As Expression
            Return Polynomial.Factor(Script.ParseExpression(expr), var)
        End Function

        Public Function Factor(expr As Expression, Optional var As String = Nothing) As Expression
            Return Polynomial.Factor(expr, var)
        End Function

        Public Function PolynomialGCD(a As String, b As String, Optional var As String = Nothing) As Expression
            Return Polynomial.PolynomialGCD(Script.ParseExpression(a), Script.ParseExpression(b), var)
        End Function

        Public Function PolynomialGCD(a As Expression, b As Expression, Optional var As String = Nothing) As Expression
            Return Polynomial.PolynomialGCD(a, b, var)
        End Function

        Public Function PolynomialMultiply(a As String, b As String, Optional var As String = Nothing) As Expression
            Return Polynomial.PolynomialMultiply(Script.ParseExpression(a), Script.ParseExpression(b), var)
        End Function

        Public Function PolynomialDivide(dividend As String, divisor As String, Optional var As String = Nothing) As Expression
            Return Polynomial.PolynomialDivide(Script.ParseExpression(dividend), Script.ParseExpression(divisor), var)
        End Function

        Public Function PolynomialRemainder(dividend As String, divisor As String, Optional var As String = Nothing) As Expression
            Return Polynomial.PolynomialRemainder(Script.ParseExpression(dividend), Script.ParseExpression(divisor), var)
        End Function

        ' ------------------------------------------------------------------
        ' Boolean algebra
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' Enumerate the truth table of a boolean function and return the minterms where it is true.
        ''' </summary>
        Public Function TruthTable(vars As String(), f As Func(Of Boolean(), Boolean)) As Integer()
            Return BooleanAlgebra.TruthTable(vars, f)
        End Function

        ''' <summary>
        ''' Quine-McCluskey minimisation, returning the prime implicants as 0/1/- strings.
        ''' </summary>
        Public Function QuineMcCluskey(vars As String(), minterms As Integer(), Optional dontCares As Integer() = Nothing) As String()
            Return BooleanAlgebra.QuineMcCluskey(vars, minterms, dontCares)
        End Function

        ''' <summary>
        ''' Minimal Sum-Of-Products of a boolean function.
        ''' </summary>
        Public Function QMCSimplifySOP(vars As String(), minterms As Integer(), Optional dontCares As Integer() = Nothing) As Expression
            Return BooleanAlgebra.QMCSimplifySOP(vars, minterms, dontCares)
        End Function

        ''' <summary>
        ''' Minimal Product-Of-Sums of a boolean function.
        ''' </summary>
        Public Function QMCSimplifyPOS(vars As String(), minterms As Integer(), Optional dontCares As Integer() = Nothing) As Expression
            Return BooleanAlgebra.QMCSimplifyPOS(vars, minterms, dontCares)
        End Function
    End Module
End Namespace
