Imports System.Runtime.InteropServices

Namespace Imaging.BitmapImage.FileStream

    <StructLayout(LayoutKind.Sequential, Pack:=1)>
    Public Class BitmapInfoHeaderRGBA
        Inherits BitmapInfoHeader

        Public Const RedChannelBitMask As UInteger = &HFF0000
        Public Const GreenChannelBitMask As UInteger = &HFF00
        Public Const BlueChannelBitMask As UInteger = &HFF
        Public Const AlphaChannelBitMask As UInteger = &HFF000000UI

        ''' <summary>
        ''' DIB header (bitmap information header)
        ''' This is standard Windows BITMAPINFOHEADER as described here https://en.wikipedia.org/wiki/BMP_file_format#Bitmap_file_header
        ''' </summary>
        Public Sub New(width As Integer, height As Integer,
                       Optional bitsPerPixel As BitsPerPixelEnum = BitsPerPixelEnum.RGB24,
                       Optional rawImageSize As Integer = 0,
                       Optional horizontalPixelPerMeter As Integer = 3780,
                       Optional verticalPixelPerMeter As Integer = 3780,
                       Optional compressionMethod As CompressionMethod = CompressionMethod.BI_BITFIELDS)

            MyBase.New(width, height, bitsPerPixel, rawImageSize, horizontalPixelPerMeter, verticalPixelPerMeter, compressionMethod)
            'this.Width = width;
            'this.Height = height;

            'this.BitsPerPixel = bitsPerPixel;
            'this.CompressionMethod = (uint) CompressionMethod.BI_RGB;
            'this.ImageSize = rawImageSize;
            'this.HorizontalPixelPerMeter = horizontalPixelPerMeter; // 96 DPI
            'this.VerticalPixelPerMeter = verticalPixelPerMeter;   // 96 DPI
        End Sub

        'public static int SizeInBytes => System.Runtime.InteropServices.Marshal.SizeOf(typeof(BitmapInfoHeader));
        Public Overloads Shared ReadOnly Property SizeInBytes As Integer
            Get
                Return 56
            End Get
        End Property

        ''' <summary>
        ''' This is BitmapInfoHeader for ARGB32 as described here https://en.wikipedia.org/wiki/BMP_file_format#Example_2
        ''' </summary>
        Public Overloads ReadOnly Property HeaderInfoBytes As Byte()
            Get
                Dim byteArray = New Byte(SizeInBytes - 1) {} ' 56

                ' get base array
                MyBase.HeaderInfoBytes.CopyTo(byteArray, 0)



                ' chage header size
                Dim size = BitConverter.GetBytes(SizeInBytes)

                Dim redChannelBitMaskBytes = BitConverter.GetBytes(RedChannelBitMask)
                Dim greenChannelBitMaskBytes = BitConverter.GetBytes(GreenChannelBitMask)
                Dim blueChannelBitMaskBytes = BitConverter.GetBytes(BlueChannelBitMask)
                Dim alphaChannelBitMaskBytes = BitConverter.GetBytes(AlphaChannelBitMask)

                ' BMP byte order is little endian so we have to take care on byte ordering
                If Not BitConverter.IsLittleEndian Then
                    ' we are on BigEndian system so we have to revers byte order
                    Array.Reverse(size)
                    Array.Reverse(redChannelBitMaskBytes)
                    Array.Reverse(greenChannelBitMaskBytes)
                    Array.Reverse(blueChannelBitMaskBytes)
                    Array.Reverse(alphaChannelBitMaskBytes)
                End If

                size.CopyTo(byteArray, 0)   '	0, 4 bytes 	The headerSize of the BMP file in bytes
                redChannelBitMaskBytes.CopyTo(byteArray, &H28)   '	0, 4 bytes 	The headerSize of the BMP file in bytes
                greenChannelBitMaskBytes.CopyTo(byteArray, &H2C)   '	0, 4 bytes 	The headerSize of the BMP file in bytes
                blueChannelBitMaskBytes.CopyTo(byteArray, &H30)   '	0, 4 bytes 	The headerSize of the BMP file in bytes
                alphaChannelBitMaskBytes.CopyTo(byteArray, &H34)   '	0, 4 bytes 	The headerSize of the BMP file in bytes

                Return byteArray
            End Get
        End Property

        Public Overloads Shared Function GetHeaderFromBytes(bytes As Byte()) As BitmapInfoHeaderRGBA

            If bytes.Length < BitmapInfoHeader.SizeInBytes Then
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
                'Array.Reverse( bytes, 0x2C, 4 ); // the number of colors in the color palette, or 0 to default to 2n (ignored)
                'Array.Reverse( bytes, 0x32, 4 ); // the number of important colors used, or 0 when every color is important; generally ignored 
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

            Dim infoHeader = New BitmapInfoHeaderRGBA(width, height, bitsPerPixel, rawImageSize:=0, horizontalPixelPerMeter:=horizontalPixelPerMeter, verticalPixelPerMeter:=verticalPixelPerMeter)
            Return infoHeader
        End Function
    End Class

End Namespace
