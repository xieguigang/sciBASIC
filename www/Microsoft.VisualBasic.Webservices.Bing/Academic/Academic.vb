#Region "Microsoft.VisualBasic::062d268198a4d9fce05a60cb5d6416ca, www\Microsoft.VisualBasic.Webservices.Bing\MicrosoftBing\Academic\Academic.vb"

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

    '   Total Lines: 102
    '    Code Lines: 71 (69.61%)
    ' Comment Lines: 14 (13.73%)
    '    - Xml Docs: 92.86%
    ' 
    '   Blank Lines: 17 (16.67%)
    '     File Size: 3.96 KB


    '     Module AcademicSearch
    ' 
    '         Function: BuildUrl, DownloadPage, GetDetails, Query, Search
    '                   SearchPage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser
Imports r = System.Text.RegularExpressions.Regex

Namespace Bing.Academic

    ''' <summary>
    ''' Bing Academic web API for VisualBasic
    ''' </summary>
    ''' <remarks>
    ''' 页面的解析工作已经全部交给 <see cref="AcademicParser"/> 基于
    ''' <c>Microsoft.VisualBasic.Data.NLP.Html</c> 所提供的通用 HTML DOM 来完成，
    ''' 这里只负责网络请求、分页与结果的类型封装。
    ''' </remarks>
    <Package("Bing.Academic",
         Url:="http://cn.bing.com/academic/?FORM=Z9LH2",
         Description:="",
         Category:=APICategories.UtilityTools,
         Publisher:="")>
    Public Module AcademicSearch

        ' https://cn.bing.com/academic/search?q=Danio+rerio&go=%E6%90%9C%E7%B4%A2&qs=ds&form=QBRE

        Const refer$ = "https://cn.bing.com/academic/?FORM=Z9LH2"

        ''' <summary>
        ''' 生成必应学术的搜索地址
        ''' </summary>
        ''' <param name="term">检索关键词</param>
        ''' <param name="first">分页的起始结果编号(从 1 开始)；小于等于 0 的时候不分页</param>
        Public Function BuildUrl(term As String, Optional first As Integer = -1) As String
            If first > 0 Then
                Return $"https://cn.bing.com/academic/search?q={term.UrlEncode}&first={first}&go=Search&qs=ds&form=QBRE"
            Else
                Return $"https://cn.bing.com/academic/search?q={term.UrlEncode}&go=Search&qs=ds&form=QBRE"
            End If
        End Function

        ''' <summary>
        ''' Run academic search from portal: https://cn.bing.com/academic/?FORM=HDRSC4
        ''' </summary>
        ''' <param name="term">检索关键词</param>
        ''' <returns>这一页搜索结果之上解析出来的所有条目</returns>
        Public Function Search(term As String) As AcademicResult()
            Return SearchPage(term).results
        End Function

        ''' <summary>
        ''' 执行搜索并且返回整个结果页面(包含结果总数信息)
        ''' </summary>
        ''' <param name="term">检索关键词</param>
        ''' <param name="first">分页的起始结果编号，从 1 开始；小于等于 0 的时候取第一页</param>
        Public Function SearchPage(term As String, Optional first As Integer = -1) As AcademicSearchPage
            Return DownloadPage(BuildUrl(term, first), term)
        End Function

        ''' <summary>
        ''' 下载并且解析一个必应学术的搜索结果页面
        ''' </summary>
        ''' <param name="url">搜索结果页面地址</param>
        ''' <param name="term">检索关键词，仅用于结果的溯源标记</param>
        Public Function DownloadPage(url As String, Optional term As String = Nothing) As AcademicSearchPage
            Dim html$ = url.GET(headers:=New Dictionary(Of String, String) From {{NameOf(refer), refer}})

            Return AcademicParser.ParsePage(html, url, term)
        End Function

        ''' <summary>
        ''' 分页的遍历一个关键词的所有的搜索结果
        ''' </summary>
        ''' <param name="term">检索关键词</param>
        ''' <param name="pages">需要遍历的页数</param>
        ''' <returns>每一页的搜索地址以及这一页之上解析出来的结果列表</returns>
        Public Iterator Function Query(term As String, Optional pages% = 10) As IEnumerable(Of (refer$, list As AcademicResult()))
            Dim result As AcademicResult()

            For i As Integer = 0 To pages
                Dim first As Integer = i * 10 + 1
                Dim url$ = BuildUrl(term, first)

                Try
                    result = DownloadPage(url, term).results
                Catch ex As Exception
                    result = New AcademicResult() {}
                    ex = New Exception(url, ex)
                    Call App.LogException(ex)
                End Try

                Yield (url, result)
            Next
        End Function

        ''' <summary>
        ''' Aquire details information about an article result.
        ''' </summary>
        ''' <param name="info">搜索结果条目</param>
        ''' <param name="refer">引用页地址</param>
        ''' <returns>
        ''' 只有必应学术的文献型结果(即 <see cref="AcademicResult.isAcademic"/> 为 <c>True</c> 的条目)
        ''' 才可以拉取到详情页，其他情况返回 <c>Nothing</c>。
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetDetails(info As AcademicResult, Optional refer$ = Nothing) As ArticleProfile
            If info Is Nothing OrElse Not info.isAcademic OrElse String.IsNullOrEmpty(info.profileUrl) Then
                Return Nothing
            End If

            Return ProfileResult.GetProfile(info.profileUrl, refer)
        End Function

        ''' <summary>
        ''' 将结构化的搜索结果转换为 <see cref="NamedValue(Of String)"/> 形式，便于做向后兼容的处理
        ''' </summary>
        <Extension>
        Public Function AsNamedValues(results As IEnumerable(Of AcademicResult)) As NamedValue(Of String)()
            If results Is Nothing Then
                Return New NamedValue(Of String)() {}
            End If

            Return results _
                .Select(Function(r)
                             Return New NamedValue(Of String) With {
                                 .Name = r.title,
                                 .Value = r.url,
                                 .Description = r.abstract
                             }
                         End Function) _
                .ToArray()
        End Function
    End Module
End Namespace
