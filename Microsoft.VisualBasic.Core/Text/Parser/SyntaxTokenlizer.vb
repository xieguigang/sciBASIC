Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace Text.Parser

    Public MustInherit Class SyntaxTokenlizer(Of T As IComparable, SyntaxToken As CodeToken(Of T))

        Protected ReadOnly text As CharPtr
        Protected buffer As CharBuffer

        Protected ReadOnly Property lastSplashEscape As Boolean
            Get
                If buffer = 0 Then
                    Return False
                ElseIf buffer = 1 Then
                    If buffer = "\"c Then
                        Return True
                    Else
                        Return False
                    End If
                Else
                    Return buffer(-1) = "\"c AndAlso buffer(-2) <> "\"c
                End If
            End Get
        End Property

        Sub New(text As [Variant](Of String, CharPtr))
            If text Like GetType(String) Then
                Me.text = New CharPtr(text.TryCast(Of String))
            Else
                Me.text = text.TryCast(Of CharPtr)
            End If
        End Sub

        Public Overridable Iterator Function GetTokens() As IEnumerable(Of SyntaxToken)
            Dim token As New Value(Of SyntaxToken)

            Do While text
                If Not token = walkChar(++text) Is Nothing Then
                    Yield CType(token, SyntaxToken)
                End If
            Loop

            If buffer > 0 Then
                Yield popOutToken()
            End If
        End Function

        Protected MustOverride Function walkChar(c As Char) As SyntaxToken
        Protected MustOverride Function popOutToken() As SyntaxToken
    End Class
End Namespace