Namespace model.impl

    Public MustInherit Class AbstractExpression
        Implements Expression
        Public MustOverride Function duplicate() As Expression Implements Expression.duplicate
        Public MustOverride ReadOnly Property Depth As Integer Implements Expression.Depth
        Public MustOverride Function toStringExpression() As String Implements Expression.toStringExpression
        Public MustOverride ReadOnly Property Terminal As Boolean Implements Expression.Terminal
        Public MustOverride Function eval(x As Double) As Double Implements Expression.eval

        Public Overrides Function ToString() As String
            Return toStringExpression()
        End Function

    End Class

End Namespace
