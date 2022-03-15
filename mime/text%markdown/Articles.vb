#Region "Microsoft.VisualBasic::a2b1659279149260b198d5f4bb992bd9, sciBASIC#\mime\text%markdown\Articles.vb"

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

    '   Total Lines: 42
    '    Code Lines: 19
    ' Comment Lines: 14
    '   Blank Lines: 9
    '     File Size: 1.38 KB


    ' Module WikiArticles
    ' 
    '     Function: ArticleEvaluator, Transform
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
