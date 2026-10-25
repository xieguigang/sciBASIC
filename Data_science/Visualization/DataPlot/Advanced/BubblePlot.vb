#Region "Microsoft.VisualBasic::04e834e9134aef1fc1778844fba2861a, Data_science\Visualization\DataPlot\Advanced\BubblePlot.vb"

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

    '   Total Lines: 134
    '    Code Lines: 102 (76.12%)
    ' Comment Lines: 16 (11.94%)
    '    - Xml Docs: 68.75%
    ' 
    '   Blank Lines: 16 (11.94%)
    '     File Size: 5.91 KB


    ' Class BubbleSeries
    ' 
    '     Properties: Sizes
    ' 
    ' Class BubblePlot
    ' 
    '     Properties: FillAlpha, MaxBubbleSize, MinBubbleSize, ShowSizeLegend, SizeLegendCount
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: SizeToRadius
    ' 
    '     Sub: DrawSizeLegend, Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports std = System.Math

''' <summary>气泡图数据系列（X/Y 定位，Sizes 映射第三维）</summary>
Public Class BubbleSeries
    Inherits Series
    ''' <summary>气泡大小（第三维数据）</summary>
    Public Property Sizes As Double() = {}
End Class

''' <summary>气泡图（三维散点，气泡半径映射第三维）</summary>
Public Class BubblePlot
    Inherits PlotEngine

    ''' <summary>最小气泡半径（像素）</summary>
    Public Property MinBubbleSize As Single = 4.0F
    ''' <summary>最大气泡半径（像素）</summary>
    Public Property MaxBubbleSize As Single = 30.0F
    ''' <summary>填充透明度（0-255）</summary>
    Public Property FillAlpha As Integer = 140
    ''' <summary>显示气泡大小图例</summary>
    Public Property ShowSizeLegend As Boolean = True
    ''' <summary>大小图例示例值个数</summary>
    Public Property SizeLegendCount As Integer = 3

    ''' <summary>
    ''' 当前正在绘制的气泡数据系列，由 <see cref="Plot(IList(Of BubbleSeries))"/> 与带画布尺寸的导出重载写入。
    ''' </summary>
    ''' <returns></returns>
    Protected Property SeriesList As IList(Of BubbleSeries) = Nothing

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    ''' <summary>直接在外部提供的绘图设备上绘制（图层叠加模式 / 宿主画布）。</summary>
    Public Sub New(g As IGraphics, Optional theme As PlotTheme = Nothing)
        MyBase.New(g, theme)
    End Sub

    ''' <summary>把给定的气泡系列绘制到当前画布之上</summary>
    ''' <param name="seriesList">气泡数据系列</param>
    Public Overloads Sub Plot(seriesList As IList(Of BubbleSeries))
        Me.SeriesList = seriesList
        Call Plot()
    End Sub

    ''' <summary>一次性渲染并按 <paramref name="driver"/> 指定的驱动导出绘图数据</summary>
    ''' <param name="seriesList">气泡数据系列</param>
    ''' <param name="size">画布尺寸</param>
    ''' <param name="dpi">分辨率</param>
    ''' <param name="driver">绘图驱动，决定输出格式（png/svg/pdf/ps）</param>
    ''' <returns>可直接 <c>Save(path)</c> 落盘的绘图数据对象</returns>
    Public Overloads Function Plot(seriesList As IList(Of BubbleSeries), size As Size,
                                   Optional dpi As Integer = 300,
                                   Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default) As GraphicsData
        Me.SeriesList = seriesList
        Return Plot(size, dpi, driver)
    End Function

    ''' <summary>一次性渲染并按 <paramref name="driver"/> 指定的驱动导出绘图数据</summary>
    ''' <param name="seriesList">气泡数据系列</param>
    ''' <param name="size">画布尺寸，``width,height``格式的字符串</param>
    ''' <param name="ppi">分辨率</param>
    ''' <param name="driver">绘图驱动，决定输出格式（png/svg/pdf/ps）</param>
    ''' <returns>可直接 <c>Save(path)</c> 落盘的绘图数据对象</returns>
    Public Overloads Function Plot(seriesList As IList(Of BubbleSeries), size As String,
                                   Optional ppi As Integer = 300,
                                   Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default) As GraphicsData
        Me.SeriesList = seriesList
        Return Plot(size, ppi, driver)
    End Function

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        ' ---- 坐标范围 ----
        Dim allX = SeriesList.SelectMany(Function(s) s.X).ToArray()
        Dim allY = SeriesList.SelectMany(Function(s) s.Y).ToArray()
        Dim allS = SeriesList.SelectMany(Function(s) s.Sizes).ToArray()

        Dim xmin = If(Me.XMin, allX.Min())
        Dim xmax = If(Me.XMax, allX.Max())
        Dim ymin = If(Me.YMin, allY.Min())
        Dim ymax = If(Me.YMax, allY.Max())
        If Me.XMin Is Nothing AndAlso Me.XMax Is Nothing Then
            Dim pad = (xmax - xmin) * 0.08
            If pad = 0 Then pad = 1
            xmin -= pad : xmax += pad
        End If
        If Me.YMin Is Nothing AndAlso Me.YMax Is Nothing Then
            Dim pad = (ymax - ymin) * 0.08
            If pad = 0 Then pad = 1
            ymin -= pad : ymax += pad
        End If

        Dim sMin = If(allS.Length > 0, allS.Min(), 0)
        Dim sMax = If(allS.Length > 0, allS.Max(), 1)
        If sMax <= sMin Then sMax = sMin + 1

        DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        ' ---- 绘制气泡 ----
        For i = 0 To SeriesList.Count - 1
            Dim s = SeriesList(i)
            If Not s.Visible Then Continue For
            Dim color = If(s.Color, Theme.Palette(i Mod Theme.Palette.Length))
            For j = 0 To s.X.Length - 1
                Dim px = ToPixelX(s.X(j), xmin, xmax)
                Dim py = ToPixelY(s.Y(j), ymin, ymax)
                Dim radius = SizeToRadius(s.Sizes(j), sMin, sMax)
                Using br As New SolidBrush(Color.FromArgb(FillAlpha, color))
                    _g.FillEllipse(br, px - radius, py - radius, radius * 2, radius * 2)
                End Using
                Using pen As New Pen(color, Theme.LineWidth * 0.7F)
                    _g.DrawEllipse(pen, px - radius, py - radius, radius * 2, radius * 2)
                End Using
            Next
        Next

        ' ---- 颜色图例 ----
        DrawLegend(SeriesList.Cast(Of Series)().ToList())

        ' ---- 大小图例 ----
        If ShowSizeLegend AndAlso allS.Length > 0 Then
            DrawSizeLegend(sMin, sMax)
        End If
    End Sub

    ''' <summary>数值映射为气泡半径</summary>
    Private Function SizeToRadius(v As Double, sMin As Double, sMax As Double) As Single
        Dim t = (v - sMin) / (sMax - sMin)
        t = std.Max(0, std.Min(1, t))
        ' 平方根映射，使面积（而非半径）与数值成比例
        Return CSng(MinBubbleSize + (MaxBubbleSize - MinBubbleSize) * std.Sqrt(t))
    End Function

    ''' <summary>右下角绘制大小图例（同心圆示例）</summary>
    Private Sub DrawSizeLegend(sMin As Double, sMax As Double)
        Dim lx = _plotArea.Right - MaxBubbleSize - 10
        Dim lyBase = _plotArea.Bottom - 6
        Using br As New SolidBrush(Theme.LegendBackgroundColor),
              pen As New Pen(Theme.LegendBorderColor, 0.7F)
            Dim legendW = MaxBubbleSize * 2 + 70
            Dim legendH = MaxBubbleSize * 2 + 24
            _g.FillRectangle(br, lx - 6, lyBase - legendH + 6, legendW, legendH)
            _g.DrawRectangle(pen, lx - 6, lyBase - legendH + 6, legendW, legendH)
        End Using

        For k = 0 To SizeLegendCount - 1
            Dim t = If(SizeLegendCount = 1, 1.0, k / (SizeLegendCount - 1))
            Dim v = sMin + t * (sMax - sMin)
            Dim r = SizeToRadius(v, sMin, sMax)
            Dim cy = lyBase - r
            Using br As New SolidBrush(Color.FromArgb(FillAlpha, Theme.TextColor)),
                  pen As New Pen(Theme.TextColor, 0.7F)
                _g.FillEllipse(br, lx + MaxBubbleSize - r, cy, r * 2, r * 2)
                _g.DrawEllipse(pen, lx + MaxBubbleSize - r, cy, r * 2, r * 2)
            End Using
            Using br As New SolidBrush(Theme.TextColor),
                  sf As New StringFormat()
                sf.Alignment = StringAlignment.Near
                sf.LineAlignment = StringAlignment.Center
                _g.DrawString(FormatNumber(v), Theme.TickLabelFont, br,
                              lx + MaxBubbleSize + MaxBubbleSize + 4, cy + r)
            End Using
        Next
    End Sub
End Class
