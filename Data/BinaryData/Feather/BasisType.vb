#Region "Microsoft.VisualBasic::c711fbd217a863a5d0b6e3c4f151ca8e, Data\BinaryData\Feather\BasisType.vb"

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
    '    Code Lines: 4
    ' Comment Lines: 9
    '   Blank Lines: 2
    '     File Size: 361 B


    ' Enum BasisType
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region



''' <summary>
''' Represents whether a dataframe is using 0-based or 1-based indexing.
''' </summary>
Public Enum BasisType
    ''' <summary>
    ''' 1-based indexing - intended for ports from 1-based languages
    ''' </summary>
    One = 1
    ''' <summary>
    ''' 0-based indexing - the C# standard
    ''' </summary>
    Zero = 2
End Enum

