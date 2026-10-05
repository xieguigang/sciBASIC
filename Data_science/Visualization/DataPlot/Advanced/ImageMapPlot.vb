#Region "Microsoft.VisualBasic::9f2c4b7d1a3e4805b6c8e2f7a1d3b9c4, Data_science\Visualization\DataPlot\Advanced\ImageMapPlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class ImageMapPlot
    ' 
    '     Properties: Image, ColorMap, ShowColorLegend, ShowFrame, MinValue, MaxValue
    '                 OverlayShapes
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

' ============================================================================
'  ImageMapPlot.vb - 位图数据场（二维强度图）
'
'  对应旧实现里 ImageData.vb 的二维部分（Image2DMap）：把一张图像 / 一个二维数据场
'  铺满画布并按色阶上色。三维部分（Image3DMap）按本次约定不迁移，仍留在 Canvas3D。
'
'  注意：这里接收的是「已经采样好的二维标量场」，而不是图像文件本身——
'  读像素属于 IO 职责，交给调用方可以避免绘图项目背上图像解码的依赖。
' ============================================================================

''' <summary>二维数据场强度图</summary>
Public Class ImageMapPlot
    Inherits PlotEngine

    ''' <summary>二维标量场（行 = y，列 = x）</summary>
    Public Property Image As Double(,) = Nothing
    ''' <summary>色阶方案</summary>
    Public Property ColorMap As ColorScale.ColorMapType = ColorScale.ColorMapType.Inferno
    ''' <summary>色阶下界（留空时自动取最小值）</summary>
    Public Property MinValue As Double? = Nothing
    ''' <summary>色阶上界（留空时自动取最大值）</summary>
    Public Property MaxValue As Double? = Nothing
    ''' <summary>是否绘制色阶图例条</summary>
    Public Property ShowColorLegend As Boolean = True
    ''' <summary>是否绘制外边框</summary>
    Public Property ShowFrame As Boolean = True
    ''' <summary>数据场外再叠加的多边形轮廓（可选，例如勾出感兴趣区域）</summary>
    Public Property OverlayShapes As List(Of PolygonGroup) = Nothing

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    Public Sub Plot()
        If Image Is Nothing Then Throw New InvalidOperationException("ImageMapPlot requires an image matrix.")

        DrawBackground()
        ComputePlotArea()
        DrawTitle()

        Dim rows = Image.GetLength(0)
        Dim cols = Image.GetLength(1)
        Dim vmin = If(MinValue, CastMin(Image))
        Dim vmax = If(MaxValue, CastMax(Image))
        If vmax <= vmin Then vmax = vmin + 1

        Dim cellW = _plotArea.Width / cols
        Dim cellH = _plotArea.Height / rows

        For row As Integer = 0 To rows - 1
            For col As Integer = 0 To cols - 1
                Dim color = ColorScale.GetColor(Image(row, col), vmin, vmax, ColorMap)
                Dim rect = New RectangleF(_plotArea.Left + col * cellW, _plotArea.Top + row * cellH, cellW, cellH)
                Using br As New SolidBrush(color)
                    _g.FillRectangle(br, rect)
                End Using
            Next
        Next

        If ShowFrame Then
            Using pen As New Pen(Theme.BorderColor, 0.7F)
                _g.DrawRectangle(pen, _plotArea.Left, _plotArea.Top, _plotArea.Width, _plotArea.Height)
            End Using
        End If

        If OverlayShapes IsNot Nothing Then
            DrawOverlays(rows, cols)
        End If

        If ShowColorLegend Then
            DrawColorLegend(ColorMap, vmin, vmax, horizontal:=False, tickCount:=5)
        End If
    End Sub

    Private Function CastMin(m As Double(,)) As Double
        Dim minV As Double = Double.MaxValue
        For Each v In m.Cast(Of Double)()
            If v < minV Then minV = v
        Next
        Return minV
    End Function

    Private Function CastMax(m As Double(,)) As Double
        Dim maxV As Double = Double.MinValue
        For Each v In m.Cast(Of Double)()
            If v > maxV Then maxV = v
        Next
        Return maxV
    End Function

    Private Sub DrawOverlays(rows As Integer, cols As Integer)
        For gi = 0 To OverlayShapes.Count - 1
            Dim grp = OverlayShapes(gi)
            Dim color = If(grp.Color, Theme.Palette(gi Mod Theme.Palette.Length))

            Using pen As New Pen(color, Theme.LineWidth * 1.2F)
                For Each poly In grp.SubRegions.SafeQuery
                    If poly Is Nothing OrElse poly.Length < 2 Then Continue For

                    ' 输入按「数据场栅格坐标」给出，这里换算到像素
                    Dim pts = poly.Select(Function(p) New PointF(
                                _plotArea.Left + CSng(p.X / cols * _plotArea.Width),
                                _plotArea.Top + CSng(p.Y / rows * _plotArea.Height))).ToArray()

                    _g.DrawPolygon(pen, pts)
                Next
            End Using
        Next
    End Sub
End Class
