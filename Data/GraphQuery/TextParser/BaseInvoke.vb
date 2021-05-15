Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.Text

Namespace TextParser

    Module BaseInvoke

        ''' <summary>
        ''' Extract the text of the current node
        ''' </summary>
        ''' <param name="document"></param>
        ''' <param name="parameters"></param>
        ''' <param name="isArray"></param>
        ''' <returns></returns>
        <ExportAPI("text")>
        Public Function text(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            If isArray Then
                Dim array As New HtmlElement With {.TagName = "text"}

                If TypeOf document Is HtmlElement Then
                    For Each element In DirectCast(document, HtmlElement).HtmlElements
                        If TypeOf element Is HtmlElement Then
                            array.Add(New InnerPlantText With {.InnerText = element.GetPlantText})
                        Else
                            array.Add(element)
                        End If
                    Next
                Else
                    array.Add(document)
                End If

                Return array
            Else
                If TypeOf document Is HtmlElement Then
                    Return New InnerPlantText With {
                        .InnerText = document.GetPlantText
                    }
                Else
                    Return document
                End If
            End If
        End Function

        ''' <summary>
        ''' Clear spaces and line breaks before and after the string
        ''' </summary>
        ''' <param name="document"></param>
        ''' <param name="parameters"></param>
        ''' <param name="isArray"></param>
        ''' <returns></returns>
        <ExportAPI("trim")>
        Public Function trim(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            If isArray Then
                Dim array As New HtmlElement With {.TagName = "trim"}

                If TypeOf document Is HtmlElement Then
                    For Each element In DirectCast(document, HtmlElement).HtmlElements
                        array.Add(New InnerPlantText With {.InnerText = Strings.Trim(element.GetPlantText).Trim(" "c, ASCII.TAB, ASCII.CR, ASCII.LF)})
                    Next
                Else
                    array.Add(New InnerPlantText With {.InnerText = Strings.Trim(document.GetPlantText).Trim(" "c, ASCII.TAB, ASCII.CR, ASCII.LF)})
                End If

                Return array
            Else
                Return New InnerPlantText With {
                    .InnerText = Strings _
                        .Trim(document.GetPlantText) _
                        .Trim(" "c, ASCII.TAB, ASCII.CR, ASCII.LF)
                }
            End If
        End Function
    End Module
End Namespace