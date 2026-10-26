Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Report
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.MIME.Office.WordDocument
Imports Microsoft.VisualBasic.MIME.application.pdf
Imports Microsoft.VisualBasic.MIME.RTF
Imports Microsoft.VisualBasic.MIME.OpenXPS

''' <summary>
''' 数据报告模块。
''' <para>
''' 报告的内容模型就是 Core 里的 <see cref="Block"/> 集合（与 markdown 的块级语法一一对应），
''' 用本模块的工厂函数拼装标题 / 段落 / 代码 / 列表 / 表格 / 图片，
''' 再由 <see cref="render"/> 按文件扩展名统一导出为
''' <c>.md</c> / <c>.html</c> / <c>.txt</c> / <c>.docx</c> / <c>.pdf</c> / <c>.rtf</c> / <c>.xps</c>。
''' </para>
''' <para>
''' 图片块必须引用**磁盘上已经存在**的图片文件（先用 <c>plot.save</c> 把图写出来），
''' 二进制文档格式才能真正把图片嵌进文档。
''' </para>
''' </summary>
Public Module report

    ' ======================================================== 内容块工厂

    ''' <summary>标题块，级别 1-6。</summary>
    Public Function heading(text As String, Optional level As Integer = 1) As Block
        Return New Block With {
            .type = "heading",
            .level = If(level < 1, 1, If(level > 6, 6, level)),
            .content = text
        }
    End Function

    ''' <summary>段落块。</summary>
    Public Function paragraph(text As String) As Block
        Return New Block With {.type = "paragraph", .content = text}
    End Function

    ''' <summary>代码块。</summary>
    Public Function code(text As String, Optional language As String = "vbnet") As Block
        Return New Block With {.type = "code", .content = text, .language = language}
    End Function

    ''' <summary>列表块；<paramref name="ordered"/> 为 True 时输出有序列表。</summary>
    Public Function list(items As String(), Optional ordered As Boolean = False) As Block
        Return New Block With {.type = "list", .items = items, .ordered = ordered}
    End Function

    ''' <summary>引用块。</summary>
    Public Function blockquote(text As String) As Block
        Return New Block With {.type = "blockquote", .content = text}
    End Function

    ''' <summary>分隔线。</summary>
    Public Function hr() As Block
        Return New Block With {.type = "hr"}
    End Function

    ''' <summary>原样嵌入的 html 片段（不会被转义）。</summary>
    Public Function raw(html As String) As Block
        Return New Block With {.type = "html", .content = html}
    End Function

    ''' <summary>链接块。</summary>
    Public Function link(url As String, Optional text As String = Nothing) As Block
        Return New Block With {
            .type = "link",
            .url = url,
            .content = If(text, url)
        }
    End Function

    ''' <summary>图片块；<paramref name="url"/> 必须是磁盘上存在的图片路径或可访问的 URL。</summary>
    Public Function image(url As String, Optional alt As String = Nothing, Optional title As String = Nothing) As Block
        Return New Block With {
            .type = "image",
            .url = url,
            .alt = If(alt, ""),
            .title = title
        }
    End Function

    ''' <summary>数学公式块（latex 文本）。</summary>
    Public Function math(latex As String) As Block
        Return New Block With {.type = "math", .content = latex}
    End Function

    ''' <summary>表格块。</summary>
    Public Function table(headers As String(), rows As String()(), Optional alignments As String() = Nothing) As Block
        Return New Block With {
            .type = "table",
            .headers = headers,
            .rows = rows,
            .alignments = alignments
        }
    End Function

    ''' <summary>
    ''' 由统一二维表生成表格块（行名作为第一列、标签列以 ``label:`` 前缀出现在表头）。
    ''' </summary>
    Public Function tableOf(source As NumericTable, Optional maxRows As Integer = -1) As Block
        If source Is Nothing OrElse source.nsamples = 0 Then
            Return table({"empty"}, New String()() {New String() {"-"}})
        End If

        Dim n As Integer = source.nsamples
        If maxRows > 0 AndAlso n > maxRows Then
            n = maxRows
        End If

        Dim headers As New List(Of String) From {""}
        Call headers.AddRange(source.featureNames)
        Call headers.AddRange(source.labelNames.Select(Function(name) "label:" & name))

        Dim rows(n - 1)() As String

        For i As Integer = 0 To n - 1
            Dim cells As New List(Of String) From {source.GetRowName(i)}

            For Each v As Double In source.features(i)
                Call cells.Add(FormatValue(v))
            Next

            If source.labels IsNot Nothing Then
                For Each v As Double In source.labels(i)
                    Call cells.Add(FormatValue(v))
                Next
            End If

            rows(i) = cells.ToArray
        Next

        Return table(headers.ToArray, rows,
                     Enumerable.Repeat("left", 1).Concat(Enumerable.Repeat("right", headers.Count - 1)).ToArray)
    End Function

    ''' <summary>术语/定义块。</summary>
    Public Function definitions(terms As String(), descriptions As String()) As Block
        Return New Block With {.type = "deflist", .terms = terms, .definitions = descriptions}
    End Function

    ''' <summary>任务列表块。</summary>
    Public Function tasklist(items As String(), Optional checked As Boolean() = Nothing) As Block
        Return New Block With {.type = "tasklist", .items = items, .checked = checked}
    End Function

    ''' <summary>脚注块。</summary>
    Public Function footnote(id As String, text As String) As Block
        Return New Block With {.type = "footnote", .id = id, .content = text}
    End Function

    ' ======================================================== 渲染

    ''' <summary>渲染为 markdown 文本。</summary>
    Public Function markdown(blocks As Block()) As String
        Return If(blocks, New Block() {}).ToMarkdown
    End Function

    ''' <summary>渲染为纯文本（去掉所有标记）。</summary>
    Public Function plainText(blocks As Block()) As String
        Return If(blocks, New Block() {}).ToPlainText
    End Function

    ''' <summary>渲染为完整的 HTML5 文档（含内联样式）。</summary>
    Public Function html(blocks As Block(), Optional title As String = "data report") As String
        Dim body As String = If(blocks, New Block() {}).ToHtml
        Dim sb As New StringBuilder()

        Call sb.AppendLine("<!DOCTYPE html>")
        Call sb.AppendLine("<html lang=""en"">")
        Call sb.AppendLine("<head>")
        Call sb.AppendLine("<meta charset=""utf-8"" />")
        Call sb.AppendLine($"<title>{If(title, "data report")}</title>")
        Call sb.AppendLine("<style>")
        Call sb.AppendLine("body{max-width:960px;margin:2.5rem auto;padding:0 1.25rem;font-family:""Segoe UI"",Helvetica,Arial,sans-serif;line-height:1.65;color:#1f2933;}")
        Call sb.AppendLine("h1,h2,h3,h4{line-height:1.25;margin-top:2rem;}")
        Call sb.AppendLine("h1{border-bottom:2px solid #e4e7eb;padding-bottom:.5rem;}")
        Call sb.AppendLine("table{border-collapse:collapse;width:100%;margin:1rem 0;font-size:.92rem;}")
        Call sb.AppendLine("th,td{border:1px solid #cbd2d9;padding:.35rem .6rem;}")
        Call sb.AppendLine("th{background:#f5f7fa;text-align:left;}")
        Call sb.AppendLine("tr:nth-child(even){background:#fafbfc;}")
        Call sb.AppendLine("pre{background:#0f172a;color:#e2e8f0;padding:1rem;border-radius:6px;overflow:auto;}")
        Call sb.AppendLine("code{font-family:Consolas,monospace;}")
        Call sb.AppendLine("img{max-width:100%;height:auto;}")
        Call sb.AppendLine("blockquote{border-left:4px solid #cbd2d9;margin-left:0;padding-left:1rem;color:#52606d;}")
        Call sb.AppendLine("hr{border:none;border-top:1px solid #e4e7eb;margin:2rem 0;}")
        Call sb.AppendLine("</style>")
        Call sb.AppendLine("</head>")
        Call sb.AppendLine("<body>")
        Call sb.AppendLine(body)
        Call sb.AppendLine("</body>")
        Call sb.AppendLine("</html>")

        Return sb.ToString
    End Function

    ''' <summary>
    ''' 把内容块渲染成目标文档并落盘，返回文件的绝对路径。
    ''' <para>
    ''' 支持的扩展名：<c>.md</c> / <c>.markdown</c> / <c>.txt</c> / <c>.html</c> /
    ''' <c>.docx</c> / <c>.pdf</c> / <c>.rtf</c> / <c>.xps</c> / <c>.oxps</c>。
    ''' </para>
    ''' </summary>
    Public Function render(blocks As Block(), file As String,
                           Optional title As String = Nothing,
                           Optional author As String = Nothing,
                           Optional toc As Boolean = False) As String

        Dim path As String = System.IO.Path.GetFullPath(file)
        Dim dir As String = System.IO.Path.GetDirectoryName(path)

        If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
            Call Directory.CreateDirectory(dir)
        End If

        Dim content As Block() = If(blocks, New Block() {})
        Dim ext As String = System.IO.Path.GetExtension(path).ToLower

        Select Case ext
            Case ".md", ".markdown"
                Call System.IO.File.WriteAllText(path, content.ToMarkdown, Utf8)
            Case ".txt", ".text"
                Call System.IO.File.WriteAllText(path, content.ToPlainText, Utf8)
            Case ".html", ".htm"
                Call System.IO.File.WriteAllText(path, html(content, If(title, defaultTitle(path))), Utf8)
            Case ".docx"
                Dim renderer As New WordRender With {
                    .DocumentTitle = If(title, ""),
                    .Author = If(author, ""),
                    .TableOfContents = toc
                }

                Call renderer.Render(content)
                Call renderer.Save(path)
            Case ".pdf"
                Dim renderer As New PdfRender With {
                    .DocumentTitle = If(title, ""),
                    .Author = If(author, ""),
                    .TableOfContents = toc
                }

                Call renderer.Render(content)
                Call renderer.Save(path)
            Case ".rtf"
                Dim renderer As New RtfRender With {
                    .DocumentTitle = If(title, ""),
                    .Author = If(author, ""),
                    .TableOfContents = toc
                }

                Call renderer.Render(content)
                Call renderer.Save(path)
            Case ".xps", ".oxps"
                Dim renderer As New XpsRender With {
                    .DocumentTitle = If(title, ""),
                    .Author = If(author, ""),
                    .TableOfContents = toc
                }

                Call renderer.Render(content)
                Call renderer.Save(path)
            Case Else
                Throw New NotSupportedException($"the report format '{ext}' of '{path}' is not supported; use md/html/txt/docx/pdf/rtf/xps")
        End Select

        Return path
    End Function

    ''' <summary>
    ''' 创建一个报告构建器：逐个 <c>add</c> 内容块，最后 <c>save</c> 到目标文件。
    ''' </summary>
    Public Function document(Optional title As String = Nothing, Optional author As String = Nothing, Optional toc As Boolean = False) As ReportBuilder
        Return New ReportBuilder With {
            .title = title,
            .author = author,
            .tableOfContents = toc
        }
    End Function

    Private ReadOnly Utf8 As New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False)

    Private Function defaultTitle(path As String) As String
        Return System.IO.Path.GetFileNameWithoutExtension(path)
    End Function

    Private Function FormatValue(v As Double) As String
        If Double.IsNaN(v) Then
            Return "NA"
        End If

        Return v.ToString("G6")
    End Function

    ''' <summary>
    ''' 增量式报告构建器。
    ''' </summary>
    Public Class ReportBuilder

        ''' <summary>文档标题（二元文档格式写入封面/标题，html 写入 &lt;title&gt;）。</summary>
        Public Property title As String
        ''' <summary>文档作者。</summary>
        Public Property author As String
        ''' <summary>是否插入目录（docx / pdf / rtf / xps 支持）。</summary>
        Public Property tableOfContents As Boolean = False

        Private ReadOnly _blocks As New List(Of Block)

        ''' <summary>追加一个内容块。</summary>
        Public Function add(block As Block) As ReportBuilder
            If block IsNot Nothing Then
                Call _blocks.Add(block)
            End If

            Return Me
        End Function

        ''' <summary>批量追加内容块。</summary>
        Public Function addAll(blocks As Block()) As ReportBuilder
            If blocks IsNot Nothing Then
                Call _blocks.AddRange(blocks)
            End If

            Return Me
        End Function

        ''' <summary>当前已追加的内容块数量。</summary>
        Public Function count() As Integer
            Return _blocks.Count
        End Function

        ''' <summary>以内容块数组的形式取出全部内容。</summary>
        Public Function blocks() As Block()
            Return _blocks.ToArray
        End Function

        ''' <summary>渲染为 markdown 文本。</summary>
        Public Function toMarkdown() As String
            Return _blocks.ToArray.ToMarkdown
        End Function

        ''' <summary>渲染为完整 HTML5 文档。</summary>
        Public Function toHtml() As String
            Return html(_blocks.ToArray, If(title, "data report"))
        End Function

        ''' <summary>按扩展名渲染并保存，返回绝对路径。</summary>
        Public Function save(file As String) As String
            Return render(_blocks.ToArray, file, title, author, tableOfContents)
        End Function
    End Class
End Module
