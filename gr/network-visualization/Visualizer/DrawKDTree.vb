#Region "Microsoft.VisualBasic::5f2a8c1e7d3b4906c9e4a7f2d6b8c1e5, Data_science\Visualization\DataPlot\Basic\DrawKDTree.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class DrawKDTree
    ' 
    '     Properties: K, Query
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Plot
    '     Sub: Plot
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConvexHull
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Linq

' ============================================================================
'  DrawKDTree.vb - KD-Tree 结构图
'
'  原来继承 ChartPlots 的 MustInherit Plot 并依赖它的 Theme/CSS 与 DataScaler。
'  迁移后改为继承 DataPlot 的 <see cref="PlotEngine"/>：坐标轴、网格、坐标变换
'  都走统一引擎，本文件只保留 KD-Tree 自己的连线与最近邻高亮逻辑。
' ============================================================================

''' <summary>KD-Tree 结构的可视化</summary>
Public Class DrawKDTree : Inherits PlotEngine

    ReadOnly tree As KdTree(Of Point2D)
    ReadOnly query As NamedValue(Of PointF)()
    ReadOnly k As Integer

    ''' <summary>树节点的直径</summary>
    Public Property NodeSize As Single = 0
    ''' <summary>最近邻高亮区的放大倍数</summary>
    Public Property HullEnlarge As Double = 1.125
    ''' <summary>最近邻连线的颜色（留空时按查询点描述里的颜色绘制）</summary>
    Public Property LineColor As Color? = Nothing

    Public Sub New(width As Integer, height As Integer,
                   tree As KdTree(Of Point2D), query As NamedValue(Of PointF)(), k As Integer,
                   Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)

        If tree Is Nothing Then Throw New ArgumentNullException(NameOf(tree))

        Me.tree = tree
        Me.query = query
        Me.k = If(k <= 0, 1, k)
    End Sub

    Public Overloads Sub Plot()
        Dim allPoints = tree.GetPoints.ToArray
        If allPoints.Length = 0 Then Throw New InvalidOperationException("The KD-tree has no points.")

        Dim xmin = If(Me.XMin, allPoints.Min(Function(p) p.X))
        Dim xmax = If(Me.XMax, allPoints.Max(Function(p) p.X))
        Dim ymin = If(Me.YMin, allPoints.Min(Function(p) p.Y))
        Dim ymax = If(Me.YMax, allPoints.Max(Function(p) p.Y))

        Geometry.ExpandRange(xmin, xmax, 0.05)
        Geometry.ExpandRange(ymin, ymax, 0.05)

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()
        DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        ' ---- 树的枝干 ----
        Using pen As New Pen(Theme.AxisColor, Theme.LineWidth)
            pen.DashStyle = DashStyle.Dash
            renderTree(pen, tree.rootNode, xmin, xmax, ymin, ymax)
        End Using

        If query.IsNullOrEmpty Then Return

        ' ---- 查询点与其最近邻 ----
        Dim nodeSize As Single = If(NodeSize > 0, NodeSize, Theme.MarkerSize)

        For Each q In query
            Dim color = q.Description.TranslateColor(throwEx:=False)
            If color.IsEmpty Then color = If(LineColor, Theme.Palette(0))
            Dim pos As New PointF(ToPixelX(q.Value.X, xmin, xmax), ToPixelY(q.Value.Y, ymin, ymax))
            Dim knn = tree.nearest(New Point2D(q.Value), k) _
                      .Select(Function(kn)
                                  Dim p = kn.node.data.PointF
                                  Return New PointF(ToPixelX(p.X, xmin, xmax), ToPixelY(p.Y, ymin, ymax))
                              End Function) _
                      .ToArray

            If knn.Length >= 3 Then
                Dim poly = knn.JarvisMatch.Enlarge(HullEnlarge)
                Using br As New SolidBrush(Color.FromArgb(120, color))
                    _g.FillPolygon(br, poly)
                End Using
            End If

            Using br As New SolidBrush(color),
                  pen As New Pen(color, Theme.LineWidth)
                _g.FillEllipse(br, pos.X - nodeSize * 2.5F, pos.Y - nodeSize * 2.5F, nodeSize * 5, nodeSize * 5)
                For Each p In knn
                    _g.DrawEllipse(pen, p.X - nodeSize, p.Y - nodeSize, nodeSize * 2, nodeSize * 2)
                Next
            End Using
        Next
    End Sub

    Private Sub renderTree(pen As Pen, root As KdTreeNode(Of Point2D),
                           xmin As Double, xmax As Double, ymin As Double, ymax As Double)
        If root Is Nothing Then Return

        Dim pos = Translate(root.data.PointF, xmin, xmax, ymin, ymax)
        Dim size = If(NodeSize > 0, NodeSize, Theme.MarkerSize)

        Using br As New SolidBrush(Color.LightGray)
            _g.FillEllipse(br, pos.X - size / 2, pos.Y - size / 2, size, size)
        End Using

        For Each child In {root.left, root.right}
            If child Is Nothing Then Continue For

            Dim pos2 = Translate(child.data.PointF, xmin, xmax, ymin, ymax)
            ' 直角折线：先沿父节点的切分维度走，再折向子节点
            Dim corner As PointF = If(child.dimension = 0,
                                      New PointF(pos2.X, pos.Y),
                                      New PointF(pos.X, pos2.Y))

            _g.DrawLine(pen, pos, corner)
            _g.DrawLine(pen, pos2, corner)

            renderTree(pen, child, xmin, xmax, ymin, ymax)
        Next
    End Sub

    Private Function Translate(p As PointF, xmin As Double, xmax As Double,
                               ymin As Double, ymax As Double) As PointF
        Return New PointF(ToPixelX(p.X, xmin, xmax), ToPixelY(p.Y, ymin, ymax))
    End Function

    ''' <summary>
    ''' 一键绘制 KD-Tree：解析画布尺寸后交给实例方法，最后导出为 <see cref="GraphicsData"/>。
    ''' </summary>
    Public Overloads Shared Function Plot(tree As KdTree(Of Point2D),
                                          Optional query As NamedValue(Of PointF)() = Nothing,
                                          Optional k As Integer = 13,
                                          Optional size As String = "3600,2700",
                                          Optional bg As String = "white",
                                          Optional theme As PlotTheme = Nothing) As GraphicsData
        Dim parts = size.Split(","c)
        If parts.Length < 2 Then Throw New ArgumentException($"Invalid canvas size expression: {size}", NameOf(size))

        Dim w = Integer.Parse(parts(0).Trim)
        Dim h = Integer.Parse(parts(1).Trim)

        Using gd As New DrawKDTree(w, h, tree, query, k, theme)
            gd.Plot()
            Return gd.AsGraphicsData()
        End Using
    End Function
End Class
