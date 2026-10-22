# mime/text%markdown/markdown.NET5.vbproj

- RootNamespace : Microsoft.VisualBasic.MIME.text.markdown
- AssemblyName  : Microsoft.VisualBasic.MIME.text.markdown
- TargetFramework: net10.0
- Source files  : 11
- Existing Title: Markdown Parser, Block Model and Multi-Format Renderer
- Existing Desc : A markdown processor for sciBASIC#: a two-phase recursive block and inline parser that renders markdown to HTML or plain text, a typed block model with JSON, HTML and CSS styled renderers, nested table-of-contents generation from ATX headers, and block level conversion for report pipelines.
- Existing Tags : scibasic;markdown;parser;html-renderer;document;toc;blocks

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.MIME.text.markdown)

## Public types
- Class HeadingInfo (Heading\HeadingInfo.vb) - 从 markdown 源码中解析出的单个标题。 <see cref="Index"/> 与渲染后的 ``&lt;h1&gt;``..``&lt;h6&gt;`` 元素顺序一一对应 （见 <see cref="MarkdownHeadings.Parse"/>），因此可以作为 HTML 侧锚点、
- Module MarkdownHeadings (Heading\MarkdownHeadings.vb) - markdown 标题扫描器：与 <see cref="MarkdownRender"/> 的渲染结果保持一致的 标题解析实现，供目录生成、编辑器大纲与文档导航等场景复用。 解析规则：
- Module TOC (Heading\TOC.vb)
- Class MarkdownBlockParser (MarkdownBlocks.vb) - 将 markdown 文档文本解析为 <see cref="Block"/> 的平面列表。 采用与 <c>MarkdownParser</c> 相同思路的两阶段（块级判定 + 行内解析）递归结构， 但产出结构化 <see cref="Block"/> 对象而不是 HTML 字符串，且行内语法被转换为
- Class MarkdownParser (MarkdownParser.vb) - A two-phase recursive Markdown parser: block-level tokenization followed by inline parsing. Nested constructs (lists inside lists, block quotes containing lists/headers, tables with inline formatting) are handled naturally through
- Class MarkdownRender (MarkdownRender.vb) - Markdown text document transform its markup language format into html text format.
- Class NamespaceDoc (NamespaceDoc.vb) - A markdown processor: a two phase recursive block and inline parser, a typed block model with HTML and plain text renderers, and nested table of contents generation from ATX headers.
- Class HtmlRender (Render\HtmlRender.vb)
- Class MarkdownStyle (Render\MarkdownStyle.vb) - markdown html 主题样式：一段 CSS 文本 + 承载样式的 body class。 该对象只描述样式，不含任何渲染逻辑，通过 <c>New MarkdownRender(New HtmlRender(MarkdownStyle.GitHub))</c> 交给
- Class TextRender (Render\TextRender.vb)

## Notable public members
- Public Property Index As Integer
- Public Property Level As Integer
- Public Property Text As String
- Public Property LineStart As Integer
- Public Property LineLength As Integer
- Public Function Parse(markdown As String) As List(Of HeadingInfo)
- Public Function CleanHeadingText(raw As String) As String
- Public Function GenerateToc(md As String, Optional numbering As Boolean = False) As String
- Public Function AddToc(md As String, Optional numbering As Boolean = False, Optional autoSave As Boolean = False) As String
- Public Function Parse(markdown As String) As List(Of Block)
- Public Function Parse(markdown As String) As String
- Public Sub SetImageUrlRouter(router As Func(Of String, String))
- Public Function Transform(markdown As String) As String
- Public Function ParseBlocks(markdown As String) As List(Of Block)
- Public Shared Iterator Function GetTOC(md As String) As IEnumerable(Of NamedValue(Of Integer))
- Public Property Style As MarkdownStyle
- Public Sub New()
- Public Sub New(style As MarkdownStyle)
- Public Overrides Function Document(html As String) As String
- Public Overrides Function Paragraph(text As String, Optional createParagraphs As Boolean = True) As String
- Public Overrides Function Header(text As String, level As Integer) As String
- Public Overrides Function HorizontalLine() As String
- Public Overrides Function NewLine() As String
- Public Overrides Function Bold(text As String) As String
- Public Overrides Function Italic(text As String) As String
- Public Overrides Function Underline(text As String) As String
- Public Overrides Function Strikethrough(text As String) As String
- Public Overrides Function CodeSpan(text As String) As String
- Public Overrides Function CodeBlock(text As String, language As String) As String
- Public Overrides Function Image(url As String, alt As String, title As String) As String
- Public Overrides Function AnchorLink(url As String, text As String, title As String) As String
- Public Overrides Function BlockQuote(text As String) As String
- Public Overrides Function List(items As IEnumerable(Of String), orderList As Boolean, Optional startNumber As Integer = 1) As String
- Public Overrides Function Table(head() As String, rows As IEnumerable(Of String()), Optional align() As String = Nothing) As String
- Public Overrides Sub SetImageUrlRouter(router As Func(Of String, String))
- Public Property Name As String = "default"
- Public Property BodyClass As String = "markdown-body"
- Public Property Css As String = ""
- Public Function Clone() As MarkdownStyle
- Public Shared Function Create(css As String,
- Public Shared ReadOnly Property GitHub As MarkdownStyle
- Public Shared ReadOnly Property Dark As MarkdownStyle
- Public Shared ReadOnly Property Thesis As MarkdownStyle
- Public Delegate Function CodeHtmlSyntaxHighlight(code$, lang$) As String
- Public MustOverride Function Document(html As String) As String
- Public MustOverride Function Paragraph(text As String, Optional createParagraphs As Boolean = True) As String
- Public MustOverride Function Header(text As String, level As Integer) As String
- Public MustOverride Function HorizontalLine() As String
- Public MustOverride Function NewLine() As String
- Public MustOverride Function Bold(text As String) As String
- Public MustOverride Function Italic(text As String) As String
- Public MustOverride Function Underline(text As String) As String
- Public MustOverride Function Strikethrough(text As String) As String
- Public MustOverride Function CodeSpan(text As String) As String
- Public MustOverride Function CodeBlock(text As String, language As String) As String
- Public MustOverride Function Image(url As String, alt As String, title As String) As String
- Public MustOverride Function AnchorLink(url As String, text As String, title As String) As String
- Public MustOverride Function BlockQuote(text As String) As String
- Public MustOverride Function List(items As IEnumerable(Of String), orderList As Boolean, Optional startNumber As Integer = 1) As String
- Public MustOverride Function Table(head() As String, rows As IEnumerable(Of String()), Optional align() As String = Nothing) As String
- ... and 22 more

## Imports
- ASCII = Microsoft.VisualBasic.Text.ASCII
- Microsoft.VisualBasic.ApplicationServices.Report
- Microsoft.VisualBasic.ComponentModel.DataSourceModel
- Microsoft.VisualBasic.Language
- Microsoft.VisualBasic.Linq
- std = System.Math
- System.Text
- System.Text.RegularExpressions

## File tree
- Heading\HeadingInfo.vb
- Heading\MarkdownHeadings.vb
- Heading\TOC.vb
- MarkdownBlocks.vb
- MarkdownParser.vb
- MarkdownRender.vb
- NamespaceDoc.vb
- Render\HtmlRender.vb
- Render\MarkdownStyle.vb
- Render\Render.vb
- Render\TextRender.vb

