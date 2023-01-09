Imports System.IO
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

    'the memory copy API is used in the MKI$ function which converts an integer
    'value to a 2-byte string value to write to the file. (used by the Horizontal
    'Page Break function).
    Private Declare Sub CopyMemory Lib "KERNEL32" Alias "RtlMoveMemory" (lpvDest As Object, lpvSource As Object, ByVal cbCopy As Long)

    'enum to handle the various types of values that can be written
    'to the excel file.
    Public Enum ValueTypes
        xlsinteger = 0
        xlsnumber = 1
        xlsText = 2
    End Enum

    'enum to hold cell alignment
    Public Enum CellAlignment
        xlsGeneralAlign = 0
        xlsLeftAlign = 1
        xlsCentreAlign = 2
        xlsrightAlign = 3
        xlsFillCell = 4
        xlsLeftBorder = 8
        xlsRightBorder = 16
        xlsTopBorder = 32
        xlsBottomBorder = 64
        xlsShaded = 128
    End Enum

    'enum to handle selecting the font for the cell
    Public Enum CellFont
        'used by rgbAttr2
        'bits 0-5 handle the *picture* formatting, not bold/underline etc...
        'bits 6-7 handle the font number
        xlsFont0 = 0
        xlsFont1 = 64
        xlsFont2 = 128
        xlsFont3 = 192
    End Enum

    Public Enum CellHiddenLocked
        'used by rgbAttr1
        'bits 0-5 must be zero
        'bit 6 locked/unlocked
        'bit 7 hidden/not hidden
        xlsNormal = 0
        xlsLocked = 64
        xlsHidden = 128
    End Enum


    'set up variables to hold the spreadsheet's layout
    Public Enum MarginTypes
        xlsLeftMargin = 38
        xlsRightMargin = 39
        xlsTopMargin = 40
        xlsBottomMargin = 41
    End Enum


    Public Enum FontFormatting
        'add these enums together. For example: xlsBold + xlsUnderline
        xlsNoFormat = 0
        xlsBold = 1
        xlsItalic = 2
        xlsUnderline = 4
        xlsStrikeout = 8
    End Enum

    Private Structure FONT_RECORD
        Dim opcode As Integer  '49
        Dim length As Integer  '5+len(fontname)
        Dim FontHeight As Integer

        'bit0 bold, bit1 italic, bit2 underline, bit3 strikeout, bit4-7 reserved
        Dim FontAttributes1 As Byte

        Dim FontAttributes2 As Byte  'reserved - always 0

        Dim FontNameLength As Byte
    End Structure


    Private Structure PASSWORD_RECORD
        Dim opcode As Integer  '47
        Dim length As Integer  'len(password)
    End Structure


    Private Structure HEADER_FOOTER_RECORD
        Dim opcode As Integer  '20 Header, 21 Footer
        Dim length As Integer  '1+len(text)
        Dim TextLength As Byte
    End Structure


    Private Structure PROTECT_SPREADSHEET_RECORD
        Dim opcode As Integer  '18
        Dim length As Integer  '2
        Dim Protect As Integer
    End Structure

    Private Structure FORMAT_COUNT_RECORD
        Dim opcode As Integer  '1f
        Dim length As Integer '2
        Dim Count As Integer
    End Structure

    Private Structure FORMAT_RECORD
        Dim opcode As Integer  '1e
        Dim length As Integer  '1+len(format)
        Dim FormatLenght As Byte 'len(format)
    End Structure '+ followed by the Format-Picture



    Private Structure COLWIDTH_RECORD
        Dim opcode As Integer  '36
        Dim length As Integer  '4
        Dim col1 As Byte       'first column
        Dim col2 As Byte       'last column
        Dim ColumnWidth As Integer   'at 1/256th of a character
    End Structure

    'Beginning Of File record
    Private Structure BEG_FILE_RECORD
        Dim opcode As Integer
        Dim length As Integer
        Dim version As Integer
        Dim ftype As Integer
    End Structure

    'End Of File record
    Private Structure END_FILE_RECORD
        Dim opcode As Integer
        Dim length As Integer
    End Structure

    'true/false to print gridlines
    Private Structure PRINT_GRIDLINES_RECORD
        Dim opcode As Integer
        Dim length As Integer
        Dim PrintFlag As Integer
    End Structure

    'Integer record
    Private Structure tInteger
        Dim opcode As Integer
        Dim length As Integer
        Dim Row As Integer     'unsigned integer
        Dim col As Integer

        'rgbAttr1 handles whether cell is hidden and/or locked
        Dim rgbAttr1 As Byte

        'rgbAttr2 handles the Font# and Formatting assigned to this cell
        Dim rgbAttr2 As Byte

        'rgbAttr3 handles the Cell Alignment/borders/shading
        Dim rgbAttr3 As Byte

        Dim intValue As Integer  'the actual integer value
    End Structure

    'Number record
    Private Structure tNumber
        Dim opcode As Integer
        Dim length As Integer
        Dim Row As Integer
        Dim col As Integer
        Dim rgbAttr1 As Byte
        Dim rgbAttr2 As Byte
        Dim rgbAttr3 As Byte
        Dim NumberValue As Double  '8 Bytes
    End Structure

    'Label (Text) record
    Private Structure tText
        Dim opcode As Integer
        Dim length As Integer
        Dim Row As Integer
        Dim col As Integer
        Dim rgbAttr1 As Byte
        Dim rgbAttr2 As Byte
        Dim rgbAttr3 As Byte
        Dim TextLength As Byte
    End Structure

    Private Structure MARGIN_RECORD_LAYOUT
        Dim opcode As Integer
        Dim length As Integer
        Dim MarginValue As Double  '8 bytes
    End Structure

    Private Structure HPAGE_BREAK_RECORD
        Dim opcode As Integer
        Dim length As Integer
        Dim NumPageBreaks As Integer
    End Structure

    Private Structure DEF_ROWHEIGHT_RECORD
        Dim opcode As Integer
        Dim length As Integer
        Dim RowHeight As Integer
    End Structure

    Private Structure ROW_HEIGHT_RECORD
        Dim opcode As Integer  '08
        Dim length As Integer  'should always be 16 bytes
        Dim RowNumber As Integer
        Dim FirstColumn As Integer
        Dim LastColumn As Integer
        Dim RowHeight As Integer  'written to file as 1/20ths of a point
        Dim internal As Integer
        Dim DefaultAttributes As Byte  'set to zero for no default attributes
        Dim FileOffset As Integer
        Dim rgbAttr1 As Byte
        Dim rgbAttr2 As Byte
        Dim rgbAttr3 As Byte
    End Structure

    Private FileNumber As BinaryWriter
    Private BEG_FILE_MARKER As BEG_FILE_RECORD
    Private END_FILE_MARKER As END_FILE_RECORD
    Private HORIZ_PAGE_BREAK As HPAGE_BREAK_RECORD

    'create an array that will hold the rows where a horizontal page
    'break will be inserted just before.
    Private HorizPageBreakRows() As Integer
    Private NumHorizPageBreaks As Integer

    Public Function CreateFile(ByVal FileName As String) As Integer
        FileNumber = New BinaryWriter(FileName.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False))
        FileNumber.Write(BEG_FILE_MARKER)  'must always be written first

        Call WriteDefaultFormats

        'create the Horizontal Page Break array
        ReDim HorizPageBreakRows(0)
        NumHorizPageBreaks = 0

        FileNumber.Flush()
        FileNumber.Close()
        FileNumber.Dispose()
    End Function

    Public Function CloseFile() As Integer

        On Error GoTo Write_Error

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
            Put #FileNumber, , HORIZ_PAGE_BREAK

        'now write the actual page break values
            'the MKI$ function is standard in other versions of BASIC but
            'VisualBasic does not have it. A KnowledgeBase article explains
            'how to recreate it (albeit using 16-bit API, I switched it
            'to 32-bit).
            For x% = 1 To UBound(HorizPageBreakRows)
                Put #FileNumber, , MKI$(HorizPageBreakRows(x%))
        Next
        End If

        Put #FileNumber, , END_FILE_MARKER
    Close #FileNumber

    CloseFile = 0  'return with no error code

        Exit Function

