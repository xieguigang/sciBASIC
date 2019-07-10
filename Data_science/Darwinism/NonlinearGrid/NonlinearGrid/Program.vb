Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.NonlinearGridTopology
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure
Imports Table = Microsoft.VisualBasic.Data.csv.IO.DataSet

Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    <ExportAPI("/summary")>
    <Usage("/summary /in <model.Xml> /data <trainingSet.Xml>")>
    Public Function Summary(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim data$ = args <= "/data"
        Dim model = [in].LoadXml(Of GridMatrix).CreateSystem
        Dim dataset = data.LoadXml(Of DataSet)
        Dim summaryResult = dataset.DataSamples _
            .Select(Function(sample, i)
                        Dim result = model.Evaluate(sample.status.vector)

                        Return New Table With {
                            .ID = sample.ID,
                            .Properties = New Dictionary(Of String, Double) From {
                                {"actual", sample.target(Scan0)},
                                {"fit", result},
                                {"errors", Math.Abs(sample.target(Scan0) - result)}
                            }
                        }
                    End Function) _
            .ToArray
        Dim strings = summaryResult.ToCsvDoc _
            .AsMatrix _
            .Select(Function(r) r.ToArray) _
            .ToArray

        Call strings.PrintTable

        Return 0
    End Function

    <ExportAPI("/training")>
    <Usage("/training /in <trainingSet.Xml> [/model <model.XML> /popSize <default=5000> /out <output_model.Xml>]")>
    Public Function trainGA(args As CommandLine) As Integer
        Dim inFile As String = args <= "/in"
        Dim out$ = args("/out") Or $"{inFile.TrimSuffix}.minError.Xml"
        Dim model$ = args("/model")
        Dim seed As GridSystem = Nothing
        Dim popSize% = args("/popSize") Or 5000

        If Not inFile.FileExists Then
            Call "No input file was found!".PrintException
        Else
            seed = If(model.FileExists, model.LoadXml(Of GridMatrix).CreateSystem, Nothing)
        End If

        Dim trainingSet = inFile.LoadXml(Of DataSet)
        Dim chromesome As GridSystem = If(seed, Loader.EmptyGridSystem(trainingSet.width))
        Dim population As Population(Of Genome) = New Genome(chromesome).InitialPopulation(popSize)
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

        Return 0
    End Function
End Module
