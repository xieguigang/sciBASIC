Imports Microsoft.VisualBasic.Data.visualize.Network.Styling

Module expressionTest

    Const rangeMapper = "map(degree, [100,200])"
    Const discreteMapper = "map(domain, A=1, B=2, C= 99, G=8888)"

    Sub Main()

        Dim a = SyntaxExtensions.MapExpressionParser(rangeMapper)
        Dim b = SyntaxExtensions.MapExpressionParser(discreteMapper)

        Pause()

    End Sub
End Module
