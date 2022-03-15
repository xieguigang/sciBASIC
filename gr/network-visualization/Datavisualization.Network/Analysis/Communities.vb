#Region "Microsoft.VisualBasic::f3de3c1ad2639a3aac10bd19245b5588, sciBASIC#\gr\network-visualization\Datavisualization.Network\Analysis\Communities.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 122
    '    Code Lines: 79
    ' Comment Lines: 17
    '   Blank Lines: 26
    '     File Size: 4.57 KB


    '     Class Communities
    ' 
    '         Function: Analysis, AnalysisUnweighted, Community, GetCommunitySet, Modularity
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis.FastUnfolding
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

        Public Shared Function AnalysisUnweighted(ByRef g As NetworkGraph, Optional directed As Boolean = True) As NetworkGraph
            Dim maps As New KeyMaps

            For Each link As Edge In g.graphEdges
                Call maps(link.U.label).Add(link.V.label)
                'If Not directed Then
                Call maps(link.V.label).Add(link.U.label)
                'End If
            Next

            Dim clustering As New FastUnfolding(maps)
            Dim communities = clustering.Analysis

            maps = communities.Item1

            For Each group In maps.Keys
                For Each id As String In maps(group)
                    g.GetElementByID(id).data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) = group
                Next
            Next

            Return g
        End Function

        ''' <summary>
        ''' analysis network node community structure
        ''' </summary>
        ''' <param name="g">
        ''' 请注意，这个必须要要求节点的编号是连续的``0:n``序列中的值，不可以存在重复编号
        ''' </param>
        ''' <returns>
        ''' a network model with the <see cref="NamesOf.REFLECTION_ID_MAPPING_NODETYPE"/> 
        ''' property data has been assigned as the community tags.
        ''' </returns>
        Public Shared Function Analysis(ByRef g As NetworkGraph, Optional eps As Double = 0.00001) As NetworkGraph
            Dim clusters As String() = Louvain.Builder _
                .Load(g, eps:=eps) _
                .SolveClusters _
                .GetCommunity

            For Each v As Node In g.vertex
                v.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) = clusters(v.ID)
            Next

            Return g
        End Function
    End Class
End Namespace
