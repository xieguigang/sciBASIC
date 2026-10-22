#Region "Microsoft.VisualBasic::be6bc3664c09ed7c03c3a3c97d9d65a8, tutorials\Marked\FormMarkWeb.vb"

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

'   Total Lines: 30
'    Code Lines: 23 (76.67%)
'    Comment Lines: 0 (0.00%)
'     - Xml Docs: 0.00%
' 
'   Blank Lines: 7 (23.33%)
'     File Size: 1.18 KB


' Class FormMarkWeb
' 
'     Sub: ToolStripButton1_Click
' 
' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ApplicationServices.Report
Imports Microsoft.VisualBasic.MIME.text.markdown
Imports Microsoft.Web.WebView2.Core

Public Class FormMarkWeb

    ReadOnly render As New MarkdownRender

    Private _headings As List(Of HeadingInfo)
    Private _loading As Boolean = False
    Private _navigating As Boolean = False
    Private _pendingScrollIndex As Integer = -1

    Private ReadOnly atxHeader As New Regex("^(#{1,6})\s+(.*?)\s*#*\s*$", RegexOptions.Multiline)
    Private ReadOnly headingTag As New Regex("<(h[1-6])>", RegexOptions.IgnoreCase)
    Private ReadOnly setextH1 As New Regex("^=+\s*$", RegexOptions.Multiline)
    Private ReadOnly setextH2 As New Regex("^-{2,}\s*$", RegexOptions.Multiline)
    Private ReadOnly listMarker As New Regex("^(\s*[-+*]|\s*\d+[.])[ \t]+")

    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        Using file As New OpenFileDialog With {.Filter = "Markdown 文档(*.md)|*.md"}
            If file.ShowDialog = DialogResult.OK Then
                Dim md As String = file.FileName.ReadAllText

                ' Suppress the TextChanged -> debounce chain while we load the file,
                ' then render once synchronously for an immediate result.
                _loading = True
                Call TextBox1.Clear()
                TextBox1.Text = md
                _loading = False

                Call Rebuild()
            End If
        End Using
    End Sub

    Private Async Sub FormMarkWeb_Load(sender As Object, e As EventArgs) Handles Me.Load
        Await WebViewLoader.Init(WebView21)

        If WebView21.CoreWebView2 IsNot Nothing Then
            AddHandler WebView21.CoreWebView2.NavigationCompleted, AddressOf OnNavigationCompleted
        End If
    End Sub

    ' ---------------------------------------------------------------------
    ' Heading parsing / html anchor injection / tree building
    ' ---------------------------------------------------------------------

    ''' <summary>
    ''' Scan the markdown source for ATX (``#``..``######``) and setext
    ''' (underline ``=`` / ``-``) headers, mirroring the rules used by
    ''' <see cref="MarkdownParser"/> so that the detected order matches the
    ''' order of ``&lt;h*&gt;`` elements produced by <see cref="MarkdownRender"/>.
    ''' </summary>
    Private Function ParseHeadings(md As String) As List(Of HeadingInfo)
        Dim headings As New List(Of HeadingInfo)
        If String.IsNullOrEmpty(md) Then
            Return headings
        End If

        Dim text = md.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
        Dim lines = text.Split(vbLf)
        Dim offset As Integer = 0

        Dim inFence As Boolean = False
        Dim fenceChar As Char = " "c

        For i As Integer = 0 To lines.Length - 1
            Dim rawLine = lines(i)
            Dim t = rawLine.Trim(" "c, vbTab, vbCr, vbLf)

            ' Track fenced code blocks (``` or ~~~); headers inside a fence are
            ' not rendered as <h*> by the parser, so skip them to keep the
            ' heading index aligned with the rendered HTML anchor order.
            Dim fm = Regex.Match(t, "^( {0,3})(`{3,}|~{3,})")
            If fm.Success Then
                If Not inFence Then
                    inFence = True
                    fenceChar = fm.Groups(2).Value(0)
                ElseIf fm.Groups(2).Value(0) = fenceChar Then
                    inFence = False
                End If
                offset += rawLine.Length + 1
                Continue For
            End If
            If inFence Then
                offset += rawLine.Length + 1
                Continue For
            End If

            ' Strip blockquote prefixes ("> " may be repeated) so that a heading
            ' nested inside a blockquote is still detected.
            Dim work = t
            While work.StartsWith(">")
                work = work.Substring(1).TrimStart(" "c)
            End While

            ' Strip list-item markers (including nesting) so a heading written
            ' inside a list item is detected the same way the parser renders it.
            Dim lm = listMarker.Match(work)
            While lm.Success
                work = work.Substring(lm.Length)
                lm = listMarker.Match(work)
            End While

            Dim level As Integer = 0
            Dim headingText As String = Nothing

            Dim atx = atxHeader.Match(work)
            If atx.Success Then
                level = atx.Groups(1).Value.Length
                headingText = atx.Groups(2).Value.Trim()
            ElseIf i + 1 < lines.Length Then
                Dim nxt = lines(i + 1).Trim(" "c, vbTab, vbCr, vbLf)
                If setextH1.IsMatch(nxt) Then
                    level = 1
                    headingText = work
                ElseIf setextH2.IsMatch(nxt) Then
                    level = 2
                    headingText = work
                End If
            End If

            If level > 0 AndAlso Not String.IsNullOrWhiteSpace(headingText) Then
                headings.Add(New HeadingInfo With {
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
    ''' Strip inline markdown emphasis / code / strikethrough delimiters so the
    ''' tree label is human readable.
    ''' </summary>
    Private Function CleanHeadingText(raw As String) As String
        Dim s = Regex.Replace(raw, "\\|(\*\*\*|\*\*|__|~~|\*|`|_)", "")
        Return s.Trim()
    End Function

    ''' <summary>
    ''' Inject a stable ``id="md-h{index}"`` anchor into every ``&lt;h*&gt;`` opening
    ''' tag, in document order, so the index matches <see cref="ParseHeadings"/>.
    ''' </summary>
    Private Function InjectHeadingIds(html As String) As String
        Dim counter As Integer = 0
        Return headingTag.Replace(html, Function(m)
                                            Dim id = $"md-h{counter}"
                                            counter += 1
                                            Return $"<{m.Groups(1).Value} id=""{id}"">"
                                        End Function)
    End Function

    Private Sub BuildTree(headings As List(Of HeadingInfo))
        TreeView1.Nodes.Clear()

        Dim stack As New Stack(Of Tuple(Of Integer, TreeNode))

        For Each h In headings
            While stack.Count > 0 AndAlso stack.Peek().Item1 >= h.Level
                stack.Pop()
            End While

            Dim parent As TreeNodeCollection = If(stack.Count = 0, TreeView1.Nodes, stack.Peek().Item2.Nodes)
            Dim node = parent.Add(h.Text)
            node.Tag = h

            stack.Push(Tuple.Create(h.Level, node))
        Next
    End Sub

    ' ---------------------------------------------------------------------
    ' Rebuild: parse + inject + render + build tree (debounced on edit)
    ' ---------------------------------------------------------------------

    Private Sub Rebuild()
        Dim md = TextBox1.Text
        Dim headings = ParseHeadings(md)
        _headings = headings

        Dim html = render.Transform(md)
        html = InjectHeadingIds(html)

        Call BuildTree(headings)

        ' The WebView2 core may not be ready yet on the very first load; the
        ' tree can still be built so navigation works once it is initialised.
        If WebView21.CoreWebView2 IsNot Nothing Then
            _navigating = True
            Call WebViewLoader.NavigateToLargeString(WebView21, html)
        End If
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        If _loading Then
            Return
        End If
        ' Debounce rapid keystrokes: only rebuild after a 300ms pause.
        Timer1.Stop()
        Timer1.Start()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Timer1.Stop()
        Call Rebuild()
    End Sub

    ' ---------------------------------------------------------------------
    ' Navigation: clicking a heading node jumps both panes
    ' ---------------------------------------------------------------------

    Private Async Sub TreeView1_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles TreeView1.AfterSelect
        If e.Node.Tag Is Nothing Then
            Return
        End If

        Dim h = CType(e.Node.Tag, HeadingInfo)

        ' 1) Markdown source pane: select and scroll the heading line.
        If h.LineStart >= 0 AndAlso h.LineStart <= TextBox1.Text.Length Then
            TextBox1.SelectionStart = h.LineStart
            TextBox1.SelectionLength = Math.Min(h.LineLength, TextBox1.Text.Length - h.LineStart)
            TextBox1.ScrollToCaret()
            TextBox1.Focus()
        End If

        ' 2) HTML preview pane: scroll the matching anchor into view.
        If WebView21.CoreWebView2 Is Nothing Then
            Return
        End If

        _pendingScrollIndex = h.Index

        If _navigating Then
            ' The preview is still loading a (re)render; OnNavigationCompleted
            ' will apply the scroll once the new document is ready.
            Return
        End If

        Dim js = $"var el=document.getElementById('md-h{h.Index}'); if(el){{el.scrollIntoView({{behavior:'smooth', block:'start'}});}}"
        Await WebView21.CoreWebView2.ExecuteScriptAsync(js)
        _pendingScrollIndex = -1
    End Sub

    ''' <summary>
    ''' When the preview is re-rendered (e.g. after an edit) the anchor may not
    ''' exist yet at click time, so re-apply a pending scroll once navigation
    ''' completes.
    ''' </summary>
    Private Async Sub OnNavigationCompleted(sender As Object, e As CoreWebView2NavigationCompletedEventArgs)
        _navigating = False

        If _pendingScrollIndex < 0 Then
            Return
        End If

        Dim idx = _pendingScrollIndex
        _pendingScrollIndex = -1

        If WebView21.CoreWebView2 Is Nothing Then
            Return
        End If

        Dim js = $"var el=document.getElementById('md-h{idx}'); if(el){{el.scrollIntoView({{behavior:'smooth', block:'start'}});}}"
        Await WebView21.CoreWebView2.ExecuteScriptAsync(js)
    End Sub

    Private Sub ToolStripButton2_Click(sender As Object, e As EventArgs) Handles ToolStripButton2.Click
        Using file As New SaveFileDialog With {
            .Filter = {
                "Markdown(*.md)|*.md",
                "Microsoft Word(*.docx)|*.docx",
                "PDF(*.pdf)|*.pdf",
                "Open XPS(*.oxps)|*.oxps",
                "Microsoft XPS(*.xps)|*.xps",
                "RTF(*.rtf)|*.rtf",
                "Plain Text(*.txt)|*.txt",
                "Html Document(*.html)|*.html"
            }.JoinBy("|")
        }

            If file.ShowDialog = DialogResult.OK Then
                Dim blocks As Block()
                Dim render As ReportRender

                Call render.Render(blocks)
            End If
        End Using
    End Sub
End Class
