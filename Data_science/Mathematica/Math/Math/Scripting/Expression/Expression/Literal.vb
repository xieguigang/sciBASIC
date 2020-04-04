Namespace Scripting.MathExpression.Impl

    Public Class Literal : Inherits Expression

        Public ReadOnly Property number As Double

        Sub New(text As String)
            Me.number = Val(text)
        End Sub

        Public Overrides Function Evaluate(env As ExpressionEngine) As Double
            Return number
        End Function

        Public Overrides Function ToString() As String
            Return number
        End Function
    End Class
End Namespace