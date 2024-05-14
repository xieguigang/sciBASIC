#Region "Microsoft.VisualBasic::5fe0dba05ae83114adbf0ac246d20c59, Microsoft.VisualBasic.Core\src\Extensions\CompareMethod.vb"

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

    '   Total Lines: 17
    '    Code Lines: 6
    ' Comment Lines: 11
    '   Blank Lines: 0
    '     File Size: 550 B


    '     Enum CompareMethod
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#If NETSTANDARD1_2_OR_GREATER Then
    '
    ' 摘要:
    '     Indicates how to compare strings when calling comparison functions.
    Public Enum CompareMethod
        '
        ' 摘要:
        '     Performs a binary comparison. This member is equivalent to the Visual Basic constant
        '     vbBinaryCompare.
        Binary = 0
        '
        ' 摘要:
        '     Performs a textual comparison. This member is equivalent to the Visual Basic
        '     constant vbTextCompare.
        Text = 1
    End Enum
#End If
