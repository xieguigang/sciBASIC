#Region "Microsoft.VisualBasic::2c893d313c5e8914106eceda10b4fa30, gr\Microsoft.VisualBasic.Imaging\test\SVGTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 84
    '    Code Lines: 59
    ' Comment Lines: 2
    '   Blank Lines: 23
    '     File Size: 2.52 KB


    ' Module SVGTest
    ' 
    '     Sub: IOtest, Main1, pathParserTest, Test
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

    Sub Main1()
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
        Dim svg As New GraphicsSVG(600, 1000, 300, 300)
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
