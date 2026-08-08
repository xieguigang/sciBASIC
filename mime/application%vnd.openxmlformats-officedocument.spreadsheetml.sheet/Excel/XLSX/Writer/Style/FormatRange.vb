#Region "Microsoft.VisualBasic::22d2676138944d3c1e46ff55351cdde9, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Style\FormatRange.vb"

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

    '   Total Lines: 25
    '    Code Lines: 8 (32.00%)
    ' Comment Lines: 15 (60.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (8.00%)
    '     File Size: 866 B


    '     Enum FormatRange
    ' 
    '         custom_format, defined_format, invalid, undefined
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
    ''' Range or validity of the format number
    ''' </summary>
    Public Enum FormatRange
        ''' <summary>
        ''' Format from 0 to 164 (with gaps)
        ''' </summary>
        defined_format
        ''' <summary>
        ''' Custom defined formats from 164 and higher. Although 164 is already custom, it is still defined as enum value
        ''' </summary>
        custom_format
        ''' <summary>
        ''' Probably invalid format numbers (e.g. negative value)
        ''' </summary>
        invalid
        ''' <summary>
        ''' Values between 0 and 164 that are not defined as enum value. This may be caused by changes of the OOXML specifications or Excel versions that have encoded loaded files
        ''' </summary>
        undefined
    End Enum
End Namespace
