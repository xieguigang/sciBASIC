#Region "Microsoft.VisualBasic::c7b1e9d4a2f64803e8c5b1d9f3a7c2e5, Data_science\Visualization\DataPlot\Statistics\ForestPlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class ForestPlot
    ' 
    '     Properties: Entries, NullLine, ShowLabelColumn, ShowEntriesColumn
    '                 Header1, Header2
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports System.Runtime.CompilerServices
Imports std = System.Math

' ============================================================================
'  ForestPlot.vb - 森林图
'
'  旧项目里的 ForestPlot.vb 只有一个空 Module（带 XML 注释，没有任何实现），
'  这里是按标准森林图补齐：左边表格列写研究标签与效应量文本，中间是点估计 + 置信区间，
'  一条零效应参考线穿过所有行。
' ============================================================================

''' <summary>森林图（效应量与置信区间）</summary>
Public Class ForestPlot
    Inherits PlotEngine

    ''' <summary>每一行一个条目</summary>
    Public Property Entries As List(Of ForestEntry) = New List(Of ForestEntry)()
    ''' <summary>零效应参考线的位置</summary>
    Public Property NullLine As Double = 0
    ''' <summary>是否在左侧额外列出「效应量 (95% CI)」文本列</summary>
    Public Property ShowValueColumn As Boolean = True
    ''' <summary>数值列表头</summary>
    Public Property ValueColumnHeader As String = "Effect (95% CI)"
    ''' <summary>标签列表头</summary>
    Public Property LabelColumnHeader As String = "Study"
    ''' <summary>点估计标记颜色</summary>
    Public Property MarkerColor As Color? = Nothing

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        If Entries Is Nothing OrElse Entries.Count = 0 Then
            Throw New InvalidOperationException("ForestPlot requires at least one entry.")
        End If

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        ' ---- 左侧表格列宽度 ----
        Dim labelWidth As Single = 0
        Dim valueWidth As Single = 0

        For Each e In Entries
            labelWidth = std.Max(labelWidth, MeasureString(e.Label, Theme.TickLabelFont).Width)
            If ShowValueColumn Then
                valueWidth = std.Max(valueWidth, MeasureString(EffectText(e), Theme.TickLabelFont).Width)
            End If
        Next

        labelWidth = std.Max(labelWidth + 12, MeasureString(LabelColumnHeader, Theme.LegendFont).Width + 12)
        If ShowValueColumn Then
            valueWidth = std.Max(valueWidth + 12, MeasureString(ValueColumnHeader, Theme.LegendFont).Width + 12)
        Else
            valueWidth = 0
        End If

        ' ---- 数值值域（含置信区间） ----
        Dim lo As Double = Double.MaxValue, hi As Double = Double.MinValue
        For Each e In Entries
            If Not Double.IsNaN(e.Lower) Then lo = std.Min(lo, e.Lower)
            If Not Double.IsNaN(e.Upper) Then hi = std.Max(hi, e.Upper)
            lo = std.Min(lo, e.Effect)
            hi = std.Max(hi, e.Effect)
        Next
        lo = std.Min(lo, NullLine)
        hi = std.Max(hi, NullLine)

        Dim pad = (hi - lo) * 0.12
        If pad <= 0 Then pad = 1
        lo -= pad : hi += pad

        ' ---- 布局：把绘图区收缩到「表格右侧」，后续的 ToPixelX 才会落在该区域内 ----
        Dim tableW = labelWidth + valueWidth
        Dim rowH = std.Min(_plotArea.Height / (Entries.Count + 1), 48.0F)

        _plotArea = New RectangleF(_plotArea.Left + tableW, _plotArea.Top + rowH,
                                   std.Max(60.0F, _plotArea.Width - tableW),
                                   std.Max(rowH, _plotArea.Height - rowH))

        DrawColumnHeaders(rowH, labelWidth)

        ' ---- 零效应参考线 ----
        Dim nullX = ToPixelX(NullLine, lo, hi)

        Using pen As New Pen(Theme.AxisColor, Theme.LineWidth)
            pen.DashStyle = DashStyle.Dash
            _g.DrawLine(pen, nullX, _plotArea.Top, nullX, _plotArea.Top + rowH * Entries.Count)
        End Using

        Dim color = If(MarkerColor, Theme.Palette(0))

        For i = 0 To Entries.Count - 1
            Dim e = Entries(i)
            Dim top = _plotArea.Top + rowH * i
            Dim cy = top + rowH / 2

            DrawRowText(e, top, rowH, labelWidth)
            DrawEffect(e, cy, rowH, lo, hi, color)
        Next
    End Sub

    Private Sub DrawColumnHeaders(rowH As Single, labelWidth As Single)
        Dim top = _plotArea.Top - rowH
        Using br As New SolidBrush(Theme.TextColor)
            _g.DrawString(LabelColumnHeader, Theme.LegendFont, br, _plotArea.Left, top + rowH / 2 - 6)
            If ShowValueColumn Then
                _g.DrawString(ValueColumnHeader, Theme.LegendFont, br, _plotArea.Left + labelWidth, top + rowH / 2 - 6)
            End If
        End Using
    End Sub

    Private Sub DrawRowText(e As ForestEntry, top As Single, rowH As Single, labelWidth As Single)
        Using br As New SolidBrush(Theme.TextColor)
            _g.DrawString(e.Label, Theme.TickLabelFont, br, _plotArea.Left, top + rowH / 2 - 7)
            If ShowValueColumn Then
                _g.DrawString(EffectText(e), Theme.TickLabelFont, br, _plotArea.Left + labelWidth, top + rowH / 2 - 7)
            End If
        End Using
    End Sub

    Private Sub DrawEffect(e As ForestEntry, cy As Single, rowH As Single,
                           lo As Double, hi As Double, color As Color)
        Dim px = ToPixelX(e.Effect, lo, hi)
        Dim hasCI = Not Double.IsNaN(e.Lower) AndAlso Not Double.IsNaN(e.Upper)

        If hasCI Then
            Dim pxL = ToPixelX(e.Lower, lo, hi)
            Dim pxR = ToPixelX(e.Upper, lo, hi)

            Using pen As New Pen(color, Theme.LineWidth)
                _g.DrawLine(pen, pxL, cy, pxR, cy)
                ' 区间两端的短竖线
                Dim cap = rowH * 0.18F
                _g.DrawLine(pen, pxL, cy - cap, pxL, cy + cap)
                _g.DrawLine(pen, pxR, cy - cap, pxR, cy + cap)
            End Using
        End If

        ' 方块尺寸可以承载权重（meta 分析里越重的样本块越大）
        Dim boxW = rowH * 0.42F
        If e.Weight > 0 Then
            Dim maxW = Entries.Max(Function(x) If(x.Weight > 0, x.Weight, 1))
            boxW = CSng(rowH * 0.2F + rowH * 0.3F * (e.Weight / maxW))
        End If

        Using br As New SolidBrush(color)
            _g.FillRectangle(br, px - boxW / 2, cy - boxW / 2, boxW, boxW)
        End Using
    End Sub

    Private Function EffectText(e As ForestEntry) As String
        If Double.IsNaN(e.Lower) OrElse Double.IsNaN(e.Upper) Then
            Return Geometry.NumberLabel(e.Effect)
        End If
        Return $"{Geometry.NumberLabel(e.Effect)} [{Geometry.NumberLabel(e.Lower)}, {Geometry.NumberLabel(e.Upper)}]"
    End Function
End Class
