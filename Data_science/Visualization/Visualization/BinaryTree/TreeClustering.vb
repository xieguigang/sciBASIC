#Region "Microsoft.VisualBasic::603268f6e84fac891de4124de70602f3, sciBASIC#\Data_science\Visualization\Visualization\BinaryTree\TreeClustering.vb"

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

    '   Total Lines: 211
    '    Code Lines: 138
    ' Comment Lines: 44
    '   Blank Lines: 29
    '     File Size: 9.59 KB


    '     Module TreeClustering
    ' 
    '         Function: __firstCluster, __rootCluster, __treeCluster, (+3 Overloads) TreeCluster
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Parallel.Tasks

Namespace KMeans

    Public Module TreeClustering

        ''' <summary>
        ''' 二叉树树形聚类，请注意，所输入的数据的名字不可以一样，不然无法正确生成cluster标记
        ''' </summary>
        ''' <param name="resultSet"></param>
        ''' <param name="stop">Max iteration number for the kmeans kernel</param>
        ''' <returns></returns>
        ''' <param name="parallelDepth">-1表示不会限制并行的深度</param>
        <ExportAPI("Cluster.Trees")>
        <Extension> Public Function TreeCluster(resultSet As IEnumerable(Of EntityClusterModel),
                                            Optional parallel As Boolean = False,
                                            Optional [stop] As Integer = -1,
                                            Optional parallelDepth% = 3) As EntityClusterModel()

            Dim source As EntityClusterModel() = resultSet.ToArray
            Dim mapNames As String() = source(Scan0).Properties.Keys.ToArray   ' 得到所有属性的名称
            Dim ds As ClusterEntity() = source.Select(
                Function(x) New ClusterEntity With {
                    .uid = x.ID,
                    .entityVector = mapNames.Select(Function(s) x.Properties(s))
                }).ToArray  ' 在这里生成计算模型
            Dim tree As ClusterEntity() = TreeCluster(ds, parallel, [stop], parallelDepth)   ' 二叉树聚类操作
            Dim saveResult As EntityClusterModel() = tree.Select(Function(x) x.ToDataModel(mapNames))   ' 重新生成回数据模型

            For Each name As String In source.Select(Function(x) x.ID)
                For Each x As EntityClusterModel In saveResult
                    If InStr(x.ID, name) > 0 Then
                        x.Cluster = x.ID.Replace(name & ".", "")
                        x.ID = name
                        Exit For
                    End If
                Next
            Next

            Return saveResult
        End Function

        ''' <summary>
        ''' 二叉树聚类的路径会在<see cref="ClusterEntity.uid"/>上面出现
        ''' </summary>
        ''' <param name="source">函数会在这里自动调用ToArray方法结束Linq查询</param>
        ''' <param name="parallel"></param>
        ''' <param name="stop">Max iteration number for the kmeans kernel</param>
        ''' <returns></returns>
        <ExportAPI("Cluster.Trees")>
        <Extension> Public Function TreeCluster(source As IEnumerable(Of ClusterEntity),
                                            Optional parallel As Boolean = False,
                                            Optional [stop] As Integer = -1,
                                            Optional parallelDepth% = 3) As ClusterEntity()
            Return TreeCluster(Of ClusterEntity)(
                source.ToArray,
                parallel,
                [stop],
                parallelDepth)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="parallel"></param>
        ''' <param name="[stop]"></param>
        ''' <param name="parallelDepth%">-1表示不会限制，但是0表示只会是第一层为并行计算模式</param>
        ''' <returns></returns>
        Public Function TreeCluster(Of T As ClusterEntity)(source As IEnumerable(Of T),
                                                Optional parallel As Boolean = False,
                                                Optional [stop] As Integer = -1,
                                                Optional parallelDepth% = 3) As ClusterEntity()
            If parallelDepth <= -1 Then
                parallelDepth = Integer.MaxValue
            End If

            If parallel Then
                Return __firstCluster(
                    source,
                    [stop]:=CInt(source.Count / 2),
                    kmeansStop:=[stop],
                    parallelDepth:=parallelDepth)
            Else
                Return __treeCluster(
                    source,
                    Scan0,
                    CInt(source.Count / 2),
                    kmeansStop:=[stop],
                    parallelDepth:=parallelDepth)
            End If
        End Function

        ''' <summary>
        ''' 两条线程并行化进行二叉树聚类
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="[stop]"></param>
        ''' <returns></returns>
        Private Function __firstCluster(Of T As ClusterEntity)(source As IEnumerable(Of T), [stop] As Integer, kmeansStop As Integer, parallelDepth%) As ClusterEntity()
            Dim result As KMeansCluster(Of T)() = source.ClusterDataSet(2, debug:=True, [stop]:=kmeansStop).ToArray
            ' 假设在刚开始不会出现为零的情况
            Dim cluster1 As AsyncHandle(Of ClusterEntity()) =
                New AsyncHandle(Of ClusterEntity())(Function() __rootCluster(result(0), "1", [stop], kmeansStop, parallelDepth)).Run    ' cluster1
            Dim list As List(Of ClusterEntity) = New List(Of ClusterEntity) + __rootCluster(result(1), "2", [stop], kmeansStop, parallelDepth) ' cluster2
            list += cluster1.GetValue
            Return list.ToArray
        End Function

        Private Function __rootCluster(Of T As ClusterEntity)(cluster As KMeansCluster(Of T), id As String, [stop] As Integer, kmeansStop As Integer, parallelDepth%) As ClusterEntity()
            For Each x In cluster
                x.uid &= ("." & id)
            Next

            If cluster.NumOfEntity <= 1 Then
                Return cluster.ToArray
            Else
                Return __treeCluster(cluster.ToArray, Scan0, [stop], kmeansStop, parallelDepth)  ' 递归聚类分解
            End If
        End Function

        Private Function __treeCluster(Of T As ClusterEntity)(source As IEnumerable(Of T), depth As Integer, [stop] As Integer, kmeansStop As Integer, parallelDepth%) As ClusterEntity()

            If source.Count = 2 Then
