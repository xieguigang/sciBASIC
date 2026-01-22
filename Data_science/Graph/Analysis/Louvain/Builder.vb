#Region "Microsoft.VisualBasic::25b23436fc2946f08a6563a0a68d9f6f, Data_science\Graph\Analysis\Louvain\Builder.vb"

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

    '   Total Lines: 111
    '    Code Lines: 86 (77.48%)
    ' Comment Lines: 3 (2.70%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 22 (19.82%)
    '     File Size: 4.47 KB


    '     Class Builder
    ' 
    '         Function: Load
    ' 
    '         Sub: addEdge, addGlobalEdge, loadGraphMatrix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphTheory.Network
Imports Microsoft.VisualBasic.Linq

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
            global_head(u) = global_top
            global_top += 1
        End Sub

        Friend Overridable Sub addEdge(ByRef louvain As LouvainCommunity,
                                       u As Integer,
                                       v As Integer,
                                       weight As Double)

            If louvain.edge(louvain.top) Is Nothing Then
                louvain.edge(louvain.top) = New Edge()
            End If

            louvain.edge(louvain.top).v = v
            louvain.edge(louvain.top).weight = weight
            louvain.edge(louvain.top).next = louvain.head(u)
            louvain.head(u) = louvain.top
            louvain.top += 1
        End Sub

        Public Shared Function Load(Of Node As {New, Network.Node},
                                       Edge As {New, Network.Edge(Of Node)})(g As NetworkGraph(Of Node, Edge), Optional eps As Double = 0.00000001) As LouvainCommunity

            Dim louvain As New LouvainCommunity(eps:=eps) With {
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

            For i As Integer = 0 To louvain.n - 1
                builder.global_head(i) = -1
            Next

            louvain.global_cluster = New Integer(louvain.n - 1) {}

            For i As Integer = 0 To louvain.global_n - 1
                louvain.global_cluster(i) = i
            Next

            louvain.node_weight = New Double(louvain.n - 1) {}
            louvain.totalEdgeWeight = 0.0
            louvain.DoCall(Sub(alg) loadGraphMatrix(alg, builder, g))
            louvain.resolution = 1 / louvain.totalEdgeWeight

            Return louvain
        End Function

        Private Shared Sub loadGraphMatrix(Of Node As {New, Network.Node},
                                              Edge As {New, Network.Edge(Of Node)})(ByRef louvain As LouvainCommunity,
                                                                                    builder As Builder,
                                                                                    g As NetworkGraph(Of Node, Edge))

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
