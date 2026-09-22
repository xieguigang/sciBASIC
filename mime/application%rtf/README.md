# RTF 与 Office MathML 文档模型

## 引言：为什么还需要 RTF？

在富文本格式层出不穷的今天，RTF（Rich Text Format）依然有它不可替代的位置：它是**纯文本**，可以用任意编辑器打开与 diff；它被 Word、LibreOffice、写字板等几乎全部办公软件原生支持；它不需要引入 OOXML 那样庞大的对象模型，就能生成一份带字体、颜色与样式的文档。

`Microsoft.VisualBasic.MIME.RTF` 正是 `sciBASIC#` 中负责这一层的轻量实现，由四部分组成：

1. **RTF 文档对象模型** —— 逐步追加带样式的文本，最终输出 `.rtf` 文件；
2. **RTF 文档写入器** —— `RtfDocument` 实现统一的 `IDocumentWriter` 接口，可由 Markdown 内容块直接生成文档；
3. **RTF 文档解析器** —— `RtfTextReader` 把 `.rtf` 读回纯文本、段落数组与文档元数据；
4. **Office / OMML XML 模型** —— 可序列化的 Office 数学标记（OMML）与文档属性模型，用于 Word XML 往返。

## 设计目标

- **够用就好**：类模型刻意保持精简，只有「文档 / 格式区域 / 字体」三个核心概念。
- **增量构建**：`AppendText` / `AppendLine` 每次调用都可以携带自己的字体样式，文档在追加过程中被组织为若干「格式区域」。
- **接口一致**：`RtfDocument` 与 docx 的 `WordDocument`、pdf 的 `PdfDocument` 共用 `IDocumentWriter` 契约，同一段写入代码换一个实例即可产出不同格式。
- **可读可写**：既能生成规范 RTF，也能把任意 RTF（含 Word / 写字板产出）解析回文本，便于做内容抽取与往返校验。
- **无损序列化**：Office / OMML 模型全部是可序列化的 XML 模型，便于与报告生成流水线对接。

## 核心特性

- 通过 `AppendText` / `AppendLine` 增量构建文档，每次调用可选择自带字体样式；
- 在文本缓冲之上跟踪**样式区域**，可通过 `SetFormat` 重新应用或拆分格式；
- 通过 `Save` 输出 RTF 1.x 文档（字体表、颜色表、生成器元数据）；
- **`IDocumentWriter` 实现**：文档元数据、6 级标题与正文/代码/引用/大标题样式、页面设置、列表与任务列表、定义列表、分割线、分页、静态目录、表格（等宽 / 自适应 / 居中 / 三线表 / 逐列对齐）、图片十六进制内嵌；
- **Markdown 内容块写入**：`WriteBlocks` 可直接消费 `JSONSchema.Block`（heading / paragraph / code / list / tasklist / deflist / table / blockquote / hr / image / link / math / footnote）；
- **文本回读**：整篇纯文本、按段落切分的数组、文档元数据（标题 / 作者 / 主题 / 关键字 / 公司 / 备注 / 生成程序），并支持逐段流式产出；
- 提供可序列化的 **Office / OMML XML 模型** —— 文档属性、Word 文档设置、数学属性与 html head，用于 Word XML 往返。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.MIME.RTF`（根） | RTF 文档模型与读写：`Rtf`、`FormatedRegion`、`Font`、`RtfDocument`、`RtfBlock`、`RtfSerializer`、`RtfTextReader`、`RtfDocumentInfo` |
| `Microsoft.VisualBasic.MIME.RTF.Models` | 共享字体数据模型 `Font`（字族、字号、粗体、斜体、下划线、颜色） |
| `Microsoft.VisualBasic.MIME.RTF.Omml` | Office / OMML XML 模型：`HTML`、`DocumentXmlProperty`、`DocumentProperties`、`WordDocument`、`OfficeDocumentSettings`、`mathPr`、`Paragraph`、`Font`、`StyleTokens` |

工程内目录约定：`Writer/` 为写入侧（`RtfBlock` / `RtfDocument` / `RtfSerializer`），`Reader/` 为读取侧（`RtfLexer` / `RtfTextReader`），`test/` 为独立的控制台示例工程（不编入库）。

## 关键类型与 API

- `Microsoft.VisualBasic.MIME.RTF.Rtf` —— 文档对象模型：追加带样式的文本并写出 `.rtf` 文件；
- `Microsoft.VisualBasic.MIME.RTF.Font` —— 文本区域的字体样式，同时负责生成 RTF 样式标记（`\b`、`\i`、`\fs`、`\ul`）；
- `Models.Font` —— 被 `Font` 继承的纯字体数据（字族、字号、粗体、斜体、下划线、颜色）；
- `FormatedRegion` —— 文档中的一个样式化文本片段：记录自身的起止偏移并渲染自己的 RTF 文本；
- `RtfDocument` —— **`IDocumentWriter` 实现**：流式写入标题 / 段落 / 表格 / 图片 / 内容块，`Save` 输出 `.rtf`；
- `RtfBlock` / `RtfBlockType` —— 写入期的中间内容块模型（样式在入队时快照，`Save` 时统一序列化）；
- `RtfWriteMeta` —— 写入时携带的文档元数据（标题 / 作者 / 主题 / 描述 / 关键字 / 生成程序）；
- `RtfSerializer` —— RTF 1.x 序列化器：字体表、颜色表、转义、表格、图片内嵌与落盘；
- `RtfTextReader` —— **解析器**：`ExtractText`、`ExtractParagraphs`、`ExtractMetadata` 与共享的 `GetText` 逐段迭代；
- `RtfDocumentInfo` —— 解析出的文档元数据（标题 / 作者 / 主题 / 关键字 / 公司 / 备注 / 生成程序）；
- `RtfLexer` —— RTF 单遍词法解析器（内部分组栈、忽略目标跳过、多代码页字节解码）；
- `Omml.HTML` —— 携带 Word / VML / OMML 命名空间声明的 Office html/xml 文档根模型；
- `Omml.DocumentXmlProperty` / `Omml.DocumentProperties` —— 可序列化为 Office XML 的 `xml` 与属性块；
- `Omml.WordDocument` / `Omml.OfficeDocumentSettings` / `Omml.mathPr` —— Word 文档设置、兼容性与数学属性模型。

## 快速上手

```vbnet
Imports System.Drawing
Imports Microsoft.VisualBasic.MIME.RTF

