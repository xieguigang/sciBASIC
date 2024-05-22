#Region "Microsoft.VisualBasic::31ea600258d26ab129fb033a307fb4ab, Data\BinaryData\SQLite3\Objects\BTreeInteriorTablePage.vb"

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
    '    Code Lines: 27 (71.05%)
    ' Comment Lines: 3 (7.89%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (21.05%)
    '     File Size: 1.33 KB


    '     Class BTreeInteriorTablePage
    ' 
    '         Properties: Cells
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: ParseInternal
    '         Structure Cell
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Internal
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Headers

Namespace ManagedSqlite.Core.Objects
    ''' <summary>
    ''' SQLite B-Tree datastructure that contains other Interior / Leaf pages
    ''' </summary>
    Friend Class BTreeInteriorTablePage
        Inherits BTreePage
        Public Property Cells() As Cell()

        Public Sub New(reader As ReaderBase, page As UInteger, header As BTreeHeader, cellOffsets As UShort())

            MyBase.New(reader, page, header, cellOffsets)
        End Sub

        Protected Overrides Sub ParseInternal()
            Cells = New Cell(CellOffsets.Length - 1) {}

            For i As Integer = 0 To Cells.Length - 1
                Reader.SeekPage(Page, CellOffsets(i))

                Dim leftPagePointer As UInteger = Reader.ReadUInt32()
                Dim intKey As Long = Reader.ReadVarInt()

                Cells(i) = New Cell() With {
                     .LeftPagePointer = leftPagePointer,
                     .IntegerKey = intKey
                }
            Next
        End Sub

        Public Structure Cell
            Public LeftPagePointer As UInteger
            Public IntegerKey As Long
        End Structure
    End Class
End Namespace
