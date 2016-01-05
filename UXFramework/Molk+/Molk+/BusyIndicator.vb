Friend Class BusyIndicator

    Dim a As Double
    Dim d As Double = 2 * Math.PI / 1000
    Dim Indicator As Image = My.Resources.Minimize

    Dim Device As GDIPlusDeviceHandle
    Dim res As Image

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        a += d
        If a >= 2 * Math.PI Then
            a = 0
        End If

        Call DrawImageRotatedAroundCenter(Device.Gr_Device, New Point(Width / 2, Height / 2), Indicator, a)

        BackgroundImage = Device.ImageResource
        Call BackgroundImage.Save("x:\" & a & ".jpg")
    End Sub

    ''' <summary>
    ''' Draws an image onto the given graphics object. The image is rotated by a specified angle (in radians) around its center and then drawn at the given center point.
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="center"></param>
    ''' <param name="img"></param>
    ''' <param name="angle"></param>
    ''' <remarks></remarks>
    Private Sub DrawImageRotatedAroundCenter(g As Graphics, center As Point, img As Image, angle As Double)

        ' Think of the image as a rectangle that needs to be drawn rotated.
        ' Rotate the coordinates of the rectangle's corners.
        Dim upperLeft As Point = RotatePoint(New Point(-img.Width / 2, img.Height / 2), angle)
        Dim upperRight As Point = RotatePoint(New Point(img.Width / 2, img.Height / 2), angle)
        Dim lowerLeft As Point = RotatePoint(New Point(-img.Width / 2, -img.Height / 2), angle)

        ' Create the points array by offsetting the coordinates with the center.
        Dim points() As Point = {upperLeft + center, upperRight + center, lowerLeft + center}

        ' Draw the rotated image.
        g.DrawImage(img, points)
    End Sub

    ' Rotates a point around the origin by the specified angle (in radians).
    ' Rotation adheres to the following rules for the new coordinates:
    ' x' = x cos(a) + y sin(a)
    ' y' = -x sin(a) + y cos(a)
    Private Function RotatePoint(p As Point, angle As Double) As Point

        Dim x As Integer = p.X * Math.Cos(angle) + p.Y * Math.Sin(angle)
        Dim y As Integer = -p.X * Math.Sin(angle) + p.Y * Math.Cos(angle)

        Return New Point(x, y)
    End Function

    Private Sub BusyIndicator_Load(sender As Object, e As EventArgs) Handles Me.Load
        CheckForIllegalCrossThreadCalls = False
    End Sub

    Private Sub BusyIndicator_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Device = Me.Size.CreateGDIDevice
        res = Device.ImageResource.Clone
    End Sub
End Class
