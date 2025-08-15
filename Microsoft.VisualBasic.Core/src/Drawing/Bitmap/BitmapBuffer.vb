#Region "Microsoft.VisualBasic::43ed9200d171c2d04c2acb1f82851a99, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\BitmapBuffer.vb"

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

    '   Total Lines: 702
    '    Code Lines: 414 (58.97%)
    ' Comment Lines: 179 (25.50%)
    '    - Xml Docs: 81.56%
    ' 
    '   Blank Lines: 109 (15.53%)
    '     File Size: 23.93 KB


    '     Class BitmapBuffer
    ' 
    '         Properties: Height, Size, SortBins, Stride, Width
    ' 
    '         Constructor: (+5 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) FromBitmap, FromImage, GetAlpha, GetARGB, GetARGBStream
    '                   GetBlue, GetColor, GetEnumerator, GetGreen, GetImage
    '                   (+2 Overloads) GetIndex, (+3 Overloads) GetPixel, GetPixelChannels, GetPixelsAll, GetRed
    '                   OutOfRange, ToPixel2D, ToString, Unpack, White
    ' 
    '         Sub: Dispose, (+2 Overloads) Save, SetAlpha, SetBlue, SetGreen
    '              (+4 Overloads) SetPixel, SetRed, WriteARGBStream
    ' 
    '         Operators: +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.Linq
Imports BitsPerPixelEnum = Microsoft.VisualBasic.Imaging.BitmapImage.FileStream.BitsPerPixelEnum
Imports MemoryBmp = Microsoft.VisualBasic.Imaging.BitmapImage.FileStream.Bitmap
Imports std = System.Math

Namespace Imaging.BitmapImage

    ''' <summary>
    ''' Unsafe memory pointer of the <see cref="Bitmap"/> data buffer.
    ''' </summary>
    ''' <remarks>
    ''' (线程不安全的图片数据对象)
    ''' </remarks>
    Public Class BitmapBuffer : Inherits Emit.Marshal.Byte
        Implements IDisposable
        Implements Enumeration(Of Color)

#If NET48 Then
        ReadOnly raw As Bitmap
        ReadOnly handle As BitmapData
