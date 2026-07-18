#Region "Microsoft.VisualBasic::47961646d8c0c4242db8da7186cce6f4, Data_science\Mathematica\Math\MathLambda\test\SymbolicTest.vb"

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

    '   Total Lines: 230
    '    Code Lines: 173 (75.22%)
    ' Comment Lines: 7 (3.04%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 50 (21.74%)
    '     File Size: 9.38 KB


    ' Module SymbolicTest
    ' 
    '     Sub: Main, test_AutoFactor, test_Boolean, test_Derivative, test_EnhancedIntegration
    '          test_Integration, test_Limit, test_MultivariateFactor, test_Polynomial, test_Rationalize
    '          test_Simplify, test_Substitute, test_Taylor, unit_test
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Lambda
Imports Sym = Microsoft.VisualBasic.Math.Lambda.Symbolic.Symbolic
Imports Microsoft.VisualBasic.Math.Scripting

Module SymbolicTest

    Sub Main()
        ' Call expands()
        ' Call unit_test()
        Call test_Simplify()
        Call test_Substitute()
        Call test_Derivative()
        Call test_Polynomial()
        Call test_Integration()
        Call test_Limit()
        Call test_Taylor()
        Call test_Boolean()
        Call test_Rationalize()
        Call test_AutoFactor()
        Call test_EnhancedIntegration()
        Call test_MultivariateFactor()

        Pause()
    End Sub

    Sub unit_test()
        Call Console.WriteLine(Sym.Simplify("0+x"))
        Call Console.WriteLine(Sym.Simplify("0+x + 0 * a"))
        Pause()
    End Sub

    Sub test_Simplify()
        Console.WriteLine("=== Simplification ===")

        Dim samples = {
            "(5+5) * (2*x + x / 5 + x ^ 3)",
            "sin(x) ^ 2 + cos(x) ^ 2",
            "1 - sin(x) ^ 2",
            "x ^ 2 + 2 * x + 1",
            "x * 0 + 3 * x - x",
            "(a + a) * (b + b)"
        }

        For Each s In samples
            Dim expr = ScriptEngine.ParseExpression(s)
            Console.WriteLine($"  {s}  ->  {Sym.Simplify(expr)}")
        Next

        Console.WriteLine()
    End Sub

    Sub test_Substitute()
        Console.WriteLine("=== Substitution ===")

        Dim expr = ScriptEngine.ParseExpression("x ^ 2 + 3 * x + 1")
        Console.WriteLine($"  f(x)      = {expr}")
        Console.WriteLine($"  f(y+1)    = {Sym.Substitute(expr, "x", "y + 1")}")
        Console.WriteLine($"  f(2)      = {Sym.Substitute(expr, "x", 2.0)}")

        Dim mapping As New Dictionary(Of String, String) From {{"x", "a + b"}, {"y", "c"}}
        Dim g = ScriptEngine.ParseExpression("x * y")
        Console.WriteLine($"  x*y, x->a+b, y->c  = {Sym.Substitute(g, mapping)}")

        Console.WriteLine()
    End Sub

    Sub test_Derivative()
        Console.WriteLine("=== Differentiation ===")

        For Each s In {"x ^ 2", "sin(x)", "exp(x)", "x * exp(x)", "x ^ 3 + 2 * x", "ln(x)"}
            Dim expr = ScriptEngine.ParseExpression(s)
            Console.WriteLine($"  d/dx {s}  =  {Sym.Derivative(expr, "x")}")
        Next

        Console.WriteLine($"  d^2/dx^2 x^3  =  {Sym.DerivativeN(ScriptEngine.ParseExpression("x ^ 3"), "x", 2)}")
        Console.WriteLine($"  d/dx x^4 (n=4) = {Sym.DerivativeN(ScriptEngine.ParseExpression("x ^ 4"), "x", 4)}")

        ' Jacobian and Hessian
        Dim funcs = {ScriptEngine.ParseExpression("x ^ 2 + y"), ScriptEngine.ParseExpression("x * y")}
        Dim J = Sym.Jacobian(funcs, {"x", "y"})
        Console.WriteLine("  Jacobian:")
        For i = 0 To J.GetLength(0) - 1
            For k = 0 To J.GetLength(1) - 1
                Console.Write($"    {J(i, k)}")
            Next
            Console.WriteLine()
        Next

        Dim H = Sym.Hessian(ScriptEngine.ParseExpression("x ^ 2 * y + x * y ^ 2"), {"x", "y"})
        Console.WriteLine("  Hessian:")
        For i = 0 To H.GetLength(0) - 1
            For k = 0 To H.GetLength(1) - 1
                Console.Write($"    {H(i, k)}")
            Next
            Console.WriteLine()
        Next

        ' Implicit differentiation: x^2 + y^2 = 1  ->  -x/y
        Dim F = ScriptEngine.ParseExpression("x ^ 2 + y ^ 2 - 1")
        Console.WriteLine($"  dy/dx of x^2+y^2=1  =  {Sym.ImplicitDerivative(F, "y", "x")}")

        Console.WriteLine()
    End Sub

    Sub test_Polynomial()
        Console.WriteLine("=== Polynomial ===")

        Console.WriteLine($"  Factor(x^2+2x+1)        = {Sym.Factor("x ^ 2 + 2 * x + 1")}")
        Console.WriteLine($"  Factor(x^2-1)           = {Sym.Factor("x ^ 2 - 1")}")
        Console.WriteLine($"  Factor(x^3-x)           = {Sym.Factor("x ^ 3 - x")}")
        Console.WriteLine($"  (x+1)*(x-1)             = {Sym.PolynomialMultiply("x + 1", "x - 1")}")
        Console.WriteLine($"  (x^3-1)/(x-1) quotient  = {Sym.PolynomialDivide("x ^ 3 - 1", "x - 1")}")
        Console.WriteLine($"  (x^3-1)/(x-1) remainder = {Sym.PolynomialRemainder("x ^ 3 - 1", "x - 1")}")
        Console.WriteLine($"  GCD(x^2-1, x^2+2x+1)    = {Sym.PolynomialGCD("x ^ 2 - 1", "x ^ 2 + 2 * x + 1")}")

        Console.WriteLine()
    End Sub

    Sub test_Integration()
        Console.WriteLine("=== Integration ===")

        For Each s In {"x ^ 2", "x ^ 3 + 2 * x", "1 / x", "exp(x)", "sin(x)", "cos(x)", "x * exp(x)", "1 / (1 + x ^ 2)", "1 / (x ^ 2) ?"}
            ' skip the malformed one gracefully
        Next

        For Each s In {"x ^ 2", "x ^ 3 + 2 * x", "1 / x", "exp(x)", "sin(x)", "cos(x)", "x * exp(x)", "1 / (1 + x ^ 2)", "1 / sqrt(1 - x ^ 2)"}
            Dim expr = ScriptEngine.ParseExpression(s)
            Console.WriteLine($"  ∫ {s} dx  =  {Sym.Integrate(expr, "x")}")
        Next

        Console.WriteLine($"  ∫ 2x dx from 0 to 1  =  {Sym.DefiniteIntegral("2 * x", "x", 0, 1)}  (expected 1)")
        Console.WriteLine($"  ∫ x^2 dx from 0 to 2 =  {Sym.DefiniteIntegral("x ^ 2", "x", 0, 2)}  (expected 8/3)")

        Console.WriteLine()
    End Sub

    Sub test_Limit()
        Console.WriteLine("=== Limits ===")

        Console.WriteLine($"  lim x->0 sin(x)/x   =  {Sym.Limit("sin(x) / x", "x", "0")}  (expected 1)")
        Console.WriteLine($"  lim x->0 (1-cos(x))/x^2 = {Sym.Limit("(1 - cos(x)) / (x ^ 2)", "x", "0")}  (expected 1/2)")
        Console.WriteLine($"  lim x->2 (x^2-4)/(x-2)  =  {Sym.Limit("(x ^ 2 - 4) / (x - 2)", "x", "2")}  (expected 4)")
        Console.WriteLine($"  lim x->0 exp(x)         =  {Sym.Limit("exp(x)", "x", "0")}  (expected 1)")
        Console.WriteLine($"  lim x->inf (x^2+1)/(2x^2+3) = {Sym.Limit("(x ^ 2 + 1) / (2 * x ^ 2 + 3)", "x", "inf")}  (expected 1/2)")

        Console.WriteLine()
    End Sub

    Sub test_Taylor()
        Console.WriteLine("=== Taylor series ===")

        For Each s In {"exp(x)", "sin(x)", "cos(x)", "1 / (1 - x)"}
            Dim expr = ScriptEngine.ParseExpression(s)
            Dim series = Sym.Taylor(expr, "x", ScriptEngine.ParseExpression("0"), 4)
            Console.WriteLine($"  T_4 {s} about 0  =  {series}")
        Next

        Dim ex = ScriptEngine.ParseExpression("exp(x)")
        Dim res = Sym.TaylorWithRemainder(ex, "x", ScriptEngine.ParseExpression("0"), 3)
        Console.WriteLine($"  e^x T_3 = {res.polynomial}")
        Console.WriteLine($"  e^x R_3 = {res.remainder}")

        Console.WriteLine()
    End Sub

    Sub test_Boolean()
        Console.WriteLine("=== Boolean algebra (Quine-McCluskey) ===")

        ' f(A,B,C) = Sigma(1,3,7)  ->  A'C + BC
        Dim f = {1, 3, 7}
        Console.WriteLine($"  minterms {{1,3,7}}  ->  {String.Join(" + ", Sym.QuineMcCluskey({"A", "B", "C"}, f))}")
        Console.WriteLine($"  SOP expr           ->  {Sym.QMCSimplifySOP({"A", "B", "C"}, f)}")

        ' truth table derived minterms
        Dim mt = Sym.TruthTable({"A", "B", "C"}, Function(b) (b(0) AndAlso b(1)) OrElse (Not b(2)))
        Console.WriteLine($"  truth-table minterms of (A AND B) OR (NOT C): {mt.JoinBy(", ")}")
        Console.WriteLine($"  SOP                 ->  {Sym.QMCSimplifySOP({"A", "B", "C"}, mt)}")
        Console.WriteLine($"  POS                 ->  {Sym.QMCSimplifyPOS({"A", "B", "C"}, mt)}")

        Console.WriteLine()
    End Sub

    Sub test_Rationalize()
        Console.WriteLine("=== Rationalization ===")

        For Each s In {"1 / (1 + sqrt(2))", "1 / sqrt(2)", "1 / (sqrt(a) + sqrt(b))", "a / (b + sqrt(c))", "1 / (x + y * i)"}
            Dim expr = ScriptEngine.ParseExpression(s)
            Console.WriteLine($"  rationalize({s})  =  {Sym.Rationalize(expr)}")
        Next

        Console.WriteLine()
    End Sub

    Sub test_AutoFactor()
        Console.WriteLine("=== Simplify auto-factor ===")

        For Each s In {"x ^ 2 + 2 * x + 1", "x ^ 2 - 1", "x ^ 3 - x", "x ^ 2 + x", "x + x"}
            Dim expr = ScriptEngine.ParseExpression(s)
            Console.WriteLine($"  Simplify({s})  =  {Sym.Simplify(expr)}")
        Next

        Console.WriteLine()
    End Sub

    Sub test_EnhancedIntegration()
        Console.WriteLine("=== Enhanced integration ===")

        For Each s In {"tan(x)", "sec(x)", "csc(x)", "cot(x)", "sin(x) ^ 2", "cos(x) ^ 2", "sec(x) ^ 2",
                       "1 / (x ^ 2 - 4)", "1 / (4 - x ^ 2)", "1 / (x ^ 2 + 4)",
                       "2 * x * exp(x ^ 2)", "2 * x * cos(x ^ 2)"}
            Dim expr = ScriptEngine.ParseExpression(s)
            Console.WriteLine($"  ∫ {s} dx  =  {Sym.Integrate(expr, "x")}")
        Next

        Console.WriteLine()
    End Sub

    Sub test_MultivariateFactor()
        Console.WriteLine("=== Multivariate factor ===")

        For Each s In {"x ^ 2 - y ^ 2", "x ^ 2 + 2 * x * y + y ^ 2", "x ^ 4 - y ^ 4",
                       "x * y + x", "4 * x ^ 2 - 9 * y ^ 2", "x ^ 2 + y ^ 2"}
            Dim expr = ScriptEngine.ParseExpression(s)
            Console.WriteLine($"  Factor({s})  =  {Sym.Factor(expr)}")
        Next

        Console.WriteLine()
    End Sub
End Module
