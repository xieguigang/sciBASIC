#Region "Microsoft.VisualBasic::b718e21710b5cd8bdb17cad25c12f313, Data_science\Visualization\DataPlot\Advanced\ViolinPlot.vb"

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

    '   Total Lines: 109
    '    Code Lines: 90 (82.57%)
    ' Comment Lines: 5 (4.59%)
    '    - Xml Docs: 20.00%
    ' 
    '   Blank Lines: 14 (12.84%)
    '     File Size: 4.48 KB


    ' Class ViolinPlot
    ' 
    '     Properties: Groups, ShowBoxInside
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Quantile
    ' 
    '     Sub: Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

''' <summary>小提琴图（Violin Plot）</summary>
Public Class ViolinPlot
    Inherits PlotEngine

    Public Property Groups As New List(Of BoxGroup)()
    Public Property ShowBoxInside As Boolean = True

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim nGrp = Groups.Count
        Dim allData = Groups.SelectMany(Function(g) g.Data).ToList()
        Dim ymin = If(Me.YMin, allData.Min())
        Dim ymax = If(Me.YMax, allData.Max())
        Dim pad = (ymax - ymin) * 0.05
        ymin -= pad : ymax += pad

        Dim xTicks = Enumerable.Range(0, nGrp).Select(Function(i) CDbl(i)).ToArray()
        Dim yTicks = GenerateTicks(ymin, ymax)
        DrawAxisAndGrid(-0.5, nGrp - 0.5, ymin, ymax, xTicks, yTicks,
                        Groups.Select(Function(g) g.Name).ToArray(), Nothing)

        Dim groupWidth = _plotArea.Width / nGrp
        Dim maxViolinWidth = groupWidth * 0.4

        For i = 0 To nGrp - 1
            Dim g = Groups(i)
            Dim color = If(g.Color, Theme.Palette(i Mod Theme.Palette.Length))
            Dim sorted = g.Data.OrderBy(Function(v) v).ToArray()
            If sorted.Length < 2 Then Continue For

            ' KDE：用直方图密度估计
            Dim nBins = Math.Min(40, Math.Max(10, sorted.Length \ 5))
            Dim dmin = sorted.Min()
            Dim dmax = sorted.Max()
            If dmax <= dmin Then dmax = dmin + 1
            Dim binW = (dmax - dmin) / nBins
            Dim counts = New Integer(nBins - 1) {}
            For Each v In sorted
                Dim idx = CInt(Math.Floor((v - dmin) / binW))
                If idx >= nBins Then idx = nBins - 1
                If idx < 0 Then idx = 0
                counts(idx) += 1
            Next
            Dim maxCount = counts.Max()

            Dim center As Single = _plotArea.Left + (i + 0.5) * groupWidth

            ' 构造小提琴轮廓（左右对称）
            Dim leftPts As New List(Of PointF)()
            Dim rightPts As New List(Of PointF)()
            For b = 0 To nBins - 1
                Dim y0 = dmin + b * binW
                Dim y1 = y0 + binW
                Dim yMid = (y0 + y1) / 2
                Dim w = counts(b) / maxCount * maxViolinWidth
                Dim py = ToPixelY(yMid, ymin, ymax)
                leftPts.Add(New PointF(center - w, py))
                rightPts.Add(New PointF(center + w, py))
            Next
            ' 闭合路径
            Dim allPts As New List(Of PointF)()
            allPts.AddRange(leftPts)
            rightPts.Reverse()
            allPts.AddRange(rightPts)

            Using br As New SolidBrush(color.FromArgb(160, color)),
                  pen As New Pen(color, Theme.LineWidth)
                _g.FillPolygon(br, allPts.ToArray())
                _g.DrawPolygon(pen, allPts.ToArray())
            End Using

            ' 内部盒须
            If ShowBoxInside Then
                Dim q1 As Single = Quantile(sorted, 0.25)
                Dim q2 As Single = Quantile(sorted, 0.5)
                Dim q3 As Single = Quantile(sorted, 0.75)
                Dim innerW As Single = maxViolinWidth * 0.15
                Dim pyQ1 = ToPixelY(q1, ymin, ymax)
                Dim pyQ2 = ToPixelY(q2, ymin, ymax)
                Dim pyQ3 = ToPixelY(q3, ymin, ymax)
                Using br As New SolidBrush(color.FromArgb(220, 30, 30, 30)),
                      pen As New Pen(color.FromArgb(50, 50, 50), Theme.LineWidth)
                    _g.FillRectangle(br, center - innerW, pyQ3, innerW * 2, pyQ1 - pyQ3)
                    _g.DrawRectangle(pen, center - innerW, pyQ3, innerW * 2, pyQ1 - pyQ3)
                    _g.FillEllipse(br, center - 2, pyQ2 - 2, 4, 4)
                End Using
            End If
        Next
    End Sub

    Private Shared Function Quantile(sorted As Double(), q As Double) As Double
        Dim pos = (sorted.Length - 1) * q
        Dim lo = CInt(Math.Floor(pos))
        Dim hi = CInt(Math.Ceiling(pos))
        If lo = hi Then Return sorted(lo)
        Return sorted(lo) + (sorted(hi) - sorted(lo)) * (pos - lo)
    End Function
End Class
