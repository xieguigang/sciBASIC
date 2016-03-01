Imports System.Runtime.CompilerServices

Public Module ListExtensions

    <Extension> Public Function ToList(Of T)(source As IEnumerable(Of T)) As List(Of T)
        Return New List(Of T)(source)
    End Function

    <Extension> Public Function ToList(Of T)(linq As ParallelQuery(Of T)) As List(Of T)
        Return New List(Of T)(linq)
    End Function
End Module
