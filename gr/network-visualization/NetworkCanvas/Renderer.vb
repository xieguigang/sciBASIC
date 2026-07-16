#Region "Microsoft.VisualBasic::40b6a511bfae8f090322218c5edc6403, gr\network-visualization\NetworkCanvas\Renderer.vb"

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

    ' Class Renderer
    ' 
    '     Properties: ClientRegion, Font, ShowLabels, ZeroFilter
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: (+2 Overloads) GraphToScreen, ScreenToGraph
    ' 
    '     Sub: Clear, DirectDraw, doEdgeDrawing, drawEdge, drawNode
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.SpringForce
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.SpringForce.Interfaces
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports std = System.Math

Public Class Renderer : Inherits AbstractRenderer
    Implements IGraphicsEngine

    ''' <summary>
    ''' Gets the graphics source
    ''' </summary>
    Protected graphicsProvider As Func(Of IGraphics)
    ''' <summary>
    ''' gets the graphics region for the projections: <see cref="GraphToScreen"/> and <see cref="ScreenToGraph"/>
    ''' </summary>
    Protected regionProvider As Func(Of Rectangle)

    ''' <summary>
    ''' 当前帧复用的 GDI+ 绘图接口（整帧仅取一次，避免逐图元锁）。
    ''' </summary>
    Protected frameCanvas As IGraphics

    ''' <summary>
    ''' 渲染视图状态（视口、悬停/选中、LOD、网格）。由 Canvas 在每帧绘制前下发。
    ''' </summary>
    Public Property View As New CanvasViewState Implements IGraphicsEngine.View

    Public ReadOnly Property ClientRegion As Rectangle
        Get
            Return regionProvider()
        End Get
    End Property

    Public Property ShowLabels As Boolean Implements IGraphicsEngine.ShowLabels
        Get
            Return If(View Is Nothing, False, View.ShowLabels)
        End Get
        Set(value As Boolean)
            If View IsNot Nothing Then
                View.ShowLabels = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' 高亮笔刷（悬停=橙，选中=红），用于节点描边环。
    ''' </summary>
    Protected Friend ReadOnly hoverBrush As New Microsoft.VisualBasic.Imaging.SolidBrush(System.Drawing.Color.Orange)
    Protected Friend ReadOnly selectBrush As New Microsoft.VisualBasic.Imaging.SolidBrush(System.Drawing.Color.Red)
    Private ReadOnly gridPen As New Microsoft.VisualBasic.Imaging.Pen(System.Drawing.Color.FromArgb(40, 150, 150, 150), 1)

    ''' <summary>
    ''' 这个构造函数会生成一些静态数据的缓存
    ''' </summary>
    ''' <param name="canvas"></param>
    ''' <param name="regionProvider"></param>
    ''' <param name="iForceDirected"></param>
    Public Sub New(canvas As Func(Of IGraphics), regionProvider As Func(Of Rectangle), iForceDirected As IForceDirected)
        MyBase.New(iForceDirected)

        Me.graphicsProvider = canvas
        Me.regionProvider = regionProvider

        ' using cache
        Dim ws As New Dictionary(Of Edge, Pen)
        Dim nr As New Dictionary(Of Node, Single)

        For Each edge As Edge In iForceDirected.graph.graphEdges
            If edge.data.style Is Nothing Then
                Dim w As Single = CSng(5.0! * edge.weight)
                w = If(w < 3.0!, 3.0!, w)
                Call ws.Add(edge, New Pen(System.Drawing.Color.LightGray, w))
            Else
                Call ws.Add(edge, edge.data.style)
            End If
        Next
        For Each n As Node In iForceDirected.graph.vertex
            Dim r As Single = If(n.data.size.IsNullOrEmpty, 0, n.data.size(0))

            If r = 0! Then
                r = If(n.data.neighborhoods < 30,
                    n.data.neighborhoods * 9,
                    n.data.neighborhoods * 7)
                r = If(r = 0, 20, r)

                n.data.size = {r}
            End If

            Call nr.Add(n, r)
        Next

        edgeStyles = ws
        radiushash = nr
    End Sub

    Public Property ZeroFilter As Boolean = True

    Public Overrides Sub DirectDraw()
        ' 整帧仅取一次绘图接口，移除逐图元 SyncLock（绘制只在 UI 线程进行，
        ' 与物理线程通过 Canvas_Paint 的 SyncLock fdgRenderer 互斥）。
        Me.frameCanvas = graphicsProvider()

        Call DrawGrid(Me.frameCanvas, regionProvider())
        forceDirected.EachEdge(AddressOf doEdgeDrawing)
        forceDirected.EachNode(Sub(node As Node, point As LayoutPoint) drawNode(node, point.position))

        Me.frameCanvas = Nothing
    End Sub

    Protected Sub doEdgeDrawing(edge As Edge, spring As Spring)
        If ZeroFilter Then
            If (edge.U.data.size(0) < 0.6 OrElse edge.V.data.size(0) < 0.6) Then
                Return
            ElseIf edge.U.data.size(0) > 500 OrElse edge.V.data.size(0) > 500 Then
                Return
            End If
        End If

        Call drawEdge(edge, spring.A.position, spring.B.position)
    End Sub

    Public Overrides Sub Clear()

    End Sub

    ''' <summary>
    ''' Projects the data model to our screen for display.（兼容旧逻辑：无缩放/平移）
    ''' </summary>
    Public Shared Function GraphToScreen(iPos As FDGVector2, rect As Rectangle) As Point
        Dim x As Integer = CInt(std.Truncate(iPos.x + (CSng(rect.Right - rect.Left) / 2.0F)))
        Dim y As Integer = CInt(std.Truncate(iPos.y + (CSng(rect.Bottom - rect.Top) / 2.0F)))
        Return New Point(x, y)
    End Function

    Public Shared Function GraphToScreen(iPos As Point, rect As Rectangle) As Point
        Dim x As Integer = CInt(std.Truncate(iPos.X + (CSng(rect.Right - rect.Left) / 2.0F)))
        Dim y As Integer = CInt(std.Truncate(iPos.Y + (CSng(rect.Bottom - rect.Top) / 2.0F)))
        Return New Point(x, y)
    End Function

    ''' <summary>
    ''' Projects the data model to our screen for display.（带视口变换）
    ''' </summary>
    Public Shared Function GraphToScreen(iPos As AbstractVector, rect As Rectangle, vp As Viewport) As PointF
        Return vp.ToScreen(iPos.x, iPos.y, rect)
    End Function

    ''' <summary>
    ''' 节点在屏幕上的显示半径（已含视口缩放）。
    ''' </summary>
    Public Shared Function NodeRadius(n As Node, vp As Viewport) As Single
        Dim baseR As Single = If(n.data.size.IsNullOrEmpty, 0, n.data.size(0))
        If baseR = 0 Then
            baseR = 20
        End If
        Return std.Max(0.5F, baseR * vp.zoom)
    End Function

    ''' <summary>
    ''' 节点缓存半径（图坐标，未缩放），供命中测试复用。
    ''' </summary>
    Public Function GetNodeRadius(n As Node) As Single
        Return radiushash(n)
    End Function

    ''' <summary>
    ''' Projects the client graphics data to the data model.
    ''' </summary>
    Public Function ScreenToGraph(iScreenPos As Point) As FDGVector2
        Dim rect = regionProvider()
        Dim vp = If(View Is Nothing, New Viewport, View.Viewport)
        Dim retVec As New FDGVector2()
        retVec.x = (CSng(iScreenPos.X) - vp.panX - CSng(rect.Width) / 2.0F) / vp.zoom
        retVec.y = (CSng(iScreenPos.Y) - vp.panY - CSng(rect.Height) / 2.0F) / vp.zoom
        Return retVec
    End Function

    ''' <summary>
    ''' 两端点是否都在视口外侧同一方向（可跳过绘制）。
    ''' </summary>
    Protected Shared Function IsOffscreen(p1 As PointF, p2 As PointF, rect As Rectangle) As Boolean
        Dim m As Single = 64.0F
        Dim w = rect.Width
        Dim h = rect.Height

        Dim bothLeft = p1.X < -m AndAlso p2.X < -m
        Dim bothRight = p1.X > w + m AndAlso p2.X > w + m
        Dim bothTop = p1.Y < -m AndAlso p2.Y < -m
        Dim bothBottom = p1.Y > h + m AndAlso p2.Y > h + m

        Return bothLeft OrElse bothRight OrElse bothTop OrElse bothBottom
    End Function

    ''' <summary>
    ''' 绘制随视口平移/缩放的背景网格。
    ''' </summary>
    Private Sub DrawGrid(canvas As IGraphics, rect As Rectangle)
        If View Is Nothing OrElse Not View.ShowGrid Then
            Return
        End If

        Dim vp = View.Viewport
        Dim step_ As Single = 50.0F * vp.zoom

        ' 避免网格过密
        If step_ < 8.0F Then
            Dim k As Integer = CInt(std.Ceiling(8.0F / step_))
            step_ *= k
        End If

        Dim offsetX As Single = vp.panX Mod step_
        Dim offsetY As Single = vp.panY Mod step_

        Dim x As Single = offsetX
        Do While x <= rect.Width
            canvas.DrawLine(gridPen, x, 0, x, rect.Height)
            x += step_
        Loop

        Dim y As Single = offsetY
        Do While y <= rect.Height
            canvas.DrawLine(gridPen, 0, y, rect.Width, y)
            y += step_
        Loop
    End Sub

    ''' <summary>
    ''' The edge drawing width cache
    ''' </summary>
    Protected edgeStyles As IReadOnlyDictionary(Of Edge, Pen)
    ''' <summary>
    ''' The node drawing radius cache
    ''' </summary>
    Protected radiushash As IReadOnlyDictionary(Of Node, Single)

    Protected Overrides Sub drawEdge(iEdge As Edge, iPosition1 As AbstractVector, iPosition2 As AbstractVector)
        Dim rect As Rectangle = regionProvider()
        Dim vp = If(View Is Nothing, New Viewport, View.Viewport)
        Dim pos1 As PointF = vp.ToScreen(iPosition1.x, iPosition1.y, rect)
        Dim pos2 As PointF = vp.ToScreen(iPosition2.x, iPosition2.y, rect)

        ' 视口裁剪：两端都在视口外侧同一方向则跳过
        If IsOffscreen(pos1, pos2, rect) Then
            Return
        End If

        Dim canvas As IGraphics = frameCanvas

        Try
            If View IsNot Nothing AndAlso View.IsHighlighted(iEdge.U) OrElse (View IsNot Nothing AndAlso View.IsHighlighted(iEdge.V)) Then
                Dim w As Single = edgeStyles(iEdge).Width + 1.5F
                Using hp As New Microsoft.VisualBasic.Imaging.Pen(System.Drawing.Color.Orange, w)
                    canvas.DrawLine(hp, pos1.X, pos1.Y, pos2.X, pos2.Y)
                End Using
            Else
                canvas.DrawLine(edgeStyles(iEdge), pos1.X, pos1.Y, pos2.X, pos2.Y)
            End If
        Catch ex As Exception

        End Try
    End Sub

    Public Property Font As Font = New Font(FontFace.SegoeUI, 6, FontStyle.Regular)

    Protected Overrides Sub drawNode(n As Node, iPosition As AbstractVector)
        Dim rect As Rectangle = regionProvider()
        Dim vp = If(View Is Nothing, New Viewport, View.Viewport)
        Dim pos As PointF = vp.ToScreen(iPosition.x, iPosition.y, rect)

        Dim r As Single = radiushash(n) * vp.zoom

        ' 视口裁剪：节点整体在视口外则跳过
        If pos.X < -r - 4 OrElse pos.X > rect.Width + r + 4 OrElse
           pos.Y < -r - 4 OrElse pos.Y > rect.Height + r + 4 Then
            Return
        End If

        Dim canvas As IGraphics = frameCanvas

        Try
            ' 悬停/选中高亮环
            If View IsNot Nothing AndAlso View.IsHighlighted(n) Then
                Dim rr As Single = r + 3.0F
                Dim ring = If(View.Hovered Is n AndAlso Not View.Selected.Contains(n), hoverBrush, selectBrush)
                canvas.FillPie(ring, pos.X - rr, pos.Y - rr, rr * 2.0F, rr * 2.0F, 0, 360)
            End If

            Dim pt As New Point(CInt(pos.X - r / 2), CInt(pos.Y - r / 2))
            Dim nrect As New Rectangle(pt, New Size(CInt(r), CInt(r)))
            canvas.FillPie(n.data.color, nrect, 0, 360)

            ' LOD：缩放过小或节点过多时隐藏标签
            If View IsNot Nothing AndAlso View.ShouldDrawLabels(forceDirected.graph.vertex.Count, vp.zoom) Then
                Dim labelText As String = n.data.label

                If Not String.IsNullOrEmpty(labelText) Then
                    Dim sz As SizeF = canvas.MeasureString(labelText, Font)
                    Dim cx As Single = pos.X - sz.Width / 2
                    Dim cy As Single = pos.Y - sz.Height / 2
                    canvas.DrawString(labelText, Font, Brushes.Black, cx, cy)
                End If
            End If
        Catch ex As Exception

        End Try
    End Sub
End Class
