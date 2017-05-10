Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports NetGraph = Microsoft.VisualBasic.Data.visualize.Network.FileStream.Network

Namespace Analysis

    Public Module Statistics

        ''' <summary>
        ''' 获取得到网络之中的每一个节点的Degree度
        ''' </summary>
        ''' <param name="net"></param>
        ''' <returns></returns>
        <Extension> Public Function GetDegrees(net As NetGraph) As Dictionary(Of String, Integer)
            Dim degree As New Dictionary(Of String, Integer)
            Dim counts As Action(Of String) =
 _
                Sub(node$) _
 _
                    If degree.ContainsKey(node) Then _
                        degree(node) += 1 _
                    Else _
                        Call degree.Add(node, 1)

            For Each edge As NetworkEdge In net.Edges
                Call counts(edge.FromNode)
                Call counts(edge.ToNode)
            Next

            Return degree
        End Function

        ''' <summary>
        ''' 统计网络之中的每一种类型的节点的数量
        ''' </summary>
        ''' <param name="net"></param>
        ''' <returns></returns>
        <Extension>
        Public Function NodesGroupCount(net As NetGraph) As Dictionary(Of String, Integer)
            Return net.Nodes _
                .GroupBy(Function(n) n.NodeType) _
                .ToDictionary(Function(x) x.Key, 
                              Function(c) c.Count)
        End Function
    End Module
End Namespace
