#Region "Microsoft.VisualBasic::298f52a4a2bf8a51569212a072b2cc70, Data_science\Visualization\DataPlot\Engine\PlotTheme.vb"

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

    '   Total Lines: 180
    '    Code Lines: 144 (80.00%)
    ' Comment Lines: 20 (11.11%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 16 (8.89%)
    '     File Size: 7.80 KB


    ' Class PlotTheme
    ' 
    '     Properties: AnnotationFont, AntiAlias, AxisColor, AxisLabelFont, AxisLineWidth
    '                 BackgroundColor, BarPadding, BorderColor, GridColor, GridLineWidth
    '                 HighQualityText, LegendBackgroundColor, LegendBorderColor, LegendFont, LineWidth
    '                 MarginBottom, MarginLeft, MarginRight, MarginTop, MarkerSize
    '                 MinorGridColor, Palette, PlotAreaColor, ShowGrid, ShowLegendBorder
    '                 ShowMinorGrid, SubTitleColor, SubTitleFont, TextColor, TickLabelFont
    '                 TitleColor, TitleFont
    ' 
    '     Function: Clone, Dark, Grayscale, Light, Nature
    '               Science
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports FontStyle = Microsoft.VisualBasic.Imaging.FontStyle

' ============================================================================
'  PlotTheme.vb - 主题系统
'  提供预定义主题（Light / Dark / Nature / Science / Custom）与主题定制能力
' ============================================================================

