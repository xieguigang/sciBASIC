#Region "Microsoft.VisualBasic::c01efe879a62dd983db25056f69b3d3a, sciBASIC#\Data_science\DataMining\DataMining\Clustering\KMeans\Evaluation.vb"

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

    '   Total Lines: 106
    '    Code Lines: 72
    ' Comment Lines: 13
    '   Blank Lines: 21
    '     File Size: 3.66 KB


    '     Module Evaluation
    ' 
    '         Function: AverageDistance, Dunn, Silhouette
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Correlations
Imports stdNum = System.Math

Namespace KMeans

    ''' <summary>
    ''' 判断聚类结果优劣的两个距离判定方法
    ''' </summary>
    Public Module Evaluation

        ''' <summary>
        ''' Silhouette Coefficient
        ''' </summary>
        ''' <param name="clusters"></param>
        ''' <returns></returns>
        Public Function Silhouette(clusters As ClusterEntity()())
            Dim clusterInDist As Double = 0
            Dim clusterOutDist As Double = 0
            Dim cluster As ClusterEntity()
            Dim nextCluster As ClusterEntity()

            For c As Integer = 0 To clusters.Length - 1
                cluster = clusters(c)

                If c + 1 >= clusters.Length Then
                    nextCluster = clusters(Scan0)
                Else
                    nextCluster = clusters(c + 1)
                End If

                clusterInDist += AverageDistance(cluster, cluster)
                clusterOutDist += AverageDistance(cluster, nextCluster)
            Next

            clusterInDist /= clusters.Length
            clusterOutDist /= clusters.Length

            Dim maxDist As Double = stdNum.Max(clusterInDist, clusterOutDist)
            Dim SI As Double = (clusterOutDist - clusterInDist) / maxDist

            Return SI
        End Function

        Private Function AverageDistance(a As ClusterEntity(), b As ClusterEntity()) As Double
            Dim clusterAvgInDist As Double = 0

            For Each individual1 In a
                Dim avgInDist As Double = 0

                For Each individual2 In b
                    avgInDist += individual1.entityVector.EuclideanDistance(individual2.entityVector)
                Next

                avgInDist /= a.Length
                clusterAvgInDist += avgInDist
            Next

            Return clusterAvgInDist / a.Length
        End Function

        ''' <summary>
        ''' Dunn Index
        ''' </summary>
        ''' <param name="clusters"></param>
        ''' <returns></returns>
        Public Function Dunn(clusters As ClusterEntity()()) As Double
            Dim minOutDist = Double.MaxValue
            Dim maxInDist As Double = 0
            Dim dist As Double

            For Each cluster In clusters
                For Each individual1 In cluster
                    For Each cluster2 In clusters
                        If Not cluster Is cluster2 Then
                            For Each individual2 In cluster2
                                dist = individual1.entityVector.EuclideanDistance(individual2.entityVector)

                                If dist < minOutDist Then
                                    minOutDist = dist
                                End If
                            Next
                        End If
                    Next
                Next
            Next

            For Each cluster In clusters
                For Each individual1 In cluster
                    For Each individual2 In cluster
                        If Not individual1 Is individual2 Then
                            dist = individual1.entityVector.EuclideanDistance(individual2.entityVector)

                            If dist > maxInDist Then
                                maxInDist = dist
                            End If
                        End If
                    Next
                Next
            Next

            Dim Di = minOutDist / maxInDist

            Return Di
        End Function
    End Module
End Namespace
