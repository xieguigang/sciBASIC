# XLSX Workbook Reader, Writer and Legacy BIFF Writer

Office Open XML (SpreadsheetML) support for sciBASIC#: read an xlsx package into typed parts and write fully styled workbooks, with an additional legacy BIFF 2.1 writer.

## Overview
- Read: open the xlsx zip package into typed directory parts (content types, rels, docProps, xl with worksheets, sharedStrings, styles, theme, calcChain) and expose sheets as table frames or typed records.
- Write: create workbooks, worksheets, cells, formulas, ranges, column widths and frozen panes, then save to a file or stream.
- Styling: fonts, fills, borders, number formats, alignments and a managed style repository applied per cell or per range.
- Legacy BIFF 2.1 record writer for producing old-format `.xls` files, plus a report helper that lays out pre-styled scientific report sheets.

## Key Types
- `Microsoft.VisualBasic.MIME.Office.Excel.XLSX.File` — an opened xlsx package; `Open`, `SheetNames`, `GetTable`, `GetWorksheet`, `LoadDataSet(Of T)`, `IDisposable`.
- `XLSX.FileIO.ZipPackage` / `LowLevel` / `DocumentPath` — zip package handling, XML writing and packing of the OPC parts.
- `XLSX.Writer.Workbook` — workbook generator: `AddWorksheet`, `SetCurrentWorksheet`, `Save` / `SaveAs` / `SaveAsStream`.
- `XLSX.Writer.Worksheet.Worksheet` — the cell grid: `AddCell`, `AddCellFormula`, column widths, merged cells and active style.
- `XLSX.Writer.Cell.Cell` / `Cell.Address` / `Cell.CellType` — one typed cell plus A1/RC address resolution.
- `XLSX.Writer.Styling.Style` (with `Font`, `Fill`, `Border`, `NumberFormat`, `CellXf`) — the styling model consumed by `StyleManager`.
- `XLSX.Model.StoreProcedure` / `SheetTable` — read a worksheet into a csv frame, or save a csv frame into a worksheet.
- `XLS.BIFF.BiffWriter` — writes legacy Excel BIFF 2.1 record streams (margins, fonts, column widths, headers/footers, protection).
- `Microsoft.VisualBasic.MIME.Office.Excel.ReportHelper` — extension that writes a pre-styled report sheet into a workbook.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer

' read an existing workbook
Using xlsx As File = File.Open("data.xlsx")
    For Each name As String In xlsx.SheetNames()
        Console.WriteLine(name)
    Next
End Using

' write a new workbook
Dim book As New Workbook("out.xlsx", "Sheet1")

Call book.CurrentWorksheet.AddCell("Name", "A1")
Call book.CurrentWorksheet.AddCell(3.14159, "B1")
Call book.Save()
```

## Package
- Assembly: `Microsoft.VisualBasic.MIME.Office.Excel`
- TargetFramework: `net10.0`
- Tags: `scibasic;xlsx;excel;openxml;spreadsheet`

## License
GPL-3.0-or-later
