#Region "Microsoft.VisualBasic::83b4d43249766aa52ed6d9a724447399, Data_science\DataMining\hierarchical-clustering\HCTreePlot\Circular.vb"

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

    '   Total Lines: 141
    '    Code Lines: 112 (79.43%)
    ' Comment Lines: 5 (3.55%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 24 (17.02%)
    '     File Size: 6.16 KB


    ' Class Circular
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: DendrogramPlot, PlotInternal
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports std = System.Math

Public Class Circular : Inherits DendrogramPanel

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

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Dim plotRegion = canvas.PlotRegion
        Dim maxRadius As Double = std.Min(plotRegion.Width, plotRegion.Height) / 2
        ' 每一个样本点都平分一段长度
        Dim unitAngle As Double = (2 * std.PI) / hist.Leafs
        Dim axisTicks As Double()
        Dim center As New PointF(plotRegion.Left + plotRegion.Width / 2, plotRegion.Top + plotRegion.Height / 2)

        If hist.DistanceValue <= 0.1 Then
            axisTicks = {0, hist.DistanceValue}.Range.CreateAxisTicks(decimalDigits:=-1)
        Else
            axisTicks = {0, hist.DistanceValue}.Range.CreateAxisTicks
        End If

        Dim scaleR As d3js.scale.LinearScale = d3js.scale _
            .linear() _
            .domain(values:=axisTicks) _
            .range(integers:={0, maxRadius})
        Dim css As CSSEnvirnment = g.LoadEnvironment

        ' 绘制距离标尺
        Dim outer = scaleR(axisTicks.Max)
        Dim inner = scaleR(0)
        Dim tickFont As Font = css.GetFont(CSSFont.TryParse(theme.axisTickCSS))
        Dim tickFontHeight As Single = g.MeasureString("0", tickFont).Height
        Dim dh As Double = tickFontHeight / 3
        Dim tickLable As String
        Dim tickLabelSize As SizeF
        Dim labelPadding As Integer
        Dim charWidth As Integer = g.MeasureString("0", labelFont).Width
        Dim axisPen As Pen = Stroke.TryParse(theme.axisStroke)
        Dim angle As Double = 0
        Dim r As Double

        If classinfo.IsNullOrEmpty Then
            labelPadding = g.MeasureString("0", labelFont).Width / 2
        Else
            labelPadding = g.MeasureString("00", labelFont).Width
        End If

        If showRuler Then
            Call g.DrawLine(axisPen, New PointF(outer, angle), New PointF(inner, angle))

            For Each tick As Double In axisTicks
                r = scaleR(tick)

                tickLable = tick.ToString(theme.XaxisTickFormat)
                tickLabelSize = g.MeasureString(tickLable, tickFont)

                g.DrawLine(axisPen, New PolarPoint(r, angle).Point, New PolarPoint(r, angle).Point)
                g.DrawString(tickLable, tickFont, Brushes.Black, New PolarPoint(r, angle).Point)
            Next
        End If

        Call DendrogramPlot(hist, unitAngle, g, plotRegion, 0, scaleR, Nothing, labelPadding, charWidth, center)
    End Sub

    Private Overloads Sub DendrogramPlot(partition As Cluster,
                                         unitAngle As Double,
                                         g As IGraphics,
                                         plotRegion As Rectangle,
                                         i As Integer,
                                         scaleX As d3js.scale.LinearScale,
                                         parentPt As PointF,
                                         labelPadding As Integer,
                                         charWidth As Integer,
                                         center As PointF)

        Dim orders As Cluster() = partition.Children.OrderBy(Function(a) a.Leafs).ToArray
        Dim x = plotRegion.Left + plotRegion.Right - scaleX(partition.DistanceValue)
        Dim angle As Integer

        If partition.isLeaf Then
            angle = i * unitAngle + unitAngle
        Else
            ' 连接节点在中间？
            angle = (i + 0.5) * unitAngle + (partition.Leafs * unitAngle) / 2
        End If

        If Not parentPt.IsEmpty Then
            ' 绘制连接线
            Call g.DrawLine(linkColor, parentPt, New PointF(parentPt.X, angle))
            Call g.DrawLine(linkColor, New PointF(x, angle), New PointF(parentPt.X, angle))
        End If

        If partition.isLeaf OrElse showAllNodes Then
            Call g.DrawCircle(New PointF(x, angle), theme.pointSize, pointColor)
        End If

        If partition.isLeaf OrElse showAllLabels Then
            Dim lsize As SizeF = g.MeasureString(partition.Name, labelFont)
            Dim lpos As New PointF(x + labelPadding, angle - lsize.Height / 2)

            Call g.DrawString(partition.Name, labelFont, Brushes.Black, lpos)
        End If

        If partition.isLeaf Then
            ' 绘制class颜色块
            Dim color As New SolidBrush(GetColor(partition.Name))
            Dim d As Double = std.Max(charWidth / 2, theme.pointSize)
            Dim layout As New Rectangle With {
                .Location = New Point(x + d, angle - unitAngle / 2),
                .Size = New Size(labelPadding - d * 1.25, unitAngle)
            }

            Call g.FillRectangle(color, layout)
        Else
            Dim n As Integer = 0

            parentPt = New PointF(x, angle)

            For Each part As Cluster In orders
                DendrogramPlot(part, unitAngle, g, plotRegion, i + n, scaleX, parentPt, labelPadding, charWidth, center)
                n += part.Leafs
            Next
        End If
    End Sub
End Class
