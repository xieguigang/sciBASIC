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
        If showRuler Then
            maxRadius -= g.MeasureString("0.00", css.GetFont(CSSFont.TryParse(theme.axisTickCSS))).Width
        End If
        If showLeafLabels Then
            maxRadius -= g.MeasureString("SAMPLE", css.GetFont(CSSFont.TryParse(theme.tagCSS))).Width
        End If

        ' 计算比例尺：将距离映射到半径
        ' 这里的逻辑是：距离0在圆心（或接近圆心），最大距离在圆周
        Dim axisTicks As Double()
        If hist.DistanceValue <= 0.1 Then
            axisTicks = {0, hist.DistanceValue}.Range.CreateAxisTicks(decimalDigits:=-1)
        Else
            axisTicks = {0, hist.DistanceValue}.Range.CreateAxisTicks
        End If

        Me.labelFont = css.GetFont(CSSFont.TryParse(theme.tagCSS))

        Dim radiusScale As d3js.scale.LinearScale = d3js.scale _
            .linear() _
            .domain(values:=axisTicks) _
            .range(integers:={0, maxRadius})

        ' 绘制距离标尺（同心圆）
        If showRuler Then
            Dim tickFont As Font = css.GetFont(CSSFont.TryParse(theme.axisTickCSS))
            Dim axisPen As Pen = css.GetPen(Stroke.TryParse(theme.axisStroke))
            Dim tickLabelSize As SizeF

            For Each tick As Double In axisTicks
                Dim r As Double = radiusScale(tick)
                ' 避免半径过小
                If r < 1 Then r = 1

                ' 绘制圆
                g.DrawEllipse(axisPen,
                              New RectangleF(centerX - r, centerY - r, r * 2, r * 2))

                ' 绘制刻度值 (绘制在圆环的顶部)
                Dim tickLable As String = tick.ToString(theme.XaxisTickFormat)
                tickLabelSize = g.MeasureString(tickLable, tickFont)

                ' 将刻度值放置在圆环顶部外侧
                Dim labelX As Single = centerX - tickLabelSize.Width / 2
                Dim labelY As Single = centerY - r - tickLabelSize.Height - 2

                g.DrawString(tickLable, tickFont, Brushes.Black, New PointF(labelX, labelY))
            Next
        End If

        ' 每个叶节点占用的角度 (弧度)
        Dim unitAngle As Double = 2 * std.PI / hist.Leafs

        ' 开始递归绘制
        ' 初始参数：当前节点=hist, 起始角度=0, 父节点位置=Nothing, 父节点半径=0
        Call RadialPlot(hist, 0, unitAngle, g, radiusScale, Nothing, 0, centerX, centerY)
    End Sub

    ''' <summary>
    ''' 递归绘制节点
    ''' </summary>
    ''' <param name="partition">当前节点</param>
    ''' <param name="startAngleIndex">当前节点在序列中的起始索引（用于计算角度）</param>
    ''' <param name="unitAngle">单位角度</param>
    ''' <param name="g">绘图对象</param>
    ''' <param name="scale">半径映射比例尺</param>
    ''' <param name="parentPt">父节点坐标 (PointF)</param>
    ''' <param name="parentRadius">父节点半径</param>
    ''' <param name="centerX">圆心X</param>
    ''' <param name="centerY">圆心Y</param>
    Private Sub RadialPlot(partition As Cluster,
                           startAngleIndex As Integer,
                           unitAngle As Double,
                           g As IGraphics,
                           scale As LinearScale,
                           parentPt As PointF,
                           parentRadius As Double,
                           centerX As Double,
                           centerY As Double)

        ' 对子节点按叶节点数量排序，保持绘图逻辑一致性
        Dim orders As Cluster() = partition.Children.OrderBy(Function(a) a.Leafs).ToArray

        ' 计算当前节点的半径
        Dim currentRadius As Double = scale(partition.DistanceValue)

        ' 计算当前节点的角度（位于其所有叶节点扇区的中心）
        ' 角度 = 起始索引 + (当前节点叶数 / 2) * 单位角度
        Dim currentAngle As Double = (startAngleIndex + (partition.Leafs / 2)) * unitAngle

        ' 将极坐标转换为笛卡尔坐标
        Dim currentPt As PointF = PolarToCartesian(centerX, centerY, currentRadius, currentAngle)

        Dim css As CSSEnvirnment = g.LoadEnvironment
        Dim linkColor As Pen = css.GetPen(Me.linkColor)

        ' 绘制连接线
        If Not parentPt.IsEmpty Then
            ' 1. 绘制从父节点角度到当前节点角度的圆弧 (半径为父节点半径)
            ' 对应原本逻辑中的横向连线
            Dim parentAngle As Double = GetAngle(centerX, centerY, parentPt)

            ' 确定绘制圆弧的矩形
            Dim arcRect As New RectangleF(centerX - parentRadius, centerY - parentRadius, parentRadius * 2, parentRadius * 2)

            ' 计算起始和结束角度 (Graphics.DrawArc 使用度数)
            Dim startDegree As Single = -parentAngle * 180 / std.PI
            Dim endDegree As Single = -currentAngle * 180 / std.PI

            ' 确保顺时针或逆时针绘制正确
            If startDegree > endDegree Then
                Dim temp As Single = startDegree
                startDegree = endDegree
                endDegree = temp
            End If

            ' 如果角度差很小，直接画线即可，否则画弧
            If std.Abs(currentAngle - parentAngle) < 0.001 Then
                g.DrawLine(linkColor, parentPt, currentPt)
            Else
                ' 注意：GDI+ 的 DrawArc 角度是顺时针为正，且0度在3点钟方向。
                ' 我们的数学计算中通常逆时针为正。这里需要根据具体计算调整方向。
                ' 这里简化处理，直接画贝塞尔曲线或者折线替代圆弧以避免复杂的GDI角度转换
                ' 或者使用 DrawArc，需要注意 SweepDirection
                g.DrawArc(linkColor, arcRect, startDegree, endDegree - startDegree)
            End If

            ' 2. 绘制从父节点半径到当前节点半径的径向线 (角度为当前节点角度)
            ' 对应原本逻辑中的纵向连线
            ' 圆弧的终点
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
            Dim labelPadding As Double = 5 ' 基础间距

            ' 计算标签绘制点的坐标
            Dim labelRadius As Double = currentRadius + labelPadding + theme.pointSize
            Dim labelPt As PointF = PolarToCartesian(centerX, centerY, labelRadius, currentAngle)

            ' 文本对齐逻辑：根据角度调整绘制起点
            Dim textPos As PointF

            ' 将角度规范化到 0-2PI
            Dim normAngle As Double = currentAngle Mod (2 * std.PI)
            If normAngle < 0 Then normAngle += 2 * std.PI

            ' 简单的对齐逻辑：左侧圆文本右对齐，右侧圆文本左对齐，上方/下方居中
            If normAngle > std.PI / 2 AndAlso normAngle < 3 * std.PI / 2 Then
                ' 左半圆
                textPos = New PointF(labelPt.X - 2, labelPt.Y - lsize.Height / 2)
            Else
                ' 右半圆
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
                RadialPlot(part, startAngleIndex + n, unitAngle, g, scale, currentPt, currentRadius, centerX, centerY)
                n += part.Leafs
            Next
        End If
    End Sub

    ''' <summary>
    ''' 极坐标转笛卡尔坐标
    ''' </summary>
    Private Function PolarToCartesian(cx As Double, cy As Double, radius As Double, angle As Double) As PointF
        ' 这里的 angle 是弧度，0度在3点钟方向，逆时针为正 (数学标准)
        ' 若要符合通常屏幕坐标系(顺时针)，可调整正负
        ' 这里假设输入的 angle 是数学标准弧度
        Return New PointF(cx + radius * std.Cos(angle), cy - radius * std.Sin(angle)) ' Y轴向上为负，屏幕坐标通常Y向下
    End Function

    ''' <summary>
    ''' 计算点相对于圆心的角度
    ''' </summary>
    Private Function GetAngle(cx As Double, cy As Double, pt As PointF) As Double
        Dim dx As Double = pt.X - cx
        Dim dy As Double = cy - pt.Y ' 注意Y轴方向
        Return std.Atan2(dy, dx)
    End Function

End Class
