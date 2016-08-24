#Region "Microsoft.VisualBasic::17bcc0ebafcb3768ec3344b92262c9e2, ..\visualbasic_App\UXFramework\UWP\UWP\UserAvatarMenu.vb"

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
