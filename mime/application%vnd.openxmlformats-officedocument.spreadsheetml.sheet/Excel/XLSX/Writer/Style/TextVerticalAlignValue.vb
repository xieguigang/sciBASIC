#Region "Microsoft.VisualBasic::d33351e6687003ee6d55c48ab8603002, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Style\TextVerticalAlignValue.vb"

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

    '   Total Lines: 15
    '    Code Lines: 7 (46.67%)
    ' Comment Lines: 7 (46.67%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 1 (6.67%)
    '     File Size: 506 B


    '     Enum TextVerticalAlignValue
    ' 
    '         none, subscript, superscript
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
    ''' Enum for the vertical alignment of the text from base line
    ''' </summary>
    Public Enum TextVerticalAlignValue
        ' baseline, // Maybe not used in Excel
        ''' <summary>Text will be rendered as subscript</summary>
        subscript
        ''' <summary>Text will be rendered as superscript</summary>
        superscript
        ''' <summary>Text will be rendered normal</summary>
        none
    End Enum
End Namespace
