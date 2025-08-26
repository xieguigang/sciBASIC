#Region "Microsoft.VisualBasic::db1e4540e324e698d242e2dc818304ad, gr\network-visualization\Visualizer\Render\EdgeRendering.vb"

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

    '   Total Lines: 216
    '    Code Lines: 163 (75.46%)
    ' Comment Lines: 24 (11.11%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 29 (13.43%)
    '     File Size: 8.27 KB


    ' Class EdgeRendering
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: drawEdges, renderEdge, rendering
    ' 
    ' Class LineSegmentRender
    ' 
    '     Function: Render
    ' 
    ' /********************************************************************************/

#End Region

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

Friend Class EdgeRendering

    ReadOnly linkWidth As Func(Of Edge, Single)
    ReadOnly edgeDashTypes As Dictionary(Of String, DashStyle)
    ReadOnly scalePos As Dictionary(Of String, PointF)
    ReadOnly throwEx As Boolean
    ReadOnly edgeShadowDistance As Single
    ReadOnly defaultEdgeColor As Color
    ReadOnly drawEdgeBends As Boolean
    ReadOnly drawEdgeDirection As Boolean

    Sub New(linkWidth As Func(Of Edge, Single),
            edgeDashTypes As Dictionary(Of String, DashStyle),
            scalePos As Dictionary(Of String, PointF),
            throwEx As Boolean,
            edgeShadowDistance As Single,
            defaultEdgeColor As Color,
            drawEdgeBends As Boolean,
            drawEdgeDirection As Boolean)

        Me.linkWidth = linkWidth
        Me.edgeDashTypes = edgeDashTypes
        Me.scalePos = scalePos
        Me.throwEx = throwEx
        Me.edgeShadowDistance = edgeShadowDistance
        Me.defaultEdgeColor = defaultEdgeColor
        Me.drawEdgeBends = drawEdgeBends
        Me.drawEdgeDirection = drawEdgeDirection
    End Sub

    ''' <summary>
    ''' 这个函数会将edge作为一个layout的shape返回用于标签的布局计算
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="graph"></param>
    ''' <returns></returns>
    Public Iterator Function drawEdges(g As IGraphics, graph As NetworkGraph) As IEnumerable(Of LayoutLabel)
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
            lineColor = New Pen(defaultEdgeColor, w)
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

            If throwEx Then
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
            .drawDir = drawEdgeDirection,
            .edgeShadowColor = edgeShadowColor,
            .edgeShadowDistance = edgeShadowDistance,
            .lineColor = lineColor
        }
        Dim bends As XYMetaHandle() = edge.data.bends.SafeQuery.ToArray
        Dim isNan As Boolean = bends.Any(Function(bend) bend.isNaN)

        If (Not isNan) AndAlso drawEdgeBends AndAlso Not bends.IsNullOrEmpty Then
            If bends.Length <> edge.data.bends.Length Then
                Call $"{edge.ID} removes {edge.data.bends.Length - bends.Length} bends points.".debug
            End If

            If bends.Length = 1 Then
                Yield draw.Render(g, {a, b})
            Else
                Dim segmentTuples = bends.SlideWindows(2).ToArray

                For i As Integer = 0 To segmentTuples.Length - 1
                    Dim line As SlideWindow(Of XYMetaHandle) = segmentTuples(i)
                    Dim pta = line(Scan0).GetPoint(a.X, a.Y, b.X, b.Y)
                    Dim ptb = line(1).GetPoint(a.X, a.Y, b.X, b.Y)

                    draw.drawDir = If(i = segmentTuples.Length - 1, drawEdgeDirection, False)

                    Yield draw.Render(g, {pta, ptb})
                Next
            End If
        Else
            If drawEdgeDirection Then
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
