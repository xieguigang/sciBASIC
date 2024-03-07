Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

Namespace Language

    Public Class Tokenlizer

        Dim buf As CharBuffer
        Dim s As CharPtr
        Dim styles As Styles
        Dim startNewLine As Boolean = True

        Sub New(text As String)
            s = text
        End Sub

        Public Iterator Function GetTokens() As IEnumerable(Of Token)
            Dim tokens As Value(Of Token()) = {}

            Do While (tokens = WalkChar(++s).ToArray).Length > 0
                For Each t As Token In tokens.AsEnumerable
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
                    Yield measure()
                End If

                buf += c
            ElseIf c = ">"c Then
            Else
                If c = ASCII.CR OrElse c = ASCII.LF Then
                    startNewLine = True
                    Yield measure()
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