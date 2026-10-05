#Region "Microsoft.VisualBasic::d2e6a8c4b1f74905c6a3e8d2b7f1c9a5, Data_science\Visualization\DataPlot\Statistics\RegressionPlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class RegressionPlot
    ' 
    '     Properties: Fit, ShowConfidenceBand, ShowEquation, ShowPoints
    '                 PointColor, FitColor
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
'  RegressionPlot.vb - 回归拟合图
'
'  拟合算法由调用方完成（旧实现依赖 DataFittings 的 IFitted），结果通过
'  <see cref="IRegressionFit"/> 传进来：观测点 x/y、拟合值 yfit、可选的置信带
'  与方程文本。DataPlot 只负责把这几样东西画明白。
' ============================================================================

''' <summary>回归拟合图</summary>
Public Class RegressionPlot
    Inherits PlotEngine

    ''' <summary>拟合结果</summary>
    Public Property Fit As IRegressionFit = Nothing
    ''' <summary>是否绘制观测散点</summary>
    Public Property ShowPoints As Boolean = True
    ''' <summary>是否绘制置信带（数据可用时）</summary>
    Public Property ShowConfidenceBand As Boolean = True
    ''' <summary>是否在图上标注拟合方程</summary>
    Public Property ShowEquation As Boolean = True
    ''' <summary>观测点颜色</summary>
    Public Property PointColor As Color = Color.SteelBlue
    ''' <summary>拟合曲线颜色</summary>
    Public Property FitColor As Color = Color.Crimson
    ''' <summary>观测点直径，小于等于 0 时用主题设定</summary>
    Public Property MarkerSize As Single = 0

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    Public Sub Plot()
        If Fit Is Nothing Then Throw New InvalidOperationException("RegressionPlot requires a fit result.")

        Dim x = Fit.X
        Dim y = Fit.Y
        Dim yfit = Fit.Yfit

        If x Is Nothing OrElse y Is Nothing OrElse x.Length = 0 Then
            Throw New InvalidOperationException("Regression fit has no observation data.")
        End If

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim xmin = If(Me.XMin, x.Min())
        Dim xmax = If(Me.XMax, x.Max())
        Dim ymin = If(Me.YMin, std.Min(y.Min(), If(yfit IsNot Nothing AndAlso yfit.Length > 0, yfit.Min(), y.Min())))
        Dim ymax = If(Me.YMax, std.Max(y.Max(), If(yfit IsNot Nothing AndAlso yfit.Length > 0, yfit.Max(), y.Max())))

        If ShowConfidenceBand AndAlso Fit.BandLower IsNot Nothing AndAlso Fit.BandUpper IsNot Nothing Then
            ymin = std.Min(ymin, Fit.BandLower.Min())
            ymax = std.Max(ymax, Fit.BandUpper.Max())
        End If

        Geometry.ExpandRange(xmin, xmax, 0.05)
        Geometry.ExpandRange(ymin, ymax, 0.08)

        DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        If ShowConfidenceBand Then DrawBand(x, xmin, xmax, ymin, ymax)

        If yfit IsNot Nothing AndAlso yfit.Length > 1 Then
            Dim order = Enumerable.Range(0, std.Min(x.Length, yfit.Length)) _
                                  .Select(Function(i) New PointF(ToPixelX(x(i), xmin, xmax),
                                                                 ToPixelY(yfit(i), ymin, ymax))) _
                                  .OrderBy(Function(p) p.X).ToArray()
            Using pen As New Pen(FitColor, Theme.LineWidth * 1.5F)
                _g.DrawLines(pen, order)
            End Using
        End If

        If ShowPoints Then
            Dim size = If(MarkerSize > 0, MarkerSize, Theme.MarkerSize)
            For i = 0 To y.Length - 1
                DrawMarker(ToPixelX(x(i), xmin, xmax), ToPixelY(y(i), ymin, ymax),
                           MarkerShape.Circle, size, PointColor)
            Next
        End If

        If ShowEquation AndAlso Not String.IsNullOrEmpty(Fit.EquationText) Then
            DrawEquationLabel(Fit.EquationText)
        End If

        DrawFitLegend()
    End Sub

    Private Sub DrawBand(x As Double(), xmin As Double, xmax As Double, ymin As Double, ymax As Double)
        Dim lower = Fit.BandLower, upper = Fit.BandUpper
        If lower Is Nothing OrElse upper Is Nothing Then Return

        Dim n = System.Math.Min(System.Math.Min(x.Length, lower.Length), upper.Length)
        If n < 2 Then Return

        Dim idx = Enumerable.Range(0, n).OrderBy(Function(i) x(i)).ToArray()
        Dim poly As New List(Of PointF)()

        For Each i In idx
            poly.Add(New PointF(ToPixelX(x(i), xmin, xmax), ToPixelY(upper(i), ymin, ymax)))
        Next
        For Each i In idx.Reverse()
            poly.Add(New PointF(ToPixelX(x(i), xmin, xmax), ToPixelY(lower(i), ymin, ymax)))
        Next

        Using br As New SolidBrush(Color.FromArgb(50, FitColor))
            _g.FillPolygon(br, poly.ToArray())
        End Using
    End Sub

    Private Sub DrawEquationLabel(text As String)
        Dim size = MeasureString(text, Theme.AnnotationFont)
        Dim x = _plotArea.Right - size.Width - 10
        Dim y = _plotArea.Top + 6

        DrawLabel(text, Theme.AnnotationFont, New SolidBrush(Theme.TextColor), x, y,
                  backgroundColor:=Color.FromArgb(230, Theme.LegendBackgroundColor),
                  borderColor:=Theme.LegendBorderColor)
    End Sub

    Private Sub DrawFitLegend()
        Dim legends As New List(Of Series) From {
            New Series With {.Name = "observed", .Color = PointColor, .MarkerShape = MarkerShape.Circle},
            New Series With {.Name = "fitted", .Color = FitColor, .MarkerShape = MarkerShape.None}
        }

        DrawLegend(legends)
    End Sub
End Class
