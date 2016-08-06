#Region "Microsoft.VisualBasic::22d327a245242f86260f8aa48c34c746, ..\VisualBasic_AppFramework\Datavisualization\Datavisualization.Network\NetworkCanvas\Canvas.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Timers
Imports Microsoft.VisualBasic.ComponentModel.Settings.Inf
Imports Microsoft.VisualBasic.DataVisualization.Network.Graph
Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts
Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts.Interfaces
Imports Microsoft.VisualBasic.Parallel.Tasks

''' <summary>
''' Controls for view the network model.
''' </summary>
Public Class Canvas

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="space">Is 3D network viewer canvas</param>
    ''' <returns></returns>
    Public Property Graph(Optional space As Boolean = False) As NetworkGraph
        Get
            If net Is Nothing Then
                Call __invokeSet(New NetworkGraph, space3D)
            End If

            Return net
        End Get
        Set(value As NetworkGraph)
            space3D = space
            Call __invokeSet(value, space3D)
        End Set
    End Property

    ''' <summary>
    ''' Render and layout engine works in 3D mode?
    ''' </summary>
    Dim space3D As Boolean

    Private Sub __invokeSet(g As NetworkGraph, space As Boolean)
        net = g

        If Not inputs Is Nothing Then
            Call inputs.Dispose()
            GC.SuppressFinalize(inputs)
            inputs = Nothing
        End If

        If space Then
            fdgPhysics = New ForceDirected3D(net, FdgArgs.Stiffness, FdgArgs.Repulsion, FdgArgs.Damping)
            fdgRenderer = New Renderer3D(
                Function() paper,
                Function() New Rectangle(New Point, Size),
                fdgPhysics)
            inputs = New Input3D(Me)
        Else
            fdgPhysics = New ForceDirected2D(net, FdgArgs.Stiffness, FdgArgs.Repulsion, FdgArgs.Damping)
            fdgRenderer = New Renderer(
                Function() paper,
                Function() New Rectangle(New Point, Size),
                fdgPhysics)
            inputs = New InputDevice(Me)
        End If

        fdgRenderer.Asynchronous = False
    End Sub

    Public ReadOnly Property FdgArgs As ForceDirectedArgs = Config.Load

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
            value.Damping)
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
    Protected Friend timer As New UpdateThread(30, AddressOf __invokePaint)
    ''' <summary>
    ''' The graphics rendering provider
    ''' </summary>
    Protected Friend fdgRenderer As Renderer
    ''' <summary>
    ''' GDI+ interface for the canvas control.
    ''' </summary>
    Dim paper As Graphics

    Public Property AutoRotate As Boolean = True

    Private Sub __invokePaint()
        Call Me.Invoke(Sub() Call Invalidate())

        If _AutoRotate Then
            Static r As Double = -100.0R
            r += 0.4
            Call SetRotate(r)
        End If
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

    Public Sub [Stop]()
        Call timer.Stop()
    End Sub

    Public Sub Run()
        Call timer.Start()
    End Sub

    ''' <summary>
    ''' Write the node layout position into its extensions data, for generates the svg graphics.
    ''' </summary>
    Public Sub WriteLayout()
        Call Graph.WriteLayouts(fdgPhysics)
    End Sub

    Private Sub Canvas_Disposed(sender As Object, e As EventArgs) Handles Me.Disposed
        timer.Dispose()
    End Sub
End Class
