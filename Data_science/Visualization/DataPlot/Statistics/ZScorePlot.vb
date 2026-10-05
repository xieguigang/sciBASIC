#Region "Microsoft.VisualBasic::3d7b1e4a9c5f4915f8b4d7e1a3c6f9e2, Data_science\Visualization\DataPlot\Statistics\ZScorePlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class ZScorePlot
    ' 
    '     Properties: Entries, VariableNames, DisplayZERO, ColorMap, ShowValues
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
'  ZScorePlot.vb - Z-score 矩阵图
'
'  Z 值由调用方算好（旧实现用 Math.LinearAlgebra 的 Vector.Z 做标准化），
'  这里只把它铺成零中心的色阶矩阵；分组色条用来标出样本所属类别。
' ============================================================================

''' <summary>Z-score 矩阵图</summary>
Public Class ZScorePlot
    Inherits PlotEngine

    ''' <summary>每个观测对象一行</summary>
    Public Property Entries As List(Of ZScoreEntry) = New List(Of ZScoreEntry)()
    ''' <summary>变量名（列标签）</summary>
    Public Property VariableNames As String() = Nothing
    ''' <summary>是否显示取值为 0 的格子（False 时这些格子留白）</summary>
    Public Property DisplayZERO As Boolean = True
    ''' <summary>色阶（默认零中心发散）</summary>
    Public Property ColorMap As ColorScale.ColorMapType = ColorScale.ColorMapType.CoolWarm
    ''' <summary>是否绘制色阶图例条</summary>
    Public Property ShowColorLegend As Boolean = True
    ''' <summary>是否在格子中写出数值</summary>
    Public Property ShowValues As Boolean = False
    ''' <summary>分组标注条的宽度，0 表示不画</summary>
    Public Property GroupBarWidth As Single = 10.0F

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    Public Sub Plot()
        If Entries Is Nothing OrElse Entries.Count = 0 Then
            Throw New InvalidOperationException("ZScorePlot requires at least one entry.")
        End If

        DrawBackground()
        ComputePlotArea()
        DrawTitle()

        Dim rows = Entries.Count
        Dim cols = Entries.Max(Function(e) e.Values.Length)

        Dim vmin = Entries.SelectMany(Function(e) e.Values).Min()
        Dim vmax = Entries.SelectMany(Function(e) e.Values).Max()
        Dim bound = std.Max(std.Abs(vmin), std.Abs(vmax))
        If bound = 0 Then bound = 1

        Dim labelPad As Single = 90
        Dim groupPad = If(GroupBarWidth > 0, GroupBarWidth + 6, 0)
        Dim rightPad As Single = If(ShowColorLegend, 80, 20)
        Dim x0 = _plotArea.Left + labelPad + groupPad
        Dim y0 = _plotArea.Top
        Dim w = std.Max(40.0F, _plotArea.Width - labelPad - groupPad - rightPad)
        Dim h = std.Max(40.0F, _plotArea.Height)
        Dim cellW = w / cols
        Dim cellH = h / rows

        For r = 0 To rows - 1
            Dim entry = Entries(r)

            For col As Integer = 0 To entry.Values.Length - 1
                Dim v = entry.Values(col)

                If Not DisplayZERO AndAlso v = 0 Then Continue For

                Dim rect = New RectangleF(x0 + col * cellW, y0 + r * cellH, cellW, cellH)
                Using br As New SolidBrush(ColorScale.GetColor(v, -bound, bound, ColorMap))
                    _g.FillRectangle(br, rect)
                End Using

                If ShowValues Then
                    Dim text = v.ToString("F1")
                    Dim size = MeasureString(text, Theme.TickLabelFont)
                    Using br As New SolidBrush(ColorScale.ReadableTextColor(ColorScale.GetColor(v, -bound, bound, ColorMap)))
                        _g.DrawString(text, Theme.TickLabelFont, br,
                                      rect.X + rect.Width / 2 - size.Width / 2,
                                      rect.Y + rect.Height / 2 - size.Height / 2)
                    End Using
                End If
            Next

            ' 行名
            Using br As New SolidBrush(Theme.TextColor)
                Dim size = MeasureString(entry.Name, Theme.TickLabelFont)
                _g.DrawString(entry.Name, Theme.TickLabelFont, br, _plotArea.Left + labelPad - size.Width - 6,
                              y0 + (r + 0.5) * cellH - size.Height / 2)
            End Using

            ' 分组色条
            If GroupBarWidth > 0 AndAlso Not String.IsNullOrEmpty(entry.Group) Then
                Dim color = If(entry.Color, GroupColor(entry.Group))
                Using br As New SolidBrush(color)
                    _g.FillRectangle(br, _plotArea.Left + labelPad, y0 + r * cellH, GroupBarWidth, cellH)
                End Using
            End If
        Next

        Using pen As New Pen(Theme.BorderColor, 0.5F)
            _g.DrawRectangle(pen, x0, y0, w, h)
        End Using

        If VariableNames IsNot Nothing Then DrawColumnLabels(x0, y0, cellW)

        If ShowColorLegend Then
            ColorScale.DrawColorLegend(_g, Theme, ColorMap, -bound, bound,
                                       x0 + w + 20, y0, Theme.ColorBarWidth, h, title:="Z")
        End If
    End Sub

    Private Function GroupColor(name As String) As Color
        Dim groups = Entries.Select(Function(e) e.Group).Distinct().OrderBy(Function(g) g).ToArray()
        Dim idx = Array.IndexOf(groups, name)
        If idx < 0 Then idx = 0
        Return Theme.Palette(idx Mod Theme.Palette.Length)
    End Function

    Private Sub DrawColumnLabels(x0 As Single, y0 As Single, cellW As Single)
        Using br As New SolidBrush(Theme.TextColor)
            For col As Integer = 0 To VariableNames.Length - 1
                Dim size = MeasureString(VariableNames(col), Theme.TickLabelFont)
                _g.TranslateTransform(x0 + (col + 0.5) * cellW, y0 - 4)
                _g.RotateTransform(-45)
                _g.DrawString(VariableNames(col), Theme.TickLabelFont, br, -size.Width, -size.Height / 2)
                _g.ResetTransform()
            Next
        End Using
    End Sub
End Class
