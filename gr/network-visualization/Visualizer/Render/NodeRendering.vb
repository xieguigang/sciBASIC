#Region "Microsoft.VisualBasic::c60f6b61d6b23ed0e5e839b40a44aa40, gr\network-visualization\Visualizer\Render\NodeRendering.vb"

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

    '   Total Lines: 218
    '    Code Lines: 183 (83.94%)
    ' Comment Lines: 5 (2.29%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 30 (13.76%)
    '     File Size: 8.00 KB


    ' Class NodeRendering
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: DefaultDrawNodeShape, DrawDefaultCircle, RenderingVertexNodes, renderNode
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Friend Class NodeRendering

    ReadOnly radiusValue As Func(Of Node, Single()),
        fontSizeValue As Func(Of Node, Single),
        defaultColor As Color,
        stroke As Pen,
        baseFont As Font,
        scalePos As Dictionary(Of String, PointF),
        throwEx As Boolean,
        getDisplayLabel As Func(Of Node, String),
        getLabelPosition As GetLabelPosition,
        labelWordWrapWidth As Integer,
        nodeWidget As Func(Of IGraphics, PointF, Double, Node, RectangleF)

    Dim drawNodeShape As DrawNodeShape
    Dim drawShape As DrawShape
    Dim graph As NetworkGraph

    Sub New(graph As NetworkGraph,
            radiusValue As Func(Of Node, Single()),
            fontSizeValue As Func(Of Node, Single),
            defaultColor As Color,
            stroke As Pen,
            baseFont As Font,
            scalePos As Dictionary(Of String, PointF),
            throwEx As Boolean,
            getDisplayLabel As Func(Of Node, String),
            drawNodeShape As DrawNodeShape,
            drawShape As DrawShape,
            getLabelPosition As GetLabelPosition,
            labelWordWrapWidth As Integer,
            nodeWidget As Func(Of IGraphics, PointF, Double, Node, RectangleF))

        Me.radiusValue = radiusValue
        Me.fontSizeValue = fontSizeValue
        Me.defaultColor = defaultColor
        Me.stroke = stroke
        Me.graph = graph
        Me.baseFont = baseFont
        Me.scalePos = scalePos
        Me.drawShape = drawShape
        Me.throwEx = throwEx
        Me.getDisplayLabel = getDisplayLabel
        Me.drawNodeShape = drawNodeShape
        Me.getLabelPosition = getLabelPosition
        Me.labelWordWrapWidth = labelWordWrapWidth
        Me.nodeWidget = nodeWidget
    End Sub

    Private Function DefaultDrawNodeShape(id As String, g As IGraphics, brush As Brush, radius As Single(), center As PointF) As RectangleF
        Dim v As Node = graph.GetElementByID(id)
        Dim shape As String = If(v.data("shape"), "circle")
        Dim size As SizeF

        If radius.Length = 1 Then
            size = New SizeF(radius(0), radius(0))
        Else
            size = New SizeF(radius(0), radius(1))
        End If

        If drawShape Is Nothing Then
            ' draw circle by default
            Return DrawDefaultCircle(center, g, radius, brush, Nothing)
        Else
            center = New PointF(center.X - size.Width / 2, center.Y - size.Height / 2)
            drawShape(g, center, size, shape, brush, Nothing, radius.Average, Nothing, 1)
        End If

        Return New RectangleF(center, size)
    End Function

    Public Iterator Function RenderingVertexNodes(g As IGraphics, drawPoints As Node()) As IEnumerable(Of LayoutLabel)
        If drawNodeShape Is Nothing Then
            If drawPoints.Any(Function(v) v.data.HasProperty("shape")) Then
                drawNodeShape = AddressOf DefaultDrawNodeShape
            End If
        End If

        Call "Rendering nodes...".__DEBUG_ECHO

        For Each n As Node In drawPoints
            For Each label As LayoutLabel In renderNode(n, g)
                Yield label
            Next
        Next
    End Function

    Private Function DrawDefaultCircle(center As PointF, g As IGraphics, r As Single(), br As Brush, ByRef invalidRegion As Boolean) As RectangleF
        Dim pt As Point

        With center
            pt = New Point(.X - r(0) / 2, .Y - r(0) / 2)
        End With

        Dim rect As New RectangleF(pt, New Size(r(0), r(0)))

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
            Call g.DrawCircle(center, DirectCast(br, SolidBrush).Color, stroke, radius:=r(0))
        End If

        Return rect
    End Function

    Private Iterator Function renderNode(n As Node, g As IGraphics) As IEnumerable(Of LayoutLabel)
        Dim r As Single() = radiusValue(n)
        Dim center As PointF = scalePos(n.label)
        Dim invalidRegion As Boolean = False
        Dim br As Brush
        Dim rect As RectangleF

        With DirectCast(New SolidBrush(defaultColor), Brush).AsDefault(n.NodeBrushAssert)
            br = n.data.color Or .ByRef
        End With

        If drawNodeShape Is Nothing Then
            rect = DrawDefaultCircle(center, g, r, br, invalidRegion)
        Else
            rect = drawNodeShape(n.label, g, br, r, center)
        End If

        If Not nodeWidget Is Nothing Then
            Dim rectLayout As RectangleF = nodeWidget(g, center, r(0), n)

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
                .pinned = n.pinned
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
    End Function
End Class
