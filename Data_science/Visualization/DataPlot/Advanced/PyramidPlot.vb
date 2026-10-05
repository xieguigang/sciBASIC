#Region "Microsoft.VisualBasic::8e4b2c7f1a3d4905c7e9b2f4a6d8c1e3, Data_science\Visualization\DataPlot\Advanced\PyramidPlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class PyramidPlot
    ' 
    '     Properties: Categories, LeftName, LeftValues, RightName, RightValues
    '                 ShowValueLabels, Stacked
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

' ============================================================================
'  PyramidPlot.vb - 金字塔图（人口年龄结构图）
'
'  旧实现里的 Pyramid 其实是一条「自下而上逐层收窄的堆叠三角比例图」，
'  与常见的人口金字塔不是一回事。这里按后者重写：中间是分类轴（年龄组），
'  左右两侧各显示一个分组的取值，方便直接比较两个群体的构成。
' ============================================================================

''' <summary>金字塔图（人口年龄结构图）</summary>
Public Class PyramidPlot
    Inherits PlotEngine

    ''' <summary>分类（通常是年龄组）</summary>
    Public Property Categories As String() = {}
    ''' <summary>左侧分组的取值</summary>
    Public Property LeftValues As Double() = {}
    ''' <summary>右侧分组的取值</summary>
    Public Property RightValues As Double() = {}
    ''' <summary>左侧分组名</summary>
    Public Property LeftName As String = "Left"
    ''' <summary>右侧分组名</summary>
    Public Property RightName As String = "Right"
    ''' <summary>是否在条形末端写出数值</summary>
    Public Property ShowValueLabels As Boolean = False
    ''' <summary>左右两侧的填充色，留空时用主题调色板前两色</summary>
    Public Property LeftColor As Color? = Nothing
    Public Property RightColor As Color? = Nothing

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    ''' <summary>直接在外部提供的绘图设备上绘制（图层叠加模式 / 宿主画布）。</summary>
    Public Sub New(g As IGraphics, Optional theme As PlotTheme = Nothing)
        MyBase.New(g, theme)
    End Sub

    Public Sub Plot()
        If Categories Is Nothing OrElse Categories.Length = 0 Then
            Throw New InvalidOperationException("PyramidPlot requires at least one category.")
        End If

        Dim nCat = Categories.Length

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim maxLeft = If(LeftValues.Length > 0, LeftValues.Max(), 0)
        Dim maxRight = If(RightValues.Length > 0, RightValues.Max(), 0)
        Dim vmax = std.Max(maxLeft, maxRight)
        If vmax <= 0 Then vmax = 1
        vmax *= 1.08

        ' 横轴 = 数值（左右对称），纵轴 = 分类
        Dim vticks = Enumerable.Range(0, CInt(vmax)).Select(Function(i) CDbl(i)).ToArray()
        DrawAxisAndGrid(-vmax, vmax, -0.5, nCat - 0.5, Nothing,
                        Enumerable.Range(0, nCat).Select(Function(i) CDbl(i)).ToArray(),
                        Nothing, Categories)

        Dim groupH = _plotArea.Height / nCat
        Dim barH = groupH * (1 - Theme.BarPadding * 2)
        Dim zero = ToPixelX(0, -vmax, vmax)
        Dim leftColor = If(Me.LeftColor, Theme.Palette(0))
        Dim rightColor = If(Me.RightColor, Theme.Palette(1))

        For i = 0 To nCat - 1
            Dim cy = _plotArea.Top + (i + 0.5) * groupH
            Dim top = cy - barH / 2

            If i < LeftValues.Length Then
                Dim px = ToPixelX(-LeftValues(i), -vmax, vmax)
                DrawBar(px, top, zero - px, barH, leftColor, LeftValues(i), toLeft:=True)
            End If
            If i < RightValues.Length Then
                Dim px = ToPixelX(RightValues(i), -vmax, vmax)
                DrawBar(zero, top, px - zero, barH, rightColor, RightValues(i), toLeft:=False)
            End If
        Next

        DrawGroupLegend(leftColor, rightColor)
    End Sub

    Private Sub DrawBar(x As Single, y As Single, w As Single, h As Single, color As Color,
                        value As Double, toLeft As Boolean)
        Using br As New SolidBrush(color),
              pen As New Pen(Theme.BorderColor, 0.5F)
            _g.FillRectangle(br, x, y, w, h)
            _g.DrawRectangle(pen, x, y, w, h)
        End Using

        If Not ShowValueLabels Then Return

        Dim text = FormatNumber(value)
        Dim size = MeasureString(text, Theme.TickLabelFont)

        Using br As New SolidBrush(Theme.TextColor)
            If toLeft Then
                _g.DrawString(text, Theme.TickLabelFont, br, x - size.Width - 4, y + h / 2 - size.Height / 2)
            Else
                _g.DrawString(text, Theme.TickLabelFont, br, x + w + 4, y + h / 2 - size.Height / 2)
            End If
        End Using
    End Sub

    Private Sub DrawGroupLegend(leftColor As Color, rightColor As Color)
        Dim legends As New List(Of Series) From {
            New Series With {.Name = LeftName, .Color = leftColor, .MarkerShape = MarkerShape.Square},
            New Series With {.Name = RightName, .Color = rightColor, .MarkerShape = MarkerShape.Square}
        }

        DrawLegend(legends)
    End Sub
End Class
