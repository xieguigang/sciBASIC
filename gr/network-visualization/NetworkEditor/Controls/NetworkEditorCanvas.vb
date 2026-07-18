#Region "Microsoft.VisualBasic::bf80e3314dfb5207918c028b6cb93b32, gr\network-visualization\NetworkEditor\Controls\NetworkEditorCanvas.vb"

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

    '   Total Lines: 496
    '    Code Lines: 419 (84.48%)
    ' Comment Lines: 13 (2.62%)
    '    - Xml Docs: 23.08%
    ' 
    '   Blank Lines: 64 (12.90%)
    '     File Size: 19.77 KB


    '     Class NetworkEditorCanvas
    ' 
    '         Properties: LinkMode, State, ViewOffset, ViewScale
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: DistToSegment, EdgeAt, GetGraphPointAtClientCenter, GetNodeScreenRadius, GetViewRectGraph
    '                   GetWorldBounds, GraphToScreen, NodeAt, NormalizeRect, ScreenToGraph
    ' 
    '         Sub: CenterOnGraph, FitView, OnMouseDown, OnMouseMove, OnMouseUp
    '              OnMouseWheel, OnPaint, OnStateChanged, RaiseEventSelection, RaiseViewChanged
    '              ResetView
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports NetworkEditor.Models

