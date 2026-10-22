#Region "Microsoft.VisualBasic::markdown-blocks, mime\text%markdown\JSONSchema\MarkdownBlocks.vb"

' Author:
'
'       xieguigang (xie.guigang@live.com)
'
' Copyright (c) 2018 GPL3 Licensed
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

' /********************************************************************************/

' Class MarkdownBlockParser
'
'     Function: CollectBlockQuote, CollectFence, CollectListItems, CollectTable
'               IsBlockStart, IsTableStart, MatchAutoLink, MatchBareUrl
'               MatchCodeSpan, MatchEmphasis, MatchFence, MatchLink, MatchListItem
'               MatchStrike, Parse, ParseAligns, ParseBlocks, ParseInlinePlain
'               SplitRow
'
'     Sub: CollapseCode, FindParenClose, GetIndent, IndexOfDelim, IsWord
'          NoLink, UnderscoreDelimiter
'
' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ApplicationServices.Report
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports ASCII = Microsoft.VisualBasic.Text.ASCII
Imports std = System.Math

''' <summary>
''' 将 markdown 文档文本解析为 <see cref="Block"/> 的平面列表。
''' 采用与 <c>MarkdownParser</c> 相同思路的两阶段（块级判定 + 行内解析）递归结构，
''' 但产出结构化 <see cref="Block"/> 对象而不是 HTML 字符串，且行内语法被转换为
''' <see cref="Block.content"/> 所需的**纯文本**（去掉 * _ ~ 标记，链接取显示文本、
''' 图片取 alt、代码段取原始内容）。这样解析出的 Block 可直接交给
''' <c>WordDocument.WriteBlocks</c> 或 "BlockRenderer" 使用。
''' </summary>
Public Class MarkdownBlockParser

    ' =====================================================================
    ' 入口
    ' =====================================================================

    ''' <summary>
    ''' 将 markdown 文档文本解析为 <see cref="Block"/> 平面列表。
    ''' 空或 null 输入返回空列表；无法识别的块退化为 paragraph。
    ''' </summary>
    ''' <param name="markdown">markdown 源文本。</param>
    ''' <returns>按文档顺序排布的 Block 列表（content 为纯文本）。</returns>
    Public Function Parse(markdown As String) As List(Of Block)
        If String.IsNullOrEmpty(markdown) Then
            Return New List(Of Block)
        End If

        Dim text = markdown.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
        Dim lines = text.LineTokens

        Return ParseBlocks(lines)
    End Function

    ' =====================================================================
    ' 块级解析
    ' =====================================================================

    Private Function ParseBlocks(lines() As String) As List(Of Block)
        Dim blocks As New List(Of Block)
        Dim i = 0
        Dim n = lines.Length

        While i < n
            Dim line = lines(i)
            Dim t = line.Trim(" "c, ASCII.TAB, vbCr, vbLf)

            If t.StringEmpty Then
                i += 1
                Continue While
            End If

            ' 1. 围栏代码块（``` 或 ~~~），math/tex/latex 视为 math 块
            Dim fence = MatchFence(line)
            If fence.match Then
                Dim code As String = ""
                Dim lang As String = ""
                Dim endIdx = CollectFence(lines, i, fence, code, lang)

                If lang IsNot Nothing AndAlso New String() {"math", "tex", "latex", "equation"}.Contains(lang.ToLower) Then
                    blocks.Add(New Block With {.type = "math", .content = code, .language = lang})
                Else
                    blocks.Add(New Block With {.type = "code", .content = code, .language = lang})
                End If

                i = endIdx + 1
                Continue While
            End If

            ' 2. 独立数学块 $$ ... $$
            If t.Trim() = "$$" Then
                Dim content As New List(Of String)
                i += 1
                While i < n AndAlso lines(i).Trim() <> "$$"
                    content.Add(lines(i))
                    i += 1
                End While
                If i < n Then
                    i += 1
                End If
                blocks.Add(New Block With {.type = "math", .content = content.JoinBy(vbLf)})
                Continue While
            End If

            ' 3. ATX 标题 (# .. ######)
            Dim hm = Regex.Match(t, "^(#{1,6})\s+(.*?)\s*#*\s*$")
            If hm.Success Then
                Dim level = hm.Groups(1).Value.Length
                If level < 1 Then level = 1
                If level > 6 Then level = 6
                blocks.Add(New Block With {
                    .type = "heading",
                    .level = level,
                    .content = ParseInlinePlain(hm.Groups(2).Value)
                })
                i += 1
                Continue While
            End If

            ' 4. 引用块 (>)
            If t.StartsWith(">") Then
                Dim inner As String() = Nothing
                Dim endIdx = CollectBlockQuote(lines, i, inner)
                Dim innerBlocks = Parse(inner.JoinBy(vbLf))
                Dim content = innerBlocks _
                    .Select(Function(b) b.ToMarkdownBlock) _
                    .JoinBy(vbCrLf)
                blocks.Add(New Block With {.type = "blockquote", .content = content})
                i = endIdx + 1
                Continue While
            End If

            ' 5. 列表 / 任务列表（有序 / 无序，扁平化嵌套）
            Dim li = MatchListItem(line)
            If li.match Then
                Dim itemsRaw As New List(Of String)
                Dim endIdx = CollectListItems(lines, i, itemsRaw, li)

                Dim allTask As Boolean = True
                Dim checkedList As New List(Of Boolean)
                Dim taskTexts As New List(Of String)

                For Each raw As String In itemsRaw.SafeQuery
                    Dim tm = Regex.Match(raw.Trim(), "^\[([ xX])\]\s+(.*)$")
                    If tm.Success Then
                        checkedList.Add(tm.Groups(1).Value.ToLower() = "x")
                        taskTexts.Add(ParseInlinePlain(tm.Groups(2).Value))
                    Else
                        allTask = False
                        Exit For
                    End If
                Next

                If allTask AndAlso itemsRaw.Count > 0 Then
                    blocks.Add(New Block With {
                        .type = "tasklist",
                        .ordered = li.ordered,
                        .items = taskTexts.ToArray,
                        .checked = checkedList.ToArray
                    })
                Else
                    Dim rendered = itemsRaw _
                        .Select(Function(raw) RenderItemText(raw)) _
                        .ToArray
                    blocks.Add(New Block With {
                        .type = "list",
                        .ordered = li.ordered,
                        .items = rendered
                    })
                End If

                i = endIdx + 1
                Continue While
            End If

            ' 6. 脚注 [^id]: content
            Dim fm = Regex.Match(t, "^\[\^([^\]]+)\]:\s*(.*)$")
            If fm.Success Then
                blocks.Add(New Block With {
                    .type = "footnote",
                    .id = fm.Groups(1).Value,
                    .content = ParseInlinePlain(fm.Groups(2).Value)
                })
                i += 1
                Continue While
            End If

            ' 7. 定义列表（term 行后跟 : definition 行）
            If Not IsBlockStart(line, lines, i) AndAlso i + 1 < n Then
                Dim nxtDef = lines(i + 1).Trim(" "c, ASCII.TAB, vbCr, vbLf)
                If nxtDef.StartsWith(":") Then
                    Dim terms As New List(Of String)
                    Dim defs As New List(Of String)
                    Dim j = i

                    While j < n
                        Dim cur = lines(j).Trim(" "c, ASCII.TAB, vbCr, vbLf)
                        If cur.StringEmpty Then
                            Exit While
                        End If
                        If cur.StartsWith(":") Then
                            Exit While
                        End If
                        If IsBlockStart(lines(j), lines, j) Then
                            Exit While
                        End If

                        terms.Add(ParseInlinePlain(cur))
                        j += 1

                        If j < n AndAlso lines(j).Trim(" "c, ASCII.TAB, vbCr, vbLf).StartsWith(":") Then
                            defs.Add(ParseInlinePlain(lines(j).Trim(" "c, ASCII.TAB, vbCr, vbLf).Substring(1).Trim()))
                            j += 1
                        Else
                            defs.Add("")
                        End If
                    End While

                    blocks.Add(New Block With {
                        .type = "deflist",
                        .terms = terms.ToArray,
                        .definitions = defs.ToArray
                    })
                    i = j
                    Continue While
                End If
            End If

            ' 8. 表格
            If IsTableStart(lines, i) Then
                Dim headerCells = SplitRow(lines(i).Trim("|"c).Trim(" "c, ASCII.TAB))
                Dim sepCells = SplitRow(lines(i + 1).Trim("|"c).Trim(" "c, ASCII.TAB))
                Dim aligns = ParseAligns(sepCells)

                Dim rows As New List(Of String())
                Dim j = i + 2
                While j < n
                    Dim l = lines(j).Trim(" "c, ASCII.TAB, vbCr, vbLf)
                    If l.StringEmpty OrElse Not l.Contains("|") OrElse IsBlockStart(l, lines, j) Then
                        Exit While
                    End If
                    Dim cells = SplitRow(l.Trim("|"c).Trim(" "c, ASCII.TAB))
                    rows.Add(cells.Select(Function(c) ParseInlinePlain(c.Trim())).ToArray)
                    j += 1
                End While

                Dim head = headerCells.Select(Function(c) ParseInlinePlain(c.Trim())).ToArray
                blocks.Add(New Block With {
                    .type = "table",
                    .headers = head,
                    .alignments = aligns,
                    .rows = rows.ToArray
                })
                i = j
                Continue While
            End If

            ' 9. setext 标题（下划线 = 或 -）
            If i + 1 < n Then
                Dim nxt = lines(i + 1).Trim(" "c, ASCII.TAB, vbCr, vbLf)
                If Regex.IsMatch(nxt, "^=+\s*$") Then
                    blocks.Add(New Block With {.type = "heading", .level = 1, .content = ParseInlinePlain(t)})
                    i += 2
                    Continue While
                End If
                If Regex.IsMatch(nxt, "^-{2,}\s*$") Then
                    blocks.Add(New Block With {.type = "heading", .level = 2, .content = ParseInlinePlain(t)})
                    i += 2
                    Continue While
                End If
            End If

            ' 10. 分隔线
            If Regex.IsMatch(t, "^\s*([-*_])(\s*\1){2,}\s*$") Then
                blocks.Add(New Block With {.type = "hr"})
                i += 1
                Continue While
            End If

            ' 11. 原生 HTML 块
            If t.StartsWith("<") AndAlso
               Regex.IsMatch(t, "^<[a-zA-Z!]") AndAlso
               Not Regex.IsMatch(t, "^<[a-zA-Z][a-zA-Z0-9+.\-]*://") Then

                Dim htmlLines As New List(Of String)
                While i < n AndAlso Not lines(i).StringEmpty
                    htmlLines.Add(lines(i))
                    i += 1
                End While
                blocks.Add(New Block With {.type = "html", .content = htmlLines.JoinBy(vbLf)})
                Continue While
            End If

            ' 12. 段落（可能整行仅为图片/链接）
            Dim para As New List(Of String)
            While i < n
                Dim l = lines(i)
                Dim lt = l.Trim(" "c, ASCII.TAB, vbCr, vbLf)
                If lt.StringEmpty Then
                    Exit While
                End If
                If IsBlockStart(l, lines, i) Then
                    Exit While
                End If
                para.Add(l.Trim(" "c, ASCII.TAB, vbCr, vbLf))
                i += 1
            End While

            Dim paraText = para.JoinBy(" ")
            Dim linkCheck = MatchLink(paraText, 0)

            If linkCheck.matched AndAlso Not linkCheck.isRef AndAlso linkCheck.endIndex = paraText.Length Then
                If paraText.StartsWith("!") Then
                    blocks.Add(New Block With {
                        .type = "image",
                        .url = linkCheck.url,
                        .alt = linkCheck.alt,
                        .title = linkCheck.title
                    })
                Else
                    blocks.Add(New Block With {
                        .type = "link",
                        .url = linkCheck.url,
                        .alt = linkCheck.text,
                        .title = linkCheck.title
                    })
                End If
            Else
                blocks.Add(New Block With {.type = "paragraph", .content = ParseInlinePlain(paraText)})
            End If
        End While

        Return blocks
    End Function

    ''' <summary>
    ''' 将单个列表项的正文（可能含多段/嵌套）渲染为该项的纯文本描述。
    ''' </summary>
    Private Function RenderItemText(raw As String) As String
        Dim inner = Parse(raw)

        ' 单项内容只有一个行内块时，直接输出纯文本：列表项在 Block 模型中的约定是
        ' 纯文本，若先解析成 link/image 块再 ToMarkdownBlock 回来，项文本里会残留
        ' [text](url) 之类的 markdown 标记，导致 html/纯文本渲染器原样输出。
        If inner.Count = 1 Then
            Select Case inner(0).type
                Case "paragraph", "link", "image"
                    Return ParseInlinePlain(raw)
            End Select
        End If

        Return inner _
            .Select(Function(b) b.ToMarkdownBlock) _
            .JoinBy(vbCrLf)
    End Function

    ''' <summary>
    ''' 判断 <paramref name="i"/> 处是否开启了新的块，用于让段落收集在遇到块边界时停止。
    ''' </summary>
    Private Function IsBlockStart(line As String, lines As String(), i As Integer) As Boolean
        Dim t = line.Trim(" "c, ASCII.TAB, vbCr, vbLf)
        If t.StringEmpty Then
            Return False
        End If
        If MatchFence(line).match Then
            Return True
        End If
        If t.Trim() = "$$" Then
            Return True
        End If
        If Regex.IsMatch(t, "^(#{1,6})\s+") Then
            Return True
        End If
        If t.StartsWith(">") Then
            Return True
        End If
        If MatchListItem(line).match Then
            Return True
        End If
        If Regex.IsMatch(t, "^\[\^") Then
            Return True
        End If
        If t.StartsWith("<") AndAlso
           Regex.IsMatch(t, "^<[a-zA-Z!]") AndAlso
           Not Regex.IsMatch(t, "^<[a-zA-Z][a-zA-Z0-9+.\-]*://") Then
            Return True
        End If
        If IsTableStart(lines, i) Then
            Return True
        End If
        If Regex.IsMatch(t, "^\s*([-*_])(\s*\1){2,}\s*$") Then
            Return True
        End If
        If i + 1 < lines.Length Then
            Dim nxt = lines(i + 1).Trim(" "c, ASCII.TAB, vbCr, vbLf)
            If Regex.IsMatch(nxt, "^=+\s*$") OrElse Regex.IsMatch(nxt, "^-{2,}\s*$") Then
                Return True
            End If
        End If
        Return False
    End Function

    ' ----- 围栏代码块 -----

    Private Structure FenceInfo
        Dim match As Boolean
        Dim ch As Char
        Dim length As Integer
        Dim lang As String
    End Structure

    Private Function MatchFence(line As String) As FenceInfo
        Dim m = Regex.Match(line, "^( {0,3})(`{3,}|~{3,})(.*)$")
        If m.Success Then
            Dim fenc = m.Groups(2).Value
            Return New FenceInfo With {
                .match = True,
                .ch = fenc(0),
                .length = fenc.Length,
                .lang = m.Groups(3).Value.Trim()
            }
        End If
        Return New FenceInfo With {.match = False}
    End Function

    Private Function CollectFence(lines() As String, start As Integer, fence As FenceInfo, ByRef code As String, ByRef lang As String) As Integer
        lang = fence.lang
        Dim content As New List(Of String)
        Dim i = start + 1
        While i < lines.Length
            Dim l = lines(i)
            Dim m = Regex.Match(l, "^( {0,3})(`{3,}|~{3,})\s*$")
            If m.Success AndAlso m.Groups(2).Value(0) = fence.ch AndAlso m.Groups(2).Value.Length >= fence.length Then
                Exit While
            End If
            content.Add(l)
            i += 1
        End While
        code = content.JoinBy(vbLf)
        Return i
    End Function

    ' ----- 引用块 -----

    Private Function CollectBlockQuote(lines() As String, start As Integer, ByRef inner As String()) As Integer
        Dim content As New List(Of String)
        Dim i = start
        While i < lines.Length
            Dim l = lines(i)
            Dim t = l.Trim(" "c, ASCII.TAB, vbCr, vbLf)
            If t.StartsWith(">") Then
                Dim rest = t.Substring(1)
                If rest.StartsWith(" ") Then
                    rest = rest.Substring(1)
                End If
                content.Add(rest)
                i += 1
            ElseIf l.StringEmpty Then
                Dim j = i + 1
                While j < lines.Length AndAlso lines(j).StringEmpty
                    j += 1
                End While
                If j < lines.Length AndAlso lines(j).Trim(" "c, ASCII.TAB, vbCr, vbLf).StartsWith(">") Then
                    content.Add(l)
                    i += 1
                Else
                    Exit While
                End If
            Else
                If content.Count > 0 AndAlso Not IsBlockStart(l, lines, i) Then
                    content.Add(l)
                    i += 1
                Else
                    Exit While
                End If
            End If
        End While
        inner = content.ToArray()
        Return i - 1
    End Function

    ' ----- 列表 -----

    Private Structure ListInfo
        Dim match As Boolean
        Dim indent As Integer
        Dim ordered As Boolean
        Dim startNumber As Integer
        Dim contentColumn As Integer
    End Structure

    Private Function MatchListItem(line As String) As ListInfo
        Dim m = Regex.Match(line, "^( *)([-+*]|\d+[.])[ \t]+")
        If m.Success Then
            Dim marker = m.Groups(2).Value
            Dim info As New ListInfo
            info.match = True
            info.indent = m.Groups(1).Value.Length
            info.ordered = Char.IsDigit(marker(0))
            If info.ordered Then
                Integer.TryParse(marker.TrimEnd("."c), info.startNumber)
            Else
                info.startNumber = 1
            End If
            info.contentColumn = m.Length
            Return info
        End If
        Return New ListInfo With {.match = False}
    End Function

    ''' <summary>
    ''' 收集同缩进层级的所有列表项原始正文（含续行与更深缩进的嵌套，扁平化为文本）。
    ''' </summary>
    Private Function CollectListItems(lines() As String, start As Integer, ByRef items As List(Of String), first As ListInfo) As Integer
        Dim indent = first.indent
        Dim i = start
        Dim curItem As New List(Of String)
        Dim haveItem = False

        While i < lines.Length
            Dim l = lines(i)
            Dim lt = l.Trim(" "c, ASCII.TAB, vbCr, vbLf)

            If lt.StringEmpty Then
                Dim j = i + 1
                While j < lines.Length AndAlso lines(j).StringEmpty
                    j += 1
                End While
                If j < lines.Length Then
                    Dim nxt = lines(j)
                    Dim nxtInfo = MatchListItem(nxt)
                    If nxtInfo.match AndAlso nxtInfo.indent > indent Then
                        ' 更深缩进的嵌套列表属于当前列表项
                        curItem.Add(l)
                        i += 1
                        Continue While
                    ElseIf nxtInfo.match AndAlso nxtInfo.indent = indent Then
                        ' 空行后的同级列表项 => 当前列表结束。
                        ' 当前项交由循环结束后的统一收尾代码添加，避免重复计入。
                        Exit While
                    ElseIf GetIndent(nxt) >= first.contentColumn Then
                        curItem.Add(l)
                        i += 1
                        Continue While
                    Else
                        Exit While
                    End If
                Else
                    Exit While
                End If
            End If

            Dim info = MatchListItem(l)
            If info.match AndAlso info.indent = indent Then
                If haveItem Then
                    items.Add(curItem.JoinBy(vbLf))
                End If
                curItem = New List(Of String)
                curItem.Add(l.Substring(std.Min(info.contentColumn, l.Length)))
                haveItem = True
                i += 1
            ElseIf info.match AndAlso info.indent > indent Then
                curItem.Add(l)
                i += 1
            ElseIf GetIndent(l) >= first.contentColumn Then
                If haveItem Then
                    curItem.Add(l.Substring(std.Min(first.contentColumn, l.Length)))
                End If
                i += 1
            Else
                Exit While
            End If
        End While

        If haveItem Then
            items.Add(curItem.JoinBy(vbLf))
        End If

        Return i - 1
    End Function

    Private Function GetIndent(line As String) As Integer
        Dim k = 0
        While k < line.Length AndAlso (line(k) = " "c OrElse line(k) = ASCII.TAB)
            k += 1
        End While
        Return k
    End Function

    ' ----- 表格 -----

    Private Function IsTableStart(lines As String(), i As Integer) As Boolean
        If i + 1 >= lines.Length Then
            Return False
        End If
        Dim header = lines(i).Trim(" "c, ASCII.TAB, vbCr, vbLf)
        Dim sep = lines(i + 1).Trim(" "c, ASCII.TAB, vbCr, vbLf)
        If Not header.Contains("|") Then
            Return False
        End If
        If Not sep.Contains("|") Then
            Return False
        End If
        Dim s2 = sep.Trim("|"c).Trim(" "c, ASCII.TAB)
        Return Regex.IsMatch(s2, "^:?-{1,}:?(\s*\|\s*:?-{1,}:?\s*)*$")
    End Function

    Private Function SplitRow(row As String) As String()
        Dim cells As New List(Of String)
        Dim cur As New StringBuilder
        Dim k = 0
        While k < row.Length
            Dim c = row(k)
            If c = "\"c AndAlso k + 1 < row.Length AndAlso row(k + 1) = "|"c Then
                cur.Append("|"c)
                k += 2
                Continue While
            End If
            If c = "|"c Then
                cells.Add(cur.ToString)
                cur.Clear()
                k += 1
                Continue While
            End If
            cur.Append(c)
            k += 1
        End While
        cells.Add(cur.ToString)
        Return cells.ToArray
    End Function

    Private Function ParseAligns(cells As String()) As String()
        Dim out(cells.Length - 1) As String
        For idx = 0 To cells.Length - 1
            Dim c = cells(idx).Trim()
            If c.StartsWith(":") AndAlso c.EndsWith(":") Then
                out(idx) = "center"
            ElseIf c.EndsWith(":") Then
                out(idx) = "right"
            ElseIf c.StartsWith(":") Then
                out(idx) = "left"
            Else
                out(idx) = "none"
            End If
        Next
        Return out
    End Function

    ' =====================================================================
    ' 行内解析（纯文本输出）
    ' =====================================================================

    Private Function ParseInlinePlain(text As String) As String
        If String.IsNullOrEmpty(text) Then
            Return ""
        End If

        Dim sb As New StringBuilder
        Dim i = 0
        Dim n = text.Length

        While i < n
            Dim c = text(i)

            ' 反斜杠转义
            If c = "\"c AndAlso i + 1 < n AndAlso "*_[]()~`<>#+-.!".Contains(text(i + 1)) Then
                sb.Append(text(i + 1))
                i += 2
                Continue While
            End If

            ' 行内代码段
            If c = "`"c Then
                Dim cs = MatchCodeSpan(text, i)
                If cs.matched Then
                    sb.Append(cs.content)
                    i = cs.endIndex
                    Continue While
                End If
            End If

            ' 图片 ![alt](url "title")
            If c = "!"c AndAlso i + 1 < n AndAlso text(i + 1) = "["c Then
                Dim img = MatchLink(text, i)
                If img.matched AndAlso Not img.isRef Then
                    sb.Append(ParseInlinePlain(img.alt))
                    i = img.endIndex
                    Continue While
                End If
            End If

            ' 链接 [text](url "title")
            If c = "["c Then
                Dim link = MatchLink(text, i)
                If link.matched AndAlso Not link.isRef Then
                    sb.Append(ParseInlinePlain(link.text))
                    i = link.endIndex
                    Continue While
                ElseIf link.matched AndAlso link.isRef Then
                    sb.Append(link.raw)
                    i = link.endIndex
                    Continue While
                End If
            End If

            ' 自动链接 <url>
            If c = "<"c Then
                Dim al = MatchAutoLink(text, i)
                If al.matched Then
                    sb.Append(al.url)
                    i = al.endIndex
                    Continue While
                End If
            End If

            ' 粗体 / 斜体（去标记，保留内层纯文本）
            If c = "*"c Then
                Dim em = MatchEmphasis(text, i, "*"c)
                If em.matched Then
                    sb.Append(ParseInlinePlain(em.inner))
                    i = em.endIndex
                    Continue While
                End If
            End If

            If c = "_"c Then
                Dim em = MatchEmphasis(text, i, "_"c)
                If em.matched Then
                    sb.Append(ParseInlinePlain(em.inner))
                    i = em.endIndex
                    Continue While
                End If
            End If

            ' 删除线 ~~
            If c = "~"c AndAlso i + 1 < n AndAlso text(i + 1) = "~"c Then
                Dim st = MatchStrike(text, i)
                If st.matched Then
                    sb.Append(ParseInlinePlain(st.content))
                    i = st.endIndex
                    Continue While
                End If
            End If

            ' 裸链接（http/https/ftp）
            If c = "h"c OrElse c = "f"c Then
                Dim bu = MatchBareUrl(text, i)
                If bu.matched Then
                    sb.Append(bu.content)
                    i = bu.endIndex
                    Continue While
                End If
            End If

            sb.Append(c)
            i += 1
        End While

        Return sb.ToString
    End Function

    Private Function MatchCodeSpan(text As String, i As Integer) As SpanInfo
        Dim k = i
        While k < text.Length AndAlso text(k) = "`"c
            k += 1
        End While
        Dim tickLen = k - i
        Dim j = k
        While j < text.Length
            If text(j) = "`"c Then
                Dim m = j
                While m < text.Length AndAlso text(m) = "`"c
                    m += 1
                End While
                If m - j = tickLen Then
                    Dim content = text.Substring(k, j - k)
                    Return New SpanInfo With {
                        .matched = True,
                        .content = CollapseCode(content),
                        .endIndex = m
                    }
                End If
                j = m
            Else
                j += 1
            End If
        End While
        Return New SpanInfo With {.matched = False}
    End Function

    Private Function CollapseCode(s As String) As String
        If s.Length >= 2 AndAlso s(0) = " "c AndAlso s(s.Length - 1) = " "c Then
            If s.IndexOf(" "c) < s.Length - 1 Then
                Return s.Substring(1, s.Length - 2)
            End If
        End If
        Return s
    End Function

    Private Structure SpanInfo
        Dim matched As Boolean
        Dim content As String
        Dim endIndex As Integer
    End Structure

    Private Structure LinkInfo
        Dim matched As Boolean
        Dim isRef As Boolean
        Dim text As String
        Dim alt As String
        Dim url As String
        Dim title As String
        Dim raw As String
        Dim endIndex As Integer
    End Structure

    Private Function MatchLink(text As String, i As Integer) As LinkInfo
        Dim isImage = (i < text.Length AndAlso text(i) = "!"c)
        Dim openBracket = If(isImage, i + 1, i)
        If openBracket >= text.Length OrElse text(openBracket) <> "["c Then
            Return NoLink()
        End If

        Dim close = text.IndexOf("]", openBracket + 1)
        If close = -1 Then
            Return NoLink()
        End If

        Dim labelText = text.Substring(openBracket + 1, close - (openBracket + 1))

        If close + 1 < text.Length AndAlso text(close + 1) = "("c Then
            Dim endp = FindParenClose(text, close + 1)
            If endp = -1 Then
                Return NoLink()
            End If
            Dim dest = text.Substring(close + 2, endp - (close + 2)).Trim()
            Dim url = dest
            Dim title = ""
            Dim sp = dest.LastIndexOf(" """)
            If sp > 0 AndAlso dest.EndsWith("""") Then
                url = dest.Substring(0, sp).Trim().Trim("<"c, ">"c)
                title = dest.Substring(sp + 2).Trim(""""c).Trim()
            Else
                url = dest.Trim("<"c, ">"c)
            End If
            If isImage Then
                Return New LinkInfo With {.matched = True, .isRef = False, .alt = labelText, .url = url, .title = title, .endIndex = endp + 1}
            Else
                Return New LinkInfo With {.matched = True, .isRef = False, .text = labelText, .url = url, .title = title, .endIndex = endp + 1}
            End If
        End If

        If close + 1 < text.Length AndAlso text(close + 1) = "["c Then
            Dim close2 = text.IndexOf("]", close + 1)
            If close2 = -1 Then
                Return NoLink()
            End If
            Dim raw = text.Substring(i, close2 + 1 - i)
            Return New LinkInfo With {.matched = True, .isRef = True, .raw = raw, .endIndex = close2 + 1}
        End If

        Return NoLink()
    End Function

    Private Function NoLink() As LinkInfo
        Return New LinkInfo With {.matched = False}
    End Function

    Private Function FindParenClose(text As String, openIndex As Integer) As Integer
        Dim depth = 0
        Dim k = openIndex
        While k < text.Length
            Dim ch = text(k)
            If ch = "("c Then
                depth += 1
            ElseIf ch = ")"c Then
                depth -= 1
                If depth = 0 Then
                    Return k
                End If
            ElseIf ch = """"c Then
                k += 1
                While k < text.Length AndAlso text(k) <> """"c
                    k += 1
                End While
            End If
            k += 1
        End While
        Return -1
    End Function

    Private Structure AutoLinkInfo
        Dim matched As Boolean
        Dim url As String
        Dim endIndex As Integer
    End Structure

    Private Function MatchAutoLink(text As String, i As Integer) As AutoLinkInfo
        Dim close = text.IndexOf(">", i + 1)
        If close = -1 Then
            Return New AutoLinkInfo With {.matched = False}
        End If
        Dim inner = text.Substring(i + 1, close - (i + 1))
        If Regex.IsMatch(inner, "^[a-zA-Z][a-zA-Z0-9+.\-]*://") OrElse Regex.IsMatch(inner, "^[^@\s]+@[^@\s]+\.[^@\s]+$") Then
            Return New AutoLinkInfo With {.matched = True, .url = inner, .endIndex = close + 1}
        End If
        Return New AutoLinkInfo With {.matched = False}
    End Function

    Private Structure EmphasisInfo
        Dim matched As Boolean
        Dim kind As String
        Dim inner As String
        Dim endIndex As Integer
    End Structure

    Private Function MatchEmphasis(text As String, i As Integer, delim As Char) As EmphasisInfo
        Dim n = text.Length

        If i + 2 < n AndAlso text(i + 1) = delim AndAlso text(i + 2) = delim Then
            Dim close = IndexOfDelim(text, i + 3, delim, 3)
            If close >= 0 Then
                Dim inner = text.Substring(i + 3, close - (i + 3))
                If inner.Length > 0 Then
                    Return New EmphasisInfo With {.matched = True, .kind = "bolditalic", .inner = inner, .endIndex = close + 3}
                End If
            End If
        End If

        If i + 1 < n AndAlso text(i + 1) = delim Then
            Dim close = IndexOfDelim(text, i + 2, delim, 2)
            If close >= 0 Then
                Dim inner = text.Substring(i + 2, close - (i + 2))
                If inner.Length > 0 Then
                    Return New EmphasisInfo With {.matched = True, .kind = "bold", .inner = inner, .endIndex = close + 2}
                End If
            End If
        End If

        If delim = "_"c AndAlso Not UnderscoreDelimiter(text, i) Then
            Return New EmphasisInfo With {.matched = False}
        End If

        Dim close2 = IndexOfDelim(text, i + 1, delim, 1)
        If close2 >= 0 Then
            If delim = "_"c AndAlso Not UnderscoreDelimiter(text, close2) Then
                Return New EmphasisInfo With {.matched = False}
            End If
            Dim inner = text.Substring(i + 1, close2 - (i + 1))
            If inner.Length > 0 Then
                Return New EmphasisInfo With {.matched = True, .kind = "italic", .inner = inner, .endIndex = close2 + 1}
            End If
        End If

        Return New EmphasisInfo With {.matched = False}
    End Function

    Private Function IndexOfDelim(text As String, from As Integer, delim As Char, count As Integer) As Integer
        Dim k = from
        While k + count - 1 < text.Length
            Dim ok = True
            For c As Integer = 0 To count - 1
                If text(k + c) <> delim Then
                    ok = False
                    Exit For
                End If
            Next
            If ok Then
                Return k
            End If
            k += 1
        End While
        Return -1
    End Function

    Private Function UnderscoreDelimiter(text As String, pos As Integer) As Boolean
        Dim before = If(pos > 0, text(pos - 1), " "c)
        Dim after = If(pos + 1 < text.Length, text(pos + 1), " "c)
        Dim bw = IsWord(before)
        Dim aw = IsWord(after)
        Return Not (bw AndAlso aw)
    End Function

    Private Function IsWord(c As Char) As Boolean
        Return Char.IsLetterOrDigit(c) OrElse c = "_"c
    End Function

    Private Function MatchStrike(text As String, i As Integer) As SpanInfo
        Dim close = text.IndexOf("~~", i + 2)
        If close >= 0 Then
            Dim inner = text.Substring(i + 2, close - (i + 2))
            If inner.Length > 0 Then
                Return New SpanInfo With {.matched = True, .content = inner, .endIndex = close + 2}
            End If
        End If
        Return New SpanInfo With {.matched = False}
    End Function

    Private Function MatchBareUrl(text As String, i As Integer) As SpanInfo
        Dim m = Regex.Match(text.Substring(i), "^(?:https?|ftp)://[^\s<>""')]+")
        If m.Success Then
            Dim url = m.Value
            While url.Length > 0 AndAlso ".),".Contains(url(url.Length - 1))
                url = url.Substring(0, url.Length - 1)
            End While
            Return New SpanInfo With {.matched = True, .content = url, .endIndex = i + url.Length}
        End If
        Return New SpanInfo With {.matched = False}
    End Function

End Class


