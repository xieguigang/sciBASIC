#Region "Microsoft.VisualBasic::27189afbb0857b5f8146ade8e67b4590, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots\BarPlot\AlignmentPlot.vb"

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
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports signals = System.ValueTuple(Of Double, Double)

Namespace BarPlot

    ''' <summary>
    ''' Visualize and comparing two discrete signals.(以条形图的方式可视化绘制两个离散的信号的比对的图形)
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
        ''' <param name="htmlLabel">Draw axis label using html render?? default is no.</param>
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
                                      Optional labelPlotStrength# = 0.25,
                                      Optional htmlLabel As Boolean = False,
                                      Optional idTag$ = Nothing) As GraphicsData

            Dim q As New Signal With {
                .Name = queryName,
                .Color = cla,
                .signals = query
            }
            Dim s As New Signal With {
                .Name = subjectName,
                .Color = clb,
                .signals = subject
            }

            Return PlotAlignmentGroups({q}, {s}, xrange, yrange,
                                       size, padding,
                                       xlab, ylab, labelCSS, queryName, subjectName,
                                       title, tickCSS, titleCSS,
                                       legendFontCSS, bw, format, displayX, X_CSS,
                                       yAxislabelPosition,
                                       labelPlotStrength,
                                       htmlLabel:=htmlLabel,
                                       idTag:=idTag)
        End Function

        Public Structure Signal
            Dim Name$
            Dim Color$
            Dim signals As signals()

            Public Overrides Function ToString() As String
                Return Name & $" ({Color})"
            End Function
        End Structure

        <Extension> Private Function Hit(highlights#(), err#) As Func(Of Double, (err#, X#, yes As Boolean))
            If highlights.IsNullOrEmpty Then
                Return Function() (-1, -1, False)
            Else
                Return Function(x)
                           Dim e#

                           For Each n In highlights
                               e = Math.Abs(n - x)

                               If e <= err Then
                                   Return (e, n, True)
                               End If
                           Next

                           Return (-1, -1, False)
                       End Function
            End If
        End Function

        ''' <summary>
        ''' 以条形图的方式可视化绘制两个离散的信号的比对的图形，由于绘制的时候是分别对<paramref name="query"/>和<paramref name="subject"/>
        ''' 信号数据使用For循环进行绘图的，所以数组最后一个位置的元素会在最上层
        ''' 并且绘制图例的时候，使用的是最上层的信号的颜色
        ''' </summary>
        ''' <param name="query">The query signals</param>
        ''' <param name="subject">The subject signal values</param>
        ''' <param name="displayX">是否在信号的柱子上面显示出X坐标的信息</param>
        ''' <returns></returns>
        <Extension>
        Public Function PlotAlignmentGroups(query As Signal(), subject As Signal(),
                                            Optional xrange As DoubleRange = Nothing,
                                            Optional yrange As DoubleRange = Nothing,
                                            Optional size$ = "1200,800",
                                            Optional padding$ = "padding: 70 30 50 100;",
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
                                            Optional labelPlotStrength# = 0.25,
                                            Optional hitsHightLights As Double() = Nothing,
                                            Optional xError# = 0.5,
                                            Optional highlight$ = Stroke.StrongHighlightStroke,
                                            Optional highlightMargin! = 2,
                                            Optional htmlLabel As Boolean = False,
                                            Optional idTag$ = Nothing) As GraphicsData
            If xrange Is Nothing Then
                Dim ALL = query _
                    .Select(Function(x) x.signals.Keys) _
                    .Join(subject.Select(Function(x) x.signals.Keys)) _
                    .ToArray
                xrange = New DoubleRange(ALL)
            End If
            If yrange Is Nothing Then
                Dim ALL = query _
                    .Select(Function(x) x.signals.Values) _
                    .Join(subject.Select(Function(x) x.signals.Values)) _
                    .ToArray
                yrange = New DoubleRange(ALL)
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
                                            Dim pos As New Point(.Left - dt - tsize.Width, y - tsize.Height / 2)

                                            Call c.DrawString(label, tickFont, Brushes.Black, pos)
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
                        Dim labSize As SizeF = g.MeasureString(ylab, labelFont)
                        Dim labPos As PointF

                        ' Y 坐标轴
                        Call g.DrawLine(axisPen, .Location, New Point(.Left, .Bottom))

                        Select Case yAxislabelPosition
                            Case YlabelPosition.InsidePlot
                                labPos = New Point(.Left + 3, .Top)
                                Call g.DrawString(ylab, labelFont, Brushes.Black, labPos)
                            Case YlabelPosition.LeftCenter
                                labPos = New PointF(
                                    (.Left - labSize.Height) / 4,
                                    .Top * 2.5 + (.Height - labSize.Width) / 2)

                                With New GraphicsText(DirectCast(g, Graphics2D).Graphics)
                                    Call .DrawString(ylab, labelFont, Brushes.Black, labPos, -90)
                                End With
                            Case Else
                                ' 不进行标签的绘制
                        End Select

                        ' X 坐标轴
                        Dim fWidth! = g.MeasureString(xlab, labelFont).Width
                        Call g.DrawLine(axisPen, New Point(.Left, ymid), New Point(.Right, ymid))
                        Call g.DrawString(xlab, labelFont, Brushes.Black, New Point(.Right - fWidth, ymid + 2))

                        Dim left!
                        'Dim ba As New SolidBrush(cla.TranslateColor)
                        'Dim bb As New SolidBrush(clb.TranslateColor)
                        Dim xCSSFont As Font = CSSFont.TryParse(X_CSS).GDIObject
                        Dim xsz As SizeF
                        Dim xpos As PointF
                        Dim xlabel$
                        Dim highlightPen As Pen = Stroke.TryParse(highlight).GDIObject
