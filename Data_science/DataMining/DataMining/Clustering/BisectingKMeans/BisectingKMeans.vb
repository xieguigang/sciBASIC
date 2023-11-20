Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace BisectingKMeans

    ''' <summary>
    ''' Created by touhid on 12/21/15.
    ''' 
    ''' @author touhid
    ''' </summary>
    Public Class BisectingKMeans

        Dim NUM_ITERATIONS_BISECTION As Integer = 5
        Dim K_BISECTING As Integer = 6
        Dim CENTROID_THRESHOLD As Double = 0.005
        Dim clusterList As New List(Of Cluster)

        Sub New(dataList As IEnumerable(Of ClusterEntity),
                Optional k As Integer = 6,
                Optional iterations As Integer = 6,
                Optional sse_threshold As Double = 0.005)

            Call init(dataList)

            Me.CENTROID_THRESHOLD = sse_threshold
            Me.K_BISECTING = k
            Me.NUM_ITERATIONS_BISECTION = iterations
        End Sub

        Private Sub init(dataList As IEnumerable(Of ClusterEntity))
            Dim cluster As Cluster = calcCluster(dataList)
            cluster.DataPoints = dataList
            clusterList.Add(cluster)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub runBisectingKMeans()
            Call runBisectingKMeans(clusterList.First)
        End Sub

        Private Sub runBisectingKMeans(worstCluster As Cluster)
            Dim cluster1 As Cluster = Nothing, cluster2 As Cluster = Nothing
            Dim minSSE As Double = Double.MaxValue

            clusterList.Remove(worstCluster)

            For i As Integer = 0 To NUM_ITERATIONS_BISECTION - 1
                Dim ac As Cluster() = kMeansClustering(worstCluster.DataPoints).ToArray
                Dim sumSSE As Double = ac(0).SSE + ac(1).SSE

                If sumSSE < minSSE Then
                    cluster1 = ac(0)
                    cluster2 = ac(1)
                End If
            Next

            clusterList.Add(cluster1)
            clusterList.Add(cluster2)

            If clusterList.Count < K_BISECTING Then
                runBisectingKMeans(GetWorstCluster)
            End If
        End Sub

        ''' <summary>
        ''' k = 2
        ''' </summary>
        ''' <param name="dataPoints"></param>
        ''' <returns></returns>
        Private Iterator Function kMeansClustering(dataPoints As List(Of ClusterEntity)) As IEnumerable(Of Cluster)
            Dim kmeans = New KMeansAlgorithm(Of ClusterEntity)().ClusterDataSet(dataPoints, k:=2)

            For Each cluster As KMeansCluster(Of ClusterEntity) In kmeans
                Yield New Cluster(cluster.ClusterMean, cluster.AsList)
            Next
        End Function

        Public Function GetWorstCluster() As Cluster
            Dim maxSSE As Double = -1
            Dim worst As Cluster = Nothing
            For Each c As Cluster In clusterList
                Dim sse As Double = c.SSE
                If sse > maxSSE Then
                    worst = c
                    maxSSE = sse
                End If
            Next c
            Return worst
        End Function

        ''' <summary>
        ''' calculate the input data center as first cluster
        ''' </summary>
        ''' <param name="dataList"></param>
        ''' <returns></returns>
        Private Function calcCluster(dataList As IEnumerable(Of ClusterEntity)) As Cluster
            Dim centroid As Vector = Nothing

            For Each p As ClusterEntity In dataList
                If centroid Is Nothing Then
                    centroid = New Vector(p.entityVector)
                Else
                    centroid = centroid + New Vector(p.entityVector)
                End If
            Next

            centroid = centroid / dataList.Count

            Return New Cluster(centroid)
        End Function
    End Class
End Namespace
