Namespace Scripting.MathExpression.Impl

    Public Class UnaryNot : Inherits Expression

        Public Property value As Expression

        Public Overrides Function Evaluate(env As ExpressionEngine) As Double
            Dim val As Double = value.Evaluate(env)
            Return If(val = 0.0, 1, 0)
        End Function
    End Class
End Namespace