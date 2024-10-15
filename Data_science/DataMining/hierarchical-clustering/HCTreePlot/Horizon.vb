#Region "Microsoft.VisualBasic::cfae2ab7e36972df9d5d0776aceb5771, Data_science\DataMining\hierarchical-clustering\HCTreePlot\Horizon.vb"

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
'    Code Lines: 118 (80.27%)
' Comment Lines: 5 (3.40%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 24 (16.33%)
'     File Size: 6.60 KB


' Class Horizon
' 
'     Constructor: (+1 Overloads) Sub New
'     Sub: DendrogramPlot, PlotInternal
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.scale
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports std = System.Math

Public Class Horizon : Inherits DendrogramPanelV2

    Public Sub New(hist As Cluster, theme As Theme,
                   Optional classes() As ColorClass = Nothing,
                   Optional classinfo As Dictionary(Of String, String) = Nothing,
                   Optional showAllLabels As Boolean = False,
                   Optional showAllNodes As Boolean = False,
                   Optional pointColor As String = "red",
                   Optional showRuler As Boolean = True,
                   Optional showLeafLabels As Boolean = True)

        MyBase.New(hist, theme, classes, classinfo, showAllLabels, showAllNodes, pointColor, showRuler, showLeafLabels)
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Dim css As CSSEnvirnment = g.LoadEnvironment
        Dim plotRegion As Rectangle = canvas.PlotRegion(css)
        ' 每一个样本点都平分一段长度
        Dim unitWidth As Double = plotRegion.Width / hist.Leafs
        Dim axisTicks As Double()

        If hist.DistanceValue <= 0.1 Then
            axisTicks = {0, hist.DistanceValue}.Range.CreateAxisTicks(decimalDigits:=-1)
        Else
            axisTicks = {0, hist.DistanceValue}.Range.CreateAxisTicks
        End If

        Me.labelFont = css.GetFont(CSSFont.TryParse(theme.tagCSS))

        Dim scaleY As d3js.scale.LinearScale = d3js.scale _
            .linear() _
            .domain(values:=axisTicks) _
            .range(integers:={plotRegion.Top, plotRegion.Bottom})

        ' 绘制距离标尺
        Dim top = plotRegion.Top + plotRegion.Bottom - scaleY(axisTicks.Max)
        Dim bottom = plotRegion.Top + plotRegion.Bottom - scaleY(0)
        Dim x = plotRegion.Left + unitWidth - unitWidth / 2
        Dim y!
        Dim tickFont As Font = css.GetFont(CSSFont.TryParse(theme.axisTickCSS))
        Dim tickFontHeight As Single = g.MeasureString("0", tickFont).Height
        Dim dh As Double = tickFontHeight / 3
        Dim tickLable As String
        Dim tickLabelSize As SizeF
        Dim labelPadding As Integer
        Dim charWidth As Integer = g.MeasureString("0", labelFont).Width
        Dim axisPen As Pen = css.GetPen(Stroke.TryParse(theme.axisStroke))

        If classinfo.IsNullOrEmpty Then
            labelPadding = g.MeasureString("0", labelFont).Width / 2
        Else
            labelPadding = g.MeasureString("00", labelFont).Width
        End If

        If showRuler Then
            Call g.DrawLine(axisPen, New PointF(x, top), New PointF(x, bottom))

            For Each tick As Double In axisTicks
                y = plotRegion.Top + plotRegion.Bottom - scaleY(tick)
                tickLable = tick.ToString(theme.XaxisTickFormat)
                tickLabelSize = g.MeasureString(tickLable, tickFont)

                g.DrawLine(axisPen, New PointF(x, y), New PointF(x - dh, y))
                g.DrawString(tickLable, tickFont, Brushes.Black, New PointF(x - dh - tickLabelSize.Width, y - tickFontHeight / 2))
            Next
        End If

        Call DendrogramPlot(hist, unitWidth, g, plotRegion, 0, scaleY, Nothing, labelPadding, charWidth)
    End Sub

    Protected Overrides Sub DendrogramPlot(partition As Cluster,
                                           unitWidth As Double,
                                           g As IGraphics,
                                           plotRegion As Rectangle,
                                           i As Integer,
                                           scaleX As LinearScale,
                                           parentPt As PointF,
                                           labelPadding As Integer,
                                           charWidth As Integer)

        Dim orders As Cluster() = partition.Children.OrderBy(Function(a) a.Leafs).ToArray
        Dim y = plotRegion.Top + plotRegion.Bottom - scaleX(partition.DistanceValue)
        Dim x As Integer
        Dim css As CSSEnvirnment = g.LoadEnvironment
        Dim linkColor As Pen = css.GetPen(Me.linkColor)

        If partition.isLeaf Then
            x = plotRegion.Left + i * unitWidth + unitWidth
            labels += New NamedValue(Of PointF) With {
                .Name = partition.Name,
                .Value = New PointF(x, y)
            }
        Else
            ' 连接节点在中间？
            x = plotRegion.Left + (i + 0.5) * unitWidth + (partition.Leafs * unitWidth) / 2
        End If

        If Not parentPt.IsEmpty Then
            ' 绘制连接线
            Call g.DrawLine(linkColor, parentPt, New PointF(x, parentPt.Y))
            Call g.DrawLine(linkColor, New PointF(x, y), New PointF(x, parentPt.Y))
        End If

        If (partition.isLeaf OrElse showAllNodes) AndAlso theme.pointSize > 0 Then
            Call g.DrawCircle(New PointF(x, y), theme.pointSize, pointColor)
        End If

        If showLeafLabels AndAlso (partition.isLeaf OrElse showAllLabels) Then
            Dim lsize As SizeF = g.MeasureString(partition.Name, labelFont)
            Dim lpos As New PointF(x - lsize.Width / 2, y + labelPadding)

            Call g.DrawString(partition.Name, labelFont, Brushes.Black, lpos)
        End If

        If partition.isLeaf Then
            ' 绘制class颜色块
            Dim color As New SolidBrush(GetColor(partition.Name))
            Dim d As Double = std.Max(charWidth / 2, theme.pointSize)
            Dim layout As New Rectangle With {
                .Location = New Point(x - unitWidth / 2, y + d),
                .Size = New Size(unitWidth, labelPadding - d * 1.25)
            }

            Call g.FillRectangle(color, layout)
        Else
            Dim n As Integer = 0

            parentPt = New PointF(x, y)

            For Each part As Cluster In orders
                DendrogramPlot(part, unitWidth, g, plotRegion, i + n, scaleX, parentPt, labelPadding, charWidth)
                n += part.Leafs
            Next
        End If
    End Sub
End Class
