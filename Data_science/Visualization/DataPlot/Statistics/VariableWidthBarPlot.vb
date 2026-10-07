#Region "Microsoft.VisualBasic::2c6a9d3f8b4e4914e7a3c6d9f2b5e8d1, Data_science\Visualization\DataPlot\Statistics\VariableWidthBarPlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class VariableWidthBarPlot
    ' 
    '     Properties: Bars, ShowValueLabels, ShowLegendImpl, ColorMap, UseColorScale
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

' ============================================================================
'  VariableWidthBarPlot.vb - 变宽条形图
'
'  与普通柱状图的区别：每根柱子的<b>宽度</b>本身也承载信息（常常是样本量），
'  高度是主指标。横轴的刻度落在每根柱子的区间端点上，而不是柱子中心。
' ============================================================================

''' <summary>变宽条形图</summary>
Public Class VariableWidthBarPlot
    Inherits PlotEngine

    ''' <summary>每一根柱子</summary>
    Public Property Bars As List(Of VariableBarData) = New List(Of VariableBarData)()
    ''' <summary>是否在柱子上方写出数值</summary>
    Public Property ShowValueLabels As Boolean = True
    ''' <summary>是否用色阶给柱子上色（按高度）</summary>
    Public Property UseColorScale As Boolean = False
    ''' <summary>色阶方案</summary>
    Public Property ColorMap As ScalerPalette = ScalerPalette.viridis
    ''' <summary>是否绘制色阶图例条</summary>
    Public Property ShowColorLegend As Boolean = True
    ''' <summary>是否在横轴上写出柱子的名字</summary>
    Public Property ShowBarNames As Boolean = True

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    ''' <summary>直接在外部提供的绘图设备上绘制（图层叠加模式 / 宿主画布）。</summary>
    Public Sub New(g As IGraphics, Optional theme As PlotTheme = Nothing)
        MyBase.New(g, theme)
    End Sub

    Public Sub Plot()
        If Bars Is Nothing OrElse Bars.Count = 0 Then
            Throw New InvalidOperationException("VariableWidthBarPlot requires at least one bar.")
        End If

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim widthSum = Bars.Sum(Function(b) std.Max(b.Width, 1))
        Dim vmax = Bars.Max(Function(b) b.Value)
        If vmax <= 0 Then vmax = 1
        vmax *= 1.08

        Dim rightPad As Single = If(UseColorScale AndAlso ShowColorLegend, 90, 20)
        Dim region As New RectangleF(_plotArea.Left, _plotArea.Top,
                                     std.Max(40.0F, _plotArea.Width - rightPad),
                                     _plotArea.Height - 40.0F)
        _plotArea = region

        ' ---- 横轴与基线 ----
        Using pen As New Pen(Theme.AxisColor, Theme.AxisLineWidth)
            _g.DrawLine(pen, region.Left, region.Bottom, region.Right, region.Bottom)
            _g.DrawLine(pen, region.Left, region.Top, region.Left, region.Bottom)
        End Using

        ' ---- Y 轴刻度 ----
        Dim yTicks = Geometry.NiceTicks(0, vmax)
        Using pen As New Pen(Theme.GridColor, Theme.GridLineWidth),
              br As New SolidBrush(Theme.TextColor)
            If Theme.ShowGrid Then
                For Each t In yTicks
                    Dim py = ToPixelY(t, 0, vmax)
                    _g.DrawLine(pen, region.Left, py, region.Right, py)
                Next
            End If
            For Each t In yTicks
                Dim py = ToPixelY(t, 0, vmax)
                Dim text = Geometry.NumberLabel(t)
                Dim size = MeasureString(text, Theme.TickLabelFont)
                _g.DrawString(text, Theme.TickLabelFont, br, region.Left - size.Width - 6, py - size.Height / 2)
            Next
        End Using

        ' ---- 逐根柱子：宽度按累计宽度分配 ----
        Dim cursor As Single = region.Left
        Dim vmin = Bars.Min(Function(b) b.Value)

        For i = 0 To Bars.Count - 1
            Dim bar = Bars(i)
            Dim w = CSng(std.Max(bar.Width, 1) / widthSum * region.Width)
            Dim color = If(bar.Color, If(UseColorScale,
                                         ColorScale.GetColor(bar.Value, vmin, vmax, ColorMap),
                                         Theme.Palette(i Mod Theme.Palette.Length)))
            Dim top = ToPixelY(bar.Value, 0, vmax)
            Dim h = region.Bottom - top

            Using br As New SolidBrush(color),
                  pen As New Pen(Theme.BorderColor, 0.5F)
                _g.FillRectangle(br, cursor, top, w, h)
                _g.DrawRectangle(pen, cursor, top, w, h)
            End Using

            If ShowValueLabels Then
                Dim text = Geometry.NumberLabel(bar.Value)
                Dim size = MeasureString(text, Theme.TickLabelFont)
                Using br As New SolidBrush(Theme.TextColor)
                    _g.DrawString(text, Theme.TickLabelFont, br, cursor + w / 2 - size.Width / 2, top - size.Height - 2)
                End Using
            End If

            If ShowBarNames AndAlso w > 20 Then
                Using br As New SolidBrush(Theme.TextColor)
                    Dim size = MeasureString(bar.Name, Theme.TickLabelFont)
                    _g.DrawString(bar.Name, Theme.TickLabelFont, br, cursor + w / 2 - size.Width / 2, region.Bottom + 6)
                End Using
            End If

            cursor += w
        Next

        If UseColorScale AndAlso ShowColorLegend Then
            ColorScale.DrawColorLegend(_g, Theme, ColorMap, vmin, vmax,
                                       region.Right + 20, region.Top, Theme.ColorBarWidth, region.Height)
        End If
    End Sub
End Class
