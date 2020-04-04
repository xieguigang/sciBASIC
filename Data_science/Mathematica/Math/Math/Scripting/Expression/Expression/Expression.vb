Namespace Scripting.MathExpression.Impl

    Public MustInherit Class Expression

        Public MustOverride Function Evaluate(env As ExpressionEngine) As Double

    End Class
End Namespace