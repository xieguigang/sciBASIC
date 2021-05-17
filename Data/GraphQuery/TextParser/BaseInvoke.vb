Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.C
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
            Dim pipe = trim(parameters)

            If isArray Then
                Dim array As New HtmlElement With {.TagName = "trim"}

                If TypeOf document Is HtmlElement Then
                    For Each element In DirectCast(document, HtmlElement).HtmlElements
                        array.Add(pipe(element))
                    Next
                Else
                    array.Add(pipe(document))
                End If

                Return array
            Else
                Return pipe(document)
            End If
        End Function

        Private Function trim(parameters As String()) As Func(Of InnerPlantText, InnerPlantText)
            Dim chars As Char()

            If parameters.IsNullOrEmpty Then
                chars = {" "c, ASCII.TAB, ASCII.CR, ASCII.LF}
            Else
                chars = parameters.JoinBy("").ReplaceMetaChars.ToArray
            End If

            Return Function(i)
                       Dim text As String = i.GetPlantText

                       text = Strings.Trim(text)
                       text = text.Trim(chars)

                       Return New InnerPlantText With {.InnerText = text}
                   End Function
        End Function

        ''' <summary>
        ''' Take the nth element in the current node collection
        ''' </summary>
        ''' <param name="document"></param>
        ''' <param name="parameters"></param>
        ''' <param name="isArray"></param>
        ''' <returns></returns>
        <ExportAPI("eq")>
        Public Function eq(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Dim n As Integer = Integer.Parse(parameters(Scan0))
            Dim nItem As InnerPlantText = DirectCast(document, HtmlElement).HtmlElements(n)

            Return nItem
        End Function

        ''' <summary>
        ''' removes of the text string that matched the pattern of given regexp list
        ''' </summary>
        ''' <param name="document"></param>
        ''' <param name="parameters"></param>
        ''' <param name="isArray"></param>
        ''' <returns></returns>
        <ExportAPI("filter")>
        Public Function filter(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Dim patterns As Regex() = parameters _
                .Select(Function(p) New Regex(p)) _
                .ToArray

        End Function

        ''' <summary>
        ''' match the string with given regexp pattern
        ''' </summary>
        ''' <param name="document"></param>
        ''' <param name="parameters"></param>
        ''' <param name="isArray"></param>
        ''' <returns></returns>
        <ExportAPI("match")>
        Public Function match(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText

        End Function
    End Module
End Namespace