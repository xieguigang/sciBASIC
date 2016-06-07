Imports System.Net
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Text

Namespace HTML

    Public Class HtmlDocument

        Public Const HTML_PAGE_CONTENT_TITLE As String = "<title>.+?</title>"

        Public Property Tags As InnerPlantText()

        ''' <summary>
        ''' 假设所加载的html文档是完好的格式的，即没有不匹配的标签的
        ''' </summary>
        ''' <param name="url"></param>
        ''' <returns></returns>
        Public Function LoadDocument(url As String) As HtmlDocument
            Dim pageContent As String = url.GET.Replace(vbCr, "").Replace(vbLf, "") '是使用<br />标签来分行的
            Dim List As List(Of InnerPlantText) = New List(Of InnerPlantText)

            pageContent = Regex.Replace(pageContent, "<!--.+?-->", "")

            Do While pageContent.Length > 0
                Dim element As InnerPlantText = DocParserAPI.TextParse(pageContent)
                If element Is Nothing Then
                    Exit Do
                Else
                    If Not element.IsEmpty Then
                        Call List.Add(element)
                    End If
                End If
            Loop

            Return Me.InvokeSet(NameOf(Tags), List.ToArray)
        End Function

        Public Shared Function Load(url As String) As HtmlDocument
            Return New HtmlDocument().LoadDocument(url)
        End Function
    End Class
End Namespace