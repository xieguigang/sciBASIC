Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Scripting.Runtime

Public Module RegressionPlot

    <Extension>
    Public Function Plot(fit As FittedResult,
                         Optional size$ = "2000,1800",
                         Optional bg$ = "white",
                         Optional margin$ = g.DefaultPadding,
                         Optional xLabel$ = "X",
                         Optional yLabel$ = "Y",
                         Optional pointSize! = 5,
                         Optional pointBrushStyle$ = "red") As GraphicsData

        Dim XTicks#() = fit.X.Range.CreateAxisTicks
        Dim YTicks#() = fit.Y.Range.CreateAxisTicks
        Dim pointBrush As Brush = pointBrushStyle.GetBrush

        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)
                Dim rect = region.PlotRegion
                Dim X = d3js.scale.linear.domain(XTicks).range(integers:={rect.Left, rect.Right})
                Dim Y = d3js.scale.linear.domain(YTicks).range(integers:={rect.Top, rect.Bottom})
                Dim scaler As New DataScaler With {
                    .X = X,
                    .Y = Y,
                    .Region = rect,
                    .AxisTicks = (XTicks, YTicks)
                }

                Call g.DrawAxis(
                    region, scaler, True,
                    xlabel:=xLabel, ylabel:=yLabel,
                    htmlLabel:=False
                )

                For Each point As TestPoint In fit.ErrorTest
                    Dim pt As PointF = scaler.Translate(point)

                    g.DrawCircle(
                        centra:=pt,
                        r:=pointSize,
                        color:=pointBrush
                    )
                Next
            End Sub

        Return g.GraphicsPlots(
            size.SizeParser,
            margin,
            bg,
            plotInternal)
    End Function
End Module
