Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MIME.Markup.HTML

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

        End Function
    End Module
End Namespace