Imports System.Diagnostics

Namespace ManagedSqlite.Core.Objects
    <DebuggerDisplay("Page {Page}, Size {Cell.DataSizeInCell} / {Cell.DataSize}")>
    Friend Class BTreeCellData
        Friend Property Page() As UInteger
        Friend Property Cell() As BTreeLeafTablePage.Cell
        Friend Property CellOffset() As UShort
    End Class
End Namespace
