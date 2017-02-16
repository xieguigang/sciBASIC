
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.Quantile

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
            Dim nodeTypes As Dictionary(Of String, String) = net.Nodes _
                .ToDictionary(Function(n) n.ID,
                              Function(n) n.NodeType)
            ' 从ROOT开始构建
            With New EntityNode(Tree.ROOT, nodeTypes(Tree.ROOT))
                Call .MySelf.__appendChilds(nodesTable, nodeTypes)
                Return .MySelf
            End With
        End Function

        <Extension> Private Sub __appendChilds(ByRef parent As EntityNode,
                                               nodesTable As Dictionary(Of String, NetworkEdge()),
                                               nodeTypes As Dictionary(Of String, String))

            If Not nodesTable.ContainsKey(parent.EntityID) Then
                ' 由于键值都是key，所以对于leaf而言是找不到的
                ' 已经没有子节点了，在这里直接退出递归
                Return
            End If

            Dim childs As NetworkEdge() = nodesTable(parent.EntityID)

            ' 遍历下一个连接的节点，并且进行递归构建出整个树
            For Each branch As NetworkEdge In childs
                With New EntityNode(branch.ToNode, nodeTypes(branch.ToNode))
                    Call .MySelf.__appendChilds(nodesTable, nodeTypes)
                    Call parent.AddChild(.MySelf)
                End With
            Next
        End Sub

        ReadOnly __chars As Char() = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray

        <Extension>
        Public Function PartionTable(parts As IEnumerable(Of Partition)) As Dictionary(Of String, EntityLDM())
            Dim g = parts.GroupBy(Function(p) p.Tag)
            Dim out As New Dictionary(Of String, EntityLDM())

            For Each part As IGrouping(Of String, Partition) In g
                If part.Count > 1 Then
                    Dim i As int = 0

                    For Each [sub] As Partition In part
                        Call out.Add(part.Key & $"-{__chars(++i)}", [sub].members)
                    Next
                Else
                    Call out.Add(part.Key, part.First.members)
                End If
            Next

            Return out
        End Function

        ''' <summary>
        ''' 在进行分区的时候，分支最少的路径会被切割下来，分支多的路径会继续访问直到没有path路径为止
        ''' </summary>
        ''' <param name="tree"></param>
        ''' <param name="min">分区集合之中的最小节点数，为quantile百分比值</param>
        ''' <returns></returns>
        <Extension> Public Iterator Function CutTrees(tree As EntityNode, Optional min# = 0.99) As IEnumerable(Of Partition)
            ' 为了提高计算效率，在这里首先生成每一个分支节点的子节点数的缓存
            Dim childsDistribute As New Dictionary(Of String, Double)

            Call tree.ChildCountsTravel(
                childsDistribute,
                getID:=Function(x) x.EntityID)
            Call childsDistribute.SortByKey(desc:=True)

            Dim q As Integer = childsDistribute _
                .Values _
                .QuantileThreshold(quantile:=min)

            For Each child As EntityNode In tree.ChildNodes
                For Each part As Partition In child.__cutTrees(childsDistribute, min:=q)
                    If part.members.Length > 0 Then
                        Yield part
                    End If
                Next
            Next
        End Function

        <Extension>
        Private Iterator Function __cutTrees(tree As EntityNode, childsDistribute As Dictionary(Of String, Double), min%) As IEnumerable(Of Partition)
            Dim part = Function(cut As EntityNode)
                           Dim allChilds As EntityNode() = cut _
                               .Where(Function(c) c.Type = EntityType) _
                               .ToArray
                           Return New Partition() With {
                                .Tag = cut.FullyQualifiedName,
                                .uids = allChilds.ToArray(Function(x) x.EntityID),
                                .members = allChilds.ToArray(
                                    Function(x) New EntityLDM With {
                                        .Name = x.EntityID,
                                        .Cluster = x.FullyQualifiedName
                                    })
                           }
                       End Function

            ' 如果所有的子节点数小于阈值，就作为一个分区cut下来
            If childsDistribute(tree.EntityID) <= min Then
                Yield part(cut:=tree)
                Return
            End If

            ' 第一个节点是分支数最少的，则会被cut掉
            Dim childSorts As EntityNode() = tree.ChildNodes _
                .OrderBy(Function(n) childsDistribute(n.EntityID)) _
                .ToArray
            Dim cutResult As EntityNode = childSorts.First

            ' yield cutresult
            Yield part(cutResult)

            For Each branch As EntityNode In childSorts.Skip(1)
                For Each cut In branch.__cutTrees(childsDistribute, min)
                    Yield cut
                Next
            Next
        End Function
    End Module
End Namespace