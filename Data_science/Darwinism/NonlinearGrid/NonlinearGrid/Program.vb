Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.NonlinearGridTopology
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure

Module Program

    Sub Main()
        Dim inFile As String = App.Command

        If Not inFile.FileExists Then
            Call "No input file was found!".PrintException
        Else
            Call runGA(inFile, $"{inFile.TrimSuffix}.minError.Xml")
        End If
    End Sub

    Private Sub runGA(inFile$, outFile$)
        Dim trainingSet = inFile.LoadXml(Of DataSet)
        Dim population As Population(Of Genome) = New Genome(Loader.EmptyGridSystem(trainingSet.width)).InitialPopulation(5000)
        Dim fitness As Fitness(Of Genome) = New Environment(trainingSet.DataSamples.AsEnumerable)
        Dim ga As New GeneticAlgorithm(Of Genome)(population, fitness)
        Dim engine As New EnvironmentDriver(Of Genome)(ga) With {
            .Iterations = 10000,
            .Threshold = 0.005
        }

        Call engine.AttachReporter(Sub(i, e, g)
                                       Call EnvironmentDriver(Of Genome).CreateReport(i, e, g).ToString.__DEBUG_ECHO
                                       Call g.Best _
                                             .CreateSnapshot(e) _
                                             .GetXml _
                                             .SaveTo(outFile)
                                   End Sub)
        Call engine.Train()
    End Sub
End Module
