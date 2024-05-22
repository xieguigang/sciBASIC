#Region "Microsoft.VisualBasic::492e67fc809447d63680984dd0d8a374, Data_science\Visualization\Plots-statistics\TimeTrends\TimeTrends.vb"

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

    '   Total Lines: 320
    '    Code Lines: 243 (75.94%)
    ' Comment Lines: 35 (10.94%)
    '    - Xml Docs: 91.43%
    ' 
    '   Blank Lines: 42 (13.12%)
    '     File Size: 14.04 KB


    ' Module TimeTrends
    ' 
    '     Function: Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports stdNum = System.Math

Public Module TimeTrends

    ''' <summary>
    ''' 绘制时间趋势线
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="size$"></param>
    ''' <param name="padding$"></param>
    ''' <param name="bg$"></param>
    ''' <param name="title$"></param>
    ''' <param name="subTitle$"></param>
    ''' <param name="lineWidth!"></param>
    ''' <param name="lineColor$"></param>
    ''' <param name="pointSize!"></param>
    ''' <param name="pointColor$"></param>
    ''' <param name="rangeColor$"></param>
    ''' <param name="titleColor$"></param>
    ''' <param name="rangeOpacity!"></param>
    ''' <param name="rangeStroke$"></param>
    ''' <param name="axisStrokeCSS$"></param>
    ''' <param name="yTickStrokeCSS$"></param>
    ''' <param name="cubicSplineExpected%"></param>
    ''' <param name="valueLabelFormat$"></param>
    ''' <param name="valueLabelFontCSS$"></param>
    ''' <param name="tickLabelFontCSS$"></param>
    ''' <param name="titleFontCSS$"></param>
    ''' <param name="subTitleFontCSS$"></param>
    ''' <param name="legendTitleFont$"></param>
    ''' <param name="dateFormat"></param>
    ''' <param name="legendTitle$"></param>
    ''' <param name="legendRangeTitle$"></param>
    ''' <param name="legendTitleColor$"></param>
    ''' <param name="displayLegendBorder">是否显示legend的盒子的边框</param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(data As IEnumerable(Of TimePoint),
                         Optional size$ = "3600,2700",
                         Optional padding$ = Canvas.Resolution2K.PaddingWithTopTitleAndBottomLegend,
                         Optional bg$ = "white",
                         Optional title$ = "Time trends",
                         Optional subTitle$ = "Time trends chart",
                         Optional lineWidth! = 20,
                         Optional lineColor$ = "darkblue",
                         Optional pointSize! = 30,
                         Optional pointColor$ = "#6991c7",
                         Optional rangeColor$ = "#d2dff5",
                         Optional titleColor$ = "gray",
                         Optional rangeOpacity! = 0.45,
                         Optional rangeStroke$ = "stroke: darkblue; stroke-width: 1px; stroke-dash: solid;",
                         Optional axisStrokeCSS$ = Stroke.AxisStroke,
                         Optional yTickStrokeCSS$ = Stroke.AxisGridStroke,
                         Optional cubicSplineExpected% = 25,
                         Optional valueLabelFormat$ = "G2",
                         Optional valueLabelFontCSS$ = CSSFont.Win7VeryVeryLarge,
                         Optional tickLabelFontCSS$ = CSSFont.Win7VeryLarge,
                         Optional titleFontCSS$ = CSSFont.Win7UltraLarge,
                         Optional subTitleFontCSS$ = CSSFont.Win7VeryVeryLarge,
                         Optional legendTitleFont$ = CSSFont.Win7VeryLarge,
                         Optional dateFormat As Func(Of Date, String) = Nothing,
                         Optional legendTitle$ = "Trends",
                         Optional legendRangeTitle$ = "[min, max]",
                         Optional legendTitleColor$ = "black",
                         Optional displayLegendBorder As Boolean = True,
                         Optional ppi As Integer = 100) As GraphicsData

        Static shortDateString As New [Default](Of Func(Of Date, String))(Function(d) d.ToShortDateString)

        Dim dates = data.OrderBy(Function(d) d.date).ToArray
        Dim timer As TimeRange = dates _
            .Select(Function(d) d.date) _
            .ToArray
        Dim yTicks#() = dates _
            .Select(Iterator Function(d)
                        Yield d.average
                        Yield d.range.Min
                        Yield d.range.Max
                    End Function) _
            .IteratesALL _
            .Range _
            .CreateAxisTicks(5)
        Dim rangePoly As (min As List(Of PointF), max As List(Of PointF))

        Dim valueLabelFont As Font = CSSFont.TryParse(valueLabelFontCSS).GDIObject(ppi)
        Dim tickLabelFont As Font = CSSFont.TryParse(tickLabelFontCSS).GDIObject(ppi)
        Dim titleFont As Font = CSSFont.TryParse(titleFontCSS).GDIObject(ppi)
        Dim subTitleFont As Font = CSSFont.TryParse(subTitleFontCSS).GDIObject(ppi)

        Dim lineStyle As New Pen(lineColor.TranslateColor, lineWidth)
        Dim axisPen As Pen = Stroke.TryParse(axisStrokeCSS).GDIObject
        Dim yTickPen As Pen = Stroke.TryParse(yTickStrokeCSS).GDIObject
        Dim rgPen As Pen = Stroke.TryParse(rangeStroke).GDIObject
        Dim rgColor As Color = rangeColor _
            .TranslateColor _
            .Alpha(255 * rangeOpacity)
        Dim titleBrush As Brush = titleColor.GetBrush
        Dim pointBrush As New SolidBrush(pointColor.TranslateColor)

        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)
                Dim yScaler = region.YScaler(yTicks)
                Dim xScaler = timer.Scaler(DoubleRange.TryParse(region.XRange))
                Dim rect As Rectangle = region.PlotRegion
                Dim x#, y#
                Dim ty#() = {0, 0, 0}
                Dim trends As New List(Of PointF)
                Dim labelSize As SizeF
                Dim labelText$
                Dim maxLabelXWidth!

                ' 绘制Y坐标轴
                For Each yVal As Double In yTicks
                    y = yScaler(yVal)
                    labelText = yVal
                    labelSize = g.MeasureString(labelText, tickLabelFont)

                    g.DrawLine(yTickPen, CSng(rect.Left), CSng(y), CSng(rect.Right), CSng(y))
                    g.DrawString(labelText, tickLabelFont, Brushes.Gray,
                         x:=rect.Left - labelSize.Width - 5,
                         y:=y - labelSize.Height / 2
                    )
                Next

                ' 绘制X坐标轴
                Call g.DrawLine(axisPen, rect.Left, rect.Bottom, rect.Right, rect.Bottom)

                With New GraphicsText(DirectCast(g, GDICanvas).Graphics)

                    dateFormat = dateFormat Or shortDateString

                    ' 绘制X坐标轴日期标签
                    For Each tickDate As Date In timer.Ticks
                        labelText = dateFormat(tickDate)
                        labelSize = g.MeasureString(labelText, tickLabelFont)
                        x = xScaler(tickDate)
                        x = x - labelSize.Width / 2
                        y = rect.Bottom + labelSize.Width * (3 / 4)

                        maxLabelXWidth = stdNum.Max(
                            labelSize.Width,
                            maxLabelXWidth
                        )

                        .DrawString(s:=labelText,
                                    font:=tickLabelFont,
                                    brush:=Brushes.Black,
                                    point:=New PointF(x, y),
                                    angle:=-35.0!
                         )

                        x = xScaler(tickDate)
                        y = rect.Bottom + labelSize.Height / 2

                        g.DrawLine(axisPen, CInt(x), CInt(y), CInt(x), rect.Bottom)
                    Next
                End With

                rangePoly = (New List(Of PointF), New List(Of PointF))

                For Each time As TimePoint In dates
                    x = xScaler(time.date)
                    ty = {
                        yScaler(time.range.Min),
                        yScaler(time.average),
                        yScaler(time.range.Max)
                    }
                    trends.Add(New PointF(x, ty(1)))
                    rangePoly.min.Add(New PointF(x, ty(0)))
                    rangePoly.max.Add(New PointF(x, ty(2)))
                Next

                With rangePoly
                    Dim a = .max.First +
                            .max.CubicSpline(cubicSplineExpected).AsList +
                            .max.Last
                    Dim b = .min.Last +
                            .min.ReverseIterator.CubicSpline(cubicSplineExpected).AsList +
                            .min.First
                    Dim polygon As GraphicsPath = (a.AsList + b).GraphicsPath
                    Dim br As New SolidBrush(rgColor)

                    Call g.FillPath(br, polygon)

                    If Not rgPen Is Nothing Then
                        For Each line In a.SlideWindows(winSize:=2)
                            Call g.DrawLine(rgPen, line(0), line(1))
                        Next
                        For Each line In b.SlideWindows(winSize:=2)
                            Call g.DrawLine(rgPen, line(0), line(1))
                        Next
                    End If
                End With

                trends = trends.First +
                         trends.CubicSpline(cubicSplineExpected).AsList +
                         trends.Last

                For Each line As SlideWindow(Of PointF) In trends.SlideWindows(winSize:=2)
                    Call g.DrawLine(lineStyle, line(0), line(1))
                Next

                Dim labels As New List(Of Label)
                Dim anchors As New List(Of Anchor)

                For Each time As TimePoint In dates
                    x = xScaler(time.date)
                    y = yScaler(time.average)
                    labelText = time.average.ToString(valueLabelFormat)
                    labelSize = g.MeasureString(labelText, valueLabelFont)
                    labels += New Label With {
                        .width = labelSize.Width,
                        .height = labelSize.Height,
                        .text = labelText,
                        .X = x,
                        .Y = y
                    }
                    anchors += New Anchor With {
                        .r = 5,
                        .x = x,
                        .y = y
                    }

                    Call g.DrawCircle(New PointF(x, y), pointSize, pointBrush)
                Next

                Call d3js.labeler _
                    .Size(rect.Size) _
                    .Labels(labels) _
                    .Anchors(anchors) _
                    .Start(500, showProgress:=False)

                For Each label As Label In labels
                    Call g.DrawString(label.text, valueLabelFont, Brushes.Black, label.X, label.Y)
                Next

                labelSize = g.MeasureString(title, titleFont)
                x = rect.Left + (rect.Width - labelSize.Width) / 2
                y = rect.Top / 2 - labelSize.Height / 2

                g.DrawString(title, titleFont, titleBrush, x, y)

                labelSize = g.MeasureString(subTitle, subTitleFont)
                x = rect.Left + (rect.Width - labelSize.Width) / 2
                y = y + labelSize.Height * 1.25

                g.DrawString(subTitle, subTitleFont, titleBrush, x, y)

                Dim legends As LegendObject() = {
                    New LegendObject With {
                        .color = lineColor,
                        .fontstyle = legendTitleFont,
                        .style = LegendStyles.SolidLine,
                        .title = legendTitle
                    },
                    New LegendObject With {
                        .color = rangeColor,
                        .fontstyle = legendTitleFont,
                        .style = LegendStyles.RoundRectangle,
                        .title = legendRangeTitle
                    }
                }

                Dim canvas = g

                With legends _
                    .Select(Function(l) l.MeasureTitle(canvas)) _
                    .ToArray

                    labelSize = New SizeF(
                        width:= .Max(Function(s) s.Width),
                        height:= .Max(Function(s) s.Height)
                    )
                End With

                x = rect.Left + (rect.Width - labelSize.Width) / 2
                y = rect.Bottom + maxLabelXWidth + 40

                Dim legendBorder As New Stroke With {
                    .dash = DashStyle.Solid, .fill = "black", .width = 5
                }

                If Not displayLegendBorder Then
                    legendBorder = Nothing
                End If

                Call g.DrawLegends(
                    topLeft:=New Point(x, y),
                    legends:=legends,
                    regionBorder:=legendBorder,
                    titleBrush:=legendTitleColor.GetBrush
                )
            End Sub

        Return g.GraphicsPlots(
            size.SizeParser,
            padding, bg,
            plotInternal
        )
    End Function
End Module
