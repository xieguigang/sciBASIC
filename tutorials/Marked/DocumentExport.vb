' ============================================================================
' DocumentExport.vb - 把 markdown 源文本导出为多种文档格式
'
' 流程：markdown 文本 --MarkdownBlockParser--> Block 列表 --ReportRender--> 目标文档
'
'   .md             原样保存（避免解析往返丢失格式）
'   .txt            ApplicationServices.Report.TextRender
'   .html           ApplicationServices.Report.HtmlRender + HTML5 外壳
'   .docx           WordRender
'   .pdf            PdfRender
'   .rtf            RtfRender
'   .oxps / .xps    XpsRender（OpenXPS / Microsoft XPS）
'
' 放在独立模块里而不是直接写在窗体事件中：导出逻辑不依赖 WinForms，可以被
' 控制台工具或自动化测试直接复用。
' ============================================================================

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Report
Imports Microsoft.VisualBasic.MIME.application.pdf
Imports Microsoft.VisualBasic.MIME.Office.WordDocument
Imports Microsoft.VisualBasic.MIME.OpenXPS
Imports Microsoft.VisualBasic.MIME.RTF
Imports Microsoft.VisualBasic.MIME.text.markdown
Imports ReportHtmlRender = Microsoft.VisualBasic.ApplicationServices.Report.HtmlRender
Imports ReportPlainTextRender = Microsoft.VisualBasic.ApplicationServices.Report.PlainTextRender

