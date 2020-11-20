#Region "Microsoft.VisualBasic::783de79d969a5486311c04117419425e, gr\network-visualization\Visualizer\NetworkVisualizer.vb"

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

    ' Module NetworkVisualizer
    ' 
    '     Properties: BackgroundColor
    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    '         Function: DirectMapRadius, drawEdges, DrawImage, drawVertexNodes, internalDrawEdgeLine
    ' 
    '         Sub: drawhullPolygon, drawLabels
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
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.EdgeBundling
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConcaveHull
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports stdNum = System.Math

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

    Public Delegate Function DrawNodeShape(id As String, g As IGraphics, brush As Brush, radius As Single, center As PointF) As RectangleF
    Public Delegate Function GetLabelPosition(node As Node, label$, shapeLayout As RectangleF, labelSize As SizeF) As PointF

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
                              Optional getNodeLabel As Func(Of Node, String) = Nothing,
                              Optional getLabelPosition As GetLabelPosition = Nothing,
                              Optional getLabelColor As Func(Of Node, Color) = Nothing,
                              Optional hideDisconnectedNode As Boolean = False,
                              Optional throwEx As Boolean = True,
                              Optional hullPolygonGroups As NamedValue(Of String) = Nothing,
                              Optional labelerIterations% = 1500,
                              Optional labelWordWrapWidth% = -1,
                              Optional isLabelPinned As Func(Of Node, String, Boolean) = Nothing,
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
                              Optional driver As Drivers = Drivers.Default) As GraphicsData

        Call GetType(NetworkVisualizer).Assembly _
            .FromAssembly _
            .DoCall(Sub(assm)
                        Dim driverPrompt$ = "
 Current graphic driver is pixel based gdi+ engine, and you could change the graphics driver 
 to vector based graphic engine via config in commandline:

    tool /command [...arguments] /@set ""graphic_driver=svg/ps"""

                        If g.ActiveDriver <> Drivers.GDI AndAlso g.ActiveDriver <> Drivers.Default Then
                            driverPrompt = ""
                        End If

                        VBDebugger.WaitOutput()
                        CLITools.AppSummary(assm, "Welcome to use network graph visualizer api from sciBASIC.NET framework.", driverPrompt, App.StdOut)

                        Call Console.WriteLine()
                    End Sub)

        ' 所绘制的图像输出的尺寸大小
        Dim frameSize As SizeF = PrinterDimension.SizeOf(canvasSize)
        Dim margin As Padding = CSS.Padding.TryParse(
            padding, [default]:=New Padding With {
                .Bottom = 100,
                .Left = 100,
                .Right = 100,
                .Top = 100
            })

        Call $"Canvas size expression '{canvasSize}' = [{frameSize.Width}, {frameSize.Height}]".__DEBUG_ECHO
        Call $"Canvas padding [{margin.Top}, {margin.Right}, {margin.Bottom}, {margin.Left}]".__DEBUG_ECHO

        ' 1. 先将网络图形对象置于输出的图像的中心位置
        ' 2. 进行矢量图放大
        ' 3. 执行绘图操作

        ' 获取得到当前的这个网络对象相对于图像的中心点的位移值
        Dim scalePos As Dictionary(Of String, PointF) = CanvasScaler.CalculateNodePositions(net, frameSize, margin)

        Call "Initialize gdi objects...".__INFO_ECHO

        Dim stroke As Pen = CSS.Stroke.TryParse(nodeStroke)?.GDIObject
        Dim baseFont As Font = CSSFont.TryParse(
            labelFontBase, New CSSFont With {
                .family = FontFace.MicrosoftYaHei,
                .size = 12,
                .style = FontStyle.Regular
            }).GDIObject

        Call "Initialize variables, done!".__INFO_ECHO

        If edgeDashTypes Is Nothing Then
            edgeDashTypes = New Dictionary(Of String, DashStyle)
        ElseIf edgeDashTypes Like GetType(DashStyle) Then
            edgeDashTypes = net.graphEdges _
                .ToDictionary(Function(e) e.ID,
                              Function(null)
                                  Return edgeDashTypes.VB
                              End Function)
        End If

        If getNodeLabel Is Nothing AndAlso displayId Then
            getNodeLabel = Function(node)
                               Return node.GetDisplayText
                           End Function
        ElseIf getNodeLabel Is Nothing Then
            getNodeLabel = Function(v) Nothing
        End If

        defaultColor = If(defaultColor.StringEmpty, "skyblue", defaultColor)

        ' 在这里不可以使用 <=，否则会导致等于最小值的时候出现无限循环的bug
        Dim minLinkWidthValue = minLinkWidth.AsDefault(Function(width) CInt(width) < minLinkWidth)
        Dim fontSizeMapper As Func(Of Node, Single)
        Dim nodeRadiusMapper As Func(Of Node, Single)

        If fontSize Is Nothing Then
            fontSizeMapper = Function() 16.0!
        ElseIf fontSize Like GetType(Single) Then
            Dim fsize As Single = fontSize
            fontSizeMapper = Function() fsize
        Else
            fontSizeMapper = fontSize
        End If

        If nodeRadius Is Nothing Then
            Dim min = stdNum.Min(frameSize.Width, frameSize.Height) / 25
            nodeRadiusMapper = Function() min
        ElseIf nodeRadius Like GetType(Single) Then
            Dim radius As Single = nodeRadius
            nodeRadiusMapper = Function() radius
        Else
            nodeRadiusMapper = nodeRadius
        End If

        ' if required hide disconnected nodes, then only the connected node in the network 
        ' Graph will be draw
        ' otherwise all of the nodes in target network graph will be draw onto the canvas.
        Dim connectedNodes = net.connectedNodes.AsDefault
        Dim drawPoints = net.vertex.ToArray Or connectedNodes.When(hideDisconnectedNode)
        Dim labels As New List(Of LayoutLabel)

        If linkWidth Is Nothing Then
            linkWidth = Function(edge) CSng(5 * edge.weight * 2) Or minLinkWidthValue
        End If

        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)

                If Not hullPolygonGroups.IsEmpty Then
                    Call "Render hull polygon layer...".__DEBUG_ECHO
                    Call g.drawhullPolygon(
                        drawPoints:=drawPoints,
                        hullPolygonGroups:=hullPolygonGroups,
                        scalePos:=scalePos,
                        showConvexHullLegend:=showConvexHullLegend,
                        convexHullLabelFontCSS:=convexHullLabelFontCSS$,
                        convexHullScale:=convexHullScale!,
                        convexHullCurveDegree:=convexHullCurveDegree,
                        fillPolygon:=fillConvexHullPolygon
                    )
                End If

                Call "Render network edges...".__INFO_ECHO
                ' 首先在这里绘制出网络的框架：将所有的边绘制出来
                labels += g.drawEdges(
                    net,
                    linkWidth,
                    edgeDashTypes,
                    scalePos,
                    throwEx,
                    edgeShadowDistance:=edgeShadowDistance,
                    defaultEdgeColor:=defaultEdgeColor.TranslateColor,
                    drawEdgeBends:=drawEdgeBends,
                    drawEdgeDirection:=drawEdgeDirection
                )

                Call "Render network elements...".__INFO_ECHO
                ' 然后将网络之中的节点绘制出来，同时记录下节点的位置作为label text的锚点
                ' 最后通过退火算法计算出合适的节点标签文本的位置之后，再使用一个循环绘制出
                ' 所有的节点的标签文本

                ' 在这里进行节点的绘制
                labels += g.drawVertexNodes(
                    drawPoints:=drawPoints,
                    radiusValue:=nodeRadiusMapper,
                    fontSizeValue:=fontSizeMapper,
                    defaultColor:=defaultColor.TranslateColor,
                    stroke:=stroke,
                    baseFont:=baseFont,
                    scalePos:=scalePos,
                    throwEx:=throwEx,
                    getDisplayLabel:=getNodeLabel,
                    drawNodeShape:=drawNodeShape,
                    getLabelPosition:=getLabelPosition,
                    labelWordWrapWidth:=labelWordWrapWidth,
                    isLabelPinned:=isLabelPinned,
                    nodeWidget:=nodeWidget
                )

                If displayId AndAlso labels = 0 Then
                    Call "There is no node label data could be draw currently, please check your data....".Warning
                End If

                If displayId AndAlso labels > 0 Then
                    Call g.drawLabels(
                        labels:=labels,
                        frameSize:=frameSize.ToSize,
                        labelColorAsNodeColor:=labelColorAsNodeColor,
                        iteration:=labelerIterations,
                        showLabelerProgress:=showLabelerProgress,
                        defaultLabelColorValue:=defaultLabelColor,
                        labelTextStrokeCSS:=labelTextStroke,
                        getLabelColor:=getLabelColor
                    )
                End If

                Call "Network canvas rendering job done!".__DEBUG_ECHO
            End Sub

        Call "Start Render...".__INFO_ECHO

        Return g.GraphicsPlots(frameSize.ToSize, margin, background, plotInternal, driver:=driver)
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

    <Extension>
    Private Iterator Function drawVertexNodes(g As IGraphics, drawPoints As Node(),
                                              radiusValue As Func(Of Node, Single),
                                              fontSizeValue As Func(Of Node, Single),
                                              defaultColor As Color,
                                              stroke As Pen,
                                              baseFont As Font,
                                              scalePos As Dictionary(Of String, PointF),
                                              throwEx As Boolean,
                                              getDisplayLabel As Func(Of Node, String),
                                              drawNodeShape As DrawNodeShape,
                                              getLabelPosition As GetLabelPosition,
                                              labelWordWrapWidth As Integer,
                                              isLabelPinned As Func(Of Node, String, Boolean),
                                              nodeWidget As Func(Of IGraphics, PointF, Double, Node, RectangleF)) As IEnumerable(Of LayoutLabel)
        Dim pt As Point
        Dim br As Brush
        Dim rect As RectangleF

        Call "Rendering nodes...".__DEBUG_ECHO

        If isLabelPinned Is Nothing Then
            ' all of the label is unpinned by default 
            isLabelPinned = Function(n, l) False
        End If

        For Each n As Node In drawPoints
            Dim r# = radiusValue(n)
            Dim center As PointF = scalePos(n.label)
            Dim invalidRegion As Boolean = False

            With DirectCast(New SolidBrush(defaultColor), Brush).AsDefault(n.NodeBrushAssert)
                br = n.data.color Or .ByRef
            End With

            If drawNodeShape Is Nothing Then
                With center
                    pt = New Point(.X - r / 2, .Y - r / 2)
                End With

                rect = New RectangleF(pt, New Size(r, r))

                ' 绘制节点，目前还是圆形
                If TypeOf g Is Graphics2D Then
                    Try
                        Call g.FillPie(br, rect, 0, 360)

                        If Not stroke Is Nothing Then
                            Call g.DrawEllipse(stroke, rect)
                        End If
                    Catch ex As Exception
                        If throwEx Then
                            Throw New Exception(rect.GetJson, ex)
                        Else
                            Call $"Ignore of this invalid circle region: {rect.GetJson}".Warning
                        End If

                        invalidRegion = True
                    End Try
                Else
                    Call g.DrawCircle(center, DirectCast(br, SolidBrush).Color, stroke, radius:=r)
                End If
            Else
                rect = drawNodeShape(n.label, g, br, r, center)
            End If

            If Not nodeWidget Is Nothing Then
                Dim rectLayout As RectangleF = nodeWidget(g, center, r, n)

                If Not rectLayout.IsEmpty Then
                    Yield New LayoutLabel With {
                        .anchor = New Anchor(rectLayout),
                        .color = Nothing,
                        .label = New Label With {
                            .height = rectLayout.Height,
                            .pinned = True,
                            .text = Nothing,
                            .width = rectLayout.Width,
                            .X = rectLayout.X,
                            .Y = rectLayout.Y
                        },
                        .node = n,
                        .shapeRectangle = rectLayout,
                        .style = Nothing
                    }
                End If
            End If

            ' 如果当前的节点没有超出有效的视图范围,并且参数设置为显示id编号
            ' 则生成一个label绘制的数据模型
            Dim displayID As String = getDisplayLabel(n)

            If (Not invalidRegion) AndAlso Not displayID.StringEmpty Then
                Dim fontSize! = fontSizeValue(n)
                Dim font As New Font(
                    baseFont.Name,
                    fontSize,
                    baseFont.Style,
                    baseFont.Unit,
                    baseFont.GdiCharSet,
                    baseFont.GdiVerticalFont
                )
                ' 节点的标签文本的位置默认在正中
                Dim label As New Label With {
                    .text = displayID,
                    .pinned = isLabelPinned(n, displayID)
                }

                If labelWordWrapWidth > 0 Then
                    label.text = WordWrap.DoWordWrap(label.text, labelWordWrapWidth)
                End If

                With g.MeasureString(label.text, font)
                    label.width = .Width
                    label.height = .Height

                    If getLabelPosition Is Nothing Then
                        label.X = center.X - .Width / 2
                        label.Y = center.Y - .Height / 2
                    Else
                        With .DoCall(Function(lsz) getLabelPosition(n, label.text, rect, lsz))
                            label.X = .X
                            label.Y = .Y
                        End With
                    End If
                End With

                Yield New LayoutLabel With {
                    .label = label,
                    .anchor = New Anchor(rect),
                    .style = font,
                    .color = br,
                    .node = n,
                    .shapeRectangle = rect
                }
            End If
        Next
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="drawPoints"></param>
    ''' <param name="hullPolygonGroups">
    ''' [<see cref="NodeData.Properties"/> Name => expression]
    ''' 
    ''' expression = max/min largest or smallest group
    ''' expression = 'a,b,c,d,e' node category to draw hull polygon 
    ''' </param>
    ''' <param name="scalePos"></param>
    <Extension>
    Private Sub drawhullPolygon(g As IGraphics,
                                drawPoints As Node(),
                                hullPolygonGroups As NamedValue(Of String),
                                scalePos As Dictionary(Of String, PointF),
                                showConvexHullLegend As Boolean,
                                fillPolygon As Boolean,
                                convexHullLabelFontCSS$,
                                convexHullScale!,
                                convexHullCurveDegree!)

        Dim hullPolygon As Index(Of String)
        Dim groups = drawPoints _
            .Where(Function(n) Not n.data(hullPolygonGroups.Name).StringEmpty) _
            .GroupBy(Function(n)
                         Return n.data(hullPolygonGroups.Name)
                     End Function) _
            .ToArray
        Dim colors As LoopArray(Of Color) = Designer.GetColors(hullPolygonGroups.Description Or "set1:c8".AsDefault)
        Dim convexHullLabelFont As Font = CSSFont.TryParse(convexHullLabelFontCSS$)
        Dim singleGroupKey As String = Nothing

        If hullPolygonGroups.Value.StringEmpty Then
            Return
        End If

        If hullPolygonGroups.Value.TextEquals("max") Then
            singleGroupKey = $"max({hullPolygonGroups.Name})"
            hullPolygon = {
                groups.OrderByDescending(Function(node) node.Count) _
                      .First _
                      .Key
            }
        ElseIf hullPolygonGroups.Value.TextEquals("min") Then
            singleGroupKey = $"min({hullPolygonGroups.Name})"
            hullPolygon = {
                groups.Where(Function(group) group.Count > 2) _
                      .OrderBy(Function(node) node.Count) _
                      .First _
                      .Key
            }
        ElseIf hullPolygonGroups.Value.IsPattern("top\s*\d+") Then
            hullPolygon = groups _
                .Where(Function(group) group.Count > 2) _
                .OrderByDescending(Function(n) n.Count) _
                .Take(hullPolygonGroups.Value.Match("\d+").DoCall(AddressOf Integer.Parse)) _
                .Select(Function(group) group.Key) _
                .ToArray
        Else
            hullPolygon = hullPolygonGroups.Value.Split(","c)
        End If

        Dim labels As New List(Of (String, Color))

        For Each group In groups
            If group.Count > 2 AndAlso group.Key Like hullPolygon Then

                Call $"[ConvexHull] render for {group.Key}".__DEBUG_ECHO

                Dim positions = group _
                    .Select(Function(p) scalePos(p.label)) _
                    .ConcaveHull _ ' .JarvisMatch _
                    .Enlarge(convexHullScale!)
                Dim color As Color = colors.Next

                Call g.DrawHullPolygon(
                    polygon:=positions,
                    color:=color,
                    alpha:=50,
                    convexHullCurveDegree:=convexHullCurveDegree,
                    fillPolygon:=fillPolygon
                )
                Call labels.Add((group.Key, color))
            End If
        Next

        If Not singleGroupKey.StringEmpty Then
            labels = New List(Of (String, Color)) From {(singleGroupKey, labels.Last.Item2)}
        End If

        If showConvexHullLegend Then
            Dim maxLabel = labels.Select(Function(lb) lb.Item1).MaxLengthString
            Dim maxSize As SizeF = g.MeasureString(maxLabel, convexHullLabelFont)
            Dim legendShapeSize As New SizeF With {
                .Width = maxSize.Height * 1.5,
                .Height = maxSize.Height
            }
            Dim topLeft As New PointF With {
                .X = g.Size.Width - maxSize.Width - maxSize.Height * 2.5,
                .Y = legendShapeSize.Width
            }

            For Each label In labels
                Call g.FillRectangle(New SolidBrush(label.Item2), New RectangleF(topLeft, legendShapeSize))
                Call g.DrawString(label.Item1, convexHullLabelFont, Brushes.Black, New PointF(topLeft.X + legendShapeSize.Width + 20, topLeft.Y))

                topLeft = New PointF With {
                    .X = topLeft.X,
                    .Y = topLeft.Y + maxSize.Height * 1.25
                }
            Next
        End If
    End Sub

    ''' <summary>
    ''' 这个函数会将edge作为一个layout的shape返回用于标签的布局计算
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="net"></param>
    ''' <param name="edgeDashTypes"></param>
    ''' <param name="scalePos"></param>
    ''' <param name="throwEx"></param>
    ''' <param name="edgeShadowDistance"></param>
    ''' <param name="defaultEdgeColor"></param>
    ''' <param name="drawEdgeBends"></param>
    ''' <param name="drawEdgeDirection"></param>
    ''' <returns></returns>
    <Extension>
    Private Iterator Function drawEdges(g As IGraphics, net As NetworkGraph,
                                        linkWidth As Func(Of Edge, Single),
                                        edgeDashTypes As Dictionary(Of String, DashStyle),
                                        scalePos As Dictionary(Of String, PointF),
                                        throwEx As Boolean,
                                        edgeShadowDistance As Single,
                                        defaultEdgeColor As Color,
                                        drawEdgeBends As Boolean,
                                        drawEdgeDirection As Boolean) As IEnumerable(Of LayoutLabel)

        For Each edge As Edge In net.graphEdges
            Dim n As Node = edge.U
            Dim otherNode As Node = edge.V
            Dim w! = linkWidth(edge)
            Dim lineColor As Pen

            If edge.data.color Is Nothing Then
                lineColor = New Pen(defaultEdgeColor, w)
            Else
                lineColor = New Pen(edge.data.color, w)
            End If

            With edge.data!interaction_type
                If Not .IsNothing AndAlso edgeDashTypes.ContainsKey(.ByRef) Then
                    lineColor.DashStyle = edgeDashTypes(.ByRef)
                ElseIf edgeDashTypes.ContainsKey(edge.ID) Then
                    lineColor.DashStyle = edgeDashTypes(edge.ID)
                End If
            End With

            ' 在这里绘制的是节点之间相连接的边
            Dim a = scalePos(n.label)
            Dim b = scalePos(otherNode.label)
            Dim edgeShadowColor As New Pen(Brushes.Gray) With {
                .Width = lineColor.Width,
                .DashStyle = lineColor.DashStyle
            }
            Dim draw = g.internalDrawEdgeLine(edgeShadowDistance, edgeShadowColor, lineColor)

            Try
                Dim bends As XYMetaHandle() = edge.data.bends.SafeQuery.ToArray
                Dim isNan As Boolean = bends.Any(Function(bend) bend.isNaN)

                If (Not isNan) AndAlso drawEdgeBends AndAlso Not bends.IsNullOrEmpty Then
                    If bends.Length <> edge.data.bends.Length Then
                        Call $"{edge.ID} removes {edge.data.bends.Length - bends.Length} bends points.".__DEBUG_ECHO
                    End If

                    If bends.Length = 1 Then
                        Yield draw({a, b}, drawEdgeDirection)
                    Else
                        Dim segmentTuples = bends.SlideWindows(2).ToArray

                        For i As Integer = 0 To segmentTuples.Length - 1
                            Dim line As SlideWindow(Of XYMetaHandle) = segmentTuples(i)
                            Dim pta = line(Scan0).GetPoint(a.X, a.Y, b.X, b.Y)
                            Dim ptb = line(1).GetPoint(a.X, a.Y, b.X, b.Y)

                            Yield draw({pta, ptb}, If(i = segmentTuples.Length - 1, drawEdgeDirection, False))
                        Next
                    End If
                Else
                    Yield draw({a, b}, drawEdgeDirection)
                End If
            Catch ex As Exception
                Dim line As New Dictionary(Of String, String) From {
                    {NameOf(a), $"[{a.X}, {a.Y}]"},
                    {NameOf(b), $"[{b.X}, {b.Y}]"}
                }

                If throwEx Then
                    Throw New Exception(line.GetJson, ex)
                Else
                    Call $"Ignore of this invalid line range: {line.GetJson}".Warning
                End If
            End Try
        Next
    End Function

    <Extension>
    Private Function internalDrawEdgeLine(g As IGraphics, edgeShadowDistance!, edgeShadowColor As Pen, lineColor As Pen) As Func(Of PointF(), Boolean, LayoutLabel)
        Dim pt1, pt2 As PointF

        Return Function(line, drawDir)
                   If edgeShadowDistance <> 0 Then
                       ' 绘制底层的阴影
                       pt1 = line(0).OffSet2D(edgeShadowDistance, edgeShadowDistance)
                       pt2 = line(1).OffSet2D(edgeShadowDistance, edgeShadowDistance)

                       If drawDir Then
                           edgeShadowColor.EndCap = LineCap.ArrowAnchor
                       End If

                       g.DrawLine(edgeShadowColor, pt1:=pt1, pt2:=pt2)
                       edgeShadowColor.EndCap = LineCap.Flat
                   End If

                   If drawDir Then
                       Dim bigArrow As New AdjustableArrowCap(4, 4)

                       lineColor.CustomEndCap = bigArrow ' LineCap.ArrowAnchor
                   End If

                   ' 直接画一条直线
                   g.DrawLine(lineColor, line(0), line(1))
                   lineColor.EndCap = LineCap.Flat

                   Return New LayoutLabel With {
                       .anchor = New Anchor((line(Scan0).X + line(1).X) / 2, (line(Scan0).Y + line(1).Y) / 2, 5),
                       .color = Nothing,
                       .label = New Label(Nothing, .anchor, New Size(stdNum.Abs(line(Scan0).X - line(1).X), stdNum.Abs(line(Scan0).Y - line(1).Y))) With {
                           .pinned = True
                       },
                       .node = Nothing,
                       .shapeRectangle = .label.rectangle,
                       .style = Nothing
                   }
               End Function
    End Function

    ''' <summary>
    ''' 使用退火算法计算出节点标签文本的位置
    ''' </summary>
    ''' 
    <Extension>
    Private Sub drawLabels(g As IGraphics,
                           labels As List(Of LayoutLabel),
                           frameSize As Size,
                           labelColorAsNodeColor As Boolean,
                           iteration%,
                           showLabelerProgress As Boolean,
                           defaultLabelColorValue$,
                           labelTextStrokeCSS$,
                           getLabelColor As Func(Of Node, Color))
        Dim br As Brush
        Dim rect As Rectangle
        Dim lx, ly As Single
        Dim defaultLabelColor As New SolidBrush(defaultLabelColorValue.TranslateColor)
        Dim labelTextStroke As Pen = Stroke.TryParse(labelTextStrokeCSS)
        Dim color As Color

        ' 小于等于零的时候表示不进行布局计算
        If iteration > 0 Then
            Call $"Do node label layouts, iteration={iteration}".__INFO_ECHO
            Call d3js _
                .labeler(maxMove:=1, maxAngle:=1, w_len:=1, w_inter:=2, w_lab2:=10, w_lab_anc:=10, w_orient:=2) _
                .Anchors(labels.Select(Function(x) x.anchor)) _
                .Labels(labels.Select(Function(x) x.label)) _
                .Size(frameSize) _
                .Start(nsweeps:=iteration, showProgress:=showLabelerProgress)
        End If
        If getLabelColor Is Nothing Then
            getLabelColor = Function(node) Nothing
        End If

        For Each label As LayoutLabel In labels.Where(Function(a) Not a.color Is Nothing)
            With label
                If Not labelColorAsNodeColor Then
                    color = getLabelColor(label.node)

                    If color.IsEmpty Then
                        br = defaultLabelColor
                    Else
                        br = New SolidBrush(color)
                    End If
                Else
                    br = .color
                    br = New SolidBrush(DirectCast(br, SolidBrush).Color.Darken(0.005))
                End If

                lx = .label.X
                ly = .label.Y

                If iteration > 0 Then
                    If label.offsetDistance >= stdNum.Max(g.Size.Width, g.Size.Height) * 0.01 Then
                        Call g.DrawLine(New Pen(Brushes.Gray, 10) With {.DashStyle = DashStyle.Dot}, label.anchor, label.GetTextAnchor)
                    End If
                End If

                With g.MeasureString(.label.text, .style)
                    If lx < 0 Then
                        lx = 1
                    ElseIf lx + .Width > frameSize.Width Then
                        lx -= (lx + .Width - frameSize.Width) + 5
                    End If

                    If ly < 0 Then
                        ly = 1
                    ElseIf ly + .Height > frameSize.Height Then
                        ly -= (ly + .Height - frameSize.Height) + 5
                    End If

                    rect = New Rectangle(lx, ly, .Width, .Height)
                End With

                Dim path As GraphicsPath = Imaging.GetStringPath(
                    .label.text,
                    g.DpiX,
                    rect.OffSet2D(.style.Size / 5, 0).ToFloat,
                    .style,
                    StringFormat.GenericTypographic
                )

                If Not labelTextStroke Is Nothing Then
                    ' 绘制轮廓（描边）
                    Call g.DrawString(.label.text, .style, br, lx, ly)
                    Call g.DrawPath(labelTextStroke, path)
                Else
                    Call WordWrap.DrawTextCentraAlign(g, .label, New PointF(lx, ly), br, .style)
                End If
            End With
        Next
    End Sub
End Module
