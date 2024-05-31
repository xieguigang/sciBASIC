#Region "Microsoft.VisualBasic::64f5d0d13a77df118d9b500b8062e456, mime\text%html\CSS\CssUnit.vb"

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

    '   Total Lines: 29
    '    Code Lines: 13 (44.83%)
    ' Comment Lines: 6 (20.69%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (34.48%)
    '     File Size: 428 B


    '     Enum CssUnit
    ' 
    '         Centimeters, Ems, Ex, Inches, Milimeters
    '         None, Picas, Pixels, Points
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Namespace CSS

    ''' <summary>
    ''' Represents the possible units of the CSS lengths
    ''' </summary>
    ''' <remarks>
    ''' http://www.w3.org/TR/CSS21/syndata.html#length-units
    ''' </remarks>
    Public Enum CssUnit
        None

        Ems

        Pixels

        Ex

        Inches

        Centimeters

        Milimeters

        Points

        Picas
    End Enum
End Namespace
