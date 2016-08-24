#Region "Microsoft.VisualBasic::49bc6fcd320143b6d74c96a34cfa9cee, ..\visualbasic_App\DocumentFormats\VB_HTML\VB_HTML\MarkDown\Articles.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
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

        Const RegexWikiArticles As String =
            vbCr & vbLf & "                    (?:https?\:\/\/)" & vbCr & vbLf & "                    (?:www\.)?" & vbCr & vbLf & "                    ([a-z]+)" & vbCr & vbLf & "                    \.wikipedia\.org\/wiki\/" & vbCr & vbLf & "                    ([^\^\s\(\)\[\]\<\>]+)"

        Dim _wikiArticles As New Regex(RegexWikiArticles, RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace)

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

            Return [String].Format("[{0}_wiki://{1}](https://{0}.wikipedia.org/wiki/{1})", lang, title)
        End Function
    End Module
End Namespace
