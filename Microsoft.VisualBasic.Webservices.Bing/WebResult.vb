
Imports System.Text.RegularExpressions

Public Class WebResult : Inherits Microsoft.VisualBasic.WebResult

    Const PAGET_TITLE_AND_URL As String = "<h2><a href="".+?"" target="".+?"" h="".+?"">.+?</a></h2>"

    Public Shared Function TryParse(strData As String) As WebResult
        Dim Title As String = Regex.Match(strData, PAGET_TITLE_AND_URL).Value
        Dim URL As String = Title.Get_href
        Title = Regex.Match(Title, """>.+</a>").Value
        Title = Title.GetValue.Trim

        Dim BriefText As String = Regex.Match(strData, "<div class=""b_caption""><p>.+</p>").Value.Replace("<div class=""b_caption""><p>", "")
        If Len(BriefText) > 5 Then BriefText = Mid(BriefText, 1, Len(BriefText) - 4)

        Return New WebResult With {.Title = Title, .URL = URL, .BriefText = BriefText}
    End Function
End Class
