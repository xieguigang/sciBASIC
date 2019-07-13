#Region "Microsoft.VisualBasic::2db9d059b988a989c511dd19f14d240a, www\Microsoft.VisualBasic.Webservices.Bing\Translation\Translation.vb"

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

    ' Module Translation
    ' 
    '     Function: GetTranslation, GetWords, parseResult, webGet
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Trinity.NLP
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser
Imports r = System.Text.RegularExpressions.Regex

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
                       Return Not s Is Nothing AndAlso Not s.Text.StringEmpty
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
                        Return New Word With {
                            .Class = [class],
                            .Text = w.Trim
                        }
                    End Function) _
            .ToArray
    End Function
End Module
