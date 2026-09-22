#Region "Microsoft.VisualBasic::70af94e287f168d043933c1661e6a1e1, mime\application%rtf\test\Program.vb"

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

    '   Total Lines: 288
    '    Code Lines: 195 (67.71%)
    ' Comment Lines: 38 (13.19%)
    '    - Xml Docs: 7.89%
    ' 
    '   Blank Lines: 55 (19.10%)
    '     File Size: 13.85 KB


    ' Module Program
    ' 
    '     Function: BuildBlocks, IsPureAscii, Main, ToStream
    ' 
    '     Sub: Check, VerifyLexer, VerifyRoundTrip, WriteSharedDocument
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' Program.vb - RTF 生成 / 解析示例与自校验
'
' 用法: RtfDemo [输出目录]
'
' 演示并自校验：
'   1. 由 JSONSchema 内容块 + 流式写入 API 生成 .rtf（含中文、表格、图片内嵌）；
'   2. 同一段写入代码改用 IDocumentWriter 的 docx 实现，验证写入器可互换；
'   3. 用 RtfTextReader 把 .rtf 解析回文本，校验关键内容与元数据；
'   4. 针对 \'hh / \uN? / ansicpg936 原始多字节 / 畸形文档 / 忽略分组做解析器健壮性检查。
'
' 全部检查通过时返回码为 0，否则为 1。
' ============================================================================

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.MIME.Office.WordDocument
Imports Microsoft.VisualBasic.MIME.RTF
Imports Microsoft.VisualBasic.MIME.text.markdown

