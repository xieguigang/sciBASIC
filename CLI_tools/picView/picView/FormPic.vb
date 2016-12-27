Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Parallel.Tasks

Public Class FormPic

    Dim timer As New UpdateThread(1000, AddressOf UpdateImage)
    Dim file$

    Public Sub UpdateImage()
        Static fl&

        Dim len& = file.FileLength

        If len > 0 AndAlso len <> fl Then
            fl = len
            Dim img As Image = GDIPlusExtensions.LoadImage(file)
            PictureBox1.BackgroundImage = img
        End If
    End Sub

    Private Sub PictureBox1_Click(sender As Object, e As EventArgs) Handles PictureBox1.Click

    End Sub

    Private Sub FormPic_Load(sender As Object, e As EventArgs) Handles Me.Load
        file = App.Command
        Call timer.Start()
    End Sub
End Class
