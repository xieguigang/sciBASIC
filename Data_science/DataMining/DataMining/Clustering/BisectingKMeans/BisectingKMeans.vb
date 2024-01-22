Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace BisectingKMeans

    ''' <summary>
    ''' Created by touhid on 12/21/15.
    ''' 
    ''' @author touhid
    ''' </summary>
    Public Class BisectingKMeans : Inherits TraceBackAlgorithm

        Dim NUM_ITERATIONS_BISECTION As Integer = 5
        Dim K_BISECTING As Integer = 6
        Dim clusterList As New List(Of Cluster)
        Dim n_threads As Integer = 16

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="dataList"></param>
        ''' <param name="k"></param>
        ''' <param name="iterations"></param>
        ''' <param name="traceback">record the traceback information for run debug?</param>
        Sub New(dataList As IEnumerable(Of ClusterEntity),
                Optional k As Integer = 6,
                Optional iterations As Integer = 6,
                Optional traceback As Boolean = False,
                Optional n_threads As Integer = 16)

            Call init(dataList)

            Me.K_BISECTING = k
            Me.NUM_ITERATIONS_BISECTION = iterations
            Me.n_threads = n_threads

            If traceback Then
                Me.traceback = New TraceBackIterator
                Me.traceback.SetPoints(clusterList.First.DataPoints)
                Me.traceback.AddIteration(Of ClusterEntity, Cluster)(clusterList)
            End If
        End Sub

        Private Sub init(dataList As IEnumerable(Of ClusterEntity))
            Dim pull = dataList.AsList
            Dim cluster As Cluster = calcCluster(pull)
            cluster.DataPoints = pull
            clusterList.Add(cluster)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function runBisectingKMeans() As IEnumerable(Of Cluster)
            Call runBisectingKMeans(worstCluster:=clusterList.First)
            Return clusterList
        End Function

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

            cluster1.Cluster = cluster1.GetHashCode
            cluster2.Cluster = cluster2.GetHashCode

            clusterList.Add(cluster1)
            clusterList.Add(cluster2)

            If Not traceback Is Nothing Then
                Call traceback.AddIteration(Of ClusterEntity, Cluster)(clusterList)
            End If

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
            Dim kmeans = New KMeansAlgorithm(Of ClusterEntity)(n_threads:=n_threads) _
                .ClusterDataSet(dataPoints, k:=2)

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

            Return New Cluster(centroid) With {.Cluster = Me.GetHashCode}
        End Function
    End Class
End Namespace