''' <summary>
''' markdown → 多格式文档导出器（供 Marked 窗体的"另存为"使用）。
''' </summary>
''' <remarks>
''' 输出格式由文件扩展名决定；除 <c>.md</c> 外，所有格式都会先把 markdown 解析为
''' <see cref="Block"/> 列表，再交给对应的 <see cref="ReportRender"/> 渲染。
''' </remarks>
Public Module DocumentExport

    ''' <summary>支持的导出扩展名（用于提示与自校验）。</summary>
    Public ReadOnly SupportedExtensions As String() = {
        ".md", ".txt", ".html", ".docx", ".pdf", ".rtf", ".oxps", ".xps"
    }

    ''' <summary>
    ''' 把 markdown 文本导出为指定格式的文档，格式由 <paramref name="outFile"/> 的扩展名决定。
    ''' </summary>
    ''' <param name="markdown">markdown 源文本。</param>
    ''' <param name="outFile">输出文件路径（目录不存在时自动创建）。</param>
    ''' <returns>实际使用的格式名称（用于界面提示）。</returns>
    Public Function Export(markdown As String, outFile As String) As String
        If String.IsNullOrEmpty(outFile) Then
            Throw New ArgumentException("未指定导出文件路径。", NameOf(outFile))
        End If

        Dim ext As String = Path.GetExtension(outFile).ToLowerInvariant()
        Dim folder As String = Path.GetDirectoryName(outFile)

        If Not String.IsNullOrEmpty(folder) AndAlso Not Directory.Exists(folder) Then
            Call Directory.CreateDirectory(folder)
        End If

        If ext = ".md" OrElse ext = ".markdown" Then
            ' markdown 直接落盘：块模型往返会丢失嵌套列表、原始 HTML 等细节
            Call File.WriteAllText(outFile, If(markdown, ""), New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))
        Else
            Dim blocks As List(Of Block) = New MarkdownBlockParser().Parse(markdown)

            Call ExportBlocks(blocks, outFile, ext)
        End If

        Return FormatName(ext)
    End Function

    ''' <summary>
    ''' 把已解析的 <see cref="Block"/> 列表渲染为指定格式的文档。
    ''' </summary>
    ''' <param name="blocks">markdown 内容块。</param>
    ''' <param name="outFile">输出文件路径。</param>
    ''' <param name="ext">小写扩展名（含点）。</param>
    Private Sub ExportBlocks(blocks As List(Of Block), outFile As String, ext As String)
        Select Case ext
            Case ".txt"
                Dim render As New ReportPlainTextRender

                Call render.Render(blocks)
                Call File.WriteAllText(outFile, render.Output, New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))

            Case ".html", ".htm"
                Dim render As New ReportHtmlRender

                Call render.Render(blocks)
                Call File.WriteAllText(outFile,
                                       WrapHtml(render.Output, Path.GetFileNameWithoutExtension(outFile)),
                                       New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))

            Case ".docx"
                Dim render As New WordRender

                Call render.Render(blocks)
                Call render.Save(outFile)

            Case ".pdf"
                Dim render As New PdfRender

                Call render.Render(blocks)
                Call render.Save(outFile)

            Case ".rtf"
                Dim render As New RtfRender

                Call render.Render(blocks)
                Call render.Save(outFile)

            Case ".oxps"
                Dim render As New XpsRender(XpsFlavor.Oxps)

                Call render.Render(blocks)
                Call render.Save(outFile)

            Case ".xps"
                Dim render As New XpsRender(XpsFlavor.Xps)

                Call render.Render(blocks)
                Call render.Save(outFile)

            Case Else
                Throw New NotSupportedException($"不支持的导出格式: {ext}")
        End Select
    End Sub

    ''' <summary>
    ''' 把 <see cref="ReportHtmlRender"/> 输出的 html 片段包装成完整的 HTML5 文档。
    ''' </summary>
    ''' <param name="body">html 片段。</param>
    ''' <param name="title">文档标题。</param>
    ''' <returns>完整的 html 文本。</returns>
    Public Function WrapHtml(body As String, title As String) As String
        Dim sb As New StringBuilder()

        Call sb.AppendLine("<!DOCTYPE html>")
        Call sb.AppendLine("<html lang=""zh-CN"">")
        Call sb.AppendLine("<head>")
        Call sb.AppendLine("<meta charset=""utf-8"" />")
        Call sb.AppendLine("<meta name=""viewport"" content=""width=device-width, initial-scale=1"" />")
        Call sb.AppendLine($"<title>{System.Net.WebUtility.HtmlEncode(If(title, "document"))}</title>")
        Call sb.AppendLine("<style>")
        Call sb.AppendLine("  :root { color-scheme: light dark; }")
        Call sb.AppendLine("  body { margin: 0; padding: 2em 1em; background: #fbfbfd; color: #1f2328;")
        Call sb.AppendLine("         font-family: -apple-system, ""Segoe UI"", ""Microsoft YaHei"", Roboto, Helvetica, Arial, sans-serif;")
        Call sb.AppendLine("         line-height: 1.7; }")
        Call sb.AppendLine("  main { max-width: 900px; margin: 0 auto; }")
        Call sb.AppendLine("  h1, h2 { border-bottom: 1px solid #d8dee4; padding-bottom: .3em; }")
        Call sb.AppendLine("  h1 { font-size: 2em; } h2 { font-size: 1.5em; margin-top: 1.6em; }")
        Call sb.AppendLine("  h3 { font-size: 1.25em; margin-top: 1.4em; }")
        Call sb.AppendLine("  code { background: #eff1f3; border-radius: 4px; padding: .15em .35em;")
        Call sb.AppendLine("         font-family: Consolas, ""Cascadia Mono"", monospace; font-size: .92em; }")
        Call sb.AppendLine("  pre { background: #f4f6f8; border: 1px solid #e4e8ec; border-radius: 6px;")
        Call sb.AppendLine("        padding: .8em 1em; overflow-x: auto; }")
        Call sb.AppendLine("  pre code { background: none; padding: 0; }")
        Call sb.AppendLine("  blockquote { margin: 1em 0; padding: .2em 1em; color: #57606a;")
        Call sb.AppendLine("               border-left: 4px solid #d0d7de; background: #f6f8fa; }")
        Call sb.AppendLine("  table { border-collapse: collapse; margin: 1em 0; }")
        Call sb.AppendLine("  th, td { border: 1px solid #d0d7de; padding: .35em .75em; }")
        Call sb.AppendLine("  th { background: #f6f8fa; }")
        Call sb.AppendLine("  img { max-width: 100%; }")
        Call sb.AppendLine("  hr { border: 0; border-top: 1px solid #d8dee4; margin: 2em 0; }")
        Call sb.AppendLine("  .footnote { color: #57606a; font-size: .9em; }")
        Call sb.AppendLine("  .task-list { list-style: none; padding-left: 1.2em; }")
        Call sb.AppendLine("</style>")
        Call sb.AppendLine("</head>")
        Call sb.AppendLine("<body>")
        Call sb.AppendLine("<main>")
        Call sb.AppendLine(If(body, ""))
        Call sb.AppendLine("</main>")
        Call sb.AppendLine("</body>")
        Call sb.AppendLine("</html>")

        Return sb.ToString()
    End Function

    ''' <summary>扩展名对应的可读格式名。</summary>
    ''' <param name="ext">小写扩展名（含点）。</param>
    ''' <returns>格式名称。</returns>
    Public Function FormatName(ext As String) As String
        Select Case ext
            Case ".md", ".markdown" : Return "Markdown"
            Case ".txt" : Return "纯文本"
            Case ".html", ".htm" : Return "HTML 文档"
            Case ".docx" : Return "Word 文档"
            Case ".pdf" : Return "PDF 文档"
            Case ".rtf" : Return "RTF 文档"
            Case ".oxps" : Return "OpenXPS 文档"
            Case ".xps" : Return "Microsoft XPS 文档"
            Case Else : Return ext
        End Select
    End Function

End Module
