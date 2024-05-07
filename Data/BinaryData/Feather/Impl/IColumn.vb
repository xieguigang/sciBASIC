Imports System
Imports System.Collections.Generic
Imports System.Runtime.InteropServices

Namespace Impl
    Friend Interface IColumn(Of T)
        Inherits IList(Of T) ' implement so Linq's ElementAt is as fast as you'd expect
        ReadOnly Property Index As Long
        ReadOnly Property Name As String
        ReadOnly Property Type As Type
        ReadOnly Property Length As Long

        Function GetRange(rowIndex As Long, length As Integer) As T()

        Function ToArray() As T()

        Sub ToArray(ByRef array As T())
        Sub ToArray(Of V)(ByRef array As V())
        Sub ToArrayValue(ByRef array As Value())

        Sub GetRange(rowSourceIndex As Long, length As Integer, ByRef array As T())
        Sub GetRange(Of V)(rowSourceIndex As Long, length As Integer, ByRef array As V())
        Sub GetRangeValue(rowSourceIndex As Long, length As Integer, ByRef array As Value())

        Sub GetRange(rowSourceIndex As Long, length As Integer, ByRef array As T(), destinationIndex As Integer)
        Sub GetRange(Of V)(rowSourceIndex As Long, length As Integer, ByRef array As V(), destinationIndex As Integer)
        Sub GetRangeValue(rowSourceIndex As Long, length As Integer, ByRef array As Value(), destinationIndex As Integer)

        Function TryGetValue(rowIndex As Long, <Out> ByRef value As T) As Boolean
        Function TryGetValue(Of V)(rowIndex As Long, <Out> ByRef value As V) As Boolean
        Function TryGetValueCell(rowIndex As Long, <Out> ByRef value As Value) As Boolean
    End Interface
End Namespace
