#Region "Microsoft.VisualBasic::5cab8d942e2473ae56fecef99ef0629a, Data_science\Mathematica\Math\MathLambda\test\SymbolicTest.vb"

'     Module SymbolicTest
' 
'         Verification examples for the symbolic computation engine.
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Lambda
Imports Microsoft.VisualBasic.Math.Lambda.Symbolic
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

        Pause()
    End Sub

    Sub unit_test()
        Call Console.WriteLine(Symbolic.Simplify("0+x"))
        Call Console.WriteLine(Symbolic.Simplify("0+x + 0 * a"))
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
            Console.WriteLine($"  {s}  ->  {Symbolic.Simplify(expr)}")
        Next

        Console.WriteLine()
    End Sub

    Sub test_Substitute()
        Console.WriteLine("=== Substitution ===")

        Dim expr = ScriptEngine.ParseExpression("x ^ 2 + 3 * x + 1")
        Console.WriteLine($"  f(x)      = {expr}")
        Console.WriteLine($"  f(y+1)    = {Symbolic.Substitute(expr, "x", "y + 1")}")
        Console.WriteLine($"  f(2)      = {Symbolic.Substitute(expr, "x", 2.0)}")

        Dim mapping As New Dictionary(Of String, String) From {{"x", "a + b"}, {"y", "c"}}
        Dim g = ScriptEngine.ParseExpression("x * y")
        Console.WriteLine($"  x*y, x->a+b, y->c  = {Symbolic.Substitute(g, mapping)}")

        Console.WriteLine()
    End Sub

    Sub test_Derivative()
        Console.WriteLine("=== Differentiation ===")

        For Each s In {"x ^ 2", "sin(x)", "exp(x)", "x * exp(x)", "x ^ 3 + 2 * x", "ln(x)"}
            Dim expr = ScriptEngine.ParseExpression(s)
            Console.WriteLine($"  d/d{x} {s}  =  {Symbolic.Derivative(expr, "x")}")
        Next

        Console.WriteLine($"  d^2/dx^2 x^3  =  {Symbolic.DerivativeN(ScriptEngine.ParseExpression("x ^ 3"), "x", 2)}")
        Console.WriteLine($"  d/dx x^4 (n=4) = {Symbolic.DerivativeN(ScriptEngine.ParseExpression("x ^ 4"), "x", 4)}")

        ' Jacobian and Hessian
        Dim funcs = {ScriptEngine.ParseExpression("x ^ 2 + y"), ScriptEngine.ParseExpression("x * y")}
        Dim J = Symbolic.Jacobian(funcs, {"x", "y"})
        Console.WriteLine("  Jacobian:")
        For i = 0 To J.GetLength(0) - 1
            For J = 0 To J.GetLength(1) - 1
                Console.Write($"    {J(i, J)}")
            Next
            Console.WriteLine()
        Next

        Dim H = Symbolic.Hessian(ScriptEngine.ParseExpression("x ^ 2 * y + x * y ^ 2"), {"x", "y"})
        Console.WriteLine("  Hessian:")
        For i = 0 To H.GetLength(0) - 1
            For J = 0 To H.GetLength(1) - 1
                Console.Write($"    {H(i, J)}")
            Next
            Console.WriteLine()
        Next

        ' Implicit differentiation: x^2 + y^2 = 1  ->  -x/y
        Dim F = ScriptEngine.ParseExpression("x ^ 2 + y ^ 2 - 1")
        Console.WriteLine($"  dy/dx of x^2+y^2=1  =  {Symbolic.ImplicitDerivative(F, "y", "x")}")

        Console.WriteLine()
    End Sub

    Sub test_Polynomial()
        Console.WriteLine("=== Polynomial ===")

        Console.WriteLine($"  Factor(x^2+2x+1)        = {Symbolic.Factor("x ^ 2 + 2 * x + 1")}")
        Console.WriteLine($"  Factor(x^2-1)           = {Symbolic.Factor("x ^ 2 - 1")}")
        Console.WriteLine($"  Factor(x^3-x)           = {Symbolic.Factor("x ^ 3 - x")}")
        Console.WriteLine($"  (x+1)*(x-1)             = {Symbolic.PolynomialMultiply("x + 1", "x - 1")}")
        Console.WriteLine($"  (x^3-1)/(x-1) quotient  = {Symbolic.PolynomialDivide("x ^ 3 - 1", "x - 1")}")
        Console.WriteLine($"  (x^3-1)/(x-1) remainder = {Symbolic.PolynomialRemainder("x ^ 3 - 1", "x - 1")}")
        Console.WriteLine($"  GCD(x^2-1, x^2+2x+1)    = {Symbolic.PolynomialGCD("x ^ 2 - 1", "x ^ 2 + 2 * x + 1")}")

        Console.WriteLine()
    End Sub

    Sub test_Integration()
        Console.WriteLine("=== Integration ===")

        For Each s In {"x ^ 2", "x ^ 3 + 2 * x", "1 / x", "exp(x)", "sin(x)", "cos(x)", "x * exp(x)", "1 / (1 + x ^ 2)", "1 / (x ^ 2) ?"}
            ' skip the malformed one gracefully
        Next

        For Each s In {"x ^ 2", "x ^ 3 + 2 * x", "1 / x", "exp(x)", "sin(x)", "cos(x)", "x * exp(x)", "1 / (1 + x ^ 2)", "1 / sqrt(1 - x ^ 2)"}
            Dim expr = ScriptEngine.ParseExpression(s)
            Console.WriteLine($"  ∫ {s} dx  =  {Symbolic.Integrate(expr, "x")}")
        Next

        Console.WriteLine($"  ∫ 2x dx from 0 to 1  =  {Symbolic.DefiniteIntegral("2 * x", "x", 0, 1)}  (expected 1)")
        Console.WriteLine($"  ∫ x^2 dx from 0 to 2 =  {Symbolic.DefiniteIntegral("x ^ 2", "x", 0, 2)}  (expected 8/3)")

        Console.WriteLine()
    End Sub

    Sub test_Limit()
        Console.WriteLine("=== Limits ===")

        Console.WriteLine($"  lim x->0 sin(x)/x   =  {Symbolic.Limit("sin(x) / x", "x", "0")}  (expected 1)")
        Console.WriteLine($"  lim x->0 (1-cos(x))/x^2 = {Symbolic.Limit("(1 - cos(x)) / (x ^ 2)", "x", "0")}  (expected 1/2)")
        Console.WriteLine($"  lim x->2 (x^2-4)/(x-2)  =  {Symbolic.Limit("(x ^ 2 - 4) / (x - 2)", "x", "2")}  (expected 4)")
        Console.WriteLine($"  lim x->0 exp(x)         =  {Symbolic.Limit("exp(x)", "x", "0")}  (expected 1)")
        Console.WriteLine($"  lim x->inf (x^2+1)/(2x^2+3) = {Symbolic.Limit("(x ^ 2 + 1) / (2 * x ^ 2 + 3)", "x", "inf")}  (expected 1/2)")

        Console.WriteLine()
    End Sub

    Sub test_Taylor()
        Console.WriteLine("=== Taylor series ===")

        For Each s In {"exp(x)", "sin(x)", "cos(x)", "1 / (1 - x)"}
            Dim expr = ScriptEngine.ParseExpression(s)
            Dim series = Symbolic.Taylor(expr, "x", "0", 4)
            Console.WriteLine($"  T_4 {s} about 0  =  {series}")
        Next

        Dim ex = ScriptEngine.ParseExpression("exp(x)")
        Dim res = Microsoft.VisualBasic.Math.Lambda.Symbolic.Taylor.TaylorWithRemainder(ex, "x", ScriptEngine.ParseExpression("0"), 3)
        Console.WriteLine($"  e^x T_3 = {res.polynomial}")
        Console.WriteLine($"  e^x R_3 = {res.remainder}")

        Console.WriteLine()
    End Sub

    Sub test_Boolean()
        Console.WriteLine("=== Boolean algebra (Quine-McCluskey) ===")

        ' f(A,B,C) = Sigma(1,3,7)  ->  A'C + BC
        Dim f = {1, 3, 7}
        Console.WriteLine($"  minterms {{1,3,7}}  ->  {String.Join(" + ", Symbolic.QuineMcCluskey({"A", "B", "C"}, f))}")
        Console.WriteLine($"  SOP expr           ->  {Symbolic.QMCSimplifySOP({"A", "B", "C"}, f)}")

        ' truth table derived minterms
        Dim mt = Symbolic.TruthTable({"A", "B", "C"}, Function(b) (b(0) AndAlso b(1)) OrElse (Not b(2)))
        Console.WriteLine($"  truth-table minterms of (A AND B) OR (NOT C): {mt.JoinBy(", ")}")
        Console.WriteLine($"  SOP                 ->  {Symbolic.QMCSimplifySOP({"A", "B", "C"}, mt)}")
        Console.WriteLine($"  POS                 ->  {Symbolic.QMCSimplifyPOS({"A", "B", "C"}, mt)}")

        Console.WriteLine()
    End Sub
End Module
