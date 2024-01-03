Imports System.Runtime.CompilerServices

Friend Module Utils

    <Extension()>
    Public Sub addAll(Of T)([set] As ISet(Of T), data As IEnumerable(Of T))
        For Each x As T In data
            [set].Add(x)
        Next
    End Sub

    <Extension()>
    Public Sub removeAll(Of T)([set] As ISet(Of T), data As IEnumerable(Of T))
        For Each x As T In data
            [set].Remove(x)
        Next
    End Sub

End Module
