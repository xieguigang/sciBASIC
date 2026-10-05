#Region "Microsoft.VisualBasic::5a9c3e7b2d4f4808e1a6d9c3b5f7e2d4, Data_science\Visualization\DataPlot\Statistics\CorrelationHeatmapPlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class CorrelationHeatmapPlot
    ' 
    '     Properties: Correlation, RowTree, ColTree, ShowValues, ShowTrees
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

' ============================================================================
'  CorrelationHeatmapPlot.vb - 相关性矩阵热图
'
'  相关性系数由调用方算好放进 <see cref="CorrelationMatrix"/>（Pearson/Spearman
'  等任意度量均可）。本类只负责：对称零中心的色阶、行列名、可选的两侧聚类树。
' ============================================================================

''' <summary>相关性矩阵热图</summary>
Public Class CorrelationHeatmapPlot
    Inherits PlotEngine

    ''' <summary>相关性矩阵</summary>
    Public Property Correlation As CorrelationMatrix = Nothing
    ''' <summary>行 / 列的聚类树（可选）</summary>
    Public Property RowTree As ClusterTreeNode = Nothing
    ''' <summary>是否画聚类树（两侧都画）</summary>
    Public Property ShowTrees As Boolean = True
    ''' <summary>聚类树占用的画布比例</summary>
    Public Property TreeFraction As Double = 0.16
    ''' <summary>是否在格子中写出系数</summary>
    Public Property ShowValues As Boolean = False
    ''' <summary>色阶：默认用零中心发散色阶</summary>
    Public Property ColorMap As ColorScale.ColorMapType = ColorScale.ColorMapType.CoolWarm
    ''' <summary>系数的值域（通常为 -1~1）</summary>
    Public Property ValueRange As (min As Double, max As Double) = (-1, 1)

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    ''' <summary>直接在外部提供的绘图设备上绘制（图层叠加模式 / 宿主画布）。</summary>
    Public Sub New(g As IGraphics, Optional theme As PlotTheme = Nothing)
        MyBase.New(g, theme)
    End Sub

    Public Sub Plot()
        If Correlation Is Nothing OrElse Not Correlation.ValidateShape() Then
            Throw New InvalidOperationException("A well-shaped correlation matrix is required.")
        End If

        Dim names = Correlation.Names
        Dim m = Correlation.Matrix
        Dim n = names.Length

        DrawBackground()
        ComputePlotArea()
        DrawTitle()

        Dim labelPad As Single = 70
        Dim treeW = If(ShowTrees AndAlso RowTree IsNot Nothing, CSng(_plotArea.Width * TreeFraction), 0)
        Dim heatX = _plotArea.Left + labelPad + treeW
        Dim heatY = _plotArea.Top
        Dim heatSide = std.Min(_plotArea.Width - labelPad - treeW - 80, _plotArea.Height)
        Dim cellW = heatSide / n
        Dim cellH = heatSide / n

        If ShowTrees AndAlso RowTree IsNot Nothing Then
            Using pen As New Pen(Theme.AxisColor, Theme.AxisLineWidth)
                ClusterTree.Draw(_g, RowTree, New RectangleF(_plotArea.Left + labelPad, heatY, treeW, heatSide),
                                 ClusterTree.TreeOrientation.LeftToRight, Theme, pen)
            End Using
        End If

        For i = 0 To n - 1
            For j = 0 To n - 1
                Dim v = m(i, j)
                Dim rect = New RectangleF(heatX + j * cellW, heatY + i * cellH, cellW, cellH)

                Using br As New SolidBrush(ColorScale.GetColor(v, ValueRange.min, ValueRange.max, ColorMap))
                    _g.FillRectangle(br, rect)
                End Using

                If ShowValues Then
                    Dim size As Single = If(cellW < 40, 7.0F, 9.0F)
                    Dim text = v.ToString("F2")
                    Dim measured = MeasureString(text, Theme.TickLabelFont)
                    Using br As New SolidBrush(ColorScale.ReadableTextColor(
                            ColorScale.GetColor(v, ValueRange.min, ValueRange.max, ColorMap)))
                        _g.DrawString(text, Theme.TickLabelFont, br,
                                      rect.X + rect.Width / 2 - measured.Width / 2,
                                      rect.Y + rect.Height / 2 - measured.Height / 2)
                    End Using
                End If
            Next
        Next

        Using pen As New Pen(Theme.BorderColor, 0.5F)
            _g.DrawRectangle(pen, heatX, heatY, heatSide, heatSide)
        End Using

        DrawLabels(names, heatX, heatY, cellW, cellH, n)

        ColorScale.DrawColorLegend(_g, Theme, ColorMap, ValueRange.min, ValueRange.max,
                                   heatX + heatSide + 20, heatY, Theme.ColorBarWidth, heatSide,
                                   title:="correlation")
    End Sub

    Private Sub DrawLabels(names As String(), x As Single, y As Single, cellW As Single, cellH As Single, n As Integer)
        Using br As New SolidBrush(Theme.TextColor)
            For i = 0 To n - 1
                Dim text = names(i)
                Dim size = MeasureString(text, Theme.TickLabelFont)

                _g.DrawString(text, Theme.TickLabelFont, br, _plotArea.Left + 64 - size.Width,
                              y + (i + 0.5) * cellH - size.Height / 2)

                _g.TranslateTransform(x + (i + 0.5) * cellW, y - 4)
                _g.RotateTransform(-45)
                _g.DrawString(text, Theme.TickLabelFont, br, -size.Width, -size.Height / 2)
                _g.ResetTransform()
            Next
        End Using
    End Sub
End Class
