Imports Microsoft.VisualBasic.Math.Lambda
Imports Microsoft.VisualBasic.Math.Scripting
Imports Sym = Microsoft.VisualBasic.Math.Lambda.Symbolic.Symbolic

Module Module1

    Sub Main()
        Console.WriteLine("=== Rationalization ===")
        For Each s In {"1 / (1 + sqrt(2))", "1 / sqrt(2)", "1 / (sqrt(a) + sqrt(b))", "a / (b + sqrt(c))", "1 / (x + y * i)"}
            Console.WriteLine($"  rationalize({s}) = {Sym.Rationalize(ScriptEngine.ParseExpression(s))}")
        Next

        Console.WriteLine("=== Simplify auto-factor ===")
        For Each s In {"x ^ 2 + 2 * x + 1", "x ^ 2 - 1", "x ^ 3 - x", "x ^ 2 + x", "x + x"}
            Console.WriteLine($"  Simplify({s}) = {Sym.Simplify(ScriptEngine.ParseExpression(s))}")
        Next

        Console.WriteLine("=== Enhanced integration ===")
        For Each s In {"tan(x)", "sec(x)", "csc(x)", "cot(x)", "sin(x) ^ 2", "cos(x) ^ 2", "sec(x) ^ 2",
                       "1 / (x ^ 2 - 4)", "1 / (4 - x ^ 2)", "1 / (x ^ 2 + 4)",
                       "2 * x * exp(x ^ 2)", "2 * x * cos(x ^ 2)"}
            Console.WriteLine($"  Int {s} = {Sym.Integrate(ScriptEngine.ParseExpression(s), "x")}")
        Next

        Console.WriteLine("=== Multivariate factor ===")
        For Each s In {"x ^ 2 - y ^ 2", "x ^ 2 + 2 * x * y + y ^ 2", "x ^ 4 - y ^ 4",
                       "x * y + x", "4 * x ^ 2 - 9 * y ^ 2", "x ^ 2 + y ^ 2"}
            Console.WriteLine($"  Factor({s}) = {Sym.Factor(ScriptEngine.ParseExpression(s))}")
        Next
    End Sub
End Module
