
Imports System.Drawing
Imports System.Runtime.CompilerServices
''' <summary>
''' Specifies the file format of the image. Not inheritable.
''' </summary>
Public Module ImageFormat

    ''' <summary>
    ''' Specifies the file format of the image.
    ''' </summary>
    Public Enum ImageFormats As Integer

        ''' <summary>
        ''' Gets the bitmap (BMP) image format.
        ''' </summary>
        Bmp
        ''' <summary>
        ''' Gets the enhanced metafile (EMF) image format.
        ''' </summary>
        Emf
        ''' <summary>
        ''' Gets the Exchangeable Image File (Exif) format.
        ''' </summary>
        Exif
        ''' <summary>
        ''' Gets the Graphics Interchange Format (GIF) image format.
        ''' </summary>
        Gif
        ''' <summary>
        ''' Gets the Windows icon image format.
        ''' </summary>
        Icon
        ''' <summary>
        ''' Gets the Joint Photographic Experts Group (JPEG) image format.
        ''' </summary>
        Jpeg
        ''' <summary>
        ''' Gets the format of a bitmap in memory.
        ''' </summary>
        MemoryBmp
        ''' <summary>
        ''' Gets the W3C Portable Network Graphics (PNG) image format.
        ''' </summary>
        Png
        ''' <summary>
        ''' Gets the Tagged Image File Format (TIFF) image format.
        ''' </summary>
        Tiff
        ''' <summary>
        ''' Gets the Windows metafile (WMF) image format.
        ''' </summary>
        Wmf
    End Enum

    <Extension> Public Function GetFormat(format As ImageFormat.ImageFormats) As System.Drawing.Imaging.ImageFormat
        Return __formats(format)
    End Function

    ReadOnly __formats As SortedDictionary(Of ImageFormats, System.Drawing.Imaging.ImageFormat) =
        New SortedDictionary(Of ImageFormats, Drawing.Imaging.ImageFormat) From {
 _
        {ImageFormats.Bmp, System.Drawing.Imaging.ImageFormat.Bmp},
        {ImageFormats.Emf, System.Drawing.Imaging.ImageFormat.Emf},
        {ImageFormats.Exif, System.Drawing.Imaging.ImageFormat.Exif},
        {ImageFormats.Gif, System.Drawing.Imaging.ImageFormat.Gif},
        {ImageFormats.Icon, System.Drawing.Imaging.ImageFormat.Icon},
        {ImageFormats.Jpeg, System.Drawing.Imaging.ImageFormat.Jpeg},
        {ImageFormats.MemoryBmp, System.Drawing.Imaging.ImageFormat.MemoryBmp},
        {ImageFormats.Png, System.Drawing.Imaging.ImageFormat.Png},
        {ImageFormats.Tiff, System.Drawing.Imaging.ImageFormat.Tiff},
        {ImageFormats.Wmf, System.Drawing.Imaging.ImageFormat.Wmf}
    }

    ''' <summary>
    ''' Saves this System.Drawing.Image to the specified file in the specified format.
    ''' </summary>
    ''' <param name="res"></param>
    ''' <param name="path"></param>
    ''' <param name="format"></param>
    ''' <returns></returns>
    <Extension> Public Function SaveAs(res As Image, path As String, format As ImageFormats) As Boolean
        Try
            Call res.Save(path, format.GetFormat)
        Catch ex As Exception
            Call App.LogException(New Exception(path, ex))
            Return False
        End Try

        Return True
    End Function
End Module
