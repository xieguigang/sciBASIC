#Region "Microsoft.VisualBasic::3073b2e01657ca78d51e3e12c14e6eb0, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLS\BIFF\Form1.vb"

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

    '   Total Lines: 144
    '    Code Lines: 51
    ' Comment Lines: 48
    '   Blank Lines: 45
    '     File Size: 7.02 KB


    ' Module test
    ' 
    '     Sub: cmdCreate_Click, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.Office.Excel.XLS.BIFF

Public Module test

    Sub Main()
        Call cmdCreate_Click()
    End Sub

    Private Sub cmdCreate_Click()

        Dim myExcelFile As New BiffWriter

        With myExcelFile
            'Create the new spreadsheet
            Dim FileName$ = $"{App.HOME}\vbtest.xls"  'create spreadsheet in the current directory
            .CreateFile(FileName)

            'set a Password for the file. If set, the rest of the spreadsheet will
            'be encrypted. If a password is used it must immediately follow the
            'CreateFile method.
            'This is different then protecting the spreadsheet (see below).
            'NOTE: For some reason this function does not work. Excel will
            'recognize that the file is password protected, but entering the password
            'will not work. Also, the file is not encrypted. Therefore, do not use
            'this function until I can figure out why it doesn't work. There is not
            'much documentation on this function available.
            '.SetFilePassword "PAUL"



            'specify whether to print the gridlines or not
            'this should come before the setting of fonts and margins
            .SetPrintGridLines(False)


            'it is a good idea to set margins, fonts and column widths
            'prior to writing any text/numerics to the spreadsheet. These
            'should come before setting the fonts.

            .SetMargin(MarginTypes.xlsTopMargin, 1.5)  'set to 1.5 inches
            .SetMargin(MarginTypes.xlsLeftMargin, 1.5)
            .SetMargin(MarginTypes.xlsRightMargin, 1.5)
            .SetMargin(MarginTypes.xlsBottomMargin, 1.5)


            'to insert a Horizontal Page Break you need to specify the row just
            'after where you want the page break to occur. You can insert as many
            'page breaks as you wish (in any order).
            .InsertHorizPageBreak(10)
            .InsertHorizPageBreak(20)

            'set a default row height for the entire spreadsheet (1/20th of a point)
            .SetDefaultRowHeight(14)


            'Up to 4 fonts can be specified for the spreadsheet. This is a
            'limitation of the Excel 2.1 format. For each value written to the
            'spreadsheet you can specify which font to use.

            .SetFont("Arial", 10, FontFormatting.xlsNoFormat)              'font0
            .SetFont("Arial", 10, FontFormatting.xlsBold)           'font1
            .SetFont("Arial", 10, FontFormatting.xlsBold + FontFormatting.xlsUnderline)   'font2
            .SetFont("Courier", 16, FontFormatting.xlsBold + FontFormatting.xlsItalic)          'font3


            'Column widths are specified in Excel as 1/256th of a character.
            .SetColumnWidth(1, 5, 18)

            'Set special row heights for row 1 and 2
            .SetRowHeight(1, 30)
            .SetRowHeight(2, 30)


            'set any header or footer that you want to print on
            'every page. This text will be centered at the top and/or
            'bottom of each page. The font will always be the font that
            'is specified as font0, therefore you should only set the
            'header/footer after specifying the fonts through SetFont.
            .SetHeader("BIFF 2.1 API")
            .SetFooter("Paul Squires - Excel BIFF Class")

            'write a normal left aligned string using font3 (Courier Italic)
            .WriteValue(ValueTypes.xlsText, CellFont.xlsFont3, CellAlignment.xlsLeftAlign, CellHiddenLocked.xlsNormal, 1, 1, "Quarterly Report")
            .WriteValue(ValueTypes.xlsText, CellFont.xlsFont1, CellAlignment.xlsLeftAlign, CellHiddenLocked.xlsNormal, 2, 1, "Cool Guy Corporation")

            'write some data to the spreadsheet
            'Use the default format #3 "#,##0" (refer to the WriteDefaultFormats function)
            'The WriteDefaultFormats function is compliments of Dieter Hauk in Germany.
            .WriteValue(ValueTypes.xlsinteger, CellFont.xlsFont0, CellAlignment.xlsLeftAlign, CellHiddenLocked.xlsNormal, 6, 1, 2000, 3)


            'write a cell with a shaded number with a bottom border
            .WriteValue(ValueTypes.xlsnumber, CellFont.xlsFont1,
                        CellAlignment.xlsrightAlign + CellAlignment.xlsBottomBorder + CellAlignment.xlsShaded,
                        CellHiddenLocked.xlsNormal, 7, 1, 12123.456, 4)

            'write a normal left aligned string using font2 (bold & underline)
            .WriteValue(ValueTypes.xlsText, CellFont.xlsFont2, CellAlignment.xlsLeftAlign, CellHiddenLocked.xlsNormal, 8, 1, "This is a test string")

            'write a locked cell. The cell will not be able to be overwritten, BUT you
            'must set the sheet PROTECTION to on before it will take effect!!!
            .WriteValue(ValueTypes.xlsText, CellFont.xlsFont3, CellAlignment.xlsLeftAlign, CellHiddenLocked.xlsLocked, 9, 1, "This cell is locked")

            'fill the cell with "F"'s
            .WriteValue(ValueTypes.xlsText, CellFont.xlsFont0, CellAlignment.xlsFillCell, CellHiddenLocked.xlsNormal, 10, 1, "F")

            'write a hidden cell to the spreadsheet. This only works for cells
            'that contain formulae. Text, Number, Integer value text can not be hidden
            'using this feature. It is included here for the sake of completeness.
            .WriteValue(ValueTypes.xlsText, CellFont.xlsFont0, CellAlignment.xlsCentreAlign, CellHiddenLocked.xlsHidden, 11, 1, "If this were a formula it would be hidden!")


            'write some dates to the file. NOTE: you need to write dates as xlsNumber
            Dim d As Date
            d = #1/15/2001 00:00:00#
            .WriteValue(ValueTypes.xlsnumber, CellFont.xlsFont0, CellAlignment.xlsCentreAlign, CellHiddenLocked.xlsNormal, 15, 1, d, 12)

            d = #12/31/1999#
            .WriteValue(ValueTypes.xlsnumber, CellFont.xlsFont0, CellAlignment.xlsCentreAlign, CellHiddenLocked.xlsNormal, 16, 1, d, 12)

            d = #01/04/2002#
            .WriteValue(ValueTypes.xlsnumber, CellFont.xlsFont0, CellAlignment.xlsCentreAlign, CellHiddenLocked.xlsNormal, 17, 1, d, 12)

            d = #2/11/1998#
            .WriteValue(ValueTypes.xlsnumber, CellFont.xlsFont0, CellAlignment.xlsCentreAlign, CellHiddenLocked.xlsNormal, 18, 1, d, 12)

            'PROTECT the spreadsheet so any cells specified as LOCKED will not be
            'overwritten. Also, all cells with HIDDEN set will hide their formulae.
            'PROTECT does not use a password.
            .SetProtectSpreadsheet(True)


            'Finally, close the spreadsheet
            .CloseFile

            ' MsgBox "Excel BIFF Spreadsheet created." & vbCrLf & "Filename: " & FileName$, vbInformation + vbOKOnly, "Excel Class"

        End With


    End Sub


End Module
