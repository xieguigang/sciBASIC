# DOCX Word 文档生成与文本提取

## 引言

`docx` 同样是 OPC（ZIP + XML）包：`word/document.xml` 承载正文，`word/styles.xml` 定义样式，图片放在 `word/media/`，彼此通过关系表引用。这意味着**生成一份 Word 文档**本质上要做三件事：

1. 把内容组织成 WordprocessingML 段落与运行（run）；
2. 把样式声明为可复用的样式定义，而不是逐段内联；
3. 把正文、样式与媒体资源打包成合法的 OPC 包。

本包用一条**流式（fluent）API** 把这三步封装起来，并且与 PDF 写入器共享同一个文档契约。

## 设计目标

- **可读的构建代码**：`doc.H1("...").Paragraph("...").Table(...)` 这样的链式调用，使文档结构在源码里一目了然；
- **契约统一**：`IDocumentWriter` 同时被本包与 PDF 写入器实现，因此**同一段文档代码可以输出两种格式**；
- **可回读**：不仅写，还能把既有 `.docx` 的文本、段落与核心属性读回来。

## 核心特性

- **流式 `WordDocument` 构建器**：文档标题、H1–H6 标题、段落、代码块、引用块、有序 / 无序 / 任务 / 定义列表、分隔线、分页符、表格与嵌入图片；
- **样式模型**：`WordStyle`（字符与段落格式）、`TableStyle`（表格边框、表头、自适应）、`WordColors`（现成的 OOXML 六位十六进制颜色常量）；
- **页面设置**：以 twips 为单位，支持 A4 / Letter 预设或显式指定宽高与页边距；支持自动生成目录并限制最大标题层级；
- **往返读取**：`DocxTextReader` 提取纯文本、逐段数组、表格文本与 `docProps/core.xml` 元数据；
- **与 markdown 互通**：可把 markdown 的块模型（`JSONSchema.Block`）序列直接渲染成 Word 文档。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.MIME.Office.WordDocument`（根） | `WordDocument` 构建器、`IDocumentWriter`、`DocxPackager`、`DocxTextReader`、`ImageHelper` |
| `....WordDocument.WordStyle` | `WordStyle`、`TableStyle`、`WordColors` |

## 关键类型与 API

- `WordDocument` —— 流式文档构建器兼 `IDocumentWriter` 实现；`Save` 写出 `.docx` 文件；
- `IDocumentWriter` —— 共享的文档与元数据契约，PDF 写入器亦实现该接口，因此同一段代码可以输出任一格式；
- `DocxPackager` —— 把正文 XML、样式与嵌入图片打包为 OPC（`.docx`）zip 包；
- `DocxTextReader` —— 从 `.docx` 提取纯文本、段落、表格文本与核心元数据；
- `WordStyle` —— 字体名与字号、粗体 / 斜体 / 下划线、前景 / 背景色、对齐与间距；
- `TableStyle` —— 表格边框、表头与布局样式；
- `WordColors` —— 常用 OOXML 六位十六进制 RGB 颜色常量；
- `ImageHelper` —— 探取 PNG / JPEG 尺寸，供嵌入图片使用。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MIME.Office.WordDocument

Dim doc As New WordDocument(author:="sciBASIC", title:="Annual Report")

doc.PageSetupA4().
    DocTitle("Annual Report").
    H1("Introduction").
    Paragraph("Generated with sciBASIC#.").
    CodeBlock("Dim x As Integer = 1", "vbnet").
    Toc().
    Table({"Name", "Value"}, {{"a", "1"}, {"b", "2"}})

Call doc.Save("report.docx")
```

## 实现要点

- **共享 `IDocumentWriter` 的价值**：报表流水线只面向接口编程，「输出 Word」与「输出 PDF」的差别仅在于实例化哪个写入器。
- **OPC 打包顺序无关紧要**：OPC 通过关系表（`_rels`）而非目录结构定位部件，因此 `DocxPackager` 可以自由组织包内路径，只要关系与内容类型声明一致即可。
- **目录（TOC）的生成方式**：本包直接由标题层级生成静态目录条目，避免依赖 Word 打开时的域更新。

## 包信息

- Assembly：`Microsoft.VisualBasic.MIME.Office.WordDocument`
- TargetFramework：`net10.0`
- Tags：`scibasic;docx;word;office-openxml;document-generation;word-document;text-extraction`
- 许可：GPL-3.0-or-later
