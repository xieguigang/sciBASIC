Imports std = System.Math

Namespace model.impl

    Public Class Logarithm : Inherits AbstractUnaryExpression

        Public Sub New(child As Expression)
            MyBase.New(child)
        End Sub

        Public Overrides Function eval(x As Double) As Double
            Return std.Log(m_child.eval(x))
        End Function

        Public Overrides Function toStringExpression() As String
            Return String.Format("log({0})", m_child.toStringExpression())
        End Function

        Public Overrides Function ToString() As String
            Return "log()"
        End Function

    End Class

End Namespace
