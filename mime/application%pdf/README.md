# PDF 文档读取、文本提取与生成库

## 引言

PDF 是一种「写给打印机看」的格式：它把内容、字体、编码与图形压缩进一个个对象，再用交叉引用表串起来。想要正确地从已有文件中**取出文字**，必须一路还原到字体级别的字符映射；想要**生成** PDF，则必须自己完成分页布局、对象编号与 xref 表的序列化。

本包把这两条路径都实现为纯托管代码，不依赖任何原生库：

- **读**：词法分析 → 对象解析 → xref 解析 → 流解码 → 页面树遍历 → 文本提取（ToUnicode CMap）；
- **写**：内容块收集 → 分页布局 → 对象 / 内容流 / xref / trailer 序列化。

## 设计目标

- **纯托管**：不引入原生依赖，跨平台可用；
- **与 Word 文档共用契约**：`PdfDocument` 实现 `IDocumentWriter`，因此按 `WordDocument` 写的文档代码可以**一行不改**地输出 PDF；
- **文本正确性优先**：文本提取不止于「读出字节」，而是通过 ToUnicode CMap 把字符码映射回 Unicode。

## 核心特性

- **底层读取器**：词法分析器、对象解析器、xref 表解析、对象 / 流解码（Flate、ASCII85、predictor）与页面树遍历；
- **文本提取**：结合 ToUnicode CMap 与字体信息，把页面内容流还原为可读文本；
- **流式写入器**：收集内容块（标题、段落、表格、图片），完成分页布局后序列化为符合规范的 PDF；
- **共用文档契约**：写入器实现 `IDocumentWriter`，与 `WordDocument` 共用同一套文档 API。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.MIME.application.pdf`（根） | `PDF.GetText` 文本提取入口 |
| `....PdfReader` | 词法分析、对象解析、xref 解析、流解码、文本提取 |
| `....PdfWriter` | 文档模型、分页布局引擎、对象序列化、字体与图像资源 |

## 关键类型与 API

- `PDF` —— 模块入口；`GetText(Stream)` 按顺序产出每一页的文本；
- `PdfReader` —— 解析文件路径或字节缓冲，解析间接引用并枚举页面字典；
- `PdfLexer` / `PdfObjectParser` / `PdfObject` 家族 —— 词法分析器与解析器，产出 `PdfDictionary`、`PdfArray`、`PdfStream`、`PdfIndirectObject` 等；
- `FlateDecode` —— 流过滤器：deflate 解压、ASCII85 解码与 predictor 还原；
- `TextExtractor` / `ToUnicodeCMap` —— 由内容流与字体 CMap 重建页面的可读文本；
- `PdfDocument` —— 实现 `IDocumentWriter`；提供流式 `Heading` / `Paragraph` / `Table` / `Image` API 与 `Save`；
- `PdfLayoutEngine` / `PdfWriter` —— 把内容块分页，并序列化对象、内容流、xref 表与 trailer；
- `PdfFontResource` / `PdfImageXObject` / `PdfColor` —— 字体度量与编码、图像 XObject 编码与 RGB 颜色算子。

## 快速上手

```vbnet
Imports System.IO
Imports Microsoft.VisualBasic.MIME.application.pdf

' 读取：逐页输出文本
Using fs As Stream = File.OpenRead("input.pdf")
    For Each page As String In PDF.GetText(fs)
        Console.WriteLine(page)
    Next
End Using

' 生成：一份新文档
Dim doc As New PdfDocument(author:="sciBASIC", title:="Report")

Call doc.Heading(1, "Results").Paragraph("Hello PDF")
Call doc.Table({"Name", "Value"}, {{"a", "1"}, {"b", "2"}})
Call doc.Save("report.pdf")
```

## 实现要点

- **为什么必须解析流过滤器**：绝大多数 PDF 的内容流是 deflate 压缩的，且常常叠加 predictor 做逐行差分；不还原这两层就无法得到原始内容流。
- **ToUnicode CMap 的意义**：PDF 中的字符码与 Unicode 没有必然关系（子集化字体的编码是自定义的），CMap 是唯一的还原依据。
- **共用文档契约的价值**：报表流水线只需面向 `IDocumentWriter` 编程，输出 Word 还是 PDF 只是构造参数不同。

## 包信息

- Assembly：`Microsoft.VisualBasic.MIME.application.pdf`
- TargetFramework：`net10.0`
- Tags：`scibasic;pdf;pdf-reader;text-extraction;pdf-writer;document-generation;flate-decode`
- 许可：GPL-3.0-or-later
