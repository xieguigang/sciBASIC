#Region "Microsoft.VisualBasic::2ac28dc7fd8f4306a1d658eee7356070, Data_science\Visualization\DataPlot\Advanced\BoxPlot.vb"

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

    '   Total Lines: 142
    '    Code Lines: 122 (85.92%)
    ' Comment Lines: 9 (6.34%)
    '    - Xml Docs: 11.11%
    ' 
    '   Blank Lines: 11 (7.75%)
    '     File Size: 6.66 KB


    ' Class BoxPlot
    ' 
    '     Properties: Groups, Horizontal, ShowMean, ShowOutliers
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Quantile
    ' 
    '     Sub: Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging

''' <summary>盒须图（Box Plot / Box-and-Whisker）</summary>
Public Class BoxPlot
    Inherits PlotEngine

    Public Property Groups As New List(Of BoxGroup)()
    Public Property Horizontal As Boolean = False
    Public Property ShowOutliers As Boolean = True
    Public Property ShowMean As Boolean = True

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim nGrp = Groups.Count
        ' 计算全局 Y 范围
        Dim allData = Groups.SelectMany(Function(g) g.Data).ToList()
        Dim ymin = If(Me.YMin, allData.Min())
        Dim ymax = If(Me.YMax, allData.Max())
        Dim pad = (ymax - ymin) * 0.05
        ymin -= pad : ymax += pad

        Dim xTicks = Enumerable.Range(0, nGrp).Select(Function(i) CDbl(i)).ToArray()
        Dim yTicks = GenerateTicks(ymin, ymax)
        If Horizontal Then
            DrawAxisAndGrid(ymin, ymax, -0.5, nGrp - 0.5, yTicks, xTicks, Nothing, Groups.Select(Function(g) g.Name).ToArray())
        Else
            DrawAxisAndGrid(-0.5, nGrp - 0.5, ymin, ymax, xTicks, yTicks, Groups.Select(Function(g) g.Name).ToArray(), Nothing)
        End If

        Dim groupWidth = If(Horizontal, _plotArea.Height / nGrp, _plotArea.Width / nGrp)
        Dim boxWidth = groupWidth * 0.5

        For i = 0 To nGrp - 1
            Dim g = Groups(i)
            Dim color = If(g.Color, Theme.Palette(i Mod Theme.Palette.Length))
            Dim sorted = g.Data.OrderBy(Function(v) v).ToArray()
            Dim q1 = Quantile(sorted, 0.25)
            Dim q2 = Quantile(sorted, 0.5)
            Dim q3 = Quantile(sorted, 0.75)
            Dim iqr = q3 - q1
            Dim wLo = q1 - 1.5 * iqr
            Dim wHi = q3 + 1.5 * iqr
            Dim outliers = sorted.Where(Function(v) v < wLo OrElse v > wHi).ToList()
            Dim whiskerLo = sorted.Where(Function(v) v >= wLo).Min()
            Dim whiskerHi = sorted.Where(Function(v) v <= wHi).Max()

            Dim center As Single = If(Horizontal,
                            _plotArea.Top + (i + 0.5) * groupWidth,
                            _plotArea.Left + (i + 0.5) * groupWidth)

            If Horizontal Then
                ' 横向：X 是数值
                Dim pxQ1 = ToPixelX(q1, ymin, ymax)
                Dim pxQ2 = ToPixelX(q2, ymin, ymax)
                Dim pxQ3 = ToPixelX(q3, ymin, ymax)
                Dim pxWLo = ToPixelX(whiskerLo, ymin, ymax)
                Dim pxWHi = ToPixelX(whiskerHi, ymin, ymax)
                Dim by0 As Single = center - boxWidth / 2
                Dim by1 As Single = center + boxWidth / 2
                ' 盒子
                Using br As New SolidBrush(color.FromArgb(180, color)),
                      pen As New Pen(color, Theme.LineWidth)
                    _g.FillRectangle(br, pxQ1, by0, pxQ3 - pxQ1, by1 - by0)
                    _g.DrawRectangle(pen, pxQ1, by0, pxQ3 - pxQ1, by1 - by0)
                    ' 中位数线
                    _g.DrawLine(pen, pxQ2, by0, pxQ2, by1)
                    ' 须
                    _g.DrawLine(pen, pxQ1, center, pxWLo, center)
                    _g.DrawLine(pen, pxQ3, center, pxWHi, center)
                    _g.DrawLine(pen, pxWLo, by0 + 4, pxWLo, by1 - 4)
                    _g.DrawLine(pen, pxWHi, by0 + 4, pxWHi, by1 - 4)
                    ' 均值
                    If ShowMean Then
                        Dim mean = g.Data.Average()
                        Dim pxM = ToPixelX(mean, ymin, ymax)
                        Using dsh As New Pen(color, Theme.LineWidth)
                            dsh.DashStyle = DashStyle.Dot
                            _g.DrawLine(dsh, pxM, CSng(by0), pxM, CSng(by1))
                        End Using
                    End If
                End Using
                ' 异常值
                If ShowOutliers Then
                    For Each o In outliers
                        Dim px = ToPixelX(o, ymin, ymax)
                        DrawMarker(px, center, MarkerShape.Circle, 4, color)
                    Next
                End If
            Else
                ' 纵向：Y 是数值
                Dim pyQ1 = ToPixelY(q1, ymin, ymax)
                Dim pyQ2 = ToPixelY(q2, ymin, ymax)
                Dim pyQ3 = ToPixelY(q3, ymin, ymax)
                Dim pyWLo = ToPixelY(whiskerLo, ymin, ymax)
                Dim pyWHi = ToPixelY(whiskerHi, ymin, ymax)
                Dim bx0 = center - boxWidth / 2
                Dim bx1 = center + boxWidth / 2
                Using br As New SolidBrush(color.FromArgb(180, color)),
                      pen As New Pen(color, Theme.LineWidth)
                    _g.FillRectangle(br, CSng(bx0), pyQ3, CSng(bx1 - bx0), pyQ1 - pyQ3)
                    _g.DrawRectangle(pen, CSng(bx0), pyQ3, CSng(bx1 - bx0), pyQ1 - pyQ3)
                    _g.DrawLine(pen, CSng(bx0), pyQ2, CSng(bx1), pyQ2)
                    _g.DrawLine(pen, CSng(center), pyQ1, CSng(center), pyWLo)
                    _g.DrawLine(pen, CSng(center), pyQ3, CSng(center), pyWHi)
                    _g.DrawLine(pen, CSng(bx0 + 4), pyWLo, CSng(bx1 - 4), pyWLo)
                    _g.DrawLine(pen, CSng(bx0 + 4), pyWHi, CSng(bx1 - 4), pyWHi)
                    If ShowMean Then
                        Dim mean = g.Data.Average()
                        Dim pyM = ToPixelY(mean, ymin, ymax)
                        Using dsh As New Pen(color, Theme.LineWidth)
                            dsh.DashStyle = DashStyle.Dot
                            _g.DrawLine(dsh, CSng(bx0), pyM, CSng(bx1), pyM)
                        End Using
                    End If
                End Using
                If ShowOutliers Then
                    For Each o In outliers
                        Dim py = ToPixelY(o, ymin, ymax)
                        DrawMarker(center, py, MarkerShape.Circle, 4, color)
                    Next
                End If
            End If
        Next
    End Sub

    Private Shared Function Quantile(sorted As Double(), q As Double) As Double
        If sorted.Length = 0 Then Return 0
        Dim pos = (sorted.Length - 1) * q
        Dim lo = CInt(Math.Floor(pos))
        Dim hi = CInt(Math.Ceiling(pos))
        If lo = hi Then Return sorted(lo)
        Return sorted(lo) + (sorted(hi) - sorted(lo)) * (pos - lo)
    End Function
End Class
