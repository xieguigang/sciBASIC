Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON
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
        Private Function GetTarget(a As String) As Link
            Dim attrs = a.TagAttributes.ToArray
            Dim name$ = a _
                .GetValue _
                .StripHTMLTags

            Return New Link With {
                .title = name,
                .href = attrs.KeyItem("href").Value,
                .attr = attrs.KeyItem("h").Value
            }
        End Function

        <Extension>
        Public Function GetProfileID(article As ArticleProfile) As String
            Return article.URL.QueryStringParameters!id
        End Function

        Public Function GetProfile(url As String, Optional refer$ = Nothing) As ArticleProfile
            Dim html$ = url.GET(refer:=refer) _
                .RemovesCSSstyles _
                .RemovesImageLinks _
                .RemovesHtmlHead _
                .RemovesFooter
            Dim count As cites() = html _
                .GetBetween("""BarData""", "BarChart.render") _
                .GetStackValue(":", "}") _
                .LoadObject(Of cites())

            html = html.RemovesJavaScript

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
                .Select(Function(key)
                            Return New NamedValue(Of String)(key.GetBetween("<span class=""aca_label"">", "</span>"), key)
                        End Function) _
                .ToDictionary

            Dim time$
            Dim journal As Link
            Dim volumn$
            Dim issue$
            Dim pageSpan$
            Dim doi$
            Dim areas As Link()

            If contents.Keys.Any(Function(fieldName) Not ASCII.IsASCIIString(fieldName)) Then
                ' 中文的
                time = contents("发表日期").Value.GetBetween("<div>", "</div>")
                journal = contents("期　　刊").Value.GetBetween("<div>", "</div>").GetTarget
                volumn = contents("卷　　号").Value.GetBetween("<div>", "</div>")
                issue = contents("期　　号").Value.GetBetween("<div>", "</div>")
                pageSpan = contents("页码范围").Value.GetBetween("<div>", "</div>")
                doi = contents("DOI").Value.GetBetween("<div>", "</div>")
                areas = contents("研究领域") _
                    .Value _
                    .Matches("<a .+?</a>") _
                    .Select(AddressOf GetTarget) _
                    .ToArray
            Else
                With contents
                    ' English
                    time = !Year.Value.GetBetween("<div>", "</div>")
                    journal = !Journal.Value.GetBetween("<div>", "</div>").GetTarget
                    volumn = !Volume.Value.GetBetween("<div>", "</div>")
                    issue = !Issue.Value.GetBetween("<div>", "</div>")
                    pageSpan = !Pages.Value.GetBetween("<div>", "</div>")
                    doi = !DOI.Value.GetBetween("<div>", "</div>")
                    areas = !Keywords _
                        .Value _
                        .Matches(HtmlLink) _
                        .Select(AddressOf GetTarget) _
                        .ToArray

                    If abstract.StringEmpty Then

                        ' 因为split之后，<div被去掉后会留下class=...>，StripHTMLTags会因为标记不完整而无法去除
                        ' 所以需要在开头额外添加标签标记来完成html标签的删除
                        abstract = ("<null " & !Introduction.Value.SplitBy("<div").Last) _
                            .StripHTMLTags(stripBlank:=True)
                    End If
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
                .title = title,
                .abstract = abstract,
                .authors = authors.Where(Function(l) l.href <> "javascript:void(0);").ToArray,
                .DOI = doi,
                .issue = issue,
                .journal = journal,
                .pages = pageSpan,
                .volume = volumn,
                .PubDate = pubDate,
                .source = source.Where(Function(l) l.href <> "javascript:void(0);").ToArray,
                .keywords = areas.Where(Function(l) l.href <> "javascript:void(0);").ToArray,
                .cites = count,
                .URL = url
            }
        End Function
    End Module
End Namespace