''' <summary>
''' 图表主题：包含颜色、字体、线宽、边距等所有视觉样式参数。
''' 可直接使用预定义主题，也可基于预定义主题派生自定义主题。
''' </summary>
Public Class PlotTheme

    ' ---------- 颜色 ----------
    Public Property BackgroundColor As Color = Color.White
    Public Property PlotAreaColor As Color = Color.White
    Public Property AxisColor As Color = Color.FromArgb(60, 60, 60)
    Public Property GridColor As Color = Color.FromArgb(220, 220, 220)
    Public Property MinorGridColor As Color = Color.FromArgb(240, 240, 240)
    Public Property TextColor As Color = Color.FromArgb(40, 40, 40)
    Public Property TitleColor As Color = Color.FromArgb(20, 20, 20)
    Public Property SubTitleColor As Color = Color.FromArgb(90, 90, 90)
    Public Property LegendBackgroundColor As Color = Color.FromArgb(250, 250, 250)
    Public Property LegendBorderColor As Color = Color.FromArgb(200, 200, 200)
    Public Property BorderColor As Color = Color.FromArgb(180, 180, 180)

    ' ---------- 字体 ----------
    Public Property TitleFont As New Font("Microsoft YaHei", 16, FontStyle.Bold)
    Public Property SubTitleFont As New Font("Microsoft YaHei", 10, FontStyle.Regular)
    Public Property AxisLabelFont As New Font("Microsoft YaHei", 11, FontStyle.Regular)
    Public Property TickLabelFont As New Font("Microsoft YaHei", 9, FontStyle.Regular)
    Public Property LegendFont As New Font("Microsoft YaHei", 9, FontStyle.Regular)
    Public Property AnnotationFont As New Font("Microsoft YaHei", 9, FontStyle.Regular)

    ' ---------- 调色板（用于多系列 / 分类） ----------
    Public Property Palette As Color() = {
        Color.FromArgb(31, 119, 180),
        Color.FromArgb(255, 127, 14),
        Color.FromArgb(44, 160, 44),
        Color.FromArgb(214, 39, 40),
        Color.FromArgb(148, 103, 189),
        Color.FromArgb(140, 86, 75),
        Color.FromArgb(227, 119, 194),
        Color.FromArgb(127, 127, 127),
        Color.FromArgb(188, 189, 34),
        Color.FromArgb(23, 190, 207)
    }

    ' ---------- 尺寸 ----------
    Public Property LineWidth As Single = 1.5F
    Public Property AxisLineWidth As Single = 1.0F
    Public Property GridLineWidth As Single = 0.7F
    Public Property MarkerSize As Single = 6.0F
    Public Property BarPadding As Single = 0.15F

    ' ---------- 边距（像素） ----------
    Public Property MarginLeft As Single = 80
    Public Property MarginRight As Single = 30
    Public Property MarginTop As Single = 70
    Public Property MarginBottom As Single = 70

    ' ---------- 其它开关 ----------
    Public Property ShowGrid As Boolean = True
    Public Property ShowMinorGrid As Boolean = False
    Public Property ShowLegendBorder As Boolean = True
    Public Property AntiAlias As Boolean = True
    Public Property HighQualityText As Boolean = True

    ''' <summary>浅色主题（默认，适合论文白底插图）</summary>
    Public Shared Function Light() As PlotTheme
        Return New PlotTheme()
    End Function

    ''' <summary>深色主题（适合演示 / 海报）</summary>
    Public Shared Function Dark() As PlotTheme
        Dim t As New PlotTheme()
        t.BackgroundColor = Color.FromArgb(30, 30, 30)
        t.PlotAreaColor = Color.FromArgb(40, 40, 40)
        t.AxisColor = Color.FromArgb(200, 200, 200)
        t.GridColor = Color.FromArgb(70, 70, 70)
        t.MinorGridColor = Color.FromArgb(50, 50, 50)
        t.TextColor = Color.FromArgb(230, 230, 230)
        t.TitleColor = Color.FromArgb(245, 245, 245)
        t.SubTitleColor = Color.FromArgb(170, 170, 170)
        t.LegendBackgroundColor = Color.FromArgb(45, 45, 45)
        t.LegendBorderColor = Color.FromArgb(90, 90, 90)
        t.BorderColor = Color.FromArgb(110, 110, 110)
        t.Palette = {
            Color.FromArgb(86, 180, 233),
            Color.FromArgb(230, 159, 0),
            Color.FromArgb(0, 158, 115),
            Color.FromArgb(240, 228, 66),
            Color.FromArgb(0, 114, 178),
            Color.FromArgb(213, 94, 0),
            Color.FromArgb(204, 121, 167)
        }
        Return t
    End Function

    ''' <summary>Nature 期刊风格（清淡、克制）</summary>
    Public Shared Function Nature() As PlotTheme
        Dim t As New PlotTheme()
        t.BackgroundColor = Color.White
        t.PlotAreaColor = Color.White
        t.AxisColor = Color.Black
        t.GridColor = Color.FromArgb(235, 235, 235)
        t.TextColor = Color.Black
        t.TitleColor = Color.Black
        t.TitleFont = New Font("Times New Roman", 14, FontStyle.Bold)
        t.SubTitleFont = New Font("Times New Roman", 9, FontStyle.Italic)
        t.AxisLabelFont = New Font("Times New Roman", 11, FontStyle.Regular)
        t.TickLabelFont = New Font("Times New Roman", 9, FontStyle.Regular)
        t.LegendFont = New Font("Times New Roman", 9, FontStyle.Regular)
        t.Palette = {
            Color.FromArgb(204, 121, 167),
            Color.FromArgb(86, 180, 233),
            Color.FromArgb(0, 158, 115),
            Color.FromArgb(240, 228, 66),
            Color.FromArgb(0, 114, 178),
            Color.FromArgb(213, 94, 0),
            Color.FromArgb(230, 159, 0)
        }
        t.LineWidth = 1.2F
        t.AxisLineWidth = 0.8F
        t.ShowMinorGrid = False
        Return t
    End Function

    ''' <summary>Science 期刊风格（紧凑、专业）</summary>
    Public Shared Function Science() As PlotTheme
        Dim t As New PlotTheme()
        t.BackgroundColor = Color.White
        t.PlotAreaColor = Color.White
        t.AxisColor = Color.FromArgb(50, 50, 50)
        t.GridColor = Color.FromArgb(230, 230, 230)
        t.TextColor = Color.FromArgb(30, 30, 30)
        t.TitleColor = Color.Black
        t.TitleFont = New Font("Arial", 13, FontStyle.Bold)
        t.AxisLabelFont = New Font("Arial", 10, FontStyle.Bold)
        t.TickLabelFont = New Font("Arial", 8, FontStyle.Regular)
        t.LegendFont = New Font("Arial", 8, FontStyle.Regular)
        t.Palette = {
            Color.FromArgb(0, 114, 178),
            Color.FromArgb(213, 94, 0),
            Color.FromArgb(0, 158, 115),
            Color.FromArgb(86, 180, 233),
            Color.FromArgb(204, 121, 167),
            Color.FromArgb(230, 159, 0),
            Color.FromArgb(240, 228, 66)
        }
        t.LineWidth = 1.4F
        t.MarginLeft = 65
        t.MarginBottom = 60
        Return t
    End Function

    ''' <summary>灰色主题（适合黑白印刷）</summary>
    Public Shared Function Grayscale() As PlotTheme
        Dim t As New PlotTheme()
        t.Palette = {
            Color.FromArgb(20, 20, 20),
            Color.FromArgb(80, 80, 80),
            Color.FromArgb(140, 140, 140),
            Color.FromArgb(180, 180, 180),
            Color.FromArgb(50, 50, 50),
            Color.FromArgb(110, 110, 110),
            Color.FromArgb(200, 200, 200)
        }
        Return t
    End Function

    ''' <summary>基于当前主题派生自定义主题（链式调用）</summary>
    Public Function Clone() As PlotTheme
        Return CType(Me.MemberwiseClone(), PlotTheme)
    End Function

End Class


