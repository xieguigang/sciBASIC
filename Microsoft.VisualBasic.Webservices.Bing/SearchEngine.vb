Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

<[PackageNamespace]("Bing", Publisher:="", Category:=APICategories.UtilityTools, Description:="", Url:="http://cn.bing.com/")>
Public Module SearchEngineProvider

    Const _BING_SEARCH_URL As String = "https://www.bing.com/search?q={0}&PC=U316&FORM=Firefox"
    Const LIST_ITEM_REGEX As String = "<li class=""b_algo""><div class=""b_title"">.+?</div></div></li>"

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="KeywordExpression"></param>
    ''' <param name="getCounts">if the result count is smaller that this parameter value then will return the real counts. Default value is 10(the bing default first page item count).</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <ExportAPI("search")>
    Public Function Search(KeywordExpression As String, Optional getCounts As Integer = 10) As WebResult()
        Dim Url As String = String.Format(_BING_SEARCH_URL, KeywordExpression.Replace(" ", "%20"))
        Dim PageContent As String = Regex.Replace(Url.GET, "<strong>|</strong>", "", RegexOptions.IgnoreCase)
        PageContent = Strings.Split(PageContent, "</script><div id=""b_content""><div id=""b_pole""><div class=""b_poleContent""><ul class=""b_hList"">").Last
        PageContent = Strings.Split(PageContent, "</li><li class=""b_pag""><span class=""sb_count"">").First
        Dim ListItems As String() = (From m As Match In Regex.Matches(PageContent, LIST_ITEM_REGEX) Select m.Value).ToArray
        Dim LQuery = (From strValue As String In ListItems Select WebResult.TryParse(strValue)).ToArray
        Return LQuery
    End Function
End Module
