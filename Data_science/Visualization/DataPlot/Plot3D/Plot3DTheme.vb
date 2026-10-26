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

        ' ---------------------------------------------------------------
        '  以下为出版级排版新增的样式参数，全部带有默认值：
        '  既有调用方（ggplot 的 ThemeBridge、Kmeans、R# 绑定）在不显式赋值时
        '  仍然得到与旧版本一致的字段语义。
        ' ---------------------------------------------------------------

        ''' <summary>图表标题字体 CSS（为空字符串时使用 <see cref="axisLabelCSS"/> 的加粗版本）</summary>
        Public Property titleCSS As String = CSSFont.Win7LargeBold

        ''' <summary>副标题字体 CSS</summary>
        Public Property subtitleCSS As String = CSSFont.Win7Normal

        ''' <summary>坐标轴的刻度值字体 CSS</summary>
        Public Property tickCSS As String = CSSFont.Win7Normal

        ''' <summary>三维盒框的棱线描边 CSS</summary>
        Public Property boxStroke As String = Stroke.AxisStroke

        ''' <summary>背板网格线的描边 CSS</summary>
        Public Property gridStroke As String = Stroke.AxisGridStroke

        ''' <summary>刻度值的数字格式（默认保留两位有效数字）</summary>
        Public Property tickFormat As String = "G2"

        ''' <summary>是否绘制三维盒框（12 条棱）</summary>
        Public Property showBox As Boolean = True

        ''' <summary>是否绘制背板网格（按相机朝向自动挑选朝向背面的三个面）</summary>
        Public Property showGrid As Boolean = True

        ''' <summary>是否绘制三条主轴上的刻度线与刻度值</summary>
        Public Property showTicks As Boolean = True

        ''' <summary>是否绘制带箭头的主轴线（即旧版本的 <c>AxisDraw.Axis</c> 行为）</summary>
        Public Property showAxisLines As Boolean = True

        ''' <summary>
        ''' 深度淡化强度，取值 [0,1]：0 表示关闭，越大则"离视点越远"的图元越淡。
        ''' 仅对三维散点图生效。
        ''' </summary>
        Public Property depthFade As Double = 0

        ''' <summary>
        ''' 是否尝试开启抗锯齿。底层 <c>IGraphics</c> 没有跨驱动的抗锯齿开关，
        ''' 这里会在 GDI 栅格画布上通过反射设置 GDI+ 的高质量绘制参数，
        ''' 其它驱动（SVG / PostScript）下自动退化为不处理。
        ''' </summary>
        Public Property antiAlias As Boolean = True

        ''' <summary>图表标题（为空则不绘制）</summary>
        Public Property title As String = ""

        ''' <summary>图表副标题（为空则不绘制）</summary>
        Public Property subTitle As String = ""

        ''' <summary>图例项的形状尺寸（"宽,高"，单位像素）</summary>
        Public Property legendShapeSize As String = "12,12"

        ''' <summary>连续色标（colorbar）的宽度与高度（"宽,高"，单位像素）</summary>
        Public Property colorBarSize As String = "16,240"

    End Class
End Namespace
