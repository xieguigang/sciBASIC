Namespace Imaging.BitmapImage.FileStream

    Public Enum BitsPerPixelEnum As Integer
        Monochrome = 1
        Four = 4
        Eight = 8
        RBG16 = 16
        RGB24 = 24
        RGBA32 = 32
    End Enum

    ''' <summary>
    ''' Number of bytes for specific Pixel format.
    ''' </summary>
    Public Enum BytesPerPixelEnum As Integer
        RBG16 = 2
        RGB24 = 3
        RGBA32 = 4
    End Enum

    Public Enum CompressionMethod As Integer
        BI_RGB = 0 ' none
        BI_RLE8 = 1
        BI_RLE4 = 2
        BI_BITFIELDS = 3
        BI_JPEG = 4
        BI_PNG = 5
        BI_ALPHABITFIELDS = 6
        BI_CMYK = 11
        BI_CMYKRLE8 = 12
        BI_CMYKRLE4 = 13
    End Enum

End Namespace