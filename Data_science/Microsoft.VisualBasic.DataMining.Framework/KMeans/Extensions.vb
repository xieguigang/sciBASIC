#Region "Microsoft.VisualBasic::6f13c6e3e45f00584edcb788a850d390, ..\visualbasic_App\Data_science\Microsoft.VisualBasic.DataMining.Framework\KMeans\Extensions.vb"

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
                    .Name = i & ":" & x,
                    .Properties = New Dictionary(Of String, Double) From {{"val", x}}
                })
            Return entities.Kmeans(nd)
        End Function

        ''' <summary>
        ''' Performance the clustering operation on the entity data model.
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        <Extension> Public Function Kmeans(source As IEnumerable(Of EntityLDM), n As Integer) As List(Of EntityLDM)
            Dim maps As String() = source.First.Properties.Keys.ToArray
            Dim clusters As ClusterCollection(Of Entity) = n.ClusterDataSet(source.ToArray(Function(x) x.ToModel))
            Dim result As New List(Of EntityLDM)

            n = 1

            For Each cluster As KMeansCluster(Of Entity) In clusters
                Dim values As EntityLDM() = cluster.ToArray(Function(x) x.ToLDM(maps))

                For Each x In values
                    x.Cluster = n
                Next

                result += values
                n += 1
            Next

            Return result
        End Function
    End Module
End Namespace
