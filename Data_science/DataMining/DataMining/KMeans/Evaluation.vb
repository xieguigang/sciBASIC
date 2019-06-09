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

            Dim maxDist As Double = Math.Max(clusterInDist, clusterOutDist)
            Dim SI As Double = (clusterOutDist - clusterInDist) / maxDist

            Return SI
        End Function

        Private Function AverageDistance(a As ClusterEntity(), b As ClusterEntity()) As Double
            Dim clusterAvgInDist As Double = 0

            For Each individual1 In a
                Dim avgInDist As Double = 0

                For Each individual2 In b
                    avgInDist += individual1.Properties.EuclideanDistance(individual2.Properties)
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
                                dist = individual1.Properties.EuclideanDistance(individual2.Properties)

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
                            dist = individual1.Properties.EuclideanDistance(individual2.Properties)

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