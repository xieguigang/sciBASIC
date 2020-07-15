Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Lambda
Imports Microsoft.VisualBasic.Math.Scripting

Module SymbolicTest

    Sub Main()
        Dim symbols = ScriptEngine.ParseExpression("(5+5) * (2*x + x / 5 + x ^ 3)")
        Dim result = symbols.DoCall(AddressOf Symbolic.Simplify)

        Console.WriteLine($"{symbols} -> {result}")

        ScriptEngine.SetVariable("x", 33)

        Console.WriteLine($"{symbols} = {symbols.Evaluate(ScriptEngine.Expression)}")
        Console.WriteLine($"{result} = {result.Evaluate(ScriptEngine.Expression)}")

        Pause()
    End Sub
End Module
