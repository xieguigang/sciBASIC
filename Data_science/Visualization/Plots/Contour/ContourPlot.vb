#Region "Microsoft.VisualBasic::aa0b79c050ecdcfbfdfc77c7c526c94b, Data_science\Visualization\Plots\Contour\ContourPlot.vb"

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

    '   Total Lines: 129
    '    Code Lines: 106 (82.17%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 23 (17.83%)
    '     File Size: 5.41 KB


    '     Class ContourPlot
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: getDimensions
    ' 
    '         Sub: PlotInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.MarchingSquares
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render

#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
Imports FontStyle = System.Drawing.FontStyle
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
Imports FontStyle = Microsoft.VisualBasic.Imaging.FontStyle
#End If

Namespace Contour

    Public Class ContourPlot : Inherits Plot

        ReadOnly contours As GeneralPath()

        Friend xlim As Double = Double.NaN
        Friend ylim As Double = Double.NaN

        Public Sub New(sample As IEnumerable(Of MeasureData), interpolateFill As Boolean, theme As Theme)
            MyBase.New(theme)

            contours = ContourLayer _
                .GetContours(sample, interpolateFill:=interpolateFill) _
                .ToArray
        End Sub

        Sub New(sample As IEnumerable(Of GeneralPath), theme As Theme)
            MyBase.New(theme)

            contours = sample.OrderBy(Function(g) g.level).ToArray
        End Sub

        Sub New(sample As IEnumerable(Of ContourLayer), theme As Theme)
            MyBase.New(theme)

            contours = sample _
                .OrderBy(Function(layer) layer.threshold) _
                .Select(Function(layer) New GeneralPath(layer)) _
                .ToArray
        End Sub

        Private Function getDimensions() As Size
            Dim layers = contours.Select(Function(layer) layer.GetContour.shapes).IteratesALL.ToArray
            Dim size As Size

            If layers.Length = 0 Then
                size = New Size
            Else
                Dim w As Integer = Aggregate polygon In layers Into Max(polygon.x.Max)
                Dim h As Integer = Aggregate polygon In layers Into Max(polygon.y.Max)

                size = New Size(w, h)
            End If

            If Not xlim.IsNaNImaginary Then
                size = New Size(xlim, size.Height)
            End If
            If Not ylim.IsNaNImaginary Then
                size = New Size(size.Width, ylim)
            End If

            Return size
        End Function

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            Dim level_cutoff As Double() = contours.Select(Function(c) c.level).ToArray
            Dim colors As SolidBrush() = Designer _
                .GetColors(theme.colorSet, level_cutoff.Length) _
                .Select(Function(c) New SolidBrush(c)) _
                .ToArray
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim i As i32 = Scan0
            Dim dims As Size = getDimensions()
            Dim rect As Rectangle = canvas.PlotRegion(css)

            If Not (xlim.IsNaNImaginary AndAlso ylim.IsNaNImaginary) Then
                Call g.FillRectangle(colors(Scan0), rect)
            End If

            If dims.Width * dims.Height > 0 Then
                Dim scaleX = d3js.scale.linear.domain(values:=New Double() {0, dims.Width}).range(values:=New Double() {rect.Left, rect.Right})
                Dim scaleY = d3js.scale.linear.domain(values:=New Double() {0, dims.Height}).range(values:=New Double() {rect.Top, rect.Bottom})

                For Each polygon As GeneralPath In contours
                    Dim color As SolidBrush = colors(++i)

                    Call polygon.Fill(g, color, scaleX, scaleY)
                    Call polygon.Draw(g, Pens.Black, scaleX, scaleY)
                Next
            End If

            Dim layout As New Rectangle(rect.Right + 10, rect.Top, canvas.Padding.Right / 3 * 2, rect.Height / 3 * 2)
            Dim legendTitleFont As Font = css.GetFont(CSSFont.TryParse(theme.legendTitleCSS))
            Dim tickFont As Font = css.GetFont(CSSFont.TryParse(theme.legendTickCSS))
            Dim tickStroke As Pen = css.GetPen(Stroke.TryParse(theme.legendTickAxisStroke))

            Call g.ColorMapLegend(layout, colors, level_cutoff, legendTitleFont, title:=legendTitle, tickFont, tickStroke)
        End Sub
    End Class
End Namespace
