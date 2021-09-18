Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.MarchingSquares
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Plots

    Public Class PolygonPlot2D : Inherits Plot

        ReadOnly polygons As SerialData()
        ReadOnly reverse As Boolean = False

        Public Sub New(data As IEnumerable(Of SerialData), theme As Theme)
            MyBase.New(theme)

            polygons = data.ToArray
        End Sub

        Sub New(data As IEnumerable(Of GeneralPath), theme As Theme, Optional names As String() = Nothing, Optional reverse As Boolean = False)
            Call Me.New(CreateSerial(data.ToArray, theme, names), theme)

            Me.reverse = reverse
        End Sub

        Private Shared Iterator Function CreateSerial(data As GeneralPath(), theme As Theme, names As String()) As IEnumerable(Of SerialData)
            Dim colors As Color() = Designer.GetColors(theme.colorSet, data.Length)
            Dim i As Integer = 0
            Dim titleName As String

            For Each path As GeneralPath In data
                If names.IsNullOrEmpty OrElse names.ElementAtOrDefault(i).StringEmpty Then
                    titleName = colors(i).ToHtmlColor
                Else
                    titleName = names(i)
                End If

                For Each part As PointF() In path.GetPolygons
                    Yield New SerialData With {
                        .color = colors(i),
                        .title = titleName,
                        .lineType = DashStyle.Solid,
                        .pointSize = 5,
                        .shape = LegendStyles.Circle,
                        .width = 2,
                        .pts = part _
                            .Select(Function(p)
                                        Return New PointData With {
                                            .pt = p
                                        }
                                    End Function) _
                            .ToArray
                    }
                Next

                i += 1
            Next
        End Function

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            Dim xTicks As Double() = polygons.Select(Function(p) p.pts.Select(Function(pi) CDbl(pi.pt.X))).IteratesALL.CreateAxisTicks
            Dim yTicks As Double() = polygons.Select(Function(p) p.pts.Select(Function(pi) CDbl(pi.pt.Y))).IteratesALL.CreateAxisTicks
            Dim rect = canvas.PlotRegion
            Dim xscale = d3js.scale.linear.domain(xTicks).range(New Double() {rect.Left, rect.Right})
            Dim yscale = d3js.scale.linear.domain(yTicks).range(New Double() {rect.Top, rect.Bottom})
            Dim shape As PointF()
            Dim scale As New DataScaler() With {
                .AxisTicks = (xTicks.AsVector, yTicks.AsVector),
                .region = rect,
                .X = xscale,
                .Y = yscale
            }

            Call Axis.DrawAxis(
                g, canvas, scale,
                showGrid:=theme.drawGrid,
                xlabel:=xlabel,
                ylabel:=ylabel,
                gridFill:=theme.gridFill
            )

            For Each polygon As SerialData In polygons
                shape = polygon.pts _
                    .Select(Function(p)
                                If reverse Then
                                    Return New PointF With {
                                        .X = scale.TranslateX(p.pt.X),
                                        .Y = canvas.Height - scale.TranslateY(p.pt.Y)
                                    }
                                Else
                                    Return scale.Translate(p.pt)
                                End If
                            End Function) _
                    .ToArray

                Call g.FillPolygon(New SolidBrush(polygon.color), shape)
            Next
        End Sub
    End Class
End Namespace