Imports Microsoft.VisualBasic.Math

Module scriptTest
    Sub Main()
        'Dim tokens = New ExpressionTokenIcer("(((1+X33 + 9!) ^ 9) * (5+8! %33))").GetTokens.ToArray
        'Dim expression = ExpressionBuilder.BuildExpression(tokens)

        'Console.WriteLine(New ExpressionEngine().SetSymbol("X33", -100).Evaluate("(((1+X33 + 9!) ^ 9) * (5+8! %33))"))
        Console.WriteLine(New ExpressionEngine().Evaluate("1+ abs(-100-99)"))

        Pause()
    End Sub
End Module
