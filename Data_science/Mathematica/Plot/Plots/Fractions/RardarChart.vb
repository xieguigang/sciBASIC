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
                             Optional shapeBorderWidth! = 2) As GraphicsData

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

                    For i As Integer = 0 To serials.Length - 1
                        serial = serials(i)
                        color = serialColors(i)
                        pen = borderPens(i)

                        With serial.Value.ToDictionary
                            For Each key As String In directions
                                f = .Item(key)

                                If f Is Nothing Then
                                    value = axisRange.Min
                                End If

                                r = axisRange.ScaleMapping(value, radius)
                                shape += (r, alpha).ToCartesianPoint
                                alpha += dDegree
                            Next

                            ' 填充区域
                            Call g.FillPolygon(New SolidBrush(color), shape)
                            Call g.DrawPolygon(pen, shape)
                        End With

                        alpha = 0
                    Next
                End Sub

            Return g.GraphicsPlots(size.SizeParser, margin, bg, plotInternal)
        End Function
    End Module
End Namespace