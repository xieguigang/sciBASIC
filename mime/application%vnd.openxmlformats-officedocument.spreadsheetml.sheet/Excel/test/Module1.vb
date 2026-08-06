#Region "Microsoft.VisualBasic::ccab767fc2f28e93176df6dd914e6a9c, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\test\Module1.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:


' Code Statistics:

'   Total Lines: 29
'    Code Lines: 21 (72.41%)
' Comment Lines: 1 (3.45%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 7 (24.14%)
'     File Size: 1.10 KB


' Module Module1
' 
'     Sub: testWriter, zip_test
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.MIME.Office.Excel
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.FileIO
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer.Styling

Module Module1

    Sub testWriter()
        Dim workbook As New Workbook("basic.xlsx", "SXheet124234") ' Create New workbook
        workbook.CurrentWorksheet.AddNextCell("Test") ' Add cell A1
        ' NOTE: Fill.ForegroundColor is NOT the font color - for a solid fill it is the
        ' cell background (the visible color for solid fills comes from fgColor). The
        ' font color is set via CurrentFont.ColorValue. The two are demonstrated below.
        workbook.CurrentWorksheet.AddNextCell(55.2, New Style With {
            .CurrentFill = New Fill With {.BackgroundColor = "#FF00FF00"},
            .CurrentFont = New Font With {.ColorValue = "#FFFF0000"}
        }) ' Add cell B1 (green fill, red font)
        workbook.CurrentWorksheet.AddNextCell(DateTime.Now) ' Add cell C1

        workbook.AddWorksheet("page_nooote")
        workbook.CurrentWorksheet.AddNextCell("Test22222") ' Add cell A1
        workbook.CurrentWorksheet.AddNextCell(4323355.2, New Style With {.CurrentFill = New Fill With {.BackgroundColor = "#FFFFBB66"}}) ' Add cell B1
        workbook.CurrentWorksheet.AddNextCell(DateTime.Now) ' Add cell C1

        Call testFillColors(workbook)
        Call testFontColors(workbook)
        Call testReport(workbook)

        workbook.Save()

        Pause()
    End Sub

    ''' <summary>
    ''' Writes a sheet where every row exercises one of the supported fill notations.
    ''' Open the generated file and compare the rendered color against the label in
    ''' column A: they must match, and no cell may turn up black unexpectedly.
    ''' </summary>
    Private Sub testFillColors(workbook As Workbook)
        workbook.AddWorksheet("fill_colors")

        Dim sheet = workbook.CurrentWorksheet

        ' background color only, hash prefixed with alpha - the original bug report
        Call writeFillSample(sheet, "#FFFFBB66 -> orange", New Fill With {.BackgroundColor = "#FFFFBB66"})
        ' background color only, hash prefixed without alpha
        Call writeFillSample(sheet, "#4472C4 -> blue", New Fill With {.BackgroundColor = "#4472C4"})
        ' background color only, plain 8 digit notation
        Call writeFillSample(sheet, "FF70AD47 -> green", New Fill With {.BackgroundColor = "FF70AD47", .ForegroundColor = "FFFF0000"})
        ' background color only, plain 6 digit notation (alpha is completed to FF)
        Call writeFillSample(sheet, "ED7D31 -> amber", New Fill With {.BackgroundColor = "ED7D31"})
        ' lower case input has to be normalized as well
        Call writeFillSample(sheet, "#ffc000 -> yellow", New Fill With {.BackgroundColor = "#ffc000"})
        ' foreground color only, which is what SetColor / ColorizedBackground produce
        Call writeFillSample(sheet, "foreground A5A5A5 -> gray", New Fill With {.ForegroundColor = "FFA5A5A5"})
        ' both colors set - background wins for a solid fill
        Call writeFillSample(sheet, "fg black + bg red -> red", New Fill With {.ForegroundColor = "FF000000", .BackgroundColor = "FFFF0000"})
        ' an explicitly requested black background must stay black
        Call writeFillSample(sheet, "#FF000000 -> black", New Fill With {.BackgroundColor = "#FF000000"})
        ' non solid pattern fill keeps its original fgColor / bgColor semantics
        Call writeFillSample(sheet, "gray125 pattern", New Fill With {.PatternFill = PatternValue.gray125})
        ' built in helper style has to stay compatible
        Call writeFillSample(sheet, "ColorizedBackground(00B0F0)", BasicStyles.ColorizedBackground("00B0F0").CurrentFill)

        ' no fill at all - this cell must render without any shading
        sheet.AddNextCell("no fill -> transparent")
        sheet.AddNextCell("sample")
        sheet.GoToNextRow()

        ' a default constructed fill is still "none" and must not paint anything
        Call writeFillSample(sheet, "default Fill() -> transparent", New Fill())
    End Sub

    Private Sub writeFillSample(sheet As Worksheet, label As String, fill As Fill)
        sheet.AddNextCell(label)
        sheet.AddNextCell("sample", New Style With {.CurrentFill = fill})
        sheet.GoToNextRow()
    End Sub

    ''' <summary>
    ''' Writes a sheet where every row exercises one of the supported font color notations.
    ''' Open the generated file and compare the rendered font color against the label in
    ''' column A: they must match. A font color must never silently fall back to black.
    ''' </summary>
    Private Sub testFontColors(workbook As Workbook)
        workbook.AddWorksheet("font_colors")

        Dim sheet = workbook.CurrentWorksheet

        ' hash prefixed with alpha - the original failure case (was written verbatim as
        '"#FF00FF00" and ignored by Excel, falling back to black)
        Call writeFontSample(sheet, "#FF00FF00 -> green", New Font With {.ColorValue = "#FF00FF00"})
        ' hash prefixed without alpha (completed to FF)
        Call writeFontSample(sheet, "#4472C4 -> blue", New Font With {.ColorValue = "#4472C4"})
        ' plain 6 digit notation (alpha is completed to FF)
        Call writeFontSample(sheet, "ED7D31 -> amber", New Font With {.ColorValue = "ED7D31"})
        ' lower case input has to be normalized as well
        Call writeFontSample(sheet, "#ff0000 -> red", New Font With {.ColorValue = "#ff0000"})
        ' font color + background color combined: they must not interfere with each other
        Call writeFontSample(sheet, "green font on orange fill", New Font With {.ColorValue = "#FF00FF00"}, New Fill With {.BackgroundColor = "#FFFFBB66"})
        ' no ColorValue at all - must fall back to the default theme color (not black)
        Call writeFontSample(sheet, "default font -> theme color", New Font())
        ' built in helper style has to stay compatible
        Call writeFontSample(sheet, "ColorizedText(00B0F0) -> light blue", BasicStyles.ColorizedText("00B0F0").CurrentFont)
        ' border color with a hash prefix must also be normalized
        Call writeFontSample(sheet, "red font + red border", New Font With {.ColorValue = "#FFFF0000"}, border:=New Border With {.BottomColor = "#FFFF0000", .BottomStyle = StyleValue.thin})
    End Sub

    Private Sub writeFontSample(sheet As Worksheet, label As String, font As Font, Optional fill As Fill = Nothing, Optional border As Border = Nothing)
        sheet.AddNextCell(label)
        Dim style As New Style()
        style.CurrentFont = font
        If fill IsNot Nothing Then style.CurrentFill = fill
        If border IsNot Nothing Then style.CurrentBorder = border
        sheet.AddNextCell("sample", style)
        sheet.GoToNextRow()
    End Sub

    ''' <summary>
    ''' Demonstrates the reusable report writer: a styled comment row, a header row,
    ''' body rows with italic row-titles in the first column, and a B2 freeze anchor.
    ''' </summary>
    Private Sub testReport(workbook As Workbook)
        Dim headers = {"Sample", "Mean", "StdDev", "p-value"}
        Dim rowTitles = {"Control", "Treatment A", "Treatment B"}
        Dim data As IEnumerable(Of IEnumerable(Of Object)) = {
            New Object() {"12.3", "1.1", "0.042"},
            New Object() {"15.7", "1.4", "0.011"},
            New Object() {"18.2", "1.9", "0.003"}
        }
        Call workbook.WriteReportSheet(
                              "report",
                              "Generated by automated report tool - experiment #2026-08-06",
                              headers,
                              rowTitles,
                              data)
    End Sub

    Sub zip_test()
        Dim xlsx As New ZipStream("basic.xlsx", is_readonly:=True)
        Dim reader = xlsx.LoadZip

        Pause()
    End Sub
End Module
