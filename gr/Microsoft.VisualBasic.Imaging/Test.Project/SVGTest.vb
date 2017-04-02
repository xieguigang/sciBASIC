Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.SVG

Module SVGTest

    Sub Test()
        Dim svg As New GraphicsSVG
        Call svg.Clear(Color.Green)
        Call svg.DrawString("Hello World!", New Font(FontFace.MicrosoftYaHei, 25), Brushes.Cyan, New PointF(100, 200))
        Call svg.DrawLine(Pens.Red, New Point(100, 100), New Point(300, 500))
        Call svg.WriteSVG("x:\test.svg")

        Pause()
    End Sub
End Module
