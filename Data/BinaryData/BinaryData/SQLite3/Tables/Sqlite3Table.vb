#Region "Microsoft.VisualBasic::140f519ece5d2133816e5ec3fbbac20f, Data\BinaryData\BinaryData\SQLite3\Tables\Sqlite3Table.vb"

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

    '     Class Sqlite3Table
    ' 
    '         Properties: RootPage, SchemaDefinition
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: EnumerateRows
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Helpers
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Internal
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Enums

Namespace ManagedSqlite.Core.Tables
    Public Class Sqlite3Table

        Private ReadOnly reader As ReaderBase

        Private ReadOnly Property RootPage() As BTreePage
        Public ReadOnly Property SchemaDefinition() As Sqlite3SchemaRow

        Friend Sub New(reader As ReaderBase, rootPage As BTreePage, table As Sqlite3SchemaRow)
            SchemaDefinition = table
            Me.reader = reader
            Me.RootPage = rootPage
        End Sub

        Public Iterator Function EnumerateRows() As IEnumerable(Of Sqlite3Row)
            Dim cells As IEnumerable(Of BTreeCellData) = BTreeTools.WalkTableBTree(RootPage)

            Dim metaInfos As New List(Of ColumnDataMeta)()
            For Each cell As BTreeCellData In cells
                metaInfos.Clear()

                ' Create a new stream to cover any fragmentation that might occur
                ' The stream is started in the current cells "resident" data, and will overflow to any other pages as needed
                Using dataStream As New SqliteDataStream(reader, cell.Page, CUShort(cell.CellOffset + cell.Cell.CellHeaderSize), cell.Cell.DataSizeInCell, cell.Cell.FirstOverflowPage, cell.Cell.DataSize)
                    Dim reader As New ReaderBase(dataStream, Me.reader)
                    Dim null As Byte
                    Dim headerSize As Long = reader.ReadVarInt(null)

                    While reader.Position < headerSize
                        Dim columnInfo As Long = reader.ReadVarInt(null)

                        Dim meta As New ColumnDataMeta()
                        If columnInfo = 0 Then
                            meta.Type = SqliteDataType.Null
                        ElseIf columnInfo = 1 Then
                            meta.Type = SqliteDataType.[Integer]
                            meta.Length = 1
                        ElseIf columnInfo = 2 Then
                            meta.Type = SqliteDataType.[Integer]
                            meta.Length = 2
                        ElseIf columnInfo = 3 Then
                            meta.Type = SqliteDataType.[Integer]
                            meta.Length = 3
                        ElseIf columnInfo = 4 Then
                            meta.Type = SqliteDataType.[Integer]
                            meta.Length = 4
                        ElseIf columnInfo = 5 Then
                            meta.Type = SqliteDataType.[Integer]
                            meta.Length = 6
                        ElseIf columnInfo = 6 Then
                            meta.Type = SqliteDataType.[Integer]
                            meta.Length = 8
                        ElseIf columnInfo = 7 Then
                            meta.Type = SqliteDataType.Float
                            meta.Length = 8
                        ElseIf columnInfo = 8 Then
                            meta.Type = SqliteDataType.Boolean0
                        ElseIf columnInfo = 9 Then
                            meta.Type = SqliteDataType.Boolean1
                        ElseIf columnInfo = 10 OrElse columnInfo = 11 Then
                            Throw New ArgumentOutOfRangeException()
                        ElseIf (columnInfo And &H1) = &H0 Then
                            ' Even number
                            meta.Type = SqliteDataType.Blob
                            meta.Length = CUShort((columnInfo - 12) \ 2)
                        Else
                            ' Odd number
                            meta.Type = SqliteDataType.Text
                            meta.Length = CUShort((columnInfo - 13) \ 2)
                        End If

                        metaInfos.Add(meta)
                    End While

                    Dim rowData As Object() = New Object(metaInfos.Count - 1) {}
                    For i As Integer = 0 To metaInfos.Count - 1
                        Dim meta As ColumnDataMeta = metaInfos(i)
                        Select Case meta.Type
                            Case SqliteDataType.Null
                                rowData(i) = Nothing
                                
                            Case SqliteDataType.[Integer]
                                ' TODO: Do we handle negatives correctly?
                                rowData(i) = reader.ReadInteger(CByte(meta.Length))
                                
                            Case SqliteDataType.Float
                                rowData(i) = BitConverter.Int64BitsToDouble(reader.ReadInteger(CByte(meta.Length)))
                                
                            Case SqliteDataType.Boolean0
                                rowData(i) = False
                                
                            Case SqliteDataType.Boolean1
                                rowData(i) = True
                                
                            Case SqliteDataType.Blob
                                rowData(i) = reader.Read(meta.Length)
                                
                            Case SqliteDataType.Text
                                rowData(i) = reader.ReadString(meta.Length)
                                
                            Case Else
                                Throw New ArgumentOutOfRangeException()
                        End Select
                    Next

                    Yield New Sqlite3Row(Me, cell.Cell.RowId, rowData)
                End Using
            Next
        End Function
    End Class
End Namespace
