Public Class SymbolExpression : Inherits Expression

    Public ReadOnly Property symbolName As String

    Public Overrides Function Evaluate(env As ExpressionEngine) As Double
        Return env.GetSymbolValue(symbolName)
    End Function

    Public Overrides Function ToString() As String
        Return symbolName
    End Function
End Class
