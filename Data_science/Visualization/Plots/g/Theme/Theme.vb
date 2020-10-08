#Region "Microsoft.VisualBasic::58077c6b20a05471b6aa4adc11dc1904, Data_science\Visualization\Plots\g\Theme\Theme.vb"

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
'         Properties: axisLabelCSS, background, drawGrid, drawLegend, legendBoxStroke
'                     legendLabelCSS, legendLayout, legendTitleCSS, mainCSS, padding
'                     subtitleCSS, tagCSS, xAxisLayout, yAxisLayout
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Graphic.Canvas

    Public Class Theme

        ''' <summary>
        ''' 背景色
        ''' </summary>
        ''' <returns></returns>
        Public Property background As String
        ''' <summary>
        ''' 绘图区域的位置与布局
        ''' </summary>
        ''' <returns></returns>
        Public Property padding As String
        ''' <summary>
        ''' 大标题字体样式
        ''' </summary>
        ''' <returns></returns>
        Public Property mainCSS As String
        ''' <summary>
        ''' 副标题字体样式
        ''' </summary>
        ''' <returns></returns>
        Public Property subtitleCSS As String
        ''' <summary>
        ''' 图例标题字体样式
        ''' </summary>
        ''' <returns></returns>
        Public Property legendTitleCSS As String
        ''' <summary>
        ''' 图例标签字体样式
        ''' </summary>
        ''' <returns></returns>
        Public Property legendLabelCSS As String
        ''' <summary>
        ''' 图例的布局位置
        ''' </summary>
        ''' <returns></returns>
        Public Property legendLayout As Layout
        ''' <summary>
        ''' 图例的边框线条样式
        ''' </summary>
        ''' <returns></returns>
        Public Property legendBoxStroke As String
        ''' <summary>
        ''' 在图表之中的某一个数据点的显示字体样式
        ''' </summary>
        ''' <returns></returns>
        Public Property tagCSS As String
        ''' <summary>
        ''' X坐标轴的布局
        ''' </summary>
        ''' <returns></returns>
        Public Property xAxisLayout As XAxisLayoutStyles
        ''' <summary>
        ''' Y坐标轴的布局
        ''' </summary>
        ''' <returns></returns>
        Public Property yAxisLayout As YAxisLayoutStyles
        ''' <summary>
        ''' 坐标轴上的标签的字体样式
        ''' </summary>
        ''' <returns></returns>
        Public Property axisLabelCSS As String
        ''' <summary>
        ''' 是否显示图例
        ''' </summary>
        ''' <returns></returns>
        Public Property drawLegend As Boolean
        ''' <summary>
        ''' 是否再作图区显示网格？
        ''' </summary>
        ''' <returns></returns>
        Public Property drawGrid As Boolean

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Class
End Namespace
