Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Analysis.SimilarityImpl

    Module ClassConnectivity

        Public Function MeasureCosValue(a As Node, b As Node) As Double
            Dim atypes As Dictionary(Of String, Integer) = a.nodeGroupCounts
            Dim btypes As Dictionary(Of String, Integer) = b.nodeGroupCounts
            Dim allGroups As Index(Of String) = atypes.Keys.AsList + btypes.Keys
            Dim av As New Vector(allGroups.EnumerateMapKeys.Select(AddressOf atypes.TryGetValue))
            Dim bv As New Vector(allGroups.EnumerateMapKeys.Select(AddressOf btypes.TryGetValue))
            Dim cos As Double = Math.SSM(av, bv)

            Return cos
        End Function

        <Extension>
        Private Function nodeGroupCounts(v As Node) As Dictionary(Of String, Integer)
            Return (From type As String In v.AllNodeTypes Group By type Into Count) _
                .ToDictionary(Function(group) group.type,
                              Function(group)
                                  Return group.Count
                              End Function)
        End Function
    End Module
End Namespace