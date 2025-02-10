#Region "Microsoft.VisualBasic::eebccc4ba090d822c53aa53aadf2f32b, Data_science\Visualization\Visualization\BinaryTree\NodeTrees.vb"

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

    '   Total Lines: 153
    '    Code Lines: 109 (71.24%)
    ' Comment Lines: 22 (14.38%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 22 (14.38%)
    '     File Size: 6.95 KB


    '     Module NodeTrees
    ' 
    '         Function: __cutTrees, BuildTree, CutTrees, PartionTable
    ' 
    '         Sub: __appendChilds
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Quantile

Namespace KMeans

    Public Module NodeTrees

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="net">使用边对象的数据来构建整颗树</param>
        ''' <returns></returns>
        ''' <remarks>请注意：在这里面边是具有方向的，from到to</remarks>
        <Extension> Public Function BuildTree(net As NetworkTables) As EntityNode
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
        Public Function PartionTable(parts As IEnumerable(Of Partition)) As Dictionary(Of String, EntityClusterModel())
            Dim g = parts.GroupBy(Function(p) p.Tag)
            Dim out As New Dictionary(Of String, EntityClusterModel())

            For Each part As IGrouping(Of String, Partition) In g
                If part.Count > 1 Then
                    Dim i As i32 = 0

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

            Dim q# = childsDistribute _
                .Values _
                .Threshold(quantile:=min)

            For Each child As EntityNode In tree.ChildNodes
                For Each part As Partition In child.__cutTrees(childsDistribute, min:=CInt(q))
                    If part.members.Length > 0 Then
                        Yield part
                    End If
                Next
            Next
        End Function

        <Extension>
        Private Iterator Function __cutTrees(tree As EntityNode, childsDistribute As Dictionary(Of String, Double), min%) As IEnumerable(Of Partition)
            Dim part = Function(cut As EntityNode)
                           Dim allChilds As EntityNode() = cut.AsEnumerable _
                               .Where(Function(c) c.Type = EntityType) _
                               .ToArray

                           Return New Partition() With {
                                .Tag = cut.FullyQualifiedName,
                                .uids = allChilds.Select(Function(x) x.EntityID),
                                .members = allChilds.Select(
                                    Function(x) New EntityClusterModel With {
                                        .ID = x.EntityID,
                                        .Cluster = x.FullyQualifiedName
                                    }).ToArray
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
            Dim skipn% = 1   ' 跳过第一个分组

            ' yield cutresult
            ' 2017-2-16 直接将最小的集合切分去除可能会导致出现大片的区域被标注为相同的颜色，
            ' 则在这里会首先判断是否符合阈值， 不符合阈值的话才会被切割出去
            If childsDistribute(cutResult.EntityID) <= min Then
                Yield part(cutResult)
            Else
                skipn = 0   ' 集合的大小任然符合阈值要求，不会被跳过排除
            End If

            For Each branch As EntityNode In childSorts.Skip(skipn)
                For Each cut In branch.__cutTrees(childsDistribute, min)
                    Yield cut
                Next
            Next
        End Function
    End Module
End Namespace
