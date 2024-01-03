Imports std = System.Math

Namespace model.impl

    Public Class PI
        Inherits Number

        Public Shared ReadOnly Pi As PI = New PI()

        Public Sub New()
            MyBase.New(std.PI)
        End Sub

        Public Overrides Function duplicate() As Expression
            Return Pi
        End Function

        Public Overrides Function toStringExpression() As String
            Return "π"
        End Function

    End Class

End Namespace
