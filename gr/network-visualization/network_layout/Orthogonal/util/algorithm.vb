Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Orthogonal.optimization

Namespace Orthogonal

    Public Module algorithm

        <Extension>
        Public Function AsGraphMatrix(g As NetworkGraph) As Integer()()
            Dim vlist As Node() = g.vertex.ToArray
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

                    If g.GetEdges(v, node).Any Then
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

            Dim layout As OrthographicEmbeddingResult = g _
                .AsGraphMatrix _
                .RunLayoutMatrix(
                    numberOfAttempts:=numberOfAttempts,
                    optimize:=optimize,
                    simplify:=simplify,
                    fixNonOrthogonal:=fixNonOrthogonal)

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

                For attempt = 0 To numberOfAttempts - 1
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