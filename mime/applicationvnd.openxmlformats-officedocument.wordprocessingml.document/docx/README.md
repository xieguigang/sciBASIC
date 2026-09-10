# DOCX Word Document Generator and Text Extractor

Fluent Office Open XML writer for sciBASIC# that builds a `.docx` package from code and reads plain text back out of an existing document.

## Overview
- Fluent `WordDocument` builder: document title, headings H1-H6, paragraphs, code blocks, block quotes, ordered/unordered/task/definition lists, horizontal rules, page breaks, tables and embedded images.
- Style model: `WordStyle` for character and paragraph formatting, `TableStyle` for table borders, headers and auto-fit, and `WordColors` with ready-made OOXML hex colors.
- Page setup in twips with A4/Letter presets or explicit width, height and margins; auto-generated table of contents with a configurable max heading level.
- Round-trip reading: `DocxTextReader` extracts plain text, a per-paragraph array, table text and `docProps/core.xml` metadata.
- Renders a `Microsoft.VisualBasic.MIME.text.markdown.JSONSchema.Block` sequence (the markdown block model) straight into a Word document.

## Key Types
- `Microsoft.VisualBasic.MIME.Office.WordDocument.WordDocument` — the fluent document builder and `IDocumentWriter` implementation; `Save` writes the `.docx` file.
- `Microsoft.VisualBasic.MIME.Office.WordDocument.IDocumentWriter` — shared document and metadata contract, also implemented by the PDF writer so the same code can emit either format.
- `Microsoft.VisualBasic.MIME.Office.WordDocument.DocxPackager` — packs body XML, styles and embedded images into the OPC (`.docx`) zip package.
- `Microsoft.VisualBasic.MIME.Office.WordDocument.DocxTextReader` — extracts plain text, paragraphs, table text and core metadata from a `.docx`.
- `Microsoft.VisualBasic.MIME.Office.WordDocument.WordStyle` — font name and size, bold/italic/underline, foreground/background color, alignment and spacing.
- `Microsoft.VisualBasic.MIME.Office.WordDocument.TableStyle` — table border, header and layout styling.
- `Microsoft.VisualBasic.MIME.Office.WordDocument.WordColors` — common OOXML 6-digit hex RGB color constants.
- `Microsoft.VisualBasic.MIME.Office.WordDocument.ImageHelper` — PNG/JPEG dimension probing and test image generation for embedded images.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.MIME.Office.WordDocument

Dim doc As New WordDocument(author:="sciBASIC", title:="Annual Report")

doc.PageSetupA4().
    DocTitle("Annual Report").
    H1("Introduction").
    Paragraph("Generated with sciBASIC#.").
    CodeBlock("Dim x As Integer = 1", "vbnet").
    Toc().
    Table({"Name", "Value"}, New String(,) {{"A", "1"}, {"B", "2"}}).
    PageBreak()

doc.Save("report.docx")

Dim text As String = New DocxTextReader().ExtractText("report.docx")
Dim meta As Dictionary(Of String, String) = New DocxTextReader().ExtractMetadata("report.docx")
```

## Package
- Assembly: `Microsoft.VisualBasic.MIME.Office.WordDocument`
- TargetFramework: `net10.0`
- Tags: `scibasic;docx;word;office-openxml;document-generation`

## License
GPL-3.0-or-later
