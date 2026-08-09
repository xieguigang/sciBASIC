#Region "Microsoft.VisualBasic::3d6d75239ff31be2ee647fae2235b0e5, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Style\FillType.vb"

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

    '   Total Lines: 33
    '    Code Lines: 15 (45.45%)
    ' Comment Lines: 15 (45.45%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (9.09%)
    '     File Size: 929 B


    '     Enum FillType
    ' 
    '         fillColor, patternColor
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum PatternValue
    ' 
    '         darkGray, gray0625, gray125, lightGray, mediumGray
    '         none, solid
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
    ''' Enum for the type of the color
    ''' </summary>
    Public Enum FillType
        ''' <summary>Color defines a pattern color </summary>
        patternColor
        ''' <summary>Color defines a solid fill color </summary>
        fillColor
    End Enum

    ''' <summary>
    ''' Enum for the pattern values
    ''' </summary>
    Public Enum PatternValue
        ''' <summary>No pattern (default)</summary>
        none
        ''' <summary>Solid fill (for colors)</summary>
        solid
        ''' <summary>Dark gray fill</summary>
        darkGray
        ''' <summary>Medium gray fill</summary>
        mediumGray
        ''' <summary>Light gray fill</summary>
        lightGray
        ''' <summary>6.25% gray fill</summary>
        gray0625
        ''' <summary>12.5% gray fill</summary>
        gray125
    End Enum
End Namespace
