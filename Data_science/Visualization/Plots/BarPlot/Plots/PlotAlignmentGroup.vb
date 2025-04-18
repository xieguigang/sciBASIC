﻿#Region "Microsoft.VisualBasic::8b697b48c58e4e85c1abb95d49c75937, Data_science\Visualization\Plots\BarPlot\Plots\PlotAlignmentGroup.vb"

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

    '   Total Lines: 587
    '    Code Lines: 412 (70.19%)
    ' Comment Lines: 72 (12.27%)
    '    - Xml Docs: 11.11%
    ' 
    '   Blank Lines: 103 (17.55%)
    '     File Size: 25.74 KB


    '     Class PlotAlignmentGroup
    ' 
    '         Properties: bw, displayX, highlightMargin, hitsHightLights, idTag
    '                     labelPlotStrength, legendLayout, queryName, subjectName, XAxisLabelCss
    '                     xError
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Hit
    ' 
    '         Sub: DrawAlignmentBars, DrawLegendTitleRegion, DrawLegendTopRight, DrawLegeneds, DrawTextLabels
    '              PlotInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text.Nudge
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports std = System.Math

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

Namespace BarPlot

    Public Class PlotAlignmentGroup : Inherits Plot

        Dim query As Signal(), subject As Signal()
        Dim xrange As DoubleRange, yrange As DoubleRange
        Dim rectangleStyle As RectangleStyling
        Dim highlightStyle As RectangleStyling

        Public Property XAxisLabelCss As String
        Public Property displayX As Boolean
        Public Property queryName As String
        Public Property subjectName As String
        Public Property highlightMargin As Single
        Public Property hitsHightLights As NamedValue(Of Double)()
        Public Property labelPlotStrength As Double
        Public Property idTag As String
        ''' <summary>
        ''' the width of the spectrum bar
        ''' </summary>
        ''' <returns></returns>
        Public Property bw As Single
        Public Property xError As Double
        Public Property legendLayout As String = "top-right"

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

            Dim highlights = Stroke.TryParse(theme.lineStroke)
            Dim highlightColor As Brush = Nothing

            If highlights IsNot Nothing Then
                highlightColor = highlights.fill.GetBrush
            End If

            Me.highlightStyle =
                Sub(g As IGraphics, color As SolidBrush, layout As Rectangle, unsealSide As RectangleSides)
                    Call g.FillRectangle(highlightColor, layout)
                End Sub

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
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim highlightPen As Pen = css.GetPen(Stroke.TryParse(theme.lineStroke))
            Dim paddingTop = css.GetHeight(canvas.Padding.Top)
            Dim paddingBottom = css.GetHeight(canvas.Padding.Bottom)
            Dim height As Double
            Dim isHighlight = Hit(Me.hitsHightLights, Me.xError)
            Dim hasHighlights = Not hitsHightLights.IsNullOrEmpty
            Dim highlight_scale As Single = 1.5

            ' 上半部分的蓝色条
            For Each part As Signal In query
                Dim ba As New SolidBrush(part.Color.TranslateColor)

                For Each o As (x#, value#) In part.signals _
                    .Where(Function(f)
                               Return f.Item2 <> 0R
                           End Function)

                    Dim makeHighlights = hasHighlights AndAlso isHighlight(o.x).yes
                    Dim bw As Single = Me.bw

                    If makeHighlights Then
                        bw = bw * highlight_scale
                    End If

                    left = xscale(o.x) - bw / 2
                    height = yscale(o.value)
                    y = ymid - height
                    position = New Point(left, y)
                    sz = New Size(bw, height)
                    rect = New Rectangle With {
                        .Location = position,
                        .Size = sz
                    }

                    If makeHighlights Then
                        Call highlightStyle(g, ba, rect, RectangleSides.Bottom)
                    Else
                        Call rectangleStyle(g, ba, rect, RectangleSides.Bottom)
                    End If
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
                    Dim makeHighlights = hasHighlights AndAlso isHighlight(o.x).yes
                    Dim bw As Single = Me.bw

                    If makeHighlights Then
                        bw = bw * highlight_scale
                    End If

                    y = ymid + scaleY
                    left = xscale(o.x) - bw / 2

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

                    If makeHighlights Then
                        Call highlightStyle(g, bb, rect, RectangleSides.Top)
                    Else
                        Call rectangleStyle(g, bb, rect, RectangleSides.Top)
                    End If
                Next
            Next
        End Sub

        Public Const DefaultGridYStroke As String = "stroke: #EBEBEB; stroke-width: 1px; stroke-dash: solid;"
        Public Const DefaultGridXStroke As String = "stroke: lightgray; stroke-width: 1px; stroke-dash: dash;"

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim rect As Rectangle = canvas.PlotRegion(css)
            Dim scaleX = d3js.scale.linear().domain(values:={xrange.Min, xrange.Max}).range(values:={rect.Left, rect.Right})
            Dim scaleY = d3js.scale.linear().domain(values:={0, yrange.Max}).range(values:={0, rect.Height / 2})
            Dim padding As PaddingLayout = PaddingLayout.EvaluateFromCSS(css, canvas.Padding)
            Dim ymid! = rect.Height / 2 + css.GetHeight(padding.Top)

            css.SetBaseStyles(New Font(FontFace.MicrosoftYaHei, 12.0!))

            With rect
                Dim axisPen As New Pen(Color.Black, 2)
                Dim dy = yrange.Length / 5
                Dim y!
                ' #EBEBEB
                Dim gridPen As Pen = css.GetPen(Stroke.TryParse(theme.gridStrokeY))
                Dim dt! = 15
                Dim tickPen As New Pen(Color.Black, 1)
                Dim tickFont As Font = css.GetFont(CSSFont.TryParse(theme.axisTickCSS, ))
                Dim drawlabel = Sub(c As IGraphics, label$)
                                    Dim tsize = c.MeasureString(label, tickFont)
                                    Dim pos As New PointF(.Left - dt - tsize.Width, y - tsize.Height / 2)

                                    Call c.DrawString(label, tickFont, Brushes.Black, pos)
                                End Sub

                g.Stroke = tickPen

                If theme.drawGrid AndAlso Not theme.gridStrokeX.StringEmpty(, True) Then
                    Dim ticks = xrange.CreateAxisTicks
                    Dim top_y As Double = rect.Top
                    Dim bottom_y As Double = rect.Bottom
                    Dim stroke_x As Pen = css.GetPen(Stroke.TryParse(theme.gridStrokeX))

                    For Each tick As Double In ticks.Skip(1)
                        Dim xi As Double = scaleX(tick)
                        Dim top As New PointF(xi, top_y)
                        Dim bottom As New PointF(xi, bottom_y)

                        Call g.DrawLine(stroke_x, top, bottom)
                    Next
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

                css.SetBaseStyles(New Font(FontFace.MicrosoftYaHei, 12.0!, FontStyle.Bold))

                Dim labelFont As Font = css.GetFont(CSSFont.TryParse(theme.axisLabelCSS, ))
                Dim labSize As SizeF = g.MeasureString(Me.ylabel, labelFont)
                Dim labPos As PointF

                ' Y 坐标轴
                Call g.DrawLine(axisPen, .Location, New Point(.Left, .Bottom))

                Select Case theme.yAxislabelPosition
                    Case YlabelPosition.InsidePlot
                        labPos = New Point(.Left + 3, .Top)
                        Call g.DrawString(ylabel, labelFont, Brushes.Black, labPos)
                    Case YlabelPosition.LeftCenter
                        Dim shape_size As SizeF = g.MeasureString(ylabel, labelFont)

                        labPos = New PointF((padding.Left - shape_size.Height) / 2, padding.Top + (canvas.Height - shape_size.Width) / 2)
                        g.DrawString(ylabel, labelFont, Brushes.Black, labPos.X, labPos.Y, -90)
                    Case Else
                        ' 不进行标签的绘制
                End Select

                ' X 坐标轴
                Dim fWidth! = g.MeasureString(Me.xlabel, labelFont).Width

                Call g.DrawLine(axisPen, New Point(.Left, ymid), New Point(.Right, ymid))
                Call g.DrawString(Me.xlabel, labelFont, Brushes.Black, New Point(.Right - fWidth, ymid + 2))

#Region "绘制柱状图"
                Call DrawAlignmentBars(g, canvas, ymid, scaleX, scaleY)
#End Region
                ' 考虑到x轴标签可能会被柱子挡住，所以在这里将柱子和x标签的绘制分开在两个循环之中来完成
#Region "绘制横坐标轴"
                Call DrawTextLabels(g, ymid, scaleX, scaleY)
#End Region
                rect = canvas.PlotRegion(css)

                If theme.drawLegend Then
                    Call DrawLegeneds(g, rect)
                End If

                If Strings.LCase(legendLayout) <> "title" Then
                    Call DrawMainTitle(g, canvas.PlotRegion(css), offsetFactor:=1.5)
                End If

                If Not idTag Is Nothing Then
                    css.SetBaseStyles(New Font(FontFace.MicrosoftYaHei, 16.0!))

                    Dim titleFont As Font = css.GetFont(CSSFont.TryParse(theme.mainCSS, ))

                    ' 绘制右下角的编号标签
                    Dim titleSize = g.MeasureString(idTag, titleFont)
                    Dim tl As New PointF With {
                        .X = rect.Right - titleSize.Width - 20,
                        .Y = rect.Bottom - titleSize.Height - 20
                    }

                    Call g.DrawString(idTag, titleFont, Brushes.Gray, tl)
                End If
            End With
        End Sub

        Private Sub DrawTextLabels(g As IGraphics, ymid As Double,
                                   scaleX As d3js.scale.LinearScale,
                                   scaleY As d3js.scale.LinearScale)

            Dim textCloud As New CloudOfTextRectangle
            Dim text As TextRectangle
            Dim move As Boolean = False
            Dim rect As RectangleF
            Dim y As Double
            Dim left!
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim tag_label As Font = css.GetFont(CSSFont.TryParse(XAxisLabelCss))
            Dim tag_highlights As Font = css.GetFont(CSSFont.TryParse(XAxisLabelCss), scale:=1.125)
            Dim xsz As SizeF
            Dim xpos As PointF
            Dim xlabel$
            Dim round As Integer = 0
            Dim isHighlight = Hit(Me.hitsHightLights, Me.xError)
            Dim hasHighlights = Not hitsHightLights.IsNullOrEmpty

            tag_highlights = New Font(tag_highlights, FontStyle.Bold)

            For Each part As Signal In query
                For Each o As (x#, value#) In part.signals
                    Dim xCSSFont As Font
                    Dim checkHighlight = isHighlight(o.x)

                    y = o.value
                    y = ymid - scaleY(y)
                    left = scaleX(o.x)
                    rect = New RectangleF(New PointF(left, y), New SizeF(bw, scaleY(o.value)))

                    If hasHighlights AndAlso checkHighlight.yes Then
                        xCSSFont = tag_highlights
                    Else
                        xCSSFont = tag_label
                    End If

                    ' Call textCloud.add_label(New TextRectangle("", rect))

                    If displayX AndAlso o.value / yrange.Max >= labelPlotStrength Then
                        xlabel = o.x.ToString(theme.tagFormat)

                        If checkHighlight.yes Then
                            xlabel = xlabel & $" [{checkHighlight.X.Name}]"
                        End If

                        xsz = g.MeasureString(xlabel, xCSSFont)
                        xpos = New PointF(rect.Left + (rect.Width - xsz.Width) / 2, rect.Top - xsz.Height)
                        text = New TextRectangle(xlabel, New RectangleF(xpos, xsz))
                        move = False
                        round = 0

                        'Call textCloud.add_label(text)

                        'Do While textCloud.get_conflicts > 0
                        '    Dim conflict = textCloud.conflicts_with(text)

                        '    If conflict Is Nothing Then
                        '        Dim text_rect As RectangleF = text.rect
                        '        xpos = New PointF(text_rect.Left, text_rect.Top)
                        '        move = True
                        '        Exit Do
                        '    Else
                        '        Call textCloud.remove_label(text)
                        '        nextPos = New PointF(xpos.X, xpos.Y - xsz.Height)
                        '        text = New TextRectangle(xlabel, New RectangleF(nextPos, xsz))
                        '        xpos = nextPos
                        '        Call textCloud.add_label(text)
                        '    End If

                        '    If round > 100 Then
                        '        Exit Do
                        '    Else
                        '        round += 1
                        '    End If
                        'Loop

                        If move Then
                            ' draw connection link
                            'Dim pBar As New Point(left, y)
                            'Dim pText = New Label(text).GetTextAnchor(pBar)

                            'Call g.DrawLine(Pens.Black, pBar, pText)
                        End If

                        g.DrawString(xlabel, xCSSFont, Brushes.Black, xpos)
                    End If
                Next
            Next

            textCloud = New CloudOfTextRectangle

            For Each part As Signal In subject
                For Each o As (x#, value#) In part.signals
                    Dim xCssFont As Font = Nothing
                    Dim checkHighlight = isHighlight(o.x)

                    y = o.value
                    y = ymid + scaleY(y)
                    left = scaleX(o.x)
                    rect = Rectangle(ymid, left, left + bw, y)

                    If hasHighlights AndAlso checkHighlight.yes Then
                        xCssFont = tag_highlights
                    Else
                        xCssFont = tag_label
                    End If

                    ' Call textCloud.add_label(New TextRectangle("", rect))

                    If displayX AndAlso o.value / yrange.Max >= labelPlotStrength Then
                        xlabel = o.x.ToString(theme.tagFormat)

                        If checkHighlight.yes Then
                            xlabel = xlabel & $" [{checkHighlight.X.Name}]"
                        End If

                        xsz = g.MeasureString(xlabel, xCssFont)
                        xpos = New PointF(rect.Left + (rect.Width - xsz.Width) / 2, rect.Bottom + 3)
                        text = New TextRectangle(xlabel, New RectangleF(xpos, xsz))
                        move = False
                        round = 0

                        'Call textCloud.add_label(text)

                        'Do While textCloud.get_conflicts > 0
                        '    Dim conflict = textCloud.conflicts_with(text)

                        '    If conflict Is Nothing Then
                        '        Dim text_rect As RectangleF = text.rect
                        '        xpos = New PointF(text_rect.Left, text_rect.Top)
                        '        move = True
                        '        Exit Do
                        '    Else
                        '        Call textCloud.remove_label(text)
                        '        nextPos = New PointF(xpos.X, xpos.Y + xsz.Height)
                        '        text = New TextRectangle(xlabel, New RectangleF(nextPos, xsz))
                        '        xpos = nextPos
                        '        Call textCloud.add_label(text)
                        '    End If

                        '    If round > 100 Then
                        '        Exit Do
                        '    Else
                        '        round += 1
                        '    End If
                        'Loop

                        If move Then
                            ' draw connection link
                            'Dim pBar As New Point(left, y)
                            'Dim pText = New Label(text).GetTextAnchor(pBar)

                            'Call g.DrawLine(Pens.Black, pBar, pText)
                        End If

                        g.DrawString(xlabel, xCssFont, Brushes.Black, xpos)
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="rect">it is the canvas plot region</param>
        Private Sub DrawLegeneds(g As IGraphics, rect As Rectangle)
            Select Case Strings.LCase(legendLayout)
                Case "top-right" : Call DrawLegendTopRight(g, rect)
                Case "title" : Call DrawLegendTitleRegion(g, rect)
                Case "none"
                    ' do nothing for skip drawing
                Case Else
                    Throw New NotImplementedException(legendLayout)
            End Select
        End Sub

        Private Sub DrawLegendTitleRegion(g As IGraphics, rect As Rectangle)
            Dim box As Rectangle
            Dim css As CSSEnvirnment = g.LoadEnvironment.SetBaseStyles(New Font(FontFace.MicrosoftYaHei, 16.0!))
            Dim legendFont As Font = css.GetFont(CSSFont.TryParse(theme.legendLabelCSS, ))
            ' get height of a single box
            Dim fHeight As Single = g.MeasureString("A", legendFont).Height
            Dim totalHeight As Single = fHeight * 2.5
            Dim offsetY As Single = (rect.Top - totalHeight) / 2 - fHeight / 2

            box = New Rectangle(New Point(rect.Left, offsetY), New Size(20, 20))
            Call g.FillRectangle(query.Last.Color.GetBrush, box)
            Call g.DrawString(queryName, legendFont, Brushes.Black, box.Location.OffSet2D(25, 0))

            box = New Rectangle(New Point(box.Left, box.Bottom + 10), box.Size)
            Call g.FillRectangle(subject.Last.Color.GetBrush, box)
            Call g.DrawString(subjectName, legendFont, Brushes.Black, box.Location.OffSet2D(25, 0))
        End Sub

        Private Sub DrawLegendTopRight(g As IGraphics, rect As Rectangle)
            Dim boxWidth! = 350
            Dim y As Double

            ' legend 的圆角矩形
            Call Shapes.RoundRect.Draw(g,
                New Point(rect.Right - (boxWidth + 10), rect.Top + 6),
                New Size(boxWidth, 80), 8,
                Brushes.White,
                New Stroke With {
                    .dash = DashStyle.Solid,
                    .fill = "black",
                    .width = 2
                })

            Dim css = g.LoadEnvironment.SetBaseStyles(New Font(FontFace.MicrosoftYaHei, 16.0!))
            Dim box As Rectangle
            Dim legendFont As Font = css.GetFont(CSSFont.TryParse(theme.legendLabelCSS, ))

            y = 3

            box = New Rectangle(New Point(rect.Right - boxWidth, rect.Top + 20), New Size(20, 20))
            Call g.FillRectangle(query.Last.Color.GetBrush, box)
            Call g.DrawString(queryName, legendFont, Brushes.Black, box.Location.OffSet2D(25, -y))

            box = New Rectangle(New Point(box.Left, box.Top + 30), box.Size)
            Call g.FillRectangle(subject.Last.Color.GetBrush, box)
            Call g.DrawString(subjectName, legendFont, Brushes.Black, box.Location.OffSet2D(25, -y))
        End Sub

        Private Shared Function Hit(highlights As NamedValue(Of Double)(), err#) As Func(Of Double, (err#, X As NamedValue(Of Double), yes As Boolean))
            If highlights.IsNullOrEmpty Then
                Return Function() (-1, Nothing, False)
            Else
                Return Function(x)
                           Dim e#

                           For Each n As NamedValue(Of Double) In highlights
                               e = std.Abs(n.Value - x)

                               If e <= err Then
                                   Return (e, n, True)
                               End If
                           Next

                           Return (-1, Nothing, False)
                       End Function
            End If
        End Function
    End Class
End Namespace
