#Region "Microsoft.VisualBasic::1e7ba082a8c5add9600ea86f40e1fcc2, gr\network-visualization\network_layout\Orthogonal\util\algorithm.vb"

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

    '   Total Lines: 234
    '    Code Lines: 165 (70.51%)
    ' Comment Lines: 36 (15.38%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 33 (14.10%)
    '     File Size: 10.12 KB


    '     Module algorithm
    ' 
    '         Function: (+2 Overloads) AsGraphMatrix, DoLayout, RunLayoutMatrix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.EdgeBundling
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Orthogonal.optimization
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

Namespace Orthogonal

    Public Module algorithm

        <Extension>
        Public Function AsGraphMatrix(g As NetworkGraph) As Integer()()
            Return g.AsGraphMatrix(vlist:=g.vertex.ToArray)
        End Function

        <Extension>
        Private Function AsGraphMatrix(g As NetworkGraph, vlist As Node()) As Integer()()
            Dim graph As Integer()() = RectangularArray.Matrix(Of Integer)(vlist.Length, vlist.Length)

            For i As Integer = 0 To vlist.Length - 1
                Dim vr As Integer() = graph(i)
                Dim v = vlist(i)
                Dim j As Integer = -1

                For Each node As Node In vlist
                    j += 1

                    If node Is v Then
                        Continue For
                    End If

                    ' 20240408 the graph matrix needs to be a un-directed graph
                    ' the getEdges function is a kind of function for get directed edges set
                    ' so we needs a tuple and reverse tuple combine for create a un-directed
                    ' edge set
                    If g.GetEdges(v, node).Any OrElse g.GetEdges(node, v).Any Then
                        vr(j) = 1
                    End If
                Next
            Next

            Return graph
        End Function

        ''' <summary>
        ''' do orthogonal layout and then write node layout position information into network graph
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="numberOfAttempts"></param>
        ''' <param name="optimize"></param>
        ''' <param name="simplify"></param>
        ''' <param name="fixNonOrthogonal"></param>
        ''' <returns></returns>
        <Extension>
        Public Function DoLayout(<Out> ByRef g As NetworkGraph,
                                 Optional numberOfAttempts As Integer = 10,
                                 Optional optimize As Boolean = True,
                                 Optional simplify As Boolean = True,
                                 Optional fixNonOrthogonal As Boolean = True) As NetworkGraph

            Dim vlist As Node() = g.vertex.ToArray
            Dim layout As OrthographicEmbeddingResult = g _
                .AsGraphMatrix(vlist) _
                .RunLayoutMatrix(
                    numberOfAttempts:=numberOfAttempts,
                    optimize:=optimize,
                    simplify:=simplify,
                    fixNonOrthogonal:=fixNonOrthogonal
                )
            Dim index As Integer
            Dim u, v As Node

            If layout Is Nothing Then
                Throw New InvalidProgramException("run layout failure...")
            End If

            Dim minx As Double = -1
            Dim maxx As Double = -1
            Dim miny As Double = -1
            Dim maxy As Double = -1

            For i = 0 To layout.nodeIndexes.Length - 1
                If i = 0 Then
                    maxx = layout.x(i)
                    minx = layout.x(i)
                    maxy = layout.y(i)
                    miny = layout.y(i)
                Else
                    If layout.x(i) < minx Then
                        minx = layout.x(i)
                    End If
                    If layout.x(i) > maxx Then
                        maxx = layout.x(i)
                    End If
                    If layout.y(i) < miny Then
                        miny = layout.y(i)
                    End If
                    If layout.y(i) > maxy Then
                        maxy = layout.y(i)
                    End If
                End If
            Next

            For i As Integer = 0 To layout.nodeIndexes.Length - 1
                index = layout.nodeIndexes(i)

                If index > -1 Then
                    v = vlist(index)
                    v.data.initialPostion = New FDGVector2(layout(i))
                    v.data.initialPostion.x -= minx
                    v.data.initialPostion.y -= miny
                End If
            Next

            For i = 0 To layout.nodeIndexes.Length - 1
                For j = 0 To layout.nodeIndexes.Length - 1
                    If layout.edges(i)(j) Then
                        Dim x0 = std.Min(layout.x(i), layout.x(j)) - minx
                        Dim y0 = std.Min(layout.y(i), layout.y(j)) - miny
                        Dim x1 = std.Max(layout.x(i), layout.x(j)) - minx
                        Dim y1 = std.Max(layout.y(i), layout.y(j)) - miny

                        ' try to find a node
                        For Each edge As Edge In g.graphEdges
                            Dim pu = edge.U.data.initialPostion.Point2D
                            Dim pv = edge.V.data.initialPostion.Point2D

                            If std.Abs(pu.X - x0) < 0.1 AndAlso std.Abs(pu.Y - y0) < 0.1 Then
                                ' pu is [x0,y0]
                                If std.Abs(pv.X - x1) < 0.1 AndAlso std.Abs(pv.Y - y1) < 0.1 Then
                                    ' pv is [x1,y1]
                                    ' is a direct link between two node
                                    ' ignores
                                    Exit For
                                Else
                                    ' [x1,y1] is an edge turn point
                                    edge.data.bends = {WayPointVector.CreateVector(pu, pv, x1, y1)}
                                    Exit For
                                End If
                            ElseIf std.Abs(pu.X - x1) < 0.1 AndAlso std.Abs(pu.Y - y1) < 0.1 Then
                                ' pu is [x1,y1]
                                If std.Abs(pv.X - x0) < 0.1 AndAlso std.Abs(pv.Y - y0) < 0.1 Then
                                    ' pv is [x0,y0]
                                    ' is a direct link between two node
                                    ' ignores
                                    Exit For
                                Else
                                    ' [x0,y0] is an edge turn point
                                    edge.data.bends = {WayPointVector.CreateVector(pu, pv, x0, y0)}
                                    Exit For
                                End If
                            Else
                                ' current edge is not a hit
                                ' just ignores
                            End If
                        Next
                    End If
                Next
            Next

            Return g
        End Function

        ''' <summary>
        ''' create orthographic layout
        ''' </summary>
        ''' <param name="graph"></param>
        ''' <param name="numberOfAttempts"></param>
        ''' <param name="optimize"></param>
        ''' <param name="simplify"></param>
        ''' <param name="fixNonOrthogonal"></param>
        ''' <returns></returns>
        <Extension>
        Public Function RunLayoutMatrix(ByRef graph As Integer()(),
                                        Optional numberOfAttempts As Integer = 10,
                                        Optional optimize As Boolean = True,
                                        Optional simplify As Boolean = True,
                                        Optional fixNonOrthogonal As Boolean = True) As OrthographicEmbeddingResult

            Dim disconnectedGraphSet As IList(Of IList(Of Integer)) = DisconnectedGraphs.findDisconnectedGraphs(graph)
            Dim disconnectedEmbeddings As New List(Of OrthographicEmbeddingResult)()
            Dim g_oe As OrthographicEmbeddingResult

            For Each nodeSubset In disconnectedGraphSet
                ' calculate the embedding:
                Dim best_g_oe As OrthographicEmbeddingResult = Nothing
                Dim g As Integer()() = DisconnectedGraphs.subgraph(graph, nodeSubset)
                Dim comparator As New SegmentLengthEmbeddingComparator()

                For attempt As Integer = 0 To numberOfAttempts - 1
                    g_oe = Nothing

                    Try
                        g_oe = OrthographicEmbedding.orthographicEmbedding(g, simplify, fixNonOrthogonal)
                    Catch ex As Exception
                        Continue For
                    End Try

                    If g_oe Is Nothing Then
                        Continue For
                    End If

                    If Not g_oe.sanityCheck(False) Then
                        Throw New Exception("The orthographic projection contains errors!")
                    End If

                    If optimize Then
                        g_oe = OrthographicEmbeddingOptimizer.optimize(g_oe, g, comparator)

                        If Not g_oe.sanityCheck(False) Then
                            Throw New Exception("The orthographic projection after optimization contains errors!")
                        End If
                    End If

                    If best_g_oe Is Nothing Then
                        best_g_oe = g_oe
                    Else
                        If comparator.compare(g_oe, best_g_oe) < 0 Then
                            best_g_oe = g_oe
                        End If
                    End If
                Next

                Call disconnectedEmbeddings.Add(best_g_oe)
            Next

            Return DisconnectedGraphs.mergeDisconnectedEmbeddingsSideBySide(disconnectedEmbeddings, disconnectedGraphSet, 1.0)
        End Function

    End Module
End Namespace
