#Region "Microsoft.VisualBasic::AcademicResult, www\Microsoft.VisualBasic.Webservices.Bing\Academic\AcademicResult.vb"

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

    ' Class AcademicResult
    '
    '     Properties: abstract, authors, cites, displayUrl, isAcademic
    '                 journal, keywords, pdfUrl, profileID, profileUrl
    '                 rank, refer, term, title, url
    '                 year
    '
    '     Function: ToString
    '
    ' Class AcademicSearchPage
    '
    '     Properties: results, term, total, url
    '
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace Bing.Academic

    ''' <summary>
    ''' 必应学术搜索结果之中的一个条目
    ''' </summary>
    ''' <remarks>
    ''' 必应学术的搜索结果页面之上存在两种形态的结果：
    '''
    ''' <list type="number">
    ''' <item>学术文献型：链接指向必应学术的详情页(``/academic/profile?id=...``)，
    ''' 结果项之中带有作者、发表年份、期刊与被引数等信息</item>
    ''' <item>网页型：链接直接指向出版商或者原始网页，结果项之中只有展示链接与摘要片段</item>
    ''' </list>
    '''
    ''' 这两种形态都会被解析为这个数据模型；缺失的字段为空值或者 0，不会抛出异常。
    ''' </remarks>
    Public Class AcademicResult

        ''' <summary>
        ''' 在当前页面之中的排名，从 1 开始
        ''' </summary>
        Public Property rank As Integer

        ''' <summary>
        ''' 检索所使用的关键词
        ''' </summary>
        Public Property term As String

        ''' <summary>
        ''' 标题文本(已经做过 HTML 实体解码与空白归一化)
        ''' </summary>
        Public Property title As String

        ''' <summary>
        ''' 结果的目标链接地址(绝对地址)
        ''' </summary>
        Public Property url As String

        ''' <summary>
        ''' 页面上展示出来的链接地址文本
        ''' </summary>
        Public Property displayUrl As String

        ''' <summary>
        ''' 摘要文本
        ''' </summary>
        Public Property abstract As String

        ''' <summary>
        ''' 作者列表；网页型结果没有这个信息，为空数组
        ''' </summary>
        Public Property authors As String()

        ''' <summary>
        ''' 发表的期刊或者会议名称
        ''' </summary>
        Public Property journal As String

        ''' <summary>
        ''' 发表年份；解析不出来的时候为 0
        ''' </summary>
        Public Property year As Integer

        ''' <summary>
        ''' 被引次数；解析不出来的时候为 0
        ''' </summary>
        Public Property cites As Integer

        ''' <summary>
        ''' 关键词标签
        ''' </summary>
        Public Property keywords As String()

        ''' <summary>
        ''' 全文/PDF 的下载链接；不存在的时候为空字符串
        ''' </summary>
        Public Property pdfUrl As String

        ''' <summary>
        ''' 必应学术的详情页地址；只有学术文献型的结果才有这个值，网页型结果为空字符串
        ''' </summary>
        Public Property profileUrl As String

        ''' <summary>
        ''' 必应学术详情页地址之中的文献编号；不存在的时候为空字符串
        ''' </summary>
        Public Property profileID As String

        ''' <summary>
        ''' 这个结果是否为必应学术的文献结果(即是否可以通过 <see cref="AcademicSearch.GetDetails"/> 拉取详情页)
        ''' </summary>
        Public Property isAcademic As Boolean

        ''' <summary>
        ''' 这个结果所在的搜索结果页面的地址
        ''' </summary>
        Public Property refer As String

        Public Sub New()
            authors = New String() {}
            keywords = New String() {}
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{rank}] {title}"
        End Function
    End Class

    ''' <summary>
    ''' 一页必应学术搜索结果
    ''' </summary>
    Public Class AcademicSearchPage

        ''' <summary>
        ''' 检索所使用的关键词
        ''' </summary>
        Public Property term As String

        ''' <summary>
        ''' 这一页结果对应的搜索地址
        ''' </summary>
        Public Property url As String

        ''' <summary>
        ''' 搜索结果的总数；页面上没有这个信息的时候为 -1
        ''' </summary>
        Public Property total As Integer

        ''' <summary>
        ''' 这一页之上解析出来的结果列表
        ''' </summary>
        Public Property results As AcademicResult()

        Public Sub New()
            total = -1
            results = New AcademicResult() {}
        End Sub

        Public Overrides Function ToString() As String
            Return $"{term} ({total} results) -> {results.Length} parsed"
        End Function
    End Class
End Namespace
