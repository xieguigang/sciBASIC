Imports System.Drawing
Imports System.Runtime.CompilerServices

Public Module hcBitmap

    <Extension>
    Public Function GetBinaryBitmap(res As Image) As Bitmap
        Dim bmp As New Bitmap(DirectCast(res.Clone, Image))
        bmp.Binarization
        Return bmp
    End Function

    <Extension> Public Sub Binarization(ByRef curBitmap As Bitmap)
        Dim iR As Integer = 0 ' Red
        Dim iG As Integer = 0  ' Green
        Dim iB As Integer = 0   ' Blue

        ' Lock the bitmap's bits.  
        Dim rect As New Rectangle(0, 0, curBitmap.Width, curBitmap.Height)
        Dim bmpData As Imaging.BitmapData = curBitmap.LockBits(rect, Imaging.ImageLockMode.ReadWrite, curBitmap.PixelFormat)
        ' Get the address of the first line.
        Dim ptr As IntPtr = bmpData.Scan0
        ' Declare an array to hold the bytes of the bitmap.
        Dim bytes As Integer = Math.Abs(bmpData.Stride) * curBitmap.Height

        Using rgbValues As Marshal.Byte = New Marshal.Byte(ptr, bytes)

            ' Set every third value to 255. A 24bpp bitmap will binarization.  
            For counter As Integer = 0 To rgbValues.Length - 1 Step 3
                ' Get the red channel
                iR = rgbValues(counter + 2)
                ' Get the green channel
                iG = rgbValues(counter + 1)
                ' Get the blue channel
                iB = rgbValues(counter + 0)
                ' If the gray value more than threshold and then set a white pixel.
                If (iR + iG + iB) \ 3 > 100 Then
                    ' White pixel
                    rgbValues(counter + 2) = 255
                    rgbValues(counter + 1) = 255
                    rgbValues(counter + 0) = 255
                Else
                    ' Black pixel
                    rgbValues(counter + 2) = 0
                    rgbValues(counter + 1) = 0
                    rgbValues(counter + 0) = 0
                End If
            Next
        End Using

        ' Unlock the bits.
        Call curBitmap.UnlockBits(bmpData)
    End Sub
End Module