Write_Error:
        CloseFile = Err.Number
        Exit Function

    End Function


    Sub New()

        'Set up default values for records
        'These should be the values that are the same for every record of these types

        With BEG_FILE_MARKER  'beginning of file
            .opcode = 9
            .length = 4
            .version = 2
            .ftype = 10
        End With

        With END_FILE_MARKER  'end of file marker
            .opcode = 10
        End With


    End Sub


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

        Exit Function


Page_Break_Error:
        InsertHorizPageBreak = Err.Number
        Exit Function


    End Function



    Public Function WriteValue(ValueType As ValueTypes, CellFontUsed As CellFont, Alignment As CellAlignment, HiddenLocked As CellHiddenLocked, lrow As Long, lcol As Long, value As Variant, Optional CellFormat As Long = 0) As Integer

        On Error GoTo Write_Error

        'the row and column values are written to the excel file as
        'unsigned integers. Therefore, must convert the longs to integer.

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
                Put #FileNumber, , INTEGER_RECORD


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
                    .NumberValue = CDbl(value)
                End With
                Put #FileNumber, , NUMBER_RECORD


      Case ValueTypes.xlsText
                Dim b As Byte
                st$ = CStr(value)
                l% = Len(st$)

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
                    Put #FileNumber, , TEXT_RECORD

          'Then the actual string data
                    For a = 1 To l%
                        b = Asc(Mid$(st$, a, 1))
                        Put #FileNumber, , b
          Next
                End With

        End Select

        WriteValue = 0   'return with no error

        Exit Function

