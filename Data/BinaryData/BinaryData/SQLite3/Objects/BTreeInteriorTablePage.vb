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
