#Region "Microsoft.VisualBasic::1a0d6719b48db0175c8fd3709cfe4d21, gr\network-visualization\Visualizer\MingleRender\MingleRender.vb"

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

    '   Total Lines: 190
    '    Code Lines: 145
    ' Comment Lines: 21
    '   Blank Lines: 24
    '     File Size: 7.59 KB


    ' Class MingleRender
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: adjustPosition
    ' 
    '     Sub: renderBezier, renderLine, renderQuadratic
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.EdgeBundling.Mingle
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports number = System.Double
Imports stdNum = System.Math

Public Class MingleRender

    ReadOnly options As RenderOptions
    ReadOnly ctx As RenderContext

    Sub New(options As RenderOptions, ctx As RenderContext)
        Me.ctx = ctx
        Me.options = options
    End Sub

    ' do render for each node
    ' graph.each(Function(node) {
    '    var edges = node.unbundleEdges(delta);
    '    Bundler.graph['render' + type](ctx, edges, {
    '      curviness: curviness,
    '      delta: delta,
    '      margin: margin
    '    });
    '  });    

    Public Sub renderLine(edges As PosItem()())
        Dim lineWidth = options.lineWidth
        Dim fillStyle = options.fillStyle Or "gray".AsDefault
        Dim pos As Double()

        ctx.fillStyle = fillStyle
        ctx.lineWidth = lineWidth

        For Each e As PosItem() In edges
            ctx.beginPath()

            For j As Integer = 0 To e.Length - 1
                pos = e(j).unbundledPos

                If (j = 0) Then
                    ctx.moveTo(pos(0), pos(1))
                Else
                    ctx.lineTo(pos(0), pos(1))
                End If
            Next

            ctx.stroke()
            ctx.closePath()
        Next
    End Sub

    Public Function adjustPosition(id As String, posItem As PosItem, pos As Vector, margin As number, delta As number) As Vector
        Dim nodeArray = DirectCast(posItem.node.data, MingleNodeData).nodeArray,
            epsilon = 1,
            nodeLength As number
        Dim index As Integer
        Dim lengthBefore As number,
            lengthAfter As number
        Dim node As Node

        If Not nodeArray.IsNullOrEmpty Then
            nodeLength = nodeArray.Length
            index = Integer.MinValue
            lengthBefore = 0
            lengthAfter = 0
            For k As Integer = 0 To nodeLength - 1
                node = nodeArray(k)
                If (node.ID = id) Then
                    index = k
                End If
                If (k < index) Then
                    lengthBefore += DirectCast(node.data, MingleNodeData).mass + margin
                ElseIf (k > index) Then
                    lengthAfter += DirectCast(node.data, MingleNodeData).mass + margin
                End If
            Next
            ' remove -margin to get the line weight into account.
            ' pos = $add(pos, $mult((lengthBefore - (lengthBefore + lengthAfter) / 2) * -margin, posItem.normal));
            pos = pos + (posItem.normal * (lengthBefore - (lengthBefore + lengthAfter) / 2) * stdNum.Min(epsilon, delta))
        End If

        Return pos
    End Function

    Public Sub renderBezier(edges As PosItem()())
        Dim pct = options.curviness
        Dim midpoint As number(), c1 As number(), c2 As number(), start As number(), [end] As number()
        Dim e As PosItem()

        For i As Integer = 0 To edges.Length - 1
            e = edges(i)
            start = e(0).unbundledPos
            ctx.strokeStyle = If(e(0).node.data.color Is Nothing, ctx.strokeStyle, DirectCast(e(0).node.data.color, SolidBrush).Color.ToHtmlColor)
            ctx.lineWidth = If(e(0).node.data.mass = 0, 1, e(0).node.data.mass)
            midpoint = e((e.Length - 1) / 2).unbundledPos
            If (e.Length > 3) Then
                c1 = e(1).unbundledPos
                c2 = e((e.Length - 1) / 2 - 1).unbundledPos
                [end] = lerp(midpoint, c2, 1 - pct)
                ctx.beginPath()
                ctx.moveTo(start(0), start(1))
                ctx.bezierCurveTo(c1(0), c1(1), c2(0), c2(1), [end](0), [end](1))
                c1 = e((e.Length - 1) / 2 + 1).unbundledPos
                c2 = e(e.Length - 2).unbundledPos
                [end] = e(e.Length - 1).unbundledPos
                If (1 - pct) Then
                    ' line to midpoint + pct of something
                    start = lerp(midpoint, c1, 1 - pct)
                    ctx.lineTo(start(0), start(1))
                End If
                ctx.bezierCurveTo(c1(0), c1(1), c2(0), c2(1), [end](0), [end](1))
                ctx.stroke()
                ctx.closePath()
            Else
                ctx.beginPath()
                ctx.moveTo(start(0), start(1))
                [end] = e(e.Length - 1).unbundledPos
                ctx.lineTo([end](0), [end](1))
            End If
        Next
    End Sub

    Public Sub renderQuadratic(edges As PosItem()())
        Dim lineWidth = If(options.lineWidth = 0, 1, options.lineWidth),
            fillStyle = options.fillStyle Or "gray".AsDefault,
            margin = options.margin * options.delta,
       n As Integer, e As PosItem(), pos As Vector, pos0 As number(), pos1 As number(), pos2 As number(), pos3 As number(),
            midPos, quadStart As number(),
          posStart As number(), nodeStart As Node, PosItem As PosItem


        Dim x As Double

        ctx.fillStyle = fillStyle
        ctx.lineWidth = lineWidth

        For i As Integer = 0 To edges.Length - 1
            e = edges(i)
            quadStart = Nothing
            posStart = Nothing
            nodeStart = e(0).node
            n = e.Length

            x = stdNum.Max(1, nodeStart.data.mass)

            ctx.lineWidth = If(x = 0, 1, x) * If(options.scale = 0, 1, options.scale)
            ctx.strokeStyle = If(nodeStart.data.color Is Nothing, ctx.strokeStyle, DirectCast(nodeStart.data.color, SolidBrush).Color.ToHtmlColor)
            ctx.globalAlpha = If(DirectCast(nodeStart.data, MingleNodeData).alpha = 0, 1, DirectCast(nodeStart.data, MingleNodeData).alpha)
            ctx.beginPath()

            For j As Integer = 0 To e.Length - 1
                PosItem = e(j)
                pos = PosItem.unbundledPos
                If j <> 0 Then
                    pos0 = If(posStart, e(j - 1).unbundledPos)
                    pos = adjustPosition(nodeStart.ID, PosItem, pos, margin, options.delta)
                    midPos = lerp(pos0, pos, 0.5)
                    pos1 = lerp(pos0, midPos, If(j = 1, 0, options.curviness))
                    pos3 = pos
                    pos2 = lerp(midPos, pos3, If(j = n - 1, 1, (1 - (options.curviness))))
                    'ctx.lineCap = 'butt';//'round';
                    'ctx.beginPath();
                    If Not quadStart.IsNullOrEmpty Then
                        'ctx.strokeStyle = 'black';
                        ctx.moveTo(quadStart(0), quadStart(1))
                        ctx.quadraticCurveTo(pos0(0), pos0(1), pos1(0), pos1(1))
                        'ctx.stroke();
                        'ctx.closePath();
                    End If
                    'ctx.beginPath();
                    'ctx.strokeStyle = 'red';
                    ctx.moveTo(pos1(0), pos1(1))
                    ctx.lineTo(pos2(0), pos2(1))
                    'ctx.stroke();
                    'ctx.closePath();
                    quadStart = pos2
                    posStart = pos
                End If
            Next
            ctx.stroke()
            ctx.closePath()
        Next
    End Sub

End Class
