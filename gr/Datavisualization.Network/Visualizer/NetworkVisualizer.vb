#Region "Microsoft.VisualBasic::c47845253e008ea4f6f1a7b9c9e6cff7, ..\sciBASIC#\gr\Datavisualization.Network\Visualizer\NetworkVisualizer.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Styling
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime

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
    Public Property DefaultEdgeColor As Color = Color.FromArgb(131, 131, 131)

    <Extension>
    Public Function GetDisplayText(n As Node) As String
        If n.Data Is Nothing OrElse (n.Data.origID.StringEmpty AndAlso n.Data.label.StringEmpty) Then
            Return n.ID
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
    Public Function CentralOffsets(nodes As Dictionary(Of Node, Point), size As Size) As PointF
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
    ''' 假若属性是空值的话，在绘图之前可以调用<see cref="ApplyAnalysis"/>拓展方法进行一些分析
    ''' </summary>
    ''' <param name="net"></param>
    ''' <param name="canvasSize">画布的大小</param>
    ''' <param name="padding">上下左右的边距分别为多少？</param>
    ''' <param name="background">背景色或者背景图片的文件路径</param>
    ''' <param name="defaultColor"></param>
    ''' <param name="nodePoints">如果还需要获取得到节点的绘图位置的话，则可以使用这个可选参数来获取返回</param>
    ''' <param name="fontSizeFactor">这个参数值越小，字体会越大</param>
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
                              Optional labelFontBase$ = CSSFont.Win7Normal,
                              Optional ByRef nodePoints As Dictionary(Of Node, Point) = Nothing,
                              Optional fontSizeFactor# = 1.5,
                              Optional edgeDashTypes As Dictionary(Of String, DashStyle) = Nothing,
                              Optional getNodeLabel As Func(Of Node, String) = Nothing) As GraphicsData

        Dim frameSize As Size = canvasSize.SizeParser  ' 所绘制的图像输出的尺寸大小
        Dim br As Brush
        Dim rect As Rectangle
        Dim cl As Color

        ' 1. 先将网络图形对象置于输出的图像的中心位置
        ' 2. 进行矢量图放大
        ' 3. 执行绘图操作

        ' 获取得到当前的这个网络对象相对于图像的中心点的位移值
        Dim scalePos As Dictionary(Of Node, Point) = net _
            .nodes _
            .ToDictionary(Function(n) n,
                          Function(node)
                              Return node.Data.initialPostion.Point2D
                          End Function)
        Dim offset As Point = scalePos.CentralOffsets(frameSize).ToPoint

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

        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)

                Call "Render network edges...".__INFO_ECHO

                For Each edge As Edge In net.edges
                    Dim n As Node = edge.Source
                    Dim otherNode As Node = edge.Target

                    cl = DefaultEdgeColor

                    If edge.Data.weight < 0.5 Then
                        cl = Color.Gray
                    ElseIf edge.Data.weight < 0.75 Then
                        cl = Color.Blue
                    End If

                    Dim w As Integer = 5 * edge.Data.weight
                    w = If(w < 1.5, 1.5, w)
                    Dim lineColor As New Pen(cl, w)

                    With edge.Data!interaction_type
                        If edgeDashTypes.ContainsKey(.ref) Then
                            lineColor.DashStyle = edgeDashTypes(.ref)
                        End If
                    End With

                    ' 在这里绘制的是节点之间相连接的边
                    Dim a = scalePos(n), b = scalePos(otherNode)

                    Call g.DrawLine(lineColor, a, b)
                Next

                defaultColor = If(defaultColor.IsEmpty, Color.Black, defaultColor)

                Dim pt As Point

                Call "Render network nodes...".__INFO_ECHO

                For Each n As Node In net.nodes  ' 在这里进行节点的绘制
                    Dim r# = n.Data.radius

                    ' 当网络之中没有任何边的时候，r的值会是NAN
                    If r = 0# OrElse r.IsNaNImaginary Then
                        r = If(n.Data.Neighborhoods < 30, n.Data.Neighborhoods * 9, n.Data.Neighborhoods * 7)
                        r = If(r = 0, 9, r)
                    End If

                    br = If(n.Data.Color Is Nothing, New SolidBrush(defaultColor), n.Data.Color)
                    pt = scalePos(n)
                    With pt
                        pt = New Point(.X - r / 2, .Y - r / 2)
                    End With
                    rect = New Rectangle(pt, New Size(r, r))

                    Call g.FillPie(br, rect, 0, 360)
                    Call g.DrawEllipse(stroke, rect)

                    If displayId Then

                        Dim font As New Font(baseFont.Name, (baseFont.Size + r) / fontSizeFactor)
                        Dim s As String = n.GetDisplayText
                        Dim size As SizeF = g.MeasureString(s, font)
                        Dim sloci As New Point With {
                            .X = pt.X + r * 1.25,
                            .Y = pt.Y - (r - size.Height) / 2
                        }

                        If sloci.X < margin.Left Then
                            sloci = New Point(margin.Left, sloci.Y)
                        End If
                        If sloci.Y + size.Height > frameSize.Height - margin.Bottom Then
                            sloci = New Point(sloci.X, frameSize.Height - margin.Bottom - size.Height)
                        End If
                        If sloci.X + size.Width > frameSize.Width - margin.Right Then
                            sloci = New Point(frameSize.Width - margin.Right - size.Width, sloci.Y)
                        End If

                        If Not labelColorAsNodeColor Then
                            br = Brushes.Black
                        End If

                        Call g.DrawString(s, font, br, sloci)

                    End If
                Next
            End Sub

        Call "Start Render...".__INFO_ECHO

        Return GraphicsPlots(frameSize, margin, background, plotInternal)
    End Function
End Module
