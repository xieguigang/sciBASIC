Imports System.Timers
Imports Microsoft.VisualBasic.DataVisualization.Network.Graph
Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts
Imports Microsoft.VisualBasic.Parallel.Tasks

Public Class Canvas

    Public Property Graph As NetworkGraph
        Get
            If net Is Nothing Then
                Call __invokeSet(New NetworkGraph)
            End If

            Return net
        End Get
        Set(value As NetworkGraph)
            Call __invokeSet(value)
        End Set
    End Property

    Private Sub __invokeSet(g As NetworkGraph)
        net = g
        fdgPhysics = New ForceDirected2D(net, FdgArgs.Stiffness, FdgArgs.Repulsion, FdgArgs.Damping)
        fdgRenderer = New Renderer(
            Function() paper,
            Function() New Rectangle(New Point, Size),
            fdgPhysics)
        inputs = New InputDevice(Me)
        fdgRenderer.Asynchronous = False
    End Sub

    Public ReadOnly Property FdgArgs As ForceDirectedArgs = Config.Load

    ''' <summary>
    ''' The network data model for the visualization 
    ''' </summary>
    Dim net As NetworkGraph
    ''' <summary>
    ''' Layout provider engine
    ''' </summary>
    Protected Friend fdgPhysics As ForceDirected2D
    ''' <summary>
    ''' The graphics updates thread.
    ''' </summary>
    Protected Friend timer As New UpdateThread(30, AddressOf __invokePaint)
    ''' <summary>
    ''' The graphics rendering provider
    ''' </summary>
    Protected Friend fdgRenderer As Renderer
    ''' <summary>
    ''' GDI+ interface for the canvas control.
    ''' </summary>
    Dim paper As Graphics

    Private Sub __invokePaint()
        Call Me.Invoke(Sub() Invalidate())
    End Sub

    Private Sub Canvas_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        paper = e.Graphics
        paper.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
        paper.SmoothingMode = Drawing2D.SmoothingMode.HighQuality

        Call fdgRenderer.Draw(0.05F)
    End Sub

    Dim inputs As InputDevice

    Private Sub Canvas_Load(sender As Object, e As EventArgs) Handles Me.Load
        Graph = New NetworkGraph
        timer.ErrHandle = AddressOf App.LogException
        timer.Start()
    End Sub

    Private Sub Canvas_Disposed(sender As Object, e As EventArgs) Handles Me.Disposed
        timer.Dispose()
    End Sub
End Class
