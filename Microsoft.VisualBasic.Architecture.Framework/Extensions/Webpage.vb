Imports System.Text.RegularExpressions
Imports System.Runtime.CompilerServices

Public Module Webpage

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="s_Data">A string that contains the url string pattern like: href="url_text"</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Get_href(s_Data As String) As String
        Dim url As String = Regex.Match(s_Data, "href="".+?""", RegexOptions.IgnoreCase).Value
        If String.IsNullOrEmpty(url) Then
            Return ""
        Else
            url = Mid(url, 6)
            url = Mid(url, 2, Len(url) - 2)
            Return url
        End If
    End Function

    Const HTML_TAG As String = "</?.+?(\s+.+?="".+?"")*>"

    <Extension> Public Function TrimHTMLTag(str As String) As String
        If String.IsNullOrEmpty(str) Then
            Return ""
        End If

        str = Regex.Replace(str, HTML_TAG, "")
        Return str
    End Function
End Module
