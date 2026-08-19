#Region "Microsoft.VisualBasic::7a5c2e9f4b1d83067a5c2e9f4b1d8306, Data_science\Graph\Analysis\Community\LPA\Builder.vb"

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

    '   Total Lines: 130
    '    Code Lines: 90 (69.23%)
    ' Comment Lines: 20 (15.38%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 20 (15.38%)
    '     File Size: 5.10 KB


    '     Class Builder
    ' 
    '         Function: Load
    ' 
    '         Sub: addEdge
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphTheory.Network
Imports Microsoft.VisualBasic.Linq

Namespace Analysis.LPA

    ''' <summary>
    ''' 将<see cref="NetworkGraph"/>网络图对象构建为LPA标签传播算法
    ''' 所需要的链式前向星邻接表数据结构
    ''' </summary>
    Public Class Builder

        Friend Overridable Sub addEdge(ByRef lpa As LabelPropagation,
                                       u As Integer,
                                       v As Integer,
                                       weight As Double)

            If lpa.edge(lpa.top) Is Nothing Then
                lpa.edge(lpa.top) = New Edge()
            End If

            lpa.edge(lpa.top).v = v
            lpa.edge(lpa.top).weight = weight
            lpa.edge(lpa.top).next = lpa.head(u)
            lpa.head(u) = lpa.top
            lpa.top += 1
        End Sub

        ''' <summary>
        ''' 从网络图对象构建LPA标签传播算法对象
        ''' </summary>
        ''' <typeparam name="Node"></typeparam>
        ''' <typeparam name="Edge"></typeparam>
        ''' <param name="g">
        ''' 目标网络图，要求图之中的节点的ID编号为从0开始的连续编号
        ''' （与Louvain模块的要求相一致）
        ''' </param>
        ''' <param name="maxIterations">最大迭代次数上限，默认为100</param>
        ''' <returns>
        ''' 返回完成初始化之后的<see cref="LabelPropagation"/>算法对象，
        ''' 每个节点都会被分配得到一个独一无二的独立标签
        ''' </returns>
        ''' <remarks>
        ''' 假若网络图之中的所有的边的权重值都是零，则会被看作为无权图，
        ''' 所有的边的权重值都会被设置为1。
        ''' </remarks>
        Public Shared Function Load(Of Node As {New, Network.Node},
                                        Edge As {New, Network.Edge(Of Node)})(
            g As NetworkGraph(Of Node, Edge),
            Optional maxIterations As Integer = 100) As LabelPropagation

            Dim lpa As New LabelPropagation(maxIterations) With {
                .n = g.size.vertex,
                .m = g.size.edges * 2,
                .edge = New LPA.Edge(.m - 1) {},
                .head = New Integer(.n - 1) {},
                .label = New Integer(.n - 1) {},
                .nodeLabels = New String(.n - 1) {}
            }

            For i As Integer = 0 To lpa.n - 1
                lpa.head(i) = -1
                ' 初始时每个节点都拥有一个独一无二的独立标签
                lpa.label(i) = i
            Next

            ' 缓存节点ID下标所对应的节点名，用于最后的社区划分结果的输出
            For Each node As Node In g.vertex
                lpa.nodeLabels(node.ID) = node.label
            Next

            Dim builder As New Builder
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

                ' 社区发现一般针对于无向图，所以在这里进行双向插边
                Call builder.addEdge(lpa, u, v, curw)
                Call builder.addEdge(lpa, v, u, curw)
            Next

            Return lpa
        End Function
    End Class
End Namespace
