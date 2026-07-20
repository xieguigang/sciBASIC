#Region "Microsoft.VisualBasic::666c244acc777dc7ea9e2d14cf59fd5f, Data_science\Visualization\DataPlot\Engine\PlotEngine.vb"

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

    '   Total Lines: 428
    '    Code Lines: 333 (77.80%)
    ' Comment Lines: 57 (13.32%)
    '    - Xml Docs: 19.30%
    ' 
    '   Blank Lines: 38 (8.88%)
    '     File Size: 18.75 KB


    ' Class PlotEngine
    ' 
    '     Properties: LegendLocation, ShowLegend, SubTitle, Theme, Title
    '                 XLabel, XMax, XMin, YLabel, YMax
    '                 YMin
    '     Enum LegendPos
    ' 
    '         BottomOutside, LowerLeft, LowerRight, RightOutside, UpperLeft
    '         UpperRight
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: CanvasHeight, CanvasWidth, PlotArea
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: FormatNumber, GenerateTicks, GetGraphics, NiceStep, ToPixelX
    '               ToPixelY
    ' 
    '     Sub: ApplyQuality, AutoRange, ComputePlotArea, Dispose, DrawAxisAndGrid
    '          DrawBackground, DrawLegend, DrawMarker, DrawPlotArea, DrawTitle
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver
Imports StringAlignment = Microsoft.VisualBasic.Imaging.StringAlignment
Imports StringFormat = Microsoft.VisualBasic.Imaging.StringFormat

' ============================================================================
'  PlotEngine.vb - 核心绘图引擎
'  负责画布管理、坐标变换、坐标轴 / 网格 / 图例绘制、PNG 高清导出
' ============================================================================

Public MustInherit Class SeriesPlotEngine : Inherits PlotEngine

    Protected Sub New(bmp As Bitmap)
        MyBase.New(bmp)
    End Sub

    Protected Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public MustOverride Sub Plot(seriesList As IList(Of Series))

End Class