Dim doc As New Rtf("Cambria", 11, Color.Black)

Call doc.AppendLine("Hello sciBASIC#")
Call doc.AppendLine("styled text",
                    New Font(size:=11, Bold:=True, Name:="Cambria",
                             Italic:=False, Underline:=False, Color:=Color.Red))
Call doc.Save("demo.rtf")
```

### 由 Markdown 内容块生成 RTF（`IDocumentWriter`）

```vbnet
Imports Microsoft.VisualBasic.MIME.Office.WordDocument
Imports Microsoft.VisualBasic.MIME.RTF
Imports Microsoft.VisualBasic.MIME.text.markdown

' 用接口变量承载，即可与 docx / pdf 写入器互换
Dim writer As IDocumentWriter = New RtfDocument(
    author:="sciBASIC#",
    title:="RTF 生成示例",
    tags:={"rtf", "markdown"})

Call writer.PageSetupA4()
Call writer.DocTitle("RTF 生成示例")
Call writer.Toc(2)
Call writer.H1("由内容块生成")
Call writer.WriteBlocks(JSONSchema.JSONRenderer.Parse(jsonDocument))
Call writer.Table({"名称", "数值"}, {{"alpha", "1"}, {"beta", "2"}}, {"left", "right"})
Call writer.Image("chart.png", caption:="图 1")
Call writer.Save("demo-block.rtf")
```

### 把 RTF 读回文本

```vbnet
Imports Microsoft.VisualBasic.MIME.RTF

Dim reader As New RtfTextReader()

Dim text = reader.ExtractText("demo-block.rtf")            ' 整篇纯文本
Dim paragraphs = reader.ExtractParagraphs("demo-block.rtf") ' 按段落切分
Dim info = reader.ExtractMetadata("demo-block.rtf")        ' 标题 / 作者 / 生成程序 等

For Each paragraph In RtfTextReader.GetText("demo-block.rtf")
    Console.WriteLine(paragraph)                            ' 逐段流式产出
