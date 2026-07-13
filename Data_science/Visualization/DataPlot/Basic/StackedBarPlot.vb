#Region "Microsoft.VisualBasic::5bb03c2582b1096f5dc3fb9bbf54ea9c, Data_science\Visualization\DataPlot\Basic\StackedBarPlot.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 178
    '    Code Lines: 157 (88.20%)
    ' Comment Lines: 10 (5.62%)
    '    - Xml Docs: 30.00%
    ' 
    '   Blank Lines: 11 (6.18%)
    '     File Size: 8.23 KB


    ' Class StackedBarPlot
    ' 
    '     Properties: Categories, Horizontal, MultiValues, SeriesNames, ShowTotalLabel
    '                 ShowValueLabels
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

''' <summary>堆叠柱状图（多系列沿 Y 轴累加堆叠）</summary>
Public Class StackedBarPlot
    Inherits PlotEngine

    Public Property Categories As String() = {}
    ''' <summary>多系列数据 [系列, 类别]</summary>
    Public Property MultiValues As Double(,) = Nothing
    Public Property SeriesNames As String() = {}
    Public Property Horizontal As Boolean = False
    Public Property ShowValueLabels As Boolean = False
    ''' <summary>仅显示总计标签（堆叠顶部）</summary>
    Public Property ShowTotalLabel As Boolean = False

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim nSer = MultiValues.GetLength(0)
        Dim nCat = MultiValues.GetLength(1)

        ' ---- 计算每个类别的累加和与最大值 ----
        Dim stackSums(nCat - 1) As Double
        Dim vmax = Double.MinValue
        Dim vmin = 0.0
        For j = 0 To nCat - 1
            Dim sum = 0.0
            For i = 0 To nSer - 1
                sum += MultiValues(i, j)
            Next
            stackSums(j) = sum
            If sum > vmax Then vmax = sum
        Next
        ' 考虑负值情况
        For j = 0 To nCat - 1
            For i = 0 To nSer - 1
                If MultiValues(i, j) < vmin Then vmin = MultiValues(i, j)
            Next
        Next
        If vmax <= 0 Then vmax = 1
        vmax *= 1.1
        If vmin < 0 Then vmin *= 1.1

        ' ---- 坐标轴 ----
        If Horizontal Then
            Dim xTicks = GenerateTicks(vmin, vmax)
            Dim yTicks = Enumerable.Range(0, nCat).Select(Function(i) CDbl(i)).ToArray()
            DrawAxisAndGrid(vmin, vmax, -0.5, nCat - 0.5, xTicks, yTicks, Nothing, Categories)
        Else
            Dim xTicks = Enumerable.Range(0, nCat).Select(Function(i) CDbl(i)).ToArray()
            Dim yTicks = GenerateTicks(vmin, vmax)
            DrawAxisAndGrid(-0.5, nCat - 0.5, vmin, vmax, xTicks, yTicks, Categories, Nothing)
        End If

        ' ---- 绘制堆叠柱 ----
        Dim groupWidth = If(Horizontal, _plotArea.Height / nCat, _plotArea.Width / nCat)
        Dim barWidth = groupWidth * (1 - Theme.BarPadding * 2)
        ' 每个类别各系列的累积偏移（正/负分别累加）
        Dim posBase(nCat - 1) As Double
        Dim negBase(nCat - 1) As Double

        For i = 0 To nSer - 1
            Dim color = Theme.Palette(i Mod Theme.Palette.Length)
            For j = 0 To nCat - 1
                Dim val = MultiValues(i, j)
                If Horizontal Then
                    Dim cy = _plotArea.Top + (j + 0.5) * groupWidth
                    Dim bx0Base As Double, bx1Base As Double
                    If val >= 0 Then
                        bx0Base = posBase(j) : bx1Base = posBase(j) + val
                        posBase(j) += val
                    Else
                        bx0Base = negBase(j) + val : bx1Base = negBase(j)
                        negBase(j) += val
                    End If
                    Dim bx0 = ToPixelX(bx0Base, vmin, vmax)
                    Dim bx1 = ToPixelX(bx1Base, vmin, vmax)
                    Dim rect = New RectangleF(Math.Min(bx0, bx1), cy - barWidth * 0.45,
                                              Math.Abs(bx1 - bx0), barWidth * 0.9)
                    Using br As New SolidBrush(color)
                        _g.FillRectangle(br, rect)
                    End Using
                    Using pen As New Pen(Theme.BorderColor, 0.5F)
                        _g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height)
                    End Using
                    If ShowValueLabels Then
                        Using br As New SolidBrush(Theme.TextColor),
                              sf As New StringFormat()
                            sf.Alignment = StringAlignment.Center
                            sf.LineAlignment = StringAlignment.Center
                            _g.DrawString(FormatNumber(val), Theme.TickLabelFont, br,
                                          rect.X + rect.Width / 2, rect.Y + rect.Height / 2)
                        End Using
                    End If
                Else
                    Dim cx = _plotArea.Left + (j + 0.5) * groupWidth
                    Dim by0Base As Double, by1Base As Double
                    If val >= 0 Then
                        by0Base = posBase(j) : by1Base = posBase(j) + val
                        posBase(j) += val
                    Else
                        by0Base = negBase(j) + val : by1Base = negBase(j)
                        negBase(j) += val
                    End If
                    Dim by0 = ToPixelY(by0Base, vmin, vmax)
                    Dim by1 = ToPixelY(by1Base, vmin, vmax)
                    Dim bx = cx - barWidth / 2
                    Dim rect = New RectangleF(bx, Math.Min(by0, by1),
                                              barWidth, Math.Abs(by1 - by0))
                    Using br As New SolidBrush(color)
                        _g.FillRectangle(br, rect)
                    End Using
                    Using pen As New Pen(Theme.BorderColor, 0.5F)
                        _g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height)
                    End Using
                    If ShowValueLabels Then
                        Using br As New SolidBrush(Theme.TextColor),
                              sf As New StringFormat()
                            sf.Alignment = StringAlignment.Center
                            sf.LineAlignment = StringAlignment.Center
                            _g.DrawString(FormatNumber(val), Theme.TickLabelFont, br,
                                          rect.X + rect.Width / 2, rect.Y + rect.Height / 2)
                        End Using
                    End If
                End If
            Next
        Next

        ' ---- 总计标签（堆叠顶部）----
        If ShowTotalLabel Then
            Using br As New SolidBrush(Theme.TextColor)
                For j = 0 To nCat - 1
                    If stackSums(j) = 0 Then Continue For
                    If Horizontal Then
                        Dim cy = _plotArea.Top + (j + 0.5) * groupWidth
                        Dim bxEnd = ToPixelX(stackSums(j), vmin, vmax)
                        Using sf As New StringFormat()
                            sf.Alignment = StringAlignment.Near
                            sf.LineAlignment = StringAlignment.Center
                            _g.DrawString(FormatNumber(stackSums(j)), Theme.TickLabelFont, br,
                                          bxEnd + 4, cy)
                        End Using
                    Else
                        Dim cx = _plotArea.Left + (j + 0.5) * groupWidth
                        Dim pyTop = ToPixelY(stackSums(j), vmin, vmax)
                        Using sf As New StringFormat()
                            sf.Alignment = StringAlignment.Center
                            sf.LineAlignment = StringAlignment.Far
                            _g.DrawString(FormatNumber(stackSums(j)), Theme.TickLabelFont, br,
                                          cx, pyTop - 2)
                        End Using
                    End If
                Next
            End Using
        End If

        ' ---- 图例 ----
        If nSer > 1 Then
            Dim seriesList As New List(Of Series)()
            For i = 0 To nSer - 1
                seriesList.Add(New Series With {
                    .Name = If(SeriesNames IsNot Nothing AndAlso i < SeriesNames.Length, SeriesNames(i), "Series " & (i + 1)),
                    .Color = Theme.Palette(i Mod Theme.Palette.Length),
                    .MarkerShape = MarkerShape.Square
                })
            Next
            DrawLegend(seriesList)
        End If
    End Sub
End Class