#End If

        ''' <summary>
        ''' current bitmap data is construct from a pixel data array, not read from memory via pointer.
        ''' </summary>
        ReadOnly memoryBuffer As Boolean = False

        ''' <summary>
        ''' 图片可能是 BGRA 4通道
        ''' 也可能是 BGR 3通道的
        ''' </summary>
        ReadOnly channels As Integer

        Public ReadOnly Property SortBins As Dictionary(Of Byte, Integer)
            Get
                Return buffer _
                    .GroupBy(Function(b) b) _
                    .ToDictionary(Function(b) b.Key,
                                  Function(b)
                                      Return b.Count
                                  End Function)
            End Get
        End Property

#If NET48 Then
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
            Me.memoryBuffer = False
        End Sub
#End If

        ''' <summary>
        ''' Make the memory data copy
        ''' </summary>
        ''' <param name="ptr">the memory data will be copy via this pointer</param>
        ''' <param name="byts"></param>
        ''' <param name="channel"></param>
        Sub New(ptr As IntPtr, byts%, channel As Integer)
            Call MyBase.New(ptr, byts)

            Me.memoryBuffer = False
            Me.channels = channel

            Throw New NotImplementedException
        End Sub

        Sub New(memory As Byte(), size As Size, channel As Integer)
            Call MyBase.New(memory)

            channels = channel
            memoryBuffer = True

            _Stride = size.Width * channel
            _Size = size
            _Width = size.Width
            _Height = size.Height
        End Sub

        Sub New(width As Integer, height As Integer, Optional channels As Integer = 4)
            Call Me.New(New Byte(width * height * channels - 1) {}, New Size(width, height), channels)
        End Sub

        Sub New(pixels As Color(,), size As Size)
            Call MyBase.New(Unpack(pixels, size))

            channels = 4 ' argb
            memoryBuffer = True

            _Stride = size.Width * channels
            _Size = size
            _Width = size.Width
            _Height = size.Height
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

        ''' <summary>
        ''' Stride is the number of bytes your code must iterate past to reach the next vertical pixel.
        ''' </summary>
        ''' <returns>
        ''' always be width * pixel_size on .NET 8.0 runtime;
        ''' may be not matched with width * pixel_size on .net 4.8 runtime
        ''' </returns>
        Public ReadOnly Property Stride As Integer

        Public Shared Function White(width As Integer, height As Integer) As BitmapBuffer
            Dim bytes As Byte() = New Byte(width * height * 4 - 1) {}
            Call bytes.fill(255)
            Return New BitmapBuffer(bytes, New Size(width, height), 4)
        End Function

        ''' <summary>
        ''' get the pixel channels in memory buffer
        ''' </summary>
        ''' <returns>
        ''' 3 - for 24bit rgb pixel format
        ''' 4 - for 32bit argb pixel format
        ''' </returns>
        Public Function GetPixelChannels() As Integer
            Return channels
        End Function

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

#If NET48 Then
            Return DirectCast(raw.Clone, Bitmap)
#Else
            Return New Bitmap(Me)
#End If
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

        Public Function GetARGB() As Color(,)
            Dim pixels As Color(,) = New Color(Height - 1, Width - 1) {}

            For y As Integer = 0 To Height
                For x As Integer = 0 To Width
                    If Not OutOfRange(x, y) Then
                        pixels(y, x) = GetPixel(x, y)
                    End If
                Next
            Next

            Return pixels
        End Function

        ''' <summary>
        ''' get the alpha channel data from a given pixel
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Function GetAlpha(x As Integer, y As Integer) As Byte
            Dim i As Integer = GetIndex(x, y)
            Dim iA As Byte = 255

            If channels = 4 Then
                iA = buffer(i + 3)
            End If

            Return iA
        End Function

        Public Function GetRed(x As Integer, y As Integer) As Byte
            Dim i As Integer = GetIndex(x, y)

            If i < 0 Then
                Return 0
            Else
                Return buffer(i + 2)
            End If
        End Function

        Public Function GetGreen(x As Integer, y As Integer) As Byte
            Dim i As Integer = GetIndex(x, y)

            If i < 0 Then
                Return 0
            Else
                Return buffer(i + 1)
            End If
        End Function

        Public Function GetBlue(x As Integer, y As Integer) As Byte
            Dim i As Integer = GetIndex(x, y)

            If i < 0 Then
                Return 0
            Else
                Return buffer(i + 0)
            End If
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

        Public Shared Function Unpack(pixels As Color(,), size As Size) As Byte()
            Dim channels As Integer = 4
            Dim bytes As Byte() = New Byte(channels * pixels.Length - 1) {}

            For y As Integer = 0 To size.Height - 1
                For x As Integer = 0 To size.Width - 1
                    Dim pixel As Color = pixels(y, x)
                    Dim i As Integer = GetIndex(x, y, size.Width, channels)

                    bytes(i) = pixel.A
                    bytes(i + 1) = pixel.R
                    bytes(i + 2) = pixel.G
                    bytes(i + 3) = pixel.B
                Next
            Next

            Return bytes
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

            If i < 0 Then
                Return
            End If

            buffer(i + 2) = R
            buffer(i + 1) = G
            buffer(i + 0) = B
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetPixel(x As Integer, y As Integer, R As Byte, G As Byte, B As Byte, A As Byte)
            Dim i As Integer = GetIndex(x, y)

            If i < 0 Then
                Return
            End If
            If channels = 4 Then
                buffer(i + 3) = A
            End If

            buffer(i + 2) = R
            buffer(i + 1) = G
            buffer(i + 0) = B
        End Sub

        Public Sub SetAlpha(x As Integer, y As Integer, A As Byte)
            Dim i As Integer = GetIndex(x, y)

            If i < 0 Then
                Return
            End If
            If channels = 4 Then
                buffer(i + 3) = A
            End If
        End Sub

        Public Sub SetRed(x As Integer, y As Integer, R As Byte)
            Dim i As Integer = GetIndex(x, y)

            If i < 0 Then
                Return
            End If

            buffer(i + 2) = R
        End Sub

        Public Sub SetGreen(x As Integer, y As Integer, G As Byte)
            Dim i As Integer = GetIndex(x, y)

            If i < 0 Then
                Return
            End If

            buffer(i + 1) = G
        End Sub

        Public Sub SetBlue(x As Integer, y As Integer, B As Byte)
            Dim i As Integer = GetIndex(x, y)

            If i < 0 Then
                Return
            End If

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
        ''' save in-memory data as local bitmap file
        ''' </summary>
        ''' <param name="file"></param>
        Public Sub Save(file As String)
            Using s As Stream = file.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
                Call Save(s)
            End Using
        End Sub

        ''' <summary>
        ''' save in-memory data as bitmap file
        ''' </summary>
        ''' <param name="s"></param>
        Public Sub Save(s As Stream)
            Dim pixelFormat As BitsPerPixelEnum = If(GetPixelChannels() = 3, BitsPerPixelEnum.RGB24, BitsPerPixelEnum.RGBA32)
            Dim writer As New MemoryBmp(Width, Height, RawBuffer, pixelFormat)

            Call writer.Save(s, flipped:=True)
            Call s.Flush()
        End Sub

        Public Overrides Function ToString() As String
            Return $"memory_bitmap({Width}x{Height}); sizeof={StringFormats.Lanudry(bytes:=Width * Height * channels)}"
        End Function

        ''' <summary>
        ''' 这个函数会自动复制原始图片数据里面的东西的
        ''' </summary>
        ''' <param name="res"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function FromImage(res As Image) As BitmapBuffer
#If NET48 Then
            Dim copy As New Bitmap(res.Width, res.Height, format:=PixelFormat.Format32bppArgb)
            Dim g As Graphics = Graphics.FromImage(copy)

            Call g.DrawImageUnscaled(res, New Point)
            Call g.Flush()
            Call g.Dispose()

            Return BitmapBuffer.FromBitmap(copy)
#Else
            Return FromBitmap(New Bitmap(res))
#End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="curBitmap"></param>
        ''' <returns>
        ''' get the reference of the <see cref="Bitmap.MemoryBuffer"/> data directly
        ''' </returns>
        Public Shared Function FromBitmap(curBitmap As Bitmap) As BitmapBuffer
#If NET48 Then
            Return FromBitmap(curBitmap, ImageLockMode.ReadWrite)
#Else
            Return curBitmap.MemoryBuffer
#End If
        End Function

#If NET48 Then

        ''' <summary>
        ''' 使用这个函数进行写数据的话，会修改到原图
        ''' </summary>
        ''' <param name="curBitmap"></param>
        ''' <param name="mode"></param>
        ''' <returns></returns>
        Public Shared Function FromBitmap(curBitmap As Bitmap, mode As ImageLockMode) As BitmapBuffer
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
#End If

        Protected Overrides Sub Dispose(disposing As Boolean)
            If Not memoryBuffer Then
                ' write data back to the memory via the 
                ' managed memory pointer
                Call Write()
            End If
#If NET48 Then
            Call raw.UnlockBits(handle)
#End If
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of Color) Implements Enumeration(Of Color).GenericEnumerator
            For Each pixel As Color In buffer.Colors
                Yield pixel
            Next
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
