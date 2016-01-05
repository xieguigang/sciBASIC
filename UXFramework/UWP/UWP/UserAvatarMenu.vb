Public Class UserAvatarMenu

    Public Overrides Property Text As String
        Get
            Return Label1.Text
        End Get
        Set(value As String)
            Label1.Text = value
            Size = New Size(Label1.Width + PictureBox1.Width + 20, Height)
            Menu.Label1.Text = value
        End Set
    End Property

    Public Property EMail As String
        Get
            Return Menu.Label2.Text
        End Get
        Set(value As String)
            Menu.Label2.Text = value
        End Set
    End Property

    Dim _avatar As Image

    Public Property Avatar As Image
        Get
            Return _avatar
        End Get
        Set(value As Image)
            PictureBox1.BackgroundImage = GDIPlusExtensions.TrimRoundAvatar(value, 200)
            _avatar = value
            Menu.PictureBox1.BackgroundImage = PictureBox1.BackgroundImage
        End Set
    End Property

    Public ReadOnly Property Menu As New UserMenu

    Private Sub UserAvatarMenu_Click(sender As Object, e As EventArgs) Handles Me.Click, Label1.Click, PictureBox1.Click
        Menu.Visible = Not Menu.Visible
    End Sub

    Public Sub RePositionMenu()
        Menu.Location = New Point(Me.Location.X + Me.Width - Menu.Width, Me.Height + Me.Location.Y)
    End Sub

    Private Sub UserAvatarMenu_Load(sender As Object, e As EventArgs) Handles Me.Load
        Call Me.Parent.Controls.Add(Menu)
        Menu.Visible = False
        Call RePositionMenu()
    End Sub
End Class
