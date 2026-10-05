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

#If NET48 Then
Imports Font = System.Drawing.Font
Imports SolidBrush = System.Drawing.SolidBrush
#Else
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
#End If

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


            ' 然后生成底部的网格
            For Each line As Element3D In Grids.Grid1(css, X, Y, (X(1) - X(0), Y(1) - Y(0)), Z.Min)
                Yield line
            Next

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

            ' 最后混合进入系列点
            For Each serial As Serial3D In serials
                Dim data As NamedValue(Of Point3D)() = serial.Points
                Dim size As New Size With {
                    .Width = serial.PointSize,
                    .Height = serial.PointSize
                }
                Dim color As New SolidBrush(serial.Color)

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

        Private Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
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
            Dim font As Font = css.GetFont(CSSFont.TryParse(theme.axisLabelCSS))
            Dim region As Rectangle = canvas.PlotRegion(css)

            Dim legendHeight! = (legends.Length * (font.Height + 5))
            Dim legendTop! = (region.Height - legendHeight) / 2
            Dim maxLegendLabelSize As SizeF = legends.Select(Function(s) s.title) _
                .MaxLengthString _
                .MeasureSize(g, font)
            Dim legendWidth = maxLegendLabelSize.Height * 1.125
            Dim legendLeft! = region.Right - maxLegendLabelSize.Width - legendWidth
            Dim topLeft As New Point With {
                .X = legendLeft,
                .Y = legendTop
            }
            Dim legendSize$ = $"{legendWidth},{legendWidth}"

            ' 要先绘制三维图形，要不然会将图例遮住的
            Call populateModels(css).RenderAs3DChart(
                canvas:=g,
                camera:=camera,
                region:=canvas,
                theme:=theme
            )

            If theme.drawLegend Then
                Call g.DrawLegends(
                    topLeft:=topLeft,
                    legends:=legends,
                    gSize:=legendSize,
                    d:=5,
                    regionBorder:=Stroke.AxisStroke
                )
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
        ''' <param name="serials"></param>
        ''' <param name="camera"></param>
        ''' <param name="bg$"></param>
        ''' <param name="padding$"></param>
        ''' <param name="axisLabelFontCSS$"></param>
        ''' <param name="elementLabelFont$"></param>
        ''' <param name="boxStroke$"></param>
        ''' <param name="axisStroke$"></param>
        ''' <param name="showHull">show convex hull for each <paramref name="serials"/> data.</param>
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
                             Optional driver As Drivers = Drivers.Default) As GraphicsData

            Dim theme As New Plot3DTheme With {
                .padding = padding,
                .background = bg,
                .axisStroke = axisStroke,
                .axisLabelCSS = axisLabelFontCSS,
                .tagCSS = elementLabelFont,
                .drawLegend = showLegend,
                .legendBoxStroke = boxStroke
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
            }.Plot(size:=$"{camera.screen.Width},{camera.screen.Height}", driver:=driver)
        End Function
    End Module
End Namespace