#Region "Microsoft.VisualBasic::cccc247a90a767eac97b91db48465b0e, sciBASIC#\tutorials\core.test\wmfImage.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 111
    '    Code Lines: 47
    ' Comment Lines: 34
    '   Blank Lines: 30
    '     File Size: 2.96 KB


    ' Module wmfImage
    ' 
    '     Function: colorsTest
    ' 
    '     Sub: Main, replaceColorsTest
    ' 
    ' /********************************************************************************/

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
