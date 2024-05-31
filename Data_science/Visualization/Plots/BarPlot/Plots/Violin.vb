#Region "Microsoft.VisualBasic::68d827d1d049e56f2e5d777f8cbe25c6, Data_science\Visualization\Plots\BarPlot\Plots\Violin.vb"

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

    '   Total Lines: 276
    '    Code Lines: 216 (78.26%)
    ' Comment Lines: 17 (6.16%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 43 (15.58%)
    '     File Size: 11.10 KB


    ' Class Violin
    ' 
    '     Properties: showStats, splineDegree
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: removesOutliers
    ' 
    '     Sub: PlotInternal, PlotViolin
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Interpolation
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports std = System.Math

Public Class Violin : Inherits Plot

    ReadOnly matrix As NamedCollection(Of Double)()

    Public Property showStats As Boolean
    Public Property splineDegree As Integer

    Public Sub New(matrix As IEnumerable(Of NamedCollection(Of Double)), theme As Theme)
        MyBase.New(theme)

        Me.matrix = matrix.ToArray
    End Sub

    Public Shared Iterator Function removesOutliers(matrix As IEnumerable(Of NamedCollection(Of Double))) As IEnumerable(Of NamedCollection(Of Double))
        For Each i As NamedCollection(Of Double) In matrix
            Dim quar = i.Quartile
            Dim normals = quar.Outlier(i).normal

            Yield New NamedCollection(Of Double) With {
                .name = i.name,
                .description = i.description,
                .value = normals
            }
        Next
    End Function

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        ' 用来构建Y坐标轴的总体数据
        Dim alldata = matrix _
            .Select(Function(d)
                        Dim dq = d.Quartile
                        Return {dq.Q1 - 1.5 * dq.IQR, dq.Q3 + 1.5 * dq.IQR}.AsList + d.AsEnumerable
                    End Function) _
            .IteratesALL _
            .ToArray
        Dim css As CSSEnvirnment = g.LoadEnvironment
        Dim ppi As Integer = g.Dpi
        Dim yticks = alldata.Range.CreateAxisTicks
        Dim yTickFont As Font = css.GetFont(CSSFont.TryParse(theme.axisTickCSS))
        Dim yTickColor As Brush = CSSFont.TryParse(theme.axisTickCSS).color.GetBrush
        Dim colors As LoopArray(Of Color) = Designer.GetColors(theme.colorSet, matrix.Length)
        Dim labelSize As SizeF
        Dim labelFont As Font = css.GetFont(CSSFont.TryParse(theme.axisLabelCSS))
        Dim labelColor As Brush = CSSFont.TryParse(theme.axisLabelCSS).color.GetBrush
        Dim labelPos As PointF
        Dim polygonStroke As Pen = css.GetPen(Stroke.TryParse(theme.lineStroke))
        Dim titleFont As Font = css.GetFont(CSSFont.TryParse(theme.mainCSS))
        Dim plotRegion As Rectangle = canvas.PlotRegion
        Dim Y = d3js.scale _
            .linear _
            .domain(values:=yticks) _
            .range(integers:={plotRegion.Top, plotRegion.Bottom})
        Dim yScale As New YScaler(False) With {
            .region = plotRegion,
            .Y = Y
        }

        Call Axis.DrawY(g,
                        Pens.Black,
                        ylabel,
                        yScale,
                        0,
                        yticks,
                        YAxisLayoutStyles.Left,
                        Nothing,
                        theme.axisLabelCSS,
                        labelColor,
                        yTickFont,
                        yTickColor,
                        htmlLabel:=False,
                        tickFormat:=theme.YaxisTickFormat
        )

        Dim groupInterval As Double = plotRegion.Width * 0.1
        Dim maxWidth As Double = (plotRegion.Width - groupInterval) / matrix.Length

        groupInterval = groupInterval / matrix.Length

        Dim semiWidth As Double = maxWidth / 2
        Dim X As Single = plotRegion.Left + groupInterval / 2 + semiWidth

        labelSize = g.MeasureString(main, titleFont)
        labelPos = New PointF With {
            .X = plotRegion.Left + (plotRegion.Width - labelSize.Width) / 2,
            .Y = plotRegion.Y - labelSize.Height
        }
        g.DrawString(main, titleFont, Brushes.Black, labelPos)

        For Each group As NamedCollection(Of Double) In matrix
            Call PlotViolin(group,
                            X,
                            yScale,
                            25,
                            semiWidth,
                            splineDegree,
                            polygonStroke,
                            showStats,
                            True,
                            labelFont,
                            color:=++colors,
                            g:=g,
                            canvas:=canvas,
                            theme:=theme)

            If theme.xAxisRotate = 0.0 Then
                labelPos = New PointF With {
                .X = X - labelSize.Width / 2,
                .Y = plotRegion.Bottom + labelSize.Height * 1.125
            }

                Call g.DrawString(group.name, labelFont, Brushes.Black, labelPos)
            Else
                labelPos = New PointF With {
                        .X = X - labelSize.Width / 2,
                        .Y = plotRegion.Bottom + labelSize.Width * std.Sin(std.PI / 4)
                    }

                ' 绘制X坐标轴分组标签
                Call New GraphicsText(DirectCast(g, GDICanvas).Graphics).DrawString(
                s:=group.name,
                font:=labelFont,
                brush:=Brushes.Black,
                point:=labelPos,
                angle:=theme.xAxisRotate
            )
            End If

            X += semiWidth + groupInterval + semiWidth
        Next
    End Sub

    Public Shared Sub PlotViolin(group As NamedCollection(Of Double),
                                 x As Single,
                                 yscale As YScaler,
                                 nbins As Integer,
                                 semiWidth As Double,
                                 splineDegree As Single,
                                 polygonStroke As Pen,
                                 showStats As Boolean,
                                 zeroBreak As Boolean,
                                 labelFont As Font,
                                 color As Color,
                                 g As IGraphics,
                                 canvas As GraphicsRegion,
                                 theme As Theme)

        Dim plotRegion As Rectangle = canvas.PlotRegion
        Dim quartile As DataQuartile = group.Quartile
        Dim lowerBound = quartile.Q1 - 1.5 * quartile.IQR
        Dim upperBound = quartile.Q3 + 1.5 * quartile.IQR
        Dim upper = yscale.TranslateY(upperBound)
        Dim lower = yscale.TranslateY(lowerBound)
        ' 计算数据分布的密度之后，进行左右对称的线条的生成
        Dim line_l As New List(Of PointF)
        Dim line_r As New List(Of PointF)
        Dim q0 = lowerBound  'group.Min
        Dim dstep = (upperBound - lowerBound) / nbins ' (group.Max - group.Min) / n
        Dim dy = std.Abs(upper - lower) / nbins

        For p As Integer = 0 To nbins
            Dim q1 = q0 + dstep
            Dim range As DoubleRange = {q0, q1}
            Dim density = quartile.ModelSamples.normal.Count(AddressOf range.IsInside)
            Dim yi As Double = lower - p * dy

            If yi <= 0 AndAlso zeroBreak Then
                Exit For
            End If

            line_l += New PointF With {.X = density, .Y = yi}
            line_r += New PointF With {.X = density, .Y = yi}
            q0 = q1
        Next

        ' Call $"{group.name} = {New Double() {group.Min, group.Max}.GetJson}".__DEBUG_ECHO

        ' 进行宽度伸缩映射
        Dim maxDensity As DoubleRange = line_l.X
        Dim densityWidth As Single

        For i As Integer = 0 To line_r.Count - 1
            densityWidth = (line_l(i).X - maxDensity.Min) / maxDensity.Length * semiWidth

            line_l(i) = New PointF With {.X = x - densityWidth, .Y = line_l(i).Y}
            line_r(i) = New PointF With {.X = x + densityWidth, .Y = line_r(i).Y}
        Next

        line_l = line_l.BSpline(degree:=splineDegree).AsList
        line_r = line_r.BSpline(degree:=splineDegree).AsList

        ' 需要插值么？
        ' 生成多边形
        Dim polygon As New List(Of PointF)

        ' 左下 -> 左上
        polygon += line_l
        ' 左上 -> 右上
        polygon += line_r.Last
        ' 右上 -> 右下
        polygon += line_r.ReverseIterator.Skip(1)
        ' 最后 右下 -> 左下会自动封闭

        ' 绘制当前的这个多边形
        Call g.DrawPolygon(polygonStroke, polygon)
        Call g.FillPolygon(New SolidBrush(color), polygon)

        ' 绘制quartile
        Dim yQ1 As Double = yscale.TranslateY(quartile.Q1)
        Dim wd As Double = semiWidth * 0.65

        If wd > 32 Then
            wd = 32
        End If

        ' draw IQR
        Dim iqrBox As New RectangleF With {
            .Width = wd,
            .X = x - .Width / 2,
            .Y = yscale.TranslateY(quartile.Q3),
            .Height = std.Abs(.Y - yQ1)
        }

        Call g.FillRectangle(polygonStroke.Brush, iqrBox)

        ' draw 95% CI
        upperBound = group.Average + 1.96 * group.SD
        lowerBound = group.Average - 1.96 * group.SD

        Dim lowerDraw As Double = lowerBound

        If zeroBreak AndAlso lowerDraw < 0 Then
            lowerDraw = 0
        End If

        Call g.DrawLine(
            pen:=polygonStroke,
            pt1:=New PointF(x, yscale.TranslateY(lowerDraw)),
            pt2:=New PointF(x, yscale.TranslateY(upperBound))
        )

        ' draw median point
        Call g.DrawCircle(New PointF(x + 1, yscale.TranslateY(quartile.Q2) - 1), wd / 4, color:=Pens.White)

        If showStats Then
            ' 在右上绘制数据的分布信息
            Dim sampleDescrib As String =
                $"CI95%: {lowerBound.ToString(theme.YaxisTickFormat)} ~ {upperBound.ToString(theme.YaxisTickFormat)}" & vbCrLf &
                $"Median: {quartile.Q2.ToString(theme.YaxisTickFormat)}" & vbCrLf &
                $"Normal Range: {(quartile.Q1 - 1.5 * quartile.IQR).ToString(theme.YaxisTickFormat)} ~ {(quartile.Q3 + 1.5 * quartile.IQR).ToString(theme.YaxisTickFormat)}"

            Dim labelSize = g.MeasureString(group.name, labelFont)

            Call g.DrawString(sampleDescrib, labelFont, Brushes.Black, New PointF(x + semiWidth / 5, upper + labelSize.Height * 2))
        End If
    End Sub
End Class
