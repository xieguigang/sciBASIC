#Region "Microsoft.VisualBasic::1ff373be14dc1263b63cdc3e1eb3d107, Microsoft.VisualBasic.Core\src\ComponentModel\Ranges\Relations.vb"

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

    '   Total Lines: 22
    '    Code Lines: 8 (36.36%)
    ' Comment Lines: 12 (54.55%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (9.09%)
    '     File Size: 556 B


    '     Enum Relations
    ' 
    '         Equals, Include, IncludeBy
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Ranges

    Public Enum Relations As Byte

        ''' <summary>
        ''' a &lt;> b
        ''' </summary>
        Irrelevant = 0
        ''' <summary>
        ''' a = b
        ''' </summary>
        Equals
        ''' <summary>
        ''' a = {b} (a including b, b is one of the element in range a)
        ''' </summary>
        Include
        ''' <summary>
        ''' b = {a} (a is included by b, a is one of the element in range b)
        ''' </summary>
        IncludeBy
    End Enum
End Namespace
