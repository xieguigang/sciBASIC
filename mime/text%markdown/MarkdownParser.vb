Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports ASCII = Microsoft.VisualBasic.Text.ASCII
Imports std = System.Math

''' <summary>
''' A two-phase recursive Markdown parser: block-level tokenization followed by
''' inline parsing. Nested constructs (lists inside lists, block quotes containing
''' lists/headers, tables with inline formatting) are handled naturally through
''' recursion instead of the previous flat, document-wide regular expression
''' replacement pipeline.
''' </summary>
Public Class MarkdownParser

    Private ReadOnly render As Render

    Sub New(render As Render)
        Me.render = render
    End Sub

    ''' <summary>
    ''' Parse a full markdown document into the target format (html by default).
    ''' </summary>
    Public Function Parse(markdown As String) As String
        If String.IsNullOrEmpty(markdown) Then
            Return ""
        End If

        Dim text = markdown.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
        Dim lines = text.LineTokens

        Return ParseBlocks(lines).Trim(vbLf, vbCr, " "c)
    End Function

    ' =====================================================================
    ' Block level parsing
    ' =====================================================================

    Private Function ParseBlocks(lines() As String) As String
        Dim sb As New StringBuilder
        Dim i = 0
        Dim n = lines.Length

        While i < n
            Dim line = lines(i)
            Dim t = line.Trim(" "c, ASCII.TAB, vbCr, vbLf)

            If t.StringEmpty Then
                i += 1
                Continue While
            End If

            ' 1. fenced code block (``` or ~~~)
            Dim fence = MatchFence(line)
            If fence.match Then
                Dim code As String = ""
                Dim lang As String = ""
                Dim endIdx = CollectFence(lines, i, fence, code, lang)
                sb.Append(render.CodeBlock(code, lang)).Append(vbLf).Append(vbLf)
                i = endIdx + 1
                Continue While
            End If

            ' 2. ATX header (# .. ######)
            Dim hm = Regex.Match(t, "^(#{1,6})\s+(.*?)\s*#*\s*$")
            If hm.Success Then
                sb _
                    .Append(render.Header(ParseInline(hm.Groups(2).Value), hm.Groups(1).Value.Length)) _
                    .Append(vbLf).Append(vbLf)
                i += 1
                Continue While
            End If

            ' 3. block quote (>)
            If t.StartsWith(">") Then
                Dim inner As String() = Nothing
                Dim endIdx = CollectBlockQuote(lines, i, inner)
                sb _
                    .Append(render.BlockQuote(ParseBlocks(inner))) _
                    .Append(vbLf).Append(vbLf)
                i = endIdx + 1
                Continue While
            End If

            ' 4. list (ordered / unordered, nested)
            Dim li = MatchListItem(line)
            If li.match Then
                Dim html As String = ""
                Dim endIdx = CollectList(lines, i, html)
                sb.Append(html).Append(vbLf).Append(vbLf)
                i = endIdx + 1
                Continue While
            End If

            ' 5. table
            If IsTableStart(lines, i) Then
                Dim html As String = ""
                Dim endIdx = CollectTable(lines, i, html)
                sb.Append(html).Append(vbLf).Append(vbLf)
                i = endIdx + 1
                Continue While
            End If

            ' 6. setext header (underline of = or -)
            If i + 1 < n Then
                Dim nxt = lines(i + 1).Trim(" "c, ASCII.TAB, vbCr, vbLf)
                If Regex.IsMatch(nxt, "^=+\s*$") Then
                    sb.Append(render.Header(ParseInline(t), 1)).Append(vbLf).Append(vbLf)
                    i += 2
                    Continue While
                End If
                If Regex.IsMatch(nxt, "^-{2,}\s*$") Then
                    sb.Append(render.Header(ParseInline(t), 2)).Append(vbLf).Append(vbLf)
                    i += 2
                    Continue While
                End If
            End If

            ' 7. horizontal rule
            If Regex.IsMatch(t, "^\s*([-*_])(\s*\1){2,}\s*$") Then
                sb.Append(render.HorizontalLine()).Append(vbLf).Append(vbLf)
                i += 1
                Continue While
            End If

            ' 8. paragraph
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
            sb _
                .Append(render.Paragraph(ParseInline(paraText), True)) _
                .Append(vbLf).Append(vbLf)
        End While

        Return sb.ToString
    End Function

    ''' <summary>
    ''' Detect whether the line at <paramref name="i"/> starts a new block so that
    ''' paragraph collection can stop before it.
    ''' </summary>
    Private Function IsBlockStart(line As String, lines As String(), i As Integer) As Boolean
        Dim t = line.Trim(" "c, ASCII.TAB, vbCr, vbLf)
        If t.StringEmpty Then
            Return False
        End If
        If MatchFence(line).match Then
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

    ' ----- fenced code block -----

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

    ' ----- block quote -----

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
                ' keep a blank line only when the next non-blank line continues the quote
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
                ' lazy continuation of the quote
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

    ' ----- list -----

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

    Private Function CollectList(lines() As String, start As Integer, ByRef html As String) As Integer
        Dim first = MatchListItem(lines(start))
        Dim indent = first.indent
        Dim ordered = first.ordered
        Dim startNumber = first.startNumber

        Dim items As New List(Of String)
        Dim i = start

        While i < lines.Length
            Dim li = MatchListItem(lines(i))
            If Not li.match OrElse li.indent <> indent Then
                Exit While
            End If

            Dim itemLines As New List(Of String)
            Dim rest = lines(i).Substring(std.Min(li.contentColumn, lines(i).Length))
            itemLines.Add(rest)
            i += 1

            ' collect item body (continuation lines + nested blocks)
            While i < lines.Length
                Dim l = lines(i)
                If l.StringEmpty Then
                    Dim j = i + 1
                    While j < lines.Length AndAlso lines(j).StringEmpty
                        j += 1
                    End While
                    If j < lines.Length Then
                        Dim nxt = lines(j)
                        Dim nxtInfo = MatchListItem(nxt)
                        If nxtInfo.match AndAlso nxtInfo.indent = indent Then
                            Exit While
                        ElseIf GetIndent(nxt) >= li.contentColumn OrElse (nxtInfo.match AndAlso nxtInfo.indent > indent) Then
                            itemLines.Add(l)
                            i += 1
                            Continue While
                        Else
                            Exit While
                        End If
                    Else
                        Exit While
                    End If
                Else
                    Dim ind = GetIndent(l)
                    If ind >= li.contentColumn Then
                        itemLines.Add(l)
                        i += 1
                    ElseIf MatchListItem(l).match AndAlso MatchListItem(l).indent = indent Then
                        Exit While
                    Else
                        Exit While
                    End If
                End If
            End While

            ' strip the item indentation and recursively parse the item content
            Dim stripped As New List(Of String)
            For Each ln In itemLines
                If ln.StringEmpty Then
                    stripped.Add(ln)
                ElseIf GetIndent(ln) >= li.contentColumn Then
                    stripped.Add(ln.Substring(li.contentColumn))
                Else
                    stripped.Add(ln)
                End If
            Next

            Dim itemHtml = ParseBlocks(stripped.ToArray).Trim(vbLf, vbCr, " "c)
            itemHtml = TightenListItem(itemHtml)
            items.Add(itemHtml)
        End While

        html = render.List(items, ordered, startNumber)
        Return i - 1
    End Function

    ''' <summary>
    ''' For a single-paragraph list item, drop the wrapping &lt;p&gt; tag so that
    ''' tight lists are rendered without unnecessary paragraph elements.
    ''' </summary>
    Private Function TightenListItem(html As String) As String
        Dim t = html.Trim(vbLf, vbCr, " "c)
        If t.StartsWith("<p>") AndAlso t.EndsWith("</p>") Then
            Dim inner = t.Substring(3, t.Length - 7)
            If inner.IndexOf("<p", StringComparison.OrdinalIgnoreCase) = -1 AndAlso
               inner.IndexOf("<ul", StringComparison.OrdinalIgnoreCase) = -1 AndAlso
               inner.IndexOf("<ol", StringComparison.OrdinalIgnoreCase) = -1 AndAlso
               inner.IndexOf("<blockquote", StringComparison.OrdinalIgnoreCase) = -1 AndAlso
               inner.IndexOf("<table", StringComparison.OrdinalIgnoreCase) = -1 AndAlso
               inner.IndexOf("<pre", StringComparison.OrdinalIgnoreCase) = -1 AndAlso
               inner.IndexOf("<h1", StringComparison.OrdinalIgnoreCase) = -1 AndAlso
               inner.IndexOf("<h2", StringComparison.OrdinalIgnoreCase) = -1 AndAlso
               inner.IndexOf("<h3", StringComparison.OrdinalIgnoreCase) = -1 AndAlso
               inner.IndexOf("<h4", StringComparison.OrdinalIgnoreCase) = -1 AndAlso
               inner.IndexOf("<h5", StringComparison.OrdinalIgnoreCase) = -1 AndAlso
               inner.IndexOf("<h6", StringComparison.OrdinalIgnoreCase) = -1 Then
                Return inner
            End If
        End If
        Return html
    End Function

    Private Function GetIndent(line As String) As Integer
        Dim k = 0
        While k < line.Length AndAlso (line(k) = " "c OrElse line(k) = ASCII.TAB)
            k += 1
        End While
        Return k
    End Function

    ' ----- table -----

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

    Private Function CollectTable(lines() As String, start As Integer, ByRef html As String) As Integer
        Dim headerCells = SplitRow(lines(start).Trim("|"c).Trim(" "c, ASCII.TAB))
        Dim sepCells = SplitRow(lines(start + 1).Trim("|"c).Trim(" "c, ASCII.TAB))
        Dim aligns = ParseAligns(sepCells)

        Dim rows As New List(Of String())
        Dim i = start + 2
        While i < lines.Length
            Dim l = lines(i).Trim(" "c, ASCII.TAB, vbCr, vbLf)
            If l.StringEmpty Then
                Exit While
            End If
            If Not l.Contains("|") Then
                Exit While
            End If
            If IsBlockStart(l, lines, i) Then
                Exit While
            End If
            Dim cells = SplitRow(l.Trim("|"c).Trim(" "c, ASCII.TAB))
            Dim rendered = cells.Select(Function(c) ParseInline(c.Trim())).ToArray
            rows.Add(rendered)
            i += 1
        End While

        Dim head = headerCells.Select(Function(c) ParseInline(c.Trim())).ToArray
        html = render.Table(head, rows, aligns)
        Return i - 1
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
    ' Inline parsing (cursor based, no greedy document-wide regex)
    ' =====================================================================

    Private Function ParseInline(text As String) As String
        If String.IsNullOrEmpty(text) Then
            Return ""
        End If

        Dim sb As New StringBuilder
        Dim plain As New StringBuilder
        Dim i = 0
        Dim n = text.Length

        While i < n
            Dim c = text(i)

            ' backslash escape
            If c = "\"c AndAlso i + 1 < n AndAlso "*_[]()~`<>#+-.!".Contains(text(i + 1)) Then
                Flush(sb, plain)
                sb.Append(EscapeText(text(i + 1).ToString))
                i += 2
                Continue While
            End If

            ' inline code span
            If c = "`"c Then
                Dim cs = MatchCodeSpan(text, i)
                If cs.matched Then
                    Flush(sb, plain)
                    sb.Append(render.CodeSpan(cs.content))
                    i = cs.endIndex
                    Continue While
                End If
            End If

            ' image ![alt](url "title")
            If c = "!"c AndAlso i + 1 < n AndAlso text(i + 1) = "["c Then
                Dim img = MatchLink(text, i)
                If img.matched AndAlso Not img.isRef Then
                    Flush(sb, plain)
                    sb.Append(render.Image(img.url, img.alt, img.title))
                    i = img.endIndex
                    Continue While
                End If
            End If

            ' link [text](url "title")
            If c = "["c Then
                Dim link = MatchLink(text, i)
                If link.matched AndAlso Not link.isRef Then
                    Flush(sb, plain)
                    sb.Append(render.AnchorLink(link.url, ParseInline(link.text), link.title))
                    i = link.endIndex
                    Continue While
                ElseIf link.matched AndAlso link.isRef Then
                    Flush(sb, plain)
                    sb.Append(EscapeText(link.raw))
                    i = link.endIndex
                    Continue While
                End If
            End If

            ' autolink <url>
            If c = "<"c Then
                Dim al = MatchAutoLink(text, i)
                If al.matched Then
                    Flush(sb, plain)
                    sb.Append(render.AnchorLink(al.url, al.url, al.url))
                    i = al.endIndex
                    Continue While
                End If
            End If

            ' bold / italic with *
            If c = "*"c Then
                Dim em = MatchEmphasis(text, i, "*"c)
                If em.matched Then
                    Flush(sb, plain)
                    sb.Append(RenderEmphasis(em))
                    i = em.endIndex
                    Continue While
                End If
            End If

            ' bold / italic with _
            If c = "_"c Then
                Dim em = MatchEmphasis(text, i, "_"c)
                If em.matched Then
                    Flush(sb, plain)
                    sb.Append(RenderEmphasis(em))
                    i = em.endIndex
                    Continue While
                End If
            End If

            ' strikethrough ~~
            If c = "~"c AndAlso i + 1 < n AndAlso text(i + 1) = "~"c Then
                Dim st = MatchStrike(text, i)
                If st.matched Then
                    Flush(sb, plain)
                    sb.Append(render.Strikethrough(ParseInline(st.content)))
                    i = st.endIndex
                    Continue While
                End If
            End If

            ' bare url (http/https/ftp)
            If c = "h"c OrElse c = "f"c Then
                Dim bu = MatchBareUrl(text, i)
                If bu.matched Then
                    Flush(sb, plain)
                    sb.Append(render.AnchorLink(bu.content, bu.content, bu.content))
                    i = bu.endIndex
                    Continue While
                End If
            End If

            ' literal character
            plain.Append(c)
            i += 1
        End While

        Flush(sb, plain)
        Return sb.ToString
    End Function

    Private Sub Flush(sb As StringBuilder, plain As StringBuilder)
        If plain.Length > 0 Then
            sb.Append(EscapeText(plain.ToString))
            plain.Clear()
        End If
    End Sub

    Private Function RenderEmphasis(em As EmphasisInfo) As String
        If em.kind = "bolditalic" Then
            Return render.Bold(render.Italic(ParseInline(em.inner)))
        ElseIf em.kind = "bold" Then
            Return render.Bold(ParseInline(em.inner))
        Else
            Return render.Italic(ParseInline(em.inner))
        End If
    End Function

    Private Function EscapeText(s As String) As String
        Return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
    End Function

    ' ----- inline code span -----

    Private Structure SpanInfo
        Dim matched As Boolean
        Dim content As String
        Dim endIndex As Integer
    End Structure

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

    ' ----- links / images -----

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

    ' ----- emphasis -----

    Private Structure EmphasisInfo
        Dim matched As Boolean
        Dim kind As String
        Dim inner As String
        Dim endIndex As Integer
    End Structure

    Private Function MatchEmphasis(text As String, i As Integer, delim As Char) As EmphasisInfo
        Dim n = text.Length

        ' ***/___  bold + italic
        If i + 2 < n AndAlso text(i + 1) = delim AndAlso text(i + 2) = delim Then
            Dim close = IndexOfDelim(text, i + 3, delim, 3)
            If close >= 0 Then
                Dim inner = text.Substring(i + 3, close - (i + 3))
                If inner.Length > 0 Then
                    Return New EmphasisInfo With {.matched = True, .kind = "bolditalic", .inner = inner, .endIndex = close + 3}
                End If
            End If
        End If

        ' **/__  bold
        If i + 1 < n AndAlso text(i + 1) = delim Then
            Dim close = IndexOfDelim(text, i + 2, delim, 2)
            If close >= 0 Then
                Dim inner = text.Substring(i + 2, close - (i + 2))
                If inner.Length > 0 Then
                    Return New EmphasisInfo With {.matched = True, .kind = "bold", .inner = inner, .endIndex = close + 2}
                End If
            End If
        End If

        ' single _ only at a word boundary (so my_var is not italic)
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
        ' valid only when it is not surrounded by word characters on both sides
        Return Not (bw AndAlso aw)
    End Function

    Private Function IsWord(c As Char) As Boolean
        Return Char.IsLetterOrDigit(c) OrElse c = "_"c
    End Function

    ' ----- strikethrough -----

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

    ' ----- bare url -----

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
