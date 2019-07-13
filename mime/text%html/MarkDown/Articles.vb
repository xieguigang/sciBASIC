#Region "Microsoft.VisualBasic::2fc0e94666485ad40df0645fd97770b6, mime\text%html\MarkDown\Articles.vb"

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

    '     Module WikiArticles
    ' 
    '         Function: ArticleEvaluator, Transform
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'*
' * This file is part of the MarkdownSharp package
' * For the full copyright and license information,
' * view the LICENSE file that was distributed with this source code.
' 


Imports System.Net
Imports System.Text.RegularExpressions

Namespace MarkDown

    ''' <summary>
    ''' Create short link for https://wikipedia.org
    ''' ex: https://en.wikipedia.org/wiki/Southern_Ontario => en_wiki://Southern_Ontario
    ''' </summary>
    Public Module WikiArticles

        Const RegexWikiArticles As String = "

                (?:https?\:\/\/)
                (?:www\.)?
                ([a-z]+)
                \.wikipedia\.org\/wiki\/
                ([^\^\s\(\)\[\]\<\>]+)"

        ReadOnly _wikiArticles As Regex = RegexWikiArticles.PythonRawRegexp

        ''' <summary>
        ''' <see cref="ExtensionTransform"/>
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        Public Function Transform(text As String) As String
            Return _wikiArticles.Replace(text, New MatchEvaluator(AddressOf ArticleEvaluator))
        End Function

        Private Function ArticleEvaluator(match As Match) As String
            Dim lang As String = match.Groups(1).Value
            Dim title As String = WebUtility.UrlDecode(match.Groups(2).Value)

            Return String.Format("[{0}_wiki://{1}](https://{0}.wikipedia.org/wiki/{1})", lang, title)
        End Function
    End Module
End Namespace
