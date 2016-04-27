Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.CompilerServices

Namespace Imaging

    ''' <summary>
    ''' Specifies the file format of the image. Not inheritable.
    ''' </summary>
    Public Module ImageFormatExtensions

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

        <Extension> Public Function GetFormat(format As ImageFormats) As ImageFormat
            Return __formats(format)
        End Function

        ReadOnly __formats As SortedDictionary(Of ImageFormats, ImageFormat) =
            New SortedDictionary(Of ImageFormats, ImageFormat) From {
 _
            {ImageFormats.Bmp, ImageFormat.Bmp},
            {ImageFormats.Emf, ImageFormat.Emf},
            {ImageFormats.Exif, ImageFormat.Exif},
            {ImageFormats.Gif, ImageFormat.Gif},
            {ImageFormats.Icon, ImageFormat.Icon},
            {ImageFormats.Jpeg, ImageFormat.Jpeg},
            {ImageFormats.MemoryBmp, ImageFormat.MemoryBmp},
            {ImageFormats.Png, ImageFormat.Png},
            {ImageFormats.Tiff, ImageFormat.Tiff},
            {ImageFormats.Wmf, ImageFormat.Wmf}
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
                Dim parent As String = FileIO.FileSystem.GetParentPath(path)
                Call FileIO.FileSystem.CreateDirectory(parent)
                Call res.Save(path, format.GetFormat)
            Catch ex As Exception
                ex = New Exception(path.ToFileURL, ex)
                Call App.LogException(ex)
                Call ex.PrintException
                Return False
            End Try

            Return True
        End Function
    End Module
End Namespace