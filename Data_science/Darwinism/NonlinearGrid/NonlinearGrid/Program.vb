#Region "Microsoft.VisualBasic::03976b42f1e58a8561e28d2d7f3a6600, Data_science\Darwinism\NonlinearGrid\NonlinearGrid\Program.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:

' Module Program
' 
'     Function: DumpAsNetwork, ExportFactorImpact, Main, trainGA, ValidationSummary
' 
'     Sub: RunFitProcess
' 
' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.ReplacementStrategy
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.NonlinearGridTopology
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.Text

<CLI>
<Description("")>
Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    <ExportAPI("/formula.R")>
    <Usage("/formula.R /in <model.Xml> [/out <formula.R>]")>
    Public Function Formula(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim out$ = args("/out") Or ([in].TrimSuffix & ".formula.R")
        Dim model As GridMatrix = [in].LoadXml(Of GridMatrix)

        Return model _
            .ToString(lang:=Languages.R) _
            .SaveTo(out, Encoding.ASCII) _
            .CLICode
    End Function

    <ExportAPI("/dump.network")>
    <Usage("/dump.network /in <model.Xml> [/threshold <default=1> /out <out.directory>]")>
    <Description("Export a correlation network from a grid system model.")>
    Public Function DumpAsNetwork(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim threshold As Double = args("/threshold")
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.threshold={threshold}.network/"
        Dim matrix As GridMatrix = [in].LoadXml(Of GridMatrix)
        Dim graph As NetworkGraph = matrix.CreateGraph(cutoff:=threshold)
        Dim network = graph.Tabular({"impacts", "correlation"})

        Return network.Save(out, Encodings.ASCII).CLICode
    End Function

    <ExportAPI("/factor.impacts")>
    <Usage("/factor.impacts /in <model.Xml> [/order <asc/desc> /correlation /out <out.csv>]")>
    Public Function ExportFactorImpact(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim model = [in].LoadXml(Of GridMatrix)
        Dim isCor As Boolean = args("/correlation")
        Dim impacts As NamedValue(Of Double)()

        If isCor Then
            impacts = model.NodeCorrelation.ToArray
        Else
            impacts = model.NodeImpacts.ToArray
        End If

        If args.ContainsParameter("/order") Then
            If args("/order").DefaultValue.TextEquals("asc") Then
                impacts = impacts.OrderBy(Function(x) x.Value).ToArray
            Else
                impacts = impacts.OrderByDescending(Function(x) x.Value).ToArray
            End If
        End If

        With args <= "/out"
            If .StringEmpty Then
                Call impacts.ToCsvDoc _
                    .AsMatrix _
                    .Select(Function(r) r.ToArray) _
                    .PrintTable
            Else
                Call impacts.SaveTo(.ByRef)
            End If
        End With

        Return 0
    End Function

    <ExportAPI("/validates.ROC")>
    <Usage("/validates.ROC /validates <fittingValitation.csv> [/out <output.csv>]")>
    Public Function ValidationROC(args As CommandLine) As Integer
        Dim outputResult$ = args("/validates")
        Dim out$ = args("/out") Or (outputResult.TrimSuffix & ".validation_ROC.csv")
        Dim result = outputResult.LoadCsv(Of FittingValidation)
        Dim ROC = result.ROC _
            .Select(Function(d)
                        Return New IO.DataSet With {
                            .ID = d.Threshold,
                            .Properties = d.ToDataSet
                        }
                    End Function) _
            .ToArray

        Return ROC.SaveTo(out).CLICode
    End Function

    <ExportAPI("/validates")>
    <Usage("/validates /in <model.Xml> /data <trainingSet.Xml> [/order <asc/desc> /out <out.csv>]")>
    <Description("Do model validations.")>
    Public Function ValidationSummary(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim data$ = args <= "/data"
        Dim model = [in].LoadXml(Of GridMatrix).CreateSystem
        Dim dataset = data.LoadXml(Of DataSet)
        Dim summaryResult = dataset.DataSamples _
            .Select(Function(sample, i)
                        Dim result = model.Evaluate(sample.status.vector)

                        Return New FittingValidation With {
                            .sampleID = sample.ID,
                            .actual = sample.target(Scan0),
                            .predicts = result
                        }
                    End Function) _
            .ToArray
        Dim R2Test = dataset.DataSamples _
            .Select(Function(sample, i)
                        Return New TrainingSet(sample)
                    End Function) _
            .ToArray _
            .DoCall(apply:=Function(d) model.R2(d, True))

        If args.ContainsParameter("/order") Then
            If args("/order").DefaultValue.TextEquals("asc") Then
                summaryResult = summaryResult.OrderBy(Function(r) r.errors).ToArray
            Else
                summaryResult = summaryResult _
                    .OrderByDescending(Function(r) r.errors) _
                    .ToArray
            End If
        End If

        With args <= "/out"
            If .StringEmpty Then
                Dim strings = summaryResult.ToCsvDoc _
                    .AsMatrix _
                    .Select(Function(r) r.ToArray) _
                    .ToArray

                Call strings.PrintTable
            Else
                Call summaryResult.SaveTo(.ByRef)
            End If
        End With

        Call VBDebugger.WaitOutput()
        Call Console.WriteLine()
        Call Console.WriteLine($"DataFitting Errors={summaryResult.GroupBy(Function(r) r.actual.ToString).Select(Function(g) g.Select(Function(r) r.errors).Average).Average}")
        Call Console.WriteLine($"R2_error={R2Test}")
        Call Console.WriteLine()
        Call summaryResult.Select(Function(r) r.errors).Summary

        Return 0
    End Function

    <ExportAPI("/training")>
    <Usage("/training /in <trainingSet.Xml> [/model <model.XML> /popSize <default=5000> /rate <default=0.1> /truncate <default=1000> /out <output_model.Xml>]")>
    <Description("Training a grid system use GA method.")>
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
        Dim rate As Double = args("/rate") Or 0.1
        Dim truncate As Double = args("/truncate") Or 1000.0

        Call $"Mutation rate = {rate}".__DEBUG_ECHO
        Call $"Population size = {popSize}".__DEBUG_ECHO
        Call trainingSet _
            .RunFitProcess(
                out, seed, popSize,
                factorNames:=trainingSet.NormalizeMatrix.names,
                mutationRate:=rate,
                truncate:=truncate
        )

        Return 0
    End Function

    <Extension>
    Public Sub RunFitProcess(trainingSet As DataSet, outFile$, seed As GridSystem, popSize%, factorNames$(), mutationRate As Double, truncate As Double)
        Dim cor As Vector = trainingSet.DataSamples.AsEnumerable.Correlation
        Dim max As Vector = trainingSet.NormalizeMatrix.matrix.Select(Function(r) 1 / (r.max * 1000)).AsVector
        Call "Create a base chromosome".__DEBUG_ECHO
        Dim chromesome As GridSystem = If(seed, Loader.EmptyGridSystem(trainingSet.width, cor, max))
        Call "Initialize populations".__DEBUG_ECHO
        Call $"value truncate at ABS limits {truncate}".__DEBUG_ECHO
        Dim population As Population(Of Genome) = New Genome(chromesome, mutationRate, truncate).InitialPopulation(popSize, parallel:=True)
        Call "Initialize environment".__DEBUG_ECHO
        Dim fitness As Fitness(Of Genome) = New Environment(trainingSet.DataSamples.AsEnumerable, FitnessMethods.LabelGroupAverage)
        Call "Create algorithm engine".__DEBUG_ECHO
        Dim ga As New GeneticAlgorithm(Of Genome)(population, fitness, Strategies.Naive)
        Call "Load driver".__DEBUG_ECHO

        Dim takeBestSnapshot = Sub(best As Genome, error#)
                                   Call best _
                                       .CreateSnapshot(factorNames, [error]) _
                                       .GetXml _
                                       .SaveTo(outFile.TrimSuffix & $"_localOptimal/{[error]}.Xml")
                               End Sub
        Dim engine As New EnvironmentDriver(Of Genome)(ga, takeBestSnapshot) With {
            .Iterations = 1000000,
            .Threshold = 0.005
        }

        Call engine.AttachReporter(Sub(i, e, g)
                                       Call EnvironmentDriver(Of Genome).CreateReport(i, e, g).ToString.__DEBUG_ECHO
                                       Call g.Best _
                                             .CreateSnapshot(factorNames, e) _
                                             .GetXml _
                                             .SaveTo(outFile)
                                   End Sub)

        Call "Run GA!".__DEBUG_ECHO
        Call engine.Train()
    End Sub
End Module
