' ============================================================================
' Program.vb - XPS / OXPS 生成与提取自校验
'
' 用法: XpsSelfTest [输出目录]
'
' 检查项：
'   1. 生成 .oxps 并解包校验部件、命名空间、内容类型、字体与图片资源；
'   2. 生成 .xps，校验方言差异（内容类型与关系类型）；
'   3. 关闭字体嵌入时的回退路径（包结构合法、文本仍可提取）；
'   4. 自动分页（多页文档的页序与文本顺序）；
'   5. XpsRender 统一报告接口（Render / Save / Generate）端到端；
'   6. 文本提取（ExtractText / ExtractPages / ExtractParagraphs / ExtractMetadata）。
'
' 全部检查通过时返回码为 0，否则为 1。
' ============================================================================

Imports System.IO
Imports System.IO.Compression
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Report
Imports Microsoft.VisualBasic.MIME.OpenXPS

Module Program

    Private failures As Integer = 0

    Private Const TitleText As String = "XPS/OXPS 生成与提取示例"

    Function Main(args As String()) As Integer
        Console.OutputEncoding = Encoding.UTF8

        Dim outDir As String = If(args.Length > 0, args(0), Path.Combine(AppContext.BaseDirectory, "demo-output"))

        Call Directory.CreateDirectory(outDir)

        Dim imagePath As String = Path.Combine(outDir, "demo-image.png")

        Call CreateTestPng(imagePath, 12, 8)

        Console.WriteLine("== 1. 生成 OXPS 并校验包结构 ==")
        Call DemoOxps(outDir, imagePath)

        Console.WriteLine("== 2. 生成 XPS 并校验方言差异 ==")
        Call DemoXps(outDir, imagePath)

        Console.WriteLine("== 3. 不嵌入字体的回退模式 ==")
        Call DemoFallback(outDir)

        Console.WriteLine("== 4. 自动分页 ==")
        Call DemoPagination(outDir)

        Console.WriteLine("== 5. XpsRender 统一报告接口 ==")
        Call DemoRender(outDir, imagePath)

        Console.WriteLine()

        If failures = 0 Then
            Console.WriteLine("全部检查通过。")
            Return 0
        Else
            Console.WriteLine($"存在 {failures} 项失败。")
            Return 1
        End If
    End Function

    ' ========================================================================
    ' 1. OXPS
    ' ========================================================================

    Private Sub DemoOxps(outDir As String, imagePath As String)
        Dim doc As New XpsDocument(author:="sciBASIC#",
                                   title:=TitleText,
                                   tags:={"xps", "oxps", "report"},
                                   subject:="XpsDocument",
                                   description:="演示由内容块生成 OXPS 并回读文本")

        doc.Flavor = XpsFlavor.Oxps
        doc.DocTitle(TitleText)
        doc.Toc(2)
        doc.WriteBlocks(BuildBlocks(imagePath))
        doc.Paragraph("结尾段落。")

        Dim outPath As String = Path.Combine(outDir, "demo.oxps")

        doc.Save(outPath)

        Console.WriteLine($"  已生成: {outPath} ({New FileInfo(outPath).Length:N0} 字节)")

        Dim names As String() = EntryNames(outPath)

        Call Check("包含 [Content_Types].xml", names.Contains("[Content_Types].xml"))
        Call Check("包含 _rels/.rels", names.Contains("_rels/.rels"))
        Call Check("包含 FixedDocumentSequence.fdseq", names.Contains("FixedDocumentSequence.fdseq"))
        Call Check("包含 FixedDocument.fdoc", names.Contains("Documents/1/FixedDocument.fdoc"))
        Call Check("包含页面关系", names.Contains("Documents/1/_rels/FixedDocument.fdoc.rels"))
        Call Check("包含页面部件", names.Any(Function(n) n.EndsWith(".fpage")))
        Call Check("包含页面关系部件", names.Any(Function(n) n.EndsWith(".fpage.rels")))

        Dim types As String = ReadEntry(outPath, "[Content_Types].xml")

        Call Check("内容类型为 OXPS 方言",
                   types.Contains("application/oxps.fixedpage+xml") AndAlso
                   types.Contains("application/oxps.fixeddocumentsequence+xml") AndAlso
                   types.Contains("application/oxps.fixeddocument+xml"))

        Dim rels As String = ReadEntry(outPath, "_rels/.rels")

        Call Check("关系类型为 openxps fixedrepresentation",
                   rels.Contains("http://schemas.openxps.org/oxps/v1.0/fixedrepresentation"))

        Dim pagePart As String = PagePartName(names, 1)
        Dim page As String = ReadEntry(outPath, pagePart)

        Call Check("FixedPage 使用 OXPS 命名空间",
                   page.Contains("xmlns=""http://schemas.openxps.org/oxps/v1.0"""))
        Call Check("页面包含 Glyphs 文本",
                   page.Contains("<Glyphs") AndAlso page.Contains("UnicodeString="))
        Call Check("页面包含表格网格线", page.Contains("<Path Stroke="))
        Call Check("页面包含代码块底纹", page.Contains("Fill=""#F2F2F2"""))
        Call Check("页面包含图片 ImageBrush",
                   page.Contains("<ImageBrush") AndAlso page.Contains("Viewbox=""0,0,12,8"""))
        Call Check("图片资源已写入",
                   names.Any(Function(n) n.StartsWith("Resources/Images/") AndAlso n.EndsWith(".png")))

        Dim fonts As String() = names.Where(Function(n) n.EndsWith(".odttf")).ToArray()

        Call Check("字体资源已嵌入", fonts.Length = 1)

        If fonts.Length = 1 Then
            Call Check("Glyphs 引用字体部件", page.Contains("FontUri=""/Resources/Fonts/"))

            Dim systemFont As String = XpsFontResource.FindSystemFont()

            If String.IsNullOrEmpty(systemFont) Then
                Console.WriteLine("  [跳过] 本机未找到系统字体，无法比对 ODTTF 混淆")
            Else
                Dim embedded As Byte() = ReadBytes(outPath, fonts(0))
                Dim original As Byte() = File.ReadAllBytes(systemFont)

                Call Check("字体前 32 字节已混淆", Not SamePrefix(embedded, original, 32))
            End If
        End If

        ' ---- 文本回读 ----
        Dim reader As New XpsTextReader()
        Dim text As String = reader.ExtractText(outPath)

        For Each expected As String In {
            TitleText,
            "第一章 文档生成",
            "1.1 列表与任务",
            "内容块模型",
            "已完成项",
            "XML Paper Specification",
            "第一行",
            "Dim doc As New XpsDocument()",
            "引用块内容。",
            "测试图片",
            "[1] 脚注内容。",
            "sciBASIC#](https://github.com/xieguigang/sciBASIC)",
            "结尾段落。"
        }
            Call Check("回读包含: " & expected, text.Contains(expected))
        Next

        Dim pages As String() = reader.ExtractPages(outPath)

        Call Check("ExtractPages 页数与 fpage 数量一致",
                   pages.Length = names.Count(Function(n) n.EndsWith(".fpage")))

        Dim paragraphs As String() = reader.ExtractParagraphs(outPath)

        Call Check("ExtractParagraphs 返回多个段落", paragraphs.Length > 5)

        Dim info As XpsDocumentInfo = reader.ExtractMetadata(outPath)

        Call Check("元数据 title", info.Title = TitleText)
        Call Check("元数据 author", info.Author = "sciBASIC#")
        Call Check("元数据 subject", info.Subject = "XpsDocument")
        Call Check("元数据 keywords", info.Keywords.Contains("oxps"))
    End Sub

    ' ========================================================================
    ' 2. XPS
    ' ========================================================================

    Private Sub DemoXps(outDir As String, imagePath As String)
        Dim doc As New XpsDocument(author:="sciBASIC#", title:="XPS 方言示例")

        doc.Flavor = XpsFlavor.Xps
        doc.WriteBlocks(BuildBlocks(imagePath))

        Dim outPath As String = Path.Combine(outDir, "demo.xps")

        doc.Save(outPath)

        Console.WriteLine($"  已生成: {outPath} ({New FileInfo(outPath).Length:N0} 字节)")

        Dim types As String = ReadEntry(outPath, "[Content_Types].xml")

        Call Check("内容类型为 XPS 方言",
                   types.Contains("application/vnd.ms-package.xps-fixedpage+xml") AndAlso
                   types.Contains("application/vnd.ms-package.xps-fixeddocumentsequence+xml") AndAlso
                   types.Contains("application/vnd.ms-package.obfuscated-opentype"))

        Dim rels As String = ReadEntry(outPath, "_rels/.rels")

        Call Check("关系类型为 microsoft xps fixedrepresentation",
                   rels.Contains("http://schemas.microsoft.com/xps/2005/06/fixedrepresentation"))

        Dim page As String = ReadEntry(outPath, PagePartName(EntryNames(outPath), 1))

        Call Check("FixedPage 使用 XPS 命名空间",
                   page.Contains("xmlns=""http://schemas.microsoft.com/xps/2005/06"""))

        Dim text As String = New XpsTextReader().ExtractText(outPath)

        Call Check("XPS 文本可回读", text.Contains("第一章 文档生成"))
    End Sub

    ' ========================================================================
    ' 3. 字体回退
    ' ========================================================================

    Private Sub DemoFallback(outDir As String)
        Dim doc As New XpsDocument(author:="sciBASIC#", title:="无字体回退示例")

        doc.EmbedFont = False
        doc.Paragraph("关闭字体嵌入时文档仍然可以生成，并且文本仍然可以被提取。")

        Dim outPath As String = Path.Combine(outDir, "demo_no_font.oxps")

        doc.Save(outPath)

        Console.WriteLine($"  已生成: {outPath} ({New FileInfo(outPath).Length:N0} 字节)")

        Dim names As String() = EntryNames(outPath)

        Call Check("回退模式不写入字体部件", Not names.Any(Function(n) n.EndsWith(".odttf")))

        Dim page As String = ReadEntry(outPath, PagePartName(names, 1))

        Call Check("回退模式不写 FontUri", Not page.Contains("FontUri="))
        Call Check("回退模式 Glyphs 仍然存在", page.Contains("<Glyphs"))

        Dim text As String = New XpsTextReader().ExtractText(outPath)

        Call Check("回退模式文本可回读", text.Contains("关闭字体嵌入时文档仍然可以生成"))
    End Sub

    ' ========================================================================
    ' 4. 分页
    ' ========================================================================

    Private Sub DemoPagination(outDir As String)
        Dim doc As New XpsDocument(title:="分页测试")

        doc.EmbedFont = False

        For i As Integer = 1 To 120
            Call doc.Paragraph($"自动分页测试段落 {i}：XPS 是固定版式格式，因此分页必须在序列化之前完成。")
        Next

        Dim outPath As String = Path.Combine(outDir, "demo_pages.oxps")

        doc.Save(outPath)

        Dim names As String() = EntryNames(outPath)
        Dim pageCount As Integer = names.Count(Function(n) n.EndsWith(".fpage"))
        Dim reader As New XpsTextReader()
        Dim pages As String() = reader.ExtractPages(outPath)
        Dim text As String = String.Join(vbCrLf, pages)

        Console.WriteLine($"  页数: {pageCount}")

        Call Check("内容溢出后自动分页", pageCount > 1)
        Call Check("ExtractPages 与页面部件数量一致", pages.Length = pageCount)
        Call Check("首段与末段顺序正确",
                   text.IndexOf("自动分页测试段落 1：", StringComparison.Ordinal) >= 0 AndAlso
                   text.IndexOf("自动分页测试段落 1：", StringComparison.Ordinal) <
                   text.IndexOf("自动分页测试段落 120：", StringComparison.Ordinal))
    End Sub

    ' ========================================================================
    ' 5. XpsRender
    ' ========================================================================

    Private Sub DemoRender(outDir As String, imagePath As String)
        Dim render As New XpsRender(XpsFlavor.Oxps) With {
            .DocumentTitle = "XpsRender 报告示例",
            .TableOfContents = True,
            .TocMaxLevel = 2,
            .Author = "sciBASIC#",
            .Title = "XpsRender 示例",
            .Subject = "XpsRender",
            .Tags = New String() {"xps", "report"}
        }

        Call render.Render(BuildBlocks(imagePath))

        Dim outPath As String = Path.Combine(outDir, "demo_render.oxps")

        Call render.Save(outPath)

        Console.WriteLine($"  已生成: {outPath} ({New FileInfo(outPath).Length:N0} 字节)")

        Dim reader As New XpsTextReader()
        Dim text As String = reader.ExtractText(outPath)

        Call Check("渲染器输出文档题目", text.Contains("XpsRender 报告示例"))
        Call Check("渲染器输出正文标题", text.Contains("第一章 文档生成"))
        Call Check("渲染器输出目录条目", text.Contains("1.1 列表与任务"))

        Dim info As XpsDocumentInfo = reader.ExtractMetadata(outPath)

        Call Check("渲染器元数据 author", info.Author = "sciBASIC#")
        Call Check("渲染器元数据 title", info.Title = "XpsRender 示例")
        Call Check("渲染器元数据 subject", info.Subject = "XpsRender")

        ' ---- 一次性生成（XPS 方言） ----
        Dim oneShot As String = Path.Combine(outDir, "demo_render_generate.xps")

        Call XpsRender.Generate(BuildBlocks(imagePath), oneShot, "一句话生成 XPS 报告", XpsFlavor.Xps)

        Call Check("Generate 生成文件非空", File.Exists(oneShot) AndAlso New FileInfo(oneShot).Length > 0)
        Call Check("Generate 文本可回读",
                   New XpsTextReader().ExtractText(oneShot).Contains("一句话生成 XPS 报告"))
    End Sub

    ' ========================================================================
    ' 内容块
    ' ========================================================================

    Private Function BuildBlocks(imagePath As String) As Block()
        Return {
            New Block With {.type = "heading", .level = 1, .content = "第一章 文档生成"},
            New Block With {.type = "paragraph", .content = "本文档由 XpsDocument 从内容块直接渲染为 XPS/OXPS 固定版式文档。"},
            New Block With {.type = "heading", .level = 2, .content = "1.1 列表与任务"},
            New Block With {.type = "list", .ordered = True, .items = {"内容块模型", "固定页排版", "文件打包"}},
            New Block With {.type = "tasklist", .items = {"已完成项", "未完成项"}, .checked = {True, False}},
            New Block With {.type = "deflist", .terms = {"XPS"}, .definitions = {"XML Paper Specification"}},
            New Block With {.type = "heading", .level = 2, .content = "1.2 表格"},
            New Block With {.type = "table",
                            .headers = {"名称", "说明"},
                            .alignments = {"left", "center"},
                            .rows = {New String() {"alpha", "第一行"}, New String() {"beta", "第二行"}}},
            New Block With {.type = "code", .language = "vbnet", .content = "Dim doc As New XpsDocument()" & vbCrLf & "doc.Save(""demo.oxps"")"},
            New Block With {.type = "blockquote", .content = "引用块内容。"},
            New Block With {.type = "hr"},
            New Block With {.type = "image", .url = imagePath, .alt = "测试图片"},
            New Block With {.type = "footnote", .id = "1", .content = "脚注内容。"},
            New Block With {.type = "link", .url = "https://github.com/xieguigang/sciBASIC", .alt = "sciBASIC#"}
        }
    End Function

    ' ========================================================================
    ' 断言与辅助
    ' ========================================================================

    Private Sub Check(name As String, condition As Boolean)
        If condition Then
            Console.WriteLine($"  [通过] {name}")
        Else
            failures += 1
            Console.WriteLine($"  [失败] {name}")
        End If
    End Sub

    Private Function EntryNames(packagePath As String) As String()
        Using archive As ZipArchive = ZipFile.OpenRead(packagePath)
            Return archive.Entries.Select(Function(entry) entry.FullName).ToArray()
        End Using
    End Function

    Private Function PagePartName(names As String(), pageNumber As Integer) As String
        Return names.First(Function(name) name.EndsWith($"/{pageNumber}.fpage", StringComparison.Ordinal))
    End Function

    Private Function ReadEntry(packagePath As String, partName As String) As String
        Using archive As ZipArchive = ZipFile.OpenRead(packagePath)
            Dim entry As ZipArchiveEntry = archive.GetEntry(partName)

            If entry Is Nothing Then
                Return ""
            End If

            Using reader As New StreamReader(entry.Open(), Encoding.UTF8)
                Return reader.ReadToEnd()
            End Using
        End Using
    End Function

    Private Function ReadBytes(packagePath As String, partName As String) As Byte()
        Using archive As ZipArchive = ZipFile.OpenRead(packagePath)
            Dim entry As ZipArchiveEntry = archive.GetEntry(partName)

            If entry Is Nothing Then
                Return New Byte() {}
            End If

            Using ms As New MemoryStream()
                Using es As Stream = entry.Open()
                    Call es.CopyTo(ms)
                End Using

                Return ms.ToArray()
            End Using
        End Using
    End Function

    Private Function SamePrefix(left As Byte(), right As Byte(), count As Integer) As Boolean
        For i As Integer = 0 To System.Math.Min(count, System.Math.Min(left.Length, right.Length)) - 1
            If left(i) <> right(i) Then
                Return False
            End If
        Next

        Return True
    End Function

    ' ========================================================================
    ' 测试用 PNG（不依赖任何绘图库）
    ' ========================================================================

    Private Sub CreateTestPng(imagePath As String, width As Integer, height As Integer)
        Dim signature As Byte() = {&H89, &H50, &H4E, &H47, &HD, &HA, &H1A, &HA}
        Dim raw As New MemoryStream()

        For y As Integer = 0 To height - 1
            Call raw.WriteByte(0) ' 每行的 filter 类型

            For x As Integer = 0 To width - 1
                Call raw.WriteByte(CByte(60 + (x * 8) Mod 160))
                Call raw.WriteByte(CByte(90 + (y * 8) Mod 120))
                Call raw.WriteByte(200)
            Next
        Next

        Dim compressed As New MemoryStream()

        Using zlib As New ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen:=True)
            Dim rawData As Byte() = raw.ToArray()

            Call zlib.Write(rawData, 0, rawData.Length)
        End Using

        Dim ihdr As New MemoryStream()

        Call WriteInt32BE(ihdr, width)
        Call WriteInt32BE(ihdr, height)
        Call ihdr.WriteByte(8)  ' bit depth
        Call ihdr.WriteByte(2)  ' color type: RGB
        Call ihdr.WriteByte(0)  ' compression
        Call ihdr.WriteByte(0)  ' filter
        Call ihdr.WriteByte(0)  ' interlace

        Using fs As New FileStream(imagePath, FileMode.Create)
            Call fs.Write(signature, 0, signature.Length)
            Call WriteChunk(fs, "IHDR", ihdr.ToArray())
            Call WriteChunk(fs, "IDAT", compressed.ToArray())
            Call WriteChunk(fs, "IEND", New Byte() {})
        End Using
    End Sub

    Private Sub WriteChunk(stream As Stream, chunkType As String, data As Byte())
        Call WriteInt32BE(stream, data.Length)

        Dim typeBytes As Byte() = Encoding.ASCII.GetBytes(chunkType)
        Dim crcData(typeBytes.Length + data.Length - 1) As Byte

        Call Array.Copy(typeBytes, 0, crcData, 0, typeBytes.Length)
        Call Array.Copy(data, 0, crcData, typeBytes.Length, data.Length)

        Call stream.Write(typeBytes, 0, typeBytes.Length)
        Call stream.Write(data, 0, data.Length)
        Call WriteInt32BE(stream, CInt(Crc32(crcData) And &H7FFFFFFFUI))
    End Sub

    Private Shared Function Crc32(data As Byte()) As UInteger
        Dim crc As UInteger = &HFFFFFFFFUI

        For Each b As Byte In data
            crc = crc Xor b

            For bit As Integer = 0 To 7
                If (crc And 1UI) <> 0UI Then
                    crc = (crc >> 1) Xor &HEDB88320UI
                Else
                    crc >>= 1
                End If
            Next
        Next

        Return crc Xor &HFFFFFFFFUI
    End Function

    Private Sub WriteInt32BE(stream As Stream, value As Integer)
        Call stream.WriteByte(CByte((value >> 24) And &HFF))
        Call stream.WriteByte(CByte((value >> 16) And &HFF))
        Call stream.WriteByte(CByte((value >> 8) And &HFF))
        Call stream.WriteByte(CByte(value And &HFF))
    End Sub

End Module
