
Namespace ManagedSqlite.Core.Tables
    Public Class Sqlite3Row
        Public ReadOnly Property Table() As Sqlite3Table
        Public ReadOnly Property RowId() As Long
        Public ReadOnly Property ColumnData() As Object()

        Friend Sub New(table As Sqlite3Table, rowId As Long, columnData As Object())
            Me.Table = table
            Me.RowId = rowId
            Me.ColumnData = columnData
        End Sub

        Public Function TryGetOrdinal(index As UShort, ByRef value As Object) As Boolean
            value = Nothing

            If ColumnData.Length > index Then
                value = ColumnData(index)
                Return True
            End If

            Return False
        End Function

        Public Function TryGetOrdinal(Of T)(index As UShort, Optional ByRef value As T = Nothing) As Boolean
            Dim tmp As Object = Nothing

            If Not TryGetOrdinal(index, tmp) Then
                Return False
            End If

            ' TODO: Is null a success case?
            If tmp Is Nothing Then
                Return False
            End If

            value = DirectCast(Convert.ChangeType(tmp, GetType(T)), T)
            Return True
        End Function
    End Class
End Namespace
