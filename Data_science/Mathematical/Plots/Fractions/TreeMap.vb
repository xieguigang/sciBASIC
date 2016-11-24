Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.g
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Public Module TreeMap


    Public Function Plot(data As IEnumerable(Of Fractions),
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional bg$ = "white") As Bitmap

        Dim array As Fractions() =
            data _
            .OrderByDescending(Function(x) x.Percentage) _
            .ToArray

        Return GraphicsPlots(
            size, margin,
            bg,
            Sub(ByRef g, region)

            End Sub)
    End Function
End Module
