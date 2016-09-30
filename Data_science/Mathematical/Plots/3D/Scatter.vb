Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing2D

Namespace Plot3D

    Public Module Scatter

        Public Function Plot(func As Func(Of Double, Double, Double),
                             x As DoubleRange,
                             y As DoubleRange,
                             camera As Camera,
                             Optional xsteps! = 0.1,
                             Optional ysteps! = 0.1,
                             Optional font As Font = Nothing,
                             Optional bg$ = "white") As Bitmap

            Dim data As Point3D() = Evaluate(func, x, y, xsteps, ysteps)

            Return GraphicsPlots(
                camera.screen, New Size(5, 5), bg,
                Sub(ByRef g, region)
                    Call AxisDraw.DrawAxis(g, data, camera, font)

                    With camera

                        For Each pt As Point3D In data
                            pt = .Project(.Rotate(pt))
                            Call g.FillPie(Brushes.Red, New Rectangle(pt.PointXY, New Size(5, 5)), 0, 360)
                        Next
                    End With
                End Sub)
        End Function
    End Module
End Namespace