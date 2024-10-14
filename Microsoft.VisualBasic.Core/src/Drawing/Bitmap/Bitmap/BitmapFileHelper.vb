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
