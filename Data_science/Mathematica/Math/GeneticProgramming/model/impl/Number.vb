Namespace model.impl

    Public Class Number
        Inherits AbstractExpression

        Private number As Double

        Public Sub New(number As Double)
            Me.number = number
        End Sub

        Public Overridable Function getNumber() As Double
            Return number
        End Function

        Public Overridable Sub setNumber(number As Double)
            Me.number = number
        End Sub

        Public Overrides Function duplicate() As Expression
            Return New Number(number)
        End Function

        Public Overrides Function eval(x As Double) As Double
            Return number
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
            Return String.Format("{0:g}", number)
        End Function

    End Class

End Namespace
