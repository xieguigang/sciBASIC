' ============================================================================
' Program.vb - Word 文档生成模块 Demo
'
' 展示 WordDocument 的所有功能：
'   1. 设置文档元数据和样式
'   2. 标题、TOC、分页
'   3. 各级标题和段落
'   4. 表格（带样式）
'   5. 图片插入
'   6. 代码块、引用、列表
'   7. Block 模型兼容
'   8. 文本提取（读回 docx）
' ============================================================================

Imports System.IO
Imports System.Text
Imports WordDocument.JSONSchema

Module Program

    Function Main(args As String()) As Integer
        Console.OutputEncoding = Encoding.UTF8
        Console.WriteLine("=== Word 文档生成模块 Demo ===")
        Console.WriteLine()

        ' 创建输出目录
        Dim outDir As String = "/home/z/my-project/WordDocument/output"
        If Not Directory.Exists(outDir) Then Directory.CreateDirectory(outDir)

        ' 生成测试图片
        Console.WriteLine("[1] 生成测试图片...")
        Dim imgPath As String = Path.Combine(outDir, "test_chart.png")
        ImageHelper.CreateTestPng(imgPath, 600, 400, &H4A, &H90, &HE5)
        Dim img2Path As String = Path.Combine(outDir, "test_diagram.png")
        ImageHelper.CreateTestPng(img2Path, 400, 300, &HE5, &H90, &H4A)
        Console.WriteLine($"  -> {imgPath}")
        Console.WriteLine($"  -> {img2Path}")
        Console.WriteLine()

        ' ================================================================
        ' Demo 1: 完整功能演示
        ' ================================================================
        Console.WriteLine("[2] 生成完整功能演示文档...")
        DemoFullFeatures(outDir)
        Console.WriteLine()

        ' ================================================================
        ' Demo 2: Block 模型兼容
        ' ================================================================
        Console.WriteLine("[3] 生成 Block 模型兼容文档...")
        DemoBlockModel(outDir)
        Console.WriteLine()

        ' ================================================================
        ' Demo 3: 文本提取
        ' ================================================================
        Console.WriteLine("[4] 从 .docx 提取文本...")
        DemoTextExtraction(outDir)
        Console.WriteLine()

        Console.WriteLine("=== Demo 完成 ===")
        Return 0
    End Function

    ' ================================================================
    ' Demo 1: 完整功能演示
    ' ================================================================
    Private Sub DemoFullFeatures(outDir As String)
        Dim doc As New WordDocument(
            author:="科研数据分析系统",
            title:="2024年度实验数据分析报告",
            tags:={"数据分析", "实验报告", "Q4"},
            subject:="生物医学实验数据分析",
            description:="本报告包含第四季度全部实验数据的统计分析和可视化结果。"
        )

        ' 设置页面 (A4, 1 英寸边距)
        doc.PageSetupA4()

        ' 设置样式
        doc.HeadingStyle(1, New WordStyle With {
            .FontName = "Microsoft YaHei",
            .FontNameEastAsia = "Microsoft YaHei",
            .Size = 25,
            .Bold = True,
            .ForeColor = WordColors.DarkBlue,
            .SpaceBefore = 12,
            .SpaceAfter = 8
        }).HeadingStyle(2, New WordStyle With {
            .FontName = "Microsoft YaHei",
            .FontNameEastAsia = "Microsoft YaHei",
            .Size = 20,
            .Bold = True,
            .ForeColor = WordColors.Heading2Color,
            .SpaceBefore = 10,
            .SpaceAfter = 6
        }).HeadingStyle(3, New WordStyle With {
            .FontName = "Microsoft YaHei",
            .FontNameEastAsia = "Microsoft YaHei",
            .Size = 16,
            .Bold = True,
            .ForeColor = WordColors.Heading3Color
        }).ParagraphStyle(New WordStyle With {
            .FontName = "Calibri",
            .FontNameEastAsia = "Microsoft YaHei",
            .Size = 12,
            .LineSpacing = 1.5,
            .SpaceAfter = 8,
            .FirstLineIndent = 24
        }).TableStyle(New TableStyle With {
            .HeaderBackColor = "4472C4",
            .HeaderForeColor = "FFFFFF",
            .HeaderBold = True,
            .BorderColor = "8EAADB",
            .BorderSize = 4,
            .AltRowBackColor = "D6E4F0"
        })

        ' 文档标题
        doc.Title("2024 年度实验数据分析报告")

        ' 目录
        doc.Toc(maxLevel:=3)

        ' 分页
        doc.PageBreak()

        ' 第一章
        doc.H1("第一章 研究概述")

        doc.H2("1.1 研究背景")
        doc.Paragraph("本研究旨在分析第四季度采集的生物医学实验数据，通过对基因组表达谱的统计分析，识别差异表达基因并验证其生物学功能。实验采用高通量测序技术，共获得 3,200 万条有效读段，覆盖 28,000 余个基因位点。")
        doc.Paragraph("数据分析流程包括：质量控制（FastQC + Trimmomatic）、序列比对（STAR aligner）、表达定量（HTSeq-count）、差异分析（DESeq2）、功能富集分析（GO + KEGG）。")

        doc.H2("1.2 研究方法")
        doc.Paragraph("下表列出了本研究使用的主要分析工具及其版本信息：")

        doc.Table(
            {"工具名称", "版本", "用途", "引用"},
            {{"FastQC", "v0.12.1", "质量控制", "Andrews (2010)"},
             {"Trimmomatic", "v0.39", "接头去除", "Bolger et al. (2014)"},
             {"STAR", "2.7.11a", "序列比对", "Dobin et al. (2013)"},
             {"HTSeq-count", "v2.0.2", "表达定量", "Anders et al. (2015)"},
             {"DESeq2", "v1.40.2", "差异分析", "Love et al. (2014)"}},
            {"left", "center", "left", "center"}
        )

        doc.H3("1.2.1 质量控制标准")
        doc.Paragraph("所有样本的测序质量需满足以下标准方可用进入下游分析：")
        doc.List({
            "碱基质量值 Q30 比例 ≥ 85%",
            "GC 含量分布在理论值 ± 5% 范围内",
            "接头序列污染比例 ≤ 1%",
            "重复序列比例 ≤ 20%"
        }, ordered:=True)

        ' 分页
        doc.PageBreak()

        ' 第二章
        doc.H1("第二章 结果与分析")

        doc.H2("2.1 测序数据质量统计")
        doc.Paragraph("经过质量控制和过滤，共保留 2,847 万条高质量读段，平均保留率为 89.0%。下表为各样本的质量统计摘要：")

        doc.Table(
            {"样本编号", "原始读段数", "过滤后读段数", "保留率", "Q30 比例"},
            {{"S001", "4,250,000", "3,810,000", "89.6%", "93.2%"},
             {"S002", "4,180,000", "3,725,000", "89.1%", "92.8%"},
             {"S003", "4,320,000", "3,842,000", "88.9%", "93.5%"},
             {"S004", "4,090,000", "3,658,000", "89.4%", "94.1%"},
             {"S005", "4,510,000", "4,012,000", "88.9%", "93.8%"},
             {"S006", "4,280,000", "3,796,000", "88.7%", "93.0%"}},
            {"center", "right", "right", "center", "center"}
        )

        doc.H2("2.2 差异表达基因分析")
        doc.Paragraph("使用 DESeq2 进行差异表达分析，以 |log2FoldChange| > 1 且 adjusted p-value < 0.05 为筛选标准。结果显示，共有 847 个基因差异表达，其中上调 412 个，下调 435 个。")

        doc.Image(imgPath, width:=450, caption:="图 1. 差异表达基因火山图")
        doc.Image(img2Path, width:=350, caption:="图 2. 基因表达聚类热图")

        doc.H2("2.3 GO 功能富集分析")
        doc.Paragraph("对差异表达基因进行 Gene Ontology (GO) 功能富集分析，主要富集在以下生物学过程中：")

        doc.Table(
            {"GO 术语", "描述", "基因数", "P 值", "FDR"},
            {{"GO:0006950", "应激反应", "68", "1.2E-12", "3.4E-09"},
             {"GO:0006355", "转录调控", "55", "3.8E-10", "5.7E-07"},
             {"GO:0007165", "信号转导", "49", "2.1E-08", "1.9E-05"},
             {"GO:0006915", "细胞凋亡", "42", "8.5E-08", "5.2E-05"},
             {"GO:0008283", "细胞增殖", "38", "1.4E-07", "7.1E-05"}},
            {"center", "left", "center", "right", "right"}
        )

        ' 分页
        doc.PageBreak()

        ' 第三章
        doc.H1("第三章 讨论")
        doc.H2("3.1 主要发现")
        doc.Paragraph("本研究通过系统的转录组分析，鉴定出 847 个差异表达基因。其中应激反应相关基因显著富集，表明实验处理引起了细胞应激反应通路的激活。")
        doc.Blockquote("转录组数据的可重复性是确保生物学结论可靠性的关键因素之一。在本研究中，生物学重复样本间的皮尔逊相关系数均大于 0.95，表明数据具有高度可重复性。")

        doc.H2("3.2 代码示例")
        doc.Paragraph("以下是差异表达分析的核心 R 代码：")
        doc.CodeBlock(
"library(DESeq2)

' 创建 DESeqDataSet
dds <- DESeqDataSetFromMatrix(
  countData = count_matrix,
  colData   = sample_info,
  design    = ~ condition
)

' 差异分析
dds <- DESeq(dds)
results <- results(dds,
  contrast     = c(""condition"", ""treated"", ""control""),
  alpha        = 0.05,
  lfcThreshold = 1
)

' 筛选显著差异基因
sig_genes <- subset(results, padj < 0.05 & abs(log2FoldChange) > 1)",
            "r"
        )

        doc.H2("3.3 局限性")
        doc.Hr()
        doc.Paragraph("本研究存在以下局限性：")
        doc.List({
            "样本量较小（n=6），统计功效有限",
            "仅进行了转录组层面的分析，未验证蛋白质水平的变化",
            "未考虑长链非编码 RNA 的表达变化",
            "功能验证实验尚在进行中"
        }, ordered:=True)

        ' 分页
        doc.PageBreak()

        ' 第四章
        doc.H1("第四章 结论")
        doc.Paragraph("本研究通过系统的转录组分析，鉴定出 847 个差异表达基因，并发现应激反应通路显著激活。这些发现为后续的功能验证实验提供了候选基因列表。未来工作将聚焦于关键基因的功能验证及其在疾病发生发展中的作用机制。")

        doc.H2("参考文献")
        doc.List({
            "[1] Love MI, Huber W, Anders S. Moderated estimation of fold change and dispersion for RNA-seq data with DESeq2. Genome Biology. 2014;15(12):550.",
            "[2] Dobin A, Davis CA, Schlesinger F, et al. STAR: ultrafast universal RNA-seq aligner. Bioinformatics. 2013;29(1):15-21.",
            "[3] Bolger AM, Lohse M, Usadel B. Trimmomatic: a flexible trimmer for Illumina sequence data. Bioinformatics. 2014;30(15):2114-2120."
        }, ordered:=False)

        ' 保存
        Dim outPath As String = Path.Combine(outDir, "demo_full_report.docx")
        doc.Save(outPath)
        Console.WriteLine($"  已保存: {outPath}")
    End Sub

    ' ================================================================
    ' Demo 2: Block 模型兼容
    ' ================================================================
    Private Sub DemoBlockModel(outDir As String)
        ' 创建 Block 列表 (模拟用户现有的 JSONSchema.Block 模型)
        Dim blocks As New List(Of JSONSchema.Block) From {
            New JSONSchema.Block With {
                .type = "heading",
                .level = 1,
                .content = "Block 模型兼容测试"
            },
            New JSONSchema.Block With {
                .type = "paragraph",
                .content = "本段落通过 JSONSchema.Block 模型生成，验证 WordDocument 的 WriteBlocks 方法对用户现有 markdown block 渲染模型的兼容性。"
            },
            New JSONSchema.Block With {
                .type = "heading",
                .level = 2,
                .content = "2.1 代码块测试"
            },
            New JSONSchema.Block With {
                .type = "code",
                .language = "vbnet",
                .content = "Dim doc As New WordDocument() aligner" & vbCrLf &
                           "doc.H1(""标题"").Paragraph(""正文"")" & vbCrLf &
                           "doc.Save(""output.docx"")"
            },
            New JSONSchema.Block With {
                .type = "heading",
                .level = 2,
                .content = "2.2 列表测试"
            },
            New JSONSchema.Block With {
                .type = "list",
                .ordered = True,
                .items = {"第一项：有序列表", "第二项：有序列表", "第三项：有序列表"}
            },
            New JSONSchema.Block With {
                .type = "list",
                .ordered = False,
                .items = {"无序项 A", "无序项 B", "无序项 C"}
            },
            New JSONSchema.Block With {
                .type = "heading",
                .level = 2,
                .content = "2.3 表格测试"
            },
            New JSONSchema.Block With {
                .type = "table",
                .headers = {"参数", "值", "单位"},
                .alignments = {"left", "center", "right"},
                .rows = {New String() {"温度", "37.5", "°C"},
                         New String() {"pH", "7.4", ""},
                         New String() {"湿度", "65", "%"}}
            },
            New JSONSchema.Block With {
                .type = "heading",
                .level = 2,
                .content = "2.4 引用和分割线"
            },
            New JSONSchema.Block With {
                .type = "blockquote",
                .content = "这是一段引用文字。引用块在 Word 中会显示为左侧带彩色边框的缩进段落，文字为斜体。"
            },
            New JSONSchema.Block With {
                .type = "hr"
            },
            New JSONSchema.Block With {
                .type = "paragraph",
                .content = "上方为水平分割线。"
            },
            New JSONSchema.Block With {
                .type = "heading",
                .level = 2,
                .content = "2.5 定义列表"
            },
            New JSONSchema.Block With {
                .type = "deflist",
                .terms = {"RNA-seq", "FDR", "GO"},
                .definitions = {"转录组测序技术，用于分析细胞中所有 mRNA 的表达水平",
                                "False Discovery Rate，错误发现率，多重检验校正后的 p 值",
                                "Gene Ontology，基因本体论，标准化的基因功能分类体系"}
            },
            New JSONSchema.Block With {
                .type = "heading",
                .level = 2,
                .content = "2.6 任务列表"
            },
            New JSONSchema.Block With {
                .type = "tasklist",
                .items = {"完成数据分析", "撰写报告初稿", "导师审阅", "终稿提交"},
                .checked = {True, True, False, False}
            }
        }

        ' 创建 WordDocument 并写入 Block 列表
        Dim doc As New WordDocument(
            author:="Block 模型测试",
            title:="Block 模型兼容测试文档",
            tags:={"Block", "Markdown", "兼容性"}
        )

        doc.PageSetupA4()
        doc.WriteBlocks(blocks)

        Dim outPath As String = Path.Combine(outDir, "demo_blocks.docx")
        doc.Save(outPath)
        Console.WriteLine($"  已保存: {outPath}")
    End Sub

    ' ================================================================
    ' Demo 3: 从 .docx 提取文本
    ' ================================================================
    Private Sub DemoTextExtraction(outDir As String)
        Dim reader As New DocxTextReader()
        Dim docxPath As String = Path.Combine(outDir, "demo_full_report.docx")

        Console.WriteLine($"  读取: {docxPath}")
        Console.WriteLine()

        ' 提取元数据
        Dim meta As Dictionary(Of String, String) = reader.ExtractMetadata(docxPath)
        Console.WriteLine("  文档元数据:")
        For Each kvp As KeyValuePair(Of String, String) In meta
            Console.WriteLine($"    {kvp.Key}: {kvp.Value}")
        Next
        Console.WriteLine()

        ' 提取文本
        Dim text As String = reader.ExtractText(docxPath)
        Console.WriteLine("  提取的文本内容 (前 800 字):")
        Console.WriteLine(New String("-"c, 60))
        If text.Length > 800 Then
            Console.WriteLine(text.Substring(0, 800) & "...")
        Else
            Console.WriteLine(text)
        End If
        Console.WriteLine(New String("-"c, 60))
        Console.WriteLine($"  总字符数: {text.Length}")
        Console.WriteLine()

        ' 提取段落数组
        Dim paragraphs As String() = reader.ExtractParagraphs(docxPath)
        Console.WriteLine($"  段落数: {paragraphs.Length}")

        ' 保存提取的文本
        Dim txtPath As String = Path.Combine(outDir, "extracted_text.txt")
        File.WriteAllText(txtPath, text, Encoding.UTF8)
        Console.WriteLine($"  纯文本已保存: {txtPath}")
    End Sub

End Module
