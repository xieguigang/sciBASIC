Imports Microsoft.VisualBasic.Math

Module scriptTest
    Sub Main()
        Dim tokens = New ExpressionParser("((1+X33 + 9!) ^ 9) * (5+8! %33)").GetTokens.ToArray

        Pause()
    End Sub
End Module
