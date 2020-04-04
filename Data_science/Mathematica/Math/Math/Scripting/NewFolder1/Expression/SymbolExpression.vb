Public Class SymbolExpression : Inherits Expression

    Public ReadOnly Property symbolName As String

    Sub New(symbolName As String)
        Me.symbolName = symbolName
    End Sub

    Public Overrides Function Evaluate(env As ExpressionEngine) As Double
        Return env.GetSymbolValue(symbolName)
    End Function

    Public Overrides Function ToString() As String
        Return symbolName
    End Function
End Class
