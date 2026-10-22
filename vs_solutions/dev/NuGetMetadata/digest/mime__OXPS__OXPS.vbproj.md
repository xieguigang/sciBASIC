# mime/OXPS/OXPS.vbproj

- RootNamespace : Microsoft.VisualBasic.MIME.OpenXPS
- AssemblyName  : Microsoft.VisualBasic.MIME.OpenXPS
- TargetFramework: net10.0
- Source files  : 11
- Existing Title: XPS / OXPS Document Generator and Text Extractor
- Existing Desc : Generates and reads XPS and OpenXPS fixed-page documents for sciBASIC#: a block based writer lays out text, tables and images onto fixed pages with optional ODTTF font embedding, packs them into an OPC package, extracts plain text back out through the Glyphs content streams, and implements the shared ReportRender contract for report generation.
- Existing Tags : scibasic;xps;oxps;fixedpage;opc;document-generation;text-extraction;markdown

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.MIME.OpenXPS)

## Public types
- Class NamespaceDoc (NamespaceDoc.vb) - <para> 解析链路：<see cref="XpsTextReader"/> 按 fdseq → fdoc → fpage 的关系链定位页面， 逐页收集 &lt;Glyphs UnicodeString&gt; 并按基线位置重组成行，提供 ExtractText /
- Enum XpsBlockType (XpsBlock.vb)
- Class XpsTextStyle (XpsBlock.vb) - 文本样式快照。所有尺寸单位为磅（pt），<see cref="Indent"/> 为左缩进。
- Class XpsBlock (XpsBlock.vb)
- Module XpsTextMetrics (XpsBlock.vb) - XPS 文本度量：XPS 的 Glyphs 需要显式坐标，因此换行必须在布局前用近似 字宽计算完成。CJK / 全角字符按 1 em，ASCII 按 0.5 em（Helvetica 平均宽度）。
- Class XpsDocument (XpsDocument.vb) - XPS / OXPS 文档生成器。收集内容与样式，最终由 <see cref="XpsLayoutEngine"/> 排版、<see cref="XpsPackager"/> 打包为 .oxps / .xps 文件。 页面尺寸与边距入参单位为 twips（1 pt = 20 twips），内部换算为磅后排版。
- Class XpsDocumentInfo (XpsDocumentInfo.vb)
- Enum XpsFlavor (XpsFlavor.vb) - 输出的 XPS 包方言：OpenXPS（ECMA-388 2nd edition）或 Microsoft XPS（2005/06）。
- Module XpsSpec (XpsFlavor.vb) - XPS / OXPS 规格常量：两种方言的命名空间、内容类型、关系类型，以及页面与 XML 输出相关的公共辅助函数。
- Class XpsFontResource (XpsFontResource.vb) - XPS 字体资源：负责查找可用字体、按 ODTTF 规范混淆，并在没有可用字体时安全回退。 嵌入的是**完整**字体文件（不做子集化），因此使用中文字体时文档包体会达到数 MB； 对体积敏感的场景可以关闭 <see cref="XpsDocument.EmbedFont"/>（回退输出的包只有几 KB，
- Class XpsImageResource (XpsImageResource.vb)
- Class XpsGlyphs (XpsLayoutEngine.vb)
- Class XpsLineSegment (XpsLayoutEngine.vb)
- Class XpsRectangle (XpsLayoutEngine.vb)
- Class XpsImagePlacement (XpsLayoutEngine.vb)
- Class XpsPage (XpsLayoutEngine.vb)
- Class XpsLayoutEngine (XpsLayoutEngine.vb) - 固定页排版引擎：把内容块队列排成页面（自动折行、自动分页、表格网格、 目录展开与图片版面）。
- Class XpsWriteMeta (XpsPackager.vb)
- Class XpsPackager (XpsPackager.vb) - XPS / OXPS 包的序列化器：把页面与资源写成 OPC(zip) 字节流。
- Class XpsRender (XpsRender.vb) - XPS / OXPS 报告渲染器：把 markdown 内容块（<see cref="Block"/>）渲染为 XPS/OXPS 固定版式文档，是 <see cref="ReportRender"/> 在 OPC 包格式上的实现。 输出方言、字体与元数据在文档第一次被访问（<see cref="Render"/>、<see cref="Save"/>
- Class XpsTextReader (XpsTextReader.vb) - XPS / OXPS 文本提取器：把固定页面文档还原为纯文本、按页文本、段落数组与 文档属性。API 形态与 <c>DocxTextReader</c> / <c>RtfTextReader</c> 保持一致。

