Namespace BIFF

    Public Structure FONT_RECORD
        Dim opcode As Integer  '49
        Dim length As Integer  '5+len(fontname)
        Dim FontHeight As Integer

        'bit0 bold, bit1 italic, bit2 underline, bit3 strikeout, bit4-7 reserved
        Dim FontAttributes1 As Byte

        Dim FontAttributes2 As Byte  'reserved - always 0

        Dim FontNameLength As Byte
    End Structure


    Public Structure PASSWORD_RECORD
        Dim opcode As Integer  '47
        Dim length As Integer  'len(password)
    End Structure


    Public Structure HEADER_FOOTER_RECORD
        Dim opcode As Integer  '20 Header, 21 Footer
        Dim length As Integer  '1+len(text)
        Dim TextLength As Byte
    End Structure


    Public Structure PROTECT_SPREADSHEET_RECORD
        Dim opcode As Integer  '18
        Dim length As Integer  '2
        Dim Protect As Integer
    End Structure

    Public Structure FORMAT_COUNT_RECORD
        Dim opcode As Integer  '1f
        Dim length As Integer '2
        Dim Count As Integer
    End Structure

    Public Structure FORMAT_RECORD
        Dim opcode As Integer  '1e
        Dim length As Integer  '1+len(format)
        Dim FormatLenght As Byte 'len(format)
    End Structure '+ followed by the Format-Picture



    Public Structure COLWIDTH_RECORD
        Dim opcode As Integer  '36
        Dim length As Integer  '4
        Dim col1 As Byte       'first column
        Dim col2 As Byte       'last column
        Dim ColumnWidth As Integer   'at 1/256th of a character
    End Structure

    'Beginning Of File record
    Public Structure BEG_FILE_RECORD
        Dim opcode As Integer
        Dim length As Integer
        Dim version As Integer
        Dim ftype As Integer
    End Structure

    'End Of File record
    Public Structure END_FILE_RECORD
        Dim opcode As Integer
        Dim length As Integer
    End Structure

    'true/false to print gridlines
    Public Structure PRINT_GRIDLINES_RECORD
        Dim opcode As Integer
        Dim length As Integer
        Dim PrintFlag As Integer
    End Structure

    'Integer record
    Public Structure tInteger
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
    Public Structure tNumber
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
    Public Structure tText
        Dim opcode As Integer
        Dim length As Integer
        Dim Row As Integer
        Dim col As Integer
        Dim rgbAttr1 As Byte
        Dim rgbAttr2 As Byte
        Dim rgbAttr3 As Byte
        Dim TextLength As Byte
    End Structure

    Public Structure MARGIN_RECORD_LAYOUT
        Dim opcode As Integer
        Dim length As Integer
        Dim MarginValue As Double  '8 bytes
    End Structure

    Public Structure HPAGE_BREAK_RECORD
        Dim opcode As Integer
        Dim length As Integer
        Dim NumPageBreaks As Integer
    End Structure

    Public Structure DEF_ROWHEIGHT_RECORD
        Dim opcode As Integer
        Dim length As Integer
        Dim RowHeight As Integer
    End Structure

    Public Structure ROW_HEIGHT_RECORD
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
End Namespace