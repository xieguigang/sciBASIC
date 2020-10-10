#Region "Microsoft.VisualBasic::3d1f20edf0b7c1eb08f5f2fe33692bbb, Data\Trinity\Html\Html2Article.vb"

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

' Class Html2Article
' 
'     Properties: AppendMode, Depth, LimitCount
' 
'     Function: FormatTag, GetArticle, GetPublishDate, GetTitle
' 
'     Sub: GetContent
' 
' /********************************************************************************/

#End Region

' Author: StanZhai 翟士丹（mail@zhaishidan.cn）. All rights reserved. See License.md in the project root for license information.
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web
Imports Microsoft.VisualBasic.ComponentModel.Collection

''' <summary>
''' 解析Html页面的文章正文内容,基于文本密度的HTML正文提取类
''' https://github.com/stanzhai/Html2Article
''' http://www.cnblogs.com/jasondan/p/3497757.html
''' 
''' Date:   2012/12/30
''' Update: 
'''     2013/7/10   优化文章头部分析算法，优化
'''     2014/4/25   添加Html代码中注释过滤的正则
'''         
''' </summary>
Public Class Html2Article

#Region "参数设置"

    ' 正则表达式过滤：正则表达式，要替换成的文本
    ' 过滤Html代码中的注释
    ' 针对链接密集型的网站的处理，主要是门户类的网站，降低链接干扰
    Shared ReadOnly Filters As String()() = {
        {"(?is)<script.*?>.*?</script>", ""},
        {"(?is)<style.*?>.*?</style>", ""},
        {"(?is)<!--.*?-->", ""},
        {"(?is)</a>", "</a>" & vbLf}
    }.ToVectorList

    ''' <summary>
    ''' 是否使用追加模式，默认为false
    ''' 使用追加模式后，会将符合过滤条件的所有文本提取出来
    ''' </summary>
    Public Shared Property AppendMode() As Boolean = False

    ''' <summary>
    ''' 按行分析的深度，默认为6
    ''' </summary>
    Public Shared Property Depth() As Integer = 6

    ''' <summary>
    ''' 字符限定数，当分析的文本数量达到限定数则认为进入正文内容
    ''' 默认180个字符数
    ''' </summary>
    Public Shared Property LimitCount() As Integer = 180


    ' 确定文章正文头部时，向上查找，连续的空行到达_headEmptyLines，则停止查找
    Shared _headEmptyLines As Integer = 2
    ' 用于确定文章结束的字符数
    Shared _endLimitCharCount As Integer = 20

