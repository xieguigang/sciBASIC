
Imports System.Runtime.CompilerServices

<HideModuleName>
Public Module Extensions

    <Extension>
    Public Sub TakeSnapshots(Of T As Individual)(simulator As Simulator(Of T), getKey As Func(Of T, String), counts As Dictionary(Of String, List(Of Integer)))
        Dim groups = simulator.Snapshot.GroupBy(getKey).ToDictionary(Function(n) n.Key, Function(n) n.Count)

        For Each type In counts.Keys
            If groups.ContainsKey(type) Then
                counts(type).Add(groups(type))
            Else
                counts(type).Add(0)
            End If
        Next
    End Sub
End Module
