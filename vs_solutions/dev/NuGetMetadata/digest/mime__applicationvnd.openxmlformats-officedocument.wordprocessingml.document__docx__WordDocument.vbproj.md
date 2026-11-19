# mime/applicationvnd.openxmlformats-officedocument.wordprocessingml.document/docx/WordDocument.vbproj

- RootNamespace : Microsoft.VisualBasic.MIME.Office.WordDocument
- AssemblyName  : Microsoft.VisualBasic.MIME.Office.WordDocument
- TargetFramework: net10.0
- Source files  : 7
- Existing Title: DOCX Word Document Generator and Text Extractor
- Existing Desc : Generates Office Word documents in sciBASIC#: a fluent writer builds document titles, headings H1-H6, paragraphs, tables, images, lists, code blocks and a table of contents, packs them into a .docx package, and extracts plain text back out.
- Existing Tags : scibasic;docx;word;office-openxml;document-generation;text-extraction

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.MIME.Office.WordDocument)

## Public types
- Class DocxPackager (DocxPackager.vb) - .docx 文件打包器。
- Class DocxTextReader (DocxTextReader.vb) - .docx 文本提取器。
- Class NamespaceDoc (NamespaceDoc.vb) - Word .docx document generation: builds paragraphs, styled text runs, lists, code blocks and a table of contents into an OOXML package, extracts plain text back out, and provides the Word style, table style and color definitions.
- Module OmmlBuilder (OMML.vb) - OMML（Office Math Markup Language）序列化器。
- Class ThesisFormatting (ThesisFormatting.vb) - 学位论文主题格式对象。默认内置《江南大学研究生学位论文要求及格式规范（2025年修订）》 的版式规则，通过 <see cref="ApplyTo"/> 一次性应用到 <see cref="WordDocument"/> 的样式体系，使 <see cref="WordDocument.WriteBlocks"/> 写入的文档内容自动获得论文格式。
- Module OOXMLNamespaces (WordDocument.vb)
- Class WordDocument (WordDocument.vb) - Word 文档生成器。 支持通过流式 API 构建 docx 文档，包括标题、段落、表格、图片、 目录(TOC)、分页符、代码块、引用、列表等。
- Class HeaderFooterPart (WordDocument.vb)
- Class SectionDef (WordDocument.vb) - 节属性定义。每个节可独立设置奇偶页页眉与页脚页码格式； sectPr 位于节末（段落级 sectPr 或文档末尾的 body 级 sectPr）。
- Class ImageEntry (WordDocument.vb)
- Class WordRender (WordRender.vb) - word 报告渲染器：将 markdown 内容块（<see cref="Block"/>）渲染为 microsoft office word 文档，是 <see cref="ReportRender"/> 在二进制文档格式上的实现。 通过 <see cref="Theme"/> 属性可以套用 <see cref="ThesisFormatting"/> 等版式主题。

## Notable public members
- Public Sub Save(doc As WordDocument, filePath As String)
- Public Function ExtractText(filePath As String) As String
- Public Function ExtractParagraphs(filePath As String) As String()
- Public Function ExtractMetadata(filePath As String) As Dictionary(Of String, String)
- Public Function ToOmml(lambda As LambdaExpression, Optional fontSizePt As Double = 0) As String
- Public Function ToOmml(exp As MathExpression, Optional fontSizePt As Double = 0) As String
- Public Property LatinFont As String = "Times New Roman"
- Public Property ChineseBodyFont As String = "宋体"
- Public Property ChineseHeadingFont As String = "黑体"
- Public Const SizeXiaoEr As Double = 18
- Public Const SizeSanHao As Double = 16
- Public Const SizeSiHao As Double = 14
- Public Const SizeXiaoSi As Double = 12
- Public Const SizeWuHao As Double = 10.5
- Public Property PageWidth As Integer = 11906       ' A4 宽 210mm
- Public Property PageHeight As Integer = 16838      ' A4 高 297mm
- Public Property MarginTop As Integer = 1417        ' 2.5cm
- Public Property MarginRight As Integer = 1417      ' 2.5cm
- Public Property MarginBottom As Integer = 1417     ' 2.5cm
- Public Property MarginLeft As Integer = 1417       ' 2.5cm
- Public Property UseThreeLineTable As Boolean = True
- Public Property Heading1Style As WordStyle
- Public Property Heading2Style As WordStyle
- Public Property Heading3Style As WordStyle
- Public Property Heading4Style As WordStyle
- Public Property Heading5Style As WordStyle
- Public Property Heading6Style As WordStyle
- Public Property BodyStyle As WordStyle
- Public Property TitleStyle As WordStyle
- Public Property CodeStyle As WordStyle
- Public Property BlockquoteStyle As WordStyle
- Public Property TableTextStyle As WordStyle
- Public Property CaptionStyle As WordStyle
- Public Property ReferenceStyle As WordStyle
- Public Property Table As TableStyle
- Public Sub New()
- Public Overridable Function ApplyTo(doc As WordDocument) As WordDocument
- Friend Const NS_W As String = "http://schemas.openxmlformats.org/wordprocessingml/2006/main"
- Friend Const NS_R As String = "http://schemas.openxmlformats.org/officeDocument/2006/relationships"
- Public Property Author As String = "" Implements IDocumentWriter.Author
- Public Property Title As String = "" Implements IDocumentWriter.Title
- Public Property Subject As String = "" Implements IDocumentWriter.Subject
- Public Property Description As String = "" Implements IDocumentWriter.Description
- Public Property Tags As String() = {} Implements IDocumentWriter.Tags
- Public Property ApplicationName As String = "VB.NET WordDocument Generator" Implements IDocumentWriter.ApplicationName
- Public Property RelId As String
- Public Property Kind As String      ' "header" / "footer"
- Public Property Xml As String       ' 部件完整 XML
- Public Property HeaderOddRelId As String = ""   ' 奇数页页眉（evenAndOddHeaders 开启时 default 类型即奇数页）
- Public Property HeaderEvenRelId As String = ""  ' 偶数页页眉
- Public Property FooterRelId As String = ""      ' 页脚（居中 PAGE 页码域，格式由 pgNumType 决定）
- Public Property PageNumFmt As String = ""       ' 页码格式：upperRoman（前置部分）/ decimal（正文）；空=不指定
- Public Property PageNumStart As Integer = 0     ' 页码起始号；0=续前节
- Public Property RelId As String
- Public Property Extension As String
- Public Property Data As Byte()
- Public Property WidthEmu As Integer
- Public Property HeightEmu As Integer
- Public Sub New(Optional author As String = "",
- Public Function HeadingStyle(level As Integer, style As WordStyle) As WordDocument
- ... and 76 more

## Imports
- Microsoft.VisualBasic.ApplicationServices.Report
- Microsoft.VisualBasic.Imaging.BitmapImage
- Microsoft.VisualBasic.MIME.application.xml.MathML
- std = System.Math
- System.Drawing
- System.Globalization
- System.IO
- System.IO.Compression
- System.Text

## File tree
- DocxPackager.vb
- DocxTextReader.vb
- NamespaceDoc.vb
- OMML.vb
- ThesisFormatting.vb
- WordDocument.vb
- WordRender.vb

