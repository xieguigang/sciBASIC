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

            For Each edge As Edge In g.graphEdges
                Dim pu = edge.U.data.initialPostion.Point2D
                Dim pv = edge.V.data.initialPostion.Point2D

                If std.Abs(pu.X - pv.X) < 0.01 OrElse std.Abs(pu.Y - pv.Y) < 0.01 Then
                    ' u - v on a line
                    ' no bends
                Else
                    ' needs a middle point in route
                    If layout.find(pu.X, pv.Y) Then
                        edge.data.bends = {XYMetaHandle.CreateVector(pu, pv, pu.X, pv.Y)}
                    ElseIf layout.find(pv.X, pu.Y) Then
                        edge.data.bends = {XYMetaHandle.CreateVector(pu, pv, pv.X, pu.Y)}
                    Else
                        ' no bends?
                    End If
                End If
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