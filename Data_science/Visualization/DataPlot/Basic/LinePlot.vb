#Region "Microsoft.VisualBasic::2fb51ff8ed72ff3c45ac80f26ce1c5f2, Data_science\Visualization\DataPlot\Basic\LinePlot.vb"

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

    '   Total Lines: 74
    '    Code Lines: 62 (83.78%)
    ' Comment Lines: 3 (4.05%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 9 (12.16%)
    '     File Size: 2.79 KB


    ' Class LinePlot
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Sub: Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Imports Microsoft.VisualBasic.Imaging

''' <summary>折线图（默认无标记，可单独配置）</summary>
Public Class LinePlot
    Inherits SeriesPlotEngine

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    ''' <summary>直接在已有的位图上绘制（用于宿主程序 PictureBox 等）。</summary>
    Public Sub New(bmp As Microsoft.VisualBasic.Imaging.Bitmap)
        MyBase.New(bmp)
    End Sub

    ''' <summary>
    ''' 直接在外部绘图设备上绘制 —— 例如 DirectX 的 GPU 画布
    ''' （<c>Microsoft.VisualBasic.Drawing.DirectX.DxGraphics</c>）。
    ''' </summary>
    ''' <param name="g">
    ''' 目标绘图设备。<c>DxCanvas</c> 控件的 <c>Graphics</c> 属性、
    ''' 或者它的 <c>Render</c> 事件参数 <c>e.Graphics</c> 就是可直接传入的实例 ——
    ''' 也就是说曲线图会<b>直接画在控件上</b>，不需要中间位图。
    ''' </param>
    ''' <param name="theme">主题（省略时用浅色主题）</param>
    ''' <remarks>
    ''' 用这种方式构造时，画布所有权仍归调用方（<see cref="PlotEngine.Dispose"/> 不会释放它），
    ''' 因此可以放心写成 <c>Using plot ... End Using</c>。
    ''' 
    ''' 注意画布尺寸取自 <c>g.Size</c>：控件尺寸变化后要重新构造本对象，否则排版仍按旧尺寸。
    ''' </remarks>
    Public Sub New(g As Microsoft.VisualBasic.Imaging.IGraphics,
                   Optional theme As PlotTheme = Nothing)
        MyBase.New(g, theme)
    End Sub

    Public Overrides Sub Plot(seriesList As IList(Of Series))
        ' 折线图默认不显示标记
        For Each s In seriesList
            If s.MarkerShape = MarkerShape.Circle Then s.MarkerShape = MarkerShape.None
        Next
        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

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

        For i = 0 To seriesList.Count - 1
            Dim s = seriesList(i)
            If Not s.Visible Then Continue For
            Dim color = If(s.Color, Theme.Palette(i Mod Theme.Palette.Length))
            Dim pts = New List(Of PointF)()
            For j = 0 To s.X.Length - 1
                pts.Add(New PointF(ToPixelX(s.X(j), xmin, xmax),
                                   ToPixelY(s.Y(j), ymin, ymax)))
            Next
            If pts.Count > 1 Then
                Using pen As New Pen(color, Theme.LineWidth)
                    pen.DashStyle = s.LineStyle
                    pen.StartCap = LineCap.Round
                    pen.EndCap = LineCap.Round
                    pen.LineJoin = LineJoin.Round
                    _g.DrawLines(pen, pts.ToArray())
                End Using
            End If
            If s.MarkerShape <> MarkerShape.None Then
                For Each p In pts
                    DrawMarker(p.X, p.Y, s.MarkerShape, Theme.MarkerSize, color)
                Next
            End If
        Next

        DrawLegend(seriesList)
    End Sub
End Class
