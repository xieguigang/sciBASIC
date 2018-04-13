Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Interpolation
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports sys = System.Math

Namespace Fractions

    Public Module RardarChart

        <Extension>
        Public Function Plot(serials As NamedValue(Of FractionData())(),
                             Optional size$ = "3000,2700",
                             Optional margin$ = g.DefaultPadding,
                             Optional bg$ = "white",
                             Optional serialColorSchema$ = "alpha(Set1:c8, 0.65)",
                             Optional axisRange As DoubleRange = Nothing,
                             Optional shapeBorderWidth! = 2,
                             Optional axisStrokeStyle$ = Stroke.HighlightStroke,
                             Optional spline% = 0) As GraphicsData

            Dim serialColors As Color() = Designer.GetColors(serialColorSchema)
            Dim borderPens As Pen() = serialColors _
                .Select(Function(c) New Pen(c.Alpha(255), shapeBorderWidth)) _
                .ToArray
            Dim directions$() = serials.Select(Function(s) s.Value) _
                                       .IteratesALL _
                                       .Keys _
                                       .Distinct _
                                       .ToArray
            Dim dDegree# = 360 / directions.Length
            Dim axisPen As Pen = Stroke.TryParse(axisStrokeStyle).GDIObject

            If axisRange Is Nothing Then
                axisRange = serials.Values _
                                   .IteratesALL _
                                   .Select(Function(f) f.Value) _
                                   .AsVector _
                                   .CreateAxisTicks
            End If

            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Dim center As PointF = region.PlotRegion.Centre
                    Dim radius As DoubleRange = {0, sys.Min(region.Width, region.Height) / 2}
                    Dim serial As NamedValue(Of FractionData())
                    Dim r#, alpha!
                    Dim shape As New List(Of PointF)
                    Dim f As FractionData
                    Dim value#
                    Dim color As Color
                    Dim pen As Pen
                    Dim label$

                    ' 绘制出中心点以及坐标轴
                    Call g.FillPie(Brushes.Gray, New Rectangle(center.X - 2, center.Y - 2, 4, 4), 0, 360)

                    For i As Integer = 0 To directions.Length - 1
                        label = directions(i)
                        g.DrawLine(axisPen, (radius.Max, alpha).ToCartesianPoint.OffSet2D(center), center)
                        alpha += dDegree
                    Next

                    Dim dr = radius.Max / 5

                    For i As Integer = 1 To 4
                        r = dr * i
                        g.DrawEllipse(axisPen, New RectangleF(center.OffSet2D(-r, -r), New SizeF(r * 2, r * 2)))
                    Next

                    For i As Integer = 0 To serials.Length - 1
                        serial = serials(i)
                        color = serialColors(i)
                        pen = borderPens(i)
                        shape *= 0
                        alpha = 0

                        With serial.Value.ToDictionary
                            For Each key As String In directions
                                f = .Item(key)

                                If f Is Nothing Then
                                    value = axisRange.Min
                                Else
                                    value = f.Value
                                End If

                                r = axisRange.ScaleMapping(value, radius)
                                shape += (r, alpha).ToCartesianPoint.OffSet2D(center)
                                alpha += dDegree
                            Next

                            If spline > 0 Then
                                shape = shape.CubicSpline(spline).AsList
                            End If

                            ' 填充区域
                            Call g.FillPolygon(New SolidBrush(color), shape)
                            Call g.DrawPolygon(pen, shape)
                        End With
                    Next
                End Sub

            Return g.GraphicsPlots(size.SizeParser, margin, bg, plotInternal)
        End Function
    End Module
End Namespace