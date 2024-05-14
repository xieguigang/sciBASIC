#Region "Microsoft.VisualBasic::923b3c33434f8f19177cfd2cac21c939, Data_science\Visualization\test\PieChartTest.vb"

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

    '   Total Lines: 173
    '    Code Lines: 113
    ' Comment Lines: 20
    '   Blank Lines: 40
    '     File Size: 6.81 KB


    ' Module PieChartTest
    ' 
    '     Sub: glowTest2, Main, radar2, radarTest, shapeGlowTest
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.Fractions
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors

Public Module PieChartTest

    Sub Main()

        '  Call glowTest2()
        '  Call shapeGlowTest()

        Call radar2()
        Call radarTest()

        Dim rnd As New Random
        Dim data As New List(Of NamedValue(Of Integer))

        For i As Integer = 0 To 6
            data.Add(
                New NamedValue(Of Integer) With {
                     .Name = "block#" & i,
                     .Value = rnd.Next(300)
                })
        Next

        Call data _
            .Fractions(ColorBrewer.QualitativeSchemes.Accent8) _
            .Plot(legendAlt:=True) _
            .Save("./test_pie.png")
    End Sub

    Sub radarTest()
        Dim rnd As New Random
        Dim data As New List(Of NamedValue(Of Integer))

        For i As Integer = 0 To 6
            data.Add(
                New NamedValue(Of Integer) With {
                     .Name = "block#" & i,
                     .Value = rnd.Next(300)
                })
        Next

        Dim s = data _
            .Fractions(ColorBrewer.QualitativeSchemes.Accent8)

        Call RadarChart.Plot({New NamedValue(Of FractionData())("test1", s)}, spline:=200).Save("./test_radar.png")

        Pause()
    End Sub

    Sub radar2()

        Dim data1 = {New FractionData With {.Name = "心衰", .Value = 0.87287910860989326},
        New FractionData With {.Name = "脑卒中", .Value = 0.82750643633469156}, New FractionData With {.Name = "高血压", .Value = 0.95399002029690139},
        New FractionData With {.Name = "血脂异常", .Value = 0.67480404525157278},
        New FractionData With {.Name = "冠心病", .Value = 0.88863976807377087},
        New FractionData With {.Name = "胰岛素抵抗", .Value = 0.81755565655800488},
        New FractionData With {.Name = "动脉硬化", .Value = 0.95399002029690139}}
        Dim data2 = {New FractionData With {.Name = "心衰", .Value = 0.4660571945689998},
         New FractionData With {.Name = "脑卒中", .Value = 0.58808376037243382},
        New FractionData With {.Name = "高血压", .Value = 0.37740323894862676},
        New FractionData With {.Name = "血脂异常", .Value = 0.32589371354648861},
        New FractionData With {.Name = "冠心病", .Value = 0.5158286425323747},
        New FractionData With {.Name = "胰岛素抵抗", .Value = 0.65688382193268191},
        New FractionData With {.Name = "动脉硬化", .Value = 0.37740323894862676}}

        Dim colors = "#EDC951,#CC333F"

        Dim serials = {New NamedValue(Of FractionData())("第一次", data1), New NamedValue(Of FractionData())("第二次", data2)}

        Call RadarChart.Plot(serials).Save("./test_radar.png")

        Pause()
    End Sub


    Sub glowTest2()

        Using Graphics As Graphics2D = New Size(1000, 1000).CreateGDIDevice

            Graphics.SmoothingMode = SmoothingMode.AntiAlias
            Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic

            Dim brushWhite = New SolidBrush(Color.White)
            Graphics.FillRectangle(brushWhite, 0, 0,
    Graphics.Width, Graphics.Height)

            Dim FontFamily = New FontFamily("Arial")
            Dim strformat = New StringFormat()
            Dim szbuf$ = "Text Designer"

            Dim path As New GraphicsPath()
            path.AddString(szbuf, FontFamily, FontStyle.Regular, 48.0F, New Point(10, 10), strformat)

            path.AddPolygon({New Point(100, 100), New Point(100, 800), New Point(800, 800), New Point(700, 200)})
            path.CloseAllFigures()

            For i As Integer = 1 To 20

                Dim Pen = New Pen(Color.FromArgb(32, 0, 128, 192), i)
                Pen.LineJoin = LineJoin.Round
                Graphics.DrawPath(Pen, path)
                Pen.Dispose()
            Next

            Dim Brush = New SolidBrush(Color.FromArgb(255, 255, 255))
            Graphics.FillPath(Brush, path)

            brushWhite.Dispose()
            FontFamily.Dispose()
            path.Dispose()
            Brush.Dispose()

            Call Graphics.ImageResource.SaveAs("./g2.png")

        End Using


        Pause()
    End Sub

    Sub shapeGlowTest()

        Dim captionbitmap As New Bitmap(3000, 2500)
        Dim _imageCaption As Image = captionbitmap
        Dim hGraph As Graphics = Graphics.FromImage(_imageCaption)
        ' Create a bitmap in a fixed ratio to the original drawing area.
        Dim bm As New Bitmap(CInt(300 / 5), CInt(25 / 5))
        ' Create a GraphicsPath object. 
        Dim pth As New GraphicsPath()
        ' Add the string in the chosen style. 
        pth.AddString("Test", New FontFamily("Century Gothic"), FontStyle.Bold, 184, New Point(1000, 1500), StringFormat.GenericTypographic)
        ' Get the graphics object for the image. 
        Dim g = Graphics.FromImage(bm)
        ' Create a matrix that shrinks the drawing output by the fixed ratio. 
        Dim mx As New Matrix(1.0F / 5, 0, 0, 1.0F / 5, -(1.0F / 5), -(1.0F / 5))
        ' Choose an appropriate smoothing mode for the halo. 
        g.SmoothingMode = SmoothingMode.AntiAlias
        ' Transform the graphics object so that the same half may be used for both halo And text output. 
        g.Transform = mx
        ' Using a suitable pen...
        Dim p As New Pen(Color.Red, 5)
        ' Draw around the outline of the path
        g.DrawPath(p, pth)
        ' And then fill in for good measure. 
        g.FillPath(Brushes.Red, pth)
        ' We no longer need this graphics object
        g.Dispose()
        ' this just shifts the effect a little bit so that the edge isn't cut off in the demonstration
        ' hGraph.Transform = New Matrix(1, 0, 0, 1, 50, 50);
        ' setup the smoothing mode for path drawing
        hGraph.SmoothingMode = SmoothingMode.AntiAlias
        ' And the interpolation mode for the expansion of the halo bitmap
        hGraph.InterpolationMode = InterpolationMode.HighQualityBicubic
        ' expand the halo making the edges nice And fuzzy. 
        hGraph.DrawImage(bm, New Rectangle(0, 0, (captionbitmap.Width - 50), 25), 0, 0, bm.Width, bm.Height, GraphicsUnit.Pixel)
        ' Redraw the original text
        hGraph.FillPath(New SolidBrush(Color.Black), pth)
        ' And you're done. 
        pth.Dispose()

        Call _imageCaption.SaveAs("./hhhhh.png")



        Pause()
    End Sub
End Module
