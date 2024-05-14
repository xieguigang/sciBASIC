#Region "Microsoft.VisualBasic::a9d1c622d3793b1e51fe219b77fcfd47, Data_science\Visualization\Plots\Fractions\Pyramid.vb"

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

    '   Total Lines: 99
    '    Code Lines: 72
    ' Comment Lines: 13
    '   Blank Lines: 14
    '     File Size: 3.94 KB


    '     Module Pyramid
    ' 
    '         Function: Plot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.g
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Html.CSS

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

            Dim array As FractionData() =
            data _
            .OrderByDescending(Function(x) x.Percentage) _
            .ToArray
            Dim margin As Padding = padding

            If size.IsEmpty Then
                size = New Size(3000, 2000)
            End If

            Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)
                Dim height% = region.PlotRegion.Height
                Dim width% = region.PlotRegion.Width * wp
                Dim left! = (region.PlotRegion.Width - width) / 2 + margin.Left
                Dim tan_ab = height / (width / 2) ' tan(a)
                Dim right! = (left + width)
                Dim bottom! = region.PlotRegion.Bottom

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

                Dim font As New Font(FontFace.MicrosoftYaHei, 32)
                Dim gr As IGraphics = g
                Dim maxL = data.Select(Function(x) gr.MeasureString(x.Name, font).Width).Max
                left = size.Width - (margin.Horizontal) - maxL
                Dim top = margin.Top
                Dim legends As New List(Of LegendObject)

                For Each x As FractionData In data
                    legends += New LegendObject With {
                       .color = x.Color.RGBExpression,
                       .style = LegendStyles.Rectangle,
                       .title = x.Name,
                       .fontstyle = CSSFont.GetFontStyle(font.Name, font.Style, font.Size)
                    }
                Next

                Call g.DrawLegends(New Point(left, top), legends, ,, shapeBorder:=legendBorder)
            End Sub

            Return GraphicsPlots(size, margin, bg, plotInternal)
        End Function
    End Module
End Namespace
