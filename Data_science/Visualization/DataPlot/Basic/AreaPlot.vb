#Region "Microsoft.VisualBasic::c77b8fff0bcbcfcbdbbee21c0079a2ff, Data_science\Visualization\DataPlot\Basic\AreaPlot.vb"

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

    '   Total Lines: 111
    '    Code Lines: 88 (79.28%)
    ' Comment Lines: 10 (9.01%)
    '    - Xml Docs: 60.00%
    ' 
    '   Blank Lines: 13 (11.71%)
    '     File Size: 4.54 KB


    ' Class AreaPlot
    ' 
    '     Properties: Baseline, FillAlpha, Smooth, SmoothSegments
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: CatmullRom
    ' 
    '     Sub: Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

''' <summary>基础面积图（曲线下面积填充）</summary>
Public Class AreaPlot
    Inherits PlotEngine

    ''' <summary>是否启用 Catmull-Rom 平滑曲线</summary>
    Public Property Smooth As Boolean = True
    ''' <summary>平滑插值每段采样点数</summary>
    Public Property SmoothSegments As Integer = 20
    ''' <summary>填充透明度（0-255）</summary>
    Public Property FillAlpha As Integer = 120
    ''' <summary>面积基线 Y 值（默认 0，即曲线下面积）</summary>
    Public Property Baseline As Double = 0

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot(seriesList As IList(Of Series))
        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        ' ---- 计算坐标范围 ----
        Dim allX = seriesList.SelectMany(Function(s) s.X).ToArray()
        Dim allY = seriesList.SelectMany(Function(s) s.Y).ToArray()
        Dim xmin = If(Me.XMin, allX.Min())
        Dim xmax = If(Me.XMax, allX.Max())
        Dim yLo = Math.Min(allY.Min(), Baseline)
        Dim yHi = Math.Max(allY.Max(), Baseline)
        Dim ymin = If(Me.YMin, yLo)
        Dim ymax = If(Me.YMax, yHi)
        If Me.XMin Is Nothing AndAlso Me.XMax Is Nothing Then
            Dim pad = (xmax - xmin) * 0.05
            If pad = 0 Then pad = 1
            xmin -= pad : xmax += pad
        End If
        If Me.YMin Is Nothing AndAlso Me.YMax Is Nothing Then
            Dim pad = (ymax - ymin) * 0.08
            If pad = 0 Then pad = 1
            ymin -= pad : ymax += pad
        End If

        DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        ' ---- 逐系列绘制面积 ----
        For i = 0 To seriesList.Count - 1
            Dim s = seriesList(i)
            If Not s.Visible Then Continue For
            Dim color = If(s.Color, Theme.Palette(i Mod Theme.Palette.Length))

            Dim pts = New List(Of PointF)()
            For j = 0 To s.X.Length - 1
                pts.Add(New PointF(ToPixelX(s.X(j), xmin, xmax),
                                   ToPixelY(s.Y(j), ymin, ymax)))
            Next
            If pts.Count = 0 Then Continue For

            ' 平滑插值
            Dim drawPts = If(Smooth AndAlso pts.Count >= 3,
                             CatmullRom(pts, SmoothSegments), pts)

            ' 构造面积多边形：线上点 + 基线点（反向闭合）
            Dim baselinePx = ToPixelY(Baseline, ymin, ymax)
            Dim poly = New List(Of PointF)(drawPts)
            For k = drawPts.Count - 1 To 0 Step -1
                poly.Add(New PointF(drawPts(k).X, baselinePx))
            Next

            Using br As New SolidBrush(Color.FromArgb(FillAlpha, color))
                _g.FillPolygon(br, poly.ToArray())
            End Using
            If drawPts.Count > 1 Then
                Using pen As New Pen(color, Theme.LineWidth)
                    _g.DrawLines(pen, drawPts.ToArray())
                End Using
            End If
        Next

        DrawLegend(seriesList)
    End Sub

    ''' <summary>Catmull-Rom 样条插值，返回密集采样点列表</summary>
    Private Function CatmullRom(pts As List(Of PointF), segments As Integer) As List(Of PointF)
        Dim result As New List(Of PointF)()
        If pts.Count < 2 Then Return pts
        For i = 0 To pts.Count - 2
            Dim p0 = If(i = 0, pts(0), pts(i - 1))
            Dim p1 = pts(i)
            Dim p2 = pts(i + 1)
            Dim p3 = If(i + 2 < pts.Count, pts(i + 2), pts(i + 1))
            For t = 0 To segments - 1
                Dim s As Double = t / segments
                Dim s2 = s * s
                Dim s3 = s2 * s
                Dim x = 0.5 * ((2 * p1.X) + (-p0.X + p2.X) * s +
                               (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) * s2 +
                               (-p0.X + 3 * p1.X - 3 * p2.X + p3.X) * s3)
                Dim y = 0.5 * ((2 * p1.Y) + (-p0.Y + p2.Y) * s +
                               (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) * s2 +
                               (-p0.Y + 3 * p1.Y - 3 * p2.Y + p3.Y) * s3)
                result.Add(New PointF(CSng(x), CSng(y)))
            Next
        Next
        result.Add(pts(pts.Count - 1))
        Return result
    End Function
End Class

