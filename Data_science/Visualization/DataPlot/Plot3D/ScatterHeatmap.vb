#Region "Microsoft.VisualBasic::3f8c4fd7e4de5a2fdaa03a7d9f0c1b22, Data_science\Visualization\DataPlot\Plot3D\ScatterHeatmap.vb"

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

    '   Total Lines: 338
    '    Code Lines: 226 (66.86%)
    ' Comment Lines: 66 (19.53%)
    '    - Xml Docs: 40.91%
    ' 
    '   Blank Lines: 46 (13.61%)
    '     File Size: 17.44 KB


    '     Module ScatterHeatmap
    ' 
    '         Function: GetPlotFunction, Plot
    ' 
    '     Class Scatter3DPoint
    ' 
    '         Properties: c, x, y, z
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
'  ScatterHeatmap.vb - 三维曲面图（函数曲面 / 矩阵曲面）
'
'  出版级改造要点：
'   1. 恢复并重写坐标轴：三维盒框 + 背板网格 + 三轴刻度与刻度值（旧版本的
'      DrawAxis 整段被注释掉，画出来的图完全没有坐标系）；
'   2. 曲面面片改成 Element3D 的 Polygon，与坐标轴共用同一条
'      RenderAs3DChart 投影 / 缩放 / Z 排序管线，保证曲面与坐标轴严格对齐；
'   3. 色标自己绘制（旧实现把位图图例 DrawImageUnscaled 到画布上，
'      既依赖 GDI 位图、位置还被算到了画布之外）；
'   4. 去掉写死的 Drivers.GDI，driver 与 dpi 透传；默认页边距改为 DefaultPadding。
' ============================================================================

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.Plots.Plot3D.Device
Imports Microsoft.VisualBasic.Data.Plots.Plot3D.Model
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.MIME.Html
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports std = System.Math

