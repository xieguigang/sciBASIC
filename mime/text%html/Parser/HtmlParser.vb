Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Markup.HTML

Public Class HtmlParser

    Public Shared Function ParseTree(document As String) As HtmlElement
        Dim tokens = New TokenIcer(document).GetTokens.ToArray
        Dim i As Pointer(Of Token) = tokens
        Dim html As New HtmlElement With {.Name = "~"}

        Call WalkElement(i, html)

        Return html
    End Function

    Private Shared Sub WalkElement(i As Pointer(Of Token), parent As HtmlElement)
LOOP_NEXT:
        Dim a As Value(Of Token) = ++i

        If Not a.HasValue Then
            Return
        End If

        Select Case CType(a, Token).name
            Case HtmlTokens.openTag
                ' add new element tag
                ' <tag
                Dim tagName As String = (++i).text

                If tagName = "/" Then
                    ' </tag>
                    If i.Current.text = parent.Name Then
                        i.MoveNext()
                        i.MoveNext()
                        Return
                    End If
                End If

                Dim newTag As New HtmlElement With {.Name = tagName}

                Do While (a = ++i).name <> HtmlTokens.closeTag
                    ' name=value
                    If i.Current.name = HtmlTokens.equalsSymbol Then
                        i.MoveNext()
                        newTag.Add(CType(a, Token).text, (++i).text)
                    Else
                        newTag.Add(CType(a, Token).text, "")
                    End If
                Loop

                parent.Add(newTag)

                WalkElement(i, newTag)
                GoTo LOOP_NEXT
            Case HtmlTokens.splash
                If i.Current.name = HtmlTokens.closeTag Then
                    ' <../>
                    i.MoveNext()
                    Return
                Else
                    parent.Add(New InnerPlantText("/"))
                End If
            Case HtmlTokens.text
                parent.Add(New InnerPlantText(CType(a, Token).text))
                GoTo LOOP_NEXT
        End Select
    End Sub
End Class
