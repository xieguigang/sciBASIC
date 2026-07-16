#Region "Microsoft.VisualBasic::e294c700dc7d04dbae6f856ec5c58715, gr\network-visualization\NetworkCanvas\Canvas.vb"

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

' Class Canvas
' 
'     Properties: AutoRotate, DynamicsRadius, FdgArgs, Graph, ShowLabel
'                 ViewDistance
' 
'     Function: GetSnapshot, GetTargetNode, WriteLayout
' 
'     Sub: [Stop], Canvas_Disposed, Canvas_Load, Canvas_Paint, Canvas_SizeChanged
'          doPaint, doPhysicsUpdates, Run, SetFDGParams, SetPhysical
'          SetRotate, setupGraph
' 
' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.SpringForce
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.SpringForce.Interfaces
Imports Microsoft.VisualBasic.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Parallel.Tasks
Imports Bitmap = System.Drawing.Bitmap

''' <summary>
''' Controls for view the network model.
''' </summary>
Public Class Canvas

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="space3D">Is 3D network viewer canvas</param>
    ''' <returns></returns>
    Public Property Graph(Optional space3D As Boolean = False) As NetworkGraph
        Get
            If net Is Nothing Then
                Call setupGraph(New NetworkGraph, Me.space3D)
            End If

            Return net
        End Get
        Set(value As NetworkGraph)
            If Not value Is Nothing Then
                Call setupGraph(value, space3D)
            End If
        End Set
    End Property

    ''' <summary>
    ''' Render and layout engine works in 3D mode?
    ''' </summary>
    Friend space3D As Boolean = False

    Private Sub setupGraph(net As NetworkGraph, space3D As Boolean)
        Dim showLabel As Boolean = Me.ShowLabel

        Me.net = net
        Me.space3D = space3D

        If Not inputs Is Nothing Then
            Call inputs.Dispose()
            GC.SuppressFinalize(inputs)
            inputs = Nothing
        End If

        If space3D Then
            fdgPhysics = New ForceDirected3D(Me.net, FdgArgs.Stiffness, FdgArgs.Repulsion, FdgArgs.Damping)
            fdgRenderer = New Renderer3D(
                Function() paper,
                Function() New Rectangle(New Point, Size),
                fdgPhysics, DynamicsRadius)
            DirectCast(fdgRenderer, Renderer3D).ViewDistance = viewDist
            inputs = New Input3D(Me)
        Else
            fdgPhysics = New ForceDirected2D(Me.net, FdgArgs.Stiffness, FdgArgs.Repulsion, FdgArgs.Damping)
            fdgRenderer = New Renderer(
                Function() paper,
                Function() New Rectangle(New Point, Size),
                fdgPhysics)
            inputs = New InputDevice(Me)
        End If

        Me.fdgPhysics.interactiveMode = True
        Me.fdgPhysics.width = Width
        Me.fdgPhysics.height = Height
        Me.ShowLabel = showLabel
    End Sub

    Public ReadOnly Property FdgArgs As New ForceDirectedArgs

    Public Sub SetRotate(x As Double)
        If Not space3D Then
        Else
            DirectCast(fdgRenderer, Renderer3D).rotate = x
        End If
    End Sub

    Public Sub SetFDGParams(value As ForceDirectedArgs)
        FdgArgs.Damping = value.Damping
        FdgArgs.Repulsion = value.Repulsion
        FdgArgs.Stiffness = value.Stiffness

        Call fdgPhysics.SetPhysics(
            value.Stiffness,
            value.Repulsion,
            value.Damping
        )
    End Sub

    ''' <summary>
    ''' The network data model for the visualization 
    ''' </summary>
    Dim net As NetworkGraph
    ''' <summary>
    ''' Layout provider engine
    ''' </summary>
    Protected Friend fdgPhysics As IForceDirected
    ''' <summary>
    ''' The graphics updates thread.
    ''' </summary>
    Protected Friend timer As New UpdateThread(30, AddressOf doPaint)
    Protected Friend physicsEngine As New UpdateThread(30, AddressOf doPhysicsUpdates)
    ''' <summary>
    ''' The graphics rendering provider
    ''' </summary>
    Protected Friend fdgRenderer As Renderer
    ''' <summary>
    ''' GDI+ interface for the canvas control.
    ''' </summary>
    Dim paper As IGraphics
    Dim viewDist As Double = -450

    ''' <summary>
    ''' 渲染视图状态（视口、悬停/选中、LOD、网格），在 Canvas 与 Renderer 间共享。
    ''' </summary>
    Protected Friend view As New CanvasViewState

    ''' <summary>
    ''' 离屏缓冲与对应的 GDI+ 设备上下文，用于整帧绘制后单次 Blit，减少闪烁。
    ''' </summary>
    Private backBuffer As Bitmap
    Private backG As System.Drawing.Graphics
    Private nodeTooltip As New ToolTip
    Private grid As New SpatialGrid

    Public Property AutoRotate As Boolean = True
    Public Property DynamicsRadius As Boolean = False

    Public Property ViewDistance As Double
        Get
            If Not space3D Then
                Return 0
            Else
                Return DirectCast(fdgRenderer, Renderer3D).ViewDistance
            End If
        End Get
        Set(value As Double)
            If space3D Then
                DirectCast(fdgRenderer, Renderer3D).ViewDistance = value
            End If

            viewDist = value
        End Set
    End Property

    <DefaultValue(True)>
    Public Property ShowLabel As Boolean
        Get
            If fdgRenderer Is Nothing Then
                Return False
            End If
            Return DirectCast(fdgRenderer, IGraphicsEngine).ShowLabels
        End Get
        Set(value As Boolean)
            If Not fdgRenderer Is Nothing Then
                DirectCast(fdgRenderer, IGraphicsEngine).ShowLabels = value
            End If
        End Set
    End Property

    Private Sub doPaint()
        Try
            Call Me.Invoke(Sub() Call Invalidate())

            If _AutoRotate Then
                Static r As Double = -100.0R
                r += 0.4
                Call SetRotate(r)
            End If
        Catch ex As Exception
            Call App.LogException(ex)
        End Try
    End Sub

    ''' <summary>
    ''' get target node object which is pointed by the mouse pointer
    ''' </summary>
    ''' <returns></returns>
    Public Function GetTargetNode(p As Point) As Node
        Return inputs.GetPointedNode(p)
    End Function

    Public Function GetSnapshot() As Bitmap
        Dim bitmap As New Bitmap(Width, Height)
        Call Me.DrawToBitmap(bitmap, New Rectangle(0, 0, bitmap.Width, bitmap.Height))
        Return bitmap
    End Function

    Private Sub doPhysicsUpdates()
        SyncLock fdgRenderer
            If Not fdgRenderer Is Nothing Then
                Call fdgRenderer.PhysicsEngine.Collide(0.05F)
            End If
        End SyncLock
    End Sub

    Private Sub Canvas_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        If Width <= 0 OrElse Height <= 0 Then
            Return
        End If

        ' 按需重建离屏缓冲
        If backBuffer Is Nothing OrElse backBuffer.Width <> Width OrElse backBuffer.Height <> Height Then
            If backBuffer IsNot Nothing Then backBuffer.Dispose()
            If backG IsNot Nothing Then backG.Dispose()

            backBuffer = New Bitmap(Width, Height)
            backG = System.Drawing.Graphics.FromImage(backBuffer)
            ' 高质量绘制：抗锯齿 + 半像素偏移 + ClearType 文本
            backG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias
            backG.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half
            backG.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit
            paper = New Graphics2D(backG, Size)
        End If

        ' 清空背景（背景网格由 Renderer 叠加绘制）
        backG.Clear(BackColor)

        If fdgRenderer IsNot Nothing Then
            fdgRenderer.View = view
            ' 整帧单次锁，与物理线程（doPhysicsUpdates 中的 SyncLock fdgRenderer）互斥
            SyncLock fdgRenderer
                fdgRenderer.Draw(0.05F, physicsUpdate:=False)
            End SyncLock
        End If

        ' 单次 Blit 到控件表面
        e.Graphics.DrawImage(backBuffer, 0, 0)
    End Sub

    Dim inputs As InputDevice

    ''' <summary>
    ''' 命中测试：返回屏幕点 p 下的节点（2D 视图使用空间网格加速）。
    ''' </summary>
    Public Function HitTest(p As Point) As Node
        If fdgRenderer Is Nothing Then
            Return Nothing
        End If

        If space3D Then
            ' 3D 视图暂不做悬停命中（保留原有行为）
            Return Nothing
        End If

        Dim rect = fdgRenderer.ClientRegion
        Dim vp = view.Viewport

        SyncLock fdgRenderer
            grid.Build(Graph.vertex,
                Function(n)
                    Dim pos = fdgPhysics.GetPoint(n).position
                    Return vp.ToScreen(CSng(pos.x), CSng(pos.y), rect)
                End Function,
                Function(n)
                    Dim baseR = If(n.data.size.IsNullOrEmpty, 0, CSng(n.data.size(0)))
                    If baseR = 0 Then baseR = 20
                    Return System.Math.Max(2.0F, baseR * vp.zoom)
                End Function,
                System.Math.Max(24.0F, 2.0F * 20.0F * vp.zoom + 1))

            Return grid.Query(New PointF(p.X, p.Y),
                Function(n)
                    Dim pos = fdgPhysics.GetPoint(n).position
                    Return vp.ToScreen(CSng(pos.x), CSng(pos.y), rect)
                End Function,
                Function(n)
                    Dim baseR = If(n.data.size.IsNullOrEmpty, 0, CSng(n.data.size(0)))
                    If baseR = 0 Then baseR = 20
                    Return System.Math.Max(2.0F, baseR * vp.zoom)
                End Function)
        End SyncLock
    End Function

    Public Sub ShowNodeTooltip(n As Node, p As Point)
        If n Is Nothing OrElse n.data Is Nothing Then
            Return
        End If
        nodeTooltip.Show(n.data.label, Me, p.X + 12, p.Y + 12)
    End Sub

    Public Sub HideTooltip()
        nodeTooltip.Hide(Me)
    End Sub

    Private Sub Canvas_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Graph Is Nothing Then
            ' 假若在load之前已经加载graph数据则在这里会清除掉先前的数据
            ' 所以需要判断一下
            Graph = New NetworkGraph
        End If

        On Error Resume Next

        timer.ErrHandle = AddressOf App.LogException
        timer.Start()
        physicsEngine.ErrHandle = AddressOf App.LogException
        physicsEngine.Start()
    End Sub

    Public Sub SetPhysical(status As Boolean)
        If status Then
            physicsEngine.Start()
        Else
            physicsEngine.Stop()
        End If
    End Sub

    Public Sub [Stop]()
        Call timer.Stop()
        Call physicsEngine.Stop()
    End Sub

    Public Sub Run()
        Call timer.Start()
        Call physicsEngine.Start()
    End Sub

    ''' <summary>
    ''' Write the node layout position into its extensions data, for generates the svg graphics.
    ''' </summary>
    Public Function WriteLayout() As NetworkGraph
        Call Graph.WriteLayouts(fdgPhysics)
        Return Graph
    End Function

    Private Sub Canvas_Disposed(sender As Object, e As EventArgs) Handles Me.Disposed
        timer.Dispose()
        physicsEngine.Dispose()
        If backBuffer IsNot Nothing Then backBuffer.Dispose()
        If backG IsNot Nothing Then backG.Dispose()
    End Sub

    Private Sub Canvas_SizeChanged(sender As Object, e As EventArgs) Handles Me.SizeChanged
        If Not fdgPhysics Is Nothing Then
            fdgPhysics.width = Width
            fdgPhysics.height = Height
        End If
    End Sub
End Class
