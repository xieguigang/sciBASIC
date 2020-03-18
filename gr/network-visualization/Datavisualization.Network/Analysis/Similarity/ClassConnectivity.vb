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

            If av.Length = 1 Then
                ' 20200318 deal with the NaN result value.
                If av(Scan0) = bv(Scan0) Then
                    Return 1
                Else
                    Return 0
                End If
            ElseIf av.Length = 0 Then
                Return 0
            Else
                Dim cos As Double = Math.SSM(av, bv)
                Return cos
            End If
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