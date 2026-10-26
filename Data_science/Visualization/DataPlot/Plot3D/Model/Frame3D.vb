' ============================================================================
'  Frame3D.vb - 出版级三维坐标系（盒框棱线 / 背板网格 / 三轴刻度与刻度值）
'
'  旧实现只有"底面网格 + 三条带箭头的主轴 + 三个轴名"，
'  既没有 Z 轴刻度，也没有盒框，无法满足论文插图的要求。
'
'  这里把坐标系拆成三组可独立开关的图元，全部以 Element3D 的形式产出，
'  交给既有的 RenderAs3DChart 管线统一做投影、Z 排序与绘制：
'    1. Box       —— 12 条棱，勾出数据范围；
'    2. BackGrids —— 按相机朝向挑选"朝向背面"的三个面画网格，不会遮挡数据；
'    3. Ticks     —— 三条主轴上的刻度短线与刻度值文本。
' ============================================================================

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.Plots.Plot3D.Device
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports std = System.Math

Namespace Plot3D.Model

    ''' <summary>
    ''' 生成出版级三维坐标系所需的绘图元素（盒框、背板网格、刻度）
    ''' </summary>
    Public Module AxisFrame

        ''' <summary>一个候选的盒面（用于按相机朝向挑选背板）</summary>
        Private Structure WallFace
            Public Property depth As Double
            Public Property lines As Point3D()()

            ''' <summary>面中心点（模型空间）</summary>
            Public Property center As Point3D
        End Structure

        ''' <summary>
        ''' 由三轴数据范围生成三维盒框的 12 条棱线
        ''' </summary>
        ''' <param name="css">画布 CSS 环境（用于解析描边）</param>
        ''' <param name="xrange">X 轴范围</param>
        ''' <param name="yrange">Y 轴范围</param>
        ''' <param name="zrange">Z 轴范围</param>
        ''' <param name="theme">三维绘图主题</param>
        ''' <returns>12 条棱线元素</returns>
        Public Iterator Function Box(css As CSSEnvirnment,
                                     xrange As DoubleRange,
                                     yrange As DoubleRange,
                                     zrange As DoubleRange,
                                     theme As Plot3DTheme) As IEnumerable(Of Element3D)

            Dim pen As Pen = css.GetPen(Stroke.TryParse(theme.boxStroke))
            Dim x0# = xrange.Min, x1# = xrange.Max
            Dim y0# = yrange.Min, y1# = yrange.Max
            Dim z0# = zrange.Min, z1# = zrange.Max
            Dim corners As Point3D() = {
                New Point3D(x0, y0, z0), New Point3D(x1, y0, z0),
                New Point3D(x1, y1, z0), New Point3D(x0, y1, z0),
                New Point3D(x0, y0, z1), New Point3D(x1, y0, z1),
                New Point3D(x1, y1, z1), New Point3D(x0, y1, z1)
            }
            ' 立方体的 12 条棱
            Dim edges As Integer(,) = {
                {0, 1}, {1, 2}, {2, 3}, {3, 0},
                {4, 5}, {5, 6}, {6, 7}, {7, 4},
                {0, 4}, {1, 5}, {2, 6}, {3, 7}
            }

            For i As Integer = 0 To edges.GetLength(0) - 1
                Yield New Line(corners(edges(i, 0)), corners(edges(i, 1))) With {
                    .Stroke = pen
                }
            Next
        End Function

        ''' <summary>
        ''' 按相机朝向挑选"朝向背面"的三个盒面并绘制网格线。
        '''
        ''' <para>
        ''' 投影之后 Z 越大表示离视点越远（<c>PainterAlgorithm</c> 的约定），
        ''' 因此取深度最大的三个面即为背面，这样网格永远落在数据点的后面，不会遮挡。
        ''' </para>
        ''' </summary>
        ''' <param name="css">画布 CSS 环境</param>
        ''' <param name="xrange">X 轴范围</param>
        ''' <param name="yrange">Y 轴范围</param>
        ''' <param name="zrange">Z 轴范围</param>
        ''' <param name="theme">三维绘图主题</param>
        ''' <param name="camera">相机（用于判断朝向）</param>
        ''' <param name="tickCount">每个方向上期望的刻度/网格线条数</param>
        ''' <returns>背板网格线元素</returns>
        Public Iterator Function BackGrids(css As CSSEnvirnment,
                                           xrange As DoubleRange,
                                           yrange As DoubleRange,
                                           zrange As DoubleRange,
                                           theme As Plot3DTheme,
                                           camera As Camera,
                                           Optional tickCount As Integer = 6) As IEnumerable(Of Element3D)

            Dim pen As Pen = css.GetPen(Stroke.TryParse(theme.gridStroke))
            Dim x0# = xrange.Min, x1# = xrange.Max
            Dim y0# = yrange.Min, y1# = yrange.Max
            Dim z0# = zrange.Min, z1# = zrange.Max
            Dim xticks As Double() = InRangeTicks(xrange, tickCount)
            Dim yticks As Double() = InRangeTicks(yrange, tickCount)
            Dim zticks As Double() = InRangeTicks(zrange, tickCount)
            Dim faces As New List(Of WallFace)

            Call faces.Add(New WallFace With {.center = New Point3D((x0 + x1) / 2, (y0 + y1) / 2, z0), .lines = XYPlaneLines(xticks, yticks, y0, y1, x0, x1, z0)})
            Call faces.Add(New WallFace With {.center = New Point3D((x0 + x1) / 2, (y0 + y1) / 2, z1), .lines = XYPlaneLines(xticks, yticks, y0, y1, x0, x1, z1)})
            Call faces.Add(New WallFace With {.center = New Point3D((x0 + x1) / 2, y0, (z0 + z1) / 2), .lines = XZPlaneLines(xticks, zticks, z0, z1, x0, x1, y0)})
            Call faces.Add(New WallFace With {.center = New Point3D((x0 + x1) / 2, y1, (z0 + z1) / 2), .lines = XZPlaneLines(xticks, zticks, z0, z1, x0, x1, y1)})
            Call faces.Add(New WallFace With {.center = New Point3D(x0, (y0 + y1) / 2, (z0 + z1) / 2), .lines = YZPlaneLines(yticks, zticks, z0, z1, y0, y1, x0)})
            Call faces.Add(New WallFace With {.center = New Point3D(x1, (y0 + y1) / 2, (z0 + z1) / 2), .lines = YZPlaneLines(yticks, zticks, z0, z1, y0, y1, x1)})

            Dim screen As Size = camera.screen

            For Each face As WallFace In faces _
                .Select(Function(f)
                            ' 旋转 + 投影之后的 Z 越大表示离视点越远
                            f.depth = camera.Project(camera.Rotate(f.center)).Z

                            Return f
                        End Function) _
                .OrderByDescending(Function(f) f.depth) _
                .Take(3)

                For Each segment As Point3D() In face.lines
                    Yield New Line(segment(0), segment(1)) With {.Stroke = pen}
                Next
            Next
        End Function

        ''' <summary>
        ''' 沿三条主轴生成刻度短线与刻度值文本（不改变既有 <c>AxisDraw.Axis</c> 的行为，
        ''' 该函数只负责刻度，轴名与主轴线仍由 <c>AxisDraw.Axis</c> 绘制）
        ''' </summary>
        ''' <param name="css">画布 CSS 环境</param>
        ''' <param name="xrange">X 轴范围</param>
        ''' <param name="yrange">Y 轴范围</param>
        ''' <param name="zrange">Z 轴范围</param>
        ''' <summary>
        ''' 三维坐标系的绘制锚点：相机自适应地取"屏幕上最靠下"的盒角作为原点，
        ''' 三条刻度轨从该角出发分别沿 X / Y / Z 方向延伸。
        ''' </summary>
        Public Structure FrameAnchor
            ''' <summary>锚点（屏幕最下方的那个盒角，模型坐标）</summary>
            Public Property corner As Point3D
            ''' <summary>X 轨的远端</summary>
            Public Property xEnd As Point3D
            ''' <summary>Y 轨的远端</summary>
            Public Property yEnd As Point3D
            ''' <summary>Z 轨的远端</summary>
            Public Property zEnd As Point3D
        End Structure

        ''' <summary>
        ''' 依据相机朝向计算三维坐标系的绘制锚点（前下角）与三条轨道的端点。
        '''
        ''' <para>
        ''' 把盒体的 8 个角点投影到屏幕，取屏幕 y 最大（最靠下）的角作为锚点；
        ''' 这样无论相机怎么转，刻度与轴名都会落在图的下方左侧，而不是跟着跑到右上角。
        ''' </para>
        ''' </summary>
        ''' <param name="camera">相机</param>
        ''' <param name="xrange">X 轴范围</param>
        ''' <param name="yrange">Y 轴范围</param>
        ''' <param name="zrange">Z 轴范围</param>
        Public Function AnchorOf(camera As Camera,
                                 xrange As DoubleRange,
                                 yrange As DoubleRange,
                                 zrange As DoubleRange) As FrameAnchor

            Dim xs As Double() = {xrange.Min, xrange.Max}
            Dim ys As Double() = {yrange.Min, yrange.Max}
            Dim zs As Double() = {zrange.Min, zrange.Max}
            Dim corners As New List(Of Point3D)

            For Each xv As Double In xs
                For Each yv As Double In ys
                    For Each zv As Double In zs
                        corners.Add(New Point3D(xv, yv, zv))
                    Next
                Next
            Next

            Dim best As Point3D = corners(0)
            Dim bestY As Double = Double.MinValue
            Dim bestX As Double = Double.MaxValue

            For Each pt As Point3D In corners
                Dim screen As Point3D = camera.Project(camera.Rotate(pt))
                ' 屏幕 y 越大表示越靠下；同高时取更靠左的那个角
                If screen.Y > bestY + 1.0E-06 OrElse
                   (std.Abs(screen.Y - bestY) <= 1.0E-06 AndAlso screen.X < bestX) Then
                    best = pt
                    bestY = screen.Y
                    bestX = screen.X
                End If
            Next

            Dim cx# = (xrange.Min + xrange.Max) / 2
            Dim cy# = (yrange.Min + yrange.Max) / 2
            Dim cz# = (zrange.Min + zrange.Max) / 2

            Return New FrameAnchor With {
                .corner = best,
                .xEnd = New Point3D(If(best.X < cx, xrange.Max, xrange.Min), best.Y, best.Z),
                .yEnd = New Point3D(best.X, If(best.Y < cy, yrange.Max, yrange.Min), best.Z),
                .zEnd = New Point3D(best.X, best.Y, If(best.Z < cz, zrange.Max, zrange.Min))
            }
        End Function

        ''' <summary>
        ''' 计算某条轨道在屏幕上的"外法向"（单位向量，指向远离盒心的一侧）。
        '''
        ''' <para>
        ''' 刻度值/轴名用这个方向做像素级偏移：既保证文字永远落在盒体外侧，
        ''' 又不会像三维外推那样把缩放域撑大、进而缩小整个盒体。
        ''' </para>
        ''' </summary>
        ''' <param name="camera">相机</param>
        ''' <param name="start">轨道起点（模型坐标）</param>
        ''' <param name="finish">轨道终点（模型坐标）</param>
        ''' <param name="center">盒心（模型坐标）</param>
        Private Function ScreenOutward(camera As Camera, start As Point3D, finish As Point3D, center As Point3D) As PointF
            Dim a As Point3D = camera.Project(camera.Rotate(start))
            Dim b As Point3D = camera.Project(camera.Rotate(finish))
            Dim c As Point3D = camera.Project(camera.Rotate(center))
            Dim dx# = b.X - a.X
            Dim dy# = b.Y - a.Y
            Dim nx# = -dy
            Dim ny# = dx
            Dim mx# = (a.X + b.X) / 2 - c.X
            Dim my# = (a.Y + b.Y) / 2 - c.Y

            If nx * mx + ny * my < 0 Then
                nx = -nx
                ny = -ny
            End If

            Dim len# = std.Sqrt(nx * nx + ny * ny)

            If len <= 1.0E-12 Then
                Return New PointF(0, 1)
            End If

            Return New PointF(CSng(nx / len), CSng(ny / len))
        End Function

        ''' <summary>
        ''' 沿三条主轴生成刻度短线与刻度值文本（锚点由相机决定，刻度集中在左下角）
        ''' </summary>
        ''' <param name="css">画布 CSS 环境</param>
        ''' <param name="xrange">X 轴范围</param>
        ''' <param name="yrange">Y 轴范围</param>
        ''' <param name="zrange">Z 轴范围</param>
        ''' <param name="theme">三维绘图主题</param>
        ''' <param name="camera">相机（用于确定左下角锚点）</param>
        ''' <param name="tickCount">每根轴上期望的刻度数量</param>
        ''' <returns>刻度短线与刻度值文本元素</returns>
        Public Iterator Function Ticks(css As CSSEnvirnment,
                                       xrange As DoubleRange,
                                       yrange As DoubleRange,
                                       zrange As DoubleRange,
                                       theme As Plot3DTheme,
                                       camera As Camera,
                                       Optional tickCount As Integer = 6) As IEnumerable(Of Element3D)

            Dim brush As Brush = theme.mainTextColor.GetBrush
            Dim format As String = If(theme.tickFormat.StringEmpty, "G2", theme.tickFormat)
            Dim anchor As FrameAnchor = AnchorOf(camera, xrange, yrange, zrange)
            Dim center As New Point3D(
                (xrange.Min + xrange.Max) / 2,
                (yrange.Min + yrange.Max) / 2,
                (zrange.Min + zrange.Max) / 2)
            ' 三条轨道在屏幕上的外法向：刻度值沿这个方向做像素偏移
            Dim outX As PointF = ScreenOutward(camera, anchor.corner, anchor.xEnd, center)
            Dim outY As PointF = ScreenOutward(camera, anchor.corner, anchor.yEnd, center)
            Dim outZ As PointF = ScreenOutward(camera, anchor.corner, anchor.zEnd, center)

            ' X 轴：沿锚点所在的 Y/Z 平面
            For Each xv As Double In InRangeTicks(xrange, tickCount)
                Dim p As New Point3D(xv, anchor.corner.Y, anchor.corner.Z)

                Yield TickLabel(xv, format, p, outX, theme, brush)
            Next

            ' Y 轴（跳过与锚点重合的那个刻度值，避免和 Z 轴的最小刻度叠在一起）
            For Each yv As Double In InRangeTicks(yrange, tickCount)
                If Near(yv, anchor.corner.Y, SafeLength(yrange) * 0.02) Then
                    Continue For
                End If

                Yield TickLabel(yv, format, New Point3D(anchor.corner.X, yv, anchor.corner.Z), outY, theme, brush)
            Next

            ' Z 轴
            For Each zv As Double In InRangeTicks(zrange, tickCount)
                If Near(zv, anchor.corner.Z, SafeLength(zrange) * 0.02) Then
                    Continue For
                End If

                Yield TickLabel(zv, format, New Point3D(anchor.corner.X, anchor.corner.Y, zv), outZ, theme, brush)
            Next
        End Function

        ''' <summary>判断两个数值是否在给定容差内相等（用于剔除与锚点重合的刻度值）</summary>
        Private Function Near(a As Double, b As Double, tolerance As Double) As Boolean
            Return std.Abs(a - b) <= std.Max(tolerance, 1.0E-09)
        End Function

        ''' <summary>
        ''' 在相机确定的左下角锚点处绘制三条带箭头的主轴与三个轴名
        ''' （旧版 <c>AxisDraw.Axis</c> 固定把轴与轴名放在盒体的 xmax/ymin 边上，会跑到图的右上角）
        ''' </summary>
        ''' <param name="css">画布 CSS 环境</param>
        ''' <param name="xrange">X 轴范围</param>
        ''' <param name="yrange">Y 轴范围</param>
        ''' <param name="zrange">Z 轴范围</param>
        ''' <param name="labels">三个轴的标题</param>
        ''' <param name="theme">三维绘图主题</param>
        ''' <param name="camera">相机</param>
        ''' <param name="arrowFactor">箭头尺寸因子（"宽,高"）</param>
        Public Iterator Function AxisLines(css As CSSEnvirnment,
                                           xrange As DoubleRange,
                                           yrange As DoubleRange,
                                           zrange As DoubleRange,
                                           labels As (X$, Y$, Z$),
                                           theme As Plot3DTheme,
                                           camera As Camera,
                                           Optional arrowFactor$ = "2,2") As IEnumerable(Of Element3D)

            Dim pen As Pen = css.GetPen(Stroke.TryParse(theme.axisStroke))
            Dim brush As Brush = theme.mainTextColor.GetBrush
            Dim anchor As FrameAnchor = AnchorOf(camera, xrange, yrange, zrange)

            With arrowFactor.FloatSizeParser
                pen.CustomEndCap = New AdjustableArrowCap(pen.Width * .Width, pen.Width * .Height)
            End With

            If DebugFrame Then
                Call Console.WriteLine(
                    $"[3d frame] corner={anchor.corner} -> x={anchor.xEnd} y={anchor.yEnd} z={anchor.zEnd}")
                Call Console.WriteLine(
                    $"[3d frame] screen: corner={Project(camera, anchor.corner)} x={Project(camera, anchor.xEnd)} y={Project(camera, anchor.yEnd)} z={Project(camera, anchor.zEnd)}")
            End If

            Yield New Line(anchor.corner, anchor.xEnd) With {.Stroke = pen}
            Yield New Line(anchor.corner, anchor.yEnd) With {.Stroke = pen}
            Yield New Line(anchor.corner, anchor.zEnd) With {.Stroke = pen}

            ' 轴名放在各轴的远端，并沿"盒体外侧"再推出一段，避免压住最末一个刻度值
            Dim ox# = anchor.outX.X * SafeLength(yrange) * 0.10
            Dim oy# = anchor.outY.X * SafeLength(xrange) * 0.10
            Dim ozX# = anchor.outZ.X * SafeLength(xrange) * 0.10
            Dim ozY# = anchor.outZ.Y * SafeLength(yrange) * 0.10

            Yield New Label With {
                .Text = labels.X,
                .FontCss = theme.axisLabelCSS,
                .Color = brush,
                .Location = New Point3D(anchor.xEnd.X, anchor.xEnd.Y + ox, anchor.xEnd.Z)
            }
            Yield New Label With {
                .Text = labels.Y,
                .FontCss = theme.axisLabelCSS,
                .Color = brush,
                .Location = New Point3D(anchor.yEnd.X + oy, anchor.yEnd.Y, anchor.yEnd.Z)
            }
            Yield New Label With {
                .Text = labels.Z,
                .FontCss = theme.axisLabelCSS,
                .Color = brush,
                .Location = New Point3D(
                    anchor.zEnd.X + ozX,
                    anchor.zEnd.Y + ozY,
                    anchor.zEnd.Z + SafeLength(zrange) * 0.08)
            }
        End Function

        ''' <summary>轴长度的安全取值（避免退化范围导致除零或 0 偏移）</summary>
        Private Function SafeLength(range As DoubleRange) As Double
            Dim len# = range.Length

            Return If(len > 0, len, 1)
        End Function

        ''' <summary>
        ''' 调试开关：环境变量 <c>PLOT3D_DEBUG=1</c> 时输出三维坐标系锚点的投影位置
        ''' </summary>
        Private ReadOnly DebugFrame As Boolean =
            String.Equals(Environment.GetEnvironmentVariable("PLOT3D_DEBUG"), "1", StringComparison.Ordinal)

        ''' <summary>把模型坐标投影成屏幕坐标（仅用于诊断输出）</summary>
        Private Function Project(camera As Camera, pt As Point3D) As PointF
            Dim p As Point3D = camera.Project(camera.Rotate(pt))

            Return New PointF(CSng(p.X), CSng(p.Y))
        End Function

        ''' <summary>
        ''' 格式化刻度值：接近 0 的浮点噪声统一显示成 <c>0</c>（否则会出现 <c>-0</c> 这种不雅的标签）
        ''' </summary>
        ''' <param name="v">刻度值</param>
        ''' <param name="format">数字格式，缺省 <c>G2</c></param>
        Public Function FormatTick(v As Double, Optional format$ = "G2") As String
            If std.Abs(v) < 0.05 Then
                Return "0"
            End If

            Return v.ToString(If(format.StringEmpty, "G2", format))
        End Function

        ''' <summary>刻度值相对轨道的屏幕偏移量（像素）</summary>
        Private Const TickGap As Single = 12

        ''' <summary>轴名相对轴端的屏幕偏移量（像素）</summary>
        Private Const TitleGap As Single = 24

        ''' <summary>生成一个刻度值文本元素（沿 <paramref name="outward"/> 方向做像素外偏移）</summary>
        Private Function TickLabel(v As Double, format$, location As Point3D,
                                   outward As PointF, theme As Plot3DTheme, brush As Brush) As Element3D
            Return New Label With {
                .Text = FormatTick(v, format),
                .FontCss = theme.tickCSS,
                .Color = brush,
                .Location = location,
                .Offset = New PointF(outward.X * TickGap, outward.Y * TickGap)
            }
        End Function

        ''' <summary>
        ''' 计算落在 <paramref name="range"/> 之内的"好看"刻度值
        ''' </summary>
        Private Function InRangeTicks(range As DoubleRange, tickCount As Integer) As Double()
            Dim ticks As Double() = Geometry.NiceTicks(range.Min, range.Max, tickCount)

            Return ticks _
                .Where(Function(t) t >= range.Min AndAlso t <= range.Max) _
                .ToArray
        End Function

        Private Function XYPlaneLines(xticks As Double(), yticks As Double(),
                                      y0#, y1#, x0#, x1#, z#) As Point3D()()
            Dim lines As New List(Of Point3D())

            For Each xv As Double In xticks
                lines += {New Point3D(xv, y0, z), New Point3D(xv, y1, z)}
            Next
            For Each yv As Double In yticks
                lines += {New Point3D(x0, yv, z), New Point3D(x1, yv, z)}
            Next

            Return lines.ToArray
        End Function

        Private Function XZPlaneLines(xticks As Double(), zticks As Double(),
                                      z0#, z1#, x0#, x1#, y#) As Point3D()()
            Dim lines As New List(Of Point3D())

            For Each xv As Double In xticks
                lines += {New Point3D(xv, y, z0), New Point3D(xv, y, z1)}
            Next
            For Each zv As Double In zticks
                lines += {New Point3D(x0, y, zv), New Point3D(x1, y, zv)}
            Next

            Return lines.ToArray
        End Function

        Private Function YZPlaneLines(yticks As Double(), zticks As Double(),
                                      z0#, z1#, y0#, y1#, x#) As Point3D()()
            Dim lines As New List(Of Point3D())

            For Each yv As Double In yticks
                lines += {New Point3D(x, yv, z0), New Point3D(x, yv, z1)}
            Next
            For Each zv As Double In zticks
                lines += {New Point3D(x, y0, zv), New Point3D(x, y1, zv)}
            Next

            Return lines.ToArray
        End Function
    End Module
End Namespace
