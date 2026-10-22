Imports System.Text.RegularExpressions

''' <summary>
''' markdown 标题扫描器：与 <see cref="MarkdownRender"/> 的渲染结果保持一致的
''' 标题解析实现，供目录生成、编辑器大纲与文档导航等场景复用。
''' </summary>
''' <remarks>
''' 解析规则：
''' <list type="bullet">
''' <item>ATX 标题：``#`` .. ``######`` 后跟空格与文本；</item>
''' <item>setext 标题：下一行为 ``===`` 或 ``---`` 的段落；</item>
''' <item>围栏代码块（``` 或 ~~~）内部的 ``#`` 不算标题；</item>
''' <item>引用块（``&gt;``）与列表项（``-`` / ``*`` / ``+`` / ``1.``）内部书写的标题
''' 仍会被识别，与渲染器的行为一致。</item>
''' </list>
''' </remarks>
Public Module MarkdownHeadings

    ''' <summary>ATX 标题（作用于单行文本）。</summary>
    Private ReadOnly atxHeader As New Regex("^(#{1,6})\s+(.*?)\s*#*\s*$")

    ''' <summary>setext 一级标题下划线。</summary>
    Private ReadOnly setextH1 As New Regex("^=+\s*$")

    ''' <summary>setext 二级标题下划线。</summary>
    Private ReadOnly setextH2 As New Regex("^-{2,}\s*$")

    ''' <summary>列表项标记（可重复嵌套）。</summary>
    Private ReadOnly listMarker As New Regex("^(\s*[-+*]|\s*\d+[.])[ \t]+")

    ''' <summary>围栏代码块起始/结束标记。</summary>
    Private ReadOnly fenceMarker As New Regex("^( {0,3})(`{3,}|~{3,})")

    ''' <summary>行内强调 / 代码 / 删除线分隔符与转义反斜杠。</summary>
    Private ReadOnly inlineMarkers As New Regex("\\|(\*\*\*|\*\*|__|~~|\*|`|_)")

    ''' <summary>
    ''' 扫描 markdown 源码中的标题，返回按文档顺序排列的标题列表。
    ''' </summary>
    ''' <param name="markdown">markdown 源文本。</param>
    ''' <returns>标题列表；没有标题时返回空列表。</returns>
    Public Function Parse(markdown As String) As List(Of HeadingInfo)
        Dim headings As New List(Of HeadingInfo)

        If String.IsNullOrEmpty(markdown) Then
            Return headings
        End If

        ' 按原始文本切行并记录每行的起始偏移：CRLF 文本若先归一化为 LF，累加得到的
        ' 偏移量会比真实位置每行少 1 个字符，导致调用方在源码中定位到错误的位置。
        Dim lines As String() = Nothing
        Dim lineStarts As Integer() = Nothing

        Call SplitLines(markdown, lines, lineStarts)

        Dim inFence As Boolean = False
        Dim fenceChar As Char = " "c

        For i As Integer = 0 To lines.Length - 1
            Dim rawLine As String = lines(i)
            Dim trimmed As String = rawLine.Trim(" "c, ControlChars.Tab)

            ' 跟踪围栏代码块：围栏内部的 # 不会被渲染为标题，跳过以保持索引对齐
            Dim fence As Match = fenceMarker.Match(trimmed)

            If fence.Success Then
                If Not inFence Then
                    inFence = True
                    fenceChar = fence.Groups(2).Value(0)
                ElseIf fence.Groups(2).Value(0) = fenceChar Then
                    inFence = False
                End If

                Continue For
            End If

            If inFence Then
                Continue For
            End If

            ' 剥离引用块前缀（"> " 可以重复），使引用块内的标题也能被识别
            Dim work As String = trimmed

            While work.StartsWith(">")
                work = work.Substring(1).TrimStart(" "c)
            End While

            ' 剥离列表项标记（可嵌套），使列表项内的标题也能被识别
            Dim marker As Match = listMarker.Match(work)

            While marker.Success
                work = work.Substring(marker.Length)
                marker = listMarker.Match(work)
            End While

            Dim level As Integer = 0
            Dim headingText As String = Nothing

            Dim atx As Match = atxHeader.Match(work)

            If atx.Success Then
                level = atx.Groups(1).Value.Length
                headingText = atx.Groups(2).Value.Trim()
            ElseIf i + 1 < lines.Length Then
                Dim nextLine As String = lines(i + 1).Trim(" "c, ControlChars.Tab, ControlChars.Cr, ControlChars.Lf)

                If setextH1.IsMatch(nextLine) Then
                    level = 1
                    headingText = work
                ElseIf setextH2.IsMatch(nextLine) Then
                    level = 2
                    headingText = work
                End If
            End If

            If level > 0 AndAlso Not String.IsNullOrWhiteSpace(headingText) Then
                Call headings.Add(New HeadingInfo With {
                    .Index = headings.Count,
                    .Level = level,
                    .Text = CleanHeadingText(headingText),
                    .LineStart = lineStarts(i),
                    .LineLength = rawLine.Length
                })
            End If
        Next

        Return headings
    End Function

    ''' <summary>
    ''' 按 CRLF / LF / CR 切分文本，并返回每行在原始文本中的起始偏移。
    ''' （<paramref name="text"/> 不做换行归一化，因此偏移量可以直接用于源码定位。）
    ''' </summary>
    ''' <param name="text">原始文本。</param>
    ''' <param name="lines">切分出的行（不含换行符）。</param>
    ''' <param name="starts">每行的起始字符偏移。</param>
    Private Sub SplitLines(text As String, ByRef lines As String(), ByRef starts As Integer())
        Dim lineList As New List(Of String)()
        Dim startList As New List(Of Integer)()
        Dim start As Integer = 0
        Dim pos As Integer = 0

        While pos < text.Length
            Dim c As Char = text(pos)

            If c = ControlChars.Cr OrElse c = ControlChars.Lf Then
                Call lineList.Add(text.Substring(start, pos - start))
                Call startList.Add(start)

                If c = ControlChars.Cr AndAlso pos + 1 < text.Length AndAlso text(pos + 1) = ControlChars.Lf Then
                    pos += 1
                End If

                pos += 1
                start = pos
            Else
                pos += 1
            End If
        End While

        If start <= text.Length Then
            Call lineList.Add(text.Substring(start, text.Length - start))
            Call startList.Add(start)
        End If

        lines = lineList.ToArray()
        starts = startList.ToArray()
    End Sub

    ''' <summary>
    ''' 剥离行内 markdown 强调 / 代码 / 删除线分隔符与转义反斜杠，得到可读的标题文本。
    ''' </summary>
    ''' <param name="raw">原始标题文本。</param>
    ''' <returns>清理后的标题文本。</returns>
    Public Function CleanHeadingText(raw As String) As String
        If String.IsNullOrEmpty(raw) Then
            Return ""
        End If

        Return inlineMarkers.Replace(raw, "").Trim()
    End Function

End Module
