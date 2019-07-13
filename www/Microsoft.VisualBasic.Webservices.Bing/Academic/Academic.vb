#Region "Microsoft.VisualBasic::54107145f56d52a0fbdf477c8a045fd0, www\Microsoft.VisualBasic.Webservices.Bing\Academic\Academic.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

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

Namespace Academic

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
