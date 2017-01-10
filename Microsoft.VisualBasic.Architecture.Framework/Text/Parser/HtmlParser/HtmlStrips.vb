Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Text.HtmlParser

    Public Module HtmlStrips

        Public Const PAGE_CONTENT_TITLE As String = "<title>.+</title>"

        ''' <summary>
        ''' Parsing the title text from the html inputs.
        ''' </summary>
        ''' <param name="html"></param>
        ''' <returns></returns>
        <Extension> Public Function HTMLTitle(html As String) As String
            Dim title As String =
                Regex.Match(html, PAGE_CONTENT_TITLE, RegexOptions.IgnoreCase).Value

            If String.IsNullOrEmpty(title) Then
                title = "NULL_TITLE"
            Else
                title = title.GetValue
            End If

            Return title
        End Function

        ''' <summary>
        ''' Removes the html tags from the text string.
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        <ExportAPI("Html.Tag.Trim"), Extension> Public Function StripHTMLTags(s$) As String
            If String.IsNullOrEmpty(s) Then
                Return ""
            End If

            s = Regex.Replace(s, "<[^>]+>", "")
            s = Regex.Replace(s, "</[^>]+>", "")

            Return s
        End Function

        Const HTML_TAG As String = "</?.+?(\s+.+?="".+?"")*>"

        ''' <summary>
        ''' Gets the link text in the html fragement text.
        ''' </summary>
        ''' <param name="html">A string that contains the url string pattern like: href="url_text"</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Html.Href")>
        <Extension> Public Function href(<Parameter("HTML", "A string that contains the url string pattern like: href=""url_text""")> html$) As String

            If String.IsNullOrEmpty(html) Then
                Return ""
            End If

            Dim url As String = Regex.Match(html, "href="".+?""", RegexOptions.IgnoreCase).Value

            If String.IsNullOrEmpty(url) Then
                Return ""
            Else
                url = Mid(url, 6)
                url = Mid(url, 2, Len(url) - 2)
                Return url
            End If
        End Function

        Public Const IMAGE_SOURCE As String = "<img.+?src=.+?>"

        ''' <summary>
        ''' Parsing image source url from the img html tag.
        ''' </summary>
        ''' <param name="str"></param>
        ''' <returns></returns>
        <Extension> Public Function ImageSource(str As String) As String
            str = Regex.Match(str, "src="".+?""", RegexOptions.IgnoreCase).Value
            str = Mid(str, 5)
            str = Mid(str, 2, Len(str) - 2)
            Return str
        End Function

        ''' <summary>
        ''' 有些时候后面可能会存在多余的vbCrLf，则使用这个函数去除
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        <Extension> Public Function TrimResponseTail(value As String) As String
            If String.IsNullOrEmpty(value) Then
                Return ""
            End If

            Dim l As Integer = Len(value)
            Dim i As Integer = value.LastIndexOf(vbCrLf)
            If i = l - 2 Then
                Return Mid(value, 1, l - 2)
            Else
                Return value
            End If
        End Function

        Private ReadOnly vbCrLfLen As Integer = Len(vbCrLf)

        ''' <summary>
        ''' 获取两个尖括号之间的内容
        ''' </summary>
        ''' <param name="html"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Html.GetValue", Info:="Gets the string value between two wrapper character.")>
        <Extension> Public Function GetValue(html As String) As String
            Return html.GetStackValue(">", "<")
        End Function
    End Module
End Namespace