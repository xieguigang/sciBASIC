Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' Interface wrapper of Microsoft bing.com search engine.
''' </summary>
<[PackageNamespace]("Bing",
                    Publisher:="",
                    Category:=APICategories.UtilityTools,
                    Description:="",
                    Url:="http://cn.bing.com/")>
Public Module SearchEngineProvider

    Const _BING_SEARCH_URL As String = "https://www.bing.com/search?q={0}&PC=U316&FORM=Firefox"
    Const LIST_ITEM_REGEX As String = "<li class=""b_algo""><div class=""b_title"">.+?</div></div></li>"

    ''' <summary>
    ''' Bing.com online web search engine services provider
    ''' </summary>
    ''' <param name="KeywordExpression"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <ExportAPI("search")>
    Public Function Search(KeywordExpression As String) As SearchResult
        Dim Url As String = String.Format(_BING_SEARCH_URL, WebServiceUtils.UrlEncode(KeywordExpression))
        Dim web As String = Regex.Replace(Url.GET, "<strong>|</strong>", "", RegexOptions.IgnoreCase)
        web = Strings.Split(web, "</script><div id=""b_content""><div id=""b_pole""><div class=""b_poleContent""><ul class=""b_hList"">").Last
        web = Strings.Split(web, "</li><li class=""b_pag""><span class=""sb_count"">").First
        Dim htmlItems As String() = Regex.Matches(web, LIST_ITEM_REGEX).ToArray
        Dim result As WebResult() = htmlItems.ToArray(AddressOf WebResult.TryParse)

        Return New SearchResult With {.CurrentPage = result}
    End Function
End Module
