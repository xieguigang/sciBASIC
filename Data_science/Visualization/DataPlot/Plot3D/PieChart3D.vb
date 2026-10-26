#Region "Microsoft.VisualBasic::fbddcc382215cc496d17fca1972b7bb4, Data_science\Visualization\DataPlot\Plot3D\PieChart3D.vb"

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

    '   Total Lines: 123
    '    Code Lines: 83 (67.48%)
    ' Comment Lines: 25 (20.33%)
    '    - Xml Docs: 60.00%
    ' 
    '   Blank Lines: 15 (12.20%)
    '     File Size: 5.05 KB


    '     Class PieSlice
    ' 
    '         Properties: Color, Name, Percentage, Value
    ' 
    '         Function: AsSlices, ToString
    ' 
    '     Module PieChart3D
    ' 
    '         Function: Plot3D
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
'  PieChart3D.vb - 3D 饼图
'
'  从旧 Plots 项目的 3D/PieChart3D.vb 迁移而来：
'   1. 去掉对旧绘图库 ChartPlots.Fractions.FractionData 的依赖，
'      改用本文件内的 PieSlice 数据模型；
'   2. 3D 模型元素（Pie）仍然来自 imaging 项目的 Drawing3D 等距投影模型；
'   3. 渲染输出走 g.GraphicsPlots，支持 driver 参数。
' ============================================================================

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Plots.Plot3D.Device
Imports Microsoft.VisualBasic.Data.Plots.Plot3D.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models.Isometric.Shapes
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Matrix = Microsoft.VisualBasic.Imaging.Drawing3D.Math3D.Matrix
Imports std = System.Math

