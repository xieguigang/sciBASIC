Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Statistics.MomentFunctions
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

''' <summary>
''' 通过正态分布曲线和散点图来可视化用户的样品数据
''' </summary>
Public Module SampleView

    Const defaultMeanLineStyle$ = "stroke: green; stroke-width: 2px; stroke-dash: dash;"
    Const defaultNormalDistLineStyle$ = "stroke: " & NameOf(Color.Purple) & "; stroke-width: 2px; stroke-dash: dash;"
    Const outlierLineStyle$ = "stroke: red; stroke-width: 2px; stroke-dash: solid;"
    Const normalErrorLineStyle$ = "stroke: green; stroke-width: 2px; stroke-dash: solid;"

    <Extension>
    Public Function NormalDistributionPlot(sample As IEnumerable(Of Double),
                                           Optional size$ = "2000,1800",
                                           Optional bg$ = "white",
                                           Optional margin$ = g.DefaultPadding,
                                           Optional dotSize! = 5,
                                           Optional normaldistLineColor$ = defaultNormalDistLineStyle,
                                           Optional outlierColor$ = outlierLineStyle,
                                           Optional normalErrorColor$ = normalErrorLineStyle,
                                           Optional meanLineCSS$ = defaultMeanLineStyle) As GraphicsData

        Dim data As New BasicProductMoments(sample)
        Dim meanLine As Pen = Stroke.TryParse(meanLineCSS).GDIObject
        Dim normalErrorLine As Pen = Stroke.TryParse(normalErrorColor).GDIObject
        Dim outlierLine As Pen = Stroke.TryParse(outlierColor).GDIObject
        Dim normaldistLine As Pen = Stroke.TryParse(normaldistLineColor).GDIObject
        Dim d1# = data.StDev
        Dim d2# = 2 * d1
        Dim d3# = 3 * d1
        Dim d4# = 4 * d1
        Dim d5# = 5 * d1
        Dim d6# = 6 * d1
        Dim xrange As Vector = {data.Mean - d6, data.Mean + d6} _
            .Range _
            .Enumerate(n:=200) _
            .AsVector
        Dim points As PointF() = xrange _
            .AsVector _
            .ProbabilityDensity(data.Mean, d1) _
            .Select(Function(y, i)
                        Return New PointF With {
                            .X = xrange(i),
                            .Y = y
                        }
                    End Function) _
            .ToArray
        Dim XTicks = xrange.Range().CreateAxisTicks
        Dim YTicks = points.Y.Range.CreateAxisTicks

        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)
                Dim X, Y As d3js.scale.LinearScale
                Dim rect = region.PlotRegion

                X = d3js.scale.linear.domain(XTicks).range(integers:={rect.Left, rect.Right})
                Y = d3js.scale.linear.domain(YTicks).range(integers:={0, rect.Bottom - rect.Top})

                Dim scaler As New DataScaler With {
                    .X = X,
                    .Y = Y,
                    .ChartRegion = rect,
                    .AxisTicks = (XTicks, YTicks)
                }

                For Each pair In points.SlideWindows(2, offset:=1)
                    Dim p1 As PointF = pair(0), p2 As PointF = pair(1)
                    p1 = scaler.Translate(p1.X, p1.Y)
                    p2 = scaler.Translate(p2.X, p2.Y)

                    Call g.DrawLine(normaldistLine, p1, p2)
                Next
            End Sub

        Return g.GraphicsPlots(
            size.SizeParser,
            margin,
            bg,
            plotAPI:=plotInternal
        )
    End Function
End Module
