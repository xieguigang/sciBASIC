#Region "Microsoft.VisualBasic::33fb17a6646b7fd50d4d06f503949227, Data_science\Visualization\DataPlot\Basic\ScatterPlot.vb"

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

    '   Total Lines: 79
    '    Code Lines: 61 (77.22%)
    ' Comment Lines: 9 (11.39%)
    '    - Xml Docs: 22.22%
    ' 
    '   Blank Lines: 9 (11.39%)
    '     File Size: 3.08 KB


    ' Class ScatterPlot
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Sub: Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports LineCap = Microsoft.VisualBasic.Imaging.LineCap
Imports Pen = Microsoft.VisualBasic.Imaging.Pen

' ============================================================================
'  ChartsBasic.vb - 基础图表：散点图 / 折线图 / 柱状图 / 直方图
' ============================================================================

''' <summary>散点图</summary>
Public Class ScatterPlot
    Inherits SeriesPlotEngine

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    ''' <summary>直接在已有的位图上绘制（用于宿主程序 PictureBox 等）。</summary>
    Public Sub New(bmp As Microsoft.VisualBasic.Imaging.Bitmap)
        MyBase.New(bmp)
    End Sub

    Public Overrides Sub Plot(seriesList As IList(Of Series))
        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        ' 计算范围
        Dim allX = seriesList.SelectMany(Function(s) s.X).ToArray()
        Dim allY = seriesList.SelectMany(Function(s) s.Y).ToArray()
        Dim xmin = If(Me.XMin, allX.Min())
        Dim xmax = If(Me.XMax, allX.Max())
        Dim ymin = If(Me.YMin, allY.Min())
        Dim ymax = If(Me.YMax, allY.Max())
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

        ' 绘制每个系列
        For i = 0 To seriesList.Count - 1
            Dim s = seriesList(i)
            If Not s.Visible Then Continue For
            Dim color = If(s.Color, Theme.Palette(i Mod Theme.Palette.Length))
            Dim pts = New List(Of PointF)()
            For j = 0 To s.X.Length - 1
                pts.Add(New PointF(ToPixelX(s.X(j), xmin, xmax),
                                   ToPixelY(s.Y(j), ymin, ymax)))
            Next
            ' 连线
            If s.LineStyle <> DashStyle.Custom AndAlso pts.Count > 1 Then
                Using pen As New Pen(color, Theme.LineWidth)
                    pen.DashStyle = s.LineStyle
                    pen.StartCap = LineCap.Round
                    pen.EndCap = LineCap.Round
                    _g.DrawLines(pen, pts.ToArray())
                End Using
            End If
            ' 标记
            If s.MarkerShape <> MarkerShape.None Then
                For Each p In pts
                    DrawMarker(p.X, p.Y, s.MarkerShape, Theme.MarkerSize, color)
                Next
            End If
        Next

        DrawLegend(seriesList)
    End Sub
End Class
