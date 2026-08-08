#Region "Microsoft.VisualBasic::b0b847dec671906c327ae2b640c2b03a, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Style\VerticalAlignValue.vb"

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

    '   Total Lines: 20
    '    Code Lines: 10 (50.00%)
    ' Comment Lines: 9 (45.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (5.00%)
    '     File Size: 693 B


    '     Enum VerticalAlignValue
    ' 
    '         bottom, center, distributed, justify, none
    '         top
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
    ''' Enum for the vertical alignment of a cell 
    ''' </summary>
    Public Enum VerticalAlignValue
        ''' <summary>Content will be aligned on the bottom (default)</summary>
        bottom
        ''' <summary>Content will be aligned on the top</summary>
        top
        ''' <summary>Content will be aligned in the center</summary>
        center
        ''' <summary>justify alignment</summary>
        justify
        ''' <summary>Distributed alignment</summary>
        distributed
        ''' <summary>No alignment. The alignment will not be used in a style</summary>
        none
    End Enum
End Namespace
