Imports System.Drawing.Drawing2D

Public Class ImageCorping

    Public ReadOnly Property CorpedImage As Image
        Get
            Return crobPictureBox.BackgroundImage
        End Get
    End Property

    Dim originalImage As Image

    ''' <summary>
    ''' 属性会自动缩放原始图片和控件一致的尺寸比例
    ''' </summary>
    ''' <returns></returns>
    Public Property SourceImage As Image
        Get
            Return BackgroundImage
        End Get
        Set(value As Image)

            If value Is Nothing Then
                Return
            Else
                value = ZoomImage(Me.Size, value)
            End If

            Dim masked As Image = value.Clone
            Dim maskedGr As Graphics = Graphics.FromImage(masked)

            Call maskedGr.FillRectangle(New SolidBrush(Color.FromArgb(185, Color.Black)), New Rectangle(New Point, Me.Size))

            BackgroundImage = masked
            originalImage = value

            Call Reset()
        End Set
    End Property

    Private Function ZoomImage(Limits As Size, res As Image) As Image
        Dim bitmap As New Bitmap(Limits.Width, Limits.Height)
        Dim gr As Graphics = Graphics.FromImage(bitmap)

        Call gr.FillRectangle(New SolidBrush(Color.FromArgb(0, Color.Black)), New Rectangle(New Point, Limits))

        Dim drWidth, drHeight As Integer, drPoint As Point

        If res.Width > res.Height Then
            drWidth = Limits.Width
            Dim p = drWidth / res.Width
            drHeight = p * res.Height

        ElseIf res.Width < res.Height
            drHeight = Limits.Height
            Dim p = drHeight / res.Height
            drWidth = p * res.Width

        Else
            Dim Min = Math.Min(Limits.Width, Limits.Height)
            drWidth = Min
            drHeight = Min
        End If

        drPoint = New Point((Limits.Width - drWidth) / 2, (Limits.Height - drHeight) / 2)
        Call gr.DrawImage(res, x:=drPoint.X, y:=drPoint.Y, width:=drWidth, height:=drHeight)

        Return bitmap
    End Function

    Private Sub ImageCorping_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim Move = New API.ControlMoverOrResizer(crobPictureBox) With {.WorkType = API.ControlMoverOrResizer.MoveOrResize.MoveAndResize, .KeepAspectRatio = True}

        crobPictureBox.Location = New Point((Width - crobPictureBox.Width) / 2, (Height - crobPictureBox.Height) / 2)
    End Sub

    Public Sub Reset()
        Dim minD As Integer = Math.Min(Width, Height) / 2.5

        crobPictureBox.Size = New Size(minD, minD)
        crobPictureBox.Location = New Point((Width - crobPictureBox.Width) / 2, (Height - crobPictureBox.Height) / 2)
        ctrMouseDown = True
        Call crobPictureBox_MouseMove(Nothing, Nothing)
        ctrMouseDown = False
    End Sub

#Region "Image Cropping"
    Private Sub crobPictureBox_MouseMove(sender As Object, e As MouseEventArgs) Handles crobPictureBox.MouseMove
        If Not ctrMouseDown OrElse originalImage Is Nothing Then
            Return
        End If

        Dim rect As Rectangle = New Rectangle(crobPictureBox.Location, crobPictureBox.Size)
        Dim bit As Bitmap = New Bitmap(originalImage, Me.BackgroundImage.Width, Me.BackgroundImage.Height)

        Dim cropBitmap As Bitmap = New Bitmap(crobPictureBox.Width, crobPictureBox.Height)

        Dim g As Graphics = Graphics.FromImage(cropBitmap)
        g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
        g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
        g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
        g.DrawImage(bit, 0, 0, rect, GraphicsUnit.Pixel)
        crobPictureBox.BackgroundImage = cropBitmap
    End Sub
#End Region

    Dim ctrMouseDown As Boolean

    Private Sub crobPictureBox_MouseUp(sender As Object, e As MouseEventArgs) Handles crobPictureBox.MouseUp
        ctrMouseDown = False
        RaiseEvent CorpDone(Me.CorpedImage)
    End Sub

    Public Event CorpDone(corped As Image)

    Private Sub crobPictureBox_MouseDown(sender As Object, e As MouseEventArgs) Handles crobPictureBox.MouseDown
        If e.Button = MouseButtons.Left Then
            ctrMouseDown = True
        End If
    End Sub

    Public Shared Function Corping(source As Image, Rect As Rectangle) As Image
        Dim bit As Bitmap = New Bitmap(Clone(source), source.Width, source.Height)
        Dim cropBitmap As Bitmap = New Bitmap(Rect.Width, Rect.Height)
        Dim Gr As Graphics = Graphics.FromImage(cropBitmap)

        Gr.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
        Gr.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
        Gr.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
        Gr.DrawImage(bit, 0, 0, Rect, GraphicsUnit.Pixel)

        Return cropBitmap
    End Function
End Class
