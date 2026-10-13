#Region "Microsoft.VisualBasic::542810bf3f36fd804771f2e10ef2d46e, Data_science\Visualization\DataPlot\Plot3D\Plot3DTheme.vb"

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

    '   Total Lines: 49
    '    Code Lines: 16 (32.65%)
    ' Comment Lines: 19 (38.78%)
    '    - Xml Docs: 68.42%
    ' 
    '   Blank Lines: 14 (28.57%)
    '     File Size: 1.90 KB


    '     Class Plot3DTheme
    ' 
    '         Properties: axisLabelCSS, axisStroke, background, drawLabels, drawLegend
    '                     legendBoxStroke, mainTextColor, padding, tagColor, tagCSS
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
'  Plot3DTheme.vb - 3D 绘图主题（从旧 Plots g/Graphic.Theme 精简而来）
'
'  只保留 3D 散点图 / 3D 饼图绘制所需要的那一组样式参数，
'  其余的旧 Theme 字段在这个新的 DataPlot 引擎之中不再需要。
' ============================================================================

Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace Plot3D

    ''' <summary>
    ''' 3D 绘图（散点图、饼图）的主题样式参数
    ''' </summary>
    Public Class Plot3DTheme

        ''' <summary>画布页边距 CSS 表达式</summary>
        Public Property padding As String = g.DefaultPadding

        ''' <summary>画布背景色表达式</summary>
        Public Property background As String = "white"

        ''' <summary>坐标轴线画笔 CSS</summary>
        Public Property axisStroke As String = Stroke.AxisStroke

        ''' <summary>坐标轴标签字体 CSS</summary>
        Public Property axisLabelCSS As String = CSSFont.Win7Normal

        ''' <summary>数据点标签字体 CSS</summary>
        Public Property tagCSS As String = CSSFont.Win10Normal

        ''' <summary>数据点标签颜色</summary>
        Public Property tagColor As String = "black"

        ''' <summary>轴标签文本颜色</summary>
        Public Property mainTextColor As String = "black"

        ''' <summary>图例框描边 CSS</summary>
        Public Property legendBoxStroke As String = Stroke.StrongHighlightStroke

        ''' <summary>是否绘制图例</summary>
        Public Property drawLegend As Boolean = True

        ''' <summary>是否绘制数据点文本标签</summary>
        Public Property drawLabels As Boolean = True

    End Class
End Namespace
