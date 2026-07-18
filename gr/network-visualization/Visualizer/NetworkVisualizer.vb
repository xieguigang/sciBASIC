#Region "Microsoft.VisualBasic::d84e08954f6cc047539faacbdf53bd68, gr\network-visualization\Visualizer\NetworkVisualizer.vb"

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

    '   Total Lines: 199
    '    Code Lines: 127 (63.82%)
    ' Comment Lines: 60 (30.15%)
    '    - Xml Docs: 86.67%
    ' 
    '   Blank Lines: 12 (6.03%)
    '     File Size: 10.60 KB


    ' Module NetworkVisualizer
    ' 
    '     Properties: BackgroundColor
    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Delegate Sub
    ' 
    '         Function: DirectMapRadius, DrawImage
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConcaveHull
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports std = System.Math

<Assembly: InternalsVisibleTo("ggraph")>

''' <summary>
''' Image drawing of a network model
''' </summary>
<Package("Network.Visualizer", Publisher:="xie.guigang@gmail.com")>
Public Module NetworkVisualizer

    ''' <summary>
    ''' This background color was picked from https://github.com/whichlight/reddit-network-vis
    ''' </summary>
    ''' <returns></returns>
    Public Property BackgroundColor As Color = Color.FromArgb(219, 243, 255)

    Const WhiteStroke$ = "stroke: white; stroke-width: 2px; stroke-dash: solid;"

    Public Delegate Function DrawNodeShape(id As String, g As IGraphics, brush As Brush, radius As Single(), center As PointF) As RectangleF
    Public Delegate Function GetLabelPosition(node As Node, label$, shapeLayout As RectangleF, labelSize As SizeF) As PointF
    Public Delegate Sub DrawShape(g As IGraphics, pos As PointF, gSize As SizeF, shape As String, color As Brush, border As Stroke, radius%, ByRef labelPos As PointF, lineWidth!)

    ''' <summary>
    ''' Rendering png or svg image from a given network graph model.
    ''' (假若属性是空值的话，在绘图之前可以调用<see cref="ApplyAnalysis"/>拓展方法进行一些分析)
    ''' </summary>
    ''' <param name="net"></param>
    ''' <param name="canvasSize">画布的大小</param>
    ''' <param name="padding">上下左右的边距分别为多少？</param>
    ''' <param name="background">背景色或者背景图片的文件路径</param>
    ''' <param name="defaultColor"></param>
    ''' <param name="hullPolygonGroups">
    ''' ```
    ''' [<see cref="NodeData.Properties"/> Name => expression]
    ''' ```
    ''' 
    ''' + expression = max/min largest or smallest group
    ''' + expression = 'a,b,c,d,e' node category to draw hull polygon 
    ''' + expression = top&lt;n> show top n largest group
    ''' 
    ''' (需要显示分组的多边形的分组的名称的列表，也可以是一个表达式max或者min，分别表示最大或者最小的分组)
    ''' </param>
    ''' <param name="nodeRadius">By default all of the node have the same radius size</param>
    ''' <param name="labelFontBase">
    ''' 这个参数会提供字体的一些基础样式,字体的大小会从节点的属性中计算出来
    ''' </param>
    ''' <param name="displayId">
    ''' 是否现在节点的标签文本
    ''' </param>
    ''' <param name="labelerIterations">
    ''' 0表示不进行
    ''' </param>
    ''' <param name="edgeDashTypes">
    ''' 1. ``interaction_type`` property value in <see cref="Edge.data"/>, or
    ''' 2. <see cref="Edge.ID"/> value
    ''' </param>
    ''' <param name="labelTextStroke">
    ''' 当这个参数为空字符串的时候，将不进行描边
    ''' </param>
    ''' <param name="labelWordWrapWidth">
    ''' 每一行文本所限定的字符数量，小于等于零表示不进行自动textwrap
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 一些内置的样式支持:
    ''' 
    ''' + 节点的颜色或者纹理: <see cref="NodeData.color"/>
    ''' + 如果<see cref="EdgeData.bends"/>不是空的话，会按照这个定义的点集合绘制边
    '''   否则会直接在两个节点之间绘制一条直线作为边连接
    ''' </remarks>
    <ExportAPI("Draw.Image")>
    <Extension>
    Public Function DrawImage(net As NetworkGraph,
                              Optional canvasSize$ = "1024,1024",
                              Optional padding$ = g.DefaultPadding,
                              Optional background$ = "white",
                              Optional defaultColor$ = "skyblue",
                              Optional displayId As Boolean = True,
                              Optional labelColorAsNodeColor As Boolean = False,
                              Optional nodeStroke$ = WhiteStroke,
                              Optional minLinkWidth! = 2,
                              Optional linkWidth As Func(Of Edge, Single) = Nothing,
                              Optional nodeRadius As [Variant](Of Func(Of Node, Single), Single) = Nothing,
                              Optional fontSize As [Variant](Of Func(Of Node, Single), Single) = Nothing,
                              Optional labelFontBase$ = CSSFont.Win7Normal,
                              Optional edgeDashTypes As [Variant](Of Dictionary(Of String, DashStyle), DashStyle) = Nothing,
                              Optional edgeShadowDistance As Single = 0,
                              Optional drawNodeShape As DrawNodeShape = Nothing,
                              Optional nodeWidget As Func(Of IGraphics, PointF, Double, Node, RectangleF) = Nothing,
                              Optional shapeRender As DrawShape = Nothing,
                              Optional getNodeLabel As Func(Of Node, String) = Nothing,
                              Optional getLabelPosition As GetLabelPosition = Nothing,
                              Optional getLabelColor As Func(Of Node, Color) = Nothing,
                              Optional hideDisconnectedNode As Boolean = False,
                              Optional throwEx As Boolean = True,
                              Optional hullPolygonGroups As NamedValue(Of String) = Nothing,
                              Optional labelerIterations% = 1500,
                              Optional labelWordWrapWidth% = -1,
                              Optional showLabelerProgress As Boolean = True,
                              Optional defaultEdgeColor$ = NameOf(Color.LightGray),
                              Optional defaultLabelColor$ = "black",
                              Optional labelTextStroke$ = "stroke: lightgray; stroke-width: 1px; stroke-dash: solid;",
                              Optional showConvexHullLegend As Boolean = True,
                              Optional drawEdgeBends As Boolean = True,
                              Optional drawEdgeDirection As Boolean = False,
                              Optional convexHullLabelFontCSS$ = CSSFont.Win7VeryLarge,
                              Optional convexHullScale! = 1.0125,
                              Optional convexHullCurveDegree As Single = 2,
                              Optional fillConvexHullPolygon As Boolean = True,
                              Optional driver As Drivers = Drivers.Default,
                              Optional ppi As Integer = 100) As GraphicsData

        ' 兼容外壳：将原先一长串的 Optional 参数收敛到 NetworkRenderConfig 配置对象，
        ' 然后委托给模块化的 NetworkPlot 渲染类完成实际的绘制工作。
        ' 实际的绘制流程（坐标换算、边/节点/标签/凸包多边形渲染）已经迁移至
        ' NetworkPlot 以及 Render 目录下的各个渲染子类。
        Dim config As New NetworkRenderConfig With {
            .CanvasSize = canvasSize,
            .Padding = padding,
            .Background = background,
            .DefaultColor = defaultColor,
            .DisplayId = displayId,
            .LabelColorAsNodeColor = labelColorAsNodeColor,
            .NodeStroke = nodeStroke,
            .MinLinkWidth = minLinkWidth,
            .LinkWidth = linkWidth,
            .NodeRadius = nodeRadius,
            .FontSize = fontSize,
            .LabelFontBase = labelFontBase,
            .EdgeDashTypes = edgeDashTypes,
            .EdgeShadowDistance = edgeShadowDistance,
            .DrawNodeShape = drawNodeShape,
            .NodeWidget = nodeWidget,
            .ShapeRender = shapeRender,
            .GetNodeLabel = getNodeLabel,
            .GetLabelPosition = getLabelPosition,
            .GetLabelColor = getLabelColor,
            .HideDisconnectedNode = hideDisconnectedNode,
            .ThrowEx = throwEx,
            .HullPolygonGroups = hullPolygonGroups,
            .LabelerIterations = labelerIterations,
            .LabelWordWrapWidth = labelWordWrapWidth,
            .ShowLabelerProgress = showLabelerProgress,
            .DefaultEdgeColor = defaultEdgeColor,
            .DefaultLabelColor = defaultLabelColor,
            .LabelTextStroke = labelTextStroke,
            .ShowConvexHullLegend = showConvexHullLegend,
            .DrawEdgeBends = drawEdgeBends,
            .DrawEdgeDirection = drawEdgeDirection,
            .ConvexHullLabelFontCSS = convexHullLabelFontCSS,
            .ConvexHullScale = convexHullScale,
            .ConvexHullCurveDegree = convexHullCurveDegree,
            .FillConvexHullPolygon = fillConvexHullPolygon,
            .Driver = driver,
            .Ppi = ppi
        }

        Return New NetworkPlot(net, config).Render()
    End Function

    Public Function DirectMapRadius(Optional scale# = 1) As Func(Of Node, Single)
        Return Function(n)
                   Dim r As Single = n.data.size(0)

                   ' 当网络之中没有任何边的时候，r的值会是NAN
                   If r = 0# OrElse r.IsNaNImaginary Then
                       r = If(n.data.neighborhoods < 30, n.data.neighborhoods * 9, n.data.neighborhoods * 7)
                       r = If(r = 0, 9, r)
                   End If

                   Return r * scale
               End Function
    End Function

End Module
