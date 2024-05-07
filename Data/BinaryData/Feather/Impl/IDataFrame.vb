Imports System.Runtime.InteropServices

Namespace FeatherDotNet.Impl
    Friend Interface IDataFrame
        ReadOnly Property RowCount As Long
        ReadOnly Property ColumnCount As Long

        ReadOnly Property Basis As BasisType

        Default ReadOnly Property Item(rowIndex As Long, columnIndex As Long) As Value
        Default ReadOnly Property Item(rowIndex As Long, columnName As String) As Value

        Function TryGetValue(rowIndex As Long, columnIndex As Long, <Out> ByRef value As Value) As Boolean
        Function TryGetValue(Of T)(rowIndex As Long, columnIndex As Long, <Out> ByRef value As T) As Boolean

        Function TryGetValue(rowIndex As Long, columnName As String, <Out> ByRef value As Value) As Boolean
        Function TryGetValue(Of T)(rowIndex As Long, columnName As String, <Out> ByRef value As T) As Boolean
    End Interface
End Namespace
