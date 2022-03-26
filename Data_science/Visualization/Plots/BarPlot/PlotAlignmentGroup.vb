Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports FontStyle = System.Drawing.FontStyle
Imports stdNum = System.Math

Namespace BarPlot

    Public Class PlotAlignmentGroup : Inherits Plot

        Dim query As Signal(), subject As Signal()
        Dim xrange As DoubleRange, yrange As DoubleRange
        Dim rectangleStyle As RectangleStyling

        Public Property XAxisLabelCss As String
        Public Property displayX As Boolean
        Public Property queryName As String
        Public Property subjectName As String
        Public Property highlightMargin As Single
        Public Property hitsHightLights As Double()
        Public Property labelPlotStrength As Double
        Public Property idTag As String
        Public Property bw As Single
        Public Property xError As Double

        Public Sub New(query As Signal(),
                       subject As Signal(),
                       xrange As DoubleRange,
                       yrange As DoubleRange,
                       rectangleStyle As RectangleStyling,
                       theme As Theme)

            MyBase.New(theme)

            Me.query = query
            Me.subject = subject
            Me.xrange = xrange
            Me.yrange = yrange
            Me.rectangleStyle = rectangleStyle Or RectangleStyles.DefaultStyle

            If xrange Is Nothing Then
                Dim ALL = query _
                    .Select(Function(x) x.signals.Keys) _
                    .JoinIterates(subject.Select(Function(x) x.signals.Keys)) _
                    .IteratesALL _
                    .ToArray
                Me.xrange = New DoubleRange(ALL)
            End If
            If yrange Is Nothing Then
                Dim ALL = query _
                    .Select(Function(x) x.signals.Values) _
                    .JoinIterates(subject.Select(Function(x) x.signals.Values)) _
                    .IteratesALL _
                    .ToArray
                Me.yrange = New DoubleRange(ALL)
            End If
        End Sub

        Private Sub DrawAlignmentBars(g As IGraphics,
                                      canvas As GraphicsRegion,
                                      ymid As Single,
                                      xscale As d3js.scale.LinearScale,
                                      yscale As d3js.scale.LinearScale)
            Dim left As Double
            Dim y As Double
            Dim position As Point
            Dim sz As Size
            Dim rect As Rectangle
            Dim highlightPen As Pen = Stroke.TryParse(theme.lineStroke).GDIObject
            Dim paddingTop = canvas.Padding.Top
            Dim paddingBottom = canvas.Padding.Bottom
            Dim height As Double

            ' 上半部分的蓝色条
            For Each part As Signal In query
                Dim ba As New SolidBrush(part.Color.TranslateColor)

                For Each o As (x#, value#) In part.signals _
                    .Where(Function(f)
                               Return f.Item2 <> 0R
                           End Function)

                    left = xscale(o.x)
                    height = yscale(o.value)
                    y = ymid - height
                    position = New Point(left, y)
                    sz = New Size(bw, height)
                    rect = New Rectangle With {
                        .Location = position,
                        .Size = sz
                    }

                    ' Call g.FillRectangle(ba, rect)
                    Call rectangleStyle(g, ba, rect, RectangleSides.Bottom)
                Next
            Next

            ' 下半部分的棕色条
            For Each part As Signal In subject
                Dim bb As New SolidBrush(part.Color.TranslateColor)

                For Each o As (x#, value#) In part.signals _
                            .Where(Function(f)
                                       Return f.Item2 <> 0R
                                   End Function)

                    Dim scaleY = yscale(o.value)

                    y = ymid + scaleY
                    left = xscale(o.x)

                    If canvas.device.driverUsed = Drivers.PDF Then
                        rect = New Rectangle With {
                            .X = left,
                            .Y = y - ymid + paddingTop + paddingBottom,
                            .Width = bw,
                            .Height = scaleY - ymid + paddingTop
                        }
                    Else
                        rect = Rectangle(ymid, left, left + bw, y)
                    End If

                    ' g.FillRectangle(bb, rect)
                    Call rectangleStyle(g, bb, rect, RectangleSides.Top)
                Next
            Next

            ' 绘制高亮的区域
            Dim highlights = HighlightGroups(query, subject, hitsHightLights, xError)
            Dim right!
            Dim blockHeight!

            For Each block As (xmin#, xmax#, query#, subject#) In highlights
                left = xscale(block.xmin)
                right = xscale(block.xmax) + bw
                y = ymid - yscale(block.query)
                blockHeight = yscale(block.query) + yscale(block.subject)

                rect = New Rectangle With {
                            .Location = New Point(left - highlightMargin, y - highlightMargin),
                            .Size = New Size With {
                                .Width = right - left + 2 * highlightMargin,
                                .Height = blockHeight + 2 * highlightMargin
                            }
                        }

                g.DrawRectangle(highlightPen, rect)
            Next
        End Sub

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            Dim rect As Rectangle = canvas.PlotRegion
            Dim scaleX = d3js.scale.linear().domain(values:={xrange.Min, xrange.Max}).range(values:={rect.Left, rect.Right})
            Dim scaleY = d3js.scale.linear().domain(values:={0, yrange.Max}).range(values:={0, rect.Height / 2})
            Dim ymid! = rect.Height / 2 + canvas.Padding.Top

            With rect
                Dim axisPen As New Pen(Color.Black, 2)
                Dim dy = yrange.Length / 5
                Dim y!
                Dim gridPen As New Pen(Color.FromArgb(230, 230, 230), 2) 'With {
                '.DashStyle = DashStyle.Solid,
                '    .DashPattern = {15.0!, 4.0!}
                '}
                Dim dt! = 15
                Dim tickPen As New Pen(Color.Black, 1)
                Dim tickFont As Font = CSSFont.TryParse(theme.axisTickCSS, [default]:=New Font(FontFace.MicrosoftYaHei, 12.0!)).GDIObject(g.Dpi)
                Dim drawlabel = Sub(c As IGraphics, label$)
                                    Dim tsize = c.MeasureString(label, tickFont)
                                    Dim pos As New Point(.Left - dt - tsize.Width, y - tsize.Height / 2)

                                    Call c.DrawString(label, tickFont, Brushes.Black, pos)
                                End Sub

                If TypeOf g Is Graphics2D Then
                    DirectCast(g, Graphics2D).Stroke = tickPen
                End If

                For i As Integer = 0 To 5
                    Dim label$ = (i * dy).ToString(theme.YaxisTickFormat) & "%"

                    y = ymid - scaleY(i * dy) ' 上半部分
                    Call g.DrawLine(tickPen, New PointF(.Left, y), New Point(.Left - dt, y))

                    If theme.drawGrid Then
                        Call g.DrawLine(gridPen, New Point(.Left, y), New Point(.Right, y))
                    End If

                    Call drawlabel(g, label)

                    If i = 0 Then
                        Continue For
                    End If

                    y = ymid + scaleY(i * dy) ' 下半部分
                    Call g.DrawLine(tickPen, New PointF(.Left, y), New Point(.Left - dt, y))

                    If theme.drawGrid Then
                        Call g.DrawLine(gridPen, New Point(.Left, y), New Point(.Right, y))
                    End If

                    Call drawlabel(g, label)
                Next

                Dim labelFont As Font = CSSFont.TryParse(theme.axisLabelCSS, [default]:=New Font(FontFace.MicrosoftYaHei, 12.0!, FontStyle.Bold)).GDIObject(g.Dpi)
                Dim labSize As SizeF = g.MeasureString(Me.ylabel, labelFont)
                Dim labPos As PointF

                ' Y 坐标轴
                Call g.DrawLine(axisPen, .Location, New Point(.Left, .Bottom))

                Select Case theme.yAxislabelPosition
                    Case YlabelPosition.InsidePlot
                        labPos = New Point(.Left + 3, .Top)
                        Call g.DrawString(ylabel, labelFont, Brushes.Black, labPos)
                    Case YlabelPosition.LeftCenter
                        If TypeOf g Is Graphics2D Then
                            Dim lx = (.Left - labSize.Height) / 4
                            Dim ly = .Top * 2.5 + (.Height - labSize.Width) / 2

                            labPos = New PointF(lx, ly)

                            With New GraphicsText(DirectCast(g, Graphics2D).Graphics)
                                Call .DrawString(ylabel, labelFont, Brushes.Black, labPos, -90)
                            End With
                        Else
                            ' 20220324 pdf设备还没有找到办法兼容这个操作
                            ' 所以在这里正常绘制，不做角度旋转
                            Call g.DrawString(ylabel, labelFont, Brushes.Black, labPos.X, labPos.Y)
                        End If
                    Case Else
                        ' 不进行标签的绘制
                End Select

                ' X 坐标轴
                Dim fWidth! = g.MeasureString(Me.xlabel, labelFont).Width

                Call g.DrawLine(axisPen, New Point(.Left, ymid), New Point(.Right, ymid))
                Call g.DrawString(Me.xlabel, labelFont, Brushes.Black, New Point(.Right - fWidth, ymid + 2))

                Dim left!
                Dim xCSSFont As Font = CSSFont.TryParse(XAxisLabelCss).GDIObject(g.Dpi)
                Dim xsz As SizeF
                Dim xpos As PointF
                Dim xlabel$

#Region "绘制柱状图"
                Call DrawAlignmentBars(g, canvas, ymid, scaleX, scaleY)
#End Region
                ' 考虑到x轴标签可能会被柱子挡住，所以在这里将柱子和x标签的绘制分开在两个循环之中来完成
#Region "绘制横坐标轴"
                For Each part As Signal In query
                    For Each o As (x#, value#) In part.signals
                        y = o.value
                        y = ymid - scaleY(y)
                        left = scaleX(o.x)
                        rect = New Rectangle(New Point(left, y), New Size(bw, scaleY(o.value)))

                        If displayX AndAlso o.value / yrange.Max >= labelPlotStrength Then
                            xlabel = o.x.ToString(theme.tagFormat)
                            xsz = g.MeasureString(xlabel, xCSSFont)
                            xpos = New PointF(rect.Left + (rect.Width - xsz.Width) / 2, rect.Top - xsz.Height)
                            g.DrawString(xlabel, xCSSFont, Brushes.Black, xpos)
                        End If
                    Next
                Next

                For Each part As Signal In subject
                    For Each o As (x#, value#) In part.signals
                        y = o.value
                        y = ymid + scaleY(y)
                        left = scaleX(o.x)
                        rect = Rectangle(ymid, left, left + bw, y)

                        If displayX AndAlso o.value / yrange.Max >= labelPlotStrength Then
                            xlabel = o.x.ToString(theme.tagFormat)
                            xsz = g.MeasureString(xlabel, xCSSFont)
                            xpos = New PointF(rect.Left + (rect.Width - xsz.Width) / 2, rect.Bottom + 3)
                            g.DrawString(xlabel, xCSSFont, Brushes.Black, xpos)
                        End If
                    Next
                Next
#End Region
                rect = canvas.PlotRegion

                If theme.drawLegend Then
                    Call DrawLegeneds(g, rect)
                End If

                Call DrawMainTitle(g, canvas.PlotRegion, offsetFactor:=1.5)

                If Not idTag Is Nothing Then
                    Dim titleFont As Font = CSSFont _
                            .TryParse(theme.mainCSS, [default]:=New Font(FontFace.MicrosoftYaHei, 16.0!)) _
                            .GDIObject(g.Dpi)

                    ' 绘制右下角的编号标签
                    Dim titleSize = g.MeasureString(idTag, titleFont)
                    Dim tl As New Point With {
                        .X = rect.Right - titleSize.Width - 20,
                        .Y = rect.Bottom - titleSize.Height - 20
                    }

                    Call g.DrawString(idTag, titleFont, Brushes.Gray, tl)
                End If
            End With
        End Sub

        Private Sub DrawLegeneds(g As IGraphics, rect As Rectangle)
            Dim boxWidth! = 350
            Dim y As Double

            ' legend 的圆角矩形
            Call Shapes.RoundRect.Draw(
                        g,
                        New Point(rect.Right - (boxWidth + 10), rect.Top + 6),
                        New Size(boxWidth, 80), 8,
                        Brushes.White,
                        New Stroke With {
                            .dash = DashStyle.Solid,
                            .fill = "black",
                            .width = 2
                        })

            Dim box As Rectangle
            Dim legendFont As Font = CSSFont _
                        .TryParse(theme.legendLabelCSS, [default]:=New Font(FontFace.MicrosoftYaHei, 16.0!)) _
                        .GDIObject(g.Dpi)
            Dim fHeight! = g.MeasureString("1", legendFont).Height

            y = 3

            box = New Rectangle(New Point(rect.Right - boxWidth, rect.Top + 20), New Size(20, 20))
            Call g.FillRectangle(query.Last.Color.GetBrush, box)
            Call g.DrawString(queryName, legendFont, Brushes.Black, box.Location.OffSet2D(25, -y))

            box = New Rectangle(New Point(box.Left, box.Top + 30), box.Size)
            Call g.FillRectangle(subject.Last.Color.GetBrush, box)
            Call g.DrawString(subjectName, legendFont, Brushes.Black, box.Location.OffSet2D(25, -y))
        End Sub

        Private Function HighlightGroups(query As Signal(), subject As Signal(), highlights#(), err#) As (xmin#, xmax#, query#, subject#)()
            If highlights.IsNullOrEmpty Then
                Return {}
            End If

            Dim isHighlight = Hit(highlights, err)
            Dim qh = query.createHits(isHighlight)
            Dim sh = subject.createHits(isHighlight)
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

        Private Shared Function Hit(highlights#(), err#) As Func(Of Double, (err#, X#, yes As Boolean))
            If highlights.IsNullOrEmpty Then
                Return Function() (-1, -1, False)
            Else
                Return Function(x)
                           Dim e#

                           For Each n In highlights
                               e = stdNum.Abs(n - x)

                               If e <= err Then
                                   Return (e, n, True)
                               End If
                           Next

                           Return (-1, -1, False)
                       End Function
            End If
        End Function
    End Class
End Namespace