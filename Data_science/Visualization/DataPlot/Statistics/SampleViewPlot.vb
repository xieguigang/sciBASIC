#Region "Microsoft.VisualBasic::4e8c2f5b1a6d4916a9c5e8f2b4d7a1c3, Data_science\Visualization\DataPlot\Statistics\SampleViewPlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class SampleViewPlot
    ' 
    '     Properties: Sample, ShowDensityCurve, ShowSigmaBands, ShowOutliers
    '                 SigmaLevels, OutlierColor
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
'  SampleViewPlot.vb - 样本正态性视图
'
'  旧实现的 SampleView 用 stats 包的 BasicProductMoments 算矩、用
'  Vector.ProbabilityDensity 画正态曲线；这里矩由 <see cref="SampleMoments"/> 传入，
'  正态分布密度在本地按解析式计算（只有一个 exp，没必要为此引入统计包）。
'
'  图上四件套：直方图底图 + 正态密度曲线 + 均值 ± kσ 标线 + 离群点。
' ============================================================================

''' <summary>样本正态性视图</summary>
Public Class SampleViewPlot
    Inherits PlotEngine

    ''' <summary>样本及其矩</summary>
    Public Property Sample As SampleMoments = Nothing
    ''' <summary>是否叠加正态密度曲线</summary>
    Public Property ShowDensityCurve As Boolean = True
    ''' <summary>是否标出 ±kσ 的位置</summary>
    Public Property ShowSigmaBands As Boolean = True
    ''' <summary>要标注的 σ 倍数</summary>
    Public Property SigmaLevels As Integer() = {1, 2, 3}
    ''' <summary>是否用不同颜色标出离群点</summary>
    Public Property ShowOutliers As Boolean = True
    ''' <summary>离群点判定阈值（σ 倍数）</summary>
    Public Property OutlierSigma As Double = 3
    ''' <summary>离群点颜色</summary>
    Public Property OutlierColor As Color = Color.Red
    ''' <summary>样本点颜色</summary>
    Public Property SampleColor As Color = Color.SteelBlue
    ''' <summary>直方图分箱数</summary>
    Public Property Bins As Integer = 30

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    Public Sub Plot()
        If Sample Is Nothing OrElse Sample.Data Is Nothing OrElse Sample.Data.Length = 0 Then
            Throw New InvalidOperationException("SampleViewPlot requires sample data.")
        End If

        Dim data = Sample.Data
        Dim sd = If(Sample.StandardDeviation > 0, Sample.StandardDeviation, 1)
        Dim mean = Sample.Mean

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim xmin = std.Min(data.Min(), mean - 4 * sd)
        Dim xmax = std.Max(data.Max(), mean + 4 * sd)
        Geometry.ExpandRange(xmin, xmax, 0.02)

        Dim binW = (xmax - xmin) / Bins
        Dim counts = New Double(Bins - 1) {}
        For Each v In data
            Dim idx = CInt(std.Floor((v - xmin) / binW))
            idx = std.Max(0, std.Min(Bins - 1, idx))
            counts(idx) += 1
        Next

        Dim ymax = counts.Max() * 1.15
        Dim ymin = 0

        _viewMin = xmin : _viewMax = xmax : _viewYMin = ymin : _viewYMax = ymax

        DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        ' ---- 直方图底图 ----
        Using br As New SolidBrush(Color.FromArgb(160, SampleColor)),
              pen As New Pen(SampleColor, Theme.LineWidth)
            For i = 0 To Bins - 1
                Dim px0 = ToPixelX(xmin + i * binW, xmin, xmax)
                Dim px1 = ToPixelX(xmin + (i + 1) * binW, xmin, xmax)
                Dim py1 = ToPixelY(counts(i), ymin, ymax)
                Dim py0 = ToPixelY(0, ymin, ymax)
                _g.FillRectangle(br, px0, py1, px1 - px0, py0 - py1)
                _g.DrawRectangle(pen, px0, py1, px1 - px0, py0 - py1)
            Next
        End Using

        If ShowSigmaBands Then DrawSigmaBands(mean, sd, ymin, ymax)
        If ShowDensityCurve Then DrawNormalCurve(mean, sd, data.Length, binW, xmin, xmax, ymin, ymax)
        If ShowOutliers Then DrawOutliers(data, mean, sd, ymin)
    End Sub

    Private Sub DrawSigmaBands(mean As Double, sd As Double, ymin As Double, ymax As Double)
        Using pen As New Pen(Theme.AxisColor, Theme.LineWidth),
              br As New SolidBrush(Theme.TextColor)

            pen.DashStyle = DashStyle.Dash
            _g.DrawLine(pen, ToPixelX(mean, _viewMin, _viewMax), ToPixelY(ymin, ymin, ymax),
                        ToPixelX(mean, _viewMin, _viewMax), ToPixelY(ymax, ymin, ymax))

            For Each k In SigmaLevels
                For Each sign In {-1, 1}
                    Dim xv = mean + sign * k * sd
                    Dim px = ToPixelX(xv, _viewMin, _viewMax)
                    _g.DrawLine(pen, px, ToPixelY(ymin, ymin, ymax), px, ToPixelY(ymax * 0.9, ymin, ymax))
                    Dim label = If(sign < 0, "-" & k & "σ", "+" & k & "σ")
                    _g.DrawString(label, Theme.TickLabelFont, br, px - 8, ToPixelY(ymax * 0.92, ymin, ymax))
                Next
            Next
        End Using
    End Sub

    ' Plot() 算好的视图值域，供各个子步骤复用（避免重复传入导致不一致）
    Private _viewMin As Double
    Private _viewMax As Double
    Private _viewYMin As Double
    Private _viewYMax As Double

    Private Sub DrawNormalCurve(mean As Double, sd As Double, n As Integer, binW As Double,
                                xmin As Double, xmax As Double, ymin As Double, ymax As Double)
        Dim steps = 256
        Dim pts As New List(Of PointF)()
        Dim scale = n * binW

        For i = 0 To steps - 1
            Dim x = xmin + (xmax - xmin) * i / (steps - 1)
            Dim z = (x - mean) / sd
            Dim density = std.Exp(-0.5 * z * z) / (sd * std.Sqrt(2 * std.PI)) * scale

            pts.Add(New PointF(ToPixelX(x, xmin, xmax), ToPixelY(density, ymin, ymax)))
        Next

        Using pen As New Pen(Color.Crimson, Theme.LineWidth * 1.4F)
            _g.DrawLines(pen, pts.ToArray())
        End Using
    End Sub

    Private Sub DrawOutliers(data As Double(), mean As Double, sd As Double, ymin As Double)
        Dim lo = mean - OutlierSigma * sd
        Dim hi = mean + OutlierSigma * sd

        Using br As New SolidBrush(OutlierColor)
            For Each v In data
                If v < lo OrElse v > hi Then
                    Dim px = ToPixelX(v, _viewMin, _viewMax)
                    _g.FillEllipse(br, px - 3, ToPixelY(ymin, _viewYMin, _viewYMax) - 6, 6, 6)
                End If
            Next
        End Using
    End Sub
End Class
