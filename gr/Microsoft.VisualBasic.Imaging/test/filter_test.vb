Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.DelaunayVoronoi
Imports Microsoft.VisualBasic.Imaging.Filters
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
        Dim points = PoissonDiskGenerator.Generate(30, 1000)
        Dim split As New Voronoi(points)


        Pause()
    End Sub
End Module
