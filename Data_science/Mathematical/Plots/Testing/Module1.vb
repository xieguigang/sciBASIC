#Region "Microsoft.VisualBasic::2c6bf70ffdd672151319686d419cc2ba, ..\sciBASIC#\Data_science\Mathematical\Plots\Testing\Module1.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D.Device
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.DocumentStream
Imports Microsoft.VisualBasic.DataMining.FuzzyCMeans
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical.Correlations
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Mathematical

Module Module1

    ''' <summary>
    ''' Assign random points within given range
    ''' </summary>
    ''' <param name="raw"></param>
    ''' <param name="rnd"></param>
    ''' <param name="n%"></param>
    ''' <param name="up%"></param>
    <Extension>
    Private Sub AddPoints(ByRef raw As List(Of Entity), rnd As Random, n%, up%)
        For i As Integer = 0 To n
            raw += New Entity With {
                .uid = i,
                .Properties = {
                    rnd.Next(0, up),
                    rnd.Next(0, up)
                }
            }
        Next
    End Sub

    Private Function CMeans() As (raw As Entity(), n%, trace As Dictionary(Of Integer, List(Of Entity)))
        Dim raw As New List(Of Entity)
        Dim rnd As New Random(Now.Millisecond)
        Dim up% = 1500

        ' initizlize of the points
        For i As Integer = 0 To 25
            Call raw.AddPoints(rnd, 50, up)
            up -= 50
        Next

        Dim n% = 10  ' required of 10 clusters
        Dim trace As New Dictionary(Of Integer, List(Of Entity))

        ' invoke cmeans cluster and gets the centra points
        Dim centras = raw.FuzzyCMeans(n, 2, trace:=trace)

#Region "DEBUG INFO OUTPUTS"
        For Each x In centras
            Call $"centra {x.uid} =>  {x.Properties.GetJson}".PrintException
        Next

        For Each x In raw
            Call ($"{x.uid}: {x.Properties.GetJson} => " & x.Memberships.GetJson).__DEBUG_ECHO
        Next
#End Region

        Return (raw, n, trace)
    End Function

    Private Sub CMeansVisualize()
        Call CMeans.Visualize
    End Sub

    <Extension>
    Private Sub Visualize(data As (raw As Entity(), n%, trace As Dictionary(Of Integer, List(Of Entity))))
        Dim trace As Dictionary(Of Integer, List(Of Entity)) = data.trace

        ' data plots visualize
        Dim plotData As New List(Of SerialData)
        ' using ColorBrewer color patterns from the visualbasic internal color designer
        Dim colors As Color() = Designer.GetColors("Paired:c10", data.n)

        ' generates serial data for each point in the raw inputs
        For Each x As Entity In data.raw
            Dim r = colors(x.ProbablyMembership).R
            Dim g = colors(x.ProbablyMembership).G
            Dim b = colors(x.ProbablyMembership).B
            Dim c As Color = Color.FromArgb(CInt(r), CInt(g), CInt(b))

            plotData += Scatter.FromPoints(
                {New PointF(x(0), x(1))},
                c.RGBExpression,
                ptSize:=30,
                title:="Point " & x.uid)
        Next

        Dim traceSerials As New List(Of List(Of Entity))

        For i As Integer = 0 To data.n - 1
            traceSerials += New List(Of Entity)
        Next

        For Each k In trace.Keys.OrderBy(Function(x) x)
            For i As Integer = 0 To data.n - 1
                traceSerials(i) += trace(k)(i)
            Next
        Next

        ' generates the serial data for each centra points
        For i = 0 To data.n - 1
            Dim points As IEnumerable(Of PointF) =
                traceSerials(i) _
                .Select(Function(x) New PointF(x(0), x(1)))
            plotData += Scatter.FromPoints(
                points,
                colors(i).RGBExpression,
                ptSize:=10,
                title:="Cluster " & i)
            plotData.Last.AddMarker(
                plotData.Last.pts.Last.pt.X,
                "Cluster " & i,
                "red",
                style:=LegendStyles.Triangle)
        Next

        Call Scatter.Plot(plotData, New Size(5000, 3000), fillPie:=True, showLegend:=False) _
            .SaveAs("./CMeans.png")
    End Sub


    Public Sub scatterHeatmapTest()
        Dim f As Func(Of Double, Double, Double) =
            Function(x, y) Math.Sqrt(x * x + y * y)

        Call ScatterHeatmap _
            .Plot(f, "(-10,10)", "(-10,10)", legendTitle:="z = x ^ 2 + y ^ 3") _
            .SaveAs("./scatter-heatmap.png")

        'Dim matrix As New List(Of DataSet)

        'Call ScatterHeatmap _
        '    .Plot("(x ^ 2) / (y + 10)", "(-1,1)", "(-5,1)",
        '          colorMap:="PRGn:c10",
        '          legendTitle:="z = x / (y + 1)",
        '          unit:=1,
        '          matrix:=matrix) _
        '    .SaveAs("./scatter-heatmap-exp.png")
        'Call matrix.SaveTo("./scatter-heatmap.matrix.csv")


        Pause()
    End Sub

    Sub d3heatmap()
        Dim func As Func(Of Double, Double, (Z#, Color#)) = Function(x, y) (3 * Math.Sin(x) * Math.Cos(y), Color:=x + y ^ 2)

        Call Plot3D.ScatterHeatmap.Plot(
    func, "-3,3", "-3,3",
    New Camera With {
        .screen = New Size(3600, 2500),
        .ViewDistance = -3.4,
        .angleZ = 30,
        .angleX = 30,
        .angleY = -30,
        .offset = New Point(0, -100)
    }, bg:="transparent", showLegend:=False) _
    .SaveAs("./3d-heatmap.png")
    End Sub

    Public Sub heatmap2()

        Dim data = LoadData("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\heatmap\Sample.csv", True)
        Dim spcc = data.CorrelationMatrix(AddressOf Spearman)

        Call HeatmapTable.Plot(spcc,) _
            .SaveAs("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\heatmap\Sample.SPCC.png")
        Call Heatmap.Plot(spcc, mapLevels:=25) _
            .SaveAs("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\heatmap\Sample.heatmap.png")

    End Sub


    Sub axisScallingTest()
        Call AxisScalling.GetAxisByTick(1, 0.1,).FormatNumeric.GetJson.__DEBUG_ECHO
        Call AxisScalling.GetAxisValues(1).FormatNumeric.GetJson.__DEBUG_ECHO
        Call AxisScalling.GetAxisValues(10).FormatNumeric.GetJson.__DEBUG_ECHO
        Call AxisScalling.GetAxisValues(100).FormatNumeric.GetJson.__DEBUG_ECHO
        Call AxisScalling.GetAxisValues(1000).FormatNumeric.GetJson.__DEBUG_ECHO
        Call AxisScalling.GetAxisValues(1.0E+30).FormatNumeric.GetJson.__DEBUG_ECHO
        Call AxisScalling.GetAxisValues(1.0E-30).FormatNumeric.GetJson.__DEBUG_ECHO
        Call AxisScalling.GetAxisValues(0.1).FormatNumeric.GetJson.__DEBUG_ECHO
        Call AxisScalling.GetAxisValues(0.25).FormatNumeric.GetJson.__DEBUG_ECHO

        Pause()
    End Sub

    Sub Main()
        Call axisScallingTest()

        '        Call heatmap2()

        ''Pause()

        'Call d3heatmap()
        'Pause()
        'Call scatterHeatmapTest()

        'Return

        'Call CMeansVisualize()
        'Pause()

        Dim datahm = Heatmap.LoadDataSet("C:\Users\xieguigang\OneDrive\1.5\hh.csv")
        Call Heatmap.Plot(datahm, mapName:=ColorMap.PatternHot,
                          kmeans:=AddressOf KmeansReorder,
                          mapLevels:=20,
                          margin:=New Size(300, 300),
                          legendTitle:="Spearman correlations",
                          fontStyle:=CSSFont.GetFontStyle(FontFace.BookmanOldStyle, FontStyle.Bold, 24)).SaveAs("x:\spcc.png")
        Pause()
        Dim data = csv.LoadBarData(
    "G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\Fruit_consumption.csv",
    {
        "rgb(124,181,236)",
        "rgb(67,67,72)",
        "gray"
    })

        Call BarPlot.Plot(data, size:=New Size(2000, 2500), stacked:=True) _
    .SaveAs("X:/Fruit_consumption-bar-stacked.png")

        Pause()


        Call {New csv.SerialData}.SaveTo("./template.csv")

        'Dim raw = "G:\GCModeller\src\runtime\visualbasic_App\Data_science\Mathematical\data\ManhattanStatics\example.csv".LoadCsv(Of csv.SerialData).ToArray

        'raw = csv.SerialData.Interpolation(raw)
        'Call raw.SaveTo("G:\GCModeller\src\runtime\visualbasic_App\Data_science\Mathematical\data\ManhattanStatics\example.csv")
        'Pause()
        Dim example = csv.SerialData.GetData("G:\GCModeller\src\runtime\visualbasic_App\Data_science\Mathematical\data\ManhattanStatics\example.csv", {Color.Red}, 5).First

        Call ManhattanStatics.Plot(example).SaveAs("G:\GCModeller\src\runtime\visualbasic_App\Data_science\Mathematical\data\ManhattanStatics/demo.png")

        Pause()
    End Sub
End Module
