#Region "Microsoft.VisualBasic::dd6b6ce85a2a901ddef2cd22610613a7, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Style\HorizontalAlignValue.vb"

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

    '   Total Lines: 27
    '    Code Lines: 13 (48.15%)
    ' Comment Lines: 12 (44.44%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (7.41%)
    '     File Size: 904 B


    '     Enum HorizontalAlignValue
    ' 
    '         center, centerContinuous, distributed, fill, general
    '         justify, left, none, right
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
    ''' Enum for the horizontal alignment of a cell 
    ''' </summary>
    Public Enum HorizontalAlignValue
        ''' <summary>Content will be aligned left</summary>
        left
        ''' <summary>Content will be aligned in the center</summary>
        center
        ''' <summary>Content will be aligned right</summary>
        right
        ''' <summary>Content will fill up the cell</summary>
        fill
        ''' <summary>justify alignment</summary>
        justify
        ''' <summary>General alignment</summary>
        general
        ''' <summary>Center continuous alignment</summary>
        centerContinuous
        ''' <summary>Distributed alignment</summary>
        distributed
        ''' <summary>No alignment. The alignment will not be used in a style</summary>
        none
    End Enum

End Namespace
