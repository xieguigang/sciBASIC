#Region "Microsoft.VisualBasic::0b21ffe1969bf59ee5fc8469f3981216, Data_science\DataMining\hierarchical-clustering\HCTreePlot\DendrogramPanelV2.vb"

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

    '   Total Lines: 161
    '    Code Lines: 126 (78.26%)
    ' Comment Lines: 8 (4.97%)
    '    - Xml Docs: 37.50%
    ' 
    '   Blank Lines: 27 (16.77%)
    '     File Size: 7.10 KB


    ' Class DendrogramPanelV2
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Paint
    ' 
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
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports std = System.Math

''' <summary>
''' 绘制层次聚类图(竖直方向)
''' </summary>
Public Class DendrogramPanelV2 : Inherits DendrogramPanel

    Protected labels As New List(Of NamedValue(Of PointF))

    Public Sub New(hist As Cluster, theme As Theme,
                   Optional classes As ColorClass() = Nothing,
                   Optional classinfo As Dictionary(Of String, String) = Nothing,
                   Optional showAllLabels As Boolean = False,
                   Optional showAllNodes As Boolean = False,
                   Optional pointColor$ = "red",
                   Optional showRuler As Boolean = True,
                   Optional showLeafLabels As Boolean = True)

        MyBase.New(hist, theme, classes, classinfo, showAllLabels, showAllNodes, pointColor, showLeafLabels, showRuler)
    End Sub

    Public Function Paint(g As IGraphics, layout As Rectangle) As IEnumerable(Of NamedValue(Of PointF))
        Call labels.Clear()
        Call PlotInternal(g, EvaluateLayout(g, layout))

        Return labels
    End Function

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Dim plotRegion As Rectangle = canvas.PlotRegion
        ' 每一个样本点都平分一段长度
        Dim unitWidth As Double = plotRegion.Height / hist.Leafs
        Dim axisTicks As Double()
        Dim css As CSSEnvirnment = g.LoadEnvironment

        If hist.DistanceValue <= 0.1 Then
            axisTicks = {0, hist.DistanceValue}.Range.CreateAxisTicks(decimalDigits:=-1)
        Else
            axisTicks = {0, hist.DistanceValue}.Range.CreateAxisTicks
        End If

        labelFont = css.GetFont(CSSFont.TryParse(theme.tagCSS))

        Dim scaleX As d3js.scale.LinearScale = d3js.scale _
            .linear() _
            .domain(values:=axisTicks) _
            .range(integers:={plotRegion.Left, plotRegion.Right})

        ' 绘制距离标尺
        Dim left = plotRegion.Left + plotRegion.Right - scaleX(axisTicks.Max)
        Dim right = plotRegion.Left + plotRegion.Right - scaleX(0)
        Dim y = plotRegion.Top + unitWidth - unitWidth / 2
        Dim x!
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
            Call g.DrawLine(axisPen, New PointF(left, y), New PointF(right, y))

            For Each tick As Double In axisTicks
                x = plotRegion.Left + plotRegion.Right - scaleX(tick)
                tickLable = tick.ToString(theme.XaxisTickFormat)
                tickLabelSize = g.MeasureString(tickLable, tickFont)

                g.DrawLine(axisPen, New PointF(x, y), New PointF(x, y - dh))
                g.DrawString(tickLable, tickFont, Brushes.Black, New PointF(x - tickLabelSize.Width / 2, y - dh - tickFontHeight))
            Next
        End If

        Call DendrogramPlot(hist, unitWidth, g, plotRegion, 0, scaleX, Nothing, labelPadding, charWidth)
    End Sub

    Protected Overridable Overloads Sub DendrogramPlot(partition As Cluster,
                                                       unitWidth As Double,
                                                       g As IGraphics,
                                                       plotRegion As Rectangle,
                                                       i As Integer,
                                                       scaleX As d3js.scale.LinearScale,
                                                       parentPt As PointF,
                                                       labelPadding As Integer,
                                                       charWidth As Integer)

        Dim orders As Cluster() = partition.Children.OrderBy(Function(a) a.Leafs).ToArray
        Dim x = plotRegion.Left + plotRegion.Right - scaleX(partition.DistanceValue)
        Dim y As Integer
        Dim css As CSSEnvirnment = g.LoadEnvironment
        Dim linkColor As Pen = css.GetPen(Me.linkColor)

        If partition.isLeaf Then
            y = plotRegion.Top + i * unitWidth + unitWidth
            labels += New NamedValue(Of PointF) With {
                .Name = partition.Name,
                .Value = New PointF(x, y)
            }
        Else
            ' 连接节点在中间？
            y = plotRegion.Top + (i + 0.5) * unitWidth + (partition.Leafs * unitWidth) / 2
        End If

        If Not parentPt.IsEmpty Then
            ' 绘制连接线
            Call g.DrawLine(linkColor, parentPt, New PointF(parentPt.X, y))
            Call g.DrawLine(linkColor, New PointF(x, y), New PointF(parentPt.X, y))
        End If

        If (partition.isLeaf OrElse showAllNodes) AndAlso theme.pointSize > 0 Then
            Call g.DrawCircle(New PointF(x, y), theme.pointSize, pointColor)
        End If

        If showLeafLabels AndAlso (partition.isLeaf OrElse showAllLabels) Then
            Dim lsize As SizeF = g.MeasureString(partition.Name, labelFont)
            Dim lpos As New PointF(x + labelPadding, y - lsize.Height / 2)

            Call g.DrawString(partition.Name, labelFont, Brushes.Black, lpos)
        End If

        If partition.isLeaf Then
            If showLeafLabels Then
                ' 绘制class颜色块
                Dim color As New SolidBrush(GetColor(partition.Name))
                Dim d As Double = std.Max(charWidth / 2, theme.pointSize)
                Dim layout As New Rectangle With {
                    .Location = New Point(x + d, y - unitWidth / 2),
                    .Size = New Size(labelPadding - d * 1.25, unitWidth)
                }

                Call g.FillRectangle(color, layout)
            End If
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
