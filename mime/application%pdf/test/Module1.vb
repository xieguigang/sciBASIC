#Region "Microsoft.VisualBasic::dd064fdcbe53b66fec7e7342877e9548, mime\application%pdf\test\Module1.vb"

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

    '   Total Lines: 163
    '    Code Lines: 54 (33.13%)
    ' Comment Lines: 63 (38.65%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 46 (28.22%)
    '     File Size: 6.16 KB


    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.MIME.application.pdf

Module Module1

    Sub Main()
        Using Document As New PdfDocument(PaperType.A4, False, UnitOfMeasure.Inch, "./test.pdf")

            Dim Info = PdfInfo.CreatePdfInfo(Document)

            Info.Title("Article Example")
            Info.Author("Uzi Granot Granotech Limited")
            Info.Keywords("PDF, .NET, C#, Library, Document Creator")
            Info.Subject("PDF File Writer C# Class Library (Version 1.15.0)")

            Dim LocalTime = DateTime.Now
            Info.CreationDate(LocalTime)
            Info.ModDate(LocalTime)

            Info.Creator("PdfFileWriter C# Class Library Version " & PdfDocument.RevisionNumber)
            Info.Producer("PdfFileWriter C# Class Library Version " & PdfDocument.RevisionNumber)

            Dim Page = New PdfPage(Document)

            ' Step 4Add contents to page
            Dim Contents = New PdfContents(Page)
            Dim FontName1 = "Arial"
            Dim ArialNormal = PdfFont.CreatePdfFont(Document, FontName1, FontStyle.Regular, True)

            Contents.DrawText(ArialNormal, 9.0, 1.1, 0.85, "PdfFileWriter \u25cf PDF File Writer C# Class Library \u25cf Author: Uzi Granot")

            ' save graphics state
            Contents.SaveGraphicsState()

            Dim ArticleLink = "http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version"

            ' create PDF417 barcode
            Dim Pdf417 As New Pdf417Encoder()
            Pdf417.DefaultDataColumns = 3
            Pdf417.Encode(ArticleLink)
            Pdf417.WidthToHeightRatio(2.5)

            ' convert Pdf417 to black And white image
            Dim BarcodeImage = New PdfImage(Document)
            BarcodeImage.LoadImage(Pdf417)

            ' draw image
            Contents.DrawImage(BarcodeImage, 1.1, 5.2, 2.5)

            ' define a web link area coinsiding with the qr code
            Dim Height = 2.5 * Pdf417.ImageHeight / Pdf417.ImageWidth
            Page.AddWebLink(1.1, 5.2, 1.1 + 2.5, 5.2 + Height, ArticleLink)

            ' restore graphics sate
            Contents.RestoreGraphicsState()

            ' save graphics state
            Contents.SaveGraphicsState()

            ' translate coordinate origin to the center of the happy face
            Contents.Translate(4.25, 7.5)

            ' change nonstroking (fill) color to yellow
            Contents.SetColorNonStroking(Color.Yellow)

            ' draw happy face yellow oval
            Contents.DrawOval(-1.5, -1.0, 3.0, 2.0, PaintOp.Fill)

            ' set line width to 0.2" this is the black circle around the eye
            Contents.SetLineWidth(0.2)

            ' eye color is white with black outline circle
            Contents.SetColorNonStroking(Color.White)
            Contents.SetColorStroking(Color.Black)

            ' draw eyes
            Contents.DrawOval(-0.75, 0.0, 0.5, 0.5, PaintOp.CloseFillStroke)
            Contents.DrawOval(0.25, 0.0, 0.5, 0.5, PaintOp.CloseFillStroke)

            ' mouth color is black
            Contents.SetColorNonStroking(Color.Black)

            ' draw mouth by creating a path made of one line and one Bezier curve 
            Contents.MoveTo(-0.6, -0.4)
            Contents.LineTo(0.6, -0.4)
            Contents.DrawBezier(0.0, -0.8, 0, -0.8, -0.6, -0.4)

            ' fill the path with black color
            Contents.SetPaintOp(PaintOp.Fill)

            ' restore graphics sate
            Contents.RestoreGraphicsState()



            ' save graphics state
            'Contents.SaveGraphicsState()

            '' create chart
            Dim PieChart = PdfChart.CreateChart(Document, 1.8, 1.5, 300.0)

            '' create PdfChart object with Chart object
            Dim PiePdfChart = New PdfChart(Document, PieChart)
            'PiePdfChart.SaveAs = SaveImageAs.IndexedImage


            '' make sure we have good quality image
            'PieChart.AntiAliasing = AntiAliasingStyles.None ' .All;

            '' set colors
            'PieChart.BackColor = Color.FromArgb(220, 220, 255)
            'PieChart.Palette = ChartColorPalette.BrightPastel

            '' default font
            'Dim DefaultFont = PiePdfChart.CreateFont("Verdana", FontStyle.Regular, 0.05, FontSizeUnit.UserUnit)
            'Dim TitleFont = PiePdfChart.CreateFont("Verdana", FontStyle.Bold, 0.07, FontSizeUnit.UserUnit)

            '' title (font size Is 0.25 inches)
            'Dim Title1 = New Title("Pie Chart Example", Docking.Top, TitleFont, Color.Purple)
            'PieChart.Titles.Add(Title1)

            '' legend
            'Dim Legend1 = New Legend()
            'PieChart.Legends.Add(Legend1)
            'Legend1.BackColor = Color.FromArgb(230, 230, 255)
            'Legend1.Docking = Docking.Bottom
            'Legend1.Font = DefaultFont

            '' chart area
            'Dim ChartArea1 = New ChartArea()
            'PieChart.ChartAreas.Add(ChartArea1)

            '' chart area background color
            'ChartArea1.BackColor = Color.FromArgb(255, 200, 255)

            '' series 1
            'Dim Series1 = New Series()
            'PieChart.Series.Add(Series1)
            'Series1.ChartType = SeriesChartType.Pie
            'Series1.Font = DefaultFont
            'Series1.IsValueShownAsLabel = True
            'Series1.LabelFormat = "{0} %"

            '' series values
            'Series1.Points.Add(22.0)
            'Series1.Points(0).LegendText = "Apple"
            'Series1.Points.Add(27.0)
            'Series1.Points(1).LegendText = "Banana"
            'Series1.Points.Add(33.0)
            'Series1.Points(2).LegendText = "Orange"
            'Series1.Points.Add(18.0)
            'Series1.Points(3).LegendText = "Grape"

            Contents.DrawChart(PiePdfChart, 5.6, 5.0)

            '' restore graphics state
            'Contents.RestoreGraphicsState()

            Document.CreateFile()
        End Using
    End Sub

End Module
