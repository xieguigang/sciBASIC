Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.MIME.Markup.HTML

Public Class HtmlParser

    Public Shared Function ParseTree(document As String) As HtmlElement
        Dim tokens = New TokenIcer(document).GetTokens.ToArray
        Dim i As Pointer(Of Token) = tokens
        Dim html As New HtmlElement

        Call WalkElement(i, html)

        Return html
    End Function

    Private Shared Sub WalkElement(i As Pointer(Of Token), parent As HtmlElement)
LOOP_NEXT:
        Dim a As Token = ++i

        Select Case a.name
            Case HtmlTokens.openTag
                ' add new element tag
                Dim newTag As New HtmlElement With {.Name = (++i).text}

                parent.Add(newTag)
                WalkElement(i, newTag)
            Case HtmlTokens.splash
                If i.Current.name = HtmlTokens.closeTag Then
                    ' <../>
                    i.MoveNext()
                    Return
                Else
                    parent.Add(New InnerPlantText("/"))
                End If
            Case HtmlTokens.text
                If i.Current.name = HtmlTokens.equalsSymbol Then
                    ' xxx = xxx
                    i.MoveNext()
                    parent.Add(a.text, (++i).text)
                    GoTo LOOP_NEXT
                Else
                    parent.Add(New InnerPlantText(a.text))
                End If
        End Select
    End Sub
End Class
