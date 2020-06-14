#Region "Microsoft.VisualBasic::5c930a6c8fe1ca8a11c2fcacd92e9813, gr\network-visualization\Datavisualization.Network\Analysis\Statistics.vb"

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

'     Module Statistics
' 
'         Function: ComputeDegreeData, ComputeNodeDegrees, Sum
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis.Dijkstra
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.Abstract
Imports Microsoft.VisualBasic.Linq
Imports GraphNetwork = Microsoft.VisualBasic.Data.GraphTheory.Network
Imports names = Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic.NamesOf

Namespace Analysis

    Public Module Statistics

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ComputeDegreeData(Of T As IInteraction)(edges As IEnumerable(Of T)) As ([in] As Dictionary(Of String, Integer), out As Dictionary(Of String, Integer))
            Return GraphNetwork.ComputeDegreeData(edges, Function(l) l.source, Function(l) l.target)
        End Function

        <Extension>
        Public Function Sum(degrees As ([in] As Dictionary(Of String, Integer), out As Dictionary(Of String, Integer))) As Dictionary(Of String, Integer)
            Dim degreeValue As New Dictionary(Of String, Integer)(degrees.in)

            For Each node In degrees.out
                degreeValue(node.Key) += degreeValue(node.Key) + node.Value
            Next

            Return degreeValue
        End Function

        <Extension>
        Public Function BetweennessCentrality(graph As NetworkGraph, Optional undirected As Boolean = False) As Dictionary(Of String, Integer)
            Return DijkstraRouter.FromNetwork(graph, undirected).BetweennessCentrality
        End Function

        ''' <summary>
        ''' 这个函数计算网络的节点的degree，然后将degree数据写入节点的同时，通过字典返回给用户
        ''' </summary>
        ''' <param name="g"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ComputeNodeDegrees(ByRef g As NetworkGraph) As Dictionary(Of String, Integer)
            Dim connectNodes = g _
                .graphEdges _
                .Select(Function(link) {link.U.label, link.V.label}) _
                .IteratesALL _
                .GroupBy(Function(id) id) _
                .ToDictionary(Function(ID) ID.Key,
                              Function(list)
                                  Return list.Count
                              End Function)
            Dim d%

            With g.graphEdges.ComputeDegreeData
                For Each node In g.vertex

                    If Not connectNodes.ContainsKey(node.label) Then
                        ' 这个节点是孤立的节点，度为零
                        node.data.Add(names.REFLECTION_ID_MAPPING_DEGREE, 0)
                        node.data.Add(names.REFLECTION_ID_MAPPING_DEGREE_IN, 0)
                        node.data.Add(names.REFLECTION_ID_MAPPING_DEGREE_OUT, 0)

                    Else
                        d = connectNodes(node.label)
                        node.data.Add(names.REFLECTION_ID_MAPPING_DEGREE, d)

                        If .in.ContainsKey(node.label) Then
                            d = .in(node.label)
                            node.data.Add(names.REFLECTION_ID_MAPPING_DEGREE_IN, d)
                        End If
                        If .out.ContainsKey(node.label) Then
                            d = .out(node.label)
                            node.data.Add(names.REFLECTION_ID_MAPPING_DEGREE_OUT, d)
                        End If
                    End If
                Next
            End With

            Return connectNodes
        End Function
    End Module
End Namespace
