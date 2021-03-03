Namespace MathML

    Public Class MathFunctionExpression : Inherits MathExpression

        Public Property name As String
        Public Property parameters As MathExpression()

        Public Overrides Function ToString() As String
            Return $"{name}({parameters.JoinBy(", ")})"
        End Function
    End Class
End Namespace