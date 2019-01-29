
Namespace MBW.Utilities.ManagedSqlite.Core.Tables
	Public Class Sqlite3Row
		Public ReadOnly Property Table() As Sqlite3Table
		Public ReadOnly Property RowId() As Long
		Public ReadOnly Property ColumnData() As Object()

		Friend Sub New(table__1 As Sqlite3Table, rowId__2 As Long, columnData__3 As Object())
			Table = table__1
			RowId = rowId__2
			ColumnData = columnData__3
		End Sub

		Public Function TryGetOrdinal(index As UShort, ByRef value As Object) As Boolean
			value = Nothing

			If ColumnData.Length > index Then
				value = ColumnData(index)
				Return True
			End If

			Return False
		End Function

		Public Function TryGetOrdinal(Of T)(index As UShort, ByRef value As T) As Boolean
			Dim tmp As Object

			value = Nothing

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
