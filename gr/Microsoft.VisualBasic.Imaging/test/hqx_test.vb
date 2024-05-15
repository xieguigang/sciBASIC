#Region "Microsoft.VisualBasic::3995fd60face0724f4ba5557e5393a6e, gr\Microsoft.VisualBasic.Imaging\test\hqx_test.vb"

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

    '   Total Lines: 43
    '    Code Lines: 26
    ' Comment Lines: 4
    '   Blank Lines: 13
    '     File Size: 1.44 KB


    ' Module hqx_test
    ' 
    '     Sub: bufferTest, hqx_4, Main1
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Drawing2D.HeatMap

Module hqx_test

    Dim sourcePixels As String = "D:\GCModeller\src\R-sharp\test\demo\imageProcessing\hqx\pixel.bmp"

    Sub Main1()
        ' Call bufferTest()
        Call hqx_4()
    End Sub

    Sub hqx_4()
        Dim img As Bitmap = New Bitmap(sourcePixels.LoadImage)
        Dim raster As New RasterScaler(img)

        'Dim bmpData = img.LockBits(New Rectangle(Point.Empty, img.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
        'Dim destData = img.LockBits(New Rectangle(Point.Empty, img.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb)
        'Dim sp As IntPtr = destData.Scan0


        Call raster.Scale(hqx:=hqx.HqxScales.Hqx_4x).SaveAs($"{App.HOME}/pixels_hqx_4x.png")

        Pause()
    End Sub

    Sub bufferTest()
        Dim img As Bitmap = New Bitmap(sourcePixels.LoadImage)
        Dim blank As New Bitmap(img.Width, img.Height)
        Dim buffer As BitmapBuffer = BitmapBuffer.FromBitmap(img)
        Dim bytes = buffer.GetARGBStream

        Dim anotherBlank As BitmapBuffer = BitmapBuffer.FromBitmap(blank)

        Call anotherBlank.WriteARGBStream(bytes)
        Call anotherBlank.Dispose()

        Call blank.SaveAs($"{App.HOME}/pixels.png")
    End Sub

End Module
