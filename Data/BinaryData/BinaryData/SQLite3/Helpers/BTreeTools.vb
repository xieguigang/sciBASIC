Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects

Namespace ManagedSqlite.Core.Helpers
    Friend NotInheritable Class BTreeTools
        Private Sub New()
        End Sub
        Public Shared Function WalkTableBTree(node As BTreePage) As IEnumerable(Of BTreeCellData)
            If node.[GetType]() Is GetType(BTreeInteriorTablePage) Then
                Return WalkTableBTree(DirectCast(node, BTreeInteriorTablePage))
            End If

            If node.[GetType]() Is GetType(BTreeLeafTablePage) Then
                Return WalkTableBTree(DirectCast(node, BTreeLeafTablePage))
            End If

            Throw New ArgumentException("Did not receive a compatible BTreePage", NameOf(node))
        End Function

        Private Shared Iterator Function WalkTableBTree(interior As BTreeInteriorTablePage) As IEnumerable(Of BTreeCellData)
            ' Walk sub-pages and yield their data
            For Each cell As BTreeInteriorTablePage.Cell In interior.Cells
                Dim subPage As BTreePage = BTreePage.Parse(interior.Reader, cell.LeftPagePointer)

                For Each data As BTreeCellData In WalkTableBTree(subPage)
                    Yield data
                Next
            Next

            If interior.Header.RightMostPointer > 0 Then
                ' Process sibling page
                Dim subPage As BTreePage = BTreePage.Parse(interior.Reader, interior.Header.RightMostPointer)

                For Each data As BTreeCellData In WalkTableBTree(subPage)
                    Yield data
                Next
            End If
        End Function

        Private Shared Iterator Function WalkTableBTree(leaf As BTreeLeafTablePage) As IEnumerable(Of BTreeCellData)
            ' Walk cells and yield their data
            For i As Integer = 0 To leaf.Cells.Length - 1
                Dim cell As BTreeLeafTablePage.Cell = leaf.Cells(i)

                Dim res As New BTreeCellData()

                res.Cell = cell
                res.CellOffset = leaf.CellOffsets(i)
                res.Page = leaf.Page

                Yield res
            Next
        End Function
    End Class
End Namespace
