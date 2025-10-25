Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.DataMining.DBSCAN
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.Statistics.Linq

Namespace Clustering

    Public Class KNNCluster(Of T)

        ReadOnly points As DbscanPoint(Of T)()
        ReadOnly k As Integer
        ReadOnly p As Integer
        ReadOnly distanceMap As DistanceMap(Of DbscanPoint(Of T))

        Sub New(data As IEnumerable(Of T), metricFunc As Func(Of T, T, Double), knn As Integer, Optional threshold As Double = 0.8)
            points = data.SafeQuery.Select(Function(a, i) New DbscanPoint(Of T)(a, i)).ToArray
            k = knn
            p = threshold * knn
            distanceMap = New DistanceMap(Of DbscanPoint(Of T))(points, Function(a, b) metricFunc(a.ClusterPoint, b.ClusterPoint))
        End Sub

        Public Function AssignClusterId() As IEnumerable(Of DbscanPoint(Of T))
            Dim cluster_id As i32 = 1
            Dim bar As Tqdm.ProgressBar = Nothing

            For Each point As DbscanPoint(Of T) In TqdmWrapper.Wrap(points, bar:=bar, wrap_console:=App.EnableTqdm)
                If Not point.IsVisited Then
                    point.ClusterId = ++cluster_id

                    Call bar.SetLabel($"processing cluster {point.ClusterId}...")
                    Call ExpandCluster(point)
                End If
            Next

            Return points
        End Function

        Private Sub ExpandCluster(seedPoint As DbscanPoint(Of T))
            Dim pool As New Stack(Of DbscanPoint(Of T))

            pool.Push(seedPoint)
            seedPoint.IsVisited = True

            Do While pool.Count > 0
                seedPoint = pool.Pop

                Dim knn() = FindKNearestNeighbors(seedPoint).ToArray
                Dim cutoff As Double = If(knn.Length = 0, Double.MaxValue, knn.Select(Function(a) a.dist).Median)
                Dim filter = (From a As (dist As Double, p As DbscanPoint(Of T))
                              In knn
                              Where a.dist <= cutoff
                              Select a.p).ToArray

                If filter.Length >= p Then
                    For Each neighborPoint As DbscanPoint(Of T) In filter
                        If Not neighborPoint.IsVisited Then
                            neighborPoint.ClusterId = seedPoint.ClusterId
                            neighborPoint.IsVisited = True
                            pool.Push(neighborPoint)
                        End If
                    Next
                End If
            Loop
        End Sub

        Private Function FindKNearestNeighbors(targetPoint As DbscanPoint(Of T)) As IEnumerable(Of (dist As Double, p As DbscanPoint(Of T)))
            Return From p As DbscanPoint(Of T)
                   In points
                   Where p IsNot targetPoint
                   Let d As Double = distanceMap(targetPoint, p)
                   Order By d
                   Select (d, p)
                   Take k
        End Function
    End Class

    Public Module KNNClusterRunner

        <Extension>
        Public Iterator Function MakeKNNCluster(data As IEnumerable(Of ClusterEntity), Optional k As Integer = 32, Optional p As Double = 0.8) As IEnumerable(Of ClusterEntity)
            Dim knn As New KNNCluster(Of ClusterEntity)(data, Function(a, b) a.SquareDistance(b), k, p)

            For Each point As DbscanPoint(Of ClusterEntity) In knn.AssignClusterId
                point.ClusterPoint.cluster = point.ClusterId
                Yield point.ClusterPoint
            Next
        End Function
    End Module
End Namespace