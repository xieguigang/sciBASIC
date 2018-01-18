Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

Public Module RegressionPlot

    <Extension>
    Public Function Plot(fit As FittedResult,
                         Optional size$ = "2100,1600",
                         Optional bg$ = "white",
                         Optional margin$ = g.DefaultPadding,
                         Optional xLabel$ = "X",
                         Optional yLabel$ = "Y",
                         Optional pointSize! = 5,
                         Optional pointBrushStyle$ = "red",
                         Optional errorFitPointStyle$ = "blue",
                         Optional predictPointStyle$ = "green",
                         Optional regressionLineStyle$ = "stroke: black; stroke-width: 2px; stroke-dash: solid;",
                         Optional predictPointStroke$ = "stroke: black; stroke-width: 2px; stroke-dash: dash;",
                         Optional predictedX#() = Nothing,
                         Optional showLegend As Boolean = True,
                         Optional legendLabelFontCSS$ = CSSFont.Win7LargerNormal) As GraphicsData

        Dim XTicks#() = fit.X.Range.CreateAxisTicks
        Dim YTicks#() = fit.Y.Range.CreateAxisTicks
        Dim pointBrush As Brush = pointBrushStyle.GetBrush
        Dim regressionPen As Pen = Stroke.TryParse(regressionLineStyle).GDIObject
        Dim predictedPointBorder As Pen = Stroke.TryParse(predictPointStroke).GDIObject
        Dim predictedBrush As Brush = predictPointStyle.GetBrush
        Dim errorFitPointBrush As Brush = errorFitPointStyle.GetBrush
        Dim legendLabelFont As Font = CSSFont.TryParse(legendLabelFontCSS)
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

                ' scatter plot
                For Each point As TestPoint In fit.ErrorTest
                    Dim pt As PointF = scaler.Translate(point)

                    g.DrawCircle(
                        centra:=pt,
                        r:=pointSize,
                        color:=pointBrush
                    )
                Next

                If fit.IsPolyFit Then
                    For Each t In XTicks.SlideWindows(2)
                        Dim A As New PointF With {.X = t(0), .Y = fit(.X)}
                        Dim B As New PointF With {.X = t(1), .Y = fit(.X)}

                        A = scaler.Translate(A)
                        B = scaler.Translate(B)

                        Call g.DrawLine(regressionPen, A, B)
                    Next
                Else
                    ' regression line
                    Dim A As New PointF With {.X = XTicks.Min, .Y = fit(.X)}
                    Dim B As New PointF With {.X = XTicks.Max, .Y = fit(.X)}

                    A = scaler.Translate(A)
                    B = scaler.Translate(B)

                    Call g.DrawLine(regressionPen, A, B)
                End If

                For Each point As TestPoint In fit.ErrorTest
                    Dim pt As New PointF With {
                        .X = point.X,
                        .Y = point.Yfit
                    }

                    pt = scaler.Translate(pt)

                    g.DrawCircle(
                        centra:=pt,
                        r:=pointSize,
                        color:=errorFitPointBrush
                    )
                    g.DrawCircle(
                        centra:=pt,
                        r:=pointSize,
                        color:=predictedPointBorder,
                        fill:=False
                    )
                Next

                If Not predictedX.IsNullOrEmpty Then
                    For Each ptX As Double In predictedX
                        Dim pt As New PointF With {.X = ptX, .Y = fit(.X)}

                        pt = scaler.Translate(pt)

                        g.DrawCircle(
                            centra:=pt,
                            r:=pointSize,
                            color:=predictedBrush
                        )
                        g.DrawCircle(
                            centra:=pt,
                            r:=pointSize,
                            color:=predictedPointBorder,
                            fill:=False
                        )
                    Next
                End If

                If showLegend Then
                    Dim eq$ = "f(x) = " & fit.Polynomial.ToString("G2")
                    Dim R2$ = "R2 = " & fit.R_square.ToString("F4")
                    Dim pt As New PointF With {
                        .X = rect.Left + 20,
                        .Y = rect.Top + 20
                    }

                    Call g.DrawString(eq, legendLabelFont, Brushes.Black, pt)

                    pt = New PointF With {
                        .X = pt.X,
                        .Y = pt.Y + legendLabelFont.Height + 5
                    }

                    Call g.DrawString(R2, legendLabelFont, Brushes.Black, pt)
                End If
            End Sub

        Return g.GraphicsPlots(
            size.SizeParser,
            margin,
            bg,
            plotInternal)
    End Function
End Module
