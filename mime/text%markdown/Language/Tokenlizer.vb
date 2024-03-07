Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

Namespace Language

    Public Class Tokenlizer

        Dim buf As New CharBuffer
        Dim s As CharPtr
        Dim styles As New Styles
        Dim startNewLine As Boolean = True

        Sub New(text As String)
            ' normalized newline token
            s = text.LineTokens.JoinBy(ASCII.LF)
        End Sub

        Public Iterator Function GetTokens() As IEnumerable(Of Token)
            Do While True
                If s.EndRead Then
                    Exit Do
                End If

                For Each t As Token In WalkChar(++s)
                    If Not t Is Nothing Then
                        Yield t
                    End If
                Next
            Loop
        End Function

        Private Iterator Function WalkChar(c As Char) As IEnumerable(Of Token)
            If c = "#"c Then
                If buf > 0 AndAlso buf.Last <> "#"c Then
                    Yield measure()
                End If

                buf += c
            ElseIf c = "*"c Then
                If buf > 0 AndAlso buf.Last <> "*" Then
                    If styles.bold Then

                    End If
                End If

                buf += c
            ElseIf c = ">"c Then
                Yield measure()

                If startNewLine Then
                    styles.quote = True
                End If
            Else
                If c = ASCII.LF Then
                    startNewLine = True
                    Yield measure()
                Else
                    buf += c
                End If
            End If
        End Function

        Private Function measure() As Token
            If buf = 0 Then
                Return Nothing
            End If

            If buf.StartsWith("#"c) Then
                Dim levels As Integer = buf.AsEnumerable.TakeWhile(Function(c) c = "#"c).Count
                Dim title As String = New String(buf.PopAllChars).Trim(" "c, ASCII.TAB, "#"c)

                Return New Token(TokenTypes.Header, title, styles) With {.level = levels}
            End If

            Return Nothing
        End Function
    End Class
End Namespace