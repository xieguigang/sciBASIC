#Region "Microsoft.VisualBasic::60a15f208598b5cf7d89ab2de85f461e, Data_science\Visualization\Plots\Scatter\Bubble.vb"

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

    ' Module Bubble
    ' 
    '     Function: logRadius, Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports stdNum = System.Math

Public Module Bubble

    Private Function logRadius(R#) As Double
        Return stdNum.Log(R + 1) + 1
    End Function

    ReadOnly usingLogRadius As New [Default](Of Func(Of Double, Double))(AddressOf logRadius)

    ''' <summary>
    ''' <see cref="PointData.value"/>是Bubble的半径大小
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="size"></param>
    ''' <param name="bg"></param>
    ''' <param name="legend"></param>
    ''' <param name="xAxis"><see cref="AxisProvider"/></param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(data As IEnumerable(Of SerialData),
                         Optional size As Size = Nothing,
                         Optional padding$ = g.DefaultPadding,
                         Optional bg As String = "white",
                         Optional legend As Boolean = True,
                         Optional usingLogScaleRadius As Boolean = False,
                         Optional legendBorder As Stroke = Nothing,
                         Optional bubbleBorder As Stroke = Nothing,
                         Optional xAxis$ = Nothing,
                         Optional yAxis$ = Nothing,
                         Optional xlabel$ = "",
                         Optional ylabel$ = "",
                         Optional axisLabelFontCSS$ = CSSFont.Win7LargeBold,
                         Optional tagFontCSS$ = CSSFont.Win10Normal,
                         Optional strokeColorAsMainColor As Boolean = False,
                         Optional positiveRangeY As Boolean = False,
                         Optional legendLabelFontCSS$ = CSSFont.Win10NormalLarge,
                         Optional legendAnchor As PointF = Nothing) As GraphicsData

        Dim margin As Padding = padding
        Dim tagLabelFont As Font = CSSFont.TryParse(tagFontCSS).GDIObject

        Dim plotInternal =
            Sub(ByRef g As IGraphics, grect As GraphicsRegion)
                Dim array As SerialData() = data.ToArray
                Dim mapper As Mapper
                Dim rangeData As New Scaling(array, False)

                If xAxis.StringEmpty Then
                    ' 任意一个位空值就会使用普通的axis数据计算方法
                    ' 这个并不是以y值来表示数量上的关系的，point是随机位置，所以在这里使用相对scalling
                    mapper = New Mapper(rangeData)
                Else
                    mapper = rangeData.yrange _
                        .GetAxisValues _
                        .DoCall(Function(axisdata) New AxisProvider(axisdata)) _
                        .DoCall(Function(ya)
                                    Return New Mapper(x:=xAxis, y:=ya, range:=rangeData)
                                End Function)
                End If

                Dim scale As Func(Of Double, Double) = New Func(Of Double, Double)(Function(r) r) Or usingLogRadius.When(usingLogScaleRadius)
                Dim x, y As d3js.scale.LinearScale
                Dim xTicks = array.Select(Function(sr) sr.Select(Function(p) CDbl(p.pt.X))).IteratesALL.CreateAxisTicks
                Dim yTicks = array.Select(Function(sr) sr.Select(Function(p) CDbl(p.pt.Y))).IteratesALL.CreateAxisTicks
                Dim canvas = g
                Dim labels As New List(Of Label)
                Dim anchors As New List(Of Anchor)
                Dim labelSize As SizeF
                Dim plotrect As Rectangle = grect.PlotRegion

                If positiveRangeY Then
                    yTicks = yTicks.Where(Function(t) t >= 0).ToArray
                End If

                With grect.PlotRegion
                    x = d3js.scale.linear.domain(xTicks).range(integers:={ .Left, .Right})
                    y = d3js.scale.linear.domain(yTicks).range(integers:={ .Top, .Bottom})
                End With

                Dim scaler As New DataScaler With {
                    .AxisTicks = (xTicks, yTicks),
                    .region = grect.PlotRegion,
                    .X = x,
                    .Y = y
                }

                Call g.DrawAxis(
                    region:=grect,
                    scaler:=scaler,
                    showGrid:=True,
                    xlabel:=xlabel,
                    ylabel:=ylabel,
                    labelFont:=axisLabelFontCSS,
                    htmlLabel:=False
                )

                Dim bubblePen As Pen = Nothing

                If Not bubbleBorder Is Nothing Then
                    bubblePen = bubbleBorder.GDIObject
                End If

                For Each s As SerialData In mapper.ForEach(size, margin)
                    Dim b As SolidBrush = Nothing
                    Dim getRadius = Function(pt As PointData)
                                        Dim r# = scale(pt.value)

                                        If r = 0R Then
                                            Return s.pointSize
                                        Else
                                            Return r
                                        End If
                                    End Function

                    If Not (s.color.IsEmpty) Then
                        b = New SolidBrush(s.color)
                    End If

                    For Each pt As PointData In s
                        Dim r As Double = getRadius(pt)
                        Dim p As New Point(CInt(pt.pt.X - r), CInt(pt.pt.Y - r))
                        Dim rect As New Rectangle(p, New Size(r * 2, r * 2))

                        With pt.color
                            If .StringEmpty Then
                                Call g.FillPie(b, rect, 0, 360)
                            Else
                                Call g.FillPie(New SolidBrush(.TranslateColor), rect, 0, 360)
                            End If
                        End With

                        If pt.stroke.StringEmpty Then
                            If Not bubblePen Is Nothing Then
                                Call g.DrawCircle(pt.pt, r, bubblePen, fill:=False)
                            End If
                        Else
                            Call Stroke.TryParse(pt.stroke) _
                                .GDIObject _
                                .DoCall(Sub(pen)
                                            Call canvas.DrawCircle(pt.pt, r, pen, fill:=False)
                                        End Sub)

                        End If

                        If Not pt.Tag.StringEmpty Then
                            labelSize = g.MeasureString(pt.Tag, tagLabelFont)
                            labels += New Label With {
                                .text = pt.Tag,
                                .X = rect.Right,
                                .Y = rect.Top,
                                .width = labelSize.Width,
                                .height = labelSize.Height
                            }
                            anchors += New Anchor With {
                                .r = r,
                                .x = rect.Right - r,
                                .y = rect.Top + r
                            }
                        End If
                    Next
                Next

                Call d3js.labeler(30, 1) _
                    .Width(plotrect.Width) _
                    .Height(plotrect.Height) _
                    .Anchors(anchors) _
                    .Labels(labels) _
                    .Start(showProgress:=False, nsweeps:=2000)

                Dim anchor As Anchor
                Dim label As Label

                For Each index As SeqValue(Of Label) In labels.SeqIterator
                    label = index
                    anchor = anchors(index)

                    ' Call g.DrawLine(Pens.Gray, anchor, label.GetTextAnchor(anchor))
                    Call g.DrawString(label.text, tagLabelFont, Brushes.Black, label)
                Next

                If legend Then

                    Dim legendLabelFont As Font = CSSFont.TryParse(legendLabelFontCSS)
                    Dim maxSize! = array _
                        .Select(Function(s) s.title) _
                        .Select(Function(str) canvas.MeasureString(str, legendLabelFont).Width) _
                        .Max
                    Dim topLeft As Point

                    If legendAnchor.IsEmpty Then
                        topLeft = New Point With {
                            .X = grect.PlotRegion.Right - maxSize * 1.5,
                            .Y = margin.Top + grect.PlotRegion.Height * 0.05
                        }
                    Else
                        Dim px As Double
                        Dim py As Double

                        If legendAnchor.X < 1 Then
                            px = grect.PlotRegion.Right - grect.PlotRegion.Width * legendAnchor.X
                        Else
                            px = legendAnchor.X
                        End If
                        If legendAnchor.Y < 1 Then
                            py = margin.Top + grect.PlotRegion.Height * legendAnchor.Y
                        Else
                            py = legendAnchor.Y
                        End If

                        topLeft = New Point With {.X = px, .Y = py}
                    End If

                    Dim legends = LinqAPI.Exec(Of Legend) <=
 _
                        From serial As SerialData
                        In array
                        Let color As String = If(
                            strokeColorAsMainColor,
                            Stroke.TryParse(serial.pts(serial.pts.Length \ 2).stroke).fill,
                            serial.color.RGBExpression)
                        Select New Legend With {
                            .color = color,
                            .fontstyle = legendLabelFontCSS,
                            .style = LegendStyles.Circle,
                            .title = serial.title
                        }

                    Call g.DrawLegends(topLeft, legends,,, shapeBorder:=legendBorder)
                End If
            End Sub

        Return GraphicsPlots(size, margin, bg, plotInternal)
    End Function
End Module
