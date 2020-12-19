#Region "Microsoft.VisualBasic::0a1c0e310973767bd872994f4199bf8d, Data_science\Visualization\Plots\g\Theme\Theme.vb"

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

'     Class Theme
' 
'         Properties: axisLabelCSS, axisStroke, axisTickCSS, axisTickFormat, axisTickPadding
'                     axisTickStroke, background, colorSet, drawGrid, drawLabels
'                     drawLegend, gridStroke, legendBoxStroke, legendLabelCSS, legendLayout
'                     legendTitleCSS, mainCSS, padding, pointSize, subtitleCSS
'                     tagColor, tagCSS, xAxisLayout, xlabel, yAxisLayout
'                     ylabel, zlabel
' 
'         Function: ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Graphic.Canvas

    Public Class Theme

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
        ''' <summary>
        ''' 副标题字体样式
        ''' </summary>
        ''' <returns></returns>
        Public Property subtitleCSS As String = CSSFont.PlotSubTitle
        ''' <summary>
        ''' 图例标题字体样式
        ''' </summary>
        ''' <returns></returns>
        Public Property legendTitleCSS As String = CSSFont.Win7LargerBold
        ''' <summary>
        ''' 图例标签字体样式
        ''' </summary>
        ''' <returns></returns>
        Public Property legendLabelCSS As String = CSSFont.Win7LargerNormal
        Public Property legendSplitSize As Integer
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

        ''' <summary>
        ''' 在图表之中的某一个数据点的显示字体样式
        ''' </summary>
        ''' <returns></returns>
        Public Property tagCSS As String = CSSFont.PlotLabelNormal
        Public Property tagColor As String = "black"

        ''' <summary>
        ''' 数据点的大小值
        ''' </summary>
        ''' <returns></returns>
        Public Property pointSize As Integer = 5

        ''' <summary>
        ''' X坐标轴的布局
        ''' </summary>
        ''' <returns></returns>
        Public Property xAxisLayout As XAxisLayoutStyles = XAxisLayoutStyles.Bottom
        ''' <summary>
        ''' Y坐标轴的布局
        ''' </summary>
        ''' <returns></returns>
        Public Property yAxisLayout As YAxisLayoutStyles = YAxisLayoutStyles.Left

        Public Property xlabel As String = "X"
        Public Property ylabel As String = "Y"
        Public Property zlabel As String = "Z"

        Public Property drawAxis As Boolean = True

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
        Public Property axisTickFormat As String = "F2"

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
        Public Property gridStrokeX As String = Stroke.AxisGridStroke
        Public Property gridStrokeY As String = Stroke.AxisGridStroke
        Public Property gridFill As String = "white"

        Public Property htmlLabel As Boolean = False
        Public Property colorSet As String = "Set1:c9"

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Function GetLegendPosition(canvas As GraphicsRegion, dependency As LayoutDependency) As PointF
            Return legendLayout.GetLocation(canvas, dependency)
        End Function

    End Class
End Namespace