Write_Error:
        WriteValue = Err.Number
        Exit Function

    End Function


    Public Function SetMargin(Margin As MarginTypes, MarginValue As Double) As Integer

        On Error GoTo Write_Error

        'write the spreadsheet's layout information (in inches)
        Dim MarginRecord As MARGIN_RECORD_LAYOUT

        With MarginRecord
            .opcode = Margin
            .length = 8
            .MarginValue = MarginValue 'in inches
        End With
        Put #FileNumber, , MarginRecord

    SetMargin = 0

        Exit Function

Write_Error:
        SetMargin = Err.Number
        Exit Function

    End Function


    Public Function SetColumnWidth(FirstColumn As Byte, LastColumn As Byte, WidthValue As Integer)

        On Error GoTo Write_Error

        Dim COLWIDTH As COLWIDTH_RECORD

        With COLWIDTH
            .opcode = 36
            .length = 4
            .col1 = FirstColumn - 1
            .col2 = LastColumn - 1
            .ColumnWidth = WidthValue * 256  'values are specified as 1/256 of a character
        End With
        Put #FileNumber, , COLWIDTH

    SetColumnWidth = 0

        Exit Function

Write_Error:
        SetColumnWidth = Err.Number
        Exit Function

    End Function


    Public Function SetFont(FontName As String, FontHeight As Integer, FontFormat As FontFormatting) As Integer

        On Error GoTo Write_Error

        'you can set up to 4 fonts in the spreadsheet file. When writing a value such
        'as a Text or Number you can specify one of the 4 fonts (numbered 0 to 3)

        Dim FONTNAME_RECORD As FONT_RECORD

        l% = Len(FontName)

        With FONTNAME_RECORD
            .opcode = 49
            .length = 5 + l%
            .FontHeight = FontHeight * 20
            .FontAttributes1 = CByte(FontFormat)  'bold/underline etc...
            .FontAttributes2 = CByte(0) 'reserved-always zero!!
            .FontNameLength = CByte(Len(FontName))
        End With
        Put #FileNumber, , FONTNAME_RECORD

    'Then the actual font name data
        Dim b As Byte
        For a = 1 To l%
            b = Asc(Mid$(FontName, a, 1))
            Put #FileNumber, , b
    Next

        SetFont = 0

        Exit Function

Write_Error:
        SetFont = Err.Number
        Exit Function


    End Function


    Public Function SetHeader(HeaderText As String) As Integer

        On Error GoTo Write_Error

        Dim HEADER_RECORD As HEADER_FOOTER_RECORD

        l% = Len(HeaderText)

        With HEADER_RECORD
            .opcode = 20
            .length = 1 + l%
            .TextLength = CByte(Len(HeaderText))
        End With
        Put #FileNumber, , HEADER_RECORD

    'Then the actual Header text
        Dim b As Byte
        For a = 1 To l%
            b = Asc(Mid$(HeaderText, a, 1))
            Put #FileNumber, , b
    Next

        SetHeader = 0

        Exit Function

