#Region "Microsoft.VisualBasic::b708a6535a64e0db78dd6ec0bf4765fd, sciBASIC#\Data_science\Visualization\Canvas3D\Device\GDIDevice.vb"

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

    '   Total Lines: 235
    '    Code Lines: 161
    ' Comment Lines: 41
    '   Blank Lines: 33
    '     File Size: 7.65 KB


    '     Class GDIDevice
    ' 
    '         Properties: Animation, AutoRotation, bg, DisableScreenResize, DrawPath
    '                     FOV, LightColor, LightIllumination, Model, Plot
    '                     RefreshInterval, RotationThread, ShowDebugger, ShowHorizontalPanel, ViewDistance
    ' 
    '         Sub: ___animationLoop, __init, __initDevice, _animationLoop_Tick, Dispose
    '              GDIDevice_KeyDown, GDIDevice_Load, GDIDevice_MouseWheel, GDIDevice_Resize, InitializeComponent
    '              Pause, Rotate, RotateX, RotateY, RotateZ
    '              Run
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Math
Imports Microsoft.VisualBasic.Data.ChartPlots.Drawing3D.Device.Worker
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Keyboard = System.Windows.Forms.Keys

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
            .viewDistance = -40
        }

        Dim rotationWorker As New AutoRotation(Me)
        Dim worker As New Worker(Me)
        Dim mouse As New Mouse(Me, _camera)

        ''' <summary>
        ''' 是否绘制出模型的边界线
        ''' </summary>
        ''' <returns></returns>
        Public Property DrawPath As Boolean
        Public Property LightIllumination As Boolean

        Public Property LightColor As Color
            Get
                Return _camera.lightColor
            End Get
            Set(value As Color)
                _camera.lightColor = value
            End Set
        End Property

        Public Property ViewDistance As Single
            Get
                Return _camera.viewDistance
            End Get
            Set
                _camera.viewDistance = Value
            End Set
        End Property
        Public Property FOV As Double
            Get
                Return _camera.fov
            End Get
            Set(value As Double)
                _camera.fov = value
            End Set
        End Property

        Public Property ShowDebugger As Boolean

        Protected Overrides Sub Dispose(disposing As Boolean)
            Call worker.Dispose()
            Call Pause()

            MyBase.Dispose(disposing)
        End Sub

        Public Property AutoRotation As Boolean
        Public ReadOnly Property RotationThread As AutoRotation
            Get
                Return rotationWorker
            End Get
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
        ''' <summary>
        ''' ```vbnet
        ''' Public Delegate Sub IGraphics(g As <see cref="Graphics"/>, camera As <see cref="Camera"/>)
        ''' ```
        ''' 这个接口提供一些额外的信息的显示，例如调试信息
        ''' </summary>
        ''' <returns></returns>
        Public Property Plot As DrawGraphics
        Public Property bg As Color
        Public Property ShowHorizontalPanel As Boolean = False

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
            Dim d% = Sign(e.Delta)
            _camera.viewDistance += d
            '#If DEBUG Then
            '            Call _camera.GetJson.__DEBUG_ECHO
            '#End If
        End Sub

        Private Sub GDIDevice_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
            Select Case e.KeyCode
                Case Keyboard.Up
                    rotationWorker.Y += 5
                Case Keyboard.Down
                    rotationWorker.Y -= 5
                Case Keyboard.Left
                    rotationWorker.X += 5
                Case Keyboard.Right
                    rotationWorker.X -= 5
                Case Else
                    ' Do Nothing
            End Select

            Call rotationWorker.RunRotate()
        End Sub
    End Class
End Namespace
