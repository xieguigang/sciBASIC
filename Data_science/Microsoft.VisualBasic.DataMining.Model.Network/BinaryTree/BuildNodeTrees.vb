
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Linq

Namespace KMeans

    Public Module BuildNodeTrees

        Delegate Sub TreeTraveller(ByRef parent As EntityNode)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="net">使用边对象的数据来构建整颗树</param>
        ''' <returns></returns>
        ''' <remarks>请注意：在这里面边是具有方向的，from到to</remarks>
        <Extension> Public Function BuildTree(net As Network) As EntityNode
            Dim nodes As Dictionary(Of String, NetworkEdge()) =
                net.Edges _
                .GroupBy(Function(edge) edge.FromNode) _
                .ToDictionary(Function(k) k.Key,
                              Function(c) c.ToArray)
            ' 从ROOT开始构建
            Dim root As New EntityNode(Tree.ROOT)
            Dim appendChilds As New TreeTraveller(
                Sub(ByRef parent As EntityNode)

                    Dim childs As IEnumerable(Of NetworkEdge) =
                        nodes(Tree.ROOT) _
                        .Select(Function(id) nodes(id.FromNode)) _
                        .IteratesALL

                    ' 遍历下一个连接的节点，并且进行递归构建出整个树
                    For Each branch As NetworkEdge In childs
                        With New EntityNode(branch.FromNode)
                            Call appendChilds(.MySelf)
                            Call parent.AddChild(.MySelf)
                        End With
                    Next
                End Sub)

            Call appendChilds(root)

            Return root
        End Function
    End Module
End Namespace