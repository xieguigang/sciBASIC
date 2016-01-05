Public Class TransparentLinkLable

    Public Property LabelText As String
        Get
            Return MyBase.Text
        End Get
        Set(value As String)
            MyBase.Text = value
            Call Update()
        End Set
    End Property

    Public Overrides Property Text As String
        Get
            Return MyBase.Text
        End Get
        Set(value As String)
            MyBase.Text = value
            Call Update()
        End Set
    End Property

    Public Overrides Property Font As Font
        Get
            Return MyBase.Font
        End Get
        Set(value As Font)
            MyBase.Font = value
            Call Update()
        End Set
    End Property

    Public Overrides Property ForeColor As Color
        Get
            Return MyBase.ForeColor
        End Get
        Set(value As Color)
            MyBase.ForeColor = value
            Call Update()
        End Set
    End Property

    Public Overloads Sub Update()
        If String.IsNullOrEmpty(Text) Then
            Return
        End If

        If Me.Parent Is Nothing Then
            Return
        End If

        On Error Resume Next

        Dim Size = Text.MeasureString(Me.Font)
        Dim Bitmap As New Bitmap(CInt(Size.Width), CInt(Size.Height))
        Dim Gr = Graphics.FromImage(Bitmap)

        Me.Size = New Size(Size.Width, Size.Height)

        Gr.CompositingQuality = Drawing2D.CompositingQuality.HighQuality

        If Parent.BackgroundImage Is Nothing Then
            Call Gr.FillRectangle(New SolidBrush(Parent.BackColor), New Rectangle(New Point, Size))
        Else
            Dim Crop = Me.Parent.BackgroundImage.ImageCrop(Me.Location, Me.Size)
            Call Gr.DrawImage(Crop, New Point)
        End If

        Call Gr.DrawString(Text, Font, New SolidBrush(ForeColor), New Point)

        Me.BackgroundImage = Bitmap
    End Sub

    Private Sub TransparentLinkLable_Load(sender As Object, e As EventArgs) Handles Me.Load
        Call Update()
    End Sub
End Class
