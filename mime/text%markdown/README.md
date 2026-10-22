# Markdown 解析器、块模型与多格式渲染

## 引言

Markdown 的语法看似简单，但「嵌套」会让实现难度陡增：引用块里可以有标题，列表里可以有代码块，表格单元格里可以有行内格式。若要正确地处理这些情况，**递归下降几乎不可避免**。

本包采用**两阶段递归解析**：先做块级切分，再做行内解析；渲染端则通过抽象 `Render` 基类实现「同一份解析结果 → HTML 或纯文本」的可插拔输出。此外还提供一套**类型化的块模型**，用于承载结构化内容（例如大语言模型的输出）并再次渲染。

## 设计目标

- **递归正确性优先**：块级与行内两阶段解析，保证嵌套结构不丢信息；
- **输出可插拔**：`Render` 抽象把「解析」与「渲染」解耦；
- **结构化内容可往返**：JSON 块模型 ↔ markdown / HTML 的双向转换。

## 核心特性

- **两阶段递归解析器 `MarkdownParser`**：先块级分词，再行内 span 解析，因此嵌套列表、包含标题或列表的引用块、带行内格式的表格都能通过递归正确处理；
- **可插拔输出**：抽象 `Render` 基类定义输出契约，`HtmlRender` 输出 HTML、`TextRender` 输出纯文本；`MarkdownRender` 负责组装并暴露图片 URL 路由；
- **主题样式 `MarkdownStyle`**：`New MarkdownRender(New HtmlRender(MarkdownStyle.GitHub))` 即可产出内嵌 `<style>` 的完整 HTML 文档（内置 `GitHub` / `Dark` / `Thesis` 主题，或用 `MarkdownStyle.Create(css)` 自定义）；`New HtmlRender()` 不带样式时仍返回纯 html 片段，行为向后兼容；
  - `Thesis` 为论文学术风格：三号黑体居中章标题、四号宋体加粗节标题、小四宋体正文首行缩进两字符并两端对齐、无竖线的三线表、无底纹代码块与不倾斜引用块，版式与 docx 的 `ThesisFormatting` 保持一致，打印时章标题自动另起一页；
- **markdown → 块模型 `MarkdownBlockParser.Parse`**：把 markdown 文本解析为 `ApplicationServices.Report.Block` 平面列表（标题 / 段落 / 代码 / 列表 / 任务列表 / 定义列表 / 表格 / 引用 / 分割线 / 图片 / 链接 / 脚注 / 公式），行内语法统一转成纯文本，可直接交给 `WordRender` / `PdfRender` / `RtfRender` / `XpsRender` / `TextRender` / `HtmlRender` / `PlainTextRender` 渲染；块模型本身与 JSON 输入的 `JsonReport.Parse` 共用同一套 `Block` 定义；
- **目录生成**：`TOC.GenerateToc` 由标题构建嵌套 markdown 列表，`TOC.AddToc` 插入到文档前部，`MarkdownRender.GetTOC` 返回标题文本及其层级；
- **统一的标题扫描 `MarkdownHeadings.Parse`**：ATX（`#`..`######`）与 setext（`===` / `---`）标题、跳过围栏代码块内的伪标题、识别引用块/列表项内的标题，并给出与渲染顺序一致的 `Index` 与源码位置（`LineStart` / `LineLength`），供目录生成、编辑器大纲与文档导航复用。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.MIME.text.markdown`（根） | `MarkdownParser`、`MarkdownRender`、`Render` / `HtmlRender` / `TextRender`、`MarkdownStyle`、`MarkdownBlockParser`、`TOC`、`MarkdownHeadings` / `HeadingInfo` |
| `Microsoft.VisualBasic.ApplicationServices.Report`（Core） | 块模型 `Block`、各种 `ReportRender`（`TextRender` / `HtmlRender` / `PlainTextRender` / Word / RTF / PDF / XPS） |

## 关键类型与 API

- `MarkdownParser` —— 递归块 / 行内解析器，针对给定 `Render` 产出渲染结果字符串；
- `MarkdownRender` —— 入口：`Transform` 把 markdown 渲染为 HTML（或配合 `TextRender` 渲染为文本），`GetTOC` 列出标题；
- `Render` —— 抽象渲染契约，由 `HtmlRender` 与 `TextRender` 实现；
- `HtmlRender` —— 把 markdown 结构渲染为 HTML 标签；传入 `MarkdownStyle` 时 `Document` 会包装成内嵌 `<style>` 的完整 HTML 文档；
- `MarkdownStyle` —— HTML 主题样式（CSS + body class），内置 `GitHub` 浅色、`Dark` 深色与 `Thesis` 论文风格主题，`Create(css)` 可自定义；
- `TextRender` —— 把 markdown 结构渲染为纯文本；
- `MarkdownBlockParser` —— markdown 文本 → `Report.Block` 列表（行内语法转纯文本，可交给任意格式的渲染器）；
- `ApplicationServices.Report.Block` —— 类型化块模型：类型、层级、内容、表头、行与列表数据；
- `TOC` —— 由标题生成并插入嵌套目录（内部复用 `MarkdownHeadings.Parse`）；
- `MarkdownHeadings` / `HeadingInfo` —— 权威的标题扫描实现：目录、`GetTOC`、编辑器大纲与源码跳转都基于它。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MIME.text.markdown

Dim md As String = "# Title" & vbLf & "Hello **world** and `code`."

Dim html As String = New MarkdownRender().Transform(md)                    ' 纯 html 片段
Dim styled As String = New MarkdownRender(New HtmlRender(MarkdownStyle.GitHub)).Transform(md)  ' 内嵌样式
Dim dark As String = New MarkdownRender(New HtmlRender(MarkdownStyle.Dark)).Transform(md)      ' 深色主题
Dim thesis As String = New MarkdownRender(New HtmlRender(MarkdownStyle.Thesis)).Transform(md)  ' 论文学术风格
Dim plain As String = New MarkdownRender(New TextRender()).Transform(md)
Dim toc As String = TOC.GenerateToc(md)

For Each header In MarkdownRender.GetTOC(md)
    Console.WriteLine($"H{header.Value}: {header.Name}")
Next

' markdown 文本 -> 块模型 -> 任意格式
Dim blocks As List(Of ApplicationServices.Report.Block) = New MarkdownBlockParser().Parse(md)

Dim markdown2 As String = blocks.ToMarkdown()      ' 重新渲染为 markdown
Dim html2 As String = blocks.ToHtml()              ' 渲染为 html 片段
Dim plain2 As String = blocks.ToPlainText()        ' 渲染为纯文本（无标记）

' JSON 文档（例如 LLM 输出）-> 同一个块模型（mime/application%json 工程的 JsonReport）
Dim blocks2 = JsonReport.Parse(json)
```

## 实现要点

- **为什么必须两阶段**：只做行内替换的实现无法正确处理「引用块内的列表」这类嵌套；先块级后行内的递归顺序是保证正确性的关键。
- **块模型的实际价值**：当内容来源不是 markdown 文本而是结构化 JSON（典型如 LLM 返回的 JSON），块模型提供了统一的中转表示，之后无论是转 markdown 还是 HTML 都只需一次转换。

## 包信息

- Assembly：`Microsoft.VisualBasic.MIME.text.markdown`
- TargetFramework：`net10.0`
- Tags：`scibasic;markdown;markdown-parser;html-renderer;text-renderer;table-of-contents;json;document`
- 许可：GPL-3.0-or-later
