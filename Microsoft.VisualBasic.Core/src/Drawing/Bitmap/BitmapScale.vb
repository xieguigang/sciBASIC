#Region "Microsoft.VisualBasic::b7890cc9e4914d312c3b27738e90c39e, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\BitmapScale.vb"

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

    '   Total Lines: 350
    '    Code Lines: 188 (53.71%)
    ' Comment Lines: 115 (32.86%)
    '    - Xml Docs: 79.13%
    ' 
    '   Blank Lines: 47 (13.43%)
    '     File Size: 12.99 KB


    '     Module BitmapScale
    ' 
    '         Function: GetBinaryBitmap
    '         Enum BinarizationStyles
    ' 
    ' 
    ' 
    ' 
    '         Delegate Sub
    ' 
    '             Function: ByteLength, Colors, ContrastLookup, (+2 Overloads) Grayscale, (+2 Overloads) GrayScale
    '                       GrayScaleF
    ' 
    '             Sub: (+2 Overloads) AdjustContrast, Binarization, BitmapPixelScans, (+2 Overloads) scanInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit
Imports std = System.Math

Namespace Imaging.BitmapImage

    ''' <summary>
    ''' Grayscale and binarization extensions
    ''' </summary>
    Public Module BitmapScale

        ''' <summary>
        ''' A, R, G, B
        ''' </summary>
        Friend Const PixelSize% = 4
        Friend Const RGBSize% = 3

        <Extension>
        Public Function GetBinaryBitmap(res As Image,
                                        Optional style As BinarizationStyles = BinarizationStyles.Binary) As Bitmap

            Dim bmp As New Bitmap(DirectCast(res.Clone, Image))
            Call bmp.Binarization(style)
            Return bmp
        End Function

        Public Enum BinarizationStyles
            SparseGray = 3
            Binary = 4
        End Enum

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="byts">Unmanaged memory pointer that point to the bitmap data buffer.</param>
        Public Delegate Sub PixelScanPointer(byts As Marshal.Byte)

        ''' <summary>
        ''' A generic bitmap pixel scan framework that using memory pointer
        ''' </summary>
        ''' <param name="curBitmap"></param>
        ''' <param name="scan"></param>
        <Extension>
        Public Sub BitmapPixelScans(ByRef curBitmap As Bitmap, scan As PixelScanPointer)
#If NET48 Then
            ' Lock the bitmap's bits.  
            Dim rect As New Rectangle(0, 0, curBitmap.Width, curBitmap.Height)
            Dim bmpData As BitmapData = curBitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                curBitmap.PixelFormat
            )
            ' Get the address of the first line.
            Dim ptr As IntPtr = bmpData.Scan0
            ' Declare an array to hold the bytes of the bitmap.
            Dim bytes As Integer = std.Abs(bmpData.Stride) * curBitmap.Height

            Using rgbValues As Marshal.Byte = New Marshal.Byte(ptr, bytes)
                ' Calls unmanaged memory write when this 
                ' memory pointer was disposed
                Call scan(rgbValues)
            End Using

            ' Unlock the bits.
            Call curBitmap.UnlockBits(bmpData)
#Else
            Call scan(curBitmap.MemoryBuffer)
#End If

        End Sub

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
        ''' The author should read about ``Bitmap.LockBits`` first.
        ''' </remarks>
        <Extension>
        Public Sub Binarization(ByRef curBitmap As Bitmap, Optional style As BinarizationStyles = BinarizationStyles.Binary)
            Call curBitmap.BitmapPixelScans(Sub(byts) BitmapScale.scanInternal(byts, style))
        End Sub

        ''' <summary>
        ''' Binarization: just black and white
        ''' </summary>
        ''' <param name="byts"></param>
        ''' <param name="style"></param>
        Private Sub scanInternal(byts As Marshal.Byte, style As BinarizationStyles)
            Dim iR As Integer = 0 ' Red
            Dim iG As Integer = 0 ' Green
            Dim iB As Integer = 0 ' Blue

            ' Set every third value to 255. A 24bpp bitmap will binarization.  
            Do While Not byts.NullEnd(3)
                ' Get the red channel
                iR = byts(2)
                ' Get the green channel
                iG = byts(1)
                ' Get the blue channel
                iB = byts(0)

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

                ' move forward this memory pointer by a specific offset.
                byts += style
            Loop
        End Sub

        Private Function ContrastLookup(contrast As Double) As Byte()
            Dim lookups As Byte() = New Byte(255) {}
            Dim newValue As Double = 0
            Dim c As Double = (100.0 + contrast) / 100.0

            c *= c

            For i As Integer = 0 To 255
                newValue = CDbl(i)
                newValue /= 255.0
                newValue -= 0.5
                newValue *= c
                newValue += 0.5
                newValue *= 255

                If newValue < 0 Then
                    newValue = 0
                End If
                If newValue > 255 Then
                    newValue = 255
                End If

                lookups(i) = CByte(Truncate(newValue))
            Next

            Return lookups
        End Function

        ''' <summary>
        ''' Adjust the contrast of an image.
        ''' (调整图像的对比度)
        ''' </summary>
        ''' <param name="bmp"></param>
        ''' <param name="contrast">
        ''' Used to set the contrast (-100 to 100)
        ''' </param>
        ''' <remarks>
        ''' https://stackoverflow.com/questions/3115076/adjust-the-contrast-of-an-image-in-c-sharp-efficiently
        ''' </remarks>
        <Extension>
        Public Sub AdjustContrast(ByRef bmp As Bitmap, contrast#)
            Using bitmapdata As BitmapBuffer = BitmapBuffer.FromBitmap(bmp)
                Call bitmapdata.AdjustContrast(contrast)
            End Using
        End Sub

        ''' <summary>
        ''' Adjust the contrast of an image.
        ''' (调整图像的对比度)
        ''' </summary>
        ''' <param name="destPixels">target bitmap data buffer for make contrast processing.</param>
        ''' <param name="contrast">
        ''' Used to set the contrast (-100 to 100)
        ''' </param>
        ''' <remarks>
        ''' https://stackoverflow.com/questions/3115076/adjust-the-contrast-of-an-image-in-c-sharp-efficiently
        ''' </remarks>
        <Extension>
        Public Sub AdjustContrast(ByRef destPixels As BitmapBuffer, contrast#)
            Dim contrastLookup As Byte()
            Dim key As String = contrast.ToString("F4")

            Static lookups As New Dictionary(Of String, Byte())

            contrastLookup = lookups.ComputeIfAbsent(key, Function() BitmapScale.ContrastLookup(contrast))

            For y As Integer = 0 To destPixels.Height - 1
                destPixels += destPixels.Stride

                For x As Integer = 0 To destPixels.Width - 1
                    Dim i As Integer = x * PixelSize

                    If i + destPixels.Position < destPixels.Length Then
                        destPixels(i) = contrastLookup(destPixels(i))
                        destPixels(i + 1) = contrastLookup(destPixels(i + 1))
                        destPixels(i + 2) = contrastLookup(destPixels(i + 2))
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' convert color bitmaps to grayscale.(灰度图)
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Grayscale(source As Image) As Bitmap
            Dim curBitmap As New Bitmap(source)
            Dim scanInternal As PixelScanPointer = AddressOf BitmapScale.scanInternal
            Call curBitmap.BitmapPixelScans(scanInternal)
            Return curBitmap
        End Function

        ''' <summary>
        ''' 将彩色图转换为灰度图
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 这个函数会直接改变输入数据的值，如果不想对输入的位图对象做修改，请先复制一份，再使用这个函数进行灰度处理
        ''' </remarks>
        <Extension>
        Public Function Grayscale(ByRef source As BitmapBuffer,
                                  Optional wr As Single = 0.3,
                                  Optional wg As Single = 0.59,
                                  Optional wb As Single = 0.11) As BitmapBuffer
            Call BitmapScale.scanInternal(source, wr, wg, wb)
            Return source
        End Function

        <Extension>
        Friend Sub scanInternal(byts As Marshal.Byte,
                                Optional wr As Single = 0.3,
                                Optional wg As Single = 0.59,
                                Optional wb As Single = 0.11)

            Dim iR As Integer = 0 ' Red
            Dim iG As Integer = 0 ' Green
            Dim iB As Integer = 0 ' Blue
            Dim luma%

            ' Set every third value to 255. A 24bpp bitmap will binarization.  
            Do While Not byts.NullEnd(3)
                ' Get the red channel
                iR = byts(2)
                ' Get the green channel
                iG = byts(1)
                ' Get the blue channel
                iB = byts(0)

                luma = GrayScale(iR, iG, iB, wr, wg, wb)

                ' gray pixel
                byts(2) = luma
                byts(1) = luma
                byts(0) = luma

                byts += BinarizationStyles.Binary
            Loop
        End Sub

        ''' <summary>
        ''' luma
        ''' </summary>
        ''' <param name="R"></param>
        ''' <param name="G"></param>
        ''' <param name="B"></param>
        ''' <returns>[0,255]</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GrayScale(R%, G%, B%,
                                  Optional wr As Single = 0.3,
                                  Optional wg As Single = 0.59,
                                  Optional wb As Single = 0.11) As Integer

            Return CInt(Truncate(R * wr + G * wg + B * wb))
        End Function

        ''' <summary>
        ''' luma
        ''' </summary>
        ''' <param name="R"></param>
        ''' <param name="G"></param>
        ''' <param name="B"></param>
        ''' <returns>[0,255]</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GrayScaleF(R%, G%, B%,
                                   Optional wr As Single = 0.3,
                                   Optional wg As Single = 0.59,
                                   Optional wb As Single = 0.11) As Single

            Return R * wr + G * wg + B * wb
        End Function

        ''' <summary>
        ''' Color gray scale
        ''' </summary>
        ''' <param name="c"></param>
        ''' <returns>[0,255]</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GrayScale(c As Color,
                                  Optional wr As Single = 0.3,
                                  Optional wg As Single = 0.59,
                                  Optional wb As Single = 0.11) As Integer

            Return GrayScale(c.R, c.G, c.B, wr, wg, wb)
        End Function

        ''' <summary>
        ''' How many bytes does this bitmap contains?
        ''' </summary>
        ''' <param name="rect">The bitmap size or a specific region on the bitmap.</param>
        ''' <returns></returns>
        <Extension>
        Public Function ByteLength(rect As Rectangle) As Integer
            Dim width As Integer = rect.Width * PixelSize  ' ARGB -> 4
            Return width * rect.Height
        End Function

        ''' <summary>
        ''' Convert the bitmap memory bytes into pixels
        ''' </summary>
        ''' <param name="buffer"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function Colors(buffer As Byte()) As IEnumerable(Of Color)
            Dim iR As Byte
            Dim iG As Byte
            Dim iB As Byte

            ' offset ARGB 4 bytes
            For i As Integer = 0 To buffer.Length - 1 Step PixelSize
                iR = buffer(i + 2)
                iG = buffer(i + 1)
                iB = buffer(i + 0)

                Yield Color.FromArgb(CInt(iR), CInt(iG), CInt(iB))
            Next
        End Function
    End Module
End Namespace
