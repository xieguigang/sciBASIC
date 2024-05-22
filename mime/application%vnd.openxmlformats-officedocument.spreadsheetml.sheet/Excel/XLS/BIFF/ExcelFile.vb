#Region "Microsoft.VisualBasic::cfb39159bc0fb76eb7606a5c38d613a6, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLS\BIFF\ExcelFile.vb"

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

    '   Total Lines: 506
    '    Code Lines: 351 (69.37%)
    ' Comment Lines: 68 (13.44%)
    '    - Xml Docs: 32.35%
    ' 
    '   Blank Lines: 87 (17.19%)
    '     File Size: 18.36 KB


    '     Class BiffWriter
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CloseFile, CreateFile, InsertHorizPageBreak, SetColumnWidth, SetDefaultRowHeight
    '                   SetFilePassword, SetFont, SetFooter, SetHeader, SetMargin
    '                   SetRowHeight, WriteDefaultFormats, WriteValue
    ' 
    '         Sub: SetPrintGridLines, SetProtectSpreadsheet
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Serialization.BinaryDumping

Namespace XLS.BIFF

    ''' <summary>
    ''' Class file for writing Microsoft Excel BIFF 2.1 files.
    '''
    ''' This class is intended for users who do not want to use the huge
    ''' Jet or ADO providers if they only want to export their data to
    ''' an Excel compatible file.
    '''
    ''' Newer versions of Excel use the OLE Structure Storage methods
    ''' which are quite complicated.
    '''
    ''' Paul Squires, November 10, 2001
    ''' rambo2000@canada.com
    '''
    ''' Added default-cellformats: Dieter Hauk January 8, 2001 dieter.hauk@epost.de
    ''' Added default row height: Matthew Brewster November 9, 2001
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/Planet-Source-Code/paul-squires-excel-class-write-to-an-xls-file-without-dll-s-or-excel-automation__1-11898
    ''' </remarks>
    Public Class BiffWriter

        Private FileNumber As BinaryWriter
        Private BEG_FILE_MARKER As BEG_FILE_RECORD
        Private END_FILE_MARKER As END_FILE_RECORD
        Private HORIZ_PAGE_BREAK As HPAGE_BREAK_RECORD

        ''' <summary>
        ''' create an array that will hold the rows where a horizontal page
        ''' break will be inserted just before.
        ''' </summary>
        Private HorizPageBreakRows() As Short
        Private NumHorizPageBreaks As Integer

        ''' <summary>
        ''' create a new excel xls file biff writer
        ''' </summary>
        Sub New()
            ' Set up default values for records
            ' These should be the values that are the same for every record of these types

            ' beginning of file
            With BEG_FILE_MARKER
                .opcode = 9
                .length = 4
                .version = 2
                .ftype = 10
            End With

            ' end of file marker
            With END_FILE_MARKER
                .opcode = 10
            End With
        End Sub

        Public Function CreateFile(FileName As String) As Integer
            FileNumber = New BinaryWriter(FileName.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False))
            FileNumber.Write(BEG_FILE_MARKER)  'must always be written first

            Call WriteDefaultFormats()

            'create the Horizontal Page Break array
            ReDim HorizPageBreakRows(0)
            NumHorizPageBreaks = 0
            FileNumber.Flush()

            Return 0
        End Function

        Public Function CloseFile() As Integer
            If FileNumber Is Nothing Then Return 0

            'write the horizontal page breaks if necessary
            If NumHorizPageBreaks > 0 Then
                'the Horizontal Page Break array must be in sorted order.
                'Use a simple Bubble sort because the size of this array would
                'be pretty small most of the time. A QuickSort would probably
                'be overkill.
                Dim lLoop1 As Long
                Dim lLoop2 As Long
                Dim lTemp As Long
                For lLoop1 = UBound(HorizPageBreakRows) To LBound(HorizPageBreakRows) Step -1
                    For lLoop2 = LBound(HorizPageBreakRows) + 1 To lLoop1
                        If HorizPageBreakRows(lLoop2 - 1) > HorizPageBreakRows(lLoop2) Then
                            lTemp = HorizPageBreakRows(lLoop2 - 1)
                            HorizPageBreakRows(lLoop2 - 1) = HorizPageBreakRows(lLoop2)
                            HorizPageBreakRows(lLoop2) = lTemp
                        End If
                    Next lLoop2
                Next lLoop1

                'write the Horizontal Page Break Record
                With HORIZ_PAGE_BREAK
                    .opcode = 27
                    .length = 2 + (NumHorizPageBreaks * 2)
                    .NumPageBreaks = NumHorizPageBreaks
                End With
                Put(FileNumber, HORIZ_PAGE_BREAK)

                'now write the actual page break values
                'the MKI$ function is standard in other versions of BASIC but
                'VisualBasic does not have it. A KnowledgeBase article explains
                'how to recreate it (albeit using 16-bit API, I switched it
                'to 32-bit).
                Put(FileNumber, HorizPageBreakRows)
            End If

            Put(FileNumber, END_FILE_MARKER)
            FileNumber.Flush()
            FileNumber.Close()
            FileNumber.Dispose()

            ' return with no error code
            Return 0
        End Function

        Public Function InsertHorizPageBreak(lrow As Long) As Integer
            Dim Row As Integer

            'the row and column values are written to the excel file as
            'unsigned integers. Therefore, must convert the longs to integer.
            If lrow > 32767 Then
                Row% = CInt(lrow - 65536)
            Else
                Row% = CInt(lrow) - 1    'rows/cols in Excel binary file are zero based
            End If

            NumHorizPageBreaks = NumHorizPageBreaks + 1
            ReDim Preserve HorizPageBreakRows(NumHorizPageBreaks)

            HorizPageBreakRows(NumHorizPageBreaks) = Row%

            Return 0
        End Function

        Public Function WriteValue(ValueType As ValueTypes,
                               CellFontUsed As CellFont,
                               Alignment As CellAlignment,
                               HiddenLocked As CellHiddenLocked,
                               lrow As Long,
                               lcol As Long,
                               value As Object,
                               Optional CellFormat As Long = 0) As Integer

            'the row and column values are written to the excel file as
            'unsigned integers. Therefore, must convert the longs to integer.
            Dim Row As Integer
            Dim col As Integer

            If lrow > 32767 Then
                Row% = CInt(lrow - 65536)
            Else
                Row% = CInt(lrow) - 1    'rows/cols in Excel binary file are zero based
            End If

            If lcol > 32767 Then
                col% = CInt(lcol - 65536)
            Else
                col% = CInt(lcol) - 1    'rows/cols in Excel binary file are zero based
            End If


            Select Case ValueType
                Case ValueTypes.xlsinteger
                    Dim INTEGER_RECORD As tInteger
                    With INTEGER_RECORD
                        .opcode = 2
                        .length = 9
                        .Row = Row%
                        .col = col%
                        .rgbAttr1 = CByte(HiddenLocked)
                        .rgbAttr2 = CByte(CellFontUsed + CellFormat)
                        .rgbAttr3 = CByte(Alignment)
                        .intValue = CInt(value)
                    End With
                    Put(FileNumber, INTEGER_RECORD)


                Case ValueTypes.xlsnumber
                    Dim NUMBER_RECORD As tNumber

                    With NUMBER_RECORD
                        .opcode = 3
                        .length = 15
                        .Row = Row%
                        .col = col%
                        .rgbAttr1 = CByte(HiddenLocked)
                        .rgbAttr2 = CByte(CellFontUsed + CellFormat)
                        .rgbAttr3 = CByte(Alignment)

                        If TypeOf value Is Date Then
                            .NumberValue = DirectCast(value, Date).ToOADate
                        Else
                            .NumberValue = CDbl(value)
                        End If
                    End With

                    Call Put(FileNumber, NUMBER_RECORD)
                Case ValueTypes.xlsText
                    Dim b As Byte
                    Dim st$ = CStr(value)
                    Dim l% = Len(st$)

                    Dim TEXT_RECORD As tText
                    With TEXT_RECORD
                        .opcode = 4
                        .length = 10
                        'Length of the text portion of the record
                        .TextLength = l%

                        'Total length of the record
                        .length = 8 + l

                        .Row = Row%
                        .col = col%

                        .rgbAttr1 = CByte(HiddenLocked)
                        .rgbAttr2 = CByte(CellFontUsed + CellFormat)
                        .rgbAttr3 = CByte(Alignment)

                        'Put record header
                        Put(FileNumber, TEXT_RECORD)

                        'Then the actual string data
                        For a = 1 To l%
                            b = Asc(Mid$(st$, a, 1))
                            Put(FileNumber, b)
                        Next
                    End With
            End Select

            ' return with no error
            Return 0
        End Function

        Public Function SetMargin(Margin As MarginTypes, MarginValue As Double) As Integer
            ' write the spreadsheet's layout information (in inches)
            Dim MarginRecord As MARGIN_RECORD_LAYOUT

            With MarginRecord
                .opcode = Margin
                .length = 8
                .MarginValue = MarginValue 'in inches
            End With
            Put(FileNumber, MarginRecord)

            Return 0
        End Function

        Public Function SetColumnWidth(FirstColumn As Byte, LastColumn As Byte, WidthValue As Integer)
            Dim COLWIDTH As COLWIDTH_RECORD

            With COLWIDTH
                .opcode = 36
                .length = 4
                .col1 = FirstColumn - 1
                .col2 = LastColumn - 1
                .ColumnWidth = WidthValue * 256  'values are specified as 1/256 of a character
            End With
            Put(FileNumber, COLWIDTH)

            Return 0
        End Function


        Public Function SetFont(FontName As String, FontHeight As Integer, FontFormat As FontFormatting) As Integer
            'you can set up to 4 fonts in the spreadsheet file. When writing a value such
            'as a Text or Number you can specify one of the 4 fonts (numbered 0 to 3)

            Dim FONTNAME_RECORD As FONT_RECORD
            Dim l% = Len(FontName)

            With FONTNAME_RECORD
                .opcode = 49
                .length = 5 + l%
                .FontHeight = FontHeight * 20
                .FontAttributes1 = CByte(FontFormat)  'bold/underline etc...
                .FontAttributes2 = CByte(0) 'reserved-always zero!!
                .FontNameLength = CByte(Len(FontName))
            End With
            Put(FileNumber, FONTNAME_RECORD)

            'Then the actual font name data
            Dim b As Byte
            For a = 1 To l%
                b = Asc(Mid$(FontName, a, 1))
                Put(FileNumber, b)
            Next

            Return 0
        End Function

        Public Function SetHeader(HeaderText As String) As Integer
            Dim HEADER_RECORD As HEADER_FOOTER_RECORD
            Dim l% = Len(HeaderText)

            With HEADER_RECORD
                .opcode = 20
                .length = 1 + l%
                .TextLength = CByte(Len(HeaderText))
            End With
            Put(FileNumber, HEADER_RECORD)

            'Then the actual Header text
            Dim b As Byte
            For a = 1 To l%
                b = Asc(Mid$(HeaderText, a, 1))
                Put(FileNumber, b)
            Next

            Return 0
        End Function

        Public Function SetFooter(FooterText As String) As Integer
            Dim FOOTER_RECORD As HEADER_FOOTER_RECORD
            Dim l% = Len(FooterText)

            With FOOTER_RECORD
                .opcode = 21
                .length = 1 + l%
                .TextLength = CByte(Len(FooterText))
            End With
            Put(FileNumber, FOOTER_RECORD)

            'Then the actual Header text
            Dim b As Byte
            For a = 1 To l%
                b = Asc(Mid$(FooterText, a, 1))
                Put(FileNumber, b)
            Next

            Return 0
        End Function

        Public Function SetFilePassword(PasswordText As String) As Integer
            Dim FILE_PASSWORD_RECORD As PASSWORD_RECORD
            Dim l% = Len(PasswordText)

            With FILE_PASSWORD_RECORD
                .opcode = 47
                .length = l%
            End With
            Put(FileNumber, FILE_PASSWORD_RECORD)

            'Then the actual Password text
            Dim b As Byte
            For a = 1 To l%
                b = Asc(Mid$(PasswordText, a, 1))
                Put(FileNumber, b)
            Next

            Return 0
        End Function

        Public Sub SetPrintGridLines(newvalue As Boolean)
            Dim GRIDLINES_RECORD As PRINT_GRIDLINES_RECORD

            With GRIDLINES_RECORD
                .opcode = 43
                .length = 2
                If newvalue = True Then
                    .PrintFlag = 1
                Else
                    .PrintFlag = 0
                End If

            End With
            Put(FileNumber, GRIDLINES_RECORD)
        End Sub

        Public Sub SetProtectSpreadsheet(newvalue As Boolean)
            Dim PROTECT_RECORD As PROTECT_SPREADSHEET_RECORD

            With PROTECT_RECORD
                .opcode = 18
                .length = 2
                If newvalue = True Then
                    .Protect = 1
                Else
                    .Protect = 0
                End If

            End With
            Put(FileNumber, PROTECT_RECORD)
        End Sub

        Public Function WriteDefaultFormats() As Integer
            Dim cFORMAT_COUNT_RECORD As FORMAT_COUNT_RECORD
            Dim cFORMAT_RECORD As FORMAT_RECORD
            Dim lIndex As Long
            Dim aFormat(0 To 23) As String
            Dim l As Long
            Dim q As String
            q = Chr(34)

            aFormat(0) = "General"
            aFormat(1) = "0"
            aFormat(2) = "0.00"
            aFormat(3) = "#,##0"
            aFormat(4) = "#,##0.00"
            aFormat(5) = "#,##0\ " & q & "$" & q & ";\-#,##0\ " & q & "$" & q
            aFormat(6) = "#,##0\ " & q & "$" & q & ";[Red]\-#,##0\ " & q & "$" & q
            aFormat(7) = "#,##0.00\ " & q & "$" & q & ";\-#,##0.00\ " & q & "$" & q
            aFormat(8) = "#,##0.00\ " & q & "$" & q & ";[Red]\-#,##0.00\ " & q & "$" & q
            aFormat(9) = "0%"
            aFormat(10) = "0.00%"
            aFormat(11) = "0.00E+00"
            aFormat(12) = "dd/mm/yy"
            aFormat(13) = "dd/\ mmm\ yy"
            aFormat(14) = "dd/\ mmm"
            aFormat(15) = "mmm\ yy"
            aFormat(16) = "h:mm\ AM/PM"
            aFormat(17) = "h:mm:ss\ AM/PM"
            aFormat(18) = "hh:mm"
            aFormat(19) = "hh:mm:ss"
            aFormat(20) = "dd/mm/yy\ hh:mm"
            aFormat(21) = "##0.0E+0"
            aFormat(22) = "mm:ss"
            aFormat(23) = "@"

            With cFORMAT_COUNT_RECORD
                .opcode = &H1F
                .length = &H2
                .Count = CInt(UBound(aFormat))
            End With
            Put(FileNumber, cFORMAT_COUNT_RECORD)

            For lIndex = LBound(aFormat) To UBound(aFormat)
                l = Len(aFormat(lIndex))
                With cFORMAT_RECORD
                    .opcode = &H1E
                    .length = CInt(l + 1)
                    .FormatLenght = CInt(l)
                End With
                Put(FileNumber, cFORMAT_RECORD)

                'Then the actual format
                Dim b As Byte, a As Integer
                For a = 1 To l
                    b = Asc(Mid$(aFormat(lIndex), a, 1))
                    Put(FileNumber, b)
                Next
            Next lIndex

            Return 0
        End Function

        Public Function SetDefaultRowHeight(HeightValue As Integer)
            'Height is defined in units of 1/20th of a point. Therefore, a 10-point font
            'would be 200 (i.e. 200/20 = 10). This function takes a HeightValue such as
            '14 point and converts it the correct size before writing it to the file.

            Dim DEFHEIGHT As DEF_ROWHEIGHT_RECORD

            With DEFHEIGHT
                .opcode = 37
                .length = 2
                .RowHeight = HeightValue * 20  'convert points to 1/20ths of point
            End With
            Put(FileNumber, DEFHEIGHT)

            Return 0
        End Function

        Public Function SetRowHeight(lrow As Long, HeightValue As Integer)
            'the row and column values are written to the excel file as
            'unsigned integers. Therefore, must convert the longs to integer.
            Dim Row As Integer

            If lrow > 32767 Then
                Row% = CInt(lrow - 65536)
            Else
                Row% = CInt(lrow) - 1    'rows/cols in Excel binary file are zero based
            End If


            'Height is defined in units of 1/20th of a point. Therefore, a 10-point font
            'would be 200 (i.e. 200/20 = 10). This function takes a HeightValue such as
            '14 point and converts it the correct size before writing it to the file.

            Dim ROWHEIGHTREC As ROW_HEIGHT_RECORD

            With ROWHEIGHTREC
                .opcode = 8
                .length = 16
                .RowNumber = Row%
                .FirstColumn = 0
                .LastColumn = 256
                .RowHeight = HeightValue * 20 'convert points to 1/20ths of point
                .internal = 0
                .DefaultAttributes = 0
                .FileOffset = 0
                .rgbAttr1 = 0
                .rgbAttr2 = 0
                .rgbAttr3 = 0
            End With
            Put(FileNumber, ROWHEIGHTREC)

            Return 0
        End Function
    End Class
End Namespace
