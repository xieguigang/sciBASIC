#Region "Microsoft.VisualBasic::e3af82e7e7524242e75386fd61af2758, Data_science\Visualization\Plots\Scatter\Plot\LinePlot2D.vb"

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

    '   Total Lines: 339
    '    Code Lines: 250 (73.75%)
    ' Comment Lines: 35 (10.32%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 54 (15.93%)
    '     File Size: 13.90 KB


    '     Class LinePlot2D
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CreateScaler
    ' 
    '         Sub: DrawLine, PlotInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.scale
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Interpolation
Imports Microsoft.VisualBasic.Math.LinearAlgebra
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

Namespace Plots

    Public Class LinePlot2D : Inherits Plot

        ReadOnly array As SerialData()
        ReadOnly xaxis$, yaxis$
        ReadOnly ablines As Line()
        ReadOnly fill As Boolean
        ReadOnly fillPie As Boolean
        ReadOnly interplot As Splines

        Friend xlim As Double() = Nothing
        Friend ylim As Double() = Nothing

        Public Sub New(data As IEnumerable(Of SerialData), theme As Theme,
                       Optional fill As Boolean = False,
                       Optional fillPie As Boolean = True,
                       Optional interplot As Splines = Splines.None)

            MyBase.New(theme)

            Me.fill = fill
            Me.array = data.ToArray
            Me.interplot = interplot
            Me.fillPie = fillPie
        End Sub

        Public Shared Sub DrawLine(g As IGraphics,
                                   rect As GraphicsRegion,
                                   scaler As DataScaler,
                                   line As SerialData,
                                   interplot As Splines,
                                   Optional fill As Boolean = False,
                                   Optional fillPie As Boolean = False)

            Dim defaultPen As Pen = line.GetPen
            Dim br As New SolidBrush(line.color)
            Dim fillBrush As New SolidBrush(Color.FromArgb(100, baseColor:=line.color))
            Dim d! = line.pointSize
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim r As Single = line.pointSize / 2
            Dim bottom! = rect.PlotRegion(css).Bottom
            Dim getPointBrush = Function(pt As PointData)
                                    If pt.color.StringEmpty Then
                                        Return br
                                    Else
                                        Return pt.color.GetBrush
                                    End If
                                End Function
            Dim polygon As New List(Of PointF)
            Dim pt1, pt2 As PointF

            If line.pts.Length = 1 Then
                Dim a As PointData = line.pts.First
                Dim b As New PointData(New PointF(a.pt.X, 0))

                pt1 = scaler.Translate(a)
                pt2 = scaler.Translate(b)

                Call g.DrawLine(defaultPen, pt1, pt2)

                Return
            End If

            Dim pts As SlideWindow(Of PointData)() = line.pts _
                .getSplinePoints(spline:=interplot) _
                .SlideWindows(2) _
                .ToArray
            Dim color1 As Color
            Dim color2 As Color
            Dim color3 As Vector

            For Each pt As SlideWindow(Of PointData) In pts
                Dim a As PointData = pt.First
                Dim b As PointData = pt.Last
                Dim pen As Pen = defaultPen

                If Not (a.color.StringEmpty AndAlso b.color.StringEmpty) Then
                    color1 = a.color.TranslateColor
                    color2 = b.color.TranslateColor
                    color3 = (color1.ToVector + color2.ToVector) / 2

                    pen = New Pen(color3.ArgbColor, defaultPen.Width) With {
                        .DashStyle = defaultPen.DashStyle
                    }
                End If

                pt1 = scaler.Translate(a)
                pt2 = scaler.Translate(b)

                polygon.Add(pt1)
                polygon.Add(pt2)

                Call g.DrawLine(pen, pt1, pt2)

                If fill Then
                    Dim path As New GraphicsPath
                    Dim ptc As New PointF(pt2.X, bottom) ' c
                    Dim ptd As New PointF(pt1.X, bottom) ' d


                    '   /-b
                    ' a-  |
                    ' |   |
                    ' |   |
                    ' d---c

                    path.AddLine(pt1, pt2)
                    path.AddLine(pt2, ptc)
                    path.AddLine(ptc, ptd)
                    path.AddLine(ptd, pt1)
                    path.CloseFigure()

                    Call g.FillPath(fillBrush, path)
                End If

                If fillPie Then
                    Call g.FillPie(getPointBrush(a), pt1.X - r, pt1.Y - r, d, d, 0, 360)
                    Call g.FillPie(getPointBrush(b), pt2.X - r, pt2.Y - r, d, d, 0, 360)
                End If

                ' 绘制误差线
                ' 首先计算出误差的长度，然后可pt1,pt2的Y相加减即可得到新的位置
                ' 最后划线即可
                'If a.errPlus > 0 Then
                '    Call g.drawErrorLine(scaler, pt1, a.errPlus + a.pt.Y, width, br)
                'End If
                'If a.errMinus > 0 Then
                '    Call g.drawErrorLine(scaler, pt1, a.pt.Y - a.errMinus, width, br)
                'End If
                'If b.errPlus > 0 Then
                '    Call g.drawErrorLine(scaler, pt2, b.errPlus + b.pt.Y, width, br)
                'End If
                'If b.errMinus > 0 Then
                '    Call g.drawErrorLine(scaler, pt2, b.pt.Y - b.errMinus, width, br)
                'End If
            Next
        End Sub

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, rect As GraphicsRegion)
            Dim canvas As IGraphics = g
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim region As Rectangle = rect.PlotRegion(css)
            Dim scaler As DataScaler = CreateScaler(g, rect)
            Dim gSize As Size = rect.Size

            If theme.drawAxis Then
                Call g.DrawAxis(scaler, rect, theme.drawGrid,
                    xlabel:=xlabel, ylabel:=ylabel,
                    htmlLabel:=theme.htmlLabel,
                    tickFontStyle:=theme.axisTickCSS,
                    labelFontStyle:=theme.axisLabelCSS,
                    xlayout:=theme.xAxisLayout,
                    ylayout:=theme.yAxisLayout,
                    gridX:=theme.gridStrokeX,
                    gridY:=theme.gridStrokeY,
                    gridFill:=theme.gridFill,
                    XtickFormat:=theme.XaxisTickFormat,
                    YtickFormat:=theme.YaxisTickFormat,
                    axisStroke:=theme.axisStroke,
                    xlabelRotate:=theme.xAxisRotate
                )
            End If

            Dim width As Double = region.Width / 200
            Dim annotations As New Dictionary(Of String, (raw As SerialData, line As SerialData))

            For Each line As SerialData In array
                Call DrawLine(g, rect, scaler, line, interplot, fill, fillPie)

                If Not line.DataAnnotations.IsNullOrEmpty Then
                    Dim raw = array.Where(Function(s) s.title = line.title).First

                    Call annotations.Add(line.title, (raw, line))
                End If
            Next

            For Each part In annotations.Values
                For Each annotation As Annotation In part.line.DataAnnotations
                    Call annotation.Draw(g, scaler, part.raw, rect)
                Next
            Next

            If theme.drawLegend Then
                Dim legends As LegendObject() = LinqAPI.Exec(Of LegendObject) _
                                                                              _
                () <= From s As SerialData
                      In array
                      Let sColor As String = s.color.RGBExpression
                      Select New LegendObject With {
                          .color = sColor,
                          .fontstyle = theme.legendLabelCSS,
                          .style = s.shape,
                          .title = s.title
                      }

                Call DrawLegends(g, legends, showBorder:=True, canvas:=rect)
            End If

            Call DrawMainTitle(g, region)

            ' draw ablines
            For Each line As Line In ablines.SafeQuery
                Dim a As PointF = scaler.Translate(line.A)
                Dim b As PointF = scaler.Translate(line.B)
                Dim style As Pen = css.GetPen(line.Stroke)

                Call g.DrawLine(style, a, b)
            Next
        End Sub

        Public Function CreateScaler(ByRef g As IGraphics, rect As GraphicsRegion) As DataScaler
            Dim canvas As IGraphics = g
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim region As Rectangle = rect.PlotRegion(css)
            Dim XTicks#(), YTicks#()

            '    With array.CreateAxisTicks(
            '    preferPositive:=preferPositive,
            '    scaleX:=If(XaxisAbsoluteScalling, 1, 1.25),
            '    scaleY:=If(YaxisAbsoluteScalling, 1, 1.25)
            ')

            '        XTicks = .x
            '        YTicks = .y
            '    End With

            '    If ticksY > 0 Then
            '        YTicks = AxisScalling.GetAxisByTick(YTicks, tick:=ticksY)
            '    End If

            XTicks = array.Select(Function(s) s.pts).IteratesALL.Select(Function(p) CDbl(p.pt.X)).ToArray
            YTicks = array.Select(Function(s) s.pts).IteratesALL.Select(Function(p) CDbl(p.pt.Y)).ToArray

            Dim X As Scaler
            Dim Y As LinearScale

            ' 使用手动指定的范围
            ' 手动指定坐标轴值的范围的时候，X坐标轴无法使用term离散映射
            If Not xaxis.StringEmpty AndAlso Not yaxis.StringEmpty Then
                XTicks = AxisProvider.TryParse(xaxis).AxisTicks
                YTicks = AxisProvider.TryParse(yaxis).AxisTicks
                X = XTicks.LinearScale.range(integers:={region.Left, region.Right})
                Y = YTicks.LinearScale.range(integers:={region.Bottom, region.Top})
            Else
                ' 如果所有数据点都有单词，则X轴使用离散映射
                If array.All(Function(line) line.pts.All(Function(a) Not a.axisLabel.StringEmpty)) Then
                    Dim allTermLabels As String() = array _
                        .Select(Function(line)
                                    Return line.pts.Select(Function(a) a.axisLabel)
                                End Function) _
                        .IteratesALL _
                        .Distinct _
                        .ToArray

                    YTicks = YTicks.Range.CreateAxisTicks
                    X = d3js.scale _
                        .ordinal _
                        .domain(allTermLabels) _
                        .range(integers:={region.Left, region.Right})
                Else
                    If Not xlim.IsNullOrEmpty Then
                        XTicks = xlim.CreateAxisTicks
                    Else
                        XTicks = XTicks.Range.CreateAxisTicks
                    End If
                    If Not ylim.IsNullOrEmpty Then
                        YTicks = ylim.CreateAxisTicks
                    Else
                        YTicks = YTicks.Range.CreateAxisTicks
                    End If

                    X = d3js.scale _
                        .linear(reverse:=theme.xAxisReverse) _
                        .domain(values:=XTicks) _
                        .range(integers:={region.Left, region.Right})
                End If

                Y = d3js.scale.linear.domain(values:=YTicks).range(integers:={region.Bottom, region.Top})
            End If

            Return New DataScaler With {
                .X = X,
                .Y = Y,
                .region = region,
                .AxisTicks = (XTicks, YTicks)
            }
        End Function

    End Class
End Namespace
