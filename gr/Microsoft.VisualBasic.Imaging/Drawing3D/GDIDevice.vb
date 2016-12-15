#Region "Microsoft.VisualBasic::002e20ff39ab0e9aa40dc1e71657b695, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing3D\GDIDevice.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
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

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Parallel.Tasks
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing3D

    ''' <summary>
    ''' GDI+图形设备的简易抽象
    ''' </summary>
    Public Class GDIDevice : Inherits UserControl

        Protected WithEvents _animationLoop As Timer
        Protected camera As Camera
        Protected models As New List(Of I3DModel)

        Dim _rotationThread As New UpdateThread(
            200, Sub()
                     SyncLock camera
                         If keyRotate.X <> 0R OrElse keyRotate.Y <> 0R OrElse keyRotate.Z <> 0R Then
                             camera.angleX += keyRotate.X
                             camera.angleY += keyRotate.Y
                             camera.angleZ += keyRotate.Z
                         Else
                             camera.angleX += 0.01
                             camera.angleY += 0.01
                             camera.angleZ += 0.01
                         End If
                     End SyncLock
                 End Sub)

        Public Property AutoRotation As Boolean
            Get
                Return _rotationThread.Running
            End Get
            Set(value As Boolean)
                If value Then
                    _rotationThread.Start()
                Else
                    _rotationThread.Stop()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Enable Single-buffering to eliminate flickering.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Protected Sub GDIDevice_Load(sender As Object, e As EventArgs) Handles Me.Load
            camera = New Camera With {
                .angleX = 0,
                .angleY = 0,
                .angleZ = 0,
                .fov = 256,
                .screen = Size,
                .ViewDistance = -40
            }

            Call Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
            Call Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
            Call Me.__initDevice()
            Call Me.__init()
        End Sub

        Private Sub __initDevice()
            _animationLoop = New Timer With {
                .Interval = 25,
                .Enabled = False
            }
        End Sub

        Public Sub Run()
            _animationLoop.Enabled = True
            _animationLoop.Start()
        End Sub

        Public Sub Pause()
            _animationLoop.Enabled = False
            _animationLoop.Stop()
        End Sub

        Protected Overridable Sub __init()
            Try
                Throw New Exception("Please Implements the initialize code at here.")
            Catch ex As Exception
                Call ex.__DEBUG_ECHO
            End Try
        End Sub

        ''' <summary>
        ''' Forces the Paint event to be called.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub _animationLoop_Tick(sender As Object, e As EventArgs) Handles _animationLoop.Tick
            Call Me.Invalidate()
            Call Me.___animationLoop()
        End Sub

        Protected Overridable Sub ___animationLoop()
            Try
                Throw New Exception("Please Implements the control code at here.")
            Catch ex As Exception
                Call ex.__DEBUG_ECHO
            End Try
        End Sub

        Protected Overridable Sub __updateGraphics(sender As Object, ByRef g As Graphics, region As Rectangle)
            Call g.Clear(Color.LightBlue)

            For Each model As I3DModel In models
                Call model.Copy(camera.Rotate(model)).Draw(g, camera)
            Next
        End Sub

        Private Sub GDIDevice_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear

            Call __updateGraphics(sender, g:=e.Graphics, region:=e.ClipRectangle)
        End Sub

        Private Sub InitializeComponent()
            Me.SuspendLayout()
            '
            'GDIDevice
            '
            Me.Name = "GDIDevice"
            Me.Size = New System.Drawing.Size(438, 355)
            Me.ResumeLayout(False)

        End Sub

        Dim _rotate As Boolean

        Public Event RotateCamera(angleX!, angleY!, angleZ!)

        Private Sub GDIDevice_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown
            _rotate = True
        End Sub

        Private Sub GDIDevice_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
            If _rotate Then
                camera.angleX += 1
                camera.angleY += 1
                camera.angleZ += 1
            End If

            RaiseEvent RotateCamera(camera.angleX, camera.angleY, camera.angleZ)
        End Sub

        Public Sub RotateX(angle!)
            camera.angleX = angle
        End Sub

        Public Sub RotateY(angle!)
            camera.angleY = angle
        End Sub

        Public Sub RotateZ(angle!)
            camera.angleZ = angle
        End Sub

        Public Sub Rotate(angle As Point3D)
            camera.angleX = angle.X
            camera.angleY = angle.Y
            camera.angleZ = angle.Z
        End Sub

        Private Sub GDIDevice_MouseUp(sender As Object, e As MouseEventArgs) Handles Me.MouseUp
            _rotate = False
        End Sub

        Private Sub GDIDevice_Resize(sender As Object, e As EventArgs) Handles Me.Resize
            If camera Is Nothing Then
                Call "Camera object not initialized!".__DEBUG_ECHO
            Else
                camera.screen = Size
            End If
        End Sub

        Private Sub GDIDevice_MouseWheel(sender As Object, e As MouseEventArgs) Handles Me.MouseWheel
            Dim d% = Math.Sign(e.Delta)
            camera.ViewDistance += d
        End Sub

        Dim keyRotate As Point3D

        Public Sub SetAutoRotate(angle As Point3D)
            keyRotate = angle
            AutoRotation = True
        End Sub

        Private Sub GDIDevice_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
            AutoRotation = True

            Select Case e.KeyCode
                Case System.Windows.Forms.Keys.Up
                    keyRotate = New Point3D(0, 1, 0)
                Case System.Windows.Forms.Keys.Down
                    keyRotate = New Point3D(0, -1, 0)
                Case System.Windows.Forms.Keys.Left
                    keyRotate = New Point3D(1, 0, 0)
                Case System.Windows.Forms.Keys.Right
                    keyRotate = New Point3D(-1, 0, 0)
                Case Else
                    ' Do Nothing
                    AutoRotation = False
            End Select
        End Sub

        Private Sub GDIDevice_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
            keyRotate = Nothing
            AutoRotation = False
        End Sub
    End Class
End Namespace
