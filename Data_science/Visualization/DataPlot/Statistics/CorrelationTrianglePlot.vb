#Region "Microsoft.VisualBasic::6b1d4f8c3e5a4909f2b7e1d4c6a8f3e5, Data_science\Visualization\DataPlot\Statistics\CorrelationTrianglePlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class CorrelationTrianglePlot
    ' 
    '     Properties: Correlation, ShowValues, ShowUpper, ColorMap
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

' ============================================================================
'  CorrelationTrianglePlot.vb - 相关矩阵下三角图
'
'  矩阵是对称的，画半边就够了：对角线用圆形做大、颜色表示符号与强度，
'  这种形式在高维变量下比整张矩阵更容易读。
' ============================================================================

''' <summary>相关性矩阵下三角图</summary>
Public Class CorrelationTrianglePlot
    Inherits PlotEngine

    ''' <summary>相关性矩阵</summary>
    Public Property Correlation As CorrelationMatrix = Nothing
    ''' <summary>True 时画上三角，False 时画下三角</summary>
    Public Property ShowUpper As Boolean = False
    ''' <summary>是否把系数写成数字叠在圆点上</summary>
    Public Property ShowValues As Boolean = True
    ''' <summary>色阶（默认零中心发散）</summary>
    Public Property ColorMap As ColorScale.ColorMapType = ColorScale.ColorMapType.CoolWarm
    Public Property ValueRange As (min As Double, max As Double) = (-1, 1)

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    ''' <summary>直接在外部提供的绘图设备上绘制（图层叠加模式 / 宿主画布）。</summary>
    Public Sub New(g As IGraphics, Optional theme As PlotTheme = Nothing)
        MyBase.New(g, theme)
    End Sub

    Public Sub Plot()
        If Correlation Is Nothing OrElse Not Correlation.ValidateShape() Then
            Throw New InvalidOperationException("A well-shaped correlation matrix is required.")
        End If

        Dim names = Correlation.Names
        Dim m = Correlation.Matrix
        Dim n = names.Length

        DrawBackground()
        ComputePlotArea()
        DrawTitle()

        Dim labelPad As Single = 80
        Dim side = std.Min(_plotArea.Width - labelPad - 80, _plotArea.Height - 20)
        Dim cell = side / n
        Dim x0 = _plotArea.Left + labelPad
        Dim y0 = _plotArea.Top + 10

        For i = 0 To n - 1
            For j = 0 To n - 1
                Dim keep = If(ShowUpper, j > i, j < i)
                If Not keep Then Continue For

                Dim v = m(i, j)
                Dim cx As Single = CSng(x0 + (j + 0.5) * cell)
                Dim cy As Single = CSng(y0 + (i + 0.5) * cell)
                Dim radius As Single = CSng(std.Abs(v) * cell * 0.45)
                If radius < 0.5F Then Continue For

                Using br As New SolidBrush(ColorScale.GetColor(v, ValueRange.min, ValueRange.max, ColorMap))
                    _g.FillEllipse(br, cx - radius, cy - radius, radius * 2, radius * 2)
                End Using

                If ShowValues AndAlso cell > 26 Then
                    Dim text = v.ToString("F2")
                    Dim size = MeasureString(text, Theme.TickLabelFont)
                    Using br As New SolidBrush(ColorScale.ReadableTextColor(
                            ColorScale.GetColor(v, ValueRange.min, ValueRange.max, ColorMap)))
                        _g.DrawString(text, Theme.TickLabelFont, br, cx - size.Width / 2, cy - size.Height / 2)
                    End Using
                End If
            Next
        Next

        DrawVariableNames(names, x0, y0, cell, n)

        ColorScale.DrawColorLegend(_g, Theme, ColorMap, ValueRange.min, ValueRange.max,
                                   x0 + side + 20, y0, Theme.ColorBarWidth, side, title:="correlation")
    End Sub

    Private Sub DrawVariableNames(names As String(), x0 As Single, y0 As Single, cell As Single, n As Integer)
        Using br As New SolidBrush(Theme.TextColor)
            For i = 0 To n - 1
                Dim size = MeasureString(names(i), Theme.TickLabelFont)
                _g.DrawString(names(i), Theme.TickLabelFont, br, x0 - size.Width - 6,
                              y0 + (i + 0.5) * cell - size.Height / 2)

                _g.TranslateTransform(x0 + (i + 0.5) * cell, y0 - 6)
                _g.RotateTransform(-45)
                _g.DrawString(names(i), Theme.TickLabelFont, br, -size.Width, -size.Height / 2)
                _g.ResetTransform()
            Next
        End Using
    End Sub
End Class
