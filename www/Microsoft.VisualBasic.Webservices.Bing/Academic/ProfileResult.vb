Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text.HtmlParser
Imports r = System.Text.RegularExpressions.Regex

Namespace Academic

    ''' <summary>
    ''' Example as:
    ''' 
    ''' https://cn.bing.com/academic/profile?id=24ca0003c2b5935f1335003ca712b889&encoded=0&v=paper_preview&mkt=zh-cn
    ''' </summary>
    Public Module ProfileResult

        <Extension>
        Private Function GetTarget(a As String) As NamedValue(Of String)
            Dim attrs = a.TagAttributes.ToArray
            Dim name$ = a _
                .GetValue _
                .StripHTMLTags

            Return New NamedValue(Of String) With {
                .Name = name,
                .Value = attrs.Item("href").Value,
                .Description = attrs.Item("h").Value
            }
        End Function

        Public Function GetProfile(url As String) As ArticleProfile
            Dim html$ = url.GET _
                .RemovesJavaScript _
                .RemovesCSSstyles _
                .RemovesImageLinks _
                .RemovesHtmlHead _
                .RemovesFooter

            Dim title$ = r.Match(html, "<li class=""aca_title"">.+?</li>", RegexICSng) _
                .Value _
                .GetBetween(">", "<")
            Dim authors = r.Match(html, "<div class=""aca_desc b_snippet"">.+?</div>", RegexICSng) _
                .Value _
                .Matches("<a\s.+?</a>") _
                .Select(AddressOf GetTarget) _
                .ToArray
            Dim abstract$ = r.Match(html, "<span title="".+?"">", RegexICSng) _
                .Value _
                .TagAttributes _
                .Where(Function(t) t.Name.TextEquals("title")) _
                .FirstOrDefault _
                .Value
            Dim contents = r.Matches(html, "<div class=""b_hPanel"">.+?</div>", RegexICSng) _
                .EachValue _
                .ToDictionary(Function(key)
                                  Return key.GetBetween("<span class=""aca_label"">", "</span>")
                              End Function)

            Dim time = contents("发表日期").GetBetween("<div>", "</div>")
            Dim journal = contents("期　　刊").GetBetween("<div>", "</div>").GetTarget
            Dim volumn = contents("卷　　号").GetBetween("<div>", "</div>")
            Dim issue = contents("期　　号").GetBetween("<div>", "</div>")
            Dim pageSpan = contents("页码范围").GetBetween("<div>", "</div>")
            Dim citeCount = contents("被 引 量").GetBetween("<div>", "</div>")
            Dim doi = contents("DOI").GetBetween("<div>", "</div>")
            Dim areas = contents("研究领域") _
                .Matches("<a .+?</a>") _
                .Select(AddressOf GetTarget) _
                .ToArray

            Dim source = Strings _
                .Split(html, "<div class=""aca_source"">") _
                .Last _
                .Matches("<a .+?</a>", RegexICSng) _
                .Where(Function(a)
                           Return InStr(a, "</span>", CompareMethod.Text) > 0
                       End Function) _
                .Select(AddressOf GetTarget) _
                .ToArray

            Dim pubDate As Date

            If time.IsPattern("\d+") Then
                pubDate = New Date(time, 1, 1)
            Else
                pubDate = Date.Parse(time)
            End If

            Return New ArticleProfile With {
                .Title = title,
                .Abstract = abstract,
                .Authors = authors,
                .DOI = doi,
                .Issue = issue,
                .Journal = journal,
                .Pages = pageSpan,
                .Volume = volumn,
                .PubDate = pubDate,
                .source = source,
                .Areas = areas
            }
        End Function
    End Module
End Namespace