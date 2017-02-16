
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Linq

Namespace KMeans

    Public Module NodeTrees

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
            If Not nodesTable.ContainsKey(parent.EntityID) Then
                ' 由于键值都是key，所以对于leaf而言是找不到的
                ' 已经没有子节点了，在这里直接退出递归
                Return
            End If

            Dim childs As NetworkEdge() = nodesTable(parent.EntityID)

            ' 遍历下一个连接的节点，并且进行递归构建出整个树
            For Each branch As NetworkEdge In childs
                With New EntityNode(branch.ToNode)
                    Call .MySelf.__appendChilds(nodesTable)
                    Call parent.AddChild(.MySelf)
                End With
            Next
        End Sub

        ''' <summary>
        ''' 在进行分区的时候，分支少的路径会被切割下来，分支多的路径会继续访问直到没有path路径为止
        ''' </summary>
        ''' <param name="tree"></param>
        ''' <returns></returns>
        <Extension> Public Iterator Function CutTrees(tree As EntityNode) As IEnumerable(Of Partition)
            ' 为了提高计算效率，在这里首先生成每一个分支节点的子节点数的缓存
            Dim childsDistribute As New Dictionary(Of String, Double)
            Call tree.ChildCountsTravel(
                childsDistribute,
                getID:=Function(x) x.EntityID)
            Call childsDistribute.SortByKey(desc:=True)


        End Function
    End Module
End Namespace