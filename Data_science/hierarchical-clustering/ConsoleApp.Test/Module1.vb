Imports System.Drawing
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.DendrogramVisualize

Module Module1

    Public Sub Main()
        Dim dp As New DendrogramPanel With {
            .Size = New Size(1024, 768)
        }

        dp.LineColor = Color.Black
        dp.ScaleValueDecimals = 0
        dp.ScaleValueInterval = 1

        Dim cluster As Cluster = createSampleCluster()
        dp.Model = cluster
    End Sub

    Private Function createSampleCluster() As Cluster
        Dim distances As Double()() = {New Double() {0, 1, 9, 7, 11, 14}, New Double() {1, 0, 4, 3, 8, 10}, New Double() {9, 4, 0, 9, 2, 8}, New Double() {7, 3, 9, 0, 6, 13}, New Double() {11, 8, 2, 6, 0, 10}, New Double() {14, 10, 8, 13, 10, 0}}
        Dim names As String() = {"O1", "O2", "O3", "O4", "O5", "O6"}
        Dim alg As ClusteringAlgorithm = New DefaultClusteringAlgorithm
        Dim cluster As Cluster = alg.performClustering(distances, names, New AverageLinkageStrategy)
        cluster.toConsole(0)
        Return cluster
    End Function
End Module
