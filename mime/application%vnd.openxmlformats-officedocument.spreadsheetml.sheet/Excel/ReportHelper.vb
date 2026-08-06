Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer.Styling

''' <summary>
''' Helper for writing scientific-data report sheets with a consistent, pre-styled layout.
'''
''' Layout applied by <see cref="WriteReportSheet"/>:
'''   - Row 1  : comment line   - white background, italic grass-green font, left aligned, merged across all columns.
'''   - Row 2  : header line    - dark-blue background, white bold font.
'''   - Row 3+ : body           - default style (Cambria 11). First column of each row is an
'''                              italic dark-grey row-title.
'''   - Freeze : panes frozen at B2 (first row + first column).
''' </summary>
Public Module ReportHelper

    ' Colour constants (normalised on assignment by the style properties).
    Private ReadOnly COLOR_COMMENT_FONT As String = "#70AD47"   ' grass green
    Private ReadOnly COLOR_HEADER_FILL As String = "#1F4E78"    ' dark blue
    Private ReadOnly COLOR_HEADER_FONT As String = "#FFFFFFFF"  ' white
    Private ReadOnly COLOR_ROWTITLE_FONT As String = "#595959"  ' dark grey
    Private ReadOnly COLOR_WHITE_FILL As String = "#FFFFFFFF"   ' white

    ''' <summary>
    ''' Writes a fully styled report sheet into the given workbook.
    ''' </summary>
    ''' <param name="workbook">Target workbook (the new sheet is added and made current).</param>
    ''' <param name="sheetName">Name of the worksheet to create.</param>
    ''' <param name="commentText">Text shown in the merged comment row (row 1).</param>
    ''' <param name="headers">Column titles (row 2). Count defines the number of data columns.</param>
    ''' <param name="rowTitles">Row titles for the first column (row 3+). One per data row.</param>
    ''' <param name="data">Body values, one inner enumerable per data row (matching <paramref name="rowTitles"/> order).</param>
    ''' <returns>The created and styled <see cref="Worksheet"/> for further manipulation.</returns>
    ''' 
    <Extension>
    Public Function WriteReportSheet(workbook As Workbook,
                                     sheetName As String,
                                     commentText As String,
                                     headers As IEnumerable(Of String),
                                     rowTitles As IEnumerable(Of String),
                                     data As IEnumerable(Of IEnumerable(Of Object))) As Worksheet

        Dim headerList = headers.ToList()
        Dim rowTitleList = rowTitles.ToList()
        Dim dataList = data.ToList()

        Dim columnCount = headerList.Count
        If columnCount < 1 Then columnCount = 1

        Dim sheet = workbook.AddWorksheet(sheetName)

        ' NOTE: this library applies the active style to every cell *before* an explicitly
        ' passed style, so an explicit style passed to AddNextCell is effectively ignored
        ' while an active style is set. We therefore disable the active style and pass a
        ' fully self-contained style for every cell instead.
        sheet.SetActiveStyle(Nothing)

        ' Self-contained base font (Cambria 11) shared by every style in the sheet.
        Dim baseFont As New Font() With {.Name = "Cambria", .Size = 11.0F}

        ' 1) Default style for body cells.
        Dim defaultStyle As New Style() With {.CurrentFont = CType(baseFont.Copy(), Font)}

        ' 2) Comment row (row 1): white background, italic grass-green font, left aligned.
        Dim commentStyle As New Style() With {
            .CurrentFill = New Fill() With {.BackgroundColor = COLOR_WHITE_FILL},
            .CurrentFont = New Font() With {.Name = "Cambria", .Size = 11.0F, .ColorValue = COLOR_COMMENT_FONT, .Italic = True},
            .CurrentCellXf = New CellXf() With {.HorizontalAlign = HorizontalAlignValue.left}
        }
        sheet.AddNextCell(commentText, commentStyle)

        ' Merge the whole first row (A1 : lastColumn1).
        Dim lastColLetter = ColumnLetter(columnCount)
        sheet.MergeCells("A1:" & lastColLetter & "1")
        sheet.GoToNextRow()

        ' 3) Header row (row 2): dark-blue background, white bold font.
        Dim headerStyle As New Style() With {
            .CurrentFill = New Fill() With {.BackgroundColor = COLOR_HEADER_FILL},
            .CurrentFont = New Font() With {.Name = "Cambria", .Size = 11.0F, .ColorValue = COLOR_HEADER_FONT, .Bold = True}
        }
        For Each h In headerList
            sheet.AddNextCell(h, headerStyle)
        Next
        sheet.GoToNextRow()

        ' 4) Body rows (row 3+): first column italic dark-grey row-title, rest use default style.
        Dim rowTitleStyle As New Style() With {
            .CurrentFont = New Font() With {.Name = "Cambria", .Size = 11.0F, .ColorValue = COLOR_ROWTITLE_FONT, .Italic = True}
        }
        For i As Integer = 0 To rowTitleList.Count - 1
            sheet.AddNextCell(rowTitleList(i), rowTitleStyle)
            If i < dataList.Count Then
                For Each cellValue In dataList(i)
                    sheet.AddNextCell(cellValue, defaultStyle)
                Next
            End If
            sheet.GoToNextRow()
        Next

        ' 5) Freeze first row and first column, anchored at B2.
        ' NOTE: SetVerticalSplit / SetHorizontalSplit each overwrite the shared pane state,
        ' so both must be set in a single combined SetSplit call.
        Dim anchor As New Address("B2")
        sheet.SetSplit(1, 1, freeze:=True, topLeftCell:=anchor, activePane:=WorksheetPane.bottomRight)

        Return sheet
    End Function

    ''' <summary>
    ''' Converts a 1-based column index to its spreadsheet letter(s), e.g. 1 -> "A", 28 -> "AB".
    ''' </summary>
    Private Function ColumnLetter(columnIndex As Integer) As String
        Dim letters As String = ""
        Dim idx = columnIndex
        While idx > 0
            Dim rem_ = (idx - 1) Mod 26
            letters = ChrW(Asc("A"c) + rem_) & letters
            idx = (idx - 1) \ 26
        End While
        Return letters
    End Function

End Module
