#Region "Microsoft.VisualBasic::f8edb1eb25280fb797194ab9ff43c9ba, Data\BinaryData\Feather\Impl\FeatherMagic.vb"

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

    '   Total Lines: 13
    '    Code Lines: 11
    ' Comment Lines: 0
    '   Blank Lines: 2
    '     File Size: 735 B


    '     Module FeatherMagic
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Namespace Impl
    Friend Module FeatherMagic
        Public Const ARROW_ALIGNMENT As Integer = 8  ' 64-bits per https://arrow.apache.org/docs/memory_layout.html
        Public Const FEATHER_VERSION As Integer = 2
        Public Const NULL_BITMASK_ALIGNMENT As Integer = 8
        Public Const MAGIC_HEADER_SIZE As Integer = 4
        Public Const MAGIC_HEADER As Integer = Microsoft.VisualBasic.AscW("F"c) << 8 * 0 Or Microsoft.VisualBasic.AscW("E"c) << 8 * 1 Or Microsoft.VisualBasic.AscW("A"c) << 8 * 2 Or Microsoft.VisualBasic.AscW("1"c) << 8 * 3 ' 'FEA1', little endian

        Public ReadOnly DATETIME_EPOCH As Date = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    End Module
End Namespace

