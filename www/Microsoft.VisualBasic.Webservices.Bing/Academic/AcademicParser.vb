#Region "Microsoft.VisualBasic::AcademicParser, www\Microsoft.VisualBasic.Webservices.Bing\Academic\AcademicParser.vb"

    ' Author:
    '
    '       xieguigang (xie.guigang@live.com)
    '
    ' Copyright (c) 2018 GPL3 Licensed
    '
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

    ' Module AcademicParser
    '
    '     Function: ParseFile, ParsePage
    '
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Data.NLP.Html

Namespace Bing.Academic

    ''' <summary>
    ''' 必应学术搜索结果页面的解析器：将页面 DOM 映射为结构化的 <see cref="AcademicResult"/>
    ''' </summary>
    ''' <remarks>
    ''' 这个模块是整个必应学术模块之中唯一与页面结构耦合的地方，所有的抽取逻辑都基于
    ''' <see cref="HtmlDocument"/> 的选择器来完成，并且每一个字段都带有兜底(fallback)策略：
    ''' 首选按 css class 定位，定位不到的时候再按结构特征定位，仍然定位不到的时候返回空值。
    '''
    ''' 这样处理的原因在于必应的页面结构会经常的发生变化，并且同一个页面之上还会同时存在
    ''' 学术文献型与网页型这两种结构完全不同的结果条目。
    ''' </remarks>
    Public Module AcademicParser

        ''' <summary>
        ''' 用于把页面之上的相对链接补全为绝对链接的基准地址
        ''' </summary>
        Public Const BingBaseUrl As String = "https://cn.bing.com/"

        ''' <summary>
        ''' 结果与作者、期刊之间所使用的分隔符(&lt;code&gt;&amp;#183;&lt;/code&gt; 中点)
        ''' </summary>
        Private ReadOnly dot As String = ChrW(&HB7)

        ''' <summary>
        ''' 解析一页必应学术的搜索结果
        ''' </summary>
        ''' <param name="html">搜索结果页面的原始 HTML 文本</param>
        ''' <param name="url">这一页结果所对应的搜索地址，用于结果溯源</param>
        ''' <param name="term">检索所使用的关键词</param>
        ''' <returns>解析结果；解析不到任何条目的时候 <see cref="AcademicSearchPage.results"/> 为空数组</returns>
        Public Function ParsePage(html As String, Optional url As String = Nothing, Optional term As String = Nothing) As AcademicSearchPage
            Dim page As New AcademicSearchPage With {
                .term = term,
                .url = url
            }

            If String.IsNullOrWhiteSpace(html) Then
                Return page
            End If

            Dim doc As HtmlDocument = HtmlDocument.Parse(html)

            page.total = GetTotalCount(doc)

            Dim items As HtmlNode() = GetResultItems(doc)
            Dim results As New List(Of AcademicResult)()
            Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim rank As Integer = 0

            For Each li As HtmlNode In items
                Dim item As AcademicResult = ParseItem(li, url, term)

                If item Is Nothing Then
                    Continue For
                End If
                If item.url.Length > 0 AndAlso Not seen.Add(item.url) Then
                    ' 页面上可能会存在重复的链接
                    Continue For
                End If

                rank += 1
                item.rank = rank

                Call results.Add(item)
            Next

            page.results = results.ToArray()

            Return page
        End Function

        ''' <summary>
        ''' 解析保存在本地磁盘之上的搜索结果页面
        ''' </summary>
        Public Function ParseFile(path As String, Optional term As String = Nothing) As AcademicSearchPage
            Return ParsePage(File.ReadAllText(path, Encoding.UTF8), "file:///" & path, term)
        End Function

        ''' <summary>
        ''' 定位页面之上的结果列表容器，并且取出其中所有的结果条目
        ''' </summary>
        Private Function GetResultItems(doc As HtmlDocument) As HtmlNode()
            ' 常规的路径：结果列表位于 <ol id="b_results"> 之下
            Dim container As HtmlNode = doc.GetElementById("b_results")

            If container IsNot Nothing Then
                Dim items As HtmlNode() = container.QuerySelectorAll("li.aca_algo")

                If items.Length > 0 Then
                    Return items
                End If
            End If

            ' 兜底：在整个文档之中查找学术结果条目
            Dim all As HtmlNode() = doc.QuerySelectorAll("li.aca_algo")

            If all.Length > 0 Then
                Return all
            End If

            ' 最后兜底：页面结构可能已经改版为通用的网页搜索结果
            Return doc.QuerySelectorAll("li.b_algo")
        End Function

        ''' <summary>
        ''' 读取搜索结果的总数
        ''' </summary>
        Private Function GetTotalCount(doc As HtmlDocument) As Integer
            Dim node As HtmlNode = doc.QuerySelector("li.aca_algo_count")

            If node Is Nothing Then
                node = doc.QuerySelector("span.sb_count")
            End If
            If node Is Nothing Then
                node = doc.QuerySelector("div.sb_count")
            End If
            If node Is Nothing Then
                ' 最后兜底：在整个文档之中查找带有计数类名的元素
                node = doc.Root _
                    .Descendants() _
                    .FirstOrDefault(Function(n)
                                        Return n.HasClass("aca_algo_count") OrElse n.HasClass("sb_count")
                                    End Function)
            End If
            If node Is Nothing Then
                Return -1
            End If

            Dim m As Match = Regex.Match(node.InnerText, "\d[\d,]*")

            If Not m.Success Then
                Return -1
            End If

            Dim count As Integer

            If Integer.TryParse(m.Value.Replace(",", ""), count) Then
                Return count
            End If

            Return -1
        End Function

        ''' <summary>
        ''' 解析一个结果条目
        ''' </summary>
        Private Function ParseItem(li As HtmlNode, refer As String, term As String) As AcademicResult
            Dim link As HtmlNode = FindTitleLink(li)

            If link Is Nothing Then
                Return Nothing
            End If

            Dim title As String = link.InnerText.Trim()
            Dim href As String = HtmlText.AbsoluteUrl(link.Attribute("href"), BingBaseUrl)

            If title.Length = 0 OrElse Not HtmlText.IsSafeUrl(href) Then
                Return Nothing
            End If

            Dim item As New AcademicResult With {
                .title = title,
                .url = href,
                .term = term,
                .refer = refer,
                .abstract = GetAbstract(li),
                .authors = GetAuthors(li),
                .keywords = GetKeywords(li),
                .pdfUrl = GetPdfUrl(li)
            }

            Dim display As HtmlNode = li.QuerySelector("div.caption_display_url")

            If display IsNot Nothing Then
                item.displayUrl = display.InnerText
            End If

            Call ParseVenue(li, item)
            Call DetectProfile(item)

            Return item
        End Function

        ''' <summary>
        ''' 查找结果条目的标题链接：优先 ``h2 &gt; a``，兜底为条目之内的第一个可用链接
        ''' </summary>
        Private Function FindTitleLink(li As HtmlNode) As HtmlNode
            Dim links As HtmlNode() = li.QuerySelectorAll("h2 a")

            For Each a As HtmlNode In links
                If HtmlText.IsSafeUrl(HtmlText.AbsoluteUrl(a.Attribute("href"), BingBaseUrl)) Then
                    Return a
                End If
            Next

            For Each a As HtmlNode In li.QuerySelectorAll("a[href]")
                Dim href As String = HtmlText.AbsoluteUrl(a.Attribute("href"), BingBaseUrl)

                If HtmlText.IsSafeUrl(href) AndAlso a.InnerText.Trim().Length > 0 Then
                    ' 关键词标签与作者链接不是标题链接
                    If a.HasClass("caption_keyword_badge") Then
                        Continue For
                    End If

                    Return a
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' 抽取摘要文本
        ''' </summary>
        Private Function GetAbstract(li As HtmlNode) As String
            Dim node As HtmlNode = li.QuerySelector("div.caption_abstract")

            If node IsNot Nothing Then
                Return node.InnerText
            End If

            ' 兜底：条目之内的第一个段落文本
            node = li.QuerySelector("p")

            If node IsNot Nothing Then
                Return node.InnerText
            End If

            Return ""
        End Function

        ''' <summary>
        ''' 抽取作者列表
        ''' </summary>
        Private Function GetAuthors(li As HtmlNode) As String()
            Dim block As HtmlNode = li.QuerySelector("div.caption_author")

            If block Is Nothing Then
                Return New String() {}
            End If

            Dim names As New List(Of String)()

            For Each a As HtmlNode In block.QuerySelectorAll("a")
                Dim name As String = a.InnerText.Trim()

                If name.Length > 0 Then
                    Call names.Add(name)
                End If
            Next

            If names.Count > 0 Then
                Return names.ToArray()
            End If

            ' 兜底：作者区块之内的文本使用中点分隔
            Return HtmlText.SplitBy(block.InnerText, dot, "|", "、")
        End Function

        ''' <summary>
        ''' 抽取关键词标签
        ''' </summary>
        Private Function GetKeywords(li As HtmlNode) As String()
            Dim block As HtmlNode = li.QuerySelector("div.caption_field_badge")

            If block Is Nothing Then
                Return New String() {}
            End If

            Dim words As New List(Of String)()

            ' 关键词标签的文本保存在 aria-label 属性之中
            For Each a As HtmlNode In block.QuerySelectorAll("a[aria-label]")
                Dim label As String = HtmlEntity.Decode(a.Attribute("aria-label")).Trim()

                If label.Length > 0 Then
                    Call words.Add(label)
                End If
            Next

            If words.Count = 0 Then
                For Each span As HtmlNode In block.QuerySelectorAll("span")
                    Dim text As String = span.InnerText.Trim()

                    If text.Length > 0 Then
                        Call words.Add(text)
                    End If
                Next
            End If

            Return words.Distinct().ToArray()
        End Function

        ''' <summary>
        ''' 抽取全文/PDF 链接
        ''' </summary>
        Private Function GetPdfUrl(li As HtmlNode) As String
            Dim a As HtmlNode = li.QuerySelector("a.caption_pdf_button")

            If a Is Nothing Then
                a = li.QuerySelector("div.caption_pdf_row a")
            End If
            If a Is Nothing Then
                Return ""
            End If

            Return HtmlText.AbsoluteUrl(a.Attribute("href"), BingBaseUrl)
        End Function

        ''' <summary>
        ''' 解析发表信息区块：发表年份、期刊名称以及被引次数
        ''' </summary>
        ''' <remarks>
        ''' 这一块的文本形如 <c>2006 · Journal of Morphology |被引数：95</c>，
        ''' 也可能只有年份 <c>2012</c>，或者只有被引次数。
        ''' </remarks>
        Private Sub ParseVenue(li As HtmlNode, item As AcademicResult)
            Dim venue As HtmlNode = li.QuerySelector("div.caption_venue")

            If venue Is Nothing Then
                Return
            End If

            Dim text As String = venue.InnerText

            If text.Length = 0 Then
                Return
            End If

            ' 被引数：95 / Cited by 95
            Dim citedAt As Integer = text.IndexOf("被引数", StringComparison.Ordinal)

            If citedAt < 0 Then
                citedAt = text.IndexOf("Cited by", StringComparison.OrdinalIgnoreCase)
            End If

            If citedAt >= 0 Then
                Dim citedPart As String = text.Substring(citedAt)

                text = text.Substring(0, citedAt)

                Dim num As Match = Regex.Match(citedPart, "\d[\d,]*")

                If num.Success Then
                    Dim cites As Integer

                    If Integer.TryParse(num.Value.Replace(",", ""), cites) Then
                        item.cites = cites
                    End If
                End If
            End If

            ' 发表年份
            Dim y As Match = Regex.Match(text, "(?:19|20)\d{2}")

            If y.Success Then
                Dim year As Integer

                If Integer.TryParse(y.Value, year) Then
                    item.year = year
                End If
            End If

            ' 期刊名称：年份与被引数之外的那一段文本
            For Each part As String In HtmlText.SplitBy(text, dot, "|", " - ")
                If part.Length = 0 Then
                    Continue For
                End If
                If item.year > 0 AndAlso part = item.year.ToString() Then
                    Continue For
                End If
                If part = "被引数" OrElse Regex.IsMatch(part, "^\d+$") Then
                    Continue For
                End If

                item.journal = part
                Exit For
            Next
        End Sub

        ''' <summary>
        ''' 判断这个结果是否为必应学术的文献结果，并且解析出文献编号
        ''' </summary>
        Private Sub DetectProfile(item As AcademicResult)
            If item.url.Length = 0 Then
                Return
            End If
            If item.url.IndexOf("/academic/profile", StringComparison.OrdinalIgnoreCase) < 0 Then
                Return
            End If

            item.isAcademic = True
            item.profileUrl = item.url

            Dim m As Match = Regex.Match(item.url, "[?&]id=([^&#]+)")

            If m.Success Then
                item.profileID = m.Groups(1).Value
            End If
        End Sub
    End Module

End Namespace