Namespace NetworkEditor.Controls

    ''' <summary>
    ''' 网络图渲染与交互画布：视口变换（平移/缩放）+ GDI+ 渲染 + 点选/框选/拖拽
    ''' </summary>
    Public Class NetworkEditorCanvas : Inherits UserControl

        Private _state As EditorState = Nothing

        ' 视口变换
        Private _viewOffset As PointF = New PointF(0, 0)
        Private _viewScale As Single = 1.0F

        ' 交互状态
        Private isPanning As Boolean = False
        Private panStart As PointF
        Private panOffsetStart As PointF
        Private isDraggingNodes As Boolean = False
        Private dragStartGraph As PointF
        Private dragInit As Dictionary(Of Node, FDGVector2)
        Private isRubberBand As Boolean = False
        Private rubberStart As PointF
        Private rubberRect As Rectangle = Rectangle.Empty
        Private hoverNode As Node = Nothing
        Private _linkMode As Boolean = False
        Private linkSource As Node = Nothing

        Public Event ViewChanged As EventHandler

        Private Sub RaiseViewChanged()
            RaiseEvent ViewChanged(Me, EventArgs.Empty)
        End Sub

        ' 配色
        Private Shared ReadOnly CBg As Color = Color.FromArgb(&HFF1E2430)
        Private Shared ReadOnly CSelNode As Color = Color.FromArgb(&HFF36E2C2)
        Private Shared ReadOnly CSelEdge As Color = Color.FromArgb(&HFFFFB454)
        Private Shared ReadOnly CNodeStroke As Color = Color.FromArgb(&HFF0B0E14)
        Private Shared ReadOnly CText As Color = Color.FromArgb(&HFFE6EAF0)

        Public Sub New()
            Me.DoubleBuffered = True
            Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw, True)
            Me.BackColor = CBg
        End Sub

        Public Property State As EditorState
            Get
                Return _state
            End Get
            Set(value As EditorState)
                If _state IsNot Nothing Then
                    RemoveHandler _state.SelectionChanged, AddressOf OnStateChanged
                    RemoveHandler _state.GraphChanged, AddressOf OnStateChanged
                End If
                _state = value
                If _state IsNot Nothing Then
                    AddHandler _state.SelectionChanged, AddressOf OnStateChanged
                    AddHandler _state.GraphChanged, AddressOf OnStateChanged
                End If
                Me.Invalidate()
            End Set
        End Property

        Public Property LinkMode As Boolean
            Get
                Return _linkMode
            End Get
            Set(value As Boolean)
                _linkMode = value
                If Not value Then
                    linkSource = Nothing
                End If
                Me.Cursor = If(value, Cursors.Cross, Cursors.Default)
                Me.Invalidate()
            End Set
        End Property

        Public ReadOnly Property ViewOffset As PointF
            Get
                Return _viewOffset
            End Get
        End Property

        Public ReadOnly Property ViewScale As Single
            Get
                Return _viewScale
            End Get
        End Property

        Private Sub OnStateChanged(sender As Object, e As EventArgs)
            Me.Invalidate()
        End Sub

#Region "坐标变换"

        Public Function GraphToScreen(gx As Double, gy As Double) As PointF
            Return New PointF(gx * _viewScale + _viewOffset.X, gy * _viewScale + _viewOffset.Y)
        End Function

        Public Function ScreenToGraph(sp As PointF) As PointF
            Return New PointF((sp.X - _viewOffset.X) / _viewScale, (sp.Y - _viewOffset.Y) / _viewScale)
        End Function

        Public Function GetWorldBounds() As RectangleF
            If _state Is Nothing OrElse _state.Graph Is Nothing Then
                Return RectangleF.Empty
            End If
            Dim minX = Double.MaxValue, minY = Double.MaxValue, maxX = Double.MinValue, maxY = Double.MinValue
            Dim any As Boolean = False
            For Each n As Node In _state.Graph.vertex
                If n.data.initialPostion Is Nothing Then
                    Continue For
                End If
                any = True
                minX = Math.Min(minX, n.data.initialPostion.x)
                minY = Math.Min(minY, n.data.initialPostion.y)
                maxX = Math.Max(maxX, n.data.initialPostion.x)
                maxY = Math.Max(maxY, n.data.initialPostion.y)
            Next
            If Not any Then
                Return RectangleF.Empty
            End If
            Return New RectangleF(CSng(minX), CSng(minY), CSng(maxX - minX), CSng(maxY - minY))
        End Function

        Public Function GetViewRectGraph() As RectangleF
            Dim tl = ScreenToGraph(New PointF(0, 0))
            Dim br = ScreenToGraph(New PointF(ClientSize.Width, ClientSize.Height))
            Return New RectangleF(CSng(tl.X), CSng(tl.Y), CSng(br.X - tl.X), CSng(br.Y - tl.Y))
        End Function

        Public Sub CenterOnGraph(gx As Double, gy As Double)
            _viewOffset = New PointF(ClientSize.Width / 2.0F - CSng(gx) * _viewScale, ClientSize.Height / 2.0F - CSng(gy) * _viewScale)
            Me.Invalidate()
        End Sub

        Public Sub FitView()
            Dim b = GetWorldBounds()
            If b.IsEmpty Then
                _viewScale = 1.0F
                _viewOffset = New PointF(0, 0)
                Me.Invalidate()
                Return
            End If
            Dim pad As Single = 40.0F
            Dim sx = (ClientSize.Width - pad * 2) / Math.Max(1.0F, b.Width)
            Dim sy = (ClientSize.Height - pad * 2) / Math.Max(1.0F, b.Height)
            _viewScale = CSng(Math.Min(sx, sy))
            If _viewScale <= 0 Then
                _viewScale = 1.0F
            End If
            Dim cx = b.X + b.Width / 2.0F
            Dim cy = b.Y + b.Height / 2.0F
            _viewOffset = New PointF(ClientSize.Width / 2.0F - cx * _viewScale, ClientSize.Height / 2.0F - cy * _viewScale)
            Me.Invalidate()
        End Sub

        Public Sub ResetView()
            _viewScale = 1.0F
            _viewOffset = New PointF(0, 0)
            Me.Invalidate()
        End Sub

        Public Function GetGraphPointAtClientCenter() As PointF
            Return ScreenToGraph(New PointF(ClientSize.Width / 2.0F, ClientSize.Height / 2.0F))
        End Function

#End Region

#Region "命中检测"

        Private Function GetNodeScreenRadius(n As Node) As Single
            Dim baseR As Single = 8.0F
            If n.data.size IsNot Nothing AndAlso n.data.size.Length > 0 Then
                baseR = CSng(n.data.size(0))
            End If
            Return Math.Max(3.0F, baseR * _viewScale)
        End Function

        Private Function NodeAt(pt As PointF) As Node
            Dim arr = _state.Graph.vertex.ToArray()
            For i As Integer = arr.Length - 1 To 0 Step -1
                Dim n = arr(i)
                If n.data.initialPostion Is Nothing Then
                    Continue For
                End If
                Dim c = GraphToScreen(n.data.initialPostion.x, n.data.initialPostion.y)
                Dim r = GetNodeScreenRadius(n) + 2.0F
                Dim dx = pt.X - c.X
                Dim dy = pt.Y - c.Y
                If dx * dx + dy * dy <= r * r Then
                    Return n
                End If
            Next
            Return Nothing
        End Function

        Private Function EdgeAt(pt As PointF) As Edge
            For Each ed As Edge In _state.Graph.graphEdges
                If ed.U.data.initialPostion Is Nothing OrElse ed.V.data.initialPostion Is Nothing Then
                    Continue For
                End If
                Dim a = GraphToScreen(ed.U.data.initialPostion.x, ed.U.data.initialPostion.y)
                Dim b = GraphToScreen(ed.V.data.initialPostion.x, ed.V.data.initialPostion.y)
                If DistToSegment(pt, a, b) < 4.0F Then
                    Return ed
                End If
            Next
            Return Nothing
        End Function

        Private Function DistToSegment(p As PointF, a As PointF, b As PointF) As Single
            Dim dx = b.X - a.X
            Dim dy = b.Y - a.Y
            Dim len2 = dx * dx + dy * dy
            If len2 = 0 Then
                Return CSng(Math.Sqrt((p.X - a.X) ^ 2 + (p.Y - a.Y) ^ 2))
            End If
            Dim t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / len2
            t = Math.Max(0, Math.Min(1, t))
            Dim px = a.X + t * dx
            Dim py = a.Y + t * dy
            Return CSng(Math.Sqrt((p.X - px) ^ 2 + (p.Y - py) ^ 2))
        End Function

#End Region

#Region "鼠标交互"

        Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
            MyBase.OnMouseDown(e)
            If _state Is Nothing OrElse _state.Graph Is Nothing Then
                Return
            End If

            If e.Button = MouseButtons.Middle OrElse e.Button = MouseButtons.Right Then
                isPanning = True
                panStart = e.Location
                panOffsetStart = _viewOffset
                Me.Cursor = Cursors.SizeAll
                Return
            End If

            If e.Button = MouseButtons.Left Then
                If _linkMode Then
                    Dim n = NodeAt(e.Location)
                    If n IsNot Nothing Then
                        If linkSource Is Nothing Then
                            linkSource = n
                            Me.Invalidate()
                        ElseIf linkSource IsNot n Then
                            Dim ed = _state.Graph.CreateEdge(linkSource, n)
                            If ed IsNot Nothing Then
                                _state.RaiseGraphChanged()
                            End If
                            linkSource = Nothing
                            Me.Invalidate()
                        End If
                    End If
                    Return
                End If

                Dim hitNode = NodeAt(e.Location)
                If hitNode IsNot Nothing Then
                    Dim additive = (Control.ModifierKeys And Keys.Control) = Keys.Control OrElse (Control.ModifierKeys And Keys.Shift) = Keys.Shift
                    If additive Then
                        _state.ToggleNode(hitNode)
                    ElseIf Not _state.SelectedNodes.Contains(hitNode) Then
                        _state.SelectNode(hitNode)
                    End If

                    isDraggingNodes = True
                    dragStartGraph = ScreenToGraph(e.Location)
                    dragInit = New Dictionary(Of Node, FDGVector2)
                    For Each sel As Node In _state.SelectedNodes
                        If sel.data.initialPostion Is Nothing Then
                            sel.data.initialPostion = New FDGVector2(0, 0)
                        End If
                        dragInit(sel) = New FDGVector2(sel.data.initialPostion.x, sel.data.initialPostion.y)
                    Next
                    Return
                End If

                Dim hitEdge = EdgeAt(e.Location)
                If hitEdge IsNot Nothing Then
                    _state.SelectEdge(hitEdge)
                    Return
                End If

                ' 空白处：开始框选
                isRubberBand = True
                rubberStart = e.Location
                rubberRect = Rectangle.Empty
            End If
        End Sub

        Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
            MyBase.OnMouseMove(e)
            If _state Is Nothing OrElse _state.Graph Is Nothing Then
                Return
            End If

            If isPanning Then
                _viewOffset = New PointF(panOffsetStart.X + (e.Location.X - panStart.X), panOffsetStart.Y + (e.Location.Y - panStart.Y))
                Me.Invalidate()
                Return
            End If

            If isDraggingNodes Then
                Dim cur = ScreenToGraph(e.Location)
                Dim dx = cur.X - dragStartGraph.X
                Dim dy = cur.Y - dragStartGraph.Y
                For Each kv In dragInit
                    Dim n = kv.Key
                    Dim init = kv.Value
                    n.data.initialPostion = New FDGVector2(init.x + dx, init.y + dy)
                Next
                Me.Invalidate()
                Return
            End If

            If isRubberBand Then
                rubberRect = NormalizeRect(rubberStart, e.Location)
                Me.Invalidate()
                Return
            End If

            ' 悬停高亮
            Dim h = NodeAt(e.Location)
            If h IsNot hoverNode Then
                hoverNode = h
                Me.Invalidate()
            End If
            Me.Cursor = If(_linkMode, Cursors.Cross, If(h IsNot Nothing, Cursors.Hand, Cursors.Default))
        End Sub

        Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
            MyBase.OnMouseUp(e)
            If isRubberBand Then
                Dim additive = (Control.ModifierKeys And Keys.Control) = Keys.Control OrElse (Control.ModifierKeys And Keys.Shift) = Keys.Shift
                If Not additive Then
                    _state.SelectedNodes.Clear()
                End If
                For Each n As Node In _state.Graph.vertex
                    If n.data.initialPostion Is Nothing Then
                        Continue For
                    End If
                    Dim c = GraphToScreen(n.data.initialPostion.x, n.data.initialPostion.y)
                    If rubberRect.Contains(CInt(c.X), CInt(c.Y)) Then
                        If Not _state.SelectedNodes.Contains(n) Then
                            _state.SelectedNodes.Add(n)
                        End If
                    End If
                Next
                _state.SelectedEdge = Nothing
                isRubberBand = False
                rubberRect = Rectangle.Empty
                RaiseEventSelection()
                Me.Invalidate()
                Return
            End If

            isPanning = False
            isDraggingNodes = False
            If Me.Cursor = Cursors.SizeAll Then
                Me.Cursor = Cursors.Default
            End If
        End Sub

        Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
            MyBase.OnMouseWheel(e)
            If _state Is Nothing Then
                Return
            End If
            Dim before = ScreenToGraph(e.Location)
            Dim factor As Single = If(e.Delta > 0, 1.1F, 1.0F / 1.1F)
            _viewScale = CSng(Math.Max(0.05, Math.Min(20.0, _viewScale * factor)))
            ' 保持光标下图形点不动
            _viewOffset = New PointF(e.Location.X - before.X * _viewScale, e.Location.Y - before.Y * _viewScale)
            Me.Invalidate()
        End Sub

        Private Function NormalizeRect(a As PointF, b As PointF) As Rectangle
            Dim x = Math.Min(a.X, b.X)
            Dim y = Math.Min(a.Y, b.Y)
            Dim w = Math.Abs(a.X - b.X)
            Dim h = Math.Abs(a.Y - b.Y)
            Return New Rectangle(CInt(x), CInt(y), CInt(w), CInt(h))
        End Function

        Private Sub RaiseEventSelection()
            If _state IsNot Nothing Then
                _state.RaiseSelectionChanged()
            End If
        End Sub

#End Region

#Region "渲染"

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)
            Dim g = e.Graphics
            g.SmoothingMode = SmoothingMode.HighQuality
            g.Clear(CBg)

            If _state Is Nothing OrElse _state.Graph Is Nothing Then
                Return
            End If

            ' 边
            For Each ed As Edge In _state.Graph.graphEdges
                If ed.U.data.initialPostion Is Nothing OrElse ed.V.data.initialPostion Is Nothing Then
                    Continue For
                End If
                Dim p1 = GraphToScreen(ed.U.data.initialPostion.x, ed.U.data.initialPostion.y)
                Dim p2 = GraphToScreen(ed.V.data.initialPostion.x, ed.V.data.initialPostion.y)
                Dim selected = (ed Is _state.SelectedEdge)
                Dim col = If(selected, CSelEdge, _state.GetEdgeColor(ed))
                Dim w = 1.0F + CSng(Math.Min(4.0, Math.Abs(ed.weight)))
                If selected Then
                    w = 3.0F
                End If
                Using pen = New Pen(col, w)
                    g.DrawLine(pen, p1, p2)
                End Using
            Next

            ' 节点
            For Each n As Node In _state.Graph.vertex
                If n.data.initialPostion Is Nothing Then
                    Continue For
                End If
                Dim c = GraphToScreen(n.data.initialPostion.x, n.data.initialPostion.y)
                Dim r = GetNodeScreenRadius(n)
                Dim selected = _state.SelectedNodes.Contains(n)

                If selected Then
                    Using glow = New SolidBrush(Color.FromArgb(60, &H36, &HE2, &HC2))
                        g.FillEllipse(glow, c.X - r - 4, c.Y - r - 4, (r + 4) * 2, (r + 4) * 2)
                    End Using
                End If

                Using fill = New SolidBrush(_state.GetNodeColor(n))
                    g.FillEllipse(fill, c.X - r, c.Y - r, r * 2, r * 2)
                End Using

                Using stroke = New Pen(If(selected, CSelNode, CNodeStroke), If(selected, 2.0F, 1.0F))
                    g.DrawEllipse(stroke, c.X - r, c.Y - r, r * 2, r * 2)
                End Using

                If _viewScale >= 0.6F OrElse selected OrElse (n Is hoverNode) Then
                    Dim label = If(String.IsNullOrEmpty(n.data.label), n.label, n.data.label)
                    Using br = New SolidBrush(CText)
                        Using fnt = New Font("Segoe UI", 9)
                            g.DrawString(label, fnt, br, c.X + r + 2, c.Y - 6)
                        End Using
                    End Using
                End If
            Next

            ' 框选
            If isRubberBand AndAlso Not rubberRect.IsEmpty Then
                Using pen = New Pen(CSelNode, 1.0F)
                    pen.DashStyle = DashStyle.Dash
                    g.DrawRectangle(pen, rubberRect)
                End Using
            End If

            ' 连线模式源节点高亮
            If _linkMode AndAlso linkSource IsNot Nothing AndAlso linkSource.data.initialPostion IsNot Nothing Then
                Dim c = GraphToScreen(linkSource.data.initialPostion.x, linkSource.data.initialPostion.y)
                Dim r = GetNodeScreenRadius(linkSource)
                Using pen = New Pen(CSelNode, 2.0F)
                    g.DrawEllipse(pen, c.X - r - 3, c.Y - r - 3, (r + 3) * 2, (r + 3) * 2)
                End Using
            End If

            RaiseViewChanged()
        End Sub

#End Region

    End Class

End Namespace

