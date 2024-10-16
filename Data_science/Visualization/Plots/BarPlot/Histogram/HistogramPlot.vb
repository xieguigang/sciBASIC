#Region "Microsoft.VisualBasic::01fcf2961e68dff165a846dce2abb4e1, Data_science\Visualization\Plots\BarPlot\Histogram\HistogramPlot.vb"

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

    '   Total Lines: 181
    '    Code Lines: 152 (83.98%)
    ' Comment Lines: 2 (1.10%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 27 (14.92%)
    '     File Size: 7.56 KB


    '     Class HistogramPlot
    ' 
    '         Properties: showTagChartLayer, xAxis
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: DrawSample, PlotInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.scale
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render

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

Namespace BarPlot.Histogram

    Public Class HistogramPlot : Inherits Plot

        ReadOnly groups As HistogramGroup
        ReadOnly alpha As Double
        ReadOnly drawRect As Boolean

        Public Property xAxis As String
        Public Property showTagChartLayer As Boolean

        Public Sub New(groups As HistogramGroup, alpha As Double, drawRect As Boolean, theme As Theme)
            Call MyBase.New(theme)

            Me.drawRect = drawRect
            Me.alpha = alpha
            Me.groups = groups
        End Sub

        Public Shared Sub DrawSample(g As IGraphics, region As Rectangle,
                                     hist As HistProfile,
                                     ann As NamedValue(Of Color),
                                     scaler As DataScaler,
                                     Optional alpha As Integer = 255,
                                     Optional drawRect As Boolean = True)

            Dim b As New SolidBrush(Color.FromArgb(alpha, ann.Value))

            For Each block As HistogramData In hist.data
                Dim pos As PointF = scaler.Translate(block.x1, block.y)
                Dim sizeF As New SizeF With {
                            .Width = scaler.TranslateX(block.x2) - scaler.TranslateX(block.x1),
                            .Height = region.Bottom - scaler.TranslateY(block.y)
                        }
                Dim rect As New RectangleF With {
                            .Location = pos,
                            .Size = sizeF
                        }

                Call g.FillRectangle(b, rect)

                If drawRect Then
                    Call g.DrawRectangle(
                                Pens.Black,
                                rect.Left, rect.Top,
                                rect.Width, rect.Height)
                End If
            Next
        End Sub

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim region As Rectangle = canvas.PlotRegion(css)

            If groups.Samples.Length = 1 AndAlso groups.Samples.First.data.Length = 0 Then
                Call "No content data for plot histogram chart...".Warning
                Return
            End If

            Dim scalerData As New Scaling(groups, False)
            Dim annotations As Dictionary(Of NamedValue(Of Color)) = groups.Serials.ToDictionary
            Dim gSize As Size = canvas.Size
            Dim X, Y As d3js.scale.LinearScale
            Dim XTicks#() = groups.XRange.CreateAxisTicks
            Dim YTicks#() = groups.YRange.CreateAxisTicks

            With region
                If Not xAxis.StringEmpty Then
                    XTicks = AxisProvider.TryParse(xAxis).AxisTicks
                    X = XTicks.LinearScale.range(integers:={ .Left, .Right})
                Else
                    X = d3js.scale.linear _
                                .domain(values:=XTicks) _
                                .range(integers:={ .Left, .Right})
                End If

                ' Y 为什么是从零开始的？
                Y = d3js.scale.linear _
                            .domain(values:=YTicks) _
                            .range(integers:={ .Bottom, .Top})
            End With

            Dim scaler As New DataScaler With {
                .X = X,
                .Y = Y,
                .region = region,
                .AxisTicks = (XTicks, YTicks)
            }

            Call g.DrawAxis(
                canvas, scaler, theme.drawGrid,
                xlabel:=xlabel,
                ylabel:=ylabel,
                htmlLabel:=False,
                XtickFormat:=theme.XaxisTickFormat,
                YtickFormat:=theme.YaxisTickFormat,
                xlabelRotate:=theme.xAxisRotate
            )

            If Not main.StringEmpty Then
                Dim titleFont As Font = CSS.GetFont(CSSFont.TryParse(theme.mainCSS))
                Dim titleSize As SizeF = g.MeasureString(main, titleFont)
                Dim titlePos As New PointF With {
                    .X = region.Left + (region.Width - titleSize.Width) / 2,
                    .Y = region.Top - titleSize.Height * 1.125
                }

                Call g.DrawString(main, titleFont, Brushes.Black, titlePos)
            End If

            For Each hist As HistProfile In groups.Samples
                Call DrawSample(g, region, hist, annotations(hist.legend.title), scaler, alpha, drawRect)
            Next

            If showTagChartLayer Then
                Dim serials As New List(Of SerialData)

                For Each hist As SeqValue(Of HistProfile) In groups.Samples.SeqIterator
                    serials += (+hist).GetLine(groups.Serials(hist).Value, 2, 5)
                Next

                Dim chart As GraphicsData = Scatter.Plot(
                            serials,
                            size:=$"{canvas.Width},{canvas.Height}",
                            padding:=theme.padding,
                            bg:="transparent",
                            showGrid:=False,
                            showLegend:=False,
                            drawAxis:=False)

                ' 合并图层
                Call g.DrawImageUnscaled(chart, New Rectangle(New Point, gSize))
            End If

            If theme.drawLegend Then
                Call Me.DrawLegends(g, groups.Samples.Select(Function(h) h.legend).ToArray, showBorder:=Not theme.legendBoxStroke.StringEmpty, canvas)
            End If
        End Sub
    End Class
End Namespace
