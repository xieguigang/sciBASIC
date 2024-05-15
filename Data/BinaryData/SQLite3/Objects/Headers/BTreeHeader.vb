#Region "Microsoft.VisualBasic::0e60943fa235537b1e6a63dd9725bb07, Data\BinaryData\SQLite3\Objects\Headers\BTreeHeader.vb"

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

    '   Total Lines: 38
    '    Code Lines: 29
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.35 KB


    '     Structure BTreeHeader
    ' 
    '         Function: Parse, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Internal
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Enums

Namespace ManagedSqlite.Core.Objects.Headers

    Friend Structure BTreeHeader

        Public Type As BTreeType
        Public FirstFreeBlock As UShort
        Public CellCount As UShort
        Public CellContentBegin As UShort
        Public CellContentFragmentedFreeBytes As Byte
        Public RightMostPointer As UInteger

        Public Overrides Function ToString() As String
            Return Type.ToString
        End Function

        Public Shared Function Parse(reader As ReaderBase) As BTreeHeader
            reader.CheckSize(8)

            Dim res As New BTreeHeader()

            res.Type = CType(reader.ReadByte(), BTreeType)
            res.FirstFreeBlock = reader.ReadUInt16()
            res.CellCount = reader.ReadUInt16()
            res.CellContentBegin = reader.ReadUInt16()
            res.CellContentFragmentedFreeBytes = reader.ReadByte()

            If res.Type = BTreeType.InteriorIndexBtreePage OrElse res.Type = BTreeType.InteriorTableBtreePage Then
                reader.CheckSize(4)
                res.RightMostPointer = reader.ReadUInt32()
            End If

            Return res
        End Function
    End Structure
End Namespace
