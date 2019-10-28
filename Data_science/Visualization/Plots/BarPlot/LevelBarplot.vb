Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace BarPlot

    ''' <summary>
    ''' 只针对单组数据的条形图绘制
    ''' </summary>
    Public Module LevelBarplot

        Public Function Plot(data As NamedValue(Of Double)(),
                             Optional size$ = "2000,1600",
                             Optional margin$ = Resolution2K.PaddingWithRightLegend,
                             Optional bg$ = "white",
                             Optional title$ = "BarPlot",
                             Optional titleFontCSS$ = CSSFont.Win7VeryLarge,
                             Optional labelFontCSS$ = CSSFont.Win7Large,
                             Optional chartBoxStroke$ = Stroke.ScatterLineStroke) As GraphicsData

            Dim titleFont As Font = CSSFont.TryParse(titleFontCSS)
            Dim labelFont As Font = CSSFont.TryParse(labelFontCSS)
            Dim maxLengthLabel$ = data.Keys _
                .Select(Function(lb)
                            If lb.Length > 48 Then
                                Return Mid(lb, 1, 48) & "..."
                            Else
                                Return lb
                            End If
                        End Function) _
                .MaxLengthString

            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Dim maxLabelSize As SizeF = g.MeasureString(maxLengthLabel, labelFont)
                    Dim plotRegion As Rectangle = region.PlotRegion
                    Dim pos As PointF
                    Dim chartBox As New Rectangle With {
                        .X = plotRegion.Left + maxLabelSize.Width + 5,
                        .Y = plotRegion.Top,
                        .Width = plotRegion.Width - maxLabelSize.Width - 5,
                        .Height = plotRegion.Height
                    }

                    ' draw main title
                    Dim titleSize As SizeF = g.MeasureString(title, titleFont)

                    pos = New PointF With {
                        .X = plotRegion.Left + (plotRegion.Width - titleSize.Width) / 2,
                        .Y = plotRegion.Top - 20
                    }

                    Call g.DrawString(title, titleFont, Brushes.Black, pos)
                    Call g.DrawRectangle(Stroke.TryParse(chartBoxStroke).GDIObject, chartBox)

                End Sub

            Return g.GraphicsPlots(size.SizeParser, margin, bg, plotInternal)
        End Function
    End Module
End Namespace