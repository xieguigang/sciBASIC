#Region "Microsoft.VisualBasic::7c2e5a9b4f6b4910a3c8f2e5d7b9a4f6, Data_science\Visualization\DataPlot\Statistics\BubbleHeatmapPlot.vb"

' 
'       sciBASIC.NET Foundation, GPL3 Licensed
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.

' Class BubbleHeatmapPlot
' 
'     Properties: Matrix, RowLabels, ColLabels, ColorMap, ShowColorLegend
'                 MinRadiusFraction
' 
'     Constructor: (+1 Overloads) Sub New
'     Sub: Plot
' 
#End Region

Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports std = System.Math

' ============================================================================
'  BubbleHeatmapPlot.vb - 泡泡热图
'
'  旧实现 Heatmap/BubbleHeatmap.vb 的函数体第一行就是 Throw NotImplementedException。
'  这里按标准泡泡矩阵补齐：行、列构成格子，每个格子用一个圆点表示，
'  半径编码数值大小、颜色编码数值正负 / 高低，适合稀疏或强调量级的矩阵。
' ============================================================================

''' <summary>泡泡热图</summary>
Public Class BubbleHeatmapPlot : Inherits PlotEngine

    ''' <summary>数值矩阵 [row, col]</summary>
    Public Property Matrix As Double(,) = Nothing
    ''' <summary>行名</summary>
    Public Property RowLabels As String() = Nothing
    ''' <summary>列名</summary>
    Public Property ColLabels As String() = Nothing
    ''' <summary>色阶方案</summary>
    Public Property ColorMap As ScalerPalette = ScalerPalette.viridis
    ''' <summary>是否绘制色阶图例条</summary>
    Public Property ShowColorLegend As Boolean = True
    ''' <summary>最大圆点直径占格子的比例</summary>
    Public Property MaxBubbleFraction As Double = 0.9
    ''' <summary>是否在圆点旁写出数值</summary>
    Public Property ShowValues As Boolean = False
    ''' <summary>是否用数值绝对值决定半径（否则直接用原值，要求非负）</summary>
    Public Property UseAbsoluteRadius As Boolean = True

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    ''' <summary>直接在外部提供的绘图设备上绘制（图层叠加模式 / 宿主画布）。</summary>
    Public Sub New(g As IGraphics, Optional theme As PlotTheme = Nothing)
        MyBase.New(g, theme)
    End Sub

    Public Sub Plot()
        If Matrix Is Nothing Then Throw New InvalidOperationException("BubbleHeatmapPlot requires a matrix.")

        Dim rows = Matrix.GetLength(0)
        Dim cols = Matrix.GetLength(1)

        DrawBackground()
        ComputePlotArea()
        DrawTitle()

        Dim vmin = MinValue()
        Dim vmax = MaxValue()
        If vmax <= vmin Then vmax = vmin + 1

        Dim labelPad As Single = 80
        Dim rightPad As Single = If(ShowColorLegend, 80, 10)
        Dim x0 = _plotArea.Left + labelPad
        Dim y0 = _plotArea.Top
        Dim w = std.Max(40.0F, _plotArea.Width - labelPad - rightPad)
        Dim h = std.Max(40.0F, _plotArea.Height)
        Dim cellW = w / cols
        Dim cellH = h / rows
        Dim maxCell = std.Min(cellW, cellH)

        ' 半径 scale：用绝对值或原值的最大值归一
        Dim radiusMax = 0.0
        For r = 0 To rows - 1
            For c = 0 To cols - 1
                Dim v = If(UseAbsoluteRadius, std.Abs(Matrix(r, c)), Matrix(r, c))
                If v > radiusMax Then radiusMax = v
            Next
        Next
        If radiusMax <= 0 Then radiusMax = 1

        For r = 0 To rows - 1
            For c = 0 To cols - 1
                Dim v = Matrix(r, c)
                Dim magnitude = If(UseAbsoluteRadius, std.Abs(v), v)
                Dim diameter = CSng(magnitude / radiusMax * maxCell * MaxBubbleFraction)
                If diameter <= 0.4F Then Continue For

                Dim cx As Single = CSng(x0 + (c + 0.5) * cellW)
                Dim cy As Single = CSng(y0 + (r + 0.5) * cellH)
                Dim color = ColorScale.GetColor(v, vmin, vmax, ColorMap)

                Using br As New SolidBrush(color),
                      pen As New Pen(Theme.BorderColor, 0.5F)
                    _g.FillEllipse(br, cx - diameter / 2, cy - diameter / 2, diameter, diameter)
                    _g.DrawEllipse(pen, cx - diameter / 2, cy - diameter / 2, diameter, diameter)
                End Using

                If ShowValues AndAlso diameter > 18 Then
                    Dim text = Geometry.NumberLabel(v)
                    Dim size = MeasureString(text, Theme.TickLabelFont)
                    Using br As New SolidBrush(ColorScale.ReadableTextColor(color))
                        _g.DrawString(text, Theme.TickLabelFont, br, cx - size.Width / 2, cy - size.Height / 2)
                    End Using
                End If
            Next
        Next

        DrawLabels(rows, cols, x0, y0, cellW, cellH)

        If ShowColorLegend Then
            ColorScale.DrawColorLegend(_g, Theme, ColorMap, vmin, vmax,
                                       x0 + w + 20, y0, Theme.ColorBarWidth, h)
        End If
    End Sub

    Private Sub DrawLabels(rows As Integer, cols As Integer, x0 As Single, y0 As Single,
                           cellW As Single, cellH As Single)
        Using br As New SolidBrush(Theme.TextColor)
            If RowLabels IsNot Nothing Then
                For r = 0 To std.Min(rows, RowLabels.Length) - 1
                    Dim size = MeasureString(RowLabels(r), Theme.TickLabelFont)
                    _g.DrawString(RowLabels(r), Theme.TickLabelFont, br, x0 - size.Width - 6,
                                  y0 + (r + 0.5) * cellH - size.Height / 2)
                Next
            End If

            If ColLabels IsNot Nothing Then
                For c = 0 To std.Min(cols, ColLabels.Length) - 1
                    Dim size = MeasureString(ColLabels(c), Theme.TickLabelFont)
                    _g.TranslateTransform(x0 + (c + 0.5) * cellW, y0 - 4)
                    _g.RotateTransform(-45)
                    _g.DrawString(ColLabels(c), Theme.TickLabelFont, br, -size.Width, -size.Height / 2)
                    _g.ResetTransform()
                Next
            End If
        End Using
    End Sub

    Private Function MinValue() As Double
        Dim minV As Double = Double.MaxValue
        For Each v In Matrix.Cast(Of Double)()
            If v < minV Then minV = v
        Next
        Return minV
    End Function

    Private Function MaxValue() As Double
        Dim maxV As Double = Double.MinValue
        For Each v In Matrix.Cast(Of Double)()
            If v > maxV Then maxV = v
        Next
        Return maxV
    End Function
End Class