''' <summary>
''' 绘图引擎：所有图表类型的基类与公共能力。
''' 使用 GDI+ 在内存 Bitmap 上绘制，最后导出为 PNG。
''' </summary>
Public Class PlotEngine : Implements IDisposable

    ' ---------- 画布 ----------
    Protected _g As IGraphics
    Protected _width As Integer
    Protected _height As Integer
    Protected _bitmap As Bitmap = Nothing

    ' ---------- 主题 ----------
    Public Property Theme As PlotTheme

    ' ---------- 标题 / 标签 ----------
    Public Property Title As String = ""
    Public Property SubTitle As String = ""
    Public Property XLabel As String = ""
    Public Property YLabel As String = ""

    ' ---------- 坐标范围（Nothing 表示自动） ----------
    Public Property XMin As Double? = Nothing
    Public Property XMax As Double? = Nothing
    Public Property YMin As Double? = Nothing
    Public Property YMax As Double? = Nothing

    ' ---------- 图例 ----------
    Public Property ShowLegend As Boolean = True
    Public Property LegendLocation As LegendPos = LegendPos.UpperRight

    Public Enum LegendPos
        UpperLeft
        UpperRight
        LowerLeft
        LowerRight
        RightOutside
        BottomOutside
    End Enum

    ' ---------- 绘图区（数据坐标对应的像素矩形） ----------
    Protected _plotArea As RectangleF
    Public ReadOnly Property PlotArea As RectangleF
        Get
            Return _plotArea
        End Get
    End Property

    ' ========================================================
    '  构造与释放
    ' ========================================================
    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        _width = width
        _height = height
        _Theme = If(theme, PlotTheme.Light())
        _g = DriverLoad.CreateGraphicsDevice(New Size(width, height))
        ApplyQuality(_g)
    End Sub

    ''' <summary>
    ''' 直接使用已有的 System.Drawing.Bitmap 作为绘图设备（GDI 驱动）。
    ''' 渲染结果即绘制在该位图上，可通过 ToBitmap() 取回。
    ''' </summary>
    Public Sub New(bmp As Bitmap)
        If bmp Is Nothing Then Throw New ArgumentNullException(NameOf(bmp))
        _bitmap = bmp
        _width = bmp.Width
        _height = bmp.Height
        _Theme = PlotTheme.Light()
        _g = DriverLoad.CreateGraphicsDevice(bmp, driver:=Drivers.GDI)
        ApplyQuality(_g)
    End Sub

    ''' <summary>取回底层绘图位图（若由 New(bmp) 构造则有值，否则为 Nothing）。</summary>
    Public Function ToBitmap() As Bitmap
        Return _bitmap
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        _g?.Dispose()
    End Sub

    ' ========================================================
    '  画质设置
    ' ========================================================
    Private Sub ApplyQuality(g As IGraphics)
        If Theme.AntiAlias Then
            ' g.SmoothingMode = SmoothingMode.AntiAlias
            ' g.PixelOffsetMode = PixelOffsetMode.HighQuality
            ' g.InterpolationMode = InterpolationMode.HighQualityBicubic
        End If
        If Theme.HighQualityText Then
            ' g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit
        End If
    End Sub

    ' ========================================================
    '  坐标变换
    ' ========================================================
    ''' <summary>计算绘图区矩形（基于主题边距）</summary>
    Protected Sub ComputePlotArea()
        Dim x = Theme.MarginLeft
        Dim y = Theme.MarginTop
        Dim w = _width - Theme.MarginLeft - Theme.MarginRight
        Dim h = _height - Theme.MarginTop - Theme.MarginBottom
        _plotArea = New RectangleF(x, y, w, h)
    End Sub

    ''' <summary>数据坐标 -> 像素坐标</summary>
    Protected Function ToPixelX(x As Double, xmin As Double, xmax As Double) As Single
        Return _plotArea.Left + CSng((x - xmin) / (xmax - xmin) * _plotArea.Width)
    End Function

    Protected Function ToPixelY(y As Double, ymin As Double, ymax As Double) As Single
        Return _plotArea.Bottom - CSng((y - ymin) / (ymax - ymin) * _plotArea.Height)
    End Function

    ''' <summary>计算"漂亮"的刻度间隔（基于 nice number 算法）</summary>
    Protected Shared Function NiceStep(range As Double, tickCount As Integer) As Double
        If range <= 0 Then Return 1
        Dim rough = range / tickCount
        Dim pow = Math.Pow(10, Math.Floor(Math.Log10(rough)))
        Dim norm = rough / pow
        Dim [step] As Double
        If norm < 1.5 Then
            [step] = 1
        ElseIf norm < 3 Then
            [step] = 2
        ElseIf norm < 7 Then
            [step] = 5
        Else
            [step] = 10
        End If
        Return [step] * pow
    End Function

    ''' <summary>生成从 min 到 max 的刻度序列</summary>
    Protected Shared Function GenerateTicks(min As Double, max As Double, Optional tickCount As Integer = 6) As Double()
        Dim list As New List(Of Double)
        If max <= min Then Return {min}
        Dim [step] = NiceStep(max - min, tickCount)
        Dim start = Math.Ceiling(min / [step]) * [step]
        Dim v = start
        Do While v <= max + [step] * 0.001
            list.Add(Math.Round(v, 8))
            v += [step]
        Loop
        Return list.ToArray()
    End Function

    ''' <summary>自动扩展数据范围，留 5% 余量</summary>
    Protected Shared Sub AutoRange(data As Double(), ByRef min As Double, ByRef max As Double, Optional pad As Double = 0.05)
        If data Is Nothing OrElse data.Length = 0 Then
            min = 0 : max = 1 : Return
        End If
        min = data.Min()
        max = data.Max()
        If Math.Abs(max - min) < 0.000000000001 Then
            min -= 1 : max += 1
        End If
        Dim pad_ = (max - min) * pad
        min -= pad_ : max += pad_
    End Sub

    ' ========================================================
    '  背景与边框
    ' ========================================================
    Protected Sub DrawBackground()
        Using br As New SolidBrush(Theme.BackgroundColor)
            _g.FillRectangle(br, 0, 0, _width, _height)
        End Using
    End Sub

    Protected Sub DrawPlotArea()
        Using br As New SolidBrush(Theme.PlotAreaColor)
            _g.FillRectangle(br, _plotArea)
        End Using
    End Sub

    ' ========================================================
    '  标题
    ' ========================================================
    Protected Sub DrawTitle()
        If String.IsNullOrEmpty(Title) Then Return
        Using sf As New StringFormat()
            sf.Alignment = StringAlignment.Center
            sf.LineAlignment = StringAlignment.Center
            Using br As New SolidBrush(Theme.TitleColor)
                Dim rect As New RectangleF(0, 10, _width, Theme.TitleFont.GetHeight(_g) + 6)
                _g.DrawString(Title, Theme.TitleFont, br, rect)
            End Using
        End Using
        If Not String.IsNullOrEmpty(SubTitle) Then
            Using sf As New StringFormat()
                sf.Alignment = StringAlignment.Center
                sf.LineAlignment = StringAlignment.Center
                Using br As New SolidBrush(Theme.SubTitleColor)
                    Dim y = 10 + Theme.TitleFont.GetHeight(_g) + 6
                    Dim rect As New RectangleF(0, y, _width, Theme.SubTitleFont.GetHeight(_g) + 4)
                    _g.DrawString(SubTitle, Theme.SubTitleFont, br, rect)
                End Using
            End Using
        End If
    End Sub

    ' ========================================================
    '  坐标轴 + 网格 + 刻度
    ' ========================================================
    Protected Sub DrawAxisAndGrid(xmin As Double, xmax As Double, ymin As Double, ymax As Double,
                                  Optional xTicks As Double() = Nothing,
                                  Optional yTicks As Double() = Nothing,
                                  Optional xLabels As String() = Nothing,
                                  Optional yLabels As String() = Nothing,
                                  Optional xLabelRotate As Boolean = False)
        If xTicks Is Nothing Then xTicks = GenerateTicks(xmin, xmax)
        If yTicks Is Nothing Then yTicks = GenerateTicks(ymin, ymax)

        ' ---- 网格 ----
        If Theme.ShowGrid Then
            Using pen As New Pen(Theme.GridColor, Theme.GridLineWidth)
                For Each t In yTicks
                    If t < ymin OrElse t > ymax Then Continue For
                    Dim py = ToPixelY(t, ymin, ymax)
                    _g.DrawLine(pen, _plotArea.Left, py, _plotArea.Right, py)
                Next
                For Each t In xTicks
                    If t < xmin OrElse t > xmax Then Continue For
                    Dim px = ToPixelX(t, xmin, xmax)
                    _g.DrawLine(pen, px, _plotArea.Top, px, _plotArea.Bottom)
                Next
            End Using
        End If

        ' ---- 轴线 ----
        Using pen As New Pen(Theme.AxisColor, Theme.AxisLineWidth)
            _g.DrawLine(pen, _plotArea.Left, _plotArea.Bottom, _plotArea.Right, _plotArea.Bottom)
            _g.DrawLine(pen, _plotArea.Left, _plotArea.Top, _plotArea.Left, _plotArea.Bottom)
        End Using

        ' ---- X 刻度与标签 ----
        Using tickPen As New Pen(Theme.AxisColor, Theme.AxisLineWidth),
              br As New SolidBrush(Theme.TextColor)
            For i = 0 To xTicks.Length - 1
                Dim t = xTicks(i)
                If t < xmin OrElse t > xmax Then Continue For
                Dim px = ToPixelX(t, xmin, xmax)
                _g.DrawLine(tickPen, px, _plotArea.Bottom, px, _plotArea.Bottom + 5)
                Dim label = If(xLabels IsNot Nothing AndAlso i < xLabels.Length, xLabels(i), FormatNumber(t))
                Using sf As New StringFormat()
                    sf.Alignment = StringAlignment.Center
                    sf.LineAlignment = StringAlignment.Near
                    If xLabelRotate Then
                        sf.Alignment = StringAlignment.Far
                        sf.LineAlignment = StringAlignment.Center
                        _g.TranslateTransform(px + 3, _plotArea.Bottom + 8)
                        _g.RotateTransform(45)
                        _g.DrawString(label, Theme.TickLabelFont, br, 0, 0)
                        _g.ResetTransform()
                    Else
                        Dim rect As New RectangleF(px - 50, _plotArea.Bottom + 7, 100, Theme.TickLabelFont.GetHeight(_g) + 4)
                        _g.DrawString(label, Theme.TickLabelFont, br, rect)
                    End If
                End Using
            Next

            ' ---- Y 刻度与标签 ----
            For i = 0 To yTicks.Length - 1
                Dim t = yTicks(i)
                If t < ymin OrElse t > ymax Then Continue For
                Dim py = ToPixelY(t, ymin, ymax)
                _g.DrawLine(tickPen, _plotArea.Left - 5, py, _plotArea.Left, py)
                Dim label = If(yLabels IsNot Nothing AndAlso i < yLabels.Length, yLabels(i), FormatNumber(t))
                Using sf As New StringFormat()
                    sf.Alignment = StringAlignment.Far
                    sf.LineAlignment = StringAlignment.Center
                    Dim rect As New RectangleF(0, py - Theme.TickLabelFont.GetHeight(_g) / 2 - 2,
                                               _plotArea.Left - 8, Theme.TickLabelFont.GetHeight(_g) + 4)
                    _g.DrawString(label, Theme.TickLabelFont, br, rect)
                End Using
            Next
        End Using

        ' ---- 轴标题 ----
        If Not String.IsNullOrEmpty(XLabel) Then
            Using br As New SolidBrush(Theme.TextColor),
                  sf As New StringFormat()
                sf.Alignment = StringAlignment.Center
                sf.LineAlignment = StringAlignment.Center
                Dim y = _plotArea.Bottom + 35
                Dim rect As New RectangleF(_plotArea.Left, y, _plotArea.Width, Theme.AxisLabelFont.GetHeight(_g) + 6)
                _g.DrawString(XLabel, Theme.AxisLabelFont, br, rect)
            End Using
        End If
        If Not String.IsNullOrEmpty(YLabel) Then
            Using br As New SolidBrush(Theme.TextColor),
                  sf As New StringFormat()
                sf.Alignment = StringAlignment.Center
                sf.LineAlignment = StringAlignment.Center
                _g.TranslateTransform(18, _plotArea.Top + _plotArea.Height / 2)
                _g.RotateTransform(-90)
                _g.DrawString(YLabel, Theme.AxisLabelFont, br, New RectangleF(-_plotArea.Height / 2, -Theme.AxisLabelFont.GetHeight(_g) / 2, _plotArea.Height, Theme.AxisLabelFont.GetHeight(_g) + 6))
                _g.ResetTransform()
            End Using
        End If
    End Sub

    Protected Function FormatNumber(v As Double) As String
        Dim a = Math.Abs(v)
        If a = 0 Then Return "0"
        If a >= 10000 OrElse a < 0.01 Then
            Return v.ToString("0.#E+0")
        ElseIf a >= 100 Then
            Return v.ToString("0")
        ElseIf a >= 1 Then
            Return v.ToString("0.##")
        Else
            Return v.ToString("0.###")
        End If
    End Function

    ' ========================================================
    '  图例
    ' ========================================================
    Protected Sub DrawLegend(seriesList As IList(Of Series))
        If Not ShowLegend OrElse seriesList Is Nothing OrElse seriesList.Count = 0 Then Return
        Dim visible = seriesList.Where(Function(s) s.Visible AndAlso Not String.IsNullOrEmpty(s.Name)).ToList()
        If visible.Count = 0 Then Return

        Dim lineH = Theme.LegendFont.GetHeight(_g) + 4
        Dim boxW = 22.0F
        Dim padX = 8.0F
        Dim padY = 6.0F
        Dim maxTextW = 0.0F
        For Each s In visible
            Dim w = _g.MeasureString(s.Name, Theme.LegendFont).Width
            If w > maxTextW Then maxTextW = w
        Next
        Dim legendW = padX * 2 + boxW + 4 + maxTextW
        Dim legendH = padY * 2 + lineH * visible.Count

        Dim lx, ly As Single
        Select Case LegendLocation
            Case LegendPos.UpperLeft : lx = _plotArea.Left + 8 : ly = _plotArea.Top + 8
            Case LegendPos.UpperRight : lx = _plotArea.Right - legendW - 8 : ly = _plotArea.Top + 8
            Case LegendPos.LowerLeft : lx = _plotArea.Left + 8 : ly = _plotArea.Bottom - legendH - 8
            Case LegendPos.LowerRight : lx = _plotArea.Right - legendW - 8 : ly = _plotArea.Bottom - legendH - 8
            Case LegendPos.RightOutside : lx = _plotArea.Right + 12 : ly = _plotArea.Top
            Case LegendPos.BottomOutside : lx = _plotArea.Left : ly = _plotArea.Bottom + 45
        End Select

        Dim rect As New RectangleF(lx, ly, legendW, legendH)
        Using br As New SolidBrush(Color.FromArgb(220, Theme.LegendBackgroundColor))
            _g.FillRectangle(br, rect)
        End Using
        If Theme.ShowLegendBorder Then
            Using pen As New Pen(Theme.LegendBorderColor, 0.7F)
                _g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height)
            End Using
        End If

        For i = 0 To visible.Count - 1
            Dim s = visible(i)
            Dim y = ly + padY + i * lineH + lineH / 2
            Dim color = If(s.Color, Theme.Palette(i Mod Theme.Palette.Length))
            ' 标记线
            Using pen As New Pen(color, Theme.LineWidth)
                pen.DashStyle = s.LineStyle
                _g.DrawLine(pen, lx + padX, y, lx + padX + boxW, y)
            End Using
            ' 标记点
            DrawMarker(lx + padX + boxW / 2, y, s.MarkerShape, Theme.MarkerSize * 0.8F, color)
            ' 文字
            Using br As New SolidBrush(Theme.TextColor),
                  sf As New StringFormat()
                sf.Alignment = StringAlignment.Near
                sf.LineAlignment = StringAlignment.Center
                _g.DrawString(s.Name, Theme.LegendFont, br,
                              lx + padX + boxW + 4, y - lineH / 2 + 2)
            End Using
        Next
    End Sub

    ' ========================================================
    '  标记绘制
    ' ========================================================
    Protected Sub DrawMarker(x As Single, y As Single, shape As MarkerShape, size As Single, color As Color)
        Using br As New SolidBrush(color),
              pen As New Pen(color, Theme.LineWidth)
            Select Case shape
                Case MarkerShape.Circle
                    _g.FillEllipse(br, x - size / 2, y - size / 2, size, size)
                Case MarkerShape.Square
                    _g.FillRectangle(br, x - size / 2, y - size / 2, size, size)
                Case MarkerShape.Triangle
                    Dim pts = {
                        New PointF(x, y - size / 2),
                        New PointF(x - size / 2, y + size / 2),
                        New PointF(x + size / 2, y + size / 2)
                    }
                    _g.FillPolygon(br, pts)
                Case MarkerShape.Diamond
                    Dim pts = {
                        New PointF(x, y - size / 2),
                        New PointF(x + size / 2, y),
                        New PointF(x, y + size / 2),
                        New PointF(x - size / 2, y)
                    }
                    _g.FillPolygon(br, pts)
                Case MarkerShape.Cross
                    _g.DrawLine(pen, x - size / 2, y - size / 2, x + size / 2, y + size / 2)
                    _g.DrawLine(pen, x - size / 2, y + size / 2, x + size / 2, y - size / 2)
                Case MarkerShape.Plus
                    _g.DrawLine(pen, x - size / 2, y, x + size / 2, y)
                    _g.DrawLine(pen, x, y - size / 2, x, y + size / 2)
            End Select
        End Using
    End Sub

    ''' <summary>获取内部 Graphics（高级用户自定义绘制）</summary>
    Public Function GetGraphics() As IGraphics
        Return _g
    End Function

    ''' <summary>暴露给子类使用的画布尺寸</summary>
    Public ReadOnly Property CanvasWidth As Integer
        Get
            Return _width
        End Get
    End Property

    Public ReadOnly Property CanvasHeight As Integer
        Get
            Return _height
        End Get
    End Property

End Class
