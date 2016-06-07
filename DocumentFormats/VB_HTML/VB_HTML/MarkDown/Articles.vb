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
    Public Class Articles
        Implements IExtensionInterface
        Private Shared _wikiArticles As New Regex(vbCr & vbLf & "                    (?:https?\:\/\/)" & vbCr & vbLf & "                    (?:www\.)?" & vbCr & vbLf & "                    ([a-z]+)" & vbCr & vbLf & "                    \.wikipedia\.org\/wiki\/" & vbCr & vbLf & "                    ([^\^\s\(\)\[\]\<\>]+)", RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace)


        Public Function Transform(text As String) As String Implements IExtensionInterface.Transform
            Return _wikiArticles.Replace(text, New MatchEvaluator(AddressOf ArticleEvaluator))
        End Function


        Private Function ArticleEvaluator(match As Match) As String
            Dim lang As String = match.Groups(1).Value
            Dim title As String = WebUtility.UrlDecode(match.Groups(2).Value)

            Return [String].Format("[{0}_wiki://{1}](https://{0}.wikipedia.org/wiki/{1})", lang, title)
        End Function
    End Class
End Namespace
