# mime/applicationvnd.openxmlformats-officedocument.wordprocessingml.document/docx/WordDocument.vbproj

- RootNamespace : Microsoft.VisualBasic.MIME.Office.WordDocument
- AssemblyName  : Microsoft.VisualBasic.MIME.Office.WordDocument
- TargetFramework: net10.0
- Source files  : 8
- Existing Title: DOCX Word Document Generator and Text Extractor
- Existing Desc : Generates Office Word documents in sciBASIC#: a fluent writer builds headings, paragraphs, tables, images, lists, code blocks and a table of contents, packs them into a .docx file, and extracts plain text back out.
- Existing Tags : scibasic;docx;word;office-openxml;document-generation

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.MIME.Office.WordDocument)

## Public types
- Class DocxPackager (DocxPackager.vb) - .docx 文件打包器。
- Class DocxTextReader (DocxTextReader.vb) - .docx 文本提取器。
- Interface IDocumentWriter (IDocumentWriter.vb) - 统一文档写入接口。 <see cref="WordDocument"/>（生成 docx）与 <c>Microsoft.VisualBasic.MIME.application.pdf.PdfDocument</c> （生成 pdf）均实现该接口，使同一份文档写入代码可通过传入不同实例生成不同格式。
- Module ImageHelper (ImageHelper.vb) - 图像工具：读取图像尺寸、生成测试 PNG。
- Class WordDocument (WordDocument.vb) - Word 文档生成器。 支持通过流式 API 构建 docx 文档，包括标题、段落、表格、图片、 目录(TOC)、分页符、代码块、引用、列表等。
- Class ImageEntry (WordDocument.vb)
- Class TableStyle (WordStyle\TableStyle.vb) - Word 表格样式。
- Class WordColors (WordStyle\WordColors.vb) - 常用颜色常量（使用 OOXML 的 6 位十六进制 RGB 格式）。
- Class WordStyle (WordStyle\WordStyle.vb) - Word 文档文字样式。 控制字体名称、字号、粗体/斜体/下划线、前景色/背景色、对齐方式、行间距等。

## Notable public members
- Public Sub Save(doc As WordDocument, filePath As String)
- Public Function ExtractText(filePath As String) As String
- Public Function ExtractParagraphs(filePath As String) As String()
- Public Function ExtractMetadata(filePath As String) As Dictionary(Of String, String)
- Public Function ReadPngDimensions(data As Byte()) As ImageDimensions
- Public Function ReadJpegDimensions(data As Byte()) As ImageDimensions
- Public Function ReadImageDimensions(filePath As String) As ImageDimensions
- Public Sub CreateTestPng(filePath As String, width As Integer, height As Integer,
- Public Property Author As String = "" Implements IDocumentWriter.Author
- Public Property Title As String = "" Implements IDocumentWriter.Title
- Public Property Subject As String = "" Implements IDocumentWriter.Subject
- Public Property Description As String = "" Implements IDocumentWriter.Description
- Public Property Tags As String() = {} Implements IDocumentWriter.Tags
- Public Property ApplicationName As String = "VB.NET WordDocument Generator" Implements IDocumentWriter.ApplicationName
- Public Property RelId As String
- Public Property Extension As String
- Public Property Data As Byte()
- Public Property WidthEmu As Integer
- Public Property HeightEmu As Integer
- Public Sub New(Optional author As String = "",
- Public Function HeadingStyle(level As Integer, style As WordStyle) As WordDocument
- Public Function ParagraphStyle(style As WordStyle) As WordDocument
- Public Function DefaultStyle(style As WordStyle) As WordDocument
- Public Function TableStyle(style As TableStyle) As WordDocument
- Public Function CodeStyle(style As WordStyle) As WordDocument
- Public Function BlockquoteStyle(style As WordStyle) As WordDocument
- Public Function TitleStyle(style As WordStyle) As WordDocument
- Public Function PageSetup(pageWidth As Integer, pageHeight As Integer,
- Public Function PageSetupA4() As WordDocument
- Public Function PageSetupLetter() As WordDocument
- Public Function DocTitle(text As String) As WordDocument
- Public Function H1(text As String) As WordDocument
- Public Function H2(text As String) As WordDocument
- Public Function H3(text As String) As WordDocument
- Public Function H4(text As String) As WordDocument
- Public Function H5(text As String) As WordDocument
- Public Function H6(text As String) As WordDocument
- Public Function Heading(level As Integer, text As String) As WordDocument
- Public Function Paragraph(text As String) As WordDocument
- Public Function Paragraph(text As String, style As WordStyle) As WordDocument
- Public Function CodeBlock(code As String, Optional language As String = "") As WordDocument
- Public Function Blockquote(text As String) As WordDocument
- Public Function List(items As String(), Optional ordered As Boolean = False) As WordDocument
- Public Function TaskList(items As String(), checked As Boolean()) As WordDocument
- Public Function DefinitionList(terms As String(), definitions As String()) As WordDocument
- Public Function Hr() As WordDocument
- Public Function PageBreak() As WordDocument
- Public Function Toc(Optional maxLevel As Integer = 3) As WordDocument
- Public Function Table(headers As String(), data As String(,)) As WordDocument
- Public Function Table(headers As String(), data As String(,), alignments As String()) As WordDocument
- Public Function Table(headers As String(), rows As String()(),
- Public Function TableAutoFitWindow(headers As String(), rows As String()(),
- Public Function TableAutoFitContents(headers As String(), rows As String()(),
- Public Function TableAutoFitWindow(headers As String(,), rows As String(,),
- Public Function TableAutoFitContents(headers As String(,), rows As String(,),
- Public Function Image(file As String,
- Public Function WriteBlocks(blocks As IEnumerable(Of JSONSchema.Block)) As WordDocument
- Public Sub Save(filePath As String) Implements IDocumentWriter.Save
- Friend Function GetBodyXml() As String
- Friend Function GetImages() As List(Of ImageEntry)
- ... and 56 more

## Imports
- ImageDimensions = System.Drawing.Size
- Microsoft.VisualBasic.MIME.text.markdown
- std = System.Math
- System.Drawing
- System.Globalization
- System.IO
- System.IO.Compression
- System.Text

## File tree
- DocxPackager.vb
- DocxTextReader.vb
- IDocumentWriter.vb
- ImageHelper.vb
- WordDocument.vb
- WordStyle\TableStyle.vb
- WordStyle\WordColors.vb
- WordStyle\WordStyle.vb

