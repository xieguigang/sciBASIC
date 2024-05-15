#Region "Microsoft.VisualBasic::af2dddbb8524fad28b67dfc3f06f1cf1, Microsoft.VisualBasic.Core\src\Extensions\Image\Bitmap\BitmapBuffer.vb"

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

    '   Total Lines: 412
    '    Code Lines: 223
    ' Comment Lines: 131
    '   Blank Lines: 58
    '     File Size: 14.87 KB


    '     Class BitmapBuffer
    ' 
    '         Properties: Height, Size, Stride, Width
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FromBitmap, FromImage, GetARGBStream, GetColor, GetEnumerator
    '                   GetImage, (+2 Overloads) GetIndex, (+3 Overloads) GetPixel, GetPixelsAll, IEnumerable_GetEnumerator
    '                   OutOfRange, ToPixel2D
    ' 
    '         Sub: Dispose, (+3 Overloads) SetPixel, WriteARGBStream
    ' 
    '         Operators: +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

Namespace Imaging.BitmapImage

    ''' <summary>
    ''' Unsafe memory pointer of the <see cref="Bitmap"/> data buffer.
    ''' (线程不安全的图片数据对象)
    ''' </summary>
    Public Class BitmapBuffer : Inherits Emit.Marshal.Byte
        Implements IDisposable
        Implements IEnumerable(Of Color)

        ReadOnly raw As Bitmap
        ReadOnly handle As BitmapData

        ''' <summary>
        ''' 图片可能是 BGRA 4通道
        ''' 也可能是 BGR 3通道的
        ''' </summary>
        ReadOnly channels As Integer

        Protected Sub New(ptr As IntPtr,
                          byts%,
                          raw As Bitmap,
                          handle As BitmapData,
                          channel As Integer)

            Call MyBase.New(ptr, byts)

            Me.raw = raw
            Me.handle = handle

            Me.Stride = handle.Stride
            Me.Width = raw.Width
            Me.Height = raw.Height
            Me.Size = New Size(Width, Height)
            Me.channels = channel
        End Sub

        ''' <summary>
        ''' The dimension width of the current bitmap buffer object
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Width As Integer
        ''' <summary>
        ''' The dimension height of the current bitmap buffer object
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Height As Integer
        ''' <summary>
        ''' the dimension size of current bitmap buffer object, 
        ''' it is constructed via the <see cref="Width"/> and 
        ''' <see cref="Height"/> data.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Size As Size
        Public ReadOnly Property Stride As Integer

        ''' <summary>
        ''' Gets a copy of the original raw image value that which constructed 
        ''' this bitmap object class
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetImage(Optional flush As Boolean = False) As Bitmap
            If flush Then
                Call Write()
            End If

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
        ''' <returns>B, G, R, [A]</returns>
        ''' <remarks>
        ''' ###### 2017-11-29 
        ''' 经过测试，对第一行的数据的计算没有问题
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetIndex(x As Integer, y As Integer) As Integer
            y = y * (Width * channels)
            x = x * channels
            Return x + y
        End Function

        Public Shared Function GetIndex(x As Integer, y As Integer, width As Integer, channels As Integer) As Integer
            y = y * (width * channels)
            x = x * channels
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
            Dim iA As Byte = 255

            If channels = 4 Then
                iA = buffer(i + 3)
            End If

            Dim iR As Byte = buffer(i + 2)
            Dim iG As Byte = buffer(i + 1)
            Dim iB As Byte = buffer(i + 0)

            Return Color.FromArgb(CInt(iA), CInt(iR), CInt(iG), CInt(iB))
        End Function

        ''' <summary>
        ''' get image data array in ARGB format
        ''' </summary>
        ''' <returns>
        ''' scan0.ToPointer
        ''' </returns>
        ''' <remarks>
        ''' helper function for hqx algorithm module
        ''' </remarks>
        Public Function GetARGBStream() As UInteger()
            Dim ints As UInteger() = New UInteger(buffer.Length / 4 - 1) {}
            Dim uint As Byte() = New Byte(4 - 1) {}
            Dim p As i32 = 0

            If channels = 4 Then
                For i As Integer = 0 To buffer.Length - 1 Step 4
                    'ints(i) = buffer(i + 3) ' A
                    'ints(i + 1) = buffer(i + 2) ' R
                    'ints(i + 2) = buffer(i + 1) ' G
                    'ints(i + 3) = buffer(i + 0) ' B
                    uint(0) = buffer(i) ' A
                    uint(1) = buffer(i + 1) ' R
                    uint(2) = buffer(i + 2) ' G
                    uint(3) = buffer(i + 3) ' B

                    ints(++p) = BitConverter.ToUInt32(uint, 0)
                Next
            Else
                ' channels = 3
                For i As Integer = 0 To buffer.Length - 1 Step 3
                    'ints(i) = 255 ' A
                    'ints(i + 1) = buffer(i + 2) ' R
                    'ints(i + 2) = buffer(i + 1) ' G
                    'ints(i + 3) = buffer(i + 0) ' B

                    uint(0) = 255 ' A
                    uint(1) = buffer(i) ' R
                    uint(2) = buffer(i + 1) ' G
                    uint(3) = buffer(i + 2) ' B

                    ints(++p) = BitConverter.ToUInt32(uint, 0)
                Next
            End If

            Return ints
        End Function

        Public Function GetPixelsAll() As IEnumerable(Of Color)
            Return GetPixel(New Rectangle(New Point, Size)).IteratesALL
        End Function

        Public Shared Function GetColor(uint As UInteger) As Color
            Dim bytes As Byte() = BitConverter.GetBytes(uint)
            Dim color As Color = Color.FromArgb(bytes(0), bytes(1), bytes(2), bytes(3))

            Return color
        End Function

        ''' <summary>
        ''' helper function for hqx algorithm module
        ''' </summary>
        ''' <param name="ints"></param>
        Public Sub WriteARGBStream(ints As UInteger())
            Dim p As i32 = 0

            If channels = 4 Then
                For i As Integer = 0 To buffer.Length - 1 Step 4
                    Dim uint As Byte() = BitConverter.GetBytes(ints(++p))

                    buffer(i) = uint(0)  ' A
                    buffer(i + 1) = uint(1)  ' R
                    buffer(i + 2) = uint(2)  ' G
                    buffer(i + 3) = uint(3)  ' B
                Next
            Else
                ' channels = 3
                For i As Integer = 0 To buffer.Length - 1 Step 3
                    Dim uint As Byte() = BitConverter.GetBytes(ints(++p))

                    buffer(i) = uint(1)  ' R
                    buffer(i + 1) = uint(2)  ' G
                    buffer(i + 2) = uint(3)  ' B
                Next
            End If
        End Sub

        ''' <summary>
        ''' row scans
        ''' </summary>
        ''' <param name="rect"></param>
        ''' <returns></returns>
        Public Iterator Function GetPixel(rect As Rectangle) As IEnumerable(Of Color())
            Dim row As New List(Of Color)

            For y As Integer = rect.Top To rect.Bottom
                For x As Integer = rect.Left To rect.Right
                    If Not OutOfRange(x, y) Then
                        Call row.Add(GetPixel(x, y))
                    End If
                Next

                Yield row.PopAll
            Next
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="channel">0r 1g 2b 3a</param>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Function GetPixel(channel As Integer, x As Integer, y As Integer) As Byte
            Dim i As Integer = GetIndex(x, y)

            If channel = 3 Then
                If channels = 4 Then
                    Return buffer(i + 3)
                Else
                    Return 255
                End If
            Else
                Return buffer(i + (2 - channel))
            End If
        End Function

        Public Shared Function ToPixel2D(i As Integer, width As Integer, Optional channels As Integer = 4) As Point
            i = i / channels

            Dim y As Integer = i / width
            Dim x As Integer = i Mod width

            Return New Point(x, y)
        End Function

        ''' <summary>
        ''' Sets the color of the specified pixel in this <see cref="Bitmap"/>.(这个函数线程不安全)
        ''' </summary>
        ''' <param name="x">The x-coordinate of the pixel to set. [0, width-1]</param>
        ''' <param name="y">The y-coordinate of the pixel to set. [0, height-1]</param>
        ''' <param name="color">
        ''' A <see cref="Color"/> structure that represents the color to assign to the specified
        ''' pixel.</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetPixel(x As Integer, y As Integer, color As Color)
            Dim i As Integer = GetIndex(x, y)

            If i < 0 Then
                Return
            End If
            If channels = 4 Then
                buffer(i + 3) = color.A
            End If

            buffer(i + 2) = color.R
            buffer(i + 1) = color.G
            buffer(i + 0) = color.B
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetPixel(x As Integer, y As Integer, R As Byte, G As Byte, B As Byte)
            Dim i As Integer = GetIndex(x, y)

            buffer(i + 2) = R
            buffer(i + 1) = G
            buffer(i + 0) = B
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="channel">0r 1g 2b 3a</param>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="val"></param>
        Public Sub SetPixel(channel As Integer, x As Integer, y As Integer, val As Byte)
            Dim i As Integer = GetIndex(x, y)

            If channel = 3 Then
                If channels = 4 Then
                    buffer(i + 3) = val
                Else
                    ' do nothing
                End If
            Else
                buffer(i + (2 - channel)) = val
            End If
        End Sub

        ''' <summary>
        ''' 这个函数会自动复制原始图片数据里面的东西的
        ''' </summary>
        ''' <param name="res"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function FromImage(res As Image) As BitmapBuffer
            Dim copy As New Bitmap(res.Width, res.Height, format:=PixelFormat.Format32bppArgb)
            Dim g As Graphics = Graphics.FromImage(copy)

            Call g.DrawImageUnscaled(res, New Point)
            Call g.Flush()
            Call g.Dispose()

            Return BitmapBuffer.FromBitmap(copy)
        End Function

        ''' <summary>
        ''' 使用这个函数进行写数据的话，会修改到原图
        ''' </summary>
        ''' <param name="curBitmap"></param>
        ''' <param name="mode"></param>
        ''' <returns></returns>
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
            Dim bytes As Integer = std.Abs(bmpData.Stride) * curBitmap.Height
            Dim pixels As Integer = curBitmap.Width * curBitmap.Height
            Dim channels As Integer

            If bytes = pixels * 3 Then
                channels = 3
            ElseIf bytes = pixels * 4 Then
                channels = 4
            Else
                Throw New NotImplementedException
            End If

            Return New BitmapBuffer(ptr, bytes, curBitmap, bmpData, channels)
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
