Imports System.Drawing
Imports System.Windows.Forms

Namespace Drawing3D.Device

    Public Class Debugger : Inherits IDevice

        Public BufferWorker As Long
        Public RenderingWorker As Long

        Public Sub New(dev As GDIDevice)
            MyBase.New(dev)
        End Sub

        Dim font As New Font(FontFace.Consolas, 9)
        Dim red As SolidBrush = Brushes.Red
        Dim mouse As Point

        Public Sub DrawInformation(canvas As Graphics)
            Dim top! = 15, left! = 10
            Dim camera As Camera = device._camera
            Dim draw = Sub(msg$)
                           top += 14
                           Call canvas.DrawString(msg, font, red, New PointF(left, top))
                       End Sub

            ' 显示camera的调试信息
            Call draw(msg:=$"Rotation vector:       x={camera.angleX}, y={camera.angleY}, z={camera.angleZ}")
            Call draw(msg:=$"View distance:         {camera.ViewDistance}")
            Call draw(msg:=$"Screen size:           {camera.screen.Width}px X {camera.screen.Height}px")

            ' 显示系统的性能
            Call draw(msg:=$"Buffer worker time:    {BufferWorker} (ticks)")
            Call draw(msg:=$"Rendering worker time: {RenderingWorker} (ticks)")

            ' 显示设备捕捉信息
            Call draw(msg:=$"Mouse device:          ({mouse.X}, {mouse.Y})")
        End Sub

        Private Sub device_MouseMove(sender As Object, e As MouseEventArgs) Handles device.MouseMove
            mouse = e.Location
        End Sub
    End Class
End Namespace