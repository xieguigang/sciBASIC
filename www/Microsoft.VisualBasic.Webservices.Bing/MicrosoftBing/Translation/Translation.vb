#Region "Microsoft.VisualBasic::e28f7d81aaac18c4f4c25ad43257dd7d, www\Microsoft.VisualBasic.Webservices.Bing\MicrosoftBing\Translation\Translation.vb"

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

    '   Total Lines: 107
    '    Code Lines: 88 (82.24%)
    ' Comment Lines: 5 (4.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (13.08%)
    '     File Size: 3.92 KB


    '     Module Translation
    ' 
    '         Function: GetTranslation, GetWords, parseResult, webGet
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Trinity.NLP
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser
Imports r = System.Text.RegularExpressions.Regex

Namespace Bing.Translation

    Public Module Translation

        ''' <summary>
        ''' 如果没有翻译结果，则返回空值
        ''' </summary>
        ''' <param name="word"></param>
        ''' <returns></returns>
        Public Function GetTranslation(word As Value(Of String)) As WordTranslation
            If (word = Strings.Trim(word)).StringEmpty Then
                Return Nothing
            Else
                With Translation.webGet(word)
                    If .IsNothing Then
                        Return Nothing
                    Else
                        Return .parseResult(word)
                    End If
                End With
            End If
        End Function

        Private Function webGet(word As String) As String
            Dim term$ = word.UrlEncode
            Dim url$ = $"https://cn.bing.com/dict/search?q={term}&qs=n&form=Z9LH5&sp=-1&pq={term}&sc=6-10&sk=&cvid=0BC4AECB5070489794D29912A900BEF5"
            Dim headers As New Dictionary(Of String, String) From {
                {"refer", SearchEngineProvider.BingRefer},
                {"accept-language", "zh-CN,zh;q=0.9,en;q=0.8,la;q=0.7"}
            }
            Dim meta = url _
                .GET(headers:=headers) _
                .ParseHtmlMeta
            Dim parsed = meta _
                .Where(Function(m)
                           Return m.Key.TextEquals("description")
                       End Function) _
                .FirstOrDefault

            Return parsed.Value
        End Function

        <Extension>
        Private Function parseResult(content$, word$) As WordTranslation
            Dim result As Word()
            Dim pronunciation$()

            If InStr(content, "等在线英语服务。") > 0 OrElse content = "词典" Then
                Return Nothing
            End If

            content = r.Replace(content, "必应词典为您提供.+?的释义，", "")
            pronunciation = r _
                .Matches(content, "[美英德日法]\[.+?\]") _
                .ToArray

            For Each pro In pronunciation
                content = content.Replace(pro, "")
            Next

            result = r.Replace(content, "(，\s*){2,}", "") _
                .Split("；"c) _
                .Select(Function(s) s.Trim.GetWords) _
                .IteratesALL _
                .Where(Function(s)
                           Return Not s Is Nothing AndAlso Not s.str.StringEmpty
                       End Function) _
                .ToArray

            Return New WordTranslation With {
                .Word = word,
                .Translations = result,
                .Pronunciation = pronunciation
            }
        End Function

        <Extension>
        Private Function GetWords(s As String) As Word()
            Dim cls = s.GetTagValue(" ")
            Dim [class] As WordClass
            Dim words$()

            If Not cls.Name.StringEmpty AndAlso cls.Name.Last = "."c Then
                [class] = Trinity.GetClass(cls.Name)
                words = cls.Value.Split("："c).Last.Split("；"c)
            Else
                [class] = WordClass.NA
                words = s.Split("："c).Last.Split("；"c)
            End If

            Return words _
                .Select(Function(w)
                            Return New Word(w.Trim) With {
                                .class = [class]
                            }
                        End Function) _
                .ToArray
        End Function
    End Module
End Namespace
