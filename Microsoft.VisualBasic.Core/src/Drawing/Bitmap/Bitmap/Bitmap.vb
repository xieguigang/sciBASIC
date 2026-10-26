#Region "Microsoft.VisualBasic::f26dc91cc1e52eba139bceac6d197cea, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\Bitmap\Bitmap.vb"

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

    '   Total Lines: 185
    '    Code Lines: 98 (52.97%)
    ' Comment Lines: 62 (33.51%)
    '    - Xml Docs: 90.32%
    ' 
    '   Blank Lines: 25 (13.51%)
    '     File Size: 7.79 KB


    '     Class Bitmap
    ' 
    '         Properties: BitsPerPixelEnum, BytesPerPixel, BytesPerRow, FileHeader, Height
    '                     InfoHeaderBytes, Palette, PixelData, PixelDataFliped, Stride
    '                     Width
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetBmpBytes, GetBmpStream, GrayPalette, IsPaddingRequired,
    '                   PaletteEntries, RequiredBytesPerRow, UnpaddedRowSize
    ' 
    '         Sub: Save
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Imaging.BitmapImage.FileStream

    ''' <summary>
    ''' A in-memory bitmap image model
    ''' </summary>
    ''' <remarks>
    ''' This is simple Bitmap library that helps you wrap binary data (pixels) into BMP header for saving into file and vice versa.
    ''' 
    ''' It supports only 24 bits BGR And 32 bits BGRA Byte arrays.
    ''' Library supports x86 (little-endian) And ARM (big-endian).
    ''' 
    ''' > https://github.com/dsoronda/bmp-sharp
    ''' </remarks>
    Public Class Bitmap

        Public ReadOnly Property Width As Integer = 0
        Public ReadOnly Property Height As Integer = 0
        Public ReadOnly Property BitsPerPixelEnum As BitsPerPixelEnum

        ''' <summary>
        ''' BMP file must be aligned at 4 butes at the end of row
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property BytesPerRow As Integer
            Get
                Return RequiredBytesPerRow(Width, BitsPerPixelEnum)
            End Get
        End Property

        ''' <summary>
        ''' NOTE: this is zero for the image that is less than 8 bits per pixel, 
        ''' use the <see cref="Stride"/> of current image object or the 
        ''' <see cref="UnpaddedRowSize(Integer, BitsPerPixelEnum)"/> helper for 
        ''' the sub byte bit depth image instead.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property BytesPerPixel As Integer
            Get
                Return CInt(BitsPerPixelEnum) \ 8
            End Get
        End Property

        ''' <summary>
        ''' The byte length of a single scan line of the <see cref="PixelData"/>, 
        ''' the 4 bytes row padding is not included. the scan line of the image 
        ''' that is less than 8 bits per pixel is bit packed just like the way 
        ''' that is used inside the bitmap file.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Stride As Integer
            Get
                Return UnpaddedRowSize(Width, BitsPerPixelEnum)
            End Get
        End Property

        ''' <summary>
        ''' The color palette of the indexed image, the palette entry is laid out 
        ''' as the 4 bytes BGRA color. it is nothing for the image that is more 
        ''' than 8 bits per pixel.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Palette As Byte()

        Public ReadOnly Property PixelData As Byte()

        ''' <summary>
        ''' Get reversed order or rows.
        ''' For Bitmap image, pixel rows are stored from bottom to top.
        ''' So first row in bitmap file is lowest row in Image.
        ''' </summary>
        ''' <returns>Pixel data with reversed (fliped) rows</returns>
        Public ReadOnly Property PixelDataFliped As Byte()
            Get
                Dim rowSize As Integer = Me.Stride
                Dim lastRow As Integer = Height - 1
                Dim reversedBytes As Byte() = New Byte(PixelData.Length - 1) {}

                For row As Integer = 0 To lastRow
                    ' NOTE: the rows are copied in one pass so that the row 
                    ' reversing is a O(n) operation instead of the O(n^2) one 
                    ' that is made by the linq skip/take
                    Call Buffer.BlockCopy(src:=PixelData,
                                          srcOffset:=row * rowSize,
                                          dst:=reversedBytes,
                                          dstOffset:=(lastRow - row) * rowSize,
                                          count:=rowSize)
                Next

                Return reversedBytes
            End Get
        End Property

        Public ReadOnly Property FileHeader As BitmapFileHeader
        Public ReadOnly Property InfoHeaderBytes As Byte()

        ''' <summary>
        ''' Create new Bitmap object
        ''' </summary>
        ''' <param name="width"></param>
        ''' <param name="height"></param>
        ''' <param name="pixelData">the row major pixel data, the row padding is 
        ''' not included, the scan line of the image that is less than 8 bits per 
        ''' pixel should be bit packed.</param>
        ''' <param name="bitsPerPixel"></param>
        ''' <param name="palette">
        ''' the color palette of the indexed image (1/4/8 bits per pixel), the 
        ''' palette entry is laid out as the 4 bytes BGRA color. a gray scale 
        ''' palette is generated automatically when it is not given.
        ''' </param>
        Public Sub New(width As Integer, height As Integer, pixelData As Byte(),
                       Optional bitsPerPixel As BitsPerPixelEnum = BitsPerPixelEnum.RGB24,
                       Optional palette As Byte() = Nothing)
            If width <= 0 OrElse height <= 0 Then
                Throw New ArgumentOutOfRangeException($"{width}x{height} is not a valid bitmap image size.")
            End If
            If pixelData Is Nothing Then
                Throw New ArgumentNullException(NameOf(pixelData))
            End If

            Me.Width = width
            Me.Height = height
            Me.PixelData = pixelData
            Me.BitsPerPixelEnum = bitsPerPixel

            Dim rowSize As Integer = Me.Stride
            Dim rawSize As Long = CLng(BytesPerRow) * CLng(height)

            If rawSize > Integer.MaxValue Then
                Throw New NotSupportedException($"the {width}x{height} {CInt(bitsPerPixel)}bpp image is too large to be saved at once.")
            End If

            Dim rawImageSize As Integer = CInt(rawSize)
            Dim entries As Integer = PaletteEntries(bitsPerPixel)

            ' Are we receiving proper byte[] size ?
            If pixelData.Length <> rowSize * height Then
                Throw New ArgumentOutOfRangeException(
                    $"{NameOf(pixelData)} has invalid size: {pixelData.Length} bytes given, " &
                    $"but {rowSize * height} bytes are required for a {width}x{height} {CInt(bitsPerPixel)}bpp image.")
            End If

            If entries > 0 Then
                If palette Is Nothing Then
                    Me.Palette = GrayPalette(entries)
                ElseIf palette.Length <> entries * 4 Then
                    Throw New ArgumentOutOfRangeException(
                        $"{NameOf(palette)} should be {entries * 4} bytes for a {CInt(bitsPerPixel)}bpp image, but {palette.Length} bytes given.")
                Else
                    Me.Palette = palette
                End If
            End If

            If bitsPerPixel = BitsPerPixelEnum.RGBA32 Then
                InfoHeaderBytes = New BitmapInfoHeaderRGBA(width, height, bitsPerPixel, rawImageSize).HeaderInfoBytes
            Else
                Dim infoHeader As New BitmapInfoHeader(width, height, bitsPerPixel, rawImageSize)

                If entries > 0 Then
                    ' the palette of the indexed image is stored right after the 
                    ' info header, the number of the entries must be declared
                    infoHeader.nuberOfColorsInPallete = CUInt(entries)
                End If

                InfoHeaderBytes = infoHeader.HeaderInfoBytes
            End If

            FileHeader = New BitmapFileHeader(width, height, bitsPerPixel, rawImageSize, If(Me.Palette Is Nothing, 0, Me.Palette.Length))
        End Sub

        ''' <summary>
        ''' Get bitmap as byte aray for saving to file
        ''' </summary>
        ''' <param name="flipped">Flip (reverse order of) rows. Bitmap pixel rows are stored from bottom to up as shown in image</param>
        ''' <returns></returns>
        Public Function GetBmpBytes(Optional flipped As Boolean = False) As Byte()
            Using stream As MemoryStream = GetBmpStream(flipped)
                Return stream.ToArray()
            End Using
        End Function

        Public Sub Save(stream As Stream, Optional flipped As Boolean = False)
            Dim writer As New BinaryWriter(stream)

            writer.Write(FileHeader.HeaderBytes)
            writer.Write(InfoHeaderBytes)

            If Palette IsNot Nothing Then
                ' the color palette of the indexed image is stored right after 
                ' the info header and before the pixel data
                writer.Write(Palette)
            End If

            writer.Flush()
            stream.Flush()

            Dim rowSize As Integer = Me.Stride
            Dim bytesPerRow As Integer = BytesPerRow
            Dim pixData = If(flipped, PixelDataFliped, PixelData)

            If bytesPerRow > rowSize Then
                ' the row buffer is allocated only once, the zero bytes of the 
                ' row padding are kept by the buffer
                Dim rowBuffer = New Byte(bytesPerRow - 1) {}

                For counter = 0 To Height - 1
                    Buffer.BlockCopy(src:=pixData,
                                     srcOffset:=counter * rowSize,
                                     dst:=rowBuffer,
                                     dstOffset:=0,
                                     count:=rowSize)
                    writer.Write(rowBuffer)
                Next
            Else
                writer.Write(pixData)
            End If

            Call writer.Flush()
        End Sub

        ''' <summary>
        ''' Get bitmap as byte stream for saving to file
        ''' </summary>
        ''' <param name="flipped">Flip (reverse order of) rows. Bitmap pixel rows are stored from bottom to up as shown in image</param>
        ''' <returns>
        ''' get in-memory stream data of the current bitmap object, could not 
        ''' be processed when bitmap object is greater than 2GB.
        ''' </returns>
        Public Function GetBmpStream(Optional flipped As Boolean = False) As MemoryStream
            Dim rawImageSize = BytesPerRow * Height
            Dim stream As New MemoryStream(rawImageSize)

            Save(stream, flipped)
            stream.Position = 0

            Return stream
        End Function

        ''' <summary>
        ''' BMP file must be aligned at 4 bytes at the end of row
        ''' </summary>
        ''' <param name="width">Image Width</param>
        ''' <param name="bitsPerPixel">Bits per pixel</param>
        ''' <returns>How many bytes BMP requires per row</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function RequiredBytesPerRow(width As Integer, bitsPerPixel As BitsPerPixelEnum) As Integer
            Return CInt(std.Ceiling(CDec(width * bitsPerPixel) / 32)) * 4
        End Function

        ''' <summary>
        ''' Check if padding is required (extra bytes for a row).
        ''' </summary>
        ''' <param name="width">Width of image</param>
        ''' <param name="bitsPerPixel">Bits per pixels to calculate actual byte requirement</param>
        ''' <param name="bytesPerRow">BMP required bytes per row</param>
        ''' <returns>True/false if we need to allocate extra bytes (for BMP saving) for padding</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function IsPaddingRequired(width As Integer, bitsPerPixel As BitsPerPixelEnum, bytesPerRow As Integer) As Boolean
            Return bytesPerRow <> width * bitsPerPixel / 8
        End Function
    End Class
End Namespace
