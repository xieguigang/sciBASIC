#Region "Microsoft.VisualBasic::1b9f0c7d2a4e4b6f9c1d3e5a7b8c0d2e, Data_science\Visualization\DataPlot\Basic\BarPlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class BarPlot
    ' 
    '     Properties: Categories, ColorMap, Horizontal, MultiValues, SeriesNames
    '                 ShowColorLegend, ShowValueLabels, StackMode, UseColorScale
    '                 Values, ValueLabelFormat, Diverging
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    '     Enum StackMode / Orientation
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

' ============================================================================
'  BarPlot.vb - 柱状图（一个参数化类覆盖旧实现的六种变体）
'
'  旧代码里柱状图有 SimpleBarPlot / BarPlotAlternativeDirection /
'  StackedPercentageBarPlot / BiDirectionBarPlot / LevelBarplot / StyledBarplot
'  共六份实现，它们之间的差别只是：
'    · 方向（垂直 / 水平）          -> Horizontal
'    · 堆叠方式（不堆 / 绝对 / 百分比）-> StackMode
'    · 是否以零轴为基准双向延伸      -> Diverging
'    · 颜色是分类色还是色阶           -> UseColorScale
'    · 是否自己指定每根柱的颜色/标签  -> Serials
'
'  这里把它们收敛成一组开关，从而只需要维护一条绘制管线。
' ============================================================================

