#Region "Microsoft.VisualBasic::2c9b4e7a1d3f4805e7c9b2a4d6f8c1e3, Data_science\Visualization\DataPlot\Statistics\ManhattanPlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class ManhattanPlot
    ' 
    '     Properties: Points, SuggestiveThreshold, SignificantThreshold, LabelThreshold
    '                 ShowThresholds, ChromosomeOrder
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
'  ManhattanPlot.vb - 曼哈顿图（GWAS 关联结果）
'
'  旧项目的 Scatter/ManhattanStatics.vb 整个文件都是被注释掉的死代码。
'  这里按标准曼哈顿图补齐实现：染色体沿 X 轴首尾相接排列、相邻染色体交替上色、
'  Y 轴是 -log10(p)，并给出提示线与显著线；超过标注阈值的位点会尽量写出标签
'  （用一个简易的矩形避让避免文字互相压住）。
' ============================================================================

''' <summary>曼哈顿图</summary>
Public Class ManhattanPlot
    Inherits PlotEngine

    ''' <summary>位点集合</summary>
    Public Property Points As List(Of ManhattanPoint) = New List(Of ManhattanPoint)()
    ''' <summary>提示线对应的 p 值（默认 1e-5）</summary>
    Public Property SuggestivePValue As Double = 0.00001
    ''' <summary>显著线对应的 p 值（默认 5e-8）</summary>
    Public Property SignificantPValue As Double = 0.00000005
    ''' <summary>达到该 p 值时尝试标注位点标签</summary>
    Public Property LabelThresholdPValue As Double = 0.00000005
    ''' <summary>是否绘制阈值线</summary>
    Public Property ShowThresholds As Boolean = True
    ''' <summary>是否标注位点标签</summary>
    Public Property ShowLabels As Boolean = True
    ''' <summary>点直径</summary>
    Public Property MarkerSize As Single = 5.0F
    ''' <summary>相邻染色体使用的两种颜色</summary>
    Public Property OddColor As Color = Color.FromArgb(31, 119, 180)
    Public Property EvenColor As Color = Color.FromArgb(140, 86, 75)

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    Public Sub Plot()
        If Points Is Nothing OrElse Points.Count = 0 Then
            Throw New InvalidOperationException("ManhattanPlot requires at least one locus.")
        End If

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim layout = BuildLayout()
        Dim ymax = If(Me.YMax, std.Max(-std.Log10(MinPValue()), 0) * 1.15)
        Dim ymin = If(Me.YMin, 0)
        Dim xmin = If(Me.XMin, 0)
        Dim xmax = If(Me.XMax, layout.total)

        DrawAxisAndGrid(xmin, xmax, ymin, ymax,
                        layout.ticks.Select(Function(t) t.position).ToArray(),
                        Geometry.NiceTicks(ymin, ymax),
                        layout.ticks.Select(Function(t) t.name).ToArray(),
                        Nothing)

        ' 阈值线
        If ShowThresholds Then
            DrawThreshold(SuggestivePValue, Color.FromArgb(90, 130, 200), "suggestive", ymax, xmin, xmax, ymin)
            DrawThreshold(SignificantPValue, Color.FromArgb(200, 60, 60), "significant", ymax, xmin, xmax, ymin)
        End If

        Dim occupied As New List(Of RectangleF)()

        For Each item In layout.positions
            Dim pt = item.point
            Dim color = If(item.chromIndex Mod 2 = 0, OddColor, EvenColor)
            Dim px = ToPixelX(item.x, xmin, xmax)
            Dim py = ToPixelY(item.y, ymin, ymax)

            DrawMarker(px, py, MarkerShape.Circle, MarkerSize, color)

            If ShowLabels AndAlso pt.PValue <= LabelThresholdPValue AndAlso Not String.IsNullOrEmpty(pt.Label) Then
                DrawLocusLabel(pt.Label, px, py, occupied)
            End If
        Next
    End Sub

    Private Structure LocusSlot
        Public Property point As ManhattanPoint
        Public Property x As Double
        Public Property y As Double
        Public Property chromIndex As Integer
    End Structure

    Private Structure TickSlot
        Public Property name As String
        Public Property position As Double
    End Structure

    Private Structure LayoutResult
        Public Property positions As List(Of LocusSlot)
        Public Property ticks As List(Of TickSlot)
        Public Property total As Double
    End Structure

    Private Function BuildLayout() As LayoutResult
        Dim result As New LayoutResult With {
            .positions = New List(Of LocusSlot)(),
            .ticks = New List(Of TickSlot)(),
            .total = 0
        }

        Dim groups = Points.GroupBy(Function(p) p.Chromosome).ToList()
        Dim offset As Double = 0

        For gi = 0 To groups.Count - 1
            Dim grp = groups(gi)
            Dim span = If(grp.Max(Function(p) p.Position) > 0, grp.Max(Function(p) p.Position), 1)
            Dim start = offset

            For Each p In grp
                result.positions.Add(New LocusSlot With {
                    .point = p,
                    .x = start + p.Position,
                    .y = If(p.PValue <= 0, Double.NaN, -std.Log10(p.PValue)),
                    .chromIndex = gi
                })
            Next

            result.ticks.Add(New TickSlot With {.name = grp.Key, .position = (start + start + span) / 2})
            offset = start + span * 1.05
        Next

        result.total = offset
        Return result
    End Function

    Private Function MinPValue() As Double
        Dim validP = Points.Where(Function(p) p.PValue > 0).ToArray()
        If validP.Length = 0 Then Return 1
        Return validP.Min(Function(p) p.PValue)
    End Function

    Private Sub DrawThreshold(pvalue As Double, color As Color, caption As String,
                              ymax As Double, xmin As Double, xmax As Double, ymin As Double)
        Dim y = If(pvalue > 0, -std.Log10(pvalue), 0)
        If y > ymax Then Return

        Using pen As New Pen(color, Theme.LineWidth)
            pen.DashStyle = DashStyle.Dash
            _g.DrawLine(pen, ToPixelX(xmin, xmin, xmax), ToPixelY(y, ymin, ymax),
                        ToPixelX(xmax, xmin, xmax), ToPixelY(y, ymin, ymax))
        End Using

        Using br As New SolidBrush(color)
            Dim text = $"{caption} (p={pvalue.ToString("0.#E+0")})"
            _g.DrawString(text, Theme.TickLabelFont, br, _plotArea.Right - MeasureString(text, Theme.TickLabelFont).Width,
                          ToPixelY(y, ymin, ymax) - MeasureString(text, Theme.TickLabelFont).Height - 1)
        End Using
    End Sub

    ''' <summary>带矩形避让的位点标签：遇到已占用区域就往上挪一行</summary>
    Private Sub DrawLocusLabel(text As String, px As Single, py As Single, occupied As List(Of RectangleF))
        Dim size = MeasureString(text, Theme.TickLabelFont)
        Dim rect = New RectangleF(px + 4, py - size.Height / 2, size.Width, size.Height)
        Dim shift As Integer = 0

        While occupied.Any(Function(r) r.IntersectsWith(rect)) AndAlso shift < 6
            shift += 1
            rect = New RectangleF(px + 4, py - size.Height / 2 - shift * (size.Height + 2), size.Width, size.Height)
        End While

        occupied.Add(rect)

        Using br As New SolidBrush(Theme.TextColor)
            _g.DrawString(text, Theme.TickLabelFont, br, rect.X, rect.Y)
        End Using
    End Sub
End Class
