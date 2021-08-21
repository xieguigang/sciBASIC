Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Analysis

    Public Class Communities

        Public Shared Function Community(g As NetworkGraph) As Dictionary(Of String, String)
            Return g.vertex _
                .ToDictionary(Function(v) v.label,
                              Function(v)
                                  Return v.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE)
                              End Function)
        End Function

        Public Shared Function GetCommunitySet(g As NetworkGraph) As Dictionary(Of String, Node())
            Dim [set] As New Dictionary(Of String, List(Of Node))

            For Each v As Node In g.vertex
                Dim community As String = v.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE)

                If community.StringEmpty Then
                    community = "n/a"
                End If

                If Not [set].ContainsKey(community) Then
                    [set].Add(community, New List(Of Node))
                End If

                [set](community).Add(v)
            Next

            Return [set].ToDictionary(Function(v) v.Key, Function(list) list.Value.ToArray)
        End Function

        ''' <summary>
        ''' compute the modularity of the network comminity
        ''' </summary>
        ''' <param name="g"></param>
        ''' <returns></returns>
        Public Shared Function Modularity(g As NetworkGraph) As Double
            Dim m As Double = g.ComputeNodeDegrees.Values.Sum / 2
            Dim q As Double = 0
            Dim communitySet = GetCommunitySet(g)

            For Each entry In communitySet
                Dim Cset As Node() = entry.Value

                For i As Integer = 0 To Cset.Length - 1
                    Dim u As Node = Cset(i)

                    For j As Integer = 0 To Cset.Length - 1
                        Dim v As Node = Cset(j)
                        Dim auv As Double = 0

                        If u.adjacencies.hasNeighbor(v) Then
                            auv = 1
                        End If

                        Dim ku = u.degree.In + u.degree.Out
                        Dim kv = v.degree.In + u.degree.Out
                        Dim subQ As Double = auv - ((ku * kv) / (2 * m))

                        q += subQ
                    Next
                Next
            Next

            Dim mov As Double = (1.0 / (2.0 * m)) * q
            Return mov
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g">
        ''' 请注意，这个必须要要求节点的编号是连续的``0:n``序列中的值，不可以存在重复编号
        ''' </param>
        ''' <returns></returns>
        Public Shared Function Analysis(g As NetworkGraph) As NetworkGraph
            Dim clusters As String() = Louvain.Builder _
                .Load(g) _
                .SolveClusters _
                .GetCommunity

            For Each v As Node In g.vertex
                v.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) = clusters(v.ID)
            Next

            Return g
        End Function
    End Class
End Namespace