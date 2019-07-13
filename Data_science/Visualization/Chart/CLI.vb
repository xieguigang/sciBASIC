#Region "Microsoft.VisualBasic::7055cd412191253fce6a7a4bbf2cc2ce, Data_science\Visualization\Chart\CLI.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module CLI
    ' 
    '     Function: KMeansCluster, ROC, Scatter
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.KMeans

Module CLI

    <ExportAPI("/Scatter")>
    <Usage("/Scatter /in <data.csv> /x <fieldX> /y <fieldY> [/label.X <labelX> /label.Y <labelY> /color <default=black> /out <out.png>]")>
    <Description("")>
    Public Function Scatter(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim fx$ = args <= "/x"
        Dim fy$ = args <= "/y"
        Dim labelX$ = args("/label.X") Or fx
        Dim labelY$ = args("/label.Y") Or fy
        Dim color$ = args("/color") Or "black"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}_[{fx.NormalizePathString},{fy.NormalizePathString}].png"
        Dim csv = DataSet.LoadDataSet([in]).ToArray

        Throw New NotImplementedException
    End Function

    <ExportAPI("/kmeans")>
    <Usage("/kmeans /in <matrix.csv> [/n <expected_cluster_numbers, default=3> /out <clusters.csv>]")>
    <Group("Data tools")>
    Public Function KMeansCluster(args As CommandLine) As Integer
        Dim in$ = args("/in")
        Dim n% = args("/n") Or 5
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.kmeans.csv"
        Dim data As DataSet() = DataSet.LoadDataSet([in]).ToArray
        Dim clusters As IEnumerable(Of EntityClusterModel) =
            data _
            .ToKMeansModels _
            .Kmeans(expected:=n)

        Return clusters.SaveTo(out).CLICode
    End Function

    <ExportAPI("/ROC")>
    <Usage("/ROC /in <validate.test.csv> [/out <ROC.png>]")>
    Public Function ROC(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.ROC.png"
        Dim data = DataSet.LoadDataSet([in]).CreateSerial

        Return ROCPlot.Plot(data, showReference:=True).Save(out).CLICode
    End Function
End Module
