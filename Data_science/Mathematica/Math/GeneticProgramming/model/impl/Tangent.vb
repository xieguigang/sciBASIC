Imports std = System.Math

Namespace model.impl
    Public Class Tangent
        Inherits AbstractUnaryExpression

        Public Sub New(child As Expression)
            MyBase.New(child)
        End Sub

        Public Overrides Function eval(x As Double) As Double
            Return std.Tan(childField.eval(x))
        End Function

        Public Overrides Function toStringExpression() As String
            Return String.Format("tan({0})", childField.toStringExpression())
        End Function

        Public Overrides Function ToString() As String
            Return "tan()"
        End Function

    End Class

End Namespace
