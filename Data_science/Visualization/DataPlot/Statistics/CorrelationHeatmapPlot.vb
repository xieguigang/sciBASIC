#Region "Microsoft.VisualBasic::13ea77985a450e03ef7ccd64c7205e54, Data_science\Visualization\DataPlot\Statistics\CorrelationHeatmapPlot.vb"

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

    '   Total Lines: 120
    '    Code Lines: 86 (71.67%)
    ' Comment Lines: 15 (12.50%)
    '    - Xml Docs: 60.00%
    ' 
    '   Blank Lines: 19 (15.83%)
    '     File Size: 5.54 KB


    ' Class CorrelationHeatmapPlot
    ' 
    '     Properties: ColorMap, Correlation, RowTree, ShowTrees, ShowValues
    '                 TreeFraction, ValueRange
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Sub: DrawLabels, Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
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
    Public Property ColorMap As ScalerPalette = ScalerPalette.Cool
    ''' <summary>系数的值域（通常为 -1~1）</summary>
    Public Property ValueRange As (min As Double, max As Double) = (-1, 1)

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    ''' <summary>直接在外部提供的绘图设备上绘制（图层叠加模式 / 宿主画布）。</summary>
    Public Sub New(g As IGraphics, Optional theme As PlotTheme = Nothing)
        MyBase.New(g, theme)
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
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
