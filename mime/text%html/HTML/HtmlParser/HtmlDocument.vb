#Region "Microsoft.VisualBasic::b55c6e14690ff006ade8c2ae5bc0bcd1, mime\text%html\HTML\HtmlParser\HtmlDocument.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class HtmlDocument
    ' 
    '         Properties: Tags
    ' 
    '         Function: Load, LoadDocument
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Net
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Text
Imports Microsoft.VisualBasic.Language

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
            Dim html As String = url.GET.Replace(vbCr, "").Replace(vbLf, "") '是使用<br />标签来分行的
            Dim List As New List(Of InnerPlantText)

            html = Regex.Replace(html, "<!--.+?-->", "")

            Do While html.Length > 0
                Dim element As InnerPlantText = DocParserAPI.TextParse(html)
                If element Is Nothing Then
                    Exit Do
                Else
                    If Not element.IsEmpty Then
                        Call List.Add(element)
                    End If
                End If
            Loop

            Me.Tags = List

            Return Me
        End Function

        Public Shared Function Load(url As String) As HtmlDocument
            Return New HtmlDocument().LoadDocument(url)
        End Function
    End Class
End Namespace
