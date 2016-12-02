#Region "Microsoft.VisualBasic::27b7cdb4a53b2880129c75747cf76e23, ..\sciBASIC#\Data_science\Mathematical\Plots\Testing\Module1.vb"

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
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Module Module1

    Sub Main()

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

