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

' 注意：ApplicationServices.Report 与 markdown 命名空间各有一个 HtmlRender，
' 这里使用带主题样式的 markdown html 渲染器，因此用别名避免二义性。
Imports MarkdownHtmlRender = Microsoft.VisualBasic.MIME.text.markdown.HtmlRender

Public Class FormMarkWeb

    ''' <summary>预览使用的 markdown → html 渲染器（切换样式主题时重建）。</summary>
    Private render As New MarkdownRender(New MarkdownHtmlRender(MarkdownStyle.GitHub))

    Private _headings As List(Of HeadingInfo)
    Private _loading As Boolean = False
    Private _navigating As Boolean = False
    Private _pendingScrollIndex As Integer = -1

    ''' <summary>当前打开的 markdown 文件路径（导出时作为相对图片路径的基准目录）。</summary>
    Private _sourceFile As String = ""

    ''' <summary>
    ''' 本地图片 → data URI 的缓存。键包含文件最后写入时间，避免每次按键（防抖后重排）
    ''' 都重新读取并 base64 编码大图。
    ''' </summary>
    Private ReadOnly _imageCache As New Dictionary(Of String, String)()

    Private ReadOnly headingTag As New Regex("<(h[1-6])>", RegexOptions.IgnoreCase)

    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        Using file As New OpenFileDialog With {.Filter = "Markdown 文档(*.md)|*.md"}
            If file.ShowDialog = DialogResult.OK Then
                Dim md As String = file.FileName.ReadAllText

                _sourceFile = file.FileName

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
    ' Style theme selection
    ' ---------------------------------------------------------------------

    Private Sub ToolStripMenuItemThemeGitHub_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItemThemeGitHub.Click
        Call ApplyTheme(MarkdownStyle.GitHub)
    End Sub

    Private Sub ToolStripMenuItemThemeDark_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItemThemeDark.Click
        Call ApplyTheme(MarkdownStyle.Dark)
    End Sub

    Private Sub ToolStripMenuItemThemeThesis_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItemThemeThesis.Click
        Call ApplyTheme(MarkdownStyle.Thesis)
    End Sub

    Private Sub ToolStripMenuItemThemePlain_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItemThemePlain.Click
        Call ApplyTheme(Nothing)
    End Sub

    ''' <summary>
    ''' Switch the CSS theme used by the preview pane: rebuilds the html renderer,
    ''' syncs the menu check states and re-renders the document (tree + anchors).
    ''' </summary>
    ''' <param name="style">目标主题；<c>Nothing</c> 表示不注入 CSS（输出 html 片段）。</param>
    Private Sub ApplyTheme(style As MarkdownStyle)
        If style Is Nothing Then
            render = New MarkdownRender()
        Else
            render = New MarkdownRender(New MarkdownHtmlRender(style))
        End If

        ' 勾选状态与当前主题保持一致
        ToolStripMenuItemThemeGitHub.Checked = style Is MarkdownStyle.GitHub
        ToolStripMenuItemThemeDark.Checked = style Is MarkdownStyle.Dark
        ToolStripMenuItemThemeThesis.Checked = style Is MarkdownStyle.Thesis
        ToolStripMenuItemThemePlain.Checked = style Is Nothing

        Call Rebuild()
    End Sub

    ' ---------------------------------------------------------------------
    ' Heading parsing / html anchor injection / tree building
    ' ---------------------------------------------------------------------

    ''' <summary>
    ''' Inject a stable ``id="md-h{index}"`` anchor into every ``&lt;h*&gt;`` opening
    ''' tag, in document order, so the index matches
    ''' <see cref="MarkdownHeadings.Parse"/>.
    ''' </summary>
    Private Function InjectHeadingIds(html As String) As String
        Dim counter As Integer = 0
        Return headingTag.Replace(html, Function(m)
                                            Dim id = $"md-h{counter}"
                                            counter += 1
                                            Return $"<{m.Groups(1).Value} id=""{id}"">"
                                        End Function)
    End Function

    ''' <summary>
    ''' 预览页面的图片地址路由：把 markdown 中的本地图片（相对路径相对于被打开的
    ''' markdown 文件所在目录）内联成 ``data:image/...;base64,`` URI。
    ''' </summary>
    ''' <remarks>
    ''' <see cref="WebViewLoader.NavigateToLargeString"/> 通过虚拟主机以 text/html 提供
    ''' 页面，页面里的相对路径会相对该虚拟 URL 解析，而 Chromium 又禁止非 file: 源加载
    ''' 本地文件，所以图片必须内联；远程图片（http/https）与找不到的文件保持原样。
    ''' </remarks>
    ''' <param name="url">markdown 中书写的图片地址。</param>
    ''' <returns>可直接被浏览器加载的地址。</returns>
    Private Function PreviewImageUrl(url As String) As String
        If String.IsNullOrEmpty(url) Then
            Return url
        End If

        Dim baseDir As String = If(String.IsNullOrEmpty(_sourceFile),
                                   Environment.CurrentDirectory,
                                   IO.Path.GetDirectoryName(_sourceFile))
        Dim resolved As String = DocumentExport.ResolveImagePath(url, baseDir)

        If String.IsNullOrEmpty(resolved) OrElse Not IO.File.Exists(resolved) Then
            Return url
        End If

        Dim mime As String = DocumentExport.ImageMimeType(resolved)

        If mime Is Nothing Then
            Return url
        End If

        Dim stamp As Long = 0

        Try
            stamp = IO.File.GetLastWriteTimeUtc(resolved).Ticks
        Catch ex As Exception
            ' 读取时间失败时退化为不缓存
        End Try

        Dim key As String = resolved & "|" & stamp

        SyncLock _imageCache
            If _imageCache.ContainsKey(key) Then
                Return _imageCache(key)
            End If

            Try
                Dim data As String = Convert.ToBase64String(IO.File.ReadAllBytes(resolved))
                Dim uri As String = $"data:{mime};base64,{data}"

                ' 文件被改动会生成新键，简单地限制缓存条目数量
                If _imageCache.Count > 64 Then
                    Call _imageCache.Clear()
                End If

                _imageCache(key) = uri

                Return uri
            Catch ex As Exception
                Return url
            End Try
        End SyncLock
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
        Dim headings = MarkdownHeadings.Parse(md)
        _headings = headings

        ' 预览页由 WebView2 的虚拟主机托管，相对的 / 本地的图片路径都会被浏览器拒绝
        ' 加载，因此每次渲染前都挂上「本地图片内联为 data URI」的地址路由
        Call render.SetImageUrlRouter(AddressOf PreviewImageUrl)

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

    ''' <summary>
    ''' 另存为：把编辑器中的 markdown 导出为所选格式的文档。
    ''' 实际的解析与渲染在 <see cref="DocumentExport.Export"/> 中完成。
    ''' </summary>
    Private Sub ToolStripButton2_Click(sender As Object, e As EventArgs) Handles ToolStripButton2.Click
        Using dialog As New SaveFileDialog With {
            .Filter = {
                "Markdown(*.md)|*.md",
                "Microsoft Word(*.docx)|*.docx",
                "PDF(*.pdf)|*.pdf",
                "Open XPS(*.oxps)|*.oxps",
                "Microsoft XPS(*.xps)|*.xps",
                "RTF(*.rtf)|*.rtf",
                "Plain Text(*.txt)|*.txt",
                "Html Document(*.html)|*.html"
            }.JoinBy("|"),
            .AddExtension = True,
            .DefaultExt = "md",
            .FileName = "document.md",
            .Title = "导出文档"
        }

            If dialog.ShowDialog() <> DialogResult.OK Then
                Return
            End If

            Try
                ' 图片按 markdown 源文件所在目录解析；未打开文件时使用当前工作目录
                Dim baseDir As String = If(String.IsNullOrEmpty(_sourceFile),
                                           Environment.CurrentDirectory,
                                           IO.Path.GetDirectoryName(_sourceFile))
                Dim format As String = DocumentExport.Export(TextBox1.Text, dialog.FileName, baseDir)

                Call MessageBox.Show(Me,
                                     $"已导出 {format}：{vbCrLf}{dialog.FileName}",
                                     "导出完成",
                                     MessageBoxButtons.OK,
                                     MessageBoxIcon.Information)
            Catch ex As Exception
                Call MessageBox.Show(Me,
                                     $"导出失败：{vbCrLf}{ex.Message}",
                                     "导出失败",
                                     MessageBoxButtons.OK,
                                     MessageBoxIcon.Error)
            End Try
        End Using
    End Sub
End Class
