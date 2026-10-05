#Region "Microsoft.VisualBasic::4f8a2c6e1b3d4907d9e5c1a8b2f7d3c6, Data_science\Visualization\DataPlot\Statistics\ClusterHeatmapPlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class ClusterHeatmapPlot
    ' 
    '     Properties: Matrix, RowLabels, ColLabels, RowTree, ColTree
    '                 ColorMap, Scaling, ShowRowDendrogram, ShowColDendrogram
    '                 TreeFraction, ShowValues
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

' ============================================================================
'  ClusterHeatmapPlot.vb - 带聚类树的热图
'
'  旧实现把「聚类 + 缩放 + 树 + 热图」揉在 HeatMap.vb / HeatMapPlot.vb / Internal.vb
'  三个文件里，并且直接依赖 hctree 与 HCTreePlot 两个包。这里改成：
'   · 聚类结果由调用方以 <see cref="ClusterTreeNode"/> 传入（本文件不调用任何聚类算法）；
'   · 树的绘制走 Statistics/ClusterTree.vb；
'   · 行列顺序自动跟随聚类树的叶序，不必调用方自己 reorder。
' ============================================================================

''' <summary>带聚类树的热图</summary>
Public Class ClusterHeatmapPlot
    Inherits PlotEngine

    ''' <summary>数值矩阵 [row, col]</summary>
    Public Property Matrix As Double(,) = Nothing
    ''' <summary>行名</summary>
    Public Property RowLabels As String() = Nothing
    ''' <summary>列名</summary>
    Public Property ColLabels As String() = Nothing

    ''' <summary>行聚类树（可选）</summary>
    Public Property RowTree As ClusterTreeNode = Nothing
    ''' <summary>列聚类树（可选）</summary>
    Public Property ColTree As ClusterTreeNode = Nothing

    ''' <summary>是否画行聚类树</summary>
    Public Property ShowRowDendrogram As Boolean = True
    ''' <summary>是否画列聚类树</summary>
    Public Property ShowColDendrogram As Boolean = True
    ''' <summary>聚类树占用的画布比例（0~0.4）</summary>
    Public Property TreeFraction As Double = 0.18

    ''' <summary>色阶方案</summary>
    Public Property ColorMap As ColorScale.ColorMapType = ColorScale.ColorMapType.Viridis
    ''' <summary>数据预处理方式（行/列/全局归一化）</summary>
    Public Property Scaling As Geometry.ScaleMode = Geometry.ScaleMode.None
    ''' <summary>是否在格子中写出数值</summary>
    Public Property ShowValues As Boolean = False
    ''' <summary>是否绘制色阶图例条</summary>
    Public Property ShowColorLegend As Boolean = True

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    Public Sub Plot()
        If Matrix Is Nothing Then Throw New InvalidOperationException("ClusterHeatmapPlot requires a matrix.")

        Dim rows = Matrix.GetLength(0)
        Dim cols = Matrix.GetLength(1)
        Dim values = If(Scaling = Geometry.ScaleMode.None, Matrix, Geometry.Normalize(Matrix, Scaling))

        ' 按聚类树叶序重排行列
        Dim rowOrder = Enumerable.Range(0, rows).ToArray()
        Dim colOrder = Enumerable.Range(0, cols).ToArray()
        Dim rowLabels = If(Me.RowLabels, Enumerable.Range(0, rows).Select(Function(i) "R" & (i + 1)).ToArray())
        Dim colLabels = If(Me.ColLabels, Enumerable.Range(0, cols).Select(Function(i) "C" & (i + 1)).ToArray())

        If ShowRowDendrogram AndAlso RowTree IsNot Nothing Then
            rowOrder = Reorder(rowLabels, RowTree.Leaves())
        End If
        If ShowColDendrogram AndAlso ColTree IsNot Nothing Then
            colOrder = Reorder(colLabels, ColTree.Leaves())
        End If

        Dim vmin = MinOf(values), vmax = MaxOf(values)
        If vmax <= vmin Then vmax = vmin + 1

        DrawBackground()
        ComputePlotArea()
        DrawTitle()

        ' ---- 布局：列树在上、行树在左、色条在右 ----
        Dim labelPad As Single = 70
        Dim colTreeH = If(ShowColDendrogram AndAlso ColTree IsNot Nothing, CSng(_plotArea.Height * TreeFraction), 0)
        Dim rowTreeW = If(ShowRowDendrogram AndAlso RowTree IsNot Nothing, CSng(_plotArea.Width * TreeFraction), 0)
        Dim rightPad As Single = If(ShowColorLegend, 80, 10)

        Dim heatX = _plotArea.Left + labelPad + rowTreeW
        Dim heatY = _plotArea.Top + colTreeH
        Dim heatW = std.Max(40.0F, _plotArea.Width - labelPad - rowTreeW - rightPad)
        Dim heatH = std.Max(40.0F, _plotArea.Height - colTreeH)

        ' ---- 聚类树 ----
        Dim lineColor = Theme.AxisColor
        Using pen As New Pen(lineColor, Theme.AxisLineWidth)
            If ShowColDendrogram AndAlso ColTree IsNot Nothing Then
                ClusterTree.Draw(_g, ColTree, New RectangleF(heatX, _plotArea.Top, heatW, colTreeH),
                                 ClusterTree.TreeOrientation.TopToBottom, Theme, pen)
            End If
            If ShowRowDendrogram AndAlso RowTree IsNot Nothing Then
                ClusterTree.Draw(_g, RowTree, New RectangleF(_plotArea.Left + labelPad, heatY, rowTreeW, heatH),
                                 ClusterTree.TreeOrientation.LeftToRight, Theme, pen)
            End If
        End Using

        ' ---- 热图格子 ----
        Dim cellW = heatW / cols
        Dim cellH = heatH / rows

        For ri = 0 To rows - 1
            Dim srcRow = rowOrder(ri)
            For ci = 0 To cols - 1
                Dim srcCol = colOrder(ci)
                Dim v = values(srcRow, srcCol)
                Dim rect = New RectangleF(heatX + ci * cellW, heatY + ri * cellH, cellW, cellH)

                Using br As New SolidBrush(ColorScale.GetColor(v, vmin, vmax, ColorMap))
                    _g.FillRectangle(br, rect)
                End Using

                If ShowValues Then
                    Using br As New SolidBrush(ColorScale.ReadableTextColor(ColorScale.GetColor(v, vmin, vmax, ColorMap)))
                        DrawCentered(Geometry.NumberLabel(v), rect, br)
                    End Using
                End If
            Next
        Next

        Using pen As New Pen(Theme.BorderColor, 0.5F)
            _g.DrawRectangle(pen, heatX, heatY, heatW, heatH)
        End Using

        ' ---- 行列标签 ----
        DrawRowLabels(rowOrder, rowLabels, heatX, heatY, cellH, rows)
        DrawColLabels(colOrder, colLabels, heatX, heatY, cellW, cols)

        If ShowColorLegend Then
            ColorScale.DrawColorLegend(_g, Theme, ColorMap, vmin, vmax,
                                       heatX + heatW + 20, heatY, Theme.ColorBarWidth, heatH)
        End If
    End Sub

    Private Function Reorder(labels As String(), leafOrder As String()) As Integer()
        Dim index As New Dictionary(Of String, Integer)()
        For i = 0 To labels.Length - 1
            index(labels(i)) = i
        Next

        Dim ordered As New List(Of Integer)()
        For Each leaf In leafOrder
            If index.ContainsKey(leaf) AndAlso Not ordered.Contains(index(leaf)) Then
                ordered.Add(index(leaf))
            End If
        Next
        For i = 0 To labels.Length - 1
            If Not ordered.Contains(i) Then ordered.Add(i)
        Next

        Return ordered.ToArray()
    End Function

    Private Sub DrawRowLabels(order As Integer(), labels As String(), x As Single, y As Single,
                              cellH As Single, rows As Integer)
        Using br As New SolidBrush(Theme.TextColor)
            For ri = 0 To rows - 1
                Dim text = labels(order(ri))
                Dim size = MeasureString(text, Theme.TickLabelFont)
                _g.DrawString(text, Theme.TickLabelFont, br, _plotArea.Left + 64 - size.Width,
                              y + (ri + 0.5) * cellH - size.Height / 2)
            Next
        End Using
    End Sub

    Private Sub DrawColLabels(order As Integer(), labels As String(), x As Single, y As Single,
                              cellW As Single, cols As Integer)
        Using br As New SolidBrush(Theme.TextColor)
            For ci = 0 To cols - 1
                Dim text = labels(order(ci))
                Dim size = MeasureString(text, Theme.TickLabelFont)

                _g.TranslateTransform(x + (ci + 0.5) * cellW, y - 4)
                _g.RotateTransform(-45)
                _g.DrawString(text, Theme.TickLabelFont, br, -size.Width, -size.Height / 2)
                _g.ResetTransform()
            Next
        End Using
    End Sub

    Private Sub DrawCentered(text As String, rect As RectangleF, br As Brush)
        Dim size = MeasureString(text, Theme.TickLabelFont)
        _g.DrawString(text, Theme.TickLabelFont, br, rect.X + rect.Width / 2 - size.Width / 2,
                      rect.Y + rect.Height / 2 - size.Height / 2)
    End Sub

    Private Function MinOf(m As Double(,)) As Double
        Dim minV As Double = Double.MaxValue
        For Each v In m.Cast(Of Double)()
            If v < minV Then minV = v
        Next
        Return minV
    End Function

    Private Function MaxOf(m As Double(,)) As Double
        Dim maxV As Double = Double.MinValue
        For Each v In m.Cast(Of Double)()
            If v > maxV Then maxV = v
        Next
        Return maxV
    End Function
End Class
