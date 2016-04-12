Imports System.Runtime.CompilerServices

Public Module CloneExtensions

    <Extension> Public Function Clone(Of T, V)(hash As IDictionary(Of T, V)) As Dictionary(Of T, V)
        Return New Dictionary(Of T, V)(hash)
    End Function

    <Extension> Public Function Clone(Of T)(list As List(Of T)) As List(Of T)
        Return New List(Of T)(list)
    End Function

    <Extension> Public Function Copy(Of T)(array As T()) As T()
        Return DirectCast(array.Clone, T())
    End Function

    <Extension> Public Function Clone(s As String) As String
        Return New String(s.ToCharArray)
    End Function
End Module
