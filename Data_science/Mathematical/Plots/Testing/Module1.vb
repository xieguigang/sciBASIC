#Region "Microsoft.VisualBasic::9ee41849dd8a7dcb5e5cf3c6947b481e, ..\sciBASIC#\Data_science\Mathematical\Plots\Testing\Module1.vb"

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
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.DataMining
Imports Microsoft.VisualBasic.DataMining.FuzzyCMeans
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

    <Extension>
    Private Sub AddPoints(raw As List(Of Entity), rnd As Random, n%, up%)
        For i As Integer = 0 To n
            raw += New Entity With {.uid = i, .Properties = {rnd.Next(0, up), rnd.Next(0, up)}}
        Next
    End Sub

    Private Sub cmeansVisualize()
        Dim raw As New List(Of Entity)
        Dim rnd As New Random(Now.Millisecond)
        Dim up% = 500

        For i As Integer = 0 To 10
            Call raw.AddPoints(rnd, 30, up)
            up -= 50
        Next

        Dim n = 10
        Dim trace As New Dictionary(Of Integer, List(Of Entity))
        Dim cccc = raw.FuzzyCMeans(n, 2, trace:=trace)

        For Each x In cccc
            Call $"centra {x.uid} =>  {x.Properties.GetJson}".PrintException
        Next

        For Each x In raw
            Call ($"{x.uid}: {x.Properties.GetJson} => " & x.Memberships.GetJson).__DEBUG_ECHO
        Next

        Dim s As New List(Of SerialData)

        For Each x In raw
            s += Scatter.FromPoints({New PointF(x.Properties(0), x.Properties(1))}, ptSize:=30)
        Next

        Dim ssssTrace As New List(Of List(Of KMeans.Entity))

        For i As Integer = 0 To n - 1
            ssssTrace += New List(Of KMeans.Entity)
        Next

        For Each k In trace.Keys.OrderBy(Function(x) x)
            For i As Integer = 0 To n - 1
                ssssTrace(i) += trace(k)(i)
            Next
        Next

        For i = 0 To n - 1
            s += Scatter.FromPoints(ssssTrace(i).Select(Function(x) New PointF(x.Properties(0), x.Properties(1))))
        Next

        Call Scatter.Plot(s, fillPie:=True, showLegend:=False).SaveAs("./cmeans.png")

    End Sub

    Sub Main()

        Call cmeansVisualize()
        Pause()

        Dim datahm = Heatmap.LoadDataSet("C:\Users\Admin\OneDrive\O'Connor\observation_correlates\spcc.csv")
        Call Heatmap.Plot(datahm, mapName:=ColorMap.PatternHot,
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
