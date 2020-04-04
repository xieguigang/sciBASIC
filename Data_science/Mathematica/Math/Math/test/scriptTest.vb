Imports Microsoft.VisualBasic.Math

Module scriptTest
    Sub Main()
        Dim tokens = New ExpressionTokenIcer("(((1+X33 + 9!) ^ 9) * (5+8! %33))").GetTokens.ToArray
        Dim expression = ExpressionBuilder.BuildExpression(tokens)

        Pause()
    End Sub
End Module