EXIT_:          Dim array As T() = source.ToArray

                For i As Integer = 0 To array.Length - 1
                    Dim id As String = "." & CStr(i + 1)
                    array(i).uid &= id
                Next

                Return array
            Else
                depth += 1

                If depth >= [stop] Then
                    Dim cluster As T() = source.ToArray

                    For Each x As T In cluster
                        x.uid &= ".X"
                    Next

                    Return cluster
                End If
            End If

            Dim list As New List(Of ClusterEntity)
            Dim result As KMeansCluster(Of T)() = source.ClusterDataSet(
                2, ,
                [stop]:=kmeansStop, ' 当递归的深度到达一定程度之后会自动使用非并行算法，以防止并行化的颗粒度过细，影响性能
                parallel:=parallelDepth >= 0).ToArray

            ' 检查数据
            Dim b0 As Boolean = False ', b20 As Boolean = False

            For Each x In result
                If x.NumOfEntity = 0 Then
                    b0 = True
                    'Else
                    '    Dim nl As Integer = 0.75 * source.First.Properties.Count
                    '    If (From c In x.ClusterMean Where c = 0R Select 1).Count >= nl AndAlso
                    '        (From c In x.ClusterSum Where c = 0R Select 1).Count >= nl Then
                    '        b20 = True
                    '    End If
                End If
            Next

            parallelDepth -= 1

            If b0 Then    ' 已经无法再分了，全都是0，则放在一个cluster里面
                'Dim cluster As T() = result.MatrixToVector
                'For Each x In cluster
                '    x.uid &= ".X"
                'Next

                'Call list.Add(cluster)
                Call Console.Write(">")
                Call list.Add(__treeCluster(result.Unlist, depth, [stop], kmeansStop, parallelDepth))  ' 递归聚类分解
            Else
                For i As Integer = 0 To result.Length - 1
                    Dim cluster = result(i)
                    Dim id As String = "." & CStr(i + 1)

                    For Each x In cluster
                        x.uid &= id
                    Next

                    If cluster.NumOfEntity = 1 Then
                        Call list.Add(cluster.Item(Scan0))
                    ElseIf cluster.NumOfEntity = 0 Then
                        '  不可以取消这可分支，否则会死循环
                    Else
                        Call Console.Write(">")
                        Call list.Add(__treeCluster(cluster.ToArray, depth, [stop], kmeansStop, parallelDepth))  ' 递归聚类分解
                    End If
                Next
            End If

            Call Console.Write("<")

            Return list.ToArray
        End Function
    End Module
End Namespace
