#Region "Microsoft.VisualBasic::d9bba9c4386bfc0d1b3db8f94ae14bae, mime\applicationvnd.openxmlformats-officedocument.wordprocessingml.document\test\Program.vb"

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

    '   Total Lines: 415
    '    Code Lines: 309 (74.46%)
    ' Comment Lines: 55 (13.25%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 51 (12.29%)
    '     File Size: 18.09 KB


    ' Module Program
    ' 
    '     Function: Main
    ' 
    '     Sub: DemoBlockModel, DemoFullFeatures, DemoTextExtraction
    ' 
    ' /********************************************************************************/

#End Region

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
Imports Microsoft.VisualBasic.MIME.Office.WordDocument
Imports Microsoft.VisualBasic.MIME.text.markdown

Module Program

    Function Main(args As String()) As Integer
        Console.OutputEncoding = Encoding.UTF8
        Console.WriteLine("=== Word 文档生成模块 Demo ===")
        Console.WriteLine()

        ' 创建输出目录
        Dim outDir As String = "z:/my-project/WordDocument/output"
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
        ' Demo 3: 学位论文完整结构（覆盖江南大学 2025 规范全部格式项）
        ' ================================================================
        Console.WriteLine("[4] 生成学位论文完整结构文档（覆盖江南大学 2025 规范全部格式项）...")
        DemoThesisFormatting(outDir)
        Console.WriteLine()

        ' ================================================================
        ' Demo 4: 文本提取
        ' ================================================================
        Console.WriteLine("[5] 从 .docx 提取文本...")
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
        doc.DocTitle("2024 年度实验数据分析报告")

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

        Dim imgPath = "G:\OmicsWorks\test\multiple_omics\figures\step06_volcano_proteome_Active_fermentation.png"
        Dim img2Path = "G:\OmicsWorks\test\multiple_omics\figures\step14_pathway_bridge_heatmap.png"

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
    ' Demo 3: 学位论文完整结构演示
    '
    ' 覆盖《江南大学研究生学位论文要求及格式规范（2025年修订）》
    ' "二、学位论文基本结构" + "三、学位论文格式规范" 的全部格式项，
    ' 按规范要求的顺序排列（各部分之间另起页）：
    '
    '   [1] 封面（论文题目黑体小二号居中 + 封面字段；封面整体版式由研究生院提供）
    '   [2] 论文原创性声明和使用授权说明
    '   [3] 中文摘要及关键词："摘  要"三号黑体、中间空2字符、居中、段前段后空1行；
    '       摘要正文小四宋体、1.25 倍行距、首行缩进2字符；
    '       "关键词："小四宋体加粗、另起行缩进2字符，其后 3-8 个关键词
    '   [4] 英文摘要及关键词：Times New Roman，1.25 倍行距，与中文摘要对应
    '   [5] 目录："目  录"三号黑体居中；TOC1-3 条目小四宋体、固定值 20 磅行距、
    '       两端对齐，一/二/三级目录分别左缩进 0/2/4 字符
    '   [6] 图和附表清单（必要时）
    '   [7] 缩写和符号清单（必要时）："缩写和符号清单"三号黑体居中；内容小四、1.25 倍行距
    '   [8] 论文正文（每一章另起页；章标题三号黑体居中段前段后空1行；节标题四号宋体
    '       加粗左对齐段前段后空0.5行；小节标题小四宋体加粗左对齐段前段后空0.5行；
    '       正文小四宋体首行缩进2字符1.25倍行距；中文宋体、西文 Times New Roman；
    '       表：三线表，表题五号居中置于表上方，表格内容五号；
    '       图：图题五号居中置于图下方；公式另行起；注释、列表、任务列表、引用、代码）
    '   [9] 参考文献："参考文献"三号黑体居中段前段后空1行；条目五号宋体 1.25 倍行距，
    '       按 GB/T 7714 著录格式给出 [M]/[J]/[D]/[C]/[N] 示例
    '   [10] 附录A：内容小四宋体 1.25 倍行距
    '   [11] 致谢："致  谢"三号黑体中间空2字符居中；内容小四宋体 1.25 倍行距
    '   [12] 攻读学位期间取得的学术成果清单：标题三号黑体居中；条目按著录格式五号
    '
    ' 页眉页脚（多节实现）：文档划分为 8 个节——
    '   第1节 封面+声明：无页眉页脚；
    '   第2节 前置部分（摘要/Abstract/目录/清单）：页脚大写罗马数字页码（I、II、III……）；
    '   第3-8节 正文各章/参考文献/附录/致谢/成果清单：每节奇数页页眉=章序及章题
    '   （或部分标题）、偶数页页眉="江南大学硕士学位论文"（五号宋体居中），
    '   页脚为居中阿拉伯数字页码，自第一章起从 1 连续编号。
    ' ================================================================
    Private Sub DemoThesisFormatting(outDir As String)
        ' 创建 WordDocument 并应用论文主题（江南大学 2025 规范默认样式）
        Dim doc As New WordDocument(
            author:="张某某",
            title:="基于转录组测序的差异表达基因分析研究",
            tags:={"学位论文", "江南大学", "格式规范"}
        )

        Dim theme As New ThesisFormatting()
        theme.ApplyTo(doc)

        ' 封面字段样式（规范：封面整体样式由研究生院提供，此处按常用版式近似：居中、小三号宋体）
        Dim coverStyle As New WordStyle With {
            .FontName = "Times New Roman",
            .FontNameEastAsia = "宋体",
            .Size = 15,
            .ForeColor = WordColors.Black,
            .Alignment = "center",
            .LineSpacing = 1.5,
            .SpaceBefore = 6,
            .SpaceAfter = 12
        }

        ' 参考文献条目样式（规范：内容用五号字，1.25 倍行距）
        Dim refStyle As WordStyle = theme.ReferenceStyle.Clone()

        Dim outPath As String = Path.Combine(outDir, "thesis_demo.docx")

        ' ============================================================
        ' [1] 封面
        ' ============================================================
        doc.DocTitle("基于转录组测序的差异表达基因分析研究")   ' 黑体小二号，居中
        doc.PageBreak()

        ' ============================================================
        ' [2] 论文原创性声明和使用授权说明（采用学校统一格式）
        ' ============================================================
        doc.WriteBlocks({
            New JSONSchema.Block With {.type = "heading", .level = 1, .content = "原创性声明"},
            New JSONSchema.Block With {.type = "paragraph", .content = "本人声明所呈交的学位论文是本人在导师指导下进行的研究工作及取得的研究成果。尽我所知，除了文中特别加以标注和致谢的地方外，论文中不包含其他人已经发表或撰写过的研究成果，也不包含本人为获得江南大学或其它教育机构的学位或证书而使用过的材料。与我一同工作的同志对本研究所做的任何贡献均已在论文中作了明确的说明并表示谢意。"},
            New JSONSchema.Block With {.type = "paragraph", .content = "作者签名：　　　　　　　　日期：　　　　年　　月　　日"},
            New JSONSchema.Block With {.type = "heading", .level = 1, .content = "关于论文使用授权的说明"},
            New JSONSchema.Block With {.type = "paragraph", .content = "本人完全了解江南大学有关保留、使用学位论文的规定：江南大学有权保留并向国家有关部门或机构送交论文的复印件和磁盘，允许论文被查阅和借阅，可以将学位论文的全部或部分内容编入有关数据库进行检索，可以采用影印、缩印或扫描等复制手段保存、汇编学位论文，并且本人电子文档的内容和纸质版的内容相一致。"},
            New JSONSchema.Block With {.type = "paragraph", .content = "保密的学位论文在解密后也遵守此规定。"},
            New JSONSchema.Block With {.type = "paragraph", .content = "作者签名：　　　　　　　　导师签名：　　　　　　　　"},
            New JSONSchema.Block With {.type = "paragraph", .content = "日期：　　　　年　　月　　日　　日期：　　　　年　　月　　日"}
        })
        doc.PageBreak()

        ' ============================================================
        ' 第 1 节结束（封面+声明：无页眉页脚）；开启第 2 节（前置部分）
        ' ============================================================
        doc.EndSection()

        ' 第 2 节（前置部分）：页脚为大写罗马数字页码（摘要 I、Abstract II、目录 III……）
        doc.FooterPageNumbers(roman:=True)

        ' ============================================================
        ' [3] 中文摘要及关键词
        ' ============================================================
        doc.WriteBlocks({
            New JSONSchema.Block With {.type = "heading", .level = 1, .content = "摘　要"},
            New JSONSchema.Block With {.type = "paragraph", .content = "高通量测序技术的快速发展为基因组学研究提供了海量数据支撑，如何在海量测序数据中准确识别差异表达基因并阐明其生物学功能，已成为生物信息学领域的核心问题之一。本文以本课题组 2024 年度采集的 6 个生物学重复样本的转录组测序数据为研究对象，系统开展了差异表达基因的识别与功能分析研究。"},
            New JSONSchema.Block With {.type = "paragraph", .content = "本文的主要研究工作包括：首先，建立了基于 FastQC 与 Trimmomatic 的原始数据质量控制流程，确保碱基质量值 Q30 比例不低于 85%；其次，采用 STAR aligner 将 cleaned reads 比对至参考基因组，并通过 HTSeq-count 完成基因层面的表达定量；再次，利用 DESeq2 以 |log2FoldChange| > 1 且 padj < 0.05 为筛选标准识别差异表达基因，共鉴定得到 847 个差异表达基因，其中上调 412 个、下调 435 个；最后，通过 GO 功能富集与 KEGG 通路分析，发现差异表达基因显著富集于应激反应、转录调控与信号转导等生物学过程。"},
            New JSONSchema.Block With {.type = "paragraph", .content = "本文建立了完整、可重复的转录组数据分析流程，为后续的关键基因功能验证实验提供了候选基因列表，也为同类型数据的分析提供了方法学参考。"},
            New JSONSchema.Block With {.type = "paragraph", .content = "**关键词：**转录组测序；差异表达基因；生物信息学；功能富集；GO"}
        })
        doc.PageBreak()

        ' ============================================================
        ' [4] 英文摘要及关键词（Times New Roman，1.25 倍行距，与中文摘要对应）
        ' ============================================================
        doc.WriteBlocks({
            New JSONSchema.Block With {.type = "heading", .level = 1, .content = "Abstract"},
            New JSONSchema.Block With {.type = "paragraph", .content = "The rapid development of high-throughput sequencing technology has provided massive data support for genomics research. How to accurately identify differentially expressed genes from massive sequencing data and elucidate their biological functions has become one of the core issues in bioinformatics. In this thesis, transcriptome sequencing data of 6 biological replicate samples collected in 2024 were taken as the research object, and systematic research on the identification and functional analysis of differentially expressed genes was carried out."},
            New JSONSchema.Block With {.type = "paragraph", .content = "A total of 847 differentially expressed genes were identified with |log2FoldChange| > 1 and padj < 0.05 as the screening criteria, including 412 up-regulated and 435 down-regulated genes. GO enrichment and KEGG pathway analysis revealed that the differentially expressed genes were significantly enriched in biological processes such as response to stress, regulation of transcription and signal transduction."},
            New JSONSchema.Block With {.type = "paragraph", .content = "This thesis establishes a complete and reproducible transcriptome data analysis pipeline, which provides a candidate gene list for subsequent functional validation experiments and a methodological reference for the analysis of similar data."},
            New JSONSchema.Block With {.type = "paragraph", .content = "**Keywords:** transcriptome sequencing; differentially expressed genes; bioinformatics; functional enrichment; GO"}
        })
        doc.PageBreak()

        ' ============================================================
        ' [5] 目录（TOC 域：Word 打开后右键"更新域"生成；条目样式 TOC1-3 见 styles.xml）
        ' ============================================================
        doc.Toc(maxLevel:=3)
        doc.PageBreak()

        ' ============================================================
        ' [6] 图和附表清单（必要时）+ [7] 缩写和符号清单（必要时）
        ' ============================================================
        doc.WriteBlocks({
            New JSONSchema.Block With {.type = "heading", .level = 1, .content = "图和附表清单"},
            New JSONSchema.Block With {.type = "list", .ordered = False, .items = {
                "图 2.1 实验技术路线图",
                "表 1.1 主要研究内容一览表",
                "表 2.1 测序数据质量统计摘要表"}},
            New JSONSchema.Block With {.type = "heading", .level = 1, .content = "缩写和符号清单"},
            New JSONSchema.Block With {.type = "deflist",
                .terms = {"RNA-seq", "FDR", "GO", "KEGG", "DESeq2"},
                .definitions = {
                    "转录组测序技术（RNA sequencing），用于分析细胞中所有 mRNA 的表达水平",
                    "False Discovery Rate，错误发现率，多重检验校正后的 p 值",
                    "Gene Ontology，基因本体论，标准化的基因功能分类体系",
                    "Kyoto Encyclopedia of Genes and Genomes，京都基因与基因组百科全书",
                    "基于负二项分布的差异表达基因分析 R 软件包"}
            }
        })
        doc.PageBreak()

        ' ============================================================
        ' 第 2 节结束；开启第 3 节（正文第一章）
        ' ============================================================
        doc.EndSection()

        ' 第 3 节（正文第一章）：页码从 1 重新开始（阿拉伯数字）；
        ' 奇数页页眉 = 章序及章题，偶数页页眉 = 校名（规范：页眉分奇偶页标注）
        doc.HeaderOdd("第一章 绪论")
        doc.HeaderEven("江南大学硕士学位论文")
        doc.FooterPageNumbers(roman:=False, restartAtOne:=True)

        ' ============================================================
        ' [8] 论文正文 —— 第一章（每一章另起页）
        ' ============================================================
        doc.WriteBlocks({
            New JSONSchema.Block With {.type = "heading", .level = 1, .content = "第一章 绪论"},
            New JSONSchema.Block With {.type = "heading", .level = 2, .content = "1.1 研究背景与意义"},
            New JSONSchema.Block With {.type = "paragraph", .content = "本段落在 ThesisFormatting 主题下渲染，正文为小四号宋体，首行缩进两个中文字符，1.25 倍行距，两端对齐；中文使用宋体，英文字母、阿拉伯数字和半角标点使用 Times New Roman，例如 DNA sequencing coverage 250 bp、Q30 ratio 93.2%。"},
            New JSONSchema.Block With {.type = "heading", .level = 3, .content = "1.1.1 国内外研究现状"},
            New JSONSchema.Block With {.type = "paragraph", .content = "近年来，高通量测序技术的快速发展为基因组学研究提供了海量数据支撑，相关研究成果已在 Nature、Science 等国际期刊上大量发表。"},
            New JSONSchema.Block With {.type = "paragraph", .content = "差异表达分析是转录组研究的核心内容之一[注1]，其结果直接关系到后续功能验证实验的候选基因选择。"},
            New JSONSchema.Block With {.type = "footnote", .id = "1", .content = "差异表达分析（Differential Expression Analysis）：通过统计学模型比较不同条件下基因表达水平的差异。"},
            New JSONSchema.Block With {.type = "heading", .level = 4, .content = "1.1.1.1 测序技术的发展"},
            New JSONSchema.Block With {.type = "paragraph", .content = "自 2005 年新一代测序技术问世以来，测序通量呈指数级增长，测序成本持续下降，为转录组学研究在动植物、微生物等各领域的广泛应用奠定了基础。"},
            New JSONSchema.Block With {.type = "heading", .level = 2, .content = "1.2 主要研究内容"},
            New JSONSchema.Block With {.type = "paragraph", .content = "本文的主要研究内容如下表所示："},
            New JSONSchema.Block With {.type = "paragraph", .content = "表 1.1 主要研究内容一览表"},
            New JSONSchema.Block With {.type = "table",
                .headers = {"研究内容", "技术方法", "预期成果"},
                .alignments = {"left", "center", "left"},
                .rows = {New String() {"质量控制与比对", "FastQC + STAR", "高质量比对 BAM 文件"},
                         New String() {"差异表达分析", "DESeq2", "差异基因列表"},
                         New String() {"功能富集分析", "GO + KEGG", "显著富集通路"}}
            },
            New JSONSchema.Block With {.type = "heading", .level = 2, .content = "1.3 论文组织结构"},
            New JSONSchema.Block With {.type = "paragraph", .content = "本文共分为四章：第一章为绪论，介绍研究背景与主要研究内容；第二章介绍材料与实验方法；第三章给出结果与分析；第四章为结论与展望。"},
            New JSONSchema.Block With {.type = "hr"}
        })
        doc.EndSection()

        ' 第 4 节（正文第二章）：奇数页页眉 = 本章章序章题，页码续前节
        doc.HeaderOdd("第二章 材料与方法")
        doc.HeaderEven("江南大学硕士学位论文")
        doc.FooterPageNumbers(roman:=False)

        ' ============================================================
        ' [8] 论文正文 —— 第二章
        ' （公式块说明：规范要求"另行起，缩格书写，编号置于括号内、右端对齐"；
        '   当前模块的 math 块以等宽字体代码块形式近似渲染（另行起），OMML 公式对象为后续扩展）
        ' ============================================================
        doc.WriteBlocks({
            New JSONSchema.Block With {.type = "heading", .level = 1, .content = "第二章 材料与方法"},
            New JSONSchema.Block With {.type = "heading", .level = 2, .content = "2.1 数据来源"},
            New JSONSchema.Block With {.type = "paragraph", .content = "实验数据来源于本课题组 2024 年度采集的 6 个生物学重复样本，测序平台为 Illumina NovaSeq 6000，测序策略为 PE150 双端测序。"},
            New JSONSchema.Block With {.type = "heading", .level = 2, .content = "2.2 技术路线"},
            New JSONSchema.Block With {.type = "image",
                .url = Path.Combine(outDir, "test_chart.png"),
                .alt = "图 2.1 实验技术路线图"
            },
            New JSONSchema.Block With {.type = "paragraph", .content = "图题置于图下方（五号、居中），图编号由""图""和从 1 开始的阿拉伯数字组成，全文编号方式统一。"},
            New JSONSchema.Block With {.type = "heading", .level = 2, .content = "2.3 分析流程"},
            New JSONSchema.Block With {.type = "paragraph", .content = "差异表达分析的核心 R 代码如下："},
            New JSONSchema.Block With {.type = "code",
                .language = "r",
                .content = "library(DESeq2)" & vbCrLf &
                           "dds <- DESeqDataSetFromMatrix(countData = count_matrix, colData = sample_info," & vbCrLf &
                           "                              design = ~ condition)" & vbCrLf &
                           "dds <- DESeq(dds)" & vbCrLf &
                           "res  <- results(dds, alpha = 0.05)"
            },
            New JSONSchema.Block With {.type = "list", .ordered = True, .items = {
                "原始数据质量控制：FastQC 质量评估与 Trimmomatic 接头去除",
                "序列比对：STAR aligner 比对至参考基因组",
                "表达定量：HTSeq-count 统计基因层面 read 计数",
                "差异分析：DESeq2 以 |log2FC| > 1 且 padj < 0.05 为筛选标准"}
            },
            New JSONSchema.Block With {.type = "paragraph", .content = "分析过程中各阶段的质量控制节点如下（已完成为 ☑，进行中为 ☐）："},
            New JSONSchema.Block With {.type = "tasklist",
                .items = {"原始数据质量控制", "序列比对与表达定量", "差异表达分析", "功能富集分析"},
                .checked = {True, True, True, False}
            },
            New JSONSchema.Block With {.type = "heading", .level = 2, .content = "2.4 统计模型"},
            New JSONSchema.Block With {.type = "paragraph", .content = "DESeq2 采用负二项广义线性模型估计基因表达量的离散程度。以两个条件下的均值比较为例，其标准化统计量可表示为："},
            New JSONSchema.Block With {.type = "math", .content = "W_1 = U_11 - U_12 * U_21    (2-1)"},
            New JSONSchema.Block With {.type = "paragraph", .content = "式中，U_11、U_12、U_21 分别为对应条件下的模型估计参数；较长的公式转行时应尽可能在＝处回行，或在＋、－、×、／等记号处换行。"},
            New JSONSchema.Block With {.type = "blockquote", .content = "注：所有分析均在 R 4.3.2 环境下完成，随机数种子统一设置为 42 以保证结果可重复。"},
            New JSONSchema.Block With {.type = "heading", .level = 2, .content = "2.5 测序数据质量统计"},
            New JSONSchema.Block With {.type = "paragraph", .content = "表题置于表上方（五号、居中），表格采用国际通行的三线表，转页接排的续表应重复表头："},
            New JSONSchema.Block With {.type = "paragraph", .content = "表 2.1 测序数据质量统计摘要表"},
            New JSONSchema.Block With {.type = "table",
                .headers = {"样本编号", "原始读段数", "过滤后读段数", "保留率", "Q30 比例"},
                .alignments = {"center", "right", "right", "center", "center"},
                .rows = {New String() {"S001", "4250000", "3810000", "89.6%", "93.2%"},
                         New String() {"S002", "4180000", "3725000", "89.1%", "92.8%"},
                         New String() {"S003", "4320000", "3842000", "88.9%", "93.5%"}}
            },
            New JSONSchema.Block With {.type = "paragraph", .content = "以上流程构成了本文完整的数据分析框架，后续章节将依次展开详细论述。"}
        })
        doc.EndSection()

        ' 第 5 节（参考文献）：奇数页页眉 = 部分标题，页码续前节
        doc.HeaderOdd("参考文献")
        doc.HeaderEven("江南大学硕士学位论文")
        doc.FooterPageNumbers(roman:=False)

        ' ============================================================
        ' [9] 参考文献（标题三号黑体居中；条目五号宋体 1.25 倍行距，GB/T 7714 著录格式）
        ' ============================================================
        doc.WriteBlocks({
            New JSONSchema.Block With {.type = "heading", .level = 1, .content = "参考文献"},
            New JSONSchema.Block With {.type = "paragraph", .content = "（以下条目依照在正文中被首次引用的先后次序排列，使用 ReferenceStyle 五号样式逐条写入）"}
        })
        doc.Paragraph("[1] 张伯伟. 全唐五代诗格汇考[M]. 南京：江苏古籍出版社，2002：288.", refStyle)
        doc.Paragraph("[2] 张群，程志宝，石志飞. 惯性增强动力吸振器-浮置板轨道低频减振性能研究[J]. 铁道学报，2024，46（8）：102-111.", refStyle)
        doc.Paragraph("[3] 王琦. 融合星载GNSS-R和SAR数据的高时空分辨率土壤湿度反演方法研究[D]. 武汉：武汉大学，2022：87.", refStyle)
        doc.Paragraph("[4] 王莉，牟蕾，宋涛，等. 新冠疫情常态化管理期间新疆地区医务人员心理健康状况研究[C]. 北京：中华预防医学会第32次全国医院感染学术年会，2023.", refStyle)
        doc.Paragraph("[5] 丁文详. 数字革命与竞争国际化[N]. 中国青年报，2000-11-20（15）.", refStyle)
        doc.Paragraph("[6] SHINOTSUKA H，NAGATA K，SIRIWARDANA M，et al. Sample structure prediction from measured XPS data using Bayesian estimation and SESSA simulator[J/OL]. Journal of electron spectroscopy and related phenomena，2023，267：147370.", refStyle)
        doc.EndSection()

        ' 第 6 节（附录A）
        doc.HeaderOdd("附录A 主要分析脚本清单")
        doc.HeaderEven("江南大学硕士学位论文")
        doc.FooterPageNumbers(roman:=False)

        ' ============================================================
        ' [10] 附录A（内容小四宋体 1.25 倍行距；附录中的公式及图表编号冠以附录序号字母）
        ' ============================================================
        doc.WriteBlocks({
            New JSONSchema.Block With {.type = "heading", .level = 1, .content = "附录A 主要分析脚本清单"},
            New JSONSchema.Block With {.type = "paragraph", .content = "附录中的公式及图表编号冠以附录序号字母加一短划线，如公式（A-1）、图 A-1、表 A-1 等。"},
            New JSONSchema.Block With {.type = "paragraph", .content = "本附录列出论文正文所涉及的主要分析脚本及其功能说明，包括原始数据质量控制脚本 qc_pipeline.sh、序列比对脚本 align_run.sh、差异表达分析脚本 deseq_analysis.R 以及功能富集分析脚本 enrichment.R。"},
            New JSONSchema.Block With {.type = "paragraph", .content = "全部脚本均在 Ubuntu 22.04 LTS 环境下测试通过，运行顺序及参数配置详见脚本头部注释。"}
        })
        doc.EndSection()

        ' 第 7 节（致谢）
        doc.HeaderOdd("致　谢")
        doc.HeaderEven("江南大学硕士学位论文")
        doc.FooterPageNumbers(roman:=False)

        ' ============================================================
        ' [11] 致谢（"致  谢"三号黑体中间空2字符居中；内容小四宋体 1.25 倍行距）
        ' ============================================================
        doc.WriteBlocks({
            New JSONSchema.Block With {.type = "heading", .level = 1, .content = "致　谢"},
            New JSONSchema.Block With {.type = "paragraph", .content = "本论文是在导师李某某教授的悉心指导下完成的。从论文的选题、研究方案的制定到论文的撰写与修改，导师都倾注了大量的心血，在此谨向导师表示崇高的敬意和衷心的感谢。"},
            New JSONSchema.Block With {.type = "paragraph", .content = "感谢课题组各位同学在实验数据采集与处理过程中给予的帮助与支持；感谢江南大学生物工程学院为本研究提供的实验平台与技术条件；感谢国家自然科学基金项目对本研究的资助。"},
            New JSONSchema.Block With {.type = "paragraph", .content = "最后，感谢家人一直以来的理解、支持与鼓励。"}
        })
        doc.EndSection()

        ' 第 8 节（学术成果清单，最后一节）：节属性由文档末尾的 body 级 sectPr 描述
        doc.HeaderOdd("攻读学位期间取得的学术成果清单")
        doc.HeaderEven("江南大学硕士学位论文")
        doc.FooterPageNumbers(roman:=False)

        ' ============================================================
        ' [12] 攻读学位期间取得的学术成果清单
        ' ============================================================
        doc.WriteBlocks({
            New JSONSchema.Block With {.type = "heading", .level = 1, .content = "攻读学位期间取得的学术成果清单"},
            New JSONSchema.Block With {.type = "paragraph", .content = "（学术论文按著录格式依时间顺序排列，并列出全部作者名字）"}
        })
        doc.Paragraph("[1] 张某某，李某某. 基于转录组测序的差异表达基因分析研究[J]. 生物信息学学报，2025，23（3）：45-56.", refStyle)
        doc.Paragraph("[2] 张某某，王某某，李某某. 植物抗逆相关基因的功能验证[C]. 上海：第15届全国基因组学大会，2024.", refStyle)

        doc.Save(outPath)
        Console.WriteLine($"  已保存: {outPath}")
        Console.WriteLine("  覆盖格式项：封面/原创性声明/中英文摘要及关键词/目录/图表与缩写清单/")
        Console.WriteLine("              正文(章-节-小节-正文-三线表-图-公式-注释-列表-任务列表-引用-代码)/")
        Console.WriteLine("              参考文献/附录/致谢/学术成果清单")
        Console.WriteLine("  页眉页脚：奇数页章序章题、偶数页校名（五号宋体居中）；")
        Console.WriteLine("            前置部分大写罗马数字页码、正文从1起阿拉伯数字页码（8 节）")
    End Sub

    ' ================================================================
    ' Demo 4: 从 .docx 提取文本
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