Next
```

## 实现要点

- **样式区域模型**：文档内部维护文本缓冲与格式区域列表；`SetFormat` 在修改已有区域样式时会按需**拆分区域**，从而保证任意位置都能被重新着色或加粗。
- **RTF 标记生成**：`Font` 负责把字体属性翻译为 RTF 控制字（`\b`、`\i`、`\fs`、`\ul`），`Rtf.Save` 汇总字体表、颜色表与生成器元数据后落盘。
- **写入器分两层**：`RtfDocument` 只把内容与样式快照收集为 `RtfBlock` 队列（入队时 `Clone` 样式，避免后续 `XxxStyle()` 调用回溯影响已写入内容），`Save` 时由 `RtfSerializer` 一次性渲染，因此写入过程为 O(总文本长度) 单遍拼接。
- **字体表与颜色表对齐**：表项用字典去重，序号与 `\fN` / `\cfN` 严格对齐；东亚字体标 `fcharset134` 并以 `\afN` 关联，保证中英混排时中文不被西文字体替代。
- **编码策略**：所有非 ASCII 字符一律输出为 `\uN?`（有符号 16 位，自动覆盖代理对），因此生成的 `.rtf` 本身是纯 ASCII 文本，在任何打开环境下中文都能正确显示；数字控制字后统一补分隔符空格，避免控制字与紧随其后的数字文本粘连。
- **表格与图片**：表格按 `\trowd … \cellx … \cell … \row` 输出，支持 `\trhdr` 表头行、逐列对齐、三线表与等宽 / 按窗口 / 按内容三种列宽分配；图片以 `\pict\pngblip|\jpegblip` 十六进制内嵌，十六进制转换使用查表 + 预分配缓冲，避免逐字节拼接导致的 O(n²)。图片缺失或格式不支持时告警跳过，不影响其余内容。
- **解析器为单遍词法分析**：在字节流上扫描，因此能同时处理本库产出（`\uN?` 转义）、Word 产出（`\'hh` 转义）与旧式原始多字节（`ansicpg936`）三类文档；用显式栈处理 `{}` 分组并随分组恢复代码页与 `\uc` 作用域，`fonttbl` / `colortbl` / `stylesheet` / `pict` 等目标整组跳过，`\info` 与 `\*\generator` 捕获为元数据，`\binN` 按长度跳过二进制，CR/LF 排版换行不计入正文。
- **容错**：未闭合分组、多余右花括号、非法十六进制转义等畸形输入一律安全结束并返回已解析内容，不向调用方抛异常；文件不存在时抛 `FileNotFoundException`（与 docx 的 `DocxTextReader` 一致）。
- **OMML 与报告流水线**：`Omml.*` 模型与 `sciBASIC#` 的报告生成（Word XML 往返）共享，因此公式与文档元数据可以在 `.docx` 与 RTF 之间迁移。

## 示例与自校验

`test/` 是一个独立的控制台示例工程（`RtfDemo`，执行 `dotnet run --project test/test.vbproj` 即可运行），它会：

1. 由 Markdown 内容块与流式写入 API 生成 `demo.rtf`（含中文、表格、列表、分割线与图片内嵌）；
2. 用**同一段写入代码**调用 docx 写入器生成 `demo.docx`，验证写入器可互换；
3. 用 `RtfTextReader` 把 `.rtf` 解析回文本，逐项校验关键内容、段落切分与文档元数据；
4. 对 `\'hh` 转义、`\uN?` 转义、`ansicpg936` 原始多字节、忽略分组与畸形文档做解析器健壮性检查。

全部检查通过时进程返回码为 0，可用于 CI 回归。

## 与 sciBASIC# 生态的关系

本包是 `mime` 家族的一员，与 `Microsoft.VisualBasic.MIME.Office.WordDocument`（.docx）、`Excel`、`PDF` 等同属文档输出能力；其 OMML 模型被报告生成流水线复用。

- 写入侧依赖 docx 工程提供的 `IDocumentWriter`、`WordStyle`、`TableStyle`、`WordColors` 与 `ImageHelper`（图片尺寸探测），以及 markdown 工程提供的 `JSONSchema.Block` 内容块模型，因此 RTF 与 `.docx` / `.pdf` 的写入代码可以完全共用；
- 读取侧只依赖 BCL，不引入绘图或 Office 组件。

## 包信息

- Assembly：`Microsoft.VisualBasic.MIME.RTF`
- TargetFramework：`net10.0`
- Tags：`scibasic;rtf;rich-text-format;omml;office-math;document-model;word`
- 说明：本项目为 SDK 风格工程（`<Project Sdk="Microsoft.NET.Sdk">`），`PackageTags` / `PackageReleaseNotes` / `PackageReadmeFile` 与旧式 `Tags` / `ReleaseNotes` 属性同时提供，以兼容不同的打包方式。

## 许可证

GPL-3.0-or-later
