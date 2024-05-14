#Region "Microsoft.VisualBasic::2e6f103bd6db7cb6117e0f9eb7bffbb0, Data_science\DataMining\DataMining\Clustering\KMeans\Evaluation.vb"

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

    '   Total Lines: 496
    '    Code Lines: 341
    ' Comment Lines: 76
    '   Blank Lines: 79
    '     File Size: 19.28 KB


    '     Module Evaluation
    ' 
    '         Function: AverageDistance, CalcMaxInDist, CalcMinOutDist, calcularAverageBetweenClusterDistance, calcularAverageDistance
    '                   calcularDavidBouldin, CalcularMaximumDiameter, calcularMinimumDistance, (+2 Overloads) CalinskiHarabasz, (+2 Overloads) Dunn
    '                   (+2 Overloads) Silhouette, SquaredDistance
    '         Class CalcMaxInDistTask
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '             Function: GetMax
    ' 
    '             Sub: Solve
    ' 
    '         Class CalcMinOutDistTask
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '             Function: GetMin
    ' 
    '             Sub: Solve
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Parallel
Imports std = System.Math

Namespace KMeans

    ''' <summary>
    ''' 判断聚类结果优劣的两个距离判定方法
    ''' </summary>
    Public Module Evaluation

        Public Function calcularAverageBetweenClusterDistance(clusters As Bisecting.Cluster()) As Double
            Dim averageDistanceBetween As Double
            Dim distA As Double = 0
            Dim cont As Double = 0

            For Each cluster In clusters
                For Each punto In cluster
                    For Each cluster2 In clusters
                        If cluster Is cluster2 Then
                            Continue For
                        End If
                        For Each punto2 In cluster
                            If Not punto Is punto2 Then
                                distA += punto.DistanceTo(punto2)
                                cont += 1
                            End If
                        Next
                    Next
                Next
            Next
            averageDistanceBetween = distA / cont

            Return averageDistanceBetween
        End Function

        ''' <summary>
        ''' Distancia minima entre puntos de diferentes clusters
        ''' </summary>
        ''' <param name="clusters"></param>
        ''' <returns></returns>
        Public Function calcularMinimumDistance(clusters As Bisecting.Cluster()) As Double
            Dim minimumDistance As Double = -1
            Dim aux As Double

            For Each cluster In clusters
                For Each punto In cluster
                    For Each cluster2 In clusters
                        If cluster Is cluster2 Then
                            Continue For
                        End If
                        For Each punto2 In cluster
                            If Not punto Is punto2 Then
                                If minimumDistance = -1 Then
                                    minimumDistance = punto.DistanceTo(punto2)
                                Else
                                    aux = punto.DistanceTo(punto2)
                                    If aux < minimumDistance Then
                                        minimumDistance = aux
                                    End If
                                End If
                            End If
                        Next
                    Next
                Next
            Next

            Return minimumDistance
        End Function

        Public Function calcularAverageDistance(clusters As Bisecting.Cluster()) As Double
            Dim averageDistance As Double
            Dim distA As Double = 0
            Dim cont As Double = 0

            For Each cluster In clusters
                For Each punto In cluster
                    For Each punto2 In cluster
                        If Not punto Is punto2 Then
                            distA += punto.DistanceTo(punto2)
                            cont += 1
                        End If
                    Next
                Next
            Next
            averageDistance = distA / cont

            Return averageDistance
        End Function

        Public Function calcularDavidBouldin(clusters As Bisecting.Cluster()) As Double
            Dim numberOfClusters = clusters.Length
            Dim david As Double = 0.0

            If numberOfClusters = 1 Then
                Call "Impossible to evaluate Davies-Bouldin index over a single cluster".Warning
                Return 0
            End If

            ' counting distances within
            Dim withinClusterDistance = New Double(numberOfClusters - 1) {}
            Dim i = 0

            For Each cluster In clusters
                For Each punto In cluster
                    withinClusterDistance(i) += punto.DistanceTo(cluster)
                Next
                withinClusterDistance(i) /= cluster.Size
                i += 1
            Next

            Dim result = 0.0
            Dim max = Double.NegativeInfinity

            For i = 0 To numberOfClusters - 1
                'if the cluster is null
                If clusters(i).centroid IsNot Nothing Then

                    For j = 0 To numberOfClusters - 1
                        'if the cluster is null
                        If i <> j AndAlso clusters(j).centroid IsNot Nothing Then
                            Dim val = (withinClusterDistance(i) + withinClusterDistance(j)) / clusters(i).DistanceTo(clusters(j))
                            If val > max Then
                                max = val
                            End If
                        End If
                    Next
                End If
                result = result + max
            Next

            david = result / numberOfClusters

            Return david
        End Function

        <Extension>
        Public Function SquaredDistance(clusters As Bisecting.Cluster()) As Double
            Dim squaredDist As Double = 0
            Dim aux As Double
            Dim cont As Double = 0

            For Each cluster In clusters
                Dim runPart = cluster _
                    .AsParallel _
                    .Select(Function(punto)
                                Dim auxi As Double = 0
                                Dim squared As Double = 0
                                Dim count As Double = 0

                                For Each punto2 As ClusterEntity In cluster
                                    If Not punto Is punto2 Then
                                        auxi = punto.DistanceTo(punto2)
                                        squared += aux ^ 2
                                        count += 1
                                    End If
                                Next

                                Return (squared, count)
                            End Function) _
                    .ToArray

                squaredDist += Aggregate pt In runPart Into Sum(pt.squared)
                cont += Aggregate pt In runPart Into Sum(pt.count)
            Next

            Return squaredDist / cont
        End Function

        <Extension>
        Public Function CalinskiHarabasz(result As IEnumerable(Of ClusterEntity)) As Double
            Return EvaluationScore.CreateClusters(result) _
                .ToArray _
                .CalinskiHarabasz
        End Function

        <Extension>
        Public Function CalinskiHarabasz(clusters As Bisecting.Cluster()) As Double
            Dim calinski As Double = 0.0
            Dim squaredInterCluter As Double = 0
            Dim aux As Double
            Dim cont As Double = 0

            For Each cluster In clusters
                For Each cluster2 In clusters
                    If cluster Is cluster2 Then
                        Continue For
                    End If

                    ' get cluster centroid distance
                    aux = cluster.DistanceTo(cluster2)
                    squaredInterCluter += aux ^ 2
                    cont += 1
                Next
            Next

            calinski = SquaredDistance(clusters) / (squaredInterCluter / cont)

            Return calinski
        End Function

        ''' <summary>
        ''' Diámetro máximo entre dos puntos que pertenecen al mismo cluster.
        ''' </summary>
        ''' <param name="clusters"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function CalcularMaximumDiameter(clusters As Bisecting.Cluster()) As Double
            Dim maximumDiameter As Double = 0
            Dim aux As Double

            For Each cluster In clusters
                For Each punto In cluster
                    For Each punto2 In cluster
                        If Not punto Is punto2 Then
                            aux = punto.DistanceTo(punto2)
                            If aux > maximumDiameter Then
                                maximumDiameter = aux
                            End If
                        End If
                    Next
                Next
            Next

            Return maximumDiameter
        End Function

        ''' <summary>
        ''' Silhouette Coefficient
        ''' </summary>
        ''' <param name="result"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Silhouette(result As IEnumerable(Of ClusterEntity)) As Double
            Return EvaluationScore.CreateClusters(result) _
                .ToArray _
                .Silhouette
        End Function

        ''' <summary>
        ''' Silhouette Coefficient
        ''' </summary>
        ''' <param name="clusters">the cluster result</param>
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
        Public Function Silhouette(clusters As Bisecting.Cluster()) As Double
            Dim clusterInDist As Double = 0
            Dim clusterOutDist As Double = 0
            Dim cluster As Bisecting.Cluster
            Dim nextCluster As Bisecting.Cluster

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

        Private Function AverageDistance(a As Bisecting.Cluster, b As Bisecting.Cluster) As Double
            Dim factor As Double = a.Size
            Dim clusterAvgInDist As Double =
                Aggregate individual1 As ClusterEntity
                In a.AsParallel
                Let sumInDist = b.Select(Function(individual2) individual1.DistanceTo(individual2)).Sum
                Into Sum(sumInDist / factor)

            Return clusterAvgInDist / factor
        End Function

        ''' <summary>
        ''' Dunn Index
        ''' </summary>
        ''' <param name="clusters"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Dunn(clusters As IEnumerable(Of ClusterEntity)) As Double
            Return EvaluationScore _
                .CreateClusters(clusters) _
                .DoCall(Function(c)
                            Return Dunn(c.ToArray)
                        End Function)
        End Function

        ''' <summary>
        ''' Dunn Index
        ''' </summary>
        ''' <param name="clusters">A multiple cluster result</param>
        ''' <returns></returns>
        Public Function Dunn(clusters As Bisecting.Cluster()) As Double
            Dim minOutDist As Double = clusters _
                .Select(Function(cluster) cluster.CalcMinOutDist(clusters)) _
                .Min
            Dim maxInDist As Double = clusters _
                .Select(Function(cluster) cluster.CalcMaxInDist()) _
                .Max
            Dim Di As Double = minOutDist / maxInDist

            Return Di
        End Function

        Const InternalParallelWorks As Integer = 30

        ''' <summary>
        ''' evaluate internal a cluster
        ''' </summary>
        ''' <param name="cluster"></param>
        ''' <returns></returns>
        <Extension>
        Private Function CalcMaxInDist(cluster As Bisecting.Cluster) As Double
            Dim maxInDist As Double = Double.MinValue

            If cluster.Size > VectorTask.n_threads * InternalParallelWorks Then
                Dim eval As New CalcMaxInDistTask(cluster)

                eval.Run()
                maxInDist = eval.GetMax
            Else
                For Each individual1 In cluster
                    For Each individual2 In cluster
                        If Not individual1 Is individual2 Then
                            Dim dist As Double = individual1.entityVector.EuclideanDistance(individual2.entityVector)

                            ' evaluate the max distance internal a cluster
                            If dist > maxInDist Then
                                maxInDist = dist
                            End If
                        End If
                    Next
                Next
            End If

            Return maxInDist
        End Function

        Private Class CalcMaxInDistTask : Inherits VectorTask

            Dim cluster As ClusterEntity()
            Dim maxInDist As Double()
            Dim centroid As Double()

            Sub New(cluster As Bisecting.Cluster)
                Call MyBase.New(cluster.Size)

                Me.centroid = cluster.centroid
                Me.cluster = cluster.ToArray
                Me.maxInDist = Allocate(Of Double)(all:=False)
            End Sub

            Public Function GetMax() As Double
                Return maxInDist.Max
            End Function

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                Dim max_dist As Double = Double.MinValue

                For i As Integer = start To ends
                    Dim individual1 As IVector = cluster(i)

                    For Each individual2 As ClusterEntity In cluster
                        If Not individual1 Is individual2 Then
                            Dim dist As Double = individual1.DistanceTo(individual2)

                            ' evaluate the max distance internal a cluster
                            If dist > max_dist Then
                                max_dist = dist
                            End If
                        End If
                    Next
                Next

                maxInDist(cpu_id) = max_dist
            End Sub
        End Class

        ''' <summary>
        ''' evaluate between two clusters
        ''' </summary>
        ''' <param name="cluster"></param>
        ''' <param name="clusters"></param>
        ''' <returns></returns>
        <Extension>
        Private Function CalcMinOutDist(cluster As Bisecting.Cluster, clusters As Bisecting.Cluster()) As Double
            Dim minOutDist = Double.MaxValue

            If cluster.Size > CalcMinOutDistTask.n_threads * InternalParallelWorks Then
                Dim eval As New CalcMinOutDistTask(cluster, clusters)

                eval.Run()
                minOutDist = eval.GetMin
            Else
                For Each individual1 In cluster
                    For Each cluster2 In clusters
                        If Not cluster Is cluster2 Then
                            For Each individual2 In cluster2
                                Dim dist As Double = individual1.DistanceTo(individual2)

                                ' evaluate the min distance between the clusters
                                If dist < minOutDist Then
                                    minOutDist = dist
                                End If
                            Next
                        End If
                    Next
                Next
            End If

            Return minOutDist
        End Function

        Private Class CalcMinOutDistTask : Inherits VectorTask

            Dim cluster As Bisecting.Cluster
            Dim clusters As Bisecting.Cluster()
            Dim minOutDist As Double()

            Sub New(cluster As Bisecting.Cluster, clusters As Bisecting.Cluster())
                Call MyBase.New(cluster.Size)

                Me.minOutDist = Allocate(Of Double)(all:=False)
                Me.cluster = cluster
                Me.clusters = clusters
            End Sub

            Public Function GetMin() As Double
                Return minOutDist.Min
            End Function

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                Dim min_dist As Double = Double.MaxValue

                For i As Integer = start To ends
                    Dim individual1 As IVector = cluster(i)

                    For Each cluster2 In clusters
                        If Not cluster Is cluster2 Then
                            For Each individual2 In cluster2
                                Dim dist As Double = individual1.DistanceTo(individual2)

                                ' evaluate the min distance between the clusters
                                If dist < min_dist Then
                                    min_dist = dist
                                End If
                            Next
                        End If
                    Next
                Next

                minOutDist(cpu_id) = min_dist
            End Sub
        End Class
    End Module
End Namespace
