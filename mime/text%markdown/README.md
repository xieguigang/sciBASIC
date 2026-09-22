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
- **类型化块模型 `JSONSchema.Block`**：`JSONRenderer.Parse` 把 JSON 文档转换为 `IEnumerable(Of Block)`（对 LLM 输出尤其有用），`ToMarkdown` / `ToHtml` 再渲染回去；`BlockRenderer` 转换单个块；
- **目录生成**：`TOC.GenerateToc` 由 ATX 标题构建嵌套 markdown 列表，`TOC.AddToc` 插入到文档前部，`MarkdownRender.GetTOC` 返回标题文本及其层级。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.MIME.text.markdown`（根） | `MarkdownParser`、`MarkdownRender`、`Render` / `HtmlRender` / `TextRender`、`TOC` |
| `....text.markdown.JSONSchema` | 类型化块模型 `Block`、`JSONRenderer`、`BlockRenderer` |

## 关键类型与 API

- `MarkdownParser` —— 递归块 / 行内解析器，针对给定 `Render` 产出渲染结果字符串；
- `MarkdownRender` —— 入口：`Transform` 把 markdown 渲染为 HTML（或配合 `TextRender` 渲染为文本），`GetTOC` 列出 ATX 标题；
- `Render` —— 抽象渲染契约，由 `HtmlRender` 与 `TextRender` 实现；
- `HtmlRender` —— 把 markdown 结构渲染为 HTML 标签；
- `TextRender` —— 把 markdown 结构渲染为纯文本；
- `JSONSchema.Block` —— 类型化块模型：类型、层级、内容、表头、行与列表数据；
- `JSONSchema.JSONRenderer` —— 把 JSON 文档解析为块，并把块渲染为 markdown 或 HTML；
- `TOC` —— 由 ATX 标题生成并插入嵌套目录。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MIME.text.markdown

Dim md As String = "# Title" & vbLf & "Hello **world** and `code`."

Dim html As String = New MarkdownRender().Transform(md)
Dim plain As String = New MarkdownRender(New TextRender()).Transform(md)
Dim toc As String = TOC.GenerateToc(md)

For Each header In MarkdownRender.GetTOC(md)
    Console.WriteLine($"H{header.Value}: {header.Name}")
Next

' JSON 文档（例如 LLM 输出）-> markdown / HTML
Dim blocks As IEnumerable(Of JSONSchema.Block) = JSONSchema.JSONRenderer.Parse(json)
Dim html2 As String = JSONSchema.JSONRenderer.ToHtml(blocks)
```

## 实现要点

- **为什么必须两阶段**：只做行内替换的实现无法正确处理「引用块内的列表」这类嵌套；先块级后行内的递归顺序是保证正确性的关键。
- **块模型的实际价值**：当内容来源不是 markdown 文本而是结构化 JSON（典型如 LLM 返回的 JSON），块模型提供了统一的中转表示，之后无论是转 markdown 还是 HTML 都只需一次转换。

## 包信息

- Assembly：`Microsoft.VisualBasic.MIME.text.markdown`
- TargetFramework：`net10.0`
- Tags：`scibasic;markdown;markdown-parser;html-renderer;text-renderer;table-of-contents;json;document`
- 许可：GPL-3.0-or-later
