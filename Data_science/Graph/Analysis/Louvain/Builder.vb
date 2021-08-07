Imports Microsoft.VisualBasic.Data.GraphTheory.Network
Imports Microsoft.VisualBasic.Linq
Imports stdNum = System.Math

Namespace Analysis.Louvain

    Public Class Builder

        ''' <summary>
        ''' 全局初始的临接表  只保存一次，永久不变，不参与后期运算
        ''' </summary>
        Friend global_edge As Edge()
        Friend global_head As Integer()
        Friend global_top As Integer = 0

        Friend Overridable Sub addGlobalEdge(u As Integer, v As Integer, weight As Double)
            If global_edge(global_top) Is Nothing Then
                global_edge(global_top) = New Edge()
            End If

            global_edge(global_top).v = v
            global_edge(global_top).weight = weight
            global_edge(global_top).next = global_head(u)
            global_head(u) = stdNum.Min(Threading.Interlocked.Increment(global_top), global_top - 1)
        End Sub

        Friend Overridable Sub addEdge(ByRef louvain As LouvainCommunity, u As Integer, v As Integer, weight As Double)
            If louvain.edge(louvain.top) Is Nothing Then
                louvain.edge(louvain.top) = New Edge()
            End If

            louvain.edge(louvain.top).v = v
            louvain.edge(louvain.top).weight = weight
            louvain.edge(louvain.top).next = louvain.head(u)
            louvain.head(u) = stdNum.Min(Threading.Interlocked.Increment(louvain.top), louvain.top - 1)
        End Sub

        Public Shared Function Load(Of Node As {New, Network.Node}, Edge As {New, Network.Edge(Of Node)})(g As NetworkGraph(Of Node, Edge)) As LouvainCommunity
            Dim louvain As New LouvainCommunity With {
                .n = g.size.vertex,
                .global_n = .n,
                .m = g.size.edges * 2,
                .edge = New Louvain.Edge(.m - 1) {},
                .head = New Integer(.n - 1) {}
            }
            Dim builder As New Builder

            For i As Integer = 0 To louvain.n - 1
                louvain.head(i) = -1
            Next

            louvain.top = 0
            builder.global_edge = New Louvain.Edge(louvain.m - 1) {}
            builder.global_head = New Integer(louvain.n - 1) {}

            For i = 0 To louvain.n - 1
                builder.global_head(i) = -1
            Next

            louvain.global_cluster = New Integer(louvain.n - 1) {}

            For i = 0 To louvain.global_n - 1
                louvain.global_cluster(i) = i
            Next

            louvain.node_weight = New Double(louvain.n - 1) {}
            louvain.totalEdgeWeight = 0.0
            louvain.DoCall(Sub(alg) loadGraphMatrix(alg, builder, g))
            louvain.resolution = 1 / louvain.totalEdgeWeight

            Return louvain
        End Function

        Private Shared Sub loadGraphMatrix(Of Node As {New, Network.Node}, Edge As {New, Network.Edge(Of Node)})(ByRef louvain As LouvainCommunity, builder As Builder, g As NetworkGraph(Of Node, Edge))
            Dim hasWeight As Boolean = g.graphEdges.Any(Function(l) l.weight <> 0.0)

            For Each link As Edge In g.graphEdges
                Dim u = link.U.ID
                Dim v = link.V.ID
                Dim curw As Double

                If hasWeight Then
                    curw = link.weight
                Else
                    curw = 1.0
                End If

                Call builder.addEdge(louvain, u, v, curw)
                Call builder.addEdge(louvain, v, u, curw)
                Call builder.addGlobalEdge(u, v, curw)
                Call builder.addGlobalEdge(v, u, curw)

                louvain.totalEdgeWeight += 2 * curw
                louvain.node_weight(u) += curw

                If u <> v Then
                    louvain.node_weight(v) += curw
                End If
            Next
        End Sub
    End Class
End Namespace