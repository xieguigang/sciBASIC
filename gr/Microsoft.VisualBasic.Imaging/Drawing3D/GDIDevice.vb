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
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Parallel.Tasks

Namespace Drawing3D

    Public Delegate Function ModelData() As IEnumerable(Of Surface)

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
        Protected _camera As New Camera With {
            .angleX = 0,
            .angleY = 0,
            .angleZ = 0,
            .fov = 256,
            .screen = Size,
            .ViewDistance = -40
        }

        Dim _rotationThread As New UpdateThread(15, AddressOf RunRotate)

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
        End Sub

        Public Sub Pause()
            _animationLoop.Enabled = False
            _animationLoop.Stop()
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

        Public Property Animation As Action(Of Camera)

        Private Sub ___animationLoop()
            If Not _Animation Is Nothing Then
                Call _Animation(_camera)
            End If
        End Sub

        Private Sub GDIDevice_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear

            If Not _Painter Is Nothing Then
                _camera.screen = e.ClipRectangle.Size
                _Painter(e.Graphics, _camera)
            End If
        End Sub

        Private Sub InitializeComponent()
            Me.SuspendLayout()
            '
            'GDIDevice
            '
            Me.Name = "GDIDevice"
            Me.Size = New Size(438, 355)
            Me.ResumeLayout(False)

        End Sub

        Dim _rotate As Boolean

        Public Event RotateCamera(angleX!, angleY!, angleZ!)

        Private Sub GDIDevice_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown
            _rotate = True
        End Sub

        Private Sub GDIDevice_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
            If _rotate Then
                _camera.angleX += 1
                _camera.angleY += 1
                _camera.angleZ += 1
            End If

            RaiseEvent RotateCamera(_camera.angleX, _camera.angleY, _camera.angleZ)
        End Sub

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

        Private Sub GDIDevice_MouseUp(sender As Object, e As MouseEventArgs) Handles Me.MouseUp
            _rotate = False
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
                    keyRotate = New Point3D(0, 1, 0)
                Case System.Windows.Forms.Keys.Down
                    keyRotate = New Point3D(0, -1, 0)
                Case System.Windows.Forms.Keys.Left
                    keyRotate = New Point3D(1, 0, 0)
                Case System.Windows.Forms.Keys.Right
                    keyRotate = New Point3D(-1, 0, 0)
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
