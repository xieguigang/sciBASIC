Imports std = System.Math

Namespace model.impl

    Public Class Tau : Inherits Number

        Public Shared ReadOnly Tau As New Tau

        Public Sub New()
            MyBase.New(std.Tau)
        End Sub

        Public Overrides Function duplicate() As Expression
            Return Tau
        End Function

        Public Overrides Function toStringExpression() As String
            Return "Tau"
        End Function
    End Class
End Namespace