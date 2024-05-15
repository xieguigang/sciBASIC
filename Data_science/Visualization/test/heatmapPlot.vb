#Region "Microsoft.VisualBasic::6cfbb166664865bb4c2301d99e54e2a6, Data_science\Visualization\test\heatmapPlot.vb"

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

    '   Total Lines: 56
    '    Code Lines: 42
    ' Comment Lines: 1
    '   Blank Lines: 13
    '     File Size: 2.03 KB


    ' Module heatmapPlot
    ' 
    '     Sub: Main, plotCorrelation2
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics.Heatmap
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.MIME.Html.CSS

Module heatmapPlot

    Sub plotCorrelation2()
        Dim data = DataSet.LoadDataSet("D:\GCModeller\src\runtime\sciBASIC#\Data_science\Visualization\data\sample_groups.csv")
        Dim d = data.Correlation
        Dim theme As New Theme With {
            .padding = g.DefaultPadding,
            .tagCSS = CSSFont.PlotLabelNormal,
            .gridStrokeX = Stroke.AxisGridStroke,
            .axisTickCSS = CSSFont.PlotLabelNormal,
            .axisStroke = Stroke.AxisGridStroke,
            .background = "white",
            .colorSet = "RdBu:c8"
        }
        Dim heatmap As New CorrelationHeatmap(New CorrelationData(d), theme)

        Call heatmap.Plot("3600,3600").Save("D:\GCModeller\src\runtime\sciBASIC#\Data_science\Visualization\data\sample_groups_correlation.png")

        Pause()
    End Sub

    Sub Main()

        Call plotCorrelation2()

        Dim path = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\images\dendrogram\heatmap.Test.csv"
        Dim data = DataSet.LoadDataSet(path)
        Dim experiments As New Dictionary(Of String, String) From {
            {"T1", "red"},
            {"T2", "red"},
            {"T3", "red"},
            {"T4", "red"},
            {"K1", "blue"},
            {"K2", "blue"},
            {"K3", "blue"},
            {"K4", "blue"},
            {"average", "green"}
        }

        Call Heatmap.Plot(data, size:="3200,6000", reverseClrSeq:=True, drawScaleMethod:=DrawElements.Cols, drawClass:=(Nothing, experiments)).Save(path.TrimSuffix & ".png")

        ' Call AxisScalling.CreateAxisTicks({-10.3301, 13.7566}, 20).GetJson(True).__DEBUG_ECHO

        Pause()

    End Sub


End Module
