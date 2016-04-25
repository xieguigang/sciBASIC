Imports System.Drawing
Imports System.Runtime.CompilerServices

Public Module hcBitmap

    <Extension>
    Public Function GetBinaryBitmap(res As Image) As Bitmap
        Dim bmp As New Bitmap(DirectCast(res.Clone, Image))
        bmp.Binarization
        Return bmp
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="curBitmap"></param>
    ''' <remarks>
    ''' http://www.codeproject.com/Articles/1094534/Image-Binarization-Using-Program-Languages
    ''' 
    ''' The .net Bitmap object keeps a reference to HBITMAP handle, Not to the underlying bitmap itself.
    ''' So, single pixel access call to <see cref="Bitmap.SetPixel"/>/<see cref="Bitmap.GetPixel"/> Or 
    ''' even retrieve Width/Height properties does something Like: 
    ''' lock handle In place-Get/Set value/unlock handle. It Is the most inefficient way To manipulate bitmaps In .NET. 
    ''' The author should read about <see cref="Bitmap.LockBits"/> first.
    ''' </remarks>
    <Extension> Public Sub Binarization(ByRef curBitmap As Bitmap)
        Dim iR As Integer = 0 ' Red
        Dim iG As Integer = 0 ' Green
        Dim iB As Integer = 0 ' Blue

        ' Lock the bitmap's bits.  
        Dim rect As New Rectangle(0, 0, curBitmap.Width, curBitmap.Height)
        Dim bmpData As Imaging.BitmapData =
            curBitmap.LockBits(rect, Imaging.ImageLockMode.ReadWrite, curBitmap.PixelFormat)
        ' Get the address of the first line.
        Dim ptr As IntPtr = bmpData.Scan0
        ' Declare an array to hold the bytes of the bitmap.
        Dim bytes As Integer = Math.Abs(bmpData.Stride) * curBitmap.Height

        Using rgbValues As Marshal.Byte = New Marshal.Byte(ptr, bytes)

            Dim counter As Integer

            ' Set every third value to 255. A 24bpp bitmap will binarization.  
            Do While counter < rgbValues.Length - 1
                ' Get the red channel
                iR = rgbValues.Raw(counter + 2)
                ' Get the green channel
                iG = rgbValues.Raw(counter + 1)
                ' Get the blue channel
                iB = rgbValues.Raw(counter + 0)
                ' If the gray value more than threshold and then set a white pixel.
                If (iR + iG + iB) \ 3 > 100 Then
                    ' White pixel
                    rgbValues.Raw(counter + 2) = 255
                    rgbValues.Raw(counter + 1) = 255
                    rgbValues.Raw(counter + 0) = 255
                Else
                    ' Black pixel
                    rgbValues.Raw(counter + 2) = 0
                    rgbValues.Raw(counter + 1) = 0
                    rgbValues.Raw(counter + 0) = 0
                End If

                counter += 3
            Loop
        End Using

        ' Unlock the bits.
        Call curBitmap.UnlockBits(bmpData)
    End Sub
End Module
