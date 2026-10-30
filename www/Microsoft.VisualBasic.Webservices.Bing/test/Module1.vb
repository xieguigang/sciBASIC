#Region "Microsoft.VisualBasic::50c6ea33bfbbc9a7c61974bf99ad5a2f, www\Microsoft.VisualBasic.Webservices.Bing\test\Module1.vb"

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

    ' Module Module1
    ' 
    '     Function: AuditFile
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Data.KnowledgeBase.Web.Bing.Academic
Imports Microsoft.VisualBasic.Data.NLP.Html
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' 重构之后的必应学术搜索模块的 demo 测试
''' </summary>
''' <remarks>
''' 测试以 <c>scibasic</c> 作为检索关键词，使用 Trinity 所提供的通用 HTML DOM 解析库
''' 解析必应学术的搜索结果页面，并且把解析出来的结构化结果以带缩进的 JSON 字符串打印出来。
'''
''' 测试过程之中的关键变量与断言通过 <see cref="DebugAudit"/> 记录，
''' 结束的时候会输出一份文本报告并且落盘一份 JSON 报告。
''' </remarks>
Module Module1

    ''' <summary>
    ''' demo 测试所使用的检索关键词
    ''' </summary>
    Const keyword As String = "scibasic"

    Private Function AuditFile() As String
        Return Path.Combine(AppContext.BaseDirectory, "bing_academic_audit.json")
    End Function

    ''' <summary>
    ''' 生成带缩进的、可以直接阅读的 JSON 文本
    ''' </summary>
    ''' <remarks>
    ''' 框架内置的 <c>GetJson(indent:=True)</c> 这一份输出用于持久化是可以的，
    ''' 但是框架的序列化器在当前版本之中会忽略掉缩进开关并且把非 ASCII 字符转义为
    ''' ``\uXXXX`` 的形式，不利于阅读；所以这里在打印的时候直接使用
    ''' <see cref="JsonSerializer"/> 重新生成一份带缩进的、保留中文的输出。
    ''' </remarks>
    Private Function PrettyJson(page As AcademicSearchPage) As String
        Dim options As New JsonSerializerOptions With {
            .WriteIndented = True,
            .Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }

        Return JsonSerializer.Serialize(page, options)
    End Function

    Sub Main()
        Console.OutputEncoding = Encoding.UTF8

        ' 测试开始之前清空上一轮的审计记录
        Call DebugAudit.Reset()

        Console.WriteLine($"[bing academic] keyword = {keyword}")
        Console.WriteLine($"[bing academic] html dom library = {GetType(HtmlDocument).Assembly.GetName.Name}")
        Console.WriteLine()

        Call DebugAudit.Mark("keyword", keyword)
        Call DebugAudit.Mark("dom.assembly", GetType(HtmlDocument).Assembly.GetName.Name)

        Dim page As AcademicSearchPage = Nothing
        Dim errorMsg As String = Nothing

        Using DebugAudit.Watch("bing.academic.search", logOnExit:=True)
            Try
                page = AcademicSearch.SearchPage(keyword)
            Catch ex As Exception
                errorMsg = ex.Message
                Call App.LogException(ex)
            End Try
        End Using

        Call DebugAudit.Test(errorMsg Is Nothing, $"search '{keyword}' should not throw any exception")

        If page Is Nothing Then
            Console.WriteLine($"[bing academic] FAILED: {errorMsg}")

            Call DebugAudit.Test(False, "search page should not be nothing")
            Call Console.WriteLine(DebugAudit.Dump())
            Call DebugAudit.SaveReport(AuditFile())

            Environment.Exit(1)
        End If

        Dim results As AcademicResult() = If(page.results, New AcademicResult() {})

        Call DebugAudit.Snapshot(
            DebugAudit.V(keyword),
            DebugAudit.V(page.url),
            DebugAudit.V(page.total),
            DebugAudit.V(results.Length))

        Call DebugAudit.Mark("page.url", page.url)
        Call DebugAudit.Mark("page.total", page.total)
        Call DebugAudit.Mark("page.parsed", results.Length)

        ' ---------- 断言：解析结果的有效性 ----------
        Call DebugAudit.Test(results.Length > 0, "should parse at least one academic result")
        Call DebugAudit.Test(page.total > 0 OrElse results.Length > 0, "the result counter should be available")

        If results.Length > 0 Then
            Dim first As AcademicResult = results(0)

            Call DebugAudit.Mark("first.title", first.title)
            Call DebugAudit.Mark("first.url", first.url)
            Call DebugAudit.Mark("first.abstract", first.abstract)

            Call DebugAudit.Test(first.title.Length > 0, "the title of the first result should not be empty")
            Call DebugAudit.Test(first.url.StartsWith("http"), "the url of the first result should be an absolute http(s) link")
            Call DebugAudit.Test(first.rank = 1, "the rank of the first result should be 1")
            Call DebugAudit.Test(
                results.All(Function(r) r.title.Length > 0),
                "all parsed results should carry a title")
            Call DebugAudit.Test(
                results.All(Function(r) r.url.StartsWith("http")),
                "all parsed urls should be absolute links")
            Call DebugAudit.Test(
                results.All(Function(r) Not r.url.ToLower().Contains("javascript:")),
                "no pseudo javascript link should be kept")
            Call DebugAudit.Test(
                results.Select(Function(r) r.url).Distinct().Count() = results.Length,
                "the parsed results should be de-duplicated by url")

            Dim withAbstract As Integer = results.Count(Function(r) r.abstract.Length > 0)
            Dim withAuthors As Integer = results.Count(Function(r) r.authors.Length > 0)
            Dim withJournal As Integer = results.Count(Function(r) r.journal.Length > 0)
            Dim withCites As Integer = results.Count(Function(r) r.cites > 0)
            Dim withKeywords As Integer = results.Count(Function(r) r.keywords.Length > 0)
            Dim academic As Integer = results.Count(Function(r) r.isAcademic)

            Call DebugAudit.Mark("stats.withAbstract", withAbstract)
            Call DebugAudit.Mark("stats.withAuthors", withAuthors)
            Call DebugAudit.Mark("stats.withJournal", withJournal)
            Call DebugAudit.Mark("stats.withCites", withCites)
            Call DebugAudit.Mark("stats.withKeywords", withKeywords)
            Call DebugAudit.Mark("stats.academicProfile", academic)

            Call DebugAudit.Test(withAbstract > 0, "at least one result should carry an abstract text")

            Console.WriteLine($"[bing academic] {results.Length} result(s) parsed, total = {page.total}")
            Console.WriteLine($"[bing academic] abstract={withAbstract}, authors={withAuthors}, journal={withJournal}, cites={withCites}, keywords={withKeywords}, profile={academic}")
            Console.WriteLine()
        End If

        ' ---------- 结构化 JSON 输出 ----------
        Console.WriteLine("==================== structured json ====================")
        Console.WriteLine(PrettyJson(page))
        Console.WriteLine("=========================================================")
        Console.WriteLine()

        ' ---------- 审计报告 ----------
        Console.WriteLine("==================== debug audit ====================")
        Console.WriteLine(DebugAudit.Dump())
        Console.WriteLine("=====================================================")

        Call DebugAudit.SaveReport(AuditFile())

        Console.WriteLine($"[bing academic] audit report saved to: {AuditFile()}")
        Console.WriteLine($"[bing academic] failure(s) = {DebugAudit.FailureCount}")

        Environment.Exit(If(DebugAudit.FailureCount > 0, 1, 0))
    End Sub

End Module
