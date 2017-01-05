#Region "Microsoft.VisualBasic::c0172c33af991fa46f14b594e5bff677, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Image\Bitmap\hcBitmap.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit

Namespace Imaging

    Public Module hcBitmap

        <Extension>
        Public Function GetBinaryBitmap(res As Image, Optional style As BinarizationStyles = BinarizationStyles.Binary) As Bitmap
            Dim bmp As New Bitmap(DirectCast(res.Clone, Image))
            bmp.Binarization(style)
            Return bmp
        End Function

        Public Enum BinarizationStyles
            SparseGray = 3
            Binary = 4
        End Enum

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
        <Extension> Public Sub Binarization(ByRef curBitmap As Bitmap, Optional style As BinarizationStyles = BinarizationStyles.Binary)
            Dim iR As Integer = 0 ' Red
            Dim iG As Integer = 0 ' Green
            Dim iB As Integer = 0 ' Blue

            ' Lock the bitmap's bits.  
            Dim rect As New Rectangle(0, 0, curBitmap.Width, curBitmap.Height)
            Dim bmpData As BitmapData =
            curBitmap.LockBits(rect, ImageLockMode.ReadWrite, curBitmap.PixelFormat)
            ' Get the address of the first line.
            Dim ptr As IntPtr = bmpData.Scan0
            ' Declare an array to hold the bytes of the bitmap.
            Dim bytes As Integer = Math.Abs(bmpData.Stride) * curBitmap.Height

            Using rgbValues As Marshal.Byte = New Marshal.Byte(ptr, bytes)
                Dim byts As Marshal.Byte = rgbValues

                ' Set every third value to 255. A 24bpp bitmap will binarization.  
                Do While Not rgbValues.NullEnd(3)
                    ' Get the red channel
                    iR = rgbValues(2)
                    ' Get the green channel
                    iG = rgbValues(1)
                    ' Get the blue channel
                    iB = rgbValues(0)
                    ' If the gray value more than threshold and then set a white pixel.
                    If (iR + iG + iB) / 3 > 100 Then
                        ' White pixel
                        byts(2) = 255
                        byts(1) = 255
                        byts(0) = 255
                    Else
                        ' Black pixel
                        byts(2) = 0
                        byts(1) = 0
                        byts(0) = 0
                    End If

                    byts += style
                Loop
            End Using

            ' Unlock the bits.
            Call curBitmap.UnlockBits(bmpData)
        End Sub

        ''' <summary>
        ''' convert color bitmaps to grayscale.(灰度图)
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension> Public Function Grayscale(source As Image) As Bitmap
            Dim curBitmap As New Bitmap(source)

            Dim iR As Integer = 0 ' Red
            Dim iG As Integer = 0 ' Green
            Dim iB As Integer = 0 ' Blue

            ' Lock the bitmap's bits.  
            Dim rect As New Rectangle(0, 0, curBitmap.Width, curBitmap.Height)
            Dim bmpData As BitmapData = curBitmap.LockBits(
                rect, ImageLockMode.ReadWrite, curBitmap.PixelFormat)
            ' Get the address of the first line.
            Dim ptr As IntPtr = bmpData.Scan0
            ' Declare an array to hold the bytes of the bitmap.
            Dim bytes As Integer = Math.Abs(bmpData.Stride) * curBitmap.Height

            Using rgbValues As Marshal.Byte = New Marshal.Byte(ptr, bytes)
                Dim byts As Marshal.Byte = rgbValues

                ' Set every third value to 255. A 24bpp bitmap will binarization.  
                Do While Not rgbValues.NullEnd(3)
                    ' Get the red channel
                    iR = rgbValues(2)
                    ' Get the green channel
                    iG = rgbValues(1)
                    ' Get the blue channel
                    iB = rgbValues(0)

                    Dim luma = CInt(Math.Truncate(iR * 0.3 + iG * 0.59 + iB * 0.11))
                    ' gray pixel
                    byts(2) = luma
                    byts(1) = luma
                    byts(0) = luma
                    byts += BinarizationStyles.Binary
                Loop
            End Using

            ' Unlock the bits.
            Call curBitmap.UnlockBits(bmpData)

            Return curBitmap
        End Function

        ''' <summary>
        ''' Color gray scale
        ''' </summary>
        ''' <param name="c"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GrayScale(c As Color) As Integer
            Dim luma% = CInt(Math.Truncate(c.R * 0.3 + c.G * 0.59 + c.B * 0.11))
            Return luma
        End Function

        <Extension>
        Public Function ByteLength(rect As Rectangle) As Integer
            Dim width As Integer = rect.Width * 4  ' ARGB -> 4
            Return width * rect.Height
        End Function

        <Extension>
        Public Iterator Function Colors(buffer As Byte()) As IEnumerable(Of Color)
            Dim iR As Byte
            Dim iG As Byte
            Dim iB As Byte

            For i As Integer = 0 To buffer.Length - 1 Step 4
                iR = buffer(i + 2)
                iG = buffer(i + 1)
                iB = buffer(i + 0)

                Yield Color.FromArgb(CInt(iR), CInt(iG), CInt(iB))
            Next
        End Function
    End Module
End Namespace
