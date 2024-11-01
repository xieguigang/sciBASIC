#Region "Microsoft.VisualBasic::7e43bc62de7d561a4f5773bd0a52123f, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\Bitmap\BitmapFileHelper.vb"

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

    '   Total Lines: 89
    '    Code Lines: 60 (67.42%)
    ' Comment Lines: 13 (14.61%)
    '    - Xml Docs: 69.23%
    ' 
    '   Blank Lines: 16 (17.98%)
    '     File Size: 4.12 KB


    '     Module BitmapFileHelper
    ' 
    '         Function: ParseMemoryBitmap, ReadFileAsBitmap
    ' 
    '         Sub: SaveBitmapToFile
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports MemoryBmp = Microsoft.VisualBasic.Imaging.BitmapImage.FileStream.Bitmap

Namespace Imaging.BitmapImage.FileStream
    Public Module BitmapFileHelper

        ''' <summary>
        ''' read bitmap file in-memory
        ''' </summary>
        ''' <param name="fileName"></param>
        ''' <param name="flipRows"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' only process the bitmap image data in size smaller than 2GB
        ''' </remarks>
        Public Function ReadFileAsBitmap(fileName As String, Optional flipRows As Boolean = False) As Bitmap
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
                Return ParseMemoryBitmap(fileStream)
            End Using
        End Function

        Public Function ParseMemoryBitmap(fileStream As Stream) As Bitmap
            Using bReader = New SafeBinaryReader(fileStream)
                Dim headerBytes = bReader.ReadBytes(BitmapFileHeader.BitmapFileHeaderSizeInBytes)
                Dim fileHeader = BitmapFileHeader.GetHeaderFromBytes(headerBytes)

                'If FileInfo.Length <> fileHeader.FileSize Then
                '    Throw New Exception($"File size is different than in header.")
                'End If

                Dim dibHeaderSize = bReader.ReadInt32()
                fileStream.Seek(-4, SeekOrigin.Current)

                Dim infoHeader = BitmapInfoHeader.GetHeaderFromBytes(bReader.ReadBytes(dibHeaderSize))

                Dim width = infoHeader.Width
                Dim height = infoHeader.Height

                Dim bytesPerRow = MemoryBmp.RequiredBytesPerRow(infoHeader.Width, infoHeader.BitsPerPixel)

                Dim bytesPerPixel = CInt(infoHeader.BitsPerPixel) / 8
                Dim paddingRequired = MemoryBmp.IsPaddingRequired(infoHeader.Width, infoHeader.BitsPerPixel, bytesPerRow)
                Dim pixelData = New Byte(width * height * bytesPerPixel - 1) {}
                ' seek to location where pixel data is
                fileStream.Seek(fileHeader.PixelDataOffset, SeekOrigin.Begin)

                If paddingRequired Then
                    Dim bytesToCopy = width * bytesPerPixel
                    For counter = 0 To height - 1
                        Dim rowBuffer = bReader.ReadBytes(bytesPerRow)
                        Buffer.BlockCopy(src:=rowBuffer, srcOffset:=0, dst:=pixelData, dstOffset:=counter * bytesToCopy, count:=bytesToCopy)
                    Next
                Else
                    Dim rowBuffer = bReader.ReadBytes(pixelData.Length)
                    rowBuffer.CopyTo(pixelData, 0)
                End If

                Dim bitmap As New Bitmap(width, height, pixelData:=pixelData, bitsPerPixel:=infoHeader.BitsPerPixel)
                Return bitmap
            End Using
        End Function

        Public Sub SaveBitmapToFile(fileName As String, bitmap As Bitmap)
            If bitmap Is Nothing Then Throw New ArgumentNullException(NameOf(bitmap))
            If String.IsNullOrWhiteSpace(fileName) Then Throw New ArgumentNullException(NameOf(fileName))
            Dim filePath = Path.GetDirectoryName(fileName)
            If Not Directory.Exists(filePath) Then Throw New Exception($"Destination directory not found.")

            Using fileStream = File.Create(fileName)
                Using bmpStream = bitmap.GetBmpStream()
                    bmpStream.CopyTo(fileStream, bufferSize:=16 * 1024)
                End Using
            End Using
        End Sub
    End Module
End Namespace
