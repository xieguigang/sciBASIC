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
        ''' <param name="theme">三维绘图主题</param>
        ''' <param name="tickCount">每根轴上期望的刻度数量</param>
        ''' <returns>刻度短线与刻度值文本元素</returns>
        Public Iterator Function Ticks(css As CSSEnvirnment,
                                       xrange As DoubleRange,
                                       yrange As DoubleRange,
                                       zrange As DoubleRange,
                                       theme As Plot3DTheme,
                                       Optional tickCount As Integer = 6) As IEnumerable(Of Element3D)

            Dim pen As Pen = css.GetPen(Stroke.TryParse(theme.boxStroke))
            Dim brush As Brush = theme.mainTextColor.GetBrush
            Dim format As String = If(theme.tickFormat.StringEmpty, "G2", theme.tickFormat)

            Dim x0# = xrange.Min, x1# = xrange.Max
            Dim y0# = yrange.Min, y1# = yrange.Max
            Dim z0# = zrange.Min, z1# = zrange.Max
            ' 刻度短线与文本的外推量
            Dim dx# = (x1 - x0) * 0.03
            Dim dy# = (y1 - y0) * 0.03

            If dx <= 0 Then dx = 1
            If dy <= 0 Then dy = 1

            ' X 轴：沿 y = ymin, z = zmin
            For Each xv As Double In InRangeTicks(xrange, tickCount)
                Yield New Line(New Point3D(xv, y0, z0), New Point3D(xv, y0 - dy, z0)) With {.Stroke = pen}
                Yield TickLabel(xv, format, New Point3D(xv, y0 - dy * 2.2, z0), theme, brush)
            Next

            ' Y 轴：沿 x = xmax, z = zmin
            For Each yv As Double In InRangeTicks(yrange, tickCount)
                Yield New Line(New Point3D(x1, yv, z0), New Point3D(x1 + dx, yv, z0)) With {.Stroke = pen}
                Yield TickLabel(yv, format, New Point3D(x1 + dx * 2.2, yv, z0), theme, brush)
            Next

            ' Z 轴：沿 x = xmax, y = ymin
            For Each zv As Double In InRangeTicks(zrange, tickCount)
                Yield New Line(New Point3D(x1, y0, zv), New Point3D(x1, y0 - dy, zv)) With {.Stroke = pen}
                Yield TickLabel(zv, format, New Point3D(x1, y0 - dy * 2.2, zv), theme, brush)
            Next
        End Function

        ''' <summary>生成一个刻度值文本元素</summary>
        Private Function TickLabel(v As Double, format$, location As Point3D, theme As Plot3DTheme, brush As Brush) As Element3D
            Return New Label With {
                .Text = v.ToString(format),
                .FontCss = theme.tickCSS,
                .Color = brush,
                .Location = location
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
