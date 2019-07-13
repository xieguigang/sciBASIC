#Region "Microsoft.VisualBasic::47a47b7c7d78b3ee2a1bca919d00a0b1, gr\Microsoft.VisualBasic.Imaging\test\SVGTest.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module SVGTest
    ' 
    '     Sub: IOtest, Main, pathParserTest, Test
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes
Imports Microsoft.VisualBasic.Imaging.SVG
Imports Microsoft.VisualBasic.Imaging.SVG.XML

Module SVGTest

    Sub Main()
        Call pathParserTest()
        '   Call IOtest()
        Call Test()
    End Sub

    Sub pathParserTest()
        Dim path As New Path2D

        Call path.MoveTo(100, 100)
        Call path.LineTo(200, 200)
        Call path.LineTo(500, 100)
        Call path.LineTo(300, 699)
        Call path.CloseAllFigures()
        Call path.MoveTo(800, 800)
        Call path.LineTo(200, 300)
        Call path.HorizontalTo(1000)
        Call path.VerticalTo(602)
        Call path.CloseAllFigures()

        Dim svgPath = path.SVGPath

        Dim gdi As GraphicsPath = svgPath.ParseSVGPathData


        Call gdi.SVGPath.d.__DEBUG_ECHO
        Call svgPath.d.__DEBUG_ECHO

        Pause()
    End Sub


    Sub IOtest()


        Dim sss As New SVGXml

        sss.circles = {New XML.circle With {.fill = "black", .cx = 1, .cy = 200, .r = 20, .id = "ffff"}}
        sss.Size(New Size(100, 20000))
        sss.images = {New XML.Image("D:\GCModeller\src\runtime\sciBASIC#\logo.png".LoadImage)}
        sss.space = "dddddd"

        Call sss.SaveAsXml("./testssss.svg")

        ' Pause()

        Dim svg = SVGXml.TryLoad("D:\GCModeller\src\runtime\sciBASIC#\gr\SVG\Tree_of_life_SVG.svg")

        Call svg.GetSVGXml.SaveTo("./tree.svg")


        Pause()
    End Sub

    Sub Test()
        Dim svg As New GraphicsSVG(600, 1000)
        Call svg.Clear(Color.DeepSkyBlue)

        Call svg.DrawLine(Pens.Red, New Point(100, 100), New Point(300, 500))
        Call svg.DrawRectangle(Pens.RoyalBlue, New Rectangle(300, 300, 500, 120))
        Call svg.DrawPath(Pens.RosyBrown, Pentacle.PathData(New Point(300, 500), New SizeF(500, 500)).GraphicsPath)
        Call svg.DrawPolygon(Pens.PaleVioletRed, {New PointF(220, 100), New PointF(300, 210), New PointF(170, 250), New PointF(123, 234)})
        Call svg.DrawString("Hello World!", New Font(FontFace.MicrosoftYaHei, 25), Brushes.Cyan, New PointF(100, 200))
        Call svg.WriteSVG("./draw_test.svg",, "this is the SVG author comment
dddssada
sdfsf
gdffffgd
dfgdfgdg
dfgdfg
dddddddddddddddddddddddddddddddddddddddddddd")

        Pause()
    End Sub
End Module
