#Region "Microsoft.VisualBasic::c57640bb26193712eea10857990983d5, Microsoft.VisualBasic.Core\Extensions\Image\ImageFormat.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Enum ImageFormats
    ' 
    '         Base64, Bmp, Emf, Exif, Gif
    '         Icon, Jpeg, MemoryBmp, Png, Tiff
    '         Wmf
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module ImageFormatExtensions
    ' 
    '         Properties: Png
    ' 
    '         Function: GetFormat, GetSaveImageFormat, ParseImageFormat, SaveAs
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Text
Imports defaultFormat = Microsoft.VisualBasic.Language.Default.Default(Of System.Drawing.Imaging.ImageFormat)

Namespace Imaging

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
        ''' <summary>
        ''' Base64
        ''' </summary>
        Base64
    End Enum

    ''' <summary>
    ''' Specifies the file format of the image. Not inheritable.
    ''' </summary>
    Public Module ImageFormatExtensions

        ''' <summary>
        ''' 获取 W3C 可移植网络图形 (PNG) 图像格式。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Png As defaultFormat = ImageFormat.Png

        ''' <summary>
        ''' default is <see cref="ImageFormat.Png"/>
        ''' </summary>
        ''' <param name="format">大小写不敏感</param>
        ''' <returns></returns>
        <Extension>
        Public Function GetSaveImageFormat(format As String) As ImageFormat
            Dim value As String = format.ToLower.Trim

            If ImagingFormats.ContainsKey(value) Then
                Return ImagingFormats(value)
            Else
                Return ImageFormat.Png
            End If
        End Function

        ReadOnly ImagingFormats As New Dictionary(Of String, ImageFormat) From {
            {"jpg", ImageFormat.Jpeg},
            {"bmp", ImageFormat.Bmp},
            {"emf", ImageFormat.Emf},
            {"exif", ImageFormat.Exif},
            {"gif", ImageFormat.Gif},
            {"png", ImageFormat.Png},
            {"wmf", ImageFormat.Wmf},
            {"tiff", ImageFormat.Tiff}
        }

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function GetFormat(format As ImageFormats) As ImageFormat
            Return __formats(format)
        End Function

        Dim enumFormats As Dictionary(Of String, ImageFormats) =
            [Enums](Of ImageFormats)() _
            .ToDictionary(Function(t) t.ToString.ToLower)

        ''' <summary>
        ''' 不存在的名称会返回<see cref="ImageFormats.Png"/>类型
        ''' </summary>
        ''' <param name="format$">大小写不敏感</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ParseImageFormat(format$) As ImageFormats
            Return enumFormats.TryGetValue(LCase(format), [default]:=ImageFormats.Png)
        End Function

        ReadOnly __formats As New SortedDictionary(Of ImageFormats, ImageFormat) From {
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
        ''' Saves this <see cref="System.Drawing.Image"/> to the specified file in the specified format.
        ''' (这个函数可以很容易的将图像对象保存为tiff文件)
        ''' </summary>
        ''' <param name="res">
        ''' The image resource data that will be saved to the disk.
        ''' (因为这个函数可能会被<see cref="Graphics2D.ImageResource"/>所调用，
        ''' 由于该属性的Set方法是不公开可见的，所以将会不兼容这个方法，如果这个
        ''' 参数被设置为ByRef的话)
        ''' </param>
        ''' <param name="path">path string</param>
        ''' <param name="format">Image formats enumeration.</param>
        ''' <returns></returns>
        <Extension> Public Function SaveAs(res As Image,
                                           path$,
                                           Optional format As ImageFormats = ImageFormats.Png,
                                           Optional autoDispose As Boolean = False) As Boolean
            Try
                Call path.ParentPath.MkDIR

                If format = ImageFormats.Tiff Then
                    Return New TiffWriter(res).MultipageTiffSave(path)
                ElseIf format = ImageFormats.Base64 Then
                    Return res _
                        .ToBase64String _
                        .SaveTo(path, Encodings.ASCII.CodePage)
                Else
                    Call res.Save(path, format.GetFormat)
                End If
            Catch ex As Exception
                ex = New Exception(path.ToFileURL, ex)
                Call App.LogException(ex)
                Call ex.PrintException
                Return False
            Finally
                If autoDispose Then
                    Call res.Dispose()
                    Call GC.SuppressFinalize(res)
                    Call GC.Collect()
                End If
            End Try

            Return True
        End Function
    End Module
End Namespace
