Public Class ExpressionEngine

    ReadOnly symbols As New Dictionary(Of String, Double)

    Public Function GetSymbolValue(name As String) As Double
        Return symbols(name)
    End Function

    Public Function SetSymbol(symbol As String, value As Double) As ExpressionEngine
        symbols(symbol) = value
        Return Me
    End Function

    Public Function Evaluate(expression As String) As Double
        Dim tokens As MathToken() = New ExpressionTokenIcer(expression).GetTokens.ToArray
        Dim exp As Expression = ExpressionBuilder.BuildExpression(tokens)
        Dim result As Double = exp.Evaluate(Me)

        Return result
    End Function
End Class
