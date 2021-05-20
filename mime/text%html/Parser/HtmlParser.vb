Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Html.Document
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser

Namespace Language

    Public Class HtmlParser

        Shared ReadOnly tagsBreakStack As Index(Of String) = {"meta", "link", "img", "br", "hr", "input", "source"}

        Private Shared Function GetHtmlTokens(document As String) As Token()
            Dim tokens As Token()

            document = document.Replace("<!DOCTYPE html>", "")
            tokens = New TokenIcer(New StringBuilder(document).RemovesHtmlComments) _
            .GetTokens _
            .ToArray

            Return tokens
        End Function

        Public Shared Function ParseTree(document As String) As HtmlDocument
            Dim i As Pointer(Of Token) = GetHtmlTokens(document)
            Dim html As New HtmlDocument With {.TagName = "!DOCTYPE html"}
            Dim tagStack As New Stack(Of HtmlElement)
            Dim a As New Value(Of Token)

            tagStack.Push(html)

            Do While i
                Select Case (a = ++i).name
                    Case HtmlTokens.openTag
                        Dim name As String = Strings.Trim((++i).text).Trim(ASCII.CR, ASCII.LF)

                        If name = "/" Then
                            name = (++i).text

                            If name = tagStack.Peek.TagName Then
                                tagStack.Pop()
                            End If

                            i.MoveNext()
                        Else
                            Dim newTag As New HtmlElement With {.TagName = name}
                            Dim tagClosed As Boolean = False

                            Do While Not i.EndRead AndAlso (a = ++i).name <> HtmlTokens.closeTag
                                If i.EndRead Then
                                    Exit Do
                                End If

                                ' name=value
                                If i.Current.name = HtmlTokens.equalsSymbol Then
                                    i.MoveNext()
                                    newTag.Add(CType(a, Token).text, (++i).text)
                                ElseIf CType(a, Token).name = HtmlTokens.splash AndAlso i.Current.name = HtmlTokens.closeTag Then
                                    ' <.../>
                                    i.MoveNext()
                                    tagClosed = True
                                    Exit Do
                                Else
                                    newTag.Add(CType(a, Token).text, "")
                                End If
                            Loop

                            tagStack.Peek.Add(newTag)

                            If Not tagClosed Then
                                If Not Strings.LCase(newTag.TagName) Like tagsBreakStack Then
                                    tagStack.Push(newTag)
                                End If
                            End If
                        End If
                    Case Else
                        tagStack.Peek.Add(New InnerPlantText(CType(a, Token).text))
                End Select
            Loop

            Return html
        End Function
    End Class
End Namespace