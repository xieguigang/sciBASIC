#Region "Microsoft.VisualBasic::55f97632107091f0512eb5cae7958894, gr\network-visualization\Datavisualization.Network\Analysis\Communities.vb"

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

    '   Total Lines: 174
    '    Code Lines: 102 (58.62%)
    ' Comment Lines: 40 (22.99%)
    '    - Xml Docs: 95.00%
    ' 
    '   Blank Lines: 32 (18.39%)
    '     File Size: 6.94 KB


    '     Class Communities
    ' 
    '         Function: Analysis, AnalysisUnweighted, Community, EvaluateCommunity, GetCommunitySet
    '                   Modularity
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

        ''' <summary>
        ''' extract community cluster information from the property value 
        ''' <see cref="NamesOf.REFLECTION_ID_MAPPING_NODETYPE"/>.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <returns></returns>
        Public Shared Function Community(g As NetworkGraph) As Dictionary(Of String, String)
            Return g.vertex _
                .ToDictionary(Function(v) v.label,
                              Function(v)
                                  Return v.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE)
                              End Function)
        End Function

        ''' <summary>
        ''' just group the node vertex via the data property 
        ''' <see cref="NamesOf.REFLECTION_ID_MAPPING_NODETYPE"/>
        ''' </summary>
        ''' <param name="g"></param>
        ''' <returns></returns>
        Public Shared Function GetCommunitySet(g As NetworkGraph) As Dictionary(Of String, Node())
            Dim [set] As New Dictionary(Of String, List(Of Node))
            Dim community As String

            For Each v As Node In g.vertex
                community = v.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE)

                If community.StringEmpty Then
                    community = "n/a"
                End If

                If Not [set].ContainsKey(community) Then
                    [set].Add(community, New List(Of Node))
                End If

                [set](community).Add(v)
            Next

            Return [set].ToDictionary(Function(v) v.Key,
                                      Function(list)
                                          Return list.Value.ToArray
                                      End Function)
        End Function

        ''' <summary>
        ''' compute the modularity of the network comminity
        ''' </summary>
        ''' <param name="g">
        ''' the graph data should be assign into multiple node
        ''' cluster via some algorithm at first, before calling 
        ''' this modularity evalution function.
        ''' </param>
        ''' <returns></returns>
        Public Shared Function Modularity(g As NetworkGraph, Optional ByRef qsub As Double() = Nothing) As Double
            Dim m As Double = g.ComputeNodeDegrees.Values.Sum / 2
            Dim qset As New List(Of Double)
            Dim communitySet = GetCommunitySet(g)

            For Each entry As KeyValuePair(Of String, Node()) In communitySet
                Dim Cset As Node() = entry.Value
                Dim subQ As Double = EvaluateCommunity(Cset, m)

                Call qset.Add(subQ)
            Next

            Dim q As Double = qset.Sum
            Dim mov As Double = (1.0 / (2.0 * m)) * q

            qsub = qset.ToArray

            Return mov
        End Function

        Private Shared Function EvaluateCommunity(Cset As Node(), m As Double) As Double
            Return Cset _
                .AsParallel _
                .Select(Function(u, i)
                            Dim q As Double

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

                            Return q
                        End Function) _
                .Sum
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
        ''' <param name="slotName">
        ''' the graph class community information will be save at the <see cref="NamesOf.REFLECTION_ID_MAPPING_NODETYPE"/> by default.
        ''' </param>
        ''' <param name="max_class">
        ''' controls the max number of the node class we have, default value 
        ''' means no limits: get as more number of node class we can get.
        ''' </param>
        ''' <returns>
        ''' a network model with the <see cref="NamesOf.REFLECTION_ID_MAPPING_NODETYPE"/> 
        ''' property data has been assigned as the community tags by default.
        ''' </returns>
        Public Shared Function Analysis(ByRef g As NetworkGraph,
                                        Optional eps As Double = 0.00001,
                                        Optional prefix As String = Nothing,
                                        Optional max_class As Integer = Integer.MaxValue,
                                        Optional slotName As String = NamesOf.REFLECTION_ID_MAPPING_NODETYPE) As NetworkGraph

            Dim clusters As String() = Louvain.Builder _
                .Load(g, eps:=eps) _
                .SolveClusters(max_class) _
                .GetCommunity

            If Not prefix.StringEmpty Then
                clusters = clusters _
                    .Select(Function(id) $"{prefix}{id}") _
                    .ToArray
            End If

            For Each v As Node In g.vertex
                v.data(slotName) = clusters(v.ID)
            Next

            Dim uniques = clusters.Distinct.ToArray

            Call $"get {uniques.Length} sub-network clusters:".debug

            If uniques.Length <= 13 Then
                Call uniques.JoinBy(vbTab).debug
            End If

            Return g
        End Function
    End Class
End Namespace
