# Rich Text Format and Office MathML Document Model

A compact document model that builds Rich Text Format markup, together with serializable Office/OMML XML models, for the sciBASIC# framework.

## Overview
- Build a document incrementally with `AppendText` / `AppendLine`, each call optionally carrying its own font style.
- Track styled regions over the text buffer and re-apply or split formats with `SetFormat`.
- Emit an RTF 1.x document (font table, colour table, generator metadata) through `Save`.
- Serializable Office/OMML XML models — document properties, word-document settings, math properties and an html head — for Word XML round-tripping.

## Key Types
- `Microsoft.VisualBasic.MIME.RTF.Rtf` — the document object model: appends styled text and writes the `.rtf` file.
- `Microsoft.VisualBasic.MIME.RTF.Font` — font style of a text region and generator of the RTF style tags (`\b`, `\i`, `\fs`, `\ul`).
- `Models.Font` — plain font data (family, size, bold, italic, underline, colour) inherited by `Font`.
- `FormatedRegion` — one styled span of the document; knows its start/right offsets and renders its own RTF text.
- `Omml.HTML` — root model of an Office html/xml document carrying the Word/VML/OMML namespace declarations.
- `Omml.DocumentXmlProperty` / `Omml.DocumentProperties` — `xml` and property blocks serializable to Office XML.
- `Omml.WordDocument` / `Omml.OfficeDocumentSettings` / `Omml.mathPr` — Word document settings, compatibility and math property models.

## Quick Start
```vbnet
Imports System.Drawing
Imports Microsoft.VisualBasic.MIME.RTF

Dim doc As New Rtf("Cambria", 11, Color.Black)

Call doc.AppendLine("Hello sciBASIC#")
Call doc.AppendLine("styled text",
                    New Font(size:=11, Bold:=True, Name:="Cambria",
                             Italic:=False, Underline:=False, Color:=Color.Red))
Call doc.Save("demo.rtf")
```

## Package
- Assembly: `Microsoft.VisualBasic.MIME.RTF`
- TargetFramework: `net4.8`
- Tags: `scibasic;rtf;rich-text;omml;mathml;office-xml`

## License
GPL-3.0-or-later
