Imports System.Text.RegularExpressions

''' <summary>
''' 从 markdown 源码中解析出的单个标题。
''' </summary>
''' <remarks>
''' <see cref="Index"/> 与渲染后的 ``&lt;h1&gt;``..``&lt;h6&gt;`` 元素顺序一一对应
''' （见 <see cref="MarkdownHeadings.Parse"/>），因此可以作为 HTML 侧锚点、
''' 目录条目或编辑器导航的索引使用。<see cref="LineStart"/> / <see cref="LineLength"/>
''' 记录标题在原始 markdown 文本中的位置，用于把导航反查到源码。
''' </remarks>
Public Class HeadingInfo

    ''' <summary>标题在文档中的序号（从 0 开始，与渲染后的 &lt;h*&gt; 顺序一致）。</summary>
    Public Property Index As Integer

    ''' <summary>标题层级（1-6）。</summary>
    Public Property Level As Integer

    ''' <summary>标题文本（已剥离行内强调 / 代码 / 删除线标记）。</summary>
    Public Property Text As String

    ''' <summary>标题所在行首在原始 markdown 文本中的字符偏移。</summary>
    Public Property LineStart As Integer

    ''' <summary>标题所在行的字符长度（不含换行符）。</summary>
    Public Property LineLength As Integer

End Class

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

        Dim text As String = markdown.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
        Dim lines As String() = text.Split(ControlChars.Lf)
        Dim offset As Integer = 0

        Dim inFence As Boolean = False
        Dim fenceChar As Char = " "c

        For i As Integer = 0 To lines.Length - 1
            Dim rawLine As String = lines(i)
            Dim trimmed As String = rawLine.Trim(" "c, ControlChars.Tab, ControlChars.Cr, ControlChars.Lf)

            ' 跟踪围栏代码块：围栏内部的 # 不会被渲染为标题，跳过以保持索引对齐
            Dim fence As Match = fenceMarker.Match(trimmed)

            If fence.Success Then
                If Not inFence Then
                    inFence = True
                    fenceChar = fence.Groups(2).Value(0)
                ElseIf fence.Groups(2).Value(0) = fenceChar Then
                    inFence = False
                End If

                offset += rawLine.Length + 1
                Continue For
            End If

            If inFence Then
                offset += rawLine.Length + 1
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
                    .LineStart = offset,
                    .LineLength = rawLine.Length
                })
            End If

            offset += rawLine.Length + 1
        Next

        Return headings
    End Function

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
