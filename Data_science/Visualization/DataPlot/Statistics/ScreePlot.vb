#Region "Microsoft.VisualBasic::e5f1a7c9b3d24806d2e8c4b1a7f9d3c6, Data_science\Visualization\DataPlot\Statistics\ScreePlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class ScreePlot
    ' 
    '     Properties: Contributions, ShowCumulative, ComponentNames
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

' ============================================================================
'  ScreePlot.vb - 碎石图
'
'  每个主成分一根柱子（贡献率），叠加一条累积贡献率折线，用于判断保留几个主成分。
'  旧实现依赖 ANOVA 包的 MultivariateAnalysisResult.Contributions，这里改为接收
'  Double() 数组，廷迟 shifting 由调用方决定。
' ============================================================================

''' <summary>主成分碎石图</summary>
Public Class ScreePlot
    Inherits PlotEngine

    ''' <summary>各主成分贡献率（0~1 或百分比均可，会自动识别）</summary>
    Public Property Contributions As Double() = {}
    ''' <summary>是否叠加累积贡献率曲线</summary>
    Public Property ShowCumulative As Boolean = True
    ''' <summary>主成分名，留空时显示为 PC1, PC2, ...</summary>
    Public Property ComponentNames As String() = Nothing
    ''' <summary>柱子颜色</summary>
    Public Property BarColor As Color? = Nothing
    ''' <summary>是否在柱子上写出百分比</summary>
    Public Property ShowValueLabels As Boolean = True

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        If Contributions Is Nothing OrElse Contributions.Length = 0 Then
            Throw New InvalidOperationException("ScreePlot requires contribution values.")
        End If

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        ' 统一按百分比处理
        Dim unit = If(Contributions.Max() <= 1.0000001, 100.0, 1.0)
        Dim values = Contributions.Select(Function(v) v * unit).ToArray()
        Dim labels = If(ComponentNames IsNot Nothing AndAlso ComponentNames.Length = values.Length,
                        ComponentNames,
                        values.Select(Function(v, i) "PC" & (i + 1)).ToArray())

        Dim n = values.Length
        Dim ymax = std.Max(100.0, values.Max() * 1.15)
        Dim xmin = -0.5, xmax = n - 0.5
        Dim ymin = 0

        Dim ticks = Geometry.NiceTicks(ymin, ymax)
        Dim cats = Enumerable.Range(0, n).Select(Function(i) CDbl(i)).ToArray()

        DrawAxisAndGrid(xmin, xmax, ymin, ymax, cats, ticks, labels, Nothing, xLabelRotate:=n > 8)

        Dim groupW = _plotArea.Width / n
        Dim barW = groupW * (1 - Theme.BarPadding * 2)
        Dim color = If(BarColor, Theme.Palette(0))

        For i = 0 To n - 1
            Dim cx = _plotArea.Left + (i + 0.5) * groupW
            Dim py0 = ToPixelY(0, ymin, ymax)
            Dim py1 = ToPixelY(values(i), ymin, ymax)
            Dim rect = New RectangleF(cx - barW / 2, py1, barW, py0 - py1)

            Using br As New SolidBrush(color),
                  pen As New Pen(Theme.BorderColor, 0.5F)
                _g.FillRectangle(br, rect)
                _g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height)
            End Using

            If ShowValueLabels Then
                Dim text = values(i).ToString("F1") & "%"
                Dim size = MeasureString(text, Theme.TickLabelFont)
                Using br As New SolidBrush(Theme.TextColor)
                    _g.DrawString(text, Theme.TickLabelFont, br, rect.X + rect.Width / 2 - size.Width / 2, rect.Y - size.Height - 1)
                End Using
            End If
        Next

        If ShowCumulative Then DrawCumulative(values, xmin, xmax, ymin, ymax)
    End Sub

    Private Sub DrawCumulative(values As Double(), xmin As Double, xmax As Double, ymin As Double, ymax As Double)
        Dim running As Double = 0
        Dim pts As New List(Of PointF)()

        For i = 0 To values.Length - 1
            running += values(i)
            pts.Add(New PointF(ToPixelX(i, xmin, xmax), ToPixelY(running, ymin, ymax)))
        Next

        If pts.Count < 2 Then Return

        Using pen As New Pen(Theme.Palette(3), Theme.LineWidth * 1.4F)
            _g.DrawLines(pen, pts.ToArray())
        End Using

        Using br As New SolidBrush(Theme.Palette(3))
            For Each p In pts
                _g.FillEllipse(br, p.X - 2.5F, p.Y - 2.5F, 5, 5)
            Next
        End Using
    End Sub
End Class