Namespace Plot3D

    ''' <summary>
    ''' 3D 饼图的数据切片
    ''' </summary>
    Public Class PieSlice

        Public Property Name As String
        Public Property Value As Double
        Public Property Color As Color

        ''' <summary>当前切片所占的百分比（由 <see cref="AsSlices"/> 计算填充）</summary>
        Public Property Percentage As Single

        Public Overrides Function ToString() As String
            Return $"[{Name}, {Percentage.ToString("F2")}%]"
        End Function

        ''' <summary>将一组名称-数值对转换为带有百分比信息的饼图切片</summary>
        Public Shared Function AsSlices(data As IEnumerable(Of NamedValue(Of Double)), colors As IEnumerable(Of Color)) As PieSlice()
            Dim raw As NamedValue(Of Double)() = data.ToArray
            Dim total As Double = raw.Select(Function(a) a.Value).Sum
            Dim c As Color() = colors.ToArray
            Dim slices As PieSlice() = raw _
                .SeqIterator _
                .Select(Function(a)
                            Return New PieSlice With {
                                .Name = a.value.Name,
                                .Value = a.value.Value,
                                .Color = c(a.i Mod c.Length),
                                .Percentage = If(total = 0, 0, CSng(a.value.Value / total * 100))
                            }
                        End Function) _
                .ToArray

            Return slices
        End Function

        ''' <summary>
        ''' 按 <see cref="Value"/> 重新计算 <see cref="Percentage"/>。
        '''
        ''' <para>
        ''' 调用方（例如 R# 的 <c>pie()</c>）经常只给出 <see cref="Value"/> 而不填百分比，
        ''' 旧实现在这种情况下会把所有切片的张角都算成 0，导致整张饼图画不出来。
        ''' </para>
        ''' </summary>
        ''' <param name="slices">原始切片</param>
        ''' <returns>百分比字段已经填好的切片数组（百分比为 0 或者无效的切片会被剔除）</returns>
        Public Shared Function WithPercentages(slices As IEnumerable(Of PieSlice)) As PieSlice()
            Dim raw As PieSlice() = slices.SafeQuery.ToArray
            Dim known As Double = raw.Select(Function(s) CDbl(s.Percentage)).Sum
            Dim total As Double = raw.Select(Function(s) s.Value).Sum

            If known <= 0 OrElse known > 100.0001 Then
                ' 没有给出百分比、或者百分比本身就不自洽：按数值重新计算
                If total <= 0 Then
                    Return raw
                End If

                For Each slice As PieSlice In raw
                    slice.Percentage = CSng(slice.Value / total * 100)
                Next
            End If

            Return raw _
                .Where(Function(s) s.Percentage > 0) _
                .ToArray
        End Function
    End Class

    ''' <summary>
    ''' 3D pie chart
    ''' </summary>
    Public Module PieChart3D

        ''' <summary>
        ''' render the 3d pie chart
        '''
        ''' <para>
        ''' 与旧版本相比，这里补齐了出版级插图所必需的元素：
        ''' 切片文本标签（名称 + 百分比）、分类图例、标题、可配置的饼体厚度与圆弧细分数、
        ''' 可选的切片分离，以及背景 / 页边距 / dpi / 驱动参数。
        ''' </para>
        ''' </summary>
        ''' <param name="data">饼图切片（未填写 <see cref="PieSlice.Percentage"/> 时会按 <see cref="PieSlice.Value"/> 自动计算）</param>
        ''' <param name="camera">相机</param>
        ''' <param name="driver">绘图驱动</param>
        ''' <param name="title$">图表标题</param>
        ''' <param name="showLegend">是否绘制分类图例</param>
        ''' <param name="showLabels">是否在切片上绘制标签</param>
        ''' <param name="bg$">画布背景色</param>
        ''' <param name="padding$">页边距 CSS</param>
        ''' <param name="thickness">饼体厚度（相对半径的比例）</param>
        ''' <param name="vertices">圆弧细分数，越大越平滑</param>
        ''' <param name="explode">切片分离强度（相对半径的比例，0 表示不分离）</param>
        ''' <param name="labelFont$">切片标签字体</param>
        ''' <param name="legendFont$">图例字体</param>
        ''' <param name="dpi">导出分辨率</param>
        ''' <param name="illumination">
        ''' 是否启用光照着色。开启后面片颜色会随法向与光源方向的夹角变化；
        ''' 缺省关闭，使切片颜色与图例完全一致（论文插图的常规做法）。
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot3D(data As IEnumerable(Of PieSlice),
                               camera As Camera,
                               Optional driver As Drivers = Drivers.Default,
                               Optional title$ = "",
                               Optional showLegend As Boolean = True,
                               Optional showLabels As Boolean = True,
                               Optional bg$ = "white",
                               Optional padding$ = g.DefaultPadding,
                               Optional thickness! = 0.24!,
                               Optional vertices% = 96,
                               Optional explode! = 0!,
                               Optional labelFont$ = CSSFont.Win7Normal,
                               Optional legendFont$ = CSSFont.Win7Normal,
                               Optional dpi As Integer = 100,
                               Optional illumination As Boolean = False) As GraphicsData

            Dim slices As PieSlice() = PieSlice.WithPercentages(data)
            Dim theme As New Plot3DTheme With {
                .padding = padding,
                .background = bg,
                .title = title,
                .axisLabelCSS = legendFont,
                .tickCSS = labelFont,
                .drawLegend = showLegend
            }

            If slices.IsNullOrEmpty Then
                ' 没有任何有效切片时也要给出一张带标题的空画布
                Return g.GraphicsPlots(
                    camera.screen, padding, bg,
                    Sub(ByRef canvas As IGraphics, region As GraphicsRegion)
                        Call Plot3DCanvas.DrawTitles(canvas, region.PlotRegion(canvas.LoadEnvironment), theme)
                    End Sub,
                    driver:=driver, dpi:=dpi)
            End If

            Dim view As Camera = WorkingCamera(camera)
            Dim centra As New Point3D(view.screen.Width / 2, view.screen.Height / 2, 0)
            Dim pieChart As New List(Of Surface)
            Dim topFace As New List(Of Integer)
            Dim start As Double = 0
            ' 切片被推出去 e 之后半径同步缩小 e，这样每一片的外弧中点仍然落在半径为 1 的
            ' 基准圆上，分离之后外缘不会出现"台阶"
            Dim radius As Double = 1 - explode

            If radius < 0.25 Then
                radius = 0.25
            End If

            For Each x As PieSlice In slices
                Dim sweep As Double = 360 * x.Percentage / 100
                Dim mid As Double = start + sweep / 2
                Dim origin As Point3D = centra

                If explode > 0 Then
                    ' 沿切片角平分线把切片向外推出去
                    Dim rad As Double = mid / 180 * std.PI

                    origin = New Point3D(
                        centra.X + std.Cos(rad) * explode,
                        centra.Y + std.Sin(rad) * explode,
                        centra.Z)
                End If

                Dim pie As New Pie(origin, radius, start, sweep, vertices, thickness)
                Dim model As Surface() = pie.Model3D(x.Color)

                ' Extrude 的面序是固定的：0 = 底面、1 = 顶面、其余 = 侧面。
                ' 顶面用基色、侧面压暗、底面最暗，厚度才有可辨识的明暗层次，
                ' 否则侧面会以"随机的细色条"形式出现，看起来像是切片错位。
                For i As Integer = 0 To model.Length - 1
                    Dim factor As Single = If(i = 0, BottomShade, If(i = 1, 1.0F, SideShade))

                    model(i).brush = New SolidBrush(ShadeColor(x.Color, factor))
                Next

                Dim top As Integer = TopFaceIndex(model)

                topFace += pieChart.Count + top
                pieChart += model

                start += sweep
            Next

            Dim centroid As Point3D = pieChart.Centroid
            Dim matrix As New Matrix(centroid.Offsets(pieChart))
            Dim vector = view.Rotate(matrix.Matrix)
            Dim legends As LegendObject() = slices _
                .Select(Function(s)
                            Return New LegendObject With {
                                .color = s.Color.RGBExpression,
                                .fontstyle = legendFont,
                                .style = LegendStyles.Circle,
                                .title = s.Name
                            }
                        End Function) _
                .ToArray

            Dim plot3DInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Dim css As CSSEnvirnment = g.LoadEnvironment
                    Dim plotRect As Rectangle = region.PlotRegion(css)

                    If theme.antiAlias Then
                        Call Plot3DCanvas.ApplyAntiAlias(g)
                    End If

                    ' 先测量图例，以便把饼图整体往左挪一点，避免与图例重叠
                    Dim legendBox As SizeF = If(showLegend,
                        Plot3DCanvas.MeasureLegendBox(g, legends, theme),
                        New SizeF(0, 0))

                    view.Offset = New PointF(camera.Offset.X - CSng(legendBox.Width / 2), camera.Offset.Y)

                    Dim model2D = matrix _
                        .TranslateBuffer(view, view.Rotate(matrix.Matrix), illumination) _
                        .AsList

                    ' 逐面填充并用自己的颜色描一圈 1px 的边：
                    ' 相邻面片之间的抗锯齿接缝会被这条描边盖掉，否则浅色切片上会出现白色细线
                    For Each face In model2D
                        Using br As Brush = face.brush
                            Call g.FillPolygon(br, face.points)
                        End Using

                        Dim solid = TryCast(face.brush, SolidBrush)

                        If solid IsNot Nothing Then
                            Using pen As New Pen(solid.Color, 1.0F)
                                Call g.DrawPolygon(pen, face.points)
                            End Using
                        End If
                    Next

                    If showLabels Then
                        Call DrawSliceLabels(g, view, centroid, pieChart, topFace, slices, theme, plotRect)
                    End If

                    Call Plot3DCanvas.DrawTitles(g, plotRect, theme)

                    If showLegend Then
                        Call Plot3DCanvas.DrawLegendBlock(g, plotRect, region.Size, legends, theme)
                    End If
                End Sub

            Return g.GraphicsPlots(
                camera.screen,
                padding,
                bg$,
                plot3DInternal,
                driver:=driver,
                dpi:=dpi)
        End Function

        ''' <summary>
        ''' 直接以二维饼图的分片数据（<see cref="Canvas.FractionData"/>）绘制三维饼图。
        '''
        ''' <para>
        ''' 这是一个便捷重载：旧调用方（例如 GCModeller 的 KOBAS 富集分析插画）
        ''' 手里拿到的是 <c>FractionData()</c> 而不是 <see cref="PieSlice"/>，
        ''' 这里直接完成类型转换并转发给主入口，避免调用方自己拼装切片。
        ''' </para>
        ''' </summary>
        ''' <param name="fractions">二维饼图分片数据</param>
        ''' <param name="camera">相机</param>
        ''' <param name="driver">绘图驱动</param>
        ''' <param name="title">图表标题</param>
        ''' <param name="showLegend">是否绘制分类图例</param>
        ''' <param name="showLabels">是否在切片上绘制标签</param>
        ''' <param name="bg">画布背景色</param>
        ''' <param name="padding">页边距 CSS</param>
        ''' <param name="thickness">饼体厚度（相对半径的比例）</param>
        ''' <param name="vertices">圆弧细分数</param>
        ''' <param name="explode">切片分离强度</param>
        ''' <param name="dpi">导出分辨率</param>
        ''' <param name="illumination">是否启用光照着色</param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot3D(fractions As IEnumerable(Of Canvas.FractionData),
                               camera As Camera,
                               Optional driver As Drivers = Drivers.Default,
                               Optional title$ = "",
                               Optional showLegend As Boolean = True,
                               Optional showLabels As Boolean = True,
                               Optional bg$ = "white",
                               Optional padding$ = g.DefaultPadding,
                               Optional thickness! = 0.24!,
                               Optional vertices% = 96,
                               Optional explode! = 0!,
                               Optional dpi As Integer = 100,
                               Optional illumination As Boolean = False) As GraphicsData

            Dim slices As PieSlice() = fractions _
                .SafeQuery _
                .Select(Function(f)
                            Return New PieSlice With {
                                .Name = f.Name,
                                .Value = f.Value,
                                .Color = f.Color,
                                .Percentage = CSng(f.Percentage)
                            }
                        End Function) _
                .ToArray

            Return slices.Plot3D(
                camera:=camera,
                driver:=driver,
                title:=title,
                showLegend:=showLegend,
                showLabels:=showLabels,
                bg:=bg,
                padding:=padding,
                thickness:=thickness,
                vertices:=vertices,
                explode:=explode,
                dpi:=dpi,
                illumination:=illumination)
        End Function

        ''' <summary>
        ''' 构造绘制用的相机副本：把缺省的视距补成一个可用值，
        ''' 并按画布大小换算出能够让半径为 1 的饼图铺满目标区域的视场角。
        '''
        ''' <para>
        ''' 旧实现直接使用调用方传入的相机：R# 的 <c>pie()</c> 传的是
        ''' <c>ViewDistance = 10000</c> 而模型半径只有 2，投影之后整张饼图只有约 0.05 像素，
        ''' 实际上什么都看不见——这里按"目标半径"反算视场角，从根本上解决缩放问题。
        ''' </para>
        ''' </summary>
        Private Function WorkingCamera(camera As Camera) As Camera
            Dim distance! = camera.ViewDistance

            If distance = 0 OrElse std.Abs(distance) < 1 Then
                distance = 1000
            End If

            distance = std.Abs(distance)

            Dim view As New Camera(New Point3D(camera.AngleX, camera.AngleY, camera.AngleZ)) With {
                .Screen = camera.screen,
                .Offset = camera.Offset,
                .ViewDistance = distance,
                .LightDirection = camera.LightDirection,
                .AmbientStrength = camera.AmbientStrength,
                .LightColor = camera.LightColor
            }
            Dim radiusPx! = 0.32! * std.Min(view.screen.Width, view.screen.Height)

            ' factor = fov / (distance + z)，模型以质心为原点故 z≈0，半径取 1
            view.FieldOfView = CSng(radiusPx * distance)

            Return view
        End Function

        ''' <summary>
        ''' 找出切片顶面（Z 最大的那个面）在 <see cref="Surface"/> 数组之中的下标，
        ''' 顶面的投影中心就是切片标签的锚点。
        ''' </summary>
        Private Function TopFaceIndex(model As Surface()) As Integer
            Dim best As Integer = 0
            Dim maxZ As Double = Double.MinValue

            For i As Integer = 0 To model.Length - 1
                Dim z As Double = model(i).vertices.Average(Function(p) p.Z)

                If z > maxZ Then
                    maxZ = z
                    best = i
                End If
            Next

            Return best
        End Function

        ''' <summary>饼图侧面的明度系数（相对顶面基色）</summary>
        Private Const SideShade As Single = 0.84F

        ''' <summary>饼图底面的明度系数（相对顶面基色）</summary>
        Private Const BottomShade As Single = 0.72F

        ''' <summary>按比例调整颜色的明度（保持色相与透明度不变）</summary>
        ''' <param name="color">基色</param>
        ''' <param name="factor">明度系数，小于 1 表示压暗</param>
        Private Function ShadeColor(color As Color, factor As Single) As Color
            Dim r% = CInt(std.Max(0, std.Min(255, color.R * factor)))
            Dim g% = CInt(std.Max(0, std.Min(255, color.G * factor)))
            Dim b% = CInt(std.Max(0, std.Min(255, color.B * factor)))

            Return Color.FromArgb(color.A, r, g, b)
        End Function

        ''' <summary>文字光晕的四个偏移方向</summary>
        Private ReadOnly HaloOffsets As PointF() = {
            New PointF(-1.2F, 0), New PointF(1.2F, 0),
            New PointF(0, -1.2F), New PointF(0, 1.2F)
        }

        ''' <summary>
        ''' 在每一个切片的顶面中心绘制"名称 + 百分比"标签
        ''' </summary>
        Private Sub DrawSliceLabels(g As IGraphics, view As Camera, centroid As Point3D,
                                    model As List(Of Surface), topFace As List(Of Integer),
                                    slices As PieSlice(), theme As Plot3DTheme, plotRect As Rectangle)
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim font As Font = css.GetFont(CSSFont.TryParse(theme.tickCSS))

            For i As Integer = 0 To topFace.Count - 1
                Dim face As Surface = model(topFace(i))
                Dim anchor As New Point3D(
                    face.vertices.Average(Function(p) p.X),
                    face.vertices.Average(Function(p) p.Y),
                    face.vertices.Average(Function(p) p.Z))
                ' 与 BufferPainting 完全相同的变换：减去质心 → 旋转 → 投影 → 取屏幕坐标
                Dim rel As Point3D = New Point3D(anchor.X - centroid.X, anchor.Y - centroid.Y, anchor.Z - centroid.Z)
                Dim pt As PointF = view _
                    .Project(view.Rotate(rel)) _
                    .PointXY(view.screen)
                Dim text As String = $"{slices(i).Name} {slices(i).Percentage.ToString("F1")}%"
                Dim size As SizeF = g.MeasureString(text, font)
                Dim x! = pt.X - size.Width / 2
                Dim y! = pt.Y - size.Height / 2

                x = std.Max(plotRect.Left, std.Min(x, plotRect.Right - size.Width))
                y = std.Max(plotRect.Top, std.Min(y, plotRect.Bottom - size.Height))

                ' 先描一圈白色光晕再写字：标签往往会跨出切片边界，
                ' 单色文字落在背景上会"消失"（白字落白底 / 黑字落深色切片）
                Using halo As New SolidBrush(Color.FromArgb(210, Color.White))
                    For Each offset As PointF In HaloOffsets
                        Call g.DrawString(text, font, halo, x + offset.X, y + offset.Y)
                    Next
                End Using

                Using br As New SolidBrush(Color.Black)
                    Call g.DrawString(text, font, br, x, y)
                End Using
            Next
        End Sub
    End Module
End Namespace
