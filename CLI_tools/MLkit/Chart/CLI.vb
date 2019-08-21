#Region "Microsoft.VisualBasic::ea4b1b523082c25071bbb2405d2fa9dc, CLI_tools\MLkit\Chart\CLI.vb"

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

' Module CLI
' 
'     Function: BarPlotCLI, KMeansCluster, ROC, Scatter
' 
' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Data
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Evaluation
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Text.Xml.Models

<CLI> Module CLI

    <ExportAPI("/barplot")>
    <Usage("/barplot /in <data.csv> [/name <default=Name> /value <default=Value> /size <default=2000,1700> /out <plot.png>]")>
    Public Function BarPlotCLI(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim name$ = args("/name") Or "Name"
        Dim value$ = args("/value") Or "Value"
        Dim size$ = args("/size") Or "2000,1700"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.barplot.png"
        Dim data As NamedValue(Of Double)() = EntityObject _
            .LoadDataSet([in], uidMap:=name) _
            .Select(Function(d)
                        Return New NamedValue(Of Double) With {
                            .Name = d.ID,
                            .Value = d(value).ParseDouble
                        }
                    End Function) _
            .ToArray
        Dim barData As BarDataGroup = data.SimpleSerials
        Dim image = BarPlotAPI.Plot(barData, size.SizeParser)

        Return image.Save(out).CLICode
    End Function

    <ExportAPI("/Scatter")>
    <Usage("/Scatter /in <data.csv> /x <fieldX> /y <fieldY> [/label.X <labelX> /label.Y <labelY> /color <default=black> /out <out.png>]")>
    <Description("Scatter plot based on a given dataset.")>
    <Argument("/in", False, CLITypes.File, PipelineTypes.std_in,
              AcceptTypes:={GetType(DataSet)},
              Extensions:="*.csv",
              Description:="The target dataset table. And this dataset file should contains two fields for X,Y data at least.")>
    <Argument("/x", False, CLITypes.String,
              AcceptTypes:={GetType(String)},
              Description:="The column name for read X data in target dataset.")>
    <Argument("/y", False, CLITypes.String,
              AcceptTypes:={GetType(String)},
              Description:="The column name for read Y data in target dataset.")>
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
    <Description("Draw ROC chart plot.")>
    <Argument("/in", False, CLITypes.File, PipelineTypes.std_in,
              AcceptTypes:={GetType(DataSet)},
              Extensions:="*.csv",
              Description:="This file should contains at least two fields: ``Specificity`` and ``Sensibility``.")>
    Public Function ROC(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.ROC.png"
        Dim data = DataSet.LoadDataSet([in]).CreateSerial

        Return ROCPlot.Plot(data, showReference:=True) _
            .Save(out) _
            .CLICode
    End Function

    <ExportAPI("/ROC.regression")>
    <Usage("/ROC.regression /in <validate.test.csv> [/out <ROC.png>]")>
    <Description("Draw ROC chart plot of the regression classifier output result.")>
    <Argument("/in", False, CLITypes.File, PipelineTypes.std_in,
              AcceptTypes:={GetType(RegressionClassify)},
              Extensions:="*.csv",
              Description:="")>
    Public Function RegressionROC(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.ROC.png"
        Dim data = [in].LoadCsv(Of RegressionClassify) _
            .Select(Function(p)
                        Return New Validate With {
                            .actuals = {p.actual},
                            .predicts = {p.predicts}
                        }
                    End Function) _
            .ToArray
        Dim actuals = data _
            .Select(Function(p) p.actuals(Scan0)) _
            .ToArray
        Dim points As New Sequence With {
            .n = 100,
            .range = New DoubleRange(actuals)
        }
        Dim validation As NamedCollection(Of Validation) = Validate.ROC(data, threshold:=points).First
        Dim serials As SerialData = validation.CreateSerial _
            .With(Sub(sr)
                      sr.title = Validate.AUC(data).First.Value
                  End Sub)

        Return ROCPlot.Plot(serials, showReference:=True) _
            .Save(out) _
            .CLICode
    End Function
End Module
