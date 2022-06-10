#Region "Microsoft.VisualBasic::2b625d888ad097a1c355b31d3adc6bf3, sciBASIC#\Data_science\DataMining\DataMining\Clustering\KMeans\Extensions.vb"

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

    '   Total Lines: 132
    '    Code Lines: 107
    ' Comment Lines: 13
    '   Blank Lines: 12
    '     File Size: 5.84 KB


    '     Module Extensions
    ' 
    '         Function: (+2 Overloads) Kmeans, ToKMeansModels, ValueGroups
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
        Public Function ToKMeansModels(data As IEnumerable(Of NamedCollection(Of Double))) As IEnumerable(Of EntityClusterModel)
            Return data.Select(Function(d)
                                   Return New EntityClusterModel With {
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
                               End Function)
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
        ''' Grouping the numeric values by using the kmeans cluserting operations.
        ''' (对一组数字进行聚类操作，其实在这里就是将这组数值生成Entity数据对象，然后将数值本身作为自动生成的Entity对象的一个唯一属性)
        ''' </summary>
        ''' <param name="array"></param>
        ''' <param name="nd"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ValueGroups(array As IEnumerable(Of Double), nd%) As List(Of EntityClusterModel)
            Dim entities As EntityClusterModel() = array _
                .Select(Function(x, i)
                            Return New EntityClusterModel With {
                                .ID = i & ":" & x,
                                .Properties = New Dictionary(Of String, Double) From {
                                    {"val", x}
                                }
                            }
                        End Function) _
                .ToArray
            Return entities.Kmeans(nd)
        End Function

        ''' <summary>
        ''' Performance the clustering operation on the entity data model.
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="expected"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Kmeans(source As IEnumerable(Of EntityClusterModel),
                               expected%,
                               Optional debug As Boolean = True,
                               Optional parallel As Boolean = True) As List(Of EntityClusterModel)

            Dim rawInput As EntityClusterModel() = source.ToArray
            Dim maps As String() = rawInput _
                .Select(Function(a) a.Properties.Keys) _
                .IteratesALL _
                .Distinct _
                .ToArray
            Dim clusters As ClusterCollection(Of ClusterEntity) = ClusterDataSet(
                clusterCount:=expected,
                source:=rawInput _
                    .Select(Function(xi)
                                Return xi.ToModel(projection:=maps)
                            End Function) _
                    .ToArray,
                debug:=debug,
                parallel:=parallel
            )
            Dim result As New List(Of EntityClusterModel)

            For Each cluster As SeqValue(Of KMeansCluster(Of ClusterEntity)) In clusters.SeqIterator(offset:=1)
                Dim values As EntityClusterModel() = (+cluster) _
                    .Select(Function(x) x.ToDataModel(maps)) _
                    .ToArray

                For Each x As EntityClusterModel In values
                    x.Cluster = cluster.i
                Next

                result += values
            Next

            Return result
        End Function

        <Extension>
        Public Function Kmeans(points As IEnumerable(Of PointF),
                               Optional expected% = 3,
                               Optional debug As Boolean = True,
                               Optional parallel As Boolean = True) As NamedCollection(Of PointF)()

            Dim source As EntityClusterModel() = points _
                .Select(Function(pt, i)
                            Return New EntityClusterModel With {
                                .ID = i,
                                .Cluster = 0,
                                .Properties = New Dictionary(Of String, Double) From {
                                    {"x", CDbl(pt.X)},
                                    {"y", CDbl(pt.Y)}
                                }
                            }
                        End Function) _
                .ToArray
            Dim result = source.Kmeans(expected, debug, parallel)
            Dim resultPoints As NamedCollection(Of PointF)() = result _
                .GroupBy(Function(e) e.Cluster) _
                .Select(Function(e)
                            Return New NamedCollection(Of PointF) With {
                                .name = e.Key,
                                .value = e _
                                    .Select(Function(p)
                                                Return New PointF With {
                                                    .X = p!x,
                                                    .Y = p!y
                                                }
                                            End Function) _
                                    .ToArray
                            }
                        End Function) _
                .ToArray

            Return resultPoints
        End Function
    End Module
End Namespace
