Imports System.Collections.Generic
Imports MBW.Utilities.ManagedSqlite.Core.Helpers
Imports MBW.Utilities.ManagedSqlite.Core.Internal
Imports MBW.Utilities.ManagedSqlite.Core.Objects
Imports MBW.Utilities.ManagedSqlite.Core.Objects.Enums

Namespace MBW.Utilities.ManagedSqlite.Core.Tables
	Public Class Sqlite3Table
		Private ReadOnly _reader As ReaderBase

		Private ReadOnly Property RootPage() As BTreePage
		Public ReadOnly Property SchemaDefinition() As Sqlite3SchemaRow

		Friend Sub New(reader As ReaderBase, rootPage__1 As BTreePage, table As Sqlite3SchemaRow)
			SchemaDefinition = table
			_reader = reader
			RootPage = rootPage__1
		End Sub

        Public Iterator Function EnumerateRows() As IEnumerable(Of Sqlite3Row)
            Dim cells As IEnumerable(Of BTreeCellData) = BTreeTools.WalkTableBTree(RootPage)

            Dim metaInfos As New List(Of ColumnDataMeta)()
            For Each cell As BTreeCellData In cells
                metaInfos.Clear()

                ' Create a new stream to cover any fragmentation that might occur
                ' The stream is started in the current cells "resident" data, and will overflow to any other pages as needed
                Using dataStream As New SqliteDataStream(_reader, cell.Page, CUShort(cell.CellOffset + cell.Cell.CellHeaderSize), cell.Cell.DataSizeInCell, cell.Cell.FirstOverflowPage, cell.Cell.DataSize)
                    Dim reader As New ReaderBase(dataStream, _reader)
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
                                Exit Select
                            Case SqliteDataType.[Integer]
                                ' TODO: Do we handle negatives correctly?
                                rowData(i) = reader.ReadInteger(CByte(meta.Length))
                                Exit Select
                            Case SqliteDataType.Float
                                rowData(i) = BitConverter.Int64BitsToDouble(reader.ReadInteger(CByte(meta.Length)))
                                Exit Select
                            Case SqliteDataType.Boolean0
                                rowData(i) = False
                                Exit Select
                            Case SqliteDataType.Boolean1
                                rowData(i) = True
                                Exit Select
                            Case SqliteDataType.Blob
                                rowData(i) = reader.Read(meta.Length)
                                Exit Select
                            Case SqliteDataType.Text
                                rowData(i) = reader.ReadString(meta.Length)
                                Exit Select
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
