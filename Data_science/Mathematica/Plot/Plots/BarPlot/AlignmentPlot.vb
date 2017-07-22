#Region "Microsoft.VisualBasic::41b03b5bd8e00446e8a62679cb2e2bd8, ..\sciBASIC#\Data_science\Mathematical\Plots\BarPlot\AlignmentPlot.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports signals = System.ValueTuple(Of Double, Double)

Namespace BarPlot

    ''' <summary>
    ''' 以条形图的方式可视化绘制两个离散的信号的比对的图形
    ''' </summary>
    Public Module AlignmentPlot

        <Extension>
        Public Function Keys(signals As signals()) As Double()
            Return signals.Select(Function(t) t.Item1).ToArray
        End Function

        <Extension>
        Private Function Values(signals As signals()) As Double()
            Return signals.Select(Function(t) t.Item2).ToArray
        End Function

        ''' <summary>
        ''' 以条形图的方式可视化绘制两个离散的信号的比对的图形
        ''' </summary>
        ''' <param name="query">The query signals</param>
        ''' <param name="subject">The subject signal values</param>
        ''' <param name="cla$">Color expression for <paramref name="query"/></param>
        ''' <param name="clb$">Color expression for <paramref name="subject"/></param>
        ''' <param name="displayX">是否在信号的柱子上面显示出X坐标的信息</param>
        ''' <returns></returns>
        <Extension>
        Public Function PlotAlignment(query As (X#, value#)(), subject As (X#, value#)(),
                                      Optional xrange As DoubleRange = Nothing,
                                      Optional yrange As DoubleRange = Nothing,
                                      Optional size$ = "1200,800",
                                      Optional padding$ = "padding: 70 30 50 100;",
                                      Optional cla$ = "steelblue",
                                      Optional clb$ = "brown",
                                      Optional xlab$ = "X",
                                      Optional ylab$ = "Y",
                                      Optional labelCSS$ = CSSFont.Win7Bold,
                                      Optional queryName$ = "query",
                                      Optional subjectName$ = "subject",
                                      Optional title$ = "Alignments Plot",
                                      Optional tickCSS$ = CSSFont.Win7Normal,
                                      Optional titleCSS$ = CSSFont.Win10NormalLarger,
                                      Optional legendFontCSS$ = CSSFont.Win10Normal,
                                      Optional bw! = 8,
                                      Optional format$ = "F2",
                                      Optional displayX As Boolean = True,
                                      Optional X_CSS$ = CSSFont.Win10Normal,
                                      Optional yAxislabelPosition As YlabelPosition = YlabelPosition.InsidePlot,
                                      Optional labelPlotStrength# = 0.25) As GraphicsData

            If xrange Is Nothing Then
                xrange = New DoubleRange(query.Keys.Join(subject.Keys).ToArray)
            End If
            If yrange Is Nothing Then
                yrange = New DoubleRange(query.Values.Join(subject.Values).ToArray)
            End If

            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)

                    Dim rect As Rectangle = region.PlotRegion
                    Dim yLength! = yrange.Length
                    Dim xLength! = xrange.Length
                    Dim xmin# = xrange.Min
                    Dim ymid! = rect.Height / 2 + region.Padding.Top
                    Dim width! = rect.Width
                    Dim height! = rect.Height / 2
                    Dim yscale = Function(y!)
                                     ' 因为ymin总是0，所以在这里就不需要将减ymin写出来了
                                     Return (y / yLength) * (height)
                                 End Function
                    Dim xscale = Function(x!)
                                     ' width 乘上百分比
                                     Return ((x - xmin) / xLength) * width
                                 End Function

                    With rect
                        Dim axisPen As New Pen(Color.Black, 2)
                        Dim dy = yrange.Length / 5
                        Dim y!
                        Dim gridPen As New Pen(Color.Gray, 1) With {
                            .DashStyle = DashStyle.Dot,
                            .DashPattern = {10.0!, 10.0!}
                        }
                        Dim dt! = 15
                        Dim tickPen As New Pen(Color.Black, 1)
                        Dim tickFont As Font = CSSFont.TryParse(tickCSS, [default]:=New Font(FontFace.MicrosoftYaHei, 12.0!)).GDIObject
                        Dim drawlabel = Sub(c As IGraphics, label$)
                                            Dim tsize = c.MeasureString(label, tickFont)
                                            Call c.DrawString(label, tickFont, Brushes.Black, New Point(.Left - dt - tsize.Width, y - tsize.Height / 2))
                                        End Sub

                        If TypeOf g Is Graphics2D Then
                            DirectCast(g, Graphics2D).Stroke = tickPen
                        End If

                        For i As Integer = 0 To 5
                            Dim label$ = (i * dy).ToString(format) & "%"

                            y = ymid - yscale(i * dy) ' 上半部分
                            Call g.DrawLine(tickPen, New PointF(.Left, y), New Point(.Left - dt, y))
                            Call g.DrawLine(gridPen, New Point(.Left, y), New Point(.Right, y))
                            Call drawlabel(g, label)

                            If i = 0 Then
                                Continue For
                            End If

                            y = ymid + yscale(i * dy) ' 下半部分
                            Call g.DrawLine(tickPen, New PointF(.Left, y), New Point(.Left - dt, y))
                            Call g.DrawLine(gridPen, New Point(.Left, y), New Point(.Right, y))
                            Call drawlabel(g, label)
                        Next

                        Dim labelFont As Font = CSSFont.TryParse(labelCSS, [default]:=New Font(FontFace.MicrosoftYaHei, 12.0!, FontStyle.Bold)).GDIObject

                        ' Y 坐标轴
                        Call g.DrawLine(axisPen, .Location, New Point(.Left, .Bottom))
                        Select Case yAxislabelPosition
                            Case YlabelPosition.InsidePlot
                                Call g.DrawImageUnscaled(Axis.DrawLabel(ylab, labelFont, ), New Point(.Left + 3, .Top))
                            Case YlabelPosition.LeftCenter
                                Dim labelImage = Axis.DrawLabel(ylab, labelFont, )
                                Dim yLabelPoint As New Point(
                                    (.Left - labelImage.Width) / 3,
                                    .Top + (.Height - labelImage.Height) / 2)
                                Call g.DrawImageUnscaled(labelImage, yLabelPoint)
                            Case Else
                                ' 不进行标签的绘制
                        End Select

                        ' X 坐标轴
                        Dim fWidth! = g.MeasureString(xlab, labelFont).Width
                        Call g.DrawLine(axisPen, New Point(.Left, ymid), New Point(.Right, ymid))
                        Call g.DrawString(xlab, labelFont, Brushes.Black, New Point(.Right - fWidth, ymid + 2))

                        Dim left!
                        Dim ba As New SolidBrush(cla.TranslateColor)
                        Dim bb As New SolidBrush(clb.TranslateColor)
                        Dim xCSSFont As Font = CSSFont.TryParse(X_CSS).GDIObject
                        Dim xsz As SizeF
                        Dim xpos As PointF
                        Dim xlabel$

#Region "绘制柱状图"
                        For Each o In query
                            y = o.value
                            y = ymid - yscale(y)
                            left = region.Padding.Left + xscale(o.X)
                            rect = New Rectangle(New Point(left, y), New Size(bw, yscale(o.value)))
                            g.FillRectangle(ba, rect)
                        Next
                        For Each o In subject
                            y = o.value
                            y = ymid + yscale(y)
                            left = region.Padding.Left + xscale(o.X)
                            rect = Rectangle(ymid, left, left + bw, y)
                            g.FillRectangle(bb, rect)
                        Next
#End Region
                        ' 考虑到x轴标签可能会被柱子挡住，所以在这里将柱子和x标签的绘制分开在两个循环之中来完成
#Region "绘制横坐标轴"
                        For Each o In query
                            y = o.value
                            y = ymid - yscale(y)
                            left = region.Padding.Left + xscale(o.X)
                            rect = New Rectangle(New Point(left, y), New Size(bw, yscale(o.value)))

                            If displayX AndAlso o.value / yLength >= labelPlotStrength Then
                                xlabel = o.X.ToString("F2")
                                xsz = g.MeasureString(xlabel, xCSSFont)
                                xpos = New PointF(rect.Left + (rect.Width - xsz.Width) / 2, rect.Top - xsz.Height)
                                g.DrawString(xlabel, xCSSFont, Brushes.Black, xpos)
                            End If
                        Next
                        For Each o In subject
                            y = o.value
                            y = ymid + yscale(y)
                            left = region.Padding.Left + xscale(o.X)
                            rect = Rectangle(ymid, left, left + bw, y)

                            If displayX AndAlso o.value / yLength >= labelPlotStrength Then
                                xlabel = o.X.ToString("F2")
                                xsz = g.MeasureString(xlabel, xCSSFont)
                                xpos = New PointF(rect.Left + (rect.Width - xsz.Width) / 2, rect.Bottom + 3)
                                g.DrawString(xlabel, xCSSFont, Brushes.Black, xpos)
                            End If
                        Next
#End Region
                        rect = region.PlotRegion

                        ' legend 的圆角矩形
                        Call Shapes.RoundRect.Draw(
                            g,
                            New Point(rect.Right - 340, rect.Top + 6),
                            New Size(330, 80), 8,
                            Brushes.White,
                            New Stroke With {
                                .dash = DashStyle.Solid,
                                .fill = "black",
                                .width = 2
                            })

                        Dim box As Rectangle
                        Dim legendFont As Font = CSSFont _
                            .TryParse(legendFontCSS, [default]:=New Font(FontFace.MicrosoftYaHei, 16.0!)) _
                            .GDIObject
                        Dim fHeight! = g.MeasureString("1", legendFont).Height

                        y = 3

                        box = New Rectangle(New Point(rect.Right - 330, rect.Top + 20), New Size(20, 20))
                        Call g.FillRectangle(ba, box)
                        Call g.DrawString(queryName, legendFont, Brushes.Black, box.Location.OffSet2D(25, -y))

                        box = New Rectangle(New Point(box.Left, box.Top + 30), box.Size)
                        Call g.FillRectangle(bb, box)
                        Call g.DrawString(subjectName, legendFont, Brushes.Black, box.Location.OffSet2D(25, -y))

                        Dim titleFont As Font = CSSFont _
                            .TryParse(titleCSS, [default]:=New Font(FontFace.MicrosoftYaHei, 16.0!)) _
                            .GDIObject
                        Dim titleSize As SizeF = g.MeasureString(title, titleFont)
                        Dim tl As New Point(
                            rect.Left + (rect.Width - titleSize.Width) / 2,
                            (region.Padding.Top - titleSize.Height) / 2)

                        Call g.DrawString(title, titleFont, Brushes.Black, tl)
                    End With
                End Sub

            Return g.GraphicsPlots(
                size.SizeParser, padding,
                "white",
                plotInternal)
        End Function
    End Module
End Namespace
