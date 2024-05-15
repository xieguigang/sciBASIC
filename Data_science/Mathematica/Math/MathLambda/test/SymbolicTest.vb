#Region "Microsoft.VisualBasic::5cab8d942e2473ae56fecef99ef0629a, Data_science\Mathematica\Math\MathLambda\test\SymbolicTest.vb"

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

    '   Total Lines: 73
    '    Code Lines: 47
    ' Comment Lines: 3
    '   Blank Lines: 23
    '     File Size: 2.27 KB


    ' Module SymbolicTest
    ' 
    '     Sub: expands, Main, test_Simplify, unit_test
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
        '  Call unit_test()
        Call test_Simplify()

        Pause()
    End Sub

    Sub unit_test()

        Call Console.WriteLine(Symbolic.Simplify("0+x"))
        Call Console.WriteLine(Symbolic.Simplify("0+x + 0 * a"))

        Pause()

    End Sub

    Sub test_Simplify()
        Dim symbols = ScriptEngine.ParseExpression("(5+5) * (2*x + x / 5 + x ^ 3)")
        Dim result = symbols.DoCall(AddressOf Symbolic.Simplify)

        Console.WriteLine($"{symbols} -> {result}")

        ScriptEngine.SetVariable("x", 33)

        Console.WriteLine($"{symbols} = {symbols.Evaluate(ScriptEngine.Expression)}")
        Console.WriteLine($"{result} = {result.Evaluate(ScriptEngine.Expression)}")

        ' (x ^ 2 * 2 + -1) ^ -2
        symbols = ScriptEngine.ParseExpression("(2 * x ^ 2 - 1 + 0 * a) ^ -1 * (2 * x ^ 2  - 1 * 1) ^ -1")
        result = symbols.DoCall(AddressOf Symbolic.Simplify)

        Console.WriteLine($"{symbols} -> {result}")

        ScriptEngine.SetVariable("a", Double.MaxValue)
        ScriptEngine.SetVariable("x", 3)

        Console.WriteLine($"{symbols} = {symbols.Evaluate(ScriptEngine.Expression)}")
        Console.WriteLine($"{result} = {result.Evaluate(ScriptEngine.Expression)}")
    End Sub

    Sub expands()
        Dim expr1 = ScriptEngine.ParseExpression($"(a+b)^3")
        Dim result1 = expr1.Expands

        expr1 = ScriptEngine.ParseExpression($"(a+b+c)^3")
        result1 = expr1.Expands

        ScriptEngine.SetVariable("a", 1)
        ScriptEngine.SetVariable("b", 2)
        ScriptEngine.SetVariable("c", 3)

        Call Console.WriteLine(expr1.Evaluate(ScriptEngine.Expression))
        Call Console.WriteLine(result1.Evaluate(ScriptEngine.Expression))

        For i As Integer = 0 To 4
            Dim expr = ScriptEngine.ParseExpression($"(a+b)^{i}")
            Dim result = expr.Expands

            Console.WriteLine($"{expr} -> {result}")
        Next

        Pause()
    End Sub
End Module
