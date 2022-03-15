#Region "Microsoft.VisualBasic::1654a2f12652611d4e1c910916b6e7f3, sciBASIC#\gr\network-visualization\Visualizer\Render\NodeRendering.vb"

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

    '   Total Lines: 196
    '    Code Lines: 171
    ' Comment Lines: 4
    '   Blank Lines: 21
    '     File Size: 7.33 KB


    ' Class NodeRendering
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: RenderingVertexNodes, renderNode
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
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports stdNum = System.Math

Friend Class NodeRendering

    ReadOnly radiusValue As Func(Of Node, Single),
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
        nodeWidget As Func(Of IGraphics, PointF, Double, Node, RectangleF)

    Sub New(radiusValue As Func(Of Node, Single),
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
            nodeWidget As Func(Of IGraphics, PointF, Double, Node, RectangleF))

        Me.radiusValue = radiusValue
        Me.fontSizeValue = fontSizeValue
        Me.defaultColor = defaultColor
        Me.stroke = stroke
        Me.baseFont = baseFont
        Me.scalePos = scalePos
        Me.throwEx = throwEx
        Me.getDisplayLabel = getDisplayLabel
        Me.drawNodeShape = drawNodeShape
        Me.getLabelPosition = getLabelPosition
        Me.labelWordWrapWidth = labelWordWrapWidth
        Me.nodeWidget = nodeWidget
    End Sub

    Public Iterator Function RenderingVertexNodes(g As IGraphics, drawPoints As Node()) As IEnumerable(Of LayoutLabel)
        Call "Rendering nodes...".__DEBUG_ECHO

        For Each n As Node In drawPoints
            For Each label As LayoutLabel In renderNode(n, g)
                Yield label
            Next
        Next
    End Function

    Private Iterator Function renderNode(n As Node, g As IGraphics) As IEnumerable(Of LayoutLabel)
        Dim r# = radiusValue(n)
        Dim center As PointF = scalePos(n.label)
        Dim invalidRegion As Boolean = False
        Dim pt As Point
        Dim br As Brush
        Dim rect As RectangleF

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

