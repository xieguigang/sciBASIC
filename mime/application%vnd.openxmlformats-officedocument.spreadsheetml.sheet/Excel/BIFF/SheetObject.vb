#Region "Microsoft.VisualBasic::4b003c3e983d1888193391182f91617e, sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\BIFF\SheetObject.vb"

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

    '   Total Lines: 154
    '    Code Lines: 115
    ' Comment Lines: 10
    '   Blank Lines: 29
    '     File Size: 4.45 KB


    '     Structure FONT_RECORD
    ' 
    ' 
    ' 
    '     Structure PASSWORD_RECORD
    ' 
    ' 
    ' 
    '     Structure HEADER_FOOTER_RECORD
    ' 
    ' 
    ' 
    '     Structure PROTECT_SPREADSHEET_RECORD
    ' 
    ' 
    ' 
    '     Structure FORMAT_COUNT_RECORD
    ' 
    ' 
    ' 
    '     Structure FORMAT_RECORD
    ' 
    ' 
    ' 
    '     Structure COLWIDTH_RECORD
    ' 
    ' 
    ' 
    '     Structure BEG_FILE_RECORD
    ' 
    ' 
    ' 
    '     Structure END_FILE_RECORD
    ' 
    ' 
    ' 
    '     Structure PRINT_GRIDLINES_RECORD
    ' 
    ' 
    ' 
    '     Structure tInteger
    ' 
    ' 
    ' 
    '     Structure tNumber
    ' 
    ' 
    ' 
    '     Structure tText
    ' 
    ' 
    ' 
    '     Structure MARGIN_RECORD_LAYOUT
    ' 
    ' 
    ' 
    '     Structure HPAGE_BREAK_RECORD
    ' 
    ' 
    ' 
    '     Structure DEF_ROWHEIGHT_RECORD
    ' 
    ' 
    ' 
    '     Structure ROW_HEIGHT_RECORD
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
