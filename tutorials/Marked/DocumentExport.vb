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
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ApplicationServices.Report
Imports Microsoft.VisualBasic.Linq
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
    ''' <param name="baseDirectory">
    ''' markdown 中相对图片路径的基准目录（通常是 markdown 源文件所在目录）；
    ''' 为空时使用当前工作目录。
    ''' </param>
    ''' <returns>实际使用的格式名称（用于界面提示）。</returns>
    Public Function Export(markdown As String, outFile As String,
                           Optional baseDirectory As String = Nothing) As String
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
            Dim baseDir As String = If(String.IsNullOrEmpty(baseDirectory), Environment.CurrentDirectory, baseDirectory)
            Dim blocks As List(Of Block) = New MarkdownBlockParser().Parse(markdown)

            ' markdown 中的图片通常是相对路径：按源文件目录解析成绝对路径之后，
            ' 二进制/RTF 写入器才能读到图片字节并内嵌到目标文档中。
            ' 文本类格式不需要嵌入，保持原始（相对）路径更易读；html 由 InlineImages 自行解析。
            If NeedsAbsoluteImagePath(ext) Then
                Call ResolveImages(blocks, baseDir)
            End If

            Call ExportBlocks(blocks, outFile, ext, baseDir)
        End If

        Return FormatName(ext)
    End Function

    ''' <summary>
    ''' 把块模型里的相对图片路径解析为可直接读取的绝对路径。
    ''' 远程图片（http/https）与 data URI 保持原样；解析不到本地文件时也保持原样，
    ''' 由写入器降级为文本占位。
    ''' </summary>
    ''' <param name="blocks">markdown 内容块（会被原地更新）。</param>
    ''' <param name="baseDir">相对路径基准目录。</param>
    Public Sub ResolveImages(blocks As List(Of Block), baseDir As String)
        For Each block As Block In blocks.SafeQuery
            If block Is Nothing Then
                Continue For
            End If
            If Not IsImageBlock(block.type) Then
                Continue For
            End If

            block.url = ResolveImagePath(block.url, baseDir)
        Next
    End Sub

    ''' <summary>
    ''' 该导出格式是否需要把图片读取为绝对路径后内嵌
    ''' （docx / pdf / rtf / oxps / xps）。
    ''' </summary>
    ''' <param name="ext">小写扩展名（含点）。</param>
    ''' <returns></returns>
    Public Function NeedsAbsoluteImagePath(ext As String) As Boolean
        Select Case LCase(If(ext, ""))
            Case ".docx", ".pdf", ".rtf", ".oxps", ".xps"
                Return True
            Case Else
                Return False
        End Select
    End Function

    ''' <summary>判断块类型是否携带图片地址。</summary>
    ''' <param name="blockType">block.type 的取值。</param>
    ''' <returns></returns>
    Public Function IsImageBlock(blockType As String) As Boolean
        Select Case LCase(If(blockType, ""))
            Case "image", "img"
                Return True
            Case Else
                Return False
        End Select
    End Function

    ''' <summary>
    ''' 把 markdown 中的图片地址解析为可读取的绝对文件路径。
    ''' </summary>
    ''' <param name="url">markdown 中书写的图片地址。</param>
    ''' <param name="baseDir">相对路径基准目录。</param>
    ''' <returns>存在的绝对路径；远程地址 / data URI / 找不到文件时返回原值。</returns>
    Public Function ResolveImagePath(url As String, baseDir As String) As String
        If String.IsNullOrEmpty(url) Then
            Return url
        End If

        Dim raw As String = url.Trim()

        If raw.StartsWith("data:", StringComparison.OrdinalIgnoreCase) OrElse
           raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase) OrElse
           raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase) OrElse
           raw.StartsWith("//") Then
            Return raw
        End If

        If raw.StartsWith("file://", StringComparison.OrdinalIgnoreCase) Then
            Try
                raw = New Uri(raw).LocalPath
            Catch ex As Exception
                Return url
            End Try
        End If

        ' markdown 里常把空格写成 %20
        Try
            raw = Uri.UnescapeDataString(raw)
        Catch ex As Exception
            ' 保留原始字符串
        End Try

        If Path.IsPathRooted(raw) Then
            Return If(File.Exists(raw), raw, url)
        End If

        If String.IsNullOrEmpty(baseDir) Then
            Return url
        End If

        Try
            Dim combined As String = Path.GetFullPath(Path.Combine(baseDir, raw))

            Return If(File.Exists(combined), combined, url)
        Catch ex As Exception
            Return url
        End Try
    End Function

    ''' <summary>
    ''' 把已解析的 <see cref="Block"/> 列表渲染为指定格式的文档。
    ''' </summary>
    ''' <param name="blocks">markdown 内容块。</param>
    ''' <param name="outFile">输出文件路径。</param>
    ''' <param name="ext">小写扩展名（含点）。</param>
    ''' <param name="baseDir">相对图片路径的基准目录（html 内联图片时使用）。</param>
    Private Sub ExportBlocks(blocks As List(Of Block), outFile As String, ext As String, baseDir As String)
        Select Case ext
            Case ".txt"
                Dim render As New ReportPlainTextRender

                Call render.Render(blocks)
                Call File.WriteAllText(outFile, render.Output, New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))

            Case ".html", ".htm"
                Dim render As New ReportHtmlRender

                Call render.Render(blocks)

                ' 单文件 html：把本地图片内联为 data URI，避免移动文件后图片失效
                Dim body As String = InlineImages(render.Output, baseDir)

                Call File.WriteAllText(outFile,
                                       WrapHtml(body, Path.GetFileNameWithoutExtension(outFile)),
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

    ''' <summary>html 中 &lt;img src="..."&gt; 的匹配（只替换 src 的值）。</summary>
    Private ReadOnly imgSource As New Regex("(<img\b[^>]*?\bsrc\s*=\s*"")([^""]*)("")", RegexOptions.IgnoreCase)

    ''' <summary>
    ''' 把 html 中的本地图片内联为 data URI，使导出的 .html 成为可独立分发的单文件文档。
    ''' 远程图片（http/https）与找不到的本地文件保持原样。
    ''' </summary>
    ''' <param name="html">html 片段。</param>
    ''' <param name="baseDir">相对路径基准目录。</param>
    ''' <returns>内联后的 html。</returns>
    Public Function InlineImages(html As String, baseDir As String) As String
        If String.IsNullOrEmpty(html) Then
            Return html
        End If

        Dim basePath As String = If(String.IsNullOrEmpty(baseDir), Environment.CurrentDirectory, baseDir)

        Return imgSource.Replace(html,
            Function(match)
                Dim src As String = match.Groups(2).Value
                Dim resolved As String = ResolveImagePath(src, basePath)

                If String.IsNullOrEmpty(resolved) OrElse Not File.Exists(resolved) Then
                    Return match.Value
                End If

                Dim mime As String = ImageMimeType(resolved)

                If mime Is Nothing Then
                    Return match.Value
                End If

                Dim data As String = Convert.ToBase64String(File.ReadAllBytes(resolved))

                Return match.Groups(1).Value & "data:" & mime & ";base64," & data & match.Groups(3).Value
            End Function)
    End Function

    ''' <summary>按扩展名推断图片的 MIME 类型；不支持的格式返回 Nothing。</summary>
    ''' <param name="filePath">图片文件路径。</param>
    ''' <returns>MIME 类型。</returns>
    Public Function ImageMimeType(filePath As String) As String
        Select Case Path.GetExtension(filePath).ToLowerInvariant()
            Case ".png" : Return "image/png"
            Case ".jpg", ".jpeg" : Return "image/jpeg"
            Case ".gif" : Return "image/gif"
            Case ".bmp" : Return "image/bmp"
            Case ".webp" : Return "image/webp"
            Case ".svg" : Return "image/svg+xml"
            Case Else : Return Nothing
        End Select
    End Function

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
