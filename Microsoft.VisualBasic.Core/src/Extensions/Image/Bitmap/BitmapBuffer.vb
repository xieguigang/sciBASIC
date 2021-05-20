#Region "Microsoft.VisualBasic::5d3a8020e3457d39268c231f63cb01ac, Microsoft.VisualBasic.Core\src\Extensions\Image\Bitmap\BitmapBuffer.vb"

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

    '     Class BitmapBuffer
    ' 
    '         Properties: Height, Size, Stride, Width
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FromBitmap, FromImage, GetEnumerator, GetImage, GetIndex
    '                   GetPixel, IEnumerable_GetEnumerator, OutOfRange
    ' 
    '         Sub: Dispose, SetPixel
    ' 
    '         Operators: +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.CompilerServices
Imports stdNum = System.Math

Namespace Imaging.BitmapImage

    ''' <summary>
    ''' Unsafe memory pointer of the <see cref="Bitmap"/> data buffer.(线程不安全的图片数据对象)
    ''' </summary>
    Public Class BitmapBuffer : Inherits Emit.Marshal.Byte
        Implements IDisposable
        Implements IEnumerable(Of Color)

        ReadOnly raw As Bitmap
        ReadOnly handle As BitmapData

        Protected Sub New(ptr As IntPtr,
                          byts%,
                          raw As Bitmap,
                          handle As BitmapData)

            Call MyBase.New(ptr, byts)

            Me.raw = raw
            Me.handle = handle

            Stride = handle.Stride
            Width = raw.Width
            Height = raw.Height
            Size = New Size(Width, Height)
        End Sub

        Public ReadOnly Property Width As Integer
        Public ReadOnly Property Height As Integer
        Public ReadOnly Property Size As Size
        Public ReadOnly Property Stride As Integer

        ''' <summary>
        ''' Gets a copy of the original raw image value that which constructed this bitmap object class
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetImage() As Bitmap
            Return DirectCast(raw.Clone, Bitmap)
        End Function

        ' pixel:  (1,1)(2,1)(3,1)(4,1)(1,2)(2,2)(3,2)(4,2)
        ' buffer: BGRA|BGRA|BGRA|BGRA|BGRA|BGRA|BGRA|BGRA|
        ' bitmap pixels:
        ' 
        '    (1,1)(2,1)(3,1)(4,1)
        '    (1,2)(2,2)(3,2)(4,2)
        '
        ' width  = 4 pixel
        ' height = 2 pixel

        ''' <summary>
        ''' 返回第一个元素的位置
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns>B, G, R</returns>
        ''' <remarks>
        ''' ###### 2017-11-29 
        ''' 经过测试，对第一行的数据的计算没有问题
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetIndex(x As Integer, y As Integer) As Integer
            y = y * (Width * 4)
            x = x * 4
            Return x + y
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function OutOfRange(x%, y%) As Boolean
            Return x < 0 OrElse x >= Width OrElse y < 0 OrElse y >= Height
        End Function

        ''' <summary>
        ''' Gets the color of the specified pixel in this <see cref="Bitmap"/>.
        ''' (<paramref name="x"/>和<paramref name="y"/>都是以零为底的)
        ''' </summary>
        ''' <param name="x">The x-coordinate of the pixel to retrieve.</param>
        ''' <param name="y">The y-coordinate of the pixel to retrieve.</param>
        ''' <returns>
        ''' A <see cref="Color"/> structure that represents the color of the specified pixel.
        ''' </returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetPixel(x As Integer, y As Integer) As Color
            Dim i As Integer = GetIndex(x, y)
            Dim iA As Byte = buffer(i + 3)
            Dim iR As Byte = buffer(i + 2)
            Dim iG As Byte = buffer(i + 1)
            Dim iB As Byte = buffer(i + 0)

            Return Color.FromArgb(CInt(iA), CInt(iR), CInt(iG), CInt(iB))
        End Function

        ''' <summary>
        ''' Sets the color of the specified pixel in this System.Drawing.Bitmap.(这个函数线程不安全)
        ''' </summary>
        ''' <param name="x">The x-coordinate of the pixel to set.</param>
        ''' <param name="y">The y-coordinate of the pixel to set.</param>
        ''' <param name="color">
        ''' A System.Drawing.Color structure that represents the color to assign to the specified
        ''' pixel.</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetPixel(x As Integer, y As Integer, color As Color)
            Dim i As Integer = GetIndex(x, y)

            buffer(i + 3) = color.A
            buffer(i + 2) = color.R
            buffer(i + 1) = color.G
            buffer(i + 0) = color.B
        End Sub

        ''' <summary>
        ''' 这个函数会自动复制原始图片数据里面的东西的
        ''' </summary>
        ''' <param name="res"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function FromImage(res As Image) As BitmapBuffer
            Return BitmapBuffer.FromBitmap(New Bitmap(res))
        End Function

        Public Shared Function FromBitmap(curBitmap As Bitmap, Optional mode As ImageLockMode = ImageLockMode.ReadWrite) As BitmapBuffer
            ' Lock the bitmap's bits.  
            Dim rect As New Rectangle(0, 0, curBitmap.Width, curBitmap.Height)
            Dim bmpData As BitmapData = curBitmap.LockBits(
                rect:=rect,
                flags:=mode,
                format:=curBitmap.PixelFormat
            )

            ' Get the address of the first line.
            Dim ptr As IntPtr = bmpData.Scan0
            ' Declare an array to hold the bytes of the bitmap.
            Dim bytes As Integer = stdNum.Abs(bmpData.Stride) * curBitmap.Height

            Return New BitmapBuffer(ptr, bytes, curBitmap, bmpData)
        End Function

        Protected Overrides Sub Dispose(disposing As Boolean)
            Call Write()
            Call raw.UnlockBits(handle)
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of Color) Implements IEnumerable(Of Color).GetEnumerator
            For Each x As Color In buffer.Colors
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        ''' <summary>
        ''' Current pointer location offset to next position
        ''' </summary>
        ''' <param name="bmp"></param>
        ''' <param name="offset%"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator +(bmp As BitmapBuffer, offset%) As BitmapBuffer
            bmp.index += offset
            Return bmp
        End Operator
    End Class
End Namespace
