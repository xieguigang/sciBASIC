#Region "Microsoft.VisualBasic::841eb41c6fe26943cbcf87bc0f868065, ..\sciBASIC#\Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\KMeans\Extensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Data.csv.IO

Namespace KMeans

    Public Module Extensions

        ''' <summary>
        ''' Grouping the numeric values by using the kmeans cluserting operations.
        ''' (对一组数字进行聚类操作，其实在这里就是将这组数值生成Entity数据对象，然后将数值本身作为自动生成的Entity对象的一个唯一属性)
        ''' </summary>
        ''' <param name="array"></param>
        ''' <param name="nd"></param>
        ''' <returns></returns>
        <Extension> Public Function ValueGroups(array As IEnumerable(Of Double), nd As Integer) As List(Of EntityLDM)
            Dim entities As EntityLDM() = array.ToArray(
                Function(x, i) New EntityLDM With {
                    .ID = i & ":" & x,
                    .Properties = New Dictionary(Of String, Double) From {
                        {"val", x}
                    }
                })
            Return entities.Kmeans(nd)
        End Function

        ''' <summary>
        ''' Performance the clustering operation on the entity data model.
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="expected"></param>
        ''' <returns></returns>
        <Extension> Public Function Kmeans(source As IEnumerable(Of EntityLDM),
                                           expected As Integer,
                                           Optional debug As Boolean = True,
                                           Optional parallel As Boolean = True) As List(Of EntityLDM)

            Dim maps As String() = source _
                .First _
                .Properties _
                .Keys _
                .ToArray
            Dim clusters As ClusterCollection(Of Entity) =
                ClusterDataSet(clusterCount:=expected,
                               source:=source.ToArray(Function(x) x.ToModel),
                               debug:=debug,
                               parallel:=parallel)
            Dim result As New List(Of EntityLDM)

            For Each cluster As SeqValue(Of KMeansCluster(Of Entity)) In clusters.SeqIterator(offset:=1)
                Dim values As EntityLDM() = (+cluster) _
                    .ToArray(Function(x) x.ToLDM(maps))

                For Each x As EntityLDM In values
                    x.Cluster = cluster.i
                Next

                result += values
            Next

            Return result
        End Function

        <Extension>
        Public Function ToKMeansModels(data As IEnumerable(Of DataSet)) As EntityLDM()
            Return data.Select(
                Function(d) New EntityLDM With {
                    .ID = d.ID,
                    .Cluster = "",
                    .Properties = New Dictionary(Of String, Double)(d.Properties)
                }).ToArray
        End Function
    End Module
End Namespace