''' <summary>柱状图（分类数据）</summary>
Public Class BarPlot
    Inherits PlotEngine

    ''' <summary>堆叠方式</summary>
    Public Enum StackMode
        ''' <summary>不堆叠，多系列并排绘制</summary>
        None
        ''' <summary>绝对堆叠</summary>
        Stacked
        ''' <summary>百分比堆叠（每根柱子归一到 100%）</summary>
        Percent
    End Enum

    ''' <summary>分类名</summary>
    Public Property Categories As String() = {}
    ''' <summary>单系列时的取值</summary>
    Public Property Values As Double() = {}
    ''' <summary>系列名（多系列时使用）</summary>
    Public Property SeriesNames As String() = {}
    ''' <summary>多系列时使用 [系列, 类别] 二维数组</summary>
    Public Property MultiValues As Double(,) = Nothing

    ''' <summary>横向布局：分类轴放到 Y 轴</summary>
    Public Property Horizontal As Boolean = False
    ''' <summary>堆叠方式</summary>
    Public Property Stack As StackMode = StackMode.None
    ''' <summary>双向（发散）柱状图：负值向左 / 向下延伸</summary>
    Public Property Diverging As Boolean = False
    ''' <summary>是否在柱子上写出数值</summary>
    Public Property ShowValueLabels As Boolean = False
    ''' <summary>数值标签格式串，留空时用统一的格式化规则</summary>
    Public Property ValueLabelFormat As String = Nothing
    ''' <summary>柱子基础线，通常为 0；双向图以此为对称轴</summary>
    Public Property Baseline As Double = 0

    ''' <summary>用色阶给柱子上色（LevelBarplot 的行为）</summary>
    Public Property UseColorScale As Boolean = False
    ''' <summary>色阶方案（<see cref="UseColorScale"/> 为 True 时生效）</summary>
    Public Property ColorMap As ColorScale.ColorMapType = ColorScale.ColorMapType.Viridis
    ''' <summary>是否绘制色阶图例条</summary>
    Public Property ShowColorLegend As Boolean = True

    ''' <summary>自定义柱子：给了值时，完全按调用方指定的颜色与顺序绘制</summary>
    Public Property Serials As BarSerial() = Nothing

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    ''' <summary>直接在外部提供的绘图设备上绘制（图层叠加模式 / 宿主画布）。</summary>
    Public Sub New(g As IGraphics, Optional theme As PlotTheme = Nothing)
        MyBase.New(g, theme)
    End Sub

    Public Sub Plot()
        If Serials IsNot Nothing AndAlso Serials.Length > 0 Then
            DrawCustomSerials()
            Return
        End If

        Dim isMulti = (MultiValues IsNot Nothing)
        Dim nCat = If(isMulti, MultiValues.GetLength(1), If(Categories IsNot Nothing, Categories.Length, Values.Length))
        Dim nSer = If(isMulti, MultiValues.GetLength(0), 1)

        If nCat <= 0 Then Throw New InvalidOperationException("BarPlot requires at least one category.")

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        ' ---- 计算堆叠后的实际数据矩阵 ----
        Dim matrix = BuildMatrix(nSer, nCat)
        Dim vmin As Double, vmax As Double
        ValueRange(matrix, nSer, nCat, vmin, vmax)

        If Diverging AndAlso vmin > 0 Then vmin = 0

        DrawAxisByOrientation(nCat, vmin, vmax)

        Dim groupWidth = If(Horizontal, _plotArea.Height / nCat, _plotArea.Width / nCat)

        For j = 0 To nCat - 1
            Dim stackCursor As Double = Baseline

            For i = 0 To nSer - 1
                Dim val = matrix(i, j)
                Dim lo = If(Stack = StackMode.None, Baseline, stackCursor)
                Dim hi = If(Stack = StackMode.None, val, stackCursor + val)

                If Stack <> StackMode.None Then stackCursor += val

                Dim color = BarColor(val, i, vmin, vmax)
                DrawBar(j, i, groupWidth, nSer, lo, hi, vmin, vmax, color, val)
            Next
        Next

        If UseColorScale AndAlso ShowColorLegend Then
            DrawColorLegend(ColorMap, vmin, vmax, horizontal:=Horizontal)
        End If

        If nSer > 1 Then DrawSeriesLegend(nSer)
    End Sub

    ' ---------------- 内部实现 ----------------

    Private Sub DrawCustomSerials()
        Dim n = Serials.Length

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Categories = Serials.Select(Function(s) s.Label).ToArray()

        Dim vmax = Serials.Max(Function(s) s.Value)
        Dim vmin = std.Min(Baseline, Serials.Min(Function(s) s.Value))
        If vmax <= vmin Then vmax = vmin + 1
        vmax *= 1.1

        DrawAxisByOrientation(n, vmin, vmax)

        Dim groupWidth = If(Horizontal, _plotArea.Height / n, _plotArea.Width / n)

        For i = 0 To n - 1
            Dim color As Color = If(Serials(i).Brush, If(Serials(i).Color, Theme.Palette(i Mod Theme.Palette.Length)))
            DrawBar(i, 0, groupWidth, 1, Baseline, Serials(i).Value, vmin, vmax, color, Serials(i).Value)
        Next
    End Sub

    Private Function BuildMatrix(nSer As Integer, nCat As Integer) As Double(,)
        Dim matrix(nSer - 1, nCat - 1) As Double

        If MultiValues IsNot Nothing Then
            Array.Copy(MultiValues, matrix, MultiValues.Length)
        Else
            For j = 0 To nCat - 1
                matrix(0, j) = If(j < Values.Length, Values(j), 0)
            Next
        End If

        If Stack = StackMode.Percent Then
            Dim totals(nCat - 1) As Double

            For j = 0 To nCat - 1
                Dim sum As Double = 0
                For i = 0 To nSer - 1
                    sum += std.Abs(matrix(i, j))
                Next
                totals(j) = If(sum = 0, 1, sum)
            Next

            For j = 0 To nCat - 1
                For i = 0 To nSer - 1
                    matrix(i, j) = matrix(i, j) / totals(j) * 100
                Next
            Next
        End If

        Return matrix
    End Function

    Private Sub ValueRange(matrix As Double(,), nSer As Integer, nCat As Integer,
                           ByRef vmin As Double, ByRef vmax As Double)
        vmin = 0
        vmax = Double.MinValue

        If Stack = StackMode.None Then
            For i = 0 To nSer - 1
                For j = 0 To nCat - 1
                    If matrix(i, j) < vmin Then vmin = matrix(i, j)
                    If matrix(i, j) > vmax Then vmax = matrix(i, j)
                Next
            Next
        Else
            For j = 0 To nCat - 1
                Dim sum As Double = Baseline
                For i = 0 To nSer - 1
                    sum += matrix(i, j)
                Next
                If sum < vmin Then vmin = sum
                If sum > vmax Then vmax = sum
            Next
        End If

        If vmax = Double.MinValue Then vmax = 1
        If vmax <= vmin Then vmax = vmin + 1

        If Stack = StackMode.Percent Then
            vmin = 0 : vmax = 100
        Else
            Dim pad = (vmax - vmin) * 0.05
            vmin -= pad : vmax += pad
        End If

        vmin = If(Me.YMin IsNot Nothing AndAlso Not Horizontal, Me.YMin.Value, vmin)
        vmax = If(Me.YMax IsNot Nothing AndAlso Not Horizontal, Me.YMax.Value, vmax)
    End Sub

    Private Sub DrawAxisByOrientation(nCat As Integer, vmin As Double, vmax As Double)
        Dim catTicks = Enumerable.Range(0, nCat).Select(Function(i) CDbl(i)).ToArray()
        Dim valueTicks = Geometry.NiceTicks(vmin, vmax)

        If Horizontal Then
            DrawAxisAndGrid(vmin, vmax, -0.5, nCat - 0.5, valueTicks, catTicks, Nothing, Categories)
        Else
            DrawAxisAndGrid(-0.5, nCat - 0.5, vmin, vmax, catTicks, valueTicks, Categories, Nothing)
        End If
    End Sub

    Private Function BarColor(val As Double, i As Integer, vmin As Double, vmax As Double) As Color
        If UseColorScale Then Return ColorScale.GetColor(val, vmin, vmax, ColorMap)
        Return Theme.Palette(i Mod Theme.Palette.Length)
    End Function

    Private Sub DrawBar(j As Integer, i As Integer, groupWidth As Single, nSer As Integer,
                        lo As Double, hi As Double, vmin As Double, vmax As Double,
                        color As Color, label As Double)
        Dim barWidth = groupWidth * (1 - Theme.BarPadding * 2) / If(nSer > 0, nSer, 1)

        If nSer > 1 AndAlso Stack = StackMode.None Then
            ' 并列模式下按系列错开
        Else
            barWidth = groupWidth * (1 - Theme.BarPadding * 2)
        End If

        If Horizontal Then
            Dim cy = _plotArea.Top + (j + 0.5) * groupWidth
            Dim px0 = ToPixelX(lo, vmin, vmax)
            Dim px1 = ToPixelX(hi, vmin, vmax)
            Dim by = cy - barWidth / 2 + If(nSer > 1 AndAlso Stack = StackMode.None,
                                            i * (barWidth / nSer), 0)
            Dim h As Single = If(nSer > 1 AndAlso Stack = StackMode.None, barWidth / nSer, barWidth) * 0.9
            Dim rect = New RectangleF(std.Min(px0, px1), by, std.Abs(px1 - px0), h)

            DrawBarBody(rect, color)

            If ShowValueLabels Then
                WriteBarLabel(label, rect, vertical:=False, toLeft:=hi < lo)
            End If
        Else
            Dim cx = _plotArea.Left + (j + 0.5) * groupWidth
            Dim py0 = ToPixelY(lo, vmin, vmax)
            Dim py1 = ToPixelY(hi, vmin, vmax)
            Dim w As Single = If(nSer > 1 AndAlso Stack = StackMode.None, barWidth / nSer, barWidth) * 0.9
            Dim x As Single = cx - w / 2 + If(nSer > 1 AndAlso Stack = StackMode.None,
                                              i * (barWidth / nSer) - (nSer - 1) * (barWidth / nSer) / 2, 0)
            Dim rect = New RectangleF(x, std.Min(py0, py1), w, std.Abs(py1 - py0))

            DrawBarBody(rect, color)

            If ShowValueLabels Then
                WriteBarLabel(label, rect, vertical:=True, toLeft:=hi < lo)
            End If
        End If
    End Sub

    Private Sub DrawBarBody(rect As RectangleF, color As Color)
        Using br As New SolidBrush(color)
            _g.FillRectangle(br, rect)
        End Using
        Using pen As New Pen(Theme.BorderColor, 0.5F)
            _g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height)
        End Using
    End Sub

    Private Sub WriteBarLabel(val As Double, rect As RectangleF, vertical As Boolean, toLeft As Boolean)
        Dim text = If(String.IsNullOrEmpty(ValueLabelFormat), FormatNumber(val), val.ToString(ValueLabelFormat))
        Dim size = MeasureString(text, Theme.TickLabelFont)

        Using br As New SolidBrush(Theme.TextColor)
            If vertical Then
                If toLeft Then
                    _g.DrawString(text, Theme.TickLabelFont, br, rect.X + rect.Width / 2 - size.Width / 2, rect.Bottom + 2)
                Else
                    _g.DrawString(text, Theme.TickLabelFont, br, rect.X + rect.Width / 2 - size.Width / 2, rect.Y - size.Height - 2)
                End If
            Else
                If toLeft Then
                    _g.DrawString(text, Theme.TickLabelFont, br, rect.X - size.Width - 4, rect.Y + rect.Height / 2 - size.Height / 2)
                Else
                    _g.DrawString(text, Theme.TickLabelFont, br, rect.Right + 4, rect.Y + rect.Height / 2 - size.Height / 2)
                End If
            End If
        End Using
    End Sub

    Private Sub DrawSeriesLegend(nSer As Integer)
        Dim seriesList As New List(Of Series)()

        For i = 0 To nSer - 1
            seriesList.Add(New Series With {
                .Name = If(SeriesNames IsNot Nothing AndAlso i < SeriesNames.Length, SeriesNames(i), "Series " & (i + 1)),
                .Color = Theme.Palette(i Mod Theme.Palette.Length),
                .MarkerShape = MarkerShape.Square
            })
        Next

        DrawLegend(seriesList)
    End Sub
End Class
