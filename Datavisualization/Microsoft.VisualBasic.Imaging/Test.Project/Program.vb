Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D

Module Program

    Sub Main()

        Call New Form1().ShowDialog()

        Dim i As Integer = 1

        'For Each t In TextureResourceLoader.LoadInternalDefaultResource
        '    Call t.Save("x:\" & i & ".png")
        '    i += 1
        'Next


        '    Call TextureResourceLoader.AdjustColor(Microsoft.VisualBasic.Drawing.TextureResourceLoader.LoadInternalDefaultResource.First, Color.SeaGreen).Save("x:\reddddd.bmp")

        Dim vec = New Vectogram(1000, 1000)

        Call vec.AddTextElement("1234", New Font(FontFace.MicrosoftYaHei, 20), System.Drawing.Color.DarkCyan, vec.GDIDevice.Center)

        Call vec.AddCircle(Color.Red, New Point(100, 20), 100)

        Dim res = vec.ToImage

        Call res.Save("x:\ffff.png")

        Call New DrawingScript(vec).ToScript.SaveTo("script.txt")

    End Sub

End Module
