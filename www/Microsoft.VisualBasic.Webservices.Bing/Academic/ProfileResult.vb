Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.HtmlParser
Imports r = System.Text.RegularExpressions.Regex

Namespace Academic

    ''' <summary>
    ''' Example as:
    ''' 
    ''' https://cn.bing.com/academic/profile?id=24ca0003c2b5935f1335003ca712b889&amp;encoded=0&amp;v=paper_preview&amp;mkt=zh-cn
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
                .Value = attrs.KeyItem("href").Value,
                .Description = attrs.KeyItem("h").Value
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
                .Matches(HtmlLink) _
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

            Dim time$
            Dim journal As NamedValue(Of String)
            Dim volumn$
            Dim issue$
            Dim pageSpan$
            Dim citeCount$
            Dim doi$
            Dim areas As NamedValue(Of String)()

            If contents.Keys.Any(Function(fieldName) Not ASCII.IsASCIIString(fieldName)) Then
                ' 中文的
                time = contents("发表日期").GetBetween("<div>", "</div>")
                journal = contents("期　　刊").GetBetween("<div>", "</div>").GetTarget
                volumn = contents("卷　　号").GetBetween("<div>", "</div>")
                issue = contents("期　　号").GetBetween("<div>", "</div>")
                pageSpan = contents("页码范围").GetBetween("<div>", "</div>")
                citeCount = contents("被 引 量").GetBetween("<div>", "</div>")
                doi = contents("DOI").GetBetween("<div>", "</div>")
                areas = contents("研究领域") _
                    .Matches("<a .+?</a>") _
                    .Select(AddressOf GetTarget) _
                    .ToArray
            Else
                With contents
                    ' English
                    time = !Year.GetBetween("<div>", "</div>")
                    journal = !Journal.GetBetween("<div>", "</div>").GetTarget
                    volumn = !Volume.GetBetween("<div>", "</div>")
                    issue = !Issue.GetBetween("<div>", "</div>")
                    pageSpan = !Pages.GetBetween("<div>", "</div>")
                    citeCount = contents("Cited by").GetBetween("<div>", "</div>")
                    doi = !DOI.GetBetween("<div>", "</div>")
                    areas = !Keywords _
                        .Matches(HtmlLink) _
                        .Select(AddressOf GetTarget) _
                        .ToArray
                End With
            End If

            Dim source = Strings _
                .Split(html, "<div class=""aca_source"">") _
                .Last _
                .Matches(HtmlLink, RegexICSng) _
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