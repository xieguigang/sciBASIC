#Region "Microsoft.VisualBasic::73e9cba40cc83cc27efa6f9de0688143, Data_science\Visualization\DataPlot\Basic\ScatterPlot.vb"

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

    '   Total Lines: 78
    '    Code Lines: 61 (78.21%)
    ' Comment Lines: 8 (10.26%)
    '    - Xml Docs: 12.50%
    ' 
    '   Blank Lines: 9 (11.54%)
    '     File Size: 3.07 KB


    ' Class ScatterPlot
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports LineCap = Microsoft.VisualBasic.Imaging.LineCap
Imports LineJoin = Microsoft.VisualBasic.Imaging.LineJoin
Imports StringAlignment = Microsoft.VisualBasic.Imaging.StringAlignment
Imports StringFormat = Microsoft.VisualBasic.Imaging.StringFormat

' ============================================================================
'  ChartsBasic.vb - 基础图表：散点图 / 折线图 / 柱状图 / 直方图
' ============================================================================

''' <summary>散点图</summary>
Public Class ScatterPlot
    Inherits PlotEngine

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot(seriesList As IList(Of Series))
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

