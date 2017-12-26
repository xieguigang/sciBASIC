Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.HtmlParser
Imports r = System.Text.RegularExpressions.Regex

Public Module Translation

    Public Function GetTranslation(word As String) As WordTranslation
        Dim term$ = word.UrlEncode
        Dim url$ = $"https://cn.bing.com/dict/search?q={term}&qs=n&form=Z9LH5&sp=-1&pq={term}&sc=6-10&sk=&cvid=0BC4AECB5070489794D29912A900BEF5"
        Dim headers As New Dictionary(Of String, String) From {
            {"refer", SearchEngineProvider.BingRefer},
            {"accept-language", "zh-CN,zh;q=0.9,en;q=0.8,la;q=0.7"}
        }
        Dim meta$() = url _
            .GET(headers:=headers) _
            .Matches("<meta.+?/>", RegexICSng) _
            .ToArray
        Dim parsed = meta _
            .Select(Function(m) m.TagAttributes.ToDictionary) _
            .Where(Function(m)
                       Return m.ContainsKey("name") AndAlso m!name = "description"
                   End Function) _
            .FirstOrDefault

        If parsed Is Nothing Then
            Return Nothing
        Else
            Dim content$ = parsed!content.Value
            content = r _
                .Replace(content, "必应词典为您提供.+?的释义，", "") _
                .Replace("un.", "") _
                .Replace("n.", "")

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