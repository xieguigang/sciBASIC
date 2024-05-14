#Region "Microsoft.VisualBasic::a2dc48651c58d8a5bb99ec999ec606ea, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLS\BIFF\SheetObject.vb"

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

    '   Total Lines: 171
    '    Code Lines: 115
    ' Comment Lines: 31
    '   Blank Lines: 25
    '     File Size: 4.81 KB


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

Namespace XLS.BIFF

    Public Structure FONT_RECORD
        Dim opcode As Short   '49
        Dim length As Short   '5+len(fontname)
        Dim FontHeight As Short

        'bit0 bold, bit1 italic, bit2 underline, bit3 strikeout, bit4-7 reserved
        Dim FontAttributes1 As Byte

        Dim FontAttributes2 As Byte  'reserved - always 0

        Dim FontNameLength As Byte
    End Structure


    Public Structure PASSWORD_RECORD
        Dim opcode As Short   '47
        Dim length As Short   'len(password)
    End Structure


    Public Structure HEADER_FOOTER_RECORD
        Dim opcode As Short   '20 Header, 21 Footer
        Dim length As Short   '1+len(text)
        Dim TextLength As Byte
    End Structure


    Public Structure PROTECT_SPREADSHEET_RECORD
        Dim opcode As Short   '18
        Dim length As Short   '2
        Dim Protect As Short
    End Structure

    Public Structure FORMAT_COUNT_RECORD
        Dim opcode As Short   '1f
        Dim length As Short  '2
        Dim Count As Short
    End Structure

    Public Structure FORMAT_RECORD
        Dim opcode As Short  '1e
        Dim length As Short   '1+len(format)
        Dim FormatLenght As Byte 'len(format)
    End Structure '+ followed by the Format-Picture

    Public Structure COLWIDTH_RECORD
        Dim opcode As Short   '36
        Dim length As Short   '4
        Dim col1 As Byte       'first column
        Dim col2 As Byte       'last column
        Dim ColumnWidth As Short    'at 1/256th of a character
    End Structure

    ''' <summary>
    ''' Beginning Of File record
    ''' </summary>
    Public Structure BEG_FILE_RECORD
        Dim opcode As Short
        Dim length As Short
        Dim version As Short
        Dim ftype As Short
    End Structure

    ''' <summary>
    ''' End Of File record
    ''' </summary>
    Public Structure END_FILE_RECORD
        Dim opcode As Short
        Dim length As Short
    End Structure

    ''' <summary>
    ''' true/false to print gridlines
    ''' </summary>
    Public Structure PRINT_GRIDLINES_RECORD
        Dim opcode As Short
        Dim length As Short
        Dim PrintFlag As Short
    End Structure

    ''' <summary>
    ''' Integer record
    ''' </summary>
    Public Structure tInteger

        Dim opcode As Short
        Dim length As Short
        Dim Row As Short      'unsigned integer
        Dim col As Short

        ''' <summary>
        ''' rgbAttr1 handles whether cell is hidden and/or locked
        ''' </summary>
        Dim rgbAttr1 As Byte
        ''' <summary>
        ''' rgbAttr2 handles the Font# and Formatting assigned to this cell
        ''' </summary>
        Dim rgbAttr2 As Byte
        ''' <summary>
        ''' rgbAttr3 handles the Cell Alignment/borders/shading
        ''' </summary>
        Dim rgbAttr3 As Byte
        ''' <summary>
        ''' the actual integer value
        ''' </summary>
        Dim intValue As Short
    End Structure

    ''' <summary>
    ''' Number record
    ''' </summary>
    Public Structure tNumber
        Dim opcode As Short
        Dim length As Short
        Dim Row As Short
        Dim col As Short
        Dim rgbAttr1 As Byte
        Dim rgbAttr2 As Byte
        Dim rgbAttr3 As Byte
        Dim NumberValue As Double  '8 Bytes
    End Structure

    ''' <summary>
    ''' Label (Text) record
    ''' </summary>
    Public Structure tText
        Dim opcode As Short
        Dim length As Short
        Dim Row As Short
        Dim col As Short
        Dim rgbAttr1 As Byte
        Dim rgbAttr2 As Byte
        Dim rgbAttr3 As Byte
        Dim TextLength As Byte
    End Structure

    Public Structure MARGIN_RECORD_LAYOUT
        Dim opcode As Short
        Dim length As Short
        Dim MarginValue As Double  '8 bytes
    End Structure

    Public Structure HPAGE_BREAK_RECORD
        Dim opcode As Short
        Dim length As Short
        Dim NumPageBreaks As Short
    End Structure

    Public Structure DEF_ROWHEIGHT_RECORD
        Dim opcode As Short
        Dim length As Short
        Dim RowHeight As Short
    End Structure

    Public Structure ROW_HEIGHT_RECORD
        Dim opcode As Short  '08
        Dim length As Short   'should always be 16 bytes
        Dim RowNumber As Short
        Dim FirstColumn As Short
        Dim LastColumn As Short
        Dim RowHeight As Short 'written to file as 1/20ths of a point
        Dim internal As Short
        Dim DefaultAttributes As Byte  'set to zero for no default attributes
        Dim FileOffset As Short
        Dim rgbAttr1 As Byte
        Dim rgbAttr2 As Byte
        Dim rgbAttr3 As Byte
    End Structure
End Namespace
