# Markdown Parser, Block Model and Multi-Format Renderer

Markdown processor for sciBASIC#: a two-phase recursive block and inline parser that renders markdown to HTML or plain text, a typed block model with markdown and HTML renderers driven by JSON documents, plus nested table-of-contents generation from ATX headers.

## Overview
- Two-phase recursive parser (`MarkdownParser`): block-level tokenization followed by inline span parsing, so nested lists, quotes containing headers/lists and tables with inline formatting work through recursion.
- Pluggable output via the abstract `Render` base class: `HtmlRender` emits HTML, `TextRender` emits plain text; `MarkdownRender` wires them together and exposes an image URL router.
- Typed block model (`JSONSchema.Block`): `JSONRenderer.Parse` turns a JSON document into an `IEnumerable(Of Block)` (useful for LLM output), and `ToMarkdown`/`ToHtml` render them back out; `BlockRenderer` converts a single block.
- Table of contents: `TOC.GenerateToc` builds a nested markdown list from ATX headers, `TOC.AddToc` prepends it to the document, and `MarkdownRender.GetTOC` returns header text with its level.

## Key Types
- `Microsoft.VisualBasic.MIME.text.markdown.MarkdownParser` — recursive block and inline parser that produces the rendered output string for a given `Render`.
- `Microsoft.VisualBasic.MIME.text.markdown.MarkdownRender` — entry point: `Transform` renders markdown to HTML (or to text with a `TextRender`), `GetTOC` lists ATX headers.
- `Microsoft.VisualBasic.MIME.text.markdown.Render` — abstract renderer contract implemented by `HtmlRender` and `TextRender`.
- `Microsoft.VisualBasic.MIME.text.markdown.HtmlRender` — renders markdown constructs to HTML tags.
- `Microsoft.VisualBasic.MIME.text.markdown.TextRender` — renders markdown constructs to plain text.
- `Microsoft.VisualBasic.MIME.text.markdown.JSONSchema.Block` — typed block model carrying type, level, content, headers, rows and list data.
- `Microsoft.VisualBasic.MIME.text.markdown.JSONSchema.JSONRenderer` — parses a JSON document into blocks and renders blocks to markdown or HTML.
- `Microsoft.VisualBasic.MIME.text.markdown.TOC` — generates and inserts a nested table of contents from ATX headers.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.MIME.text.markdown

Dim md As String = "# Title" & vbLf & "Hello **world** and `code`."

Dim html As String = New MarkdownRender().Transform(md)
Dim plain As String = New MarkdownRender(New TextRender()).Transform(md)
Dim toc As String = TOC.GenerateToc(md)

For Each header In MarkdownRender.GetTOC(md)
    Console.WriteLine($"H{header.Value}: {header.Name}")
Next

' JSON document (for example LLM output) -> markdown/HTML
Dim blocks As IEnumerable(Of JSONSchema.Block) = JSONSchema.JSONRenderer.Parse(json)
Dim html2 As String = JSONSchema.JSONRenderer.ToHtml(blocks)
```

## Package
- Assembly: `Microsoft.VisualBasic.MIME.text.markdown`
- TargetFramework: `net10.0`
- Tags: `scibasic;markdown;parser;html-renderer;document`

## License
GPL-3.0-or-later
