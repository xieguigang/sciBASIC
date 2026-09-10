# PDF Document Reader, Text Extractor and Writer Library

Pure managed PDF support for sciBASIC#: read page text out of existing documents and generate new PDF files from a streaming document API.

## Overview
- Low-level reader: lexer, object parser, xref table resolution, object/stream decoding (Flate, ASCII85, predictors) and page-tree traversal.
- Text extraction that maps character codes back to Unicode through ToUnicode CMaps and font information.
- Streaming writer: content blocks (headings, paragraphs, tables, images) are collected, laid out into pages and serialized as a standards-conforming PDF.
- The writer implements `IDocumentWriter`, so code written against the WordDocument surface can emit PDF instead.

## Key Types
- `Microsoft.VisualBasic.MIME.application.pdf.PDF` — module entry point; `GetText(Stream)` yields the text of each page in order.
- `PdfReader` — parses a file path or byte buffer, resolves indirect references and enumerates page dictionaries.
- `PdfLexer` / `PdfObjectParser` / `PdfObject` family — tokenizer and parser producing `PdfDictionary`, `PdfArray`, `PdfStream`, `PdfIndirectObject`, ...
- `FlateDecode` — stream filters: deflate decompression, ASCII85 decoding and predictor reversal.
- `TextExtractor` / `ToUnicodeCMap` — rebuild readable text for a page from content streams and font CMaps.
- `PdfDocument` — implements `IDocumentWriter`; fluent `Heading` / `Paragraph` / `Table` / `Image` API plus `Save`.
- `PdfLayoutEngine` / `PdfWriter` — paginates blocks into pages and serializes objects, content streams, xref table and trailer.
- `PdfFontResource` / `PdfImageXObject` / `PdfColor` — font metrics and encoding, image XObject encoding, and RGB colour operators.

## Quick Start
```vbnet
Imports System.IO
Imports Microsoft.VisualBasic.MIME.application.pdf

' read: page text, page by page
Using fs As Stream = File.OpenRead("input.pdf")
    For Each page As String In PDF.GetText(fs)
        Console.WriteLine(page)
    Next
End Using

' write: a new document
Dim doc As New PdfDocument(author:="sciBASIC", title:="Report")

Call doc.Heading(1, "Results").Paragraph("Hello PDF")
Call doc.Table({"Name", "Value"}, {{"a", "1"}, {"b", "2"}})
Call doc.Save("report.pdf")
```

## Package
- Assembly: `Microsoft.VisualBasic.MIME.application.pdf`
- TargetFramework: `net10.0`
- Tags: `scibasic;pdf;pdf-reader;text-extraction;pdf-writer`

## License
GPL-3.0-or-later
