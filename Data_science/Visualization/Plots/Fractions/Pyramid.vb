#Region "Microsoft.VisualBasic::dac92bcd5dfe9a378ea773983a19c1ec, Data_science\Visualization\Plots\Fractions\Pyramid.vb"

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

    '   Total Lines: 147
    '    Code Lines: 114 (77.55%)
    ' Comment Lines: 13 (8.84%)
    '    - Xml Docs: 76.92%
    ' 
    '   Blank Lines: 20 (13.61%)
    '     File Size: 5.79 KB


    '     Module Pyramid
    ' 
    '         Function: Plot
    ' 
    '     Class PyramidPlot
    ' 
    '         Properties: wp
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: PlotInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
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

Namespace Fractions

    Public Module Pyramid

        ''' <summary>
        ''' 绘制金字塔图，用来表示占比的数据可视化
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="size"></param>
        ''' <param name="padding$"></param>
        ''' <param name="bg$"></param>
        ''' <param name="legendBorder"></param>
        ''' <param name="wp#"></param>
        ''' <returns></returns>
        Public Function Plot(data As IEnumerable(Of FractionData),
                             Optional size As Size = Nothing,
                             Optional padding$ = g.DefaultPadding,
                             Optional bg$ = "white",
                             Optional legendBorder As Stroke = Nothing,
                             Optional wp# = 0.8) As GraphicsData

            Dim array As FractionData() = data.OrderByDescending(Function(x) x.Percentage).ToArray
            Dim margin As Padding = padding

            If size.IsEmpty Then
                size = New Size(3000, 2000)
            End If

            Dim theme As New Theme With {
                .padding = margin,
                .legendBoxStroke = legendBorder?.CSSValue,
                .background = bg
            }
            Dim app As New PyramidPlot(array, theme) With {
                .wp = wp
            }

            Return app.Plot(size)
        End Function
    End Module

    Public Class PyramidPlot : Inherits Plot

        ReadOnly array As FractionData()

        Public Property wp As Double = 0.8

        Public Sub New(array As FractionData(), theme As Theme)
            MyBase.New(theme)
            Me.array = array
        End Sub

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, region As GraphicsRegion)
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim margin As Padding = region.Padding
            Dim height% = region.PlotRegion(css).Height
            Dim width% = region.PlotRegion(css).Width * wp
            Dim left! = (region.PlotRegion(css).Width - width) / 2 + css.GetWidth(margin.Left)
            Dim tan_ab = height / (width / 2) ' tan(a)
            Dim right! = (left + width)
            Dim bottom! = region.PlotRegion(css).Bottom

            For Each l As FractionData In array
                Dim dh! = height * l.Percentage
                Dim dw! = dh / tan_ab
                ' b/| dh |\c
                ' ---    ---
                ' a        d
                Dim a As New Point(left, bottom)
                Dim b As New Point(left + dw, a.Y - dh)
                Dim c As New Point(right - dw, b.Y)
                Dim d As New Point(right, a.Y)

                Dim path As New GraphicsPath
                path.AddLine(a, b)
                path.AddLine(b, c)
                path.AddLine(c, d)
                path.AddLine(d, a)
                path.CloseAllFigures()

                Call g.FillPath(New SolidBrush(l.Color), path)

                left += dw
                bottom -= dh
                width -= dw * 2
                right -= dw
            Next

            Dim size As Size = g.Size
            Dim font As New Font(FontFace.MicrosoftYaHei, 32)
            Dim gr As IGraphics = g
            Dim maxL = array.Select(Function(x) gr.MeasureString(x.Name, font).Width).Max
            left = size.Width - (margin.Horizontal(css)) - maxL
            Dim top = css.GetHeight(margin.Top)
            Dim legends As New List(Of LegendObject)
            Dim legendBorder As Stroke = Stroke.TryParse(theme.legendBoxStroke)

            For Each x As FractionData In array
                legends += New LegendObject With {
                    .color = x.Color.RGBExpression,
                    .style = LegendStyles.Rectangle,
                    .title = x.Name,
                    .fontstyle = CSSFont.GetFontStyle(font.Name, font.Style, font.Size)
                }
            Next

            Call g.DrawLegends(New Point(left, top), legends, ,, shapeBorder:=legendBorder)
        End Sub
    End Class
End Namespace
