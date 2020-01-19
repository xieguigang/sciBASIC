#Region "Microsoft.VisualBasic::5c96db83dfb97cb6fa16b7d97afc657c, gr\network-visualization\Datavisualization.Network\Analysis\Similarity.vb"

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

'     Module Similarity
' 
'         Function: AllNodeTypes, GraphSimilarity, nodeGroupCounts, NodeSimilarity
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Analysis

    Public Module Similarity

        Public Function GraphSimilarity(x As NetworkGraph, y As NetworkGraph, Optional cutoff# = 0.85, Optional topologyCos As Boolean = False) As Double
            ' JaccardIndex (intersects / union) -> highly similar / (dis-similar + highly similar)
            Dim similar%
            Dim top#
            Dim cos#

            ' 20191231
            ' X should always greater than Y in graph size
            ' or vertex count - similar may be negative value
            ' the negative value will cause union value smaller 
            ' than similar count, result an invalid cos similar
            ' value which is greater than 1
            If x.size.vertex > y.size.vertex Then
                Dim tmp = x

                x = y
                y = tmp
            End If

            For Each a As Node In x.vertex
                top = -99999

                For Each b As Node In y.vertex
                    cos = Similarity.NodeSimilarity(a, b, topologyCos)

                    If cos > top Then
                        top = cos
                    End If
                Next

                If top >= cutoff Then
                    similar += 1
                End If
            Next

            Dim union As Integer = (similar + (x.vertex.Count - similar) + (y.vertex.Count - similar))
            Dim jaccardIndex As Double = similar / union

            Return jaccardIndex
        End Function

        ''' <summary>
        ''' Compare node similarity between two network graph
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Function NodeSimilarity(a As Node, b As Node, Optional topologyCos As Boolean = True) As Double
            ' consider the node itself
            ' if the two node is not in same datatype, then returns not similar
            If Scripting.ToString(a.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE)) <> Scripting.ToString(b.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE)) Then
                Return 0
            Else
                Dim atypes As Dictionary(Of String, Integer) = a.nodeGroupCounts
                Dim btypes As Dictionary(Of String, Integer) = b.nodeGroupCounts
                Dim allGroups As Index(Of String) = atypes.Keys.AsList + btypes.Keys
                Dim av As New Vector(allGroups.EnumerateMapKeys.Select(AddressOf atypes.TryGetValue))
                Dim bv As New Vector(allGroups.EnumerateMapKeys.Select(AddressOf btypes.TryGetValue))
                Dim cos As Double = Math.SSM(av, bv)

                If topologyCos Then
                    Return cos * Similarity.TopologyCos(a, b)
                Else
                    Return cos
                End If
            End If
        End Function

        Public Function TopologyCos(a As Node, b As Node) As Double
            Dim aDist = a.nodeGroupDistance
            Dim bDist = b.nodeGroupDistance
            Dim allGroups As Index(Of String) = aDist.Keys.AsList + bDist.Keys
            Dim av As New Vector(allGroups.EnumerateMapKeys.Select(AddressOf aDist.TryGetValue))
            Dim bv As New Vector(allGroups.EnumerateMapKeys.Select(AddressOf bDist.TryGetValue))
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
        Private Function nodeGroupDistance(v As Node) As Dictionary(Of String, Double)
            Return (From type As (String, Double) In v.AllNodeDistance Group By type.Item1 Into Group) _
                 .ToDictionary(Function(group) group.Item1,
                               Function(group)
                                   Return group.Group.Average(Function(n) n.Item2)
                               End Function)
        End Function

        <Extension>
        Public Function AllNodeDistance(v As Node) As IEnumerable(Of (String, Double))
            If v.adjacencies Is Nothing Then
                ' 孤立节点
                Return {}
            End If

            Dim a = v.data.initialPostion

            Return v.adjacencies _
                .EnumerateAllEdges _
                .Select(Function(e)
                            Dim partner As Node

                            If e.U Is v Then
                                partner = e.V
                            Else
                                partner = e.U
                            End If

                            Dim type$ = partner.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE)
                            Dim b = partner.data.initialPostion
                            Dim dist = (a.x - b.x) ^ 2 + (a.y - b.y) ^ 2 + (a.z - b.z) ^ 2

                            Return (If(type, "n/a"), dist)
                        End Function)
        End Function

        <Extension>
        Public Function AllNodeTypes(v As Node) As IEnumerable(Of String)
            If v.adjacencies Is Nothing Then
                ' 孤立节点
                Return {}
            End If

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
