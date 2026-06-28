Imports System.Drawing

Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports StringAlignment = Microsoft.VisualBasic.Imaging.StringAlignment
Imports StringFormat = Microsoft.VisualBasic.Imaging.StringFormat
Imports stdf = System.Math

' ============================================================================
'  ChartsAdvanced.vb - 高级图表：盒须图 / 小提琴图 / 饼图 / 热图 / 桑基图
' ============================================================================

''' <summary>桑基图（Sankey Diagram）</summary>
Public Class SankeyPlot
    Inherits PlotEngine

    Public Property Nodes As New List(Of SankeyNode)()
    Public Property Links As New List(Of SankeyLink)()

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        DrawTitle()

        ' 计算每个节点的总入流 / 出流
        For Each n In Nodes
            n.InFlow = 0 : n.OutFlow = 0
        Next
        For Each l In Links
            Dim src = Nodes.FirstOrDefault(Function(n) n.Id = l.Source)
            Dim dst = Nodes.FirstOrDefault(Function(n) n.Id = l.Target)
            If src IsNot Nothing Then src.OutFlow += l.Value
            If dst IsNot Nothing Then dst.InFlow += l.Value
        Next
        For Each n In Nodes
            n.Total = Math.Max(n.InFlow, n.OutFlow)
        Next

        ' 简单布局：按 Layer 分列
        Dim maxLayer = If(Nodes.Count > 0, Nodes.Max(Function(n) n.Layer), 0)
        Dim nLayer = maxLayer + 1
        Dim colW = (_width - Theme.MarginLeft - Theme.MarginRight) / Math.Max(1, nLayer - 1)
        Dim nodeW = 14.0F
        Dim totalValue = Nodes.Sum(Function(n) n.Total)
        Dim availH = _height - Theme.MarginTop - Theme.MarginBottom - 20

        ' 按层分组并垂直布局
        Dim layerNodes As New Dictionary(Of Integer, List(Of SankeyNode))()
        For Each n In Nodes
            If Not layerNodes.ContainsKey(n.Layer) Then layerNodes(n.Layer) = New List(Of SankeyNode)()
            layerNodes(n.Layer).Add(n)
        Next

        For Each kv In layerNodes
            Dim layerTotal = kv.Value.Sum(Function(n) n.Total)
            Dim y = Theme.MarginTop + 10.0F
            For Each n In kv.Value
                n.X = Theme.MarginLeft + kv.Key * colW
                n.Y = y
                n.Height = CSng(n.Total / layerTotal * availH)
                y += n.Height + 6
            Next
        Next

        ' 绘制连接（贝塞尔曲线）
        For Each l In Links
            Dim src = Nodes.FirstOrDefault(Function(n) n.Id = l.Source)
            Dim dst = Nodes.FirstOrDefault(Function(n) n.Id = l.Target)
            If src Is Nothing OrElse dst Is Nothing Then Continue For

            ' 计算源节点和目标节点上的偏移（按连接顺序累加）
            If Not src.OutOffsets.ContainsKey(l.Target) Then src.OutOffsets(l.Target) = 0
            If Not dst.InOffsets.ContainsKey(l.Source) Then dst.InOffsets(l.Source) = 0
            Dim srcY = src.Y + src.OutOffsets(l.Target)
            Dim dstY = dst.Y + dst.InOffsets(l.Source)
            Dim linkH = CSng(l.Value / Math.Max(src.Total, 1) * src.Height)
            Dim linkH2 = CSng(l.Value / Math.Max(dst.Total, 1) * dst.Height)
            src.OutOffsets(l.Target) += linkH
            dst.InOffsets(l.Source) += linkH2

            Dim x1 = src.X + nodeW
            Dim x2 = dst.X
            Dim cx = (x1 + x2) / 2
            Dim path As New GraphicsPath()
            path.AddBezier(
                New PointF(x1, srcY + linkH / 2),
                New PointF(cx, srcY + linkH / 2),
                New PointF(cx, dstY + linkH2 / 2),
                New PointF(x2, dstY + linkH2 / 2))
            path.AddBezier(
                New PointF(x2, dstY + linkH2 / 2),
                New PointF(cx, dstY + linkH2 / 2),
                New PointF(cx, srcY + linkH / 2),
                New PointF(x1, srcY + linkH / 2))
            path.CloseFigure()

            Dim color As Color = If(l.Color, Color.FromArgb(80, Theme.Palette(0)))
            Using br As New SolidBrush(color)
                _g.FillPath(br, path)
            End Using
        Next

        ' 绘制节点
        For Each n In Nodes
            Dim color = If(n.Color, Theme.Palette(n.Layer Mod Theme.Palette.Length))
            Using br As New SolidBrush(color),
                  pen As New Pen(Theme.BorderColor, 0.5F)
                _g.FillRectangle(br, n.X, n.Y, nodeW, n.Height)
                _g.DrawRectangle(pen, n.X, n.Y, nodeW, n.Height)
            End Using
            ' 标签
            Using br As New SolidBrush(Theme.TextColor),
                  sf As New StringFormat()
                sf.LineAlignment = StringAlignment.Center
                If n.Layer = 0 Then
                    sf.Alignment = StringAlignment.Far
                    _g.DrawString(n.Label, Theme.TickLabelFont, br, n.X - 6, n.Y + n.Height / 2)
                Else
                    sf.Alignment = StringAlignment.Near
                    _g.DrawString(n.Label, Theme.TickLabelFont, br, n.X + nodeW + 6, n.Y + n.Height / 2)
                End If
            End Using
        Next
    End Sub
End Class
