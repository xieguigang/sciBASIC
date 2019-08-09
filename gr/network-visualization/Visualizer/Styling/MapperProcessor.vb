Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Styling.CSS

Namespace Styling

    ''' <summary>
    ''' Do style mapping from the parsed css file at here
    ''' </summary>
    Module MapperProcessor

        <Extension>
        Public Function WritePropertyValue(g As NetworkGraph, styles As StyleMapper) As NetworkGraph

        End Function

        ''' <summary>
        ''' 因为在这个模块之中会涉及到修改原来的属性值
        ''' 所以为了不影响原始的数据,
        ''' 在这里必须要在新建的复制副本上来完成操作
        ''' </summary>
        ''' <param name="graph"></param>
        ''' <returns></returns>
        Private Function copy(graph As NetworkGraph) As NetworkGraph
            Dim nodes As Node() = graph.vertex.Select(Function(n) n.Clone).ToArray
            Dim edges As Edge() = graph.graphEdges _
                .Select(Function(e) e.Clone) _
                .ToArray

            ' 因为edge中的node对象还是原来的node对象
            ' 所以在这里会需要进行重新赋值来解除对原始数据的引用
            With nodes.ToDictionary(Function(n) n.label)
                edges.DoEach(Sub(edge)
                                 ' use the clone data
                                 edge.U = .Item(edge.U.label)
                                 edge.V = .Item(edge.V.label)
                             End Sub)
            End With

            Return New NetworkGraph(nodes, edges)
        End Function
    End Module
End Namespace