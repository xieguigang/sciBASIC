#Region "Microsoft.VisualBasic::3cb534bfb4cbbfeb78326ee9daf66229, Data_science\DataMining\DataMining\KMeans\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: Kmeans, ValueGroups
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace KMeans

    Public Module Extensions

        ''' <summary>
        ''' Grouping the numeric values by using the kmeans cluserting operations.
        ''' (对一组数字进行聚类操作，其实在这里就是将这组数值生成Entity数据对象，然后将数值本身作为自动生成的Entity对象的一个唯一属性)
        ''' </summary>
        ''' <param name="array"></param>
        ''' <param name="nd"></param>
        ''' <returns></returns>
        <Extension> Public Function ValueGroups(array As IEnumerable(Of Double), nd%) As List(Of EntityClusterModel)
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
        <Extension> Public Function Kmeans(source As IEnumerable(Of EntityClusterModel),
                                           expected%,
                                           Optional debug As Boolean = True,
                                           Optional parallel As Boolean = True) As List(Of EntityClusterModel)

            Dim maps As String() = source _
                .First _
                .Properties _
                .Keys _
                .ToArray
            Dim clusters As ClusterCollection(Of Entity) =
                ClusterDataSet(clusterCount:=expected,
                               source:=source.Select(Function(x) x.ToModel).ToArray,
                               debug:=debug,
                               parallel:=parallel)
            Dim result As New List(Of EntityClusterModel)

            For Each cluster As SeqValue(Of KMeansCluster(Of Entity)) In clusters.SeqIterator(offset:=1)
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
    End Module
End Namespace
