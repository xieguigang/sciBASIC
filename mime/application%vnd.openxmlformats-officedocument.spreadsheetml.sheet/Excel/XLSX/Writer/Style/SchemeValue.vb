#Region "Microsoft.VisualBasic::ab338eab08f9014443f05f53d600847d, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Style\SchemeValue.vb"

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

    '   Total Lines: 30
    '    Code Lines: 14 (46.67%)
    ' Comment Lines: 14 (46.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (6.67%)
    '     File Size: 992 B


    '     Enum SchemeValue
    ' 
    '         major, minor, none
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum UnderlineValue
    ' 
    '         doubleAccounting, none, singleAccounting, u_double, u_single
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
    ''' Enum for the font scheme
    ''' </summary>
    Public Enum SchemeValue
        ''' <summary>Font scheme is major</summary>
        major
        ''' <summary>Font scheme is minor (default)</summary>
        minor
        ''' <summary>No Font scheme is used</summary>
        none
    End Enum

    ''' <summary>
    ''' Enum for the style of the underline property of a stylized text
    ''' </summary>
    Public Enum UnderlineValue
        ''' <summary>Text contains a single underline</summary>
        u_single
        ''' <summary>Text contains a double underline</summary>
        u_double
        ''' <summary>Text contains a single, accounting underline</summary>
        singleAccounting
        ''' <summary>Text contains a double, accounting underline</summary>
        doubleAccounting
        ''' <summary>Text contains no underline (default)</summary>
        none
    End Enum
End Namespace