Write_Error:
        SetHeader = Err.Number
        Exit Function

    End Function



    Public Function SetFooter(FooterText As String) As Integer

        On Error GoTo Write_Error

        Dim FOOTER_RECORD As HEADER_FOOTER_RECORD

        l% = Len(FooterText)

        With FOOTER_RECORD
            .opcode = 21
            .length = 1 + l%
            .TextLength = CByte(Len(FooterText))
        End With
        Put #FileNumber, , FOOTER_RECORD

    'Then the actual Header text
        Dim b As Byte
        For a = 1 To l%
            b = Asc(Mid$(FooterText, a, 1))
            Put #FileNumber, , b
    Next

        SetFooter = 0

        Exit Function

Write_Error:
        SetFooter = Err.Number
        Exit Function

    End Function



    Public Function SetFilePassword(PasswordText As String) As Integer

        On Error GoTo Write_Error

        Dim FILE_PASSWORD_RECORD As PASSWORD_RECORD

        l% = Len(PasswordText)

        With FILE_PASSWORD_RECORD
            .opcode = 47
            .length = l%
        End With
        Put #FileNumber, , FILE_PASSWORD_RECORD

    'Then the actual Password text
        Dim b As Byte
        For a = 1 To l%
            b = Asc(Mid$(PasswordText, a, 1))
            Put #FileNumber, , b
    Next

        SetFilePassword = 0

        Exit Function

Write_Error:
        SetFilePassword = Err.Number
        Exit Function

    End Function




    Public Property Let PrintGridLines(ByVal newvalue As Boolean)

    On Error GoTo Write_Error

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
    Put #FileNumber, , GRIDLINES_RECORD

Exit Property

Write_Error:
    Exit Property


    End Property




    Public Property Let ProtectSpreadsheet(ByVal newvalue As Boolean)

    On Error GoTo Write_Error

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
    Put #FileNumber, , PROTECT_RECORD

Exit Property

Write_Error:
    Exit Property


    End Property


    Public Function WriteDefaultFormats() As Integer

        Dim cFORMAT_COUNT_RECORD As FORMAT_COUNT_RECORD
        Dim cFORMAT_RECORD As FORMAT_RECORD
        Dim lIndex As Long
        Dim aFormat(0 To 23) As String
        Dim l As Long
        Dim q As String
        q = Chr$(34)

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
        Put #FileNumber, , cFORMAT_COUNT_RECORD

    For lIndex = LBound(aFormat) To UBound(aFormat)
            l = Len(aFormat(lIndex))
            With cFORMAT_RECORD
                .opcode = &H1E
                .length = CInt(l + 1)
                .FormatLenght = CInt(l)
            End With
            Put #FileNumber, , cFORMAT_RECORD

        'Then the actual format
            Dim b As Byte, a As Long
            For a = 1 To l
                b = Asc(Mid$(aFormat(lIndex), a, 1))
                Put #FileNumber, , b
        Next
        Next lIndex

        Exit Function

    End Function


    Function MKI$(x As Integer)
        'used for writing integer array values to the disk file
        temp$ = Space$(2)
        CopyMemory ByVal temp$, x%, 2
   MKI$ = temp$
    End Function


    Public Function SetDefaultRowHeight(HeightValue As Integer)

        On Error GoTo Write_Error

        'Height is defined in units of 1/20th of a point. Therefore, a 10-point font
        'would be 200 (i.e. 200/20 = 10). This function takes a HeightValue such as
        '14 point and converts it the correct size before writing it to the file.

        Dim DEFHEIGHT As DEF_ROWHEIGHT_RECORD

        With DEFHEIGHT
            .opcode = 37
            .length = 2
            .RowHeight = HeightValue * 20  'convert points to 1/20ths of point
        End With
        Put #FileNumber, , DEFHEIGHT

    SetDefaultRowHeight = 0

        Exit Function

Write_Error:
        SetDefaultRowHeight = Err.Number
        Exit Function

    End Function


    Public Function SetRowHeight(lrow As Long, HeightValue As Integer)

        On Error GoTo Write_Error

        'the row and column values are written to the excel file as
        'unsigned integers. Therefore, must convert the longs to integer.

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
        Put #FileNumber, , ROWHEIGHTREC

    SetRowHeight = 0

        Exit Function

Write_Error:
        SetRowHeight = Err.Number
        Exit Function

    End Function


End Class