## Notable public members
- Public Property FontFamily As String = "SimHei"
- Public Property Size As Double = 10.5
- Public Property Bold As Boolean = False
- Public Property Italic As Boolean = False
- Public Property ForeColor As String = "#000000"
- Public Property SpaceBefore As Double = 0
- Public Property SpaceAfter As Double = 0
- Public Property LineSpacing As Double = 1.4
- Public Property Indent As Double = 0
- Public Property Alignment As String = "left"
- Public ReadOnly Property LineHeight As Double
- Public Function Clone() As XpsTextStyle
- Public Property Type As XpsBlockType
- Public Property Text As String
- Public Property Level As Integer = 1
- Public Property Ordered As Boolean = False
- Public Property Checked As Boolean()
- Public Property Items As String()
- Public Property Terms As String()
- Public Property Definitions As String()
- Public Property TableHeaders As String()
- Public Property TableRows As String()()
- Public Property TableAlignments As String()
- Public Property ImagePath As String
- Public Property ImageResource As XpsImageResource
- Public Property ImageWidth As Double = 0
- Public Property ImageHeight As Double = 0
- Public Property ImageCaption As String
- Public Property Style As XpsTextStyle
- Public Const TabWidth As Double = 2.0
- Public Const MinLineSpacing As Double = 1.0
- Public Function IsWide(ByVal c As Char) As Boolean
- Public Function CharWidth(ByVal c As Char) As Double
- Public Function MeasureText(text As String, size As Double) As Double
- Public Function WrapText(text As String, size As Double, maxWidth As Double) As String()
- Public Function WrapText(text As String, size As Double, width As Double,
- Public Property Author As String = ""
- Public Property Title As String = ""
- Public Property Subject As String = ""
- Public Property Description As String = ""
- Public Property Tags As String() = Nothing
- Public Property ApplicationName As String = "VB.NET XpsDocument Generator"
- Public Property Flavor As XpsFlavor = XpsFlavor.Oxps
- Public Property FontFile As String = Nothing
- Public Property EmbedFont As Boolean = True
- Public Sub New(Optional author As String = "",
- Public Function HeadingStyle(level As Integer, style As XpsTextStyle) As XpsDocument
- Public Function ParagraphStyle(style As XpsTextStyle) As XpsDocument
- Public Function CodeStyle(style As XpsTextStyle) As XpsDocument
- Public Function BlockquoteStyle(style As XpsTextStyle) As XpsDocument
- Public Function TitleStyle(style As XpsTextStyle) As XpsDocument
- Public Function PageSetup(pageWidth As Integer, pageHeight As Integer,
- Public Function PageSetupA4() As XpsDocument
- Public Function PageSetupLetter() As XpsDocument
- Public Function DocTitle(text As String) As XpsDocument
- Public Function Heading(level As Integer, text As String) As XpsDocument
- Public Function H1(text As String) As XpsDocument
- Public Function H2(text As String) As XpsDocument
- Public Function H3(text As String) As XpsDocument
- Public Function H4(text As String) As XpsDocument
- ... and 144 more

## Imports
- Microsoft.VisualBasic.ApplicationServices.Report
- System.IO
- System.IO.Compression
- System.Text
- System.Text.RegularExpressions
- System.Xml.Linq

## File tree
- NamespaceDoc.vb
- XpsBlock.vb
- XpsDocument.vb
- XpsDocumentInfo.vb
- XpsFlavor.vb
- XpsFontResource.vb
- XpsImageResource.vb
- XpsLayoutEngine.vb
- XpsPackager.vb
- XpsRender.vb
- XpsTextReader.vb

