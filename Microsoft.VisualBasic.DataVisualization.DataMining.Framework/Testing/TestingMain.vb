Imports Microsoft.VisualBasic.DataVisualization.DataMining.Framework
Imports Microsoft.VisualBasic.DataVisualization.DataMining.Framework.ComponentModel
Imports Microsoft.VisualBasic.DataVisualization.DataMining.Framework.KMeans
Imports Microsoft.VisualBasic.DataVisualization.Network
Imports Microsoft.VisualBasic.DataVisualization.Network.FileStream
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.Linq

Module TestingMain

    Class Student : Inherits EntityBase(Of Double)

        Property Name As String

        Public Overrides Function ToString() As String
            Return Name
        End Function

        Public Shared Function [New](data As Double()) As Student
            Return New Student With {.Name = Rnd(), .Properties = data}
        End Function
    End Class

    Sub Main()

        Dim nnnnet = "E:\Microsoft.VisualBasic_Framework\Datavisualization\Datavisualization.Network\TestNET\Edges.csv".LoadCsv(Of NetworkEdge)
        Dim finderdfff = Dijkstra.DijkstraAPI.CreatePathwayFinder(Dijkstra.DijkstraAPI.ImportsNetwork(nnnnet))
        Dim pathssss = Dijkstra.DijkstraAPI.FindAllPath(finderdfff, "xcb_M00552", "xcb_M00024")

        Dim parts = TreeAPI.ClusterParts(nnnnet)


        Dim source = "F:\1.13.RegPrecise_network\MEME_OUT\Modules\100.MEME.ClusterMatrix\ClusterMatrix.Csv".LoadCsv(Of EntityLDM)
        Dim net = source.TreeNET

        Call net.Save("./test_tree/", Encodings.ASCII)


        Dim StudentA As New Student With {.Properties = New Double() {15, 32, 35.6}, .Name = "A"},
        StudentB As New Student With {.Properties = New Double() {55, 60, 65.1}, .Name = "B"},
        StudentC As New Student With {.Properties = New Double() {52, 57, 65.6}, .Name = "C"},
          StudentD As New Student With {.Properties = New Double() {14, 32, 35.6}, .Name = "D"},
            StudentE As New Student With {.Properties = New Double() {18, 32, 35.6}, .Name = "E"}


        Dim clusters As ClusterCollection(Of Student)

        Dim data = {StudentA, StudentB, StudentC, StudentD, StudentE}.Randomize

        clusters = KMeans.ClusterDataSet(2, data)


        Dim distance = KMeans.EuclideanDistance(StudentA.Properties, StudentB.Properties)

        Dim distances = KMeans.ManhattanDistance(StudentA.Properties, StudentB.Properties)

        Dim cluster = {{15, 32, 35.6}, {19, 54, 65.1}}
        Dim centroid = KMeans.ClusterMean(cluster)
        Console.Write("<br/>Cluster mean Calc: " + centroid.ToString())












        Dim nnn = 1.Sequence
        nnn = 0.Sequence

        nnn = (-1).Sequence

        Dim Data0 = Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream.File.FastLoad("E:\xcb_vcell\xcb_model\Result\MAT_OUT.csv")
        Dim MAT = Microsoft.VisualBasic.DataVisualization.DataMining.Framework.SerialsVarialble.Load(Data0)

        Dim datad = Microsoft.VisualBasic.DataVisualization.DataMining.Framework.BezierCurve.BezierSmoothInterpolation(MAT(1).SerialsData, 100)

        Call datad.SaveTo("./Bezier.csv")

        Dim DFT = New Microsoft.VisualBasic.DataVisualization.DataMining.Framework.TFftAlgorithm(datad)
        '   Call MAT(1).SerialsData.SaveTo("./vec.csv")

        Call DFT.FourierTransformation()

        Call DFT.a.SaveTo("./dft.a.csv")
        Call DFT.b.SaveTo("./dft.b.csv")
        Call DFT.cosine.SaveTo("./dft.cosine.csv")
        Call DFT.sine.SaveTo("./dft.sine.csv")
        Call DFT.xw.SaveTo("./dft.xw.csv")
        Call DFT.y.SaveTo("./dft.y.csv")


        Call Microsoft.VisualBasic.DataVisualization.DataMining.Framework.WaveletTransform.FWT(datad)
        Call datad.SaveTo("./wat.csv")

        Dim Factors = New List(Of Microsoft.VisualBasic.DataVisualization.DataMining.Framework.DFL_Driver.I_FactorElement)
        Call Factors.Add(New DataVisualization.DataMining.Framework.DFL_Driver.I_FactorElement() With {.Weight = 0.5}.set_Quantity(2))
        Call Factors.Add(New DataVisualization.DataMining.Framework.DFL_Driver.I_FactorElement() With {.Weight = 0.6}.set_Quantity(3))
        Call Factors.Add(New DataVisualization.DataMining.Framework.DFL_Driver.I_FactorElement() With {.Weight = -0.99}.set_Quantity(1))
        Call Factors.Add(New DataVisualization.DataMining.Framework.DFL_Driver.I_FactorElement() With {.Weight = -0.1}.set_Quantity(8))
        Call Factors.Add(New DataVisualization.DataMining.Framework.DFL_Driver.I_FactorElement() With {.Weight = 0.2}.set_Quantity(2))
        Call Factors.Add(New DataVisualization.DataMining.Framework.DFL_Driver.I_FactorElement() With {.Weight = 1}.set_Quantity(0.4))

        Dim node = New Microsoft.VisualBasic.DataVisualization.DataMining.Framework.DFL_Driver.dflNode(Factors)

        Call Console.WriteLine(node.State)
        Call Console.Read()
    End Sub
End Module
