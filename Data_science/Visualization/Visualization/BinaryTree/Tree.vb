#Region "Microsoft.VisualBasic::36cda9d966282aa9ed688b5de426cfcc, Data_science\Visualization\Visualization\BinaryTree\Tree.vb"

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

    '   Total Lines: 212
    '    Code Lines: 158
    ' Comment Lines: 21
    '   Blank Lines: 33
    '     File Size: 8.09 KB


    '     Module Tree
    ' 
    '         Function: __buildNET, bTreeNET, Partitioning
    '         Structure __edgePath
    ' 
    '             Function: ToString
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports stdNum = System.Math

Namespace KMeans

    ''' <summary>
    ''' KMeans.Tree.NET
    ''' </summary>
    <Package("KMeans.Tree.NET",
                      Category:=APICategories.ResearchTools,
                      Publisher:="smrucc@gcmodeller.org")>
    Public Module Tree

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="cluster"></param>
        ''' <param name="depth">将会以最短的聚类作为数据分区的深度</param>
        ''' <returns></returns>
        <Extension>
        Public Function Partitioning(cluster As IEnumerable(Of EntityClusterModel), Optional depth As Integer = -1, Optional trim As Boolean = True) As List(Of Partition)
            Dim list As New List(Of EntityClusterModel)(cluster)

            If depth <= 0 Then
                depth = (From x As EntityClusterModel
                         In list
                         Select l = x.Cluster
                         Order By l.Length Ascending).First.Split("."c).Length
            End If

            Dim clusters As New List(Of String)({""})

            For i As Integer = 0 To depth - 1
                Dim temp As New List(Of String)

                For Each x As String In clusters
                    temp += x & ".1"
                    temp += x & ".2"
                Next

                clusters = temp
            Next

            For i As Integer = 0 To clusters.Count - 1   ' 去掉最开始的小数点
                clusters(i) = Mid(clusters(i), 2)
            Next

            Dim partitions As New List(Of Partition)

            For Each tag As String In clusters
                Dim LQuery As EntityClusterModel() =
                    LinqAPI.Exec(Of EntityClusterModel) <=
 _
                    From x As EntityClusterModel
                    In list.AsParallel
                    Where InStr(x.Cluster, tag, CompareMethod.Binary) = 1
                    Select x

                list -= LQuery
                partitions += New Partition With {
                    .Tag = tag,
                    .uids = LQuery.Select(Function(x) x.ID),
                    .members = LQuery
                }
            Next

            If Not list.IsNullOrEmpty Then
                partitions += New Partition With {
                    .Tag = "Unclass",
                    .uids = list.Select(Function(x) x.ID),
                    .members = list.ToArray
                }
            End If

            If trim Then
                partitions = New List(Of Partition)(
                    partitions.Where(Function(x) x.NumOfEntity > 0))
            End If

            Return partitions
        End Function

        Private Structure __edgePath
            Public path As String()
            Public node As EntityClusterModel

            Public Overrides Function ToString() As String
                Return $"[{node.Cluster}] --> {node.ID}"
            End Function
        End Structure

        Public Const ROOT$ = NameOf(ROOT)
        Public Const EntityType$ = "Entity"

        ''' <summary>
        ''' Create network model for visualize the binary tree clustering result.(将树形聚类的结果转换为网络文件)
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Function bTreeNET(source As IEnumerable(Of EntityClusterModel), Optional removesProperty As Boolean = True) As FileStream.NetworkTables
            Dim array = (From x As EntityClusterModel
                         In source
                         Let path As String() = x.Cluster.Split("."c)
                         Select New __edgePath With {
                             .node = x,
                             .path = path
                         }).ToArray
            Dim nodes As List(Of Node) = array.Select(
                Function(x) As Node
                    Dim props As Dictionary(Of String, String)

                    If removesProperty Then
                        props = New Dictionary(Of String, String)
                    Else
                        props = x.node _
                            .Properties _
                            .ToDictionary(Function(xx) xx.Key,
                                          Function(xx)
                                              Return CStr(stdNum.Round(xx.Value, 4))
                                          End Function)
                    End If

                    Return New FileStream.Node With {
                        .ID = x.node.ID,
                        .NodeType = Tree.EntityType,
                        .Properties = props
                    }
                End Function).AsList

            nodes += New FileStream.Node With {
                .ID = ROOT,
                .NodeType = ROOT
            }

            Dim edges = __buildNET(
                array,
                nodes ^ Function(x As Node) String.Equals(x.NodeType, ROOT),
                Scan0,
                nodes)

            Return New FileStream.NetworkTables With {
                .edges = edges,
                .nodes = nodes.ToArray
            }
        End Function

        ''' <summary>
        ''' 从某一个分支点下来
        ''' </summary>
        ''' <param name="array"></param>
        ''' <param name="depth"></param>
        ''' <param name="nodes"></param>
        ''' <returns></returns>
        Private Function __buildNET(array As __edgePath(),
                                    parent As FileStream.Node,
                                    depth As Integer,
                                    ByRef nodes As List(Of FileStream.Node)) As NetworkEdge()

            Dim [next] As Integer = depth + 1  ' 下一层节点的深度

            If depth = array(Scan0).path.Length AndAlso
                array(Scan0).path.Last = "X"c Then

                Return array.Select(
                    Function(x) New NetworkEdge With {
                        .FromNode = parent.ID,
                        .ToNode = x.node.ID,
                        .Interaction = "Leaf-X"
                    }).ToArray
            End If

            Dim edges As New List(Of NetworkEdge)
            Dim groups = (From x As __edgePath
                          In array
                          Let cur = x.path(depth)
                          Select cur, x
                          Group By cur Into Group).ToArray

            For Each part In groups
                Dim parts = part.Group.Select(Function(x) x.x).ToArray

                If parts.Length = 1 Then ' 叶节点
                    Dim leaf As __edgePath = parts.First

                    edges += New NetworkEdge With {
                        .FromNode = parent.ID,
                        .ToNode = leaf.node.ID,
                        .Interaction = "Leaf"
                    }
                Else     ' 继续递归
                    Dim uid As String = $"[{part.cur}]" & parts.First.path.Take(depth).JoinBy(".")
                    Dim virtual As New FileStream.Node With {
                        .ID = uid,
                        .NodeType = "Virtual"
                    }
                    Call nodes.Add(virtual)
                    Call edges.Add(New NetworkEdge With {.FromNode = parent.ID, .ToNode = uid, .Interaction = "Path"})
                    Call edges.Add(__buildNET(parts, virtual, [next], nodes))
                End If
            Next

            Return edges.ToArray
        End Function
    End Module
End Namespace
