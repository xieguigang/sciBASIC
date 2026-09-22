#Region "Microsoft.VisualBasic::5a89be92ce534f0eafd446851f4ab446, Data_science\DataMining\DataMining\Clustering\KMeans\Evaluation.vb"

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

    '   Total Lines: 497
    '    Code Lines: 342 (68.81%)
    ' Comment Lines: 76 (15.29%)
    '    - Xml Docs: 82.89%
    ' 
    '   Blank Lines: 79 (15.90%)
    '     File Size: 19.35 KB


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
Imports Microsoft.VisualBasic.DataMining.ComponentModel.EntityModels
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Correlations
Imports std = System.Math
Imports ClusteringIndices = Microsoft.VisualBasic.DataMining.Evaluation.ClusteringIndices

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

        ''' <summary>
        ''' Davies–Bouldin 指数（越小越好）。
        ''' 
        ''' 已重构为委托统一实现 <see cref="ClusteringIndices.DaviesBouldin(Double()(), Integer())"/>。
        ''' </summary>
        ''' <param name="clusters"></param>
        ''' <returns></returns>
        Public Function calcularDavidBouldin(clusters As Bisecting.Cluster()) As Double
            Dim data = ToIndicesInput(clusters)
            Return ClusteringIndices.DaviesBouldin(data.features, data.labels)
        End Function

        ''' <summary>
        ''' 把聚类模型转换为统一指标模块所需的「特征矩阵 + 整数簇标签」输入。
        ''' </summary>
        ''' <param name="clusters"></param>
        ''' <returns></returns>
        Private Function ToIndicesInput(clusters As Bisecting.Cluster()) As (features As Double()(), labels As Integer())
            If clusters Is Nothing Then
                Return (New Double()() {}, New Integer() {})
            End If

            Dim features As New List(Of Double())()
            Dim labels As New List(Of Integer)()
            Dim clusterId As Integer = 0

            For Each cluster As Bisecting.Cluster In clusters
                For Each point As ClusterEntity In cluster
                    features.Add(point.entityVector)
                    labels.Add(clusterId)
                Next

                clusterId += 1
            Next

            Return (features.ToArray, labels.ToArray)
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

        ''' <summary>
        ''' Calinski–Harabasz 指数（越大越好）。
        ''' 
        ''' 已重构为委托统一实现 <see cref="ClusteringIndices.CalinskiHarabasz(Double()(), Integer())"/>。
        ''' </summary>
        ''' <param name="clusters"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CalinskiHarabasz(clusters As Bisecting.Cluster()) As Double
            Dim data = ToIndicesInput(clusters)
            Return ClusteringIndices.CalinskiHarabasz(data.features, data.labels)
        End Function

        ''' <summary>
        ''' 最大簇内直径。
        ''' 
        ''' 已重构为委托统一实现 <see cref="ClusteringIndices.MaximumDiameter(Double()(), Integer())"/>。
        ''' </summary>
        ''' <param name="clusters"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CalcularMaximumDiameter(clusters As Bisecting.Cluster()) As Double
            Dim data = ToIndicesInput(clusters)
            Return ClusteringIndices.MaximumDiameter(data.features, data.labels)
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
            Dim data = ToIndicesInput(clusters)
            Return ClusteringIndices.Silhouette(data.features, data.labels)
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
        ''' Dunn 指数（越大越好）。
        ''' 
        ''' 已重构为委托统一实现 <see cref="ClusteringIndices.Dunn(Double()(), Integer())"/>。
        ''' </summary>
        ''' <param name="clusters">A multiple cluster result</param>
        ''' <returns></returns>
        Public Function Dunn(clusters As Bisecting.Cluster()) As Double
            Dim data = ToIndicesInput(clusters)
            Return ClusteringIndices.Dunn(data.features, data.labels)
        End Function

    End Module
End Namespace
