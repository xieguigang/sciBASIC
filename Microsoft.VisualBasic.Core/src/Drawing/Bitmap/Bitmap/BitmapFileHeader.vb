#Region "Microsoft.VisualBasic::8d511b32d03e9294000d43916255b2a7, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\Bitmap\BitmapFileHeader.vb"

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

    '   Total Lines: 119
    '    Code Lines: 63 (52.94%)
    ' Comment Lines: 33 (27.73%)
    '    - Xml Docs: 84.85%
    ' 
    '   Blank Lines: 23 (19.33%)
    '     File Size: 4.79 KB


    '     Class BitmapFileHeader
    ' 
    '         Properties: FileSize, HeaderBytes
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetHeaderFromBytes, InfoHeaderSize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices

Namespace Imaging.BitmapImage.FileStream

    ''' <summary>
    ''' Keep proper byte layout in memory
    ''' </summary>
    <StructLayout(LayoutKind.Sequential, Pack:=1)>
    Public Class BitmapFileHeader

        ''' <summary>
        ''' FileHeader headerSize in bytes (fist 14 bytes from start)
        ''' </summary>
        Public Const BitmapFileHeaderSizeInBytes As Integer = 14

        Public Const ByteZero As Byte = &H42
        Public Const ByteOne As Byte = &H4D

        Public ReadOnly Property FileSize As UInteger

        ''' <summary>
        ''' The offset, i.e. starting address, of the byte where the bitmap image data (pixel array) can be found from the beggining of the file
        ''' </summary>
        Public PixelDataOffset As UInteger

        ''' <summary>
        ''' Explicitly set file in size
        ''' </summary>
        ''' <param name="fileSize"></param>
        Public Sub New(fileSize As UInteger)
            Me.FileSize = fileSize
        End Sub

        ''' <summary>
        ''' Create header and calculate file size depending on input data
        ''' </summary>
        ''' <param name="width"></param>
        ''' <param name="height"></param>
        ''' <param name="bitsPerPixel"></param>
        ''' <param name="rawImageSize">Depends on row padding and number of rows</param>
        ''' <param name="paletteSize">
        ''' the byte length of the color palette of the indexed image, the palette 
        ''' is stored right after the info header so that it is a part of the 
        ''' info header here.
        ''' </param>
        Public Sub New(Optional width As Integer = 1,
                       Optional height As Integer = 1,
                       Optional bitsPerPixel As BitsPerPixelEnum = BitsPerPixelEnum.RGB24,
                       Optional rawImageSize As Integer = 0,
                       Optional paletteSize As Integer = 0)

            'if (System.BitConverter.IsLittleEndian)
            Dim dibSize As Integer = InfoHeaderSize(bitsPerPixel, paletteSize)
            Dim total As Long = CLng(BitmapFileHeaderSizeInBytes) + dibSize + CLng(rawImageSize)

            If total > UInteger.MaxValue Then
                Throw New NotSupportedException($"the {width}x{height} {CInt(bitsPerPixel)}bpp image is too large to be saved as a bmp file: {total} bytes.")
            End If

            FileSize = CUInt(total)

            PixelDataOffset = CUInt(BitmapFileHeaderSizeInBytes + dibSize)

            'infoHeader = new BitmapInfoHeader( Width, Height, BitsPerPixel: BitsPerPixel, rawImageSize: rawImageSize );
        End Sub

        ''' <summary>
        ''' the byte length of the DIB header of the image, the color palette of 
        ''' the indexed image is a part of the DIB header section.
        ''' </summary>
        ''' <param name="bitsPerPixel"></param>
        ''' <param name="paletteSize">the byte length of the color palette.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' the 32bpp image requires the 56 bytes BITMAPV3INFOHEADER for the four 
        ''' channel bit masks, but the other bit depth are all handled by the 
        ''' standard 40 bytes BITMAPINFOHEADER.
        ''' </remarks>
        Public Shared Function InfoHeaderSize(bitsPerPixel As BitsPerPixelEnum, paletteSize As Integer) As Integer
            If bitsPerPixel = BitsPerPixelEnum.RGBA32 Then
                Return BitmapInfoHeaderRGBA.SizeInBytes
            Else
                Return BitmapInfoHeader.SizeInBytes + paletteSize
            End If
        End Function

        ''' <summary>
        ''' Get header bytes
        ''' </summary>
        Public ReadOnly Property HeaderBytes As Byte()
            Get
                Dim byteArray = New Byte(13) {} ' 14
                '{ 0x42, 0x4d } BM string
                byteArray(0) = ByteZero ' B
                byteArray(1) = ByteOne  ' M
                Dim sizeBytes = BitConverter.GetBytes(FileSize)
                Dim offset = BitConverter.GetBytes(PixelDataOffset)

                ' BMP byte order is little endian so we have to take care on byte ordering
                If Not BitConverter.IsLittleEndian Then
                    ' we are on BigEndian system so we have to revers byte order
                    Array.Reverse(sizeBytes)
                    Array.Reverse(offset)
                End If

                sizeBytes.CopyTo(byteArray, 2) ' 02  2   4 bytes 	The headerSize of the BMP file in bytes
                offset.CopyTo(byteArray, 10)  ' 0A  10  4 bytes 	The offset, i.e. starting address, of the byte where the bitmap image data (pixel array) can be found.

                Return byteArray
            End Get
        End Property

        ''' <summary>
        ''' Generate BitmapFileHeader from first 14 bytes
        ''' </summary>
        ''' <param name="headerBytes"></param>
        ''' <returns>BitmapFileHeader or throws exception</returns>
        Public Shared Function GetHeaderFromBytes(headerBytes As Byte()) As BitmapFileHeader
            If headerBytes Is Nothing Then Throw New ArgumentNullException(NameOf(headerBytes))
            If headerBytes.Length <> BitmapFileHeaderSizeInBytes Then
                Throw New ArgumentOutOfRangeException($"{NameOf(headerBytes)} should be {BitmapFileHeaderSizeInBytes} bytes in headerSize")
            End If

            If Not BitConverter.IsLittleEndian Then
                Array.Reverse(headerBytes, 2, 4)
                Array.Reverse(headerBytes, 10, 4)
            End If

            Dim sizeBytes = BitConverter.ToUInt32(headerBytes, 2)
            Dim offset = BitConverter.ToUInt32(headerBytes, 10)
            Dim header As New BitmapFileHeader() With {
                .PixelDataOffset = offset,
                ._FileSize = sizeBytes
            }

            Return header
        End Function
    End Class


End Namespace
