Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports std = System.Math

''' <summary>
''' 网络节点的渲染类。从共享的 <see cref="NetworkRenderConfig"/> 配置对象读取参数，
''' 取代原先一长串的独立构造函数参数。
''' </summary>
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

    Sub New(graph As NetworkGraph, config As NetworkRenderConfig, scalePos As Dictionary(Of String, PointF))
        Me.graph = graph
        Me.scalePos = scalePos

        Dim dc = If(config.DefaultColor.StringEmpty, "skyblue", config.DefaultColor)
        defaultColor = dc.TranslateColor

        Dim env As CSSEnvirnment = CSSEnvirnment.Empty(config.Ppi)
        stroke = env.GetPen(Microsoft.VisualBasic.MIME.Html.CSS.Stroke.TryParse(config.NodeStroke), allowNull:=True)
        baseFont = env.GetFont(Microsoft.VisualBasic.MIME.Html.CSS.CSSFont.TryParse(config.LabelFontBase, New CSSFont With {
            .family = FontFace.MicrosoftYaHei,
            .size = 12,
            .style = FontStyle.Regular
        }))

        throwEx = config.ThrowEx
        getLabelPosition = config.GetLabelPosition
        labelWordWrapWidth = config.LabelWordWrapWidth
        nodeWidget = config.NodeWidget
        drawShape = config.ShapeRender
        drawNodeShape = config.DrawNodeShape

        If config.GetNodeLabel Is Nothing AndAlso config.DisplayId Then
            getDisplayLabel = Function(node) node.GetDisplayText
        ElseIf config.GetNodeLabel Is Nothing Then
            getDisplayLabel = Function(v) Nothing
        Else
            getDisplayLabel = config.GetNodeLabel
        End If

        If config.FontSize Is Nothing Then
            fontSizeValue = Function() 16.0!
        ElseIf config.FontSize Like GetType(Single) Then
            Dim fsize As Single = config.FontSize
            fontSizeValue = Function() fsize
        Else
            fontSizeValue = config.FontSize
        End If

        If config.NodeRadius Is Nothing Then
            ' check for node size data
            If graph.vertex.All(Function(v) v.data.size.IsNullOrEmpty) Then
                ' all nodes has unify size
                Dim frameSize As SizeF = PrinterDimension.SizeOf(config.CanvasSize)
                Dim min = std.Min(frameSize.Width, frameSize.Height) / 100
                radiusValue = Function() {min}
            Else
                ' use the node size
                radiusValue = Function(v)
                                   Return v.data.size _
                                      .Select(Function(d) CSng(d)) _
                                      .ToArray
                               End Function
            End If
        ElseIf config.NodeRadius Like GetType(Single) Then
            Dim radius As Single = config.NodeRadius
            radiusValue = Function() {radius}
        Else
            Dim func As Func(Of Node, Single) = config.NodeRadius
            radiusValue = Function(n) {func(n)}
        End If
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

        Call "Rendering nodes...".debug

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
                baseFont.Style
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
