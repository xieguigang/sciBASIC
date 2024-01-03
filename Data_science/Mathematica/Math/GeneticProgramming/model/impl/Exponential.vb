Imports std = System.Math

Namespace model.impl

    Public Class Exponential
        Inherits AbstractUnaryExpression

        Public Sub New(child As Expression)
            MyBase.New(child)
        End Sub

        Public Overrides Function eval(x As Double) As Double
            Return std.Exp(childField.eval(x))
        End Function

        Public Overrides Function toStringExpression() As String
            Return String.Format("exp({0})", childField.toStringExpression())
        End Function

        Public Overrides Function ToString() As String
            Return "exp()"
        End Function

    End Class

End Namespace
