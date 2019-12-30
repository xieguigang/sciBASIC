Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Analysis

    Public Module Similarity

        Public Function GraphSimilarity(x As NetworkGraph, y As NetworkGraph) As Double

        End Function

        ''' <summary>
        ''' Compare node similarity between two network graph
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Function NodeSimilarity(a As Node, b As Node) As Double
            Dim atypes As Dictionary(Of String, Integer) = a.nodeGroupCounts
            Dim btypes As Dictionary(Of String, Integer) = b.nodeGroupCounts
            Dim allGroups As Index(Of String) = atypes.Keys.AsList + btypes.Keys
            Dim av As New Vector(allGroups.Select(AddressOf atypes.TryGetValue))
            Dim bv As New Vector(allGroups.Select(AddressOf btypes.TryGetValue))
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

        <Extension>
        Public Function AllNodeTypes(v As Node) As IEnumerable(Of String)
            Return v.adjacencies _
                .EnumerateAllEdges _
                .Select(Function(e)
                            If e.U Is v Then
                                Return e.V.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE)
                            Else
                                Return e.U.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE)
                            End If
                        End Function) _
                .Select(AddressOf Scripting.ToString)
        End Function

    End Module
End Namespace