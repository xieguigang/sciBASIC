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
                .Select(Function(a)
                            Dim attrs = a.TagAttributes.ToArray
                            Dim name$ = a.GetValue

                            Return New NamedValue(Of String) With {
                                .Name = name,
                                .Value = attrs.Item("href").Value,
                                .Description = attrs.Item("h").Value
                            }
                        End Function) _
                .ToArray
            Dim abstract$ = r.Match(html, "<span title="".+?"">", RegexICSng) _
                .Value _
                .TagAttributes _
                .Where(Function(t) t.Name.TextEquals("title")) _
                .FirstOrDefault _
                .Value


            Return New ArticleProfile With {
                .Title = title,
                .Abstract = abstract,
                .Authors = authors
            }
        End Function
    End Module
End Namespace