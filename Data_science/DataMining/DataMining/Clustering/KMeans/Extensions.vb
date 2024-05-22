#Region "Microsoft.VisualBasic::bfd34e11f0b543897b0eb82c90fadfa4, Data_science\DataMining\DataMining\Clustering\KMeans\Extensions.vb"

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

    '   Total Lines: 117
    '    Code Lines: 84 (71.79%)
    ' Comment Lines: 22 (18.80%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (9.40%)
    '     File Size: 5.33 KB


    '     Module Extensions
    ' 
    '         Function: (+2 Overloads) Kmeans, PopulateObjects, (+2 Overloads) ToKMeansModels
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

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
                source:=maps.GetVectors(rawInput).ToArray
            )

            Return clusters.PopulateObjects(maps)
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
