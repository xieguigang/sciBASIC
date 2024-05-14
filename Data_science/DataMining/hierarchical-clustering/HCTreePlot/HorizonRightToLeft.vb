#Region "Microsoft.VisualBasic::554478e05ebd81c1c4c93baddc0c38e7, Data_science\DataMining\hierarchical-clustering\HCTreePlot\HorizonRightToLeft.vb"

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

    '   Total Lines: 103
    '    Code Lines: 72
    ' Comment Lines: 13
    '   Blank Lines: 18
    '     File Size: 4.22 KB


    ' Class HorizonRightToLeft
    ' 
    '     Properties: GetColor, labelFont, labelPadding, linkColor, log_base
    '                 log_scale, pointSize, showLeafLabels
    ' 
    '     Sub: (+2 Overloads) DendrogramPlot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports std = System.Math

Public Class HorizonRightToLeft

    Public Property showLeafLabels As Boolean = False
    Public Property linkColor As Pen
    Public Property labelFont As Font
    Public Property labelPadding As Single = 5
    Public Property pointSize As Single = 5
    Public Property log_scale As Boolean = True
    Public Property log_base As Double = 10

    Dim labels As New List(Of NamedValue(Of PointF))

    Public Property GetColor As Func(Of String, Color)

    Public Sub DendrogramPlot(hist As Cluster, g As IGraphics, plotRegion As Rectangle)
        ' 每一个样本点都平分一段长度
        Dim unitWidth As Double = plotRegion.Height / hist.Leafs
        Dim axisTicks As Double()

        If hist.DistanceValue <= 0.1 Then
            axisTicks = {0, hist.DistanceValue}.Range.CreateAxisTicks(decimalDigits:=-1)
            log_scale = False
        Else
            axisTicks = {0, If(log_scale, std.Log(hist.DistanceValue, log_base), hist.DistanceValue)}.Range.CreateAxisTicks
        End If

        Dim charWidth As Integer = g.MeasureString("0", labelFont).Width
        Dim scaleX As d3js.scale.LinearScale = d3js.scale _
            .linear() _
            .domain(values:=axisTicks) _
            .range(integers:={0, plotRegion.Width})

        Call DendrogramPlot(hist, unitWidth, g, plotRegion, 0, scaleX, Nothing, charWidth)
    End Sub

    Private Sub DendrogramPlot(partition As Cluster,
                               unitWidth As Double,
                               g As IGraphics,
                               plotRegion As Rectangle,
                               i As Integer,
                               scaleX As d3js.scale.LinearScale,
                               parentPt As PointF,
                               charWidth As Single)

        Dim orders As Cluster() = partition.Children.OrderBy(Function(a) a.Leafs).ToArray
        Dim x = plotRegion.Left + scaleX(If(log_scale AndAlso partition.DistanceValue > 0, std.Log(partition.DistanceValue, log_base), partition.DistanceValue))
        Dim y As Integer

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

        If showLeafLabels AndAlso partition.isLeaf Then
            Dim lsize As SizeF = g.MeasureString(partition.Name, labelFont)
            Dim lpos As New PointF(x - labelPadding, y - lsize.Height / 2)

            Call g.DrawString(partition.Name, labelFont, Brushes.Black, lpos)
        End If

        If partition.isLeaf Then
            'If showLeafLabels Then
            '    ' 绘制class颜色块
            '    Dim color As New SolidBrush(GetColor(partition.Name))
            '    Dim d As Double = std.Max(charWidth / 2, pointSize)
            '    Dim layout As New Rectangle With {
            '        .Location = New Point(x - d, y - unitWidth / 2),
            '        .Size = New Size(labelPadding - d * 1.25, unitWidth)
            '    }

            '    Call g.FillRectangle(color, layout)
            'End If
        Else
            Dim n As Integer = 0

            parentPt = New PointF(x, y)

            For Each part As Cluster In orders
                DendrogramPlot(part, unitWidth, g, plotRegion, i + n, scaleX, parentPt, charWidth)
                n += part.Leafs
            Next
        End If
    End Sub
End Class

