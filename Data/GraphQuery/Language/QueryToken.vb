Namespace Language

    Public Class QueryToken

        Public Property token As Token
        Public Property func As Parser

        Public ReadOnly Property name As Tokens
            Get
                If token Is Nothing Then
                    Return Tokens.NA
                Else
                    Return token.name
                End If
            End Get
        End Property

        Public ReadOnly Property text As String
            Get
                If token Is Nothing Then
                    Return ""
                Else
                    Return token.text
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            If token Is Nothing Then
                Return func.ToString
            Else
                Return token.text
            End If
        End Function

    End Class
End Namespace