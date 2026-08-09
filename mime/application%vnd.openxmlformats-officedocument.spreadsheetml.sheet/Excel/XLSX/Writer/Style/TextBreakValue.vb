#Region "Microsoft.VisualBasic::7c4b68ce06b6b6f680c67a454bdcd1a4, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Style\TextBreakValue.vb"

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

    '   Total Lines: 24
    '    Code Lines: 11 (45.83%)
    ' Comment Lines: 11 (45.83%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (8.33%)
    '     File Size: 715 B


    '     Enum TextBreakValue
    ' 
    '         none, shrinkToFit, wrapText
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum TextDirectionValue
    ' 
    '         horizontal, vertical
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
    ''' Enum for text break options
    ''' </summary>
    Public Enum TextBreakValue
        ''' <summary>Word wrap is active</summary>
        wrapText
        ''' <summary>Text will be resized to fit the cell</summary>
        shrinkToFit
        ''' <summary>Text will overflow in cell</summary>
        none
    End Enum

    ''' <summary>
    ''' Enum for the general text alignment direction
    ''' </summary>
    Public Enum TextDirectionValue
        ''' <summary>Text direction is horizontal (default)</summary>
        horizontal
        ''' <summary>Text direction is vertical</summary>
        vertical
    End Enum
End Namespace
