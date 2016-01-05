Public Class Form2

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        MsgBox(23)
    End Sub

    Public Shared Sub Main()
        Call (New Form2).ShowDialog()
    End Sub

    Private Sub PreviewHandle1_Load(sender As Object, e As EventArgs) Handles PreviewHandle1.Load
        ' PreviewHandle1.BackgroundImage = Image.FromFile("E:\SIYU_DNAuction\Message-Notification-UI-Box-PSD.jpg")
    End Sub

    Private Sub Ping1_Load(sender As Object, e As EventArgs) Handles Ping1.Load
        '  Ping1.IPAddress = "112.74.64.178"
        Ping1.HostName = "codeproject.com"
    End Sub

    Dim i As Integer = 1

    Private Sub Ping1_MouseClick(sender As Object, e As MouseEventArgs) Handles Ping1.MouseClick
        Call Ping1.Updates(i, 0)
        i += 1
        If i > 5 Then
            i = 1
        End If
    End Sub
End Class