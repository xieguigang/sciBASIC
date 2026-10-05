' ============================================================================
'  LayerRender.vb - ggplot 图层 -> DataPlot 图型类的委派渲染器
'
'  ggplot 的图层模型要求多个图层共享同一块画布（IGraphics）与同一套坐标系统，
'  而 DataPlot 的图型类默认是"独占画布、一次画完"。这里提供两个能力：
'
'  1) Attach: 把图型类装配到共享画布上（关闭框架重绘 + 钉死绘图区与数据范围）
'  2) Draw*:  把 ggplot 的 SerialData 系列转换为新引擎的 Series 之后委派绘制
'
'  这样每个 geom 图层只需要替换掉旧 ChartPlots 的绘图调用即可，
'  不必重写 ggplot 的数据映射管线。
' ============================================================================

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.Plots.Plot3D.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.d3js.scale
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports std = System.Math

Namespace Canvas

    Public Module LayerRender

        ''' <summary>
        ''' 在宿主画布的给定子区域内绘制一个带坐标轴框架的序列面板。
        ''' 等价于旧引擎的 <c>Scatter.Plot(c, g, rect, ...)</c> 多面板布局模式：
        ''' 依据序列数据范围自动计算刻度与线性缩放，先绘制坐标轴/网格框架，
        ''' 再把几何体委派给新引擎的折线/散点绘制。
        ''' </summary>
        ''' <param name="rect">面板布局区域（画布尺寸 + padding）</param>
        ''' <param name="theme">ggplot CSS 主题（控制网格/轴样式/刻度字体等）</param>
        <Extension>
        Public Sub DrawPanel(g As IGraphics, rect As GraphicsRegion, theme As Theme,
                             serials As IEnumerable(Of SerialData),
                             Optional xlabel$ = "",
                             Optional ylabel$ = "",
                             Optional drawLine As Boolean = True,
                             Optional nticksX As Integer = 9,
                             Optional nticksY As Integer = 9)

            Dim pts As PointF() = serials _
                .SelectMany(Function(s) s.pts) _
                .Select(Function(p) p.pt) _
                .ToArray

            If pts.IsNullOrEmpty Then
                Return
            End If

            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim plotRect As Rectangle = rect.PlotRegion(css)
            Dim xrange As New DoubleRange(pts.Select(Function(p) CDbl(p.X)))
            Dim yrange As New DoubleRange(pts.Select(Function(p) CDbl(p.Y)))
            Dim xticks As Double() = xrange.CreateAxisTicks(nticksX)
            Dim yticks As Double() = yrange.CreateAxisTicks(nticksY)
            Dim scaleX = d3js.scale.linear.domain(values:=xticks).range(integers:={plotRect.Left, plotRect.Right})
            Dim scaleY = d3js.scale.linear.domain(values:=yticks).range(integers:={plotRect.Bottom, plotRect.Top})
            Dim scaler As New DataScaler() With {
                .AxisTicks = (New Vector(xticks), New Vector(yticks)),
                .region = plotRect,
                .X = scaleX,
                .Y = scaleY
            }

            ' 先画坐标轴与网格框架（旧 Scatter.Plot 面板行为）
            Call g.DrawAxis(
                scaler, rect,
                showGrid:=theme.drawGrid,
                xlabel:=xlabel, ylabel:=ylabel,
                labelFontStyle:=theme.axisLabelCSS,
                xlayout:=theme.xAxisLayout, ylayout:=theme.yAxisLayout,
                gridFill:=theme.gridFill,
                gridX:=theme.gridStrokeX, gridY:=theme.gridStrokeY,
                axisStroke:=theme.axisStroke,
                tickFontStyle:=theme.axisTickCSS,
                htmlLabel:=theme.htmlLabel,
                XtickFormat:=theme.XaxisTickFormat,
                YtickFormat:=theme.YaxisTickFormat,
                xlabelRotate:=theme.xAxisRotate)

            ' 再画几何体
            If drawLine Then
                Call DrawLines(g, scaler, theme, serials)
            Else
                Call DrawPoints(g, scaler, theme, serials)
            End If
        End Sub

        ''' <summary>
        ''' 在宿主画布的给定子区域内绘制气泡图面板。
        ''' 等价于旧引擎 <see cref="Plots.Bubble"/> 的图层式绘制：
        ''' <see cref="PointData.value"/> 为气泡半径（像素），正 Y 值域（positiveRangeY）
        ''' 与旧引擎保持一致。
        ''' </summary>
        <Extension>
        Public Sub DrawBubbles(g As IGraphics, rect As GraphicsRegion, theme As Theme,
                               serials As IEnumerable(Of SerialData),
                               Optional xlabel$ = "",
                               Optional ylabel$ = "",
                               Optional nticksX As Integer = 9,
                               Optional nticksY As Integer = 9,
                               Optional positiveRangeY As Boolean = True,
                               Optional bubblePen As Pen = Nothing,
                               Optional showGrid As Boolean = True)

            Dim pts As PointF() = serials _
                .SelectMany(Function(s) s.pts) _
                .Select(Function(p) p.pt) _
                .ToArray

            If pts.IsNullOrEmpty Then
                Return
            End If

            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim plotRect As Rectangle = rect.PlotRegion(css)
            Dim xrange As New DoubleRange(pts.Select(Function(p) CDbl(p.X)))
            Dim ymin As Double = pts.Select(Function(p) CDbl(p.Y)).Min
            Dim ymax As Double = pts.Select(Function(p) CDbl(p.Y)).Max

            If positiveRangeY AndAlso ymin > 0 Then
                ymin = 0
            End If

            Dim xticks As Double() = xrange.CreateAxisTicks(nticksX)
            Dim yticks As Double() = New DoubleRange(ymin, ymax).CreateAxisTicks(nticksY)
            Dim scaleX = d3js.scale.linear.domain(values:=xticks).range(integers:={plotRect.Left, plotRect.Right})
            Dim scaleY = d3js.scale.linear.domain(values:=yticks).range(integers:={plotRect.Top, plotRect.Bottom})
            Dim scaler As New DataScaler() With {
                .AxisTicks = (New Vector(xticks), New Vector(yticks)),
                .region = plotRect,
                .X = scaleX,
                .Y = scaleY
            }

            Call g.DrawAxis(
                scaler, rect,
                showGrid:=showGrid,
                xlabel:=xlabel, ylabel:=ylabel,
                labelFontStyle:=theme.axisLabelCSS,
                xlayout:=theme.xAxisLayout, ylayout:=theme.yAxisLayout,
                gridFill:=theme.gridFill,
                gridX:=theme.gridStrokeX, gridY:=theme.gridStrokeY,
                axisStroke:=theme.axisStroke,
                tickFontStyle:=theme.axisTickCSS,
                htmlLabel:=theme.htmlLabel,
                XtickFormat:=theme.XaxisTickFormat,
                YtickFormat:=theme.YaxisTickFormat,
                xlabelRotate:=theme.xAxisRotate)

            ' 气泡几何体：半径来自 PointData.value，0 值回退为 pointSize
            For Each s As SerialData In serials
                Dim b As SolidBrush = If(Not s.color.IsEmpty, New SolidBrush(s.color), Nothing)

                For Each pt As PointData In s.pts
                    Dim r As Double = If(pt.value = 0R, s.pointSize, pt.value)

                    If r.IsNaNImaginary OrElse r <= 0 Then
                        Continue For
                    End If

                    Dim p As PointF = scaler.Translate(pt.pt.X, pt.pt.Y)
                    Dim bubble As New RectangleF(p.X - r, p.Y - r, r * 2, r * 2)

                    If pt.color.StringEmpty Then
                        If b IsNot Nothing Then
                            Call g.FillPie(b, bubble, 0, 360)
                        End If
                    Else
                        Call g.FillPie(New SolidBrush(pt.color.TranslateColor), bubble, 0, 360)
                    End If

                    If bubblePen IsNot Nothing Then
                        Call g.DrawCircle(pt.pt, r, bubblePen, fill:=False)
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' 把 ggplot 的旧 <see cref="SerialData"/> 系列转换为新引擎的 <see cref="Series"/> 系列
        ''' </summary>
        <Extension>
        Public Function ToSeries(serials As IEnumerable(Of SerialData)) As List(Of Series)
            Dim list As New List(Of Series)

            For Each s As SerialData In If(serials, New SerialData() {})
                Dim n As Integer = If(s.pts Is Nothing, 0, s.pts.Length)
                Dim x As Double() = New Double(n - 1) {}
                Dim y As Double() = New Double(n - 1) {}
                Dim size As Double() = New Double(n - 1) {}

                For i As Integer = 0 To n - 1
                    x(i) = s.pts(i).pt.X
                    y(i) = s.pts(i).pt.Y
                    size(i) = If(s.pts(i).size, s.pointSize)
                Next

                list.Add(New Series With {
                    .Name = s.title,
                    .Color = s.color,
                    .X = x,
                    .Y = y,
                    .Size = size,
                    .LineStyle = s.lineType,
                    .MarkerShape = ToMarkerShape(s.shape),
                    .PointSize = If(s.pointSize > 0 AndAlso s.pointSize < 100, s.pointSize, 0)
                })
            Next

            Return list
        End Function

        ''' <summary>旧图例形状枚举到新引擎标记形状的映射</summary>
        Public Function ToMarkerShape(style As LegendStyles) As MarkerShape
            Select Case style
                Case LegendStyles.Rectangle, LegendStyles.RoundRectangle : Return MarkerShape.Square
                Case LegendStyles.Diamond : Return MarkerShape.Diamond
                Case LegendStyles.Triangle : Return MarkerShape.Triangle
                Case LegendStyles.Hexagon : Return MarkerShape.Hexagon
                Case LegendStyles.Pentacle : Return MarkerShape.Star
                Case LegendStyles.SolidLine, LegendStyles.DashLine : Return MarkerShape.None
                Case Else : Return MarkerShape.Circle
            End Select
        End Function

        ''' <summary>
        ''' 把图型类装配到 ggplot 的共享画布上：
        ''' 关闭框架重绘（背景 / 边框 / 标题 / 坐标轴 / 图例由 ggplot 画布层统一负责），
        ''' 并使用宿主给定的绘图区与跨图层联合计算出来的数据范围。
        ''' </summary>
        Public Function Attach(Of T As PlotEngine)(engine As T,
                                                   scaler As DataScaler,
                                                   Optional area As Rectangle? = Nothing) As T
            Dim rect As Rectangle = If(area, scaler.region)

            engine.DrawFrame = False
            engine.SetPlotArea(New RectangleF(rect.X, rect.Y, rect.Width, rect.Height))

            ' 分类轴（ordinal）不会有数值刻度范围，此时只钉死存在的那一维
            Dim ticks = scaler.AxisTicks

            If ticks.Y IsNot Nothing AndAlso ticks.Y.Length > 0 Then
                engine.YMin = ticks.Y.Min
                engine.YMax = ticks.Y.Max
            End If

            If ticks.X IsNot Nothing AndAlso ticks.X.Length > 0 Then
                engine.XMin = ticks.X.Min
                engine.XMax = ticks.X.Max
            End If

            Return engine
        End Function

        ''' <summary>折线 / 路径图层</summary>
        Public Sub DrawLines(g As IGraphics, scaler As DataScaler, theme As Theme,
                             serials As IEnumerable(Of SerialData),
                             Optional smooth As Boolean = False)

            If serials Is Nothing Then Return

            Using plt As New LinePlot(g, ThemeBridge.ToPlotTheme(theme)) With {.Smooth = smooth}
                Call Attach(plt, scaler)
                Call plt.Plot(ToSeries(serials))
            End Using
        End Sub

        ''' <summary>散点 / 点图层（支持抖动与凸包轮廓）</summary>
        Public Sub DrawPoints(g As IGraphics, scaler As DataScaler, theme As Theme,
                              serials As IEnumerable(Of SerialData),
                              Optional jitter As Boolean = False,
                              Optional showConvexHull As Boolean = False)

            If serials Is Nothing Then Return

            Using plt As New ScatterPlot(g, ThemeBridge.ToPlotTheme(theme)) With {
                .Jitter = jitter,
                .ShowConvexHull = showConvexHull,
                .ShowErrorBars = False
            }
                Call Attach(plt, scaler)
                Call plt.Plot(ToSeries(serials))
            End Using
        End Sub

        ''' <summary>面积 / 置信带图层</summary>
        Public Sub DrawAreas(g As IGraphics, scaler As DataScaler, theme As Theme,
                             serials As IEnumerable(Of SerialData))

            If serials Is Nothing Then Return

            Using plt As New AreaPlot(g, ThemeBridge.ToPlotTheme(theme))
                Call Attach(plt, scaler)
                Call plt.Plot(ToSeries(serials))
            End Using
        End Sub

        ' ====================================================================
        '  分组型（分类轴）图层：箱线图 / 小提琴图 / 抖动图
        '  这几类图表的分类位置由引擎自己按绘图区等分排布，
        '  Y 轴范围沿用 ggplot 跨图层联合计算出来的结果。
        ' ====================================================================

        ''' <summary>箱线图图层</summary>
        Public Sub DrawBoxes(g As IGraphics, scaler As DataScaler, theme As Theme,
                             groups As IEnumerable(Of BoxGroup),
                             Optional showOutliers As Boolean = False,
                             Optional horizontal As Boolean = False)

            If groups Is Nothing Then Return

            Using plt As New BoxPlot(g, ThemeBridge.ToPlotTheme(theme)) With {
                .Groups = groups.ToList(),
                .ShowOutliers = showOutliers,
                .Horizontal = horizontal
            }
                Call Attach(plt, scaler)
                Call plt.Plot()
            End Using
        End Sub

        ''' <summary>小提琴图图层</summary>
        Public Sub DrawViolins(g As IGraphics, scaler As DataScaler, theme As Theme,
                               groups As IEnumerable(Of BoxGroup))

            If groups Is Nothing Then Return

            Using plt As New ViolinPlot(g, ThemeBridge.ToPlotTheme(theme)) With {.Groups = groups.ToList()}
                Call Attach(plt, scaler)
                Call plt.Plot()
            End Using
        End Sub

        ''' <summary>抖动散点图层</summary>
        Public Sub DrawJitters(g As IGraphics, scaler As DataScaler, theme As Theme,
                               groups As IEnumerable(Of BoxGroup),
                               Optional jitterWidth As Single = 0.4F)

            If groups Is Nothing Then Return

            Using plt As New JitterPlot(g, ThemeBridge.ToPlotTheme(theme)) With {
                .Groups = groups.ToList(),
                .JitterWidth = jitterWidth
            }
                Call Attach(plt, scaler)
                Call plt.Plot()
            End Using
        End Sub

        ''' <summary>柱状图图层（分类 + 取值，支持堆叠 / 百分比堆叠）</summary>
        Public Sub DrawBars(g As IGraphics, scaler As DataScaler, theme As Theme,
                            categories As String(), values As Double(),
                            Optional colors As Color() = Nothing,
                            Optional stack As BarPlot.StackMode = BarPlot.StackMode.None,
                            Optional horizontal As Boolean = False,
                            Optional multiValues As Double(,) = Nothing,
                            Optional seriesNames As String() = Nothing)

            ' ggplot 的堆叠柱状图（geom_bar + position=stack）在旧引擎之中是按
            ' 百分比堆叠绘制的：每根柱子按系列占比填满整个绘图区高度，且不参与
            ' ggplot 的数据坐标换算（以像素绘图区为单位）。这里保持同样的语义，
            ' 直接用画布原语绘制，从而与旧引擎的输出保持一致。
            Dim nCat As Integer = If(categories Is Nothing, 0, categories.Length)

            If nCat = 0 Then Return

            Dim nSer As Integer = If(multiValues Is Nothing, 1, multiValues.GetLength(0))
            Dim palette As Color() = If(colors, ThemeBridge.ToPlotTheme(theme).Palette)
            Dim area As New RectangleF(scaler.region.X, scaler.region.Y, scaler.region.Width, scaler.region.Height)
            Dim band As Single = If(horizontal, area.Height, area.Width) / nCat
            Dim thickness As Single = band * 0.7F
            Dim offset As Single = (band - thickness) / 2

            For j As Integer = 0 To nCat - 1
                Dim cell As Double() = New Double(nSer - 1) {}
                Dim total As Double = 0

                For i As Integer = 0 To nSer - 1
                    cell(i) = If(multiValues Is Nothing,
                                 If(values IsNot Nothing AndAlso j < values.Length, values(j), 0),
                                 multiValues(i, j))
                    total += std.Abs(cell(i))
                Next

                If total <= 0 Then Continue For

                Dim isPercent As Boolean = stack = BarPlot.StackMode.Percent
                Dim cursor As Single = 0

                For i As Integer = 0 To nSer - 1
                    Dim value As Double = If(isPercent, std.Abs(cell(i)) / total, std.Abs(cell(i)) / total)
                    Dim len As Single = CSng(value * If(horizontal, area.Width, area.Height))

                    If len <= 0 Then Continue For

                    Dim color As Color = palette(i Mod palette.Length)
                    Dim bar As RectangleF

                    If horizontal Then
                        bar = New RectangleF(area.X + cursor, area.Y + j * band + offset, len, thickness)
                    Else
                        bar = New RectangleF(area.X + j * band + offset, area.Bottom - cursor - len, thickness, len)
                    End If

                    Using brush As New SolidBrush(color)
                        Call g.FillRectangle(brush, bar)
                    End Using
                    Using pen As New Pen(color.Darken, 1.0F)
                        Call g.DrawRectangle(pen, bar)
                    End Using

                    cursor += len
                Next
            Next
        End Sub

        ''' <summary>直方图图层</summary>
        Public Sub DrawHistogram(g As IGraphics, scaler As DataScaler, theme As Theme,
                                 data As Double(),
                                 Optional bins As Integer = 0,
                                 Optional color As Color = Nothing)

            If data Is Nothing OrElse data.Length = 0 Then Return

            Using plt As New HistogramPlot(g, ThemeBridge.ToPlotTheme(theme)) With {
                .Data = data,
                .Bins = If(bins > 0, bins, 30)
            }
                If Not color.IsEmpty Then plt.Color = color

                Call Attach(plt, scaler)
                Call plt.Plot()
            End Using
        End Sub

        ''' <summary>多个分组的直方图图层</summary>
        Public Sub DrawHistograms(g As IGraphics, scaler As DataScaler, theme As Theme,
                                  groups As IEnumerable(Of CategoryGroup),
                                  Optional bins As Integer = 0)

            If groups Is Nothing Then Return

            Using plt As New HistogramPlot(g, ThemeBridge.ToPlotTheme(theme)) With {
                .Groups = groups.ToList(),
                .Bins = If(bins > 0, bins, 30)
            }
                Call Attach(plt, scaler)
                Call plt.Plot()
            End Using
        End Sub

        ''' <summary>饼图图层</summary>
        Public Sub DrawPie(g As IGraphics, scaler As DataScaler, theme As Theme,
                           labels As String(), values As Double(), colors As Color())

            Using plt As New PiePlot(g, ThemeBridge.ToPlotTheme(theme)) With {
                .Labels = If(labels, New String() {}),
                .Values = If(values, New Double() {}),
                .Colors = If(colors, New Color() {})
            }
                Call Attach(plt, scaler)
                Call plt.Plot()
            End Using
        End Sub
    End Module
End Namespace