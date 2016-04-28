Imports Microsoft.VisualBasic.Imaging

Public Class FormAuth

    Private Sub FormAuth_Load(sender As Object, e As EventArgs) Handles Me.Load
        Call New Microsoft.VisualBasic.Net.NETProtocol.Captcha().CaptchaImage.SaveAs("./captcha.png", ImageFormats.Png)
    End Sub
End Class
