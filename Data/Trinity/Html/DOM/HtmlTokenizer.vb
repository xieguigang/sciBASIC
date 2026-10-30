Imports System.Collections.Generic
Imports System.Text

Namespace Html

    ''' <summary>
    ''' HTML 文档的词法扫描器：对原始文档做一次线性的单遍扫描，产生记号序列
    ''' </summary>
    ''' <remarks>
    ''' 这里使用手写状态机而不是正则表达式，主要是出于两个方面的考虑：
    '''
    ''' <list type="number">
    ''' <item>真实世界的页面体积经常在数百 KB 的量级，基于回溯的正则匹配会非常慢，甚至会触发超时</item>
    ''' <item>``&lt;script&gt;`` 代码块之中大量存在不成对的尖括号，正则无法正确的区分标签与文本</item>
    ''' </list>
    '''
    ''' 扫描器是宽容的(lenient)：遇到不认识的写法不会报错，而是尽最大努力的产生一个合理的记号序列。
    ''' </remarks>
    Friend Module HtmlTokenizer

        ''' <summary>
        ''' 对给定的 HTML 文档文本做词法扫描
        ''' </summary>
        ''' <param name="html">原始的 HTML 文档文本</param>
        ''' <returns>记号序列；空的输入会返回一个空序列</returns>
        Friend Function Scan(html As String) As List(Of HtmlToken)
            Dim tokens As New List(Of HtmlToken)()

            If String.IsNullOrEmpty(html) Then
                Return tokens
            End If

            Dim n As Integer = html.Length
            Dim i As Integer = 0
            Dim buffer As New StringBuilder()

            While i < n
                Dim ch As Char = html(i)

                If ch <> "<"c Then
                    Call buffer.Append(ch)
                    i += 1
                    Continue While
                End If

                ' 遇到标签开始符号，先把之前累积下来的文本输出为一个文本记号
                Call FlushText(tokens, buffer, i)

                If i + 1 >= n Then
                    ' 文档以一个孤立的 '<' 结束
                    Call buffer.Append("<"c)
                    Exit While
                End If

                Dim c1 As Char = html(i + 1)

                If c1 = "!"c Then
                    i = ScanBang(html, i, tokens)
                ElseIf c1 = "?"c Then
                    ' 处理指令，例如 <?xml version="1.0"?>，直接丢弃
                    Dim gt As Integer = html.IndexOf(">"c, i + 2)
                    i = If(gt < 0, n, gt + 1)
                ElseIf c1 = "/"c Then
                    i = ScanCloseTag(html, i, tokens)
                ElseIf IsNameChar(c1) Then
                    i = ScanOpenTag(html, i, tokens, buffer)
                Else
                    ' '<' 后面不是标签名字符，例如 a &lt; b，将其作为普通文本保留下来
                    Call buffer.Append("<"c)
                    i += 1
                End If
            End While

            Call FlushText(tokens, buffer, n)

            Return tokens
        End Function

        Private Sub FlushText(tokens As List(Of HtmlToken), buffer As StringBuilder, pos As Integer)
            If buffer.Length = 0 Then
                Return
            End If

            Call tokens.Add(HtmlToken.OfText(buffer.ToString(), pos))
            Call buffer.Clear()
        End Sub

        ''' <summary>
        ''' 扫描 ``&lt;!`` 开头的记号：注释、CDATA 或者 DOCTYPE 声明
        ''' </summary>
        Private Function ScanBang(html As String, i As Integer, tokens As List(Of HtmlToken)) As Integer
            If MatchAt(html, i, "<!--") Then
                Dim close As Integer = html.IndexOf("-->", i + 4, StringComparison.Ordinal)

                If close < 0 Then
                    ' 没有闭合的注释，则文档剩下的所有内容都作为注释处理
                    Call tokens.Add(HtmlToken.OfComment(html.Substring(i + 4), i))
                    Return html.Length
                End If

                Call tokens.Add(HtmlToken.OfComment(html.Substring(i + 4, close - i - 4), i))
                Return close + 3
            End If

            If MatchAt(html, i, "<![CDATA[") Then
                Dim close As Integer = html.IndexOf("]]>", i + 9, StringComparison.Ordinal)

                If close < 0 Then
                    Call tokens.Add(HtmlToken.OfCData(html.Substring(i + 9), i))
                    Return html.Length
                End If

                Call tokens.Add(HtmlToken.OfCData(html.Substring(i + 9, close - i - 9), i))
                Return close + 3
            End If

            ' DOCTYPE 声明，或者写法不规范的注释声明
            Dim gt As Integer = html.IndexOf(">"c, i + 2)

            If gt < 0 Then
                Return html.Length
            End If

            Call tokens.Add(HtmlToken.OfDoctype(html.Substring(i + 2, gt - i - 2).Trim(), i))
            Return gt + 1
        End Function

        ''' <summary>
        ''' 扫描一个结束标签
        ''' </summary>
        Private Function ScanCloseTag(html As String, i As Integer, tokens As List(Of HtmlToken)) As Integer
            Dim gt As Integer = html.IndexOf(">"c, i + 2)

            If gt < 0 Then
                Return html.Length
            End If

            Dim name As String = html.Substring(i + 2, gt - i - 2).Trim().TrimEnd("/"c).Trim()

            If name.Length > 0 Then
                Call tokens.Add(New HtmlToken With {
                    .Type = HtmlTokenType.CloseTag,
                    .Name = name.ToLowerInvariant(),
                    .Position = i
                })
            End If

            Return gt + 1
        End Function

        ''' <summary>
        ''' 扫描一个开始标签；如果标签是原始文本元素，则继续把块内的原始文本一并扫描出来
        ''' </summary>
        Private Function ScanOpenTag(html As String,
                                     i As Integer,
                                     tokens As List(Of HtmlToken),
                                     buffer As StringBuilder) As Integer

            Dim gt As Integer = FindTagEnd(html, i + 1)

            If gt < 0 Then
                ' 没有闭合的标签，剩下的内容全部作为文本处理
                Call buffer.Append(html.Substring(i))
                Return html.Length
            End If

            Dim raw As String = html.Substring(i + 1, gt - i - 1)
            Dim selfClosing As Boolean = raw.TrimEnd().EndsWith("/")

            If selfClosing Then
                raw = raw.TrimEnd().TrimEnd("/"c)
            End If

            Dim name As String = ParseTagName(raw)

            If name.Length = 0 Then
                Call buffer.Append(html.Substring(i, gt - i + 1))
                Return gt + 1
            End If

            Dim isVoid As Boolean = HtmlTags.IsVoid(name)
            Dim type As HtmlTokenType = If(selfClosing OrElse isVoid,
                HtmlTokenType.SelfClosingTag,
                HtmlTokenType.OpenTag)

            Call tokens.Add(New HtmlToken With {
                .Type = type,
                .Name = name.ToLowerInvariant(),
                .Attributes = ParseAttributes(raw.Substring(name.Length)),
                .Position = i
            })

            Dim nextPos As Integer = gt + 1

            If type = HtmlTokenType.OpenTag AndAlso HtmlTags.IsRawText(name) Then
                nextPos = ScanRawText(html, nextPos, name.ToLowerInvariant(), tokens)
            End If

            Return nextPos
        End Function

        ''' <summary>
        ''' 扫描 ``&lt;script&gt;`` ``&lt;style&gt;`` 这类原始文本块之内的内容
        ''' </summary>
        Private Function ScanRawText(html As String,
                                     i As Integer,
                                     name As String,
                                     tokens As List(Of HtmlToken)) As Integer

            Dim close As String = "</" & name
            Dim idx As Integer = IndexOfIgnoreCase(html, close, i)

            If idx < 0 Then
                ' 没有结束标签，剩下的所有内容都作为这个块的原始文本
                Call tokens.Add(HtmlToken.OfText(html.Substring(i), i))
                Return html.Length
            End If

            Call tokens.Add(HtmlToken.OfText(html.Substring(i, idx - i), i))
            Call tokens.Add(New HtmlToken With {
                .Type = HtmlTokenType.CloseTag,
                .Name = name,
                .Position = idx
            })

            Dim gt As Integer = html.IndexOf(">"c, idx + close.Length)
            Return If(gt < 0, html.Length, gt + 1)
        End Function

        ''' <summary>
        ''' 从标签的开始位置开始查找标签的结束符号 ``&gt;``，查找的过程之中会跳过引号之内的内容
        ''' </summary>
        Friend Function FindTagEnd(html As String, start As Integer) As Integer
            Dim i As Integer = start
            Dim n As Integer = html.Length

            While i < n
                Dim ch As Char = html(i)

                If ch = """"c Then
                    i = html.IndexOf(""""c, i + 1)
                    If i < 0 Then Return -1
                    i += 1
                ElseIf ch = "'"c Then
                    i = html.IndexOf("'"c, i + 1)
                    If i < 0 Then Return -1
                    i += 1
                ElseIf ch = ">"c Then
                    Return i
                Else
                    i += 1
                End If
            End While

            Return -1
        End Function

        ''' <summary>
        ''' 从标签文本的开头位置解析出标签名
        ''' </summary>
        Private Function ParseTagName(raw As String) As String
            Dim i As Integer = 0
            Dim n As Integer = raw.Length

            While i < n
                Dim ch As Char = raw(i)

                If Char.IsWhiteSpace(ch) OrElse ch = "/"c OrElse ch = ">"c Then
                    Exit While
                End If

                i += 1
            End While

            Return raw.Substring(0, i)
        End Function

        ''' <summary>
        ''' 解析标签之中的属性列表
        ''' </summary>
        ''' <param name="raw">标签名之后的标签文本片段</param>
        ''' <remarks>
        ''' 支持四种常见的属性写法：``name="value"``、``name='value'``、``name=value`` 以及
        ''' 没有值的布尔属性 ``name``。重复出现的属性保留第一个。
        ''' </remarks>
        Friend Function ParseAttributes(raw As String) As List(Of KeyValuePair(Of String, String))
            Dim attrs As New List(Of KeyValuePair(Of String, String))()

            If String.IsNullOrEmpty(raw) Then
                Return attrs
            End If

            Dim i As Integer = 0
            Dim n As Integer = raw.Length

            While i < n
                ' 跳过属性之间的空白以及多余的自闭合符号
                While i < n AndAlso (Char.IsWhiteSpace(raw(i)) OrElse raw(i) = "/"c)
                    i += 1
                End While

                If i >= n Then
                    Exit While
                End If

                Dim start As Integer = i

                While i < n AndAlso
                    Not Char.IsWhiteSpace(raw(i)) AndAlso
                    raw(i) <> "="c AndAlso
                    raw(i) <> ">"c AndAlso
                    raw(i) <> "/"c

                    i += 1
                End While

                If i = start Then
                    ' 无法识别的字符，跳过去继续
                    i += 1
                    Continue While
                End If

                Dim key As String = raw.Substring(start, i - start)

                While i < n AndAlso Char.IsWhiteSpace(raw(i))
                    i += 1
                End While

                Dim value As String = ""

                If i < n AndAlso raw(i) = "="c Then
                    i += 1

                    While i < n AndAlso Char.IsWhiteSpace(raw(i))
                        i += 1
                    End While

                    If i < n AndAlso (raw(i) = """"c OrElse raw(i) = "'"c) Then
                        Dim quote As Char = raw(i)
                        Dim vs As Integer = i + 1

                        i = vs
                        While i < n AndAlso raw(i) <> quote
                            i += 1
                        End While

                        Dim ve As Integer = If(i < n, i, n)

                        value = raw.Substring(vs, ve - vs)

                        If i < n Then
                            i += 1
                        End If
                    Else
                        Dim vs As Integer = i

                        While i < n AndAlso Not Char.IsWhiteSpace(raw(i)) AndAlso raw(i) <> ">"c
                            i += 1
                        End While

                        value = raw.Substring(vs, i - vs)
                    End If
                End If

                Dim lowerKey As String = key.ToLowerInvariant()
                Dim exists As Boolean = False

                For Each a As KeyValuePair(Of String, String) In attrs
                    If String.Equals(a.Key, lowerKey, StringComparison.Ordinal) Then
                        exists = True
                        Exit For
                    End If
                Next

                If Not exists Then
                    Call attrs.Add(New KeyValuePair(Of String, String)(lowerKey, value))
                End If
            End While

            Return attrs
        End Function

        ''' <summary>
        ''' 标签名以及属性名所允许出现的字符
        ''' </summary>
        Private Function IsNameChar(ch As Char) As Boolean
            If Char.IsLetterOrDigit(ch) Then
                Return True
            End If

            Return ch = "-"c OrElse ch = "_"c OrElse ch = ":"c OrElse ch = "."c OrElse ch = "*"c
        End Function

        Private Function MatchAt(html As String, i As Integer, text As String) As Boolean
            If i + text.Length > html.Length Then
                Return False
            End If

            Return String.Compare(html, i, text, 0, text.Length, StringComparison.Ordinal) = 0
        End Function

        Private Function IndexOfIgnoreCase(html As String, text As String, start As Integer) As Integer
            If start < 0 Then
                start = 0
            End If
            If start >= html.Length Then
                Return -1
            End If

            Return html.IndexOf(text, start, StringComparison.OrdinalIgnoreCase)
        End Function
    End Module

End Namespace
