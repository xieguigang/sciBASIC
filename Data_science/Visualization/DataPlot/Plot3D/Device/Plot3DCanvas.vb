' ============================================================================
'  Plot3DCanvas.vb - 三维图表共用的二维排版助手
'
'  三维图元本身由 RenderAs3DChart 负责投影与 Z 排序，
'  而"标题、图例、连续色标、抗锯齿"这些二维排版工作在三个图表里完全一致，
'  这里抽出一份实现，避免散点图 / 饼图 / 曲面图各写一遍。
' ============================================================================

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Plots.Plot3D.Legend
Imports Microsoft.VisualBasic.Data.Plots.Plot3D.Model
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports std = System.Math

Namespace Plot3D.Device

    ''' <summary>
    ''' 三维图表的二维排版助手：标题、图例块（自动钳制在画布内）、连续色标与抗锯齿
    ''' </summary>
    Public Module Plot3DCanvas

        ''' <summary>
        ''' 尽力开启抗锯齿：底层 <c>IGraphics</c> 接口并没有跨驱动的画质开关，
        ''' 这里通过反射取出 GDI 栅格画布暴露的 <c>Graphics</c> 对象并设置 GDI+ 的高质量参数，
        ''' 在 SVG / PostScript 等矢量驱动下自动退化为什么都不做（不会抛异常）。
        ''' </summary>
        ''' <param name="g">绘图设备</param>
        Public Sub ApplyAntiAlias(g As IGraphics)
            If g Is Nothing Then
                Return
            End If

            Try
                ' 这里完全走反射：DataPlot 工程并不直接引用 System.Drawing.Common，
                ' 因此不能在编译期写出 GDI+ 的类型
                Dim prop As Reflection.PropertyInfo = g.GetType().GetProperty("Graphics")

                If prop Is Nothing Then
                    Return
                End If

                Dim gdi As Object = prop.GetValue(g)

                If gdi Is Nothing Then
                    Return
                End If

                Dim gdiType As Type = gdi.GetType()

                Call SetEnumValue(gdi, gdiType, "SmoothingMode", "HighQuality")
                Call SetEnumValue(gdi, gdiType, "CompositingQuality", "HighQuality")
                Call SetEnumValue(gdi, gdiType, "PixelOffsetMode", "HighQuality")
                Call SetEnumValue(gdi, gdiType, "TextRenderingHint", "ClearTypeGridFit")
            Catch ex As Exception
                ' 抗锯齿只是锦上添花，任何异常都不应该影响出图
            End Try
        End Sub

        ''' <summary>通过反射把 GDI+ 画布上的某个枚举属性设置为指定名称的枚举值</summary>
        Private Sub SetEnumValue(target As Object, type As Type, propertyName$, enumValue$)
            Dim prop As Reflection.PropertyInfo = type.GetProperty(propertyName)

            If prop Is Nothing OrElse Not prop.CanWrite Then
                Return
            End If

            Dim value As Object = [Enum].Parse(prop.PropertyType, enumValue)

            Call prop.SetValue(target, value)
        End Sub

        ''' <summary>
        ''' 在绘图区顶部绘制图表标题与副标题（居中），没有任何文本时不做任何绘制
        ''' </summary>
        ''' <param name="g">绘图设备</param>
        ''' <param name="plotRect">绘图区像素矩形</param>
        ''' <param name="theme">三维绘图主题</param>
        ''' <param name="bandTop">
        ''' 标题带的起始 y 坐标（可选）。当调用方已经通过 <see cref="ReserveRegion"/>
        ''' 在绘图区顶部预留了标题带时，把标题画在这条带子里（从 <paramref name="bandTop"/>
        ''' 开始向下排布），避免与三维场景内的刻度文字相互叠压；
        ''' 缺省时标题画在绘图区上方的页边距里。
        ''' </param>
        Public Sub DrawTitles(g As IGraphics, plotRect As Rectangle, theme As Plot3DTheme,
                              Optional bandTop As Single = Single.NaN)
            If theme Is Nothing Then
                Return
            End If
            If theme.title.StringEmpty AndAlso theme.subTitle.StringEmpty Then
                Return
            End If

            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim titleFont As Font = css.GetFont(CSSFont.TryParse(theme.titleCSS))
            Dim subFont As Font = css.GetFont(CSSFont.TryParse(theme.subtitleCSS))
            Dim titleSize As SizeF = If(theme.title.StringEmpty,
                New SizeF(0, 0), g.MeasureString(theme.title, titleFont))
            Dim subSize As SizeF = If(theme.subTitle.StringEmpty,
                New SizeF(0, 0), g.MeasureString(theme.subTitle, subFont))
            Dim cx As Single = plotRect.Left + plotRect.Width / 2
            ' 标题优先画在预留的标题带里；没有预留时画在绘图区上方的留白里
            Dim top As Single = If(Single.IsNaN(bandTop),
                plotRect.Top - titleSize.Height - subSize.Height - 6,
                bandTop + 2)

            If Not theme.title.StringEmpty Then
                Using br As Brush = theme.mainTextColor.GetBrush
                    Call g.DrawString(theme.title, titleFont, br, cx - titleSize.Width / 2, top)
                End Using

                top += titleSize.Height + 2
            End If

            If Not theme.subTitle.StringEmpty Then
                Using br As Brush = theme.mainTextColor.GetBrush
                    Call g.DrawString(theme.subTitle, subFont, br, cx - subSize.Width / 2, top)
                End Using
            End If
        End Sub

        ''' <summary>
        ''' 计算标题与副标题所需要的垂直留白（像素）；
        ''' 没有任何标题文本时返回 0
        ''' </summary>
        ''' <param name="g">绘图设备</param>
        ''' <param name="theme">三维绘图主题</param>
        Public Function TitlesHeight(g As IGraphics, theme As Plot3DTheme) As Single
            If theme Is Nothing Then
                Return 0
            End If
            If theme.title.StringEmpty AndAlso theme.subTitle.StringEmpty Then
                Return 0
            End If

            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim titleFont As Font = css.GetFont(CSSFont.TryParse(theme.titleCSS))
            Dim subFont As Font = css.GetFont(CSSFont.TryParse(theme.subtitleCSS))
            ' 注意：这里必须用 MeasureString 实测高度。
            ' Font.GetHeight 在该字体模型下返回的数值明显偏小，
            ' 会导致预留的标题带不够高、副标题与三维场景顶部的刻度文字叠压。
            Dim height As Single = 14

            If Not theme.title.StringEmpty Then
                height += g.MeasureString("Ag", titleFont).Height
            End If
            If Not theme.subTitle.StringEmpty Then
                height += g.MeasureString("Ag", subFont).Height + 4
            End If

            Return height
        End Function

        ''' <summary>
        ''' 复制一份绘图区域，并在右侧与顶部额外扣除一段留白，
        ''' 供图例 / 色标与图表标题使用——这样三维场景本身就会被缩放到剩下的区域里，
        ''' 不会与图例、标题相互叠压。
        ''' </summary>
        ''' <param name="region">原始绘图区域</param>
        ''' <param name="theme">三维绘图主题</param>
        ''' <param name="rightExtra">右侧额外扣除的像素</param>
        ''' <param name="topExtra">顶部额外扣除的像素</param>
        Public Function ReserveRegion(region As GraphicsRegion,
                                      theme As Plot3DTheme,
                                      rightExtra As Single, topExtra As Single) As GraphicsRegion

            Dim scene As GraphicsRegion = region
            Dim pad As Padding = region.Padding

            scene.Padding = New Padding With {
                .Top = $"{CInt(Val(If(pad.Top, "0")) + topExtra)}px",
                .Right = $"{CInt(Val(If(pad.Right, "0")) + rightExtra)}px",
                .Bottom = pad.Bottom,
                .Left = pad.Left
            }

            Return scene
        End Function

        ''' <summary>
        ''' 计算图例块（含外框）的实际像素尺寸。
        '''
        ''' <para>
        ''' 尺寸公式与 <see cref="LegendPlotExtensions.DrawLegends"/> 内部一致：
        ''' <c>width = w + 1.25h + maxTitleWidth</c>、
        ''' <c>height = (Max(h, maxTitleHeight) + d + 1.25) * n</c>，
        ''' 其中 <c>w,h</c> 为图例形状尺寸。
        ''' </para>
        ''' </summary>
        ''' <param name="g">绘图设备</param>
        ''' <param name="legends">图例项</param>
        ''' <param name="theme">三维绘图主题</param>
        ''' <param name="d">图例项之间的间距</param>
        Public Function MeasureLegendBox(g As IGraphics, legends As IEnumerable(Of LegendObject),
                                         theme As Plot3DTheme, Optional d As Integer = 5) As SizeF
            Dim list As LegendObject() = legends.SafeQuery.ToArray

            If list.Length = 0 Then
                Return New SizeF(0, 0)
            End If

            ' 与 DrawLegends 共用同一个外框几何，避免"预留空间"和"实际画框"各算一套
            Return LegendRegion(PointF.Empty, ShapeSize(theme), list.Length,
                                 list.MaxLegendSize(g), d).Size
        End Function

        ''' <summary>
        ''' 在绘图区右侧绘制图例，并保证整个图例块（含外框）不会越出画布
        ''' </summary>
        ''' <param name="g">绘图设备</param>
        ''' <param name="plotRect">绘图区像素矩形</param>
        ''' <param name="canvasSize">画布尺寸</param>
        ''' <param name="legends">图例项</param>
        ''' <param name="theme">三维绘图主题</param>
        ''' <param name="d">图例项之间的间距</param>
        Public Sub DrawLegendBlock(g As IGraphics, plotRect As Rectangle, canvasSize As Size,
                                   legends As IEnumerable(Of LegendObject),
                                   theme As Plot3DTheme, Optional d As Integer = 5)
            Dim list As LegendObject() = legends.SafeQuery.ToArray

            If list.Length = 0 OrElse Not theme.drawLegend Then
                Return
            End If

            Dim box As SizeF = MeasureLegendBox(g, list, theme, d)
            Dim margin As Single = 8

            ' 这里定位的是"外框"的左上角；DrawLegends 会以 topLeft = 外框 + (d, d) 反推
            Dim bx As Single = plotRect.Right - box.Width - margin
            Dim by As Single = plotRect.Top + (plotRect.Height - box.Height) / 2

            bx = std.Max(plotRect.Left + 2, std.Min(bx, canvasSize.Width - box.Width - margin))
            by = std.Max(plotRect.Top + 2, std.Min(by, canvasSize.Height - box.Height - margin))

            Dim topLeft As New PointF(bx + d, by + d)

            Call g.DrawLegends(
                topLeft:=topLeft,
                legends:=list,
                gSize:=$"{ShapeSize(theme).Width},{ShapeSize(theme).Height}",
                d:=d,
                fillBg:="white",
                regionBorder:=Stroke.TryParse(theme.legendBoxStroke))
        End Sub

        ''' <summary>
        ''' 绘制连续色标（竖直渐变条 + 刻度 + 标题），用于三维曲面图。
        ''' 自己绘制而不是复用位图图例，这样 SVG / PostScript 驱动同样可以出图。
        ''' </summary>
        ''' <param name="g">绘图设备</param>
        ''' <param name="x">色标左上角 x</param>
        ''' <param name="y">色标左上角 y</param>
        ''' <param name="width">色标宽度</param>
        ''' <param name="height">色标高度</param>
        ''' <param name="colors">由小到大排列的颜色</param>
        ''' <param name="min">色标下限</param>
        ''' <param name="max">色标上限</param>
        ''' <param name="title">色标标题</param>
        ''' <param name="theme">三维绘图主题</param>
        Public Sub DrawColorBar(g As IGraphics, x As Single, y As Single,
                                width As Single, height As Single,
                                colors As Color(), min As Double, max As Double,
                                title As String, theme As Plot3DTheme)

            If colors.IsNullOrEmpty OrElse width <= 0 OrElse height <= 0 Then
                Return
            End If

            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim tickFont As Font = css.GetFont(CSSFont.TryParse(theme.tickCSS))
            Dim steps As Integer = CInt(std.Ceiling(height))

            For i As Integer = 0 To steps - 1
                Dim t As Double = 1 - i / CDbl(steps)
                Dim color As Color = ColorScale.LerpPalette(t, colors)

                ' 注意：GDI 驱动的 FillRectangle(brush, x, y, w, h) 重载是空实现，
                ' 必须走 RectangleF 重载才会真的绘制出来
                Using br As New SolidBrush(color)
                    Call g.FillRectangle(br, New RectangleF(x, y + i, width, 1))
                End Using
            Next

            Using border As Pen = css.GetPen(Stroke.TryParse(theme.axisStroke)),
                  text As Brush = theme.mainTextColor.GetBrush

                Call g.DrawRectangle(border, x, y, width, height)

                For Each tick As Double In Geometry.NiceTicks(min, max, 5)
                    If tick < min OrElse tick > max Then
                        Continue For
                    End If

                    Dim pos As Single = If(max = min, 0, CSng((tick - min) / (max - min)))
                    Dim py As Single = y + height - pos * height
                    Dim label As String = AxisFrame.FormatTick(tick, theme.tickFormat)
                    Dim size As SizeF = g.MeasureString(label, tickFont)

                    Call g.DrawLine(border, x + width, py, x + width + 4, py)
                    Call g.DrawString(label, tickFont, text, x + width + 6, py - size.Height / 2)
                Next

                If Not title.StringEmpty Then
                    Dim titleFont As Font = css.GetFont(CSSFont.TryParse(theme.axisLabelCSS))
                    Dim size As SizeF = g.MeasureString(title, titleFont)

                    ' 标题左对齐到色标条的左边界，避免居中之后向左侧越界压到三维场景的刻度文字
                    Call g.DrawString(title, titleFont, text, x, y - size.Height - 4)
                End If
            End Using
        End Sub

        ''' <summary>解析主题里的图例形状尺寸字符串</summary>
        Private Function ShapeSize(theme As Plot3DTheme) As SizeF
            Dim expr As String = If(theme.legendShapeSize, "12,12")
            Dim size As SizeF = expr.FloatSizeParser

            If size.Width <= 0 Then size.Width = 12
            If size.Height <= 0 Then size.Height = 12

            Return size
        End Function
    End Module
End Namespace
