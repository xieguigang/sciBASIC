#Region "Microsoft.VisualBasic::19045b127682121f427f99e91e43e8bf, Data_science\Visualization\Plots\g\Theme\Theme.vb"

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

    '   Total Lines: 203
    '    Code Lines: 76 (37.44%)
    ' Comment Lines: 96 (47.29%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 31 (15.27%)
    '     File Size: 7.07 KB


    '     Class Theme
    ' 
    '         Properties: axisLabelCSS, axisStroke, axisTickCSS, axisTickPadding, axisTickStroke
    '                     background, colorSet, drawAxis, drawGrid, drawLabels
    '                     drawLegend, flipAxis, gridFill, gridStrokeX, gridStrokeY
    '                     htmlLabel, legendBoxBackground, legendBoxStroke, legendLabelCSS, legendLayout
    '                     legendSplitSize, legendTickAxisStroke, legendTickCSS, legendTickFormat, legendTitleCSS
    '                     lineStroke, mainCSS, mainTextColor, mainTextWrap, padding
    '                     pointSize, subtitleCSS, tagColor, tagCSS, tagFormat
    '                     tagLinkStroke, xAxisLayout, xAxisReverse, xAxisRotate, XaxisTickFormat
    '                     yAxislabelPosition, yAxisLayout, YaxisTickFormat
    ' 
    '         Function: Clone, GetLegendPosition, NewColorSet, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Graphic.Canvas

    ''' <summary>
    ''' The plot style theme definition 
    ''' </summary>
    Public Class Theme : Implements ICloneable

        ''' <summary>
        ''' 背景色
        ''' </summary>
        ''' <returns></returns>
        Public Property background As String = "white"
        ''' <summary>
        ''' 绘图区域的位置与布局
        ''' </summary>
        ''' <returns></returns>
        Public Property padding As String = g.DefaultPadding
        ''' <summary>
        ''' 大标题字体样式
        ''' </summary>
        ''' <returns></returns>
        Public Property mainCSS As String = CSSFont.PlotTitle
        Public Property mainTextColor As String = "black"
        Public Property mainTextWrap As Boolean = False

        ''' <summary>
        ''' 副标题字体样式
        ''' </summary>
        ''' <returns></returns>
        Public Property subtitleCSS As String = CSSFont.PlotSubTitle

#Region "legend styles"

        ''' <summary>
        ''' 图例标题字体样式
        ''' </summary>
        ''' <returns></returns>
        Public Property legendTitleCSS As String = CSSFont.Win7LargerBold

        ''' <summary>
        ''' 这个是为绘制colorMap的图例的标尺准备的
        ''' </summary>
        ''' <returns></returns>
        Public Property legendTickCSS As String = CSSFont.Win7Normal
        Public Property legendTickAxisStroke As String = Stroke.AxisStroke
        Public Property legendTickFormat As String = "F2"

        ''' <summary>
        ''' 图例标签字体样式
        ''' </summary>
        ''' <returns></returns>
        Public Property legendLabelCSS As String = CSSFont.Win7LargerNormal
        ''' <summary>
        ''' divided the legend elements into multiple block group by this element number value.
        ''' </summary>
        ''' <returns></returns>
        Public Property legendSplitSize As Integer = 16
        ''' <summary>
        ''' 图例的布局位置
        ''' </summary>
        ''' <returns></returns>
        Public Property legendLayout As Layout
        ''' <summary>
        ''' 图例的边框线条样式
        ''' </summary>
        ''' <returns></returns>
        Public Property legendBoxStroke As String = Stroke.AxisStroke
        Public Property legendBoxBackground As String = "transparent"

#End Region

#Region "data point styles"

        ''' <summary>
        ''' 在图表之中的某一个数据点的显示字体样式
        ''' </summary>
        ''' <returns></returns>
        Public Property tagCSS As String = CSSFont.PlotLabelNormal
        Public Property tagColor As String = "black"
        ''' <summary>
        ''' 在图表上的数字标签的格式
        ''' </summary>
        ''' <returns></returns>
        Public Property tagFormat As String = "F2"
        ''' <summary>
        ''' 数据点与数据标签之间的连接线的样式
        ''' </summary>
        ''' <returns></returns>
        Public Property tagLinkStroke As String = Stroke.HighlightStroke

        ''' <summary>
        ''' 数据点的大小值
        ''' </summary>
        ''' <returns></returns>
        Public Property pointSize As Integer = 5

#End Region

#Region "axis styles"

        ''' <summary>
        ''' X坐标轴的布局
        ''' </summary>
        ''' <returns></returns>
        Public Property xAxisLayout As XAxisLayoutStyles = XAxisLayoutStyles.Bottom

        Public Property xAxisRotate As Double = 0
        Public Property xAxisReverse As Boolean = False

        ''' <summary>
        ''' Swapping X- and Y-Axes?
        ''' </summary>
        ''' <returns></returns>
        Public Property flipAxis As Boolean = False

        ''' <summary>
        ''' Y坐标轴的布局
        ''' </summary>
        ''' <returns></returns>
        Public Property yAxisLayout As YAxisLayoutStyles = YAxisLayoutStyles.Left
        Public Property yAxislabelPosition As YlabelPosition = YlabelPosition.LeftCenter

        ''' <summary>
        ''' 坐标轴上的标签的字体样式
        ''' </summary>
        ''' <returns></returns>
        Public Property axisLabelCSS As String = CSSFont.Win7LargeBold
        ''' <summary>
        ''' 坐标轴上的标尺的字体样式
        ''' </summary>
        ''' <returns></returns>
        Public Property axisTickCSS As String = CSSFont.PlotLabelNormal
        Public Property axisTickStroke As String = Stroke.ScatterLineStroke
        Public Property axisTickPadding As Double = 5
        Public Property axisStroke As String = Stroke.AxisStroke

        ''' <summary>
        ''' 一般为F2或者G3
        ''' </summary>
        ''' <returns></returns>
        Public Property XaxisTickFormat As String = "F2"
        Public Property YaxisTickFormat As String = "F2"

#End Region

#Region "options"

        Public Property drawAxis As Boolean = True
        ''' <summary>
        ''' 是否显示图例
        ''' </summary>
        ''' <returns></returns>
        Public Property drawLegend As Boolean = True
        Public Property drawLabels As Boolean = False
        ''' <summary>
        ''' 是否再作图区显示网格？
        ''' </summary>
        ''' <returns></returns>
        Public Property drawGrid As Boolean = True

#End Region

        ''' <summary>
        ''' 绘制的线条的样式
        ''' </summary>
        ''' <returns></returns>
        Public Property lineStroke As String = Stroke.AxisStroke

        Public Property gridStrokeX As String = Stroke.AxisGridStroke
        Public Property gridStrokeY As String = Stroke.AxisGridStroke
        ''' <summary>
        ''' the background of the charting plot region
        ''' </summary>
        ''' <returns></returns>
        Public Property gridFill As String = "white"

        Public Property htmlLabel As Boolean = False
        Public Property colorSet As String = "Set1:c9"

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Function NewColorSet(colorSet As Color()) As Theme
            Dim seq As String = colorSet.Select(Function(c) c.ToHtmlColor).JoinBy(",")
            Dim theme As Theme = Me.GetJson.LoadJSON(Of Theme)
            theme.colorSet = seq
            Return theme
        End Function

        Public Function GetLegendPosition(canvas As GraphicsRegion, dependency As LayoutDependency) As PointF
            Return legendLayout.GetLocation(canvas, dependency)
        End Function

        Public Function Clone() As Object Implements ICloneable.Clone
            Return Me.GetJson.LoadJSON(Of Theme)
        End Function
    End Class
End Namespace
