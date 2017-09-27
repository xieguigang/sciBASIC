
Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D.Device
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Plot3D.Model

    Public Module GridBottom

        Public Function Grid(xrange As DoubleRange, yrange As DoubleRange, steps As (X!, Y!), Z#, Optional strokeCSS$ = Stroke.AxisGridStroke) As Line()
            Dim gridData As New List(Of Line)
            Dim a, b As Point3D
            Dim pen As Pen = Stroke.TryParse(strokeCSS).GDIObject

            For X As Double = xrange.Min To xrange.Max Step steps.X
                a = New Point3D With {.X = X, .Y = yrange.Min, .Z = Z}
                b = New Point3D With {.X = X, .Y = yrange.Max, .Z = Z}
                gridData += New Line(a, b) With {
                    .Stroke = pen
                }
            Next

            For Y As Double = yrange.Min To yrange.Max Step steps.Y
                a = New Point3D With {.X = xrange.Min, .Y = Y, .Z = Z}
                b = New Point3D With {.X = xrange.Max, .Y = Y, .Z = Z}
                gridData += New Line(a, b) With {
                    .Stroke = pen
                }
            Next

            Return gridData
        End Function
    End Module
End Namespace