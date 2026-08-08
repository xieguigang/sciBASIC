#Region "Microsoft.VisualBasic::8b3b6d4219ed4fbe5e79e44a462a1e50, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Style\StyleValue.vb"

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

    '   Total Lines: 36
    '    Code Lines: 18 (50.00%)
    ' Comment Lines: 17 (47.22%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (2.78%)
    '     File Size: 1.13 KB


    '     Enum StyleValue
    ' 
    '         dashDot, dashDotDot, dashed, dotted, hair
    '         medium, mediumDashDot, mediumDashDotDot, mediumDashed, none
    '         s_double, slantDashDot, thick, thin
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
    ''' Enum for the border style
    ''' </summary>
    Public Enum StyleValue
        ''' <summary>no border</summary>
        none
        ''' <summary>hair border</summary>
        hair
        ''' <summary>dotted border</summary>
        dotted
        ''' <summary>dashed border with double-dots</summary>
        dashDotDot
        ''' <summary>dash-dotted border</summary>
        dashDot
        ''' <summary>dashed border</summary>
        dashed
        ''' <summary>thin border</summary>
        thin
        ''' <summary>medium-dashed border with double-dots</summary>
        mediumDashDotDot
        ''' <summary>slant dash-dotted border</summary>
        slantDashDot
        ''' <summary>medium dash-dotted border</summary>
        mediumDashDot
        ''' <summary>medium dashed border</summary>
        mediumDashed
        ''' <summary>medium border</summary>
        medium
        ''' <summary>thick border</summary>
        thick
        ''' <summary>double border</summary>
        s_double
    End Enum
End Namespace
