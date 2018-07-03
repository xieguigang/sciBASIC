Imports System.Runtime.CompilerServices

Public Module IsNullOrEmptyExtensions

    <Extension>
    Public Function Empty(Of T)(list As IEnumerable(Of T)) As Boolean
        Return list Is Nothing OrElse Not list.Any
    End Function
End Module
