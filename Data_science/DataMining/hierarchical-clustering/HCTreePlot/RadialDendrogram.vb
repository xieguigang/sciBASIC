Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.scale
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports std = System.Math

''' <summary>
''' 绘制环状（辐射状）层次聚类树
''' </summary>
Public Class RadialDendrogram : Inherits DendrogramPanel

    Public Sub New(hist As Cluster, theme As Theme,
                   Optional classes() As ColorClass = Nothing,
                   Optional classinfo As Dictionary(Of String, String) = Nothing,
                   Optional showAllLabels As Boolean = False,
                   Optional showAllNodes As Boolean = False,
                   Optional pointColor As String = "red",
                   Optional showRuler As Boolean = True,
                   Optional showLeafLabels As Boolean = True)

        MyBase.New(hist, theme, classes, classinfo, showAllLabels, showAllNodes, pointColor, showLeafLabels, showRuler)
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Dim css As CSSEnvirnment = g.LoadEnvironment
        Dim plotRegion As Rectangle = canvas.PlotRegion(css)

        ' 确定绘图中心和最大半径
        Dim centerX As Double = plotRegion.Left + plotRegion.Width / 2
        Dim centerY As Double = plotRegion.Top + plotRegion.Height / 2
        Dim maxRadius As Double = std.Min(plotRegion.Width, plotRegion.Height) / 2

        ' 预留空间给标签和标尺
        ' 计算字体尺寸以确定边缘留白
        Dim tickFont As Font = css.GetFont(CSSFont.TryParse(theme.axisTickCSS))
        Dim labelFont As Font = css.GetFont(CSSFont.TryParse(theme.tagCSS))
        Me.labelFont = labelFont

        If showRuler Then
            maxRadius -= g.MeasureString("0.00", tickFont).Height
        End If
        If showLeafLabels Then
            ' 估算最大标签宽度，留出空间
            maxRadius -= g.MeasureString("SAMPLE", labelFont).Width
        End If

        ' 确保半径至少为正
        maxRadius = std.Max(1, maxRadius)

        ' 计算比例尺：将距离映射到半径
        ' 关键修改：将 Range 设为 {maxRadius, 0}
        ' 这样：最大距离（根节点） -> 0（圆心）；最小距离（叶节点） -> maxRadius（最外圈）
        Dim axisTicks As Double()
        If hist.DistanceValue <= 0.1 Then
            axisTicks = {0, hist.DistanceValue}.Range.CreateAxisTicks(decimalDigits:=-1)
        Else
            axisTicks = {0, hist.DistanceValue}.Range.CreateAxisTicks
        End If

        Dim radiusScale As d3js.scale.LinearScale = d3js.scale _
            .linear() _
            .domain(values:=axisTicks) _
            .range(integers:={maxRadius, 0}) ' 反转范围：根在内，叶在外

        ' 绘制距离标尺（同心圆）
        If showRuler Then
            Dim axisPen As Pen = css.GetPen(Stroke.TryParse(theme.axisStroke))

            For Each tick As Double In axisTicks
                Dim r As Double = maxRadius - radiusScale(tick)
                ' 避免半径过小
                If r < 1 Then r = 1

                ' 绘制圆
                g.DrawEllipse(axisPen,
                              New RectangleF(centerX - r, centerY - r, r * 2, r * 2))

                ' 绘制刻度值 (绘制在圆环的顶部)
                Dim tickLable As String = tick.ToString(theme.XaxisTickFormat)
                Dim tickLabelSize As SizeF = g.MeasureString(tickLable, tickFont)

                ' 将刻度值放置在圆环顶部外侧 (12点钟方向)
                Dim labelX As Single = centerX - tickLabelSize.Width / 2
                Dim labelY As Single = centerY - r - tickLabelSize.Height - 2

                g.DrawString(tickLable, tickFont, Brushes.Black, New PointF(labelX, labelY))
            Next
        End If

        ' 每个叶节点占用的角度 (弧度)
        ' 修改为 2 * PI 以绘制完整的圆形树（原代码是 PI，即半圆）
        Dim unitAngle As Double = (2 * std.PI) / hist.Leafs

        ' 开始递归绘制
        ' 初始参数：当前节点=hist, 起始角度=0, 父节点位置=Nothing, 父节点半径=0
        Call RadialPlot(hist, 0, unitAngle, g, radiusScale, Nothing, 0, centerX, centerY, maxRadius)
    End Sub

    ''' <summary>
    ''' 递归绘制节点
    ''' </summary>
    Private Sub RadialPlot(partition As Cluster,
                           startAngleIndex As Integer,
                           unitAngle As Double,
                           g As IGraphics,
                           scale As LinearScale,
                           parentPt As PointF,
                           parentRadius As Double,
                           centerX As Double,
                           centerY As Double,
                           maxRadius As Double)

        ' 对子节点按叶节点数量排序，保持绘图逻辑一致性
        Dim orders As Cluster() = partition.Children.OrderBy(Function(a) a.Leafs).ToArray

        ' 计算当前节点的半径
        ' 由于比例尺已反转，Root 会是小半径，Leaf 会是大半径
        Dim currentRadius As Double = maxRadius - scale(partition.DistanceValue)

        ' 计算当前节点的角度（位于其所有叶节点扇区的中心）
        Dim currentAngle As Double = (startAngleIndex + (partition.Leafs / 2)) * unitAngle

        ' 将极坐标转换为笛卡尔坐标
        Dim currentPt As PointF = PolarToCartesian(centerX, centerY, currentRadius, currentAngle)

        Dim css As CSSEnvirnment = g.LoadEnvironment
        Dim linkColor As Pen = css.GetPen(Me.linkColor)

        ' 绘制连接线
        If Not parentPt.IsEmpty Then
            ' 绘制逻辑：
            ' 1. 从父节点角度沿父节点半径画弧到当前节点角度
            ' 2. 从弧的终点沿当前角度画直线到当前节点

            Dim parentAngle As Double = GetAngle(centerX, centerY, parentPt)

            ' 确定绘制圆弧的矩形 (基于父节点半径)
            Dim arcRect As New RectangleF(centerX - parentRadius, centerY - parentRadius, parentRadius * 2, parentRadius * 2)

            ' 计算起始和结束角度 (GDI+ DrawArc 使用度数，且顺时针为正)
            ' 我们的数学计算是逆时针，且Y轴向下，需注意转换
            Dim startDegree As Single = -parentAngle * 180 / std.PI
            Dim endDegree As Single = -currentAngle * 180 / std.PI

            ' 规范化角度差以确定绘制方向
            Dim sweepAngle As Single = endDegree - startDegree

            ' 如果角度差过大（跨越了0/360界线），或者我们希望总是取较短路径
            ' 这里为了简单，直接按排序顺序绘制，通常角度是递增的
            ' 但由于坐标系转换，可能需要调整
            If sweepAngle > 180 Then sweepAngle -= 360
            If sweepAngle < -180 Then sweepAngle += 360

            ' 绘制圆弧 (如果父节点半径太小，则直接画线)
            If parentRadius > 1 Then
                g.DrawArc(linkColor, arcRect, startDegree, sweepAngle)
            Else
                ' 如果父节点几乎在圆心，直接画线
                g.DrawLine(linkColor, parentPt, New PointF(centerX + parentRadius * std.Cos(currentAngle), centerY - parentRadius * std.Sin(currentAngle)))
            End If

            ' 绘制径向线：从 (当前角度, 父半径) 到 (当前角度, 当前半径)
            ' 即圆弧的终点到当前节点
            Dim arcEndPt As PointF = PolarToCartesian(centerX, centerY, parentRadius, currentAngle)
            g.DrawLine(linkColor, arcEndPt, currentPt)
        End If

        ' 绘制节点点
        If (partition.isLeaf OrElse showAllNodes) AndAlso theme.pointSize > 0 Then
            Call g.DrawCircle(currentPt, theme.pointSize, pointColor)
        End If

        ' 绘制标签和Class颜色块
        If showLeafLabels AndAlso (partition.isLeaf OrElse showAllLabels) Then
            Dim lsize As SizeF = g.MeasureString(partition.Name, labelFont)

            ' 计算标签位置：在当前半径向外延伸一点
            Dim labelPadding As Double = 4 ' 基础间距

            ' 计算标签绘制点的坐标
            Dim labelRadius As Double = currentRadius + labelPadding + theme.pointSize
            Dim labelPt As PointF = PolarToCartesian(centerX, centerY, labelRadius, currentAngle)

            ' 文本对齐逻辑：根据角度调整绘制起点
            Dim textPos As PointF
            ' Dim format As StringFormat = StringFormat.GenericDefault

            ' 将角度规范化到 0-2PI
            Dim normAngle As Double = currentAngle Mod (2 * std.PI)
            If normAngle < 0 Then normAngle += 2 * std.PI

            ' 简单的对齐逻辑：
            ' 左半圆 (PI/2 ~ 3PI/2)：文字在点的左侧，右对齐
            ' 右半圆 (-PI/2 ~ PI/2)：文字在点的右侧，左对齐
            If normAngle > std.PI / 2 AndAlso normAngle < 3 * std.PI / 2 Then
                ' 左半圆
                ' format.Alignment = StringAlignment.Far
                textPos = New PointF(labelPt.X - 2, labelPt.Y - lsize.Height / 2)
            Else
                ' 右半圆
                ' format.Alignment = StringAlignment.Near
                textPos = New PointF(labelPt.X + 2, labelPt.Y - lsize.Height / 2)
            End If

            Call g.DrawString(partition.Name, labelFont, Brushes.Black, textPos)

            ' 绘制class颜色块
            If partition.isLeaf Then
                Dim color As Color = GetColor(partition.Name)
                If color <> Nothing Then
                    Dim colorSize As Single = std.Min(labelPadding, theme.pointSize * 1.5)
                    Dim colorBrush As New SolidBrush(color)
                    ' 颜色块位于节点和标签之间
                    Dim colorRadius As Double = currentRadius + theme.pointSize + (labelPadding / 2)
                    Dim colorPt As PointF = PolarToCartesian(centerX, centerY, colorRadius, currentAngle)

                    Dim colorRect As New RectangleF(colorPt.X - colorSize / 2, colorPt.Y - colorSize / 2, colorSize, colorSize)
                    Call g.FillRectangle(colorBrush, colorRect)
                End If
            End If
        End If

        ' 递归处理子节点
        If Not partition.isLeaf Then
            Dim n As Integer = 0
            For Each part As Cluster In orders
                ' 子节点起始索引 = 当前起始索引 + 已处理的兄弟节点叶数
                RadialPlot(part, startAngleIndex + n, unitAngle, g, scale, currentPt, currentRadius, centerX, centerY, maxRadius)
                n += part.Leafs
            Next
        End If
    End Sub

    ''' <summary>
    ''' 极坐标转笛卡尔坐标
    ''' </summary>
    Private Function PolarToCartesian(cx As Double, cy As Double, radius As Double, angle As Double) As PointF
        ' angle 是弧度，0度在3点钟方向，逆时针为正 (数学标准)
        ' 屏幕坐标系Y轴向下，所以 Y = cy - r * sin(angle)
        Return New PointF(cx + radius * std.Cos(angle), cy - radius * std.Sin(angle))
    End Function

    ''' <summary>
    ''' 计算点相对于圆心的角度
    ''' </summary>
    Private Function GetAngle(cx As Double, cy As Double, pt As PointF) As Double
        Dim dx As Double = pt.X - cx
        Dim dy As Double = cy - pt.Y ' 注意Y轴方向调整以匹配 PolarToCartesian
        Return std.Atan2(dy, dx)
    End Function

End Class
