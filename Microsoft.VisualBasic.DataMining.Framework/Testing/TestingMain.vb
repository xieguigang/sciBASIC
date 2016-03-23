Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.DataMining.Framework
Imports Microsoft.VisualBasic.DataMining.Framework.QLearning
Imports Microsoft.VisualBasic.DataMining.Framework.ComponentModel
Imports Microsoft.VisualBasic.DataMining.Framework.KMeans
Imports Microsoft.VisualBasic.DataVisualization.Network
Imports Microsoft.VisualBasic.DataVisualization.Network.FileStream
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.DataMining.Framework.FuzzyLogic

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
        'Dim app As New QLearning(Of   )
        'Try
        '    app.runLearningLoop()
        'Catch e As Exception
        '    Console.WriteLine("Thread.sleep interrupted!")
        'End Try

        Dim water As New LinguisticVariable("Water")
        water.MembershipFunctionCollection.Add(New MembershipFunction("Cold", 0, 0, 20, 40))
        water.MembershipFunctionCollection.Add(New MembershipFunction("Tepid", 30, 50, 50, 70))
        water.MembershipFunctionCollection.Add(New MembershipFunction("Hot", 50, 80, 100, 100))

        Dim power As LinguisticVariable = New LinguisticVariable("Power")
        power.MembershipFunctionCollection.Add(New MembershipFunction("Low", 0, 25, 25, 50))
        power.MembershipFunctionCollection.Add(New MembershipFunction("High", 25, 50, 50, 75))

        Dim FuzzyEngine As New FuzzyEngine()
        FuzzyEngine.LinguisticVariableCollection.Add(water)
        FuzzyEngine.LinguisticVariableCollection.Add(power)
        FuzzyEngine.Consequent = "Power"
        FuzzyEngine.FuzzyRuleCollection.Add(New FuzzyRule("IF (Water IS Cold) OR (Water IS Tepid) THEN Power IS High"))
        FuzzyEngine.FuzzyRuleCollection.Add(New FuzzyRule("IF (Water IS Hot) THEN Power IS Low"))

        water.InputValue = 60

        Call FuzzyEngine.Save("x:\fff.xml", Encodings.UTF8)

        FuzzyEngine = Nothing
        FuzzyEngine = Models.FuzzyModel.FromXml("x:\fff.xml")

        Try
            MsgBox(FuzzyEngine.Defuzzify().ToString())
        Catch ex As Exception
            Call ex.PrintException
        End Try


        Dim v As New List(Of Value(Of String))

            v += New Value(Of String)("1234")
            v += New Value(Of String)("369")
            v -= v(0)

            Dim nnnnet =
                "F:\1.13.RegPrecise_network\MEME_OUT\Regulons.MEME\250.MEME_SW-TOM.OUT\SW-TOM.Hits.VirtualFootprints.Trim.PhenotypeRegulates.TreeNET\Edges.csv".LoadCsv(Of NetworkEdge)
            '  Dim finderdfff = Dijkstra.DijkstraAPI.CreatePathwayFinder(Dijkstra.DijkstraAPI.ImportsNetwork(nnnnet, 1), True)
            '  Dim pathssss = Dijkstra.DijkstraAPI.FindAllPath(finderdfff, "xcb_M00552", "xcb_M00024")

            Dim parts = TreeAPI.ClusterParts(nnnnet)

            Dim ssssst = (From x In parts Select x.Value).MatrixAsIterator.Distinct.ToArray.Length
            Call ssssst.__DEBUG_ECHO


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
            Dim MAT = Microsoft.VisualBasic.DataMining.Framework.Serials.PeriodAnalysis.SerialsVarialble.Load(Data0)

            Dim datad = Microsoft.VisualBasic.DataMining.Framework.BezierCurve.BezierSmoothInterpolation(MAT(1).SerialsData, 100)

            Call datad.SaveTo("./Bezier.csv")

            Dim DFT = New Microsoft.VisualBasic.DataMining.Framework.TFftAlgorithm(datad)
            '   Call MAT(1).SerialsData.SaveTo("./vec.csv")

            Call DFT.FourierTransformation()

            Call DFT.a.SaveTo("./dft.a.csv")
            Call DFT.b.SaveTo("./dft.b.csv")
            Call DFT.cosine.SaveTo("./dft.cosine.csv")
            Call DFT.sine.SaveTo("./dft.sine.csv")
            Call DFT.xw.SaveTo("./dft.xw.csv")
            Call DFT.y.SaveTo("./dft.y.csv")


            Call Microsoft.VisualBasic.DataMining.Framework.WaveletTransform.FWT(datad)
            Call datad.SaveTo("./wat.csv")

            Dim Factors = New List(Of Microsoft.VisualBasic.DataMining.Framework.DFL_Driver.I_FactorElement)
            Call Factors.Add(New DataMining.Framework.DFL_Driver.I_FactorElement() With {.Weight = 0.5}.set_Quantity(2))
            Call Factors.Add(New DataMining.Framework.DFL_Driver.I_FactorElement() With {.Weight = 0.6}.set_Quantity(3))
            Call Factors.Add(New DataMining.Framework.DFL_Driver.I_FactorElement() With {.Weight = -0.99}.set_Quantity(1))
            Call Factors.Add(New DataMining.Framework.DFL_Driver.I_FactorElement() With {.Weight = -0.1}.set_Quantity(8))
            Call Factors.Add(New DataMining.Framework.DFL_Driver.I_FactorElement() With {.Weight = 0.2}.set_Quantity(2))
            Call Factors.Add(New DataMining.Framework.DFL_Driver.I_FactorElement() With {.Weight = 1}.set_Quantity(0.4))

            Dim node = New DataMining.Framework.DFL_Driver.dflNode(Factors)

            Call Console.WriteLine(node.State)
            Call Console.Read()
    End Sub
End Module
