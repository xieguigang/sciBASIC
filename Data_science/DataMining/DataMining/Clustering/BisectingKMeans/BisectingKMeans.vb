Namespace BisectingKMeans


    ''' <summary>
    ''' Created by touhid on 12/21/15.
    ''' 
    ''' @author touhid
    ''' </summary>
    Public Class BisectingKMeans

        Private Const NUM_ITERATIONS_BISECTION As Integer = 5

        Private Const K_BISECTING As Integer = 6

        Private clusterList As List(Of Cluster)

        Private Sub New()
            Dim dataList As List(Of DataPoint)
            clusterList = New List(Of Cluster)()

            Dim cluster As Cluster = calcCluster(dataList)
            cluster.DataPoints = dataList
            clusterList.Add(cluster)
            runBisectingKMeans(cluster)

            Dim i As Integer = 1
            For Each c As Cluster In clusterList
                'JAVA TO VB CONVERTER WARNING: An assignment within expression was extracted from the following statement:
                'ORIGINAL LINE: buet.touhiDroid.BisectingKMeans.utils.Lg.pl("Cluster "+ i++ + " :" + c.getCx() + ", " + c.getCy());
                Console.WriteLine("Cluster " & i & " :" & c.Cx & ", " & c.Cy)
                i += 1
            Next c
        End Sub

        Private Sub runBisectingKMeans(ByVal worstCluster As Cluster)
            clusterList.Remove(worstCluster) ' TODO Recheck whether it's being removed or not

            Dim cluster1 As Cluster = Nothing, cluster2 As Cluster = Nothing
            Dim minSSE As Double = Double.MaxValue

            For i As Integer = 0 To NUM_ITERATIONS_BISECTION - 1
                Dim ac As List(Of Cluster) = kMeansClustering(2, worstCluster.DataPoints)

                Dim sumSSE As Double = ac(0).SSE + ac(1).SSE
                If sumSSE < minSSE Then
                    cluster1 = ac(0)
                    cluster2 = ac(1)
                End If
            Next i
            clusterList.Add(cluster1)
            clusterList.Add(cluster2)

            If clusterList.Count < K_BISECTING Then
                runBisectingKMeans(worstCluster)
            End If
        End Sub

        Private Function kMeansClustering(ByVal k As Integer, ByVal dataPoints As List(Of DataPoint)) As List(Of Cluster)
            Dim rand As New Random()

            Dim tempClusterList As New List(Of Cluster)()
            For i As Integer = 0 To k - 1
                Dim cluster As New Cluster(dataPoints(rand.Next(dataPoints.Count)))
                tempClusterList.Add(cluster)
            Next i

            Dim isUpdated As Boolean = True
            Do
                ' Assign Data-points to each of the clusters
                tempClusterList(0).DataPoints = New List(Of DataPoint)()
                tempClusterList(1).DataPoints = New List(Of DataPoint)()
                For Each p As DataPoint In dataPoints
                    Dim minDist As Double = Double.MaxValue
                    Dim idx As Integer = 0

                    For i As Integer = 0 To tempClusterList.Count - 1
                        Dim d As Double = tempClusterList(i).getDistSq(p)
                        If d < minDist Then
                            minDist = d
                            idx = i
                        End If
                    Next i
                    tempClusterList(idx).addPoint(p)
                Next p
                isUpdated = False
                For i As Integer = 0 To k - 1
                    Dim isClUpdated As Boolean = tempClusterList(i).updateCentroid()
                    If isClUpdated Then
                        isUpdated = True
                    End If
                Next i
            Loop While isUpdated

            Return tempClusterList
        End Function

        Private ReadOnly Property WorstCluster As Cluster
            Get
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
            End Get
        End Property

        Private Function calcCluster(ByVal dataList As List(Of DataPoint)) As Cluster
            Dim scx As Double = 0, scy As Double = 0
            For Each p As DataPoint In dataList
                scx += p.Dx
                scy += p.Dy
            Next p
            Dim size As Integer = dataList.Count
            Return New Cluster(scx / size, scy / size)
        End Function

    End Class

End Namespace