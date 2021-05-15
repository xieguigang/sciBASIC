Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

Namespace Language

    Public Class TokenIcer : Inherits SyntaxTokenlizer(Of Tokens, Token)

        ReadOnly escape As New Escaping

        Public Sub New(text As [Variant](Of String, CharPtr))
            MyBase.New(text)
        End Sub

        Protected Overrides Function walkChar(c As Char) As Token
            If escape.string Then
                If c <> """"c Then
                    buffer += c
                    Return Nothing
                Else
                    escape.string = False
                    Return New Token(Tokens.text, buffer.PopAllChars)
                End If
            ElseIf escape.comment Then
                If c <> ASCII.CR AndAlso c <> ASCII.LF Then
                    buffer += c
                    Return Nothing
                Else
                    Dim t As New Token(Tokens.comment, buffer.PopAllChars)
                    escape.comment = False
                    buffer += ASCII.LF
                    Return t
                End If
            Else
                Select Case c
                    Case "{"c, "("c, "["c, "}"c, "]"c, ")"c, "|"c
                        Dim t As Token = popOutToken()
                        buffer += c
                        Return t
                    Case " "c, ASCII.TAB
                        Return popOutToken()
                    Case ASCII.CR, ASCII.LF
                        Dim t = popOutToken()
                        buffer += ASCII.LF
                        Return t
                    Case """"c
                        Dim t As Token = popOutToken()
                        escape.string = True
                        Return t
                    Case "#"c
                        Dim t As Token = popOutToken()
                        escape.comment = True
                        Return t
                    Case Else
                        Dim t As Token = Nothing

                        If buffer = 1 AndAlso buffer.ToString = "|" Then
                            t = popOutToken()
                        End If

                        buffer += c

                        Return t
                End Select
            End If
        End Function

        Protected Overrides Function popOutToken() As Token
            If buffer = 0 Then
                Return Nothing
            End If

            Dim text As New String(buffer.PopAllChars)

            Select Case text
                Case "{", "[", "(" : Return New Token(Tokens.open, text)
                Case "}", "]", ")" : Return New Token(Tokens.close, text)
                Case "|" : Return New Token(Tokens.pipeline, text)
                Case ASCII.LF
                    ' Return New Token(Tokens.terminator, ";")
                    Return Nothing
                Case Else
                    Return New Token(Tokens.symbol, text.Trim(ASCII.LF, ASCII.CR))
            End Select
        End Function
    End Class
End Namespace