Imports System.Windows.Forms

Namespace Drawing3D

    ''' <summary>
    ''' GDI+图形设备的简易抽象
    ''' </summary>
    Public Class GDIDevice : Inherits UserControl

        Protected WithEvents _animationLoop As Timer

        ''' <summary>
        ''' Enable double-buffering to eliminate flickering.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub GDIDevice_Load(sender As Object, e As EventArgs) Handles Me.Load
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
            Throw New Exception("Please Implements the initialize code at here.")
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
            Throw New Exception("Please Implements the control code at here.")
        End Sub

        Protected Overridable Sub __updateGraphics(sender As Object, Gr As PaintEventArgs)
            Throw New Exception("Please Implements the graphics updates code at here.")
        End Sub

        Private Sub GDIDevice_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
            Call __updateGraphics(sender, Gr:=e)
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
    End Class
End Namespace