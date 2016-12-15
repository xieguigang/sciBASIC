
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Data.csv.DocumentStream
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Plot3D

    Public Module ScatterHeatmap

        <Extension>
        Public Function Plot(f As Func(Of Double, Double, (Z#, color#)),
                             xrange As DoubleRange,
                             yrange As DoubleRange,
                             camera As Camera,
                             Optional xn% = 1000,
                             Optional yn% = 1000,
                             Optional legendTitle$ = "3D scatter heatmap",
                             Optional mapName$ = "Spectral:c10",
                             Optional mapLevels% = 25,
                             Optional bg$ = "white",
                             Optional parallel As Boolean = False,
                             Optional matrix As List(Of EntityObject) = Nothing,
                             Optional axisFont$ = CSSFont.Win10Normal,
                             Optional legendFont As Font = Nothing,
                             Optional ptSize% = 5) As Bitmap

            Dim data As (pt As Point3D, c#)() = f.Evaluate(
                xrange, yrange,
                xrange.Length / xn,
                yrange.Length / yn,
                parallel, matrix).IteratesALL.ToArray
            Dim levels As Integer() = data _
                .Select(Function(c) c.c) _
                .GenerateMapping(mapLevels) _
                .ToArray
            Dim colors As SolidBrush() = Designer _
                .GetBrushes(mapName, mapLevels,)
            Dim rawPoints As Point3D() =
                data.ToArray(Function(o) o.pt)

            Return GraphicsPlots(
                camera.screen, New Size,
                bg$,
                Sub(ByRef g, region)

                    Call g.DrawAxis(
                        rawPoints,
                        camera,
                        CSSFont.TryParse(axisFont).GDIObject)

                    With camera

                        Dim pts = .Project(.Rotate(rawPoints)) _
                            .SeqIterator _
                            .Select(Function(ip) (idx:=ip.i, p3D:=ip.value)) _
                            .OrderBy(Function(z) z.p3D.Z)
                        Dim ptSz As New Size(ptSize, ptSize)

                        For Each pt As (idx%, p3D As Point3D) In pts

                            Dim p2D As Point = pt.p3D.PointXY(.screen)
                            Dim lv = levels(pt.idx) - 1

                            If lv >= colors.Length Then
                                lv = colors.Length - 1
                            ElseIf lv < 0 Then
                                lv = 0
                            End If

                            Dim color As SolidBrush = colors(lv)

                            Try
                                Call g.FillPie(color, New Rectangle(p2D, ptSz), 0, 360)
                            Catch ex As Exception

                            End Try
                        Next
                    End With

                    ' Draw legends
                    Dim legend As Bitmap = colors.ColorMapLegend(
                        haveUnmapped:=False,
                        min:=Math.Round(data.Min(Function(z) z.c), 1),
                        max:=Math.Round(data.Max(Function(z) z.c), 1),
                        title:=legendTitle,
                        titleFont:=legendFont)
                    Dim lsize As Size = legend.Size
                    Dim left% = camera.screen.Width - lsize.Width + 150
                    Dim top% = camera.screen.Height / 3

                    Call g.DrawImageUnscaled(legend, left, top)
                End Sub)
        End Function
    End Module
End Namespace