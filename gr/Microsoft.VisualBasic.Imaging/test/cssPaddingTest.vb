Imports System.Drawing
Imports Microsoft.VisualBasic.MIME.Html.CSS

Module cssPaddingTest

    Sub Main()
        Dim css As New CSSEnvirnment(New Size(1000, 1000), dpi:=300)
        Dim pad1 As Padding = "padding: 5% 5% 5% 5%;"
        Dim pad2 As Padding = "padding: 200px 300px 35% 45%;"



        Pause()
    End Sub
End Module
