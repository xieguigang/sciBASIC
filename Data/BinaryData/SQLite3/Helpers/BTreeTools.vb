#Region "Microsoft.VisualBasic::8b87e48927d79dbf31a5f3010c7a4181, Data\BinaryData\SQLite3\Helpers\BTreeTools.vb"

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

    '   Total Lines: 54
    '    Code Lines: 39 (72.22%)
    ' Comment Lines: 3 (5.56%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 12 (22.22%)
    '     File Size: 2.17 KB


    '     Module BTreeTools
    ' 
    '         Function: (+3 Overloads) WalkTableBTree
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects

Namespace ManagedSqlite.Core.Helpers

    Module BTreeTools

        Public Function WalkTableBTree(node As BTreePage) As IEnumerable(Of BTreeCellData)
            If node.[GetType]() Is GetType(BTreeInteriorTablePage) Then
                Return WalkTableBTree(DirectCast(node, BTreeInteriorTablePage))
            End If

            If node.[GetType]() Is GetType(BTreeLeafTablePage) Then
                Return WalkTableBTree(DirectCast(node, BTreeLeafTablePage))
            End If

            Throw New ArgumentException("Did not receive a compatible BTreePage", NameOf(node))
        End Function

        Private Iterator Function WalkTableBTree(interior As BTreeInteriorTablePage) As IEnumerable(Of BTreeCellData)
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

        Private Iterator Function WalkTableBTree(leaf As BTreeLeafTablePage) As IEnumerable(Of BTreeCellData)
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
    End Module
End Namespace
