
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
                             Optional xn% = 100,
                             Optional yn% = 100,
                             Optional legendTitle$ = "3D scatter heatmap",
                             Optional mapName$ = "Spectral:c10",
                             Optional mapLevels% = 25,
                             Optional bg$ = "white",
                             Optional parallel As Boolean = False,
                             Optional matrix As List(Of EntityObject) = Nothing,
                             Optional axisFont$ = CSSFont.Win10Normal,
                             Optional legendFont As Font = Nothing) As Bitmap

            Dim data As (sf As Surface, c As Double())() =
                f.Surface(
                xrange, yrange,
                xrange.Length / xn,
                yrange.Length / yn,
                parallel, matrix).ToArray

            Return data.Plot(
                camera, legendTitle,
                mapName, mapLevels,
                bg,
                axisFont, legendFont)
        End Function

        <Extension>
        Public Function Plot(data As (sf As Surface, c As Double())(),
                             camera As Camera,
                             Optional legendTitle$ = "3D scatter heatmap",
                             Optional mapName$ = "Spectral:c10",
                             Optional mapLevels% = 25,
                             Optional bg$ = "white",
                             Optional axisFont$ = CSSFont.Win10Normal,
                             Optional legendFont As Font = Nothing) As Bitmap

            Dim averages As Double() = data _
                .ToArray(Function(c) c.c.Average)
            Dim levels As Integer() = averages _
                .GenerateMapping(mapLevels) _
                .ToArray
            Dim colors As SolidBrush() =
                Designer _
                .GetBrushes(mapName, mapLevels,)
            Dim rawPoints As Point3D() = data _
                .Select(Function(s) s.sf.vertices) _
                .IteratesALL _
                .ToArray

            Return GraphicsPlots(
                camera.screen, New Size,
                bg$,
                Sub(ByRef g, region)

                    Call g.DrawAxis(
                        rawPoints,
                        camera,
                        CSSFont.TryParse(axisFont).GDIObject)

                    With camera

                        ' 绘制通过函数所计算出来的三维表面
                        For Each sf In data.SeqIterator
                            Dim surface As Surface = (+sf).sf
                            Dim level% = levels(sf.i)

                            If level > colors.Length - 1 Then
                                level = colors.Length - 1
                            ElseIf level < 0 Then
                                level = 0
                            End If

                            surface.brush = colors(level)
                            surface.vertices =
                                .Rotate(surface.vertices) _
                                .ToArray
                            surface.Allocation()
                            surface.Draw(g, camera)
                        Next
                    End With

                    ' Draw legends
                    Dim legend As Bitmap = colors.ColorMapLegend(
                        haveUnmapped:=False,
                        min:=Math.Round(averages.Min, 1),
                        max:=Math.Round(averages.Max, 1),
                        title:=legendTitle,
                        titleFont:=legendFont)
                    Dim lsize As Size = legend.Size
                    Dim left% = camera.screen.Width - lsize.Width + 150
                    Dim top% = camera.screen.Height / 3

                    Call g.DrawImageUnscaled(legend, left, top)
                End Sub)
        End Function

        ''' <summary>
        ''' 3D heatmap plot from matrix data
        ''' </summary>
        ''' <param name="matrix"></param>
        ''' <param name="Camera"></param>
        ''' <param name="legendTitle$"></param>
        ''' <param name="mapName$"></param>
        ''' <param name="mapLevels%"></param>
        ''' <param name="bg$"></param>
        ''' <param name="axisFont$"></param>
        ''' <param name="legendFont"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot(matrix As IEnumerable(Of EntityObject),
                             Camera As Camera,
                             Optional legendTitle$ = "3D scatter heatmap",
                             Optional mapName$ = "Spectral:c10",
                             Optional mapLevels% = 25,
                             Optional bg$ = "white",
                             Optional axisFont$ = CSSFont.Win10Normal,
                             Optional legendFont As Font = Nothing) As Bitmap

            Return matrix.Surface.ToArray _
                .Plot(
                Camera, legendTitle,
                mapName, mapLevels,
                bg,
                axisFont, legendFont)
        End Function
    End Module
End Namespace