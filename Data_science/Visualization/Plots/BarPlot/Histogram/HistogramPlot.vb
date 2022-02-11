Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.scale
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace BarPlot.Histogram

    Public Class HistogramPlot : Inherits Plot

        ReadOnly groups As HistogramGroup
        ReadOnly alpha As Double
        ReadOnly drawRect As Boolean

        Public Property xAxis As String
        Public Property showTagChartLayer As Boolean

        Public Sub New(groups As HistogramGroup, alpha As Double, drawRect As Boolean, theme As Theme)
            Call MyBase.New(theme)

            Me.drawRect = drawRect
            Me.alpha = alpha
            Me.groups = groups
        End Sub

        Public Shared Sub DrawSample(g As IGraphics, region As Rectangle,
                                     hist As HistProfile,
                                     ann As NamedValue(Of Color),
                                     scaler As DataScaler,
                                     Optional alpha As Integer = 255,
                                     Optional drawRect As Boolean = True)

            Dim b As New SolidBrush(Color.FromArgb(alpha, ann.Value))

            For Each block As HistogramData In hist.data
                Dim pos As PointF = scaler.Translate(block.x1, block.y)
                Dim sizeF As New SizeF With {
                            .Width = scaler.TranslateX(block.x2) - scaler.TranslateX(block.x1),
                            .Height = region.Bottom - scaler.TranslateY(block.y)
                        }
                Dim rect As New RectangleF With {
                            .Location = pos,
                            .Size = sizeF
                        }

                Call g.FillRectangle(b, rect)

                If drawRect Then
                    Call g.DrawRectangle(
                                Pens.Black,
                                rect.Left, rect.Top,
                                rect.Width, rect.Height)
                End If
            Next
        End Sub

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            Dim region As Rectangle = canvas.PlotRegion

            If groups.Samples.Length = 1 AndAlso groups.Samples.First.data.Length = 0 Then
                Call "No content data for plot histogram chart...".Warning
                Return
            End If

            Dim scalerData As New Scaling(groups, False)
            Dim annotations As Dictionary(Of NamedValue(Of Color)) = groups.Serials.ToDictionary
            Dim gSize As Size = canvas.Size
            Dim X, Y As d3js.scale.LinearScale
            Dim XTicks#() = groups.XRange.CreateAxisTicks
            Dim YTicks#() = groups.YRange.CreateAxisTicks

            With canvas.PlotRegion
                If Not xAxis.StringEmpty Then
                    XTicks = AxisProvider.TryParse(xAxis).AxisTicks
                    X = XTicks.LinearScale.range(integers:={ .Left, .Right})
                Else
                    X = d3js.scale.linear _
                                .domain(values:=XTicks) _
                                .range(integers:={ .Left, .Right})
                End If

                ' Y 为什么是从零开始的？
                Y = d3js.scale.linear _
                            .domain(values:=YTicks) _
                            .range(integers:={ .Bottom, .Top})
            End With

            Dim scaler As New DataScaler With {
                        .X = X,
                        .Y = Y,
                        .region = canvas.PlotRegion,
                        .AxisTicks = (XTicks, YTicks)
                    }

            Call g.DrawAxis(
                        canvas, scaler, theme.drawGrid, xlabel:=xlabel, ylabel:=ylabel,
                        htmlLabel:=False)

            If Not main.StringEmpty Then
                Dim titleFont As Font = CSSFont.TryParse(theme.mainCSS).GDIObject(g.Dpi)
                Dim titleSize As SizeF = g.MeasureString(main, titleFont)
                Dim titlePos As New PointF With {
                            .X = region.Left + (region.Width - titleSize.Width) / 2,
                            .Y = region.Top - titleSize.Height * 1.125
                        }

                Call g.DrawString(main, titleFont, Brushes.Black, titlePos)
            End If

            For Each hist As HistProfile In groups.Samples
                Call DrawSample(g, region, hist, annotations(hist.legend.title), scaler, alpha, drawRect)
            Next

            If showTagChartLayer Then
                Dim serials As New List(Of SerialData)

                For Each hist As SeqValue(Of HistProfile) In groups.Samples.SeqIterator
                    serials += (+hist).GetLine(groups.Serials(hist).Value, 2, 5)
                Next

                Dim chart As GraphicsData = Scatter.Plot(
                            serials,
                            size:=$"{canvas.Width},{canvas.Height}",
                            padding:=theme.padding,
                            bg:="transparent",
                            showGrid:=False,
                            showLegend:=False,
                            drawAxis:=False)

                ' 合并图层
                Call g.DrawImageUnscaled(chart, New Rectangle(New Point, gSize))
            End If

            If theme.drawLegend Then
                Call Me.DrawLegends(g, groups.Samples.Select(Function(h) h.legend).ToArray, showBorder:=Not theme.legendBoxStroke.StringEmpty, canvas)
            End If
        End Sub
    End Class
End Namespace