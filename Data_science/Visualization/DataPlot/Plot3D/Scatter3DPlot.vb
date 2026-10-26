#Region "Microsoft.VisualBasic::23b623c18be37d2d10b89b2b0c6b0324, Data_science\Visualization\DataPlot\Plot3D\Scatter3DPlot.vb"

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

    '   Total Lines: 298
    '    Code Lines: 213 (71.48%)
    ' Comment Lines: 53 (17.79%)
    '    - Xml Docs: 71.70%
    ' 
    '   Blank Lines: 32 (10.74%)
    '     File Size: 12.33 KB


    '     Class Scatter3D
    ' 
    '         Properties: xlabel, ylabel, zlabel
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Plot, populateModels
    ' 
    '         Sub: Plot, PlotInternal
    ' 
    '     Module Scatter3DPlot
    ' 
    '         Function: Plot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
'  Scatter3DPlot.vb - 3D 散点图
'
'  从旧 Plots 项目的 3D/Plot/Scatter3D.vb（Impl.Scatter3D : Plot）以及
'  3D/Scatter.vb（Module Scatter 包装器）合并迁移而来：
'   1. 去掉对旧绘图库 Plot 基类与 Graphic.Theme 的依赖，改用 Plot3DTheme；
'   2. 渲染输出改走 Microsoft.VisualBasic.Imaging.Drawing2D.g.GraphicsPlots，
'      从而支持 driver 参数（png / svg / postscript 等）；
'   3. 3D 场景元素（网格、坐标轴、凸包、点、标签）的建模与 Z 排序渲染逻辑
'      保持与旧版本一致。
' ============================================================================

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Plots.Plot3D.Device
Imports Microsoft.VisualBasic.Data.Plots.Plot3D.Legend
Imports Microsoft.VisualBasic.Data.Plots.Plot3D.Model
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports std = System.Math

