Imports System.Runtime.InteropServices

Namespace Imaging.BitmapImage.FileStream

    <StructLayout(LayoutKind.Sequential, Pack:=1)>
    Public Class BitmapInfoHeader
        ' NOTE : do not reorder fields !!! we use this layout for direct binary de/serialization!!

        ' Warning CS0414  The field 'BitmapInfoHeader.HorizontalPixelPerMeter' is assigned but its value is never used
        ' disable error warning , we dont need values in those fields !!

        Private _BitmapInfoHeaderSize As UInteger, _Width As Integer, _Height As Integer, _BitsPerPixel As BitsPerPixelEnum
        Dim _CompressionMethod As CompressionMethod = CompressionMethod.BI_RGB
        Dim _ImageSize As Integer, _HorizontalPixelPerMeter As Integer, _VerticalPixelPerMeter As Integer


        Public Property BitmapInfoHeaderSize As UInteger
            Get
                Return _BitmapInfoHeaderSize
            End Get
            Protected Set(value As UInteger)
                _BitmapInfoHeaderSize = value
            End Set
        End Property

        ''' <summary>
        ''' the bitmap Width in pixels (signed integer)
        ''' </summary>
        Public Property Width As Integer
            Get
                Return _Width
            End Get
            Protected Set(value As Integer)
                _Width = value
            End Set
        End Property

        ''' <summary>
        ''' the bitmap Height in pixels (signed integer)
        ''' </summary>
        Public Property Height As Integer
            Get
                Return _Height
            End Get
            Protected Set(value As Integer)
                _Height = value
            End Set
        End Property

        ''' <summary>
        ''' the number of color planes (must be 1)
        ''' </summary>
        Public ReadOnly Property ColorPlanes As Short
            Get
                Return 1
            End Get
        End Property

        ''' <summary>
        ''' the number of bits per pixel, which is the color depth of the image. Typical values are 1, 4, 8, 16, 24 and 32.
        ''' </summary>
        Public Property BitsPerPixel As BitsPerPixelEnum
            Get
                Return _BitsPerPixel
            End Get
            Protected Set(value As BitsPerPixelEnum)
                _BitsPerPixel = value
            End Set
        End Property

        ''' <summary>
        ''' 0 	BI_RGB (UNCOMPRESSED)
        ''' </summary>
        Public Property CompressionMethod As CompressionMethod
            Get
                Return _CompressionMethod
            End Get
            Protected Set(value As CompressionMethod)
                _CompressionMethod = value
            End Set
        End Property

        ''' <summary>
        ''' the image size. This is the size of the raw bitmap data; a dummy 0 can be given for BI_RGB bitmaps.
        ''' </summary>
        Public Property ImageSize As Integer
            Get
                Return _ImageSize
            End Get
            Protected Set(value As Integer)
                _ImageSize = value
            End Set
        End Property


        ''' <summary>
        ''' the horizontal resolution of the image. (pixel per metre, signed integer)
        ''' </summary>
        Public Property HorizontalPixelPerMeter As Integer
            Get
                Return _HorizontalPixelPerMeter
            End Get
            Protected Set(value As Integer)
                _HorizontalPixelPerMeter = value
            End Set
        End Property

        ''' <summary>
        ''' the vertical resolution of the image. (pixel per metre, signed integer)
        ''' </summary>
        Public Property VerticalPixelPerMeter As Integer
            Get
                Return _VerticalPixelPerMeter
            End Get
            Protected Set(value As Integer)
                _VerticalPixelPerMeter = value
            End Set
        End Property

        ''' <summary>
        ''' the number of colors in the color palette, or 0 to default to 2n (not used)
        ''' </summary>
        Public nuberOfColorsInPallete As UInteger
        ''' <summary>
        ''' numberOfImportantColorsUsed (not used)
        ''' </summary>
        Public numberOfImportantColorsUsed As UInteger

        ''' <summary>
        ''' DIB header (bitmap information header)
        ''' This is standard Windows BITMAPINFOHEADER as described here https://en.wikipedia.org/wiki/BMP_file_format#Bitmap_file_header
        ''' </summary>
        ''' <param name="width"></param>
        ''' <param name="height"></param>
        ''' <param name="bitsPerPixel"></param>
        ''' <param name="rawImageSize"></param>
        Public Sub New(width As Integer, height As Integer,
                       Optional bitsPerPixel As BitsPerPixelEnum = BitsPerPixelEnum.RGB24,
                       Optional rawImageSize As Integer = 0,
                       Optional horizontalPixelPerMeter As Integer = 3780,
                       Optional verticalPixelPerMeter As Integer = 3780,
                       Optional compressionMethod As CompressionMethod = CompressionMethod.BI_RGB)

            Me.Width = width
            Me.Height = height
            Me.BitsPerPixel = bitsPerPixel
            Me.CompressionMethod = compressionMethod  ' CompressionMethod.BI_RGB;
            ImageSize = rawImageSize
            Me.HorizontalPixelPerMeter = horizontalPixelPerMeter ' 96 DPI
            Me.VerticalPixelPerMeter = verticalPixelPerMeter   ' 96 DPI
            nuberOfColorsInPallete = 0 ' ignored
            numberOfImportantColorsUsed = 0    ' ignored
        End Sub

        'public static int SizeInBytes => System.Runtime.InteropServices.Marshal.SizeOf(typeof(BitmapInfoHeader));
        Public Shared ReadOnly Property SizeInBytes As Integer
            Get
                Return 40
            End Get
        End Property


        Public Overridable ReadOnly Property HeaderInfoBytes As Byte()
            Get
                Dim byteArray = New Byte(SizeInBytes - 1) {} ' 40

                Dim size = BitConverter.GetBytes(SizeInBytes)

                Dim width = BitConverter.GetBytes(Me.Width)
                Dim height = BitConverter.GetBytes(Me.Height)
                Dim colorPlanes = BitConverter.GetBytes(Me.ColorPlanes)
                Dim bitsPerPixel = BitConverter.GetBytes(CShort(Me.BitsPerPixel))
                Dim compressionMethod = BitConverter.GetBytes(Me.CompressionMethod)
                Dim imageSize = BitConverter.GetBytes(Me.ImageSize)
                Dim horizontalPixelPerMeter = BitConverter.GetBytes(Me.HorizontalPixelPerMeter)
                Dim verticalPixelPerMeter = BitConverter.GetBytes(Me.VerticalPixelPerMeter)

                Dim nuberOfColorsInPallete = BitConverter.GetBytes(CInt(Me.nuberOfColorsInPallete))
                Dim numberOfImportantColorsUsed = BitConverter.GetBytes(CInt(Me.numberOfImportantColorsUsed))
                ' total 40 bytes


                ' BMP byte order is little endian so we have to take care on byte ordering
                If Not BitConverter.IsLittleEndian Then
                    ' we are on BigEndian system so we have to revers byte order
                    Array.Reverse(size)
                    Array.Reverse(width)
                    Array.Reverse(height)
                    Array.Reverse(colorPlanes)
                    Array.Reverse(bitsPerPixel)
                    Array.Reverse(compressionMethod)
                    Array.Reverse(imageSize)
                    Array.Reverse(horizontalPixelPerMeter)
                    Array.Reverse(verticalPixelPerMeter)
                    Array.Reverse(nuberOfColorsInPallete)
                    Array.Reverse(numberOfImportantColorsUsed)
                End If

                size.CopyTo(byteArray, 0)   '	0, 4 bytes 	The headerSize of the BMP file in bytes
                width.CopyTo(byteArray, 4)   '	 4 bytes
                height.CopyTo(byteArray, 8)   ' 4 bytes
                colorPlanes.CopyTo(byteArray, 12)   '	12, 4 bytes
                bitsPerPixel.CopyTo(byteArray, 14)   '	2 bytes
                compressionMethod.CopyTo(byteArray, 16)   ' 4 bytes
                imageSize.CopyTo(byteArray, 20)   '	 4 bytes
                horizontalPixelPerMeter.CopyTo(byteArray, 24) '	4 bytes
                verticalPixelPerMeter.CopyTo(byteArray, 28)   '	4 bytes
                nuberOfColorsInPallete.CopyTo(byteArray, 32)   ' 4 bytes
                numberOfImportantColorsUsed.CopyTo(byteArray, 36) '	4 bytes

                Return byteArray
            End Get
        End Property

        Public Shared Function GetHeaderFromBytes(bytes As Byte()) As BitmapInfoHeader

            If bytes.Length < SizeInBytes Then
                Throw New ArgumentOutOfRangeException($"Info header should be at least 40 bytes. Smaller versions are not supported.")
            End If

            ' NOTE offses are 0 based for current byteArray (different than in wiki)
            Const BITS_PER_PIXEL_OFFSET = &HE
            Const COMPRESSION_METHOD_OFFSET = &H10

            Const HORIZONTAL_RESOLUTION_OFFSET = &H18
            Const VERTICAL_RESOLUTION_OFFSET = &H1C
            If Not BitConverter.IsLittleEndian Then
                ' BMP file is in little endian, we have to reverse bytes for parsing on Big-endian platform
                Array.Reverse(bytes, 0, 4) ' size of header
                Array.Reverse(bytes, 4, 4) ' size of Width
                Array.Reverse(bytes, 8, 4) ' size of Height
                Array.Reverse(bytes, BITS_PER_PIXEL_OFFSET, 2) ' BitsPerPixelEnum
                Array.Reverse(bytes, COMPRESSION_METHOD_OFFSET, 4) ' CompressionMethod
                Array.Reverse(bytes, &H20, 4) ' the image size. This is the size of the raw bitmap data; a dummy 0 can be given for BI_RGB bitmaps.
                Array.Reverse(bytes, HORIZONTAL_RESOLUTION_OFFSET, 4) ' the horizontal resolution of the image. (pixel per metre, signed integer) 
                Array.Reverse(bytes, VERTICAL_RESOLUTION_OFFSET, 4) ' the vertical resolution of the image. (pixel per metre, signed integer) 
                Array.Reverse(bytes, &H2C, 4) ' the number of colors in the color palette, or 0 to default to 2n (ignored)
                Array.Reverse(bytes, &H32, 4) ' the number of important colors used, or 0 when every color is important; generally ignored 
            End If

            Dim headerSize = BitConverter.ToInt32(bytes, 0)
            Dim width = BitConverter.ToInt32(bytes, 4)
            Dim height = BitConverter.ToInt32(bytes, 8)
            Dim bitsPerPixel = CType(BitConverter.ToInt16(bytes, BITS_PER_PIXEL_OFFSET), BitsPerPixelEnum)

            Dim compression = CType(BitConverter.ToInt16(bytes, COMPRESSION_METHOD_OFFSET), CompressionMethod)
            If Not (compression = CompressionMethod.BI_RGB OrElse compression = CompressionMethod.BI_BITFIELDS) Then
                Throw New Exception($"This {[Enum].GetName(compression.GetType(), compression)} is not supported.")
            End If

            Dim horizontalPixelPerMeter = BitConverter.ToInt32(bytes, HORIZONTAL_RESOLUTION_OFFSET)
            Dim verticalPixelPerMeter = BitConverter.ToInt32(bytes, VERTICAL_RESOLUTION_OFFSET)

            Dim infoHeader = New BitmapInfoHeader(width, height, bitsPerPixel, rawImageSize:=0, horizontalPixelPerMeter:=horizontalPixelPerMeter, verticalPixelPerMeter:=verticalPixelPerMeter, compressionMethod:=compression)
            Return infoHeader
        End Function
    End Class

End Namespace
