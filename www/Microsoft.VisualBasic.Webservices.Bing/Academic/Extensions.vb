#Region "Microsoft.VisualBasic::25fbcd0712d279fed27abdd65c98993e, www\Microsoft.VisualBasic.Webservices.Bing\MicrosoftBing\Academic\Extensions.vb"

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

    '   Total Lines: 82
    '    Code Lines: 71 (86.59%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (13.41%)
    '     File Size: 2.97 KB


    '     Module Extensions
    ' 
    '         Function: Summary
    ' 
    '         Sub: Build_KB
    ' 
    '     Class Summary
    ' 
    '         Properties: authors, cites, doi, journal, pubmed
    '                     title, year
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace Bing.Academic

    Public Module Extensions

        ''' <summary>
        ''' 遍历一个关键词的所有的学术搜索结果，将结果保存到本地，用于知识库的构建
        ''' </summary>
        ''' <param name="term">检索关键词</param>
        ''' <param name="out">结果的输出文件夹</param>
        ''' <param name="pages">需要遍历的页数</param>
        ''' <param name="flat">为<c>True</c>的时候所有的结果都平铺在输出文件夹之下</param>
        ''' <param name="sleepInterval">每一次请求之间的间隔毫秒数，用于避免对必应的高频请求</param>
        ''' <param name="details">
        ''' 是否额外的拉取必应学术的详情页；只有学术文献型的结果才有详情页，
        ''' 网页型的结果会被跳过。
        ''' </param>
        Public Sub Build_KB(term$, out$, Optional pages% = 10,
                            Optional flat As Boolean = True,
                            Optional sleepInterval% = 5000,
                            Optional details As Boolean = False)

            For Each entry As (refer$, list As AcademicResult()) In AcademicSearch.Query(term, pages)
                For Each item As AcademicResult In entry.list
                    Try
                        Dim id$ = FileName(item)
                        Dim path$

                        If flat Then
                            path = $"{out}/{id}.json"
                        Else
                            path = $"{out}/{Mid(id, 1, 3)}/{id}.json"
                        End If

                        Call item _
                            .GetJson(indent:=True) _
                            .SaveTo(path, TextEncodings.UTF8WithoutBOM)

                        If details AndAlso item.isAcademic Then
                            Dim profile As ArticleProfile = item.GetDetails(refer:=entry.refer)

                            If profile IsNot Nothing Then
                                Call profile _
                                    .GetXml _
                                    .SaveTo($"{out}/{id}.xml", TextEncodings.UTF8WithoutBOM)
                            End If
                        End If
                    Catch ex As Exception
                        ex = New Exception(item.url, ex)
                        Call App.LogException(ex)
                    Finally
                        Call Thread.Sleep(sleepInterval)
                    End Try
                Next
            Next
        End Sub

        ''' <summary>
        ''' 由一个搜索结果条目生成一个在文件系统之中安全的文件名
        ''' </summary>
        Private Function FileName(item As AcademicResult) As String
            If Not String.IsNullOrEmpty(item.profileID) Then
                Return item.profileID
            End If

            Dim id As String = If(item.url, "")

            For Each ch As Char In Path.GetInvalidFileNameChars()
                id = id.Replace(ch, "_"c)
            Next

            id = id _
                .Replace("://", "_") _
                .Replace("/"c, "_"c) _
                .Replace("?"c, "_"c) _
                .Replace("="c, "_"c) _
                .Replace("&"c, "_"c) _
                .Replace(":"c, "_"c)

            If id.Length > 96 Then
                id = id.Substring(0, 96)
            End If
            If id.Length = 0 Then
                id = "result_" & item.rank
            End If

            Return id
        End Function

        ''' <summary>
        ''' 将搜索结果转换为文献的概要信息
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Summary(articles As IEnumerable(Of ArticleProfile)) As Summary()
            Return articles _
                .Select(Function(a) New Summary(a)) _
                .ToArray
        End Function

        ''' <summary>
        ''' 将搜索结果页面之上解析出来的条目转换为文献的概要信息
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Summary(results As IEnumerable(Of AcademicResult)) As Summary()
            Return results _
                .Select(Function(r) New Summary(r)) _
                .ToArray
        End Function
    End Module

    Public Class Summary

        Public Property title As String
        Public Property authors As String()
        Public Property year As String
        Public Property journal As String
        Public Property doi As String
        Public Property cites As Integer
        Public Property pubmed As String
        ''' <summary>
        ''' 这个概要所对应的原始链接地址
        ''' </summary>
        Public Property url As String

        Sub New()
        End Sub

        Sub New(article As ArticleProfile)
            title = article.title
            authors = article _
                .authors _
                .Select(Function(a) a.title) _
                .ToArray
            year = article.PubDate.Year
            journal = article.journal.title
            doi = article.DOI
            cites = article.cites.Sum(Function(c) c.Volume)
            pubmed = article _
                .source _
                .Where(Function(link)
                           Return link.title.TextEquals("www.ncbi.nlm.nih.gov")
                       End Function) _
                .FirstOrDefault _
                .href
            url = article.URL
        End Sub

        ''' <summary>
        ''' 由搜索结果页面之上解析出来的条目构造概要信息
        ''' </summary>
        ''' <remarks>
        ''' 搜索结果页面之上没有 DOI 与 PubMed 的信息，这两个字段会被留空，
        ''' 需要通过 <see cref="AcademicSearch.GetDetails"/> 拉取详情页之后才可以补齐。
        ''' </remarks>
        Sub New(result As AcademicResult)
            title = result.title
            authors = result.authors
            year = If(result.year > 0, result.year.ToString(), "")
            journal = result.journal
            cites = result.cites
            url = result.url
        End Sub

        Public Overrides Function ToString() As String
            Return $"{authors.FirstOrDefault} ({year})"
        End Function
    End Class
End Namespace
