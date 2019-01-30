Imports System.Collections.Generic

Namespace ManagedSqlite.Core.Tables
    Friend Class Sqlite3MasterTable
        Public ReadOnly Property Tables() As List(Of Sqlite3SchemaRow)

        Public Sub New(table As Sqlite3Table)
            Tables = New List(Of Sqlite3SchemaRow)()

            Dim rows As IEnumerable(Of Sqlite3Row) = table.EnumerateRows()

            For Each row As Sqlite3Row In rows
                Dim other As New Sqlite3SchemaRow()
                Dim str As String = Nothing
                Dim lng As Long

                row.TryGetOrdinal(0, str)
                other.Type = str

                row.TryGetOrdinal(1, str)
                other.Name = str

                row.TryGetOrdinal(2, str)
                other.TableName = str

                row.TryGetOrdinal(3, lng)
                other.RootPage = CUInt(lng)

                row.TryGetOrdinal(4, str)
                other.Sql = str

                Tables.Add(other)
            Next
        End Sub
    End Class
End Namespace
