Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.EdgeBundling
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports std = System.Math

''' <summary>
''' 网络边的渲染类。从共享的 <see cref="NetworkRenderConfig"/> 配置对象读取参数，
''' 取代原先一长串的独立构造函数参数。
''' </summary>
Friend Class EdgeRendering

    ReadOnly config As NetworkRenderConfig
    ReadOnly scalePos As Dictionary(Of String, PointF)
    ReadOnly graph As NetworkGraph
    ReadOnly linkWidth As Func(Of Edge, Single)
    ReadOnly edgeDashTypes As Dictionary(Of String, DashStyle)

    Sub New(config As NetworkRenderConfig, scalePos As Dictionary(Of String, PointF), graph As NetworkGraph)
        Me.config = config
        Me.scalePos = scalePos
        Me.graph = graph

        If config.EdgeDashTypes Is Nothing Then
            edgeDashTypes = New Dictionary(Of String, DashStyle)
        ElseIf config.EdgeDashTypes Like GetType(DashStyle) Then
            edgeDashTypes = graph.graphEdges _
                .ToDictionary(Function(e) e.ID,
                              Function(null)
                                  Return config.EdgeDashTypes.VB
                              End Function)
        Else
            edgeDashTypes = config.EdgeDashTypes
        End If

        If config.LinkWidth Is Nothing Then
            Dim minLinkWidthValue = config.MinLinkWidth.AsDefault(Function(width) CInt(width) < config.MinLinkWidth)
            linkWidth = Function(edge) CSng(5 * edge.weight * 2) Or minLinkWidthValue
        Else
            linkWidth = config.LinkWidth
        End If
    End Sub

    ''' <summary>
    ''' 这个函数会将edge作为一个layout的shape返回用于标签的布局计算
    ''' </summary>
    ''' <param name="g"></param>
    ''' <returns></returns>
    Public Iterator Function drawEdges(g As IGraphics) As IEnumerable(Of LayoutLabel)
        For Each edge As Edge In graph.graphEdges
            For Each label As LayoutLabel In renderEdge(edge, g)
                Yield label
            Next
        Next
    End Function

    Private Iterator Function renderEdge(edge As Edge, g As IGraphics) As IEnumerable(Of LayoutLabel)
        Dim n As Node = edge.U
        Dim otherNode As Node = edge.V
        Dim w! = linkWidth(edge)
        Dim lineColor As Pen

        If edge.data.style Is Nothing Then
            lineColor = New Pen(config.DefaultEdgeColor.TranslateColor, w)
        Else
            lineColor = edge.data.style
        End If

        With edge.data!interaction_type
            If Not .IsNothing AndAlso edgeDashTypes.ContainsKey(.ByRef) Then
                lineColor.DashStyle = edgeDashTypes(.ByRef)
            ElseIf edgeDashTypes.ContainsKey(edge.ID) Then
                lineColor.DashStyle = edgeDashTypes(edge.ID)
            End If
        End With

        ' 在这里绘制的是节点之间相连接的边
        Dim a As PointF = scalePos(n.label)
        Dim b As PointF = scalePos(otherNode.label)
        Dim edgeShadowColor As New Pen(Brushes.Gray) With {
            .Width = lineColor.Width,
            .DashStyle = lineColor.DashStyle
        }

        Try
            For Each label As LayoutLabel In rendering(edge, edgeShadowColor, lineColor, g, a, b)
                Yield label
            Next
        Catch ex As Exception
            Dim line As New Dictionary(Of String, String) From {
                {NameOf(a), $"[{a.X}, {a.Y}]"},
                {NameOf(b), $"[{b.X}, {b.Y}]"}
            }

            If config.ThrowEx Then
                Throw New Exception(line.GetJson, ex)
            Else
                Call $"Ignore of this invalid line range: {line.GetJson}".Warning
            End If
        End Try
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="edge"></param>
    ''' <param name="edgeShadowColor"></param>
    ''' <param name="lineColor"></param>
    ''' <param name="g"></param>
    ''' <param name="a">location of the node a</param>
    ''' <param name="b">location of the node b</param>
    ''' <returns></returns>
    Private Iterator Function rendering(edge As Edge,
                                        edgeShadowColor As Pen,
                                        lineColor As Pen,
                                        g As IGraphics,
                                        a As PointF,
                                        b As PointF) As IEnumerable(Of LayoutLabel)

        Dim draw As New LineSegmentRender With {
            .drawDir = config.DrawEdgeDirection,
            .edgeShadowColor = edgeShadowColor,
            .edgeShadowDistance = config.EdgeShadowDistance,
            .lineColor = lineColor
        }
        Dim bends As WayPointVector() = edge.data.bends.SafeQuery.ToArray
        Dim isNan As Boolean = bends.Any(Function(bend) bend.isNaN)

        If (Not isNan) AndAlso config.DrawEdgeBends AndAlso Not bends.IsNullOrEmpty Then
            If bends.Length <> edge.data.bends.Length Then
                Call $"{edge.ID} removes {edge.data.bends.Length - bends.Length} bends points.".debug
            End If

            If bends.Length = 1 Then
                Yield draw.Render(g, {a, b})
            Else
                Dim segmentTuples = bends.SlideWindows(2).ToArray

                For i As Integer = 0 To segmentTuples.Length - 1
                    Dim line As SlideWindow(Of WayPointVector) = segmentTuples(i)
                    Dim pta = line(Scan0).GetPoint(a.X, a.Y, b.X, b.Y)
                    Dim ptb = line(1).GetPoint(a.X, a.Y, b.X, b.Y)

                    draw.drawDir = If(i = segmentTuples.Length - 1, config.DrawEdgeDirection, False)

                    Yield draw.Render(g, {pta, ptb})
                Next
            End If
        Else
            If config.DrawEdgeDirection Then
                ' needs reduce the line length
                ' or the line arrow will be masked by the node shape
                Dim x1 = a.X
                Dim y1 = a.Y
                Dim x2 = b.X
                Dim y2 = b.Y
                Dim originalLength As Double = std.Sqrt((x2 - x1) ^ 2 + (y2 - y1) ^ 2)
                Dim shortenBy As Double = originalLength / 5
                Dim newX2 As Double = x2 - (x2 - x1) / originalLength * shortenBy
                Dim newY2 As Double = y2 - (y2 - y1) / originalLength * shortenBy

                b = New PointF(newX2, newY2)
            End If

            Yield draw.Render(g, {a, b})
        End If
    End Function
End Class

''' <summary>
''' draw a line segment
''' </summary>
Friend Class LineSegmentRender

    Friend drawDir As Boolean
    Friend edgeShadowDistance!
    Friend edgeShadowColor As Pen
    Friend lineColor As Pen

    Public Function Render(g As IGraphics, line As PointF()) As LayoutLabel
        If edgeShadowDistance <> 0 Then
            ' 绘制底层的阴影
            Dim pt1 = line(0).OffSet2D(edgeShadowDistance, edgeShadowDistance)
            Dim pt2 = line(1).OffSet2D(edgeShadowDistance, edgeShadowDistance)

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
            .label = New Label(Nothing, .anchor, New Size(std.Abs(line(Scan0).X - line(1).X), std.Abs(line(Scan0).Y - line(1).Y))) With {
                .pinned = True
            },
            .node = Nothing,
            .shapeRectangle = .label.rectangle,
            .style = Nothing
        }
    End Function
End Class
