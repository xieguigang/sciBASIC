#Region "Microsoft.VisualBasic::37b6d5a7270f5537fb17e5fe853143b6, Data\BinaryData\SQLite3\Objects\Enums\BTreeType.vb"

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

    '   Total Lines: 8
    '    Code Lines: 8
    ' Comment Lines: 0
    '   Blank Lines: 0
    '     File Size: 250 B


    '     Enum BTreeType
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ManagedSqlite.Core.Objects.Enums
    Friend Enum BTreeType As Byte
        InteriorIndexBtreePage = &H2
        InteriorTableBtreePage = &H5
        LeafIndexBtreePage = &HA
        LeafTableBtreePage = &HD
    End Enum
End Namespace
