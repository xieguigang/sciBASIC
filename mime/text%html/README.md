# HTML/CSS Parser, DOM and CSS Style Environment

An HTML and CSS toolkit for sciBASIC#: tokenizes HTML into a queryable DOM, parses CSS selectors, rule files and property values into typed models, and resolves CSS lengths, fonts and strokes against a canvas style environment.

## Overview
- Tokenizer and DOM: `TokenIcer` produces a `Token` stream (`HtmlTokens`), `HtmlParser.ParseTree` builds a queryable `HtmlDocument`, and `HtmlDocument.LoadDocument` loads from a file or URL with optional tag stripping.
- DOM queries on `HtmlElement`: `getElementById`, `getElementsByName`, `getElementsByTagName`, `getElementsByClassName`, `GetAllChilds`, `GetAllChildsByNodeName` and `hasAttribute`.
- CSS parsing: `CssParser.ParseStyle` and `CssParser.GetTagWithCSS` build `Selector` and `CSSFile` models; `CssProperty`, `CSSSelectorTypes` and `HtmlTags` enumerate the supported properties, selectors and tags.
- CSS value model: `Fill`, `Stroke`, `Padding`, `CSSsize`, `CSSFont` and `CSSEnvirnment` turn CSS strings into brushes, pens, sizes and fonts for the imaging layer.
- Extras: `HtmlCompress`, `DocumentFormatter` (link/URL/e-mail highlighting in plain text) and `StripHTMLDirectly` for text extraction.
- Note: the GDI+ box layout and painting engine under the `Render` namespace (`HtmlRenderer.Render`, `InitialContainer`, `CssBox`, `CssLayoutEngine`, `CssTable`) is compiled only when the `NET48` constant is defined; it is not part of the `net10.0` build.

## Key Types
- `Microsoft.VisualBasic.MIME.Html.Language.TokenIcer` — tokenizes HTML source into a stream of `Token` values.
- `Microsoft.VisualBasic.MIME.Html.Language.HtmlParser` — builds an `HtmlDocument` DOM from HTML text.
- `Microsoft.VisualBasic.MIME.Html.Document.HtmlDocument` — root document element, loadable from a file handle or URL.
- `Microsoft.VisualBasic.MIME.Html.Document.HtmlElement` — DOM node with attributes, child nodes and id/name/tag/class queries.
- `Microsoft.VisualBasic.MIME.Html.Language.CSS.CssParser` — parses style strings and selector blocks into `Selector` and `CSSFile` models.
- `Microsoft.VisualBasic.MIME.Html.Language.CSS.Selector` — a CSS rule: selector text, type and its property set.
- `Microsoft.VisualBasic.MIME.Html.CSS.CSSEnvirnment` — canvas context (size, dpi, base font, base stroke) used to resolve CSS lengths, fonts and strokes.
- `Microsoft.VisualBasic.MIME.Html.Render.HtmlRenderer` — GDI+ painting entry point (`NET48` only) plus `LoadEnvironment` for the graphics canvas.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.MIME.Html.Document
Imports Microsoft.VisualBasic.MIME.Html.Language
Imports Microsoft.VisualBasic.MIME.Html.Language.CSS

Dim html As String = IO.File.ReadAllText("page.html")
Dim doc As HtmlDocument = HtmlParser.ParseTree(html)

For Each link As HtmlElement In doc.getElementsByTagName("a")
    Console.WriteLine(link.ToString())
Next

Dim styles As CSSFile = CssParser.GetTagWithCSS("a { color: #0066cc; }")
Dim rule As Selector = CssParser.ParseStyle("h1 { font-size: 24; font-family: Microsoft YaHei; }")
```

## Package
- Assembly: `Microsoft.VisualBasic.MIME.Html`
- TargetFramework: `net10.0`
- Tags: `scibasic;html;css;html-renderer;dom`

## License
GPL-3.0-or-later
