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
