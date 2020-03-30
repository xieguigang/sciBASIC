
Imports System.Runtime.CompilerServices

<HideModuleName>
Public Module Extensions

    <Extension>
    Public Sub TakeSnapshots(Of T As Individual)(simulator As Simulator(Of T), getKey As Func(Of T, String), counts As Dictionary(Of String, List(Of Integer)))
        Dim groups = simulator.Snapshot.GroupBy(getKey).ToArray

        For Each group As IGrouping(Of String, T) In groups
            counts(group.Key).Add(group.Count)
        Next
    End Sub
End Module
