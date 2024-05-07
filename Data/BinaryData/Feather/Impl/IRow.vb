Imports System.Runtime.InteropServices

Namespace Impl
    Friend Interface IRow
        ReadOnly Property Index As Long
        ReadOnly Property Length As Long

        Function TryGetValue(columnIndex As Long, <Out> ByRef value As Value) As Boolean
        Function TryGetValue(Of T)(columnIndex As Long, <Out> ByRef value As T) As Boolean

        Function TryGetValue(columnName As String, <Out> ByRef value As Value) As Boolean
        Function TryGetValue(Of T)(columnName As String, <Out> ByRef value As T) As Boolean

        Function ToArray() As Value()
        Function GetRange(columnIndex As Long, length As Integer) As Value()

        Sub ToArray(ByRef array As Value())
        Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value())
        Sub GetRange(columnSourceIndex As Long, length As Integer, ByRef array As Value(), destinationIndex As Integer)
    End Interface
End Namespace
