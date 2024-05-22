#Region "Microsoft.VisualBasic::7f4d963c03dbeda8c599e5c7d62cb5d9, Data_science\Visualization\Plots\BarPlot\Plots\SimpleBarPlot.vb"

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

    '   Total Lines: 162
    '    Code Lines: 136 (83.95%)
    ' Comment Lines: 5 (3.09%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 21 (12.96%)
    '     File Size: 6.63 KB


    '     Class SimpleBarPlot
    ' 
    '         Properties: angle, stacked, stackReorder
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: PlotInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Data
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace BarPlot

    Public Class SimpleBarPlot : Inherits Plot

        ReadOnly data As BarDataGroup

        Public Property stackReorder As Boolean = True
        Public Property stacked As Boolean = False
        Public Property angle As Double = -45

        Public Sub New(data As BarDataGroup, theme As Theme)
            MyBase.New(theme)

            Me.data = data
        End Sub

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            Dim n As Integer
            Dim yTicks As Double() = data.Samples _
                .Select(Function(s) s.data) _
                .IteratesALL _
                .Range _
                .CreateAxisTicks
            Dim xscale = d3js.scale.ordinal() _
                .domain(tags:=data.Samples.Select(Function(s) s.tag).ToArray) _
                .range(values:=New Double() {
                    canvas.PlotRegion.Left,
                    canvas.PlotRegion.Right
                })
            Dim yscale = d3js.scale.linear() _
                .domain(values:=yTicks) _
                .range(values:=New Double() {
                    canvas.PlotRegion.Top,
                    canvas.PlotRegion.Bottom
                })
            Dim yscaler As New YScaler(reversed:=False) With {
                .region = canvas.PlotRegion,
                .Y = yscale
            }

            Call Axis.DrawAxis(
                g:=g,
                scaler:=New DataScaler With {
                    .AxisTicks = (Nothing, yTicks.AsVector),
                    .region = canvas.PlotRegion,
                    .X = xscale,
                    .Y = yscale
                },
                region:=canvas,
                showGrid:=theme.drawGrid,
                offset:=Nothing,
                xlabel:=xlabel,
                ylabel:=ylabel,
                labelFontStyle:=theme.axisLabelCSS,
                gridFill:=theme.gridFill,
                gridX:=theme.gridStrokeX,
                gridY:=theme.gridStrokeY,
                axisStroke:=theme.axisStroke,
                tickFontStyle:=theme.axisTickCSS,
                XtickFormat:=theme.XaxisTickFormat,
                YtickFormat:=theme.YaxisTickFormat
            )

            If stacked Then
                n = data.Samples.Length
            Else
                n = data.Samples.Sum(Function(x) x.data.Length) - 1
            End If

            Dim bottom As Double = canvas.PlotRegion.Bottom
            Dim barWidth As Double = xscale.binWidth

            For Each sample As SeqValue(Of BarDataSample) In data.Samples.SeqIterator
                Dim x As Double = xscale(sample.value.tag)

                If stacked Then
                    ' 改变Y
                    Dim right As Double = x + barWidth
                    Dim top = yscaler.TranslateY(sample.value.StackedSum)
                    ' 畫布的高度
                    Dim canvasHeight = canvas.Size.Height - (canvas.Padding.Vertical)
                    ' 底部減去最高的就是實際的高度（縂的）
                    Dim actualHeight = bottom - top
                    Dim stack As IEnumerable(Of SeqValue(Of Double))

                    If stackReorder Then
                        stack = sample.value _
                            .data _
                            .SeqIterator _
                            .OrderBy(Function(o) o.value)
                    Else
                        stack = sample.value _
                            .data _
                            .SeqIterator
                    End If

                    For Each val As SeqValue(Of Double) In stack
                        Dim topleft As New Point(x, top)
                        ' 百分比
                        Dim barHeight! = (+val) / (+sample).StackedSum
                        barHeight = barHeight * actualHeight
                        Dim barSize As New Size(barWidth, barHeight)
                        Dim rect As New Rectangle(topleft, barSize)

                        Call g.FillRectangle(New SolidBrush(data.Serials(val.i).Value), rect)

                        top += barHeight
                    Next
                Else
                    Dim dw As Double = barWidth / sample.value.data.Length

                    x = x - barWidth / 2

                    ' 改变X
                    For Each val As SeqValue(Of Double) In sample.value.data.SeqIterator
                        Dim right As Double = x + dw
                        Dim top As Double = yscaler.TranslateY(val.value)
                        Dim rect As Rectangle = BarPlotAPI.Rectangle(top, x, right, bottom)

                        Call g.DrawRectangle(Pens.Black, rect)
                        Call g.FillRectangle(
                            New SolidBrush(data.Serials(val.i).Value),
                            BarPlotAPI.Rectangle(top + 1,
                                      x + 1,
                                      right - 1,
                                      bottom - 1))
                        x += dw
                    Next
                End If
            Next

            If theme.drawLegend Then
                Dim cssStyle As String = theme.legendLabelCSS
                Dim legends As LegendObject() = LinqAPI.Exec(Of LegendObject) <=
                                                                                _
                    From x As NamedValue(Of Color)
                    In data.Serials
                    Select New LegendObject With {
                        .color = x.Value.RGBExpression,
                        .fontstyle = cssStyle,
                        .style = LegendStyles.Rectangle,
                        .title = x.Name
                    }

                Call DrawLegends(g, legends, False, canvas)
            End If
        End Sub
    End Class
End Namespace
