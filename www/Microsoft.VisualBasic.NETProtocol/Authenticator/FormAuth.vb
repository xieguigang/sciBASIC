#Region "Microsoft.VisualBasic::efd5952c1358648984db348369d7fc41, www\Microsoft.VisualBasic.NETProtocol\Authenticator\FormAuth.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Class FormAuth
    ' 
    '     Sub: FormAuth_Load
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging

Public Class FormAuth

    Private Sub FormAuth_Load(sender As Object, e As EventArgs) Handles Me.Load
        Call New Microsoft.VisualBasic.Net.NETProtocol.Captcha().CaptchaImage.SaveAs("./captcha.png", ImageFormats.Png)
    End Sub
End Class
