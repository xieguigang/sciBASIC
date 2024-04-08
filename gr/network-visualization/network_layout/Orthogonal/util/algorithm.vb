Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.EdgeBundling
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Orthogonal.optimization
Imports Microsoft.VisualBasic.Imaging.Math2D
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

            For i As Integer = 0 To layout.nodeIndexes.Length - 1
                index = layout.nodeIndexes(i)

                If index > -1 Then
                    v = vlist(index)
                    v.data.initialPostion = New FDGVector2(layout(i))
                End If
            Next

            Dim minx = layout.x.Min
            Dim miny = layout.y.Min
            Dim maxx = layout.x.Max
            Dim maxy = layout.y.Max
            Dim edgeBends As New Dictionary(Of String, List(Of XYMetaHandle))

            For i = 0 To layout.nodeIndexes.Length - 1
                For j = 0 To layout.nodeIndexes.Length - 1
                    If layout.edges(i)(j) Then
                        Dim x0 = std.Min(layout.x(i), layout.x(j)) - minx
                        Dim y0 = std.Min(layout.y(i), layout.y(j)) - miny

                        u = vlist(layout.nodeIndexes(i))
                        v = vlist(layout.nodeIndexes(j))

                        Dim key As String = {u.ID, v.ID}.OrderBy(Function(vi) vi).JoinBy(" -> ")
                        Dim ps As PointF = layout(i)
                        Dim pt As PointF = layout(j)

                        If Not edgeBends.ContainsKey(key) Then
                            edgeBends.Add(key, New List(Of XYMetaHandle))
                        End If

                        edgeBends(key).Add(XYMetaHandle.CreateVector(ps, pt, x0, y0))
                    End If
                Next
            Next

            For Each edge As Edge In g.graphEdges
                Dim key As String = {edge.U.ID, edge.V.ID}.OrderBy(Function(vi) vi).JoinBy(" -> ")

                If Not edgeBends.ContainsKey(key) Then
                    Continue For
                End If

                Dim ps As PointF = edge.U.data.initialPostion.Point2D
                Dim bends As XYMetaHandle() = edgeBends(key) _
                    .OrderBy(Function(e)
                                 Return e.GetPoint(edge.U, edge.V).Distance(ps)
                             End Function) _
                    .ToArray

                edge.data.bends = bends
            Next

            Return g
        End Function

        <Extension>
        Public Function RunLayoutMatrix(ByRef graph As Integer()(),
                                        Optional numberOfAttempts As Integer = 10,
                                        Optional optimize As Boolean = True,
                                        Optional simplify As Boolean = True,
                                        Optional fixNonOrthogonal As Boolean = True) As OrthographicEmbeddingResult

            Dim disconnectedGraphSet As IList(Of IList(Of Integer)) = DisconnectedGraphs.findDisconnectedGraphs(graph)
            Dim disconnectedEmbeddings As New List(Of OrthographicEmbeddingResult)()

            For Each nodeSubset In disconnectedGraphSet
                ' calculate the embedding:
                Dim best_g_oe As OrthographicEmbeddingResult = Nothing
                Dim g As Integer()() = DisconnectedGraphs.subgraph(graph, nodeSubset)
                Dim comparator As New SegmentLengthEmbeddingComparator()

                For attempt As Integer = 0 To numberOfAttempts - 1
                    Dim g_oe As OrthographicEmbeddingResult = OrthographicEmbedding.orthographicEmbedding(g, simplify, fixNonOrthogonal)

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