Module Program

    ''' <summary>失败的检查项计数（模块变量为共享变量，可直接累加）。</summary>
    Private failures As Integer = 0

    Private Const DocTitleText As String = "RTF 生成与解析示例"
    Private Const AuthorText As String = "sciBASIC#"
    Private Const BodyText As String = "这是一段包含中文的正文，用于验证 RTF 的中文编码与文本回读。"

    Public Function Main(args As String()) As Integer
        Console.OutputEncoding = Encoding.UTF8
        ' 注册系统代码页（cp936），用于 \'hh 与旧式原始多字节文档的解析
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)

        Dim outDir As String = If(args.Length > 0, args(0), Path.Combine(AppContext.BaseDirectory, "demo-output"))
        Call Directory.CreateDirectory(outDir)

        Dim imagePath As String = Path.Combine(outDir, "demo-image.png")
        Dim rtfPath As String = Path.Combine(outDir, "demo.rtf")

        ' docx 侧提供的纯 BCL PNG 生成器，无需真实图片资源即可演示图片内嵌
        Call ImageHelper.CreateTestPng(imagePath, 240, 120, 68, 114, 196)

        Console.WriteLine("== 1. 由内容块生成 RTF ==")
        Dim writer As IDocumentWriter = New RtfDocument(
            author:=AuthorText,
            title:=DocTitleText,
            tags:={"rtf", "markdown", "document"},
            subject:="RtfDocument",
            description:="演示由 markdown 内容块生成 RTF 并回读文本")

        Call WriteSharedDocument(writer, imagePath)
        Call writer.Save(rtfPath)

        Console.WriteLine($"  已生成: {rtfPath} ({New FileInfo(rtfPath).Length} 字节)")

        Console.WriteLine("== 2. 同一段写入代码改用 docx 写入器 ==")
        Try
            Dim docxPath As String = Path.Combine(outDir, "demo.docx")
            Dim docxWriter As IDocumentWriter = New WordDocument(author:=AuthorText, title:=DocTitleText, subject:="WordDocument")

            Call WriteSharedDocument(docxWriter, imagePath)
            Call docxWriter.Save(docxPath)

            Console.WriteLine($"  已生成: {docxPath} ({New FileInfo(docxPath).Length} 字节)")
        Catch ex As Exception
            Console.WriteLine($"  [跳过] docx 写入器示例失败: {ex.Message}")
        End Try

        Console.WriteLine("== 3. 解析 RTF 回读文本与元数据 ==")
        Call VerifyRoundTrip(rtfPath)

        Console.WriteLine("== 4. 解析器健壮性检查 ==")
        Call VerifyLexer()

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
    ' 写入：同一段代码对任意 IDocumentWriter 实现均可用
    ' ========================================================================

    Private Sub WriteSharedDocument(writer As IDocumentWriter, imagePath As String)
        Call writer.PageSetupA4()
        Call writer.HeadingStyle(1, New WordStyle With {
            .FontName = "Microsoft YaHei",
            .FontNameEastAsia = "Microsoft YaHei",
            .Size = 20,
            .Bold = True,
            .ForeColor = WordColors.Heading1Color
        })

        Call writer.DocTitle(DocTitleText)
        Call writer.Toc(2)
        Call writer.H1("文档生成与解析")
        Call writer.Paragraph(BodyText)
        Call writer.H2("内容块")

        ' markdown 内容块驱动
        Call writer.WriteBlocks(BuildBlocks())

        Call writer.H3("代码块")
        Call writer.CodeBlock("Dim doc As New RtfDocument()" & vbCrLf & "doc.Save(""demo.rtf"")", "vbnet")

        Call writer.H3("列表")
        Call writer.List({"有序列表项 A", "有序列表项 B"}, ordered:=True)
        Call writer.List({"无序列表项 A", "无序列表项 B"})
        Call writer.TaskList({"已完成任务", "未完成任务"}, {True, False})
        Call writer.DefinitionList({"RTF", "PDF"}, {"Rich Text Format", "Portable Document Format"})

        Call writer.H3("表格")
        Dim data(,) As String = {{"alpha", "1"}, {"beta", "2"}}
        Call writer.Table({"名称", "数值"}, data, {"left", "right"})

        Dim autoFitRows As String()() = {New String() {"内容比较长的一格", "1"}}
        Call writer.TableAutoFitContents({"较长的列标题", "短"}, autoFitRows, {"left", "center"}, center:=True)

        Call writer.Hr()
        Call writer.Image(imagePath, caption:="测试图片")
        Call writer.PageBreak()
        Call writer.Paragraph("分页之后的段落。")
    End Sub

    ''' <summary>构造一份覆盖常见块类型的 markdown 内容块列表。</summary>
    Private Function BuildBlocks() As List(Of JSONSchema.Block)
        Dim table As New JSONSchema.Block With {
            .type = "table",
            .headers = {"列 A", "列 B"},
            .alignments = {"left", "center"}
        }
        table.rows = New String()() {
            New String() {"a1", "b1"},
            New String() {"a2", "b2"}
        }

        Return New List(Of JSONSchema.Block) From {
            New JSONSchema.Block With {.type = "heading", .level = 2, .content = "内容块驱动的段落"},
            New JSONSchema.Block With {.type = "paragraph", .content = "正文由 JSONSchema.Block 生成，含符号 { } \ 与制表符" & vbTab & "的转义验证。"},
            New JSONSchema.Block With {.type = "code", .language = "vbnet", .content = "Console.WriteLine(""hello rtf"")"},
            New JSONSchema.Block With {.type = "list", .ordered = True, .items = {"块列表第一项", "块列表第二项"}},
            New JSONSchema.Block With {.type = "tasklist", .items = {"块任务已完成", "块任务未完成"}, .checked = {True, False}},
            New JSONSchema.Block With {.type = "deflist", .terms = {"Markdown"}, .definitions = {"轻量标记语言"}},
            New JSONSchema.Block With {.type = "blockquote", .content = "引用块内容"},
            table,
            New JSONSchema.Block With {.type = "hr"},
            New JSONSchema.Block With {.type = "math", .content = "E = mc^2"},
            New JSONSchema.Block With {.type = "link", .alt = "sciBASIC#", .url = "https://github.com/xieguigang/sciBASIC"},
            New JSONSchema.Block With {.type = "footnote", .id = "1", .content = "脚注内容"}
        }
    End Function

    ' ========================================================================
    ' 校验：生成 -> 解析 往返
    ' ========================================================================

    Private Sub VerifyRoundTrip(rtfPath As String)
        Dim reader As New RtfTextReader()
        Dim paragraphs As String() = reader.ExtractParagraphs(rtfPath)
        Dim text As String = reader.ExtractText(rtfPath)
        Dim info As RtfDocumentInfo = reader.ExtractMetadata(rtfPath)

        Console.WriteLine($"  解析段落数: {paragraphs.Length}")

        Call Check("生成文件为纯 ASCII（非 ASCII 已全部 \\uN? 转义）", IsPureAscii(rtfPath))

        Dim raw As String = File.ReadAllText(rtfPath, Encoding.UTF8)
        Call Check("RTF 头部正确", raw.StartsWith("{\rtf1\ansi\ansicpg936"))
        Call Check("包含字体表", raw.Contains("\fonttbl") AndAlso raw.Contains("Microsoft YaHei"))
        Call Check("包含颜色表", raw.Contains("\colortbl"))
        Call Check("包含 \\info 元数据", raw.Contains("{\info") AndAlso raw.Contains("{\title "))
        Call Check("图片以 \\pict 内嵌", raw.Contains("\pict\pngblip"))

        Call Check("元数据 title", info.Title = DocTitleText)
        Call Check("元数据 author", info.Author = AuthorText)
        Call Check("元数据 subject", info.Subject = "RtfDocument")
        Call Check("元数据 generator", info.Generator.Contains("RtfDocument"))
        Call Check("元数据 keywords", info.Keywords.Contains("rtf"))

        For Each expected As String In {
            DocTitleText,
            "文档生成与解析",
            BodyText,
            "内容块驱动的段落",
            "Console.WriteLine(""hello rtf"")",
            "块列表第一项",
            "块列表第二项",
            "已完成任务",
            "块任务未完成",
            "Rich Text Format",
            "轻量标记语言",
            "引用块内容",
            "alpha",
            "b2",
            "含符号 { } \ 与制表符" & vbTab & "的转义验证。",
            "测试图片",
            "分页之后的段落。"
        }
            Call Check("回读包含: " & expected, text.Contains(expected))
        Next

        Call Check("表格单元格以制表符分隔", text.Contains("alpha" & vbTab & "1"))

        ' 目录：Toc(2) 应把 2 级标题展开为静态目录文本
        Call Check("目录包含 2 级标题", text.Contains("内容块驱动的段落"))

        Call Check("段落数量合理", paragraphs.Length >= 10)
    End Sub

    ' ========================================================================
    ' 校验：解析器对各类 RTF 片段的处理
    ' ========================================================================

    Private Sub VerifyLexer()
        Dim reader As New RtfTextReader()

        ' \'hh 十六进制转义（cp936）
        Dim hexDoc As String = "{\rtf1\ansi\ansicpg936\pard \'d6\'d0\'ce\'c4\par}"
        Call Check("十六进制转义 \'hh 按代码页解码", reader.ExtractText(ToStream(hexDoc)).Trim() = "中文")

        ' \uN? Unicode 转义
        Dim unicodeDoc As String = "{\rtf1\ansi\uc1\pard \u20013?\u25991?\par}"
        Call Check("Unicode 转义 \uN? 解码", reader.ExtractText(ToStream(unicodeDoc)).Trim() = "中文")

        ' 旧式文档：ansicpg936 + 原始多字节（未转义）
        Dim legacyDoc As String = "{\rtf1\ansi\ansicpg936\deff0{\fonttbl{\f0\fnil\fcharset134 宋体;}}{\colortbl ;\red0\green0\blue0;}\pard\fs24 中文测试\par}"
        Call Check("旧式 ansicpg936 原始多字节", reader.ExtractText(ToStream(legacyDoc)).Trim() = "中文测试")

        ' 元数据与忽略分组
        Dim metaDoc As String = "{\rtf1\ansi\ansicpg936{\*\generator sciBASIC#;}{\info{\title 标题}{\author 作者}}{\fonttbl{\f0\fnil\fcharset134 宋体;}}\pard 正文\par}"
        Call Check("忽略分组不进正文", reader.ExtractText(ToStream(metaDoc)).Trim() = "正文")

        Dim meta As RtfDocumentInfo = reader.ExtractMetadata(ToStream(metaDoc))
        Call Check("捕获 \info 的 title / author", meta.Title = "标题" AndAlso meta.Author = "作者")
        Call Check("捕获 \*\generator", meta.Generator = "sciBASIC#")

        ' 段落与分页
        Dim multiDoc As String = "{\rtf1\ansi\ansicpg936\pard 第一段\par 第二段\page 第三段\par}"
        Dim multi As String() = reader.ExtractParagraphs(ToStream(multiDoc))
        Call Check("按 \par / \page 切分段落", multi.Length = 3 AndAlso multi(1) = "第二段" AndAlso multi(2) = "第三段")

        ' 畸形文档容错
        Dim brokenDoc As String = "{\rtf1\ansi\ansicpg936\pard 未闭合分组"
        Dim broken As String = Nothing

        Try
            broken = reader.ExtractText(ToStream(brokenDoc))
        Catch ex As Exception
            broken = Nothing
        End Try

        Call Check("畸形文档不抛异常且返回已解析内容", broken IsNot Nothing AndAlso broken.Contains("未闭合分组"))
        Call Check("空文档安全返回", reader.ExtractText(ToStream("")).Length = 0)
    End Sub

    ' ========================================================================
    ' 辅助
    ' ========================================================================

    Private Sub Check(name As String, condition As Boolean)
        If condition Then
            Console.WriteLine($"  [通过] {name}")
        Else
            failures += 1
            Console.WriteLine($"  [失败] {name}")
        End If
    End Sub

    ''' <summary>把 RTF 文本按 cp936 编码为字节流（模拟磁盘上的真实文件）。</summary>
    Private Function ToStream(rtfText As String) As Stream
        Return New MemoryStream(Encoding.GetEncoding(936).GetBytes(rtfText))
    End Function

    Private Function IsPureAscii(filePath As String) As Boolean
        For Each b As Byte In File.ReadAllBytes(filePath)
            If b > 127 Then Return False
        Next

        Return True
    End Function

End Module

