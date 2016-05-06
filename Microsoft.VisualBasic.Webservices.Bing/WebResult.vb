Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Serialization

''' <summary>
''' 一个结果条目
''' </summary>
Public Class WebResult : Inherits Microsoft.VisualBasic.WebResult

    Const PAGET_TITLE_AND_URL As String = "<h2><a href="".+?"" target="".+?"" h="".+?"">.+?</a></h2>"

    Public Shared Function TryParse(html As String) As WebResult
        Dim Title As String = Regex.Match(html, PAGET_TITLE_AND_URL).Value
        Dim URL As String = Title.Get_href
        Title = Regex.Match(Title, """>.+</a>").Value
        Title = Title.GetValue.Trim

        Dim BriefText As String = Regex.Match(html, "<div class=""b_caption""><p>.+</p>").Value.Replace("<div class=""b_caption""><p>", "")
        If Len(BriefText) > 5 Then BriefText = Mid(BriefText, 1, Len(BriefText) - 4)

        Return New WebResult With {
            .Title = Title,
            .URL = URL,
            .BriefText = BriefText
        }
    End Function
End Class

Public Class SearchResult

    ''' <summary>
    ''' 总的结果数目
    ''' </summary>
    ''' <returns></returns>
    Public Property Results As Integer
    Public Property CurrentPage As WebResult()

    Public Function NextPage() As SearchResult

    End Function

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class