
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
                             Optional xn% = 50,
                             Optional yn% = 50,
                             Optional mapName$ = "Spectral:c10",
                             Optional mapLevels% = 25,
                             Optional bg$ = "white",
                             Optional parallel As Boolean = False,
                             Optional matrix As List(Of EntityObject) = Nothing,
                             Optional axisFont$ = CSSFont.Win10Normal) As Bitmap

            Dim data As (pt As Point3D, c#)() = f.Evaluate(
                xrange, yrange,
                xrange.Length / xn,
                yrange.Length / yn,
                parallel, matrix).IteratesALL.ToArray
            Dim levels%() = data _
                .ToArray(Function(pt) pt.c) _
                .GenerateMapping(mapLevels) _
                .ToArray
            Dim colors As SolidBrush() = Designer.GetBrushes(mapName, mapLevels,)

            Return GraphicsPlots(
                camera.screen,
                New Size,
                bg$,
                Sub(ByRef g, region)

                    Call g.DrawAxis(data.ToArray(Function(o) o.pt), camera, CSSFont.TryParse(axisFont).GDIObject)



                End Sub)
        End Function
    End Module
End Namespace