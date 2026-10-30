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
    '         Function: GetDetails, getInternal, Query, Search, StripListItem
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
    <Package("Bing.Academic",
         Url:="http://cn.bing.com/academic/?FORM=Z9LH2",
         Description:="",
         Category:=APICategories.UtilityTools,
         Publisher:="")>
    Public Module AcademicSearch

        ' https://cn.bing.com/academic/search?q=Danio+rerio&go=%E6%90%9C%E7%B4%A2&qs=ds&form=QBRE

        Const refer$ = "https://cn.bing.com/academic/?FORM=Z9LH2"

        ''' <summary>
        ''' Run academic search from portal: https://cn.bing.com/academic/?FORM=HDRSC4
        ''' </summary>
        ''' <param name="term"></param>
        ''' <returns></returns>
        Public Function Search(term As String) As NamedValue(Of String)()
            Return getInternal($"https://cn.bing.com/academic/search?q={term.UrlEncode}&go=Search&qs=ds&form=QBRE")
        End Function

        Private Function getInternal(url As String) As NamedValue(Of String)()
            Dim html$ = url.GET(headers:=New Dictionary(Of String, String) From {{NameOf(refer), refer}})

            html = html _
                .RemovesJavaScript _
                .RemovesCSSstyles _
                .RemovesImageLinks _
                .RemovesHtmlHead _
                .RemovesFooter
            html = Strings.Split(html, "<ol id=""b_results""").Last

            Dim list As NamedValue(Of String)() = r _
                .Matches(html, "<li class[=]""aca_algo"">.+?</li>", RegexICSng) _
                .EachValue(AddressOf StripListItem) _
                .ToArray

            Return list
        End Function

        Public Iterator Function Query(term$, Optional pages% = 10) As IEnumerable(Of (refer$, list As NamedValue(Of String)()))
            Dim result As NamedValue(Of String)()

            For i As Integer = 0 To pages
                Dim page$ = i * 10 + 1
                Dim url$ = $"https://cn.bing.com/academic/search?q={term.UrlEncode}&first={page}&go=Search&qs=ds&form=QBRE"

                Try
                    result = getInternal(url)
                Catch ex As Exception
                    result = {}
                    ex = New Exception(url, ex)
                    App.LogException(ex)
                End Try

                Yield (url, result)
            Next
        End Function

        Private Function StripListItem(text As String) As NamedValue(Of String)
            Dim title = text.GetBetween("<h2", "</h2>")
            Dim ResultUrl = title.href
            Dim description = text _
                .GetBetween("<div class=""caption_abstract"">", "</div>") _
                .GetBetween("<p>", "</p>") _
                .StripHTMLTags

            ResultUrl = "https://cn.bing.com/" & ResultUrl.Trim("/"c).Replace("&amp;", "&")
            title = title _
                .RemovesHtmlStrong _
                .GetValue _
                .StripHTMLTags

            Return New NamedValue(Of String) With {
                .Name = title,
                .Value = ResultUrl,
                .Description = description
            }
        End Function

        ''' <summary>
        ''' Aquire details information about an article result.
        ''' </summary>
        ''' <param name="info"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetDetails(info As NamedValue(Of String), Optional refer$ = Nothing) As ArticleProfile
            Return ProfileResult.GetProfile(info.Value, refer)
        End Function
    End Module
End Namespace
