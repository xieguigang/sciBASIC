# mime/text%markdown/markdown.NET5.vbproj

- RootNamespace : Microsoft.VisualBasic.MIME.text.markdown
- AssemblyName  : Microsoft.VisualBasic.MIME.text.markdown
- TargetFramework: net10.0
- Source files  : 9
- Existing Title: Markdown Parser, Block Model and Multi-Format Renderer
- Existing Desc : A markdown processor for sciBASIC#: parses CommonMark-style blocks and inline spans into a typed block model or JSON document, renders it to HTML or plain text, and generates nested tables of contents from ATX headers.
- Existing Tags : scibasic;markdown;parser;html-renderer;document

## Namespaces
- JSONSchema  [files: 3]

## Public types
- Class Block (JSONSchema\Block.vb) - 针对markdown格式有限的支持
- Module BlockRenderer (JSONSchema\BlockRenderer.vb) - markdown block rendering extensions, converts a single <see cref="Block"/> object into its markdown or html text fragment.
- Module JSONRenderer (JSONSchema\JSONRenderer.vb) - Markdown render helper for LLM outputs
- Class MarkdownParser (MarkdownParser.vb) - A two-phase recursive Markdown parser: block-level tokenization followed by inline parsing. Nested constructs (lists inside lists, block quotes containing lists/headers, tables with inline formatting) are handled naturally through
- Class MarkdownRender (MarkdownRender.vb) - Markdown text document transform its markup language format into html text format.
- Class HtmlRender (Render\HtmlRender.vb)
- Class TextRender (Render\TextRender.vb)
- Module TOC (TOC.vb)

## Notable public members
- Public Property type As String
- Public Property level As Integer
- Public Property content As String
- Public Property language As String
- Public Property ordered As Boolean
- Public Property items As String()
- Public Property headers As String()
- Public Property alignments As String()
- Public Property rows As String()()
- Public Property url As String
- Public Property alt As String
- Public Property title As String
- Public Property checked As Boolean()
- Public Property id As String
- Public Property terms As String()
- Public Property definitions As String()
- Public Function ToMarkdownBlock(block As Block) As String
- Public Function ToHtmlBlock(block As Block) As String
- Public Iterator Function Parse(jsonstr As String) As IEnumerable(Of Block)
- Public Function ToMarkdown(docBlocks As IEnumerable(Of Block)) As String
- Public Function ToHtml(docBlocks As IEnumerable(Of Block)) As String
- Public Function Parse(markdown As String) As String
- Public Sub SetImageUrlRouter(router As Func(Of String, String))
- Public Function Transform(markdown As String) As String
- Public Shared Iterator Function GetTOC(md As String) As IEnumerable(Of NamedValue(Of Integer))
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
- Public MustOverride Sub SetImageUrlRouter(router As Func(Of String, String))
- Protected Function router(url As String) As String
- ... and 20 more

## Imports
- ASCII = Microsoft.VisualBasic.Text.ASCII
- Microsoft.VisualBasic.ComponentModel.DataSourceModel
- Microsoft.VisualBasic.Language
- Microsoft.VisualBasic.Linq
- Microsoft.VisualBasic.MIME.application.json
- Microsoft.VisualBasic.MIME.application.json.Javascript
- Microsoft.VisualBasic.MIME.application.json.LenientJson
- std = System.Math
- System.Runtime.CompilerServices
- System.Text
- System.Text.RegularExpressions

## File tree
- JSONSchema\Block.vb
- JSONSchema\BlockRenderer.vb
- JSONSchema\JSONRenderer.vb
- MarkdownParser.vb
- MarkdownRender.vb
- Render\HtmlRender.vb
- Render\Render.vb
- Render\TextRender.vb
- TOC.vb

