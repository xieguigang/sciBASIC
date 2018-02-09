#Region "Microsoft.VisualBasic::270ba81df8c7d6c9f4d383362569b6a4, ..\sciBASIC#\www\Microsoft.VisualBasic.Webservices.Bing\Translation.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
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

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.HtmlParser
Imports r = System.Text.RegularExpressions.Regex

Public Module Translation

    ''' <summary>
    ''' 如果没有翻译结果，则返回空值
    ''' </summary>
    ''' <param name="word"></param>
    ''' <returns></returns>
    Public Function GetTranslation(word As String) As WordTranslation
        Dim term$ = word.UrlEncode
        Dim url$ = $"https://cn.bing.com/dict/search?q={term}&qs=n&form=Z9LH5&sp=-1&pq={term}&sc=6-10&sk=&cvid=0BC4AECB5070489794D29912A900BEF5"
        Dim headers As New Dictionary(Of String, String) From {
            {"refer", SearchEngineProvider.BingRefer},
            {"accept-language", "zh-CN,zh;q=0.9,en;q=0.8,la;q=0.7"}
        }
        Dim meta$() = url _
            .GET(headers:=headers) _
            .Matches("<meta.+?/>", RegexICSng) _
            .ToArray
        Dim parsed = meta _
            .Select(Function(m) m.TagAttributes.ToDictionary) _
            .Where(Function(m)
                       Return m.ContainsKey("name") AndAlso m!name = "description"
                   End Function) _
            .FirstOrDefault

        If parsed Is Nothing Then
            Return Nothing
        Else
            Dim content$ = parsed!content.Value
            content = r _
                .Replace(content, "必应词典为您提供.+?的释义，", "") _
                .Replace("un.", "") _
                .Replace("n.", "")

            Return New WordTranslation With {
                .Word = word,
                .Translations = content _
                    .Split("；"c) _
                    .Select(Function(t) t.Split("："c).Last.Split("，"c)) _
                    .IteratesALL _
                    .Select(AddressOf Strings.Trim) _
                    .Where(Function(s) Not s.StringEmpty) _
                    .ToArray
            }
        End If
    End Function
End Module

''' <summary>
''' 单词翻译的结果
''' </summary>
Public Class WordTranslation

    ''' <summary>
    ''' 输入的目标单词
    ''' </summary>
    ''' <returns></returns>
    Public Property Word As String
    ''' <summary>
    ''' 该单词所产生的翻译结果列表
    ''' </summary>
    ''' <returns></returns>
    Public Property Translations As String()

    Public Overrides Function ToString() As String
        Return $"{Word} -> {Translations.GetJson}"
    End Function
End Class
