#Region "Microsoft.VisualBasic::87635e8df39662d389355e7b607d474f, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing3D\GDIDevice.vb"

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
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Device.Worker
Imports Microsoft.VisualBasic.Parallel.Tasks
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing3D.Device

    ''' <summary>
    ''' GDI+三维图形显示设备的简易抽象
    ''' </summary>
    ''' <remarks>
    ''' 在这个控件之中存在两条工作线程用来加速三维图形的绘制:
    ''' 
    ''' + 空间计算线程，用来对数据进行预处理，包括投影和排序，生成可以直接被使用的多边形缓存
    ''' + 图形渲染线程，用于进行三维图形的绘图操作，进行图像显示
    ''' </remarks>
    Public Class GDIDevice : Inherits UserControl

        Protected WithEvents _animationLoop As Timer
        Protected Friend _camera As New Camera With {
            .angleX = 0,
            .angleY = 0,
            .angleZ = 0,
            .fov = 256,
            .screen = Size,
            .ViewDistance = -40
        }

        Dim _rotationThread As New UpdateThread(15, AddressOf RunRotate)
        Dim worker As New Worker(Me)
        Dim mouse As New Mouse(Me)

        Public Property drawPath As Boolean
            Get
                Return worker.drawPath
            End Get
            Set(value As Boolean)
                worker.drawPath = value
            End Set
        End Property
        Public Property LightIllumination As Boolean
            Get
                Return worker.LightIllumination
            End Get
            Set(value As Boolean)
                worker.LightIllumination = value
            End Set
        End Property
        Public Property ViewDistance As Single
            Get
                Return _camera.ViewDistance
            End Get
            Set
                _camera.ViewDistance = Value
            End Set
        End Property

        Private Sub RunRotate()
            SyncLock _camera
                If keyRotate.X <> 0R OrElse keyRotate.Y <> 0R OrElse keyRotate.Z <> 0R Then
                    _camera.angleX += keyRotate.X
                    _camera.angleY += keyRotate.Y
                    _camera.angleZ += keyRotate.Z
                Else
                    _camera.angleX += 0.1
                    _camera.angleY += 0.1
                    _camera.angleZ += 0.1
                End If
            End SyncLock
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            Call _rotationThread.Dispose()
            Call worker.Dispose()
            Call Pause()

            MyBase.Dispose(disposing)
        End Sub

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
            Call Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
            Call Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
            Call Me.__initDevice()
            Call Me.__init()
        End Sub

        Private Sub __initDevice()
            _animationLoop = New Timer With {
                .Interval = 20,
                .Enabled = False
            }
        End Sub

        Public Sub Run()
            _animationLoop.Enabled = True
            _animationLoop.Start()
            worker.Run()
        End Sub

        Public Sub Pause()
            _animationLoop.Enabled = False
            _animationLoop.Stop()
            worker.Pause()
        End Sub

        Public Property RefreshInterval As Integer
            Get
                Return _animationLoop.Interval
            End Get
            Set(value As Integer)
                _animationLoop.Interval = value
                _animationLoop.Start()
            End Set
        End Property

        ''' <summary>
        ''' 这个属性用来提供模型数据，例如位移之后的模型数据
        ''' </summary>
        ''' <returns></returns>
        Public Property Model As ModelData
        Public Property bg As Color

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

        Public Property Animation As CameraControl

        Private Sub ___animationLoop()
            If Not _Animation Is Nothing Then
                Call _Animation(_camera)
            End If
        End Sub

        Private Sub InitializeComponent()
            Me.SuspendLayout()
            '
            'GDIDevice
            '
            Me.BackColor = System.Drawing.Color.LightBlue
            Me.Cursor = System.Windows.Forms.Cursors.Cross
            Me.Name = "GDIDevice"
            Me.Size = New System.Drawing.Size(438, 355)
            Me.ResumeLayout(False)

        End Sub

        Public Event RotateCamera(angleX!, angleY!, angleZ!)

        Public Sub RotateX(angle!)
            _camera.angleX = angle
        End Sub

        Public Sub RotateY(angle!)
            _camera.angleY = angle
        End Sub

        Public Sub RotateZ(angle!)
            _camera.angleZ = angle
        End Sub

        Public Sub Rotate(angle As Point3D)
            _camera.angleX = angle.X
            _camera.angleY = angle.Y
            _camera.angleZ = angle.Z
        End Sub

        Public Property DisableScreenResize As Boolean = False

        Private Sub GDIDevice_Resize(sender As Object, e As EventArgs) Handles Me.Resize
            If _camera Is Nothing Then
                Call "Camera object not initialized!".__DEBUG_ECHO
            Else
                If Not DisableScreenResize Then
                    _camera.screen = Size
                End If
            End If
        End Sub

        Private Sub GDIDevice_MouseWheel(sender As Object, e As MouseEventArgs) Handles Me.MouseWheel
            Dim d% = Math.Sign(e.Delta)
            _camera.ViewDistance += d

#If DEBUG Then
            Call _camera.GetJson.__DEBUG_ECHO
#End If
        End Sub

        Dim keyRotate As Point3D

        'Public Sub SetAutoRotate(angle As Point3D)
        '    keyRotate = angle
        '    ' AutoRotation = True
        'End Sub

        Private Sub GDIDevice_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
            Select Case e.KeyCode
                Case System.Windows.Forms.Keys.Up
                    keyRotate = New Point3D(0, 2, 0)
                Case System.Windows.Forms.Keys.Down
                    keyRotate = New Point3D(0, -2, 0)
                Case System.Windows.Forms.Keys.Left
                    keyRotate = New Point3D(2, 0, 0)
                Case System.Windows.Forms.Keys.Right
                    keyRotate = New Point3D(-2, 0, 0)
                Case Else
                    ' Do Nothing
            End Select

            Call RunRotate()
        End Sub

        Private Sub GDIDevice_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
            keyRotate = Nothing
        End Sub
    End Class
End Namespace
