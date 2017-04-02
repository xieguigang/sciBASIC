Module Module1

    Public Sub Main()
        Dim frame As New JFrame
        frame.setSize(400, 300)
        frame.setLocation(400, 300)
        frame.DefaultCloseOperation = WindowConstants.EXIT_ON_CLOSE

        Dim content As New JPanel
        Dim dp As New DendrogramPanel

        frame.ContentPane = content
        content.Background = Java.awt.Color.red
        content.Layout = New Java.awt.BorderLayout
        content.add(dp, Java.awt.BorderLayout.CENTER)
        dp.Background = Java.awt.Color.WHITE
        dp.lineColor = Java.awt.Color.BLACK
        dp.scaleValueDecimals = 0
        dp.scaleValueInterval = 1
        dp.ShowDistances = False

        Dim cluster As com.apporiented.algorithm.clustering.Cluster = createSampleCluster()
        dp.model = cluster
        frame.Visible = True
    End Sub

    Private Function createSampleCluster() As com.apporiented.algorithm.clustering.Cluster
        Dim distances As Double()() = {New Double() {0, 1, 9, 7, 11, 14}, New Double() {1, 0, 4, 3, 8, 10}, New Double() {9, 4, 0, 9, 2, 8}, New Double() {7, 3, 9, 0, 6, 13}, New Double() {11, 8, 2, 6, 0, 10}, New Double() {14, 10, 8, 13, 10, 0}}
        Dim names As String() = {"O1", "O2", "O3", "O4", "O5", "O6"}
        Dim alg As com.apporiented.algorithm.clustering.ClusteringAlgorithm = New com.apporiented.algorithm.clustering.DefaultClusteringAlgorithm
        Dim cluster As com.apporiented.algorithm.clustering.Cluster = alg.performClustering(distances, names, New com.apporiented.algorithm.clustering.AverageLinkageStrategy)
        cluster.toConsole(0)
        Return cluster
    End Function
End Module
