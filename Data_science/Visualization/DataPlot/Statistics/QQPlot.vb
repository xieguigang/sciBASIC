#Region "Microsoft.VisualBasic::a1c4e7f9b2d64058c3e1a7f6b9d2c5e8, Data_science\Visualization\DataPlot\Statistics\QQPlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class QQPlot
    ' 
    '     Properties: Sample1, Sample2, Sample1Name, Sample2Name, ShowReferenceLine
    '                 MarkerSizeImpl, ConfidenceIntervals
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

' ============================================================================
'  QQPlot.vb - 分位数图（Q-Q plot）
'
'  把两个样本在相同的分位点上取值后对画：点越贴近 y = x，两个分布越一致。
'  分位数按 ((i - 0.5) / n) 取，两侧样本长度不同时以较短者为准。
' ============================================================================

''' <summary>分位数图（Q-Q plot）</summary>
Public Class QQPlot
    Inherits PlotEngine

    ''' <summary>第一个样本（画在 X 轴）</summary>
    Public Property Sample1 As Double() = {}
    ''' <summary>第二个样本（画在 Y 轴）</summary>
    Public Property Sample2 As Double() = {}
    ''' <summary>第一个样本的图例名</summary>
    Public Property Sample1Name As String = "sample 1"
    ''' <summary>第二个样本的图例名</summary>
    Public Property Sample2Name As String = "sample 2"
    ''' <summary>是否绘制 y = x 参考线</summary>
    Public Property ShowReferenceLine As Boolean = True
    ''' <summary>散点直径，小于等于 0 时用主题设定</summary>
    Public Property MarkerSize As Single = 0
    ''' <summary>散点颜色</summary>
    Public Property PointColor As Color? = Nothing

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        If Sample1 Is Nothing OrElse Sample2 Is Nothing OrElse Sample1.Length = 0 OrElse Sample2.Length = 0 Then
            Throw New InvalidOperationException("QQPlot requires two non-empty samples.")
        End If

        Dim sorted1 = Sample1.OrderBy(Function(x) x).ToArray()
        Dim sorted2 = Sample2.OrderBy(Function(x) x).ToArray()
        Dim n = std.Min(sorted1.Length, sorted2.Length)

        Dim xs = New List(Of Double)()
        Dim ys = New List(Of Double)()

        For i = 0 To n - 1
            Dim p = (i + 0.5) / n
            xs.Add(Geometry.Quantile(sorted1, p))
            ys.Add(Geometry.Quantile(sorted2, p))
        Next

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim xmin = If(Me.XMin, xs.Min())
        Dim xmax = If(Me.XMax, xs.Max())
        Dim ymin = If(Me.YMin, ys.Min())
        Dim ymax = If(Me.YMax, ys.Max())

        ' Q-Q 图要求两边同量纲，否则参考线没有意义
        Dim lo = std.Min(xmin, ymin), hi = std.Max(xmax, ymax)
        If Me.XMin Is Nothing AndAlso Me.XMax Is Nothing Then xmin = lo : xmax = hi
        If Me.YMin Is Nothing AndAlso Me.YMax Is Nothing Then ymin = lo : ymax = hi

        Geometry.ExpandRange(xmin, xmax, 0.05)
        Geometry.ExpandRange(ymin, ymax, 0.05)

        DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        If ShowReferenceLine Then
            Using pen As New Pen(Theme.AxisColor, Theme.LineWidth)
                pen.DashStyle = DashStyle.Dash
                DrawAbline(0, 1, xmin, xmax, ymin, ymax, pen)
            End Using
        End If

        Dim color = If(PointColor, Theme.Palette(0))
        Dim size = If(MarkerSize > 0, MarkerSize, Theme.MarkerSize)

        Using br As New SolidBrush(color)
            For i = 0 To n - 1
                Dim px = ToPixelX(xs(i), xmin, xmax)
                Dim py = ToPixelY(ys(i), ymin, ymax)
                DrawMarker(px, py, MarkerShape.Circle, size, color)
            Next
        End Using

        Dim legends As New List(Of Series) From {
            New Series With {.Name = $"{Sample1Name} vs {Sample2Name}", .Color = color, .MarkerShape = MarkerShape.Circle}
        }
        DrawLegend(legends)
    End Sub
End Class
