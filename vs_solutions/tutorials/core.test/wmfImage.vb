#Region "Microsoft.VisualBasic::13b86d27c97a627c7ecac05e61be7e6a, wmfImage.vb"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module wmfImage
    ' 
    '     Function: colorsTest
    ' 
    '     Sub: Main, replaceColorsTest
    ' 
    ' 

#End Region

#Region "Microsoft.VisualBasic::f573f300673cb9ee52c4ef73c25a91d5, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module wmfImage
    ' 
    '     Function: colorsTest
    ' 
    '     Sub: Main, replaceColorsTest
    ' 
    ' 

#End Region

#Region "Microsoft.VisualBasic::1be04f96bf1e0ee3fedc9390bfca589f, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module wmfImage
    ' 
    '     Function: colorsTest
    ' 
    ' 
    '     Sub: Main, replaceColorsTest
    ' 
    ' 
    ' 

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage

Module wmfImage

    Public Function colorsTest() As Color
        Dim bitmap As New Bitmap("D:\GCModeller\src\runtime\sciBASIC#\tutorials\contents\BitmapBufferTest.png")
        Dim colors As New List(Of Color)

        For x As Integer = 0 To bitmap.Width - 1
            For y As Integer = 0 To bitmap.Height - 1
                colors.Add(bitmap.GetPixel(x, y))
            Next
        Next

        Dim buffer = BitmapBuffer.FromBitmap(bitmap)
        Dim colors2 As New List(Of Color)

        For x As Integer = 0 To buffer.Width - 1
            For y As Integer = 0 To buffer.Height - 1
                colors2.Add(buffer.GetPixel(x, y))
            Next
        Next

        Console.WriteLine(colors.SequenceEqual(colors2))


        Pause()
    End Function

    Sub Main()

        Call colorsTest()

        Call replaceColorsTest()

        Using wmf As New Wmf(New Size(200, 200), "./test.wmf")
            Call wmf.Graphics.DrawString("Hello world!", New Font(FontFace.BookmanOldStyle, 16, FontStyle.Bold), Brushes.Red, New Point(20, 20))
            Call wmf.Graphics.FillPie(Brushes.Green, New Rectangle(100, 100, 50, 50), 0, 196)
            Call wmf.Graphics.FillRectangle(Brushes.Blue, New RectangleF(0, 100, 60, 60))
        End Using
    End Sub

    Sub replaceColorsTest()

        Dim bitmap = New Bitmap("C:\Users\xieguigang\Downloads\3.bmp")

        bitmap = bitmap.ColorReplace(Color.FromArgb(239, 239, 239), Color.White)
        bitmap = bitmap.ColorReplace(Color.FromArgb(236, 236, 236), Color.White)
        bitmap = bitmap.ColorReplace(Color.FromArgb(241, 241, 241), Color.White)
        bitmap = bitmap.ColorReplace(Color.FromArgb(245, 245, 245), Color.White)
        bitmap = bitmap.ColorReplace(Color.FromArgb(242, 242, 242), Color.White)
        bitmap = bitmap.ColorReplace(Color.FromArgb(250, 250, 250), Color.White)

        Call bitmap.SaveAs("C:\Users\xieguigang\Downloads\3.png", ImageFormats.Png)


        Pause()
    End Sub
End Module


