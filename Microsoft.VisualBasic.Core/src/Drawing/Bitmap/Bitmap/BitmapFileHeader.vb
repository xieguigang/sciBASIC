Imports System.Runtime.InteropServices

Namespace Imaging.BitmapImage.FileStream

    ''' <summary>
    ''' Keep proper byte layout in memory
    ''' </summary>
    <StructLayout(LayoutKind.Sequential, Pack:=1)>
    Public Class BitmapFileHeader

        Private _FileSize As UInteger
        ''' <summary>
        ''' FileHeader headerSize in bytes (fist 14 bytes from start)
        ''' </summary>
        Public Const BitmapFileHeaderSizeInBytes As Integer = 14

        Public Const ByteZero As Byte = &H42
        Public Const ByteOne As Byte = &H4D

        ''' <summary>
        ''' Explicitly set file in size
        ''' </summary>
        ''' <param name="fileSize"></param>
        Public Sub New(fileSize As UInteger)
            Me.FileSize = fileSize
        End Sub

        Public Property FileSize As UInteger
            Get
                Return _FileSize
            End Get
            Private Set(value As UInteger)
                _FileSize = value
            End Set
        End Property

        ''' <summary>
        ''' The offset, i.e. starting address, of the byte where the bitmap image data (pixel array) can be found from the beggining of the file
        ''' </summary>
        Public PixelDataOffset As UInteger

        ''' <summary>
        ''' Create header and calculate file size depending on input data
        ''' </summary>
        ''' <param name="width"></param>
        ''' <param name="height"></param>
        ''' <param name="bitsPerPixel"></param>
        ''' <param name="rawImageSize">Depends on row padding and number of rows</param>
        Public Sub New(Optional width As Integer = 1,
                       Optional height As Integer = 1,
                       Optional bitsPerPixel As BitsPerPixelEnum = BitsPerPixelEnum.RGB24,
                       Optional rawImageSize As Integer = 0)

            'if (System.BitConverter.IsLittleEndian)
            Dim infoHeaderSize = If(bitsPerPixel = BitsPerPixelEnum.RGB24, BitmapInfoHeader.SizeInBytes, BitmapInfoHeaderRGBA.SizeInBytes)

            FileSize = CUInt(BitmapFileHeaderSizeInBytes + infoHeaderSize + rawImageSize)

            PixelDataOffset = CUInt(BitmapFileHeaderSizeInBytes + infoHeaderSize)

            'infoHeader = new BitmapInfoHeader( Width, Height, BitsPerPixel: BitsPerPixel, rawImageSize: rawImageSize );
        End Sub

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

            Dim header = New BitmapFileHeader() With {
                .PixelDataOffset = offset,
                .FileSize = sizeBytes
            }

            Return header
        End Function
    End Class


End Namespace
