#Region "Microsoft.VisualBasic::c26eb2f049763576926aa5a969e626fd, www\Microsoft.VisualBasic.Webservices.Bing\MicrosoftBing\Academic\ProfileResult.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 160
    '    Code Lines: 134 (83.75%)
    ' Comment Lines: 9 (5.62%)
    '    - Xml Docs: 44.44%
    ' 
    '   Blank Lines: 17 (10.62%)
    '     File Size: 6.71 KB


    '     Module ProfileResult
    ' 
    '         Function: GetProfile, GetProfileID, GetTarget
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser
Imports r = System.Text.RegularExpressions.Regex

Namespace Bing.Academic

    ''' <summary>
    ''' Example as:
    ''' 
    ''' https://cn.bing.com/academic/profile?id=24ca0003c2b5935f1335003ca712b889&amp;encoded=0&amp;v=paper_preview&amp;mkt=zh-cn
    ''' </summary>
    Public Module ProfileResult

        ReadOnly htmlLink As String = HtmlStrips.Regexp("a")

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

        ''' <summary>
        ''' 从一个详情信息表之中安全的读取一个字段：字段不存在的时候返回空字符串而不是抛出异常
        ''' </summary>
        Private Function Field(contents As Dictionary(Of String, NamedValue(Of String)), name As String) As String
            Dim value As NamedValue(Of String) = Nothing

            If contents Is Nothing OrElse String.IsNullOrEmpty(name) Then
                Return ""
            End If
            If Not contents.TryGetValue(name, value) Then
                Return ""
            End If

            Return If(value.Value, "")
        End Function

        ''' <summary>
        ''' 拉取并且解析必应学术的文献详情页
        ''' </summary>
        ''' <param name="url">必应学术的详情页地址，形如 https://cn.bing.com/academic/profile?id=...</param>
        ''' <param name="refer">引用页地址</param>
        ''' <returns>
        ''' 解析出来的文献详细信息；如果给定的地址不是一个必应学术的详情页地址，
        ''' 或者页面之上没有解析到任何有效的信息，则返回 <c>Nothing</c>。
        ''' </returns>
        Public Function GetProfile(url As String, Optional refer$ = Nothing) As ArticleProfile
            If String.IsNullOrWhiteSpace(url) Then
                Return Nothing
            End If
            If url.IndexOf("/academic/profile", StringComparison.OrdinalIgnoreCase) < 0 Then
                ' 只有必应学术的详情页才可以被这个函数解析，
                ' 搜索结果页面之上也存在直接指向出版商网站的网页型结果
                Return Nothing
            End If

            Dim html$ = url.GET(refer:=refer) _
                .RemovesCSSstyles _
                .RemovesImageLinks _
                .RemovesHtmlHead _
                .RemovesFooter
            Dim count As cites() = html _
                .GetBetween("""BarData""", "BarChart.render") _
                .GetStackValue(":", "}") _
                .LoadJSON(Of cites())

            html = html.RemovesJavaScript

            Dim title$ = r.Match(html, "<li class=""aca_title"">.+?</li>", RegexICSng) _
                .Value _
                .GetBetween(">", "<")
            Dim authors = r.Match(html, "<div class=""aca_desc b_snippet"">.+?</div>", RegexICSng) _
                .Value _
                .Matches(htmlLink) _
                .Select(AddressOf GetTarget) _
                .ToArray
            Dim abstractAttrs = r.Match(html, "<span title="".+?"">", RegexICSng) _
                .Value _
                .TagAttributes _
                .Where(Function(t) t.Name IsNot Nothing AndAlso t.Name.TextEquals("title")) _
                .ToArray
            Dim abstract As String = If(abstractAttrs.Length > 0, abstractAttrs(0).Value, "")
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

            If contents.Keys.Any(Function(fieldName) fieldName IsNot Nothing AndAlso Not ASCII.IsASCIIString(fieldName)) Then
                ' 中文的
                time = Field(contents, "发表日期").GetBetween("<div>", "</div>")
                journal = Field(contents, "期　　刊").GetBetween("<div>", "</div>").GetTarget
                volumn = Field(contents, "卷　　号").GetBetween("<div>", "</div>")
                issue = Field(contents, "期　　号").GetBetween("<div>", "</div>")
                pageSpan = Field(contents, "页码范围").GetBetween("<div>", "</div>")
                doi = Field(contents, "DOI").GetBetween("<div>", "</div>")
                areas = Field(contents, "研究领域") _
                    .Matches("<a .+?</a>") _
                    .Select(AddressOf GetTarget) _
                    .ToArray
            Else
                ' English
                time = Field(contents, "Year").GetBetween("<div>", "</div>")
                journal = Field(contents, "Journal").GetBetween("<div>", "</div>").GetTarget
                volumn = Field(contents, "Volume").GetBetween("<div>", "</div>")
                issue = Field(contents, "Issue").GetBetween("<div>", "</div>")
                pageSpan = Field(contents, "Pages").GetBetween("<div>", "</div>")
                doi = Field(contents, "DOI").GetBetween("<div>", "</div>")
                areas = Field(contents, "Keywords") _
                    .Matches(htmlLink) _
                    .Select(AddressOf GetTarget) _
                    .ToArray

                If abstract.StringEmpty Then

                    ' 因为split之后，<div被去掉后会留下class=...>，StripHTMLTags会因为标记不完整而无法去除
                    ' 所以需要在开头额外添加标签标记来完成html标签的删除
                    abstract = ("<null " & Field(contents, "Introduction").SplitBy("<div").Last) _
                        .StripHTMLTags(stripBlank:=True)
                End If
            End If

            Dim source = Strings _
                .Split(html, "<div class=""aca_source"">") _
                .Last _
                .Matches(htmlLink, RegexICSng) _
                .Where(Function(a)
                           Return InStr(a, "</span>", CompareMethod.Text) > 0
                       End Function) _
                .Select(AddressOf GetTarget) _
                .ToArray

            Dim pubDate As Date = Nothing

            If Not String.IsNullOrWhiteSpace(time) Then
                If time.IsPattern("\d+") Then
                    Dim y As Integer

                    If Integer.TryParse(time, y) AndAlso y >= 1 AndAlso y <= 9999 Then
                        pubDate = New Date(y, 1, 1)
                    End If
                ElseIf Not Date.TryParse(time, pubDate) Then
                    ' 页面之上的日期格式可能是任意的，解析不出来的时候不应该中断整个解析过程
                    pubDate = Nothing
                End If
            End If

            Return New ArticleProfile With {
                .title = title,
                .abstract = abstract _
                    .SplitParagraph(len:=200) _
                    .JoinBy(ASCII.LF),
                .authors = authors _
                    .Where(Function(l)
                               Return l.href <> "javascript:void(0);"
                           End Function) _
                    .ToArray,
                .doi = doi,
                .issue = issue,
                .journal = journal,
                .pages = pageSpan,
                .volume = volumn,
                .pubDate = pubDate,
                .source = source.Where(Function(l) l.href <> "javascript:void(0);").ToArray,
                .Keywords = areas.Where(Function(l) l.href <> "javascript:void(0);").ToArray,
                .cites = count,
                .url = url
            }
        End Function
    End Module
End Namespace
