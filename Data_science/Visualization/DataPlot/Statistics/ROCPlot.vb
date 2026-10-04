#Region "Microsoft.VisualBasic::b4d8c2e6a1f34907d5b8c1e3a7f9d2b6, Data_science\Visualization\DataPlot\Statistics\ROCPlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class ROCPlot
    ' 
    '     Properties: Curves, FillAUC, ShowReference, ShowAUCValue, ShowDiagonal
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
'  ROCPlot.vb - ROC 曲线
'
'  TPR / FPR 序列由调用方给出（旧实现依赖 DataMining 的 Validation 类型算 AUC，
'  这里改由 ROCCurve 承载，算法结果仍然在调用方）。AUC 缺失时按梯形法补算，
'  保证图上的标注始终有值。
' ============================================================================

''' <summary>ROC 曲线图</summary>
Public Class ROCPlot
    Inherits PlotEngine

    ''' <summary>要绘制的曲线（可以多条）</summary>
    Public Property Curves As List(Of ROCCurve) = New List(Of ROCCurve)()
    ''' <summary>是否用半透明色填充曲线下面积</summary>
    Public Property FillAUC As Boolean = True
    ''' <summary>是否绘制随机猜测对角线</summary>
    Public Property ShowDiagonal As Boolean = True
    ''' <summary>是否在图例文本里带上 AUC 值</summary>
    Public Property ShowAUCValue As Boolean = True

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        Dim curves = CurveList()
        If Curves Is Nothing OrElse Curves.Count = 0 Then
            Throw New InvalidOperationException("ROCPlot requires at least one ROC curve.")
        End If

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim xmin As Double = 0, xmax As Double = 1, ymin As Double = 0, ymax As Double = 1
        If Me.XMin IsNot Nothing Then xmin = Me.XMin.Value
        If Me.XMax IsNot Nothing Then xmax = Me.XMax.Value
        If Me.YMin IsNot Nothing Then ymin = Me.YMin.Value
        If Me.YMax IsNot Nothing Then ymax = Me.YMax.Value

        DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        If ShowDiagonal Then
            Using pen As New Pen(Theme.AxisColor, Theme.LineWidth)
                pen.DashStyle = DashStyle.Dash
                DrawAbline(0, 1, xmin, xmax, ymin, ymax, pen)
            End Using
        End If

        For i = 0 To curves.Count - 1
            Dim c = curves(i)
            Dim color = If(c.Color, Theme.Palette(i Mod Theme.Palette.Length))
            Dim tpr = c.TPR
            Dim fpr = c.FPR
            Dim n = std.Min(tpr.Length, fpr.Length)
            If n < 2 Then Continue For

            ' 强制从原点起、到 (1,1) 收尾，保证面积完整
            Dim pts As New List(Of PointF) From {New PointF(ToPixelX(0, xmin, xmax), ToPixelY(0, ymin, ymax))}
            For j = 0 To n - 1
                pts.Add(New PointF(ToPixelX(fpr(j), xmin, xmax), ToPixelY(tpr(j), ymin, ymax)))
            Next
            pts.Add(New PointF(ToPixelX(1, xmin, xmax), ToPixelY(1, ymin, ymax)))

            If FillAUC Then
                Dim poly As New List(Of PointF)(pts)
                poly.Add(New PointF(ToPixelX(1, xmin, xmax), ToPixelY(0, ymin, ymax)))
                poly.Add(New PointF(ToPixelX(0, xmin, xmax), ToPixelY(0, ymin, ymax)))
                Using br As New SolidBrush(Color.FromArgb(45, color))
                    _g.FillPolygon(br, poly.ToArray())
                End Using
            End If

            Using pen As New Pen(color, Theme.LineWidth * 1.4F)
                _g.DrawLines(pen, pts.ToArray())
            End Using
        Next

        DrawCurveLegend(curves)
    End Sub

    Private Function CurveList() As List(Of ROCCurve)
        Return If(Curves, New List(Of ROCCurve)())
    End Function

    ''' <summary>梯形法估算 AUC（调用方没给 AUC 时兜底）</summary>
    Public Shared Function EstimateAUC(curve As ROCCurve) As Double
        If curve Is Nothing Then Return Double.NaN
        If Not Double.IsNaN(curve.AUC) Then Return curve.AUC

        Dim n = std.Min(curve.FPR.Length, curve.TPR.Length)
        If n < 2 Then Return Double.NaN

        Dim area As Double = 0
        For i = 0 To n - 2
            Dim dx = curve.FPR(i + 1) - curve.FPR(i)
            area += dx * (curve.TPR(i) + curve.TPR(i + 1)) / 2
        Next

        Return area
    End Function

    Private Sub DrawCurveLegend(curves As List(Of ROCCurve))
        Dim legends As New List(Of Series)()

        For i = 0 To curves.Count - 1
            Dim c = curves(i)
            Dim name = If(ShowAUCValue, $"{c.Name} (AUC = {EstimateAUC(c):F3})", c.Name)

            legends.Add(New Series With {
                .Name = name,
                .Color = If(c.Color, Theme.Palette(i Mod Theme.Palette.Length)),
                .MarkerShape = MarkerShape.Circle
            })
        Next

        DrawLegend(legends)
    End Sub
End Class
