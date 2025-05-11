Namespace Scripting.MathExpression.Impl

    Public Class LogicalLiteral : Inherits Expression

        Public Property logical As Boolean

        Sub New(text As String)
            logical = text.ParseBoolean
        End Sub

        Public Overrides Function Evaluate(env As ExpressionEngine) As Double
            Return If(logical, 1, 0)
        End Function

        Public Overrides Function ToString() As String
            Return logical.ToString
        End Function
    End Class
End Namespace