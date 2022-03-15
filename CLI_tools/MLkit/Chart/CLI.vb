#Region "Microsoft.VisualBasic::462fa230031ad5f91e550b9611ccc819, sciBASIC#\CLI_tools\MLkit\Chart\CLI.vb"

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


    ' Code Statistics:

    '   Total Lines: 239
    '    Code Lines: 218
    ' Comment Lines: 0
    '   Blank Lines: 21
    '     File Size: 11.48 KB


    ' Module CLI
    ' 
    '     Function: BarPlotCLI, KMeansCluster, LinePlot, RegressionROC, ROC
    '               Scatter
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Data
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Evaluation
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
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

    <ExportAPI("/lines")>
    <Usage("/lines /in <source.csv> /x <label> /y <label.list> [/color <colorsetname, default=> /out <output.png>]")>
    Public Function LinePlot(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim xLabel$ = args <= "/x"
        Dim yLabels$() = Tokenizer.CharsParser(args <= "/y")
        Dim colors As LoopArray(Of Color) = Designer.GetColors(args("/color") Or "")
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.plot({xLabel.NormalizePathString}, {yLabels.JoinBy(",").NormalizePathString(False)}).png"
        Dim table As File = File.Load([in])
        Dim x As Double() = table.GetColumnObjects(xLabel, AddressOf Val).ToArray
        Dim serials = yLabels _
            .Select(Function(ylabel)
                        Dim y = table.GetColumnObjects(ylabel, AddressOf Val).ToArray
                        Dim points = x.Select(Function(xi, i) New PointF(xi, y(i))).ToArray
                        Dim serial As New SerialData With {
                            .color = colors.Next(),
                            .lineType = DashStyle.Solid,
                            .pointSize = 10,
                            .title = ylabel,
                            .width = 5,
                            .shape = LegendStyles.Triangle,
                            .pts = points _
                                .Select(Function(p)
                                            Return New PointData With {
                                                .pt = p
                                            }
                                        End Function) _
                                .ToArray
                        }

                        Return serial
                    End Function) _
            .ToArray

        Return ChartPlots.Scatter _
            .Plot(serials, drawLine:=True, fill:=False) _
            .Save(out) _
            .CLICode
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
        Dim data = DataSet.LoadDataSet([in]).ToArray

        If data.Length = 0 Then
            Throw New EntryPointNotFoundException($"The input data file '{[in].GetFullPath}' is not found on your file system!")
        End If

        Dim curveSerial As SerialData = data.CreateSerial

        Return ROCPlot.Plot(curveSerial, showReference:=True) _
            .Save(out) _
            .CLICode
    End Function

    <ExportAPI("/ROC.regression")>
    <Usage("/ROC.regression /in <validate.test.csv> [/label.predicts <labelName> /label.actuals <labelName> /positive.pattern <actual_label.pattern> /out <ROC.png>]")>
    <Description("Draw ROC chart plot of the regression classifier output result.")>
    <Argument("/in", False, CLITypes.File, PipelineTypes.std_in,
              AcceptTypes:={GetType(RegressionClassify), GetType(DataSet)},
              Extensions:="*.csv",
              Description:="")>
    <Argument("/label.predicts", True, CLITypes.String,
              AcceptTypes:={GetType(String)},
              Description:="The column title name label for read sample dataset predicts data.")>
    <Argument("/label.actuals", True, CLITypes.String,
              AcceptTypes:={GetType(String)},
              Description:="The column title name label for read sample dataset actual classify labels. 
              The value in this column should be integer value. ALL ZERO value is negative labels.")>
    <Argument("/positive.pattern", True, CLITypes.String,
              AcceptTypes:={GetType(String)},
              Description:="If the ``/label.predicts`` is not the integer labels value, 
              but have sample id data, and the positive label can be parse from the id data, 
              then you could specific this argument a regular expression pattern for parse such positive pattern.")>
    Public Function RegressionROC(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.ROC.png"
        Dim data As Validate()

        If MappingsHelper.Typeof([in], GetType(RegressionClassify)) Is GetType(RegressionClassify) Then
            data = [in].LoadCsv(Of RegressionClassify) _
                .Select(Function(p)
                            Return New Validate With {
                                .actuals = {p.actual},
                                .predicts = {p.predicts}
                            }
                        End Function) _
                .ToArray
        Else
            Dim labelPredicts$ = args <= "/label.predicts"
            Dim labelActuals$ = args <= "/label.actuals"
            Dim positivePattern$ = args <= "/positive.pattern"
            Dim resultSet As EntityObject() = EntityObject.LoadDataSet([in]).ToArray
            Dim parseActual As Func(Of String, Double)

            If Not positivePattern.StringEmpty Then
                Dim allPredictsValues = resultSet.Vector(labelPredicts).AsDouble
                Dim positive = allPredictsValues.Max
                Dim negative = Math.Min(allPredictsValues.Min, 0)
                Dim pattern As New Regex(positivePattern, RegexICSng)

                parseActual = Function(label As String) As Double
                                  If pattern.Match(label).Success Then
                                      Return positive
                                  Else
                                      Return negative
                                  End If
                              End Function
            Else
                parseActual = AddressOf Val
            End If

            data = resultSet _
                .Select(Function(r)
                            Return New Validate With {
                                .actuals = {parseActual(r(labelActuals))},
                                .predicts = {Val(r(labelPredicts))}
                            }
                        End Function) _
                .ToArray
        End If

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
