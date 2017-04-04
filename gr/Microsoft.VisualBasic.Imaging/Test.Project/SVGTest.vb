Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.Imaging.SVG

Module SVGTest

    Sub Test()
        Dim svg As New GraphicsSVG(600, 1000)
        Call svg.Clear(Color.Green)
        Call svg.DrawString("Hello World!", New Font(FontFace.MicrosoftYaHei, 25), Brushes.Cyan, New PointF(100, 200))
        Call svg.DrawLine(Pens.Red, New Point(100, 100), New Point(300, 500))
        Call svg.DrawRectangle(Pens.RoyalBlue, New Rectangle(300, 300, 500, 120))
        Call svg.DrawPath(Pens.RosyBrown, Pentacle.PathData(New Point(300, 500), New SizeF(500, 500)).GraphicsPath)
        Call svg.DrawPolygon(Pens.PaleVioletRed, {New PointF(220, 100), New PointF(300, 210), New PointF(170, 250), New PointF(123, 234)})
        Call svg.WriteSVG("x:\test.svg")

        Pause()
    End Sub
End Module
