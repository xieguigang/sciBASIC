Imports System.Drawing

Module Program

    Sub Main()

        Call New Form1().ShowDialog()

        Dim i As Integer = 1

        For Each t In Microsoft.VisualBasic.Drawing.TextureResourceLoader.LoadInternalDefaultResource
            Call t.Save("x:\" & i & ".png")
            i += 1
        Next


        Call Microsoft.VisualBasic.Drawing.TextureResourceLoader.AdjustColor(Microsoft.VisualBasic.Drawing.TextureResourceLoader.LoadInternalDefaultResource.First, Color.SeaGreen).Save("x:\reddddd.bmp")

        Dim vec = New Microsoft.VisualBasic.Drawing.Drawing2D.Vectogram(1000, 1000)

        Call vec.AddTextElement("1234", New Font(FONT_FAMILY_MICROSOFT_YAHEI, 20), System.Drawing.Color.DarkCyan, vec.GDIDevice.Center)

        Call vec.AddCircle(Color.Red, New Point(100, 20), 100)

        Dim res = vec.ToImage

        Call res.Save("x:\ffff.png")

        Call New Microsoft.VisualBasic.Drawing.Drawing2D.DrawingScript(vec).ToScript.SaveTo("script.txt")

    End Sub

End Module
