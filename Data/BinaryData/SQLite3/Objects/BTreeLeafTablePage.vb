#Region "Microsoft.VisualBasic::b7f3a087d06810bc2b2f4e4139a60a4c, Data\BinaryData\SQLite3\Objects\BTreeLeafTablePage.vb"

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

    '   Total Lines: 101
    '    Code Lines: 55
    ' Comment Lines: 24
    '   Blank Lines: 22
    '     File Size: 4.16 KB


    '     Class BTreeLeafTablePage
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
    ''' SQLite B-Tree datastructure that cells with data
    ''' </summary>
    Friend Class BTreeLeafTablePage
        Inherits BTreePage

        Public Property Cells() As Cell()

        Public Sub New(reader As ReaderBase, page As UInteger, header As BTreeHeader, cellOffsets As UShort())
            MyBase.New(reader, page, header, cellOffsets)
        End Sub

        Protected Overrides Sub ParseInternal()
            Cells = New Cell(CellOffsets.Length - 1) {}

            For i As Integer = 0 To Cells.Length - 1
                Reader.SeekPage(Page, CellOffsets(i))

                Dim bytesSize As Byte
                Dim rowIdSize As Byte

                Dim bytes As Long = Reader.ReadVarInt(bytesSize)
                Dim rowId As Long = Reader.ReadVarInt(rowIdSize)

                Dim overflowPage As UInteger = 0

                ' Calculate overflow size
                Dim P As Long = bytes

                ' let U be the usable size of a database page, the total page size less the reserved space at the end of each page
                Dim U As Integer = Reader.PageSize - Reader.ReservedSpace

                ' X is U-35 for table btree leaf pages or ((U-12)*64/255)-23 for index pages.
                Dim X As Integer = U - 35

                ' M is always ((U-12)*32/255)-23.
                Dim M As Integer = (U - 12) * 32 \ 255 - 23

                ' Let K be M+((P-M)%(U-4)).
                Dim K As Integer = CInt(M + ((P - M) Mod (U - 4)))

                ' If P<=X then all P bytes of payload are stored directly on the btree page without overflow.
                ' If P>X and K<=X then the first K bytes of P are stored on the btree page and the remaining P-K bytes are stored on overflow pages.
                ' If P>X and K>X then the first M bytes of P are stored on the btree page and the remaining P-M bytes are stored on overflow pages.
                ' The number of bytes stored on the leaf page is never less than M.

                Dim bytesInCell As UShort

                If P <= X Then
                    ' All data is in cell
                    bytesInCell = CUShort(P)
                ElseIf P > X AndAlso K <= X Then
                    bytesInCell = CUShort(K)
                ElseIf P > X AndAlso K > X Then
                    bytesInCell = CUShort(M)
                Else
                    Throw New InvalidOperationException("We're not supposed to be here")
                End If

                If bytes > bytesInCell Then
                    ' We have overflow
                    Reader.Skip(bytesInCell)
                    overflowPage = Reader.ReadUInt32()
                End If

                Cells(i) = New Cell() With {
                     .CellHeaderSize = CByte(bytesSize + rowIdSize),
                     .DataSize = bytes,
                     .DataSizeInCell = bytesInCell,
                     .RowId = rowId,
                     .FirstOverflowPage = overflowPage
                }
            Next
        End Sub

        Public Structure Cell
            ''' <summary>
            ''' The size of the data in the cell (including data in overflow), excluding the header and overfow page pointers
            ''' </summary>
            Public DataSize As Long

            ''' <summary>
            ''' The size of the data in the cell (in the first page only), excluding the header and overfow page pointers
            ''' Maximum cell size slightly less than one page, which is 65K
            ''' </summary>
            Public DataSizeInCell As UShort

            Public RowId As Long
            Public FirstOverflowPage As UInteger

            ''' <summary>
            ''' The size of the two VarInts in the beginning of the cell
            ''' </summary>
            Public CellHeaderSize As Byte
        End Structure
    End Class
End Namespace
