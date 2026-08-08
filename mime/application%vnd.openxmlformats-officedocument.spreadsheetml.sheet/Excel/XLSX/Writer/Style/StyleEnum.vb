#Region "Microsoft.VisualBasic::f882bb433e745fce3d2b131594a27ad6, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Style\StyleEnum.vb"

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

    '   Total Lines: 34
    '    Code Lines: 17 (50.00%)
    ' Comment Lines: 16 (47.06%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (2.94%)
    '     File Size: 1.27 KB


    '     Enum StyleEnum
    ' 
    '         bold, boldItalic, borderFrame, borderFrameHeader, dateFormat
    '         dottedFill_0_125, doubleUnderline, italic, mergeCellStyle, roundFormat
    '         strike, timeFormat, underline
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace XLSX.Writer.Styling

    ''' <summary>
    ''' Enum with style selection
    ''' </summary>
    Friend Enum StyleEnum
        ''' <summary>Format text bold</summary>
        bold
        ''' <summary>Format text italic</summary>
        italic
        ''' <summary>Format text bold and italic</summary>
        boldItalic
        ''' <summary>Format text with an underline</summary>
        underline
        ''' <summary>Format text with a double underline</summary>
        doubleUnderline
        ''' <summary>Format text with a strike-through</summary>
        strike
        ''' <summary>Format number as date</summary>
        dateFormat
        ''' <summary>Format number as time</summary>
        timeFormat
        ''' <summary>Rounds number as an integer</summary>
        roundFormat
        ''' <summary>Format cell with a thin border</summary>
        borderFrame
        ''' <summary>Format cell with a thin border and a thick bottom line as header cell</summary>
        borderFrameHeader
        ''' <summary>Special pattern fill style for compatibility purpose </summary>
        dottedFill_0_125
        ''' <summary>Style to apply on merged cells </summary>
        mergeCellStyle
    End Enum
End Namespace
