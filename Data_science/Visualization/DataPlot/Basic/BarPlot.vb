#Region "Microsoft.VisualBasic::3cbe034182385627237f304716ed7335, Data_science\Visualization\DataPlot\Basic\BarPlot.vb"

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

    '   Total Lines: 125
    '    Code Lines: 109 (87.20%)
    ' Comment Lines: 7 (5.60%)
    '    - Xml Docs: 28.57%
    ' 
    '   Blank Lines: 9 (7.20%)
    '     File Size: 5.83 KB


    ' Class BarPlot
    ' 
    '     Properties: Categories, Horizontal, MultiValues, SeriesNames, ShowValueLabels
    '                 Values
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

''' <summary>柱状图（分类数据）</summary>
Public Class BarPlot
    Inherits PlotEngine

    Public Property Categories As String() = {}
    Public Property Values As Double() = {}
    Public Property SeriesNames As String() = {}
    ''' <summary>多系列时使用 [系列, 类别] 二维数组</summary>
    Public Property MultiValues As Double(,) = Nothing
    Public Property Horizontal As Boolean = False
    Public Property ShowValueLabels As Boolean = False

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim isMulti = (MultiValues IsNot Nothing)
        Dim nCat = Categories.Length
        Dim nSer = If(isMulti, MultiValues.GetLength(0), 1)

        ' 计算数值范围
        Dim vmin = 0.0, vmax = 1.0
        If isMulti Then
            vmin = 0 : vmax = Double.MinValue
            For i = 0 To nSer - 1
                For j = 0 To nCat - 1
                    If MultiValues(i, j) > vmax Then vmax = MultiValues(i, j)
                Next
            Next
        Else
            vmax = If(Values.Length > 0, Values.Max(), 1)
        End If
        If vmax <= 0 Then vmax = 1
        vmax *= 1.1

        ' 绘制坐标轴（类别轴范围设为 -0.5 ~ nCat-0.5，使刻度对齐柱子中心）
        If Horizontal Then
            ' 横向：X 是数值，Y 是分类
            Dim xTicks = GenerateTicks(vmin, vmax)
            Dim yTicks = Enumerable.Range(0, nCat).Select(Function(i) CDbl(i)).ToArray()
            DrawAxisAndGrid(vmin, vmax, -0.5, nCat - 0.5, xTicks, yTicks, Nothing, Categories)
        Else
            Dim xTicks = Enumerable.Range(0, nCat).Select(Function(i) CDbl(i)).ToArray()
            Dim yTicks = GenerateTicks(vmin, vmax)
            DrawAxisAndGrid(-0.5, nCat - 0.5, vmin, vmax, xTicks, yTicks, Categories, Nothing)
        End If

        ' 绘制柱子
        Dim groupWidth = If(Horizontal, _plotArea.Height / nCat, _plotArea.Width / nCat)
        Dim barWidth = groupWidth * (1 - Theme.BarPadding * 2) / nSer
        For i = 0 To nSer - 1
            Dim color = Theme.Palette(i Mod Theme.Palette.Length)
            For j = 0 To nCat - 1
                Dim val = If(isMulti, MultiValues(i, j), Values(j))
                If Horizontal Then
                    Dim cy = _plotArea.Top + (j + 0.5) * groupWidth
                    Dim bx0 = ToPixelX(0, vmin, vmax)
                    Dim bx1 = ToPixelX(val, vmin, vmax)
                    Dim by = cy - (nSer * barWidth) / 2 + i * barWidth + barWidth * 0.1
                    Dim rect = New RectangleF(Math.Min(bx0, bx1), by - barWidth * 0.45,
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
                            sf.Alignment = StringAlignment.Near
                            sf.LineAlignment = StringAlignment.Center
                            _g.DrawString(FormatNumber(val), Theme.TickLabelFont, br,
                                          rect.Right + 4, rect.Y + rect.Height / 2)
                        End Using
                    End If
                Else
                    Dim cx = _plotArea.Left + (j + 0.5) * groupWidth
                    Dim by0 = ToPixelY(0, vmin, vmax)
                    Dim by1 = ToPixelY(val, vmin, vmax)
                    Dim bx = cx - (nSer * barWidth) / 2 + i * barWidth + barWidth * 0.1
                    Dim rect = New RectangleF(bx, Math.Min(by0, by1),
                                              barWidth * 0.9, Math.Abs(by1 - by0))
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
                            sf.LineAlignment = StringAlignment.Far
                            _g.DrawString(FormatNumber(val), Theme.TickLabelFont, br,
                                          rect.X + rect.Width / 2, rect.Y - 2)
                        End Using
                    End If
                End If
            Next
        Next

        ' 图例（多系列时）
        If isMulti AndAlso nSer > 1 Then
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
