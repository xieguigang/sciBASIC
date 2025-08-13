Imports System.Drawing
Imports Microsoft.VisualBasic.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.DelaunayVoronoi
Imports Microsoft.VisualBasic.Imaging.Filters
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Distributions

Module filter_test

    Sub Main()
        Call testDraw()

        Dim img = "Z:\Capture.PNG".LoadImage
        Dim bitmap = BitmapBuffer.FromImage(img)
        Dim bin = bitmap.ostuFilter(flip:=False)

        Call bin.Save("Z:/aaa.bmp")
    End Sub

    Sub testDraw()
        Dim size As New Size(1000, 800)
        Dim points = PoissonDiskGenerator.Generate(50, 1000)
        Dim split As New Voronoi(points, New Rectf(0, 0, 1000, 800))
        Dim g As Graphics2D = size.CreateGDIDevice(Color.White)

        For Each pt In points
            Call g.DrawCircle(New PointF(pt.x, pt.y), 8, Brushes.Red)
        Next

        Dim regions = split.Regions

        For Each r As Polygon2D In regions
            Call g.DrawPolygon(Pens.Blue, r.AsEnumerable.ToArray)
        Next

        Call g.Flush()
        Call g.ImageResource.SaveAs("Z:/test.png")

        Pause()
    End Sub
End Module
