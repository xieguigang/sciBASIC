#Region "Microsoft.VisualBasic::3c7f14cf9d480851c303f2b07ea5dc51, gr\network-visualization\Visualizer\NetworkVisualizer.vb"

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
    '     Properties: BackgroundColor, DefaultEdgeColor
    ' 
    '     Function: (+2 Overloads) AutoScaler, CentralOffsets, DirectMapRadius, DrawImage, drawVertexNodes
    '               GetBounds, GetDisplayText, scales
    ' 
    '     Sub: drawEdges, drawhullPolygon, drawLabels
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConvexHull
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON
Imports sys = System.Math

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
    Public Property DefaultEdgeColor As Color = Color.LightGray

    ''' <summary>
    ''' 优先显示： <see cref="NodeData.label"/> -> <see cref="NodeData.origID"/> -> <see cref="Node.ID"/>
    ''' </summary>
    ''' <param name="n"></param>
    ''' <returns></returns>
    <Extension>
    Public Function GetDisplayText(n As Node) As String
        If n.data Is Nothing OrElse (n.data.origID.StringEmpty AndAlso n.data.label.StringEmpty) Then
            Return n.Label
        ElseIf n.data.label.StringEmpty Then
            Return n.data.origID
        Else
            Return n.data.label
        End If
    End Function

    ''' <summary>
    ''' 这里是计算出网络几点偏移到图像的中心所需要的偏移量
    ''' </summary>
    ''' <param name="nodes"></param>
    ''' <param name="size"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CentralOffsets(nodes As Dictionary(Of Node, PointF), size As Size) As PointF
        Return nodes.Values.CentralOffset(size)
    End Function

    <Extension>
    Private Function scales(nodes As IEnumerable(Of Node), scale As SizeF) As Dictionary(Of Node, Point)
        Dim table As New Dictionary(Of Node, Point)

        For Each n As Node In nodes
            With n.data.initialPostion.Point2D
                Call table.Add(n, New Point(.X * scale.Width, .Y * scale.Height))
            End With
        Next

        Return table
    End Function

    <Extension>
    Public Function GetBounds(graph As NetworkGraph) As RectangleF
        Dim points As Point() = graph _
            .vertex _
            .scales(scale:=New SizeF(1, 1)) _
            .Values _
            .ToArray
        Dim rect = points.GetBounds
        Return rect
    End Function

    <Extension>
    Public Function AutoScaler(graph As NetworkGraph, frameSize As Size, padding As Padding) As SizeF
        With graph.GetBounds
            Return New SizeF(
                frameSize.Width / (.Width + padding.Horizontal),
                frameSize.Height / (.Height + padding.Vertical)
            )
        End With
    End Function

    <Extension>
    Public Function AutoScaler(shape As IEnumerable(Of PointF), frameSize As Size, padding As Padding) As SizeF
        With shape.GetBounds
            Return New SizeF(
                frameSize.Width / (.Width + padding.Horizontal),
                frameSize.Height / (.Height + padding.Vertical)
            )
        End With
    End Function

    Const WhiteStroke$ = "stroke: white; stroke-width: 2px; stroke-dash: solid;"

    ''' <summary>
    ''' Rendering png or svg image from a given network graph model.
    ''' (假若属性是空值的话，在绘图之前可以调用<see cref="ApplyAnalysis"/>拓展方法进行一些分析)
    ''' </summary>
    ''' <param name="net"></param>
    ''' <param name="canvasSize">画布的大小</param>
    ''' <param name="padding">上下左右的边距分别为多少？</param>
    ''' <param name="background">背景色或者背景图片的文件路径</param>
    ''' <param name="defaultColor"></param>
    ''' <param name="nodePoints">如果还需要获取得到节点的绘图位置的话，则可以使用这个可选参数来获取返回</param>
    ''' <param name="hullPolygonGroups">需要显示分组的多边形的分组的名称的列表，也可以是一个表达式max或者min，分别表示最大或者最小的分组</param>
    ''' <param name="nodeRadius">By default all of the node have the same radius size</param>
    ''' <param name="labelFontBase">
    ''' 这个参数会提供字体的一些基础样式,字体的大小会从节点的属性中计算出来
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 一些内置的样式支持:
    ''' 
    ''' + 节点的颜色或者纹理: <see cref="NodeData.color"/>
    ''' </remarks>
    <ExportAPI("Draw.Image")>
    <Extension>
    Public Function DrawImage(net As NetworkGraph,
                              Optional canvasSize$ = "1024,1024",
                              Optional padding$ = g.DefaultPadding,
                              Optional background$ = "white",
                              Optional defaultColor As Color = Nothing,
                              Optional displayId As Boolean = True,
                              Optional labelColorAsNodeColor As Boolean = False,
                              Optional nodeStroke$ = WhiteStroke,
                              Optional minLinkWidth! = 2,
                              Optional nodeRadius As [Variant](Of Func(Of Node, Single), Single) = Nothing,
                              Optional fontSize As [Variant](Of Func(Of Node, Single), Single) = Nothing,
                              Optional labelFontBase$ = CSSFont.Win7Normal,
                              Optional ByRef nodePoints As Dictionary(Of Node, PointF) = Nothing,
                              Optional edgeDashTypes As Dictionary(Of String, DashStyle) = Nothing,
                              Optional getNodeLabel As Func(Of Node, String) = Nothing,
                              Optional hideDisconnectedNode As Boolean = False,
                              Optional throwEx As Boolean = True,
                              Optional hullPolygonGroups$ = Nothing) As GraphicsData

        ' 所绘制的图像输出的尺寸大小
        Dim frameSize As Size = canvasSize.SizeParser
        Dim margin As Padding = CSS.Padding.TryParse(
            padding, New Padding With {
                .Bottom = 100,
                .Left = 100,
                .Right = 100,
                .Top = 100
            })

        ' 1. 先将网络图形对象置于输出的图像的中心位置
        ' 2. 进行矢量图放大
        ' 3. 执行绘图操作

        ' 获取得到当前的这个网络对象相对于图像的中心点的位移值
        Dim scalePos As Dictionary(Of Node, PointF) = net _
            .vertex _
            .ToDictionary(Function(n) n,
                          Function(node)
                              Return node.data.initialPostion.Point2D.PointF
                          End Function)
        Dim offset As Point = scalePos _
            .CentralOffsets(frameSize) _
            .ToPoint

        ' 进行位置偏移
        ' 将网络图形移动到画布的中央区域
        scalePos = scalePos.ToDictionary(Function(node) node.Key,
                                         Function(point)
                                             Return point.Value.OffSet2D(offset)
                                         End Function)
        ' 进行矢量放大
        Dim scale As SizeF = scalePos.Values.AutoScaler(frameSize, margin)
        Dim scalePoints = scalePos.Values.Enlarge((CDbl(scale.Width), CDbl(scale.Height)))

        With scalePos.Keys.AsList
            For i As Integer = 0 To .Count - 1
                scalePos(.Item(i)) = scalePoints(i)
            Next

            nodePoints = scalePos
        End With

        Call "Initialize gdi objects...".__INFO_ECHO

        Dim stroke As Pen = CSS.Stroke.TryParse(nodeStroke).GDIObject
        Dim baseFont As Font = CSSFont.TryParse(
            labelFontBase, New CSSFont With {
                .family = FontFace.MicrosoftYaHei,
                .size = 12,
                .style = FontStyle.Regular
            }).GDIObject

        Call "Initialize variables, done!".__INFO_ECHO

        If edgeDashTypes Is Nothing Then
            edgeDashTypes = New Dictionary(Of String, DashStyle)
        End If
        If getNodeLabel Is Nothing Then
            getNodeLabel = Function(node) node.GetDisplayText
        End If

        defaultColor = If(defaultColor.IsEmpty, Color.Black, defaultColor)

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
            Dim min = sys.Min(frameSize.Width, frameSize.Height) / 25
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

        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)
                Call "Render network edges...".__INFO_ECHO
                ' 首先在这里绘制出网络的框架：将所有的边绘制出来
                Call g.drawEdges(net, minLinkWidthValue, edgeDashTypes, scalePos, throwEx)

                Call "Render network nodes...".__INFO_ECHO
                ' 然后将网络之中的节点绘制出来，同时记录下节点的位置作为label text的锚点
                ' 最后通过退火算法计算出合适的节点标签文本的位置之后，再使用一个循环绘制出
                ' 所有的节点的标签文本

                If Not hullPolygonGroups.StringEmpty Then
                    Call g.drawhullPolygon(drawPoints, hullPolygonGroups, scalePos)
                End If

                ' 在这里进行节点的绘制
                labels += g.drawVertexNodes(
                    drawPoints:=drawPoints,
                    radiusValue:=nodeRadiusMapper,
                    fontSizeValue:=fontSizeMapper,
                    defaultColor:=defaultColor,
                    stroke:=stroke,
                    baseFont:=baseFont,
                    scalePos:=scalePos,
                    throwEx:=throwEx,
                    displayId:=displayId
                )

                If displayId AndAlso labels = 0 Then
                    Call "There is no node label data could be draw currently, please check your data....".Warning
                End If

                If displayId AndAlso labels > 0 Then
                    Call g.drawLabels(labels, frameSize, labelColorAsNodeColor)
                End If
            End Sub

        Call "Start Render...".__INFO_ECHO

        Return g.GraphicsPlots(frameSize, margin, background, plotInternal)
    End Function

    Public Function DirectMapRadius(Optional scale# = 1) As Func(Of Node, Single)
        Return Function(n)
                   Dim r As Single = n.data.radius

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
                                              scalePos As Dictionary(Of Node, PointF),
                                              throwEx As Boolean,
                                              displayId As Boolean) As IEnumerable(Of LayoutLabel)
        Dim pt As Point
        Dim br As Brush
        Dim rect As Rectangle

        Call "Rendering nodes...".__DEBUG_ECHO

        For Each n As Node In drawPoints
            Dim r# = radiusValue(n)

            With DirectCast(New SolidBrush(defaultColor), Brush).AsDefault(n.NodeBrushAssert)
                br = n.data.color Or .ByRef
            End With

            Dim center As PointF = scalePos(n)

            With center
                pt = New Point(.X - r / 2, .Y - r / 2)
            End With

            Dim invalidRegion As Boolean = False

            rect = New Rectangle(pt, New Size(r, r))

            ' 绘制节点，目前还是圆形
            If TypeOf g Is Graphics2D Then
                Try
                    Call g.FillPie(br, rect, 0, 360)
                    Call g.DrawEllipse(stroke, rect)
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

            ' 如果当前的节点没有超出有效的视图范围,并且参数设置为显示id编号
            ' 则生成一个label绘制的数据模型
            If (Not invalidRegion) AndAlso displayId Then
                Dim fontSize! = fontSizeValue(n)
                Dim font As New Font(
                    baseFont.Name,
                    fontSize,
                    baseFont.Style,
                    baseFont.Unit,
                    baseFont.GdiCharSet,
                    baseFont.GdiVerticalFont
                )
                Dim label As New Label With {
                    .text = n.GetDisplayText
                }

                With g.MeasureString(label.text, font)
                    label.width = .Width
                    label.height = .Height
                End With

                Yield New LayoutLabel With {
                    .label = label,
                    .anchor = New Anchor(rect),
                    .style = font,
                    .color = br
                }
            End If
        Next
    End Function

    <Extension>
    Private Sub drawhullPolygon(g As IGraphics, drawPoints As Node(), hullPolygonGroups$, scalePos As Dictionary(Of Node, PointF))
        Dim hullPolygon As Index(Of String)
        Dim groups = drawPoints _
            .GroupBy(Function(n) n.data.Properties!nodeType) _
            .ToArray

        If hullPolygonGroups.TextEquals("max") Then
            hullPolygon = {
                groups.OrderByDescending(Function(node) node.Count) _
                      .First _
                      .Key
            }
        ElseIf hullPolygonGroups.TextEquals("min") Then
            hullPolygon = {
                groups.Where(Function(group) group.Count > 2) _
                      .OrderBy(Function(node) node.Count) _
                      .First _
                      .Key
            }
        Else
            hullPolygon = hullPolygonGroups.Split(","c)
        End If

        For Each group In groups
            If group.Count > 2 AndAlso group.Key Like hullPolygon Then
                Dim positions = group _
                    .Select(Function(p) scalePos(p)) _
                    .JarvisMatch _
                    .Enlarge(1.25)
                Dim color As Color = group _
                    .Select(Function(p)
                                Return DirectCast(p.data.color, SolidBrush).Color
                            End Function) _
                    .Average

                Call g.DrawHullPolygon(positions, color)
            End If
        Next
    End Sub

    <Extension>
    Private Sub drawEdges(g As IGraphics, net As NetworkGraph,
                          minLinkWidthValue As [Default](Of Single),
                          edgeDashTypes As Dictionary(Of String, DashStyle),
                          scalePos As Dictionary(Of Node, PointF),
                          throwEx As Boolean)
        Dim cl As Color

        For Each edge As Edge In net.graphEdges
            Dim n As Node = edge.U
            Dim otherNode As Node = edge.V

            cl = DefaultEdgeColor

            If edge.data.weight < 0.5 Then
                cl = Color.LightGray
            ElseIf edge.data.weight < 0.75 Then
                cl = Color.Blue
            End If

            Dim w! = CSng(5 * edge.data.weight * 2) Or minLinkWidthValue
            Dim lineColor As New Pen(cl, w)

            With edge.data!interaction_type
                If Not .IsNothing AndAlso edgeDashTypes.ContainsKey(.ByRef) Then
                    lineColor.DashStyle = edgeDashTypes(.ByRef)
                ElseIf edgeDashTypes.ContainsKey(edge.ID) Then
                    lineColor.DashStyle = edgeDashTypes(edge.ID)
                End If
            End With

            ' 在这里绘制的是节点之间相连接的边
            Dim a = scalePos(n), b = scalePos(otherNode)

            Try
                Call g.DrawLine(lineColor, a, b)
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
    End Sub

    ''' <summary>
    ''' 使用退火算法计算出节点标签文本的位置
    ''' </summary>
    ''' 
    <Extension>
    Private Sub drawLabels(g As IGraphics,
                           labels As List(Of LayoutLabel),
                           frameSize As Size,
                           labelColorAsNodeColor As Boolean)
        Dim br As Brush
        Dim rect As Rectangle
        Dim lx, ly As Single

        Call d3js _
            .labeler(maxMove:=100, maxAngle:=1, w_len:=1, w_inter:=2, w_lab2:=50, w_lab_anc:=50, w_orient:=2) _
            .Anchors(labels.Select(Function(x) x.anchor)) _
            .Labels(labels.Select(Function(x) x.label)) _
            .Size(frameSize) _
            .Start(nsweeps:=2000, showProgress:=False)

        For Each label In labels
            With label
                If Not labelColorAsNodeColor Then
                    br = Brushes.Black
                Else
                    br = .color
                    br = New SolidBrush(DirectCast(br, SolidBrush).Color.Darken(0.005))
                End If

                lx = .label.X
                ly = .label.Y

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
                    rect.ToFloat,
                    .style,
                    StringFormat.GenericTypographic
                )

                Call g.DrawString(.label.text, .style, br, lx, ly)

                ' 绘制轮廓（描边）
                ' Call g.FillPath(br, path)
                ' Call g.DrawPath(New Pen(DirectCast(br, SolidBrush).Color.Dark(0.005), 4), path)
            End With
        Next
    End Sub
End Module
