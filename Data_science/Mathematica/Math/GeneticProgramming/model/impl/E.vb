Namespace model.impl

    Public Class E
        Inherits Number

        Public Shared ReadOnly e As E = New E()

        Public Sub New()
            MyBase.New(Math.E)
        End Sub

        Public Overrides Function duplicate() As Expression
            Return e
        End Function

        Public Overrides Function toStringExpression() As String
            Return "e"
        End Function

    End Class

End Namespace
