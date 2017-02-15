
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Linq

Namespace KMeans

    Public Module BuildNodeTrees

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="net">使用边对象的数据来构建整颗树</param>
        ''' <returns></returns>
        ''' <remarks>请注意：在这里面边是具有方向的，from到to</remarks>
        <Extension> Public Function BuildTree(net As Network) As EntityNode
            Dim nodesTable As Dictionary(Of String, NetworkEdge()) =
                net.Edges _
                .GroupBy(Function(edge) edge.FromNode) _
                .ToDictionary(Function(k) k.Key,
                              Function(c) c.ToArray)
            ' 从ROOT开始构建
            With New EntityNode(Tree.ROOT)
                Call .MySelf.__appendChilds(nodesTable)
                Return .MySelf
            End With
        End Function

        <Extension> Private Sub __appendChilds(ByRef parent As EntityNode, nodesTable As Dictionary(Of String, NetworkEdge()))
            If Not nodesTable.ContainsKey(parent.Name) Then
                ' 由于键值都是key，所以对于leaf而言是找不到的
                ' 已经没有子节点了，在这里直接退出递归
                Return
            End If

            Dim childs As NetworkEdge() = nodesTable(parent.Name)

            ' 遍历下一个连接的节点，并且进行递归构建出整个树
            For Each branch As NetworkEdge In childs
                With New EntityNode(branch.ToNode)
                    Call .MySelf.__appendChilds(nodesTable)
                    Call parent.AddChild(.MySelf)
                End With
            Next
        End Sub
    End Module
End Namespace