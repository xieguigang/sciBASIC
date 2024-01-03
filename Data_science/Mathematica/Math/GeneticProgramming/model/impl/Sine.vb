Imports std = System.Math

Namespace model.impl

    Public Class Sine
        Inherits AbstractUnaryExpression

        Public Sub New(child As Expression)
            MyBase.New(child)
        End Sub

        Public Overrides Function eval(x As Double) As Double
            Return std.Sin(childField.eval(x))
        End Function

        Public Overrides Function toStringExpression() As String
            Return String.Format("sin({0})", childField.toStringExpression())
        End Function

        Public Overrides Function ToString() As String
            Return "sin()"
        End Function

    End Class

End Namespace