#End Region

    ''' <summary>
    ''' 从给定的Html原始文本中获取正文信息
    ''' </summary>
    ''' <param name="html"></param>
    ''' <returns></returns>
    Public Shared Function GetArticle(html As String) As Article
        ' 如果换行符的数量小于10，则认为html为压缩后的html
        ' 由于处理算法是按照行进行处理，需要为html标签添加换行符，便于处理
        If html.Count(Function(c) c = ControlChars.Lf) < 10 Then
            html = html.Replace(">", ">" & vbLf)
        End If

        ' 获取html，body标签内容
        Dim body As String = ""
        Dim bodyFilter As String = "(?is)<body.*?</body>"
        Dim m As Match = Regex.Match(html, bodyFilter)
        If m.Success Then
            body = m.ToString()
        End If

        ' 过滤样式，脚本等不相干标签
        For Each Filter As String() In Filters
            body = Regex.Replace(body, Filter(0), Filter(1))
        Next

        ' 标签规整化处理，将标签属性格式化处理到同一行
        ' 处理形如以下的标签：
        '  <a 
        '   href='http://www.baidu.com'
        '   class='test'
        ' 处理后为
        '  <a href='http://www.baidu.com' class='test'>
        body = Regex.Replace(body, "(<[^<>]+)\s*\n\s*", AddressOf FormatTag)

        Dim content As String = Nothing
        Dim contentWithTags As String = Nothing

        GetContent(body, content, contentWithTags)

        Dim article As New Article() With {
            .Title = GetTitle(html),
            .PublishDate = GetPublishDate(body),
            .Content = content,
            .ContentWithTags = contentWithTags
        }

        Return article
    End Function

    ''' <summary>
    ''' 格式化标签，剔除匹配标签中的回车符
    ''' </summary>
    ''' <param name="match"></param>
    ''' <returns></returns>
    Private Shared Function FormatTag(match As Match) As String
        Dim sb As New StringBuilder()
        For Each ch As Char In match.Value
            If ch = ControlChars.Cr OrElse ch = ControlChars.Lf Then
                Continue For
            End If
            sb.Append(ch)
        Next
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' 获取时间
    ''' </summary>
    ''' <param name="html"></param>
    ''' <returns></returns>
    Private Shared Function GetTitle(html As String) As String
        Dim titleFilter As String = "<title>[\s\S]*?</title>"
        Dim h1Filter As String = "<h1.*?>.*?</h1>"
        Dim clearFilter As String = "<.*?>"

        Dim title As String = ""
        Dim match As Match = Regex.Match(html, titleFilter, RegexOptions.IgnoreCase)
        If match.Success Then
            title = Regex.Replace(match.Groups(0).Value, clearFilter, "")
        End If

        ' 正文的标题一般在h1中，比title中的标题更干净
        match = Regex.Match(html, h1Filter, RegexOptions.IgnoreCase)
        If match.Success Then
            Dim h1 As String = Regex.Replace(match.Groups(0).Value, clearFilter, "")
            If Not [String].IsNullOrEmpty(h1) AndAlso title.StartsWith(h1) Then
                title = h1
            End If
        End If
        Return title
    End Function

    ''' <summary>
    ''' 获取文章发布日期
    ''' </summary>
    ''' <param name="html"></param>
    ''' <returns></returns>
    Private Shared Function GetPublishDate(html As String) As DateTime
        ' 过滤html标签，防止标签对日期提取产生影响
        Dim text As String = Regex.Replace(html, "(?is)<.*?>", "")
        Dim match As Match = Regex.Match(text, "((\d{4}|\d{2})(\-|\/)\d{1,2}\3\d{1,2})(\s?\d{2}:\d{2})?|(\d{4}年\d{1,2}月\d{1,2}日)(\s?\d{2}:\d{2})?", RegexOptions.IgnoreCase)

        Dim result As New DateTime(1900, 1, 1)
        If match.Success Then
            Try
                Dim dateStr As String = ""
                For i As Integer = 0 To match.Groups.Count - 1
                    dateStr = match.Groups(i).Value
                    If Not [String].IsNullOrEmpty(dateStr) Then
                        Exit For
                    End If
                Next
                ' 对中文日期的处理
                If dateStr.Contains("年") Then
                    Dim sb As New StringBuilder()
                    For Each ch As Char In dateStr
                        If ch = "年"c OrElse ch = "月"c Then
                            sb.Append("/")
                            Continue For
                        End If
                        If ch = "日"c Then
                            sb.Append(" "c)
                            Continue For
                        End If
                        sb.Append(ch)
                    Next
                    dateStr = sb.ToString()
                End If
                result = Convert.ToDateTime(dateStr)
            Catch ex As Exception
                Console.WriteLine(ex)
            End Try
            If result.Year < 1900 Then
                result = New DateTime(1900, 1, 1)
            End If
        End If
        Return result
    End Function

    ''' <summary>
    ''' 从body标签文本中分析正文内容
    ''' </summary>
    ''' <param name="bodyText">只过滤了script和style标签的body文本内容</param>
    ''' <param name="content">返回文本正文，不包含标签</param>
    ''' <param name="contentWithTags">返回文本正文包含标签</param>
    Private Shared Sub GetContent(bodyText As String, ByRef content As String, ByRef contentWithTags As String)
        Dim orgLines As String() = Nothing
        ' 保存原始内容，按行存储
        Dim lines As String() = Nothing
        ' 保存干净的文本内容，不包含标签
        orgLines = bodyText.Split(ControlChars.Lf)
        lines = New String(orgLines.Length - 1) {}
        ' 去除每行的空白字符,剔除标签
        For i As Integer = 0 To orgLines.Length - 1
            Dim lineInfo As String = orgLines(i)
            ' 处理回车，使用[crlf]做为回车标记符，最后统一处理
            lineInfo = Regex.Replace(lineInfo, "(?is)</p>|<br.*?/>", "[crlf]")
            lines(i) = Regex.Replace(lineInfo, "(?is)<.*?>", "").Trim()
        Next

        Dim sb As New StringBuilder()
        Dim orgSb As New StringBuilder()

        Dim preTextLen As Integer = 0
        ' 记录上一次统计的字符数量
        Dim startPos As Integer = -1
        ' 记录文章正文的起始位置
        For i As Integer = 0 To lines.Length - _Depth - 1
            Dim len As Integer = 0
            For j As Integer = 0 To _Depth - 1
                len += lines(i + j).Length
            Next

            If startPos = -1 Then
                ' 还没有找到文章起始位置，需要判断起始位置
                If preTextLen > _LimitCount AndAlso len > 0 Then
                    ' 如果上次查找的文本数量超过了限定字数，且当前行数字符数不为0，则认为是开始位置
                    ' 查找文章起始位置, 如果向上查找，发现2行连续的空行则认为是头部
                    Dim emptyCount As Integer = 0
                    For j As Integer = i - 1 To 1 Step -1
                        If [String].IsNullOrEmpty(lines(j)) Then
                            emptyCount += 1
                        Else
                            emptyCount = 0
                        End If
                        If emptyCount = _headEmptyLines Then
                            startPos = j + _headEmptyLines
                            Exit For
                        End If
                    Next
                    ' 如果没有定位到文章头，则以当前查找位置作为文章头
                    If startPos = -1 Then
                        startPos = i
                    End If
                    ' 填充发现的文章起始部分
                    For j As Integer = startPos To i
                        sb.Append(lines(j))
                        orgSb.Append(orgLines(j))
                    Next
                End If
            Else
                'if (len == 0 && preTextLen == 0)    // 当前长度为0，且上一个长度也为0，则认为已经结束
                If len <= _endLimitCharCount AndAlso preTextLen < _endLimitCharCount Then
                    ' 当前长度为0，且上一个长度也为0，则认为已经结束
                    If Not _AppendMode Then
                        Exit For
                    End If
                    startPos = -1
                End If
                sb.Append(lines(i))
                orgSb.Append(orgLines(i))
            End If
            preTextLen = len
        Next

        Dim result As String = sb.ToString()
        ' 处理回车符，更好的将文本格式化输出
        content = result.Replace("[crlf]", Environment.NewLine)
        content = HttpUtility.HtmlDecode(content)
        ' 输出带标签文本
        contentWithTags = orgSb.ToString()
    End Sub
End Class
