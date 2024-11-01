#Region "Microsoft.VisualBasic::5f375b8a625b14061d2c0f1cd58ee5aa, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\Bitmap\Bitmap.vb"

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

    '   Total Lines: 179
    '    Code Lines: 91 (50.84%)
    ' Comment Lines: 63 (35.20%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 25 (13.97%)
    '     File Size: 7.64 KB


    '     Class Bitmap
    ' 
    '         Properties: BitsPerPixelEnum, BytesPerPixel, BytesPerRow, FileHeader, Height
    '                     InfoHeaderBytes, PixelData, PixelDataFliped, Width
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetBmpBytes, GetBmpStream, IsPaddingRequired, RequiredBytesPerRow
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
        ''' NOTE: we don't care for images that are less than 24 bits
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property BytesPerPixel As Integer
            Get
                Return CInt(BitsPerPixelEnum) / 8
            End Get
        End Property

        Public ReadOnly Property PixelData As Byte()

        ''' <summary>
        ''' Get reversed order or rows.
        ''' For Bitmap image, pixel rows are stored from bottom to top.
        ''' So first row in bitmap file is lowest row in Image.
        ''' </summary>
        ''' <returns>Pixel data with reversed (fliped) rows</returns>
        Public ReadOnly Property PixelDataFliped As Byte()
            Get
                Dim rowListData = New List(Of Byte())()
                Dim totalRows = Height
                Dim pixelsInRow = Width

                For row = totalRows - 1 To 0 Step -1
                    ' NOTE: this only works on images that are 8/24/32 bits per pixel
                    Dim one_row = PixelData.Skip(row * Width * BytesPerPixel).Take(Width * BytesPerPixel).ToArray()
                    rowListData.Add(one_row)
                Next
                Dim reversedBytes = rowListData.SelectMany(Function(row) row).ToArray()
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
        ''' <param name="pixelData"></param>
        ''' <param name="bitsPerPixel"></param>
        Public Sub New(width As Integer, height As Integer, pixelData As Byte(), Optional bitsPerPixel As BitsPerPixelEnum = BitsPerPixelEnum.RGB24)
            Me.Width = width
            Me.Height = height
            Me.PixelData = pixelData
            Me.BitsPerPixelEnum = bitsPerPixel

            Dim rawImageSize = BytesPerRow * height

            ' Are we receiving proper byte[] size ?
            If pixelData.Length <> width * height * BytesPerPixel Then
                Throw New ArgumentOutOfRangeException($"{NameOf(pixelData)} has invalid size.")
            End If

            If bitsPerPixel = BitsPerPixelEnum.RGB24 Then InfoHeaderBytes = New BitmapInfoHeader(width, height, bitsPerPixel, rawImageSize).HeaderInfoBytes
            If bitsPerPixel = BitsPerPixelEnum.RGBA32 Then
                InfoHeaderBytes = New BitmapInfoHeaderRGBA(width, height, bitsPerPixel, rawImageSize).HeaderInfoBytes
            End If

            FileHeader = New BitmapFileHeader(width, height, bitsPerPixel, rawImageSize)
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
            'using (var writer = new BinaryWriter( stream )) {
            Dim writer As New BinaryWriter(stream)

            writer.Write(FileHeader.HeaderBytes)
            writer.Write(InfoHeaderBytes)
            writer.Flush()
            stream.Flush()

            Dim paddingRequired = BytesPerRow <> Width * BytesPerPixel
            Dim bytesToCopy = Width * BytesPerPixel
            Dim pixData = If(flipped, PixelDataFliped, PixelData)

            If paddingRequired Then
                For counter = 0 To Height - 1
                    Dim rowBuffer = New Byte(BytesPerRow - 1) {}
                    Buffer.BlockCopy(src:=pixData, srcOffset:=counter * bytesToCopy, dst:=rowBuffer, dstOffset:=0, count:=bytesToCopy)
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
