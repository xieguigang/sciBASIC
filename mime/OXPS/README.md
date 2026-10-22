# XPS / OXPS 固定版式文档生成与文本提取

## 引言

XPS 与 OpenXPS 是「固定版式」文档：一切内容最终都落到一页页固定尺寸的画布上，用显式的坐标描述字形与图形。它没有流式排版的概念，因此**分页、折行、表格网格、目录页码**这些工作都必须由写入端自己完成。

本包把这条链路完整实现为纯托管代码，不依赖任何原生打印组件：

- **写**：内容块收集 → 排版引擎（折行 / 分页 / 表格 / 目录）→ OPC 打包（zip）；
- **读**：按 `fdseq → fdoc → fpage` 关系链定位页面，收集 `<Glyphs UnicodeString>`，按基线位置重组成行与段落。

同时支持两种方言：**OpenXPS**（ECMA-388 第二版）与 **Microsoft XPS**（2005/06）。二者的命名空间、内容类型与关系类型不同，由 `XpsFlavor` 与 `XpsSpec` 集中描述。

## 设计目标

- **纯托管**：不引入原生打印管线，跨平台可用；
- **与其它文档格式共用契约**：`XpsDocument` 实现 `IDocumentWriter`，`XpsRender` 继承 `ReportRender`，因此面向报表流水线写的代码可以一行不改地输出 XPS/OXPS；
- **文本可还原**：文本提取不止于读出字节，而是按 Glyphs 的基线坐标重建行、段落与文档属性。

## 核心特性

- **块式写入器**：标题 H1–H4、段落、列表、引用、代码块、表格（含列对齐）、图片与目录；
- **排版引擎**：近似字宽度量（CJK / 全角按 1 em，ASCII 按 0.5 em）驱动的自动折行、自动分页、表格网格与图片版面；
- **字体资源**：按 ODTTF 规范混淆并嵌入字体；无可用字体时安全回退（回退包只有几 KB，嵌入中文字体则会有数 MB）；
- **OPC 打包**：把页面、资源、内容类型与关系序列化为 `.xps` / `.oxps` 包；
- **文本提取**：`ExtractText` / `ExtractPages` / `ExtractParagraphs` / `ExtractMetadata`，API 形态与 `DocxTextReader`、`RtfTextReader` 保持一致；
- **报告渲染器**：`XpsRender` 直接把 `Report.Block` 内容块渲染为固定版式文档，支持文档大标题、目录与元数据。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.MIME.OpenXPS` | 文档写入器、排版引擎、打包器、文本提取器与报告渲染器（本包全部类型） |

## 关键类型与 API

- `XpsDocument` —— 写入器门面：`PageSetupA4` / `PageSetupLetter` / `PageSetup(w, h)`、`DocTitle`、`Heading(level, text)`、`H1..H4`、`Paragraph`、`Table(headers, rows)`、`Image(path)`、`Toc(maxLevel)`、`WriteBlocks(blocks)`、`Save(path)`；
- `XpsTextStyle` —— 文本样式快照（字体、字号、粗斜体、颜色、段前段后、行距、缩进、对齐），单位均为磅；
- `XpsBlock` / `XpsBlockType` —— 排版前的内容块模型；
- `XpsLayoutEngine` —— 固定页排版引擎（折行、分页、表格网格、目录展开、图片版面），产出 `XpsPage`；
- `XpsPackager` —— 把页面与资源序列化为 OPC(zip) 字节流；
- `XpsFontResource` / `XpsImageResource` —— ODTTF 字体混淆与图像资源编码；
- `XpsTextReader` —— 文本提取器：`ExtractText`、`ExtractPages`、`ExtractParagraphs`、`ExtractMetadata`；
- `XpsRender` —— 报告渲染器：`Render(blocks)` + `Save(path)`，或一步 `Generate(blocks, path, title)`；
- `XpsFlavor` / `XpsSpec` —— 方言选择与规格常量。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MIME.OpenXPS

' 生成一份文档
Dim doc As New XpsDocument(author:="sciBASIC#", title:="实验报告")

Call doc.PageSetupA4().
         DocTitle("实验数据分析报告").
         H1("结果").
         Paragraph("内容按固定版式排版，自动折行与分页。").
         Table({"Name", "Value"}, {{"a", "1"}, {"b", "2"}}).
         Toc(3)

Call doc.Save("report.oxps")
```

由 markdown 内容块渲染整份报告：

```vbnet
Imports Microsoft.VisualBasic.ApplicationServices.Report
Imports Microsoft.VisualBasic.MIME.OpenXPS

Dim blocks As Block() = { New Block With {.type = "heading", .level = 1, .content = "Results"} }

Dim render As New XpsRender(XpsFlavor.Oxps) With {
    .DocumentTitle = "实验数据分析报告",   ' 正文之前的文档大标题
    .TableOfContents = True,               ' 正文之前插入目录
    .EmbedFont = True                      ' 嵌入字体（中文字体较大）
}

Call render.Render(blocks)
Call render.Save("report-block.oxps")

' 也可以一步生成
Call XpsRender.Generate(blocks, "report-oneshot.xps", "实验数据分析报告")
```

读取已有文档：

```vbnet
Dim reader As New XpsTextReader()

Console.WriteLine(reader.ExtractText("report.oxps"))

For Each page As String In reader.ExtractPages("report.oxps")
    Console.WriteLine(page)
Next

Dim info As XpsDocumentInfo = reader.ExtractMetadata("report.oxps")
```

## 实现要点

- **为什么必须自己折行**：XPS 的 `<Glyphs>` 需要显式坐标，没有「自动换行」这一层；因此排版前必须按字体近似字宽预先切行，否则文本会溢出页面；
- **单位换算**：页面尺寸与边距的入参单位为 twips（1 pt = 20 twips），内部统一换算为磅后排版；
- **字体体积**：嵌入的是**完整**字体文件（不做子集化），使用中文字体时包体会达到数 MB。对体积敏感的场景可以关闭 `EmbedFont`，回退包只有几 KB；
- **方言差异集中管理**：`XpsSpec` 是两种方言命名空间、内容类型与关系类型的唯一事实来源，避免在打包器里散落字符串。

## 包信息

- Assembly：`Microsoft.VisualBasic.MIME.OpenXPS`
- TargetFramework：`net10.0`
- Tags：`scibasic;xps;oxps;fixedpage;opc;document-generation;text-extraction;markdown`
- 许可：GPL-3.0-or-later
