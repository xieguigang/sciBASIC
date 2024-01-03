Namespace model.impl

    Public Class Variable : Inherits AbstractExpression

        Public Shared ReadOnly X As Variable = New Variable()

        Public Overrides Function duplicate() As Expression
            Return X
        End Function

        Public Overrides Function eval(x As Double) As Double
            Return x
        End Function

        Public Overrides ReadOnly Property Terminal As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property Depth As Integer
            Get
                Return 0
            End Get
        End Property

        Public Overrides Function toStringExpression() As String
            Return "X"
        End Function

    End Class

End Namespace