Namespace Plot3D.Impl

    ''' <summary>
    ''' 3D 散点图渲染实现
    ''' </summary>
    ''' <remarks>
    ''' 首先要生成3维图表的模型元素，然后将这些元素混合在一起，最后按照Z深度的排序结果顺序绘制出来，
    ''' 才能够生成一幅有层次感的3维图表
    ''' </remarks>
    Public Class Scatter3D

        ReadOnly serials As Serial3D()
        ReadOnly camera As Camera
        ReadOnly arrowFactor As String
        ReadOnly showHull As Boolean
        ReadOnly hullAlpha As Double
        ReadOnly hullBspline As Single
        ReadOnly theme As Plot3DTheme

        Public Property xlabel As String = "X"
        Public Property ylabel As String = "Y"
        Public Property zlabel As String = "Z"

        ''' <summary>
        ''' 深度淡化强度（覆盖 <see cref="Plot3DTheme.depthFade"/>，仅在大于 0 时生效）：
        ''' 离视点越远的点越淡、越小，从而增强三维层次感。
        ''' </summary>
        Public Property depthFade As Double = -1

        ''' <summary>
        ''' </summary>
        ''' <param name="serials"></param>
        ''' <param name="camera"></param>
        ''' <param name="arrowFactor"></param>
        ''' <param name="showHull"></param>
        ''' <param name="hullAlpha">``[0, 255]``</param>
        ''' <param name="hullBspline"></param>
        ''' <param name="theme"></param>
        Public Sub New(serials As IEnumerable(Of Serial3D), camera As Camera, arrowFactor$,
                       showHull As Boolean,
                       hullAlpha As Double,
                       hullBspline As Single,
                       theme As Plot3DTheme)

            Me.serials = serials.ToArray
            Me.camera = camera
            Me.arrowFactor = arrowFactor
            Me.showHull = showHull
            Me.hullAlpha = hullAlpha
            Me.hullBspline = hullBspline
            Me.theme = theme
        End Sub

        Private Iterator Function populateModels(css As CSSEnvirnment) As IEnumerable(Of Element3D)
            Dim points As Point3D() = serials _
                .Select(Function(s) s.Points.Select(Function(pt) pt.Value)) _
                .IteratesALL _
                .ToArray

            ' 首先需要获取得到XYZ值的范围
            Dim X, Y, Z As Vector

            With points.VectorShadows
                X = DirectCast(.X, IEnumerable(Of Double)).Range.CreateAxisTicks
                Y = DirectCast(.Y, IEnumerable(Of Double)).Range.CreateAxisTicks
                Z = DirectCast(.Z, IEnumerable(Of Double)).Range.CreateAxisTicks
            End With

            ' ---------------------------------------------------------------
            '  出版级三维坐标系：盒框 → 背板网格 → 三轴刻度 → 主轴与轴名
            ' ---------------------------------------------------------------
            If theme.showBox Then
                For Each edge As Element3D In AxisFrame.Box(css, X, Y, Z, theme)
                    Yield edge
                Next
            End If

            If theme.showGrid Then
                For Each line As Element3D In AxisFrame.BackGrids(css, X, Y, Z, theme, camera)
                    Yield line
                Next
            End If

            If theme.showTicks Then
                For Each tick As Element3D In AxisFrame.Ticks(css, X, Y, Z, theme)
                    Yield tick
                Next
            End If

            If theme.showAxisLines Then
                For Each item As Element3D In AxisDraw.Axis(
                        css,
                        xrange:=X, yrange:=Y, zrange:=Z,
                        labelFontCss:=theme.axisLabelCSS,
                        labels:=(xlabel, ylabel, zlabel),
                        strokeCSS:=theme.axisStroke,
                        arrowFactor:=arrowFactor,
                        labelColorVal:=theme.mainTextColor
                    )

                    Yield item
                Next
            End If

            ' 最后混合进入系列点
            Dim fade As Double = If(depthFade >= 0, depthFade, theme.depthFade)
            Dim depth As Func(Of Point3D, Double) = Nothing

            If fade > 0 Then
                depth = DepthNormalize(points, camera)
            End If

            For Each serial As Serial3D In serials
                Dim data As NamedValue(Of Point3D)() = serial.Points

                If showHull Then
                    Yield New ConvexHullPolygon With {
                        .Brush = New SolidBrush(serial.Color.Alpha(hullAlpha)),
                        .Path = data _
                            .Select(Function(pt) pt.Value) _
                            .ToArray,
                        .bspline = hullBspline
                    }
                End If

                For Each pt As NamedValue(Of Point3D) In data
                    Dim size As New Size With {
                        .Width = serial.PointSize,
                        .Height = serial.PointSize
                    }
                    Dim color As Brush = New SolidBrush(serial.Color)

                    If depth IsNot Nothing Then
                        ' 离视点越远（归一化深度越接近 1）越淡、越小
                        Dim t As Double = depth(pt.Value)

                        color = New SolidBrush(Color.FromArgb(
                            CInt(std.Max(24, 255 * (1 - fade * t))), serial.Color))
                        size = New Size With {
                            .Width = CSng(serial.PointSize * (1 - fade * 0.35 * t)),
                            .Height = CSng(serial.PointSize * (1 - fade * 0.35 * t))
                        }
                    End If

                    Yield New ShapePoint With {
                        .Fill = color,
                        .Location = pt.Value,
                        .Size = size,
                        .Style = serial.Shape,
                        .Label = pt.Name
                    }
                Next
            Next
        End Function

        ''' <summary>
        ''' 预先计算所有数据点投影之后的归一化视图深度：0 表示最近，1 表示最远。
        ''' </summary>
        Private Shared Function DepthNormalize(points As Point3D(), camera As Camera) As Func(Of Point3D, Double)
            Dim depths As New Dictionary(Of Point3D, Double)
            Dim dmin As Double = Double.MaxValue
            Dim dmax As Double = Double.MinValue

            For Each pt As Point3D In points
                ' 投影之后 Z 越大表示离视点越远
                Dim z As Double = camera.Project(camera.Rotate(pt)).Z

                depths(pt) = z

                If z < dmin Then dmin = z
                If z > dmax Then dmax = z
            Next

            Dim span As Double = dmax - dmin

            Return Function(pt As Point3D) As Double
                       Dim z As Double

                       If Not depths.TryGetValue(pt, z) Then
                           Return 0
                       End If
                       If span <= 0 Then
                           Return 0
                       End If

                       Return std.Max(0, std.Min(1, (z - dmin) / span))
                   End Function
        End Function

        Public Function Plot(Optional size$ = Nothing,
                             Optional dpi As Integer = 100,
                             Optional driver As Drivers = Drivers.Default) As GraphicsData

            Dim sz As Size = If(size.StringEmpty, camera.screen, size.SizeParser)

            Return g.GraphicsPlots(
                size:=sz,
                padding:=theme.padding,
                bg:=theme.background,
                plotAPI:=AddressOf PlotInternal,
                driver:=driver,
                dpi:=dpi
            )
        End Function

        ''' <summary>
        ''' 渲染到已有的宿主画布（图层式共享画布，等价于旧引擎基类 Plot 的
        ''' Plot(g, layout) 入口），供 Embedding3D 之类的多图层宿主使用。
        ''' </summary>
        ''' <param name="g">宿主画布（所有权归调用方，本方法不负责释放）</param>
        ''' <param name="layout">绘图布局区域</param>
        Public Sub Plot(ByRef g As IGraphics, layout As Rectangle)
            Dim padding As New Padding With {
                .Left = layout.Left,
                .Top = layout.Top,
                .Bottom = g.Height - layout.Bottom,
                .Right = g.Width - layout.Right
            }
            Dim canvas As New GraphicsRegion(New Size(g.Width, g.Height), padding)

            Call PlotInternal(g, canvas)
        End Sub

        Private Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            If theme.antiAlias Then
                Call Plot3DCanvas.ApplyAntiAlias(g)
            End If

            Dim legends As LegendObject() = serials _
                .Select(Function(s)
                            Return New LegendObject With {
                                .color = s.Color.RGBExpression,
                                .fontstyle = theme.axisLabelCSS,
                                .style = s.Shape,
                                .title = s.Title
                            }
                        End Function) _
                .ToArray
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim region As Rectangle = canvas.PlotRegion(css)

            ' 要先绘制三维图形，要不然会将图例遮住的
            Call populateModels(css).RenderAs3DChart(
                canvas:=g,
                camera:=camera,
                region:=canvas,
                theme:=theme
            )

            ' 标题与副标题
            Call Plot3DCanvas.DrawTitles(g, region, theme)

            ' 图例：按实测尺寸排版，并钳制在画布之内
            If theme.drawLegend Then
                Call Plot3DCanvas.DrawLegendBlock(g, region, canvas.Size, legends, theme)
            End If
        End Sub
    End Class
End Namespace

Namespace Plot3D

    ''' <summary>
    ''' 3D scatter charting
    ''' </summary>
    Public Module Scatter3DPlot

        ''' <summary>
        ''' plot scatter 3D
        ''' </summary>
        ''' <param name="serials">三维散点系列</param>
        ''' <param name="camera">相机</param>
        ''' <param name="bg$">画布背景色</param>
        ''' <param name="padding$">页边距 CSS</param>
        ''' <param name="axisLabelFontCSS$">轴标题字体</param>
        ''' <param name="elementLabelFont$">数据点标签字体</param>
        ''' <param name="boxStroke$">图例外框描边</param>
        ''' <param name="axisStroke$">坐标轴线描边</param>
        ''' <param name="labX$">X 轴标题</param>
        ''' <param name="labY$">Y 轴标题</param>
        ''' <param name="labZ$">Z 轴标题</param>
        ''' <param name="arrowFactor$">主轴箭头尺寸</param>
        ''' <param name="showLegend">是否绘制图例</param>
        ''' <param name="showHull">show convex hull for each <paramref name="serials"/> data.</param>
        ''' <param name="hullAlpha">凸包透明度 ``[0,255]``</param>
        ''' <param name="hullBspline">凸包平滑阶数</param>
        ''' <param name="driver">绘图驱动</param>
        ''' <param name="title$">图表标题</param>
        ''' <param name="subTitle$">图表副标题</param>
        ''' <param name="dpi">导出分辨率</param>
        ''' <param name="showBox">是否绘制三维盒框</param>
        ''' <param name="showGrid">是否绘制背板网格</param>
        ''' <param name="showTicks">是否绘制三轴刻度与刻度值</param>
        ''' <param name="depthFade">深度淡化强度 ``[0,1]``，0 表示关闭</param>
        ''' <param name="antiAlias">是否尝试开启抗锯齿</param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot(serials As IEnumerable(Of Serial3D),
                             camera As Camera,
                             Optional bg$ = "white",
                             Optional padding$ = g.DefaultPadding,
                             Optional axisLabelFontCSS$ = CSSFont.Win7Normal,
                             Optional elementLabelFont$ = CSSFont.Win10Normal,
                             Optional boxStroke$ = Stroke.StrongHighlightStroke,
                             Optional axisStroke$ = Stroke.AxisStroke,
                             Optional labX$ = "X",
                             Optional labY$ = "Y",
                             Optional labZ$ = "Z",
                             Optional arrowFactor$ = "2,2",
                             Optional showLegend As Boolean = True,
                             Optional showHull As Boolean = True,
                             Optional hullAlpha As Integer = 150,
                             Optional hullBspline As Single = 2,
                             Optional driver As Drivers = Drivers.Default,
                             Optional title$ = "",
                             Optional subTitle$ = "",
                             Optional dpi As Integer = 100,
                             Optional showBox As Boolean = True,
                             Optional showGrid As Boolean = True,
                             Optional showTicks As Boolean = True,
                             Optional depthFade As Double = 0,
                             Optional antiAlias As Boolean = True) As GraphicsData

            Dim theme As New Plot3DTheme With {
                .padding = padding,
                .background = bg,
                .axisStroke = axisStroke,
                .axisLabelCSS = axisLabelFontCSS,
                .tagCSS = elementLabelFont,
                .drawLegend = showLegend,
                .legendBoxStroke = boxStroke,
                .title = title,
                .subTitle = subTitle,
                .showBox = showBox,
                .showGrid = showGrid,
                .showTicks = showTicks,
                .depthFade = depthFade,
                .antiAlias = antiAlias
            }

            Return New Impl.Scatter3D(
                serials:=serials,
                camera:=camera,
                arrowFactor:=arrowFactor,
                showHull:=showHull,
                hullAlpha:=hullAlpha,
                hullBspline:=hullBspline,
                theme:=theme
            ) With {
                .xlabel = labX,
                .ylabel = labY,
                .zlabel = labZ
            }.Plot(size:=$"{camera.screen.Width},{camera.screen.Height}", dpi:=dpi, driver:=driver)
        End Function
    End Module
End Namespace
