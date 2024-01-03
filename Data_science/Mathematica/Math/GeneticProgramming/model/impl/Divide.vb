Namespace model.impl

    Public Class Divide : Inherits AbstractBinaryExpression

        Public Sub New(leftChild As Expression, rightChild As Expression)
            MyBase.New(leftChild, rightChild)
        End Sub

        Public Overrides Function eval(x As Double) As Double
            Return leftChildField.eval(x) / rightChildField.eval(x)
        End Function

        Public Overrides Function toStringExpression() As String
            Return String.Format("({0} / {1})", leftChildField.toStringExpression(), rightChildField.toStringExpression())
        End Function

        Public Overrides Function ToString() As String
            Return "L/R"
        End Function

    End Class

End Namespace
