#Region "Microsoft.VisualBasic::92126f18b94c8c78ae1c8a210e4598ff, Data_science\DataMining\DataMining\Clustering\KMeans\Extensions.vb"

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

    '   Total Lines: 146
    '    Code Lines: 108 (73.97%)
    ' Comment Lines: 22 (15.07%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 16 (10.96%)
    '     File Size: 6.74 KB


    '     Module Extensions
    ' 
    '         Function: (+4 Overloads) Kmeans, PopulateObjects, (+2 Overloads) ToKMeansModels
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.DataMining.ComponentModel.EntityModels
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math

Namespace KMeans

    <HideModuleName>
    Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Iterator Function ToKMeansModels(data As IEnumerable(Of NamedCollection(Of Double))) As IEnumerable(Of EntityClusterModel)
            For Each d As NamedCollection(Of Double) In data
                Yield New EntityClusterModel With {
                    .ID = d.name,
                    .Cluster = "",
                    .Properties = d.value _
                        .Select(Function(v1, i)
                                    Return (v1, i)
                                End Function) _
                        .ToDictionary(Function(t) t.i.ToString,
                                    Function(t)
                                        Return t.v1
                                    End Function)
                }
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToKMeansModels(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(data As IEnumerable(Of DataSet)) As EntityClusterModel()
            Return data _
                .Select(Function(d)
                            Return New EntityClusterModel With {
                                .ID = d.Key,
                                .Cluster = "",
                                .Properties = New Dictionary(Of String, Double)(d.Properties)
                            }
                        End Function) _
                .ToArray
        End Function

        ''' <summary>
        ''' Performance the clustering operation on the entity data model.
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="expected">k-centers</param>
        ''' <returns>
        ''' 输出的元素和输入相比较是乱序的
        ''' </returns>
        <Extension>
        Public Function Kmeans(source As IEnumerable(Of EntityClusterModel), expected%,
                               Optional debug As Boolean = True,
                               Optional n_threads As Integer = 16) As IEnumerable(Of EntityClusterModel)

            Dim rawInput As EntityClusterModel() = source.ToArray
            Dim maps As New DataSetConvertor(rawInput)
            Dim kmeansCore As New KMeansAlgorithm(Of ClusterEntity)(debug, n_threads:=n_threads)
            Dim clusters As ClusterCollection(Of ClusterEntity) = kmeansCore.ClusterDataSet(
                k:=expected,
                source:=maps.GetVectors(rawInput)
            )

            Return clusters.PopulateObjects(maps)
        End Function

        <Extension>
        Public Iterator Function Kmeans(source As IEnumerable(Of ClusterEntity), k As Integer, Optional debug As Boolean = True, Optional n_threads As Integer = 16) As IEnumerable(Of ClusterEntity)
            Dim kmeansCore As New KMeansAlgorithm(Of ClusterEntity)(debug, n_threads:=n_threads)
            Dim clusters As ClusterCollection(Of ClusterEntity) = kmeansCore.ClusterDataSet(
                k:=k,
                source:=source
            )
            Dim class_id As Integer = 1

            For Each cluster As KMeansCluster(Of ClusterEntity) In clusters
                For Each vec As ClusterEntity In cluster.AsEnumerable
                    vec.cluster = class_id
                    Yield vec
                Next

                class_id += 1
            Next
        End Function

        ''' <summary>
        ''' 对统一二维表执行 KMeans 聚类，聚类结果会写入标签矩阵的 ``cluster`` 列。
        ''' 
        ''' ```vb
        ''' Dim result = x.kmeans(k:=3)
        ''' ```
        ''' </summary>
        ''' <param name="source">经过预处理之后的纯数值二维表</param>
        ''' <param name="k">簇的数量</param>
        ''' <param name="n_threads">并行计算所使用的线程数</param>
        ''' <param name="debug">是否输出调试信息</param>
        ''' <returns>写入聚类结果之后的原表对象</returns>
        <Extension>
        Public Function kmeans(source As NumericTable,
                               k As Integer,
                               Optional n_threads As Integer = 16,
                               Optional debug As Boolean = False) As NumericTable

            Dim engine As New KMeansEngine(debug:=debug, n_threads:=n_threads)
            Dim clusters As NumericClusterCollection = engine.ClusterDataSet(source, k)
            Dim labels As Integer() = New Integer(source.nsamples - 1) {}
            Dim class_id As Integer = 1

            For Each cluster As NumericKMeansCluster In clusters
                For i As Integer = 0 To cluster.NumOfEntity - 1
                    labels(cluster.ClusterIndex(i)) = class_id
                Next

                class_id += 1
            Next

            Return source.SetLabel("cluster", labels)
        End Function

        ''' <summary>
        ''' a helper function for convert object data
        ''' </summary>
        ''' <param name="kmeans"></param>
        ''' <param name="maps"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function PopulateObjects(kmeans As ClusterCollection(Of ClusterEntity), maps As DataSetConvertor) As IEnumerable(Of EntityClusterModel)
            For Each cluster As SeqValue(Of KMeansCluster(Of ClusterEntity)) In kmeans.SeqIterator(offset:=1)
                For Each xi As EntityClusterModel In maps.GetObjects(+cluster, setClass:=cluster.i)
                    Yield xi
                Next
            Next
        End Function

        ''' <summary>
        ''' do clustering of a collection 2D points
        ''' </summary>
        ''' <param name="points"></param>
        ''' <param name="expected">k-centers</param>
        ''' <param name="debug"></param>
        ''' <param name="n_threads"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function Kmeans(points As IEnumerable(Of PointF),
                                        Optional expected% = 3,
                                        Optional debug As Boolean = True,
                                        Optional n_threads As Integer = 16) As IEnumerable(Of NamedCollection(Of PointF))

            Dim source As ClusterEntity() = points _
                .Select(Function(pt, i)
                            Return New ClusterEntity() With {
                                .uid = i,
                                .cluster = 0,
                                .entityVector = {CDbl(pt.X), CDbl(pt.Y)}
                            }
                        End Function) _
                .ToArray
            Dim workflow As New KMeansAlgorithm(Of ClusterEntity)(debug:=debug, n_threads:=n_threads)
            Dim result As ClusterCollection(Of ClusterEntity) = workflow.ClusterDataSet(source, k:=expected)
            Dim id As i32 = 1

            For Each cluster As KMeansCluster(Of ClusterEntity) In result
                Yield New NamedCollection(Of PointF)((++id).ToString, cluster.Select(Function(c) New PointF(c(0), c(1))))
            Next
        End Function
    End Module
End Namespace
