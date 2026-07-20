#Region "Microsoft.VisualBasic::b3645d827e3840aa439d3b4bc559f183, gr\network-visualization\Visualizer\NetworkRenderConfig.vb"

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

    '   Total Lines: 83
    '    Code Lines: 50 (60.24%)
    ' Comment Lines: 23 (27.71%)
    '    - Xml Docs: 65.22%
    ' 
    '   Blank Lines: 10 (12.05%)
    '     File Size: 4.16 KB


    ' Class NetworkRenderConfig
    ' 
    '     Properties: Background, CanvasSize, ConvexHullCurveDegree, ConvexHullLabelFontCSS, ConvexHullScale
    '                 DefaultColor, DefaultEdgeColor, DefaultLabelColor, DisplayId, DrawEdgeBends
    '                 DrawEdgeDirection, DrawNodeShape, Driver, EdgeDashTypes, EdgeShadowDistance
    '                 FillConvexHullPolygon, FontSize, GetLabelColor, GetLabelPosition, GetNodeLabel
    '                 HideDisconnectedNode, HullPolygonGroups, LabelColorAsNodeColor, LabelerIterations, LabelFontBase
    '                 LabelTextStroke, LabelWordWrapWidth, LinkWidth, MinLinkWidth, NodeRadius
    '                 NodeStroke, NodeWidget, Padding, Ppi, ShapeRender
    '                 ShowConvexHullLegend, ShowLabelerProgress, ThrowEx
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Html.CSS

''' <summary>
''' 网络图绘制的所有可配置参数的统一承载对象。
''' 
''' 该配置类的每一个字段都严格对应 <see cref="NetworkVisualizer.DrawImage"/> 
''' 原有的 Optional 参数，并且默认值与原来的 Optional 默认值保持一致，
''' 以确保重构之后不会产生任何行为上的漂移。
''' </summary>
''' <remarks>
''' 将原先“每一次调用 DrawImage 都需要传递大量参数”的函数调用模式，
''' 收敛为一个模块化的配置 class 对象，配合 <see cref="NetworkPlot"/> 
''' 渲染类一起使用，即可完成网络图的可视化绘制。
''' </remarks>
Public Class NetworkRenderConfig

    ''' <summary>
    ''' 这个背景颜色会被用于节点描边的默认样式，来源于
    ''' https://github.com/whichlight/reddit-network-vis
    ''' </summary>
    Friend Const WhiteStroke$ = "stroke: white; stroke-width: 2px; stroke-dash: solid;"

    ' 画布与布局相关的参数
    Public Property CanvasSize As String = "1024,1024"
    Public Property Padding As String = g.DefaultPadding
    Public Property Background As String = "white"
    Public Property Ppi As Integer = 100
    Public Property Driver As Drivers = Drivers.Default

    ' 节点颜色与形状相关的参数
    Public Property DefaultColor As String = "skyblue"
    Public Property DisplayId As Boolean = True
    Public Property LabelColorAsNodeColor As Boolean = False
    Public Property NodeStroke As String = WhiteStroke
    Public Property NodeRadius As [Variant](Of Func(Of Node, Single), Single) = Nothing
    Public Property FontSize As [Variant](Of Func(Of Node, Single), Single) = Nothing
    Public Property LabelFontBase As String = CSSFont.Win7Normal
    Public Property LabelWordWrapWidth As Integer = -1
    Public Property HideDisconnectedNode As Boolean = False

    ' 节点标签自定义委托
    Public Property DrawNodeShape As NetworkVisualizer.DrawNodeShape = Nothing
    Public Property NodeWidget As Func(Of IGraphics, PointF, Double, Node, RectangleF) = Nothing
    Public Property ShapeRender As NetworkVisualizer.DrawShape = Nothing
    Public Property GetNodeLabel As Func(Of Node, String) = Nothing
    Public Property GetLabelPosition As NetworkVisualizer.GetLabelPosition = Nothing
    Public Property GetLabelColor As Func(Of Node, Color) = Nothing

    ' 边相关的参数
    Public Property MinLinkWidth As Single = 2
    Public Property LinkWidth As Func(Of Edge, Single) = Nothing
    Public Property EdgeDashTypes As [Variant](Of Dictionary(Of String, DashStyle), DashStyle) = Nothing
    Public Property EdgeShadowDistance As Single = 0
    Public Property DrawEdgeBends As Boolean = True
    Public Property DrawEdgeDirection As Boolean = False
    Public Property DefaultEdgeColor As String = NameOf(Color.LightGray)

    ' 节点标签布局（退火算法）相关的参数
    Public Property LabelerIterations As Integer = 1500
    Public Property ShowLabelerProgress As Boolean = True
    Public Property DefaultLabelColor As String = "black"
    Public Property LabelTextStroke As String = "stroke: lightgray; stroke-width: 1px; stroke-dash: solid;"

    ' 凸包多边形（分组）相关的参数
    Public Property HullPolygonGroups As NamedValue(Of String) = Nothing
    Public Property ShowConvexHullLegend As Boolean = True
    Public Property ConvexHullLabelFontCSS As String = CSSFont.Win7VeryLarge
    Public Property ConvexHullScale As Single = 1.0125
    Public Property ConvexHullCurveDegree As Single = 2
    Public Property FillConvexHullPolygon As Boolean = True

    ' 异常处理开关
    Public Property ThrowEx As Boolean = True

End Class

