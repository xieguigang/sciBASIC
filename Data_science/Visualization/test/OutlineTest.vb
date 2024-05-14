#Region "Microsoft.VisualBasic::6e0c49b1ebc1c6a8060f80b31f0c190a, Data_science\Visualization\test\OutlineTest.vb"

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

    '   Total Lines: 53
    '    Code Lines: 41
    ' Comment Lines: 1
    '   Blank Lines: 11
    '     File Size: 1.97 KB


    ' Module OutlineTest
    ' 
    '     Sub: Main, plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.Contour
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.ChartPlots.Plots
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.MarchingSquares

Public Module OutlineTest

    Const baseDir As String = "D:\GCModeller\src\runtime\sciBASIC#\Data_science\Visualization\data\contour_outlines"

    Sub Main()
        Call plot("region_9")
        Call plot("region_11")
        Call plot("region_2")

        Pause()

    End Sub

    Sub plot(region As String)

        Dim matrix As DataSet() = DataSet.LoadMatrix($"{baseDir}/{region}.csv").ToArray
        Dim x As Double() = matrix.Vector("X")
        Dim y As Double() = matrix.Vector("Y")
        Dim scatter As New SerialData With {
            .color = Color.Red,
            .pointSize = 30,
            .pts = x _
                .Select(Function(xi, i)
                            Return New PointData With {
                                .pt = New PointF(xi, y(i))
                            }
                        End Function) _
                .ToArray,
            .shape = LegendStyles.Square,
            .title = "region 9"
        }
        Dim theme As New Theme With {.padding = "padding: 200px 200px 300px 300px;", .drawLegend = False}
        Dim app As New Scatter2D({scatter}, theme, scatterReorder:=True, fillPie:=True)

        ' raw scatter
        Call app.Plot.Save($"{baseDir}/raw+{region}.png")

        Dim outline = ContourLayer.GetOutline(x, y, fillSize:=2)
        Dim contour As New ContourPlot({outline}, New Theme With {.padding = "padding: 200px 800px 200px 200px;"})

        Call contour.Plot.Save($"{baseDir}/outline+{region}.png")
    End Sub

End Module
