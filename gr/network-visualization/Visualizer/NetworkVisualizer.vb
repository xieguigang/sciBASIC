#Region "Microsoft.VisualBasic::93d72b5a9da31c5d09924e65cd6179b0, gr\network-visualization\Visualizer\NetworkVisualizer.vb"

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
    '     Function: __scale, AutoScaler, CentralOffsets, DrawImage, GetBounds
    '               GetDisplayText
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Styling
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConvexHull
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON

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
        If n.Data Is Nothing OrElse (n.Data.origID.StringEmpty AndAlso n.Data.label.StringEmpty) Then
            Return n.Label
        ElseIf n.Data.label.StringEmpty Then
            Return n.Data.origID
        Else
            Return n.Data.label
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
    Private Function __scale(nodes As IEnumerable(Of Node), scale As SizeF) As Dictionary(Of Node, Point)
        Dim table As New Dictionary(Of Node, Point)

        For Each n As Node In nodes
            With n.Data.initialPostion.Point2D
                Call table.Add(n, New Point(.X * scale.Width, .Y * scale.Height))
            End With
        Next

        Return table
    End Function

    <Extension>
    Public Function GetBounds(graph As NetworkGraph) As RectangleF
        Dim points As Point() = graph _
            .nodes _
            .__scale(scale:=New SizeF(1, 1)) _
            .Values _
            .ToArray
        Dim rect = points.GetBounds
        Return rect
    End Function

    <Extension>
    Public Function AutoScaler(graph As NetworkGraph, frameSize As Size) As SizeF
        With graph.GetBounds
            Return New SizeF(
                frameSize.Width / .Width,
                frameSize.Height / .Height)
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
    ''' <param name="fontSizeFactor">这个参数值越小，字体会越大</param>
    ''' <param name="hullPolygonGroups">需要显示分组的多边形的分组的名称的列表，也可以是一个表达式max或者min，分别表示最大或者最小的分组</param>
    ''' <returns></returns>
    <ExportAPI("Draw.Image")>
    <Extension>
    Public Function DrawImage(net As NetworkGraph,
                              Optional canvasSize$ = "1024,1024",
                              Optional padding$ = g.DefaultPadding,
                              Optional styling As StyleMapper = Nothing,
                              Optional background$ = "white",
                              Optional defaultColor As Color = Nothing,
                              Optional displayId As Boolean = True,
                              Optional labelColorAsNodeColor As Boolean = False,
                              Optional nodeStroke$ = WhiteStroke,
                              Optional scale# = 1.2,
                              Optional minLinkWidth! = 2,
                              Optional radiusScale# = 1.25,
                              Optional minRadius# = 5,
                              Optional minFontSize! = 10,
                              Optional labelFontBase$ = CSSFont.Win7Normal,
                              Optional ByRef nodePoints As Dictionary(Of Node, PointF) = Nothing,
                              Optional fontSizeFactor# = 1.5,
                              Optional edgeDashTypes As Dictionary(Of String, DashStyle) = Nothing,
                              Optional getNodeLabel As Func(Of Node, String) = Nothing,
                              Optional hideDisconnectedNode As Boolean = False,
                              Optional throwEx As Boolean = True,
                              Optional hullPolygonGroups$ = Nothing) As GraphicsData

        Dim frameSize As Size = canvasSize.SizeParser  ' 所绘制的图像输出的尺寸大小
        Dim br As Brush
        Dim rect As Rectangle
        Dim cl As Color

        ' 1. 先将网络图形对象置于输出的图像的中心位置
        ' 2. 进行矢量图放大
        ' 3. 执行绘图操作

        ' 获取得到当前的这个网络对象相对于图像的中心点的位移值
        Dim scalePos As Dictionary(Of Node, PointF) = net _
            .nodes _
            .ToDictionary(Function(n) n,
                          Function(node)
                              Return node.Data.initialPostion.Point2D.PointF
                          End Function)
        Dim offset As Point = scalePos _
            .CentralOffsets(frameSize) _
            .ToPoint

        ' 进行位置偏移
        scalePos = scalePos.ToDictionary(Function(node) node.Key,
                                         Function(point)
                                             Return point.Value.OffSet2D(offset)
                                         End Function)
        ' 进行矢量放大
        Dim scalePoints = scalePos.Values.Enlarge(scale)

        With scalePos.Keys.AsList
            For i As Integer = 0 To .Count - 1
                scalePos(.Item(i)) = scalePoints(i)
            Next

            nodePoints = scalePos
        End With

        Call "Initialize gdi objects...".__INFO_ECHO

        Dim margin As Padding = CSS.Padding.TryParse(
            padding, New Padding With {
                .Bottom = 100,
                .Left = 100,
                .Right = 100,
                .Top = 100
            })
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

        ' 在这里不可以使用 <=，否则会导致等于最小值的时候出现无限循环的bug
        Dim minLinkWidthValue = minLinkWidth.AsDefault(Function(width) CInt(width) < minLinkWidth)
        Dim minFontSizeValue = minFontSize.AsDefault(Function(size) Val(size) < minFontSize)
        Dim minRadiusValue = minRadius.AsDefault(Function(r) Val(r) < minRadius)

        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)

                Call "Render network edges...".__INFO_ECHO

                ' 首先在这里绘制出网络的框架：将所有的边绘制出来
                For Each edge As Edge In net.edges
                    Dim n As Node = edge.U
                    Dim otherNode As Node = edge.V

                    cl = DefaultEdgeColor

                    If edge.Data.weight < 0.5 Then
                        cl = Color.LightGray
                    ElseIf edge.Data.weight < 0.75 Then
                        cl = Color.Blue
                    End If

                    Dim w! = CSng(5 * edge.Data.weight * scale) Or minLinkWidthValue
                    Dim lineColor As New Pen(cl, w)

                    With edge.Data!interaction_type
                        If Not .IsNothing AndAlso edgeDashTypes.ContainsKey(.ByRef) Then
                            lineColor.DashStyle = edgeDashTypes(.ByRef)
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

                defaultColor = If(defaultColor.IsEmpty, Color.Black, defaultColor)

                Dim pt As Point
                Dim labels As New List(Of (label As Label, anchor As Anchor, style As Font, color As Brush))

                Call "Render network nodes...".__INFO_ECHO

                ' 然后将网络之中的节点绘制出来，同时记录下节点的位置作为label text的锚点
                ' 最后通过退火算法计算出合适的节点标签文本的位置之后，再使用一个循环绘制出
                ' 所有的节点的标签文本

                ' if required hide disconnected nodes, then only the connected node in the network 
                ' Graph will be draw
                ' otherwise all of the nodes in target network graph will be draw onto the canvas.
                Dim connectedNodes = net.connectedNodes.AsDefault
                Dim drawPoints = net.nodes Or connectedNodes.When(hideDisconnectedNode)

                If Not hullPolygonGroups.StringEmpty Then
                    Dim hullPolygon As Index(Of String)
                    Dim groups = drawPoints _
                        .GroupBy(Function(n) n.Data.Properties!nodeType) _
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
                        If group.Count > 2 AndAlso group.Key.IsOneOfA(hullPolygon) Then
                            Dim positions = group _
                                .Select(Function(p) scalePos(p)) _
                                .JarvisMatch _
                                .Enlarge(1.25)
                            Dim color As Color = group _
                                .Select(Function(p)
                                            Return DirectCast(p.Data.Color, SolidBrush).Color
                                        End Function) _
                                .Average

                            Call g.DrawHullPolygon(positions, color)
                        End If
                    Next
                End If

                ' 在这里进行节点的绘制
                For Each n As Node In drawPoints
                    Dim r# = n.Data.radius

                    ' 当网络之中没有任何边的时候，r的值会是NAN
                    If r = 0# OrElse r.IsNaNImaginary Then
                        r = If(n.Data.Neighborhoods < 30, n.Data.Neighborhoods * 9, n.Data.Neighborhoods * 7)
                        r = If(r = 0, 9, r)
                    End If

                    r = (r * radiusScale) Or minRadiusValue

                    With DirectCast(New SolidBrush(defaultColor), Brush).AsDefault(n.NodeBrushAssert)
                        br = n.Data.Color Or .ByRef
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

                    If (Not invalidRegion) AndAlso displayId Then

                        Dim fontSize! = (baseFont.Size + r) / fontSizeFactor
                        Dim font As New Font(baseFont.Name, fontSize Or minFontSizeValue, FontStyle.Bold)
                        Dim label As New Label With {
                            .text = n.GetDisplayText
                        }

                        With g.MeasureString(label.text, font)
                            label.width = .Width
                            label.height = .Height
                        End With

                        labels += (label, New Anchor(rect), font, br)
                    End If
                Next

                If displayId Then

                    ' 使用退火算法计算出节点标签文本的位置
                    Call d3js _
                        .labeler _
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
                                br = New SolidBrush(DirectCast(br, SolidBrush).Color.Dark(0.005))
                            End If

                            With g.MeasureString(.label.text, .style)
                                rect = New Rectangle(
                                    label.label.X,
                                    label.label.Y,
                                    .Width,
                                    .Height
                                )
                            End With

                            Dim path As GraphicsPath = Imaging.GetStringPath(
                                .label.text,
                                g.DpiX,
                                rect.ToFloat,
                                .style,
                                StringFormat.GenericTypographic
                            )

                            Call g.DrawString(.label.text, .style, br, .label.X, .label.Y)

                            ' 绘制轮廓（描边）
                            ' Call g.FillPath(br, path)
                            ' Call g.DrawPath(New Pen(DirectCast(br, SolidBrush).Color.Dark(0.005), 4), path)
                        End With
                    Next
                End If
            End Sub

        Call "Start Render...".__INFO_ECHO

        Return g.GraphicsPlots(frameSize, margin, background, plotInternal)
    End Function
End Module