#Region "绘制柱状图"
                        For Each part In query
                            Dim ba As New SolidBrush(part.Color.TranslateColor)

                            For Each o As (x#, value#) In part.signals
                                y = o.value
                                y = ymid - yscale(y)
                                left = region.Padding.Left + xscale(o.x)
                                rect = New Rectangle(New Point(left, y), New Size(bw, yscale(o.value)))
                                g.FillRectangle(ba, rect)
                            Next
                        Next

                        For Each part In subject
                            Dim bb As New SolidBrush(part.Color.TranslateColor)

                            For Each o As (x#, value#) In part.signals
                                y = o.value
                                y = ymid + yscale(y)
                                left = region.Padding.Left + xscale(o.x)
                                rect = Rectangle(ymid, left, left + bw, y)
                                g.FillRectangle(bb, rect)
                            Next
                        Next

                        ' 绘制高亮的区域
                        Dim highlights = HighlightGroups(query, subject, hitsHightLights, xError)
                        Dim right!
                        Dim blockHeight!

                        For Each block As (xmin#, xmax#, query#, subject#) In highlights
                            left = region.Padding.Left + xscale(block.xmin)
                            right = region.Padding.Left + xscale(block.xmax) + bw
                            y = ymid - yscale(block.query)
                            blockHeight = yscale(block.query) + yscale(block.subject)

                            rect = New Rectangle(
                                New Point(left - highlightMargin, y - highlightMargin),
                                New Size(right - left + 2 * highlightMargin, blockHeight + 2 * highlightMargin))

                            g.DrawRectangle(highlightPen, rect)
                        Next
#End Region
                        ' 考虑到x轴标签可能会被柱子挡住，所以在这里将柱子和x标签的绘制分开在两个循环之中来完成
#Region "绘制横坐标轴"
                        For Each part In query
                            For Each o As (x#, value#) In part.signals
                                y = o.value
                                y = ymid - yscale(y)
                                left = region.Padding.Left + xscale(o.x)
                                rect = New Rectangle(New Point(left, y), New Size(bw, yscale(o.value)))

                                If displayX AndAlso o.value / yLength >= labelPlotStrength Then
                                    xlabel = o.x.ToString("F2")
                                    xsz = g.MeasureString(xlabel, xCSSFont)
                                    xpos = New PointF(rect.Left + (rect.Width - xsz.Width) / 2, rect.Top - xsz.Height)
                                    g.DrawString(xlabel, xCSSFont, Brushes.Black, xpos)
                                End If
                            Next
                        Next

                        For Each part In subject
                            For Each o As (x#, value#) In part.signals
                                y = o.value
                                y = ymid + yscale(y)
                                left = region.Padding.Left + xscale(o.x)
                                rect = Rectangle(ymid, left, left + bw, y)

                                If displayX AndAlso o.value / yLength >= labelPlotStrength Then
                                    xlabel = o.x.ToString("F2")
                                    xsz = g.MeasureString(xlabel, xCSSFont)
                                    xpos = New PointF(rect.Left + (rect.Width - xsz.Width) / 2, rect.Bottom + 3)
                                    g.DrawString(xlabel, xCSSFont, Brushes.Black, xpos)
                                End If
                            Next
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
                        Call g.FillRectangle(query.Last.Color.GetBrush, box)
                        Call g.DrawString(queryName, legendFont, Brushes.Black, box.Location.OffSet2D(25, -y))

                        box = New Rectangle(New Point(box.Left, box.Top + 30), box.Size)
                        Call g.FillRectangle(subject.Last.Color.GetBrush, box)
                        Call g.DrawString(subjectName, legendFont, Brushes.Black, box.Location.OffSet2D(25, -y))

                        Dim titleFont As Font = CSSFont _
                            .TryParse(titleCSS, [default]:=New Font(FontFace.MicrosoftYaHei, 16.0!)) _
                            .GDIObject
                        Dim titleSize As SizeF = g.MeasureString(title, titleFont)
                        Dim tl As New Point With {
                            .X = rect.Left + (rect.Width - titleSize.Width) / 2,
                            .Y = (region.Padding.Top - titleSize.Height) / 2
                        }

                        Call g.DrawString(title, titleFont, Brushes.Black, tl)

                        If Not idTag Is Nothing Then
                            ' 绘制右下角的编号标签
                            titleSize = g.MeasureString(idTag, titleFont)
                            tl = New Point With {
                                .X = rect.Right - titleSize.Width - 20,
                                .Y = rect.Bottom - titleSize.Height - 20
                            }

                            Call g.DrawString(idTag, titleFont, Brushes.Gray, tl)
                        End If
                    End With
                End Sub

            Return g.GraphicsPlots(
                size.SizeParser, padding,
                "white",
                plotInternal)
        End Function

        Private Function HighlightGroups(query As Signal(), subject As Signal(), highlights#(), err#) As (xmin#, xmax#, query#, subject#)()
            If highlights.IsNullOrEmpty Then
                Return {}
            End If

            Dim isHighlight = highlights.Hit(err)
            Dim qh = query.__createHits(isHighlight)
            Dim sh = subject.__createHits(isHighlight)
            Dim out As New List(Of (xmin#, xmax#, query#, subject#))

            For Each x In highlights
                If Not qh.ContainsKey(x) OrElse Not sh.ContainsKey(x) Then
                    Continue For
                End If

                Dim q = qh(x)
                Dim s = sh(x)

                With q.x + s.x.ToArray
                    out += (.Min, .Max, q.y, s.y)
                End With
            Next

            Return out
        End Function

        <Extension>
        Private Function __createHits(data As Signal(), ishighlight As Func(Of Double, (err#, x#, yes As Boolean))) As Dictionary(Of Double, (x As List(Of Double), y#))
            Dim hits As New Dictionary(Of Double, (x As List(Of Double), y#))
            Dim source As IEnumerable(Of signals) = data _
                .Select(Function(x) x.signals) _
                .IteratesALL

            For Each o As (x#, y#) In source
                Dim hit = ishighlight(o.x)

                If hit.yes Then
                    If Not hits.ContainsKey(hit.x) Then
                        hits(hit.x) = (New List(Of Double), -100)
                    End If

                    Dim value = hits(hit.x)
                    value.x.Add(o.x)

                    If value.y < o.y Then
                        value = (value.x, o.y)
                    End If

                    hits(hit.x) = value
                End If
            Next

            Return hits
        End Function
    End Module
End Namespace
