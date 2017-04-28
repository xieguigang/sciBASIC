Imports System.Drawing
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.DendrogramVisualize
Imports Microsoft.VisualBasic.Imaging

Module Program

    Sub New()
        VBDebugger.Mute = True
    End Sub

    Public Sub Main()
        Dim cluster As Cluster = createSampleCluster()
        Dim dp As New DendrogramPanel With {
            .LineColor = Color.Black,
            .ScaleValueDecimals = 0,
            .ScaleValueInterval = 1,
            .Model = cluster
        }

        Dim g As Graphics2D = New Size(1024, 768).CreateGDIDevice(filled:=Color.White)
        Call dp.paint(g)
        Call g.Save("../../../test.png", ImageFormats.Png)

        Pause()
    End Sub

    Private Function createSampleCluster() As Cluster
        Dim distances = {
            {0#, 1, 9, 7, 11, 14},
            {1, 0, 4, 3, 8, 10},
            {9, 4, 0, 9, 2, 8},
            {7, 3, 9, 0, 6, 13},
            {11, 8, 2, 6, 0, 10},
            {14, 10, 8, 13, 10, 0}
        }
        Dim names$() = {"O1", "O2", "O3", "O4", "O5", "O6"}
        Dim alg As ClusteringAlgorithm = New DefaultClusteringAlgorithm
        Dim cluster As Cluster = alg.performClustering(
            distances.RowIterator.ToArray,
            names,
            New AverageLinkageStrategy)
        cluster.Print
        Return cluster
    End Function
End Module
