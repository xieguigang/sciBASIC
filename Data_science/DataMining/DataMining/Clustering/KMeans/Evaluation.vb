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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations
Imports std = System.Math

Namespace KMeans

    ''' <summary>
    ''' 判断聚类结果优劣的两个距离判定方法
    ''' </summary>
    Public Module Evaluation

        ''' <summary>
        ''' Silhouette Coefficient
        ''' </summary>
        ''' <param name="result"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Silhouette(result As IEnumerable(Of ClusterEntity)) As Double
            Return result _
                .GroupBy(Function(a) a.cluster) _
                .Select(Function(cluster)
                            Return New Cluster(Of ClusterEntity)(cluster)
                        End Function) _
                .Silhouette
        End Function

        ''' <summary>
        ''' Silhouette Coefficient
        ''' </summary>
        ''' <param name="result">the cluster result</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Silhouette score is used to evaluate the quality of clusters created using clustering 
        ''' algorithms such as K-Means in terms of how well samples are clustered with other samples 
        ''' that are similar to each other. The Silhouette score is calculated for each sample of 
        ''' different clusters. To calculate the Silhouette score for each observation/data point, 
        ''' the following distances need to be found out for each observations belonging to all the 
        ''' clusters:
        ''' 
        ''' Mean distance between the observation And all other data points In the same cluster. This
        ''' distance can also be called a mean intra-cluster distance. The mean distance Is denoted by a
        ''' Mean distance between the observation And all other data points Of the Next nearest cluster.
        ''' This distance can also be called a mean nearest-cluster distance. The mean distance Is 
        ''' denoted by b
        ''' 
        ''' Silhouette score, S, for Each sample Is calculated Using the following formula:
        ''' 
        ''' \(S = \frac{(b - a)}{max(a, b)}\)
        ''' 
        ''' The value Of the Silhouette score varies from -1 To 1. If the score Is 1, the cluster Is
        ''' dense And well-separated than other clusters. A value near 0 represents overlapping clusters
        ''' With samples very close To the decision boundary Of the neighboring clusters. A negative 
        ''' score [-1, 0] indicates that the samples might have got assigned To the wrong clusters.
        ''' </remarks>
        <Extension>
        Public Function Silhouette(result As IEnumerable(Of Cluster(Of ClusterEntity))) As Double
            Dim clusterInDist As Double = 0
            Dim clusterOutDist As Double = 0
            Dim cluster As Cluster(Of ClusterEntity)
            Dim nextCluster As Cluster(Of ClusterEntity)
            Dim clusters = result.SafeQuery.ToArray

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

            Dim maxDist As Double = std.Max(clusterInDist, clusterOutDist)
            Dim SI As Double = (clusterOutDist - clusterInDist) / maxDist

            Return SI
        End Function

        Private Function AverageDistance(a As Cluster(Of ClusterEntity), b As Cluster(Of ClusterEntity)) As Double
            Dim clusterAvgInDist As Double = 0

            For Each individual1 In a.m_innerList
                Dim avgInDist As Double = 0

                For Each individual2 In b.m_innerList
                    avgInDist += individual1.DistanceTo(individual2)
                Next

                avgInDist /= a.size
                clusterAvgInDist += avgInDist
            Next

            Return clusterAvgInDist / a.size
        End Function

        ''' <summary>
        ''' Dunn Index
        ''' </summary>
        ''' <param name="clusters"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Dunn(clusters As IEnumerable(Of ClusterEntity)) As Double
            Return clusters _
                .GroupBy(Function(c) c.cluster) _
                .Select(Function(c) c.ToArray) _
                .DoCall(Function(c)
                            Return Dunn(c.ToArray)
                        End Function)
        End Function

        ''' <summary>
        ''' Dunn Index
        ''' </summary>
        ''' <param name="clusters">A multiple cluster result</param>
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

    Public Class EvaluationScore

        Public Property silhouette As Double
        Public Property dunn As Double
        Public Property clusters As Dictionary(Of String, String())

        Public ReadOnly Property num_class As Integer
            Get
                Return clusters.Count
            End Get
        End Property

        Public Shared Function Evaluate(data As IEnumerable(Of ClusterEntity)) As EvaluationScore
            Dim class_groups = data _
                .GroupBy(Function(c) c.cluster) _
                .Select(Function(c) c.ToArray) _
                .ToArray
            Dim dunn = Evaluation.Dunn(class_groups)
            Dim silhouette = Evaluation.Silhouette(class_groups.Select(Function(g) New Cluster(Of ClusterEntity)(g)))

            Return New EvaluationScore With {
                .clusters = class_groups _
                    .ToDictionary(Function(c) c.First.cluster.ToString,
                                  Function(c)
                                      Return c.Keys.ToArray
                                  End Function),
                .dunn = dunn,
                .silhouette = silhouette
            }
        End Function

    End Class
End Namespace
