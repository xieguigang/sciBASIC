#Region "Microsoft.VisualBasic::c010a334d926b469f5e4a5e8008ef911, Data_science\Mathematica\Math\MathLambda\Symbolic\Symbolic.vb"

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

    '   Total Lines: 326
    '    Code Lines: 151 (46.32%)
    ' Comment Lines: 120 (36.81%)
    '    - Xml Docs: 64.17%
    ' 
    '   Blank Lines: 55 (16.87%)
    '     File Size: 15.17 KB


    '     Module Symbolic
    ' 
    '         Function: (+2 Overloads) DefiniteIntegral, (+4 Overloads) Derivative, (+2 Overloads) DerivativeN, (+4 Overloads) Factor, Hessian
    '                   ImplicitDerivative, (+2 Overloads) Integrate, Jacobian, (+2 Overloads) Limit, (+2 Overloads) PartialDerivative
    '                   PolynomialDivide, (+2 Overloads) PolynomialGCD, PolynomialMultiply, PolynomialRemainder, QMCSimplifyPOS
    '                   QMCSimplifySOP, QuineMcCluskey, (+2 Overloads) Rationalize, (+2 Overloads) Simplify, (+2 Overloads) Substitute
    '                   (+2 Overloads) Taylor, (+2 Overloads) TaylorWithRemainder, TruthTable
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

            Dim simple = MakeSimplify.simplifyExpr(raw)

            ' Auto-factor a univariate polynomial so that, for example,
            ' x^2 + 2*x + 1 simplifies straight to (x + 1)^2. Multivariate
            ' expressions are left untouched to avoid the not-supported path.
            Dim syms = GetSymbols(simple)
            If syms.Length = 1 Then
                Dim v = syms(0)
                If Polynomial.IsUnivariatePolynomial(simple, v) Then
                    Dim fac = Polynomial.Factor(simple, v)
                    If Not ExprEquals(fac, simple) Then
                        simple = fac
                    End If
                End If
            End If

            Return simple
        End Function

        ' ------------------------------------------------------------------
        ' Rationalisation
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' Rationalise the expression: remove square roots and the imaginary unit
        ''' from every denominator by conjugate multiplication, e.g.
        ''' 1/(1 + sqrt(2)) -> sqrt(2) - 1, 1/(a + b*i) -> (a - b*i)/(a^2 + b^2).
        ''' </summary>
        Public Function Rationalize(expr As String) As Expression
            Return Rationalizer.Rationalize(Script.ParseExpression(expr))
        End Function

        ''' <summary>
        ''' Rationalise the expression (string form overload).
        ''' </summary>
        Public Function Rationalize(expr As Expression) As Expression
            Return Rationalizer.Rationalize(expr)
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

        ''' <summary>
        ''' Taylor expansion together with its Lagrange remainder.
        ''' </summary>
        Public Function TaylorWithRemainder(expr As String, var$, point$, order%) As TaylorResult
            Return Microsoft.VisualBasic.Math.Lambda.Symbolic.Taylor.TaylorWithRemainder(Script.ParseExpression(expr), var, Script.ParseExpression(point), order)
        End Function

        ''' <summary>
        ''' Taylor expansion together with its Lagrange remainder.
        ''' </summary>
        Public Function TaylorWithRemainder(expr As Expression, var$, point As Expression, order%) As TaylorResult
            Return Microsoft.VisualBasic.Math.Lambda.Symbolic.Taylor.TaylorWithRemainder(expr, var, point, order)
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

        ''' <summary>
        ''' Factor a (possibly multivariate) polynomial over the explicitly given
        ''' variable set, e.g. x^2 - y^2 -> (x - y) * (x + y).
        ''' </summary>
        Public Function Factor(expr As String, vars As String()) As Expression
            Return Polynomial.Factor(Script.ParseExpression(expr), vars)
        End Function

        ''' <summary>
        ''' Factor a (possibly multivariate) polynomial over the explicitly given
        ''' variable set.
        ''' </summary>
        Public Function Factor(expr As Expression, vars As String()) As Expression
            Return Polynomial.Factor(expr, vars)
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

