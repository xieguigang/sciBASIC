' ---------------------------------------------------------------------------
'  DataPlot / Engine / ColorScale.vb
'  Copyright (c) 2018-2026 sciBASIC.NET Foundation, GPL3 Licensed
'
'  This program is free software: you can redistribute it and/or modify
'  it under the terms of the GNU General Public License as published by
'  the Free Software Foundation, either version 3 of the License, or
'  (at your option) any later version.
' ---------------------------------------------------------------------------

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

''' <summary>
''' 色阶：把数值映射到颜色，并绘制与之配套的颜色条图例。
''' 热图、等高线图、密度图、相关性矩阵、Z-score 图、变宽条形图等所有需要色阶的图表
''' 共用这一份实现，避免每种图各自写一遍取色与 colorbar 绘制。
''' </summary>
Public Module ColorScale

    ''' <summary>内置色阶方案</summary>
    Public Enum ColorMapType
        Viridis
        Plasma
        Inferno
        CoolWarm
        Grayscale
        Jet
    End Enum

    Private ReadOnly viridis As Color() = {
        Color.FromArgb(68, 1, 84), Color.FromArgb(59, 82, 139),
        Color.FromArgb(33, 145, 140), Color.FromArgb(94, 201, 98),
        Color.FromArgb(253, 231, 37)
    }

    Private ReadOnly plasma As Color() = {
        Color.FromArgb(13, 8, 135), Color.FromArgb(126, 3, 168),
        Color.FromArgb(204, 71, 120), Color.FromArgb(248, 149, 64),
        Color.FromArgb(240, 249, 33)
    }

    Private ReadOnly inferno As Color() = {
        Color.FromArgb(0, 0, 4), Color.FromArgb(87, 16, 110),
        Color.FromArgb(187, 55, 84), Color.FromArgb(249, 142, 9),
        Color.FromArgb(252, 255, 164)
    }

    Private ReadOnly coolwarm As Color() = {
        Color.FromArgb(59, 76, 192), Color.FromArgb(221, 221, 221),
        Color.FromArgb(180, 4, 38)
    }

    Private ReadOnly jet As Color() = {
        Color.FromArgb(0, 0, 131), Color.FromArgb(0, 60, 170),
        Color.FromArgb(5, 255, 255), Color.FromArgb(255, 255, 0),
        Color.FromArgb(250, 0, 0), Color.FromArgb(128, 0, 0)
    }

    ''' <summary>取色阶的控制点</summary>
    Public Function GetPalette(cmap As ColorMapType) As Color()
        Select Case cmap
            Case ColorMapType.Viridis : Return viridis
            Case ColorMapType.Plasma : Return plasma
            Case ColorMapType.Inferno : Return inferno
            Case ColorMapType.CoolWarm : Return coolwarm
            Case ColorMapType.Jet : Return jet
            Case Else : Return viridis
        End Select
    End Function

    ''' <summary>按名称取色阶；无法识别时回落到 <see cref="ColorMapType.Viridis"/></summary>
    Public Function ParseColorMap(name As String, Optional fallback As ColorMapType = ColorMapType.Viridis) As ColorMapType
        If String.IsNullOrWhiteSpace(name) Then Return fallback

        Select Case name.Trim.ToLower
            Case "viridis" : Return ColorMapType.Viridis
            Case "plasma" : Return ColorMapType.Plasma
            Case "inferno" : Return ColorMapType.Inferno
            Case "coolwarm", "cool-warm" : Return ColorMapType.CoolWarm
            Case "grayscale", "gray", "grey" : Return ColorMapType.Grayscale
            Case "jet" : Return ColorMapType.Jet
            Case Else : Return fallback
        End Select
    End Function

    ''' <summary>把 [0,1] 上的位置插值为具体颜色</summary>
    Public Function LerpPalette(t As Double, colors As Color()) As Color
        If colors Is Nothing OrElse colors.Length = 0 Then Return Color.White
        If colors.Length = 1 Then Return colors(0)

        t = std.Max(0, std.Min(1, t))

        Dim n = colors.Length - 1
        Dim pos = t * n
        Dim i = CInt(std.Floor(pos))
        If i < 0 Then i = 0
        If i >= n Then i = n - 1

        Dim f = pos - i
        Dim c1 = colors(i), c2 = colors(i + 1)
        Dim r = std.Clamp(CDbl(c1.R) + (CDbl(c2.R) - CDbl(c1.R)) * f, 0, 255)
        Dim g = std.Clamp(CDbl(c1.G) + (CDbl(c2.G) - CDbl(c1.G)) * f, 0, 255)
        Dim b = std.Clamp(CDbl(c1.B) + (CDbl(c2.B) - CDbl(c1.B)) * f, 0, 255)

        Return Color.FromArgb(CInt(r), CInt(g), CInt(b))
    End Function

    ''' <summary>把 v 在 [vmin,vmax] 上归一化后取色</summary>
    Public Function GetColor(v As Double, vmin As Double, vmax As Double, cmap As ColorMapType) As Color
        Dim t As Double

        If vmax <= vmin OrElse Double.IsNaN(v) Then
            t = 0
        Else
            t = (v - vmin) / (vmax - vmin)
        End If

        Return GetColorT(t, cmap)
    End Function

    ''' <summary>直接按 0~1 的位置取色（越界会被裁剪）</summary>
    Public Function GetColorT(t As Double, cmap As ColorMapType) As Color
        If cmap = ColorMapType.Grayscale Then
            Dim gs = CInt(std.Clamp(t, 0, 1) * 255)
            Return Color.FromArgb(gs, gs, gs)
        End If

        Return LerpPalette(t, GetPalette(cmap))
    End Function

    ''' <summary>感知亮度（0 黑 ~ 1 白），用于挑选叠在该底色上的文字颜色</summary>
    Public Function Brightness(c As Color) As Double
        Return (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255
    End Function

    ''' <summary>给定底色，返回可读的前景色</summary>
    Public Function ReadableTextColor(c As Color) As Color
        Return If(Brightness(c) > 0.5, Color.Black, Color.White)
    End Function

    ''' <summary>
    ''' 绘制颜色条图例（垂直或水平）。
    ''' </summary>
    ''' <param name="g">绘图设备</param>
    ''' <param name="theme">主题（决定字体与相关颜色）</param>
    ''' <param name="cmap">色阶方案</param>
    ''' <param name="vmin">色阶下界</param>
    ''' <param name="vmax">色阶上界</param>
    ''' <param name="x">颜色条左上角 x</param>
    ''' <param name="y">颜色条左上角 y</param>
    ''' <param name="w">颜色条长度（水平模式）或宽度（垂直模式）</param>
    ''' <param name="h">颜色条宽度（水平模式）或长度（垂直模式）</param>
    ''' <param name="horizontal">True 时横向绘制</param>
    ''' <param name="tickCount">刻度数量</param>
    ''' <param name="title">颜色条标题（可留空）</param>
    Public Sub DrawColorLegend(g As IGraphics, theme As PlotTheme, cmap As ColorMapType,
                               vmin As Double, vmax As Double,
                               x As Single, y As Single, w As Single, h As Single,
                               Optional horizontal As Boolean = False,
                               Optional tickCount As Integer = 5,
                               Optional title As String = "")
        If g Is Nothing Then Throw New ArgumentNullException(NameOf(g))
        If theme Is Nothing Then Throw New ArgumentNullException(NameOf(theme))
        If w <= 0 OrElse h <= 0 Then Return

        ' ---- 渐变条本体：逐像素填充 ----
        If horizontal Then
            Dim n = CInt(w)
            For i = 0 To n - 1
                Dim t = i / CDbl(n)
                Using br As New SolidBrush(GetColorT(t, cmap))
                    g.FillRectangle(br, x + i, y, 1.0F, h)
                End Using
            Next
        Else
            Dim n = CInt(h)
            For i = 0 To n - 1
                ' 顶部是最大值
                Dim t = 1 - i / CDbl(n)
                Using br As New SolidBrush(GetColorT(t, cmap))
                    g.FillRectangle(br, x, y + i, w, 1.0F)
                End Using
            Next
        End If

        ' ---- 边框 ----
        Using pen As New Pen(theme.BorderColor, 0.7F)
            g.DrawRectangle(pen, x, y, w, h)
        End Using

        ' ---- 刻度与标签 ----
        Dim ticks = Geometry.NiceTicks(vmin, vmax, tickCount)

        Using br As New SolidBrush(theme.TextColor),
              pen As New Pen(theme.AxisColor, 0.7F)
            For Each tv In ticks
                If tv < vmin OrElse tv > vmax Then Continue For

                Dim pos As Single = If(vmax = vmin, 0, CSng((tv - vmin) / (vmax - vmin)))
                Dim label = Geometry.NumberLabel(tv)
                Dim size = g.MeasureString(label, theme.TickLabelFont)

                If horizontal Then
                    Dim px = x + pos * w
                    g.DrawLine(pen, px, y + h, px, y + h + 4)
                    Dim lx = std.Max(0, px - size.Width / 2)
                    g.DrawString(label, theme.TickLabelFont, br, lx, y + h + 6)
                Else
                    ' 下方为最小值
                    Dim py = y + h - pos * h
                    g.DrawLine(pen, x + w, py, x + w + 4, py)
                    g.DrawString(label, theme.TickLabelFont, br, x + w + 6, py - size.Height / 2)
                End If
            Next
        End Using

        ' ---- 标题 ----
        If Not String.IsNullOrEmpty(title) Then
            Using br As New SolidBrush(theme.TextColor)
                Dim size = g.MeasureString(title, theme.LegendFont)
                If horizontal Then
                    g.DrawString(title, theme.LegendFont, br, x + w / 2 - size.Width / 2, y - size.Height - 2)
                Else
                    g.DrawString(title, theme.LegendFont, br, x - size.Width / 2 + w / 2, y - size.Height - 2)
                End If
            End Using
        End If
    End Sub
End Module
