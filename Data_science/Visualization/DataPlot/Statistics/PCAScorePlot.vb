#Region "Microsoft.VisualBasic::f3a6c9e2b7d14058a9c4e1f7d3b8a2c5, Data_science\Visualization\DataPlot\Statistics\PCAScorePlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class PCAScorePlot
    ' 
    '     Properties: Score, ShowEllipse, ShowSampleNames, EllipseSigma
    '                 ShowOriginLines
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
'  PCAScorePlot.vb - PCA 得分图（含分组置信椭圆）
'
'  得分由 <see cref="IPCAScore"/> 传入（PCA 本身由调用方的算法包完成）。
'  椭圆几何在本模块内本地实现（见 Geometry.EstimateEllipse），
'  这样既不需要 ANOVA 包提供的 ChiSquareTest，也不需要引入额外的数学依赖。
' ============================================================================

''' <summary>PCA 二维得分图</summary>
Public Class PCAScorePlot
    Inherits PlotEngine

    ''' <summary>PCA 得分结果</summary>
    Public Property Score As IPCAScore = Nothing
    ''' <summary>是否按分组绘制置信椭圆</summary>
    Public Property ShowEllipse As Boolean = True
    ''' <summary>椭圆放大倍数（标准差倍数，2 约对应 95%）</summary>
    Public Property EllipseSigma As Double = 2
    ''' <summary>是否在每个点旁写出样本名</summary>
    Public Property ShowSampleNames As Boolean = False
    ''' <summary>是否画出过原点的两条坐标轴</summary>
    Public Property ShowOriginLines As Boolean = True
    ''' <summary>点直径，小于等于 0 时用主题设定</summary>
    Public Property MarkerSize As Single = 0

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    Public Sub Plot()
        If Score Is Nothing Then Throw New InvalidOperationException("PCAScorePlot requires a score set.")

        Dim pc1 = Score.PC1, pc2 = Score.PC2
        If pc1 Is Nothing OrElse pc2 Is Nothing OrElse pc1.Length = 0 Then
            Throw New InvalidOperationException("PCA score has no sample score data.")
        End If

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim n = std.Min(pc1.Length, pc2.Length)
        Dim xmin = If(Me.XMin, pc1.Min())
        Dim xmax = If(Me.XMax, pc1.Max())
        Dim ymin = If(Me.YMin, pc2.Min())
        Dim ymax = If(Me.YMax, pc2.Max())

        Geometry.ExpandRange(xmin, xmax, 0.08)
        Geometry.ExpandRange(ymin, ymax, 0.08)

        DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        If ShowOriginLines Then
            Using pen As New Pen(Theme.AxisColor, Theme.AxisLineWidth)
                DrawAbline(0, 0, xmin, xmax, ymin, ymax, pen)  ' y = 0 水平线
                _g.DrawLine(pen, ToPixelX(0, xmin, xmax), _plotArea.Top, ToPixelX(0, xmin, xmax), _plotArea.Bottom)
            End Using
        End If

        Dim groups = ResolveGroupMap(n)
        Dim size = If(MarkerSize > 0, MarkerSize, Theme.MarkerSize)

        For gi = 0 To groups.Count - 1
            Dim grp = groups(gi)
            Dim color = Theme.Palette(gi Mod Theme.Palette.Length)

            If ShowEllipse AndAlso grp.Value.Count >= 3 Then
                DrawConfidenceEllipse(grp.Value, xmin, xmax, ymin, ymax, color)
            End If

            For Each idx In grp.Value
                DrawMarker(ToPixelX(pc1(idx), xmin, xmax), ToPixelY(pc2(idx), ymin, ymax),
                           MarkerShape.Circle, size, color)
            Next

            If ShowSampleNames Then
                Using br As New SolidBrush(Theme.TextColor)
                    For Each idx In grp.Value
                        Dim name = If(idx < Score.SampleNames.Length, Score.SampleNames(idx), "")
                        If String.IsNullOrEmpty(name) Then Continue For
                        _g.DrawString(name, Theme.TickLabelFont, br,
                                      ToPixelX(pc1(idx), xmin, xmax) + 4, ToPixelY(pc2(idx), ymin, ymax) - 6)
                    Next
                End Using
            End If
        Next

        Dim legends As New List(Of Series)()
        For gi = 0 To groups.Count - 1
            legends.Add(New Series With {
                .Name = groups(gi).Key,
                .Color = Theme.Palette(gi Mod Theme.Palette.Length),
                .MarkerShape = MarkerShape.Circle
            })
        Next
        DrawLegend(legends)
    End Sub

    Private Function ResolveGroupMap(n As Integer) As List(Of KeyValuePair(Of String, List(Of Integer)))
        Dim map As New Dictionary(Of String, List(Of Integer))()
        Dim names = Score.Groups
        Dim hasGroups = names IsNot Nothing AndAlso names.Length >= n

        For i = 0 To n - 1
            Dim key = If(hasGroups AndAlso Not String.IsNullOrEmpty(names(i)), names(i), "all samples")
            If Not map.ContainsKey(key) Then map(key) = New List(Of Integer)()
            map(key).Add(i)
        Next

        Return map.ToList()
    End Function

    Private Sub DrawConfidenceEllipse(indices As List(Of Integer), xmin As Double, xmax As Double,
                                      ymin As Double, ymax As Double, color As Color)
        Dim pts = indices.Select(Function(i) New PointF(ToPixelX(Score.PC1(i), xmin, xmax),
                                                        ToPixelY(Score.PC2(i), ymin, ymax))).ToArray()
        Dim fit = Geometry.EstimateEllipse(pts, EllipseSigma)
        Dim cx = fit.Center.X, cy = fit.Center.Y

        _g.TranslateTransform(cx, cy)
        _g.RotateTransform(CSng(fit.Rotation * 180 / std.PI))

        Using pen As New Pen(color, Theme.LineWidth)
            _g.DrawEllipse(pen, -fit.RadiusX, -fit.RadiusY, fit.RadiusX * 2, fit.RadiusY * 2)
        End Using

        _g.ResetTransform()
    End Sub
End Class