Namespace Plot3D

    ''' <summary>
    ''' 三维曲面 / 散点热图上的一个采样点
    ''' </summary>
    Public Class Scatter3DPoint

        ''' <summary>X 坐标</summary>
        Public Property x As Double
        ''' <summary>Y 坐标</summary>
        Public Property y As Double
        ''' <summary>Z 坐标（函数值）</summary>
        Public Property z As Double
        ''' <summary>该点用于上色的数值（可与 Z 不同）</summary>
        Public Property c As Double

    End Class

    ''' <summary>
    ''' 三维曲面图（<c>z = f(x, y)</c>，颜色按第四个维度映射）
    ''' </summary>
    Public Module ScatterHeatmap

        ''' <summary>
        ''' 默认的页边距（与 <c>Drawing2D.g.DefaultPadding</c> 等价，
        ''' 这里单独定义一个常量是为了避免参数名 <c>g</c> 遮蔽模块名）
        ''' </summary>
        Private Const DefaultPaddingCSS$ = "padding:100px 100px 100px 100px;"

        ''' <summary>
        ''' 依据二元函数计算并绘制三维曲面
        ''' </summary>
        ''' <param name="f">二元函数，返回 <c>(Z, color)</c> 元组</param>
        ''' <param name="xrange">X 取值范围</param>
        ''' <param name="yrange">Y 取值范围</param>
        ''' <param name="camera">相机</param>
        ''' <param name="xn">X 方向采样数</param>
        ''' <param name="yn">Y 方向采样数</param>
        ''' <param name="legendTitle">色标标题</param>
        ''' <param name="mapName">色阶名称</param>
        ''' <param name="mapLevels">色阶层级数</param>
        ''' <param name="bg">画布背景色</param>
        ''' <param name="parallel">是否并行计算函数值</param>
        ''' <param name="matrix">可选，用于接收原始采样矩阵</param>
        ''' <param name="axisFont">坐标轴字体 CSS</param>
        ''' <param name="legendFont">色标标题字体（为空时使用 <paramref name="axisFont"/>）</param>
        ''' <param name="showLegend">是否绘制色标</param>
        ''' <param name="title">图表标题</param>
        ''' <param name="labX">X 轴标题</param>
        ''' <param name="labY">Y 轴标题</param>
        ''' <param name="labZ">Z 轴标题</param>
        ''' <param name="showBox">是否绘制三维盒框</param>
        ''' <param name="showGrid">是否绘制背板网格</param>
        ''' <param name="showTicks">是否绘制三轴刻度</param>
        ''' <param name="drawPath">是否绘制面片边框（线框）</param>
        ''' <param name="illumination">是否启用光照</param>
        ''' <param name="padding">页边距 CSS，缺省使用 <c>g.DefaultPadding</c></param>
        ''' <param name="dpi">导出分辨率</param>
        ''' <param name="driver">绘图驱动（缺省跟随全局默认驱动，不再写死 GDI）</param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot(f As Func(Of Double, Double, (Z#, color#)),
                             xrange As DoubleRange,
                             yrange As DoubleRange,
                             camera As Camera,
                             Optional xn% = 100,
                             Optional yn% = 100,
                             Optional legendTitle$ = "3D scatter heatmap",
                             Optional mapName$ = "Spectral:c10",
                             Optional mapLevels% = 25,
                             Optional bg$ = "white",
                             Optional parallel As Boolean = False,
                             Optional matrix As List(Of Scatter3DPoint) = Nothing,
                             Optional axisFont$ = CSSFont.Win10Normal,
                             Optional legendFont As Font = Nothing,
                             Optional showLegend As Boolean = True,
                             Optional title$ = "",
                             Optional labX$ = "X",
                             Optional labY$ = "Y",
                             Optional labZ$ = "Z",
                             Optional showBox As Boolean = True,
                             Optional showGrid As Boolean = True,
                             Optional showTicks As Boolean = True,
                             Optional drawPath As Boolean = False,
                             Optional illumination As Boolean = True,
                             Optional padding$ = Nothing,
                             Optional dpi As Integer = 100,
                             Optional driver As Drivers = Drivers.Default) As GraphicsData

            Dim data As (sf As Surface, C As Double())() =
                f.Surface(
                    xrange, yrange,
                    xrange.Length / xn,
                    yrange.Length / yn,
                    parallel, matrix).ToArray

            Return data.Plot(
                camera, legendTitle,
                mapName, mapLevels,
                bg,
                axisFont, legendFont, showLegend,
                title, labX, labY, labZ,
                showBox, showGrid, showTicks,
                drawPath, illumination,
                padding, dpi, driver)
        End Function

        ''' <summary>
        ''' 生成可以复用到宿主画布之上的绘图委托
        ''' </summary>
        ''' <param name="f">二元函数</param>
        ''' <param name="xrange">X 取值范围</param>
        ''' <param name="yrange">Y 取值范围</param>
        ''' <param name="xn">X 方向采样数</param>
        ''' <param name="yn">Y 方向采样数</param>
        ''' <param name="legendTitle">色标标题</param>
        ''' <param name="mapName">色阶名称</param>
        ''' <param name="mapLevels">色阶层级数</param>
        ''' <param name="bg">画布背景色</param>
        ''' <param name="parallel">是否并行计算函数值</param>
        ''' <param name="matrix">可选，用于接收原始采样矩阵</param>
        ''' <param name="axisFont">坐标轴字体 CSS</param>
        ''' <param name="legendFont">色标标题字体</param>
        ''' <param name="showLegend">是否绘制色标</param>
        ''' <param name="title">图表标题</param>
        ''' <param name="padding">页边距 CSS</param>
        ''' <returns></returns>
        <Extension>
        Public Function GetPlotFunction(f As Func(Of Double, Double, (Z#, color#)),
                                        xrange As DoubleRange,
                                        yrange As DoubleRange,
                                        Optional xn% = 100,
                                        Optional yn% = 100,
                                        Optional legendTitle$ = "3D scatter heatmap",
                                        Optional mapName$ = "Spectral:c10",
                                        Optional mapLevels% = 25,
                                        Optional bg$ = "white",
                                        Optional parallel As Boolean = False,
                                        Optional matrix As List(Of Scatter3DPoint) = Nothing,
                                        Optional axisFont$ = CSSFont.Win10Normal,
                                        Optional legendFont As Font = Nothing,
                                        Optional showLegend As Boolean = True,
                                        Optional title$ = "",
                                        Optional padding$ = Nothing) As DrawGraphics

            Dim data As (sf As Surface, C As Double())() =
                f.Surface(
                     xrange, yrange,
                     xrange.Length / xn,
                     yrange.Length / yn,
                     parallel, matrix) _
                 .ToArray

            Return data.GetPlotFunction(
                legendTitle,
                mapName, mapLevels,
                bg,
                axisFont, legendFont, showLegend, title, padding)
        End Function




        ''' <summary>
        ''' 生成可以复用到宿主画布之上的绘图委托（已采样好的曲面数据）
        ''' </summary>
        ''' <param name="data">曲面面片与对应的上色数值</param>
        ''' <param name="legendTitle">色标标题</param>
        ''' <param name="mapName">色阶名称</param>
        ''' <param name="mapLevels">色阶层级数</param>
        ''' <param name="bg">画布背景色</param>
        ''' <param name="axisFont">坐标轴字体 CSS</param>
        ''' <param name="legendFont">色标标题字体</param>
        ''' <param name="showLegend">是否绘制色标</param>
        ''' <param name="title">图表标题</param>
        ''' <param name="padding">页边距 CSS</param>
        ''' <returns></returns>
        <Extension>
        Public Function GetPlotFunction(data As (sf As Surface, c As Double())(),
                                        Optional legendTitle$ = "3D scatter heatmap",
                                        Optional mapName$ = "Spectral:c10",
                                        Optional mapLevels% = 25,
                                        Optional bg$ = "white",
                                        Optional axisFont$ = CSSFont.Win10Normal,
                                        Optional legendFont As Font = Nothing,
                                        Optional showLegend As Boolean = True,
                                        Optional title$ = "",
                                        Optional padding$ = Nothing) As DrawGraphics

            Dim internal As __plot = __plot.Create(
                data, legendTitle, mapName, mapLevels, bg,
                axisFont, legendFont, showLegend, title,
                padding:=padding)

            Return Sub(ByRef g, camera)
                       If internal.theme.antiAlias Then
                           Call Plot3DCanvas.ApplyAntiAlias(g)
                       End If

                       Call g.Clear(bg.ToColor)
                       Call internal.Plot(g, camera,
                                          New GraphicsRegion(camera.screen, If(padding, DefaultPaddingCSS)))
                   End Sub
        End Function

        ''' <summary>
        ''' 绘制已经采样好的三维曲面数据
        ''' </summary>
        ''' <param name="data">曲面面片与对应的上色数值</param>
        ''' <param name="camera">相机</param>
        ''' <param name="legendTitle">色标标题</param>
        ''' <param name="mapName">色阶名称</param>
        ''' <param name="mapLevels">色阶层级数</param>
        ''' <param name="bg">画布背景色</param>
        ''' <param name="axisFont">坐标轴字体 CSS</param>
        ''' <param name="legendFont">色标标题字体</param>
        ''' <param name="showLegend">是否绘制色标</param>
        ''' <param name="title">图表标题</param>
        ''' <param name="labX">X 轴标题</param>
        ''' <param name="labY">Y 轴标题</param>
        ''' <param name="labZ">Z 轴标题</param>
        ''' <param name="showBox">是否绘制三维盒框</param>
        ''' <param name="showGrid">是否绘制背板网格</param>
        ''' <param name="showTicks">是否绘制三轴刻度</param>
        ''' <param name="drawPath">是否绘制面片边框（线框）</param>
        ''' <param name="illumination">是否启用光照</param>
        ''' <param name="padding">页边距 CSS</param>
        ''' <param name="dpi">导出分辨率</param>
        ''' <param name="driver">绘图驱动</param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot(data As (sf As Surface, c As Double())(),
                             camera As Camera,
                             Optional legendTitle$ = "3D scatter heatmap",
                             Optional mapName$ = "Spectral:c10",
                             Optional mapLevels% = 25,
                             Optional bg$ = "white",
                             Optional axisFont$ = CSSFont.Win10Normal,
                             Optional legendFont As Font = Nothing,
                             Optional showLegend As Boolean = True,
                             Optional title$ = "",
                             Optional labX$ = "X",
                             Optional labY$ = "Y",
                             Optional labZ$ = "Z",
                             Optional showBox As Boolean = True,
                             Optional showGrid As Boolean = True,
                             Optional showTicks As Boolean = True,
                             Optional drawPath As Boolean = False,
                             Optional illumination As Boolean = True,
                             Optional padding$ = Nothing,
                             Optional dpi As Integer = 100,
                             Optional driver As Drivers = Drivers.Default) As GraphicsData

            Dim internal As __plot = __plot.Create(
                data, legendTitle, mapName, mapLevels, bg,
                axisFont, legendFont, showLegend, title,
                labX, labY, labZ, showBox, showGrid, showTicks, drawPath, illumination,
                padding:=padding)

            Return GraphicsPlots(
                camera.screen,
                If(padding, DefaultPaddingCSS),
                bg$,
                driver:=driver,
                dpi:=dpi,
                plotAPI:=Sub(ByRef g, region)
                             internal.Plot(g, camera, region)
                         End Sub)
        End Function

        ''' <summary>
        ''' 三维曲面图的绘制状态：把"取样 → 分层配色 → 渲染"三段串起来
        ''' </summary>
        Private Structure __plot

            Dim averages As Double()
            Dim levels As Integer()
            Dim colors As SolidBrush()
            Dim stops As Color()
            Dim rawPoints As Point3D()
            Dim data As (sf As Surface, c As Double())()
            Dim legendTitle$
            Dim theme As Plot3DTheme
            Dim showLegend As Boolean
            Dim drawPath As Boolean
            Dim illumination As Boolean
            Dim labels As (X As String, Y As String, Z As String)

            ''' <summary>由曲面数据构建绘制状态（分层配色在此完成）</summary>
            Public Shared Function Create(data As (sf As Surface, c As Double())(),
                                          legendTitle$, mapName$, mapLevels%, bg$,
                                          axisFont$, legendFont As Font,
                                          showLegend As Boolean, title$,
                                          Optional labX$ = "X",
                                          Optional labY$ = "Y",
                                          Optional labZ$ = "Z",
                                          Optional showBox As Boolean = True,
                                          Optional showGrid As Boolean = True,
                                          Optional showTicks As Boolean = True,
                                          Optional drawPath As Boolean = False,
                                          Optional illumination As Boolean = True,
                                          Optional padding$ = Nothing) As __plot

                Dim averages As Double() = data _
                    .Select(Function(c) c.c.Average).ToArray
                Dim levels As Integer() = averages _
                    .GenerateMapping(mapLevels) _
                    .ToArray
                Dim colors As SolidBrush() = Designer _
                    .GetBrushes(mapName, mapLevels)
                Dim rawPoints As Point3D() = data _
                    .Select(Function(s) s.sf.vertices) _
                    .IteratesALL _
                    .ToArray
                Dim theme As New Plot3DTheme With {
                    .background = bg,
                    .axisLabelCSS = axisFont,
                    .title = title,
                    .padding = If(padding, DefaultPaddingCSS),
                    .showBox = showBox,
                    .showGrid = showGrid,
                    .showTicks = showTicks,
                    .drawLegend = showLegend
                }

                If Not legendFont Is Nothing Then
                    theme.tickCSS = legendFont.ToString
                    theme.axisLabelCSS = legendFont.ToString
                End If

                Return New __plot With {
                    .averages = averages,
                    .colors = colors,
                    .stops = colors.Select(Function(b) b.Color).ToArray,
                    .data = data,
                    .legendTitle = legendTitle,
                    .levels = levels,
                    .rawPoints = rawPoints,
                    .theme = theme,
                    .showLegend = showLegend,
                    .drawPath = drawPath,
                    .illumination = illumination,
                    .labels = (labX, labY, labZ)
                }
            End Function

            ''' <summary>
            ''' 渲染：坐标系 + 曲面面片走同一条 Element3D 管线，之后再叠色标与标题
            ''' </summary>
            Public Sub Plot(g As IGraphics, camera As Camera, region As GraphicsRegion)
                Dim css As CSSEnvirnment = g.LoadEnvironment
                Dim plotRect As Rectangle = region.PlotRegion(css)
                Dim xrange As New DoubleRange(rawPoints.Select(Function(p) p.X))
                Dim yrange As New DoubleRange(rawPoints.Select(Function(p) p.Y))
                Dim zrange As New DoubleRange(rawPoints.Select(Function(p) p.Z))
                Dim elements As New List(Of Element3D)

                If theme.antiAlias Then
                    Call Plot3DCanvas.ApplyAntiAlias(g)
                End If

                ' 在右侧与顶部为色标和标题预留空间，三维场景缩放到剩下的区域里
                Dim legendWidth As Single = If(showLegend, ColorBarReservedWidth(g), 0)
                Dim scene As GraphicsRegion = Plot3DCanvas.ReserveRegion(
                    region, theme, legendWidth, Plot3DCanvas.TitlesHeight(g, theme))

                ' -----------------------------------------------------------
                '  1. 三维坐标系
                ' -----------------------------------------------------------
                If theme.showBox Then
                    Call elements.AddRange(AxisFrame.Box(css, xrange, yrange, zrange, theme))
                End If
                If theme.showGrid Then
                    Call elements.AddRange(AxisFrame.BackGrids(css, xrange, yrange, zrange, theme, camera))
                End If
                If theme.showTicks Then
                    Call elements.AddRange(AxisFrame.Ticks(css, xrange, yrange, zrange, theme))
                End If

                ' -----------------------------------------------------------
                '  2. 曲面面片：与坐标系共用投影与缩放，保证严格对齐
                ' -----------------------------------------------------------
                For Each sf In data.SeqIterator
                    Dim surface As Surface = (+sf).sf
                    Dim level% = levels(sf.i)

                    If level > colors.Length - 1 Then
                        level = colors.Length - 1
                    ElseIf level < 0 Then
                        level = 0
                    End If

                    Dim model As New Surface With {
                        .brush = colors(level),
                        .vertices = surface.vertices
                    }
                    Dim brush As Brush

                    If illumination Then
                        brush = New SolidBrush(camera.Lighting(model))
                    Else
                        brush = colors(level)
                    End If

                    Call elements.Add(New Polygon With {
                        .Brush = brush,
                        .Path = surface.vertices
                    })
                Next

                Call elements.RenderAs3DChart(
                    canvas:=g,
                    camera:=camera,
                    region:=scene,
                    theme:=theme)

                ' -----------------------------------------------------------
                '  3. 色标与标题
                ' -----------------------------------------------------------
                If showLegend Then
                    Call DrawLegend(g, plotRect, region.Size)
                End If

                Call Plot3DCanvas.DrawTitles(g, plotRect, theme, plotRect.Top)
            End Sub

            ''' <summary>计算色标（含刻度数字）所需要预留的宽度</summary>
            Private Function ColorBarReservedWidth(g As IGraphics) As Single
                Dim css As CSSEnvirnment = g.LoadEnvironment
                Dim bar As SizeF = theme.colorBarSize.FloatSizeParser
                Dim tickFont As Font = css.GetFont(CSSFont.TryParse(theme.tickCSS))
                Dim min As Double = std.Round(averages.Min, 1)
                Dim max As Double = std.Round(averages.Max, 1)
                Dim tickWidth As Single = 0

                For Each tick As Double In Geometry.NiceTicks(min, max, 5)
                    If tick < min OrElse tick > max Then
                        Continue For
                    End If

                    tickWidth = std.Max(tickWidth, g.MeasureString(AxisFrame.FormatTick(tick, theme.tickFormat), tickFont).Width)
                Next

                ' 额外预留一段空间给三维场景自身向外伸出的刻度值文本
                Return bar.Width + tickWidth + 56
            End Function

            ''' <summary>在绘图区右侧绘制连续色标（按画布尺寸钳制，不会越界）</summary>
            Private Sub DrawLegend(g As IGraphics, plotRect As Rectangle, canvas As Size)
                Dim css As CSSEnvirnment = g.LoadEnvironment
                Dim bar As SizeF = theme.colorBarSize.FloatSizeParser
                Dim tickFont As Font = css.GetFont(CSSFont.TryParse(theme.tickCSS))
                Dim min As Double = std.Round(averages.Min, 1)
                Dim max As Double = std.Round(averages.Max, 1)
                Dim tickWidth As Single = 0

                For Each tick As Double In Geometry.NiceTicks(min, max, 5)
                    If tick < min OrElse tick > max Then
                        Continue For
                    End If

                    tickWidth = std.Max(tickWidth, g.MeasureString(AxisFrame.FormatTick(tick, theme.tickFormat), tickFont).Width)
                Next

                Dim x As Single = plotRect.Right - bar.Width - tickWidth - 12
                Dim y As Single = plotRect.Top + (plotRect.Height - bar.Height) / 2

                x = std.Max(plotRect.Left + 2, std.Min(x, canvas.Width - bar.Width - tickWidth - 12))
                y = std.Max(plotRect.Top + 2, std.Min(y, canvas.Height - bar.Height - 8))

                Call Plot3DCanvas.DrawColorBar(g, x, y, bar.Width, bar.Height,
                                               stops, min, max, legendTitle, theme)
            End Sub
        End Structure

        ''' <summary>
        ''' 3D heatmap plot from matrix data
        ''' </summary>
        ''' <param name="matrix">二维表数据</param>
        ''' <param name="Camera">相机</param>
        ''' <param name="legendTitle">色标标题</param>
        ''' <param name="mapName">色阶名称</param>
        ''' <param name="mapLevels">色阶层级数</param>
        ''' <param name="bg">画布背景色</param>
        ''' <param name="axisFont">坐标轴字体 CSS</param>
        ''' <param name="legendFont">色标标题字体</param>
        ''' <param name="showLegend">是否绘制色标</param>
        ''' <param name="title">图表标题</param>
        ''' <param name="dpi">导出分辨率</param>
        ''' <param name="driver">绘图驱动</param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot(Of T As {INamedValue, DynamicPropertyBase(Of String)})(matrix As IEnumerable(Of T),
                             Camera As Camera,
                             Optional legendTitle$ = "3D scatter heatmap",
                             Optional mapName$ = "Spectral:c10",
                             Optional mapLevels% = 25,
                             Optional bg$ = "white",
                             Optional axisFont$ = CSSFont.Win10Normal,
                             Optional legendFont As Font = Nothing,
                             Optional showLegend As Boolean = True,
                             Optional title$ = "",
                             Optional dpi As Integer = 100,
                             Optional driver As Drivers = Drivers.Default) As GraphicsData

            Return matrix.Surface.ToArray _
                .Plot(
                    Camera, legendTitle,
                    mapName, mapLevels,
                    bg,
                    axisFont, legendFont, showLegend,
                    title, dpi:=dpi, driver:=driver)
        End Function
    End Module
End Namespace
