#Region "Microsoft.VisualBasic::166c310bb48ea02608988b72c736266a, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\Bitmap\BitmapFileHelper.vb"

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

    '   Total Lines: 93
    '    Code Lines: 63 (67.74%)
    ' Comment Lines: 13 (13.98%)
    '    - Xml Docs: 69.23%
    ' 
    '   Blank Lines: 17 (18.28%)
    '     File Size: 4.07 KB


    '     Module BitmapFileHelper
    ' 
    '         Function: ExpandPixelsToRgb24, ParseMemoryBitmap, ReadFileAsBitmap,
    '                   ReadPaletteIndex, ReverseRows
    ' 
    '         Sub: SaveBitmapToFile
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Serialization.BinaryDumping
Imports MemoryBmp = Microsoft.VisualBasic.Imaging.BitmapImage.FileStream.Bitmap

Namespace Imaging.BitmapImage.FileStream

    Public Module BitmapFileHelper

        ''' <summary>
        ''' read bitmap file in-memory
        ''' </summary>
        ''' <param name="fileName"></param>
        ''' <param name="flipRows">
        ''' Reverse the order of the pixel rows that are stored in the file: a 
        ''' bottom up bitmap image is returned with the top down row order when 
        ''' this option is enabled.
        ''' </param>
        ''' <param name="expandToRgb24">
        ''' Expand the indexed image and the 16bpp image into a 24bpp rgb color 
        ''' image, the color palette is applied on the indexed image. the image 
        ''' is kept in its original bit depth when this option is disabled.
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' only process the bitmap image data in size smaller than 2GB
        ''' </remarks>
        Public Function ReadFileAsBitmap(fileName As String,
                                         Optional flipRows As Boolean = False,
                                         Optional expandToRgb24 As Boolean = False) As Bitmap
            If String.IsNullOrWhiteSpace(fileName) Then
                Throw New ArgumentNullException(NameOf(fileName))
            End If
            If Not File.Exists(fileName) Then
                Throw New Exception($"File {fileName} not found")
            End If

            Dim fileInfo As New FileInfo(fileName)

            If fileInfo.Length <= BitmapFileHeader.BitmapFileHeaderSizeInBytes Then
                Throw New Exception($"Invalid file format. Size is too small.")
            End If

            Using fileStream = File.OpenRead(fileName)
                Return ParseMemoryBitmap(fileStream, flipRows, expandToRgb24)
            End Using
        End Function

        ''' <summary>
        ''' parse the bitmap image data from a stream
        ''' </summary>
        ''' <param name="fileStream"></param>
        ''' <param name="flipRows">Reverse the order of the pixel rows that are stored in the file.</param>
        ''' <param name="expandToRgb24">Expand the indexed image and the 16bpp image into a 24bpp rgb color image.</param>
        ''' <returns></returns>
        Public Function ParseMemoryBitmap(fileStream As Stream,
                                          Optional flipRows As Boolean = False,
                                          Optional expandToRgb24 As Boolean = False) As Bitmap
            Using bReader As New LittleEndianReader(fileStream)
                Dim headerBytes = bReader.ReadBytes(BitmapFileHeader.BitmapFileHeaderSizeInBytes)
                Dim fileHeader = BitmapFileHeader.GetHeaderFromBytes(headerBytes)
                Dim dibHeaderSize = bReader.ReadInt32()

                fileStream.Seek(-4, SeekOrigin.Current)

                If dibHeaderSize < BitmapInfoHeader.SizeInBytes Then
                    Throw New InvalidDataException($"the {dibHeaderSize} bytes DIB header is not supported, the BITMAPCOREHEADER is too old.")
                End If

                Dim infoHeader = BitmapInfoHeader.GetHeaderFromBytes(bReader.ReadBytes(dibHeaderSize))
                Dim width = infoHeader.Width
                Dim height = infoHeader.Height
                ' a negative height of the DIB header means that the pixel rows 
                ' are stored from the top to the bottom
                Dim topDown As Boolean = (height < 0)

                If height < 0 Then
                    height = -height
                End If
                If width <= 0 OrElse height <= 0 Then
                    Throw New InvalidDataException($"the bitmap image size {width}x{height} is not valid.")
                End If

                Dim bits As Integer = CInt(infoHeader.BitsPerPixel)

                If bits <> 1 AndAlso bits <> 4 AndAlso bits <> 8 AndAlso bits <> 16 AndAlso bits <> 24 AndAlso bits <> 32 Then
                    Throw New NotSupportedException($"the {bits} bits per pixel bitmap image is not supported.")
                End If

                ' the color palette of the indexed image, it is stored right 
                ' after the DIB header and before the pixel data
                Dim palette As Byte() = Nothing

                If MemoryBmp.PaletteEntries(infoHeader.BitsPerPixel) > 0 Then
                    Dim entries As Integer = MemoryBmp.PaletteEntries(infoHeader.BitsPerPixel)
                    Dim declared As Integer = CInt(infoHeader.nuberOfColorsInPallete)


                    If declared > 0 AndAlso declared <= entries Then
                        entries = declared
                    End If

                    palette = bReader.ReadBytes(entries * 4)

                    If palette.Length < entries * 4 Then
                        Throw New InvalidDataException($"the color palette of the {bits} bits per pixel bitmap image is truncated.")
                    End If
                End If

                Dim bytesPerRow = MemoryBmp.RequiredBytesPerRow(width, infoHeader.BitsPerPixel)
                Dim rowSize = MemoryBmp.UnpaddedRowSize(width, infoHeader.BitsPerPixel)
                Dim pixelData = New Byte(rowSize * height - 1) {}

                ' seek to location where pixel data is
                fileStream.Seek(fileHeader.PixelDataOffset, SeekOrigin.Begin)

                For counter = 0 To height - 1
                    Dim rowBuffer = bReader.ReadBytes(bytesPerRow)

                    If rowBuffer.Length < rowSize Then
                        Throw New InvalidDataException("the pixel data of the bitmap image is truncated.")
                    End If

                    Call System.Buffer.BlockCopy(src:=rowBuffer,
                                                 srcOffset:=0,
                                                 dst:=pixelData,
                                                 dstOffset:=counter * rowSize,
                                                 count:=rowSize)
                Next

                ' the pixel rows of a top down DIB are stored from the top to the 
                ' bottom, they are normalized into the bottom up order here so 
                ' that the image could be saved back as it is
                If topDown Then
                    pixelData = ReverseRows(pixelData, rowSize, height)
                End If
                If flipRows Then
                    pixelData = ReverseRows(pixelData, rowSize, height)
                End If

                If expandToRgb24 AndAlso bits <> 24 Then
                    Return New Bitmap(width, height,
                                      pixelData:=ExpandPixelsToRgb24(pixelData, palette, width, height, rowSize, infoHeader.BitsPerPixel),
                                      bitsPerPixel:=BitsPerPixelEnum.RGB24)
                End If

                Dim bitmap As New Bitmap(width, height,
                                         pixelData:=pixelData,
                                         bitsPerPixel:=infoHeader.BitsPerPixel,
                                         palette:=palette)

                Return bitmap
            End Using
        End Function

        ''' <summary>
        ''' reverse the order of the pixel rows in place
        ''' </summary>
        Private Function ReverseRows(pixelData As Byte(), rowSize As Integer, height As Integer) As Byte()
            Dim reversed As Byte() = New Byte(pixelData.Length - 1) {}
            Dim lastRow As Integer = height - 1

            For row As Integer = 0 To lastRow
                Call System.Buffer.BlockCopy(src:=pixelData,
                                             srcOffset:=row * rowSize,
                                             dst:=reversed,
                                             dstOffset:=(lastRow - row) * rowSize,
                                             count:=rowSize)
            Next

            Return reversed
        End Function

        ''' <summary>
        ''' expand the indexed and the 16bpp image into the 24bpp rgb color image
        ''' </summary>
        Private Function ExpandPixelsToRgb24(pixelData As Byte(), palette As Byte(),
                                       width As Integer, height As Integer,
                                       rowSize As Integer, bitsPerPixel As BitsPerPixelEnum) As Byte()
            Dim rgb As Byte() = New Byte(width * height * 3 - 1) {}

            For y As Integer = 0 To height - 1
                For x As Integer = 0 To width - 1
                    Dim target As Integer = (y * width + x) * 3

                    Select Case bitsPerPixel
                        Case BitsPerPixelEnum.Monochrome, BitsPerPixelEnum.Four, BitsPerPixelEnum.Eight
                            Dim offset As Integer = ReadPaletteIndex(pixelData, rowSize, bitsPerPixel, x, y) * 4

                            rgb(target) = palette(offset + 2)
                            rgb(target + 1) = palette(offset + 1)
                            rgb(target + 2) = palette(offset)
                        Case BitsPerPixelEnum.RBG16
                            ' the 16bpp image is stored as the RGB555 color
                            Dim source As Integer = y * rowSize + x * 2
                            Dim v As Integer = CInt(pixelData(source)) Or (CInt(pixelData(source + 1)) << 8)

                            rgb(target) = CByte(((v >> 10) And &H1F) * 255 \ 31)
                            rgb(target + 1) = CByte(((v >> 5) And &H1F) * 255 \ 31)
                            rgb(target + 2) = CByte((v And &H1F) * 255 \ 31)
                        Case Else
                            ' the 32bpp BGRA image, the alpha channel is dropped
                            Dim source As Integer = y * rowSize + x * 4

                            rgb(target) = pixelData(source + 2)
                            rgb(target + 1) = pixelData(source + 1)
                            rgb(target + 2) = pixelData(source)
                    End Select
                Next
            Next

            Return rgb
        End Function

        ''' <summary>
        ''' read the palette index of a pixel of the indexed image
        ''' </summary>
        Private Function ReadPaletteIndex(pixelData As Byte(), rowSize As Integer,
                                          bitsPerPixel As BitsPerPixelEnum, x As Integer, y As Integer) As Integer
            Dim offset As Integer = y * rowSize

            Select Case bitsPerPixel
                Case BitsPerPixelEnum.Monochrome
                    Return (CInt(pixelData(offset + x \ 8)) >> (7 - (x Mod 8))) And &H1
                Case BitsPerPixelEnum.Four
                    Dim b As Integer = pixelData(offset + x \ 2)

                    Return If((x Mod 2) = 0, (b >> 4) And &HF, b And &HF)
                Case Else
                    Return pixelData(offset + x)
            End Select
        End Function

        Public Sub SaveBitmapToFile(fileName As String, bitmap As Bitmap, Optional flipRows As Boolean = False)
            Using fileStream As Stream = fileName.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
                Using bmpStream = bitmap.GetBmpStream(flipRows)
                    bmpStream.CopyTo(fileStream, bufferSize:=16 * 1024)
                End Using
            End Using
        End Sub
    End Module
End Namespace
