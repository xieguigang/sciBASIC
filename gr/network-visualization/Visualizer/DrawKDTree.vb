#Region "Microsoft.VisualBasic::065bc54c84c1c5ade719cc20a12641bd, gr\network-visualization\Visualizer\DrawKDTree.vb"

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

    '   Total Lines: 136
    '    Code Lines: 108
    ' Comment Lines: 4
    '   Blank Lines: 24
    '     File Size: 5.44 KB


    ' Class DrawKDTree
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Plot
    ' 
    '     Sub: PlotInternal, render
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConvexHull
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.MIME.Html.CSS

Public Class DrawKDTree : Inherits Plot

    ReadOnly tree As KdTree(Of Point2D)
    ReadOnly query As NamedValue(Of PointF)()
    ReadOnly k As Integer
    ReadOnly linePen As Pen

    Public Sub New(tree As KdTree(Of Point2D), query As NamedValue(Of PointF)(), k As Integer, theme As Theme)
        MyBase.New(theme)

        Me.tree = tree
        Me.query = query
        Me.k = k
        Me.linePen = Stroke.TryParse(theme.lineStroke).GDIObject
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Dim allPoints As Point2D() = tree.GetPoints.ToArray
        Dim xTicks As Double() = allPoints.Select(Function(p) p.X).CreateAxisTicks
        Dim yTicks As Double() = allPoints.Select(Function(p) p.Y).CreateAxisTicks
        Dim rect As Rectangle = canvas.PlotRegion
        Dim xscale = d3js.scale.linear.domain(values:=xTicks).range(values:=New Double() {rect.Left, rect.Right})
        Dim yscale = d3js.scale.linear.domain(values:=yTicks).range(values:=New Double() {rect.Top, rect.Bottom})
        Dim scaler As New DataScaler() With {
            .AxisTicks = (xTicks.AsVector, yTicks.AsVector),
            .region = rect,
            .X = xscale,
            .Y = yscale
        }

        Call render(g, scaler, root:=tree.rootNode)

        If Not query.IsNullOrEmpty Then
            For Each q As NamedValue(Of PointF) In query
                Dim pos As PointF = scaler.Translate(q.Value.X, q.Value.Y)
                Dim color As Pen = New Pen(q.Description.TranslateColor, 4)
                Dim point2 As PointF() = tree _
                    .nearest(New Point2D(q.Value), k) _
                    .Select(Function(knn)
                                Dim p = knn.node.data.PointF
                                p = scaler.Translate(p.X, p.Y)
                                Return p
                            End Function) _
                    .ToArray
                Dim poly = point2.JarvisMatch.Enlarge(1.125)

                Call g.FillPolygon(New SolidBrush(color.Color.Alpha(120)), poly)
                Call g.DrawCircle(pos, theme.pointSize, color, fill:=True)

                For Each knn In point2
                    Call g.DrawCircle(knn, theme.pointSize, color, fill:=False)
                Next
            Next
        End If
    End Sub

    Private Sub render(g As IGraphics, scaler As DataScaler, root As KdTreeNode(Of Point2D))
        Dim pos As PointF, pos2 As PointF
        Dim c As PointF

        pos = root.data.PointF
        pos = scaler.Translate(pos.X, pos.Y)

        Call g.DrawCircle(pos, theme.pointSize, Pens.LightGray, fill:=True)

        If Not root.left Is Nothing Then
            pos2 = root.left.data.PointF
            pos2 = scaler.Translate(pos2.X, pos2.Y)

            If root.left.dimension = 0 Then
                ' x -> y
                c = New PointF(pos2.X, pos.Y)
            Else
                ' y -> x
                c = New PointF(pos.X, pos2.Y)
            End If

            Call g.DrawLine(linePen, pos, c)
            Call g.DrawLine(linePen, pos2, c)

            Call render(g, scaler, root.left)
        End If

        If Not root.right Is Nothing Then
            pos2 = root.right.data.PointF
            pos2 = scaler.Translate(pos2.X, pos2.Y)

            If root.left.dimension = 0 Then
                ' x -> y
                c = New PointF(pos2.X, pos.Y)
            Else
                ' y -> x
                c = New PointF(pos.X, pos2.Y)
            End If

            Call g.DrawLine(linePen, pos, c)
            Call g.DrawLine(linePen, pos2, c)

            Call render(g, scaler, root.right)
        End If
    End Sub

    Public Overloads Shared Function Plot(tree As KdTree(Of Point2D),
                                          Optional query As NamedValue(Of PointF)() = Nothing,
                                          Optional k As Integer = 13,
                                          Optional size As String = "3600,2700",
                                          Optional padding As String = g.DefaultPadding,
                                          Optional bg$ = "white",
                                          Optional pointSize As Integer = 8,
                                          Optional line As String = "stroke: black; stroke-width: 1px; stroke-dash: dash;") As GraphicsData

        Dim theme As New Theme With {
            .padding = padding,
            .background = bg,
            .pointSize = pointSize,
            .lineStroke = line
        }

        Return New DrawKDTree(tree, query, k, theme).Plot(size)
    End Function

End Class
