Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

Public Class TokenIcer

    Dim buf As New CharBuffer
    Dim text As CharPtr
    Dim escape As New Escaping

    Sub New(document As CharPtr)
        Me.text = document
    End Sub

    Public Iterator Function GetTokens() As IEnumerable(Of Token)
        Do While text
            For Each t As Token In WalkChar(++text)
                If Not t Is Nothing Then
                    Yield t
                End If
            Next
        Loop

        If buf > 0 Then
            Dim t As Token = MeasureToken(New String(buf.PopAllChars))

            If Not t Is Nothing Then
                Yield t
            End If
        End If
    End Function

    Private Iterator Function WalkChar(c As Char) As IEnumerable(Of Token)
        If escape.string Then
            If c = escape.quote Then
                Yield New Token(HtmlTokens.text, buf.PopAllChars)

                escape.string = False
                escape.quote = Nothing
            Else
                buf += c
            End If
        ElseIf c = "<"c Then
            If buf > 0 Then
                Yield MeasureToken(New String(buf.PopAllChars))
            End If

            escape.tagOpen = True

            Yield New Token(HtmlTokens.openTag, c)
        ElseIf c = "/"c Then
            If buf > 0 Then
                Yield MeasureToken(New String(buf.PopAllChars))
            End If

            Yield New Token(HtmlTokens.splash, c)
        ElseIf c = ">"c Then
            If buf > 0 Then
                Yield MeasureToken(New String(buf.PopAllChars))
            End If

            escape.tagOpen = False

            Yield New Token(HtmlTokens.closeTag, c)
        ElseIf c = """"c OrElse c = "'"c Then
            If buf > 0 Then
                Yield MeasureToken(New String(buf.PopAllChars))
            End If

            escape.string = True
            escape.quote = c
        ElseIf escape.tagOpen AndAlso (c = " "c OrElse c = ASCII.TAB) Then
            If buf > 0 Then
                Yield MeasureToken(New String(buf.PopAllChars))
            End If
        ElseIf c = "="c Then
            If buf > 0 Then
                Yield MeasureToken(New String(buf.PopAllChars))
            End If

            Yield New Token(HtmlTokens.equalsSymbol, "=")
        Else
            buf += c
        End If
    End Function

    Private Function MeasureToken(text As String) As Token
        If text = "=" Then
            Return New Token(HtmlTokens.equalsSymbol, "=")
        ElseIf text = vbCr OrElse text = vbLf OrElse text = vbCrLf Then
            Return Nothing
        ElseIf text.Trim(" "c, ASCII.CR, Ascii.LF) = "" Then
            Return Nothing
        Else
            Return New Token(HtmlTokens.text, text)
        End If
    End Function

End Class

Friend Class Escaping

    Public [string] As Boolean
    Public quote As Char
    Public tagOpen As Boolean

End Class