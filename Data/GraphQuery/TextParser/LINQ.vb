Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.MIME.Html.Document
Imports Microsoft.VisualBasic.MIME.Html.Language.CSS
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Text

Namespace TextParser

    ''' <summary>
    ''' LINQ functions for the graphquery parser
    ''' </summary>
    Module LINQ

        <ExportAPI("skip")>
        Public Function skip(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            If Not isArray Then
                Throw New InvalidExpressionException("data should be an array!")
            ElseIf Not TypeOf document Is HtmlElement Then
                Return document
            End If

            Dim array As New HtmlElement With {
                .TagName = "skip",
                .Attributes = {AutoContext.Attribute}
            }
            Dim n As Integer = Integer.Parse(parameters(Scan0))

            If TypeOf document Is HtmlElement Then
                For Each element In DirectCast(document, HtmlElement).HtmlElements.Skip(n)
                    Call array.Add(element)
                Next
            Else
                Throw New InvalidExpressionException
            End If

            Return array
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
    End Module
End Namespace