Imports System.Runtime.CompilerServices

Public Module ListExtensions

    <Extension> Public Function ToList(Of T, TOut)(source As IEnumerable(Of T),
                                                   [CType] As Func(Of T, TOut),
                                                   Optional parallel As Boolean = False) As List(Of TOut)
        Dim result As List(Of TOut)

        If parallel Then
            result = (From x As T In source.AsParallel Select [CType](x)).ToList
        Else
            result = (From x As T In source Select [CType](x)).ToList
        End If

        Return result
    End Function

    <Extension> Public Function ToList(Of T)(source As IEnumerable(Of T)) As List(Of T)
        Return New List(Of T)(source)
    End Function

    <Extension> Public Function ToList(Of T)(linq As ParallelQuery(Of T)) As List(Of T)
        Return New List(Of T)(linq)
    End Function
End Module
