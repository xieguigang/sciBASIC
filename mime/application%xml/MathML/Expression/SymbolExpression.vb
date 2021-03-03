Namespace MathML

    Public Class SymbolExpression : Inherits MathExpression

        Public Property text As String
        Public Property isNumericLiteral As Boolean

        Public Overrides Function ToString() As String
            Return text
        End Function

    End Class
End Namespace