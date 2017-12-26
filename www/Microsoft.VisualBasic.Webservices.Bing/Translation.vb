Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.HtmlParser
Imports r = System.Text.RegularExpressions.Regex

Public Module Translation

    Public Function GetTranslation(word As String) As WordTranslation
        Dim term$ = word.UrlEncode
        Dim url$ = $"https://cn.bing.com/dict/search?q={term}&qs=n&form=Z9LH5&sp=-1&pq={term}&sc=6-10&sk=&cvid=0BC4AECB5070489794D29912A900BEF5"
        Dim meta = url.GET.Matches("<meta.+?/>", RegexICSng).ToArray
        Dim parsed = meta.Select(Function(m) m.TagAttributes.ToDictionary).Where(Function(m) m.ContainsKey("name") AndAlso m!name = "description").FirstOrDefault

        If parsed Is Nothing Then
            Return Nothing
        Else
            Dim content$ = parsed!content.Value
            content = r.Replace(content, "必应词典为您提供.+?的释义，", "")
            content = content.Replace("un.", "").Replace("n.", "")

            Return New WordTranslation With {
                .Word = word,
                .Translations = content _
                    .Split("；"c) _
                    .Select(Function(t) t.Split("："c).Last.Split("，"c)) _
                    .IteratesALL _
                    .ToArray
            }
        End If
    End Function
End Module

Public Class WordTranslation

    Public Property Word As String
    Public Property Translations As String()

    Public Overrides Function ToString() As String
        Return $"{Word} -> {Translations.GetJson}"
    End Function
End Class